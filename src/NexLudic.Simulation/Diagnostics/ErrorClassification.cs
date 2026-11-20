namespace NexLudic.Simulation.Diagnostics;

/// <summary>
/// Root cause classification for scenario failures.
/// </summary>
public enum ErrorClassification
{
    None,
    ProductDefect,
    TestDefect,
    EnvironmentIssue,
    UnresolvedContract,
    ResourceConstraint,
    SecurityBoundaryViolation,
    TeardownFailure
}
