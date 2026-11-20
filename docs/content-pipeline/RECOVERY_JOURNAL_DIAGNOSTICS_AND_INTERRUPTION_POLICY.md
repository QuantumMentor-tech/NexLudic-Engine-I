# Recovery Journal, Diagnostics and Interruption Policy

**Document Identifier:** `NLE-POL-RJD-001`  
**Lifecycle Stage:** Content Pipeline Stage R Operational Policy  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Audit Logging & Interrupted Recovery Governance  

---

## 1. Objective & Audit Preservation

The **Recovery Journal, Diagnostics and Interruption Policy** governs how recovery sessions record their actions and defines the safe recovery behavior when a recovery procedure itself is interrupted by crash, power loss, or user cancellation.

### Core Historical Preservation Rule
$$\text{Recovery MUST NOT overwrite, erase, or truncate the original execution journal.}$$
The original execution history (`execution_journal.jsonl`) remains an immutable historical record. Recovery emits a separate, cross-referenced log: `recovery_journal.jsonl`.

---

## 2. Recovery Journal Event Schema (`recovery_journal.jsonl`)

```json
{"event_id":"rev-001","timestamp":"2026-11-20T09:35:10Z","session_id":"rec-sess-001","original_session_id":"sess-88412","type":"REC_SESSION_INIT","strategy":"STRAT_EXACT_ROLLBACK"}
{"event_id":"rev-002","timestamp":"2026-11-20T09:35:11Z","session_id":"rec-sess-001","type":"REC_AUTH_CONFIRMED","grant_id":"grant-rec-20261120-0012"}
{"event_id":"rev-003","timestamp":"2026-11-20T09:35:12Z","session_id":"rec-sess-001","type":"REC_OP_STARTED","op_id":"rec-op-001","action":"REMOVE_ADDED_FILE","target_path":"Data/Textures/armor_d.dds"}
{"event_id":"rev-004","timestamp":"2026-11-20T09:35:13Z","session_id":"rec-sess-001","type":"REC_OP_COMMITTED","op_id":"rec-op-001","tombstone_verified":true}
{"event_id":"rev-005","timestamp":"2026-11-20T09:35:14Z","session_id":"rec-sess-001","type":"REC_OP_STARTED","op_id":"rec-op-002","action":"RESTORE_PRE_MUTATION_BACKUP","target_path":"Data/Scripts/QuestScript.pex"}
{"event_id":"rev-006","timestamp":"2026-11-20T09:35:15Z","session_id":"rec-sess-001","type":"REC_OP_COMMITTED","op_id":"rec-op-002","restored_sha256":"sha256:112233..."}
{"event_id":"rev-007","timestamp":"2026-11-20T09:35:16Z","session_id":"rec-sess-001","type":"REC_SESSION_COMPLETED_PENDING_VERIF","total_restored":120}
```

---

## 3. Handling Interrupted Recovery Sessions

If recovery terminates abruptly before completion:
1. **Never Restart Blindly:** Do not replay the recovery plan from the beginning.
2. **Re-Run Reconciliation:** The engine executes Stage R three-way reconciliation against the newly observed target state.
3. **Quarantine & Escalate:** If the target cannot be reliably reconciled, it is locked and surfaced for human manual intervention.

---

## 4. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_RJD_JOURNAL_WRITE_FAILED` | FATAL | Cannot write to recovery_journal.jsonl | Halt recovery. Block further ops. |
| `ERR_RJD_INTERRUPTED_RECOVERY` | FATAL | Recovery session crashed mid-execution | Lock target. Re-run reconciliation. |
| `ERR_RJD_HISTORICAL_JOURNAL_MUTATED` | FATAL | Original execution journal altered | Quarantine session. Audit integrity. |
