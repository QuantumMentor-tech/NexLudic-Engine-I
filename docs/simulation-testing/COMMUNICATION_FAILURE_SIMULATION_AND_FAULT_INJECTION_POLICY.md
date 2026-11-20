# COMMUNICATION FAILURE SIMULATION AND FAULT INJECTION POLICY

**Document Identifier:** `NLE-POL-SIM-002`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 80 of 100  
**Status:** AUTHORITATIVE POLICY & SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior Reliability Engineer, Senior Security Engineer  
**Upstream Dependencies:** `NLE-SPEC-SIM-002`, `NLE-POL-SIM-001`, `NLE-AUD-PH9-002` (Reliability Audit)  
**Downstream Consumed By:** Simulation test suites across Phase 10

---

## 1. Principles of Controlled Fault Simulation

Distributed communication is inherently subject to partial failures, packet drops, clock drift, disconnections, and delayed acknowledgments. The simulation harness must reproduce these edge cases deterministically without introducing unpredictable multi-threaded race conditions or unconstrained network traffic.

---

## 2. Declarative Fault Injection Controls (`CommunicationFaultConfig`)

Scenarios configure fault injection rules declaratively per send operation:

```csharp
public sealed class CommunicationFaultConfig
{
    public bool DropNextDelivery { get; set; }
    public bool SimulateRecipientUnavailable { get; set; }
    public TimeSpan InjectedLatency { get; set; }
    public bool DuplicateNextDelivery { get; set; }
    public bool CorruptPayloadDigest { get; set; }
    public bool RejectNextSend { get; set; }
    public string RejectionErrorCode { get; set; }
    public string RejectionMessage { get; set; }
    public bool DisconnectDuringExchange { get; set; }
    public bool WithholdAcknowledgement { get; set; }
    public int MaxRetries { get; set; }
    public bool SimulateUncertainOutcome { get; set; }
}
```

### 2.1 Fault Injection Semantics
- **`DropNextDelivery`:** Message is admitted and processed by router, but discarded before delivery to recipient channel. Result state: `Uncertain`.
- **`SimulateRecipientUnavailable`:** Recipient lookup fails with `ERR_PME_007`. Result state: `Failed`.
- **`InjectedLatency`:** Advances virtual clock (`IDeterministicClock`) by the specified delta without sleeping the host thread.
- **`DuplicateNextDelivery`:** Delivers the identical envelope twice to recipient to verify idempotency and deduplication guards.
- **`CorruptPayloadDigest`:** Mutates payload digest to verify Stage 6 checksum rejection (`ERR_PME_003`).
- **`WithholdAcknowledgement`:** Recipient processes message, but acknowledgment envelope is omitted. Verifies client handling of absent ACK.
- **`SimulateUncertainOutcome`:** Flags that transport transmission succeeded, but application-level state modification cannot be verified without telemetry receipts.

---

## 3. The Uncertain Outcome Invariant

The core tenet of distributed game state interoperability:
$$\text{Missing ACK} \neq \text{Failed Operation}$$

When an acknowledgment is dropped or withheld:
1. The test harness records result state as `Uncertain`, NOT `Failed`.
2. The sender must NOT automatically replay non-idempotent operations without querying server state or checking correlation tokens.
3. Automated test assertions verify that clients do not trigger duplicate state mutations under unconfirmed outcomes.

---

## 4. Security & Resource Isolation Policy

1. **No Real Network Sockets:** All fault injection operates strictly on in-memory collections and named pipe doubles.
2. **Deterministic Cleanup:** At scenario completion, `faults.Reset()` and `router.Reset()` clear all fault rules, queues, and correlation entries.
3. **Bounded Retries:** Retry loops are capped at a maximum of 3 attempts by default to prevent runaway spin-locks.
