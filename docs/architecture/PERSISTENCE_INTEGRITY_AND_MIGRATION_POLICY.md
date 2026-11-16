# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Persistence Integrity & Migration Policy

**Document ID:** `NLE-POL-MIG-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Integrity & Evolution Policy  
**Traceability:** `ADR-0007` (Storage Baseline), `NLE-DOM-SPEC-001` (Core Domain Model), `NLE-ARC-DAT-001` (Persistence Architecture)

---

## 1. Executive Summary & Purpose

The **Persistence Integrity and Migration Policy** defines the architectural rules, transaction semantics, concurrency controls, schema evolution guidelines, and data migration safeguards for the NexLudic Engine platform.

Because NexLudic Engine relies on a **Tiered File-Based Storage Architecture** (`ADR-0007`), data integrity and referential consistency are maintained through rigorous domain-level invariants, atomic file replacement operations, optimistic concurrency tokens, and explicit migration state machines.

---

## 2. Data Invariants & Referential Integrity Rules

The persistence subsystem enforces seven structural invariants across all stored entities:

```
+----------------------------------------------------------------------------------------------------+
|                                  PERSISTENCE INTEGRITY INVARIANTS                                  |
+----+-----------------------------+-----------------------------------------------------------------+
| #  | Invariant Name              | Enforcement Rule & Validation Logic                             |
+----+-----------------------------+-----------------------------------------------------------------+
| 01 | Stable Identity Invariant   | Primary entity keys are immutable UUIDv4 strings. Renaming a    |
|    |                             | project, game, or capability NEVER alters its persistent ID.    |
+----+-----------------------------+-----------------------------------------------------------------+
| 02 | Validated Reference Rule    | Manifest references must resolve to valid entities. An invalid   |
|    |                             | foreign reference triggers immediate schema validation failure. |
+----+-----------------------------+-----------------------------------------------------------------+
| 03 | Immutable Evidence Rule     | Evidence records (`DAT-EVD-001`) are append-only. Existing test |
|    |                             | runs cannot be overwritten, edited, or re-dated.               |
+----+-----------------------------+-----------------------------------------------------------------+
| 04 | Game Version Hash Pinning   | Game version references must include the exact SHA-256 hash of  |
|    |                             | the primary game executable; vague semantic tags are prohibited.|
+----+-----------------------------+-----------------------------------------------------------------+
| 05 | Epistemic Grounding Invariant| A capability declaration cannot claim `VALIDATED` status        |
|    |                             | without linking to at least one verified Evidence Record.       |
+----+-----------------------------+-----------------------------------------------------------------+
| 06 | Staging Jail Containment    | All generated artifact paths must resolve strictly within the    |
|    |                             | project's `<project_root>/staging/` boundary.                   |
+----+-----------------------------+-----------------------------------------------------------------+
| 07 | Human Approval Precondition | Staged artifacts cannot be committed or deployed to game dirs    |
|    |                             | without an affirmative `AITaskApprovalRecord`.                  |
+----+-----------------------------+-----------------------------------------------------------------+
```

---

## 3. Atomic Write-Rename Operations (Transaction Strategy)

To achieve ACID-equivalent atomicity without database server locks, all manifest updates on disk utilize the **Atomic Write-Rename Pattern**:

```
[ In-Memory Domain Model Update ]
               |
               v
1. Serialize to Temporary File (<manifest>.json.tmp)
               |
               v
2. Flush File Stream to Disk (FlushToDisk = true)
               |
               v
3. Execute Win32 Atomic Replace:
   File.Replace(
       sourceFileName: "<manifest>.json.tmp",
       destinationFileName: "<manifest>.json",
       destinationBackupFileName: "<manifest>.json.bak"
   )
               |
               v
