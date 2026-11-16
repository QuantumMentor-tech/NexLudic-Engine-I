# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Application Shell, Navigation & Global Layout Specification

**Document ID:** `NLE-SPC-SHL-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Specification  
**Classification:** Desktop Presentation Shell Specification  
**Traceability:** `ADR-0003` (Desktop UI Framework), `NLE-ARC-PH2-001` (Phase 2 Baseline), `NLE-HND-PH3-001` (Phase 3 Handoff), `DEC-009` (Volume D: Allocation)

---

## 1. Executive Summary & Shell Objectives

The **NexLudic Engine Application Shell** is the master desktop presentation frame hosted by `NexLudic.App`. Built on **Windows Presentation Foundation (WPF)** on **.NET 8.0 LTS** (`ADR-0003`) utilizing modern Fluent Design principles, the shell coordinates all user interactions across game-pair project authoring, feasibility assessments, AI-assisted mod scaffolding, and live interoperability monitoring.

The shell enforces strict architectural boundaries:
1. **Zero Domain Logic in UI:** The presentation layer manages views, view models, and user feedback; all compatibility determinations, capability verifications, and disk operations are delegated to `NexLudic.Core`.
2. **Deterministic Contextual Navigation:** The shell clearly segregates **Global Destinations** (Settings, Project Manager) from **Project-Scoped Destinations** (Blueprints, Adapters, AI Scaffolding, Live Telemetry).
3. **Continuous Safety & Approval Visibility:** The shell maintains permanent global visibility over pending human-in-the-loop approvals (`ADR-0009`), active background tasks, and application health.

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC DESKTOP APPLICATION SHELL                              |
+----------------------------------------------------------------------------------------------------+
| [Logo] NexLudic Engine | Project: [Skyrim x Subnautica v0.1.0 (Strategy A)] [Unsaved *] | [ _ [] X ]|
+--------------------+---------------------------------------------------+---------------------------+
| PRIMARY NAVIGATION | MAIN CONTENT VIEWPORT                             | CONTEXTUAL INSPECTOR      |
| (Collapsible Rail) | (Dynamic ViewModel Region)                        | (Collapsible 320px Drawer)|
|                    |                                                   |                           |
| [O] Overview       |  +---------------------------------------------+  | [i] Project Summary       |
| [P] Projects       |  | View Header: Mashup Blueprint Editor        |  | - Strategy: Host Mod (A)  |
| [C] Compatibility  |  | Breadcrumbs: Projects > Alpha > Blueprint   |  | - Game A: Skyrim SE       |
| [A] Adapters       |  +---------------------------------------------+  | - Game B: Subnautica      |
| [T] AI Tasks (2)   |  |                                             |  |                           |
| [V] Validation     |  | [ Main Workspace Content / Forms / Grids ]  |  | [!] Pending Approvals (1) |
| [W] Workspace      |  |                                             |  | - Papyrus Script Patch    |
| [D] Diagnostics    |  |                                             |  |   [Review Diff Modal]     |
|                    |  |                                             |  |                           |
| [S] Settings       |  +---------------------------------------------+  | [x] Close Inspector       |
+--------------------+---------------------------------------------------+---------------------------+
| GLOBAL STATUS BAR: [OK] Core Ready | Background Tasks: (1 Running) [===>   ] | Memory: 84 MB | Vol D: OK |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Shell Responsibility Matrix & Boundary Allocations

To preserve the Clean Architecture boundaries established in Step 10 and the Phase 3 Handoff (`NLE-HND-PH3-001`), responsibilities are strictly allocated:

```
+----------------------------------------------------------------------------------------------------+
|                                    SHELL RESPONSIBILITY MATRIX                                     |
+-------------------------------+-----------------------+---------------------+----------------------+
| Functional Concern            | Shell (NexLudic.App)  | Core Workflows      | Domain Layer         |
+-------------------------------+-----------------------+---------------------+----------------------+
| Main Window & Layout Regions  | **Authoritative Owner**| No awareness        | No awareness         |
| Navigation Routing & History  | **Authoritative Owner**| Subscribes to events| No awareness         |
| Project Selection State       | Presentation Context  | **Authoritative**   | Root Aggregate Entity|
| Feasibility Calculation       | Displays formatted tag| **Evaluates Rules** | Enforces Invariants  |
| Staging File Modification     | Triggers Core Command | **Enforces Jail**   | Defines Contracts    |
| Human Diff Approval Dialog    | Renders Modal UI      | **Validates Gate**  | Records AI Approval  |
| DPAPI Credential Input        | Renders Masked Field  | **DPAPI Encrypts**  | No awareness         |
| Process Sandbox Supervision   | Displays Health Status| **Job Object Watch**| No awareness         |
+-------------------------------+-----------------------+---------------------+----------------------+
```

### Explicit Shell Exclusions:
* The shell **MUST NOT** directly invoke Win32 `CreateProcess` or launch external tools.
* The shell **MUST NOT** directly write or modify JSON project manifests on disk.
* The shell **MUST NOT** evaluate whether a game integration is compatible or bypass safety checks.

---

## 3. Main Window Structural Regions

The main desktop window is divided into five coordinated structural regions:

### 3.1 Region 1: Application Window Frame & Titlebar
* **Height:** 40 pixels.
* **Contents:**
  - Left: NexLudic brand icon and application title (`NexLudic Engine`).
  - Center: **Project Context Badge** showing the active project name, version tag, strategy discriminator (`[Strategy A]`), and dirty state indicator (`[Unsaved *]`).
  - Right: Global Search shortcut (`Ctrl+K`), Environment Profile indicator (`[DEV]` if non-production), and standard OS window controls (Minimize, Maximize/Restore, Close).

### 3.2 Region 2: Primary Navigation Rail / Sidebar
* **State 1 (Collapsed Rail - Default for compact widths):** 48 pixels wide. Displays vertical column of Fluent glyph icons with high-contrast active indicator and hover tooltips.
* **State 2 (Expanded Sidebar):** 240 pixels wide. Slides out smoothly, displaying category grouping labels, icon glyphs, text labels, and notification badge counters.
* **Toggle Mechanism:** Hamburger toggle button at top of rail or `Ctrl+B` keyboard shortcut.

### 3.3 Region 3: Main Content Viewport
* **Layout:** Occupies remaining central space. Hosts the dynamic view resolved by the active ViewModel (`ContentControl` with View/ViewModel locator).
* **Information Density Support:** Dynamically adjusts padding and typography to support diverse workflows:
  - Form Density (Spacious: 24px margins) for project creation wizards.
  - Data Density (Compact: 8px table padding) for compatibility matrices and test run logs.
  - Full-Bleed Split View for code editors and unified diff comparisons.

### 3.4 Region 4: Contextual Inspector & Auxiliary Drawer
* **Width:** 320 pixels (resizable between 280px and 480px).
* **Visibility:** Collapsible. Toggled via top-right `[Inspector]` button or `Ctrl+I`.
* **Contents:** Displays contextual details of the currently selected item in the main viewport (e.g. detailed evidence citations for a selected capability, pending diff summaries for an AI task, or hardware resource usage for a running adapter).

### 3.5 Region 5: Global Status Bar & Notification Tray
* **Height:** 28 pixels. Fixed to the bottom of the window frame.
* **Contents:**
  - Core supervisor health status (`[OK] Core Ready` / `[!] Degraded`).
  - Background task progress indicator (animated progress bar and task label).
  - Pending approval alert (`[!] 1 Approval Required` clickable to open Diff Review Modal).
  - Storage drive health status (`Volume D: 412 GB Free`).

---

## 4. Shell Responsive Breakpoints & Sizing Rules

In accordance with Step 16 Phase 3 Handoff, the desktop shell supports diverse Windows display resolutions and display scaling (DPI 100% to 200%):

```
+----------------------------------------------------------------------------------------------------+
|                                    RESPONSIVE SHELL BREAKPOINTS                                    |
+---------------------+-------------------+---------------------+------------------------------------+
| Breakpoint Tier     | Window Width      | Navigation State    | Contextual Inspector Behaviour     |
+---------------------+-------------------+---------------------+------------------------------------+
| **Tier 1: Compact** | 1024px to 1279px  | Collapsed Rail (48px)| Closed by default; overlays content |
| (Minimum Bound)     | (Min: 1024x700)   | (Icons Only)        | when toggled (modal overlay).      |
+---------------------+-------------------+---------------------+------------------------------------+
| **Tier 2: Standard**| 1280px to 1599px  | Expanded (240px) or | Docked side-by-side (320px).       |
| (Default Baseline)  | (1280x800 default)| Collapsed by user   | Content viewport takes remainder.  |
+---------------------+-------------------+---------------------+------------------------------------+
| **Tier 3: Widescreen| 1600px and above  | Expanded (240px)    | Permanently docked side-by-side    |
| (Multi-Monitor)     | (1920x1080 typical| permanently open    | with expanded diff preview.        |
+---------------------+-------------------+---------------------+------------------------------------+
```

### Sizing Invariants:
* **Absolute Minimum Dimensions:** `1024 x 700` pixels. Resizing below this threshold is restricted via Win32 `WM_GETMINMAXINFO` to prevent control clipping.
* **High-DPI Support:** All layout dimensions utilize density-independent pixels (`DIPs`). Icons are vector-based XAML `Path` geometries or Segoe Fluent Icons, scaling cleanly without pixelation at 125%, 150%, and 200% Windows display scaling.

