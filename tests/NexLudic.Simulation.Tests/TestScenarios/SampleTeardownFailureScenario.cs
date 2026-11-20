using NexLudic.Simulation.Contracts;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SampleTeardownFailureScenario : IScenario
{
    public string ScenarioId => "TEST-TEARDOWN-FAIL-01";
    public string Title => "Sample Teardown Failure Scenario";
    public string Description => "Exercises teardown exception capture without swallowing execution results.";
    public string Layer => "Unit";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
    public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

    public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "Execution succeeded before teardown failure."));
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Failed to release external mock resource during teardown.");
    }
}
