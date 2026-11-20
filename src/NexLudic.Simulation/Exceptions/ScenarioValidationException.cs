namespace NexLudic.Simulation.Exceptions;

/// <summary>
/// Thrown when a scenario definition is malformed, missing required metadata, or violates validation rules.
/// </summary>
public class ScenarioValidationException : Exception
{
    public string ScenarioId { get; }

    public ScenarioValidationException(string scenarioId, string message)
        : base($"[Validation Error for Scenario '{scenarioId}']: {message}")
    {
        ScenarioId = scenarioId;
    }
}
