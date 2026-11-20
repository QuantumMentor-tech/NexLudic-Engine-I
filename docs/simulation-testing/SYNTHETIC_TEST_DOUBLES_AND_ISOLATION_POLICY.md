# SYNTHETIC TEST DOUBLES AND ISOLATION POLICY SPECIFICATION

**Document Identifier:** `NLE-POL-SIM-001`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** AUTHORITATIVE POLICY & SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior Security Engineer, Senior Simulation Engineer  
**Upstream Dependencies:** `NLE-ARCH-SIM-001`, `NLE-CON-PH9-010`, `NLE-AUD-PH9-001` (Security Audit)  
**Downstream Consumed By:** Simulation harness implementations (Steps 80–87)

---

## 1. Test Double Taxonomy & Definitions

To prevent confusion between modeled behavior and real integrations, the project enforces standard definitions:

| Test Double Type | Definition & Responsibility | Permitted Scope in Phase 10 |
| :--- | :--- | :--- |
| **Stub** | Supplies fixed, canned responses to dependencies. Has no verification logic. | Unit and component tests. |
| **Mock** | Verifies expected interactions, call counts, or parameter values. | Unit, component, and contract tests. |
| **Fake** | Working implementation with simplified in-memory logic (e.g. InMemoryChannel). | Component and simulation tests. |
| **Simulator** | Modeled subsystem reproducing state transitions, timing, and errors. | Simulation layers (Steps 80–86). |
| **Synthetic Endpoint** | In-memory peer endpoint implementing bridge protocol envelopes under test. | Simulation harnesses. |
| **Real Component** | Actual engine production implementation under test. | Integration and E2E tests. |

---

## 2. Synthetic Mock Endpoint Specification (`ISyntheticEndpoint`)

The `SyntheticMockEndpoint` represents a simulated game adapter or bridge endpoint.

### 2.1 Interface Contract
```csharp
public interface ISyntheticEndpoint : IDisposable
{
    string EndpointId { get; }
    SyntheticEndpointConfig Config { get; }
    bool IsConnected { get; }
    IReadOnlyList<SyntheticMessageEnvelope> ReceivedMessages { get; }

    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    Task<SyntheticMessageEnvelope?> SendAndReceiveAsync(SyntheticMessageEnvelope envelope, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    void Reset();
}
```

### 2.2 Pre-Programmed Behaviors
- **`Acknowledge`:** Validates inbound envelope and generates standard `M01.ACK` response envelope.
- **`Reject`:** Generates standard `M02.NACK` response envelope with contract-defined rejection reason.
- **`Echo`:** Returns identical payload in an `M03.ECHO` envelope for round-trip data verification.
- **`Drop`:** Simulates network packet loss by dropping the envelope with no response.
- **`ThrowException`:** Simulates internal adapter crash or fault injection.

---

## 3. Faithfulness Limits & Negative Claims

To maintain evidence integrity:
1. **No Real-Game Guarantees:** A synthetic endpoint that accepts an envelope does NOT prove that a live game or mod loader can parse or apply it.
2. **No Hardware Timing Guarantees:** Virtual clock advancements in simulation do NOT prove real-world network latency or jitter characteristics.
3. **Explicit Labeling:** All simulation test reports must state: `"Simulated Adapter Double: Live Game Verification Deferred"`.

---

## 4. Strict Security and Process Isolation Boundaries

All synthetic test doubles and simulation harnesses must adhere to the following mandatory constraints:
1. **Medium Integrity Level (IL):** All tests execute under standard Windows Medium IL user accounts.
2. **Zero Process Memory Access:** Synthetic endpoints must enforce `DisallowProcessMemoryAccess = true`. Any attempt to call `ReadProcessMemory`, `WriteProcessMemory`, or `CreateRemoteThread` will throw an immediate `InvalidOperationException`.
3. **Zero Wire Sockets:** Default simulation tests execute strictly in-memory or via local named pipe mocks. Zero external TCP/UDP ports are opened.
4. **Anti-Cheat Non-Interference:** Test doubles must never attach debuggers or inject threads into commercial game processes.\n