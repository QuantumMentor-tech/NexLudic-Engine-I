using NexLudic.Simulation.Diagnostics;

namespace NexLudic.Simulation.Contracts;

/// <summary>
/// Detailed result of a scenario execution, capturing outcome, diagnostics, metrics, and error classification.
/// </summary>
public sealed class ScenarioExecutionResult
{
    public string ScenarioId { get; }
    public string RunId { get; }
    public ScenarioOutcome Outcome { get; }
    public string Summary { get; }
    public Exception? Error { get; }
    public ErrorClassification Classification { get; }
    public IReadOnlyList<string> AssertionFailures { get; }
    public IReadOnlyDictionary<string, object> Metadata { get; }
    public IReadOnlyList<DiagnosticEvent> DiagnosticEvents { get; }
    public TimeSpan Duration { get; }
    public bool CleanupSucceeded { get; }
    public Exception? CleanupError { get; }

    public bool IsSuccess => Outcome == ScenarioOutcome.Passed;

    public ScenarioExecutionResult(
        string scenarioId,
        string runId,
        ScenarioOutcome outcome,
        string summary,
        Exception? error = null,
        ErrorClassification classification = ErrorClassification.None,
        IReadOnlyList<string>? assertionFailures = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        IReadOnlyList<DiagnosticEvent>? diagnosticEvents = null,
        TimeSpan? duration = null,
        bool cleanupSucceeded = true,
        Exception? cleanupError = null)
    {
        ScenarioId = scenarioId;
        RunId = runId;
        Outcome = outcome;
        Summary = summary;
        Error = error;
        Classification = classification;
        AssertionFailures = assertionFailures ?? Array.Empty<string>();
        Metadata = metadata ?? new Dictionary<string, object>();
        DiagnosticEvents = diagnosticEvents ?? Array.Empty<DiagnosticEvent>();
        Duration = duration ?? TimeSpan.Zero;
        CleanupSucceeded = cleanupSucceeded;
        CleanupError = cleanupError;
    }

    public static ScenarioExecutionResult Pass(
        string scenarioId,
        string runId,
        string summary,
        IReadOnlyDictionary<string, object>? metadata = null,
        IReadOnlyList<DiagnosticEvent>? events = null,
        TimeSpan? duration = null,
        bool cleanupSucceeded = true)
    {
        return new ScenarioExecutionResult(
            scenarioId, runId, ScenarioOutcome.Passed, summary,
            metadata: metadata, diagnosticEvents: events, duration: duration, cleanupSucceeded: cleanupSucceeded);
    }

    public static ScenarioExecutionResult Fail(
        string scenarioId,
        string runId,
        string summary,
        IReadOnlyList<string> failures,
        Exception? error = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        IReadOnlyList<DiagnosticEvent>? events = null,
        TimeSpan? duration = null,
        bool cleanupSucceeded = true,
        Exception? cleanupError = null)
    {
        return new ScenarioExecutionResult(
            scenarioId, runId, ScenarioOutcome.Failed, summary,
            error: error, classification: ErrorClassification.ProductDefect,
            assertionFailures: failures, metadata: metadata, diagnosticEvents: events,
            duration: duration, cleanupSucceeded: cleanupSucceeded, cleanupError: cleanupError);
    }

    public static ScenarioExecutionResult Block(
        string scenarioId,
        string runId,
        string summary,
        Exception? error = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        IReadOnlyList<DiagnosticEvent>? events = null)
    {
        return new ScenarioExecutionResult(
            scenarioId, runId, ScenarioOutcome.Blocked, summary,
            error: error, classification: ErrorClassification.UnresolvedContract,
            metadata: metadata, diagnosticEvents: events);
    }

    public static ScenarioExecutionResult Skip(
        string scenarioId,
        string runId,
        string summary,
        IReadOnlyDictionary<string, object>? metadata = null)
    {
        return new ScenarioExecutionResult(
            scenarioId, runId, ScenarioOutcome.Skipped, summary,
            classification: ErrorClassification.None, metadata: metadata);
    }

    public static ScenarioExecutionResult Err(
        string scenarioId,
        string runId,
        string summary,
        Exception error,
        ErrorClassification classification,
        IReadOnlyDictionary<string, object>? metadata = null,
        IReadOnlyList<DiagnosticEvent>? events = null,
        TimeSpan? duration = null,
        bool cleanupSucceeded = true,
        Exception? cleanupError = null)
    {
        return new ScenarioExecutionResult(
            scenarioId, runId, ScenarioOutcome.Error, summary,
            error: error, classification: classification,
            metadata: metadata, diagnosticEvents: events, duration: duration,
            cleanupSucceeded: cleanupSucceeded, cleanupError: cleanupError);
    }
}
