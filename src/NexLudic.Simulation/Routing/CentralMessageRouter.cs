using System.Collections.Concurrent;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Faults;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Sessions;

namespace NexLudic.Simulation.Routing;

public record RouteResult(
    bool Success,
    MessageLifecycleState State,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    BridgeMessageEnvelope? ResponseEnvelope = null
);

/// <summary>
/// Central Message Router implementing Step 73 CMR-02 in-memory hub-and-spoke router.
/// Manages endpoint registration, session verification, sequential validation, deduplication,
/// request-response correlation, and fault-injected delivery.
/// </summary>
public sealed class CentralMessageRouter
{
    private readonly ConcurrentDictionary<string, ISyntheticEndpoint> _endpoints = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SimulatedSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private readonly MessageValidator _validator;
    private readonly DeduplicationGuard _dedupGuard = new();
    private readonly RequestResponseCorrelator _correlator;
    private readonly IDeterministicClock _clock;
    private readonly IDiagnosticsCollector _diagnostics;

    public CentralMessageRouter(IDeterministicClock clock, IDiagnosticsCollector diagnostics)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _validator = new MessageValidator(_clock);
        _correlator = new RequestResponseCorrelator(_clock);
    }

    public void RegisterSession(SimulatedSession session)
    {
        if (session != null)
            _sessions[session.SessionId] = session;
    }

    public void RegisterEndpoint(ISyntheticEndpoint endpoint)
    {
        if (endpoint != null)
            _endpoints[endpoint.EndpointId] = endpoint;
    }

    public void UnregisterEndpoint(string endpointId)
    {
        _endpoints.TryRemove(endpointId, out _);
    }

    public async Task<RouteResult> RouteAsync(
        BridgeMessageEnvelope envelope,
        CommunicationFaultConfig? faults = null,
        CancellationToken cancellationToken = default)
    {
        if (envelope == null)
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_001", "Envelope is null");

        _diagnostics.LogInfo("CentralMessageRouter", $"Routing message {envelope.MessageId} from {envelope.SenderEndpointId} to {envelope.RecipientEndpointId}");

        // 1. Fault Injection: Immediate rejection
        if (faults != null && faults.RejectNextSend)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Fault injected: Rejecting next send with {faults.RejectionErrorCode}");
            return new RouteResult(false, MessageLifecycleState.Failed, faults.RejectionErrorCode, faults.RejectionMessage);
        }

        // 2. Fault Injection: Corrupt digest
        if (faults != null && faults.CorruptPayloadDigest)
        {
            envelope = new BridgeMessageEnvelope
            {
                EnvelopeVersion = envelope.EnvelopeVersion,
                MessageId = envelope.MessageId,
                SessionId = envelope.SessionId,
                Category = envelope.Category,
                SenderEndpointId = envelope.SenderEndpointId,
                RecipientEndpointId = envelope.RecipientEndpointId,
                CorrelationId = envelope.CorrelationId,
                Topic = envelope.Topic,
                SchemaVersion = envelope.SchemaVersion,
                SequenceNumber = envelope.SequenceNumber,
                TimestampUtc = envelope.TimestampUtc,
                TtlMs = envelope.TtlMs,
                PayloadDigest = "sha256:corrupted_digest_00000000000000000000000000000000000000000000",
                PayloadJson = envelope.PayloadJson,
                Headers = envelope.Headers
            };
        }

        // 3. Resolve Session
        _sessions.TryGetValue(envelope.SessionId, out var session);

        // 4. Seven-Stage Validation
        var validation = _validator.Validate(envelope, session);
        if (!validation.IsValid)
        {
            _diagnostics.LogError("CentralMessageRouter", $"Validation failed: {validation.ErrorCode} - {validation.ErrorMessage}");
            return new RouteResult(false, MessageLifecycleState.Failed, validation.ErrorCode, validation.ErrorMessage);
        }

        // 5. Sequence Monotonicity & Deduplication
        var dedupResult = _dedupGuard.CheckAndRecord(envelope.SenderEndpointId, envelope.MessageId, envelope.SequenceNumber);
        if (dedupResult == DeduplicationGuard.SequenceEvaluation.DuplicateMessageId)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Duplicate message {envelope.MessageId} detected and dropped.");
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_004", $"Duplicate message_id '{envelope.MessageId}' rejected.");
        }
        if (dedupResult == DeduplicationGuard.SequenceEvaluation.SequenceRegression)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Sequence regression detected for sender {envelope.SenderEndpointId}: seq {envelope.SequenceNumber}");
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_004", $"Sequence number {envelope.SequenceNumber} regression for sender {envelope.SenderEndpointId}.");
        }

        // 6. Request Correlation
        if (envelope.Category == MessageCategory.CommandRequest)
        {
            _correlator.RegisterRequest(envelope);
        }

        // 7. Fault Injection: Drop delivery
        if (faults != null && faults.DropNextDelivery)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Fault injected: Dropping delivery of {envelope.MessageId}");
            return new RouteResult(true, MessageLifecycleState.Uncertain, null, "Message transmitted but dropped in simulated transit.");
        }

        // 8. Fault Injection: Recipient Unavailable
        if (faults != null && faults.SimulateRecipientUnavailable)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Fault injected: Recipient {envelope.RecipientEndpointId} unavailable.");
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_007", $"Destination endpoint '{envelope.RecipientEndpointId}' is unavailable.");
        }

        // 9. Fault Injection: Latency
        if (faults != null && faults.InjectedLatency > TimeSpan.Zero)
        {
            _clock.Advance(faults.InjectedLatency);
        }

        // 10. Fault Injection: Disconnection during exchange
        if (faults != null && faults.DisconnectDuringExchange)
        {
            _diagnostics.LogError("CentralMessageRouter", "Fault injected: Transport connection interrupted during exchange.");
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_010", "Connection disconnected during message exchange.");
        }

        // 11. Recipient Delivery
        if (!_endpoints.TryGetValue(envelope.RecipientEndpointId, out var recipientEndpoint))
        {
            return new RouteResult(false, MessageLifecycleState.Failed, "ERR_PME_007", $"Recipient endpoint '{envelope.RecipientEndpointId}' not found in active router registry.");
        }

        // Convert envelope to synthetic model for mock endpoint
        var synthEnvelope = new SyntheticMessageEnvelope(
            envelope.MessageId, envelope.SessionId, envelope.SenderEndpointId, envelope.RecipientEndpointId,
            envelope.Topic, envelope.SequenceNumber, envelope.TimestampUtc, envelope.PayloadJson, envelope.Headers);

        var endpointResponse = await recipientEndpoint.SendAndReceiveAsync(synthEnvelope, cancellationToken).ConfigureAwait(false);

        // 12. Fault Injection: Duplicate delivery
        if (faults != null && faults.DuplicateNextDelivery)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Fault injected: Delivering duplicate of {envelope.MessageId}");
            await recipientEndpoint.SendAndReceiveAsync(synthEnvelope, cancellationToken).ConfigureAwait(false);
        }

        // 13. Evaluate Response / ACK
        BridgeMessageEnvelope? responseEnvelope = null;
        if (endpointResponse != null && (faults == null || !faults.WithholdAcknowledgement))
        {
            responseEnvelope = BridgeMessageEnvelope.Create(
                sessionId: envelope.SessionId,
                senderEndpointId: envelope.RecipientEndpointId,
                recipientEndpointId: envelope.SenderEndpointId,
                category: MessageCategory.Acknowledgement,
                topic: envelope.Topic,
                payloadJson: endpointResponse.PayloadJson,
                sequenceNumber: endpointResponse.SequenceNumber,
                correlationId: envelope.CorrelationId,
                timestamp: _clock.UtcNow
            );
        }

        if (faults != null && faults.WithholdAcknowledgement)
        {
            _diagnostics.LogWarning("CentralMessageRouter", $"Fault injected: Withholding acknowledgement for {envelope.MessageId}");
            return new RouteResult(true, MessageLifecycleState.Uncertain, null, "Message processed by recipient but ACK withheld.");
        }

        if (faults != null && faults.SimulateUncertainOutcome)
        {
            return new RouteResult(true, MessageLifecycleState.Uncertain, null, "Application outcome is uncertain pending independent telemetry verification.");
        }

        return new RouteResult(true, MessageLifecycleState.Completed, null, "Message successfully delivered and acknowledged.", responseEnvelope);
    }

    public void Reset()
    {
        _endpoints.Clear();
        _sessions.Clear();
        _dedupGuard.Reset();
        _correlator.Reset();
    }
}
