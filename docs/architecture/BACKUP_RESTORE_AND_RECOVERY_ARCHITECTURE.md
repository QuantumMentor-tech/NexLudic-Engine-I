# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Backup, Restore & Data Recovery Architecture

**Document ID:** `NLE-ARC-REC-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Reliability & Recovery Architecture  
**Traceability:** `ADR-0007` (Storage Baseline), `ADR-0011` (Rollback Strategy), `REQ-CORE-004` (Project Lifecycle), `NLE-ARC-DAT-001` (Persistence Architecture)

---

## 1. Executive Summary & Recovery Objectives

The **Backup, Restore, and Data Recovery Architecture** establishes the formal mechanisms required to protect user investments, preserve evidence auditability, and guarantee swift recovery from partial write failures, aborted migrations, tool crashes, or accidental user actions.

Because NexLudic Engine projects reside as self-contained directory trees on Volume `D:` (`DEC-009`), backup and recovery are designed around **deterministic multi-tier snapshotting** and **staged atomic restoration**.

### 1.1 Provisional Recovery Objectives:
* **Recovery Point Objective (RPO):**
  - **Authoritative Project Metadata:** **0 seconds** (RPO = 0; every confirmed user change is committed atomically via `File.Replace`).
  - **In-Flight Generated Artifacts:** Bounded to the start of the active generation operation (RPO $\le$ current operation duration).
* **Recovery Time Objective (RTO):**
  - **Metadata Rollback:** **< 2.0 seconds** (instant swap from pre-operation snapshot).
  - **Full Project Workspace Restoration:** **< 30 seconds** for a standard 1 GB project on modern SSD storage.

---

## 2. Backup Scope & Explicit Exclusions

A project backup must be self-contained and reproducible while strictly avoiding bloat and security leaks:

```
+----------------------------------------------------------------------------------------------------+
|                                    BACKUP SCOPE DEFINITION                                         |
+----------------------------------------------------+-----------------------------------------------+
| INCLUDED IN BACKUP PACKAGE                         | EXCLUDED FROM BACKUP PACKAGE                  |
+----------------------------------------------------+-----------------------------------------------+
| 1. Authoritative Project Manifests (`project.json`,| 1. Full Game Installation Directories         |
|    `mashup-blueprint.json`, `risk-register.json`)  |    (External proprietary gigabyte binaries)   |
| 2. Evidence Registry & Citations (`evidence.json`) | 2. Ephemeral Build Caches & Temp Files        |
| 3. Test Specifications & Execution History         |    (`*.tmp`, Papyrus compiler intermediate objs)|
| 4. AI Task Approvals & User-Reviewed Diffs         | 3. Diagnostic Log Files (`nexludic-*.log`)    |
| 5. Artifact Manifest & SHA-256 Checksums           | 4. User API Keys & Secrets (DPAPI tokens)     |
| 6. Custom User-Authored Scripts & Config Stubs     | 5. In-Memory Runtime Queues & Sockets         |
| 7. Recovery Journal & Schema Version Metadata      | 6. Unrelated Projects on Volume D:            |
+----------------------------------------------------+-----------------------------------------------+
```

---

## 3. Four Standard Backup & Snapshot Types

```
                                  BACKUP & SNAPSHOT HIERARCHY
                                               |
    +--------------------+---------------------+---------------------+--------------------+
    |                    |                     |                     |                    |
 1. PRE-OPERATION     2. PRE-MIGRATION      3. MANUAL EXPORT      4. DISASTER RECOVERY
    SNAPSHOT             SNAPSHOT              PACKAGE (.nxpkg)      BUNDLE
    - Trigger: Risky task- Trigger: App update - Trigger: User export- Trigger: Periodic / User
    - Scope: Manifests   - Scope: Full project - Scope: Clean bundle - Scope: Full project
    - Retention: 24h     - Retention: 90 days  - Retention: Permanent- Retention: User-managed
```

### 3.1 Type 1: Pre-Operation Rollback Snapshot
* **Trigger:** Automatically captured by `AITaskOrchestrator` immediately before launching an external compiler, mod staging script, or automated file generation task.
* **Mechanism:** Fast in-place directory snapshot of `<project_root>/project.json`, `mashup-blueprint.json`, and an index of current staging files.
* **Retention:** Kept until the operation completes successfully; auto-purged after 24 hours.

### 3.2 Type 2: Pre-Migration Snapshot
* **Trigger:** Automatically captured by `PersistenceManager` before executing any structural schema migration.
* **Mechanism:** Complete compressed archive (`.zip` / `.tar.gz`) stored in `<project_root>/backups/migration_v{FROM}_to_v{TO}_{TIMESTAMP}.bak`.
* **Retention:** Retained for 90 days or until user explicitly dismisses.

### 3.3 Type 3: Portable Project Export Package (`.nxpkg`)
* **Trigger:** Explicit user action via `File -> Export Project`.
* **Mechanism:** Sanitized archive containing metadata, blueprints, evidence citations, and user-authored scripts. Stripped of machine-specific absolute paths.
* **Retention:** User-controlled storage.

### 3.4 Type 4: Disaster Recovery Bundle
* **Trigger:** Scheduled weekly snapshot or user-initiated full project backup.
* **Mechanism:** Complete bit-for-bit archive of project metadata and approved artifacts.

---

## 4. Staged Restoration Workflow

Restoration MUST NEVER write directly into an active project workspace without verification. All restores execute through a **Staged Recovery Pipeline**:

```
[ Backup Archive (.bak / .nxpkg) ]
                |
                v
1. Unpack into Isolated Staging Area (<project_root>/.recovery/staged_restore/)
                |
                v
2. Validate Archive Structure & Schema Version
   - Verify integrity of JSON manifests
   - Check schema compatibility with current Core version
                |
                v
3. Inspect Target Game Dependencies & Executable Hashes
   - Confirm target game versions exist on host workstation
                |
                v
4. Detect & Resolve Identity Collisions
   - If Project ID matches existing project:
     * Option A: Overwrite existing project (requires explicit typing of project name)
     * Option B: Fork as New Project (generate fresh UUIDv4, rename title to "... (Restored)")
     * Option C: Cancel restoration
                |
                v
5. Atomic Commitment Swap
   - Move old manifests to .recovery/previous/
   - Promote staged files to active project root
                |
                v
6. Post-Restoration Invariant Verification
   - Verify all file SHA-256 hashes against `artifacts-manifest.json`
   - Transition project lifecycle state to `CONFIGURED` or `PAUSED`
```

---

## 5. Recovery Verification & Integrity Checklist

A restoration operation is legally classified as **`RESTORED`** when files are unpacked, but is NOT classified as **`VERIFIED`** until it passes the following 5-point verification checklist:

1. **Schema Check:** All restored manifests deserialize cleanly without unknown required fields or syntax errors.
2. **Hash Check:** Every extracted artifact file matches its recorded SHA-256 checksum in `artifacts-manifest.json`.
3. **Reference Check:** All evidence citations, capability links, and risk references resolve to valid entities.
4. **Toolpath Check:** Configured adapter paths and tool prerequisites are validated on the current host.
5. **No Elevation Check:** Restored files execute and open under standard user permissions (Medium Integrity Level).

---

## 6. Failure Recovery & Rollback Journaling

If a restoration operation fails midway (e.g. power cut or disk full during step 5):
* The `.recovery/in_flight.journal` records the active transaction state.
* Upon next application boot, the Core detects the uncompleted journal entry, rolls back partial changes from `.recovery/previous/`, and restores the pre-restore state.
* The user is notified with an explicit recovery diagnostic dialog.

