# Content Transformation Determinism, Reproducibility & Provenance Policy

**Document Identifier:** `NLE-POL-TDP-001`  
**Classification:** Quality Assurance & Reproducibility Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-MOD-TRF-001`, `NLE-POL-API-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Determinism Principles

In an automated content pipeline, running the same transformation on the same input assets with the same configuration must yield predictable, repeatable results. This policy codifies determinism tiers, transformation provenance schemas, and semantic output comparison rules.

---

## 2. Three Determinism Classifications

| Classification | Definition | Verification Standard | Example |
| :--- | :--- | :--- | :--- |
| **Bit-Exact Reproducible** | Identical input bytes always produce identical output bytes. | SHA-256 match ($100\%$) | glTF to NIF mesh compiler, PNG decode |
| **Numerically Stable** | Minor floating-point rounding variations within bounds ($\epsilon \le 10^{-5}$). | Geometry distance tolerance $\le 0.0001$ | Coordinate normalization, matrix transforms |
| **Non-Deterministic** | Heuristic or model-based variation (REQUIRES LOGGED SEED). | Semantic equivalence review | Procedural collision decomposition (V-HACD) |

---

## 3. Transformation Provenance Dossier Schema (`transformation-provenance.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/transformation-provenance.json",
  "provenance_id": "trf-prv-20261119-001",
  "output_artifact_id": "out-nif-celestial-blade-001",
  "transformation_definition_id": "trf.gltf-to-skyrim-nif.v1",
  "transformation_version": "1.2.0",
  "adapter_identity": {
    "adapter_id": "adapter-skyrim-creation-engine",
    "adapter_version": "1.4.0",
    "binary_digest_sha256": "5e4d...1a2b"
  },
  "source_inputs": [
    {
      "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
      "revision_id": "rev-20261119-02",
      "sha256": "8d3e91f0a2b4c6e8d1a3b5c7e9f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8"
    }
  ],
  "applied_parameters": {
    "scale_factor": 1.0,
    "generate_tangents": true
  },
  "execution_environment": {
    "engine_version": "0.1.0-alpha",
    "os": "Windows 11 x86_64",
    "timestamp": "2026-11-19T18:22:10Z"
  },
  "output_digest_sha256": "9a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b"
}
```
