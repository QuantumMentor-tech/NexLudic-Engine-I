# PROJECT DESTINATION CATALOG AND INFORMATION ARCHITECTURE

**Document Identifier:** `NLE-CAT-WKS-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 21 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Destination Catalog & Information Architecture Specification  
**Parent Document:** `NLE-SPC-WKS-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md))  

---

## 1. Information Architecture Map

The project workspace provides 12 distinct, first-class destinations organized into four operational clusters:

```
[PROJECT WORKSPACE ROOT] (Context: ActiveProjectId)
 |
 +--- CLUSTER 1: PROJECT OVERVIEW & METADATA
 |     +--- 1.1 Project Overview (`NAV-PRJ-OVR`)
 |     +--- 1.2 Games & Environment (`NAV-PRJ-DET`)
 |
 +--- CLUSTER 2: SPECIFICATION & RESEARCH
 |     +--- 2.1 Requirements & Capabilities (`NAV-PRJ-REQ`)
 |     +--- 2.2 Feasibility & Research (`NAV-PRJ-FEA`)
 |     +--- 2.3 Integration Plan (`NAV-PRJ-PLN`)
 |
 +--- CLUSTER 3: IMPLEMENTATION & ORCHESTRATION
 |     +--- 3.1 Adapters & Bindings (`NAV-PRJ-ADP`)
 |     +--- 3.2 AI Tasks & Approvals (`NAV-PRJ-AI`)
 |     +--- 3.3 Workspace Artifacts (`NAV-PRJ-ART`)
 |
 +--- CLUSTER 4: VERIFICATION, DIAGNOSTICS & MANAGEMENT
       +--- 4.1 Testing & Validation (`NAV-PRJ-TST`)
       +--- 4.2 Diagnostics & Recovery (`NAV-PRJ-DIA`)
       +--- 4.3 Project History & Audit (`NAV-PRJ-HIS`)
       +--- 4.4 Project Configuration (`NAV-PRJ-CFG`)
```

---

## 2. Comprehensive Destination Catalog

### 2.1 Destination 1: Project Overview (`NAV-PRJ-OVR`)
- **User-Facing Label:** Overview
- **Purpose:** Executive dashboard summarizing project health, participant games, core thesis, active feasibility status, open blockers, recent tasks, and recommended next actions.
- **Information Domain:** Project Summary & High-Level State Aggregation.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded (`ActiveProjectId != null`).
- **Required Upstream Capabilities:** Project Manifest (`project.json`).
- **Source-of-Truth Owner:** `ProjectManifestService`, `IProjectAggregateRepository`.
- **Entry Conditions:** Standard landing destination upon opening any project.
- **Primary Actions:** `[Edit Basics]`, `[Review Feasibility]`, `[Create Task]`, `[Export Package]`.
- **Related Destinations:** All project destinations via quick-action cards.
- **Empty State:** Shows initial onboarding checklist for newly created projects.
- **Unavailable State:** Displays "Project Not Found" redirect if project folder is deleted.
- **Permission Requirements:** Standard User (Read).
- **Current Status:** Approved Specification (`Step 19 / Step 21`).
- **Traceability:** `FR-01`, `NLE-CAT-NAV-001`.

---

