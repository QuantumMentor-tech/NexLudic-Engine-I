# Target Mapping and Application Planning Synthetic Test Plan

**Document Identifier:** `NLE-CAT-TAP-001`  
**Lifecycle Stage:** Content Pipeline Stage I Quality Assurance  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**Test Suite Coverage:** Scenarios A through AJ (36 Exhaustive Test Scenarios)  
**System Status:** Authoritative Architectural Test Specification  
**Classification:** Verification Procedures & Synthetic Test Matrix  

---

## 1. Executive Summary & Test Governance

The **Target Mapping and Application Planning Synthetic Test Plan** establishes the comprehensive synthetic QA test matrix for Stage I of the Content Pipeline. It validates target environment modeling, evidence freshness, package intake, resource mapping, conflict detection, dependency ordering, non-destructive dry-run guarantees, filesystem containment, and approval governance.

This suite defines 36 discrete, reproducible synthetic test scenarios (**Scenarios A through AJ**). All scenarios run within isolated, mock synthetic harnesses without requiring access to, scanning of, or modification of real game installations.

---

## 2. Synthetic Test Scenario Matrix (Scenarios A through AJ)

| Scenario ID | Title | Domain Area | Input / Condition | Expected Behavior & Diagnostic Code | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **A** | Verified Package Matching Target | Golden Path | Valid `.nlpkg` with verified MPR; matching target profile | Pass; intake succeeds, planning proceeds | Designed |
| **B** | Ambiguous Package Identity | Package Intake | Multiple similar packages without unique digest | Abort; `ERR_TVP_AMBIGUOUS_PACKAGE_IDENTITY` | Designed |
| **C** | Stale Package Verification | Package Intake | Physical package altered after MPR issued | Reject; `ERR_TVP_STALE_PACKAGE_EVIDENCE` | Designed |
| **D** | Ambiguous Target Identity | Target Modeling | Target query matches multiple local installations | Abort; `ERR_TIS_AMBIGUOUS_TARGET` | Designed |
| **E** | Target Release Unknown | Target Compatibility| Target version cannot be determined | Abort; `ERR_TIS_UNKNOWN_RELEASE` | Designed |
| **F** | Conflicting Target Evidence | Target Evidence | Adapter declares v1.5, executable is v1.6 | Abort; `ERR_TIS_EVIDENCE_CONFLICT` | Designed |
| **G** | Target Satisfies All Constraints | Target Compatibility| Target satisfies SemVer, platform, and mod loader | Pass; target marked COMPATIBLE | Designed |
| **H** | Unsupported Target Capability | Target Compatibility| Target lacks required VFS overlay mechanism | Abort; `ERR_TIS_CAPABILITY_UNSUPPORTED` | Designed |
| **I** | Explicit Manifest Mapping | Resource Mapping | Member specifies valid target relative path | Pass; mapping created (`STRAT_MANIFEST_EXPLICIT`) | Designed |
| **J** | Bare Filename Heuristic Rejection| Resource Mapping | Member lacks explicit path; attempts filename match | Reject; `ERR_PMR_AMBIGUOUS_NAME_ONLY` | Designed |
| **K** | Unmapped Required Member | Resource Mapping | Mandatory package member has no target mapping | Abort; `ERR_PMR_UNMAPPED_REQUIRED_MEMBER` | Designed |
| **L** | Destination Collision | Conflict Detection | Two distinct members map to identical path | Abort; `ERR_PMR_DESTINATION_COLLISION` | Designed |
| **M** | Unknown Resource State | State Governance | Resource presence not verified in target | Treated as `UNKNOWN`; NOT assumed absent | Designed |
| **N** | Stale Target Evidence | Freshness Lifecycle | Target snapshot lease expired (> 1800s) | Invalidate; `ERR_TEF_LEASE_EXPIRED` | Designed |
| **O** | New Resource Addition (ADD) | Change Modeling | Target destination is `CONFIRMED_ABSENT` | Operation classified as `ADD`; zero disk write | Designed |
| **P** | Existing Resource Replacement | Change Modeling | Destination is `CONFIRMED_PRESENT` with diff hash | Operation classified as `REPLACE`; backup flagged | Designed |
| **Q** | Identical Existing Resource | Change Modeling | Destination matches member SHA-256 exactly | Operation classified as `NO_OP` with rationale | Designed |
| **R** | Unknown Prior Content | Conflict Detection | Attempt to REPLACE destination with UNKNOWN state | Abort; cannot replace unknown baseline | Designed |
| **S** | Destructive Change Unapproved | Approval Boundary | Plan contains REPLACE without approval token | Plan status remains REVIEWABLE; cannot execute | Designed |
| **T** | Circular Operation Prerequisites | Dependency DAG | Change 1 depends on Change 2, and 2 depends on 1 | Abort; `ERR_APD_CYCLE_DETECTED` | Designed |
| **U** | Optional Member Conflict Exclusion | Conflict Detection | Optional member collides; policy permits omit | Member excluded; plan remains valid with note | Designed |
| **V** | Mismatched Target Revision | Plan Integrity | Plan references Target Snapshot A, but target is B | Reject plan; `ERR_ALH_SNAPSHOT_MISMATCH` | Designed |
| **W** | Destination Scope Escape (`..`) | Path Containment | Mapping attempts `Data/../../Windows/system.dll` | Abort; `ERR_TPC_TRAVERSAL_DETECTED` | Designed |
| **X** | Executable Action Request | Security Boundary | Package contains install script / hook binary | Script rejected from execution; flagged | Designed |
| **Y** | Read-Only Inspection Permission | Permission Matrix | User token possesses `PERM_PLAN_REVIEW` only | Planning succeeds; disk writes prohibited | Designed |
| **Z** | Approval Reused for Other Target | Approval Boundary | Token signed for Skyrim instance A applied to B | Reject; `ERR_NDA_APPROVAL_SCOPE_DRIFT` | Designed |
| **AA**| Plan Mutation After Approval | Approval Boundary | Change added to plan after Ed25519 signature | Revoke approval; status reverted to DRAFT | Designed |
| **AB**| Missing License Rights | Governance | Package asset has UNRESOLVED license claim | Block planning; `ERR_TVP_LICENSE_RESTRICTION` | Designed |
| **AC**| All Mappings Resolve | Golden Path | 50 assets resolve cleanly to distinct paths | Plan status transitions to REVIEWABLE | Designed |
| **AD**| Partial Optional Mappings | Completeness | Required mapped, 2 optional unmapped | Plan marked REVIEWABLE_WITH_EXCLUSIONS | Designed |
| **AE**| Target Mutates During Planning | Concurrency | Target file modified while planner is computing | Abort; `ERR_TEF_RESOURCE_MUTATED` | Designed |
| **AF**| Mapping Rules Change Post-Plan | Invalidation | Adapter updates mapping layout schema | Invalidate plan; require re-planning | Designed |
| **AG**| Authorization Revoked Post-Plan | Governance | User planning lease revoked by admin | Invalidate plan; status set to BLOCKED | Designed |
| **AH**| Complete Static Plan Guarantee | Non-Destructive | Complete plan produced; inspect target folder | Target folder completely untouched (0 writes) | Designed |
| **AI**| Validation Service Unavailable | Reliability | Cryptographic verifier offline / uncontactable | Distinguish infra error; mark BLOCKED | Designed |
| **AJ**| Concurrent Plan Revisions | Concurrency | Two parallel plans generate different digests | Enforce revision isolation; no silent merge | Designed |

---

## 3. Deep-Dive Scenario Specifications

### Scenario AH: Complete Static Plan Guarantee (Non-Destructive Proof)
- **Objective:** Mathematically and physically prove that Stage I planning causes zero filesystem mutations.
- **Fixture:** Synthetic mock installation with 1,000 files; mock `.nlpkg` with 50 texture and script assets.
- **Assertions:**
  1. Complete `ApplicationPlan` generated with 50 proposed operations (48 `ADD`, 2 `REPLACE`).
  2. Recursive SHA-256 tree hash of mock installation directory before planning equals tree hash after planning.
  3. No temporary files or locks left in target directory.

### Scenario W: Destination Scope Escape Traversal Defense
- **Objective:** Ensure malicious relative paths cannot escape the target root directory.
- **Fixture:** Malformed member mapping containing `Data/../../../System32/drivers/etc/hosts`.
- **Assertions:**
  1. Path sanitization parser detects `..` traversal token.
  2. Planning halts immediately with `ERR_TPC_TRAVERSAL_DETECTED`.
  3. Security incident record logged with zero side effects.

---

## 4. Test Harness Compliance Invariant

All 36 synthetic scenarios must maintain 100% specification compliance in synthetic test harnesses before Stage I planning can be declared production-ready.
