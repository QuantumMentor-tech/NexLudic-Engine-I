# HYBRID INTEGRATION CAPABILITY COMPOSITION AND SELECTION POLICY

**Document Identifier:** `NLE-POL-HCS-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Capability Governance  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-CON-HIP-001` (Step 77), `NLE-SPC-HDE-001` (Step 77)  
**Downstream Dependents:** `NLE-SPC-CSR-001` (Step 77), `NLE-SPC-CSH-001` (Step 77)

---

## 1. Purpose and Scope

This policy governs the six capability evidence levels, capability-based strategy composition rules, selection algorithms, and runtime capability change handling for hybrid integrations.

Integration requirements must be expressed in terms of abstract capabilities (e.g. `CAP_SNAPSHOT_CREATE`), not tied prematurely to specific game hacks. This policy ensures strategies are composed based on verifiable evidence.

---

## 2. Six Formal Capability Evidence Levels

The engine distinguishes six levels of capability proof that **shall never** be conflated:

```
+-----------------------------------+-------------------------------------------------------------------+
| Evidence Level                    | Verification Standard & Evidentiary Proof                         |
+-----------------------------------+-------------------------------------------------------------------+
| **Level 1: DECLARED**             | Capability claimed in adapter manifest; unverified by engine.     |
| **Level 2: IMPLEMENTED**          | Code/module exists; interface methods pass unit tests.            |
| **Level 3: VERIFIED**             | Capability validated in isolated synthetic test harness (Step 75).|
| **Level 4: AUTHORIZED**           | Explicit user/admin security token grants permission to execute.  |
| **Level 5: OPERATIONAL**          | Capability actively running and responsive in live session.       |
| **Level 6: QUALIFIED**            | End-to-end game pair verified interoperable under target workload.|
+-----------------------------------+-------------------------------------------------------------------+
```

**Inviolable Invariant:** A plan requires **Level 4 (Authorized)** and **Level 3 (Verified)** before activation. Level 1 (Declared) claims **shall never** satisfy a mandatory plan dependency.

---

## 3. Capability Composition and Selection Algorithm

When compiling an integration plan for a game pair:
1. **Identify Required Capabilities:** The mashup profile generates required capability set $C_{req} = \{c_1, c_2, \dots, c_n\}$.
2. **Evaluate Strategy Candidates:** For each available strategy $S_k$, query verified capability set $C(S_k)$.
3. **Compute Minimal Covering Composition:** Select minimal subset of strategies $\{S_1, \dots, S_m\}$ such that:
   $$\bigcup_{j=1}^{m} C(S_j) \supseteq C_{req}$$
   And no two strategies hold overlapping write capabilities for the same domain unless governed by an approved partitioning contract.
4. **Fail-Closed on Unmet Capabilities:** If $\bigcup C(S_j) \subset C_{req}$, the orchestrator refuses plan activation and emits `ERR_HYBRID_CAPABILITY_UNSATISFIED`.

---

## 4. Handling Runtime Capability Loss

If an active strategy loses a capability during a live session (e.g. host game crashes, adapter times out, or read-back is disabled):
1. **Immediate Quarantine:** The affected capability is flagged as `CAPABILITY_LOST`.
2. **Dependency Suspension:** All dependent components in the dependency graph are immediately transitioned to `SUSPENDED`.
3. **Fallback Evaluation:** The orchestrator checks whether an approved Fallback Strategy exists covering the lost capability.
   - If YES: Initiate Controlled Handoff (Section 9).
   - If NO: Transition hybrid session to `DEGRADED_MODE` or `BLOCKED`.
