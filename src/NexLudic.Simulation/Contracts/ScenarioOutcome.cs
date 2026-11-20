namespace NexLudic.Simulation.Contracts;

/// <summary>
/// Authoritative outcome states for scenario execution in the NexLudic simulation harness.
/// Aligns with Step 79 Outcome Integrity standards.
/// </summary>
public enum ScenarioOutcome
{
    /// <summary>All assertions passed and scenario completed successfully.</summary>
    Passed,

    /// <summary>One or more verified assertions failed.</summary>
    Failed,

    /// <summary>An unexpected exception occurred during execution.</summary>
    Error,

    /// <summary>Scenario was explicitly skipped due to non-blocking criteria.</summary>
    Skipped,

    /// <summary>Scenario was blocked due to unmet mandatory prerequisites or missing contracts.</summary>
    Blocked,

    /// <summary>Scenario was registered but not executed.</summary>
    NotExecuted,

    /// <summary>Execution finished but a valid pass/fail verdict could not be reached.</summary>
    Inconclusive
}
