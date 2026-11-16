# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Shell, Navigation, Accessibility & State Validation Plan

**Document ID:** `NLE-PLN-SHL-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Quality Assurance & UI Validation Test Plan  
**Traceability:** `ADR-0003` (Desktop UI), `ADR-0010` (Testing Strategy), `NLE-SPC-SHL-001` (Shell Specification)

---

## 1. Executive Summary & Validation Scope

The **Shell, Navigation, Accessibility, and State Validation Plan** establishes the automated testing suites, manual test protocols, accessibility verifications, and state consistency checks governing the NexLudic Engine desktop shell.

It defines a **10-Level UI Validation Suite** and formalizes **10 AI Coding Agent Rules for Desktop UI Implementation** to ensure that future frontend development adheres strictly to accessibility, performance, and Clean Architecture constraints.

---

## 2. The 10-Level Shell Validation Suite

```
+----------------------------------------------------------------------------------------------------+
|                                THE 10-LEVEL SHELL VALIDATION PLAN                                  |
|                                                                                                    |
|  Level 1: Navigation Structure & Scope Tests (Global vs Project Route Separation)                 |
|  Level 2: Shell Layout & Region Rendering Tests (Frame, Rail, Viewport, Inspector, Status Bar)    |
|  Level 3: Project Context & Switching Tests (Unsaved Edits Interception, Stale Context Purge)      |
|  Level 4: Navigation History & Breadcrumb Tests (Context Restoration, Deep-Link Traversal)         |
|  Level 5: Interaction State Tests (Empty State, Skeleton Loading, Error Banner, Uncertain States)  |
|  Level 6: Accessibility & Keyboard Tests (Tab Order, Focus Restoration, Screen Reader Semantics)   |
|  Level 7: Window Layout & DPI Tests (1024x700 Min Bound, 125%/150%/200% DPI Scaling, Off-Screen)  |
|  Level 8: Consequential Approval Gate Tests (Diff Review Modal Enforcement, Auto-Deploy Block)     |
|  Level 9: State Consistency Tests (ViewModel Lifecycle Coupling, Zero Data Leaks Across Switches) |
|  Level 10: Visual & Binding Regression Tests (Snapshot Tests, Theme Switching, Memory Profiling)  |
+----------------------------------------------------------------------------------------------------+
```

---

### Detailed Test Specifications:

#### Level 1: Navigation Structure & Scope Tests
* `TC-SHL-L1-01`: Verify that clicking `NAV-CMP-001` (Compatibility) without an active project redirects to `NAV-PRJ-001` with an informative banner.
* `TC-SHL-L1-02`: Verify global settings (`NAV-SET-001`) remain accessible both with and without an active project.

#### Level 2: Shell Layout & Region Rendering Tests
* `TC-SHL-L2-01`: Toggle Navigation Rail between collapsed (48px) and expanded (240px); assert content viewport resizes smoothly.
* `TC-SHL-L2-02`: Toggle Contextual Inspector drawer (320px); assert right-docked alignment without overlapping primary actions.

#### Level 3: Project Context & Switching Tests
* `TC-SHL-L3-01`: Edit blueprint -> trigger project switch -> assert Modal Prompt appears ("Save changes before switching?").
* `TC-SHL-L3-02`: Click [Cancel] on unsaved prompt -> assert application remains in current project with zero data loss.
* `TC-SHL-L3-03`: Active background task in progress -> assert warning modal appears blocking project unload.

#### Level 4: Navigation History & Breadcrumb Tests
* `TC-SHL-L4-01`: Navigate through 3 destinations -> press `Alt+LeftArrow` -> assert history navigates backward cleanly.
* `TC-SHL-L4-02`: Deep link to specific capability ID resolves within project context without dropping active project badge.

#### Level 5: Interaction State Tests
* `TC-SHL-L5-01`: Launch application with empty project directory -> assert Welcome & Quick Start screen renders.
* `TC-SHL-L5-02`: Simulated adapter crash during compilation renders **Uncertain Outcome** banner with rollback prompt.

#### Level 6: Accessibility & Keyboard Tests
* `TC-SHL-L6-01`: Complete entire project creation wizard using `Tab`, `Arrow`, `Enter`, and `Esc` keys with zero mouse input.
* `TC-SHL-L6-02`: Verify high-contrast focus rings (2px solid outline) are visible on all interactive buttons and inputs.
* `TC-SHL-L6-03`: Verify screen reader announcements for background task completion using WPF UI Automation peers.

#### Level 7: Window Layout & DPI Tests
* `TC-SHL-L7-01`: Resize window to minimum boundary (`1024x700`); assert zero clipped controls or unreadable text.
* `TC-SHL-L7-02`: Test shell at 150% and 200% Windows display scaling; assert vector icons remain crisp without blur.
* `TC-SHL-L7-03`: Simulate disconnected secondary monitor; assert window repositions to primary monitor center.

#### Level 8: Consequential Approval Gate Tests
* `TC-SHL-L8-01`: External mod file staged -> assert prominent Amber alert badge appears in titlebar and status bar.
* `TC-SHL-L8-02`: Click alert -> assert Unified Diff Review Modal opens showing side-by-side changes and file checksums.
* `TC-SHL-L8-03`: Verify that file deployment commands are disabled until the user explicitly clicks `[Approve & Apply]`.

#### Level 9: State Consistency Tests
* `TC-SHL-L9-01`: Switch from Project A to Project B -> assert all Project A ViewModels are disposed and memory reclaimed.
* `TC-SHL-L9-02`: Verify zero residual dirty flags or staged file paths carry over from Project A to Project B.

#### Level 10: Visual & Binding Regression Tests
* `TC-SHL-L10-01`: Rapidly switch between Light and Dark themes 50 times; assert zero UI rendering glitches or crashes.
* `TC-SHL-L10-02`: Continuous 60 Hz telemetry graph rendering for 30 minutes; assert zero memory leaks in UI process.

---

## 3. AI Coding Agent Rules for Desktop UI Implementation

Future AI coding agents implementing UI components MUST strictly adhere to these 10 rules:

1. **No Domain Logic in Views/ViewModels:** ViewModels MUST NOT calculate feasibility scores, inspect game binaries directly, or perform file writes. They strictly invoke `NexLudic.Core` commands.
2. **Preserve the MVVM Pattern:** All user interactions must bind to `ICommand` or asynchronous methods (`RelayCommand`). No code-behind business logic in `.xaml.cs` files.
3. **No Hardcoded Drive Paths:** The default project root MUST be retrieved from `ConfigurationManager` (Volume `D:` default). Never hardcode `C:\...` paths.
4. **Always Intercept Window Closing:** Never bypass the `Window_Closing` event handler that evaluates dirty project state and active tasks.
5. **No Nested Scrollbars:** Every view must place its primary scrollbar on the outer viewport `ScrollViewer`.
6. **Mask All Secrets in UI:** Never render API tokens or private keys in plaintext UI controls; use masked password boxes or truncated strings (`sk-...****`).
7. **Adhere to Minimum Window Bounds:** Never set window minimum dimensions below `1024 x 700`.
8. **Enforce Human Approval Modals:** Never add automated "skip review" checkboxes to diff approval screens.
9. **Dispose ViewModels on Switch:** When a project is closed or switched, all associated ViewModel event subscriptions must be cleanly unregistered to prevent memory leaks.
10. **Stop and Consult:** If a requested UI feature requires modifying domain entities or bypassing Clean Architecture boundaries, halt execution and request architectural review.

