using System.Collections.Concurrent;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Exceptions;

namespace NexLudic.Simulation.Diagnostics;

/// <summary>
/// Thread-safe collector for scenario execution diagnostics.
/// </summary>
public sealed class DiagnosticsCollector : IDiagnosticsCollector
{
    private readonly ConcurrentQueue<DiagnosticEvent> _events = new();
    private readonly IDeterministicClock _clock;

    public DiagnosticsCollector(IDeterministicClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public IReadOnlyList<DiagnosticEvent> Events => _events.ToArray();

    public void LogInfo(string component, string message, IReadOnlyDictionary<string, string>? meta = null)
    {
        _events.Enqueue(new DiagnosticEvent(_clock.UtcNow, "INFO", component, message, meta));
    }

    public void LogWarning(string component, string message, IReadOnlyDictionary<string, string>? meta = null)
    {
        _events.Enqueue(new DiagnosticEvent(_clock.UtcNow, "WARN", component, message, meta));
    }

    public void LogError(string component, string message, Exception? ex = null, IReadOnlyDictionary<string, string>? meta = null)
    {
        string fullMessage = ex != null ? $"{message} (Exception: {ex.GetType().Name} - {ex.Message})" : message;
        _events.Enqueue(new DiagnosticEvent(_clock.UtcNow, "ERROR", component, fullMessage, meta));
    }

    public ErrorClassification ClassifyError(Exception? ex)
    {
        if (ex == null) return ErrorClassification.None;

        return ex switch
        {
            ScenarioAssertionException => ErrorClassification.ProductDefect,
            PrerequisiteUnmetException => ErrorClassification.UnresolvedContract,
            TeardownException => ErrorClassification.TeardownFailure,
            ScenarioValidationException => ErrorClassification.TestDefect,
            UnauthorizedAccessException => ErrorClassification.SecurityBoundaryViolation,
            OutOfMemoryException => ErrorClassification.ResourceConstraint,
            _ => ErrorClassification.TestDefect
        };
    }
}
