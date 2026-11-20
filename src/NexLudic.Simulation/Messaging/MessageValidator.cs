using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Sessions;

namespace NexLudic.Simulation.Messaging;

/// <summary>
/// Seven-stage message validation pipeline implementing Step 73 (NLE-SPC-MRD-001).
/// </summary>
public sealed class MessageValidator
{
    private readonly IDeterministicClock _clock;

    public MessageValidator(IDeterministicClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public MessageValidationResult Validate(
        BridgeMessageEnvelope envelope,
        SimulatedSession? session,
        long? expectedSessionEpoch = null)
    {
        if (envelope == null)
            return MessageValidationResult.Fail("ERR_PME_001", "Envelope is null.", 1);

        // Stage 1: Envelope Framing & Structure
        if (string.IsNullOrWhiteSpace(envelope.EnvelopeVersion) || envelope.EnvelopeVersion != "1.0")
            return MessageValidationResult.Fail("ERR_PME_001", $"Unsupported envelope version '{envelope.EnvelopeVersion}'. Expected '1.0'.", 1);

        if (string.IsNullOrWhiteSpace(envelope.MessageId) || !envelope.MessageId.StartsWith("msg-"))
            return MessageValidationResult.Fail("ERR_PME_002", "Invalid message_id: must be non-empty and prefixed with 'msg-'.", 1);

        if (string.IsNullOrWhiteSpace(envelope.SenderEndpointId))
            return MessageValidationResult.Fail("ERR_PME_002", "Missing required header: sender_endpoint_id.", 1);

        if (string.IsNullOrWhiteSpace(envelope.RecipientEndpointId))
            return MessageValidationResult.Fail("ERR_PME_002", "Missing required header: recipient_endpoint_id.", 1);

        if (envelope.Category == MessageCategory.CommandRequest && string.IsNullOrWhiteSpace(envelope.CorrelationId))
            return MessageValidationResult.Fail("ERR_PME_002", "Missing required correlation_id for COMMAND_REQUEST.", 1);

        // Stage 2: Session State & Active Epoch
        if (string.IsNullOrWhiteSpace(envelope.SessionId))
            return MessageValidationResult.Fail("ERR_PME_005", "Missing required session_id.", 2);

        if (session == null || !string.Equals(session.SessionId, envelope.SessionId, StringComparison.OrdinalIgnoreCase))
            return MessageValidationResult.Fail("ERR_PME_005", $"Session '{envelope.SessionId}' unknown or not found.", 2);

        if (!session.IsActive)
            return MessageValidationResult.Fail("ERR_PME_005", $"Session '{envelope.SessionId}' is expired or inactive.", 2);

        if (expectedSessionEpoch.HasValue && session.SessionEpoch != expectedSessionEpoch.Value)
            return MessageValidationResult.Fail("ERR_PME_005", $"Session epoch mismatch: message on epoch {expectedSessionEpoch.Value}, active epoch is {session.SessionEpoch}.", 2);

        // Stage 3: Sender Identity & Attestation
        if (!session.IsEndpointRegistered(envelope.SenderEndpointId))
            return MessageValidationResult.Fail("ERR_PME_006", $"Sender endpoint '{envelope.SenderEndpointId}' is not registered in session '{session.SessionId}'.", 3);

        // Stage 4: Recipient Resolution & Route Eligibility
        if (envelope.RecipientEndpointId != "*" && !session.IsEndpointRegistered(envelope.RecipientEndpointId))
            return MessageValidationResult.Fail("ERR_PME_007", $"Recipient endpoint '{envelope.RecipientEndpointId}' is not registered in session '{session.SessionId}'.", 4);

        // Stage 5: Capability Authorization
        if (string.IsNullOrWhiteSpace(envelope.Topic))
            return MessageValidationResult.Fail("ERR_PME_002", "Missing required topic.", 5);

        if (!session.IsTopicAuthorized(envelope.Topic))
            return MessageValidationResult.Fail("ERR_PME_008", $"Topic '{envelope.Topic}' is not authorized in session.", 5);

        // Stage 6: Payload Schema & Digest Integrity
        string expectedDigest = BridgeMessageEnvelope.ComputeDigest(envelope.PayloadJson);
        if (!string.Equals(expectedDigest, envelope.PayloadDigest, StringComparison.OrdinalIgnoreCase))
            return MessageValidationResult.Fail("ERR_PME_003", $"Payload digest mismatch: expected '{expectedDigest}', envelope contained '{envelope.PayloadDigest}'.", 6);

        // Stage 7: Temporal Invariants & TTL
        if (envelope.TtlMs <= 0)
            return MessageValidationResult.Fail("ERR_PME_009", "Invalid ttl_ms: must be greater than zero.", 7);

        TimeSpan age = _clock.UtcNow - envelope.TimestampUtc;
        if (age.TotalMilliseconds > envelope.TtlMs)
            return MessageValidationResult.Fail("ERR_PME_009", $"Envelope TTL expired: age {age.TotalMilliseconds:F0}ms exceeds TTL {envelope.TtlMs}ms.", 7);

        return MessageValidationResult.Success();
    }
}

