# Version & Environment Mapping Validation and Test Plan

**Document ID:** `NLE-PLN-VEM-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED`  
**Workstream:** `Game Version Modeling, Edition and Release Relationships, Environment Profiles, Version-Specific Technical Evidence, Compatibility Constraints, and Assessment Freshness`  
**Phase:** `4 of 12 — Game Discovery & Compatibility Intelligence`  
**Step:** `28 of 100`  
**Related Documents:**
- [GAME_VERSION_EDITION_AND_ENVIRONMENT_COMPATIBILITY_MAPPING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_VERSION_EDITION_AND_ENVIRONMENT_COMPATIBILITY_MAPPING_SPECIFICATION.md) (`NLE-SPC-VEC-001`)
- [VERSION_AND_EDITION_RELATIONSHIP_CATALOG.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/VERSION_AND_EDITION_RELATIONSHIP_CATALOG.md) (`NLE-CAT-VER-001`)
- [GAME_ENVIRONMENT_PROFILE_AND_COMPATIBILITY_CONTEXT_MODEL.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_ENVIRONMENT_PROFILE_AND_COMPATIBILITY_CONTEXT_MODEL.md) (`NLE-DOM-ENV-001`)
- [VERSION_COMPATIBILITY_EVIDENCE_AND_PROVENANCE_RULES.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/VERSION_COMPATIBILITY_EVIDENCE_AND_PROVENANCE_RULES.md) (`NLE-RUL-VCE-001`)
- [ENVIRONMENT_CHANGE_AND_STALE_ASSESSMENT_POLICY.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/ENVIRONMENT_CHANGE_AND_STALE_ASSESSMENT_POLICY.md) (`NLE-POL-ECS-001`)
- [GAME_PAIR_ENVIRONMENT_AND_DEPENDENCY_MAPPING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_PAIR_ENVIRONMENT_AND_DEPENDENCY_MAPPING_SPECIFICATION.md) (`NLE-SPC-GPE-001`)

---

## 1. Test Strategy & Architectural Mandates

This test plan defines the comprehensive verification specifications for the version, edition, environment profile, dependency, and compatibility constraint mapping modules of **NexLudic Engine**.

### 1.1 Synthetic Fixture Strategy
In accordance with Step 28 governance, all test specifications operate against **synthetic mock fixtures**:
- In-memory synthetic `GameEnvironmentProfile` records.
- Synthetic `CompatibilityConstraint` rulebooks with mathematical intervals and set expressions.
- Mocked `LocalInstallationInstance` records with simulated timestamps and SHA-256 digests.
- Zero local disk scanning, zero running game processes, and zero network calls are performed during test execution.

---

## 2. Test Suite Specification (35 Design-Level Test Cases)

### 2.1 Group 1: Version Identity & Ordering (TVEM-VER)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-VER-001` | Non-SemVer Build String Preservation | Verify raw version string is never truncated. | Input: `"v1.6.1170.0.8 (GOG)"`. | `raw_version_string` retains verbatim string. | Exact string equality verified. |
| `TVEM-VER-002` | Incomparable Build Hash Comparison | Verify opaque hashes return incomparable. | Build A: `"hash:a1b2"`, Build B: `"hash:c3d4"`. | Evaluates to `COMPARISON_UNDEFINED`; no $<$ or $>$ allowed. | Comparison operator rejected safely. |
| `TVEM-VER-003` | Rollback Branch Isolation | Verify beta/rollback branch does not pollute trunk. | Release branch: `"beta-1.5.97"`. | Mapped to separate sibling under `GameReleaseDistribution`. | Distinct `build_id` created. |
| `TVEM-VER-004` | Unknown Build Non-Blocking State | Verify unknown version does not crash mapping. | Target installation version is null. | Marked `VERSION_UNKNOWN` per `DEC-002`. | Operation succeeds with limited scope. |

### 2.2 Group 2: Edition & Release Relationships (TVEM-EDN)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-EDN-001` | Multi-Edition Engine Bitness Separation | Verify 32-bit original vs 64-bit SE separation. | Original Skyrim (32-bit) vs Skyrim SE (64-bit). | Distinct `GameEdition` records with independent constraints. | Zero constraint bleeding between editions. |
| `TVEM-EDN-002` | Storefront Release Divergence | Verify Steam vs GOG distribution mapping. | Skyrim SE on Steam (1.6.640) vs GOG (1.6.659). | Separate `GameReleaseDistribution` records mapped. | Correct distribution-specific build bound. |
| `TVEM-EDN-003` | Ambiguous Edition Non-Defaulting | Verify system does not guess edition. | Candidate directory lacks edition markers. | State set to `PARTIALLY_IDENTIFIED`; edition null. | No arbitrary edition auto-selected. |

