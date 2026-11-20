using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Tests.TestScenarios;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class SyntheticIntegrationBoundariesTests
{
    [Fact(DisplayName = "T79-19: Simulated endpoint satisfies declared test contract (ACK)")]
    public async Task T79_19_SimulatedEndpointSuccess()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleEndpointScenario("EP-ACK-01", SyntheticEndpointBehavior.Acknowledge);
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.IsSuccess);
        Assert.Contains("Received expected ACK envelope", result.Summary);
    }

    [Fact(DisplayName = "T79-20: Simulated endpoint rejection is propagated and classified correctly")]
    public async Task T79_20_SimulatedEndpointRejection()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleEndpointScenario("EP-REJ-01", SyntheticEndpointBehavior.Reject);
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.Contains("Received expected NACK envelope", result.Summary);
    }

    [Fact(DisplayName = "T79-21: Default harness run does not open, modify, or inspect live game process")]
    public void T79_21_NoImplicitRealGameAccessEnforced()
    {
        var config = new SyntheticEndpointConfig
        {
            EndpointId = "safe-endpoint-01",
            DisallowProcessMemoryAccess = false // Attempting to allow memory access violates security policy!
        };

        var clock = new Determinism.DeterministicClock();
        var diagnostics = new Diagnostics.DiagnosticsCollector(clock);

        // Security boundary check: Must throw InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => new SyntheticMockEndpoint(config, clock, diagnostics));
    }

    [Fact(DisplayName = "T79-22: Missing authoritative integration contract is reported as limitation")]
    public async Task T79_22_ContractLimitationReportedAsLimitation()
    {
        var engine = new TestHarnessEngine();
        var scenario = new MissingContractScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Blocked, result.Outcome);
        Assert.Equal(Diagnostics.ErrorClassification.UnresolvedContract, result.Classification);
        Assert.Contains("UNRESOLVED_LIVE_GAME_BINARY_HOOK_V9", result.Summary);
    }

    private class MissingContractScenario : IScenario
    {
        public string ScenarioId => "MISSING-CONTRACT-01";
        public string Title => "Missing Contract Scenario";
        public string Description => "Tests reporting of an unresolved integration contract.";
        public string Layer => "Contract";
        public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => new[] { "UNRESOLVED_LIVE_GAME_BINARY_HOOK_V9" };

        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, ""));
        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
