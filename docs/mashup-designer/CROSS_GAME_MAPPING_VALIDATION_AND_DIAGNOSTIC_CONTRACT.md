# Cross-Game Mapping Validation & Diagnostic Contract

**Document ID:** `NLE-CON-MVD-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED CONTRACT SPECIFICATION`  
**Phase:** `Phase 6 of 12 — Mashup Designer & Integration Planning`  
**Step:** `Step 47 of 100`  
**Classification:** `Mapping Quality Gates, Validation Pipeline & Diagnostics Contract`  
**Related Documents:**
- [CROSS_GAME_DATA_MAPPING_AND_TRANSFORMATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_DATA_MAPPING_AND_TRANSFORMATION_SPECIFICATION.md) (`NLE-SPC-DMT-001`)
- [CROSS_GAME_MAPPING_DOMAIN_MODEL_AND_FIELD_DICTIONARY.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_MAPPING_DOMAIN_MODEL_AND_FIELD_DICTIONARY.md) (`NLE-MOD-DMD-001`)
- [CROSS_GAME_TRANSFORMATION_CONTRACT_AND_COMPOSITION_RULES.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_TRANSFORMATION_CONTRACT_AND_COMPOSITION_RULES.md) (`NLE-RUL-TCC-001`)

---

## 1. The 10-Layer Mapping Validation Pipeline

Before any mapping specification can be marked `VALIDATED` or approved for downstream integration planning, it must sequentially satisfy ten automated **Mapping Quality Gates (`GATE-MAP-01..10`)**:

```mermaid
flowchart TD
    In["Draft Data Mapping Ingestion"] --> G1{"Gate 1: Structural Conformance<br/>(Schema NLE-SCH-DMT-001)"}
    G1 -->|"Fail"| F1["Status: STRUCTURALLY_INVALID"]
    
    G1 -->|"Pass"| G2{"Gate 2: Concept Reference Resolution<br/>(Validates Against Canonical Catalog)"}
    G2 -->|"Fail"| F2["Status: INCOMPLETE (Unresolved Concept)"]
    
    G2 -->|"Pass"| G3{"Gate 3: Type Compatibility Check<br/>(Signatures Match Transformation)"}
    G3 -->|"Fail"| F3["Status: TYPE_INCOMPATIBLE"]
    
    G3 -->|"Pass"| G4{"Gate 4: Semantic Non-Equivalence Check<br/>(Rejects False Friends RUL-SCC-02)"}
    G4 -->|"Fail"| F4["Status: NEEDS_CLARIFICATION"]
    
    G4 -->|"Pass"| G5{"Gate 5: Numeric Range & Unit Invariant<br/>(Normalizations Mathematically Bounded)"}
    G5 -->|"Fail"| F5["Status: CONSTRAINT_VIOLATED"]
    
    G5 -->|"Pass"| G6{"Gate 6: Mandatory Requirement Alignment<br/>(All Mandatory Properties Mapped)"}
    G6 -->|"Fail"| F6["Status: INCOMPLETE_REQUIREMENT_DEFICIT"]
    
    G6 -->|"Pass"| G7{"Gate 7: Evidence Provenance & Freshness<br/>(Attested by Phase 4/5 Records < 90 Days)"}
    G7 -->|"Fail"| F7["Status: INSUFFICIENT_EVIDENCE"]
    
    G7 -->|"Pass"| G8{"Gate 8: Information Loss Disclosure Gate<br/>(Lossy / Dropped Properties Accepted)"}
    G8 -->|"Fail"| F8["Status: UNAPPROVED_FIDELITY_LOSS"]
    
    G8 -->|"Pass"| G9{"Gate 9: Security & Sandbox Invariant<br/>(Medium IL, Zero Kernel Hooks BLK-SEC-01)"}
    G9 -->|"Fail"| F9["Status: BLOCKED_SECURITY_VIOLATION"]
    
    G9 -->|"Pass"| G10{"Gate 10: Dependency Acyclicity Gate<br/>(Transformation Chains are DAGs)"}
    G10 -->|"Fail"| F10["Status: CIRCULAR_DEPENDENCY"]
    
    G10 -->|"All Passed"| Success["Status: VALIDATED<br/>(Eligible for Creator Review & Planning Approval)"]

    style In fill:#f7fafc,stroke:#4a5568,stroke-width:1px
    style Success fill:#f0fff4,stroke:#22543d,stroke-width:2px
    style F1 fill:#fff5f5,stroke:#9b2c2c,stroke-width:1px
    style F6 fill:#fff5f5,stroke:#e53e3e,stroke-width:1px
    style F9 fill:#742a2a,color:#fff,stroke:#e53e3e,stroke-width:2px
```

