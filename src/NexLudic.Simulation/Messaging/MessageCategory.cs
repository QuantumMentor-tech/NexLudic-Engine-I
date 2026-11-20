namespace NexLudic.Simulation.Messaging;

/// <summary>
/// Authoritative message categories defined by Step 73 (NLE-CON-MEC-001).
/// </summary>
public enum MessageCategory
{
    ControlLifecycle,
    CommandRequest,
    CommandResponse,
    DataEvent,
    Acknowledgement,
    ErrorSignal
}
