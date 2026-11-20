# CROSS-GAME RESOLUTION POLICY FRAMEWORK AND ELIGIBILITY CONTRACT

**Document Identifier:** `NLE-CON-CRF-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Policy Governance  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-CAA-001` (Step 76), `NLE-POL-CEC-001` (Step 76)  
**Downstream Dependents:** `NLE-SPC-RSC-001` (Step 76), `NLE-POL-MSI-001` (Step 76)

---

## 1. Purpose and Scope

This contract defines the schema, evaluation criteria, precedence rules, and eligibility gates for **Resolution Policies**.

A Resolution Policy defines which strategy is authorized to resolve conflicts within a specific domain under defined conditions. Before any state mutation can be attempted, the candidate resolution must be matched against an approved, eligible policy.

---

## 2. Resolution Policy Schema

A valid Resolution Policy conforms to the following formal JSON Schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/resolution-policy-v1.json",
  "title": "ResolutionPolicyDefinition",
  "type": "object",
  "required": [
    "policy_id",
    "policy_version",
    "target_domain_id",
    "permitted_conflict_categories",
    "strategy_family",
    "execution_mode",
    "required_authority_model",
    "reversibility_tier",
    "preconditions",
    "invariants",
    "integrity_hash"
  ],
  "properties": {
    "policy_id": { "type": "string" },
    "policy_version": { "type": "string", "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$" },
    "target_domain_id": { "type": "string" },
    "permitted_conflict_categories": {
      "type": "array",
      "items": {
        "type": "string",
        "enum": [
          "AUTHORITY_CONFLICT",
          "CONCURRENT_WRITE_CONFLICT",
          "STALE_WRITE_CONFLICT",
          "SEMANTIC_CONFLICT",
          "INVARIANT_CONFLICT",
          "PARTIAL_APPLICATION_CONFLICT"
        ]
      }
    },
    "strategy_family": {
      "type": "string",
      "enum": [
        "AUTHORITATIVE_SOURCE_OVERRIDE",
        "CONDITIONAL_REPLACEMENT",
        "FIELD_LEVEL_INDEPENDENT",
        "DETERMINISTIC_MERGE",
        "COMMUTATIVE_MONOTONIC",
        "SOURCE_DIRECTED_REPLAY",
        "FRESH_SNAPSHOT_RESET",
        "MANUAL_ESCALATION",
        "REFUSAL_AND_CONTAINMENT"
      ]
    },
    "execution_mode": {
      "type": "string",
      "enum": ["FULLY_AUTOMATED", "CONDITIONAL_AUTOMATED", "REQUIRES_HUMAN_APPROVAL", "STRICTLY_PROHIBITED"]
    },
    "required_authority_model": { "type": "string" },
    "reversibility_tier": {
      "type": "string",
      "enum": ["REVERSIBLE", "COMPENSATABLE", "IRREVERSIBLE", "REVERSIBILITY_UNKNOWN"]
    },
    "preconditions": {
      "type": "object",
      "required": ["require_matching_epoch", "require_target_revision_check"],
      "properties": {
        "require_matching_epoch": { "type": "boolean" },
        "require_target_revision_check": { "type": "boolean" },
        "max_allowable_staleness_ms": { "type": "integer" }
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

## 3. Strict Policy Eligibility Evaluation Gates

A policy is deemed **ELIGIBLE** for a detected conflict if and only if all eight gates pass:

```
+----------------------------------------------------------------------------------------------------+
| 1. DOMAIN & CATEGORY MATCH: Policy covers target_domain_id and detected conflict category.         |
+----------------------------------------------------------------------------------------------------+
| 2. ACTIVE SESSION VALIDITY: Policy is active and approved in NegotiatedSessionContract.            |
+----------------------------------------------------------------------------------------------------+
| 3. AUTHORITY CONFORMANCE: Competing endpoints satisfy policy's required_authority_model.           |
+----------------------------------------------------------------------------------------------------+
| 4. REVISION PRECONDITIONS: Current target revision matches policy's expected revision bounds.     |
+----------------------------------------------------------------------------------------------------+
| 5. CAPABILITY VERIFICATION: Target adapter has verified capability for the strategy operation.     |
+----------------------------------------------------------------------------------------------------+
| 6. FRESHNESS VALIDATION: Conflict age does not exceed max_allowable_staleness_ms.                  |
+----------------------------------------------------------------------------------------------------+
| 7. INVARIANT EVALUABILITY: All declared domain invariants can be evaluated with available data.     |
+----------------------------------------------------------------------------------------------------+
| 8. AUTHORIZATION TIER SATISFIED: If mode == REQUIRES_HUMAN_APPROVAL, valid signature is present.  |
+----------------------------------------------------------------------------------------------------+
```

If any gate fails, the policy is classified as `INELIGIBLE`.

---

## 4. Policy Precedence and Safe Ambiguity Resolution

When multiple policies appear applicable to a conflict:
1. **Specificity Precedence:** Field-level policies override Concept-level policies, which override Domain-level policies.
2. **Conservative Mode Precedence:** If policies clash, `REQUIRES_HUMAN_APPROVAL` overrides `CONDITIONAL_AUTOMATED`, which overrides `FULLY_AUTOMATED`.
3. **Ambiguity Default:** If two equally specific policies prescribe contradictory strategies, **neither is executed**. The coordinator emits `ERR_CONFLICT_POLICY_AMBIGUITY` and falls back to `REFUSAL_AND_CONTAINMENT`.
