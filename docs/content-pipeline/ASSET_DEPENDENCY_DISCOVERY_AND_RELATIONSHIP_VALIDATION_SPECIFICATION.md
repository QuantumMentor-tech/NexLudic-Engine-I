# Asset Dependency Discovery & Relationship Validation Specification

**Document Identifier:** `NLE-SPC-ADD-001`  
**Classification:** Technical Architecture & Relationship Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-MOD-CDR-001`, `NLE-CON-CPM-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Dependency Scope

Modern game assets rarely exist in isolation. A 3D mesh references textures and materials; a scene references meshes, audio triggers, and navigation meshes; a weapon mod references skeleton rigs and projectile data.

This specification governs how Stage C and E discover, extract, and validate asset dependency references without automatically trusting external declarations or executing uncontrolled recursive downloads.

---

## 2. Dependency Record Schema (`AssetDependencyRecord`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/asset-dependency-record.json",
  "dependency_id": "dep-blade-texture-diffuse",
  "referencing_asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "referenced_target": {
    "target_asset_id": "asset.nexludic.skyrim.texture.celestial-blade-diffuse",
    "target_category": "TEXTURE_2D",
    "expected_format": "image/vnd-ms.dds"
  },
  "dependency_type": "REQUIRED_MATERIAL_BINDING",
  "version_constraint": "^1.0.0",
  "declaration_source": "MESH_MATERIAL_CHUNK_PARSER",
  "resolution_status": "RESOLVED_LOCAL",
  "compatibility_status": "COMPATIBLE_VERIFIED"
}
```

---

## 3. Dependency Taxonomy & Classification

| Category | Code | Description | Missing Reaction |
| :--- | :--- | :--- | :--- |
| **Critical Mesh Binding** | `DEP_CRITICAL_GEOMETRY` | Skeleton rig, base physics mesh. | Fatal (`ERR_DEP_UNRESOLVED`) |
| **Material/Texture Binding**| `DEP_TEXTURE_BINDING` | Albedo, Normal, Roughness/Metallic maps. | Fatal or Fallback to Default Mat |
| **Audio SFX Reference** | `DEP_AUDIO_EVENT` | Sound cue on impact or swing. | Degraded (Mute sound cue) |
| **Animation Sequence** | `DEP_ANIMATION` | Attack animation, holster motion. | Fallback to Vanilla Rig |
| **Shared Base Mod** | `DEP_UPSTREAM_PACKAGE` | Framework mod (e.g. RaceMenu, SKSE). | Fatal (`ERR_DEP_PACKAGE_MISSING`)|

---

## 4. Directed Acyclic Graph (DAG) Validation & Cycle Prevention

All asset dependency chains must form a strict Directed Acyclic Graph (DAG).

```
   [Celestial Weapon Pack]
              |
       +------+------+
       v             v
[Celestial Blade] [Celestial Bow]
       |             |
       +------+------+
              v
[Celestial Shared Texture Atlas]  <-- (Diamond Resolution: Permitted)
```

### Cycle Detection Algorithm (`ALG-DAG-CYCLE`):
1. Construct directed graph $G = (V, E)$ where vertices $V$ represent assets and directed edges $E = (u, v)$ represent "$u$ depends on $v$".
2. Execute depth-first search (DFS) with three-color node tracking (`WHITE = unvisited`, `GRAY = visiting`, `BLACK = visited`).
3. If a DFS traversal encounters a `GRAY` node, a back-edge is detected, representing a **circular dependency loop**.
4. Discovery terminates with error code `ERR_DEP_CYCLE_DETECTED`, listing the exact cycle cycle path:
   $$A \rightarrow B \rightarrow C \rightarrow A$$
