# PROJECT CREATION SCENARIO AND REGRESSION TEST PLAN

**Document Identifier:** `NLE-PLN-WIZ-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Verification Matrix, Test Scenarios & Implementation Rules  
**Parent Document:** `NLE-SPC-WIZ-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))  

---

## 1. Ten-Level Verification Architecture

The Project Creation Wizard verification strategy spans ten distinct evaluation tiers:

```
[Level 1: Field Unit Validation]        -> Regex, length, character set constraints
[Level 2: Stage State Transitions]      -> Forward/backward navigation guards
[Level 3: Inter-Field Dependencies]     -> Custom game conditional fields, auto-slug derivation
[Level 4: Game-Pair Conflict Logic]     -> Identity deduplication (Game A != Game B)
[Level 5: Safety & Anti-Cheat Guards]   -> Kernel anti-cheat rejection (REQ-SAFE-001)
[Level 6: Storage & Path Compliance]    -> Volume D: constraint, path traversal blocking
[Level 7: Intent & Provenance Integrity] -> Raw user wording preservation, epistemic tiers
[Level 8: Atomic Persistence Scaffold]  -> Complete directory tree and project.json generation
[Level 9: UI Accessibility & Keyboard]  -> Full keyboard trap, live alerts, contrast, scaling
[Level 10: Post-Creation Shell Handoff] -> Project context switch and dashboard navigation
```

---

## 2. Comprehensive Test Scenarios (Scenarios A through X)

| Scenario ID | Test Scenario Description | Expected Trigger & Execution | Expected Outcome / Assertions |
| :--- | :--- | :--- | :--- |
| **SCN-A** | Standard Happy Path (Skyrim SE + DOOM Eternal) | Enter valid basics, select 2 catalog games, enter goal, pick strategy, confirm | Project created in `D:\NexLudic\Projects\<slug>`, context active, dashboard opened |
| **SCN-B** | Duplicate Game Selection | Select Skyrim SE for Game A, then Skyrim SE for Game B | Immediate error banner; "Continue" disabled; border highlighted |
| **SCN-C** | Custom Game A Definition | Select "Custom Game", fill title, exe, and engine | Custom game object hydrated; custom ID generated; validation passes |
| **SCN-D** | Custom Game B Definition | Select "Custom Game" for Game B with valid metadata | Both games valid; custom ID recorded in manifest |
| **SCN-E** | Kernel Anti-Cheat Hard Block | Select game with anti-cheat flag (e.g. Valorant / Destiny 2) | Non-dismissible hard block; cannot advance past Stage 3 |
| **SCN-F** | Illegal Project Slug Traversal | Input `../../Windows` or reserved name `CON` | Slug validator rejects with error; Next button disabled |
| **SCN-G** | Non-Volume D Storage Path | Attempt to set workspace root to `C:\MyProjects` | Path validator flags violation of `DEC-009`; rejects path |
| **SCN-H** | Insufficient Disk Space (< 10 GB) | Mock disk space check returning 4 GB free on `D:` | Pre-flight check fails on Stage 7; creation blocked |
| **SCN-I** | Existing Target Folder Collision | Set name to existing folder slug without suffix | Slug collision detected; warning displayed; suggests incremented slug |
| **SCN-J** | Empty / Short Mashup Goal (< 10 chars) | Input "mod" (3 chars) in Stage 4 | Minimum length validator fails; inline error shown; cannot advance |
| **SCN-K** | Extremely Long Mashup Goal (> 4000 chars)| Paste 5000 character description | Character count counter turns red; excess truncated or blocked |
| **SCN-L** | Exploratory Mode (No Capabilities Selected)| Select zero capability chips; check "I'm not sure yet" | Allowed to proceed; project marked `EXPLORATORY`; no false errors |
| **SCN-M** | Back Navigation Data Preservation | Enter data through Stage 5, navigate back to Stage 1, forward | All entered data intact across all stages; zero state loss |
| **SCN-N** | Game Invalidation Cascade | Change Game A on Stage 2 after configuring Stage 3 | Stage 3 re-initialized; Stage 5 shows review warning; pre-flight reset |
| **SCN-O** | Cancel Wizard with Unsaved Edits | Click Cancel on Stage 4 after typing description | Modal confirmation: "Discard unsaved changes?"; Yes closes wizard |
| **SCN-P** | Double-Click Create Button | Rapidly double-click "Create Project" on Stage 7 | Concurrency guard disables button immediately; single creation task |
| **SCN-Q** | Disk I/O Write Failure During Creation | Simulate write-protected disk during directory scaffold | Atomic rollback; staging cleaned up; error message displayed with retry |
| **SCN-R** | Full Keyboard Navigation Flow | Complete Stages 1-7 using ONLY Tab, Shift-Tab, Enter, Arrows | Focus visible at all times; all controls accessible; form submitted |
| **SCN-S** | Screen Reader Live Alert Announcement | Trigger validation error via keyboard | Live region announces validation failure text; focus set to field |
| **SCN-T** | High Contrast Mode Theme Switch | Toggle Windows High Contrast Black mode | All borders, text, and selection cards remain legible (> 4.5:1 ratio) |
| **SCN-U** | Minimum Resolution Scaling (1024x700) | Resize shell to 1024x700 at 125% DPI | ScrollViewer activates; navigation buttons remain visible and clickable |
| **SCN-V** | Strategy Disclaimer Rejection | Uncheck strategy disclaimer checkbox on Stage 6 | Next button disabled until user acknowledges candidate strategy disclaimer |
| **SCN-W** | Single-Player Offline Ack Rejection | Leave single-player compliance box unchecked on Stage 7 | "Create Project" button disabled; cannot finalize project |
| **SCN-X** | Post-Creation Navigation & Context Check | Complete creation successfully | Router navigates to Project Dashboard; Active Project updated in Shell |

---

## 3. Mandatory AI Coding Agent Implementation Rules

When implementing the Project Creation Wizard in subsequent development phases, AI coding agents MUST strictly observe the following engineering rules:

1. **Zero Raw Token Injection:** Never capture, store, or display plain-text API tokens or credentials in wizard viewmodels or JSON manifests.
2. **Strict Medium Integrity Execution:** Never request UAC elevation or run admin subprocesses during project creation.
3. **Volume D Strictness:** All project paths must be rooted in `D:\` unless an explicit user override overrides system defaults, with appropriate warnings.
4. **Epistemic Labeling:** Never label a project as "Compatible" or "Supported" upon wizard completion. The initial state must always be `Feasibility: Unevaluated`.
5. **Atomic Workspace Rollback:** If any step of directory creation or JSON writing fails, the entire newly created folder structure must be cleaned up to prevent corrupted workspace residue.