---

## 2. Detailed Quality Gate Invariants

| Gate ID | Gate Name | Automated Verification Check | Rejection Code / Action |
| :--- | :--- | :--- | :--- |
| **`GATE-MAP-01`** | Structural Conformance | Full JSON validation against `NLE-SCH-DMT-001`. | `ERR_SCHEMA_VALIDATION_FAILED` |
| **`GATE-MAP-02`** | Concept Reference Resolution | Source and target concepts resolve in authoritative catalog. | `ERR_CONCEPT_REFERENCE_UNRESOLVED` |
| **`GATE-MAP-03`** | Type Compatibility | Source type matches transformation input; output matches target. | `ERR_TYPE_SIGNATURE_MISMATCH` |
| **`GATE-MAP-04`** | Semantic Non-Equivalence | Validates that identical names do not hide divergent mechanics. | `ERR_SEMANTIC_FALSE_FRIEND` |
| **`GATE-MAP-05`** | Constraint & Range Bounds | Output values mathematically bounded; units recognized in SI catalog. | `ERR_VALUE_OUT_OF_BOUNDS` |
| **`GATE-MAP-06`** | Requirement Alignment | Every mandatory requirement property achieves `EXACT` or `ADAPTED`. | `ERR_MANDATORY_PROPERTY_DROPPED` |
| **`GATE-MAP-07`** | Evidence Provenance | Supporting citations verified and fresh ($< 90\text{ days}$). | `ERR_EVIDENCE_STALE_OR_MISSING` |
| **`GATE-MAP-08`** | Loss Disclosure | Creator has accepted any lossy precision or dropped optional fields. | `ERR_UNACKNOWLEDGED_DATA_LOSS` |
| **`GATE-MAP-09`** | Security & Anti-Cheat | Zero kernel hooks; Medium IL sandboxing enforced (`BLK-SEC-01`). | `ERR_PROHIBITED_SECURITY_ASSUMPTION` |
| **`GATE-MAP-10`** | Transformation Acyclicity | Chained transformations form a Directed Acyclic Graph (Kahn DAG). | `ERR_TRANSFORMATION_CYCLE_DETECTED` |

---

## 3. Structured Mapping Diagnostic Schema

When validation detects an error, warning, or deficit, the validator outputs a structured diagnostic record conforming to the following schema:

```json
{
  "diagnostic_id": "DIAG_MAP_20261118_001",
  "mapping_id": "b3e2140a-5c12-4e89-9a74-d4b94f1c7e92",
  "property_mapping_id": "PMAP_ATLAS_RADIATION_TO_ORION",
  "gate_id": "GATE-MAP-06",
  "severity": "FATAL_BLOCKER",
  "error_code": "ERR_MANDATORY_PROPERTY_DROPPED",
  "summary": "Mandatory property 'radiation_level' cannot be dropped without creator consent",
  "detailed_explanation": "Requirement REQ-SURVIVAL-01 mandates that environmental hazards from Game Atlas must influence Game Orion. The target concept lacks a radiation property and no transformation contract was specified.",
  "source_context": {
    "concept_id": "CONCEPT_ATLAS_ATMOSPHERIC_HAZARD",
    "property_name": "radiation_level",
    "declared_type": "FLOAT32"
  },
  "target_context": {
    "concept_id": "CONCEPT_ORION_ENVIRONMENTAL_DEBUFF",
    "property_name": null,
    "declared_type": null
  },
  "remediation_guidance": "Specify an adapted transformation mapping radiation_level to Game Orion's 'poison_damage_per_sec' or adjust Requirement REQ-SURVIVAL-01 to mark radiation as optional."
}
```

---
*End of Contract — Document `NLE-CON-MVD-001`.*
