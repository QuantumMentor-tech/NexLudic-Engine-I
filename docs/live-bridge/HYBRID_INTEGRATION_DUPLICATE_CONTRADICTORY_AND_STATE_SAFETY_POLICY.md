# HYBRID INTEGRATION DUPLICATE, CONTRADICTORY AND STATE SAFETY POLICY

**Document Identifier:** `NLE-POL-HDC-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / State Safety  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-SPC-CSH-001` (Step 77), `NLE-POL-IES-001` (Step 74)  
**Downstream Dependents:** `NLE-POL-HFD-001` (Step 77), `NLE-SPC-HSL-001` (Step 77)

---

## 1. Purpose and Scope

This policy governs the prevention of duplicate operations, contradictory multi-strategy writes, and state corruption during hybrid integration transitions.

During strategy transitions or fallbacks, messages can linger in network queues or buffer retries while a new strategy activates. This policy enforces strict deduplication and single-writer boundaries to prevent split-brain state divergence.

---

## 2. Duplicate Prevention Across Strategy Boundaries

To prevent duplicate execution of non-idempotent operations (e.g. inventory transfers, damage events) across strategy handoffs:
1. **Cross-Strategy Deduplication Window:** The orchestrator maintains a global sliding window of the last $N = 2048$ processed `operation_id` records shared across all strategy instances.
2. **Handoff Token Tracking:** Every operation executed during a handoff must carry the `handoff_token_id`. If an operation arrives at a destination strategy without the matching token, it is dropped as a stray duplicate from an un-synchronized path.
3. **Non-Idempotent Operation Fencing:** `ENTITY_SPAWN` and `RESOURCE_TRANSFER` operations **shall never** be re-dispatched during a handoff without verifying tombstone records.

---

## 3. Contradictory Writer Prevention Invariant

At any point in time $t$, exactly one strategy instance holds write permission for a specific synchronization domain:
$$\text{Count}(\text{ActiveWriters}(D, t)) \le 1$$

### Dual-Writer Guardrail:
- When Strategy B is granted authority, Strategy A's write certificate is revoked in the Session Registry with an effective cutoff revision $R_{cutoff}$.
- Any subsequent mutation emitted by Strategy A with $R > R_{cutoff}$ is rejected at the message exchange boundary with `ERR_SYNC_UNAUTHORIZED_WRITER`.

---

## 4. Operation Status Progression Across Handoffs (9 States)

```
+------------------------------------+-------------------------------------------------------------------+
| Operation State Across Handoff     | Operational Handling & Permitted Actions                          |
+------------------------------------+-------------------------------------------------------------------+
| 1. `NOT_DISPATCHED`                | Re-route safely to destination strategy.                          |
| 2. `IN_FLIGHT_UNCONFIRMED`         | Hold in transition buffer; wait for predecessor drain timeout.    |
| 3. `RECEIVED_UNVALIDATED`          | Forward to destination validator or drop.                         |
| 4. `VALIDATED_UNAPPLIED`           | Re-validate against destination schema before apply.              |
| 5. `APPLIED_UNCONFIRMED`           | Status marked UNCERTAIN; execute read-back before proceeding.     |
| 6. `APPLIED_AND_VERIFIED`          | Complete; advance watermark R_app.                                |
| 7. `REJECTED_BY_PREDECESSOR`       | Do NOT re-dispatch to destination without fresh evaluation.       |
| 8. `PARTIALLY_APPLIED`             | Trigger Step 76 compensation pipeline immediately.                |
| 9. `OUTCOME_UNCERTAIN`             | Freeze domain; block handoff progression; mandate snapshot reset. |
+------------------------------------+-------------------------------------------------------------------+
```
