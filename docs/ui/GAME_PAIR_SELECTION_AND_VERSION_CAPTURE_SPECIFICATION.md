# GAME-PAIR SELECTION AND VERSION CAPTURE SPECIFICATION

**Document Identifier:** `NLE-SPC-GPS-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Architectural Specification  
**Classification:** UX Interaction Specification & Domain Contract Binding  
**Parent Document:** `NLE-SPC-WIZ-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))  

---

## 1. Interaction Model & Selection Architecture

A game mashup fundamentally requires exactly two participant games:
1. **Game A (Host / Primary):** The game environment that hosts the combined gameplay experience, runtime viewport, or primary world simulation.
2. **Game B (Donor / Secondary):** The game supplying assets, logic, telemetry, or acting as an external live simulation peer.

```
+----------------------------------------------------------------------------------------------------+
| STAGE 2: PARTICIPANT GAME PAIR SELECTION                                                           |
+----------------------------------------------------------------------------------------------------+
|  [ PRIMARY GAME A (Host) ]                             [ SECONDARY GAME B (Donor) ]               |
|  +-----------------------------------------------+     +-----------------------------------------------+
|  | Search Installed / Catalog...             [Q] |     | Search Installed / Catalog...             [Q] |
|  | +-------------------------------------------+ |     | +-------------------------------------------+ |
|  | | > The Elder Scrolls V: Skyrim SE [Modded] | |     | | > DOOM Eternal (2020)           [Steam]   | |
|  | |   Cyberpunk 2077                  [Steam] | |     | |   Subnautica                    [Epic]    | |
|  | |   The Witcher 3: Wild Hunt        [GOG]   | |     | |   Fallout 4                     [Steam]   | |
|  | |   [+ Define Custom / Unlisted Game...]    | |     | |   [+ Define Custom / Unlisted Game...]    | |
|  | +-------------------------------------------+ |     | +-------------------------------------------+ |
|  | Selected: Skyrim Special Edition (1.6.640)   |     | Selected: DOOM Eternal (Update 6.66)          |
|  | Role: [Primary World Host (Default)       v] |     | Role: [Asset & Mechanic Donor (Default)     v]|
|  +-----------------------------------------------+     +-----------------------------------------------+
|                                                                                                    |
|  [!] VALIDATION STATUS: Ready. Two distinct games selected.                                       |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Selection Modes: Catalog vs. Manual Entry

### 2.1 Catalog Search Mode
- **Source:** Local known-game registry (`known_games.json`) derived from pilot research (Steps 1–7) and local library detection (Steam AppManifest, GOG Galaxy DB, Epic Manifests).
- **Matching Rules:** Case-insensitive prefix and fuzzy search on game title, developer, and known executable names.
- **Selection Payload:** Automatically hydrates canonical `game_id`, official title, default engine type (e.g. Creation Engine, idTech 7, REDengine 4), and known modding frameworks.

### 2.2 Manual Entry Mode
When a user targets an unlisted, indie, legacy, or custom-compiled game:
- Provides a modal/drawer form requiring:
  - Game Title (e.g. "Custom Raytracing Demo")
  - Executable Name (e.g. "demo_x64.exe")
  - Suspected Game Engine (`UNITY`, `UNREAL_4`, `UNREAL_5`, `GODOT`, `PROPRIETARY_CPP`, `UNKNOWN`)
  - Target Architecture (`x64`, `x86`)
- Assigned a synthesized Game ID prefixed with `custom-` (e.g. `custom-raytracing-demo-a8f1`).

---

## 3. Anti-Duplicate Game Validation Rule

In Stage 2, selecting the same game for both Game A and Game B is strictly prohibited:
```
Condition: GameA.CanonicalId == GameB.CanonicalId
Trigger: OnSelectionChanged for either picker
Action:
  1. Display error banner: "Game A and Game B cannot be the same game. A mashup requires two distinct games."
  2. Disable the "Continue to Version & Environment" button.
  3. Mark Game B selection card with red error border.
```

---

## 4. Version & Environment Identification (Stage 3)

### 4.1 Provenance Tiers for Captured Versions
In accordance with `NLE-DOM-SPEC-001`, version records are assigned strict epistemic tiers:
1. **Tier 1 — `DECLARED` (User Input):** The user typed or selected a version string (e.g. "1.6.640") without binary file verification.
2. **Tier 2 — `DETECTED` (Filesystem Probe):** The wizard inspected local PE headers (`VS_FIXEDFILEINFO`) or store manifest files (`appmanifest_489830.acf`) to verify the exact build.
3. **Tier 3 — `UNKNOWN`:** The user selected "I don't know / Detect later". The engine logs this as unverified and schedules automatic detection for Phase 4.

### 4.2 Invalidation & Cascade Reset Rules
If the user navigates back to Stage 2 and modifies either Game A or Game B:
- **Dependent State Invalidation:**
  - Stage 3 version data for the changed game is cleared or re-initialized to catalog defaults.
  - Stage 5 capability selections are retained with a warning banner: *"Participant game was changed. Please review capability requirements in Stage 5."*
  - Pre-flight validation cache in Stage 7 is invalidated (`IsPreflightValid = false`).

