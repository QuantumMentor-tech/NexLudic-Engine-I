# PROJECT WORKSPACE LAYOUT & CONTEXTUAL NAVIGATION SPECIFICATION

**Document Identifier:** `NLE-SPC-WKS-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 21 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Information Architecture, Workspace Layout & Navigation  
**Target Desktop Framework:** WPF / XAML (.NET 8.0 LTS) via MVVM Pattern  
**Related Documents:**
- `NLE-SPC-SHL-001`: Application Shell & Layout Specification ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/APPLICATION_SHELL_AND_LAYOUT_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/APPLICATION_SHELL_AND_LAYOUT_SPECIFICATION.md))
- `NLE-CAT-NAV-001`: Navigation Hierarchy & Destination Catalog ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/NAVIGATION_HIERARCHY_AND_DESTINATION_CATALOG.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/NAVIGATION_HIERARCHY_AND_DESTINATION_CATALOG.md))
- `NLE-SPC-CTX-001`: Project Context & Switching UX Specification ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CONTEXT_AND_SWITCHING_UX_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CONTEXT_AND_SWITCHING_UX_SPECIFICATION.md))
- `NLE-SPC-WIZ-001`: Project Creation Wizard Specification ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))
- `NLE-DOM-SPEC-001`: Core Domain Model & Capability Contract ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/CORE_DOMAIN_MODEL_AND_CAPABILITY_CONTRACT_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/CORE_DOMAIN_MODEL_AND_CAPABILITY_CONTRACT_SPECIFICATION.md))
- `ADR-0009`: Workspace Filesystem Isolation & Safety Sandbox ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/decisions/ADR-0009-workspace-filesystem-isolation-and-safety-sandbox.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/decisions/ADR-0009-workspace-filesystem-isolation-and-safety-sandbox.md))

---

## 1. Executive Summary & Workspace Definition

The **Project Workspace** is the core operational environment of the NexLudic Engine desktop application. It provides creators and engineers with a unified, context-bound interface to evaluate, design, develop, validate, and manage an integration between two participant games.

### 1.1 What the Workspace Is
- **A Project-Scoped Operational Environment:** All data, views, actions, diagnostics, tasks, and artifacts displayed inside the workspace belong strictly to the single currently active project context (`ActiveProjectId`).
- **An Evidence-Bound Presentation Layer:** The workspace faithfully visualizes epistemic certainty tiers (`DECLARED`, `DETECTED`, `PROBED`, `VERIFIED`) defined in `NLE-DOM-SPEC-001`. It never represents unverified claims or candidate plans as proven compatibility.
- **A Progressive Workflow Coordinator:** Accommodates early-stage exploratory research projects (containing only a name, two games, and a raw idea) as smoothly as mature, code-backed hybrid mashup projects with compiled adapters, automated test suites, and staged assets.

### 1.2 What the Workspace Is Not
- **Not the Application Shell:** The Shell manages global window chrome, top-level navigation, global notifications, user profiles, and active project switching. The Workspace lives inside the Shell's `ContentRegion`.
- **Not the Operating System Filesystem Directory:** While a project maps to an isolated folder on Volume `D:\` (`D:\NexLudic\Projects\<slug>\`), the workspace is a semantic domain model view, not an arbitrary raw file explorer.
- **Not a Running Game Process or Adapter Runtime:** The workspace coordinates and inspects external processes through secure IPC channels (`NLE-IPC-SPEC-001`), but does not embed or execute arbitrary game binaries inside the UI thread.
- **Not an Authoritative Business Logic Engine:** The UI presents status and captures user intent; all feasibility scoring, adapter qualification, persistence, and task execution are performed by authoritative backend domain services.

---

## 2. Global vs. Project-Scoped Navigation Boundary

To prevent cognitive confusion and cross-project data leakage, the interface maintains an uncompromising boundary between **Global Navigation** and **Project-Scoped Navigation**:

```
+----------------------------------------------------------------------------------------------------+
| APPLICATION SHELL: Global Title Bar (Project Selector, Active Project Badge, Window Controls)       |
+-------------------+--------------------------------------------------------------------------------+
| GLOBAL NAV RAIL   | PROJECT CONTEXT HEADER (Project Name, Game A <-> Game B, Lifecycle, Quick Actions) |
| (Always Visible)  +------------------------+-------------------------------------------------------+
|                   | PROJECT NAVIGATION     | MAIN WORKSPACE CONTENT REGION                         |
| [Home / Overview] | (Contextual to Active) |                                                       |
| [Projects List]   |                        |  [Breadcrumb: Project > Requirements > REQ-004]       |
| [Adapter Catalog] | [Project Overview]     |  +-------------------------------------------------+  |
| [AI Engine Center]| [Games & Environment]  |  | Page Header & Primary Context Actions           |  |
| [Global Settings] | [Requirements & Caps]  |  +-------------------------------------------------+  |
| [Global Diag]     | [Feasibility Research] |  | Active Destination Viewport                     |  |
|                   | [Integration Plan]     |  | (Data Grids, Spec Cards, Visual Graph, etc.)    |  |
|                   | [Adapters & Bindings]  |  |                                                 |  |
|                   | [AI Tasks & Approvals] |  +-------------------------------------------------+  |
|                   | [Testing & Validation] | OPTIONAL CONTEXTUAL DETAIL PANEL                      |
|                   | [Workspace Artifacts]  | (Entity Metadata, Inspector, Evidence Citations)      |
|                   | [Project Diagnostics]  |                                                       |
|                   | [Project History]      |                                                       |
|                   | [Project Settings]     |                                                       |
+-------------------+------------------------+-------------------------------------------------------+
| APPLICATION SHELL: Global Status Tray (Active Operation, Background Workers, IPC Health)          |
+----------------------------------------------------------------------------------------------------+
```

### 2.1 Boundary Rules & Invariants
1. **Zero Context Leakage:** Selecting a global destination (e.g. `Global Settings` or `Global Diagnostics`) preserves the active project in memory but clears the active project viewport, ensuring global settings are not misconstrued as project-scoped overrides.
2. **Missing Active Project Behavior:** If a user enters a project-scoped destination (via URL deep link, command palette, or startup state) without an active project loaded:
   - The workspace immediately displays an informative empty state: *"No Active Project Selected"*.
   - A primary action button `[Select Existing Project]` routes to `NAV-PRJ-001` (Projects Manager).
   - A secondary action button `[Create New Project]` launches `NAV-WIZ-001` (Project Creation Wizard).
3. **Cross-Project Task Integrity:** If a user clicks an operation notification originating from *Project A* while *Project B* is currently loaded:
   - The shell intercepts navigation and evaluates unsaved changes in Project B.
   - If Project B has unsaved changes, an unsaved-change dialog is presented.
   - Upon confirmation, context formally switches to Project A before the destination task view opens.

---

## 3. Candidate Layout Regions & Structural Layout

The Project Workspace comprises six distinct architectural regions:

### 3.1 Region 1: Global Application Navigation (Left Rail)
- Managed by `ApplicationShellView`. Width: 60px (icon rail) or 200px (expanded).
- Houses top-level application destinations: Home, Projects, Adapter Catalog, AI Orchestration, Diagnostics, Global Settings.

### 3.2 Region 2: Project Context Header
- Anchored at the top of the Project Workspace area (`WorkspaceHeaderRegion`).
- Height: 52px fixed.
- **Key Display Elements:**
  - **Project Icon & Display Name:** Bold heading with truncation tooltip.
  - **Participant Game Badges:** Distinct chips for `Game A (Host)` and `Game B (Donor)` with platform icons (Steam, GOG, Custom).
  - **Lifecycle State Badge:** Visual chip representing current project maturity (`CREATED`, `RESEARCH`, `PLANNING`, `PROTOTYPE`, `VALIDATED`, `BLOCKED`, `ARCHIVED`).
  - **Feasibility Indicator:** Summarized chip (`UNEVALUATED`, `FEASIBLE`, `CONDITIONAL`, `BLOCKED`).
  - **Unsaved Changes Pill:** Visible amber dot and label when dirty state exists (`IsProjectDirty == true`).
  - **Context Actions:** `[Save Project (Ctrl+S)]`, `[Export Package]`, `[Switch Project]`.

### 3.3 Region 3: Project-Scoped Navigation (Sidebar)
- Dedicated project workflow navigation sidebar (`ProjectNavRegion`).
- Width: 220px (default), collapsible to 48px (compact icon-only), or hidden.
- Hosts the 12 authoritative project destinations grouped into logical operational phases:
  - **Overview:** Project Overview (`NAV-PRJ-OVR`).
  - **Definition & Research:** Games & Environment (`NAV-PRJ-DET`), Requirements & Capabilities (`NAV-PRJ-REQ`), Feasibility & Research (`NAV-PRJ-FEA`).
  - **Architecture & Build:** Integration Plan (`NAV-PRJ-PLN`), Adapters & Bindings (`NAV-PRJ-ADP`), AI Tasks & Approvals (`NAV-PRJ-AI`), Workspace Artifacts (`NAV-PRJ-ART`).
  - **Verification & Ops:** Testing & Validation (`NAV-PRJ-TST`), Diagnostics & Recovery (`NAV-PRJ-DIA`), Project History (`NAV-PRJ-HIS`), Project Settings (`NAV-PRJ-CFG`).

### 3.4 Region 4: Main Content Region
- Primary workspace surface (`WorkspaceContentRegion`).
- **Hierarchy:**
  1. **Breadcrumb Bar:** Displays hierarchical path (e.g. `Active Project > Feasibility Research > Skyrim Tick Hook Evidence`).
  2. **View Header:** Destination Title, Subtitle, and Primary Page Actions (e.g. `[+ Add Requirement]`, `[Run Feasibility Audit]`).
  3. **Content Body:** Responsive data grids, visual node graphs, Markdown documentation viewers, diff inspectors, or specification cards.
  4. **Scroll Architecture:** Controlled vertical scrolling with virtualized UI elements (`VirtualizingStackPanel`) for large record collections.

### 3.5 Region 5: Contextual Detail Panel (Dockable Inspector)
- Positioned on the right side of the main content (`ContextualDetailRegion`).
- Width: 320px to 480px (resizable), collapsible via toggle button or `Ctrl+B`.
- **Purpose:** Inspects the active record selected in the main content region (e.g. viewing raw evidence citations when inspecting a capability, or reviewing tool output diffs when inspecting an AI task).
- **Non-Ephemeral Rule:** Essential workflow actions must never be placed exclusively in this panel; it is an inspector and metadata viewer, not an exclusive action gate.

### 3.6 Region 6: Shell Global Status Tray
- Fixed at the bottom of the window (Height: 28px).
- Displays active background worker tasks, IPC connectivity state, memory usage, and volume storage status on `D:`.

---

## 4. Workspace Lifecycle Adaptability

The workspace must gracefully render projects across all lifecycle tiers without UI breakage:

```
+----------------------------------------------------------------------------------------------------+
| PROJECT MATURITY ADAPTATION MATRIX                                                                 |
+---------------------+-------------------------------+----------------------------------------------+
| Project State       | Navigational Presentation     | Typical Destination Behavior                 |
+---------------------+-------------------------------+----------------------------------------------+
| Newly Created       | All 12 destinations visible;  | Requirements & Feasibility show setup guides;|
| (Post-Wizard)       | badge counts set to 0 or null | Testing/Artifacts show informative empties   |
+---------------------+-------------------------------+----------------------------------------------+
| Active Research     | Feasibility & Requirements    | Feasibility displays active evidence cards;  |
|                     | highlighted with badge counts | Implementation & Testing show 'Not Started'  |
+---------------------+-------------------------------+----------------------------------------------+
| Blocked by Evidence | Red blocker indicator on      | Feasibility shows critical blocking citation;|
|                     | Context Header & Feasibility  | Integration Plan shows warning banner        |
+---------------------+-------------------------------+----------------------------------------------+
| Active Development  | AI Tasks & Adapters show      | Task review queues active; Staging artifacts |
|                     | active progress badges        | populated; Real-time IPC logs stream         |
+---------------------+-------------------------------+----------------------------------------------+
| Validated / Release | All validation suites green;  | Clean build manifest; Export Package enabled;|
|                     | Read-only lock optional       | Full historical audit log accessible         |
+---------------------+-------------------------------+----------------------------------------------+
```

---

## 5. Layout Stability & Window Resizing Rules

1. **Minimum Resolution:** The workspace guarantees full functionality at **1024 x 700 pixels** without control clipping.
2. **DPI Awareness:** Fully supports Windows Per-Monitor DPI V2 (100%, 125%, 150%, 175%, 200%).
3. **No Dynamic Layout Shifts (CLS = 0):** Asynchronous loading of metadata, background task updates, or badge counts must not reposition navigation controls, shift buttons, or cause scroll jumps.
4. **Adaptive Collapsing:**
   - Width < 1280px: Contextual Detail Panel automatically docks into a slide-over drawer mode.
   - Width < 1100px: Project Navigation Sidebar automatically collapses to icon-only rail (48px).
   - Width >= 1280px: Both Sidebar and Detail Panel can remain pinned simultaneously.

