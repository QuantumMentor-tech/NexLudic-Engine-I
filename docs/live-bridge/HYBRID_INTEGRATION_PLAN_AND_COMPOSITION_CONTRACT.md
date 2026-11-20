# HYBRID INTEGRATION PLAN AND COMPOSITION CONTRACT

**Document Identifier:** `NLE-CON-HIP-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Plan Specification  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-CON-CAA-001` (Step 76), `NLE-POL-CAP-001` (Step 70)  
**Downstream Dependents:** `NLE-SPC-HDE-001` (Step 77), `NLE-POL-HCS-001` (Step 77)

---

## 1. Purpose and Scope

This contract specifies the formal schema, validation rules, immutable invariants, and dynamic mutability constraints for **Hybrid Integration Plans (HIPs)**.

A Hybrid Integration Plan is the authoritative document that defines how multiple integration strategies are composed, sequenced, authorized, and governed for a specific game pair.

---

## 2. Hybrid Integration Plan JSON Schema

A valid Hybrid Integration Plan conforms to the following formal JSON Schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/hybrid-integration-plan-v1.json",
  "title": "HybridIntegrationPlan",
  "type": "object",
  "required": [
    "plan_id",
    "plan_version",
    "target_game_pair",
    "participating_strategies",
    "dependency_graph",
    "data_exchange_contracts",
    "authorization_tokens",
    "fallback_policies",
    "integrity_hash"
  ],
  "properties": {
    "plan_id": { "type": "string", "format": "uuid" },
    "plan_version": { "type": "string", "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$" },
    "target_game_pair": {
      "type": "object",
      "required": ["source_game_id", "target_game_id", "source_build_hash", "target_build_hash"],
      "properties": {
        "source_game_id": { "type": "string" },
        "target_game_id": { "type": "string" },
        "source_build_hash": { "type": "string", "pattern": "^sha256:[a-f0-9]{64}$" },
        "target_build_hash": { "type": "string", "pattern": "^sha256:[a-f0-9]{64}$" }
      }
    },
    "participating_strategies": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["strategy_id", "strategy_type", "implementation_version", "role", "required_capabilities"],
        "properties": {
          "strategy_id": { "type": "string" },
          "strategy_type": { "type": "string", "enum": ["HOST_GAME_MODIFICATION", "LIVE_BRIDGE", "HYBRID_SIDECAR"] },
          "implementation_version": { "type": "string" },
          "role": { "type": "string", "enum": ["PRIMARY", "SUPPORTING", "FALLBACK"] },
          "required_capabilities": { "type": "array", "items": { "type": "string" } }
        }
      }
    },
    "dependency_graph": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["source_strategy_id", "dependent_strategy_id", "dependency_type", "is_mandatory"],
        "properties": {
          "source_strategy_id": { "type": "string" },
          "dependent_strategy_id": { "type": "string" },
          "dependency_type": { "type": "string" },
          "is_mandatory": { "type": "boolean" }
        }
      }
    },
    "data_exchange_contracts": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["domain_id", "mapping_record_urn", "schema_version"],
        "properties": {
          "domain_id": { "type": "string" },
          "mapping_record_urn": { "type": "string" },
          "schema_version": { "type": "string" }
        }
      }
    },
    "authorization_tokens": {
      "type": "object",
      "required": ["phase_8_content_mod_token", "phase_9_live_bridge_token"],
      "properties": {
        "phase_8_content_mod_token": { "type": "string" },
        "phase_9_live_bridge_token": { "type": "string" }
      }
    },
    "fallback_policies": {
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

## 3. The 14-Check Plan Validation Pipeline

Before a plan can transition to `ELIGIBLE_FOR_ACTIVATION`, it must satisfy fourteen strict checks:

```
+----------------------------------------------------------------------------------------------------+
| 1. Plan Schema Conformance: Fully complies with JSON Schema Draft 2020-12.                         |
| 2. Game Build Matching: Source and target build hashes match local installed game executables.     |
| 3. Dual Authorization Presence: Independent Phase 8 and Phase 9 cryptographic tokens valid.        |
| 4. Capability Evidence Sufficiency: Every declared capability verified at Level 3+ (Verified).     |
| 5. Dependency Graph Acyclicity: Kahn's algorithm confirms topological DAG with zero cycles.        |
| 6. Precedence Invariant Compliance: Phase 8 package dependencies applied and verified prior.       |
| 7. Single Responsibility for State: Exactly one strategy holds write authority per domain.         |
| 8. Semantic Mapping Compatibility: Step 74 SMR URNs match approved active versions.                |
| 9. Message Schema Alignment: Step 73 Envelope versions compatible across all components.           |
| 10. Fallback Policy Eligibility: Permitted fallback destinations explicitly declared and signed.   |
| 11. Resource Quota Compliance: Memory, CPU, and queue budgets within engine limits.               |
| 12. Reversibility & Rollback Disclosure: Irreversible operations explicitly identified.            |
| 13. Audit Logging Readiness: Telemetry endpoints reachable and verified.                           |
| 14. Plan Checksum Verification: SHA-256 integrity hash matches canonicalized plan payload.         |
+----------------------------------------------------------------------------------------------------+
```

---

## 4. Immutable vs. Mutable Plan Attributes

To prevent Time-of-Check to Time-of-Use (TOCTOU) exploits and accidental session corruption:
- **Strictly Immutable During Session:** Strategy types, implementation versions, dependency topology, authorization tokens, and target game build hashes. Any change requires full session teardown.
- **Controlled Mutable via Transition:** Dynamic degraded-mode flags, logging verbosity, and backpressure rate limits.
