using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Harness;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class ReproducibilityAndOutcomesTests
{
    [Fact(DisplayName = "T79-15: Controlled synthetic input produces expected repeatable result")]
    public void T79_15_ControlledSyntheticInputProducesDeterministicOutput()
    {
        var seedProviderA = new ReproducibleSeedProvider(9999);
        var seedProviderB = new ReproducibleSeedProvider(9999);

        var bytesA = seedProviderA.NextBytes(32);
        var bytesB = seedProviderB.NextBytes(32);

        Assert.Equal(bytesA, bytesB);
    }

    [Fact(DisplayName = "T79-16: Randomized fixture generation records seed in test results")]
    public async Task T79_16_SeedRecordingInResult()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SeedVerifyingScenario();
        engine.Register(scenario);

        int customSeed = 778899;
        var result = await engine.RunScenarioAsync(scenario.ScenarioId, overrideSeed: customSeed);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.Metadata.ContainsKey("Seed"));
        Assert.Equal(customSeed, (int)result.Metadata["Seed"]);
    }

    [Fact(DisplayName = "T79-17: Result metadata identifies scenario, outcome, and relevant diagnostics")]
    public async Task T79_17_ResultMetadataComplete()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SeedVerifyingScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.NotNull(result.ScenarioId);
        Assert.NotNull(result.RunId);
        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.NotEmpty(result.DiagnosticEvents);
        Assert.True(result.Duration >= TimeSpan.Zero);
    }

    [Fact(DisplayName = "T79-18: Recorded failing synthetic scenario can be reproduced using documented config")]
    public async Task T79_18_FailureReproductionViaSeed()
    {
        var engine = new TestHarnessEngine();
        var scenario = new PseudoRandomFailingScenario();
        engine.Register(scenario);

        int failingSeed = 10101;
        var run1 = await engine.RunScenarioAsync(scenario.ScenarioId, overrideSeed: failingSeed);
        var run2 = await engine.RunScenarioAsync(scenario.ScenarioId, overrideSeed: failingSeed);

        Assert.Equal(ScenarioOutcome.Failed, run1.Outcome);
        Assert.Equal(ScenarioOutcome.Failed, run2.Outcome);
        Assert.Equal(run1.Summary, run2.Summary);
    }

    private class SeedVerifyingScenario : IScenario
    {
        public string ScenarioId => "SEED-VERIFY-01";
        public string Title => "Seed Verification Scenario";
        public string Description => "Verifies seed propagation into execution context.";
        public string Layer => "Unit";
        public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            context.Diagnostics.LogInfo("SeedVerifyingScenario", $"Executed with seed: {context.Seed}");
            return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, $"Verified seed: {context.Seed}"));
        }

        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class PseudoRandomFailingScenario : IScenario
    {
        public string ScenarioId => "RANDOM-FAIL-01";
        public string Title => "Pseudo-Random Failing Scenario";
        public string Description => "Deterministically fails based on seed value.";
        public string Layer => "Unit";
        public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            var seedGen = new ReproducibleSeedProvider(context.Seed);
            int value = seedGen.Next(1000);

            if (context.Seed == 10101)
            {
                return Task.FromResult(ScenarioExecutionResult.Fail(ScenarioId, context.RunId, $"Deterministic failure on seed {context.Seed} with value {value}", new[] { $"Failing seed triggered value: {value}" }));
            }

            return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, $"Success with value {value}"));
        }

        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
