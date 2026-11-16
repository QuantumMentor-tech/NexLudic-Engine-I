# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Data Inventory & Storage Decision Matrix

**Document ID:** `NLE-MAT-DAT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Data Classification & Inventory  
**Traceability:** `ADR-0007` (Storage Baseline), `NLE-ARC-DAT-001` (Persistence Architecture), `NLE-DOM-SPEC-001` (Core Domain Model)

---

## 1. Executive Summary & Inventory Scope

This document establishes the comprehensive **Data Inventory and Storage Decision Matrix** for the NexLudic Engine platform. Every persistent, semi-persistent, ephemeral, and diagnostic piece of information managed by the platform is cataloged, classified across 17 standardized dimensions, assigned an authoritative component owner, and mapped to its approved storage mechanism.

Authoritative domain records, derived caches, generated artifacts, runtime session state, and user preferences are strictly segregated to guarantee that transient failures or untrusted external inputs cannot corrupt critical project state.

---

## 2. The 10 Primary Data Categories

```
                                  NEXLUDIC DATA TAXONOMY
                                             |
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |                    |
 1. CORE PROJECT      2. COMPATIBILITY    3. EVIDENCE          4. ADAPTER METADATA  5. AI TASK
    - Project identity   - Feasibility       - Citations          - Descriptors        - Prompts/Scopes
    - Selected games     - Capability claims - Empirical tests    - Manifests          - Approvals
    - Strategy mapping   - Assessed risks    - Epistemic tiers    - Contract versions  - Task history
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |                    |
 6. GENERATED         7. TESTING          8. RUNTIME           9. DIAGNOSTICS      10. CONFIGURATION
    ARTIFACTS            & VALIDATION        & OPERATIONAL        & AUDIT              & PREFERENCES
    - Mod workspaces     - Test specs        - Worker lifecycles  - Serilog traces     - Tool paths
    - Compiled scripts   - Pass/fail runs    - Active operations  - Crash logs         - UI settings
    - Staging files      - Regression logs   - Ephemeral streams  - Security alerts    - User DPAPI tokens
