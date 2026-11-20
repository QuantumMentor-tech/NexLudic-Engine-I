# HYBRID INTEGRATION FAILURE CONTAINMENT AND RECOVERY SPECIFICATION

**Document Identifier:** `NLE-SPC-HFC-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Fault Tolerance  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-SPC-SRR-001` (Step 75), `NLE-SPC-RCR-001` (Step 76)  
**Downstream Dependents:** `NLE-POL-HSI-001` (Step 77), `NLE-CON-HOA-001` (Step 77)

---

## 1. Purpose and Scope

This specification defines the twelve failure categories, seven containment scopes, and recovery coordination rules across hybrid integration strategies.

Failures in one integration mechanism must be safely contained to prevent cascading desynchronization storms across unrelated subsystems.

---

## 2. Twelve Formal Failure Categories

```
+------------------------------------+-------------------------------------------------------------------+
| Failure Category                   | Root Cause & Diagnostic Definition                                |
+------------------------------------+-------------------------------------------------------------------+
| 1. `FAIL_COMPONENT_UNAVAILABLE`    | Strategy adapter process crashed or socket closed unexpectedly.   |
| 2. `FAIL_CAPABILITY_LOST`          | Running adapter reports a required capability is no longer viable.|
| 3. `FAIL_AUTH_REVOKED`             | Security token expired or user actively revoked permission.       |
| 4. `FAIL_CONTRACT_INCOMPATIBLE`    | Schema or message envelope mismatch detected at boundary.         |
| 5. `FAIL_MAPPING_ERROR`            | Step 74 SMR translation failed due to unmapped enum or NaN value. |
| 6. `FAIL_SYNCHRONIZATION_GAP`      | Sequence gap in delta stream exceeding replay buffer quota.       |
| 7. `FAIL_CONFLICT_UNRESOLVED`      | Invariant violation or ambiguous authority escalated (Step 76).   |
| 8. `FAIL_HANDOFF_ABORTED`          | Preconditions or baseline transfer failed during strategy handoff.|
| 9. `FAIL_OUTCOME_UNCERTAIN`        | Target adapter timed out; state commit cannot be verified.        |
| 10. `FAIL_RESOURCE_EXHAUSTED`      | Ingestion queue saturated or memory quota exceeded.               |
| 11. `FAIL_SECURITY_VIOLATION`      | Injection attempt, privilege escalation, or tampered vault token. |
| 12. `FAIL_QUALIFICATION_REJECTED`  | End-to-end integration test failed behavioral acceptance rules.   |
+------------------------------------+-------------------------------------------------------------------+
```

---

## 3. Seven Hierarchical Containment Scopes

```
[Level 1: Operation Scope] ----------> Drops/quarantines single message; pipeline stays ACTIVE.
       |
       v
[Level 2: Strategy Instance Scope] --> Restarts or replaces adapter; other strategies continue.
       |
       v
[Level 3: Domain Scope] -------------> Freezes specific domain (e.g. Inventory); Transform continues.
       |
       v
[Level 4: Sub-Component Group Scope] > Suspends dependent group (e.g. Vehicle + Passengers).
       |
       v
[Level 5: Handoff Scope] ------------> Aborts transition; freezes cutover domain.
       |
       v
[Level 6: Hybrid Session Scope] -----> Transitions plan to DEGRADED or RECOVERING.
       |
       v
[Level 7: Game-Pair Scope] ----------> Halts all integration; unlinks bridge; requires user restart.
```

---

## 4. Recovery Coordination with Steps 75 and 76

The orchestrator coordinates recovery without redefining underlying subsystem contracts:
1. **Delta Gaps & Buffer Overflow:** Delegated to Step 75 (`NLE-SPC-SRR-001`) for fresh snapshot reset or bounded replay.
2. **State Conflicts & Invariant Faults:** Delegated to Step 76 (`NLE-SPC-CRR-001`) for authority arbitration and compensation.
3. **No Overlapping Retries:** The orchestrator enforces a shared retry budget (max 3 total retries across all layers) to prevent retry amplification storms.
