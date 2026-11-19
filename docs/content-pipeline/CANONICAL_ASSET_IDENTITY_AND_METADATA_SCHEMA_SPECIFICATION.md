# Canonical Asset Identity & Metadata Schema Specification

**Document Identifier:** `NLE-SPC-CAI-001`  
**Classification:** Technical Architecture & Schema Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-MOD-CDR-001`, `NLE-SPC-CFT-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Identification Philosophy

In a universal cross-game engine, filenames such as `texture.dds` or `sword.glb` are completely ambiguous. NexLudic Engine enforces a multi-dimensional identity model that separates the **logical asset identity** (what the asset represents) from the **content revision** (the immutable snapshot of specific bytes) and the **source locator** (where it was discovered).

---

## 2. Canonical Identity Grammar

Canonical asset identifiers follow a globally unique, hierarchical, namespaced grammar:

$$\text{asset\_id} = \text{pkg}.\langle\text{namespace}\rangle.\langle\text{game\_family}\rangle.\langle\text{category}\rangle.\langle\text{entity\_name}\rangle$$

### Example Identifiers:
- `asset.nexludic.skyrim.mesh.celestial-blade`
- `asset.community.unreal5.texture.rock-granite-bc7`
- `asset.official.fallout4.audio.laser-fire-stereo`

### Invariants:
1. `asset_id` must match regex: `^[a-z0-9_.-]{10,128}$`.
2. A filename alone MUST NEVER be used as the canonical asset identifier.
3. Logical identity persists across asset revisions; byte mutations produce a new `revision_id`.

---

## 3. Logical Identity vs. Content Revision Model

```
+-------------------------------------------------------------+
|                     Logical Asset Entity                    |
|  asset_id: "asset.nexludic.skyrim.mesh.celestial-blade"    |
|  display_name: "Celestial Blade (High-Poly)"                |
|  category: "3D_MESH"                                        |
+-------------------------------------------------------------+
                 |                           |
                 v                           v
+-------------------------------+ +-------------------------------+
|     Content Revision A        | |     Content Revision B        |
| revision_id: "rev-20261101-01"| | revision_id: "rev-20261119-02"|
| sha256: "8d3e...7a1b"         | | sha256: "4c1f...9b3d"         |
| vertex_count: 8200            | | vertex_count: 8450 (LOD fix)  |
| status: SUPERSEDED            | | status: ACTIVE_HEAD           |
+-------------------------------+ +-------------------------------+
```

---

## 4. Metadata Taxonomy & Processing Levels

The metadata pipeline explicitly classifies metadata by its derivation level:

| Level | Classification | Definition | Example |
| :--- | :--- | :--- | :--- |
| **L0** | Raw Metadata | Extracted verbatim from file headers or source container. | Raw glTF JSON chunk, NIF header bits |
| **L1** | Parsed Metadata | Syntactically validated and converted into typed in-memory objects. | Width: 2048, Height: 2048, Channels: 4 |
| **L2** | Normalized Metadata | Mapped to NexLudic canonical schemas (`NLE-MOD-CDR-001`). | Bounding box $[-0.1, 0.1]$, format `glTF 2.0` |
| **L3** | Derived Metadata | Calculated or computed through static analysis. | Computed tangent space, triangle count |
| **L4** | Inferred Metadata | Heuristically estimated (REQUIRES EXPLICIT FLAG). | Guessing PBR roughness from filename `_rough`|
| **L5** | Verified Metadata | Supported by authoritative signed manifest or test evidence. | Signed build provenance, verified game build |

> [!CAUTION]
> Inferred metadata (L4) must never be promoted to verified status (L5) without independent empirical or cryptographic confirmation.

---

## 5. Metadata Schema: 3D Mesh (`mesh-metadata.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/metadata-mesh.json",
  "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "revision_id": "rev-20261119-02",
  "geometry": {
    "vertex_count": 8450,
    "triangle_count": 16200,
    "submesh_count": 2,
    "has_vertex_normals": true,
    "has_tangents": true,
    "has_vertex_colors": false,
    "uv_channels": 2
  },
  "bounding_volume": {
    "type": "AXIS_ALIGNED_BOX",
    "box_min": [-0.15, -0.05, -1.10],
    "box_max": [0.15, 0.05, 0.20],
    "bounding_sphere_radius": 0.68
  },
  "skeleton": {
    "has_rigging": false,
    "bone_count": 0
  },
  "material_slots": [
    { "slot_index": 0, "material_name": "M_CelestialHilt" },
    { "slot_index": 1, "material_name": "M_CelestialBlade" }
  ]
}
```

---

## 6. Metadata Safety & Anti-Injection Rules

Metadata extracted from untrusted community archives or third-party packages must be sanitized before ingestion:
1. **Length Ceilings:** String fields (name, description, author) capped at 512 characters.
2. **Character Set Whitelisting:** Disallow non-printable ASCII/Unicode control characters (`\x00`–`\x1F`, `\x7F`).
3. **No Dynamic Evaluation:** Metadata strings must never be passed to `eval()`, SQL formatters, shell execution, or unescaped HTML templates.
4. **AI Prompt Injection Defense:** All descriptive metadata ingested for AI coding agents must be stripped of system prompt overrides, XML injection delimiters (`<system>`, `</instructions>`), and role-switching directives.
