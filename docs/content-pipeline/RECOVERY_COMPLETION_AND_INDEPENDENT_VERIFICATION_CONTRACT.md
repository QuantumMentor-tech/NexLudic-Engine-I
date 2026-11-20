# Recovery Completion and Independent Verification Contract

**Document Identifier:** `NLE-CON-RCV-001`  
**Lifecycle Stage:** Content Pipeline Stage R Verification Handoff  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**Downstream Interface:** Stage V (Independent Verification Engine)  
**System Status:** Authoritative Architectural Contract  
**Classification:** Post-Recovery Verification & Completion Attestation  

---

## 1. Objective & Independent Verification Boundary

The **Recovery Completion and Independent Verification Contract** establishes the completion criteria for recovery sessions and governs the handoff of recovered environments to Stage V for independent post-recovery auditing.

A recovery executor's self-reported success or clean exit code is architecturally insufficient to declare a target restored. An independent verifier module must inspect the physical files on disk to confirm that the target matches the intended recovered state.

---

## 2. Formal Completion Criteria

| Recovery Strategy | Mandatory Completion Criteria | Required Verification Standard |
| :--- | :--- | :--- |
| **`STRAT_EXACT_ROLLBACK`** | All replaced/removed files match original $S_{\text{orig}}$ SHA-256; all added files confirmed absent | Bit-for-bit identity against pre-change catalog |
| **`STRAT_FORWARD_REPAIR`** | All remaining package members applied; hashes match Stage M MPR | Bit-for-bit identity against intended plan $S_{\text{int}}$ |
| **`STRAT_COMPENSATION`** | Compensation patches applied; config keys removed; residual state consistent | Schema and functional consistency audit |

---

## 3. Post-Recovery Verification Handoff Schema

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/recovery-to-verification-handoff.json",
  "handoff_id": "hnd-rec-20261120-0091",
  "recovery_session_id": "rec-sess-001",
  "target_installation_instance_id": "inst-7788-9900-aabb",
  "executed_strategy": "STRAT_EXACT_ROLLBACK",
  "recovery_journal_reference": {
    "journal_path": "D:/NexLudic/Workspace/Journals/rec-sess-001.jsonl",
    "journal_sha256": "sha256:112233445566778899aabbccddeeff00112233445566778899aabbccddeeff00"
  },
  "expected_final_state": "S_ORIGINAL_BASELINE",
  "timestamp": "2026-11-20T09:35:20Z"
}
```

---

## 4. Contract Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_RCV_VERIFICATION_MISMATCH` | FATAL | Post-recovery file hash does not match baseline | Mark RECOVERY_FAILED. Prompt manual. |
| `ERR_RCV_VERIFICATION_UNAVAILABLE` | WARNING | Independent verifier offline; state unverified | Retain unverified status. |
