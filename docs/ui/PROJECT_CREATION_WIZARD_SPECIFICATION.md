# PROJECT CREATION WIZARD & GAME-PAIR SETUP SPECIFICATION

**Document Identifier:** `NLE-SPC-WIZ-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Architectural Specification  
**Security Classification:** Public / Implementation-Neutral Specification  
**Target Desktop Framework:** WPF / XAML (.NET 8.0 LTS) via MVVM Pattern  
**Related Documents:**
- `NLE-CAT-NAV-001`: Navigation Hierarchy & Destination Catalog ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/NAVIGATION_HIERARCHY_AND_DESTINATION_CATALOG.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/NAVIGATION_HIERARCHY_AND_DESTINATION_CATALOG.md))
- `NLE-SPC-SHL-001`: Application Shell & Layout Specification ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/APPLICATION_SHELL_AND_LAYOUT_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/APPLICATION_SHELL_AND_LAYOUT_SPECIFICATION.md))
- `NLE-DOM-SPEC-001`: Core Domain Model & Capability Contract ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/CORE_DOMAIN_MODEL_AND_CAPABILITY_CONTRACT_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/CORE_DOMAIN_MODEL_AND_CAPABILITY_CONTRACT_SPECIFICATION.md))
- `NLE-ARC-DOC-004`: Storage, Schema & Migration Architecture ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/STORAGE_SCHEMA_AND_MIGRATION_ARCHITECTURE.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/STORAGE_SCHEMA_AND_MIGRATION_ARCHITECTURE.md))
- `ADR-0009`: Workspace Filesystem Isolation & Safety Sandbox ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/decisions/ADR-0009-workspace-filesystem-isolation-and-safety-sandbox.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/decisions/ADR-0009-workspace-filesystem-isolation-and-safety-sandbox.md))

---

## 1. Executive Summary & Purpose

The **Project Creation Wizard** (`NLE-SPC-WIZ-001`) defines the primary user intake and setup experience for initiating game mashup and cross-game interoperability projects within the NexLudic Engine desktop shell.

### 1.1 Core Mission
The wizard guides creators through configuring a validated, isolated two-game mashup project environment without forcing premature technical commitments or introducing false certainty. It captures:
1. Project identity, metadata, and workspace naming.
2. Two distinct participant games (Game A and Game B) selected from the known catalog or defined manually.
3. Versioning, edition, distribution channel, and local environment specifications for both games.
4. Creator intent, core mashup thesis, and primary integration objectives in verbatim user language.
5. Preliminary capability requirements (events, state reading, live control, asset imports).
6. Initial integration strategy preference (Host Modding, Live Bridge, or Hybrid) as a starting baseline.
7. Verification review, feasibility disclaimers, and atomic project workspace initialization on storage volume `D:`.

### 1.2 Epistemic Rules & Platform Safeguards
In accordance with `NLE-DOM-SPEC-001` and `REQ-FEAS-001`:
- **Creation != Compatibility:** Completing the wizard establishes project scope, intent, and workspace structure; it does *not* assert or guarantee technical compatibility between the two games.
- **Declared != Verified:** User-entered game paths, versions, and capability claims are assigned provenance tier `DECLARED` until verified by Phase 4 adapter discovery and Phase 5 capability probing.
- **Feasibility is UNEVALUATED:** Initial feasibility status is explicitly stamped as `UNEVALUATED`. No feasibility scoring is fabricated during wizard execution.
- **Offline Single-Player Guardrail:** Projects involving online anti-cheat-protected titles are flagged with hard blocking errors (`REQ-SAFE-001`).

---

## 2. Seven-Stage Wizard Architecture

The wizard operates as a progressive 7-stage modal or dedicated workspace workflow within the Application Shell's main content area (`ContentRegion`):

```
+---------------------------------------------------------------------------------------------------+
| Project Creation Wizard                                                                           |
| [Stage 1: Basics] -> [Stage 2: Games] -> [Stage 3: Versions] -> [Stage 4: Goals]                  |
|                   -> [Stage 5: Requirements] -> [Stage 6: Strategy] -> [Stage 7: Review & Create]  |
+---------------------------------------------------------------------------------------------------+
| Stage Content Body (WPF UserControl per Stage)                                                    |
|                                                                                                   |
| [Field Controls, Interactive Pickers, Validation Badges, Helper Text, Disclaimers]                 |
|                                                                                                   |
+---------------------------------------------------------------------------------------------------+
| Wizard Status Bar & Navigation Tray                                                               |
| [Cancel Wizard]                                                   [< Back]  [Next Step > / Create]|
+---------------------------------------------------------------------------------------------------+
```

### 2.1 Stage Overview Matrix

| Stage | Name | Key Purpose | Primary Domain Entities Touched | Next Step Prerequisite |
| :--- | :--- | :--- | :--- | :--- |
| **Stage 1** | **Project Basics** | Establish project display name, slug, description, and workspace path | `ProjectManifest`, `ProjectId`, `WorkspaceConfig` | Valid project name, unique slug, available target path on `D:` |
| **Stage 2** | **Game Pair Selection** | Designate Game A (Primary/Host) and Game B (Secondary/Donor) | `GameDefinition`, `GamePairBinding`, `GameRole` | Two distinct games selected; duplicate game error cleared |
| **Stage 3** | **Version & Environment** | Capture platform, edition, release version, and optional install paths | `GameInstallation`, `VersionProvenance`, `EnvironmentProfile` | Validated or acknowledged version info; anti-cheat safety cleared |
| **Stage 4** | **Mashup Goal & Thesis** | Record raw creator vision, user-language goal, and expected gameplay loop | `MashupThesis`, `IntegrationIntent`, `OriginalUserWording` | Minimum 10 characters in core goal description |
| **Stage 5** | **Capability Requirements** | Declare preliminary requirements (telemetry, injection, assets, logic) | `CapabilityRequirement`, `EvidenceRequirement`, `EpistemicTier` | At least one capability category identified or 'Exploratory' flagged |
| **Stage 6** | **Strategy Preferences** | Select initial candidate architecture (Host Mod, Live Bridge, Hybrid) | `CandidateStrategy`, `StrategyPreference`, `ArchitecturePlan` | One candidate strategy selected with risk acceptance |
| **Stage 7** | **Review & Confirmation** | Cross-field summary, pre-flight safety audit, and atomic creation | `ProjectContext`, `PersistenceEnvelope`, `AuditLog` | Confirmation checkbox checked, pre-flight verification passed |

---

## 3. Post-Creation Handoff & State Initialization

Upon user confirmation in Stage 7:
1. **Atomic Workspace Provisioning:** The desktop service requests `IProjectManagementService.CreateProjectAsync(ProjectCreationDraft)`.
2. **Directory Scaffold on Volume D:**
   ```
   D:\NexLudic\Projects\<project_slug>\
   +-- project.json           (Master project manifest, schema version 1.0.0)
   +-- staging\               (Isolated staging jail for adapter/tool outputs)
   +-- backups\               (Local backup snapshots)
   +-- logs\                  (Project-scoped audit and execution logs)
   +-- metadata\              (Local project database/SQLite cache)
   ```
3. **Shell Context Transition:**
   - Active project context updates to new `<project_id>`.
   - Global title bar and Status Tray reflect `Active Project: <Display Name>`.
   - Shell router navigates from Wizard (`Destination: ProjectCreationWizard`) directly to the **Project Dashboard** (`Destination: ProjectDashboard`).
   - Project Dashboard displays the newly minted project in initial status: `Status: Created`, `Feasibility: Unevaluated`, `Verification: Pending Adapter Scan`.

---

## 4. Traceability & Compliance

- **FR-01 (Project Creation):** Fully satisfied by Stages 1 through 7.
- **REQ-SEC-001 (Medium Integrity Level):** Wizard operates with zero admin elevation.
- **REQ-SAFE-001 (Anti-Cheat Safety):** Enforced in Stage 3 validation.
- **DEC-009 (Volume D Workspace):** Enforced in Stage 1 storage validation.
- **NLE-DOM-SPEC-001 (Epistemic Hygiene):** Enforced across Stages 3, 5, 6, and 7.

