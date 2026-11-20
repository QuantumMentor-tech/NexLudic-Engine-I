# Target State Reconciliation and External Change Protection Policy

**Document Identifier:** `NLE-POL-REC-001`  
**Lifecycle Stage:** Content Pipeline Stage R Reconciliation Policy  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Three-Way State Comparison & User Content Protection  

---

## 1. Objective & Three-Way Comparison Model

The **Target State Reconciliation and External Change Protection Policy** governs how NexLudic Engine assesses the physical reality of a target game installation before authorizing any recovery write.

To prevent destroying legitimate user edits, external mod changes, or game updates, Stage R evaluates a **Three-Way State Model**:

```
+--------------------------------------------------------------------------------------------------+
|                                THREE-WAY STATE RECONCILIATION                                    |
+--------------------------------------------------------------------------------------------------+
|  1. Original Baseline State (S_orig)  --> State recorded before Stage E execution started         |
|  2. Intended Plan State (S_int)       --> Expected final state defined by Application Plan       |
|  3. Currently Observed State (S_obs)  --> Cryptographically audited current state on disk        |
+--------------------------------------------------------------------------------------------------+
```

---

## 2. Six-Case Reconciliation Decision Matrix

For every affected target resource, the reconciler evaluates the three states against six mutually exclusive cases:

| Case | State Relationship | Classification | Permitted Automated Action |
| :--- | :--- | :--- | :--- |
| **Case 1** | $S_{\text{obs}} == S_{\text{orig}}$ | Pristine Original | No recovery action required; resource untouched. |
| **Case 2** | $S_{\text{obs}} == S_{\text{int}}$ | Complete Intended | Intended change present; rollback not needed; handoff to Stage V. |
| **Case 3** | $S_{\text{obs}} \neq S_{\text{orig}} \land S_{\text{obs}} \text{ matches session failure}$ | Known Partial Application | Standard rollback eligible (restore from pre-mutation backup). |
| **Case 4** | $S_{\text{obs}} \neq S_{\text{orig}} \land S_{\text{obs}} \neq S_{\text{int}} \land S_{\text{obs}} \text{ unexpected}$ | **External Drift / User Edit** | **BLOCK AUTOMATIC ROLLBACK!** Protect external content. |
| **Case 5** | $S_{\text{obs}} == \text{UNREADABLE / UNKNOWN}$ | Target I/O Failure | Block all recovery writes. Escalate to manual review. |
| **Case 6** | $S_{\text{obs}} \text{ mutates during recovery}$ | Concurrent Mutation | Immediate recovery abort; re-run reconciliation. |

---

## 3. Protection of External & User-Owned Changes

Under **Case 4**, where a target file differs from both the baseline and the session's intended output:
- The system **MUST NOT** restore the historical backup automatically.
- Overwriting unexpected content is strictly prohibited without explicit, informed human confirmation.
- The reconciler flags the resource as `CONF_EXTERNAL_DRIFT_DETECTED` and prompts the user with a visual diff.

---

## 4. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_REC_EXTERNAL_DRIFT_DETECTED` | FATAL | Target file altered by external actor after execution | Block automated rollback. Prompt user. |
| `ERR_REC_TARGET_STATE_UNRESOLVED` | FATAL | Observed state cannot be reconciled with journal | Halt recovery. Escalate to manual review. |
| `ERR_REC_CONCURRENT_DRIFT` | FATAL | File mutated while recovery planning was underway | Abort recovery planning. Re-audit target. |
