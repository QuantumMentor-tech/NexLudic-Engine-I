# PROJECT CREATION FIELD AND VALIDATION CATALOG

**Document Identifier:** `NLE-CAT-WIZ-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Catalog Specification  
**Classification:** Field-Level Specification & Validation Rules  
**Parent Document:** `NLE-SPC-WIZ-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))  

---

## 1. Catalog Architecture & Field Taxonomy

Every field within the Project Creation Wizard is mapped to a specific domain entity, persistence location, and provenance tracking tier in accordance with `NLE-DOM-SPEC-001` and `NLE-ARC-DOC-004`.

### 1.1 Field State Indicators
- **Required (R):** Must satisfy validation constraints before proceeding to subsequent stages.
- **Optional (O):** May remain blank; defaults or unassigned values apply.
- **Conditional (C):** Required only if specific dependent fields are active.

---

## 2. Comprehensive Field Catalog

### Stage 1: Project Basics

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-PRJ-001` | Project Display Name | String (Text) | R | 3-64 chars; valid Windows filename chars (no `<>:"/\|?*`) | "Project name must be 3-64 characters and contain only valid folder characters." | `project.json` -> `name` | `USER_ENTERED` |
| `FLD-PRJ-002` | Project Slug / ID | String (Slug) | R | Auto-generated from name; kebab-case; unique in workspace dir | "Project slug must be unique and contain only lowercase alphanumeric characters and hyphens." | `project.json` -> `id` | `SYSTEM_DERIVED` |
| `FLD-PRJ-003` | Target Directory | FilePath | R | Absolute path; MUST reside on Volume `D:\` (`DEC-009`); writeable | "Project workspace root must reside on drive D: and be writeable by current user." | `project.json` -> `workspace_root` | `USER_CONFIRMED` |
| `FLD-PRJ-004` | Description | String (Multiline) | O | Max 1000 characters; Unicode text | "Description exceeds maximum length of 1000 characters." | `project.json` -> `description` | `USER_ENTERED` |
| `FLD-PRJ-005` | Creator / Team | String (Text) | O | Max 100 characters; defaults to Windows user profile name | "Author name exceeds 100 characters." | `project.json` -> `author` | `USER_ENTERED` |

---

### Stage 2: Game-Pair Selection

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-GPS-001` | Game A (Host/Primary) | CatalogItem / Manual | R | Valid Game ID; must exist in registry or be manually defined | "Please select or define Primary Game A." | `project.json` -> `game_a.id` | `USER_SELECTED` |
| `FLD-GPS-002` | Game A Role | Enum | R | `PRIMARY_HOST` (default), `EQUAL_PEER`, `CLIENT_BRIDGE` | "Game A must have an assigned role in the mashup." | `project.json` -> `game_a.role` | `SYSTEM_DEFAULT` |
| `FLD-GPS-003` | Game B (Secondary/Donor)| CatalogItem / Manual | R | Valid Game ID; `game_b.id != game_a.id` | "Game B cannot be identical to Game A. A mashup requires two distinct games." | `project.json` -> `game_b.id` | `USER_SELECTED` |
| `FLD-GPS-004` | Game B Role | Enum | R | `DONOR_ASSET`, `SECONDARY_HOST`, `LIVE_SOURCE` | "Game B must have an assigned role in the mashup." | `project.json` -> `game_b.role` | `SYSTEM_DEFAULT` |
| `FLD-GPS-005` | Manual Game A Definition| Complex Object | C | Required if Game A selected as `MANUAL_ENTRY` | "Title and executable name are required for manual game definition." | `project.json` -> `game_a.custom` | `USER_DECLARED` |
| `FLD-GPS-006` | Manual Game B Definition| Complex Object | C | Required if Game B selected as `MANUAL_ENTRY` | "Title and executable name are required for manual game definition." | `project.json` -> `game_b.custom` | `USER_DECLARED` |

---

### Stage 3: Version & Environment Identification

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-ENV-001` | Game A Distribution | Enum | R | `STEAM`, `GOG`, `EPIC`, `STANDALONE`, `DISC`, `UNKNOWN` | "Select the distribution platform for Game A." | `game_a.env.platform` | `USER_DECLARED` |
| `FLD-ENV-002` | Game A Executable Path | FilePath | O | Valid absolute path on disk to .exe; read permission | "Path not found on disk. You may continue with unverified path." (Warning) | `game_a.env.path` | `DECLARED_PATH` |
| `FLD-ENV-003` | Game A Declared Version | String (Text) | R | SemVer, release string, or `UNKNOWN` (default: auto-fill from registry) | "Version string required. Use 'UNKNOWN' if exact build is not verified." | `game_a.env.version` | `DECLARED_VERSION`|
| `FLD-ENV-004` | Game A Anti-Cheat Flag | Boolean | R | If `TRUE`, triggers HARD BLOCK for online anti-cheat titles (`REQ-SAFE-001`) | "Projects targeting titles with active kernel/online anti-cheat are prohibited." | `game_a.env.anticheat` | `SAFETY_FLAG` |
| `FLD-ENV-005` | Game B Distribution | Enum | R | `STEAM`, `GOG`, `EPIC`, `STANDALONE`, `DISC`, `UNKNOWN` | "Select the distribution platform for Game B." | `game_b.env.platform` | `USER_DECLARED` |
| `FLD-ENV-006` | Game B Executable Path | FilePath | O | Valid absolute path on disk to .exe; read permission | "Path not found on disk. You may continue with unverified path." (Warning) | `game_b.env.path` | `DECLARED_PATH` |
| `FLD-ENV-007` | Game B Declared Version | String (Text) | R | SemVer, release string, or `UNKNOWN` | "Version string required. Use 'UNKNOWN' if exact build is not verified." | `game_b.env.version` | `DECLARED_VERSION`|
| `FLD-ENV-008` | Game B Anti-Cheat Flag | Boolean | R | If `TRUE`, triggers HARD BLOCK for online anti-cheat titles (`REQ-SAFE-001`) | "Projects targeting titles with active kernel/online anti-cheat are prohibited." | `game_b.env.anticheat` | `SAFETY_FLAG` |

---

### Stage 4: Mashup Goal & Thesis

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-GOL-001` | Raw Creator Goal / Vision | String (Multiline) | R | 10 to 4000 characters; verbatim creator expression | "Please provide at least a brief sentence (10+ characters) describing your mashup idea." | `project.json` -> `thesis.raw_intent` | `USER_ORIGINAL_TEXT` |
| `FLD-GOL-002` | Mashup Archetype | Enum | R | `ASSET_SWAP`, `MECHANIC_BRIDGE`, `CROSS_GAME_TRIGGER`, `SHARED_WORLD`, `TOTAL_CONVERSION` | "Select a primary mashup archetype to guide initial architecture." | `project.json` -> `thesis.archetype` | `USER_SELECTED` |
| `FLD-GOL-003` | Target Experience Description | String (Multiline) | O | Max 2000 characters; gameplay interaction loop | "Experience description exceeds 2000 characters." | `project.json` -> `thesis.gameplay_loop` | `USER_ENTERED` |
| `FLD-GOL-004` | AI Refinement Consent | Boolean | R | Default `TRUE`; allows local AI to extract formal capability tags from raw text | "Specify whether engine assistance may analyze your raw description." | `project.json` -> `thesis.ai_analysis_permitted` | `USER_CHOICE` |

