# Content Discovery Request & Candidate Domain Model

**Document Identifier:** `NLE-MOD-CDR-001`  
**Classification:** Technical Data Contract & Domain Model Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-CON-CSD-001`, `NLE-CON-CPS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Domain Entities & Conceptual Relationships

The discovery subsystem models four interacting data entities representing the intent to discover, the raw discovered candidate, the normalized canonical asset, and the auditable summary of the discovery run.

```
+-------------------------------------------------------------+
|                      DiscoveryRequest                       |
| (request_id, task_ref, target_game_ref, authorized_sources) |
+-------------------------------------------------------------+
                               | 1
                               | triggers
                               v *
+-------------------------------------------------------------+
|                     DiscoveredCandidate                     |
| (candidate_id, source_ref, locator, prelim_meta, status)    |
+-------------------------------------------------------------+
                               | 1
                               | promoted to
                               v 0..1
+-------------------------------------------------------------+
|                     CanonicalAssetRecord                    |
| (asset_id, logical_id, revision_id, format, provenance)     |
+-------------------------------------------------------------+
                               | *
                               | aggregated into
                               v 1
+-------------------------------------------------------------+
|                       IngestionResult                       |
| (result_id, request_id, candidates_count, validated_count)  |
+-------------------------------------------------------------+
```

---

## 2. Discovery Request Schema (`DiscoveryRequest`)

Defines the bounded parameters under which a discovery operation is authorized to run.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/content-discovery-request.json",
  "request_id": "req-disc-20261119-001",
  "task_ref": "task-mesh-import-skyrim-weapons",
  "plan_ref": "plan-mod-skyrim-weapons-v1",
  "target_game_ref": {
    "canonical_game_id": "game-skyrim-se",
    "installed_instance_id": "inst-skyrim-steam-01"
  },
  "authorized_source_ids": [
    "src-local-skyrim-data",
    "src-project-celestial-models"
  ],
  "required_content_categories": [
    "3D_MESH",
    "TEXTURE_2D"
  ],
  "permitted_scope": {
    "max_files_to_scan": 5000,
    "max_depth": 6,
    "include_patterns": ["*.glb", "*.nif", "*.dds", "*.png"],
    "exclude_patterns": ["*_temp.*", "*.bak"]
  },
  "access_restrictions": {
    "network_access_permitted": false,
    "read_only": true
  },
  "status": "APPROVED_READY"
}
```

---

## 3. Discovered Candidate Schema (`DiscoveredCandidate`)

Represents an individual resource located during an authorized scan, prior to formal validation.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/discovered-candidate.json",
  "candidate_id": "cand-blade-gltf-01",
  "discovery_request_id": "req-disc-20261119-001",
  "source_id": "src-project-celestial-models",
  "source_relative_locator": "weapons/celestial_blade.glb",
  "candidate_type": "FILE",
  "observed_file_size_bytes": 1420580,
  "observed_file_sha256": "8d3e91f0a2b4c6e8d1a3b5c7e9f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8",
  "discovery_method": "FILESYSTEM_DIRECTORY_WALK",
  "discovered_at": "2026-11-19T18:00:00Z",
  "preliminary_metadata": {
    "filename_extension": ".glb",
    "magic_bytes": "676c5446",
    "magic_detected_mime": "model/gltf-binary"
  },
  "validation_status": "PENDING_STRUCTURAL_VALIDATION",
  "provenance_status": "PENDING_PROVENANCE_CHECK"
}
```

---

## 4. Canonical Asset Record Schema (`CanonicalAssetRecord`)

Represents a candidate that has passed identity assignment, structural validation, and provenance checks.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/canonical-asset-record.json",
  "asset_id": "asset.nexludic.skyrim.celestial-blade",
  "logical_identifier": "pkg.nexludic.weapons.celestial-blade",
  "revision_id": "rev-20261119-c4d29a",
  "asset_category": "3D_MESH",
  "format_mime": "model/gltf-binary",
  "format_version": "glTF 2.0",
  "source_ref": {
    "source_id": "src-project-celestial-models",
    "locator": "weapons/celestial_blade.glb"
  },
  "integrity": {
    "sha256": "8d3e91f0a2b4c6e8d1a3b5c7e9f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8",
    "size_bytes": 1420580
  },
  "provenance": {
    "origin_author": "NexLudic Art Core",
    "license_spdx": "CC-BY-4.0",
    "provenance_tier": "TIER_1_PROJECT_INTERNAL",
    "signed_attestation_present": true
  },
  "structural_validation": {
    "status": "VALIDATED_CONFORMANT",
    "geometry_vertex_count": 8450,
    "bounding_box": {
      "min": [-0.15, -0.05, -1.10],
      "max": [0.15, 0.05, 0.20]
    }
  },
  "target_compatibility": {
    "target_game_id": "game-skyrim-se",
    "compatibility_state": "COMPATIBLE_VERIFIED",
    "transcode_adapter_required": "adapter-skyrim-creation-engine"
  },
  "authorization_status": "AUTHORIZED_FOR_TRANSFORMATION",
  "downstream_eligibility": "ELIGIBLE_FOR_STAGE_G"
}
```

---

## 5. Ingestion Result Schema (`IngestionResult`)

Authoritative summary produced at the conclusion of a discovery and validation pass.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/ingestion-result.json",
  "result_id": "res-ingest-20261119-001",
  "request_id": "req-disc-20261119-001",
  "completed_at": "2026-11-19T18:02:15Z",
  "sources_examined": [
    "src-local-skyrim-data",
    "src-project-celestial-models"
  ],
  "summary_metrics": {
    "total_files_scanned": 412,
    "candidates_identified": 24,
    "assets_validated": 22,
    "assets_rejected": 2,
    "unresolved_provenance_count": 0
  },
  "rejections": [
    {
      "candidate_id": "cand-corrupt-shield",
      "reason_code": "ERR_SRC_STRUCTURAL_CORRUPTION",
      "diagnostic": "glTF JSON chunk length mismatch"
    },
    {
      "candidate_id": "cand-thirdparty-sword",
      "reason_code": "ERR_AUTH_LICENSE_UNRESOLVED",
      "diagnostic": "Missing SPDX license declaration in candidate header"
    }
  ],
  "result_status": "COMPLETED_WITH_REJECTIONS"
}
```
