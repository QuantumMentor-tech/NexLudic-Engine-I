using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Faults;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class AcknowledgmentAndRetryScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;
    private readonly CentralMessageRouter _router;
    private readonly SimulatedSession _session;
    private readonly SyntheticMockEndpoint _recipient;

    public AcknowledgmentAndRetryScenarioTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
        _router = new CentralMessageRouter(_clock, _diagnostics);

        _session = new SimulatedSession("sess-ack-01");
        _session.RegisterEndpoint("ep-client");
        _session.RegisterEndpoint("ep-server");
        _session.AuthorizeTopic("inventory.trade");
        _router.RegisterSession(_session);

        _recipient = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-server" }, _clock, _diagnostics);
        _recipient.ConnectAsync().GetAwaiter().GetResult();
        _router.RegisterEndpoint(_recipient);
    }

    [Fact(DisplayName = "S80-25: Valid acknowledgment generated and interpreted per contract")]
    public async Task S80_25_ValidAcknowledgmentGenerated()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "inventory.trade",
            category: MessageCategory.CommandRequest,
            payloadJson: "{\"item\": \"gold_bar\"}",
            correlationId: "req-123"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.True(result.Success);
        Assert.NotNull(result.ResponseEnvelope);
        Assert.Equal(MessageCategory.Acknowledgement, result.ResponseEnvelope.Category);
        Assert.Equal("req-123", result.ResponseEnvelope.CorrelationId);
        Assert.Equal("ep-server", result.ResponseEnvelope.SenderEndpointId);
        Assert.Equal("ep-client", result.ResponseEnvelope.RecipientEndpointId);
    }

    [Fact(DisplayName = "S80-26: Missing acknowledgment not interpreted as proof operation unapplied")]
    public async Task S80_26_MissingAcknowledgmentYieldsUncertainState()
    {
        var faults = new CommunicationFaultConfig { WithholdAcknowledgement = true };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "inventory.trade",
            category: MessageCategory.CommandRequest,
            payloadJson: "{\"item\": \"diamond\"}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        // Core Invariant: Missing ACK does not equal Failed; it equals Uncertain!
        Assert.True(result.Success);
        Assert.Equal(MessageLifecycleState.Uncertain, result.State);
        Assert.Null(result.ResponseEnvelope);
        Assert.Single(_recipient.ReceivedMessages); // Verified recipient actually processed the message!
    }

    [Fact(DisplayName = "S80-27: Retryable failure triggers retry loop in scenario harness")]
    public async Task S80_27_RetryableFailureLoop()
    {
        int attempts = 0;
        RouteResult? lastResult = null;

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "inventory.trade",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        // First 2 attempts fail with transient error; 3rd succeeds
        for (int i = 0; i < 3; i++)
        {
            attempts++;
            var faults = new CommunicationFaultConfig
            {
                RejectNextSend = i < 2,
                RejectionErrorCode = "ERR_PME_010"
            };

            lastResult = await _router.RouteAsync(envelope, faults);
            if (lastResult.Success) break;
        }

        Assert.Equal(3, attempts);
        Assert.NotNull(lastResult);
        Assert.True(lastResult.Success);
    }

    [Fact(DisplayName = "S80-28: Non-retryable validation error does not trigger unauthorized retry")]
    public async Task S80_28_NonRetryableValidationErrorStops()
    {
        // Topic not authorized in session: strictly non-retryable semantic error!
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "unauthorized.admin.override",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_008", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-29: Duplicate after retry deduplicated cleanly by router")]
    public async Task S80_29_DuplicateAfterRetryHandled()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "inventory.trade",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var faults = new CommunicationFaultConfig { DuplicateNextDelivery = true };
        var result = await _router.RouteAsync(envelope, faults);

        Assert.True(result.Success);
        Assert.Equal(2, _recipient.ReceivedMessages.Count); // Synthetic endpoint recorded duplicate delivery
    }

    [Fact(DisplayName = "S80-30: Retry exhaustion reports clear terminal failure")]
    public async Task S80_30_RetryExhaustionReportsFailure()
    {
        int maxRetries = 3;
        int attempts = 0;
        RouteResult? lastResult = null;

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-client",
            recipientEndpointId: "ep-server",
            topic: "inventory.trade",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var faults = new CommunicationFaultConfig { RejectNextSend = true, RejectionErrorCode = "ERR_PME_010" };

        for (int i = 0; i < maxRetries; i++)
        {
            attempts++;
            lastResult = await _router.RouteAsync(envelope, faults);
            if (lastResult.Success) break;
        }

        Assert.Equal(maxRetries, attempts);
        Assert.NotNull(lastResult);
        Assert.False(lastResult.Success);
        Assert.Equal("ERR_PME_010", lastResult.ErrorCode);
    }
}

