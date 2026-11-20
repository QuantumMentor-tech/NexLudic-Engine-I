using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Tests.TestScenarios;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class HarnessInitializationTests
{
    [Fact(DisplayName = "T79-01: Valid harness initialization succeeds with valid configuration")]
    public void T79_01_ValidHarnessInitialization()
    {
        var config = new TestHarnessConfiguration
        {
            EnvironmentProfile = "SimulationIsolated",
            DefaultSeed = 12345,
            BaseScratchDirectory = Path.Combine(Path.GetTempPath(), "NexLudicTestHarness_Valid")
        };

        var engine = new TestHarnessEngine(config);

        Assert.NotNull(engine);
        Assert.Equal("SimulationIsolated", engine.Configuration.EnvironmentProfile);
        Assert.Equal(12345, engine.Configuration.DefaultSeed);
    }

    [Fact(DisplayName = "T79-02: Invalid configuration produces clear error rather than misleading pass")]
    public void T79_02_InvalidConfigurationThrows()
    {
        var config = new TestHarnessConfiguration
        {
            EnvironmentProfile = "" // Invalid
        };

        Assert.Throws<InvalidOperationException>(() => new TestHarnessEngine(config));
    }

    [Fact(DisplayName = "T79-03: Missing optional dependency does not break independent test")]
    public async Task T79_03_MissingOptionalDependencyDoesNotBreakTest()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SamplePassScenario();
        engine.Register(scenario);

        // Optional capabilities not configured, but scenario requires none
        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "T79-04: Unsupported mandatory prerequisite reports scenario as blocked")]
    public async Task T79_04_UnsupportedMandatoryPrerequisiteReportsBlocked()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SamplePrerequisiteScenario(); // Requires PHASE_9_STATE_SYNC_CONTRACT_V1
        engine.Register(scenario);

        // Do NOT set prerequisite to true
        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Blocked, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Contains("blocked", result.Summary, StringComparison.OrdinalIgnoreCase);
        Assert.False(scenario.SetupExecuted); // Setup must NOT be executed when blocked
    }
}
