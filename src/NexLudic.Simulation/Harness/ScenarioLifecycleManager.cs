using System.Diagnostics;
using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Exceptions;

namespace NexLudic.Simulation.Harness;

/// <summary>
/// Oversees the deterministic, isolated, failure-safe execution lifecycle of a scenario.
/// </summary>
public sealed class ScenarioLifecycleManager
{
    private readonly TestHarnessConfiguration _config;

    public ScenarioLifecycleManager(TestHarnessConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _config.Validate();
    }

    public async Task<ScenarioExecutionResult> RunScenarioAsync(
        IScenario scenario,
        int? overrideSeed = null,
        IReadOnlyDictionary<string, bool>? availablePrerequisites = null,
        CancellationToken cancellationToken = default)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));

        string runId = $"run-{Guid.NewGuid():N}";
        int seed = overrideSeed ?? _config.DefaultSeed;
        var clock = new DeterministicClock();
        var diagnostics = new DiagnosticsCollector(clock);
        string scenarioScratchDir = Path.Combine(_config.BaseScratchDirectory, $"NexLudicSim_{scenario.ScenarioId}_{runId}");

        // Validate Scenario Definition
        if (string.IsNullOrWhiteSpace(scenario.ScenarioId))
        {
            return ScenarioExecutionResult.Err(
                "UNKNOWN", runId, "Scenario validation failed: ScenarioId is empty.",
                new ScenarioValidationException("UNKNOWN", "ScenarioId cannot be empty."),
                ErrorClassification.TestDefect);
        }

        if (string.IsNullOrWhiteSpace(scenario.Title))
        {
            return ScenarioExecutionResult.Err(
                scenario.ScenarioId, runId, "Scenario validation failed: Title is empty.",
                new ScenarioValidationException(scenario.ScenarioId, "Scenario Title cannot be empty."),
                ErrorClassification.TestDefect);
        }

        // Check Prerequisites
        if (scenario.Prerequisites != null && scenario.Prerequisites.Count > 0)
        {
            foreach (var prereq in scenario.Prerequisites)
            {
                bool isSatisfied = availablePrerequisites != null &&
                                  availablePrerequisites.TryGetValue(prereq, out bool ok) && ok;
                if (!isSatisfied)
                {
                    diagnostics.LogWarning("ScenarioLifecycle", $"Prerequisite '{prereq}' is unsatisfied. Blocking execution.");
                    return ScenarioExecutionResult.Block(
                        scenario.ScenarioId, runId,
                        $"Scenario blocked because mandatory prerequisite '{prereq}' is not satisfied.",
                        new PrerequisiteUnmetException(scenario.ScenarioId, prereq, "Contract not available in simulation."),
                        new Dictionary<string, object> { ["UnmetPrerequisite"] = prereq },
                        diagnostics.Events);
                }
            }
        }

        Directory.CreateDirectory(scenarioScratchDir);
        var context = new ScenarioContext(scenario.ScenarioId, runId, seed, clock, diagnostics, scenarioScratchDir);

        var stopwatch = Stopwatch.StartNew();
        Exception? executionException = null;
        ScenarioExecutionResult? rawResult = null;
        bool setupSucceeded = false;

        try
        {
            // Stage 1: Setup
            diagnostics.LogInfo("ScenarioLifecycle", $"Starting SetupAsync for {scenario.ScenarioId}");
            await scenario.SetupAsync(context, cancellationToken).ConfigureAwait(false);
            setupSucceeded = true;

            // Stage 2: Execution
            diagnostics.LogInfo("ScenarioLifecycle", $"Starting ExecuteAsync for {scenario.ScenarioId}");
            rawResult = await scenario.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (ScenarioAssertionException ex)
        {
            executionException = ex;
            diagnostics.LogError("ScenarioLifecycle", $"Assertion failure in {scenario.ScenarioId}: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            executionException = ex;
            diagnostics.LogError("ScenarioLifecycle", $"Unhandled execution error in {scenario.ScenarioId}: {ex.Message}", ex);
        }
        finally
        {
            // Stage 3: Teardown (ALWAYS attempted even after setup or execution failure)
            diagnostics.LogInfo("ScenarioLifecycle", $"Starting TeardownAsync for {scenario.ScenarioId}");
            try
            {
                await scenario.TeardownAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception teardownEx)
            {
                diagnostics.LogError("ScenarioLifecycle", $"Teardown error in {scenario.ScenarioId}: {teardownEx.Message}", teardownEx);
                if (executionException == null)
                {
                    executionException = new TeardownException(scenario.ScenarioId, "Teardown failed", teardownEx);
                }
                else
                {
                    // Preserve both execution exception and teardown exception
                    executionException = new AggregateException(
                        $"Scenario {scenario.ScenarioId} experienced both execution and teardown failures.",
                        executionException, teardownEx);
                }
            }

            // Stage 4: Context cleanup
            if (_config.AutoCleanupOnComplete)
            {
                await context.CleanupAsync().ConfigureAwait(false);
            }
        }

        stopwatch.Stop();

        // Stage 5: Compile Final Result
        if (executionException != null)
        {
            var classification = diagnostics.ClassifyError(executionException);
            if (executionException is ScenarioAssertionException assertEx)
            {
                return ScenarioExecutionResult.Fail(
                    scenario.ScenarioId, runId, assertEx.Message,
                    new[] { assertEx.Message }, assertEx,
                    new Dictionary<string, object> { ["Seed"] = seed },
                    diagnostics.Events, stopwatch.Elapsed, context.CleanupSucceeded, context.CleanupError);
            }
            return ScenarioExecutionResult.Err(
                scenario.ScenarioId, runId, executionException.Message,
                executionException, classification,
                new Dictionary<string, object> { ["Seed"] = seed },
                diagnostics.Events, stopwatch.Elapsed, context.CleanupSucceeded, context.CleanupError);
        }

        if (rawResult != null)
        {
            // Wrap or enrich result with lifecycle metadata
            var meta = new Dictionary<string, object>(rawResult.Metadata)
            {
                ["Seed"] = seed,
                ["SetupSucceeded"] = setupSucceeded
            };

            return new ScenarioExecutionResult(
                scenario.ScenarioId, runId, rawResult.Outcome, rawResult.Summary,
                rawResult.Error, rawResult.Classification, rawResult.AssertionFailures,
                meta, diagnostics.Events, stopwatch.Elapsed,
                context.CleanupSucceeded, context.CleanupError);
        }

        return ScenarioExecutionResult.Skip(scenario.ScenarioId, runId, "Scenario yielded no execution verdict.");
    }
}
