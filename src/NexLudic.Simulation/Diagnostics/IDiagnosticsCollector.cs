namespace NexLudic.Simulation.Diagnostics;

/// <summary>
/// Collects diagnostics, timeline events, and classifications during scenario runs.
/// </summary>
public interface IDiagnosticsCollector
{
    IReadOnlyList<DiagnosticEvent> Events { get; }
    void LogInfo(string component, string message, IReadOnlyDictionary<string, string>? meta = null);
    void LogWarning(string component, string message, IReadOnlyDictionary<string, string>? meta = null);
    void LogError(string component, string message, Exception? ex = null, IReadOnlyDictionary<string, string>? meta = null);
    ErrorClassification ClassifyError(Exception? ex);
}
