# CROSS-GAME SOURCE AND TARGET DOMAIN CONCEPT DESCRIPTOR CONTRACT

**Document Identifier:** `NLE-CON-STD-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Schema Validation  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-EDR-001` (Step 71), `NLE-CON-CPV-001` (Step 72)  
**Downstream Dependents:** `NLE-POL-MIR-001` (Step 74), `NLE-CON-SEC-001` (Step 74)

---

## 1. Purpose and Scope

This contract specifies the formal schema, registration protocol, validation rules, and lifecycle requirements for **Domain Concept Descriptors (DCDs)**. A Domain Concept Descriptor defines the structural, physical, and semantic vocabulary of an endpoint's domain concepts (e.g., actor transform, character stats, inventory item, damage event, vehicle dynamics).

Every participating endpoint registered under Step 71 must publish or bind to explicit DCDs for all data categories it produces or consumes. Without an authoritative DCD, an endpoint cannot engage in semantic mapping.

---

## 2. Domain Concept Descriptor Schema

A valid Domain Concept Descriptor is an immutable JSON-LD compliant document conforming to the following formal JSON Schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/dcd-v1.json",
  "title": "DomainConceptDescriptor",
  "type": "object",
  "required": [
    "descriptor_id",
    "concept_uri",
    "schema_version",
    "domain_category",
    "coordinate_system",
    "properties",
    "integrity_hash"
  ],
  "properties": {
    "descriptor_id": {
      "type": "string",
      "format": "uuid"
    },
    "concept_uri": {
      "type": "string",
      "pattern": "^nexludic://concepts/[a-z0-9_-]+/[a-z0-9_.-]+$"
    },
    "schema_version": {
      "type": "string",
      "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$"
    },
    "domain_category": {
      "type": "string",
      "enum": [
        "SPATIAL_TRANSFORM",
        "PHYSICAL_KINEMATICS",
        "ACTOR_ATTRIBUTES",
        "INVENTORY_ECONOMY",
        "COMBAT_DAMAGE_EVENT",
        "ENVIRONMENT_WEATHER",
        "GAMEPLAY_STATE_FLAG",
        "CUSTOM_USER_DEFINED"
      ]
    },
    "coordinate_system": {
      "type": "object",
      "required": ["handedness", "up_axis", "forward_axis", "length_unit"],
      "properties": {
        "handedness": { "type": "string", "enum": ["RIGHT_HANDED", "LEFT_HANDED"] },
        "up_axis": { "type": "string", "enum": ["Y_UP", "Z_UP"] },
        "forward_axis": { "type": "string", "enum": ["Z_FORWARD", "X_FORWARD", "NEG_Z_FORWARD"] },
        "length_unit": { "type": "string", "enum": ["METER", "CENTIMETER", "INCH", "FOOT", "UNREAL_UNIT", "HAMMER_UNIT"] }
      }
    },
    "properties": {
      "type": "object",
      "additionalProperties": {
        "type": "object",
        "required": ["data_type", "nullable", "semantic_role"],
        "properties": {
          "data_type": {
            "type": "string",
            "enum": ["FLOAT32", "FLOAT64", "INT32", "INT64", "UINT32", "BOOLEAN", "STRING_UTF8", "VECTOR3", "QUATERNION", "UUID"]
          },
          "nullable": { "type": "boolean" },
          "semantic_role": {
            "type": "string",
            "enum": ["POSITION", "ORIENTATION", "LINEAR_VELOCITY", "ANGULAR_VELOCITY", "SCALAR_METRIC", "IDENTIFIER", "STATE_FLAG", "ENUM_ORDINAL"]
          },
          "unit_of_measure": { "type": "string" },
          "valid_range": {
            "type": "object",
            "properties": {
              "min": { "type": "number" },
              "max": { "type": "number" },
              "inclusive": { "type": "boolean" }
            }
          },
          "precision_epsilon": { "type": "number" },
          "enum_definition_uri": { "type": "string" }
        }
      }
    },
    "invariants": {
      "type": "array",
      "items": { "type": "string" }
    },
    "integrity_hash": {
      "type": "string",
      "pattern": "^sha256:[a-f0-9]{64}$"
    }
  }
}
```

---

## 3. Structural vs. Semantic Validation

The DCD contract establishes a two-stage evaluation gate:

### Stage 1: Structural Validation
- Evaluates datatype matching, field existence, nullability constraints, and array lengths.
- Fails immediately if a payload contains mismatched binary or JSON primitives.
- Handled at wire speed via compiled schema validators.

### Stage 2: Semantic Validation
- Evaluates coordinate frame conversions, dimensional consistency, mathematical ranges, and domain invariants.
- Example: A vector `[0, 1000000, 0]` is structurally a valid `VECTOR3`, but semantically invalid if the DCD declares a maximum altitude invariant of `50000.0` meters.
- Guarantees physical and logical plausibility before any mapping calculation occurs.

---

## 4. Descriptor Registration & Immutability Rules

1. **Deterministic URI Identification:** Concept URIs are namespaced and immutable once published (e.g., `nexludic://concepts/ue5_vehicle/suspension_telemetry`).
2. **Strict SemVer Incrementing:**
   - **Patch:** Non-breaking documentation or range clarification without schema changes.
   - **Minor:** Adding optional fields with safe defaults.
   - **Major:** Removing fields, altering coordinate systems, changing data types, or tightening ranges.
3. **Integrity Hash Enforcement:** The `integrity_hash` covers the canonicalized JSON representation (excluding the hash field itself). Descriptors with mismatched hashes are rejected during engine boot.
