using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Exceptions;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Tests.TestScenarios;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class ScenarioExecutionTests
{
    [Fact(DisplayName = "T79-05: Successful scenario executes setup, actions, assertions, and teardown")]
    public async Task T79_05_SuccessfulScenarioExecutesFullLifecycle()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SamplePassScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.IsSuccess);
        Assert.True(scenario.SetupExecuted, "Setup was not executed");
        Assert.True(scenario.ExecuteExecuted, "Execute was not executed");
        Assert.True(scenario.TeardownExecuted, "Teardown was not executed");
    }

    [Fact(DisplayName = "T79-06: Assertion failure produces failed result")]
    public async Task T79_06_AssertionFailureProducesFailedResult()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleAssertionFailureScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Failed, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorClassification.ProductDefect, result.Classification);
        Assert.NotEmpty(result.AssertionFailures);
        Assert.True(scenario.TeardownExecuted, "Teardown must execute after assertion failure");
    }

    [Fact(DisplayName = "T79-07: Unexpected exception reported distinctly from assertion failure")]
    public async Task T79_07_ExecutionExceptionReportedDistinctly()
    {
        var engine = new TestHarnessEngine();
        var scenario = new SampleExceptionScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Error, result.Outcome);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Simulated catastrophic engine failure.", result.Error.Message);
        Assert.True(scenario.TeardownExecuted, "Teardown must execute after unhandled exception");
    }

    [Fact(DisplayName = "T79-08: Malformed scenario definition is rejected before execution")]
    public void T79_08_InvalidScenarioDefinitionRejected()
    {
        var engine = new TestHarnessEngine();

        // Registering scenario with empty ScenarioId throws ScenarioValidationException
        Assert.Throws<ScenarioValidationException>(() =>
        {
            var invalidScenario = new MalformedScenario();
            engine.Register(invalidScenario);
        });
    }

    [Fact(DisplayName = "T79-09: Unknown scenario identifier reports error without claiming test ran")]
    public async Task T79_09_UnknownScenarioIdentifierReportsError()
    {
        var engine = new TestHarnessEngine();

        var result = await engine.RunScenarioAsync("NON_EXISTENT_ID");

        Assert.Equal(ScenarioOutcome.Error, result.Outcome);
        Assert.Equal(ErrorClassification.TestDefect, result.Classification);
        Assert.Contains("Unknown scenario identifier", result.Summary);
    }

    private class MalformedScenario : IScenario
    {
        public string ScenarioId => ""; // Malformed!
        public string Title => "Malformed";
        public string Description => "Desc";
        public string Layer => "Unit";
        public IReadOnlyList<string> RequirementIds => Array.Empty<string>();
        public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
        public IReadOnlyList<string> Prerequisites => Array.Empty<string>();
        public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default) =>
            Task.FromResult(ScenarioExecutionResult.Pass("", "", ""));
        public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
