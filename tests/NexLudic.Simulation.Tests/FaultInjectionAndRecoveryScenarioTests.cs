using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Faults;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class FaultInjectionAndRecoveryScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;
    private readonly CentralMessageRouter _router;
    private readonly SimulatedSession _session;
    private readonly SyntheticMockEndpoint _recipient;

    public FaultInjectionAndRecoveryScenarioTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
        _router = new CentralMessageRouter(_clock, _diagnostics);

        _session = new SimulatedSession("sess-fault-01");
        _session.RegisterEndpoint("ep-peer-a");
        _session.RegisterEndpoint("ep-peer-b");
        _session.AuthorizeTopic("world.weather");
        _router.RegisterSession(_session);

        _recipient = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-peer-b" }, _clock, _diagnostics);
        _recipient.ConnectAsync().GetAwaiter().GetResult();
        _router.RegisterEndpoint(_recipient);
    }

    [Fact(DisplayName = "S80-31: Destination unavailable simulated deterministically")]
    public async Task S80_31_DestinationUnavailableSimulated()
    {
        var faults = new CommunicationFaultConfig { SimulateRecipientUnavailable = true };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_007", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-32: Injected delivery drop produces expected uncertain outcome")]
    public async Task S80_32_InjectedDeliveryDropYieldsUncertain()
    {
        var faults = new CommunicationFaultConfig { DropNextDelivery = true };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.True(result.Success);
        Assert.Equal(MessageLifecycleState.Uncertain, result.State);
        Assert.Contains("dropped in simulated transit", result.ErrorMessage);
    }

    [Fact(DisplayName = "S80-33: Invalid response / corrupted payload digest handled safely")]
    public async Task S80_33_CorruptedPayloadDigestCaught()
    {
        var faults = new CommunicationFaultConfig { CorruptPayloadDigest = true };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_003", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-34: Disconnection during exchange reported distinctly from assertion failure")]
    public async Task S80_34_DisconnectionDuringExchangeHandled()
    {
        var faults = new CommunicationFaultConfig { DisconnectDuringExchange = true };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_010", result.ErrorCode);
        Assert.Contains("disconnected", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "S80-35: Recovery with valid prerequisites follows verified contract")]
    public async Task S80_35_RecoveryFollowsContract()
    {
        // 1. Initial exchange fails due to connection drop
        var faults = new CommunicationFaultConfig { DisconnectDuringExchange = true };
        var envelope1 = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}",
            sequenceNumber: 1
        );
        var res1 = await _router.RouteAsync(envelope1, faults);
        Assert.False(res1.Success);

        // 2. Recovery: Channel re-established with valid prerequisites, sequence advances
        faults.Reset();
        var envelope2 = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}",
            sequenceNumber: 2
        );
        var res2 = await _router.RouteAsync(envelope2, faults);
        Assert.True(res2.Success);
        Assert.Equal(MessageLifecycleState.Completed, res2.State);
    }

    [Fact(DisplayName = "S80-36: Recovery prerequisite missing reports blocked outcome")]
    public async Task S80_36_MissingRecoveryPrerequisiteReportsBlocked()
    {
        // Invalidate session to simulate missing recovery token/epoch
        _session.Invalidate();

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-peer-a",
            recipientEndpointId: "ep-peer-b",
            topic: "world.weather",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope);

        Assert.False(result.Success);
        Assert.Equal("ERR_PME_005", result.ErrorCode);
    }
}