### 2.3 Group 3: Environment Profile & Completeness (TVEM-PRF)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-PRF-001` | Completeness State Transition | Verify transition to `COMPLETE_FOR_ASSESSMENT`. | Game build, bitness, and mandatory loaders confirmed. | Completeness state transitions to `COMPLETE_FOR_ASSESSMENT`. | State matches expectation. |
| `TVEM-PRF-002` | Minimal Profile Limited Scope | Verify minimal profile restricts downstream scope. | Profile in `MINIMAL_IDENTITY_ONLY`. | Flags that version-specific adapters cannot be evaluated. | Clear diagnostic scope emitted. |
| `TVEM-PRF-003` | Medium Integrity Token Assertion | Verify profile enforces Medium IL. | Execution token checked. | Profile enforces `integrity_level = MEDIUM_INTEGRITY_LEVEL`. | Zero UAC elevation detected. |
| `TVEM-PRF-004` | Host Path Exclusion Assertion | Verify `install_path` is excluded from profile. | Profile serialized to JSON. | JSON payload contains zero drive letters or user paths. | Path privacy verified. |

### 2.4 Group 4: Version Range & Constraint Evaluation (TVEM-RNG)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-RNG-001` | Closed Interval Evaluation | Verify evaluation of `"[1.6.640, 1.6.659]"`. | Build `1.6.640`, `1.6.650`, `1.6.659`, `1.6.1130`. | First 3 return `APPLICABLE`; `1.6.1130` returns `OUT_OF_RANGE`. | Boundary conditions pass. |
| `TVEM-RNG-002` | Half-Open Interval Evaluation | Verify evaluation of `"[1.5.97, *)"`. | Build `1.5.97` (Match), `1.5.80` (Fail). | Evaluates correctly against lower boundary. | Boundary conditions pass. |
| `TVEM-RNG-003` | Blacklisted Build Exclusion | Verify evaluation of `"[1.5.97, *) (!1.6.1130)"`. | Build `1.6.1130`. | Evaluates to `EXPLICITLY_EXCLUDED`. | Blacklisted patch blocked. |
| `TVEM-RNG-004` | Discrete Set Membership Evaluation | Verify evaluation of `IN("build_1", "build_2")`. | Build `build_2` (Match), `build_3` (Fail). | Evaluates discrete set membership accurately. | Set logic passes. |
| `TVEM-RNG-005` | Platform Scope Filter | Verify Windows x64 constraint on Linux Proton. | Constraint scoped to `WINDOWS_X64`; profile is `LINUX_PROTON`. | Evaluates to `PLATFORM_MISMATCH_INAPPLICABLE`. | Platform boundary enforced. |

### 2.5 Group 5: Dependency & Loader Mapping (TVEM-DEP)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-DEP-001` | Mandatory Mod Loader Prerequisite | Verify missing mandatory loader blocks capability. | Capability requires SKSE64; loader absent. | Evaluates to `PREREQUISITE_MISSING`; capability blocked. | Missing prerequisite reported. |
| `TVEM-DEP-002` | Optional Dependency Non-Blocking | Verify missing optional loader does not block all. | Optional extension absent; base capability requested. | Evaluates to `COMPATIBLE_WITH_OPTIONAL_ABSENT`. | Base capability allowed. |
| `TVEM-DEP-003` | Independent Version Separation | Verify loader version is decoupled from game build. | Game build: `1.6.640`, SKSE version: `2.2.3`. | Both versions tracked in separate structured fields. | Zero version string conflation. |
| `TVEM-DEP-004` | Incompatible Loader Version | Verify loader version mismatch detected. | Game is `1.6.1170`, but SKSE installed is `2.2.3` (for `1.6.640`).| Flags `DEPENDENCY_VERSION_MISMATCH`. | Actionable error emitted. |

### 2.6 Group 6: Provenance, Authority & Conflicts (TVEM-PRV)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-PRV-001` | Empirical Test Precedence Over Declaration| Verify Tier 1 test overrides Tier 3 declaration. | Adapter declares support; test log proves crash. | Tier 1 test takes precedence; status `INCOMPATIBLE`. | Higher authority prevails. |
| `TVEM-PRV-002` | Audit Trail Preservation on Override | Verify superseded claim is preserved in audit. | Tier 3 superseded by Tier 1. | Tier 3 marked `SUPERSEDED` with link to Tier 1; not deleted.| Historical record preserved. |
| `TVEM-PRV-003` | Peer Conflict Flagging | Verify peer conflict sets `EVIDENCE_CONFLICT`. | Two Tier 2 maintainer notes contradict. | State set to `EVIDENCE_CONFLICT`; flagged for review. | No arbitrary choice made. |
| `TVEM-PRV-004` | Absence of Evidence Is Not Incompatibility| Verify unlisted version is not marked broken. | Version not mentioned in documentation. | State set to `UNDOCUMENTED_UNASSESSED`. | Not marked incompatible. |

