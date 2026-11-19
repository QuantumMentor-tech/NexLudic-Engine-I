# Content Transformation Synthetic Scenarios & Test Plan

**Document Identifier:** `NLE-CAT-TRF-001`  
**Classification:** Quality Assurance & Verification Catalog  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-CON-FCC-001`, `NLE-POL-TFD-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Verification Strategy

This catalog defines twenty-six comprehensive synthetic test scenarios (Scenarios A through Z) designed to validate the transformation architecture, conversion contracts, normalization invariants, loss accounting, security boundaries, and failure handling without running converters, modifying games, or transforming real assets.

> [!NOTE]
> Every scenario documented herein is **SYNTHETIC AND ILLUSTRATIVE**. These define future automated test expectations and do not represent executed tests.

---

## 2. Synthetic Test Matrix: Scenarios A through Z

### Scenario A: Valid Source and Supported Target Format
- **Initial State:** Validated glTF 2.0 sword mesh (`asset.weapons.sword`, SHA `8d3e...`); Skyrim Creation Engine target.
- **Execution:** Transformation definition `trf.gltf-to-skyrim-nif.v1` executed; unit matrix applied; coordinate frame transformed.
- **Expected Outcome:** Structurally valid `.nif` generated; bounds tolerance $\le 0.0004$; marked `TARGET_COMPATIBLE`.

### Scenario B: Recognized Extension with Unsupported Content
- **Initial State:** Input file named `mesh.glb` contains raw plain-text ASCII instead of binary glTF header.
- **Execution:** Pre-flight Gate 1 magic byte check inspects header.
- **Expected Outcome:** Pre-flight fails; error `ERR_PRE_FORMAT_MISMATCH`; transcode worker never spawned.

### Scenario C: Unsupported Source Version
- **Initial State:** Input mesh is glTF 3.0 draft format; registered adapter only supports glTF 2.0.
- **Execution:** Adapter capability query checks schema version.
- **Expected Outcome:** Halts with `ERR_TRF_FMT_VERSION`; candidate blocked.

### Scenario D: Missing Mapping Rule
- **Initial State:** Source asset defines custom shader parameter `emissive_strobe_frequency` with no Creation Engine mapping.
- **Execution:** Semantic mapping validation inspects required properties.
- **Expected Outcome:** Pipeline halts with `ERR_TRF_MAP_MISSING`; system refuses to invent arbitrary mapping.

### Scenario E: Lossy Conversion
- **Initial State:** High-poly mesh (1.2 million polygons) converted to legacy engine format capped at 65,536 vertices per partition.
- **Execution:** Decimation applied; loss accounting record generated with metric difference.
- **Expected Outcome:** Classified as `TIER_2_BOUNDED_LOSSY`; approved under task policy; loss record logged.

### Scenario F: Semantic Mismatch
- **Initial State:** Source format field `alpha` represents opacity ($0 = \text{transparent}, 1 = \text{opaque}$); target format field `alpha` represents roughness.
- **Execution:** Semantic mapping policy checks property definitions.
- **Expected Outcome:** Direct name-based copy rejected; explicit semantic transform rule enforced.

### Scenario G: Unit or Coordinate Mismatch
- **Initial State:** Source model modeled in millimeters; target engine expects centimeters; scale factor missing from metadata.
- **Execution:** Coordinate validator detects extreme metric deviation ($1000\times$).
- **Expected Outcome:** Halts with `ERR_TRF_SEM_FAILED`; unresolved unit mapping flagged for user review.

### Scenario H: Missing Metadata
- **Initial State:** Texture format requires color space tag (`sRGB` vs `Linear`), but source metadata omits it.
- **Execution:** Ingestion validator detects missing required property.
- **Expected Outcome:** Candidate blocked with `ERR_TRF_META_INSUFFICIENT`; default value not fabricated.

### Scenario I: Unsupported Optional Property
- **Initial State:** Mesh contains secondary authoring comments and viewport camera bookmarks.
- **Execution:** Target NIF format cannot store viewport bookmarks; property marked `is_optional: true`.
- **Expected Outcome:** Bookmarks omitted; `OmissionRecord` logged; conversion succeeds.

### Scenario J: Multi-Stage Conversion
- **Initial State:** Proprietary Format $A$ converted to Format $B$ via Canonical IR ($A \rightarrow \text{IR} \rightarrow B$).
- **Execution:** Stage 1 produces intermediate glTF; Stage I validates IR; Stage 2 produces final target binary.
- **Expected Outcome:** Both stages validated; provenance dossier records multi-stage lineage.

### Scenario K: Circular Transformation Dependency
- **Initial State:** Material $M$ depends on Texture $T$; Texture $T$ generated from procedural bake of Material $M$.
- **Execution:** Transformation DAG planner runs cycle check.
- **Expected Outcome:** Back-edge detected; halts with `ERR_TRF_DAG_CYCLE`.

