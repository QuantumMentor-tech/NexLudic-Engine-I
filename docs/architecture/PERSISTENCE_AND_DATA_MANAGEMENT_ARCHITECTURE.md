# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Persistence Architecture & Project Data Management Design

**Document ID:** `NLE-ARC-DAT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Architecture Specification  
**Traceability:** `ADR-0007` (Storage Baseline), `ADR-0009` (Workspace Isolation), `REQ-CORE-004` (Project Lifecycle), `REQ-SAFE-002` (Local Privacy), `DEC-009` (Volume D: Allocation), `NLE-DOM-SPEC-001` (Core Domain Model)

---

## 1. Executive Summary & Purpose

The **NexLudic Engine** is a Windows-first, AI-assisted game integration and interoperability platform. Its primary function is to empower creators to assess, plan, scaffold, validate, and execute integrations ("mashups") between disparate PC games across three architectural strategies:
1. **Strategy A (Host-Game Modification):** Game-specific modding runtime extensions (e.g. Papyrus scripts, BepInEx plugins).
2. **Strategy B (Live Interoperability Bridge):** Out-of-process live data and input exchange between running games.
3. **Strategy C (Hybrid Integration):** Coordinated combination of offline mod staging and live interoperability.

To fulfill these capabilities reliably, NexLudic Engine must manage diverse categories of information: project definitions, game version metadata, user requirements, adapter descriptors, capability claims, empirical evidence citations, test execution records, AI task logs, generated code artifacts, and diagnostic telemetry.

This document establishes the **Persistence Architecture and Project Data Management Design**. It defines the persistence boundaries, evaluates candidate storage technologies, reaffirms and refines `ADR-0007`, formalizes the multi-tier storage model, specifies workspace path governance, establishes data ownership, and mandates data safety and recovery invariants.

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC CORE APPLICATION                                       |
|                                                                                                    |
|  +--------------------+   +-----------------------+   +--------------------+   +----------------+  |
|  | Project Coordinator|   | Compatibility Service |   | Evidence Registry  |   | AI Orchestrator|  |
|  +---------+----------+   +-----------+-----------+   +---------+----------+   +--------+-------+  |
+------------|--------------------------|-------------------------|-----------------------|----------+
             | Domain Aggregates        | Value Objects           | Evidence Citations    | Tasks    
             v                          v                         v                       v          
+----------------------------------------------------------------------------------------------------+
|                                   PERSISTENCE BOUNDARY ABSTRACTIONS                                |
|                                                                                                    |
|    IProjectRepository      ICompatibilityRepository       IEvidenceRepository     IArtifactStore   |
|   (Atomic Manifests)        (Versioned Metadata)           (Audit Provenance)      (Staged Files)  |
+----------------------------------------------------------------------------------------------------+
             |                          |                         |                       |          
             v                          v                         v                       v          
+----------------------------------------------------------------------------------------------------+
|                           TIERED STORAGE ARCHITECTURE BASELINE (ADR-0007)                          |
|                                                                                                    |
|  [TIER 1: STRUCTURED PROJECT MANIFESTS]        [TIER 2: WORKSPACE & ARTIFACT REPOSITORY]          |
|  - JSON Schema 2020-12, UTF-8, Indented         - Staging Jail: <project>/staging/                 |
|  - Atomic Write-Rename (File.Replace)          - Generated Mod Workspaces & Assets                 |
|  - Location: D:\NexLudic\Projects\<id>\        - SHA-256 Content-Addressed Verification            |
|                                                                                                    |
|  [TIER 3: DIAGNOSTICS & AUDIT LOGS]            [TIER 4: EPHEMERAL RUNTIME STATE]                   |
|  - Serilog Structured Rolling JSON/Text         - In-Memory System.Threading.Channels              |
|  - Sanitized, Token-Redacted, 14-day Rolling   - Non-durable process lifecycles & session caches   |
|                                                                                                    |
|  [TIER 5: DEEP TELEMETRY & CATALOG INDEX (PHASE 3 CANDIDATE: EMBEDDED SQLITE)]                     |
|  - Formally evaluated and deferred to Phase 3; activated only when query complexity requires SQL. |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Foundational Persistence Architecture Principles

The persistence subsystem is governed by twelve engineering principles derived from core platform requirements:

1. **Clear Authoritative Ownership:** Every persistent entity has exactly one authoritative component owner. Multiple components must never maintain unsynchronized, competing copies of mutable facts.
2. **Appropriate Durability:** Information durability matches its business criticality. Authoritative project definitions and verified evidence require immediate, durable disk persistence; transient progress notifications remain ephemeral in memory.
3. **Data Integrity & Atomic Writes:** Storage operations must be immune to corruption from sudden process termination or power loss. Manifest updates use atomic write-rename strategies.
4. **Separation of Concerns:** Project metadata, large binary artifacts, user configuration, diagnostic logs, and temporary runtime states are segregated into dedicated storage tiers optimized for their access patterns.
5. **Evidence Provenance & Immutability:** Technical evidence and test observations are immutable audit records. Storing an adapter's capability claim does NOT convert it into a validated capability.
6. **Recoverability & Clean Rollback:** Every destructive or multi-file operation must provide a recoverable pre-operation snapshot or clean staging rollback path.
7. **Version Awareness:** All persistent data structures explicitly encode their schema version. Compatibility records capture exact target game versions, executable SHA-256 hashes, and adapter contract versions.
8. **Local-First & Offline Autonomy:** The core persistence engine operates 100% locally on the user's workstation without requiring cloud databases, external network listeners, or SaaS synchronization (`REQ-SAFE-002`).
9. **Zero Elevation & Privacy by Default:** Persistence operations execute under standard user privileges (Medium Integrity Level, `REQ-SEC-001`). Secrets and API tokens are never written to project manifests, logs, or exports.
10. **Explicit Schema Evolution:** Stored data structures evolve via forward-compatible, documented migrations. Unknown future schema versions are detected and safely protected against destructive downgrades.
11. **Safe Deletion & Distinction of User Content:** Deletion explicitly distinguishes between disposable build caches, generated scaffolding, and user-authored original assets.
12. **Minimal Necessary Complexity:** The architecture avoids premature database dependencies. Human-readable, git-friendly structured files are leveraged for MVP project models; embedded relational storage is deferred until complex cross-project indexing is required.

---

## 3. Evaluation of Storage Technology Alternatives

In accordance with Section 8 of the engineering standard, four primary persistence mechanisms were evaluated:

| Evaluation Dimension | 1. Structured Project Files (JSON/YAML) | 2. Embedded Relational (SQLite) | 3. Embedded Document Store (LiteDB) | 4. Hybrid Tiered Storage (Approved Baseline) |
| :--- | :--- | :--- | :--- | :--- |
| **Human Inspectability & Git Diffability** | **Excellent:** Plain text, git-trackable, readable in any editor. | **Poor:** Binary file; opaque without specialized database tools. | **Poor:** Binary BSON file; opaque to users and git. | **Excellent:** Manifests in JSON; binaries in filesystem staging. |
| **Transaction Atomicity** | **High (File-level):** Atomic write-rename via OS kernel (`File.Replace`). | **Maximum (ACID):** Write-Ahead Logging (WAL), multi-table transactions. | **Moderate:** Two-phase commit, single-file locking. | **High:** Atomic manifest swaps + staging transaction rollbacks. |
| **Referential Integrity Enforcement** | **Application-Level:** Enforced by domain models and validators. | **Database-Level:** Foreign key constraints, CASCADE triggers. | **Application-Level:** Document references. | **Application-Level:** Strong DDD domain invariant validation. |
| **Cross-Process Concurrency** | **Moderate:** Multi-reader / single-writer with file-sharing flags. | **High:** Multi-reader / single-writer with WAL mode. | **Low:** Process file-lock contention risks. | **High:** Core owns metadata; workers isolated to staging directories. |
| **Corruption Resilience** | **High:** Corrupted temp file discarded without damaging original. | **High:** WAL recovery, but binary corruption requires `.recover`. | **Moderate:** Binary corruption can lock entire database. | **Maximum:** Isolated manifest files; corruption does not cascade. |
| **Dependency Footprint** | **Zero:** Built-in `System.Text.Json` in .NET 8 runtime. | **Moderate:** Native `e_sqlite3.dll` / `Microsoft.Data.Sqlite`. | **Moderate:** Third-party managed library dependency. | **Minimal:** Zero runtime DB dependencies for MVP core. |
| **Storage Volume Compatibility** | **High:** Effortless placement on Volume `D:` (`DEC-009`). | **High:** Single file on Volume `D:`. | **High:** Single file on Volume `D:`. | **Optimal:** Full alignment with Volume `D:` capacity constraints. |
| **Cross-Project Query Complexity** | **Low:** Requires directory traversal for cross-project search. | **Maximum:** Instant SQL indexing and multi-table joins. | **High:** BSON index queries. | **Sufficient for MVP:** Flat projects; SQL deferred to Phase 3. |

### Architectural Conclusion & Decision Alignment:
The evaluation confirms the wisdom of **`ADR-0007` (Configuration, Persistent Storage, and Diagnostic Logging)**:
* **Approved Baseline (MVP / Phase 2):** **Tiered File-Based Storage**. Project definitions, mashup blueprints, and evidence manifests are stored as formatted JSON files adhering to JSON Schema Draft 2020-12. Large generated files and mod trees live directly in filesystem staging folders.
* **Provisional Deferred Technology (Phase 3+):** **Embedded Relational Storage (SQLite)** is retained as a candidate for deep telemetry indexing and global cross-project catalog querying, but is NOT authorized or required for Phase 2 project data management.

---

## 4. Multi-Tier Storage Architecture

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC MULTI-TIER STORAGE MODEL                               |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier   | Category                 | Storage Technology    | Path / Location      | Retention       |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier 1 | Project Manifests & State| Structured JSON Files | <project_root>/      | Permanent       |
|        | (Authoritative Metadata) | (System.Text.Json)    | project.json         | (User-managed)  |
|        |                          |                       | mashup-blueprint.json|                 |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier 2 | Generated Mod Artifacts  | Filesystem Hierarchy  | <project_root>/      | Lifecycle-bound |
|        | & Build Workspaces       | (Staging Jail)        | staging/             | (Quarantine /   |
|        |                          |                       | artifacts/           | Cleanable)      |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier 3 | Diagnostics, Traces &    | Serilog Rolling Sinks | <appdata>/logs/      | 14-day rolling  |
|        | Audit Logs               | (ndjson / text)       | nexludic-*.log       | (Max 500 MB)    |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier 4 | Ephemeral Runtime State  | In-Memory Data        | RAM (Core Process)   | Process lifetime|
|        | & Live Session Data      | Structures & Channels |                      | (Non-durable)   |
+--------+--------------------------+-----------------------+----------------------+-----------------+
| Tier 5 | Application Config &     | User AppData JSON     | %LocalAppData%/      | Persistent      |
|        | Global Preferences       | (Protected DPAPI)     | NexLudic/config.json | (Per-machine)   |
+--------+--------------------------+-----------------------+----------------------+-----------------+
```

