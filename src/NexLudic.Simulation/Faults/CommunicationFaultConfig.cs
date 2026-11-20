namespace NexLudic.Simulation.Faults;

/// <summary>
/// Declarative fault injection configuration for communication scenarios.
/// Scoped strictly to scenario-defined synthetic endpoints.
/// </summary>
public sealed class CommunicationFaultConfig
{
    public bool DropNextDelivery { get; set; }
    public bool SimulateRecipientUnavailable { get; set; }
    public TimeSpan InjectedLatency { get; set; } = TimeSpan.Zero;
    public bool DuplicateNextDelivery { get; set; }
    public bool CorruptPayloadDigest { get; set; }
    public bool RejectNextSend { get; set; }
    public string RejectionErrorCode { get; set; } = "ERR_PME_010";
    public string RejectionMessage { get; set; } = "Simulated delivery rejection";
    public bool DisconnectDuringExchange { get; set; }
    public bool WithholdAcknowledgement { get; set; }
    public int MaxRetries { get; set; } = 3;
    public bool SimulateUncertainOutcome { get; set; }

    public void Reset()
    {
        DropNextDelivery = false;
        SimulateRecipientUnavailable = false;
        InjectedLatency = TimeSpan.Zero;
        DuplicateNextDelivery = false;
        CorruptPayloadDigest = false;
        RejectNextSend = false;
        RejectionErrorCode = "ERR_PME_010";
        RejectionMessage = "Simulated delivery rejection";
        DisconnectDuringExchange = false;
        WithholdAcknowledgement = false;
        MaxRetries = 3;
        SimulateUncertainOutcome = false;
    }
}
