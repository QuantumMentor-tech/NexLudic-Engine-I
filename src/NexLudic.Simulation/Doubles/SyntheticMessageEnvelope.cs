namespace NexLudic.Simulation.Doubles;

/// <summary>
/// Minimal in-memory message envelope complying with Step 73 Universal Message Protocol specs.
/// Used strictly within simulation test harnesses; zero wire socket.
/// </summary>
public record SyntheticMessageEnvelope(
    string MessageId,
    string SessionId,
    string SourceEndpointId,
    string TargetEndpointId,
    string MessageType,
    long SequenceNumber,
    DateTimeOffset Timestamp,
    string PayloadJson,
    IReadOnlyDictionary<string, string>? Headers = null
);
