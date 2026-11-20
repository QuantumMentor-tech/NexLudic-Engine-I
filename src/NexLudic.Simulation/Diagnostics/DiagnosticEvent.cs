namespace NexLudic.Simulation.Diagnostics;

/// <summary>
/// Structured record of an event or log entry captured during scenario execution.
/// Sanitized to prevent leaking tokens, secrets, or raw memory.
/// </summary>
public record DiagnosticEvent(
    DateTimeOffset Timestamp,
    string Level,
    string Component,
    string Message,
    IReadOnlyDictionary<string, string>? Metadata = null
);
