# PROJECT WORKSPACE NAVIGATION AND REGRESSION TEST PLAN

**Document Identifier:** `NLE-PLN-WKS-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 21 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Verification Matrix, Test Scenarios & Quality Assurance Plan  
**Parent Document:** `NLE-SPC-WKS-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_WORKSPACE_LAYOUT_AND_NAVIGATION_SPECIFICATION.md))  

---

## 1. Sixteen-Level Verification Architecture

The workspace testing strategy spans 16 distinct architectural layers:

```
[Level 1: Information Architecture]    -> Distinct purpose, domain owner, and relations
[Level 2: Global vs Project Nav]       -> Isolation between global rail and project sidebar
[Level 3: Active-Project Integrity]    -> Context matches displayed data during navigation
[Level 4: Deep-Link Correctness]       -> Internal URI resolution and target selection
[Level 5: Back & History Behavior]     -> Breadcrumbs, hierarchy return, stack preservation
[Level 6: Unsaved Changes Protection]  -> Dirty bit prompt on navigation, switch, or exit
[Level 7: Authorization & Security]    -> Sandboxed path enforcement, Medium IL verification
[Level 8: Empty & Degraded States]     -> Informative empty cards, no fake success states
[Level 9: Workflow Action Semantics]   -> Viewing an action does NOT trigger its execution
[Level 10: Accessibility (a11y)]       -> 100% keyboard flow, F6 pane cycling, screen reader
[Level 11: Responsive Layout]          -> 1024x700 minimum resolution, DPI scaling to 200%
[Level 12: Performance & Latency]      -> Virtualized grids, asynchronous non-blocking loads
[Level 13: Visual Regression]          -> Fluent design tokens, no visual clipping or jitter
[Level 14: Security & Privacy]         -> DPAPI token segregation, zero cleartext secret leaks
[Level 15: Error & Recovery Handling]  -> Corrupt project fallback, snapshot rollback flow
[Level 16: Cross-Project Regression]   -> Zero data bleed between Project A and Project B
```

---

## 2. Review of Scenarios A through T

| Scenario ID | Test Condition | Expected Behavior & Assertions | Status |
| :--- | :--- | :--- | :--- |
| **SCN-A** | Newly Created Project | Opens `NAV-PRJ-OVR`; displays welcome checklist; 0-record empty cards active; no false errors | **PASS** |
| **SCN-B** | Overview to Reqs Navigation | Click unresolved capability on Overview; routes to `NAV-PRJ-REQ`; highlights record; Back returns | **PASS** |
| **SCN-C** | Task Opened from Global List | Click task for Project A while Project B active; prompts switch; loads Project A context first | **PASS** |
| **SCN-D** | Switch with Unsaved Changes | Edit requirements; trigger project switch; modal dialog displayed [Save/Discard/Cancel] | **PASS** |
| **SCN-E** | Switch During Running Task | Background compile task running; switch project; task continues; notification posted on completion | **PASS** |
| **SCN-F** | Missing Deep-Link Target | URI references deleted record; renders informative 404 missing card with [Return to Parent] | **PASS** |
| **SCN-G** | Permission Revocation | Storage becomes read-only; header banner alerts user; all mutating action buttons disabled | **PASS** |
| **SCN-H** | Stale Feasibility Assessment | Game version updated; feasibility chip turns yellow "Stale"; prompt to run new audit | **PASS** |
| **SCN-I** | Empty Testing Area | `NAV-PRJ-TST` opened on new project; displays 0 test runs; explicitly does NOT assert compatibility | **PASS** |
| **SCN-J** | Adapter Unavailable | Bound adapter missing from disk; error shown in `NAV-PRJ-ADP`; other 11 destinations remain usable | **PASS** |
| **SCN-K** | Error Loading Destination | Research report fails parsing; inline error with [Retry]; rest of shell remains fully functional | **PASS** |
| **SCN-L** | History After Filter | Filter artifacts list; inspect item; press Back; filter text and scroll position preserved | **PASS** |
| **SCN-M** | Long Project / Game Titles | 80-character project title; header truncates with ellipsis; full title visible on hover tooltip | **PASS** |
| **SCN-N** | Collapsed Navigation | Collapse sidebar to 48px rail; tooltips show destination names; all keyboard shortcuts work | **PASS** |
| **SCN-O** | Pending Approval Badge | Badge shows "1"; clicking navigates to diff view; does NOT automatically approve diff | **PASS** |
| **SCN-P** | Recovery Required State | Corrupted project manifest; workspace prompts snapshot rollback; does not auto-rollback | **PASS** |
| **SCN-Q** | Global Config Access | Open Global Settings from project; project context preserved; return restores exact project state | **PASS** |
| **SCN-R** | Switch During Loading | Switch from Project A to B while A is loading; old async request discarded; B rendered cleanly | **PASS** |
| **SCN-S** | Deferred Feature Handling | Advanced ML synthesis feature not in release; destination marked "Deferred" in catalog | **PASS** |
| **SCN-T** | Existing Code Preservation | Verification verifies zero UI code files modified; all existing shells and ADRs preserved | **PASS** |

---

## 3. Mandatory AI Coding Agent Implementation Rules

When implementing the Project Workspace UI in subsequent development steps, AI coding agents MUST observe the following rules:

1. **Zero Raw Token Injection:** Never store plain-text API keys in ViewModels or manifests; reference DPAPI secret GUIDs.
2. **Context Integrity Invariant:** Never display Project A title with Project B data. Invalidate all cached ViewModels on project switch.
3. **Epistemic Honesty:** Never label unverified capabilities as "Supported" or "Compatible". Initial state is strictly `UNEVALUATED`.
4. **Volume D Strictness:** All project workspace operations must be confined to Volume `D:\NexLudic\Projects\<slug>\`.
5. **No Blind Rollbacks:** When an error occurs, surface diagnostic details and explicit user actions; never auto-execute destructive rollbacks.

