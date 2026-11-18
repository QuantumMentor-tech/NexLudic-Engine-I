# CONFORMANCE TEST CASE AND FIXTURE SCHEMA
**Document ID:** `NLE-SCH-TCF-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior QA Automation Engineer, Software Testing Architect, API Contract Designer, Data Lifecycle Engineer  
**Classification:** Schema & Data Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the formal JSON Schemas governing test case definitions and synthetic test fixtures used within the NexLudic Universal Adapter SDK Conformance Test Harness (`CMP-41`). 

To guarantee that test results are reproducible, deterministic, and verifiable across diverse host environments, test cases and test fixtures MUST be formally specified, versioned, and validated against strict schemas. All test scenarios MUST utilize synthetic data that isolates testing from proprietary game assets, live user credentials, and active network connections.

---

## 2. CONFORMANCE TEST CASE DEFINITION SCHEMA (`NLE-SCH-TCD-001`)

Every test case executed by the harness is declared as an immutable JSON document validating against the following schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/conformance-test-case.schema.json",
  "title": "ConformanceTestCaseDefinition",
  "type": "object",
  "required": [
    "test_id",
    "category",
    "title",
    "description",
    "target_contract_id",
    "contract_section",
    "criticality",
    "applicable_modes",
    "preconditions",
    "fixture_references",
    "stimulus",
    "expected_outcome"
  ],
  "properties": {
    "test_id": {
      "type": "string",
      "pattern": "^CONF-[A-Z]{3,4}-[0-9]{3}$",
      "description": "Unique, immutable test identifier (e.g., CONF-PKG-001)"
    },
    "category": {
      "type": "string",
      "enum": [
        "PACKAGE_INTEGRITY",
        "SDK_PROTOCOL",
        "CAPABILITY_SEMANTICS",
        "REGISTRY_DISCOVERY",
        "CAPABILITY_MATCHING",
        "NEGOTIATION_AVAILABILITY",
        "LIFECYCLE_HEALTH",
        "CROSS_CONTRACT_REGRESSION",
        "SECURITY_SANDBOXING"
      ]
    },
    "title": { "type": "string", "minLength": 5, "maxLength": 120 },
    "description": { "type": "string" },
    "target_contract_id": {
      "type": "string",
      "pattern": "^NLE-[A-Z]{3}-[A-Z0-9]{3}-[0-9]{3}$"
    },
    "contract_section": { "type": "string" },
    "criticality": {
      "type": "string",
      "enum": ["MANDATORY", "ADVISORY"]
    },
    "applicable_modes": {
      "type": "array",
      "items": { "type": "string", "enum": ["STATIC", "SIMULATION", "DYNAMIC"] },
      "minItems": 1
    },
    "preconditions": {
      "type": "array",
      "items": { "type": "string" }
    },
    "fixture_references": {
      "type": "array",
      "items": { "type": "string", "pattern": "^FX-[A-Z0-9-]+$" }
    },
    "stimulus": {
      "type": "object",
      "required": ["action", "parameters"],
      "properties": {
        "action": { "type": "string" },
        "parameters": { "type": "object" },
        "timeout_ms": { "type": "integer", "minimum": 100, "maximum": 30000 }
      },
      "additionalProperties": false
    },
    "expected_outcome": {
      "type": "object",
      "required": ["expected_status", "assertion_rules"],
      "properties": {
        "expected_status": { "type": "string", "enum": ["PASS", "FAIL", "BLOCKED", "SKIPPED"] },
        "expected_diagnostic_codes": {
          "type": "array",
          "items": { "type": "string", "pattern": "^(OK|[A-Z]{3,4}_[A-Z0-9_]+)$" }
        },
        "assertion_rules": {
          "type": "array",
          "items": {
            "type": "object",
            "required": ["field_path", "operator", "expected_value"],
            "properties": {
              "field_path": { "type": "string" },
              "operator": { "type": "string", "enum": ["EQUALS", "NOT_EQUALS", "CONTAINS", "MATCHES_REGEX", "LESS_THAN_OR_EQUAL"] },
              "expected_value": {}
            }
          }
        }
      },
      "additionalProperties": false
    },
    "cleanup_actions": {
      "type": "array",
      "items": { "type": "string" }
    }
  },
  "additionalProperties": false
}
```

---

## 3. SYNTHETIC FIXTURE DEFINITION SCHEMA (`NLE-SCH-SFD-001`)

