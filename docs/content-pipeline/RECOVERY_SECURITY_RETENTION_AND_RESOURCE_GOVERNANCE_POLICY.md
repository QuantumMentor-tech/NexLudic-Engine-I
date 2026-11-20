# Recovery Security, Retention and Resource Governance Policy

**Document Identifier:** `NLE-POL-SRG-001`  
**Lifecycle Stage:** Content Pipeline Stage R Security & Resource Governance  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Security Architecture, Data Retention & Quota Policy  

---

## 1. Objective & Security Scope

The **Recovery Security, Retention and Resource Governance Policy** establishes containment boundaries, retention rules for pre-mutation backups, and resource constraints for recovery operations.

---

## 2. Security Boundaries & Anti-Cheat Defense (`BLK-SEC-01`)

1. **Windows Medium Integrity Level (`DEC-002`):** Recovery operates under standard user tokens. Requests for administrator elevation are rejected.
2. **Immutable Protected Blocklist:** Recovery operations are strictly forbidden from modifying or deleting files matching anti-cheat components (`EasyAntiCheat*`, `BattlEye*`) or engine executables (`*.exe`).
3. **Path Traversal Defense:** All destination paths must strictly resolve within authorized target boundaries. Any path containing `..` or leading `/` triggers instant abort (`ERR_SRG_TRAVERSAL_DETECTED`).

---

## 3. Backup Retention & Cleanup Lifecycle

1. **Mandatory Retention Period:** Pre-mutation backups MUST be retained for a minimum of **30 days** following execution, or until explicitly cleared by the user.
2. **Never Auto-Delete During Incidents:** If an execution session fails or triggers recovery, its associated backups are locked and CANNOT be pruned by automated cleanup routines until the incident is formally closed and verified.
3. **Storage Quotas:** If backup directory volume reaches 80% capacity, the user is alerted to archive or prune historical, verified backups.

---

## 4. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_SRG_TRAVERSAL_DETECTED` | FATAL | Recovery path attempts directory traversal | Hard abort. Log security incident. |
| `ERR_SRG_PROTECTED_BLOCKLIST` | FATAL | Recovery attempted write to anti-cheat/executable | Hard abort. High-severity alert. |
| `ERR_SRG_STORAGE_EXHAUSTED` | FATAL | Insufficient disk space to execute recovery | Block recovery. Free workspace space. |
