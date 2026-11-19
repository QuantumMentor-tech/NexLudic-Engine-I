# Content Format Transformation & Game Adapter Boundary Specification

**Document Identifier:** `NLE-SPC-CFT-001`  
**Classification:** Technical Architecture & Interface Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-CPS-001`, `NLE-SPC-ADA-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Architectural Division of Responsibility

NexLudic Engine enforces a strict, decoupled boundary between:
1. **Universal Core:** Manages asset metadata, canonical intermediate representations (IR), pipeline coordination, dependency graphs, security inspection, and packaging.
2. **Game-Specific Adapters:** Implements proprietary format deserialization, transcoding, optimization, and packaging for specific target engines.

```
+-----------------------------------------------------------------------------------------+
|                                    UNIVERSAL CORE                                       |
|  Canonical Intermediate Formats (glTF 2.0, OpenEXR, PNG, Ogg Vorbis, JSON Schemas)     |
+-----------------------------------------------------------------------------------------+
                               |                                ^
           Encode / Serialize  |                                | Decode / Parse
                               v                                |
+-----------------------------------------------------------------------------------------+
|                              ADAPTER PROTOCOL BOUNDARY                                  |
|   Strict IPC Interface (Protobuf / JSON Wire Protocol, Memory-Safe Sandbox)             |
+-----------------------------------------------------------------------------------------+
                               |                                ^
                               v                                |
+-----------------------------------------------------------------------------------------+
|                                 GAME-SPECIFIC ADAPTER                                   |
|  Proprietary Formats (.uasset, .pak, .unity3d, .nif, .ba2, .vpk, .bik, etc.)            |
+-----------------------------------------------------------------------------------------+
```

---

## 2. Asset Classification & Canonical Intermediate Formats

| Asset Category | Canonical Intermediate Representation (IR) | Target Engine Formats (Examples) | Transcode Mode |
| :--- | :--- | :--- | :--- |
| **3D Geometry & Meshes** | glTF 2.0 (`.glb`, `.gltf`) with meshopt | Unreal `.uasset`, Bethesda `.nif`, Source `.mdl` | Lossless / Bound Preserved |
| **Textures & Surfaces** | OpenEXR / PNG / KTX2 (Basis Universal) | DirectDraw Surface (`.dds`), Unreal Texture2D | Lossless or Calibrated Lossy |
| **Audio & SFX** | Broadcast WAV (24-bit 48kHz) / Ogg Vorbis | Wwise SoundBank (`.bnk`), FMOD (`.bank`), XWB | Lossless or Perceptual Lossy |
| **Skeletal Animation** | glTF 2.0 Skeletal Animation / BVH | Unreal AnimSequence, Unity AnimClip | Kinematic Rig Retargeting |
| **Shaders & Materials** | MaterialX / SPIR-V Universal IR | HLSL, GLSL, Engine Shader Bytecode | Cross-Compilation |
| **Level / Scene Layout**| Universal Scene JSON (`nexludic-scene-v1`)| Unreal `.umap`, Unity `.unity`, Creation `.esp`| Declarative Spawning |
| **Localization & Text** | UTF-8 JSON / Fluent Localization Tables | Bethesda `.strings`, Unreal `.locres` | Lossless String Map |
| **Logic & Scripting** | Sandboxed Lua 5.4 / WASM Module | Engine Native Script Hooks | Sandboxed Execution |

---

## 3. Transformation Contracts & Lossless vs. Lossy Rules

### 3.1 Geometric & Semantic Preservation (`TRF-GEO-01`)
When transforming 3D models from glTF to a proprietary format:
- Bounding boxes and pivot origins must be preserved within a tolerance of $\pm 0.001$ units.
- Normal vectors, tangent vectors, and UV mapping coordinates must maintain topology without seam tears.
- Vertex colors and blend weights must sum to $1.0 \pm 0.0001$.

### 3.2 Texture Transcoding Rules (`TRF-TEX-01`)
- Albedo / BaseColor textures may use BC7 / ASTC compression if target engine requires.
- Normal maps MUST use dedicated tangent-space normal compression (e.g. BC5 / RGTC) to prevent vector length degradation.
- Roughness, metallic, and ambient occlusion channels packed into composite masks must preserve channel bit-depth (minimum 8 bits per channel).

### 3.3 Audio Transcoding Rules (`TRF-AUD-01`)
- Dialogue and voice clips must preserve sample rate (minimum 44.1 kHz) and channel allocation.
- Positional SFX must retain spatialization metadata or be transcoded to mono point-emitters.

---

## 4. Adapter Interface Contract

All game-specific adapters must expose the standard `ContentTranscodeService` over the canonical wire protocol:

```protobuf
syntax = "proto3";

package nexludic.adapter.content.v1;

service ContentTranscodeService {
  rpc QueryCapabilities (CapabilityRequest) returns (CapabilityResponse);
  rpc ValidateSourceAsset (AssetValidationRequest) returns (AssetValidationResponse);
  rpc TranscodeAsset (TranscodeRequest) returns (TranscodeResponse);
  rpc PackageAssets (PackageRequest) returns (PackageResponse);
}

message TranscodeRequest {
  string asset_id = 1;
  string source_canonical_format = 2;
  string target_game_format = 3;
  bytes source_payload = 4;
  map<string, string> parameters = 5;
}

message TranscodeResponse {
  bool success = 1;
  string error_code = 2;
  string error_message = 3;
  bytes transcoded_payload = 4;
  int64 output_size_bytes = 5;
  string sha256_hash = 6;
}
```

---

## 5. Unsupported Formats & Graceful Degradation

If an incoming asset format is unsupported by the registered game adapter:
1. **Fallback Evaluation:** The engine queries whether a secondary lossy intermediate format exists (e.g., fallback from MaterialX to basic PBR textures).
2. **Non-Essential Asset Skipping:** If the asset is tagged as `optional` (e.g., ambient sound effect or decorative prop), the pipeline logs a warning and proceeds without it.
3. **Blocking Halt:** If the asset is `critical` (e.g., primary player character mesh or core level geometry), the pipeline halts at Stage F with `ERR_FMT_UNSUPPORTED`.
