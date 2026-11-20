using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Faults;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class DeliveryAndOrderingScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;
    private readonly CentralMessageRouter _router;
    private readonly SimulatedSession _session;
    private readonly SyntheticMockEndpoint _recipient;

    public DeliveryAndOrderingScenarioTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
        _router = new CentralMessageRouter(_clock, _diagnostics);

        _session = new SimulatedSession("sess-delivery-01");
        _session.RegisterEndpoint("ep-src");
        _session.RegisterEndpoint("ep-dst");
        _session.AuthorizeTopic("combat.damage");
        _router.RegisterSession(_session);

        _recipient = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-dst" }, _clock, _diagnostics);
        _recipient.ConnectAsync().GetAwaiter().GetResult();
        _router.RegisterEndpoint(_recipient);
    }

    [Fact(DisplayName = "S80-19: Configured successful path satisfies declared delivery contract")]
    public async Task S80_19_ExpectedDeliverySucceeds()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{\"damage\": 45}",
            sequenceNumber: 1
        );

        var result = await _router.RouteAsync(envelope);

        Assert.True(result.Success);
        Assert.Equal(MessageLifecycleState.Completed, result.State);
        Assert.NotNull(result.ResponseEnvelope);
    }

    [Fact(DisplayName = "S80-20: Duplicate message identity rejected per verified semantics (ERR_PME_004)")]
    public async Task S80_20_DuplicateMessageRejected()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{\"damage\": 45}",
            sequenceNumber: 1
        );

        var result1 = await _router.RouteAsync(envelope);
        var result2 = await _router.RouteAsync(envelope); // Duplicate identical message_id

        Assert.True(result1.Success);
        Assert.False(result2.Success);
        Assert.Equal("ERR_PME_004", result2.ErrorCode);
    }

    [Fact(DisplayName = "S80-21: Sequence number regression rejected per Step 73 monotonic rule (ERR_PME_004)")]
    public async Task S80_21_SequenceRegressionRejected()
    {
        var envSeq10 = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{\"damage\": 10}",
            sequenceNumber: 10
        );

        var envSeq5 = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{\"damage\": 5}",
            sequenceNumber: 5 // Regression: 5 <= 10
        );

        var res1 = await _router.RouteAsync(envSeq10);
        var res2 = await _router.RouteAsync(envSeq5);

        Assert.True(res1.Success);
        Assert.False(res2.Success);
        Assert.Equal("ERR_PME_004", res2.ErrorCode);
        Assert.Contains("regression", res2.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "S80-22: Sequence gap detected by deduplication guard")]
    public void S80_22_SequenceGapDetected()
    {
        var guard = new DeduplicationGuard();

        var check1 = guard.CheckAndRecord("sender-01", "msg-001", 1);
        var checkGap = guard.CheckAndRecord("sender-01", "msg-002", 5); // Sequence jumped from 1 to 5

        Assert.Equal(DeduplicationGuard.SequenceEvaluation.ValidNext, check1);
        Assert.Equal(DeduplicationGuard.SequenceEvaluation.OutOfOrderGap, checkGap);
    }

    [Fact(DisplayName = "S80-23: Delayed delivery advances virtual clock without sleeps")]
    public async Task S80_23_DelayedDeliveryViaVirtualClock()
    {
        var initialTime = _clock.UtcNow;
        var faults = new CommunicationFaultConfig { InjectedLatency = TimeSpan.FromSeconds(5) };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.True(result.Success);
        Assert.Equal(initialTime.AddSeconds(5), _clock.UtcNow);
    }

    [Fact(DisplayName = "S80-24: Delivery rejection distinguishable from successful delivery")]
    public async Task S80_24_DeliveryRejectionDistinguishable()
    {
        var faults = new CommunicationFaultConfig
        {
            RejectNextSend = true,
            RejectionErrorCode = "ERR_PME_010",
            RejectionMessage = "Simulated buffer saturation"
        };

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "ep-src",
            recipientEndpointId: "ep-dst",
            topic: "combat.damage",
            category: MessageCategory.DataEvent,
            payloadJson: "{}"
        );

        var result = await _router.RouteAsync(envelope, faults);

        Assert.False(result.Success);
        Assert.Equal(MessageLifecycleState.Failed, result.State);
        Assert.Equal("ERR_PME_010", result.ErrorCode);
    }
}
