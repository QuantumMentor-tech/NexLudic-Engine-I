# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Project Context & Switching UX Specification

**Document ID:** `NLE-SPC-CTX-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Specification  
**Classification:** Interaction Design & State Management Specification  
**Traceability:** `REQ-CORE-004` (Project Lifecycle), `NLE-LFC-DAT-001` (Project Lifecycle), `NLE-SPC-SHL-001` (Shell Specification)

---

## 1. Executive Summary & Purpose

NexLudic Engine is fundamentally **project-centric**. Users work within the context of a specific game-pair integration project.

The presentation shell must:
1. Clearly display the active project context at all times across all views.
2. Provide seamless, fail-safe transitions between projects.
3. Prevent **stale context contamination** where views accidentally display information from a previously opened project.
4. Guarantee that unsaved edits or active background tasks are never lost, corrupted, or silently discarded during a project switch.

---

## 2. Current-Project Visual Presentation

The active project context is permanently anchored in the central region of the shell window titlebar and replicated in the collapsed rail header:

```
+----------------------------------------------------------------------------------------------------+
| [Logo] NexLudic | Project: [Skyrim SE x Subnautica] [Strategy A] [v0.1.0] [Saved] | Ctrl+K | _ [] X|
+----------------------------------------------------------------------------------------------------+
```

### Context Indicators:
* **Project Name & Version:** Clear, high-contrast text (`Skyrim SE x Subnautica v0.1.0`).
* **Strategy Badge:** Colored tag distinguishing the architectural strategy:
  - `[Strategy A: Host Mod]` (Blue)
  - `[Strategy B: Live Bridge]` (Green)
  - `[Strategy C: Hybrid]` (Purple)
* **Save State Discriminator:**
  - `[Saved]`: Green dot; all manifests flushed to disk.
  - `[Unsaved Edits *]`: Amber dot; in-memory changes exist.
  - `[Saving...]`: Animated spinner; atomic write-rename in progress.

---

## 3. The "No Project Selected" Empty State

When the application launches without an active project, or when all projects are closed:
1. The titlebar displays: `Project: [No Project Selected]`.
2. Project-scoped navigation items (Compatibility, AI Tasks, Workspace) are dimmed or tagged with an `[Open Project Required]` indicator.
3. The main content viewport displays the **Welcome & Quick Start Hub**:
   - Primary Action: `[Create New Mashup Project]` (triggers wizard).
   - Secondary Action: `[Open Project from Volume D:]` (file picker).
   - Recent Projects List: Displays last 5 opened projects with game icons and last modified timestamps.

---

## 4. Deterministic Project Switching Workflow

Switching from Project A to Project B follows a strict 6-stage transaction workflow:

```
[ User Requests Project Switch (Click Recent Project or Open) ]
                             |
                             v
1. Inspect Unsaved In-Memory Edits (Dirty Flag Check)
   - If Project A has unsaved changes:
     * Display Modal: "Save changes to [Project A] before switching?"
     * Options: [Save & Switch] | [Discard Changes & Switch] | [Cancel]
     * If user clicks [Cancel]: Abort switch; remain in Project A.
                             |
                             v
2. Inspect Active Background Operations
   - If Project A has background tasks running (e.g. active Papyrus compile):
     * Display Modal: "[Project A] has 1 active task running. Background tasks
       cannot run while project is unloaded."
     * Options: [Wait for Completion] | [Cancel Task & Switch] | [Abort Switch]
                             |
                             v
3. Verify Target Project (Project B) Availability & Integrity
   - Inspect target directory on Volume D: (`project.json` exists, valid JSON).
   - If corrupt: Display error; offer restore from `.bak`; abort switch.
   - If Schema Version > Current Core: Warn user; force **Read-Only Mode**.
                             |
                             v
4. Purge Project A Context (Anti-Stale Protection)
   - Dispose active ViewModels for Project A.
   - Flush in-memory diff caches and temporary buffers.
   - Disconnect active adapter worker processes.
                             |
                             v
5. Load Project B Manifests & Initialize Context
   - Read `project.json`, `mashup-blueprint.json`, `artifacts-manifest.json`.
   - Initialize ViewModels bound strictly to Project B data.
                             |
                             v
6. Update Shell Presentation & Reset Navigation
   - Update titlebar context badge to [Project B].
   - Reset navigation route to Project Overview (`NAV-HOME-001`).
```

---

## 5. Stale Context Prevention Invariants

To eliminate cross-project data contamination:
1. **ViewModel Re-instantiation:** ViewModels are NEVER shared across projects. Switching projects completely tears down the active ViewModel scope and constructs fresh instances.
2. **Navigation Reset:** The navigation router resets to the default Overview destination on switch, preventing the user from viewing a sub-page that may not exist in the new project.
3. **IPC Channel Teardown:** All named pipe and stdio connections bound to Project A workers are terminated before Project B manifests are read.

