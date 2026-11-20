using System.Collections.Concurrent;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Messaging;

namespace NexLudic.Simulation.Routing;

/// <summary>
/// Request-response correlator implementing Step 73 RRC-03.
/// Tracks pending RPC command requests by correlation_id with virtual clock timeout checks.
/// </summary>
public sealed class RequestResponseCorrelator
{
    private record PendingRequest(BridgeMessageEnvelope RequestEnvelope, DateTimeOffset DeadlineUtc);

    private readonly ConcurrentDictionary<string, PendingRequest> _pending = new(StringComparer.Ordinal);
    private readonly IDeterministicClock _clock;

    public RequestResponseCorrelator(IDeterministicClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public void RegisterRequest(BridgeMessageEnvelope request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.CorrelationId)) return;

        DateTimeOffset deadline = request.TimestampUtc.AddMilliseconds(request.TtlMs);
        _pending[request.CorrelationId] = new PendingRequest(request, deadline);
    }

    public bool TryMatchResponse(BridgeMessageEnvelope response, out BridgeMessageEnvelope? originatingRequest, out bool isExpired)
    {
        originatingRequest = null;
        isExpired = false;

        if (response == null || string.IsNullOrWhiteSpace(response.CorrelationId)) return false;

        if (_pending.TryRemove(response.CorrelationId, out var pending))
        {
            originatingRequest = pending.RequestEnvelope;
            isExpired = _clock.UtcNow > pending.DeadlineUtc;
            return true;
        }

        return false;
    }

    public int PendingCount => _pending.Count;

    public void Reset()
    {
        _pending.Clear();
    }
}

