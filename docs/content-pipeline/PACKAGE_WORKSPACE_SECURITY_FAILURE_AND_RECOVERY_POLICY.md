# Package Workspace Security, Failure and Recovery Policy

**Document Identifier:** `NLE-POL-WSF-001`  
**Lifecycle Stage:** Content Pipeline Stage M Security & Operations  
**Parent Document:** [HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md) (`NLE-SPC-PMS-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Operational Security & Failure Recovery  

---

## 1. Objective & Scope

The **Package Workspace Security, Failure and Recovery Policy** defines the containment, operational security, fault handling, and cleanup protocols for the physical materialization environment. 

This policy ensures that Stage M operations execute strictly within authorized workspace boundaries under Windows Medium Integrity Level (`DEC-002`), prevents unauthorized filesystem escape or tampering, and guarantees clean, deterministic recovery from system failures, power outages, disk exhaustion, or abort signals.

---

## 2. Workspace Containment & Isolation Architecture

```
+--------------------------------------------------------------------------------------------------+
|                            WORKSPACE ISOLATION BOUNDARIES (MEDIUM IL)                            |
+--------------------------------------------------------------------------------------------------+
|                                                                                                  |
|   AUTHORIZED WORKSPACE ROOT:  D:\NexLudic\Workspace\                                             |
|                                                                                                  |
|   +---------------------------------------+      +-----------------------------------------+     |
|   | Staging Sandbox Directory             |      | Target Package Repository Store         |     |
|   | D:\NexLudic\Workspace\Staging\        |      | D:\NexLudic\Workspace\Packages\         |     |
|   | - Unique subfolder per build lease    |      | - Final verified .nlpkg artifacts only  |     |
|   | - Deny file execute permissions       |      | - Read-only to downstream stages        |     |
|   | - Auto-scrubbed on failure or exit    |      | - Cryptographically indexed             |     |
|   +---------------------------------------+      +-----------------------------------------+     |
|                                                                                                  |
|   STRICTLY FORBIDDEN TARGETS:                                                                    |
|   - Host Game Installation Folders (e.g., C:\Program Files\Steam\steamapps\common\...)           |
|   - System Windows Folders (C:\Windows, C:\Program Files)                                        |
|   - User Profile Root (C:\Users\<Name>\...) except within designated Engine AppData               |
+--------------------------------------------------------------------------------------------------+
```

### 2.1 Filesystem Access Controls & Permissions
1. **Access Control Lists (ACLs):** The temporary staging directory MUST inherit restrictive ACLs granting read/write permissions exclusively to the current user token under Windows Medium IL.
2. **Execute Permission Stripping:** Files written within the staging directory MUST NOT be granted execute permissions (`FILE_EXECUTE`). Stage M is an asset packaging pipeline, not an executable launcher.
3. **Symlink and Hardlink Blocking:** The engine must actively verify that neither the staging directory nor any of its ancestor paths are NTFS symbolic links, directory junctions, or mount points pointing outside the authorized workspace volume.

---

## 3. Failure Taxonomies & Recovery Strategies

Stage M categorizes operational failures into four distinct classes:

| Failure Class | Example Scenarios | Recovery Strategy |
| :--- | :--- | :--- |
| **Resource Exhaustion** | Disk full, memory limit exceeded, handle starvation | Safe workspace rollback. Purge temporary files. Post actionable alert to user. No retry until space freed. |
| **Integrity & Verification** | Corrupted asset bitstream, Merkle root mismatch, signature failure | Quarantine suspect container. Log cryptographic discrepancies. Purge staging workspace. Fail-closed. |
| **Security & Containment** | Path traversal attempt, forbidden characters, symlink creation | Immediate hard abort. Revoke build lease. Log high-severity security audit event. |
| **Abrupt Interruption** | Power loss, OS crash, process termination, cancellation signal | Crash-recovery cleanup handler scans staging directory on next engine startup, purging orphaned `.tmp` containers. |

---

## 4. Crash Recovery & Orphan Scavenging

To ensure that sudden engine termination does not leave gigabytes of orphaned temporary containers on user disks:

1. **Lease File Architecture:** Each active materialization lease writes a small heartbeat record:  
   `D:/NexLudic/Workspace/Staging/.leases/{lease_id}.lease`  
   containing process PID, start timestamp, and assigned temporary staging path.
2. **Startup Orphan Scavenger:** Upon engine startup, the Content Pipeline Orchestrator reads all `.lease` files:
   - If the recorded PID is dead or the lease has exceeded its maximum lease duration (default: 1 hour), the associated staging directory and `.tmp` files are immediately deleted.
   - The obsolete `.lease` record is archived to the diagnostic log.

---

## 5. Security & Recovery Invariant

Under no circumstances may Stage M leave unverified, uncataloged, or corrupted temporary build files in user workspace directories, nor may it attempt materialization if available disk storage violates the $2.5\times$ safety margin defined in `NLE-CON-MEP-001`.
