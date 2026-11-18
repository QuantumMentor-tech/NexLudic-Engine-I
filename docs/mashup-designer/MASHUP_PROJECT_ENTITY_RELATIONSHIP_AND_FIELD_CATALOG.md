# MASHUP PROJECT ENTITY RELATIONSHIP AND FIELD CATALOG
**Document ID:** `NLE-CAT-MPD-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Domain Modeling Specialist, API Contract Designer, Principal Software Architect, Senior QA Engineer  
**Classification:** Core Data Model & Field Catalog  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This catalog provides the exhaustive, implementation-ready specification of every entity, attribute, relationship, data type, validation rule, and persistence constraint within the **Mashup Project Domain** (`MashupProject`). 

Engineering teams implementing project serialization, workspace storage on Volume D (`DEC-009`), or REST/IPC contracts MUST adhere strictly to the definitions, nullability constraints, and schemas defined in this document.

---

## 2. COMPLETE DOMAIN ENTITY CATALOG

```
+--------------------------------------------------------------------------------------------------------+
|                                    MASHUP PROJECT DOMAIN ENTITIES                                      |
|                                                                                                        |
|  [ ENT-01 ] MashupProject (Aggregate Root)      - Top-level project container & lifecycle owner       |
|  [ ENT-02 ] ProjectParticipant                  - Participating game binding & assigned role           |
|  [ ENT-03 ] MashupObjective                     - High-level creative vision & user thesis             |
|  [ ENT-04 ] FeatureRequirement                  - Structured functional requirement item               |
|  [ ENT-05 ] RequirementDependency               - Relationship & conflict links between requirements   |
|  [ ENT-06 ] StrategySelection                   - Integration strategy preference & rationale          |
|  [ ENT-07 ] EnvironmentConstraint               - OS, runtime, and modding framework boundaries        |
|  [ ENT-08 ] ProjectRevision                     - Immutable snapshot tracking configuration history    |
+--------------------------------------------------------------------------------------------------------+
```

---

## 3. DETAILED FIELD SPECIFICATIONS

### 3.1 Entity 1: `MashupProject` (Aggregate Root)

| Field Name | Type | Required? | Mutability | Uniqueness | Validation Rules & Constraints | Error Code on Violation |
| :--- | :---: | :---: | :---: | :---: | :--- | :--- |
| `project_id` | `UUIDv4` | **YES** | Immutable | Global | Must be RFC 4122 compliant UUIDv4 string. | `ERR_PRJ_INVALID_UUID` |
| `schema_version` | `string` | **YES** | Versioned | N/A | Must match SemVer pattern `^1\.[0-9]+\.[0-9]+$`. | `ERR_PRJ_UNSUPPORTED_SCHEMA` |
| `display_name` | `string` | **YES** | Mutable | Workspace | 3–80 characters; cannot be whitespace-only. | `ERR_PRJ_INVALID_NAME` |
| `description` | `string` | Optional | Mutable | N/A | Max 2,000 characters; UTF-8 text sanitized. | `ERR_PRJ_DESC_TOO_LONG` |
| `created_at_utc` | `ISO 8601`| **YES** | Immutable | N/A | UTC ISO-8601 timestamp string. | `ERR_PRJ_INVALID_TIMESTAMP` |
| `updated_at_utc` | `ISO 8601`| **YES** | Mutable | N/A | UTC ISO-8601; must be $\ge created\_at\_utc$. | `ERR_PRJ_INVALID_UPDATE_TIME` |
| `lifecycle_state`| `enum` | **YES** | State-Bound| N/A | Must be one of the 8 formal lifecycle states. | `ERR_PRJ_INVALID_LIFECYCLE` |
| `workspace_path` | `string` | **YES** | Immutable | Global | Must start with `D:\NexLudic\Projects\<id>\`. | `CRIT_SEC_INVALID_WORKSPACE` |
| `revision_number`| `integer`| **YES** | Monotonic | N/A | Strictly increasing integer ($\ge 1$). | `ERR_PRJ_INVALID_REVISION` |
| `current_revision_id`| `UUIDv4` | **YES** | Mutable | Project | UUID of the active snapshot revision. | `ERR_PRJ_REVISION_NOT_FOUND` |
| `audit_digest` | `string` | **YES** | Mutable | N/A | Hexadecimal SHA-256 rolling audit digest. | `ERR_PRJ_AUDIT_HASH_INVALID` |

### 3.2 Entity 2: `ProjectParticipant`

| Field Name | Type | Required? | Mutability | Uniqueness | Validation Rules & Constraints | Error Code on Violation |
| :--- | :---: | :---: | :---: | :---: | :--- | :--- |
| `participant_id` | `UUIDv4` | **YES** | Immutable | Project | Unique identifier for participant instance. | `ERR_PAR_INVALID_UUID` |
| `project_id` | `UUIDv4` | **YES** | Immutable | Foreign | Must match parent `MashupProject.project_id`. | `ERR_PAR_ORPHANED_PARTICIPANT`|
| `canonical_game_id`| `string`| **YES** | Mutable | Project | Must match Phase 4 regex `^game\.[a-z0-9_.-]+$`. | `ERR_PAR_INVALID_GAME_ID` |
| `role` | `enum` | **YES** | Mutable | Strategy | `HOST_ROLE`, `GUEST_ROLE`, `PEER_ROLE`, `SOURCE_ROLE`, `TARGET_ROLE`. | `ERR_PAR_INVALID_ROLE` |
| `assigned_at_utc`| `ISO 8601`| **YES** | Mutable | N/A | Timestamp when role was assigned. | `ERR_PAR_INVALID_TIMESTAMP` |
| `edition_id` | `string` | Optional | Mutable | N/A | Edition identifier (e.g. `java`, `bedrock`). | `ERR_PAR_UNKNOWN_EDITION` |
| `version_constraint`| `string`| Optional | Mutable | N/A | SemVer range string (e.g. `^1.20.1`). | `ERR_PAR_INVALID_SEMVER_SPEC` |
| `local_installation_ref`| `string`| Optional | Mutable | N/A | Reference to verified local install ID. | `ERR_PAR_INVALID_INSTALL_REF` |
| `notes` | `string` | Optional | Mutable | N/A | Max 1,000 characters of developer notes. | `ERR_PAR_NOTES_TOO_LONG` |

### 3.3 Entity 3: `MashupObjective`

| Field Name | Type | Required? | Mutability | Constraints |
| :--- | :---: | :---: | :---: | :--- |
| `objective_id` | `UUIDv4` | **YES** | Immutable | Unique identifier within project. |
| `title` | `string` | **YES** | Mutable | 5–120 characters; creative project tagline. |
| `core_thesis` | `string` | **YES** | Mutable | 20–2,000 characters; high-level mashup concept in creator language. |
| `intended_experience`| `string`| Optional | Mutable | Narrative or gameplay experience walkthrough (Max 5,000 chars). |
| `target_audience` | `string` | Optional | Mutable | Intended player audience or difficulty expectations. |

### 3.4 Entity 4: `FeatureRequirement`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values |
| :--- | :---: | :---: | :---: | :--- |
| `requirement_id` | `UUIDv4` | **YES** | Immutable | Unique identifier for feature item. |
| `title` | `string` | **YES** | Mutable | 5–120 characters; concise requirement title. |
| `description` | `string` | **YES** | Mutable | Detailed functional description (Max 2,000 chars). |
| `category` | `enum` | **YES** | Mutable | `GAMEPLAY_MECHANIC`, `ASSET_TRANSFER`, `STATE_SYNC`, `UI_HUD`, `AUDIO_FX`, `CONTROLS`, `SYSTEM_EVENT`. |
| `priority` | `enum` | **YES** | Mutable | `CRITICAL`, `HIGH`, `MEDIUM`, `LOW`. |
| `criticality` | `enum` | **YES** | Mutable | `MANDATORY` (Blocks mashup if missing), `OPTIONAL` (Graceful degradation). |
| `origin` | `enum` | **YES** | Immutable | `USER_DEFINED`, `SYSTEM_DERIVED`, `TEMPLATE_GENERATED`. |
| `source_participant_id`| `UUIDv4`| Optional | Mutable | Participant providing the asset, state, or event. |
| `target_participant_id`| `UUIDv4`| Optional | Mutable | Participant receiving the asset, state, or event. |
| `status` | `enum` | **YES** | Mutable | `PROPOSED`, `ACCEPTED`, `UNFULFILLED`, `RESOLVED`, `REJECTED`. |

### 3.5 Entity 5: `RequirementDependency`

| Field Name | Type | Required? | Mutability | Constraints |
| :--- | :---: | :---: | :---: | :--- |
| `dependency_id` | `UUIDv4` | **YES** | Immutable | Unique dependency edge ID. |
| `source_requirement_id`| `UUIDv4`| **YES** | Mutable | Dependent requirement ID. |
| `target_requirement_id`| `UUIDv4`| **YES** | Mutable | Prerequisite requirement ID. |
| `dependency_type` | `enum` | **YES** | Mutable | `REQUIRES` (Hard dependency), `ENHANCES` (Optional synergy), `CONFLICTS_WITH` (Mutually exclusive). |

### 3.6 Entity 6: `StrategySelection`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values |
| :--- | :---: | :---: | :---: | :--- |
| `selection_id` | `UUIDv4` | **YES** | Immutable | Unique selection record ID. |
| `strategy_type` | `enum` | **YES** | Mutable | `STRATEGY_1_HOST_MOD`, `STRATEGY_2_LIVE_BRIDGE`, `STRATEGY_3_HYBRID`, `STRATEGY_UNDECIDED`. |
| `rationale` | `string` | Optional | Mutable | Explanation for why this strategy was chosen (Max 1,000 chars). |
| `selected_by` | `enum` | **YES** | Mutable | `USER_MANUAL`, `RECOMMENDATION_ENGINE`. |
| `selected_at_utc` | `ISO 8601`| **YES** | Mutable | Timestamp of strategy selection. |

### 3.7 Entity 7: `EnvironmentConstraint`

| Field Name | Type | Required? | Mutability | Constraints |
| :--- | :---: | :---: | :---: | :--- |
| `constraint_id` | `UUIDv4` | **YES** | Immutable | Unique constraint identifier. |
| `participant_id` | `UUIDv4` | **YES** | Immutable | Associated participant instance. |
| `os_platform` | `enum` | **YES** | Mutable | `WINDOWS_10`, `WINDOWS_11`, `ANY_WINDOWS`. |
| `architecture` | `enum` | **YES** | Mutable | `X64`, `ARM64`. |
| `required_mod_frameworks`| `array[string]`| Optional | Mutable | e.g. `["Forge:^47.2.0"]`, `["Fabric:^0.15.0"]`. |
| `graphics_api_preference`| `enum` | Optional | Mutable | `DIRECTX_11`, `DIRECTX_12`, `VULKAN`, `OPENGL`. |

### 3.8 Entity 8: `ProjectRevision`

| Field Name | Type | Required? | Mutability | Constraints |
| :--- | :---: | :---: | :---: | :--- |
| `revision_id` | `UUIDv4` | **YES** | Immutable | Unique snapshot identifier. |
| `revision_number` | `integer`| **YES** | Immutable | Monotonically increasing revision sequence ($\ge 1$). |
| `timestamp_utc` | `ISO 8601`| **YES** | Immutable | Snapshot creation timestamp. |
| `author` | `string` | **YES** | Immutable | User or automation agent creating revision. |
| `change_summary` | `string` | **YES** | Immutable | Description of changes (e.g. "Added participant Game B"). |
| `snapshot_sha256`| `string` | **YES** | Immutable | Cryptographic SHA-256 digest of snapshot JSON. |
| `snapshot_file_path`| `string`| **YES** | Immutable | Path relative to project root (`revisions/rev_0001.json`). |

---

## 4. FORMAL JSON SCHEMA (`NLE-SCH-MPD-001`)

The aggregate root document `project.json` validates strictly against the following JSON Schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/mashup-designer/mashup-project.schema.json",
  "title": "MashupProjectDocument",
  "type": "object",
  "required": [
    "schema_version",
    "project_id",
    "display_name",
    "created_at_utc",
    "updated_at_utc",
    "lifecycle_state",
    "workspace_path",
    "revision_number",
    "current_revision_id",
    "participants",
    "objectives",
    "requirements",
    "strategy",
    "audit_digest"
  ],
  "properties": {
    "schema_version": { "type": "string", "pattern": "^1\\.[0-9]+\\.[0-9]+$" },
    "project_id": { "type": "string", "format": "uuid" },
    "display_name": { "type": "string", "minLength": 3, "maxLength": 80 },
    "description": { "type": "string", "maxLength": 2000 },
    "created_at_utc": { "type": "string", "format": "date-time" },
    "updated_at_utc": { "type": "string", "format": "date-time" },
    "lifecycle_state": {
      "type": "string",
      "enum": [
        "PROJECT_DRAFT",
        "PROJECT_INCOMPLETE",
        "PROJECT_READY_FOR_ASSESSMENT",
        "PROJECT_ASSESSMENT_PENDING",
        "PROJECT_PLANNING_IN_PROGRESS",
        "PROJECT_PLAN_AVAILABLE",
        "PROJECT_ARCHIVED",
        "PROJECT_DELETED"
      ]
    },
    "workspace_path": { "type": "string", "pattern": "^D:\\\\NexLudic\\\\Projects\\\\.+" },
    "revision_number": { "type": "integer", "minimum": 1 },
    "current_revision_id": { "type": "string", "format": "uuid" },
    "participants": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["participant_id", "canonical_game_id", "role"],
        "properties": {
          "participant_id": { "type": "string", "format": "uuid" },
          "canonical_game_id": { "type": "string", "pattern": "^game\\.[a-z0-9_.-]+$" },
          "role": {
            "type": "string",
            "enum": ["HOST_ROLE", "GUEST_ROLE", "PEER_ROLE", "SOURCE_ROLE", "TARGET_ROLE"]
          },
          "edition_id": { "type": "string" },
          "version_constraint": { "type": "string" },
          "local_installation_ref": { "type": "string" },
          "notes": { "type": "string", "maxLength": 1000 }
        },
        "additionalProperties": false
      },
      "minItems": 0,
      "maxItems": 8
    },
    "objectives": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["objective_id", "title", "core_thesis"],
        "properties": {
          "objective_id": { "type": "string", "format": "uuid" },
          "title": { "type": "string", "minLength": 5, "maxLength": 120 },
          "core_thesis": { "type": "string", "minLength": 20, "maxLength": 2000 },
          "intended_experience": { "type": "string", "maxLength": 5000 },
          "target_audience": { "type": "string", "maxLength": 255 }
        },
        "additionalProperties": false
      }
    },
    "requirements": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["requirement_id", "title", "description", "category", "priority", "criticality", "status"],
        "properties": {
          "requirement_id": { "type": "string", "format": "uuid" },
          "title": { "type": "string", "minLength": 5, "maxLength": 120 },
          "description": { "type": "string", "maxLength": 2000 },
          "category": {
            "type": "string",
            "enum": ["GAMEPLAY_MECHANIC", "ASSET_TRANSFER", "STATE_SYNC", "UI_HUD", "AUDIO_FX", "CONTROLS", "SYSTEM_EVENT"]
          },
          "priority": { "type": "string", "enum": ["CRITICAL", "HIGH", "MEDIUM", "LOW"] },
          "criticality": { "type": "string", "enum": ["MANDATORY", "OPTIONAL", "CONDITIONAL"] },
          "origin": { "type": "string", "enum": ["USER_DEFINED", "SYSTEM_DERIVED", "TEMPLATE_GENERATED"] },
          "source_participant_id": { "type": "string", "format": "uuid" },
          "target_participant_id": { "type": "string", "format": "uuid" },
          "status": { "type": "string", "enum": ["PROPOSED", "ACCEPTED", "UNFULFILLED", "RESOLVED", "REJECTED"] }
        },
        "additionalProperties": false
      }
    },
    "strategy": {
      "type": "object",
      "required": ["strategy_type", "selected_by"],
      "properties": {
        "strategy_type": {
          "type": "string",
          "enum": ["STRATEGY_1_HOST_MOD", "STRATEGY_2_LIVE_BRIDGE", "STRATEGY_3_HYBRID", "STRATEGY_UNDECIDED"]
        },
        "rationale": { "type": "string", "maxLength": 1000 },
        "selected_by": { "type": "string", "enum": ["USER_MANUAL", "RECOMMENDATION_ENGINE"] },
        "selected_at_utc": { "type": "string", "format": "date-time" }
      },
      "additionalProperties": false
    },
    "audit_digest": { "type": "string", "pattern": "^[a-f0-9]{64}$" }
  },
  "additionalProperties": false
}
```

---
*End of Catalog — Document `NLE-CAT-MPD-001`.*
