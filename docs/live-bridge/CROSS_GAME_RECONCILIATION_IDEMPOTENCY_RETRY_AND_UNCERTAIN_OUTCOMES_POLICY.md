# CROSS-GAME RECONCILIATION IDEMPOTENCY, RETRY AND UNCERTAIN OUTCOMES POLICY

**Document Identifier:** `NLE-POL-RIU-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Fault Tolerance  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-RAV-001` (Step 76), `NLE-POL-DRR-001` (Step 73)  
**Downstream Dependents:** `NLE-SPC-RCR-001` (Step 76), `NLE-CON-COA-001` (Step 76)

---

## 1. Purpose and Scope

This policy governs the unique identification, idempotency guarantees, retry constraints, and dedicated handling protocols for **Uncertain Application Outcomes** during conflict reconciliation.

When an operation is dispatched across a live bridge to a host game adapter, network glitches or host game timing issues can result in unacknowledged requests. Mechanically replaying non-idempotent operations or guessing their outcome risks double-applying item grants, compounding physics forces, or corrupting state. This policy provides strict, bounded handling rules.

---

## 2. Reconciliation Operation Identity

Every reconciliation operation carries a globally unique, immutable operation descriptor:

```json
{
  "reconcile_op_id": "op:rec:7d9b2e1a-4567-89ab-cdef-0123456789ab",
  "conflict_id": "c7a8b9d0-1234-5678-9abc-def012345678",
  "policy_id": "pol:res:ue5_unity:transform_override:v1.0.0",
  "sync_session_id": "5f3c7e42-1234-4567-89ab-cdef01234567",
  "session_epoch": 3,
  "target_domain_id": "domain:hero_transform",
  "operation_hash": "sha256:d41d8cd98f00b204e9800998ecf8427e",
  "attempt_index": 1,
  "max_attempts": 3
}
```

---

## 3. Strict Retry Eligibility Matrix

| Operation Category | Idempotency Level | Permitted Retry Behavior | Precondition Revalidation Required? |
| :--- | :---: | :--- | :---: |
| **SET_VALUE / OVERRIDE** | Fully Idempotent | Retry allowed up to 3 times with exponential backoff. | YES (Verify target revision has not moved). |
| **DELTA_PATCH** | Non-Idempotent | Retry **strictly forbidden** if previous outcome uncertain. | YES (Require fresh baseline check). |
| **RESOURCE_ADD / SPEND** | Non-Idempotent | Retry **strictly forbidden** without confirmation of failure. | YES (Require full accounting audit). |
| **STATE_TRANSITION** | Conditional | Permitted only if target verified still in predecessor state. | YES (Verify target state). |
| **ENTITY_SPAWN / DESPAWN** | Non-Idempotent | Retry forbidden without explicit tombstone verification. | YES. |

---

## 4. Handling Unknown Application Outcomes (`OUTCOME_UNCERTAIN`)

When an operation is dispatched but the adapter connection times out or returns an ambiguous status:

```
[Operation Dispatched] ---> [Timeout / Ambiguous Adapter Response]
                                       |
                                       v
                     +-----------------------------------+
                     |   OUTCOME_UNCERTAIN ENCOUNTERED   |
                     +-----------------------------------+
                                       |
         +-----------------------------+-----------------------------+
         |                                                           |
         v (Target Supports Read-Back)                               v (Target Lacks Read-Back)
+-----------------------------------+               +-----------------------------------+
| Execute Target State Query        |               | FREEZE Domain Immediately         |
| - Compare actual state to expected|               | - Do NOT Retry                    |
| - IF Matched: OUTCOME_VERIFIED    |               | - Do NOT Revert                   |
| - IF Unchanged: Retry Safe        |               | - Escalate to RESYNC_REQUIRED     |
| - IF Diverged: Mark Failed        |               | - Require Authoritative Snapshot  |
+-----------------------------------+               +-----------------------------------+
```

**Inviolable Rule:** Under no circumstance shall the coordinator attempt to execute a compensating reversal or a speculative retry while the previous operation's outcome remains `OUTCOME_UNCERTAIN`.