### 2.2 Destination 2: Games & Environment (`NAV-PRJ-DET`)
- **User-Facing Label:** Games & Environment
- **Purpose:** Inspect and manage the two participant games, target executables, directory paths, detected versions, platform distributions (Steam/GOG/Custom), and runtime architecture (x64/x86).
- **Information Domain:** Game Participant Metadata & Local Installation Environments.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Game Definition Catalog, Local Filesystem Prober.
- **Source-of-Truth Owner:** `GamePairBindingService`, `GameEnvironmentRepository`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Verify Install Paths]`, `[Detect Game Versions]`, `[Swap Host/Donor Roles]`, `[Safety Scan]`.
- **Related Destinations:** `NAV-PRJ-FEA` (Feasibility), `NAV-PRJ-ADP` (Adapters).
- **Empty State:** Displays warning if game paths are unconfigured or unverified.
- **Unavailable State:** Read-only mode if project is locked for execution.
- **Permission Requirements:** Standard User (Read/Write).
- **Current Status:** Approved Specification (`NLE-SPC-GPS-001`).
- **Traceability:** `NLE-DOM-SPEC-001`, `ADR-0009`.

---

### 2.3 Destination 3: Requirements & Capabilities (`NAV-PRJ-REQ`)
- **User-Facing Label:** Requirements & Capabilities
- **Purpose:** Manage the creator's structured mashup requirements, requested capabilities (state reading, event injection, asset swaps), sync models, and verbatim user intent text.
- **Information Domain:** Mashup Thesis & Capability Contracts.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Capability Contract Schema.
- **Source-of-Truth Owner:** `RequirementsManagementService`, `requirements.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[+ Add Requirement]`, `[Apply Template]`, `[Run AI Extraction]`, `[Map to Capabilities]`.
- **Related Destinations:** `NAV-PRJ-FEA` (Feasibility), `NAV-PRJ-TST` (Validation).
- **Empty State:** Shows raw thesis text with prompt: *"No formal requirements declared yet. Click [+ Add Requirement] or [Run AI Extraction]"*.
- **Unavailable State:** Read-only during automated test execution.
- **Permission Requirements:** Standard User (Read/Write).
- **Current Status:** Approved Specification (`NLE-SPC-REQ-001`).
- **Traceability:** `NLE-DOM-SPEC-001`, `NLE-CAT-WIZ-001`.

---

### 2.4 Destination 4: Feasibility & Research (`NAV-PRJ-FEA`)
- **User-Facing Label:** Feasibility & Research
- **Purpose:** Inspect technical feasibility evaluations, compatibility matrices, research findings, risk ratings, and epistemic evidence citations.
- **Information Domain:** Technical Research, Reverse Engineering Evidence, Feasibility Logic.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Feasibility Analysis Engine (Phase 5).
- **Source-of-Truth Owner:** `FeasibilityAssessmentService`, `evidence.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Run Feasibility Audit]`, `[View Evidence Citations]`, `[Override Assessment (Expert)]`, `[Export Feasibility Report]`.
- **Related Destinations:** `NAV-PRJ-REQ` (Requirements), `NAV-PRJ-PLN` (Integration Plan).
- **Empty State:** Displays *"Feasibility: UNEVALUATED. Click [Run Feasibility Audit] to evaluate declared capabilities against known game engines."*
- **Unavailable State:** Disclaimers shown if upstream research database is offline.
- **Permission Requirements:** Standard User (Read). Expert Override requires Explicit Confirmation.
- **Current Status:** Approved Specification (`Step 5 / Step 6 / Step 21`).
- **Traceability:** `REQ-FEAS-001`, `NLE-DOM-SPEC-001`.

---

### 2.5 Destination 5: Integration Plan (`NAV-PRJ-PLN`)
- **User-Facing Label:** Integration Plan
- **Purpose:** Review, configure, and refine the chosen integration strategy (Strategy A: Host Mod, Strategy B: Live Bridge, Strategy C: Hybrid), architectural milestones, and module boundaries.
- **Information Domain:** Architecture Design & Implementation Strategy.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Feasibility Assessment (advisory).
- **Source-of-Truth Owner:** `IntegrationPlanningService`, `strategy.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Select Strategy]`, `[Generate Implementation Milestones]`, `[Review Architecture Diagram]`.
- **Related Destinations:** `NAV-PRJ-FEA` (Feasibility), `NAV-PRJ-ADP` (Adapters), `NAV-PRJ-AI` (AI Tasks).
- **Empty State:** Displays candidate strategy selected during wizard with prompt to formally approve or refine plan.
- **Unavailable State:** Blocked if critical feasibility rating is `IMPOSSIBLE` without strategy adjustment.
- **Permission Requirements:** Standard User (Read/Write).
- **Current Status:** Approved Specification (`NLE-CAT-WIZ-001`, Step 10).
- **Traceability:** `NLE-DOM-SPEC-001`, `ADR-0002`.

---

### 2.6 Destination 6: Adapters & Bindings (`NAV-PRJ-ADP`)
- **User-Facing Label:** Adapters & Bindings
- **Purpose:** Inspect, qualify, configure, and bind game adapters, mod loaders (e.g. SKSE, MelonLoader), hook injection libraries, and IPC transport endpoints.
- **Information Domain:** Game Integration Adapters & Inter-Process Communication.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Adapter Host Runtime (Phase 4).
- **Source-of-Truth Owner:** `AdapterRegistryService`, `IpcTransportService`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Scan Local Adapters]`, `[Bind Adapter to Game A/B]`, `[Test IPC Ping]`, `[Configure Endpoints]`.
- **Related Destinations:** `NAV-PRJ-PLN` (Plan), `NAV-PRJ-DIA` (Diagnostics).
- **Empty State:** *"No adapters bound. Select an approved adapter from the catalog or generate custom adapter scaffolding."*
- **Unavailable State:** Disabled if games use active kernel anti-cheat (`REQ-SAFE-001`).
- **Permission Requirements:** Standard User (Medium IL).
- **Current Status:** Approved Specification (`Step 12 / Step 13 / Step 21`).
- **Traceability:** `NLE-IPC-SPEC-001`, `ADR-0008`.

