# MASHUP GOAL AND INITIAL REQUIREMENTS UX SPECIFICATION

**Document Identifier:** `NLE-SPC-REQ-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Architectural Specification  
**Classification:** UX Interaction Specification & Requirements Modeling  
**Parent Document:** `NLE-SPC-WIZ-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))  

---

## 1. Capturing Creator Intent & Thesis (Stage 4)

### 1.1 The Primacy of Raw User Wording
The core philosophy of the NexLudic Engine is to preserve creator intent without distortion. 
- In Stage 4, the user provides a free-form description of their mashup vision in `FLD-GOL-001`.
- **Immutability Contract:** The engine preserves `thesis.raw_intent` exactly as typed by the user, byte-for-byte.
- **AI Processing Guardrail:** Even if AI analysis generates tags, summaries, or candidate architectures, the raw user text is never overwritten, replaced, or summarized away. Both exist side-by-side in `project.json`:
  ```json
  "thesis": {
    "raw_intent": "I want when Doomguy fires the BFG in Doom Eternal, a lightning storm strikes in Skyrim and spawns 10 dragons.",
    "ai_summary": "Cross-game weapon trigger mapping BFG fire event in Game B to atmospheric weather change and actor spawning in Game A.",
    "archetype": "CROSS_GAME_TRIGGER",
    "provenance": "USER_ENTERED"
  }
  ```

---

## 2. Preliminary Capability Requirements (Stage 5)

Stage 5 allows creators to specify what runtime capabilities their mashup will demand from both game runtimes.

### 2.1 Capability Category Taxonomy
Creators can select from structured requirement chips across four functional pillars:

```
+----------------------------------------------------------------------------------------------------+
| STAGE 5: PRELIMINARY CAPABILITY REQUIREMENTS                                                       |
+----------------------------------------------------------------------------------------------------+
|  [PILLAR 1: STATE READING]              [PILLAR 2: RUNTIME CONTROL]                                |
|  [x] Player Coordinates (3D Pos)        [x] Spawn Custom Actors / NPCs                             |
|  [x] Combat / Health Telemetry          [ ] Apply Damage / Status Effects                          |
|  [ ] Inventory & Item Tracking          [x] Fire In-Game Events / Triggers                         |
|  [ ] Game Pause / Time State            [ ] Modify Camera Orientation                              |
|                                                                                                    |
|  [PILLAR 3: ASSET PORTING]              [PILLAR 4: SYNCHRONIZATION]                                |
|  [x] Weapon 3D Meshes & Textures        (o) Asynchronous Event Messaging (Loose coupling)           |
|  [ ] Character Models & Rigs            ( ) Synchronous Frame-by-Frame Lockstep                     |
|  [x] Audio SFX & Voice Lines            ( ) Offline Pre-compiled Static Mod                        |
|                                                                                                    |
|  [ Quick Templates: [Weapon Mashup] [Cross-Game Telemetry] [World Port] [Clear All] ]              |
+----------------------------------------------------------------------------------------------------+
```

### 2.2 Intent vs. Feasibility Distinction
A declared capability requirement represents a **need**, not an **affordance**.
- Selecting `Spawn Custom Actors / NPCs` creates an entry in `requirements.json` with status `DECLARED`.
- It does **not** assert that Game A or Game B actually supports actor spawning via an active mod hook.
- Feasibility remains `UNEVALUATED` until Phase 5 (Feasibility Analysis Engine).

---

## 3. Handling Ambiguity & Incomplete Requirements

Creators frequently do not know the exact technical mechanisms required at inception.
- **Exploratory Mode:** Users can click *"I'm not sure yet — Let NexLudic help me analyze"* which flags the project as `EXPLORATORY`.
- **Non-Blocking Progression:** Specifying zero capability chips does not block wizard progress; the engine assigns a default baseline (`UNSPECIFIED_EXPLORATORY`) and prompts the creator to flesh out requirements during Phase 4 and Phase 5.

