using System.Collections.Concurrent;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;

namespace NexLudic.Simulation.Doubles;

/// <summary>
/// In-memory synthetic mock adapter endpoint.
/// Strictly executes at Medium Integrity Level, in-memory only. Zero live game socket, zero ReadProcessMemory.
/// </summary>
public sealed class SyntheticMockEndpoint : ISyntheticEndpoint
{
    private readonly ConcurrentBag<SyntheticMessageEnvelope> _receivedMessages = new();
    private readonly IDeterministicClock _clock;
    private readonly IDiagnosticsCollector _diagnostics;
    private long _responseSequence = 0;

    public string EndpointId => Config.EndpointId;
    public SyntheticEndpointConfig Config { get; }
    public bool IsConnected { get; private set; }
    public IReadOnlyList<SyntheticMessageEnvelope> ReceivedMessages => _receivedMessages.ToArray();

    public SyntheticMockEndpoint(
        SyntheticEndpointConfig config,
        IDeterministicClock clock,
        IDiagnosticsCollector diagnostics)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));

        if (!Config.DisallowProcessMemoryAccess)
        {
            throw new InvalidOperationException("Security violation: Synthetic endpoints must enforce DisallowProcessMemoryAccess.");
        }
    }

    public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        _diagnostics.LogInfo("SyntheticMockEndpoint", $"Endpoint {EndpointId} connected in simulation mode.");
        return Task.FromResult(true);
    }

    public Task<SyntheticMessageEnvelope?> SendAndReceiveAsync(SyntheticMessageEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException($"Endpoint {EndpointId} is not connected.");
        }

        _receivedMessages.Add(envelope);
        _diagnostics.LogInfo("SyntheticMockEndpoint", $"Received envelope {envelope.MessageId} from {envelope.SourceEndpointId}");

        if (Config.SimulatedLatency > TimeSpan.Zero)
        {
            _clock.Advance(Config.SimulatedLatency);
        }

        SyntheticMessageEnvelope? response = Config.DefaultBehavior switch
        {
            SyntheticEndpointBehavior.Acknowledge => new SyntheticMessageEnvelope(
                MessageId: Guid.NewGuid().ToString("N"),
                SessionId: envelope.SessionId,
                SourceEndpointId: EndpointId,
                TargetEndpointId: envelope.SourceEndpointId,
                MessageType: "M01.ACK",
                SequenceNumber: Interlocked.Increment(ref _responseSequence),
                Timestamp: _clock.UtcNow,
                PayloadJson: "{\"status\": \"ACK\", \"acknowledgedMessageId\": \"" + envelope.MessageId + "\"}"
            ),

            SyntheticEndpointBehavior.Reject => new SyntheticMessageEnvelope(
                MessageId: Guid.NewGuid().ToString("N"),
                SessionId: envelope.SessionId,
                SourceEndpointId: EndpointId,
                TargetEndpointId: envelope.SourceEndpointId,
                MessageType: "M02.NACK",
                SequenceNumber: Interlocked.Increment(ref _responseSequence),
                Timestamp: _clock.UtcNow,
                PayloadJson: "{\"status\": \"REJECTED\", \"reason\": \"" + Config.RejectionReason + "\"}"
            ),

            SyntheticEndpointBehavior.Echo => new SyntheticMessageEnvelope(
                MessageId: Guid.NewGuid().ToString("N"),
                SessionId: envelope.SessionId,
                SourceEndpointId: EndpointId,
                TargetEndpointId: envelope.SourceEndpointId,
                MessageType: "M03.ECHO",
                SequenceNumber: Interlocked.Increment(ref _responseSequence),
                Timestamp: _clock.UtcNow,
                PayloadJson: envelope.PayloadJson
            ),

            SyntheticEndpointBehavior.Drop => null,

            SyntheticEndpointBehavior.ThrowException => throw new InvalidOperationException($"Simulated endpoint fault on {EndpointId}: Internal processing error."),

            _ => throw new NotSupportedException($"Behavior {Config.DefaultBehavior} not supported.")
        };

        if (Config.DefaultBehavior == SyntheticEndpointBehavior.Drop)
        {
            _diagnostics.LogWarning("SyntheticMockEndpoint", $"Simulating packet drop for message {envelope.MessageId}");
        }

        return Task.FromResult(response);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        _diagnostics.LogInfo("SyntheticMockEndpoint", $"Endpoint {EndpointId} disconnected.");
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _receivedMessages.Clear();
        _responseSequence = 0;
        IsConnected = false;
    }

    public void Dispose()
    {
        Reset();
    }
}
