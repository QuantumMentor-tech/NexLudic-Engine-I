# HYBRID INTEGRATION FALLBACK AND DEGRADED OPERATION POLICY

**Document Identifier:** `NLE-POL-HFD-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Resilience & Safety  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-CON-HIP-001` (Step 77), `NLE-POL-HDC-001` (Step 77)  
**Downstream Dependents:** `NLE-SPC-HSL-001` (Step 77), `NLE-SPC-HFC-001` (Step 77)

---

## 1. Purpose and Scope

This policy governs the conditions, authorization gates, and operational boundaries for **Planned Fallback Transitions** and **Degraded Operating Modes**.

Resilience must never come at the cost of security or state integrity. This policy enforces strict fail-closed constraints against silent privilege escalation and mandates transparent disclosure of reduced guarantees during degraded operation.

---

## 2. Inviolable Anti-Escalation Rule (Fail-Closed on Privilege Elevation)

$$\textbf{A failure in a lower-privilege strategy SHALL NEVER automatically activate a higher-privilege strategy.}$$

### Concrete Enforcement Boundaries:
1. **Bridge Failure $\to$ Mod Escalation Prohibited:** If a live bridge socket disconnects, the engine **shall never** attempt to write files into the game directory or execute binary patchers as a fallback unless explicit user authorization is provided at runtime.
2. **Read-Only Failure $\to$ Write Escalation Prohibited:** A failure in an observational telemetry stream **shall never** escalate to active state injection.
3. **Unsigned Fallback Refusal:** Any fallback destination not explicitly pre-approved and cryptographically signed in the active Hybrid Integration Plan is strictly rejected (`ERR_HYBRID_UNAUTHORIZED_FALLBACK`).

---

## 3. Approved Degraded Operating Modes

When a non-critical supporting strategy fails and no seamless fallback exists, the plan may enter an approved Degraded Mode:

```
+-----------------------------------+-----------------------------------+-----------------------------------+
| Degraded Mode Designation         | Active Capabilities Retained      | Suspended / Weakened Guarantees   |
+-----------------------------------+-----------------------------------+-----------------------------------+
| 1. `MODE_TELEMETRY_ONLY`          | Read-only state display / radar.  | Active state injection disabled.  |
| 2. `MODE_ASYNC_EVENTUAL`          | Discrete quest/event sync active. | Real-time 60Hz physics suspended. |
| 3. `MODE_PARTITIONED_ISOLATED`    | Healthy domains continue (Health).| Corrupted domains frozen (Inv).   |
| 4. `MODE_STATIC_CONTENT_ONLY`     | Host-game mod assets visible.     | Live bridge completely halted.    |
+-----------------------------------+-----------------------------------+-----------------------------------+
```

### Degraded Mode Transparency Mandate:
- The UI surfaces an amber/yellow warning badge: `INTEGRATION_DEGRADED: [Mode Name]`.
- System telemetry records `sync_guarantees_weakened: true`.
- Downstream systems are prohibited from reporting full game-pair qualification while operating in degraded mode.

---

## 4. Failback Protocol (Returning to Primary Strategy)

Returning to a primary strategy after recovery is an explicit transition, not an automatic assumption:
1. **Primary Health Verification:** The primary strategy must prove Level 5 (Operational) readiness for at least $T_{soak} = 5.0$ seconds.
2. **State Reconciliation:** Reconcile all state mutations that occurred during fallback operation using Step 75 snapshot alignment.
3. **Handoff Sequence Execution:** Execute full 15-stage handoff sequence from fallback instance back to primary instance.
4. **Restricting Fallback:** Demote fallback instance to passive standby before resuming full primary throughput.
