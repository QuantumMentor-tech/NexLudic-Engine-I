namespace NexLudic.Simulation.Exceptions;

/// <summary>
/// Thrown when an explicit assertion within a scenario fails.
/// </summary>
public class ScenarioAssertionException : Exception
{
    public string ScenarioId { get; }
    public string Expected { get; }
    public string Actual { get; }

    public ScenarioAssertionException(string scenarioId, string message, string expected = "", string actual = "")
        : base($"[Assertion Failed in {scenarioId}]: {message} (Expected: '{expected}', Actual: '{actual}')")
    {
        ScenarioId = scenarioId;
        Expected = expected;
        Actual = actual;
    }
}
