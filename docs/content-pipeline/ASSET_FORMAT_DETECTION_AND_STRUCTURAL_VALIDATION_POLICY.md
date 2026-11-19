# Asset Format Detection & Structural Validation Policy

**Document Identifier:** `NLE-POL-AFD-001`  
**Classification:** Quality Assurance & Ingestion Validation Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-SPC-CAI-001`, `NLE-SPC-CFT-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Multi-Stage Format Detection

A filename extension (e.g. `.dds` or `.pak`) is trivial to spoof and completely insufficient for format validation. NexLudic Engine employs an eight-stage format detection and validation hierarchy:

```
[Level 1: Filename Extension] ---------> Lowest confidence (Heuristic hint only)
               |
[Level 2: Manifest Declaration] -------> Package manifest claims format MIME
               |
[Level 3: Magic Byte Probe] -----------> Deep binary inspection of file header
               |
[Level 4: Registered Adapter Check] ---> Check if registered adapter supports format
               |
[Level 5: Version Conformance] --------> Verify engine/format version is supported
               |
[Level 6: Structural Grammar Parse] ---> Full binary syntax and chunk validation
               |
[Level 7: Semantic Invariant Check] ---> Internal references, bounds, vertex integrity
               |
[Level 8: Target Compatibility Match] -> Compatible with target game engine build
```

---

## 2. Magic Byte & Binary Signature Registry

Every candidate asset must match its expected magic bytes before parsing:

| Format Name | Expected Extensions | Magic Bytes (Hex) | Offset | MIME Type |
| :--- | :--- | :--- | :--- | :--- |
| **glTF Binary** | `.glb` | `67 6C 54 46` (`glTF`) | 0 | `model/gltf-binary` |
| **DirectDraw Surface**| `.dds` | `44 44 53 20` (`DDS `) | 0 | `image/vnd-ms.dds` |
| **PNG Image** | `.png` | `89 50 4E 47 0D 0A 1A 0A`| 0 | `image/png` |
| **Ogg Vorbis Audio** | `.ogg`, `.oga` | `4F 67 67 53` (`OggS`) | 0 | `audio/ogg` |
| **Broadcast WAV** | `.wav` | `52 49 46 46 .... 57 41 56 45` | 0, 8 | `audio/vnd.wave` |
| **Bethesda NIF** | `.nif` | `47 61 6D 65 62 72 79 6F` or `4E 65 74 49 6D 6D 65 72` | 0 | `application/x-nif-mesh` |
| **Unreal Package** | `.uasset`, `.pak` | `C1 83 2A 9E` (Package Tag) | 0 | `application/x-unreal-package` |

### Extension Mismatch Defense:
If a file is named `hero.png` but exhibits magic bytes `4D 5A` (Windows PE executable / DLL), the file is IMMEDIATELY quarantined with fatal error code `ERR_SEC_SPOOFED_EXECUTABLE`.

---

## 3. Structural Validation Contracts by Category

### 3.1 3D Geometry & Meshes (`VAL-STR-MESH`)
1. **Header Validation:** Confirm chunk lengths match total file size.
2. **Buffer Integrity:** Ensure accessor byte offsets and strides fall strictly within buffer boundaries.
3. **Topology Check:** Reject non-manifold geometry or zero-area degenerate triangles where strict topology is required.
4. **Coordinate Bounds:** Reject coordinates with `NaN`, `+Inf`, or `-Inf` values.

### 3.2 Textures & Bitmaps (`VAL-STR-TEX`)
1. **Dimensions:** Verify width and height are within allowed engine limits ($\le 8192$).
2. **Pixel Format:** Validate pixel format descriptor matches declared fourCC (e.g. `DXT1`, `DXT5`, `BC7`).
3. **Mipmap Chain:** Confirm complete mipmap pyramid data is present if mipmapping flag is set.

### 3.3 Audio Streams (`VAL-STR-AUD`)
1. **Sample Rate:** Verify sample rate is standard (e.g. 44100 Hz or 48000 Hz).
2. **Channel Layout:** Confirm channel count is $\ge 1$ and $\le 8$.
3. **Stream Syntax:** Verify vorbis packet headers or PCM block alignment.

---

## 4. Unsupported Formats & Degradation Policy

- **Unknown Format:** If magic bytes do not match any known format, asset state transitions to `UNSUPPORTED_FORMAT`.
- **Unsupported Version:** If format is known (e.g. Unreal 5.4 `.uasset`) but adapter only supports Unreal 4.27, state transitions to `UNSUPPORTED_FORMAT_VERSION`.
- **Non-Fatal Graceful Skip:** If an unsupported asset is marked `is_optional: true` in the task manifest, the ingestion pipeline issues warning `WARN_OPTIONAL_SKIPPED` and proceeds.
- **Blocking Halt:** If an unsupported asset is marked `is_optional: false`, the discovery run halts at Stage F with `ERR_FMT_UNSUPPORTED`.
