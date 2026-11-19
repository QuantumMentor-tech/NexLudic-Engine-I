# Package Manifest Schema & Compatibility Policy

**Document Identifier:** `NLE-POL-PMS-001`  
**Classification:** Technical Contract & Schema Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-CON-AEF-001`, `NLE-CON-CPM-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Authoritative Manifest Purpose

The Package Manifest (`manifest.json`) is the single authoritative contract defining a NexLudic Content Package (`.nlpkg`). It binds package identity, target game compatibility, member artifacts, dependency resolutions, provenance attestations, and cryptographic checksums into an immutable, verifiable document.

---

## 2. Complete Package Manifest Schema (`package-manifest.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/content-package-manifest.json",
  "package_id": "pkg.nexludic.skyrim.celestial-armory",
  "version": "1.2.0",
  "package_schema_version": "1.0.0",
  "manifest_format_version": "1.0.0",
  "display_name": "Celestial Armory Weapon Pack",
  "description": "High-poly celestial blade and bow with custom PBR textures.",
  "author": "NexLudic Community Team",
  "license_spdx": "CC-BY-4.0",
  "target_games": [
    {
      "canonical_game_id": "game-skyrim-se",
      "minimum_build": "1.6.0.0",
      "maximum_build": "1.6.1170.0",
      "required_mechanisms": ["MOD_DIRECTORY_DROP"]
    }
  ],
  "required_capabilities": [
    "CAP_TEXTURE_BC7",
    "CAP_MESH_SKINNING"
  ],
  "members": [
    {
      "artifact_id": "out-nif-celestial-blade-001",
      "revision_id": "rev-20261119-02",
      "target_relative_path": "Meshes/Weapons/Celestial/blade.nif",
      "format_mime": "application/x-nif-mesh",
      "sha256": "9a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b",
      "byte_size": 1845200,
      "provenance_ref": "trf-prv-20261119-001",
      "validation_report_ref": "val-rep-stage-i-001",
      "is_optional": false
    },
    {
      "artifact_id": "out-dds-celestial-blade-diffuse-001",
      "revision_id": "rev-20261119-01",
      "target_relative_path": "Textures/Weapons/Celestial/blade_d.dds",
      "format_mime": "image/vnd-ms.dds",
      "sha256": "3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d",
      "byte_size": 4194304,
      "provenance_ref": "trf-prv-20261119-002",
      "validation_report_ref": "val-rep-stage-i-002",
      "is_optional": false
    }
  ],
  "dependencies": [
    {
      "declaration_id": "dep-shared-materials",
      "package_id": "pkg.nexludic.core.shared-materials",
      "version_constraint": "^2.0.0",
      "resolved_version": "2.1.4",
      "is_optional": false
    }
  ],
  "conflicts": [
    {
      "conflicting_package_id": "pkg.thirdparty.old-skyrim-weapons",
      "reason": "Overlapping FormID declarations and mesh sound cues."
    }
  ],
  "package_integrity": {
    "algorithm": "SHA-256",
    "manifest_digest": "7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a"
  }
}
```

---

## 3. Forward Compatibility & Unknown Field Rules

To prevent unsafe forward-parsing of future manifest versions:
1. **Unknown Optional Fields:** Consumers encountering unrecognized fields not prefixed with `x_` must log a warning and preserve them without modification.
2. **Unknown Mandatory Fields:** Any field in the root schema declaring `"mandatory": true` that is unrecognized by the validator causes Stage L validation to FAIL closed (`ERR_PMS_UNKNOWN_MANDATORY_FIELD`).
3. **Schema Migrations:** Manifest schema upgrades require explicit migration transformers; silent schema coercion is prohibited.
