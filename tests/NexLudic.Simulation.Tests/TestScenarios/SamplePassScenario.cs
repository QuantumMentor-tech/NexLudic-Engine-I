using NexLudic.Simulation.Contracts;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SamplePassScenario : IScenario
{
    public string ScenarioId => "TEST-PASS-01";
    public string Title => "Sample Successful Scenario";
    public string Description => "Exercises full lifecycle setup, execute, and teardown with passing assertion.";
    public string Layer => "Unit";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
    public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

    public bool SetupExecuted { get; private set; }
    public bool ExecuteExecuted { get; private set; }
    public bool TeardownExecuted { get; private set; }

    public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        SetupExecuted = true;
        context.SetState("Initialized", true);
        return Task.CompletedTask;
    }

    public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        ExecuteExecuted = true;
        bool isInit = context.GetRequiredState<bool>("Initialized");
        if (!isInit)
        {
            return Task.FromResult(ScenarioExecutionResult.Fail(ScenarioId, context.RunId, "Setup was not initialized", new[] { "Initialized state missing" }));
        }
        return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "Sample scenario completed successfully."));
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        TeardownExecuted = true;
        return Task.CompletedTask;
    }
}
