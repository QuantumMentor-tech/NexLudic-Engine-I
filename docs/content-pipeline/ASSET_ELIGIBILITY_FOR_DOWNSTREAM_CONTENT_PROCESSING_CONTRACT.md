# Asset Eligibility for Downstream Content Processing Contract

**Document Identifier:** `NLE-CON-AEP-001`  
**Classification:** Technical Contract & State Transition Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-CON-CPS-001`, `NLE-POL-ALO-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Eligibility Philosophy

Asset discovery and validation do not automatically entitle an asset to proceed into downstream transformation (Stage H), packaging (Stage K), or host installation (Stage N). 

Eligibility is an **operation-specific, evidence-gated decision** rendered by the engine at the boundary between Stage F (Format Resolution) and Stage G (Transformation Planning).

---

## 2. Twelve Dimension Eligibility Matrix

An asset is evaluated across twelve independent criteria before downstream clearance:

```
[1. Discovery Valid] --------> Source is authorized; candidate was legitimately found.
[2. Identity Assigned] -----> Stable canonical asset_id and revision_id generated.
[3. Metadata Complete] -----> All required category fields parsed and normalized.
[4. Structural Valid] ------> Header, chunks, and grammar pass binary validation.
[5. Provenance Verified] ---> Source origin, author, and revision chain authenticated.
[6. Trust Established] -----> Assigned Tier 1, 2, or 3 with appropriate boundaries.
[7. Licensing Cleared] -----> SPDX license permits the requested downstream operation.
[8. Task Authorized] -------> Task contract explicitly scopes this asset for inclusion.
[9. Target Compatible] -----> Target engine/game build confirmed compatible.
[10. Dependencies Met] -----> All non-optional dependencies resolved in DAG.
[11. Integrity Match] ------> SHA-256 matches manifest attestation bit-for-bit.
[12. Security Clean] -------> Zero path traversal, malware signatures, or bad AST calls.
```

---

## 3. Eligibility Decision Schema (`AssetEligibilityDecision`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/asset-eligibility-decision.json",
  "decision_id": "dec-elig-20261119-001",
  "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "revision_id": "rev-20261119-02",
  "target_pipeline_stage": "STAGE_G_TRANSFORMATION_PLANNING",
  "intended_operation": "OP_TRANSCODE_TO_NIF",
  "criteria_evaluation": {
    "discovery_valid": true,
    "identity_assigned": true,
    "metadata_complete": true,
    "structural_valid": true,
    "provenance_verified": true,
    "trust_established": true,
    "licensing_cleared": true,
    "task_authorized": true,
    "target_compatible": true,
    "dependencies_met": true,
    "integrity_match": true,
    "security_clean": true
  },
  "overall_eligibility": "ELIGIBLE_FOR_DOWNSTREAM_PROCESSING",
  "decision_timestamp": "2026-11-19T18:05:00Z",
  "rationale": "All 12 validation dimensions verified with Tier 1 internal provenance."
}
```

---

## 4. Eligibility State Machine & Transitions

```
               [IDENTIFIED_CANDIDATE]
                         |
             +-----------+-----------+
             |                       |
      (Checks Passed)         (Checks Failed)
             |                       |
             v                       v
      [VALIDATED_ASSET]         [REJECTED_ASSET]
             |
    +--------+--------+
    |                 |
(Licensing /      (Licensing /
Auth Cleared)     Auth Missing)
    |                 |
    v                 v
[ELIGIBLE_FOR_OP]  [NEEDS_REVIEW_BLOCKED]
```

### Invariant:
No asset in state `NEEDS_REVIEW_BLOCKED`, `REJECTED_ASSET`, or `UNSUPPORTED_FORMAT` may be passed to Stage G or Stage H.
