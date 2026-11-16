# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Global Layout & Window Behaviour Specification

**Document ID:** `NLE-SPC-WIN-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Layout Architecture & Window Lifecycle Specification  
**Traceability:** `ADR-0003` (Desktop UI), `NLE-SPC-SHL-001` (Shell Specification), `NLE-HND-PH3-001` (Phase 3 Handoff)

---

## 1. Executive Summary & Purpose

The **Global Layout and Window Behaviour Specification** defines the spatial hierarchy, content width constraints, vertical scrolling mechanics, multi-monitor window restoration, and safe shutdown lifecycle governing the NexLudic Engine desktop shell.

---

## 2. Content Hierarchy & Sizing Rules

To optimize usability across high-density technical tasks and guided wizards, views adhere to standardized width profiles:

```
+----------------------------------------------------------------------------------------------------+
|                                    CONTENT WIDTH PROFILES                                          |
+----+----------------------+--------------------+---------------------------------------------------+
| Tier| Content Profile      | Max Width Bounds   | Applicable Workflows & Views                      |
+----+----------------------+--------------------+---------------------------------------------------+
| 01 | **Centered Form**    | Max: 720 pixels    | New Project Wizard, AI Provider Settings, Toolpath|
|    |                      | (Horiz Centered)   | Configuration, Authentication Modals.             |
+----+----------------------+--------------------+---------------------------------------------------+
| 02 | **Structured Report**| Max: 960 pixels    | Feasibility Assessment Summary, Evidence Audit    |
|    |                      | (Horiz Centered)   | Citation Reader, Test Run Diagnostics.            |
+----+----------------------+--------------------+---------------------------------------------------+
| 03 | **Full-Bleed Grid**  | 100% Viewport      | Compatibility Matrix Table, Unified Diff Reviewer,|
|    |                      | (Dynamic Stretch)  | Staging File Tree Explorer, Live Telemetry Graphs.|
+----+----------------------+--------------------+---------------------------------------------------+
```

### 2.1 Vertical Scrolling Invariant:
* **Zero Nested Scrollbars:** Scrollbars are strictly confined to the primary `ScrollViewer` enclosing the content viewport. Nested scroll areas within child cards or forms are prohibited, preventing scroll trapping and trackpad gesture confusion.

---

## 3. Window Startup & Geometry Restoration Lifecycle

When NexLudic Engine launches, the window position and geometry are restored from `%LocalAppData%\NexLudic\appsettings.json`:

```json
{
  "window_geometry": {
    "left": 100,
    "top": 100,
    "width": 1280,
    "height": 800,
    "is_maximized": false,
    "navigation_rail_collapsed": false,
    "inspector_drawer_open": true,
    "inspector_width": 320
  }
}
```

### Multi-Monitor Disconnect Fallback Protection:
* During startup, the engine queries the Win32 `SystemParametersInfo` and `VirtualScreen` bounds.
* If the saved `(left, top)` coordinates fall outside the visible virtual desktop (e.g. an external monitor was unplugged while the laptop is undocked):
  - The saved position is automatically discarded.
  - The window is centered on the primary monitor with default dimensions: `1280 x 800`.
  - An informational log event is recorded in Serilog.

---

## 4. Application Shutdown Lifecycle & Unsaved Edits Interception

When the user attempts to close the application (clicking the `X` button, pressing `Alt+F4`, or calling `Application.Current.Shutdown()`):

```
[ User Triggers Window Close (WM_CLOSE / Window_Closing Event) ]
                                |
                                v
1. Evaluate In-Memory Dirty State across Active Project ViewModels
   - Does project.json or mashup-blueprint.json have unsaved edits?
     * YES: Cancel close event (e.Cancel = true);
            Render Modal Dialog: "Save changes to [Project Name] before closing?"
            Buttons: [Save & Exit] | [Discard Changes & Exit] | [Cancel]
     * NO: Proceed to step 2.
                                |
                                v
2. Evaluate Active Background Operations
   - Are any AI scaffolding tasks or mod compilers actively running?
     * YES: Cancel close event (e.Cancel = true);
            Render Modal Dialog: "Active task in progress: [Task Name].
            Closing now will terminate the task and revert staging changes."
            Buttons: [Cancel Task & Exit] | [Keep Working]
     * NO: Proceed to step 3.
                                |
                                v
3. Save Current Window Geometry to appsettings.json
                                |
                                v
4. Orderly Teardown of IPC Channels and Subprocesses
   - Send session.terminate.request to active adapter workers (5s grace).
   - Close Named Pipe handles.
   - Release Win32 Job Object handles.
                                |
                                v
5. Final Process Exit (ExitCode = 0)
```

### Safety Guarantee:
The application **CANNOT BE ACCIDENTALLY CLOSED** while in-flight operations or unsaved blueprint edits exist. All exits require explicit user confirmation.

