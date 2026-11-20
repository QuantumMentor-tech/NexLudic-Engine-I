using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Exceptions;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Tests.TestScenarios;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class IsolationAndCleanupTests
{
    [Fact(DisplayName = "T79-10: Teardown is attempted after assertion failure")]
    public async Task T79_10_TeardownAttemptedAfterAssertionFailure()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleAssertionFailureScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Failed, result.Outcome);
        Assert.True(scenario.TeardownExecuted, "Teardown was not attempted following assertion failure.");
    }

    [Fact(DisplayName = "T79-11: Resources created before setup failure are handled cleanly")]
    public async Task T79_11_PartialSetupFailureCleansResources()
    {
        var engine = new TestHarnessEngine();
        var scenario = new PartialSetupFailureScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Error, result.Outcome);
        Assert.True(scenario.ResourceDisposed, "Resource tracked during partial setup was not disposed.");
        Assert.True(scenario.TeardownExecuted, "Teardown was not executed after setup failure.");
    }

    [Fact(DisplayName = "T79-12: Independent scenarios do not leak mutable state across contexts")]
    public async Task T79_12_StateIsolationBetweenScenarios()
    {
        var engine = new TestHarnessEngine();
        var scenarioA = new SampleIsolationScenario("ISO-A");
        var scenarioB = new SampleIsolationScenario("ISO-B");
        engine.Register(scenarioA);
        engine.Register(scenarioB);

        var resultA = await engine.RunScenarioAsync(scenarioA.ScenarioId);
        var resultB = await engine.RunScenarioAsync(scenarioB.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, resultA.Outcome);
        Assert.Equal(ScenarioOutcome.Passed, resultB.Outcome);
        Assert.Equal("ValueFrom_ISO-A", scenarioA.ObservedValue);
        Assert.Equal("ValueFrom_ISO-B", scenarioB.ObservedValue);
    }

    [Fact(DisplayName = "T79-13: Temporary resources and directories are cleaned up")]
    public async Task T79_13_TemporaryResourceCleanup()
    {
        var engine = new TestHarnessEngine();
        var scenario = new TempFileScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.CleanupSucceeded);
        Assert.False(Directory.Exists(scenario.ObservedWorkingDir), "Scenario scratch directory was not cleaned up.");
    }

    [Fact(DisplayName = "T79-14: Cleanup failure preserves both failure information and original result")]
    public async Task T79_14_CleanupFailurePreservedInReport()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleTeardownFailureScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Error, result.Outcome);
        Assert.NotNull(result.Error);
        Assert.Contains("Teardown failed", result.Error.Message);
    }

    private class PartialSetupFailureScenario : IScenario
    {
        public string ScenarioId => "PARTIAL-SETUP-01";
        public string Title => "Partial Setup Failure";
        public string Description => "Tests disposal of early resources when setup fails halfway.";
        public string Layer => "Unit";
        public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

        public bool ResourceDisposed { get; private set; }
        public bool TeardownExecuted { get; private set; }

        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            var trackedDisposable = new DummyDisposable(() => ResourceDisposed = true);
            context.TrackDisposable(trackedDisposable);

            throw new InvalidOperationException("Failed during latter half of setup.");
        }

        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, ""));

        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            TeardownExecuted = true;
            return Task.CompletedTask;
        }
    }

    private class TempFileScenario : IScenario
    {
        public string ScenarioId => "TEMP-FILE-01";
        public string Title => "Temp File Scenario";
        public string Description => "Creates file in working dir and verifies automated cleanup.";
        public string Layer => "Unit";
        public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

        public string ObservedWorkingDir { get; private set; } = "";

        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            ObservedWorkingDir = context.WorkingDirectory;
            File.WriteAllText(Path.Combine(context.WorkingDirectory, "scratch.txt"), "temporary test content");
            return Task.CompletedTask;
        }

        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
        {
            bool exists = File.Exists(Path.Combine(context.WorkingDirectory, "scratch.txt"));
            Assert.True(exists);
            return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "File verified."));
        }

        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private class DummyDisposable : IDisposable
    {
        private readonly Action _onDispose;
        public DummyDisposable(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }
}
