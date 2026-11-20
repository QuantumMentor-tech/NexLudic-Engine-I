namespace NexLudic.Simulation.Determinism;

/// <summary>
/// Provides seed tracking and deterministic pseudo-random generators for reproducible simulations.
/// </summary>
public sealed class ReproducibleSeedProvider
{
    public int MasterSeed { get; }
    private readonly Random _random;

    public ReproducibleSeedProvider(int? seed = null)
    {
        MasterSeed = seed ?? 42_79_100;
        _random = new Random(MasterSeed);
    }

    public int Next() => _random.Next();
    public int Next(int maxValue) => _random.Next(maxValue);
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
    public double NextDouble() => _random.NextDouble();

    public byte[] NextBytes(int count)
    {
        var buffer = new byte[count];
        _random.NextBytes(buffer);
        return buffer;
    }

    public Random CreateChildRandom(string childContext)
    {
        int childSeed = HashCode.Combine(MasterSeed, childContext);
        return new Random(childSeed);
    }
}