```

---

## 3. Comprehensive 17-Dimension Data Classification Matrix

### 3.1 Category 1: Core Project Data (`DAT-PRJ-001`)
1. **Stable Data Category ID:** `DAT-PRJ-001`
2. **Description:** Authoritative definition of a game-pair mashup project, including display metadata, selected game identities, target editions/versions, requested requirements, and active integration strategy.
3. **Authoritative Owner:** `ProjectCoordinator` (`NexLudic.Core`)
4. **Primary Consumers:** Desktop UI Shell (`NexLudic.App`), Workflow Orchestrator, AI Agent Assistant.
5. **Data Sensitivity:** Confidential / User Private (Contains local filesystem paths, project names, and custom mashup designs).
6. **Expected Volume / Growth:** 10 KB to 500 KB per project; linear growth per project count (1 to 100 projects per machine).
7. **Required Lifetime:** Permanent until explicitly deleted or archived by the user.
8. **Durability Requirement:** Immediate Durable (Flushed to disk upon mutation; must survive application crash or power loss).
9. **Mutability:** Versioned-Mutable (Transitions through explicit project lifecycle states; updates update `revision_number`).
10. **Relationships & Referential Requirements:** Root aggregate. Parent of Blueprint (`DAT-PRJ-002`), links to Game Identities (`DAT-CMP-001`), references Evidence (`DAT-EVD-001`).
11. **Storage Mechanism:** Tier 1: Structured JSON (`project.json`, `mashup-blueprint.json`) with atomic write-rename (`File.Replace`).
12. **Backup & Recovery Needs:** Critical. Must be included in all project backups, snapshots, and exports.
13. **Deletion & Retention Rules:** Retained indefinitely. Soft-delete moves to `.trash` folder; hard delete requires explicit user confirmation.
14. **Versioning Requirements:** Schema versioning via Semantic Versioning (`schema_version: "1.0.0"`).
15. **Relevant Risks & ADRs:** `ADR-0007` (Storage Baseline), `ADR-0009` (Workspace Isolation), `RSK-05` (Staging Corrupted).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

### 3.2 Category 2: Compatibility & Feasibility Data (`DAT-CMP-001`)
1. **Stable Data Category ID:** `DAT-CMP-001`
2. **Description:** Technical feasibility assessments, evaluated integration approaches, capability declarations, known limitations, open research questions, and risk records for specific game pairs.
3. **Authoritative Owner:** `CompatibilityAssessmentService` (`NexLudic.Core`)
4. **Primary Consumers:** Project Planning UI, AI Guidance Engine, Feasibility Reviewers.
5. **Data Sensitivity:** Internal / Technical (No personal data; contains modding compatibility rules).
6. **Expected Volume / Growth:** 20 KB to 2 MB per game pair; static baseline with incremental user project overrides.
7. **Required Lifetime:** Permanent for authoritative engine assessments; project-specific overrides bound to project lifecycle.
8. **Durability Requirement:** Immediate Durable.
9. **Mutability:** Versioned-Mutable (Historical assessments are archived/superseded rather than overwritten in place).
10. **Relationships & Referential Requirements:** References Game Versions (`game_id`, `version_hash`), links to Evidence IDs (`DAT-EVD-001`) and Risk IDs.
11. **Storage Mechanism:** Tier 1: Structured JSON (`feasibility-assessment.json`, `risk-register.json`).
12. **Backup & Recovery Needs:** Included in project backup packages.
13. **Deletion & Retention Rules:** Historical assessments must remain accessible to interpret past architecture decisions.
14. **Versioning Requirements:** SemVer schema versioning; strict tracking of target game binary hashes (SHA-256).
15. **Relevant Risks & ADRs:** `ADR-0007`, `REQ-FEAS-001`, `REQ-FEAS-002`.
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** Cross-project shared feasibility caching mechanism in Phase 3.

---

### 3.3 Category 3: Evidence & Source Records (`DAT-EVD-001`)
1. **Stable Data Category ID:** `DAT-EVD-001`
2. **Description:** Epistemic grounding citations (Epistemic Tiers 1–4: Empirical Test Results, Official Documentation, Decompiled Source, Modding Wiki/Forum).
3. **Authoritative Owner:** `EvidenceRegistry` (`NexLudic.Core`)
4. **Primary Consumers:** Compatibility Engine, Audit & Verification UI, AI Reasoning Validator.
5. **Data Sensitivity:** Public / Technical (Documentation links, citation notes, and test hashes).
6. **Expected Volume / Growth:** 50 KB to 5 MB per project; strictly append-only.
7. **Required Lifetime:** Immutable Permanent. Must persist as long as dependent capability claims exist.
8. **Durability Requirement:** Immediate Durable.
9. **Mutability:** Immutable / Append-Only (Evidence is never mutated; contradictory evidence creates a new record).
10. **Relationships & Referential Requirements:** Referenced by Capability Claims, Feasibility Decisions, and Test Executions.
11. **Storage Mechanism:** Tier 1: Append-only JSON array (`evidence-registry.json`).
12. **Backup & Recovery Needs:** Included in project exports and full backups.
13. **Deletion & Retention Rules:** Evidence records cannot be deleted while referenced by active capability claims.
14. **Versioning Requirements:** Evidence Schema `v1.0.0`; immutable timestamp and creator attribution.
15. **Relevant Risks & ADRs:** `ADR-0007`, `NLE-DOM-SPEC-001` (Principle 05: Evidence Provenance).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

### 3.4 Category 4: Adapter & Dependency Metadata (`DAT-ADP-001`)
1. **Stable Data Category ID:** `DAT-ADP-001`
2. **Description:** Adapter descriptors, declared capabilities, supported game versions, contract interfaces, required tools (e.g. Creation Kit, BepInEx), and runtime dependencies.
3. **Authoritative Owner:** `AdapterRegistryService` (`NexLudic.Core`)
4. **Primary Consumers:** Adapter Supervisor, Process Launcher, Compatibility Evaluator.
5. **Data Sensitivity:** Internal / Technical.
6. **Expected Volume / Growth:** 5 KB to 50 KB per adapter manifest; bounded by installed adapters.
7. **Required Lifetime:** Persistent across application lifetime; updated when adapters are upgraded.
8. **Durability Requirement:** Durable-Flushed.
9. **Mutability:** Versioned-Mutable (Updated on adapter installation, upgrade, or re-qualification).
10. **Relationships & Referential Requirements:** Maps Adapter ID to Supported Game Versions, Capability Traits, and Tool Dependencies.
11. **Storage Mechanism:** Tier 1: Dedicated manifest file (`adapter.manifest.json`) in adapter installation folder.
12. **Backup & Recovery Needs:** Included in full application backup; excluded from lightweight project-only exports.
13. **Deletion & Retention Rules:** Retained until adapter is uninstalled. Historical project manifests retain snapshot copy of descriptor.
14. **Versioning Requirements:** Adapter Manifest Schema SemVer; specifies `min_core_contract_version`.
15. **Relevant Risks & ADRs:** `ADR-0002` (Out-of-Process Adapters), `ADR-0005`, `NLE-SPC-MSG-001`.
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** Sandboxed adapter signature verification format.

---

### 3.5 Category 5: AI Task & Approval Records (`DAT-TSK-001`)
1. **Stable Data Category ID:** `DAT-TSK-001`
2. **Description:** Audit trail of AI-assisted scaffolding requests, prompt scopes, generated diffs, user review decisions (Approved, Rejected, Modified), and execution results.
3. **Authoritative Owner:** `AITaskOrchestrator` (`NexLudic.Core`)
4. **Primary Consumers:** Audit Log UI, Diff Reviewer, Rollback Manager.
5. **Data Sensitivity:** Confidential / User Private (May contain prompt instructions and user code snippets).
6. **Expected Volume / Growth:** 100 KB to 20 MB per project depending on generation volume.
7. **Required Lifetime:** Project lifetime; can be pruned/archived by user after successful integration.
8. **Durability Requirement:** Immediate Durable for approval records; buffered for in-flight tasks.
9. **Mutability:** Append-Only (New tasks append to log; status updates transition deterministically).
10. **Relationships & Referential Requirements:** Links to Project ID, references generated Artifact IDs (`DAT-ART-001`).
11. **Storage Mechanism:** Tier 1: Structured JSON log (`ai-task-history.json`).
12. **Backup & Recovery Needs:** Included in full project recovery backups; excluded from minimal project exports.
13. **Deletion & Retention Rules:** Retained for minimum 30 days post-creation; prunable on project archive.
14. **Versioning Requirements:** Schema SemVer `1.0.0`.
15. **Relevant Risks & ADRs:** `ADR-0008` (AI Provider Abstraction), `ADR-0009` (Command Approval Model).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** Data minimization rules for long raw model reasoning traces.

---

### 3.6 Category 6: Generated Artifacts & Workspace Files (`DAT-ART-001`)
1. **Stable Data Category ID:** `DAT-ART-001`
2. **Description:** Actual generated mod files, compiled scripts (.pex, .dll, .gd), staged asset copies, configuration files, and mod package manifests.
3. **Authoritative Owner:** `ArtifactManagerService` (`NexLudic.Core`)
4. **Primary Consumers:** Game Mod Managers, External Compilers, Game Executables.
5. **Data Sensitivity:** Confidential / User Intellectual Property.
6. **Expected Volume / Growth:** 10 MB to 2 GB per project (dominated by audio, textures, or binary plugins).
7. **Required Lifetime:** Project lifecycle; intermediate build files cleanable on demand.
8. **Durability Requirement:** Immediate Durable on disk; managed via staging lifecycle.
9. **Mutability:** Mutable during staging; content-addressed and frozen upon user approval.
10. **Relationships & Referential Requirements:** Tracked via artifact manifest (`artifacts-manifest.json`) recording relative paths and SHA-256 hashes.
11. **Storage Mechanism:** Tier 2: Filesystem storage in `<project_root>/staging/` and `<project_root>/artifacts/`.
12. **Backup & Recovery Needs:** Staging files snapshot before risky operations. Large external assets excluded from lightweight exports.
13. **Deletion & Retention Rules:** User can clean staging cache. Validated artifacts retained until project deletion.
14. **Versioning Requirements:** Content-addressed hashing (SHA-256); associated with project release tags.
15. **Relevant Risks & ADRs:** `ADR-0009` (Workspace Isolation), `REQ-MOD-003` (Non-Distributive Asset Policy).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** Deduplication across multiple projects sharing identical asset patches.

---

### 3.7 Category 7: Testing & Validation Data (`DAT-TST-001`)
1. **Stable Data Category ID:** `DAT-TST-001`
2. **Description:** Test specifications, acceptance criteria, test execution runs, pass/fail results, runtime logs, and regression metrics.
3. **Authoritative Owner:** `ValidationWorkflowEngine` (`NexLudic.Core`)
4. **Primary Consumers:** QA Dashboard, Compatibility Service, Evidence Registry.
5. **Data Sensitivity:** Internal / Technical.
6. **Expected Volume / Growth:** 50 KB to 5 MB per test suite run.
7. **Required Lifetime:** Retained for life of project or until superseded by newer test runs.
8. **Durability Requirement:** Immediate Durable upon test run completion.
9. **Mutability:** Append-Only Test Runs (Old test runs remain immutable; new runs create new entries).
10. **Relationships & Referential Requirements:** References Test Spec ID, Project ID, Adapter ID, and generated Evidence ID.
11. **Storage Mechanism:** Tier 1: Structured JSON (`validation-runs.json`).
12. **Backup & Recovery Needs:** Included in full project backups.
13. **Deletion & Retention Rules:** Keep last 10 successful runs and all failed runs; auto-purge older passing runs.
14. **Versioning Requirements:** Test Suite Schema SemVer `1.0.0`.
15. **Relevant Risks & ADRs:** `ADR-0010` (Testing Strategy), `REQ-FEAS-003`.
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

### 3.8 Category 8: Runtime & Operational State (`DAT-OPS-001`)
1. **Stable Data Category ID:** `DAT-OPS-001`
2. **Description:** Ephemeral process handles, live IPC channel connections, heartbeat ping/pong status, in-flight operation cancellation tokens, and live telemetry queues.
3. **Authoritative Owner:** `ProcessSupervisor` & `CommunicationGateway` (`NexLudic.Core`)
4. **Primary Consumers:** Core Runtime Dispatcher, Desktop UI Health Monitor.
5. **Data Sensitivity:** Ephemeral / Internal.
6. **Expected Volume / Growth:** 10 MB to 100 MB in volatile RAM.
7. **Required Lifetime:** Process lifetime; destroyed upon application exit or crash.
8. **Durability Requirement:** Ephemeral / Non-Durable (Zero disk persistence; reconstructed on launch).
9. **Mutability:** Highly Volatile (Updated at up to 60 Hz).
10. **Relationships & Referential Requirements:** In-memory references to active worker processes and channels.
11. **Storage Mechanism:** Tier 4: In-Memory C# collections, `System.Threading.Channels`, and memory pools.
12. **Backup & Recovery Needs:** None.
13. **Deletion & Retention Rules:** Automatically reclaimed on worker/core shutdown.
14. **Versioning Requirements:** Runtime contract version negotiated during handshake.
15. **Relevant Risks & ADRs:** `ADR-0002`, `ADR-0003`, `NLE-ARC-IPC-001`.
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

### 3.9 Category 9: Diagnostics & Audit Information (`DAT-LOG-001`)
1. **Stable Data Category ID:** `DAT-LOG-001`
2. **Description:** Structured diagnostic events, exception stack traces, performance metrics, process lifecycle events, and security audit warnings.
3. **Authoritative Owner:** `LoggingService` (`NexLudic.Infrastructure` via Serilog)
4. **Primary Consumers:** Developers, Support Diagnostics, Desktop UI Console Sink.
5. **Data Sensitivity:** Internal / Technical (MUST be scrubbed of API keys, passwords, and sensitive paths).
6. **Expected Volume / Growth:** 1 MB to 20 MB per day; capped at 500 MB total.
7. **Required Lifetime:** 14-day rolling retention window; older files auto-purged.
8. **Durability Requirement:** Buffered / Flushed periodically (every 1s) and immediately on unhandled crashes.
9. **Mutability:** Append-Only text / ndjson log files.
10. **Relationships & Referential Requirements:** Emits `trace_id` and `operation_id` for distributed log correlation.
11. **Storage Mechanism:** Tier 3: Serilog rolling file sinks in `%LocalAppData%/NexLudic/logs/`.
12. **Backup & Recovery Needs:** Excluded from project backups; attached to bug reports upon explicit user request.
13. **Deletion & Retention Rules:** Auto-deleted when older than 14 days or when total log volume exceeds 500 MB.
14. **Versioning Requirements:** Serilog compact JSON format schema.
15. **Relevant Risks & ADRs:** `ADR-0007` (Diagnostics Baseline), `REQ-SAFE-002` (Local Privacy).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

### 3.10 Category 10: User Preferences & Application Configuration (`DAT-CFG-001`)
1. **Stable Data Category ID:** `DAT-CFG-001`
2. **Description:** Application-wide user preferences, UI theme, default project folder on Volume D:, detected game install locations, and encrypted AI provider credential tokens.
3. **Authoritative Owner:** `ConfigurationManager` (`NexLudic.Core`)
4. **Primary Consumers:** Desktop UI Shell, Process Launcher, AI Provider Manager.
5. **Data Sensitivity:** Confidential / Restricted (Contains encrypted API tokens).
6. **Expected Volume / Growth:** 5 KB to 50 KB per user profile.
7. **Required Lifetime:** Permanent across application upgrades; stored per user account.
8. **Durability Requirement:** Immediate Durable.
9. **Mutability:** Versioned-Mutable (Updated via Settings UI).
10. **Relationships & Referential Requirements:** References local drive paths and installed tools.
11. **Storage Mechanism:** Tier 5: JSON config in `%LocalAppData%/NexLudic/appsettings.json` with sensitive tokens encrypted via Windows DPAPI.
12. **Backup & Recovery Needs:** Excluded from project exports (contains machine-specific paths and user credentials).
13. **Deletion & Retention Rules:** Retained until application uninstallation.
14. **Versioning Requirements:** Configuration Schema SemVer `1.0.0`.
15. **Relevant Risks & ADRs:** `ADR-0007`, `REQ-SEC-001` (Zero Elevation), `DEC-009` (Volume D: default).
16. **Decision Status:** Approved Baseline.
17. **Outstanding Questions:** None.

---

## 4. Master Data Classification & Decision Matrix Summary

| Category ID | Category Name | Authoritative Owner | Tier | Primary Format | Sensitivity | Durability | Mutability | Backup Scope | Decision Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`DAT-PRJ-001`** | Core Project Data | `ProjectCoordinator` | Tier 1 | Structured JSON | Confidential | Immediate | Versioned | Full Backup & Export | **Approved** |
| **`DAT-CMP-001`** | Compatibility & Feasibility | `CompatibilityService` | Tier 1 | Structured JSON | Internal | Immediate | Versioned | Full Backup & Export | **Approved** |
| **`DAT-EVD-001`** | Evidence & Citations | `EvidenceRegistry` | Tier 1 | Append-Only JSON | Public/Int | Immediate | Immutable | Full Backup & Export | **Approved** |
| **`DAT-ADP-001`** | Adapter Descriptors | `AdapterRegistry` | Tier 1 | Manifest JSON | Internal | Flushed | Versioned | App Backup Only | **Approved** |
| **`DAT-TSK-001`** | AI Task History & Diff | `AITaskOrchestrator` | Tier 1 | Structured JSON | Confidential | Immediate | Append-Only | Full Backup Only | **Approved** |
| **`DAT-ART-001`** | Generated Mod Artifacts| `ArtifactManager` | Tier 2 | Filesystem Staging | IP/Private | Immediate | Staged/Hash | Snapshots & Select Export | **Approved** |
| **`DAT-TST-001`** | Testing & Validation | `ValidationEngine` | Tier 1 | Structured JSON | Internal | Immediate | Append-Only | Full Backup Only | **Approved** |
| **`DAT-OPS-001`** | Runtime & Session State| `ProcessSupervisor` | Tier 4 | In-Memory RAM | Ephemeral | Ephemeral | Volatile | None (Reconstructed) | **Approved** |
| **`DAT-LOG-001`** | Diagnostics & Logs | `LoggingService` | Tier 3 | Serilog Rolling | Internal | Buffered | Append-Only | Bug Reports Only | **Approved** |
| **`DAT-CFG-001`** | User Config & Tokens | `ConfigurationManager` | Tier 5 | JSON + DPAPI | Restricted | Immediate | Versioned | App Profile Only | **Approved** |

