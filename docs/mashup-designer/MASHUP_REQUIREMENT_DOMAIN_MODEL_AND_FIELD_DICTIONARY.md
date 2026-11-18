# MASHUP REQUIREMENT DOMAIN MODEL AND FIELD DICTIONARY
**Document ID:** `NLE-CAT-MRD-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Domain Modeling Architect, Systems Analyst, API Contract Designer, Senior QA Engineer  
**Classification:** Core Data Model & Data Dictionary  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This document establishes the exhaustive data dictionary, field specifications, data types, constraints, nullability rules, and JSON Schema definitions governing the **Mashup Requirements Domain Model** (`FeatureRequirement`, `FeatureGroup`, `Feature`, `AcceptanceCriterion`, `AssumptionRecord`, `OpenQuestionRecord`, `RequirementDependency`).

Engineering teams implementing requirements ingestion, UI editors, decomposition parsers, or storage engines MUST adhere strictly to the definitions cataloged herein.

---

## 2. COMPLETE REQUIREMENTS ENTITY MODEL

```
+--------------------------------------------------------------------------------------------------------+
|                                    REQUIREMENTS DOMAIN ENTITIES                                        |
|                                                                                                        |
|  [ ENT-01 ] FeatureGroup         - High-level subsystem grouping (e.g. Weaponry, Audio Overlay)       |
|  [ ENT-02 ] Feature              - User-visible capability (e.g. Super Shotgun Integration)           |
|  [ ENT-03 ] FeatureRequirement   - Atomic, testable behavioral unit                                   |
|  [ ENT-04 ] AcceptanceCriterion  - Measurable pass/fail threshold                                     |
|  [ ENT-05 ] AssumptionRecord     - Explicit unverified technical or design assumption                 |
|  [ ENT-06 ] OpenQuestionRecord   - Unresolved ambiguity requiring creator or technical clarification  |
|  [ ENT-07 ] RequirementDependency- Relationship edge (REQUIRES, CONFLICTS_WITH, ENHANCES, PRECEDES)   |
+--------------------------------------------------------------------------------------------------------+
```

---

## 3. DETAILED FIELD DICTIONARY

### 3.1 Entity 1: `FeatureGroup`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values | Diagnostic Error Code |
| :--- | :---: | :---: | :---: | :--- | :--- |
| `group_id` | `UUIDv4` | **YES** | Immutable | Unique identifier for feature group. | `ERR_GRP_INVALID_UUID` |
| `project_id` | `UUIDv4` | **YES** | Immutable | Foreign key to `MashupProject.project_id`. | `ERR_GRP_ORPHANED_GROUP` |
| `title` | `string` | **YES** | Mutable | 3 to 80 characters; non-empty. | `ERR_GRP_INVALID_TITLE` |
| `description` | `string` | Optional | Mutable | Max 1,000 characters; high-level subsystem summary. | `ERR_GRP_DESC_TOO_LONG` |
| `category` | `enum` | **YES** | Mutable | `GAMEPLAY`, `ASSETS`, `STATE_SYNC`, `UI`, `AUDIO`, `CONTROLS`. | `ERR_GRP_INVALID_CATEGORY` |
| `display_order` | `integer`| **YES** | Mutable | Integer $\ge 0$ controlling presentation order. | `ERR_GRP_INVALID_ORDER` |

### 3.2 Entity 2: `Feature`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values | Diagnostic Error Code |
| :--- | :---: | :---: | :---: | :--- | :--- |
| `feature_id` | `UUIDv4` | **YES** | Immutable | Unique identifier for feature item. | `ERR_FTR_INVALID_UUID` |
| `group_id` | `UUIDv4` | **YES** | Mutable | Parent `FeatureGroup.group_id`. | `ERR_FTR_ORPHANED_FEATURE` |
| `project_id` | `UUIDv4` | **YES** | Immutable | Foreign key to `MashupProject.project_id`. | `ERR_FTR_PROJECT_MISMATCH` |
| `title` | `string` | **YES** | Mutable | 5 to 120 characters; concise feature name. | `ERR_FTR_INVALID_TITLE` |
| `description` | `string` | **YES** | Mutable | 10 to 2,000 characters; functional summary. | `ERR_FTR_DESC_TOO_SHORT` |
| `priority` | `enum` | **YES** | Mutable | `CRITICAL`, `HIGH`, `MEDIUM`, `LOW`. | `ERR_FTR_INVALID_PRIORITY` |
| `user_intent_excerpt`| `string`| Optional | Mutable | Verbatim quote from creator's `core_thesis`. | `ERR_FTR_EXCERPT_TOO_LONG` |

### 3.3 Entity 3: `FeatureRequirement` (Atomic Behavioral Unit)

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values | Diagnostic Error Code |
| :--- | :---: | :---: | :---: | :--- | :--- |
| `requirement_id` | `UUIDv4` | **YES** | Immutable | Unique identifier for requirement item. | `ERR_REQ_INVALID_UUID` |
| `feature_id` | `UUIDv4` | **YES** | Mutable | Parent `Feature.feature_id`. | `ERR_REQ_ORPHANED_REQ` |
| `project_id` | `UUIDv4` | **YES** | Immutable | Foreign key to `MashupProject.project_id`. | `ERR_REQ_PROJECT_MISMATCH` |
| `title` | `string` | **YES** | Mutable | 5 to 120 characters. | `ERR_REQ_INVALID_TITLE` |
| `statement` | `string` | **YES** | Mutable | Formal requirement statement ("The system SHALL..."). | `ERR_REQ_STATEMENT_EMPTY` |
| `category` | `enum` | **YES** | Mutable | `GAMEPLAY_MECHANIC`, `ASSET_TRANSFER`, `STATE_SYNC`, `UI_HUD`, `AUDIO_FX`, `CONTROLS`, `SYSTEM_EVENT`. | `ERR_REQ_INVALID_CATEGORY` |
| `priority` | `enum` | **YES** | Mutable | `CRITICAL`, `HIGH`, `MEDIUM`, `LOW`. | `ERR_REQ_INVALID_PRIORITY` |
| `criticality` | `enum` | **YES** | Mutable | `MANDATORY`, `OPTIONAL`, `CONDITIONAL`, `ALTERNATIVE`. | `ERR_REQ_INVALID_CRITICALITY` |
| `origin` | `enum` | **YES** | Immutable | `USER_EXPLICIT`, `USER_CONFIRMED_INFERENCE`, `SYSTEM_PROPOSED`, `TEMPLATE_DERIVED`. | `ERR_REQ_INVALID_ORIGIN` |
| `source_participant_id`| `UUIDv4`| Optional | Mutable | Participant providing asset, state, or event. | `ERR_REQ_UNKNOWN_SOURCE` |
| `target_participant_id`| `UUIDv4`| Optional | Mutable | Participant executing or receiving behavior. | `ERR_REQ_UNKNOWN_TARGET` |
| `quality_status` | `enum` | **YES** | State-Bound| `DRAFT`, `NEEDS_CLARIFICATION`, `INCOMPLETE`, `READY_FOR_ANALYSIS`, `BLOCKED`, `CONFLICTED`, `REJECTED`. | `ERR_REQ_INVALID_QUALITY` |

### 3.4 Entity 4: `AcceptanceCriterion`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values | Diagnostic Error Code |
| :--- | :---: | :---: | :---: | :--- | :--- |
| `criterion_id` | `UUIDv4` | **YES** | Immutable | Unique identifier for criterion. | `ERR_CRT_INVALID_UUID` |
| `requirement_id` | `UUIDv4` | **YES** | Immutable | Foreign key to `FeatureRequirement.requirement_id`.| `ERR_CRT_ORPHANED_CRITERION`|
| `statement` | `string` | **YES** | Mutable | Measurable condition statement ("Given X, When Y, Then Z"). | `ERR_CRT_STATEMENT_EMPTY` |
| `metric_type` | `enum` | **YES** | Mutable | `BOOLEAN_ASSERTION`, `NUMERIC_THRESHOLD`, `LATENCY_MS`, `FRAME_RATE_HZ`, `STATE_EQUIVALENCE`. | `ERR_CRT_INVALID_METRIC` |
| `target_value` | `string` | **YES** | Mutable | Stringified expected value (e.g. "20", "< 16.6ms"). | `ERR_CRT_TARGET_EMPTY` |
| `tolerance` | `string` | Optional | Mutable | Allowable variance (e.g. "+/- 2ms", "5%"). | `ERR_CRT_INVALID_TOLERANCE` |
| `verification_method`| `enum` | **YES** | Mutable | `AUTOMATED_TEST_DOUBLE`, `INTEGRATION_TEST`, `MANUAL_INSPECTION`, `TELEMETRY_LOG`. | `ERR_CRT_INVALID_VERIF_METH`|

### 3.5 Entity 5: `AssumptionRecord`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values |
| :--- | :---: | :---: | :---: | :--- |
| `assumption_id` | `UUIDv4` | **YES** | Immutable | Unique identifier. |
| `requirement_id` | `UUIDv4` | **YES** | Immutable | Parent requirement. |
| `statement` | `string` | **YES** | Mutable | Clear technical assumption (e.g. "Target game supports custom raycast weapons"). |
| `rationale` | `string` | **YES** | Mutable | Why this assumption was made. |
| `risk_if_invalid` | `string` | **YES** | Mutable | Impact on mashup if assumption proves false. |
| `validation_status`| `enum` | **YES** | Mutable | `UNVERIFIED_ASSUMPTION`, `CONFIRMED_TRUE`, `DISPROVEN_FALSE`. |

### 3.6 Entity 6: `OpenQuestionRecord`

| Field Name | Type | Required? | Mutability | Constraints & Allowed Values |
| :--- | :---: | :---: | :---: | :--- |
| `question_id` | `UUIDv4` | **YES** | Immutable | Unique identifier. |
| `requirement_id` | `UUIDv4` | **YES** | Immutable | Parent requirement. |
| `question_text` | `string` | **YES** | Mutable | Specific ambiguity needing resolution (e.g. "What should happen if the player dies?"). |
| `affected_scope` | `string` | **YES** | Mutable | Explanation of which mechanic depends on this question. |
| `possible_answers`| `array[string]`| Optional | Mutable | Suggested options for the creator. |
| `is_blocking` | `boolean` | **YES** | Mutable | If true, blocks requirement from `READY_FOR_ANALYSIS`. |
| `answered_value` | `string` | Optional | Mutable | Creator's resolved choice. |

---

## 4. FORMAL REQUIREMENTS JSON SCHEMA (`NLE-SCH-FRD-001`)

The formal JSON Schema governing decomposed requirements within `project.json` is defined below:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/mashup-designer/feature-decomposition.schema.json",
  "title": "FeatureDecompositionDocument",
  "type": "object",
  "required": ["feature_groups", "features", "requirements", "dependencies"],
  "properties": {
    "feature_groups": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["group_id", "project_id", "title", "category", "display_order"],
        "properties": {
          "group_id": { "type": "string", "format": "uuid" },
          "project_id": { "type": "string", "format": "uuid" },
          "title": { "type": "string", "minLength": 3, "maxLength": 80 },
          "description": { "type": "string", "maxLength": 1000 },
          "category": { "type": "string", "enum": ["GAMEPLAY", "ASSETS", "STATE_SYNC", "UI", "AUDIO", "CONTROLS"] },
          "display_order": { "type": "integer", "minimum": 0 }
        },
        "additionalProperties": false
      }
    },
    "features": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["feature_id", "group_id", "project_id", "title", "description", "priority"],
        "properties": {
          "feature_id": { "type": "string", "format": "uuid" },
          "group_id": { "type": "string", "format": "uuid" },
          "project_id": { "type": "string", "format": "uuid" },
          "title": { "type": "string", "minLength": 5, "maxLength": 120 },
          "description": { "type": "string", "minLength": 10, "maxLength": 2000 },
          "priority": { "type": "string", "enum": ["CRITICAL", "HIGH", "MEDIUM", "LOW"] },
          "user_intent_excerpt": { "type": "string", "maxLength": 500 }
        },
        "additionalProperties": false
      }
    },
    "requirements": {
      "type": "array",
      "items": {
        "type": "object",
        "required": [
          "requirement_id",
          "feature_id",
          "project_id",
          "title",
          "statement",
          "category",
          "priority",
          "criticality",
          "origin",
          "quality_status",
          "acceptance_criteria"
        ],
        "properties": {
          "requirement_id": { "type": "string", "format": "uuid" },
          "feature_id": { "type": "string", "format": "uuid" },
          "project_id": { "type": "string", "format": "uuid" },
          "title": { "type": "string", "minLength": 5, "maxLength": 120 },
          "statement": { "type": "string", "minLength": 10, "maxLength": 2000 },
          "category": {
            "type": "string",
            "enum": ["GAMEPLAY_MECHANIC", "ASSET_TRANSFER", "STATE_SYNC", "UI_HUD", "AUDIO_FX", "CONTROLS", "SYSTEM_EVENT"]
          },
          "priority": { "type": "string", "enum": ["CRITICAL", "HIGH", "MEDIUM", "LOW"] },
          "criticality": { "type": "string", "enum": ["MANDATORY", "OPTIONAL", "CONDITIONAL", "ALTERNATIVE"] },
          "origin": {
            "type": "string",
            "enum": ["USER_EXPLICIT", "USER_CONFIRMED_INFERENCE", "SYSTEM_PROPOSED", "TEMPLATE_DERIVED"]
          },
          "source_participant_id": { "type": "string", "format": "uuid" },
          "target_participant_id": { "type": "string", "format": "uuid" },
          "quality_status": {
            "type": "string",
            "enum": ["DRAFT", "NEEDS_CLARIFICATION", "INCOMPLETE", "READY_FOR_ANALYSIS", "BLOCKED", "CONFLICTED", "REJECTED"]
          },
          "acceptance_criteria": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["criterion_id", "statement", "metric_type", "target_value", "verification_method"],
              "properties": {
                "criterion_id": { "type": "string", "format": "uuid" },
                "statement": { "type": "string" },
                "metric_type": {
                  "type": "string",
                  "enum": ["BOOLEAN_ASSERTION", "NUMERIC_THRESHOLD", "LATENCY_MS", "FRAME_RATE_HZ", "STATE_EQUIVALENCE"]
                },
                "target_value": { "type": "string" },
                "tolerance": { "type": "string" },
                "verification_method": {
                  "type": "string",
                  "enum": ["AUTOMATED_TEST_DOUBLE", "INTEGRATION_TEST", "MANUAL_INSPECTION", "TELEMETRY_LOG"]
                }
              },
              "additionalProperties": false
            }
          },
          "assumptions": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["assumption_id", "statement", "risk_if_invalid", "validation_status"],
              "properties": {
                "assumption_id": { "type": "string", "format": "uuid" },
                "statement": { "type": "string" },
                "rationale": { "type": "string" },
                "risk_if_invalid": { "type": "string" },
                "validation_status": { "type": "string", "enum": ["UNVERIFIED_ASSUMPTION", "CONFIRMED_TRUE", "DISPROVEN_FALSE"] }
              },
              "additionalProperties": false
            }
          },
          "open_questions": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["question_id", "question_text", "affected_scope", "is_blocking"],
              "properties": {
                "question_id": { "type": "string", "format": "uuid" },
                "question_text": { "type": "string" },
                "affected_scope": { "type": "string" },
                "possible_answers": { "type": "array", "items": { "type": "string" } },
                "is_blocking": { "type": "boolean" },
                "answered_value": { "type": "string" }
              },
              "additionalProperties": false
            }
          }
        },
        "additionalProperties": false
      }
    },
    "dependencies": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["dependency_id", "source_requirement_id", "target_requirement_id", "dependency_type"],
        "properties": {
          "dependency_id": { "type": "string", "format": "uuid" },
          "source_requirement_id": { "type": "string", "format": "uuid" },
          "target_requirement_id": { "type": "string", "format": "uuid" },
          "dependency_type": {
            "type": "string",
            "enum": ["REQUIRES", "ENHANCES", "CONFLICTS_WITH", "PRECEDES", "ALTERNATIVE_TO"]
          }
        },
        "additionalProperties": false
      }
    }
  },
  "additionalProperties": false
}
```

---
*End of Catalog — Document `NLE-CAT-MRD-001`.*