---

## 5. Persistence Boundary & Component Responsibilities

In strict accordance with Step 10 Component Architecture (`CORE_COMPONENT_ARCHITECTURE.md`) and Step 11 Domain Contracts (`UNIVERSAL_ADAPTER_CONTRACT_SPECIFICATION.md`), direct filesystem and persistence access is restricted exclusively to the `NexLudic.Infrastructure` layer:

```
[ NexLudic.App (UI / Shell) ]
             |
             v
[ NexLudic.Core (Application Workflows / Use Cases) ]
   - ProjectCoordinationService
   - CompatibilityAssessmentService
   - ValidationWorkflowEngine
             |
             | Calls domain repository interfaces
             v
[ NexLudic.Domain (Pure Business Rules & Invariants) ]
   - Defines: IProjectRepository, IEvidenceRepository, IArtifactStore
   - Zero references to System.IO, File.Replace, SQLite, or Serilog
             ^
             | Implements interfaces
             |
[ NexLudic.Infrastructure (Concrete Storage Adapters) ]
   - JsonProjectRepository (Atomic File.Replace, JSON Schema validation)
   - JsonEvidenceRepository (Append-only audit trail)
   - FilesystemArtifactStore (Staging jail containment, hash verification)
   - SerilogLoggingSink (Sanitized diagnostic emission)
```

