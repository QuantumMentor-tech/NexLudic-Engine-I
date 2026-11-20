namespace NexLudic.Simulation.Contracts;

/// <summary>
/// Authoritative contract for an executable scenario in the NexLudic simulation harness.
/// </summary>
public interface IScenario
{
    /// <summary>Unique stable scenario identifier (e.g. T79-01, B01, G01).</summary>
    string ScenarioId { get; }

    /// <summary>Human-readable scenario title.</summary>
    string Title { get; }

    /// <summary>Detailed description and objective.</summary>
    string Description { get; }

    /// <summary>Testing layer (Unit, Component, Contract, Simulation, Integration, E2E).</summary>
    string Layer { get; }

    /// <summary>Traceable requirement IDs validated by this scenario.</summary>
    IReadOnlyList<string> RequirementIds { get; }

    /// <summary>Capabilities or features required for this scenario to be eligible.</summary>
    IReadOnlyList<string> RequiredCapabilities { get; }

    /// <summary>Prerequisites that must be satisfied before execution.</summary>
    IReadOnlyList<string> Prerequisites { get; }

    /// <summary>Performs isolated scenario setup and fixture initialization.</summary>
    Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default);

    /// <summary>Executes the scenario logic and evaluates assertions.</summary>
    Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default);

    /// <summary>Performs failure-safe cleanup of scenario-allocated state.</summary>
    Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default);
}