---

### Stage 5: Capability Requirements

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-CAP-001` | Required Telemetry / Reading| MultiSelect Enum | O | `PLAYER_POSITION`, `HEALTH_STATS`, `INVENTORY`, `GAME_EVENTS`, `CAMERA_MATRIX` | Multi-select list | `requirements.json` -> `reading` | `DECLARED_REQUIREMENT` |
| `FLD-CAP-002` | Required Control / Injection| MultiSelect Enum | O | `INPUT_SIMULATION`, `SPAWN_ACTOR`, `MODIFY_STATS`, `TRIGGER_EVENT`, `UI_OVERLAY` | Multi-select list | `requirements.json` -> `injection` | `DECLARED_REQUIREMENT` |
| `FLD-CAP-003` | Asset Pipeline Needs | MultiSelect Enum | O | `STATIC_MESH_EXTRACT`, `TEXTURE_CONVERT`, `AUDIO_TRANSCODE`, `ANIMATION_PORT` | Multi-select list | `requirements.json` -> `assets` | `DECLARED_REQUIREMENT` |
| `FLD-CAP-004` | Synchronization Model | Enum | R | `REALTIME_TICK_SYNC`, `ASYNCHRONOUS_EVENTS`, `STATIC_PRECOMPILED_MOD`, `UNSPECIFIED` | "Select expected runtime synchronization model." | `requirements.json` -> `sync_model` | `USER_SELECTED` |
| `FLD-CAP-005` | Custom Requirements Note | String (Multiline) | O | Max 2000 characters | "Custom requirements note exceeds 2000 characters." | `requirements.json` -> `custom_notes` | `USER_ENTERED` |

---

### Stage 6: Integration Strategy Preferences

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-STR-001` | Candidate Strategy | Enum | R | `STRATEGY_A_HOST_MOD`, `STRATEGY_B_LIVE_BRIDGE`, `STRATEGY_C_HYBRID`, `UNDECIDED_EVALUATE` | "Please choose a candidate strategy or select 'Undecided / Evaluate All'." | `strategy.json` -> `candidate_strategy` | `USER_PREFERENCE` |
| `FLD-STR-002` | Complexity Appetite | Enum | R | `LOW_EFFORT_FAVORED`, `BALANCED`, `MAXIMUM_FIDELITY_AT_ANY_COST` | "Select your preferred development complexity trade-off." | `strategy.json` -> `complexity_preference` | `USER_PREFERENCE` |
| `FLD-STR-003` | Mod Loader Preference (Game A)| String (Text) | O | Max 50 characters (e.g. "SKSE", "MelonLoader", "Native DLL", "None") | "Mod loader string exceeds 50 characters." | `strategy.json` -> `game_a_modloader` | `USER_ENTERED` |
| `FLD-STR-004` | Strategy Disclaimer Ack | Boolean | R | MUST be `TRUE`. Acknowledges that candidate strategy is subject to Phase 5 feasibility | "You must acknowledge that candidate strategy is a hypothesis, not a guarantee." | `strategy.json` -> `hypothesis_acknowledged` | `USER_ACK` |

---

### Stage 7: Review & Pre-Flight Confirmation

| Field ID | Field Label | Data Type | Req | Validation Rules & Constraints | Error / Warning Message | Persistence Target | Provenance |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FLD-REV-001` | Pre-Flight Validation Check | System Audit | R | All Stages 1-6 valid; no active error flags; disk space > 10GB | "Pre-flight audit failed. Please review highlighted issues before creation." | System Memory | `SYSTEM_AUDITED` |
| `FLD-REV-002` | Single-Player Only Ack | Boolean | R | MUST be `TRUE`. User confirms intent for offline/single-player modding only | "NexLudic Engine strictly requires confirmation of offline single-player use." | `project.json` -> `compliance.sp_ack` | `USER_SAFETY_ACK` |
| `FLD-REV-003` | Open Workspace on Creation | Boolean | R | Default `TRUE`. Controls immediate navigation to Project Dashboard | None | UI Session | `USER_CHOICE` |

