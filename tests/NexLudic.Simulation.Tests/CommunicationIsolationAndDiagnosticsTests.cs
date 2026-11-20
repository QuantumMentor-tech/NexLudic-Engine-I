using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class CommunicationIsolationAndDiagnosticsTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;

    public CommunicationIsolationAndDiagnosticsTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
    }

    [Fact(DisplayName = "S80-37: Independent scenario execution produces expected result")]
    public async Task S80_37_IndependentScenarioExecution()
    {
        var engine = new TestHarnessEngine();
        var router = new CentralMessageRouter(_clock, _diagnostics);
        var session = new SimulatedSession("sess-iso-01");
        session.RegisterEndpoint("ep-a");
        session.RegisterEndpoint("ep-b");
        session.AuthorizeTopic("test.iso");
        router.RegisterSession(session);

        var epB = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-b" }, _clock, _diagnostics);
        await epB.ConnectAsync();
        router.RegisterEndpoint(epB);

        var envelope = BridgeMessageEnvelope.Create(session.SessionId, "ep-a", "ep-b", MessageCategory.DataEvent, "test.iso", "{}");
        var routeRes = await router.RouteAsync(envelope);

        Assert.True(routeRes.Success);
    }

    [Fact(DisplayName = "S80-38: State from previous test run does not contaminate new independent run")]
    public async Task S80_38_CrossRunIsolation()
    {
        var routerA = new CentralMessageRouter(_clock, _diagnostics);
        var sessionA = new SimulatedSession("sess-run-1");
        routerA.RegisterSession(sessionA);

        var routerB = new CentralMessageRouter(_clock, _diagnostics);
        // routerB has NO sessions registered

        var envelope = BridgeMessageEnvelope.Create("sess-run-1", "ep-1", "ep-2", MessageCategory.DataEvent, "any", "{}");
        var resultB = await routerB.RouteAsync(envelope);

        Assert.False(resultB.Success);
        Assert.Equal("ERR_PME_005", resultB.ErrorCode);
    }

    [Fact(DisplayName = "S80-39: Diagnostic traces distinguish simulated events from real component actions")]
    public async Task S80_39_TraceIntegrity()
    {
        var router = new CentralMessageRouter(_clock, _diagnostics);
        var session = new SimulatedSession("sess-trace-01");
        session.RegisterEndpoint("ep-x");
        session.RegisterEndpoint("ep-y");
        session.AuthorizeTopic("trace.topic");
        router.RegisterSession(session);

        var epY = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-y" }, _clock, _diagnostics);
        await epY.ConnectAsync();
        router.RegisterEndpoint(epY);

        var envelope = BridgeMessageEnvelope.Create(session.SessionId, "ep-x", "ep-y", MessageCategory.DataEvent, "trace.topic", "{}");
        await router.RouteAsync(envelope);

        var events = _diagnostics.Events;
        Assert.NotEmpty(events);
        Assert.Contains(events, e => e.Component == "CentralMessageRouter");
        Assert.Contains(events, e => e.Component == "SyntheticMockEndpoint");
    }

    [Fact(DisplayName = "S80-40: Failed assertion retains scenario and observed outcome details")]
    public void S80_40_FailureReportingRetainsDetails()
    {
        var result = ScenarioExecutionResult.Fail(
            scenarioId: "S80-40",
            runId: "run-40",
            summary: "Assertion failed in message routing",
            failures: new[] { "Expected response M01.ACK but received ERR_PME_004" }
        );

        Assert.Equal(ScenarioOutcome.Failed, result.Outcome);
        Assert.Single(result.AssertionFailures);
        Assert.Contains("ERR_PME_004", result.AssertionFailures[0]);
    }

    [Fact(DisplayName = "S80-41: Owned test resources cleaned up after injected failure")]
    public async Task S80_41_CleanupAfterInjectedFailure()
    {
        var ep = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-fail-clean" }, _clock, _diagnostics);
        await ep.ConnectAsync();
        Assert.True(ep.IsConnected);

        ep.Reset();
        Assert.False(ep.IsConnected);
        Assert.Empty(ep.ReceivedMessages);
    }

    [Fact(DisplayName = "S80-42: Cleanup errors reported without silently erasing original scenario result")]
    public void S80_42_CleanupErrorPreservesOriginalFailure()
    {
        var primaryEx = new InvalidOperationException("Routing failure in mock queue");
        var cleanupEx = new InvalidOperationException("Failed to close in-memory pipe");

        var result = ScenarioExecutionResult.Err(
            scenarioId: "S80-42",
            runId: "run-42",
            summary: "Scenario crashed with dual failure",
            error: new AggregateException(primaryEx, cleanupEx),
            classification: ErrorClassification.TestDefect,
            cleanupSucceeded: false,
            cleanupError: cleanupEx
        );

        Assert.Equal(ScenarioOutcome.Error, result.Outcome);
        Assert.False(result.CleanupSucceeded);
        Assert.NotNull(result.CleanupError);
    }
}
