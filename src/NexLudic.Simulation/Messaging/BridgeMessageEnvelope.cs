using System.Security.Cryptography;
using System.Text;

namespace NexLudic.Simulation.Messaging;

/// <summary>
/// Canonical Bridge Message Envelope conforming to Step 73 (NLE-CON-MEC-001).
/// Immutable data plane structure traversed across all bridge communication.
/// </summary>
public sealed class BridgeMessageEnvelope
{
    public string EnvelopeVersion { get; init; } = "1.0";
    public string MessageId { get; init; } = $"msg-{Guid.NewGuid():N}";
    public string SessionId { get; init; } = string.Empty;
    public MessageCategory Category { get; init; } = MessageCategory.DataEvent;
    public string SenderEndpointId { get; init; } = string.Empty;
    public string RecipientEndpointId { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public string Topic { get; init; } = string.Empty;
    public string SchemaVersion { get; init; } = "1.0.0";
    public long SequenceNumber { get; init; } = 1;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public int TtlMs { get; init; } = 3000;
    public string PayloadDigest { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = "{}";
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    public static string ComputeDigest(string payloadJson)
    {
        if (payloadJson == null) payloadJson = string.Empty;
        byte[] bytes = Encoding.UTF8.GetBytes(payloadJson);
        byte[] hash = SHA256.HashData(bytes);
        return "sha256:" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static BridgeMessageEnvelope Create(
        string sessionId,
        string senderEndpointId,
        string recipientEndpointId,
        MessageCategory category,
        string topic,
        string payloadJson,
        long sequenceNumber = 1,
        string correlationId = "",
        int ttlMs = 3000,
        DateTimeOffset? timestamp = null)
    {
        string digest = ComputeDigest(payloadJson);
        return new BridgeMessageEnvelope
        {
            EnvelopeVersion = "1.0",
            MessageId = $"msg-{Guid.NewGuid():N}",
            SessionId = sessionId,
            Category = category,
            SenderEndpointId = senderEndpointId,
            RecipientEndpointId = recipientEndpointId,
            CorrelationId = string.IsNullOrEmpty(correlationId) ? $"req-{Guid.NewGuid():N}" : correlationId,
            Topic = topic,
            SchemaVersion = "1.0.0",
            SequenceNumber = sequenceNumber,
            TimestampUtc = timestamp ?? DateTimeOffset.UtcNow,
            TtlMs = ttlMs,
            PayloadDigest = digest,
            PayloadJson = payloadJson
        };
    }
}
