# Cross-Game Content Semantic Mapping & Lossiness Policy

**Document Identifier:** `NLE-POL-SML-001`  
**Classification:** Quality Assurance & Semantic Mapping Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-CON-FCC-001`, `NLE-SPC-CNI-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Semantic Mapping Challenge

Cross-game mashups frequently attempt to map features between engines with fundamentally different capabilities (e.g. mapping a modern physically-based material with subsurface scattering into an older game engine supporting only legacy Blinn-Phong specular shaders).

This policy codifies semantic mapping rules, lossiness tiers, approximation boundaries, and mandatory loss accounting.

---

## 2. Four Semantic Mapping Categories

```
1. Direct Isomorphic Mapping:
   Source Property <========================================> Target Property
   (e.g. BaseColor Texture RGB -----------------------------> Diffuse Map RGB)

2. Conditional Mapping:
   Source Property ----[If Target Supports Subsurface]-----> Subsurface Profile
                   ----[Else: Fallback to Diffuse]----------> Albedo Tint

3. Approximated Lossy Mapping:
   Source Property (PBR Metallic/Roughness) ---------------> Specular Power / Glossiness
   (Approximation formula applied; loss documented)

4. Unmappable / Rejected Property:
   Source Property (Realtime Raytraced Caustics) ----------> NOT SUPPORTED
   (Action: Explicitly logged as unmapped; requires review if critical)
```

---

## 3. Four Lossiness Tiers

| Tier | Classification | Definition | Approval Required |
| :--- | :--- | :--- | :--- |
| **Tier 0** | Bit-Exact Lossless | Exact bit-for-bit or numerically lossless preservation. | Automated Approval |
| **Tier 1** | Perceptually Lossless | Minor mathematical quantization imperceptible to humans (BC7, 24-bit 48kHz audio). | Automated Approval |
| **Tier 2** | Bounded Lossy | Known approximations (e.g. LOD reduction $< 15\%$, PBR to Blinn-Phong). | Task Policy Sign-off |
| **Tier 3** | High Loss / Critical | Significant feature loss (e.g. dropping skeletal bones, audio channel reduction). | Explicit User Prompt |

---

## 4. Loss Accounting Record Schema (`loss-accounting-record.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/loss-accounting-record.json",
  "record_id": "loss-rec-20261119-001",
  "transformation_request_id": "req-trf-20261119-001",
  "affected_asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "lossiness_tier": "TIER_1_PERCEPTUALLY_LOSSLESS",
  "loss_items": [
    {
      "source_property": "materials[0].roughness_channel",
      "target_limitation": "Creation Engine uses specular glossiness power instead of PBR roughness",
      "applied_approximation_rule": "glossiness = (1.0 - roughness) * 128.0",
      "impact_assessment": "Visual specular highlight slightly wider; geometry unaffected",
      "is_mandatory_property": false,
      "approved_by": "Task_Automated_Policy_v1.0"
    }
  ]
}
```
