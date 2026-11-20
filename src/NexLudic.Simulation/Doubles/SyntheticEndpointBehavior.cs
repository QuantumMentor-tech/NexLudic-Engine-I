namespace NexLudic.Simulation.Doubles;

/// <summary>
/// Pre-programmed behavior modes for synthetic test doubles and mock endpoints.
/// </summary>
public enum SyntheticEndpointBehavior
{
    /// <summary>Normal acknowledgement with successful status.</summary>
    Acknowledge,

    /// <summary>Explicit rejection with contract-defined error code.</summary>
    Reject,

    /// <summary>Simulated packet loss / drop (no response).</summary>
    Drop,

    /// <summary>Inject artificial latency before response.</summary>
    Delay,

    /// <summary>Echo payload back to sender for round-trip verification.</summary>
    Echo,

    /// <summary>Throw internal simulation exception.</summary>
    ThrowException
}
