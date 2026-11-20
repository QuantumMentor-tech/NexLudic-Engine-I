# Recovery Trigger, Failure Classification and Assessment Contract

**Document Identifier:** `NLE-CON-TFA-001`  
**Lifecycle Stage:** Content Pipeline Stage R Assessment Contract  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Contract  
**Classification:** Incident Ingestion & Failure Assessment Model  

---

## 1. Objective & Scope

The **Recovery Trigger, Failure Classification and Assessment Contract** defines the formal events that initiate recovery assessment and specifies the classification taxonomy for execution failures, drift incidents, and verification anomalies.

---

## 2. Recovery Trigger Taxonomy

A recovery assessment is initiated upon receipt of any of eight authoritative trigger events:

| Trigger Code | Incident Classification | Description | Assessment Action |
| :--- | :--- | :--- | :--- |
| **`TRIG_PARTIAL_FAILURE`** | Execution Failure | Operation sequence halted after one or more modifications committed | Mandatory Recovery Assessment |
| **`TRIG_POSTCONDITION_FAILED`**| Write Anomaly | Written file hash does not match expected postcondition | Immediate Quarantine & Assessment |
| **`TRIG_VERIF_MISMATCH`** | Verification Failure | Stage V post-application verification detected target state mismatch | Reconciliation Assessment |
| **`TRIG_EXTERNAL_MUTATION`** | Concurrency Conflict | File modified by external process during or after execution session | Drift & Conflict Assessment |
| **`TRIG_INTERRUPTED_SESSION`**| System Crash / Power Loss | Execution session terminated abruptly without terminal journal record | Orphan Reconciliation |
| **`TRIG_CANCEL_POST_MUTATION`**| User Interruption | User issued cancellation signal after target modifications started | Safe Stopping & Assessment |
| **`TRIG_CORRUPTED_BACKUP`** | Storage Failure | Recovery prerequisite backup found corrupted or unreadable | Escalation / Manual Review |
| **`TRIG_TARGET_UNREADABLE`** | Target I/O Failure | Destination path locked, deleted, or unreadable by OS | I/O Diagnostics & Escalation |

---

## 3. Formal Recovery Assessment Schema (`RecoveryAssessmentRecord`)

Upon trigger ingestion, the assessor audits the target environment and emits a structured assessment record:

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/recovery-assessment.json",
  "assessment_id": "rec-asm-20261120-001",
  "trigger": {
    "trigger_code": "TRIG_PARTIAL_FAILURE",
    "trigger_timestamp": "2026-11-20T09:30:15Z",
    "execution_session_id": "sess-88412"
  },
  "original_execution_reference": {
    "plan_id": "plan-skyrim-nordic-armory-v1.0.0",
    "plan_digest": "sha256:4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b",
    "target_installation_instance_id": "inst-7788-9900-aabb",
    "total_planned_operations": 482,
    "committed_operations": 120,
    "failed_operation_id": "chg-121"
  },
  "affected_resources_summary": {
    "confirmed_modified_count": 120,
    "unmodified_count": 361,
    "uncertain_count": 1,
    "unexpected_external_modifications_count": 0
  },
  "available_recovery_sources": {
    "pre_mutation_backups_available": 4,
    "unmapped_added_files_to_remove": 116,
    "source_package_available": true
  },
  "recommended_strategy": "ROLLBACK_TO_ORIGINAL_BASELINE",
  "assessment_status": "ASSESSMENT_COMPLETE_ACTIONABLE"
}
```

---

## 4. Contract Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_TFA_UNKNOWN_TRIGGER` | FATAL | Unrecognized trigger event ingested | Reject trigger. Audit incident source. |
| `ERR_TFA_ORPHAN_SESSION_DETECTED` | WARNING | Interrupted session discovered without completion | Trigger orphan reconciliation. |
| `ERR_TFA_ASSESSMENT_INCOMPLETE` | FATAL | Unable to determine state of affected resources | Mark UNCERTAIN. Require manual review. |