### Boundary Constraints:
1. **Domain Layer Independence:** `NexLudic.Domain` MUST NOT depend on storage engines, file handles, or JSON serializers.
2. **Adapter Isolation (`ADR-0002`):** External game adapters NEVER access the Core project repository directly. They receive isolated staging directories and emit structured IPC messages (`NLE-SPC-MSG-001`).
3. **Write Validation Gate:** Every write to an authoritative manifest must pass JSON Schema validation and entity invariant checks before touching disk.

---

## 6. Project Workspace & Directory Hierarchy Specification

To satisfy `DEC-009` (Volume D: storage allocation) and `ADR-0009` (Workspace Isolation), projects are organized into deterministic, self-contained filesystem directories:

```
D:\App Winrar Files\Nexludic Engine (working)\projects\<PROJECT_ID>\
│
├── project.json                      # Tier 1: Authoritative project definition & metadata
├── mashup-blueprint.json             # Tier 1: Integration topology, strategies & mappings
├── evidence-registry.json            # Tier 1: Immutable evidence citations & test results
├── risk-register.json                # Tier 1: Assessed integration risks & mitigations
│
├── staging\                          # Tier 2: Staging Jail (Adapter write area)
│   ├── game_a\                       # Staged mod files for Game A
│   └── game_b\                       # Staged mod files for Game B
│
├── artifacts\                        # Tier 2: Validated & approved build artifacts
│   ├── compiled_scripts\             # Verified .pex, .dll, or .gd binaries
│   └── manifests\                    # Staged mod package descriptors
│
├── backups\                          # Pre-operation & pre-migration recovery snapshots
│   └── snapshot_20261116_120000.bak  # Compressed zip/tar metadata + manifest snapshot
│
└── .recovery\                        # Transient rollback journal during atomic operations
    └── in_flight.journal             # Lock and rollback pointer for crash recovery
```

