namespace NexLudic.Simulation.Determinism;

/// <summary>
/// Controllable virtual clock providing deterministic timestamps for simulations.
/// </summary>
public sealed class DeterministicClock : IDeterministicClock
{
    private DateTimeOffset _currentTime;

    public DeterministicClock(DateTimeOffset? initialTime = null)
    {
        _currentTime = initialTime ?? new DateTimeOffset(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
    }

    public DateTimeOffset UtcNow => _currentTime;

    public void Advance(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delta), "Cannot move virtual clock backward.");
        _currentTime = _currentTime.Add(delta);
    }

    public void SetTime(DateTimeOffset time)
    {
        _currentTime = time;
    }
}
