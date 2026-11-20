namespace NexLudic.Simulation.Messaging;

/// <summary>
/// Authoritative message lifecycle states defined by Step 73 (NLE-SPC-MRD-001).
/// Enforces: Created != Validated != Accepted != Transmitted != Received != Processing != Completed != Verified.
/// </summary>
public enum MessageLifecycleState
{
    Created,
    Validated,
    Accepted,
    Transmitted,
    Received,
    Processing,
    Completed,
    Uncertain,
    Verified,
    Failed
}

