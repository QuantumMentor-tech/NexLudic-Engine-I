namespace NexLudic.Simulation.Harness;

/// <summary>
/// Configuration for the simulation test harness engine.
/// </summary>
public sealed class TestHarnessConfiguration
{
    public string EnvironmentProfile { get; set; } = "SimulationIsolated";
    public int DefaultSeed { get; set; } = 42_79_100;
    public string BaseScratchDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "NexLudicSimHarness");
    public bool EnforceMediumIntegrityOnly { get; set; } = true;
    public bool AutoCleanupOnComplete { get; set; } = true;
    public TimeSpan DefaultScenarioTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(EnvironmentProfile))
            throw new InvalidOperationException("EnvironmentProfile must not be empty.");
        if (string.IsNullOrWhiteSpace(BaseScratchDirectory))
            throw new InvalidOperationException("BaseScratchDirectory must not be empty.");
    }
}
