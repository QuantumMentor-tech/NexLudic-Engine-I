# Host-Game Content Ingestion Synthetic Scenarios & Test Plan

**Document Identifier:** `NLE-CAT-CID-001`  
**Classification:** Quality Assurance & Verification Catalog  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-CON-AEP-001`, `NLE-POL-CDF-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Verification Strategy

This catalog defines twenty-six comprehensive synthetic test scenarios (Scenarios A through Z) designed to validate the architecture, data contracts, security boundaries, and failure handling of the content ingestion and asset discovery subsystem without executing parsers, extracting archives, or modifying live games.

> [!NOTE]
> Every scenario documented herein is **SYNTHETIC AND ILLUSTRATIVE**. These define future automated test expectations and do not represent executed tests.

---

## 2. Synthetic Test Matrix: Scenarios A through Z

### Scenario A: Authorized Project-Owned Asset
- **Initial State:** Project repository contains `models/celestial_sword.glb` with signed attestation.
- **Scope:** `SRC_PROJECT_INTERNAL` authorized in task contract.
- **Execution:** Discovery detects candidate; magic bytes verify `glTF 2.0`; metadata parsed; provenance authenticated.
- **Expected Outcome:** State transitions to `VALIDATED_ASSET`; eligible for Stage G transformation.

### Scenario B: Ambiguous Source Location
- **Initial State:** Request specifies source locator `weapons/`, matching both project models and user downloads.
- **Scope:** Ambiguous source resolution.
- **Execution:** Discovery controller detects multiple candidate roots.
- **Expected Outcome:** Halts with `ERR_SRC_AMBIGUOUS`. System refuses to guess source.

### Scenario C: Unavailable Source
- **Initial State:** Source directory is an unmounted network drive (`V:/SharedAssets/`).
- **Scope:** Remote source offline.
- **Execution:** Discovery attempt encounters I/O timeout.
- **Expected Outcome:** Marked `ERR_SRC_UNAVAILABLE`; partial discovery reported for accessible sources.

### Scenario D: Same Filename, Different Content
- **Initial State:** `SourceA/shield.dds` (SHA `1111...`) and `SourceB/shield.dds` (SHA `2222...`).
- **Scope:** Common generic filename collision.
- **Execution:** Ingestion assigns distinct canonical IDs: `asset.sourceA.shield` and `asset.sourceB.shield`.
- **Expected Outcome:** No collision or overwrite; both assets retained independently.

### Scenario E: Same Content, Different Packages
- **Initial State:** Identical sword model (SHA `3333...`) present in `PackageA` and `PackageB` with different license tags.
- **Scope:** Binary duplicate with divergent metadata.
- **Execution:** Deduplication engine detects byte match but preserves separate provenance and license records.
- **Expected Outcome:** Single physical cache entry; independent logical asset and license metadata records.

### Scenario F: Unknown Asset Format
- **Initial State:** File `magic.xyz` has unrecognized magic bytes `0xDEADBEEF`.
- **Scope:** Unsupported binary format.
- **Execution:** Magic byte detection fails; no adapter registered.
- **Expected Outcome:** Assigned status `UNSUPPORTED_FORMAT`; blocked from downstream stages.

### Scenario G: Misleading File Extension
- **Initial State:** File named `player_skin.png` contains Windows PE executable magic bytes `4D 5A`.
- **Scope:** Spoofed extension attack.
- **Execution:** Magic byte verification flags mismatch with declared `.png` extension.
- **Expected Outcome:** Immediate quarantine; fatal error `ERR_SEC_SPOOFED_EXECUTABLE`.

### Scenario H: Malformed Metadata
- **Initial State:** glTF asset contains truncated JSON chunk with unclosed braces.
- **Scope:** Binary structural corruption.
- **Execution:** Stage E parser fails syntax parse.
- **Expected Outcome:** Assigned `STRUCTURALLY_INVALID`; error code `ERR_META_MALFORMED`.

### Scenario I: Unverified Game Version
- **Initial State:** Asset manifest declares compatibility with `Skyrim 2.0`, but local instance is `1.6.1170`.
- **Scope:** Target version discrepancy.
- **Execution:** Compatibility gate evaluates game build.
- **Expected Outcome:** Marked `TARGET_INCOMPATIBLE`; blocked from deployment.

### Scenario J: Missing Provenance
- **Initial State:** Third-party weapon mesh provided with no author, commit hash, or source signature.
- **Scope:** Broken chain-of-custody.
- **Execution:** Provenance validator evaluates attestation.
- **Expected Outcome:** Assigned `UNRESOLVED_PROVENANCE`; permitted for local test, blocked from redistribution.

### Scenario K: Licensing Uncertainty
- **Initial State:** Asset has valid geometry but lacks SPDX license header or terms.
- **Scope:** Missing IP permissions.
- **Execution:** Licensing gate checks G1–G9 permissions.
- **Expected Outcome:** Local read (G1–G3) permitted; redistribution (G7–G9) blocked with `ERR_AUTH_LICENSE_UNRESOLVED`.

### Scenario L: Untrusted Third-Party Package
- **Initial State:** Package downloaded from anonymous URL containing compiled script assets.
- **Scope:** Tier 3 untrusted package ingestion.
- **Execution:** Trust classifier assigns Tier 3; script execution prohibited.
- **Expected Outcome:** Scripts quarantined; static meshes processed in sandbox.

