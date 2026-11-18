# MASHUP PROJECT DOMAIN EXAMPLES AND ACCEPTANCE SCENARIOS
**Document ID:** `NLE-SCN-MPE-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior QA Engineer, Product Systems Designer, Technical Product Designer, Software Architect  
**Classification:** Scenarios & Acceptance Criteria  

---

## 1. PURPOSE AND SYNTHETIC DATA GUIDELINES

### 1.1 Purpose
This document provides 12 comprehensive, formal acceptance scenarios demonstrating the deterministic behavior of the **Mashup Project Domain Model & Creation Workflow** (`MashupProject`). 

These scenarios serve as the definitive benchmark for engineering teams implementing project serialization, workspace management, UI wizard controllers, and validation logic.

### 1.2 Synthetic Data Invariant
In accordance with platform testing standards:
- All scenarios utilize synthetic, fictional game titles (`game.synth.orion_rpg`, `game.synth.atlas_fps`, `game.synth.chronos_builder`) to avoid implying that specific commercial games have been verified or modified.
- All file paths use synthetic Volume D roots (`D:\NexLudic\Projects\synth-01\...`).
- These scenarios represent specification acceptance tests, NOT claims of live runtime game execution.

---

## 2. THE TWELVE ACCEPTANCE SCENARIOS

---

### Scenario 1: Project Creation Prior to Local Game Installation
- **Initial Condition:** Creator has zero local game installations on workstation.
- **Entities:** `MashupProject`, `ProjectParticipant` (Game Orion, Game Atlas).
- **Trigger / User Action:** User opens creation wizard, selects `game.synth.orion_rpg` and `game.synth.atlas_fps` from the Phase 4 catalog, enters core thesis, and saves.
- **Applied Rules:** `VR-ID-01..05`, `VR-PAR-01..03`.
- **Observed State:** Project successfully saved with `lifecycle_state = PROJECT_READY_FOR_ASSESSMENT`. `local_installation_ref` is `null`.
- **System MUST NOT Infer:** The engine MUST NOT infer that the games are installed, that compatibility is guaranteed, or that adapters are loaded.

---

### Scenario 2: Canonical Games Selected with Unconstrained Version (`*`)
- **Initial Condition:** User does not know the exact version of Game Atlas.
- **Entities:** `ProjectParticipant` with `version_constraint = "*"`.
- **Trigger:** User leaves version input blank.
- **Applied Rules:** `VR-PAR-08`.
- **Observed State:** Project saves successfully. Emits non-blocking advisory warning `WARN_PAR_UNCONSTRAINED_VERSION`.
- **System MUST NOT Infer:** Engine MUST NOT treat `*` as proof that every historic release of Game Atlas is compatible.

---

### Scenario 3: Mixed Mandatory and Optional Feature Requirements
- **Initial Condition:** Project has 2 participants and valid objective.
- **Entities:** `FeatureRequirement` items:
  - Req 1: "Super Dash Mechanic" (Category: `GAMEPLAY_MECHANIC`, Criticality: `MANDATORY`, Priority: `CRITICAL`).
  - Req 2: "Retro Minimap Radar" (Category: `UI_HUD`, Criticality: `OPTIONAL`, Priority: `LOW`).
- **Trigger:** User submits requirement list.
- **Applied Rules:** `VR-REQ-01..07`.
- **Observed State:** Requirements accepted. In later Phase 6 planning, if Req 2 cannot be matched to an adapter capability, the plan gracefully degrades; if Req 1 fails, the plan blocks.

---

### Scenario 4: Saving and Resuming an Incomplete Draft
- **Initial Condition:** Creator begins wizard, enters project name "Orion Survival", adds Game Orion as Host, but leaves Game B unselected.
- **Trigger:** Creator clicks "Save Draft & Close".
- **Applied Rules:** `VR-PAR-01` (Minimum participants violated).
- **Observed State:** Engine saves `project.json` with `lifecycle_state = PROJECT_INCOMPLETE`. Project card appears in dashboard tagged with amber "Incomplete" badge. User resumes later and adds Game Atlas, advancing state to `PROJECT_READY_FOR_ASSESSMENT`.

---

### Scenario 5: Candidate Adapter Becomes Quarantined
- **Initial Condition:** Project in `PROJECT_PLANNING_IN_PROGRESS` referencing candidate adapter `com.synth.atlas-hook`.
- **Event:** Adapter `com.synth.atlas-hook` trips Phase 5 crash circuit breaker and transitions to `QUARANTINED` in Step 37 registry.
- **Applied Rules:** `CONF-REG-008`, `CONF-LFC-011`.
- **Observed State:** Project flags candidate adapter as `UNAVAILABLE_QUARANTINED`. Integration plan marks the corresponding requirement unfulfilled. Project lifecycle shifts back to `PROJECT_PLANNING_IN_PROGRESS` with warning.

---

### Scenario 6: Game Version Modification Invalidates Compatibility Report
- **Initial Condition:** Project in `PROJECT_PLANNING_IN_PROGRESS` with approved compatibility report for Game Orion `v1.2.0`.
- **Trigger:** User edits Game Orion version constraint from `^1.2.0` to `^1.3.0` due to a game patch.
- **Applied Rules:** Change Invalidation Matrix (`NLE-POL-MPL-001`, Section 6).
- **Observed State:** Previous Compatibility Report stamped `STALE`. Capability match results marked `DIRTY`. Project lifecycle shifts to `PROJECT_READY_FOR_ASSESSMENT`. Revision number increments to `2`.

---

### Scenario 7: Integration Strategy Left Undecided
- **Initial Condition:** Creator desires a mashup but is unsure whether to use host modding or live memory bridging.
- **Entities:** `StrategySelection` with `strategy_type = STRATEGY_UNDECIDED`.
- **Trigger:** User selects "Decide Strategy Later (Evaluate Feasibility First)".
- **Applied Rules:** `VR-STR-01`.
- **Observed State:** Project commits cleanly. Step 32/38 compatibility evaluation evaluates all three strategies concurrently, presenting a comparison matrix to help the creator decide.

---

### Scenario 8: Feature Requirement Cannot Be Matched to Capabilities
- **Initial Condition:** Creator requests requirement "Real-Time Volumetric Fluid Simulation" (Criticality: `OPTIONAL`).
- **Event:** Phase 5 Step 38 Matcher evaluates registry; zero adapters declare fluid simulation capabilities.
- **Applied Rules:** Step 38 `NLE-CAT-MTR-001` (Outcome `MISSING_DEPENDENCY`).
- **Observed State:** Requirement marked `UNFULFILLED`. Because criticality is `OPTIONAL`, project advances to `PROJECT_PLANNING_IN_PROGRESS` with an advisory note explaining the feature was omitted.

---

### Scenario 9: Modifying a Mandatory Requirement After Plan Creation
- **Initial Condition:** Project has achieved `PROJECT_PLAN_AVAILABLE` (Revision 3).
- **Trigger:** Creator adds a new `MANDATORY` requirement: "Synchronize Boss HP bar".
- **Applied Rules:** Change Invalidation Matrix (`NLE-POL-MPL-001`).
- **Observed State:** Project plan is marked `STALE`. Lifecycle resets to `PROJECT_PLANNING_IN_PROGRESS`. Revision increments to `4`. Plan cannot be approved until the new mandatory requirement is wired.

---

### Scenario 10: Unknown or Malformed Game Identifier Rejected
- **Initial Condition:** User manually types an uncataloged game identifier: `game.unregistered.fakegame`.
- **Trigger:** User attempts to add participant.
- **Applied Rules:** `VR-PAR-03`.
- **Observed State:** Engine rejects input with `ERR_PAR_UNKNOWN_GAME_ID`. Wizard displays inline error: *"Game identifier not found in Canonical Game Catalog. Please select a registered game."*

---

### Scenario 11: Recovery from Interrupted Creation Session
- **Initial Condition:** Creator is drafting a 5-requirement project. At Stage 5, power loss abruptly terminates the PC.
- **Recovery:** Wizard autosave had committed valid entries to `D:\NexLudic\Projects\<id>\.draft.json`.
- **Trigger:** Creator restarts NexLudic Engine.
- **Observed State:** Application Shell detects `.draft.json`, displays "Recover Unsaved Mashup Project" banner. Creator clicks "Resume", all 5 requirements restore into the form without data loss.

---

### Scenario 12: Structurally Valid Project Ready for Assessment
- **Initial Condition:** Creator completes all 7 stages of creation wizard.
- **Entities:** Project Orion-Atlas, 2 valid participants, valid thesis, 3 requirements, strategy set to `STRATEGY_2_LIVE_BRIDGE`.
- **Trigger:** Creator clicks "Create & Evaluate".
- **Applied Rules:** Pre-flight checks pass (0 errors, 0 warnings).
- **Observed State:** `project.json` written atomically. Revision 1 created. Lifecycle transitions to `PROJECT_READY_FOR_ASSESSMENT`. Phase 4/5 automated compatibility query dispatched.

---
*End of Scenarios — Document `NLE-SCN-MPE-001`.*
