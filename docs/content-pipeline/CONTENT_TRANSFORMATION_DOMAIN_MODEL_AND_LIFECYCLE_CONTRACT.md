# Content Transformation Domain Model & Lifecycle Contract

**Document Identifier:** `NLE-MOD-TRF-001`  
**Classification:** Technical Data Contract & Domain Model Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-MOD-CDR-001`, `NLE-CON-CPS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Domain Entities & Conceptual Architecture

The transformation subsystem models seven interacting entity classes that represent the transformation capability, the execution request, the unit of execution, intermediate artifacts, semantic mapping rules, generated outputs, and lifecycle decisions.

```
+----------------------------------+          +----------------------------------+
|      TransformationDefinition    | 1      * |      TransformationRequest       |
| (def_id, adapter_ref, formats)   |<-------->| (request_id, task_ref, target)   |
+----------------------------------+          +----------------------------------+
                 |                                             | 1
                 | 1                                           | executes as
                 v *                                           v *
+----------------------------------+          +----------------------------------+
|       MappingSpecification       |          |        TransformationJob         |
| (mapping_id, source/target paths)|          | (job_id, attempts[], status)     |
+----------------------------------+          +----------------------------------+
                 |                                             | 1
                 v 1                                           | generates
+----------------------------------+                           v *
|   IntermediateRepresentation     |          +----------------------------------+
| (schema_id, canonical glTF/EXR)  |          |   TransformationOutputArtifact   |
+----------------------------------+          | (artifact_id, format, hash, rev) |
                                              +----------------------------------+
                                                               | 1
                                                               | receives
                                                               v 1
                                              +----------------------------------+
                                              |    TransformationDecisionResult  |
                                              | (decision_id, eligibility, state)|
                                              +----------------------------------+
```

---

## 2. Transformation Definition Schema (`transformation-definition.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/transformation-definition.json",
  "definition_id": "trf.gltf-to-skyrim-nif.v1",
  "name": "glTF 2.0 to Creation Engine NIF Mesh Transcoder",
  "version": "1.2.0",
  "adapter_binding": {
    "adapter_id": "adapter-skyrim-creation-engine",
    "minimum_adapter_version": "1.4.0",
    "required_capabilities": ["CAP_TRANSCODE_NIF_V20", "CAP_TEXTURE_DDS_BC7"]
  },
  "source_representation": {
    "format_mime": "model/gltf-binary",
    "supported_versions": ["2.0"],
    "asset_category": "3D_MESH"
  },
  "target_representation": {
    "format_mime": "application/x-nif-mesh",
    "supported_versions": ["Gamebryo_v20.2.0.7"],
    "target_game_id": "game-skyrim-se"
  },
  "lossiness_classification": "LOSSLESS_GEOMETRY_BOUNDED",
  "determinism_classification": "BIT_EXACT_REPRODUCIBLE",
  "resource_limits": {
    "max_memory_mb": 2048,
    "timeout_seconds": 120
  },
  "approval_state": "APPROVED_STANDARD"
}
```

---

## 3. Transformation Request Schema (`TransformationRequest`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/transformation-request.json",
  "request_id": "req-trf-20261119-001",
  "task_ref": "task-mesh-import-skyrim-weapons",
  "plan_ref": "plan-mod-skyrim-weapons-v1",
  "source_asset_ref": {
    "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
    "revision_id": "rev-20261119-02",
    "content_hash_sha256": "8d3e91f0a2b4c6e8d1a3b5c7e9f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8"
  },
  "transformation_definition_id": "trf.gltf-to-skyrim-nif.v1",
  "mapping_specification_id": "map.gltf-pbr-to-skyrim-bslighting.v1",
  "target_environment": {
    "canonical_game_id": "game-skyrim-se",
    "target_build": "1.6.1170.0",
    "selected_mechanism": "MOD_DIRECTORY_DROP"
  },
  "parameters": {
    "generate_collision": true,
    "texture_prefix": "Textures/Weapons/Celestial/"
  },
  "authorization_token": "auth-tok-g4-20261119-c4d29a",
  "created_at": "2026-11-19T18:20:00Z"
}
```

---

## 4. Transformation Output Artifact Schema (`TransformationOutputArtifact`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/transformation-output-artifact.json",
  "output_artifact_id": "out-nif-celestial-blade-001",
  "request_id": "req-trf-20261119-001",
  "source_asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "source_revision_id": "rev-20261119-02",
  "format_mime": "application/x-nif-mesh",
  "format_version": "Gamebryo_v20.2.0.7",
  "staged_path": ".staging/transcode_run_001/meshes/celestial_blade.nif",
  "content_integrity": {
    "sha256": "9a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b",
    "byte_size": 1845200
  },
  "semantic_validation_summary": {
    "geometry_conformance": "PASSED",
    "metric_deviation_max": 0.0004,
    "material_slots_resolved": 2,
    "collision_mesh_present": true
  },
  "loss_accounting": {
    "has_information_loss": false,
    "unmapped_properties": []
  },
  "target_compatibility": {
    "target_game_id": "game-skyrim-se",
    "compatibility_status": "COMPATIBLE_VERIFIED"
  },
  "generated_at": "2026-11-19T18:22:10Z"
}
```

---

## 5. Lifecycle State Transitions

```
[REQUESTED] ---> [ELIGIBILITY_VERIFIED] ---> [AUTHORIZED] ---> [TRANSCODING]
                                                                     |
         +-----------------------------------------------------------+
         |
         v (Success)
  [OUTPUT_GENERATED] ---> [STRUCTURAL_VALIDATION] ---> [SEMANTIC_VALIDATION]
                                                               |
                                                               v (Passed)
                                                      [TARGET_COMPATIBLE]
                                                               |
                                                               v
                                                      [ACCEPTED_DOWNSTREAM]
```

### Terminal Failure States:
- `TRANSCODE_ABORTED`: Worker process crashed or timed out.
- `STRUCTURALLY_INVALID`: Binary syntax or chunk length check failed.
- `SEMANTICALLY_INVALID`: Coordinate bounds or metric deviations exceeded limits.
- `INCOMPATIBLE_BLOCKED`: Failed target engine compatibility check.
