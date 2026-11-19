# NexLudic Engine: Plan Review Synthetic Scenarios & Acceptance Test Catalog

**Document ID:** `NLE-CAT-IPU-001`  
**Phase:** 6 of 12 (Mashup Designer & Integration Planning)  
**Step:** 50 of 100  
**Classification:** Quality Assurance & Synthetic UX Test Catalog  
**Status:** IMPLEMENTATION-READY SPECIFICATION  

---

## 1. Test Architecture & Synthetic Test Baseline

This catalog establishes 14 deterministic synthetic UX test scenarios (Scenarios A through N) designed to validate every interaction, layout, navigation path, status badge, error boundary, and accessibility behavior of the **Mashup Integration Plan Visualization & Review Experience**.

> **NOTICE: SYNTHETIC AND ILLUSTRATIVE SPECIFICATION**  
> All participating games, component names, and integration plans in these scenarios (e.g., `Game Orion`, `Game Atlas`) are fictional synthetic test fixtures. They do not represent real-world commercial games, implemented mod runtimes, or executed application code.

### 1.1 Canonical Reference Context
- **Game Orion (`gm-orion-v1.4`):** Custom C++ racing engine; exports high-speed physics telemetry.
- **Game Atlas (`gm-atlas-v2.0`):** Unreal Engine 5.3 open-world sandbox; protected by client-side anti-cheat integrity checks (`BLK-SEC-01` applies).

---

## 2. The 14 Synthetic UX Scenarios (Scenarios A through N)

### Scenario A: Complete Planning Specification (Golden Path Review)
- **User Goal:** Creator reviews a fully assembled, verified plan to verify readiness for project planning sign-off.
- **Preconditions:** Steps 43–49 completed. 100% of mandatory requirements covered. All 10 Quality Gates passed.
- **Authoritative Data:** `plan.json` in state `ACCEPTED_FOR_PLANNING`.
- **Expected View:** Overview displays `[✓] Accepted for Planning` in solid green badge. Metric cards show `9 / 9 (100%) Mandatory Covered`, `0 Blockers`, `6 / 6 Fresh Snapshots`.
- **Interaction:** User clicks `[Explore Architecture]` tab, inspects component `COMP-LIVE-IPC-BRIDGE`, and views verified evidence from Phase 5 SDK conformance suite.
- **Negative Invariant:** UI does **not** provide an "Execute Plan" button or imply that games have been launched.

---

### Scenario B: Mandatory Requirement is Uncovered
- **User Goal:** Architect investigates why an assembled plan cannot achieve `ACCEPTED_FOR_PLANNING`.
- **Preconditions:** Requirement `REQ-MAND-007` (Vehicle Damage Mirroring) has an architecture component and data mapping, but lacks a Step 48 workflow step.
- **Authoritative Data:** `coverage_ledger` shows Tier 3 missing for `REQ-MAND-007`. `GATE-PLN-08` evaluates to `FAILED`.
- **Expected View:** Overview displays `[✕] Plan Invalid (Mandatory Gap)`. A prominent red warning banner is pinned at the top: `[✕] Mandatory Requirement Uncovered: REQ-MAND-007`.
- **Interaction:** User clicks `[View Details]`, jumping to Requirements tab with `REQ-MAND-007` highlighted. Inspector shows Tier 3 Behavioral Coverage: `[✕] Missing Workflow Step`.
- **Negative Invariant:** UI does **not** allow the user to override or bypass mandatory requirement coverage.

---

### Scenario C: Strategy Remains Undecided
- **User Goal:** User opens the plan review before Step 45 strategy evaluation has been completed.
- **Preconditions:** Step 45 `strategy.json` has `selected_strategy == null`.
- **Authoritative Data:** Precondition `PRE-PLN-03` triggered.
- **Expected View:** Overview renders an Empty/Precondition state: `[○] No Strategy Selected`.
- **Interaction:** Action card directs user: `[ Navigate to Step 45 Strategy Evaluation ]`.
- **Negative Invariant:** UI does **not** guess or auto-select a default strategy.

