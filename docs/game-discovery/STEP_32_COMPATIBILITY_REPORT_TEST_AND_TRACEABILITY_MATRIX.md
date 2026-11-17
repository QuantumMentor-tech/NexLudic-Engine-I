# Step 32 Compatibility Report Test and Traceability Matrix

**Document ID:** `NLE-MAT-STP-032`  
**Version:** 1.0.0  
**Phase:** 4 of 12 — Game Discovery & Compatibility Intelligence  
**Step:** 32 of 100 — Game-Pair Compatibility Report & Recommendation Experience Specification  
**Classification:** Requirements Traceability Matrix & QA Test Plan  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-DOM-SPEC-001`, `REQ-SEC-001`, `REQ-SAFE-001`, `DEC-002`, `DEC-009`, `NLE-SPC-RUX-001` through `NLE-SPC-WSR-001`

---

## 1. Upstream Requirements Traceability Matrix

This matrix establishes complete, unbroken traceability between high-level architectural requirements, Phase 4 domain contracts, and the Step 32 report specifications.

| Requirement ID | Architectural Mandate | Step 32 Implementing Section | Verification Method | Traceability Status |
|:---|:---|:---|:---|:---|
| `REQ-SEC-001` | Windows Medium Integrity Level; no UAC elevation; path sanitization | `NLE-PLN-ASP-001` Section 2; `NLE-CAT-RIC-001` Section 4 | Static Specification Audit | FULLY TRACED |
| `REQ-SAFE-001` | Read-only inspection; zero process injection; zero binary execution | `NLE-SPC-RUX-001` Section 1.1; `NLE-SPC-SCR-001` Section 5 | Architectural Review | FULLY TRACED |
| `DEC-002` | Loose coupling between Universal Catalog and Game Adapters | `NLE-SPC-RUX-001` Section 1.1; `NLE-CAT-RIC-001` Region 3 | Domain Interface Verification | FULLY TRACED |
| `DEC-009` | Volume D storage isolation (`D:\NexLudic\Projects\<id>\`) | `NLE-CAT-RIC-001` Section 4; `NLE-PLN-ASP-001` Section 2 | Path Normalization Contract | FULLY TRACED |
| `STEP-25-CANON` | Canonical Game Identity and Edition separation | `NLE-CAT-RIC-001` Region 1 (`REP-IDN-03` to `08`) | Schema Verification | FULLY TRACED |
| `STEP-26-METAD` | External Metadata Provider Authority and Governance | `NLE-SPC-RIS-001` Section 4 (`State DS-01`) | Provider Degradation Review | FULLY TRACED |
| `STEP-27-LOCAL` | Local Installation Verification State Model (13 states) | `NLE-CAT-RIC-001` Region 1 (`REP-IDN-05`, `08`) | Lifecycle State Mapping | FULLY TRACED |
| `STEP-28-MAPPG` | Version-Environment Compatibility Mapping & Freshness | `NLE-GDE-SVL-001` Section 2.5; `NLE-SPC-RIS-001` Section 6 | Freshness Policy Audit | FULLY TRACED |
| `STEP-29-FRAME` | 8 Compatibility Dimensions & Anti-Aggregation Invariant | `NLE-CAT-RIC-001` Region 4; `NLE-GDE-SVL-001` Section 1.1 | Rule Hierarchy Review | FULLY TRACED |
| `STEP-30-EVIDN` | Cryptographic Evidence Records & Epistemic Tiers | `NLE-SPC-EDT-001` Section 2; `NLE-GDE-SVL-001` Section 2.4 | Cryptographic Schema Audit | FULLY TRACED |
| `STEP-31-RULES` | 10-Stage Pipeline, Kahn DAG, Explainable Trace & Diff | `NLE-SPC-EDT-001` Section 3; `NLE-CAT-RIC-001` Region 7 | Pipeline Contract Audit | FULLY TRACED |

---

## 2. Comprehensive QA Validation Test Plan (35 Test Cases)

The following test cases define the future implementation verification criteria for the Game-Pair Compatibility Report view (`NAV-PRJ-FEA`).

### 2.1 Group 1: Baseline Report Scenarios
- **TC-REP-BASE-01 (Happy Path - Viable Strategy A):** Report loads with verified installations; renders Strategy A as `STRATEGY_RECOMMENDED` with 4/5 requirements satisfied and Tier 2 evidence.
- **TC-REP-BASE-02 (Happy Path - Viable Strategy B):** Both games verified; Strategy B recommended for real-time telemetry with <2ms latency budget and 4.2GB RAM headroom.
- **TC-REP-BASE-03 (Happy Path - Viable Strategy C):** High-end dual execution environment satisfies hybrid requirements with explicit multi-runtime trade-off warning.
- **TC-REP-BASE-04 (Multi-Dimension Coverage Display):** Requirements grid renders across all 8 dimensions with accurate categorical icons and filter chips.
- **TC-REP-BASE-05 (Read-Only Execution Boundary):** Verifies that interacting with any report component executes zero system processes and modifies zero local files.

### 2.2 Group 2: Status Integrity & Anti-Deception Scenarios
- **TC-REP-STAT-01 (Unknown Is Not Unsupported):** Requirement lacking evidence displays `COVERAGE_UNRESOLVED_UNKNOWN` (grey), never `COVERAGE_UNSUPPORTED` (red).
- **TC-REP-STAT-02 (Anti-Aggregation Enforcement):** System strictly rejects generating a percentage score (e.g. 80%); displays `4 of 5 Requirements Addressed`.
- **TC-REP-STAT-03 (Mandatory Blocker Dominance):** When `BLK-SEC-01` is active, header verdict displays `PROHIBITED_BLOCKED` regardless of how many individual requirements are satisfied.
- **TC-REP-STAT-04 (No Forced Winner on Tie):** When Strategy A and B have identical suitability with equal trade-offs, UI displays `DUAL_VIABLE`, preventing an arbitrary winner.
- **TC-REP-STAT-05 (Conditional Support Labeling):** A `FEASIBLE_CONDITIONAL` verdict always renders its mandatory disclaimer tooltip: *"Requires Phase 5 adapter probing to confirm."*

### 2.3 Group 3: Evidence & Provenance Scenarios
- **TC-REP-EVD-01 (Bidirectional Traceability):** Clicking citation `[EVD-8021]` in Region 4 opens `EvidenceDrilldownDrawer` with exact metadata and SHA-256 hash.
- **TC-REP-EVD-02 (Cryptographic Integrity Verification):** If cached evidence JSON hash matches authoritative digest, UI renders green verified badge; if mismatch, renders amber warning.
- **TC-REP-EVD-03 (Epistemic Tier Distinctions):** Tier 1 (`REPRODUCED_BEHAVIOR`) renders with solid purple badge; Tier 4 (`INDIRECT_HEURISTIC`) renders with dashed grey badge.
- **TC-REP-EVD-04 (Evidence Conflict Surfacing):** If an evidence record has `ConflictStatus = DISPUTED_CONFIRMED`, drawer renders both conflicting claims with provenance citations.
- **TC-REP-EVD-05 (Missing Evidence Handling):** If an evidence record was deleted or corrupted, UI displays `ERR-REP-003` in drawer without breaking the main report grid.

### 2.4 Group 4: Interaction, Navigation & State Scenarios
- **TC-REP-INT-01 (Empty State - No Game Pair):** Unassigned project displays `State ES-01` with action routing to Project Settings (`NAV-PRJ-SET`).
- **TC-REP-INT-02 (Empty State - Unevaluated):** Newly paired games display `State ES-02` with `[Run Initial Feasibility Audit]` action.
- **TC-REP-INT-03 (Drawer Context Preservation):** Opening and closing the Evidence Drawer preserves exact scroll position and filter state on the main report.
- **TC-REP-INT-04 (Historical Snapshot Viewing):** Selecting Revision 2 renders `State HS-02` with sticky historical watermark and disabled commitment actions.
- **TC-REP-INT-05 (Safe Plan Commitment Transition):** Clicking `[Select for Plan]` opens modal `CMP-045`; confirming navigates to `NAV-PRJ-PLN` without modifying disk files.

### 2.5 Group 5: Accessibility Scenarios (WCAG 2.1 AA)
- **TC-REP-A11Y-01 (Non-Color Status Discrimination):** All 6 outcome badges, coverage icons, and freshness states are distinguishable in monochrome / grayscale mode.
- **TC-REP-A11Y-02 (Full Keyboard Navigability):** Entire report, strategy cards, filter chips, table rows, and drawer can be navigated using `Tab`, `Shift+Tab`, `Space`, `Enter`, and `Esc`.
- **TC-REP-A11Y-03 (Focus Restoration on Drawer Close):** Closing the Evidence Drawer with `Esc` restores keyboard focus to the exact triggering citation chip.
- **TC-REP-A11Y-04 (Screen-Reader Table Semantics):** Requirements grid announces row headers, dimension categories, and coverage states via proper ARIA roles.
- **TC-REP-A11Y-05 (Live Announcements on Background Reassessment):** Background completion triggers polite ARIA live announcement without interrupting active typing.

### 2.6 Group 6: Security, Privacy & Sandboxing Scenarios
- **TC-REP-SEC-01 (Filesystem Path Redaction):** User profile directories (`C:\Users\username\...`) are strictly redacted to `~` or `<UserHome>`.
- **TC-REP-SEC-02 (Untrusted Content Sandboxing):** Markdown evidence excerpts containing `<script>` or `<iframe>` tags are stripped of executable code.
- **TC-REP-SEC-03 (Safe External Link Confirmation):** Clicking third-party documentation URLs triggers confirmation modal `CMP-015` and opens with `rel="noopener noreferrer"`.
- **TC-REP-SEC-04 (No Elevation Prompt):** Running evaluation or viewing reports never triggers Windows UAC elevation prompts (remains Medium Integrity).
- **TC-REP-SEC-05 (Zero Memory Hooking):** Opening report or inspecting technical dependencies never invokes `OpenProcess` or memory read APIs on running games.

### 2.7 Group 7: Performance & Degradation Scenarios
- **TC-REP-PERF-01 (Virtualization under 100 Requirements):** Table with 100 requirements scrolls smoothly at 60 fps; DOM node count remains bounded under 40 nodes.
- **TC-REP-PERF-02 (Rapid Filter Response):** Switching filter chips (e.g. from All to Satisfied) completes within $< 16\text{ ms}$.
- **TC-REP-PERF-03 (Offline Metadata Provider Graceful Degradation):** When external metadata service is unreachable, report renders local cached data with non-modal `State DS-01`.
- **TC-REP-PERF-04 (Memory Footprint Compliance):** Report ViewModel and DOM trees consume $< 35\text{ MB}$ additional memory in the Electron/desktop process.
- **TC-REP-PERF-05 (Rule Timeout Recovery):** If rule execution exceeds 10 seconds, `ERR-REP-002` displays with safe retry option using core ruleset.

---

## 3. Non-Blocking Gap Register

| Gap ID | Identified Condition | Mitigating Architectural Pattern | Follow-Up Step |
|:---|:---|:---|:---|
| `GAP-STP32-01` | Phase 5 Adapter Qualification schemas (`CMP-043`) are pending implementation in Phase 5. | Report displays adapter requirements as `DOCUMENTED_PREREQUISITE` based on Step 28/31 contracts. | Step 34 (Phase 5 Adapter Architecture) |
| `GAP-STP32-02` | Live binary hash verification of installed game executables requires local disk read consent (`NLE-SPC-LID-001`). | Report UI handles unconsented installs gracefully by displaying `MANUALLY_DECLARED` badge. | Step 27 Discovery Integration |
| `GAP-STP32-03` | Real-time multi-window sync for Strategy B requires graphics adapter capability detection. | Report marks multi-window display as `CONDITIONAL_HARDWARE_CHECK` pending runtime benchmark. | Step 42 (Live Bridge Runtime) |

