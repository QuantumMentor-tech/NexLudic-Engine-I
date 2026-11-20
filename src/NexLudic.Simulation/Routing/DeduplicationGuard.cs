using System.Collections.Concurrent;

namespace NexLudic.Simulation.Routing;

/// <summary>
/// Deduplication and replay guard implementing Step 73 DED-04.
/// Enforces monotonic sequence numbers per sender and detects duplicate message deliveries.
/// </summary>
public sealed class DeduplicationGuard
{
    private readonly ConcurrentDictionary<string, bool> _seenMessageIds = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, long> _highestSequencePerSender = new(StringComparer.OrdinalIgnoreCase);

    public enum SequenceEvaluation
    {
        ValidNext,
        DuplicateMessageId,
        SequenceRegression,
        OutOfOrderGap
    }

    public SequenceEvaluation CheckAndRecord(string senderEndpointId, string messageId, long sequenceNumber)
    {
        // 1. Check duplicate message_id
        if (!_seenMessageIds.TryAdd(messageId, true))
        {
            return SequenceEvaluation.DuplicateMessageId;
        }

        // 2. Check sequence number monotonicity
        long highest = _highestSequencePerSender.GetOrAdd(senderEndpointId, 0);

        if (sequenceNumber <= highest && highest > 0)
        {
            return SequenceEvaluation.SequenceRegression;
        }

        if (sequenceNumber > highest + 1 && highest > 0)
        {
            _highestSequencePerSender[senderEndpointId] = sequenceNumber;
            return SequenceEvaluation.OutOfOrderGap;
        }

        _highestSequencePerSender[senderEndpointId] = sequenceNumber;
        return SequenceEvaluation.ValidNext;
    }

    public bool IsDuplicate(string messageId) => _seenMessageIds.ContainsKey(messageId);

    public void Reset()
    {
        _seenMessageIds.Clear();
        _highestSequencePerSender.Clear();
    }
}

