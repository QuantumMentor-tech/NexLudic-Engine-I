# Pre-Change State Evidence and Recovery Source Integrity Policy

**Document Identifier:** `NLE-POL-SEI-001`  
**Lifecycle Stage:** Content Pipeline Stage R Policy & Provenance  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Evidence Verification & Recovery Source Governance  

---

## 1. Objective & Scope

The **Pre-Change State Evidence and Recovery Source Integrity Policy** governs the capture, storage, cryptographic verification, and eligibility of recovery sources used to restore target game environments.

Restoration must never occur from unverified, uncataloged, or corrupted backup files. A recovery source is eligible only when its identity, byte-level integrity, and direct lineage to the affected target resource are mathematically proven.

---

## 2. Recovery Source Taxonomy

Stage R recognizes four distinct classes of recovery sources:

```
+--------------------------------------------------------------------------------------------------+
|                                RECOVERY SOURCE TAXONOMY                                          |
+--------------------------------------------------------------------------------------------------+
|  1. Pre-Mutation Backup Artifact (SRC_BACKUP)                                                    |
|     - Byte-for-byte copy of original target file created during Stage E pre-mutation             |
|     - Stored at: <WorkspaceRoot>/Backups/{session_id}/{path}.bak                                  |
|     - Mandatory for restoring REPLACE or REMOVE operations                                        |
+--------------------------------------------------------------------------------------------------+
|  2. Confirmed-Absent Tombstone Record (SRC_ABSENT_TOMBSTONE)                                     |
|     - Verified evidence that a destination was CONFIRMED_ABSENT prior to mutation                |
|     - Authorizes removal of newly introduced files during rollback of ADD operations             |
+--------------------------------------------------------------------------------------------------+
|  3. Verified Content Package (.nlpkg) (SRC_PACKAGE_PAYLOAD)                                      |
|     - Materialized package container verified via Stage M MPR                                    |
|     - Utilized during forward reconciliation or repair of corrupted additions                    |
+--------------------------------------------------------------------------------------------------+
|  4. Compensating Inversion Patch (SRC_COMPENSATION_PATCH)                                         |
|     - Declarative inverse diff (e.g., config reversal, registry deregistration)                   |
|     - Utilized when exact byte-level restoration is not technically feasible                     |
+--------------------------------------------------------------------------------------------------+
```

---

## 3. Cryptographic Verification of Recovery Sources

Before any recovery source may be utilized in a recovery plan:
1. **SHA-256 Digest Revalidation:** The uncompressed byte stream of the recovery source must be hashed; the computed digest must equal the recorded `prior_sha256` in the execution journal.
2. **Provenance Linkage:** The backup must be linked to the exact `session_id`, `plan_id`, and `destination_relative_path`.
3. **No In-Place Tampering:** The backup file must reside in a read-only, workspace-contained directory. If file modification timestamps or ACLs indicate potential tampering, the source is quarantined (`ERR_SEI_SOURCE_TAMPERED`).

---

## 4. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_SEI_BACKUP_DIGEST_MISMATCH` | FATAL | Pre-mutation backup SHA-256 does not match journal | Reject source. Do NOT restore corrupted data. |
| `ERR_SEI_SOURCE_MISSING` | FATAL | Referenced backup file not found on disk | Block rollback. Evaluate compensation/manual. |
| `ERR_SEI_METADATA_ONLY_SOURCE` | WARNING | Backup contains metadata only, no file bytes | Cannot perform exact byte restoration. |
| `ERR_SEI_SOURCE_TAMPERED` | FATAL | Backup file shows evidence of external modification | Quarantine backup. Require security review. |
