# Content Transformation Output Artifact & Downstream Handoff Contract

**Document Identifier:** `NLE-CON-TOH-001`  
**Classification:** Technical Contract & Downstream Handoff Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-CON-CPS-001`, `NLE-CON-CPM-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Downstream Continuity

This contract governs the logical and physical handoff from Stage I (Intermediate Validation) to downstream pipeline stages:
- **Stage J:** Dependency & Compatibility Resolution
- **Stage K:** Package Assembly (`.nlpkg`)
- **Stage L:** Package Integrity & Security Checks

---

## 2. Transformation Handoff Dossier Schema (`transformation-handoff.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/transformation-handoff.json",
  "handoff_id": "hnd-trf-20261119-001",
  "pipeline_run_id": "pipe-run-2026-11-19-001",
  "produced_artifacts": [
    {
      "output_artifact_id": "out-nif-celestial-blade-001",
      "canonical_target_relative_path": "Meshes/Weapons/Celestial/blade.nif",
      "format_mime": "application/x-nif-mesh",
      "sha256": "9a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b",
      "byte_size": 1845200,
      "staged_file_uri": "file:///.staging/transcode_run_001/meshes/celestial_blade.nif",
      "validation_summary": {
        "structural_valid": true,
        "semantic_valid": true,
        "lossiness_tier": "TIER_0_BIT_EXACT"
      }
    }
  ],
  "associated_dependencies": [
    "asset.nexludic.skyrim.texture.celestial-blade-diffuse",
    "asset.nexludic.skyrim.texture.celestial-blade-normal"
  ],
  "target_environment": {
    "canonical_game_id": "game-skyrim-se",
    "target_build": "1.6.1170.0",
    "mechanism": "MOD_DIRECTORY_DROP"
  },
  "downstream_eligibility": "ELIGIBLE_FOR_STAGE_J_DEPENDENCY_RESOLUTION",
  "handoff_timestamp": "2026-11-19T18:25:00Z"
}
```

---

## 3. Strict Installation Boundary Invariant (`INV-TOH-01`)

Successful generation and validation of a transformation output does NOT grant authorization to copy, link, or stage files in a live game installation directory. Physical modification remains strictly governed by Stage M (Pre-Application Review) and Stage N (Application Handoff).
