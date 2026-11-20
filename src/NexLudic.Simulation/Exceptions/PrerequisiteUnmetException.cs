namespace NexLudic.Simulation.Exceptions;

/// <summary>
/// Thrown when a mandatory prerequisite or upstream dependency for a scenario is unmet.
/// </summary>
public class PrerequisiteUnmetException : Exception
{
    public string ScenarioId { get; }
    public string PrerequisiteName { get; }

    public PrerequisiteUnmetException(string scenarioId, string prerequisiteName, string details)
        : base($"[Prerequisite Unmet in {scenarioId}]: Required '{prerequisiteName}' is missing or unsatisfied. {details}")
    {
        ScenarioId = scenarioId;
        PrerequisiteName = prerequisiteName;
    }
}
