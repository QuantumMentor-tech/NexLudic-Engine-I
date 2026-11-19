# Content Format Conversion Source-Target Contract

**Document Identifier:** `NLE-CON-FCC-001`  
**Classification:** Technical Contract & Format Conversion Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-SPC-CFT-001`, `NLE-MOD-TRF-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Format Conversion Matrix

This contract establishes the formal input, output, and preservation rules for format conversion across the eight primary asset categories. Every conversion pathway must define its explicit input schema, output schema, and unsupported-field behaviors.

| Asset Category | Source Canonical Format | Target Proprietary Formats | Conversion Mode | Preserved Semantics |
| :--- | :--- | :--- | :--- | :--- |
| **3D Geometry** | glTF 2.0 Binary (`.glb`) | Bethesda NIF, Unreal `.uasset`, Source `.mdl` | Lossless / Bound Preserved | Bounds ($\pm 0.001$), Normals, UVs |
| **2D Textures** | OpenEXR, PNG, KTX2 | DirectDraw Surface (`.dds`), Unreal Texture2D | Lossless or Calibrated Lossy| Color Space, Mipmaps, Bit-depth |
| **Audio Streams**| Broadcast WAV (24-bit 48kHz) | Wwise SoundBank (`.bnk`), FMOD (`.bank`), XWB | Lossless or Perceptual Lossy| Sample Rate, Channel Layout, Peaks|
| **Skeletal Rig**| glTF 2.0 Rigged Skin | Unreal Skeleton, Unity Avatar, Havok HKT | Kinematic Retargeting | Bone Hierarchy, Bind Poses, Weights|
| **Materials** | MaterialX / PBR Universal JSON | Bethesda BSLighting, Unreal Material Instance | Shader Parameter Mapping | Roughness, Metallic, Albedo Base |
| **Scene Layout**| `nexludic-scene-v1` JSON | Unreal `.umap`, Unity `.unity`, Creation `.esp` | Declarative Spawning | Transform Matrices, Entity Refs |
| **Localization**| Fluent Translation Tables | Bethesda `.strings`, Unreal `.locres` | Lossless String Mapping | Unicode UTF-8 Text, Param Tokens |
| **Logic Hooks** | Sandboxed Lua 5.4 / WASM | Engine Native Script Bindings | Sandboxed Static Call Graph | Pure Functional State Transitions|

---

## 2. Input & Output Contracts

### 2.1 Input Contract Requirements
- **Strict Format Identity:** Input must be verified via magic bytes (`NLE-POL-AFD-001`), not merely filename extensions.
- **Syntactic Completeness:** All referenced buffers, accessors, and chunks must reside within the declared file length.
- **Zero Truncation:** Partial or corrupted source payloads are rejected immediately with `ERR_CONV_INPUT_TRUNCATED`.

### 2.2 Output Contract Requirements
- **Valid Binary Layout:** Output must adhere to the target engine's header magic, version tag, and chunk table alignment.
- **Integrity Digest:** Transcoder worker must compute and return the output SHA-256 digest and byte size.
- **Reference Resolution:** Target material and texture paths must be re-bound to valid target-game virtual paths (e.g. `Textures/Weapons/...`).

---

## 3. Handling Unsupported Fields

When a source asset contains properties that the target format cannot represent:
1. **Critical Properties:** If an unsupported property is essential to gameplay or visualization (e.g. skeletal skinning weights for a rigged character), the conversion MUST FAIL with `ERR_CONV_UNSUPPORTED_CRITICAL_PROPERTY`.
2. **Optional Properties:** If an unsupported property is non-critical (e.g. secondary vertex color channel or authoring comments), it may be omitted provided an explicit `OmissionRecord` is logged in the transformation result.
3. **Never Silently Discard:** Silent omission of unsupported fields without record is an invariant violation (`INV-TRF-02`).
