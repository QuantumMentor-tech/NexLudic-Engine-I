# CROSS-GAME CONCURRENCY AND STALE-WRITE PROTECTION SPECIFICATION

**Document Identifier:** `NLE-SPC-CSW-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Concurrency Control  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-SID-001` (Step 75), `NLE-POL-SVI-001` (Step 75)  
**Downstream Dependents:** `NLE-CON-RAV-001` (Step 76), `NLE-POL-RIU-001` (Step 76)

---

## 1. Purpose and Scope

This specification defines the concurrency control protocols, Time-of-Check to Time-of-Use (TOCTOU) race protections, and stale-write prevention mechanisms used during conflict resolution.

A resolution decision that is valid when evaluated can become dangerous by the time it is applied if the target game's simulation state has advanced in the interim. This document establishes strict atomic preconditions to prevent overwriting newer legitimate state.

---

## 2. Mandatory Precondition Tokens

Every resolution write operation dispatched to a target adapter must carry a cryptographically signed **Precondition Token**:

```json
{
  "precondition_token_id": "tok:precond:9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "sync_session_id": "5f3c7e42-1234-4567-89ab-cdef01234567",
  "session_epoch": 3,
  "domain_id": "domain:hero_transform",
  "expected_target_revision": 1042,
  "expected_baseline_hash": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "evaluation_timestamp_monotonic_ns": 18446744073709551615,
  "token_ttl_ms": 100
}
```

---

## 3. TOCTOU Invalidation Triggers

A prepared resolution operation is **instantly invalidated** before application if any of the following events occur between evaluation and execution:

```
+-----------------------------------+-------------------------------------------------------------------+
| Event / Trigger                   | Invalidation Action                                               |
+-----------------------------------+-------------------------------------------------------------------+
| 1. Target Revision Advanced       | Target revision > expected_target_revision -> Abort immediately.  |
| 2. New In-Flight Delta Arrived    | Delta targeting domain received -> Re-evaluate conflict candidate.|
| 3. Session Epoch Incremented      | Epoch mismatch -> Discard operation; reset session baseline.      |
| 4. Authority Shift Occurred       | Ownership transferred to another endpoint -> Revoke write rights. |
| 5. Token TTL Expired              | Time delta > token_ttl_ms (100ms) -> Expire token; re-evaluate.   |
+-----------------------------------+-------------------------------------------------------------------+
```

---

## 4. Per-Domain Concurrency Control

To prevent deadlocks while ensuring linearizable resolution per concept:
1. **Per-Domain Lock:** The coordinator acquires an exclusive lock scoped strictly to the target domain (`domain_id`). Unrelated domains process deltas concurrently without blocking.
2. **Optimistic Concurrency:** If locking is unsupported by the target adapter, the operation is executed optimistically using the Precondition Token. If the target reports a revision mismatch, the operation fails safely without partial state mutation (`ERR_CONFLICT_STALE_WRITE_PREVENTED`).