---

### 2.7 Destination 7: AI Tasks & Approvals (`NAV-PRJ-AI`)
- **User-Facing Label:** AI Tasks & Approvals
- **Purpose:** Orchestrate AI-assisted scaffolding, code generation, schema translation, prompt execution, diff reviews, and pending human approvals.
- **Information Domain:** AI Agent Orchestration & Human-in-the-Loop Supervision.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** AI Orchestration Provider (`google-genai`).
- **Source-of-Truth Owner:** `AiTaskOrchestrationService`, `tasks.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[New AI Task]`, `[Review Diff]`, `[Approve Execution]`, `[Reject / Revise]`, `[Cancel Running Task]`.
- **Related Destinations:** `NAV-PRJ-ART` (Artifacts), `NAV-PRJ-HIS` (History).
- **Empty State:** *"No active or queued AI tasks. Click [New AI Task] to scaffold adapters, generate configs, or analyze code."*
- **Unavailable State:** Blocked if AI provider credentials are unconfigured or rate-limited.
- **Permission Requirements:** Human-in-the-loop approval required for file writes (`ADR-0009`).
- **Current Status:** Approved Specification (`Step 15 / Step 21`).
- **Traceability:** `REQ-SEC-001`, `ADR-0009`.

---

### 2.8 Destination 8: Testing & Validation (`NAV-PRJ-TST`)
- **User-Facing Label:** Testing & Validation
- **Purpose:** Configure, execute, and inspect automated test suites (unit, mock adapter, live IPC, integration harness), validation matrices, and regression logs.
- **Information Domain:** Quality Assurance, Test Suites & Runtime Verification.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Test Runner Harness (Phase 6).
- **Source-of-Truth Owner:** `ValidationExecutionService`, `test_results.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Run All Suites]`, `[Run IPC Ping Test]`, `[Inspect Test Output]`, `[Export Test Report]`.
- **Related Destinations:** `NAV-PRJ-REQ` (Requirements), `NAV-PRJ-DIA` (Diagnostics).
- **Empty State:** *"Zero test runs recorded. Click [Run All Suites] to execute preliminary smoke tests."* (Explicit distinction: Zero tests != Test Success).
- **Unavailable State:** Disabled during active destructive file operations.
- **Permission Requirements:** Standard User (Medium IL).
- **Current Status:** Approved Specification (`Step 21`).
- **Traceability:** `REQ-VAL-001`, `NLE-DOM-SPEC-001`.

---

