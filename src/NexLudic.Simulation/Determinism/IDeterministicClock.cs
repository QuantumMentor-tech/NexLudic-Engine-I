namespace NexLudic.Simulation.Determinism;

/// <summary>
/// Contract for a deterministic time source in simulations and tests.
/// Avoids non-deterministic reliance on DateTime.UtcNow or Thread.Sleep.
/// </summary>
public interface IDeterministicClock
{
    DateTimeOffset UtcNow { get; }
    void Advance(TimeSpan delta);
    void SetTime(DateTimeOffset time);
}
