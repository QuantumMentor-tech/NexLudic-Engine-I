# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Navigation Hierarchy & Destination Catalog

**Document ID:** `NLE-CAT-NAV-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Specification  
**Classification:** Information Architecture & Destination Catalog  
**Traceability:** `NLE-SPC-SHL-001` (Shell Specification), `NLE-DOM-SPEC-001` (Core Domain Model), `NLE-HND-PH3-001` (Phase 3 Handoff)

---

## 1. Executive Summary & Information Architecture

This document establishes the comprehensive **Navigation Hierarchy and Destination Catalog** for NexLudic Engine. 

To maintain clean mental models for users, the navigation architecture strictly separates:
1. **Global Destinations:** Accessible at any time, requiring zero active project context (e.g. Project Manager, Global Settings, Engine Diagnostics).
2. **Project-Scoped Destinations:** Context-bound views representing the active mashup project (e.g. Blueprint Editor, AI Task Review, Workspace Staging). If no project is loaded, attempting to navigate here redirects to the Project Selector with an informative empty state.
3. **Context-Dependent Destinations:** Adapt their view depending on whether a project is loaded (e.g. Adapter Catalog shows installed adapters globally, or configured game-pair adapters when in a project).

---

## 2. Navigation Tree Architecture

```
                                  NEXLUDIC NAVIGATION TREE
                                             |
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |                    |
 [GLOBAL SCOPE]       [GLOBAL SCOPE]      [PROJECT SCOPE]      [CONTEXT DEPENDENT]  [PROJECT SCOPE]
 1. HOME / OVERVIEW   2. PROJECTS         3. COMPATIBILITY     4. ADAPTERS          5. AI TASKS
    `NAV-HOME-001`       `NAV-PRJ-001`       `NAV-CMP-001`        `NAV-ADP-001`        `NAV-TSK-001`
    - Recent Projects    - All Projects List - Game A vs Game B   - Installed Adapters - Prompt Scaffolder
    - Quick Start Guides - New Project Wizard- Feasibility Matrix - Qualified Status   - Diff Review Queue
    - Engine Health      - Import Package    - Capability Evidence- Staging Binding    - Task History Log
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |
 [PROJECT SCOPE]      [PROJECT SCOPE]     [CONTEXT DEPENDENT]  [GLOBAL SCOPE]
 6. VALIDATION & TESTS7. WORKSPACE & FILES8. DIAGNOSTICS & REC 9. SETTINGS
    `NAV-TST-001`        `NAV-ART-001`       `NAV-DIA-001`        `NAV-SET-001`
    - Test Suites        - Staging Explorer  - Serilog Trace Log  - UI Theme / Display
    - Execution History  - Artifact Manifest - Rollback Snapshots - Volume D: Path Config
    - Regression Reports - Diff Inspector    - Crash Dump Reports - DPAPI Secrets Vault
