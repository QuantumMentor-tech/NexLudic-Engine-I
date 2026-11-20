namespace NexLudic.Simulation.Sessions;

/// <summary>
/// In-memory representation of an active bridge session established under Step 72.
/// </summary>
public sealed class SimulatedSession
{
    private readonly HashSet<string> _participatingEndpoints = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _authorizedTopics = new(StringComparer.OrdinalIgnoreCase);

    public string SessionId { get; }
    public long SessionEpoch { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyCollection<string> ParticipatingEndpoints => _participatingEndpoints;
    public IReadOnlyCollection<string> AuthorizedTopics => _authorizedTopics;

    public SimulatedSession(string sessionId, long initialEpoch = 1, DateTimeOffset? createdAt = null)
    {
        SessionId = string.IsNullOrWhiteSpace(sessionId) ? throw new ArgumentNullException(nameof(sessionId)) : sessionId;
        SessionEpoch = initialEpoch;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public void RegisterEndpoint(string endpointId)
    {
        if (!string.IsNullOrWhiteSpace(endpointId))
            _participatingEndpoints.Add(endpointId);
    }

    public void AuthorizeTopic(string topic)
    {
        if (!string.IsNullOrWhiteSpace(topic))
            _authorizedTopics.Add(topic);
    }

    public bool IsEndpointRegistered(string endpointId) =>
        _participatingEndpoints.Contains(endpointId);

    public bool IsTopicAuthorized(string topic) =>
        _authorizedTopics.Count == 0 || _authorizedTopics.Contains(topic) || _authorizedTopics.Contains("*");

    public void AdvanceEpoch()
    {
        SessionEpoch++;
    }

    public void Invalidate()
    {
        IsActive = false;
    }
}