### 2.7 Group 7: Staleness & Reassessment Triggers (TVEM-STL)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-STL-001` | Binary Hash Update Triggers Critical Stale | Verify binary change invalidates prior assessment. | Game binary SHA-256 updated. | Assessment state transitions to `STALE_CRITICAL`. | Active integration paused. |
| `TVEM-STL-002` | Historical Test Retention on Staleness | Verify old test log retained after update. | Assessment becomes stale. | Prior `TestExecutionRecord` marked `HISTORICALLY_VALID_INAPPLICABLE`. | Old test record untouched. |
| `TVEM-STL-003` | Loader Update Triggers Major Stale | Verify loader patch triggers major staleness. | SKSE updated from `2.2.3` to `2.2.4`. | Assessment state transitions to `STALE_MAJOR`. | Adapter re-check flagged. |
| `TVEM-STL-004` | Scope Expansion Triggers Partial Stale | Verify adding new capability flags staleness. | User adds new capability to project. | New capability set to `SCOPE_STALE`; old kept valid. | Granular staleness set. |

### 2.8 Group 8: Game-Pair Scope & Capability Granularity (TVEM-GPR)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TVEM-GPR-001` | Additive Fallacy Rejection | Verify supported A + supported B $\ne$ feasible pair. | Game A valid; Game B valid; no pair bridge exists. | Pair state set to `INSUFFICIENT_PAIR_EVIDENCE`. | Pair feasibility not assumed. |
| `TVEM-GPR-002` | Capability Granularity Preservation | Verify partial project capability results. | Cap 1 valid; Cap 2 unassessed. | Project status is `PARTIALLY_FEASIBLE`; no binary collapse. | Both states displayed. |
| `TVEM-GPR-003` | Separate Participant Environments | Verify Game A and Game B environments stay isolated.| Game A is 32-bit; Game B is 64-bit. | Profiles tracked completely independently. | Zero profile leakage. |
| `TVEM-GPR-004` | Shared Communication Layer Specification | Verify bridge IPC context modeled accurately. | Named Pipes bridge protocol specified. | Protocol parameters mapped in `CrossGameBridgeContext`. | Interop channel defined. |

---

## 3. Test Traceability Matrix

| Requirement / Standard | Addressed Test Cases |
| :--- | :--- |
| `REQ-SEC-001` (Medium Integrity Level) | `TVEM-PRF-003` |
| `REQ-SAFE-001` (Anti-Cheat & Non-Invasive)| `TVEM-PRF-004`, `TVEM-GPR-004` |
| `DEC-002` (Non-Blocking Unknowns) | `TVEM-VER-004` |
| Non-SemVer Version Handling | `TVEM-VER-001`, `TVEM-VER-002`, `TVEM-VER-003` |
| Multi-Edition & Storefronts | `TVEM-EDN-001`, `TVEM-EDN-002`, `TVEM-EDN-003` |
| Profile & Privacy Governance | `TVEM-PRF-001`, `TVEM-PRF-002`, `TVEM-PRF-004` |
| Range Interval & Sets | `TVEM-RNG-001`, `TVEM-RNG-002`, `TVEM-RNG-003`, `TVEM-RNG-004`, `TVEM-RNG-005` |
| Dependencies & Loaders | `TVEM-DEP-001`, `TVEM-DEP-002`, `TVEM-DEP-003`, `TVEM-DEP-004` |
| Evidence & Provenance Tiers | `TVEM-PRV-001`, `TVEM-PRV-002`, `TVEM-PRV-003`, `TVEM-PRV-004` |
| Staleness & Reassessment | `TVEM-STL-001`, `TVEM-STL-002`, `TVEM-STL-003`, `TVEM-STL-004` |
| Pair Scope & Granular Retaining | `TVEM-GPR-001`, `TVEM-GPR-002`, `TVEM-GPR-003`, `TVEM-GPR-004` |

---

## 4. Document Sign-Off & Verification

| Role | Reviewer | Verdict | Date |
| :--- | :--- | :--- | :--- |
| **Principal Test Architect** | Quality Assurance Governance | Approved | 2026-11-17 |
| **Senior Domain-Model Architect** | Core Data Modeling | Approved | 2026-11-17 |
| **Senior Software Architect** | Engine Architecture Team | Approved | 2026-11-17 |

