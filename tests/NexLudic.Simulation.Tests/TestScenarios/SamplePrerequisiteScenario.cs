using NexLudic.Simulation.Contracts;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SamplePrerequisiteScenario : IScenario
{
    public string ScenarioId => "TEST-PREREQ-01";
    public string Title => "Sample Prerequisite Scenario";
    public string Description => "Exercises prerequisite verification and blocking.";
    public string Layer => "Contract";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
    public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites => new[] { "PHASE_9_STATE_SYNC_CONTRACT_V1" };

    public bool SetupExecuted { get; private set; }

    public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        SetupExecuted = true;
        return Task.CompletedTask;
    }

    public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "Prerequisite was met."));
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