### Scenario L: Stale Transformation Definition
- **Initial State:** Output generated under definition v1.0; definition updated to v1.2 with new normal map encoding.
- **Execution:** Pipeline invalidation check detects definition version mismatch.
- **Expected Outcome:** Cached output invalidated; re-transcode scheduled.

### Scenario M: Adapter Declares Unverified Support
- **Initial State:** Community adapter claims support for Unreal 5.4 `.uasset`, but lacks conformance test logs.
- **Execution:** Capability vs Proof engine evaluates Tier 4 claim.
- **Expected Outcome:** Marked `CAPABILITY_UNVERIFIED`; requires explicit user override to proceed.

### Scenario N: Source Asset Changes During Processing
- **Initial State:** Source `.glb` file overwritten by user mid-transcode.
- **Execution:** Post-transcode hash verification detects source digest divergence.
- **Expected Outcome:** Worker output discarded; error `ERR_TRF_SRC_MUTATED`; transcode re-queued.

### Scenario O: Output Valid Structurally, Incompatible with Target
- **Initial State:** Output NIF file parses cleanly, but uses shader flags incompatible with Skyrim SE 1.6.
- **Execution:** Gate 4 target compatibility check validates engine shader flags.
- **Expected Outcome:** State transitions to `INCOMPATIBLE_BLOCKED`; blocked from packaging.

### Scenario P: Conversion Loses Mandatory Property
- **Initial State:** Rigged character conversion drops root motion bone essential to player movement.
- **Execution:** Semantic validator checks required skeleton bones.
- **Expected Outcome:** Transformation rejected with `ERR_TRF_PROP_UNSUPPORTED`.

### Scenario Q: External Transformation Service Has Uncertain Output
- **Initial State:** Remote transcoder times out after network socket drop.
- **Execution:** Output integrity and status response missing.
- **Expected Outcome:** Result marked `ERR_TRF_OUTCOME_UNCERTAIN`; partial files cleaned; retry initiated.

### Scenario R: Resource Limit Exceeded
- **Initial State:** Massive 8K uncompressed texture causes worker process to allocate 5.2 GB RAM (limit 4 GB).
- **Execution:** OS resource watcher detects quota breach.
- **Expected Outcome:** Worker terminated; error `ERR_TRF_RES_EXCEEDED`; diagnostic bundle generated.

### Scenario S: Malicious Source Structure
- **Initial State:** glTF buffer view declares accessor count $2^{31}-1$ attempting integer overflow.
- **Execution:** Sandboxed worker integer overflow guards detect invalid allocation size.
- **Expected Outcome:** Worker cleanly rejects chunk; error `ERR_CONV_INPUT_CORRUPT`; main process unaffected.

### Scenario T: Transformation Includes Unauthorized Executable Content
- **Initial State:** Mod script transpile generates raw x86 machine code DLL.
- **Execution:** Output validator inspects magic bytes; detects Windows PE header (`4D 5A`).
- **Expected Outcome:** Immediate quarantine; fatal error `ERR_PRE_PROHIBITED_OPERATION`.

### Scenario U: Source License Restricts Transformation or Redistribution
- **Initial State:** Source asset licensed under `CC-ND` (No Derivatives).
- **Execution:** Precondition gate checks `modification_permitted` flag.
- **Expected Outcome:** Transformation rejected with `ERR_PRE_LICENSE_RESTRICTED`.

### Scenario V: Two Conversion Paths Produce Different Results
- **Initial State:** Path 1 ($A \rightarrow \text{NIF}$) and Path 2 ($A \rightarrow \text{IR} \rightarrow \text{NIF}$) yield divergent vertex counts.
- **Execution:** Ambiguity resolver evaluates paths.
- **Expected Outcome:** Halts with `ERR_TRF_PATH_CONFLICT`; system prompts user to select preferred path.

### Scenario W: Cached Output Becomes Stale
- **Initial State:** Cached texture compiled 30 days ago; target game received graphics overhaul patch.
- **Execution:** Cache validator checks target game build hash.
- **Expected Outcome:** Cache invalidated; fresh transcode executed.

### Scenario X: Unsupported Target-Game Build
- **Initial State:** Output targeted for Skyrim build `1.6.640`; installed game instance is `1.6.1170`.
- **Execution:** Target compatibility validator detects build number mismatch.
- **Expected Outcome:** Handoff blocked; requires re-targeting.

### Scenario Y: Partial Transformation Output
- **Initial State:** Transcoder crashes after writing 1.2 MB of a 4 MB mesh file.
- **Execution:** Staging manager detects non-zero exit code and truncated payload.
- **Expected Outcome:** Partial file deleted from staging; error `ERR_TRF_PARTIAL_OUTPUT` logged.

### Scenario Z: Successful Conversion, Missing Installation Authorization
- **Initial State:** NIF mesh successfully generated and verified through Stage I.
- **Execution:** User clicks "Deny" on Stage M installation review dialog.
- **Expected Outcome:** Output retained in staging cache; physical game files remain 100% untouched.