---

### Scenario D: Incomplete Data Mapping (Fidelity Limitation)
- **User Goal:** Engineer audits precision limits of coordinate transformations.
- **Preconditions:** Mapping `MAP-COORDS-02` converts double-precision coordinates to single-precision with minor drift ($<0.005\text{ cm}$).
- **Authoritative Data:** `mappings.json` records fidelity classification as `APPROXIMATE`.
- **Expected View:** Data Mappings tab displays `MAP-COORDS-02` with an amber `[▲] APPROXIMATE` badge.
- **Interaction:** User clicks row; Inspector opens showing affine transformation math, drift bounds, and affected requirement `REQ-MAND-002`.
- **Negative Invariant:** UI does **not** label lossy or approximate transforms as `LOSSLESS`.

---

### Scenario E: Blocked Workflow (Provisional SDK Item)
- **User Goal:** Reviewer checks why workflow step `WF-STEP-004` is tagged as provisional.
- **Preconditions:** Orion adapter capability depends on Phase 5 conditional gate item `P5-C02`.
- **Authoritative Data:** Step 49 gate evaluation `GATE-PLN-04` emitted `WARN_PROVISIONAL_SDK_DEP`.
- **Expected View:** Plan state shows `[!] Accepted with Risks`. Workflow step displays an amber `[?] PROVISIONAL` badge.
- **Interaction:** Clicking the step opens Inspector displaying: *"Relies on uncertified SDK condition P5-C02. Plan cannot proceed to final compilation until condition clears."*
- **Negative Invariant:** UI does **not** hide the provisional dependency.

---

### Scenario F: Complex Component Dependency (Dense Graph)
- **User Goal:** Reviewer inspects a component with multiple incoming and outgoing dependencies.
- **Preconditions:** Component `COMP-PROTOCOL-TRANSLATOR` depends on 2 adapters, 1 shared memory buffer, and outputs to 2 distinct mod plugins.
- **Authoritative Data:** Step 49 Kahn DAG records 5 dependency edges connected to this node.
- **Expected View:** In Dependency Explorer, selecting the node highlights all 5 connected edges while dimming unrelated nodes by 75%.
- **Accessibility:** User toggles `[Accessible Table View]`, viewing a clean tabular list of all 5 incoming/outgoing dependencies with parent stage numbers.

---

### Scenario G: Stale Source Artifact (Invalidation Alert)
- **User Goal:** User opens a plan where an upstream requirement was edited externally.
- **Preconditions:** `requirements.json` was modified on disk; live hash != snapshot hash recorded in `plan.json`.
- **Authoritative Data:** Step 49 event `EVT-INV-02` triggered. Lifecycle state moved to `INVALIDATED`.
- **Expected View:** Prominent orange alert banner: `[⚡] Plan Stale: Upstream Requirements Modified on Disk!`. Downstream staging controls are locked.
- **Interaction:** User clicks `[ Re-assess and Re-assemble Plan Now ]`, triggering the 15-stage pipeline to produce revision `v1.1.0`.

---

### Scenario H: Missing Supporting Evidence
- **User Goal:** Auditor checks whether Game Orion's declared 120Hz telemetry export has been verified.
- **Preconditions:** Capability manifest claims 120Hz support, but no test execution report exists.
- **Authoritative Data:** Epistemic certainty tier is `DECLARED`, not `VERIFIED`.
- **Expected View:** Evidence panel renders a neutral grey badge: `[○] DECLARED (Vendor Manifest Claim)`.
- **Interaction:** Inspector states: *"This capability is claimed by the adapter manifest but has not been verified by live test execution."*
- **Negative Invariant:** UI does **not** display a green verified badge for unverified declarations.

---

