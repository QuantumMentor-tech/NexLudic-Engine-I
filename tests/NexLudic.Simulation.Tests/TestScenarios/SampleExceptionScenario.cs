using NexLudic.Simulation.Contracts;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SampleExceptionScenario : IScenario
{
    public string ScenarioId => "TEST-EX-01";
    public string Title => "Sample Execution Exception Scenario";
    public string Description => "Exercises unhandled execution exception reporting.";
    public string Layer => "Unit";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
    public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

    public bool TeardownExecuted { get; private set; }

    public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated catastrophic engine failure.");
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        TeardownExecuted = true;
        return Task.CompletedTask;
    }
}