### Scenario M: Dependency Unavailable
- **Initial State:** Mesh references `texture_normal.dds` which is absent from all authorized sources.
- **Scope:** Incomplete asset dependency.
- **Execution:** Dependency analyzer fails to resolve reference in DAG.
- **Expected Outcome:** Ingestion pauses; error `ERR_DEP_UNRESOLVED` logged.

### Scenario N: Circular Dependency
- **Initial State:** Material $A$ references Shader $B$; Shader $B$ references Material $A$.
- **Scope:** Graph cycle.
- **Execution:** `ALG-DAG-CYCLE` detects back-edge in dependency graph.
- **Expected Outcome:** Ingestion terminates with `ERR_DEP_CYCLE_DETECTED`.

### Scenario O: Conflicting Metadata Authorities
- **Initial State:** Asset header claims `CC0` license, but upstream project repository lists it as `All Rights Reserved`.
- **Scope:** Conflicting provenance claims.
- **Execution:** Authority evaluator compares Tier 1 project repo against Tier 5 asset chunk.
- **Expected Outcome:** Higher-authority project repository wins; asset marked proprietary.

### Scenario P: Archive Path Traversal
- **Initial State:** Zip package contains entry `../../Windows/System32/cmd.exe`.
- **Scope:** Zip Slip vulnerability attempt.
- **Execution:** Path normalization detects `..` escape outside staging root.
- **Expected Outcome:** Extraction aborted; archive quarantined with `ERR_SEC_PATH_TRAVERSAL`.

### Scenario Q: Oversized Content
- **Initial State:** Raw uncompressed texture archive decompresses to 45 GB (exceeding 10 GB limit).
- **Scope:** Resource exhaustion / Zip bomb attempt.
- **Execution:** Stream monitor measures extracted byte count against quota.
- **Expected Outcome:** Decompression halted at quota; error `ERR_SEC_ARCHIVE_OVERSIZED`.

### Scenario R: Source Changes During Discovery
- **Initial State:** User edits a source 3D model in Blender while discovery scan is running.
- **Scope:** Mid-scan source mutation.
- **Execution:** File modification timestamp and hash change between Stage C and E.
- **Expected Outcome:** Invalidation triggered; error `ERR_PRV_SOURCE_MUTATED`; discovery re-queued.

### Scenario S: Stale Cached Metadata
- **Initial State:** Metadata cached 2 weeks ago; underlying adapter schema was updated yesterday.
- **Scope:** Cache invalidation.
- **Execution:** Cache validator checks adapter version stamp against cached record.
- **Expected Outcome:** Cache invalidated; fresh metadata extraction initiated.

### Scenario T: Unauthorized Private Directory
- **Initial State:** Task requests discovery of `C:/Users/Player/Documents/PrivateTaxes/`.
- **Scope:** Out-of-bounds filesystem probe.
- **Execution:** Source controller validates requested URI against authorized task scope.
- **Expected Outcome:** Immediate refusal; error `ERR_SRC_AUTH_MISSING`.

### Scenario U: Script-Bearing Asset
- **Initial State:** Mod package contains `QuestScript.pex` or `.py` file.
- **Scope:** Executable script asset ingestion.
- **Execution:** Static AST analysis inspects script without executing code.
- **Expected Outcome:** Prohibited system calls audited; zero script bytes executed.

### Scenario V: Duplicate Resource Identifier
- **Initial State:** Two separate weapon mods claim game FormID `0x00012345`.
- **Scope:** Game namespace collision.
- **Execution:** Conflict detector identifies overlapping FormID in target instance.
- **Expected Outcome:** Conflict flagged; resolution deferred to Stage J/N with side-by-side prompt.

### Scenario W: Missing Adapter Capability
- **Initial State:** Asset is a Source Engine `.vtf` texture, but installed adapter only handles Unreal `.uasset`.
- **Scope:** Adapter capability deficiency.
- **Execution:** Capability matcher checks format MIME against adapter descriptor.
- **Expected Outcome:** Ingestion pauses with `ERR_PARSER_UNAVAIL`; adapter install prompted.

### Scenario X: Discovered but Unauthorized Asset
- **Initial State:** Asset is valid and clean, but task contract was scoped strictly for audio assets, not character meshes.
- **Scope:** Scope boundary enforcement.
- **Execution:** Eligibility controller checks task requirement filter.
- **Expected Outcome:** Asset retained as `DISCOVERED` but marked `NOT_ELIGIBLE_FOR_TASK`.

### Scenario Y: Partial Discovery with Valid Candidates
- **Initial State:** Folder 1 (accessible, 10 valid meshes) and Folder 2 (locked by OS permission).
- **Scope:** Partial discovery fault tolerance.
- **Execution:** Folder 1 assets validated; Folder 2 logs `ERR_SRC_UNAVAILABLE`.
- **Expected Outcome:** `result_status: PARTIAL_DISCOVERY_COMPLETED`; valid candidates promoted.

### Scenario Z: Compatibility Claim from AI Agent
- **Initial State:** AI coding agent asserts that a proprietary PS3 asset format is compatible with PC Unreal 5.
- **Scope:** Unverified agent claim.
- **Execution:** Source authority hierarchy evaluates Tier 6 claim.
- **Expected Outcome:** Claim given zero authority; asset marked `COMPATIBLE_UNTESTED` or `UNSUPPORTED`.
