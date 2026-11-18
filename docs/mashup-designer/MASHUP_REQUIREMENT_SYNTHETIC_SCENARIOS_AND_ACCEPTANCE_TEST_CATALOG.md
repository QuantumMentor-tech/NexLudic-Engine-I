# MASHUP REQUIREMENT SYNTHETIC SCENARIOS AND ACCEPTANCE TEST CATALOG
**Document ID:** `NLE-CAT-RST-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior QA Automation Engineer, Software Testing Architect, Systems Analyst, Requirements Specialist  
**Classification:** Test Catalog & Verification Scenarios  

---

## 1. PURPOSE AND TESTING GUIDELINES

### 1.1 Purpose
This catalog defines the authoritative set of **12 Formal Synthetic Acceptance Scenarios (Scenarios A through L)** evaluating the requirements analysis and feature decomposition subsystem (`NLE-SPC-MRD-001` through `NLE-REQ-RUX-001`). 

These scenarios serve as the definitive specification acceptance benchmark for future engineering teams implementing requirement parsers, decomposition engines, and UI editors.

### 1.2 Synthetic Data Invariant
- All scenarios use fictional, synthetic game titles (`game.synth.orion_rpg`, `game.synth.atlas_fps`, `game.synth.chronos_builder`).
- These scenarios constitute specification-level acceptance tests, NOT executed runtime game code.

---

## 2. THE TWELVE SYNTHETIC ACCEPTANCE SCENARIOS

---

### Scenario A: Clear, Bounded Objective
- **Initial State:** Project `Orion-Atlas` created with Game Orion (Host) and Game Atlas (Guest).
- **Input:** *"I want the Atlas Plasma Rifle to appear in Orion, firing blue plasma bolts that deal 50 energy damage."*
- **Decomposition:**
  - Feature: "Atlas Plasma Rifle Weapon"
  - Req A1: "Import Atlas Plasma Rifle 3D Mesh and Textures" (Category: `ASSET_TRANSFER`, Criticality: `MANDATORY`).
  - Req A2: "Implement Plasma Bolt Projectile firing 50 Energy Damage" (Category: `GAMEPLAY_MECHANIC`, Criticality: `MANDATORY`).
  - Criterion A2.1: Given player presses attack, When projectile hits target, Then target takes exactly $50 \pm 0$ damage.
- **Expected Status:** Transitions to `READY_FOR_ANALYSIS`.
- **System MUST NOT Infer:** Engine MUST NOT infer that Game Orion's graphics engine natively supports the mesh format before adapter validation.

---

### Scenario B: Ambiguous Objective
- **Initial State:** Empty project.
- **Input:** *"Make Game Orion and Game Atlas work together seamlessly."*
- **Applied Rules:** Policy `NLE-POL-MOI-001` (Section 4).
- **Observed Behavior:** Analysis flags Global Scope Ambiguity. Engine generates an `OpenQuestionRecord`: *"Which game should serve as the primary host world?"* and presents asset/mechanic checklists.
- **Expected Status:** Shifts to `NEEDS_CLARIFICATION`. Requirement cannot transition to `READY_FOR_ANALYSIS`.

---

### Scenario C: Multiple Mandatory Requirements
- **Initial State:** Project `Orion-Atlas` in planning.
- **Input:** *"Transfer the Atlas Cyber-Arm into Orion, grant +20 melee strength, and render a cybernetic HUD arm overlay."*
- **Applied Rules:** Splitting Rules `RUL-SPLIT-01` & `RUL-SPLIT-04`.
- **Decomposition:** Split into 3 atomic requirements:
  - Req C1: "Import Cyber-Arm 3D Asset" (`ASSET_TRANSFER`, `MANDATORY`).
  - Req C2: "Apply +20 Melee Strength Attribute Modifier" (`GAMEPLAY_MECHANIC`, `MANDATORY`).
  - Req C3: "Render Cybernetic HUD Overlay" (`UI_HUD`, `OPTIONAL`).
- **Traceability:** All 3 requirements link to `user_intent_excerpt` and parent Feature.

---

### Scenario D: Optional Enhancement Preserved as Optional
- **Initial State:** Creator defines "Atlas Jetpack" feature.
- **Input:** *"Player can fly with a jetpack (mandatory), and it should play retro 8-bit exhaust audio when active (optional)."*
- **Decomposition:**
  - Req D1: "Jetpack Flight Physics & Fuel Mechanic" (Criticality: `MANDATORY`).
  - Req D2: "Retro 8-Bit Exhaust Sound Playback" (Criticality: `OPTIONAL`).
- **Validation:** Gate `QG-04` and `QG-06` pass.
- **System MUST NOT Infer:** Engine MUST NOT elevate Req D2 to mandatory. If the audio adapter lacks spatial playback, the plan gracefully degrades while flight mechanics proceed.

---

### Scenario E: Conflicting Requirements Detection
- **Initial State:** Two requirements authored in project:
  - Req E1: "Player health is strictly hard-capped at 100 HP" (`MANDATORY`).
  - Req E2: "Atlas Nano-Shield grants +500 Overcharge HP exceeding normal caps" (`MANDATORY`).
- **Applied Rules:** `VR-REQ-10` (`ERR_REQ_MUTUAL_CONFLICT`).
- **Observed Behavior:** Conflict detector identifies logical contradiction. Both requirements tagged `CONFLICTED`. User presented with Conflict Resolution Tray (prompting choice of `ALTERNATIVE_TO` or condition modifier).
- **System MUST NOT Infer:** Engine MUST NOT silently discard one requirement.

---

### Scenario F: Unverified Technical Capability Assumption
- **Initial State:** Creator requests: *"When Orion player fires Atlas weapon, dynamically recompile Orion's internal shader pipeline."*
- **Applied Rules:** Gate `QG-08` (`NLE-SPC-ACV-001`).
- **Observed Behavior:** The engine detects that dynamic shader recompilation is an unverified native engine hook. It generates an `AssumptionRecord` (`UNVERIFIED_ASSUMPTION`): *"Assumes target game supports runtime shader injection."*
- **System MUST NOT Infer:** The engine MUST NOT treat the assumption as verified compatibility evidence.

---

### Scenario G: Missing Target-Side Representation
- **Initial State:** Creator requests transferring "Orion Magic Spell Mana" into Game Atlas.
- **Applied Rules:** Cross-Game Semantics (`NLE-SPC-CRS-001`, Section 4).
- **Observed Behavior:** Game Atlas is a sci-fi shooter lacking any "Mana" concept. In property mapping, Mana is flagged in `unresolved_mappings`. Analysis emits `OpenQuestionRecord`: *"Atlas has no Mana pool. Should Mana convert to 'Suit Energy' or be dropped?"*
- **Expected Status:** Stamped `NEEDS_CLARIFICATION`.

---

### Scenario H: Game Version Modification Invalidates Requirements
- **Initial State:** Project has 4 requirements in `READY_FOR_ANALYSIS` for Game Orion `v1.0.0`.
- **Trigger:** Creator updates Game Orion version constraint to `^2.0.0` (Major engine update).
- **Applied Rules:** Change Invalidation Matrix (`NLE-POL-RVC-001`, Section 4).
- **Observed Behavior:** All 4 requirements reset to `DRAFT`. Dependent capability matches flagged `STALE`. Re-validation required.

---

### Scenario I: Requirement Dependency Cycle (Kahn DAG Rejection)
- **Initial State:** Three requirements configured with circular dependencies:
  - Req I1 declares `REQUIRES` $\rightarrow$ Req I2.
  - Req I2 declares `REQUIRES` $\rightarrow$ Req I3.
  - Req I3 declares `REQUIRES` $\rightarrow$ Req I1.
- **Applied Rules:** Kahn Topological Sort (`NLE-POL-RDC-001`, Section 3).
- **Observed Behavior:** Cycle detected. Save rejected with `ERR_REQ_CIRCULAR_DEPENDENCY`. Diagnostic payload highlights exact cycle: $I1 \rightarrow I2 \rightarrow I3 \rightarrow I1$.

---

### Scenario J: Security Policy Conflict Rejection
- **Initial State:** User authors requirement: *"Inject kernel-mode hook into Game Atlas memory to bypass frame rate limits."*
- **Applied Rules:** Quality Gate `QG-10` (`NLE-SPC-ACV-001`), `DEC-002`, `BLK-SEC-01`.
- **Observed Behavior:** Analysis detects kernel-hook request violating platform Medium Integrity Level. Requirement is immediately flagged `BLOCKED` with diagnostic `CRIT_SEC_ANTI_CHEAT_BLOCKED`.
- **System MUST NOT Infer:** The requirement CANNOT grant permissions or override security rules.

---

### Scenario K: Incomplete Project Draft Preservation
- **Initial State:** Creator authors a detailed creative thesis and drafts 2 features, but has not yet assigned participant roles or written acceptance criteria.
- **Trigger:** Creator clicks "Save Draft".
- **Observed Behavior:** Project saves cleanly as `PROJECT_INCOMPLETE`. Requirements saved with status `INCOMPLETE`. No user data is lost. Amber badge indicates incomplete criteria.

---

### Scenario L: Requirement Refinement and Re-Validation
- **Initial State:** Req L1 has status `NEEDS_CLARIFICATION` due to vague statement: *"Make weapon feel punchy."*
- **Trigger:** Creator refines statement: *"Apply 15% camera shake and 0.2s controller rumble upon weapon discharge."* Defines numeric acceptance criteria.
- **Applied Rules:** Quality Gates `QG-01` through `QG-10` re-evaluated.
- **Observed Behavior:** Ambiguity cleared. All 10 gates pass. Quality status transitions from `NEEDS_CLARIFICATION` to `READY_FOR_ANALYSIS`.

---
*End of Catalog — Document `NLE-CAT-RST-001`.*