### Scenario I: Conflicting Assessment Results (Anti-Cheat Veto)
- **User Goal:** Reviewer examines why a proposed host-hook component was rejected.
- **Preconditions:** Step 45 feasibility assessment identified EAC on Game Atlas (`BLK-SEC-01`). Blueprint mistakenly included a DLL hook.
- **Authoritative Data:** `GATE-PLN-03` failed with `ERR_ANTI_CHEAT_POLICY_VIOLATION`.
- **Expected View:** Architecture view renders component with a red padlock and veto banner: `Direct Hooking Prohibited by Anti-Cheat Policy`.
- **Interaction:** Inspector explains: *"Game Atlas is protected by anti-cheat. Direct memory injection violates platform policy. Must use non-invasive live bridge."*

---

### Scenario J: Partial Plan Data (Optional Items Omitted)
- **User Goal:** Creator reviews an early draft plan where cosmetic tire smoke was skipped.
- **Preconditions:** All 9 mandatory requirements covered; optional requirement `REQ-OPT-002` unmapped. Partial assembly permitted.
- **Authoritative Data:** Plan state is `ACCEPTED_WITH_RISKS` (or `ASSEMBLED_PARTIAL`).
- **Expected View:** Overview shows `[!] Accepted with Risks`. Metric card shows `3 / 4 Optional Covered (75%)`.
- **Interaction:** Clicking optional metric opens Requirements tab filtered by `priority == OPTIONAL & status == UNCOVERED`.

---

### Scenario K: User Opens an Issue (Traceability Traversal)
- **User Goal:** Creator investigates a blocking open decision from the overview to its source requirement.
- **Preconditions:** Decision `DEC-PLN-02` ("Coordinate Origin Anchor") is marked `BLOCKING_DOWNSTREAM`.
- **Authoritative Data:** Overview action card highlights `DEC-PLN-02`.
- **Interaction Path:** User clicks `[Review Decision Details]` in Overview $\rightarrow$ Opens Issues tab with `DEC-PLN-02` selected $\rightarrow$ Clicks linked requirement `[REQ-MAND-002]` $\rightarrow$ Opens Requirements tab with `REQ-MAND-002` selected $\rightarrow$ Clicks linked component `[COMP-BRIDGE]` $\rightarrow$ Contextual Inspector preserves full navigation history.

---

### Scenario L: Architecture Alternatives Comparison
- **User Goal:** Technical architect compares two proposed blueprint variations.
- **Preconditions:** Step 46 created Blueprint A (Named Pipes) and Blueprint B (Shared Memory). Blueprint A is currently active.
- **Authoritative Data:** `blueprint.json` records Candidate B as `ALTERNATIVE_DRAFT`.
- **Expected View:** Architecture view displays a switcher: `Active: Blueprint A (Selected) | Candidate: Blueprint B (Alternative)`.
- **Negative Invariant:** The alternative blueprint is clearly tagged `DRAFT ALTERNATIVE` and cannot be mistaken for the accepted plan.

---

### Scenario M: Security Blocker (Privilege Elevation Veto)
- **User Goal:** User attempts to review a blueprint component that declared Administrator requirement.
- **Preconditions:** Component `COMP-KERNEL-MONITOR` requested High Integrity Level.
- **Authoritative Data:** `GATE-PLN-03` failed with `ERR_PRIVILEGE_ELEVATION_PROHIBITED`.
- **Expected View:** Plan state shows `[✕] Plan Invalid`. Quality Gate tab highlights Gate 03 in red.
- **Interaction:** Inspector states: *"NexLudic Engine enforces Medium Integrity Level confinement (DEC-002). Administrative elevation is strictly prohibited."*

---

### Scenario N: Accessibility Alternative (Screen Reader Traversal)
- **User Goal:** Screen-reader user navigates a 4-stage dependency graph without a visual canvas.
- **Preconditions:** Windows Narrator or NVDA is active. User presses `Tab` to navigate.
- **Authoritative Data:** Step 49 Kahn execution stages $S_0..S_3$.
- **Expected View:** User activates `[Accessible Table View]` toggle.
- **Screen Reader Behavior:** Screen reader announces: *"Execution Stages Table, 4 stages, 6 items. Stage 0: Core and Adapters, 2 items. Item 1: Orion Universal Adapter, Stage 0, No prerequisites."* Arrow keys navigate cleanly across cells.