4. Delete Destination Backup (<manifest>.json.bak)
```

### Safety Guarantees:
* **Crash Resilience:** If the application process or power fails during step 1 or 2, the original `<manifest>.json` remains 100% intact. The `.tmp` file is cleaned up on next startup.
* **Kernel Atomicity:** `File.Replace` invokes the Win32 `ReplaceFileW` API, which performs an atomic file metadata swap at the NTFS filesystem driver level.
* **Zero Half-Written Files:** Manifests are never written in-place with truncating streams.

---

## 4. Concurrency & Conflict Detection

NexLudic Engine implements **Optimistic Concurrency Control (OCC)** using monotonic revision numbers and schema hashes:

```json
{
  "project_id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "revision_number": 42,
  "last_modified_utc": "2026-11-16T12:00:00.0000000Z",
  "concurrency_token": "a1b2c3d4e5f67890"
}
```

### Conflict Handling Policy:
1. When loading a project, the Core caches the `revision_number`.
2. Upon saving, the Core compares the disk `revision_number` against the cached value.
3. If the disk revision exceeds the cached revision (e.g. modified by another instance or external tool):
   * The save operation is rejected with `PERSISTENCE_CONCURRENCY_CONFLICT`.
   * The user is presented with a structured Diff Dialog showing conflicting fields.
   * Auto-merging is strictly prohibited for architectural blueprints.

---

## 5. Schema Versioning Architecture

To maintain compatibility across application releases, persistent data versions are strictly separated:

```
+----------------------------------------------------------------------------------------------------+
|                                SCHEMA VERSIONING HIERARCHY                                         |
+---------------------------------+-----------------------------------+------------------------------+
| Version Category                | Scope                             | Example                      |
+---------------------------------+-----------------------------------+------------------------------+
| Application Release Version     | Desktop Client Executable         | v0.2.0-preview               |
| Project Data Schema Version     | `project.json` envelope           | 1.0.0 (SemVer)               |
| Blueprint Schema Version        | `mashup-blueprint.json`           | 1.1.0 (SemVer)               |
| Adapter Contract Version        | Core <-> Worker IPC               | 1.0.0 (SemVer)               |
| Game Target Build Version       | Game Executable Release           | 1.6.1170.0 (Build Number)    |
+---------------------------------+-----------------------------------+------------------------------+
```

---

## 6. The 12 Migration Categories & Safeguards

When persistent data models evolve, migrations are categorized and governed according to strict risk profiles:

| # | Migration Category | Risk Profile | Migration Strategy & Safeguards |
| :--- | :--- | :--- | :--- |
| **01** | Additive Field (Optional) | Very Low | Automatic backfill with default value. No user prompt required. |
| **02** | Additive Field (Mandatory) | Moderate | Requires explicit migration step or interactive user default selection. |
| **03** | Field Renaming | Low | Transform JSON property name during deserialization; update manifest. |
| **04** | Field Removal / Deprecation | Low | Ignore unknown field on read; drop field on subsequent write. |
| **05** | Vocabulary / Status Change | Moderate | Deterministic enum value mapping (e.g. `STAGED` -> `IN_STAGING`). |
| **06** | Entity Structural Split | High | Multi-file migration; pre-migration snapshot required. |
| **07** | Identifier Migration | Critical | Update foreign key references across all dependent manifests atomically. |
| **08** | Artifact Staging Path Relocation| Moderate | Move physical directory on Volume D:; verify path containment. |
| **09** | Evidence Schema Evolution | High | Append version tag; ensure historical citations remain readable. |
| **10** | Storage Format Conversion | High | Automated conversion tool with mandatory bit-for-bit pre-backup. |
| **11** | Older Version Import | Moderate | Run sequential migration pipeline (`v1.0` -> `v1.1` -> `v1.2`). |
| **12** | Unknown Future Version Read | Critical | **Strict Read-Only or Rejection.** Overwriting is forbidden! |

### 6.1 The 5-Step Safe Migration Pipeline
Before executing any structural schema migration:
1. **Pre-Flight Inspection:** Validate source file syntax, detect current `schema_version`, and verify disk space on Volume `D:`.
2. **Mandatory Snapshot Backup:** Create a compressed archive (`.bak`) of the entire `<project_root>/` in `<project_root>/backups/`.
3. **Atomic Execution:** Perform migration transformations in temporary files (`.tmp`).
4. **Post-Migration Verification:** Validate migrated files against new JSON Schema and execute domain invariant checks.
5. **Atomic Commit:** Replace original manifests using `File.Replace`. Record migration outcome in `migration-log.json`.

---

## 7. Unknown Future Schema Version Protection

When NexLudic Engine encounters a project written by a newer version of the software:
* **Detection:** Detected when manifest `schema_version` has a `MAJOR` version greater than the Core runtime's supported schema version.
* **Enforcement:**
  - The Core MUST NOT attempt automated migration.
  - The Core MUST NOT open the project in read-write mode.
  - The UI displays an explicit warning: *"This project was created with a newer version of NexLudic Engine (Schema v2.0). Opening in read-only mode to prevent data corruption."*
  - Any attempt to save changes is blocked at the repository boundary.

