# Content Pipeline Validation & Post-Modification Verification Policy

**Document Identifier:** `NLE-POL-CPV-001`  
**Classification:** Quality Assurance & Verification Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-CPS-001`, `NLE-POL-GCC-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Verification Philosophy

Verification in the NexLudic Content Pipeline occurs continuously across the lifecycle of an asset rather than solely at final deployment. This policy codifies the validation gates at pre-flight, intermediate transformation, package assembly, and post-application deployment.

---

## 2. Four Verification Gates

```
[Gate 1: Source Validation (Stage E)]
   Validate raw assets against intermediate canonical schemas (glTF, WAV, PNG).
         |
         v
[Gate 2: Intermediate Artifact Validation (Stage I)]
   Validate transcoded game-specific assets for structural and semantic integrity.
         |
         v
[Gate 3: Package Integrity Validation (Stage L)]
   Validate package manifest, cryptographic hashes, dependencies, and license terms.
         |
         v
[Gate 4: Post-Application Verification (Stage O)]
   Validate physical filesystem state after deployment and confirm non-corruption.
```

---

## 3. Gate Verification Specifications

### 3.1 Gate 1: Source Validation (Stage E)
- **Geometry Check:** Verify glTF models have zero non-manifold edges, valid vertex normals, and bounding radii $> 0$.
- **Texture Check:** Verify image dimensions are powers-of-two ($2^n$) if required by legacy engines, maximum dimension $\le 8192 \times 8192$.
- **Audio Check:** Confirm PCM WAV audio is valid 16-bit or 24-bit uncompressed audio without clipping.

### 3.2 Gate 2: Intermediate Validation (Stage I)
- **Target Container Validation:** Confirm generated `.uasset`, `.nif`, or `.dds` files parse cleanly using engine adapter validator tools.
- **Bounding & Transform Invariants:** Verify mesh bounds match source within $\pm 0.001$ units.
- **Reference Resolution:** Ensure all internal material and texture references point to valid, present assets.

### 3.3 Gate 3: Package Integrity Validation (Stage L)
- **Hash Matching:** Compute SHA-256 for every asset in `assets/` and compare against `manifest.json`.
- **Dependency Completeness:** Verify all non-optional dependencies are satisfied in local package cache or remote registry.
- **Malware & Traversal Scan:** Run AST analysis on scripts, check archive member paths for path traversal characters (`..`).

### 3.4 Gate 4: Post-Application Verification (Stage O)
- **Target File Verification:** Verify every file listed in the deployment journal exists in the target directory with matching file size and hash.
- **Base Game Binary Integrity:** Confirm that the game executable (`.exe`) SHA-256 is identical to the baseline snapshot.
- **Clean Baseline Match:** If any unexpected file modifications are detected, trigger `ROLLBACK_VERIFICATION`.