---

## 7. Storage Decision & Change Governance

| Decision Item | Approved Baseline | Alternative Evaluated | Status | Governance Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Project Storage** | Tiered Structured JSON | SQLite Database | **Approved (`ADR-0007`)** | Human-readable, git-friendly, zero external database locks, perfectly portable. |
| **Storage Drive** | Dedicated Volume `D:` | System Volume `C:` | **Approved (`DEC-009`)** | Volume C: has <10 GB free space; Volume D: has adequate headroom for mod workspaces. |
| **Atomic Writes** | Temp Write + `File.Replace` | In-place file stream | **Approved Baseline** | Eliminates corrupt half-written files on unexpected application termination. |
| **Diagnostic Storage** | Rolling Serilog JSON/Text | Central DB log table | **Approved (`ADR-0007`)** | Decouples logging I/O from domain storage; auto-purges via size/retention policies. |
| **Telemetry Indexing** | In-Memory (Phase 2) | SQLite (Phase 3) | **Deferred to Phase 3** | Low volume during architecture phase; relational engine introduced only when justified. |
| **Secret Storage** | Windows DPAPI / CredentialMgr | Encrypted Project JSON | **Approved Security Policy**| Secrets MUST NOT reside in project workspaces or version-controlled manifests. |

---

## 8. Summary of Architectural Compliance

* **ADR-0007:** Tiered file-based storage architecture implemented without premature SQLite locks.
* **ADR-0009:** Staging jail path containment strictly bounded to `<project_root>/staging/`.
* **REQ-CORE-004:** Complete project lifecycle, archiving, and rollback mechanics defined.
* **REQ-SAFE-002:** 100% local-first data management; zero mandatory network synchronization.
* **REQ-SEC-001:** Zero administrative privilege elevation required; standard user permissions.

