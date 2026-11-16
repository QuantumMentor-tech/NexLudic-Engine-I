# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Global Status, Notifications & Approvals Specification

**Document ID:** `NLE-SPC-STS-001`  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 18 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Status Presentation & Human Approval Specification  
**Traceability:** `ADR-0009` (Command Approval Model), `NLE-STD-ERR-001` (Error Taxonomy), `NLE-SPC-SHL-001` (Shell Specification)

---

## 1. Executive Summary & Purpose

The **Global Status, Notifications, and Approvals Specification** establishes how NexLudic Engine communicates system health, background task execution progress, operational errors, and consequential approval requests.

Because AI coding agents and modding tools can perform high-impact operations (e.g. compiling scripts, deleting files, or modifying game staging directories), the shell guarantees:
1. **Unambiguous Consequential Gating:** Pending human approvals are permanently visible and cannot be bypassed.
2. **Authoritative Status Integrity:** Transient notifications never mask critical system errors or uncertain operation outcomes.
3. **Structured Notification Dismissal:** Dismissing a visual notification does NOT cancel an active task or waive a required security approval.

---

## 2. Notification Severity Taxonomy

All notifications and system alerts conform to the universal 5-level severity taxonomy:

```
+----------------------------------------------------------------------------------------------------+
|                                  NOTIFICATION SEVERITY TAXONOMY                                    |
+----+-------------------+-----------+--------------------+------------------------------------------+
| Tier| Severity Level   | Icon      | Visual Color       | Typical Trigger & Operational Meaning    |
+----+-------------------+-----------+--------------------+------------------------------------------+
| 01 | **INFORMATIONAL** | (i) Info  | Blue / Neutral     | Operation completed; snapshot saved.     |
| 02 | **WARNING**       | /!\ Warn  | Yellow             | High memory usage; non-blocking notice.  |
| 03 | **ACTION-REQUIRED**| [!] Alert | Amber / Orange     | **HUMAN APPROVAL REQUIRED** for diff.    |
| 04 | **ERROR**         | (X) Error | Bright Red         | Compilation failed; adapter disconnected.|
| 05 | **CRITICAL**      | [!!] Stop | High-Contrast Red  | Security sandbox violation; disk full.   |
+----+-------------------+-----------+--------------------+------------------------------------------+
```

---

## 3. Transient vs. Persistent Status Presentation

```
+----------------------------------------------------------------------------------------------------+
|                                    PRESENTATION CHANNELS                                           |
+--------------------------+-------------------------------------+-----------------------------------+
| Notification Channel     | Lifetime & Dismissal Behaviour      | Content Scope                     |
+--------------------------+-------------------------------------+-----------------------------------+
| **Transient Toast**      | Auto-dismisses after 4.0 seconds.   | Low-consequence acknowledgements  |
| (Bottom-Right Floating)  | Pauses timer on mouse hover.        | (e.g. "Manifest saved to disk").  |
+--------------------------+-------------------------------------+-----------------------------------+
| **Persistent Status Bar**| Permanent for process lifetime or   | Core health, memory usage, Volume |
| (Bottom Window Frame)    | until condition clears.             | D: free capacity, active task bar.|
+--------------------------+-------------------------------------+-----------------------------------+
| **Approval Banner / Badge**| Permanent until user clicks       | Mandatory diff approval queue.    |
| (Top Header & Inspector) | [Approve] or [Reject] in modal.     | Cannot be dismissed or hidden.    |
+--------------------------+-------------------------------------+-----------------------------------+
| **Notification Center**  | User-dismissable list with history. | Full audit log of all events;     |
| (Drawer Panel)           | Retains records for session.        | links to diagnostic records.      |
+--------------------------+-------------------------------------+-----------------------------------+
```

---

## 4. Pending Human-in-the-Loop Approvals UX (`ADR-0009`)

When an AI agent or modding script generates file modifications in `<project>/staging/`:
1. **Global Visual Cue:** A prominent Amber badge appears in the titlebar: `[!] Pending Approval: Papyrus Bridge Script`.
2. **Contextual Inspector Alert:** The Inspector drawer displays the file modification summary:
   - Modified: `scripts/Source/NexLudicBridge.psc` (+42 lines, -3 lines).
   - Created: `mod_manifest.json` (+18 lines).
3. **Unified Diff Review Modal:** Clicking the alert opens the **Consequential Diff Review Dialog**:
   - Side-by-side or unified git-style syntax highlighted diff.
   - Exact hash comparison (Pre-hash vs Post-hash).
   - Non-ambiguous action buttons:
     * `[Approve & Apply to Staging]` (Green primary button).
     * `[Reject & Revert Changes]` (Red button).
     * `[Edit Manually in Staging]` (Secondary button).
4. **Authoritative Gate:** The Core blocks deployment until an affirmative `AITaskApprovalRecord` is signed.

---

## 5. Active Operations & Cancellation UX

When long-running asynchronous tasks execute (e.g. compiling Papyrus scripts or unpacking game archives):
1. **Status Bar Progress Indicator:**
   - If progress is measurable: Renders a deterministic percentage bar `[=====>    ] 65%`.
   - If progress is indeterminate: Renders an indeterminate marquee sweep animation.
2. **Operation Flyout Drawer:**
   - Clicking the progress bar opens the active operation card:
     * Task Name: `Compiling Papyrus Scripts (3 of 8)`
     * Elapsed Time: `00:14 / ~00:25`
     * Active Process: `PapyrusCompiler.exe (Job Object Sandboxed)`
     * Action: `[Cancel Task]` button.
3. **Cooperative Cancellation Flow (`NLE-STD-ERR-001`):**
   - User clicks `[Cancel Task]`.
   - UI changes button to `[Cancelling...]` with spinner.
   - Core transmits `operation.cancel.request` to worker.
   - Worker ceases compilation and rolls back `.tmp` files.
   - Upon receipt of `operation.cancel.response`, task card clears with notification: *"Task cancelled by user. Staging workspace restored."*
   - If worker deadlocks for >5.0 seconds, Core watchdog triggers forceful termination via Win32 `TerminateJobObject`.

---

## 6. Error & Uncertain-Outcome Presentation

In strict accordance with `NLE-STD-ERR-001`:
* **Confirmed Failures:** Rendered with direct error code (e.g. `COMPILATION_SYNTAX_ERROR`) and jump link to line in file.
* **Uncertain External Outcomes:** If an operation produced partial disk mutations before crashing, the UI explicitly flags:
  > *"[!] Uncertain Outcome: External mod tool terminated abruptly. Some staging files may be incomplete. Recommended Action: [Rollback to Pre-Operation Snapshot]."*
  The engine NEVER masks uncertain state as a clean failure or silent success.

