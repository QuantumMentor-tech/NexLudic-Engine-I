namespace NexLudic.Simulation.Exceptions;

/// <summary>
/// Thrown when an error occurs during scenario teardown or resource release.
/// </summary>
public class TeardownException : Exception
{
    public string ScenarioId { get; }

    public TeardownException(string scenarioId, string message, Exception? innerException = null)
        : base($"[Teardown Failed in {scenarioId}]: {message}", innerException)
    {
        ScenarioId = scenarioId;
    }
}
