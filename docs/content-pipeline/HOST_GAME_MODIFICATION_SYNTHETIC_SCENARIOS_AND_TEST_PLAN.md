# Host-Game Modification Synthetic Scenarios & Test Plan

**Document Identifier:** `NLE-CAT-GMS-001`  
**Classification:** Quality Assurance & Verification Catalog  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-CPS-001`, `NLE-POL-GCC-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Verification Strategy

This catalog defines twenty-six comprehensive synthetic test scenarios (Scenarios A through Z) designed to validate the architecture, state transitions, security boundaries, and recovery behaviors of the host-game modification system without modifying actual live game installations.

---

## 2. Synthetic Test Matrix: Scenarios A through Z

### Scenario A: Supported Host-Game Extension
- **Preconditions:** Skyrim Special Edition installed; `MOD_DIRECTORY_DROP` supported; valid glTF sword model provided; user confirmed plan.
- **Pipeline Execution:** Passes Stages A through P cleanly.
- **Expected Outcome:** Transcoded `.nif` and `.dds` staged and deployed to `Data/Meshes/` and `Data/Textures/`. Post-verification succeeds.

### Scenario B: Unsupported Target Build
- **Preconditions:** Game build version is `1.5.97`; package manifest requires build `>= 1.6.0`.
- **Pipeline Execution:** Stage B detects build mismatch.
- **Expected Outcome:** Pipeline halts at Stage B with `ERR_ENV_INCOMPATIBLE`. Zero files modified.

### Scenario C: Unknown Modification Mechanism
- **Preconditions:** Target game is a proprietary C++ engine with no registered mod loader or mod folder.
- **Pipeline Execution:** Stage B strategy selection returns `UNSUPPORTED_STRATEGY`.
- **Expected Outcome:** Pipeline halts with error report. User offered manual export mode.

### Scenario D: Missing Source Artifact
- **Preconditions:** Mashup plan references `asset://models/dragon_armor.glb` which does not exist in source repository.
- **Pipeline Execution:** Stage C source discovery fails.
- **Expected Outcome:** Pipeline halts at Stage C with `ERR_SRC_NOT_FOUND`.

### Scenario E: Unsupported Content Format
- **Preconditions:** Critical character model provided in obscure proprietary format `.max` with no adapter parser.
- **Pipeline Execution:** Stage F format resolution fails.
- **Expected Outcome:** Pipeline halts at Stage F with `ERR_FMT_UNSUPPORTED`.

### Scenario F: Malformed Package
- **Preconditions:** `.nlpkg` archive has corrupted JSON syntax in `manifest.json`.
- **Pipeline Execution:** Stage E source validation attempts parse.
- **Expected Outcome:** Pipeline halts at Stage E with `ERR_SRC_MALFORMED`.

### Scenario G: Unresolved Dependency
- **Preconditions:** Package $A$ requires package $B (\ge 2.0.0)$, but only $B (1.5.0)$ is available.
- **Pipeline Execution:** Stage J dependency resolution cannot satisfy constraints.
- **Expected Outcome:** Pipeline pauses at Stage J with `ERR_DEP_UNRESOLVED`.

### Scenario H: License or Distribution Uncertainty
- **Preconditions:** Package contains textures extracted from proprietary commercial game with no license grant.
- **Pipeline Execution:** Stage D provenance verification flags copyright uncertainty.
- **Expected Outcome:** Pipeline halts at Stage D with `ERR_AUTH_UNVERIFIED`.

### Scenario I: Existing User Modifications
- **Preconditions:** User has installed a community mod that created `Data/Meshes/Weapons/blade.nif`.
- **Pipeline Execution:** Stage N detects collision during baseline snapshot.
- **Expected Outcome:** Staging engine namespaces file or prompts user for explicit overwrite approval.

### Scenario J: Package Built for Another Game Edition
- **Preconditions:** Package built for Unreal Engine 4 game attempted on Unreal Engine 5 sequel.
- **Pipeline Execution:** Stage B flags `engine_version_family` incompatibility.
- **Expected Outcome:** Application blocked; adapter conversion requested.

### Scenario K: Transformation Changes Required Semantics
- **Preconditions:** Transcoder scales model bounding box by $300\%$ exceeding $\pm 0.001$ invariant.
- **Pipeline Execution:** Stage I intermediate validation detects bounding deviation.
- **Expected Outcome:** Stage I halts with `ERR_ARTIFACT_INVALID`; transcode aborted.

