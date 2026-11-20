using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Exceptions;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SampleAssertionFailureScenario : IScenario
{
    public string ScenarioId => "TEST-FAIL-01";
    public string Title => "Sample Assertion Failure Scenario";
    public string Description => "Exercises assertion failure handling and teardown continuation.";
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
        throw new ScenarioAssertionException(ScenarioId, "Value mismatch in simulated payload", expected: "100", actual: "42");
    }

    public Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        TeardownExecuted = true;
        return Task.CompletedTask;
    }
}