### 2.9 Destination 9: Workspace & Artifacts (`NAV-PRJ-ART`)
- **User-Facing Label:** Workspace & Artifacts
- **Purpose:** Browse generated mod packages, transcompiled scripts, staging outputs (`<project>/staging/`), asset bundles, manifest hashes, and provenance records.
- **Information Domain:** Filesystem Sandboxing & Asset Artifact Management.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Sandboxed Workspace Storage on Volume `D:`.
- **Source-of-Truth Owner:** `WorkspaceArtifactService`, `staging_manifest.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Open Staging in File Explorer]`, `[Re-index Artifacts]`, `[Purge Staging Temp]`, `[Package .nxpkg]`.
- **Related Destinations:** `NAV-PRJ-AI` (AI Tasks), `NAV-PRJ-TST` (Testing).
- **Empty State:** *"Staging directory is clean. No generated mod artifacts present."*
- **Unavailable State:** Read-only if background compile task is active.
- **Permission Requirements:** Sandboxed to `D:\NexLudic\Projects\<slug>\` (`ADR-0009`).
- **Current Status:** Approved Specification (`ADR-0009`, `DEC-009`).
- **Traceability:** `REQ-SEC-001`, `ADR-0009`.

---

### 2.10 Destination 10: Diagnostics & Recovery (`NAV-PRJ-DIA`)
- **User-Facing Label:** Diagnostics & Recovery
- **Purpose:** Access project-scoped error logs, crash telemetry, IPC latency traces, memory profiles, snapshot rollbacks, and recovery wizards.
- **Information Domain:** System Telemetry, Error Recovery & Audit Logging.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Serilog Structured Logging, Snapshot Engine.
- **Source-of-Truth Owner:** `ProjectDiagnosticsService`, `logs/`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Refresh Logs]`, `[Export Diagnostic Bundle]`, `[Rollback to Last Known Good Snapshot]`, `[Clear Staging Cache]`.
- **Related Destinations:** `NAV-PRJ-HIS` (History), `NAV-DIA-001` (Global Diagnostics).
- **Empty State:** *"All diagnostic telemetry nominal. Zero error events recorded."*
- **Unavailable State:** Snapshot rollback disabled while task is actively executing.
- **Permission Requirements:** Standard User (Medium IL).
- **Current Status:** Approved Specification (`NLE-CAT-NAV-001`, Step 18).
- **Traceability:** `NLE-SPC-STS-001`, `ADR-0004`.

---

### 2.11 Destination 11: Project History & Audit (`NAV-PRJ-HIS`)
- **User-Facing Label:** Project History
- **Purpose:** Chronological timeline of all major project events, configuration revisions, feasibility audits, approved AI diffs, and validation runs.
- **Information Domain:** Event Sourcing & Audit Provenance.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** SQLite Local Audit Store.
- **Source-of-Truth Owner:** `ProjectHistoryService`, `audit.db`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Filter Timeline]`, `[Inspect Event Diff]`, `[Export Audit Report]`.
- **Related Destinations:** `NAV-PRJ-OVR` (Overview), `NAV-PRJ-DIA` (Diagnostics).
- **Empty State:** Shows initial project creation entry: *"Project initialized on <timestamp>"*.
- **Unavailable State:** Read-only at all times.
- **Permission Requirements:** Standard User (Read).
- **Current Status:** Approved Specification (`Step 14 / Step 21`).
- **Traceability:** `NLE-ARC-DOC-004`, `NLE-DOM-SPEC-001`.

---

### 2.12 Destination 12: Project Configuration (`NAV-PRJ-CFG`)
- **User-Facing Label:** Project Configuration
- **Purpose:** Manage project-specific settings, custom build arguments, staging overrides, notification verbosity, and adapter parameter bindings without altering global engine settings.
- **Information Domain:** Project Settings & Local Parameter Overrides.
- **Scope:** Project-Scoped.
- **Parent Destination:** Project Workspace Root.
- **Required Project Context:** Active Project Loaded.
- **Required Upstream Capabilities:** Configuration Management System (Step 15).
- **Source-of-Truth Owner:** `ProjectConfigurationService`, `project.json`.
- **Entry Conditions:** Active Project Loaded.
- **Primary Actions:** `[Save Settings]`, `[Revert to Defaults]`, `[Open project.json in Editor]`.
- **Related Destinations:** `NAV-SET-001` (Global Application Settings).
- **Empty State:** Displays schema defaults for all configurable keys.
- **Unavailable State:** Blocked during active test execution or compilation.
- **Permission Requirements:** Standard User (Read/Write).
- **Current Status:** Approved Specification (`Step 15 / Step 21`).
- **Traceability:** `NLE-SPC-CFG-001`, `ADR-0006`.