### Scenario L: Untrusted Script-Bearing Package
- **Preconditions:** Package from Tier 3 source contains unverified `.lua` script.
- **Pipeline Execution:** Stage L security scan checks trust tier.
- **Expected Outcome:** Rejected with `ERR_SEC_UNTRUSTED_SCRIPT`.

### Scenario M: Embedded Instruction Attack
- **Preconditions:** Package description contains prompt injection attempting to trick AI coding agents.
- **Pipeline Execution:** Stage D sanitizes metadata strings.
- **Expected Outcome:** Injection payload neutralized; agent operates under unaltered system prompt.

### Scenario N: Archive Path Traversal
- **Preconditions:** Archive member path is `../../Windows/System32/calc.exe`.
- **Pipeline Execution:** Stage E path validator detects parent traversal.
- **Expected Outcome:** Immediate abort with `ERR_SEC_PATH_TRAVERSAL`; archive quarantined.

### Scenario O: Package Integrity Mismatch
- **Preconditions:** Asset file byte altered in transit; SHA-256 does not match `manifest.json`.
- **Pipeline Execution:** Stage L computes hash mismatch.
- **Expected Outcome:** Stage L halts with `ERR_SEC_INTEGRITY_MISMATCH`.

### Scenario P: Compatibility Evidence Becomes Stale
- **Preconditions:** Game receives automatic Steam update while pipeline is at Stage M.
- **Pipeline Execution:** Stage M pre-application check re-validates game executable hash.
- **Expected Outcome:** Hash mismatch detected; compatibility invalidated to `UNKNOWN`; pipeline paused.

### Scenario Q: Content Package Ready but Installation Unauthorized
- **Preconditions:** Package staged successfully at Stage L; user clicks "Cancel" on approval dialog at Stage M.
- **Pipeline Execution:** User approval withheld.
- **Expected Outcome:** Pipeline gracefully terminates; staging directory cleaned; zero files modified.

### Scenario R: Interrupted Application Operation
- **Preconditions:** Simulated process termination mid-way through file copy in Stage N.
- **Pipeline Execution:** Recovery engine detects uncommitted journal on subsequent boot.
- **Expected Outcome:** `ROLLBACK_TRANSACTION` executed; pre-modification baseline restored cleanly.

### Scenario S: Rollback Capability Unavailable
- **Preconditions:** Backup storage drive runs out of disk space during snapshot creation.
- **Pipeline Execution:** Stage N pre-flight detects snapshot failure.
- **Expected Outcome:** Pipeline halts BEFORE modifying target game; modification refused.

### Scenario T: Successful Application, Failed Functional Verification
- **Preconditions:** Files deployed successfully, but Stage O smoke test detects missing dependency symbol.
- **Pipeline Execution:** Stage O verification fails.
- **Expected Outcome:** Automatic `ROLLBACK_VERIFICATION` triggered; baseline restored.

### Scenario U: Game Update Invalidates Installed Content
- **Preconditions:** Game patched by Steam days after modification applied.
- **Pipeline Execution:** NexLudic launch monitor detects updated game version.
- **Expected Outcome:** User notified; package disabled or queued for re-verification.

### Scenario V: Two Packages Conflict
- **Preconditions:** Package $A$ and Package $B$ declare explicit mutual conflict.
- **Pipeline Execution:** Stage J detects conflict entry.
- **Expected Outcome:** Stage J blocks concurrent installation.

### Scenario W: Optional Content Unavailable
- **Preconditions:** Package has optional orchestral audio track that failed download.
- **Pipeline Execution:** Stage C logs warning; pipeline transitions to `DEGRADED`.
- **Expected Outcome:** Core meshes and textures deploy successfully; audio skipped cleanly.

### Scenario X: Agent-Generated Compatibility Claim
- **Preconditions:** AI agent asserts package is compatible without static or empirical test evidence.
- **Pipeline Execution:** Stage B evaluates evidence tier.
- **Expected Outcome:** Tier 4 claim classified as `COMPATIBLE_UNTESTED`; user confirmation required.

### Scenario Y: Unauthorized Game-Process Interaction
- **Preconditions:** Proposed strategy attempts to hook game process memory or inject DLL.
- **Pipeline Execution:** Stage B strategy selection evaluates mechanism.
- **Expected Outcome:** Rejected immediately under `INV-GMS-01` and `P7-C03`.

### Scenario Z: Valid Package with Incomplete Provenance
- **Preconditions:** Package is syntactically valid but lacks cryptographic build signature.
- **Pipeline Execution:** Stage D checks signature.
- **Expected Outcome:** Downgraded to Tier 3; treated as untrusted third-party content.
