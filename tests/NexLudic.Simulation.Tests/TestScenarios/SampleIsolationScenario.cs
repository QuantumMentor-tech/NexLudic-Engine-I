using NexLudic.Simulation.Contracts;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SampleIsolationScenario : IScenario
{
    public string ScenarioId { get; }
    public string Title => $"Isolation Scenario {ScenarioId}";
    public string Description => "Exercises context state isolation between runs.";
    public string Layer => "Unit";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001" };
    public IReadOnlyList<string> RequiredCapabilities => Array.Empty<string>();
    public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

    public string? ObservedValue { get; private set; }

    public SampleIsolationScenario(string id)
    {
        ScenarioId = id;
    }

    public Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        // Try getting state that might have leaked from another scenario
        if (context.TryGetState<string>("ContaminatedKey", out var val))
        {
            ObservedValue = val;
            return Task.FromResult(ScenarioExecutionResult.Fail(ScenarioId, context.RunId, "Contaminated state detected", new[] { $"State key leaked: {val}" }));
        }

        // Write own state
        context.SetState("ContaminatedKey", $"ValueFrom_{ScenarioId}");
        ObservedValue = $"ValueFrom_{ScenarioId}";

        return Task.FromResult(ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "State isolated cleanly."));
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