```

---

## 3. Comprehensive Destination Catalog

### 3.1 Destination 1: Home / Overview (`NAV-HOME-001`)
* **Scope:** Global.
* **Purpose:** Initial landing page upon launching without a project. Surfaces recent projects, system health, and quick actions.
* **Entry Prerequisites:** None.
* **Empty State:** Shows "Welcome to NexLudic Engine" banner and prominent [Create New Mashup Project] button.
* **Initial Release Status:** Active in Phase 3.

### 3.2 Destination 2: Projects Manager (`NAV-PRJ-001`)
* **Scope:** Global.
* **Purpose:** Browse, filter, clone, export, and delete game-pair projects on Volume `D:`.
* **Entry Prerequisites:** None.
* **Related Domain Entities:** `GamePairProject` aggregate collection.
* **Actions:** [New Project], [Import .nxpkg], [Open Folder on Volume D:], [Soft-Delete to Trash].
* **Initial Release Status:** Active in Phase 3.

### 3.3 Destination 3: Games & Compatibility (`NAV-CMP-001`)
* **Scope:** Project-Scoped.
* **Purpose:** Displays target game versions, executable SHA-256 hashes, evaluated feasibility assessments, and epistemic evidence citations.
* **Entry Prerequisites:** Active project loaded.
* **Related Domain Entities:** `GameIdentity`, `FeasibilityAssessment`, `EvidenceCitation`.
* **Empty State:** If no games selected, prompts user to link game install paths.
* **Initial Release Status:** Active in Phase 3.

### 3.4 Destination 4: Adapters & Integrations (`NAV-ADP-001`)
* **Scope:** Context-Dependent (Global catalog when no project; project adapter bindings when active).
* **Purpose:** Configure out-of-process worker adapters (e.g. Creation Kit / BepInEx / Godot), inspect traits, and test IPC connections.
* **Entry Prerequisites:** None for catalog; active project required to bind adapter.
* **Related Domain Entities:** `AdapterDescriptor`, `IGameAdapterContract`.
* **Initial Release Status:** Preview in Phase 3; fully functional in Phase 5.

### 3.5 Destination 5: AI Tasks & Scaffolding (`NAV-TSK-001`)
* **Scope:** Project-Scoped.
* **Purpose:** Review AI-generated mod stubs, configure prompt scopes, and inspect pending unified diffs requiring human approval.
* **Entry Prerequisites:** Active project loaded.
* **Related Domain Entities:** `AITaskRecord`, `AITaskApprovalRecord`.
* **Access Gating:** Diff approval modal is hardcoded; no auto-commit without review (`ADR-0009`).
* **Initial Release Status:** Active in Phase 3 (UI Mock & Diff Viewer); live execution in Phase 5.

### 3.6 Destination 6: Testing & Validation (`NAV-TST-001`)
* **Scope:** Project-Scoped.
* **Purpose:** View test specifications, trigger automated test suite runs, inspect pass/fail metrics, and view regression logs.
* **Entry Prerequisites:** Active project loaded.
* **Related Domain Entities:** `TestSpecification`, `TestExecutionRecord`.
* **Initial Release Status:** Preview in Phase 3; fully functional in Phase 6.

### 3.7 Destination 7: Workspace & Artifacts (`NAV-ART-001`)
* **Scope:** Project-Scoped.
* **Purpose:** Tree navigation of `<project>/staging/` and `<project>/artifacts/`, viewing file checksums, and staging isolation validation.
* **Entry Prerequisites:** Active project loaded.
* **Related Domain Entities:** `ArtifactManifest`, `SandboxedFileSystem`.
* **Initial Release Status:** Active in Phase 3.

### 3.8 Destination 8: Diagnostics & Recovery (`NAV-DIA-001`)
* **Scope:** Context-Dependent.
* **Purpose:** Global Serilog log stream, process health monitoring, pre-operation snapshot restoration, and crash diagnostics.
* **Entry Prerequisites:** None for global logs; active project required for project rollback.
* **Related Domain Entities:** `RollbackSnapshot`, `SerilogDiagnosticSink`.
* **Initial Release Status:** Active in Phase 3.

### 3.9 Destination 9: Settings & Preferences (`NAV-SET-001`)
* **Scope:** Global.
* **Purpose:** Desktop theme configuration, Volume `D:` default storage root, environment profile selection (`Development` / `Testing` / `Production`), and encrypted Windows DPAPI secrets vault.
* **Entry Prerequisites:** None.
* **Security Controls:** API keys rendered as masked strings; zero plaintext export.
* **Initial Release Status:** Active in Phase 3.

---

## 4. Handling Unavailable & Deferred Destinations

When a user selects a destination that is not supported for the current project strategy (e.g. attempting to view Live Telemetry when the project is strictly Strategy A Host Mod):
1. The navigation item is **NOT silently hidden** (preventing user disorientation).
2. The destination renders an **Informative Explanation Banner**:
   > *"Live Telemetry is not enabled for this project. Strategy A (Host-Game Modification) uses offline mod script generation. To enable live state observation, switch project strategy to Strategy B (Live Bridge) or Strategy C (Hybrid) in the Project Blueprint Editor."*
3. A direct action button is provided: `[Configure Project Strategy]`.

