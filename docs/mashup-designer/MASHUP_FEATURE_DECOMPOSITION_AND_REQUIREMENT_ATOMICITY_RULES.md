# MASHUP FEATURE DECOMPOSITION AND REQUIREMENT ATOMICITY RULES
**Document ID:** `NLE-RUL-MFA-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Principal Software Architect, Requirements Engineering Specialist, Systems Analyst, Senior QA Lead  
**Classification:** Core Requirements Engineering Rules  

---

## 1. PURPOSE AND DECOMPOSITION GOALS

### 1.1 Purpose
This specification establishes the authoritative rules, atomicity criteria, granularity thresholds, and completeness checklists governing the **Decomposition of Features into Atomic Requirements** (`FeatureRequirement`) within NexLudic Engine.

High-level game mashup features (e.g. *"Full Doom Weapon Pack in Minecraft"*) combine dozens of distinct subsystems—3D geometry import, raycast physics, particle rendering, inventory ammo management, sound playback, and error recovery. Attempting to implement or test an aggregated feature as a monolithic block leads to verification failure, hidden dependencies, and untraceable regressions. This document defines the exact rules for splitting compound features into independently testable, atomic requirement units.

---

## 2. THE THREE DECOMPOSITION PRINCIPLES

Every decomposed requirement MUST satisfy three core principles:
1. **Functional Cohesion:** A derived `FeatureRequirement` MUST contribute directly to the intent of its parent `Feature`. Extraneous mechanics cannot be bundled into an unrelated feature.
2. **Singular Observable Behavior (Atomicity):** A requirement MUST describe exactly ONE observable interaction, state transformation, or constraint.
3. **Independent Verifiability:** Each requirement MUST be verifiable by an automated test double, telemetry observation, or manual check without requiring the entire rest of the mashup to be fully implemented.

---

## 3. ATOMICITY RULES AND COMPOUND REQUIREMENT SPLITTING

### 3.1 Compound Anti-Patterns
The requirements analyzer scans requirement statements for compound anti-patterns:
- **Anti-Pattern A (Multi-System Bundle):** A single statement spanning multiple functional categories (e.g. *"Import 3D model, calculate raycast damage, play firing sound, and update ammo HUD"*).
- **Anti-Pattern B (Conjoined Failure Paths):** Bundling normal path execution with network failure or mod crash recovery in one statement.
- **Anti-Pattern C (Directional Ambiguity):** Failing to identify which game produces state and which game consumes it.

### 3.2 Formal Splitting Rules (`RUL-SPLIT`)

| Rule ID | Trigger Condition | Mandatory Decomposition Action | Resulting Structure |
| :--- | :--- | :--- | :--- |
| **`RUL-SPLIT-01`** | Statement spans $\ge 2$ functional categories (e.g. Asset + Physics). | Split into discrete requirements by category. | 1 Asset Requirement + 1 Combat Requirement. |
| **`RUL-SPLIT-02`** | Statement contains coordinating conjunctions ("AND", "AS WELL AS") linking independent verbs. | Split each clause into a child `FeatureRequirement`. | 2 independent child requirements linked by `REQUIRES`. |
| **`RUL-SPLIT-03`** | Requirement mixes normal gameplay with exception/recovery handling. | Separate normal behavior from error recovery. | 1 Normal Path Requirement + 1 Recovery Requirement. |
| **`RUL-SPLIT-04`** | Requirement specifies both visual/audio presentation and state logic. | Separate gameplay logic from presentation polish. | 1 Logic Requirement (Mandatory) + 1 FX Requirement (Optional). |

---

## 4. REQUIREMENT COMPLETENESS CHECKLIST (THE 6W CRITERIA)

To transition from `DRAFT` to `READY_FOR_ANALYSIS`, a `FeatureRequirement` MUST answer the six foundational engineering questions:

```
+--------------------------------------------------------------------------------------------------------+
|                                    REQUIREMENT COMPLETENESS CHECKLIST                                  |
|                                                                                                        |
|  1. WHO?    ---> Participating Games identified (source_participant_id, target_participant_id).        |
|  2. WHAT?   ---> Clear observable behavior stated using RFC 2119 imperative ("The system SHALL...").   |
|  3. WHEN?   ---> Exact trigger or initiating condition (e.g. "On player pressing Primary Fire").      |
|  4. WHERE?  ---> Execution boundary defined (e.g. "Inside Minecraft client render thread").            |
|  5. HOW?    ---> Measurable threshold defined in at least ONE AcceptanceCriterion.                     |
|  6. WHAT IF?--> Failure or missing capability behavior specified (Graceful degrade or hard block).    |
+--------------------------------------------------------------------------------------------------------+
```

---

## 5. SCOPE BOUNDARIES: CLASSIFICATION & NON-GOALS

To prevent runaway scope creep during mashup design, the engine classifies every proposed item into one of four scope buckets:

| Scope Bucket | Definition | Planning Treatment |
| :--- | :--- | :--- |
| **`IN_SCOPE_CORE`** | Essential features required to satisfy the `core_thesis`. | Formally mapped to Phase 5 capability requirements; blocks plan if missing. |
| **`IN_SCOPE_OPTIONAL`** | Desirable enhancements that improve polish or immersion. | Planned with graceful degradation; omitted if capabilities are unavailable. |
| **`DEFERRED`** | Features planned for subsequent iterations or updates. | Captured in project backlog; excluded from active integration plan. |
| **`EXPLICIT_NON_GOAL`** | Mechanics deliberately excluded to preserve game balance or safety. | Formally documented to prevent accidental re-introduction during code generation. |

---
*End of Rules — Document `NLE-RUL-MFA-001`.*
