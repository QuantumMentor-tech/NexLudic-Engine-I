using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class RoutingAndSessionScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;
    private readonly CentralMessageRouter _router;
    private readonly SimulatedSession _session;
    private readonly SyntheticMockEndpoint _sender;
    private readonly SyntheticMockEndpoint _recipient;

    public RoutingAndSessionScenarioTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
        _router = new CentralMessageRouter(_clock, _diagnostics);

        _session = new SimulatedSession("sess-route-01");
        _session.RegisterEndpoint("ep-source-01");
        _session.RegisterEndpoint("ep-target-01");
        _session.AuthorizeTopic("game.score.sync");
        _router.RegisterSession(_session);

        _sender = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-source-01" }, _clock, _diagnostics);
        _recipient = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-target-01" }, _clock, _diagnostics);

        _sender.ConnectAsync().GetAwaiter().GetResult();
        _recipient.ConnectAsync().GetAwaiter().GetResult();

        _router.RegisterEndpoint(_sender);
        _router.RegisterEndpoint(_recipient);
    }

    [Fact(DisplayName = "S80-13: Valid message reaches the intended synthetic recipient")]
    public async Task S80_13_CorrectRecipientReached()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-source-01",
            recipientEndpointId: "ep-target-01",
            category: MessageCategory.DataEvent,
            topic: "game.score.sync",
            payloadJson: "{\"score\": 100}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.True(result.Success);
        Assert.Equal(MessageLifecycleState.Completed, result.State);
        Assert.Single(_recipient.ReceivedMessages);
        Assert.Equal(envelope.MessageId, _recipient.ReceivedMessages[0].MessageId);
    }

    [Fact(DisplayName = "S80-14: Unknown recipient produces routing failure without false success (ERR_PME_007)")]
    public async Task S80_14_UnknownRecipientFails()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-source-01",
            recipientEndpointId: "ep-non-existent-99", // Unknown!
            topic: "game.score.sync",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_007", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-15: Message cannot cross into different session (ERR_PME_005)")]
    public async Task S80_15_SessionMismatchBlocked()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: "sess-alien-session-999", // Different session!
            senderEndpointId: "ep-source-01",
            recipientEndpointId: "ep-target-01",
            topic: "game.score.sync",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_005", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-16: Expired or invalidated session handled per contract (ERR_PME_005)")]
    public async Task S80_16_ExpiredSessionFails()
    {
        _session.Invalidate(); // Mark session inactive

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-source-01",
            recipientEndpointId: "ep-target-01",
            topic: "game.score.sync",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_005", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-17: Independent synthetic endpoints do not share state or permissions")]
    public void S80_17_EndpointIsolationVerified()
    {
        var epA = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-iso-a" }, _clock, _diagnostics);
        var epB = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-iso-b" }, _clock, _diagnostics);

        Assert.NotEqual(epA.EndpointId, epB.EndpointId);
        Assert.Empty(epA.ReceivedMessages);
        Assert.Empty(epB.ReceivedMessages);
    }

    [Fact(DisplayName = "S80-18: Session epoch advance invalidates stale test messages")]
    public void S80_18_SessionEpochAdvanceDetectsStaleMessages()
    {
        var validator = new MessageValidator(_clock);
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-source-01",
            recipientEndpointId: "ep-target-01",
            topic: "game.score.sync",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        _session.AdvanceEpoch(); // Advance epoch from 1 to 2

        // Validate message claiming expected epoch 1 against session on epoch 2
        var result = validator.Validate(envelope, _session, expectedSessionEpoch: 1);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_005", result.ErrorCode);
        Assert.Contains("epoch mismatch", result.ErrorMessage);
    }
}

