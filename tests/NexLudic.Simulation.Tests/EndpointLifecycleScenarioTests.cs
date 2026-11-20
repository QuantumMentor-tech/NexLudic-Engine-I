using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class EndpointLifecycleScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;

    public EndpointLifecycleScenarioTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
    }

    [Fact(DisplayName = "S80-01: Synthetic endpoint initializes successfully with valid configuration")]
    public void S80_01_SyntheticEndpointInitialization()
    {
        var config = new SyntheticEndpointConfig
        {
            EndpointId = "inst-minecraft-01",
            AdapterType = "MockMinecraftAdapter",
            ProtocolVersion = "1.0.0"
        };

        var endpoint = new SyntheticMockEndpoint(config, _clock, _diagnostics);

        Assert.Equal("inst-minecraft-01", endpoint.EndpointId);
        Assert.False(endpoint.IsConnected);
        Assert.Empty(endpoint.ReceivedMessages);
    }

    [Fact(DisplayName = "S80-02: Invalid endpoint configuration produces clear failure")]
    public void S80_02_InvalidEndpointConfigurationThrows()
    {
        var config = new SyntheticEndpointConfig
        {
            EndpointId = "unsafe-endpoint",
            DisallowProcessMemoryAccess = false // Violates non-invasive execution policy!
        };

        Assert.Throws<InvalidOperationException>(() => new SyntheticMockEndpoint(config, _clock, _diagnostics));
    }

    [Fact(DisplayName = "S80-03: Endpoint initialization and readiness remain distinct")]
    public async Task S80_03_EndpointReadinessDistinctFromInitialization()
    {
        var config = new SyntheticEndpointConfig { EndpointId = "inst-hl2-01" };
        var endpoint = new SyntheticMockEndpoint(config, _clock, _diagnostics);

        Assert.False(endpoint.IsConnected, "Endpoint must not be ready upon raw initialization.");

        await endpoint.ConnectAsync();
        Assert.True(endpoint.IsConnected, "Endpoint should be ready/connected only after explicit ConnectAsync.");
    }

    [Fact(DisplayName = "S80-04: Endpoint shutdown releases owned resources cleanly")]
    public async Task S80_04_EndpointShutdownReleasesResources()
    {
        var config = new SyntheticEndpointConfig { EndpointId = "inst-hl2-01" };
        var endpoint = new SyntheticMockEndpoint(config, _clock, _diagnostics);

        await endpoint.ConnectAsync();
        Assert.True(endpoint.IsConnected);

        await endpoint.DisconnectAsync();
        Assert.False(endpoint.IsConnected);
    }

    [Fact(DisplayName = "S80-05: Repeated cleanup follows lifecycle contract without hiding errors")]
    public void S80_05_RepeatedCleanupSafe()
    {
        var config = new SyntheticEndpointConfig { EndpointId = "inst-repeat-01" };
        var endpoint = new SyntheticMockEndpoint(config, _clock, _diagnostics);

        endpoint.Reset();
        endpoint.Dispose();
        endpoint.Dispose(); // Idempotent second disposal

        Assert.False(endpoint.IsConnected);
        Assert.Empty(endpoint.ReceivedMessages);
    }
}
