namespace NexLudic.Simulation.Doubles;

/// <summary>
/// Configuration for a synthetic test double endpoint.
/// </summary>
public sealed class SyntheticEndpointConfig
{
    public string EndpointId { get; set; } = "endpoint-sim-01";
    public string AdapterType { get; set; } = "MockTestAdapter";
    public string ProtocolVersion { get; set; } = "1.0.0";
    public SyntheticEndpointBehavior DefaultBehavior { get; set; } = SyntheticEndpointBehavior.Acknowledge;
    public TimeSpan SimulatedLatency { get; set; } = TimeSpan.Zero;
    public string RejectionReason { get; set; } = "Simulated rejection by test harness";
    public bool EnforceMediumIntegrityOnly { get; set; } = true;
    public bool DisallowProcessMemoryAccess { get; set; } = true;
}
