# PROJECT WORKSPACE STATUS AND EMPTY STATE SPECIFICATION

**Document Identifier:** `NLE-SPC-STS-002`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 21 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Cross-Destination Status, Epistemic Indicators & Empty States  
**Parent Document:** `NLE-SPC-WKS-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md))  

---

## 1. Cross-Destination Status Indicators

Project indicators surface authoritative operational conditions across both the Project Context Header and the Project Navigation Sidebar:

```
+----------------------------------------------------------------------------------------------------+
| STATUS INDICATOR TAXONOMY MATRIX                                                                   |
+---------------------+-------------------+---------------------+----------+-------------------------+
| Indicator Name      | Authoritative     | Visual Presentation | Severity | Navigation Target       |
|                     | Source            |                     |          |                         |
+---------------------+-------------------+---------------------+----------+-------------------------+
| Unsaved Changes     | `IsDirty` Bit     | Amber Dot & Label   | Info     | `Ctrl+S` Save Action    |
| Pending Approvals   | `tasks.json`      | Blue Counter Badge  | Warning  | `NAV-PRJ-AI` (AI Tasks) |
| Active Background Op| `TaskWorkerPool`  | Rotating Spinner    | Info     | Status Tray Drawer      |
| Critical Blocker    | `evidence.json`   | Red Alert Shield    | Critical | `NAV-PRJ-FEA` (Feasib)  |
| Validation Failure  | `test_results.json`| Red Cross Chip     | High     | `NAV-PRJ-TST` (Testing) |
| Stale Feasibility   | Cache Timestamp   | Yellow Refresh Chip | Warning  | `NAV-PRJ-FEA` (Audit)   |
| Incomplete Reqs     | `requirements.json`| Orange Info Chip   | Low      | `NAV-PRJ-REQ` (Reqs)    |
| Recovery Required   | `audit.db` / Lock | Flashing Red Banner | High     | `NAV-PRJ-DIA` (Recovery)|
+---------------------+-------------------+---------------------+----------+-------------------------+
```

### 1.1 Strict Badge Count Derivation Rules
- **No Vanity Counts:** Badge counts are strictly tied to real, actionable records (e.g. number of pending diff approvals, or number of failing test cases).
- **Project Isolation:** Badge counts in the project sidebar reflect ONLY the active project. Unread counts or tasks from other projects are never aggregated into the active project sidebar.
- **Reactive Invalidation:** When a diff is approved or rejected, the badge counter updates immediately via `INotifyPropertyChanged` without requiring a manual page refresh.

---

## 2. Empty States Across All 12 Destinations

Empty states in the NexLudic Engine are active orientation guides that direct users to meaningful next actions rather than dead ends.

### 2.1 Destination-Specific Empty State Catalog

| Destination ID | Empty State Condition | User Guidance Message | Primary Action Button |
| :--- | :--- | :--- | :--- |
| `NAV-PRJ-OVR` | Newly created project | "Welcome to your new mashup workspace. Begin by defining your preliminary requirements or reviewing game paths." | `[Start Requirements Setup]` |
| `NAV-PRJ-DET` | Unverified game paths | "Game paths are unverified. Locate game executables to enable adapter probing." | `[Detect Installed Executables]` |
| `NAV-PRJ-REQ` | 0 requirements declared | "No capability requirements declared. Enter requested features or extract them from your mashup thesis." | `[+ Add Requirement]` |
| `NAV-PRJ-FEA` | Feasibility unevaluated | "Feasibility: UNEVALUATED. Evaluate your declared capabilities against engine databases." | `[Run Feasibility Audit]` |
| `NAV-PRJ-PLN` | No approved plan | "Candidate strategy selected. Generate architectural milestones to begin implementation planning." | `[Generate Plan Milestones]` |
| `NAV-PRJ-ADP` | 0 adapters bound | "No adapters bound to participant games. Scan local mod loaders or browse catalog." | `[Scan Local Adapters]` |
| `NAV-PRJ-AI` | 0 AI tasks queued | "No active or queued AI tasks. Dispatch an AI agent to scaffold adapters or generate configs." | `[New AI Task]` |
| `NAV-PRJ-TST` | 0 test suites run | "Zero test runs recorded. Note: Zero tests does not imply compatibility." | `[Run Smoke Tests]` |
| `NAV-PRJ-ART` | Staging folder empty | "Staging directory is clean. Generated mod artifacts will appear here during builds." | `[Open Staging Folder]` |
| `NAV-PRJ-DIA` | 0 error events | "Telemetry nominal. Zero runtime warnings or diagnostic errors recorded." | `[Run System Health Check]` |
| `NAV-PRJ-HIS` | Initial creation only | "Project created on <Date>. Historical actions and audits will log chronologically." | `[Export Initial Audit]` |
| `NAV-PRJ-CFG` | Defaults active | "All project configuration values are inheriting standard schema defaults." | `[Edit Configuration Keys]` |

---

## 3. Loading, Partial-Data & Degraded States

### 3.1 Skeleton Loaders vs Spinners
- Primary data grids (Requirements, Tasks, Tests) use lightweight skeleton placeholder bars during asynchronous data fetching to prevent UI jitter.
- Action buttons display embedded progress rings while an asynchronous operation is in flight.

### 3.2 Degraded & Read-Only Fallback
- If the AI Orchestration provider is offline or unauthenticated:
  - `NAV-PRJ-AI` remains accessible to view historical tasks and diffs.
  - New prompt submission controls are disabled with an explanatory tooltip: *"AI Provider Offline: Configure credentials in Global Settings"*.
- If Volume `D:` has read-only permissions:
  - The Project Context Header displays an amber banner: *"Read-Only Workspace Mode: File writes disabled"*.
  - All mutating action buttons (`[Save]`, `[Approve Diff]`, `[Add Requirement]`) are disabled.