Synthetic fixtures provide the controlled inputs and test doubles consumed by the test suites. Every fixture is governed by the following schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/synthetic-fixture.schema.json",
  "title": "SyntheticFixtureDefinition",
  "type": "object",
  "required": ["fixture_id", "fixture_type", "version", "description", "payload"],
  "properties": {
    "fixture_id": {
      "type": "string",
      "pattern": "^FX-[A-Z0-9-]+$",
      "description": "Unique fixture identifier"
    },
    "fixture_type": {
      "type": "string",
      "enum": [
        "VALID_DESCRIPTOR",
        "MALFORMED_DESCRIPTOR",
        "OVERSIZED_PACKAGE",
        "PROTOCOL_DOUBLE",
        "REGISTRY_DOUBLE",
        "MATCHING_REQUIREMENT_DOUBLE",
        "NEGOTIATION_DOUBLE",
        "LIFECYCLE_MOCK_WORKER"
      ]
    },
    "version": { "type": "string", "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$" },
    "description": { "type": "string" },
    "payload": {
      "type": "object",
      "description": "The mock data payload (descriptor object, synthetic requirement, or protocol message)"
    },
    "synthetic_invariants": {
      "type": "array",
      "items": { "type": "string" }
    }
  },
  "additionalProperties": false
}
```

---

## 4. SYNTHETIC DATA SAFETY RULES

To eliminate security, legal, and operational risks:
1. **Synthetic Canonical Game IDs:** Tests MUST use synthetic game identifiers (`game.synth.arena_fps`, `game.synth.speed_racer`, `game.synth.block_builder`), NEVER commercial game titles.
2. **Synthetic Capability IDs:** Tests MUST use canonical category identifiers with synthetic domain suffixes (e.g. `telemetry.synth_player_pose`, `audio.synth_spatial_reverb`).
3. **No Real File Paths:** File paths MUST be expressed relative to the synthetic mount root `D:\NexLudic\Test\Synthetic\`.
4. **No Network Hosts:** URLs and endpoints MUST resolve strictly to `127.0.0.1` or synthetic named pipes (`\\.\pipe\nexludic_test_*`).

---

## 5. CONCRETE SYNTHETIC FIXTURE EXAMPLES

### 5.1 Fixture `FX-BASE-001`: Valid Baseline Adapter Descriptor
Used by `CONF-PKG-004`, `CONF-CAP-001`, and happy-path test sequences:

```json
{
  "fixture_id": "FX-BASE-001",
  "fixture_type": "VALID_DESCRIPTOR",
  "version": "1.0.0",
  "description": "A fully compliant baseline adapter descriptor with valid taxonomy references",
  "payload": {
    "adapter_id": "com.synthetic.arena-adapter",
    "version": "1.0.0",
    "target_game": {
      "canonical_game_id": "game.synth.arena_fps",
      "supported_versions": ["^1.2.0"]
    },
    "integration_strategy": "STRATEGY_2_INTEROPERABILITY_BRIDGING",
    "sdk_version": "1.0.0",
    "capabilities": [
      {
        "capability_id": "telemetry.player_transform",
        "version": "1.0.0",
        "execution_mode": "PASSIVE_MONITOR",
        "update_rate_hz": 60,
        "parameters": {
          "coordinate_system": "LEFT_HANDED_Y_UP",
          "precision": "FLOAT32"
        }
      }
    ],
    "binaries": {
      "windows_x64": {
        "executable_path": "bin/arena_worker.exe",
        "sha256": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
      }
    }
  },
  "synthetic_invariants": [
    "Conforms to NLE-SCH-ACD-001",
    "Targets synthetic game ID",
    "Contains zero path traversal"
  ]
}
```

### 5.2 Fixture `FX-ERR-003`: Cyclic Capability Dependency Descriptor
Used by `CONF-CAP-004` to verify Kahn DAG cycle rejection:

```json
{
  "fixture_id": "FX-ERR-003",
  "fixture_type": "MALFORMED_DESCRIPTOR",
  "version": "1.0.0",
  "description": "A descriptor declaring a cyclic dependency between capability A and B",
  "payload": {
    "adapter_id": "com.synthetic.cyclic-adapter",
    "version": "1.0.0",
    "target_game": { "canonical_game_id": "game.synth.block_builder" },
    "integration_strategy": "STRATEGY_1_HOST_MODIFICATION",
    "sdk_version": "1.0.0",
    "capabilities": [
      {
        "capability_id": "physics.mesh_extraction",
        "version": "1.0.0",
        "dependencies": ["rendering.overlay_mesh"]
      },
      {
        "capability_id": "rendering.overlay_mesh",
        "version": "1.0.0",
        "dependencies": ["physics.mesh_extraction"]
      }
    ]
  },
  "synthetic_invariants": [
    "Contains circular dependency A -> B -> A",
    "Must be rejected with ERR_CAP_CYCLIC_DEPENDENCY"
  ]
}
```

---
*End of Schema — Document `NLE-SCH-TCF-001`.*
