# Transformed-Artifact Eligibility & Final Validation Contract

**Document Identifier:** `NLE-CON-AEF-001`  
**Classification:** Technical Contract & Layered Validation Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-CON-TOH-001`, `NLE-POL-TVC-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Layered Validation Architecture

Before an artifact transformed in Stage H can be admitted into a package definition in Stage K, it must pass a comprehensive five-layer validation check:

```
[Layer A: Identity & Integrity] --------> Hash matches attestation; revision immutable
             |
             v
[Layer B: Format & Structure] ----------> Container syntax valid; chunks conform to spec
             |
             v
[Layer C: Semantic & Target Match] -----> Metric tolerance <= 0.001; target build matches
             |
             v
[Layer D: Policy & Authorization] ------> Gate G5 (Local Package) granted; license permits
             |
             v
[Layer E: Freshness & Stale Check] -----> Inputs, definitions, and schemas unmutated
```

---

## 2. Artifact Eligibility Input Schema (`ArtifactEligibilityInput`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/artifact-eligibility-input.json",
  "artifact_id": "out-nif-celestial-blade-001",
  "revision_id": "rev-20261119-02",
  "content_integrity": {
    "sha256": "9a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b",
    "byte_size": 1845200
  },
  "format_mime": "application/x-nif-mesh",
  "format_version": "Gamebryo_v20.2.0.7",
  "target_context": {
    "canonical_game_id": "game-skyrim-se",
    "target_build": "1.6.1170.0",
    "supported_mechanism": "MOD_DIRECTORY_DROP"
  },
  "provenance_ref": "trf-prv-20261119-001",
  "authorization_gate_g5_token": "auth-g5-tok-20261119-01",
  "license_spdx": "CC-BY-4.0",
  "validation_report_ref": "val-rep-stage-i-001"
}
```

---

## 3. Eligibility Rules & Mandatory Failures

1. **Hash Mismatch (`ERR_ELIG_INTEGRITY_MISMATCH`):** If the computed SHA-256 of the artifact bytes does not match the manifest record, the artifact is rejected.
2. **Missing Gate G5 Authorization (`ERR_ELIG_AUTH_DENIED`):** If the task contract does not explicitly permit packaging (Gate G5), inclusion is blocked.
3. **Stale Transformation Output (`ERR_ELIG_STALE_EVIDENCE`):** If the upstream source asset or transformation definition has changed since validation, the artifact is marked stale and must be re-evaluated.
4. **Unsupported Target Build (`ERR_ELIG_TARGET_INCOMPATIBLE`):** If the artifact was compiled for a different game build or engine revision, it is rejected.
5. **Unresolved Critical Loss (`ERR_ELIG_LOSS_UNRESOLVED`):** If the transformation logged a Tier 3 lossiness record without explicit user approval, the artifact is blocked.

---

## 4. Eligibility Result Schema (`ArtifactEligibilityResult`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/artifact-eligibility-result.json",
  "result_id": "res-elig-20261119-001",
  "artifact_id": "out-nif-celestial-blade-001",
  "revision_id": "rev-20261119-02",
  "evaluation_layers": {
    "layer_a_identity_integrity": "PASSED",
    "layer_b_format_structure": "PASSED",
    "layer_c_semantic_compatibility": "PASSED",
    "layer_d_policy_authorization": "PASSED",
    "layer_e_freshness": "PASSED"
  },
  "overall_status": "ELIGIBLE_FOR_PACKAGE_MEMBERSHIP",
  "evaluated_at": "2026-11-19T18:40:00Z"
}
```
