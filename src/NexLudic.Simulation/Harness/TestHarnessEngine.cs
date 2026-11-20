using System.Collections.Concurrent;
using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Exceptions;

namespace NexLudic.Simulation.Harness;

/// <summary>
/// Top-level test harness engine providing scenario registration, execution orchestration,
/// and aggregated test reporting for Phase 10.
/// </summary>
public sealed class TestHarnessEngine : IScenarioRegistry
{
    private readonly ConcurrentDictionary<string, IScenario> _scenarios = new();
    private readonly ScenarioLifecycleManager _lifecycleManager;
    private readonly TestHarnessConfiguration _config;
    private readonly ConcurrentDictionary<string, bool> _availablePrerequisites = new();

    public TestHarnessConfiguration Configuration => _config;

    public TestHarnessEngine(TestHarnessConfiguration? config = null)
    {
        _config = config ?? new TestHarnessConfiguration();
        _config.Validate();
        _lifecycleManager = new ScenarioLifecycleManager(_config);
    }

    public void SetPrerequisiteAvailable(string prerequisiteName, bool isAvailable = true)
    {
        _availablePrerequisites[prerequisiteName] = isAvailable;
    }

    public void Register(IScenario scenario)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));
        if (string.IsNullOrWhiteSpace(scenario.ScenarioId))
            throw new ScenarioValidationException("UNKNOWN", "ScenarioId cannot be empty.");

        _scenarios[scenario.ScenarioId] = scenario;
    }

    public bool TryGet(string scenarioId, out IScenario? scenario)
    {
        return _scenarios.TryGetValue(scenarioId, out scenario);
    }

    public IReadOnlyList<IScenario> GetAll()
    {
        return _scenarios.Values.OrderBy(s => s.ScenarioId).ToArray();
    }

    public IReadOnlyList<IScenario> GetByLayer(string layer)
    {
        return _scenarios.Values.Where(s => string.Equals(s.Layer, layer, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    public async Task<ScenarioExecutionResult> RunScenarioAsync(string scenarioId, int? overrideSeed = null, CancellationToken cancellationToken = default)
    {
        if (!TryGet(scenarioId, out var scenario) || scenario == null)
        {
            return ScenarioExecutionResult.Err(
                scenarioId, $"run-{Guid.NewGuid():N}",
                $"Unknown scenario identifier '{scenarioId}'.",
                new KeyNotFoundException($"Scenario '{scenarioId}' is not registered in the harness engine."),
                Diagnostics.ErrorClassification.TestDefect);
        }

        return await _lifecycleManager.RunScenarioAsync(scenario, overrideSeed, _availablePrerequisites, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ScenarioExecutionResult>> RunAllAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<ScenarioExecutionResult>();
        foreach (var scenario in GetAll())
        {
            var result = await _lifecycleManager.RunScenarioAsync(scenario, null, _availablePrerequisites, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }
        return results;
    }
}
