namespace NexLudic.Simulation.Doubles;

/// <summary>
/// Interface for synthetic endpoints operating within Phase 10 simulation test harnesses.
/// Explicitly isolated from live game processes.
/// </summary>
public interface ISyntheticEndpoint : IDisposable
{
    string EndpointId { get; }
    SyntheticEndpointConfig Config { get; }
    bool IsConnected { get; }
    IReadOnlyList<SyntheticMessageEnvelope> ReceivedMessages { get; }

    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<SyntheticMessageEnvelope?> SendAndReceiveAsync(SyntheticMessageEnvelope envelope, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    void Reset();
}
