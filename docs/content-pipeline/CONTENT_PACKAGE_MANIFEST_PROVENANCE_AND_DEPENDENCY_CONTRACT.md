# Content Package Manifest, Provenance & Dependency Contract

**Document Identifier:** `NLE-CON-CPM-001`  
**Classification:** Technical Contract & Schema Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`, `NLE-CON-CPS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Objective

This contract specifies the physical and logical structure of a NexLudic Content Package (`.nlpkg`), its cryptographic manifest, origin provenance records, and dependency resolution grammar. Every modifiable unit distributed or processed in the content pipeline must conform to this schema.

---

## 2. Package Archive Anatomy (`.nlpkg`)

A `.nlpkg` archive is a standard uncompressed or Zstandard-compressed container adhering to the following directory layout:

```
[package-root]/
  |-- manifest.json              <-- Authoritative Signed Manifest (Required)
  |-- provenance.json            <-- Cryptographic Origin & Build Attestation
  |-- license.spdx.json          <-- Machine-readable License Terms
  |-- assets/                    <-- Transcoded Game-Ready Payloads
  |     |-- meshes/
  |     |-- textures/
  |     +-- audio/
  +-- overrides/                 <-- Configuration / Table Overrides
```

---

## 3. Package Manifest Schema (`manifest.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/content-package-manifest.json",
  "package_id": "pkg.nexludic.skyrim.celestial-armory",
  "version": "1.2.0",
  "display_name": "Celestial Armory Weapon Pack",
  "description": "High-poly celestial blade and bow with custom PBR textures.",
  "author": "NexLudic Community Team",
  "target_games": [
    {
      "canonical_game_id": "game-skyrim-se",
      "minimum_build": "1.6.0.0",
      "maximum_build": "1.6.9999.0",
      "required_mechanisms": ["MOD_DIRECTORY_DROP"]
    }
  ],
  "required_capabilities": [
    "CAP_TEXTURE_BC7",
    "CAP_MESH_SKINNING"
  ],
  "dependencies": [
    {
      "package_id": "pkg.nexludic.core.shared-materials",
      "version_requirement": ">=2.0.0 <3.0.0",
      "is_optional": false
    }
  ],
  "conflicts": [
    {
      "package_id": "pkg.thirdparty.old-skyrim-weapons",
      "reason": "Overwrites the same FormIDs and sound descriptors."
    }
  ],
  "content_hash_sha256": "4a7d...39b8",
  "assets": [
    {
      "path": "assets/meshes/celestial_blade.nif",
      "size_bytes": 1048576,
      "sha256": "1f8e...4b2a",
      "mime_type": "application/x-nif-mesh"
    },
    {
      "path": "assets/textures/celestial_blade.dds",
      "size_bytes": 4194304,
      "sha256": "9a3c...5d1e",
      "mime_type": "image/vnd-ms.dds"
    }
  ]
}
```

---

## 4. Provenance Dossier Schema (`provenance.json`)

To prevent supply-chain poisoning and track asset origin:

```json
{
  "$schema": "https://nexludic.org/schemas/v1/package-provenance.json",
  "package_id": "pkg.nexludic.skyrim.celestial-armory",
  "build_timestamp": "2026-11-19T17:20:00Z",
  "pipeline_run_id": "pipe-run-2026-11-19-001",
  "builder_identity": "agent-orchestrator-node-04",
  "source_inputs": [
    {
      "input_id": "src-model-gltf-01",
      "source_repository": "https://assets.nexludic.org/models/celestial.glb",
      "source_commit": "c4d29a1b8e90",
      "source_sha256": "8d3e...7a1b"
    }
  ],
  "transformation_audit": [
    {
      "stage": "STAGE_H_TRANSFORMATION",
      "adapter_id": "adapter-skyrim-v1.4",
      "input_format": "glTF 2.0",
      "output_format": "NIF / DDS",
      "tool_sha256": "3c8a...2e9f"
    }
  ],
  "signature": {
    "key_id": "key-nexludic-official-2026",
    "algorithm": "Ed25519",
    "signature_bytes": "6b2d...9f1a"
  }
}
```

---

## 5. Dependency Grammar & DAG Resolution Rules

1. **Semantic Versioning:** Version requirements follow SemVer 2.0 (`^1.0.0`, `~1.2.0`, `>=1.0.0 <2.0.0`).
2. **Directed Acyclic Graph (DAG):** All package dependencies must form a strict DAG.
3. **Cycle Rejection:** Any dependency loop (e.g. $A \rightarrow B \rightarrow A$) triggers `ERR_DEP_CYCLE_DETECTED` and aborts Stage J.
4. **Diamond Resolution:** If package $A$ depends on $B (\ge 1.0)$ and $C (\ge 1.0)$, and both depend on $D$, the resolver selects the latest compatible version satisfying all range bounds. If no intersection exists, resolution fails with `ERR_DEP_UNRESOLVABLE`.
