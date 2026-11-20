# CROSS-GAME ROLLBACK, COMPENSATION AND REVERSIBILITY SPECIFICATION

**Document Identifier:** `NLE-SPC-RCR-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / State Restoration  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-POL-RIU-001` (Step 76), `NLE-SPC-SRR-001` (Step 75)  
**Downstream Dependents:** `NLE-POL-CSA-001` (Step 76), `NLE-CON-COA-001` (Step 76)

---

## 1. Purpose and Scope

This specification defines the boundaries, technical requirements, and safety guardrails governing **Rollback**, **Compensation**, and **Reversibility** across live game bridges.

It enforces clear distinctions between restoring an exact prior state (Rollback), applying an inverse counteracting operation (Compensation), and realigning state via fresh agreement (Reconciliation).

---

## 2. Four-Tier Reversibility Classification

Every synchronized state attribute and operation must be categorized into one of four reversibility tiers:

```
+------------------------------------+-------------------------------------------------------------------+
| Reversibility Tier                 | Technical Definition & Operational Constraints                    |
+------------------------------------+-------------------------------------------------------------------+
| 1. `REVERSIBLE`                    | Target engine supports true transactional rollback / snapshot.    |
| 2. `COMPENSATABLE`                 | Action can be counteracted via inverse operation (e.g. refund).   |
| 3. `IRREVERSIBLE`                  | Action cannot be undone (e.g. consumed potion, fired projectile). |
| 4. `REVERSIBILITY_UNKNOWN`         | Target engine integration cannot prove rollback or compensation.  |
+------------------------------------+-------------------------------------------------------------------+
```

---

## 3. Multi-Part Resolution Failures & Partial Application

When a resolution decision involves multiple discrete operations (e.g. Deduct 100 gold in Game A, Grant Magic Sword in Game B):
1. **No Universal Two-Phase Commit:** Independent game engines do not support cross-process distributed transactions. Partial failure is always a real physical possibility.
2. **Detection of Partial Application:** If Operation 1 commits in Game A, but Operation 2 fails in Game B, the coordinator immediately sets status to `OUTCOME_PARTIALLY_APPLIED`.
3. **Automated Compensation Pipeline:**
   - If Game A operation is `COMPENSATABLE`, dispatch verified refund operation to Game A.
   - If compensation succeeds, domain returns to clean baseline.
   - If compensation fails or operation is `IRREVERSIBLE`, the coordinator **shall not guess**. It freezes the domain, quarantines the affected inventory/economy records, and escalates to human review.

---

## 4. Rollback Guardrails

- **Anti-Overwriting Newer State:** A rollback **shall never** be applied if the target game's state has advanced beyond the conflict context ($R_{target} > R_{conflict}$). Restoring an old state over newer legitimate gameplay is strictly prohibited.
- **Invariant Re-Check:** Any proposed rollback or compensating state must pass all Step 74 and Step 76 domain invariants before application.
