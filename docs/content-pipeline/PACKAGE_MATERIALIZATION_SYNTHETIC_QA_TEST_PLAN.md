# Package Materialization Synthetic QA Test Plan

**Document Identifier:** `NLE-CAT-PMS-001`  
**Lifecycle Stage:** Content Pipeline Stage M Quality Assurance  
**Parent Document:** [HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md) (`NLE-SPC-PMS-001`)  
**Test Suite Coverage:** Scenarios A through AJ (36 Exhaustive Test Scenarios)  
**System Status:** Authoritative Architectural Test Specification  
**Classification:** Verification Procedures & Synthetic Test Suite  

---

## 1. Executive Summary & Purpose

The **Package Materialization Synthetic QA Test Plan** defines the comprehensive, synthetic test matrix validating every requirement, invariant, preflight rule, serialization algorithm, integrity layer, containment boundary, and failure recovery mechanism in Stage M.

This test plan defines 36 discrete, reproducible synthetic test scenarios (**Scenarios A through AJ**). Each scenario specifies the test objective, precondition, input fixtures, expected behavior, failure codes, and acceptance criteria.

---

## 2. Test Execution Harness & Sandbox Rules

All synthetic test suites execute under the following constraints:
1. **Isolated Test Sandboxes:** Each test scenario runs within an isolated workspace: `D:/NexLudic/TestRuns/run_<scenario_id>/`.
2. **Deterministic Time Mocking:** Wall-clock timestamps are virtualized and frozen to `2026-11-19T18:45:00Z` to guarantee deterministic signatures and tokens.
3. **Synthetic Payloads:** Test fixtures utilize generated synthetic binary streams with known SHA-256 digests. No real game assets or copyrighted binaries are required.
4. **Medium IL Execution:** Tests run under Windows Medium Integrity Level (`DEC-002`) without administrative elevation.

---

## 3. Synthetic Test Scenario Matrix (Scenarios A through AJ)

| ID | Scenario Title | Test Classification | Primary Assertion | Expected Result |
| :--- | :--- | :--- | :--- | :--- |
| **A** | Valid Baseline LPD Serialization | Golden Path | Materialize `.nlpkg` from valid LPD | Pass; valid container, MPR emitted |
| **B** | Expired Gate G5 Token Rejection | Security / Preflight | Submit LPD with expired Gate G5 token | Abort; `ERR_MEP_GATE_TOKEN_INVALID` |
| **C** | Forged Gate G5 Signature Rejection | Security / Preflight | Submit LPD with invalid Ed25519 signature | Abort; `ERR_MEP_GATE_TOKEN_INVALID` |
| **D** | Missing Member in Frozen Snapshot Storage | Preflight Storage | LPD references asset missing from snapshot store | Abort; `ERR_MEP_SNAPSHOT_MISSING` |
| **E** | Modified Input Asset (Digest Mismatch) | Integrity / Preflight | Asset modified on disk after Gate G5 sign-off | Abort; `ERR_MEP_INPUT_DIGEST_MISMATCH` |
| **F** | Insufficient Storage Quota Rejection | Resource Bounds | Free volume space < 2.5x projected package size | Abort; `ERR_MEP_INSUFFICIENT_STORAGE` |
| **G** | Unsupported Schema Version Rejection | Schema Compatibility | LPD specifies schema version `99.0.0` | Abort; `ERR_MEP_UNSUPPORTED_SCHEMA` |
| **H** | Path Traversal Member Rejection (`../`) | Security / Layout | Member path contains `../armor.dds` | Abort; `ERR_PLM_TRAVERSAL_DETECTED` |
| **I** | Leading Slash Member Rejection (`/`) | Security / Layout | Member path begins with `/assets/` | Abort; `ERR_PLM_INVALID_PATH` |
| **J** | Windows Drive Letter Rejection (`C:`) | Security / Layout | Member path contains `C:/textures/` | Abort; `ERR_PLM_INVALID_PATH` |
| **K** | Windows Backslash Rejection (`\`) | Cross-Platform | Member path contains backslashes | Abort; `ERR_PLM_INVALID_PATH` |
| **L** | Case-Folding Collision Detection | Cross-Platform | LPD contains `Armor.dds` and `armor.dds` | Abort; `ERR_PLM_CASE_COLLISION` |
| **M** | Forbidden Character Rejection (`:`, `*`) | Filesystem Safety | Member path contains `texture:01.dds` | Abort; `ERR_PLM_INVALID_PATH` |
| **N** | Lexicographical Sorting Enforcement | Determinism | Members written in non-alphabetical order | Verifier validates sorted order in archive |
| **O** | Fixed Canonical Timestamp Enforcement | Determinism | Inspect ZIP headers for timestamp | Timestamps set to `1980-01-01 00:00:00 UTC` |
| **P** | RFC 8785 Canonical JSON Formatting | Determinism | Inspect `manifest.json` and `provenance.json` | JCS formatting confirmed, zero extra whitespace |
| **Q** | Two-Pass Bit-for-Bit Identity | Reproducibility | Build twice from same snapshot | `SHA256(Build1) == SHA256(Build2)` |
| **R** | Member Uncompressed SHA-256 Verification | Integrity Layer 1 | Validate extracted member against manifest | Member digests match exactly |
| **S** | Merkle Tree Root Hash Validation | Integrity Layer 1 | Compute Merkle tree over all member leaves | Matches `manifest.json` Merkle root |
| **T** | Full Container SHA-256 Digest Validation | Integrity Layer 2 | Compute container file hash | Matches recorded container digest |
| **U** | Ed25519 Digital Signature Verification | Security / Integrity | Validate Ed25519 signature over manifest | Signature valid against public key |
| **V** | Corrupted Member Payload Detection | Post-Materialization | Flip one bit in member payload | Verifier detects `ERR_PAI_MEMBER_DIGEST_MISMATCH` |
| **W** | Corrupted Central Directory Detection | Post-Materialization | Truncate ZIP central directory | Verifier detects `ERR_PMV_CONTAINER_MALFORMED` |
| **X** | Zip-Bomb Expansion Ratio Defense | DoS Mitigation | Pack highly compressible 1GB null stream | Rejects if ratio > 10.0x (`ERR_PMV_EXPANSION_RATIO`) |
| **Y** | Member Count Limit Quota Enforcement | DoS Mitigation | LPD declares 250,001 member entries | Abort; exceeds maximum entry limit |
| **Z** | Uncataloged Archive Entry Rejection | Security / Anti-Tamper | Inject hidden file into container | Verifier rejects: `ERR_PAI_UNAUTHORIZED_ENTRY` |
| **AA** | Missing Catalog Member Detection | Post-Materialization | Delete one member entry from container | Verifier rejects: `ERR_PAI_MISSING_ENTRY` |
| **AB** | Atomic Promotion Success Verification | Lifecycle | Verify atomic rename from `.tmp` to `.nlpkg` | `.tmp` unlinked, `.nlpkg` exists in repo |
| **AC** | Promotion Failure Workspace Containment | Lifecycle / Recovery | Simulate locked destination file | Staging file retained, no corruption |
| **AD** | Staging Directory Execute Stripping | Workspace Security | Assert staging folder ACLs lack execute | Execute permission denied |
| **AE** | Abrupt Interruption Orphan Scavenging | Recovery / Resilience | Simulate crash leaving orphaned `.lease` | Startup scavenger safely purges `.tmp` files |
| **AF** | Native Archive Adapter: Bethesda BSA/BA2 | Adapter Serialization | Materialize package targeting Skyrim BSA | Valid BSA format, embedded metadata header |
| **AG** | Native Archive Adapter: Valve VPK | Adapter Serialization | Materialize package targeting Source VPK | Valid VPK format, correct tree layout |
| **AH** | Native Archive Adapter: Unreal PAK | Adapter Serialization | Materialize package targeting UE PAK | Valid PAK format, deterministic index |
| **AI** | Streaming Memory Bound Verification | Performance / Memory | Serialize 4GB asset with memory profiler | Memory overhead remains < 512 bytes/member |
| **AJ** | End-to-End Stage M Handoff to Stage I | System Integration | Execute full workflow through MPR emission | MPR emitted, verified valid, Stage I ready |

---

## 4. Scenario Deep-Dive Specifications

### Scenario A: Valid Baseline LPD Serialization
- **Goal:** Verify that a standard compliant LPD produces a valid, verified `.nlpkg` container.
- **Fixture:** 10 texture assets (25 MB total), valid Gate G5 token, Deflate Level 6.
- **Success Criteria:** Preflight passes, archive serialized deterministically, independent verifier passes all members, `.nlpkg` promoted atomically, MPR emitted.

### Scenario Q: Two-Pass Bit-for-Bit Determinism Validation
- **Goal:** Verify absolute binary reproducibility.
- **Fixture:** 50 heterogeneous assets (meshes, textures, configs), two independent materialization passes executed 1 hour apart with different random file discovery orders.
- **Success Criteria:** Byte-by-byte comparison yields identical byte stream; `SHA256(Pass1) == SHA256(Pass2)`.

### Scenario X: Zip-Bomb Defense (Expansion Ratio Limit)
- **Goal:** Prevent decompression bomb denial-of-service attacks.
- **Fixture:** Synthetic payload of 1 GB consisting of zeroes compressed into 1 MB (ratio: 1000:1).
- **Success Criteria:** Independent validator checks uncompressed-to-compressed ratio, detects ratio > 10.0x, rejects package with `ERR_PMV_EXPANSION_RATIO_EXCEEDED`, and securely purges workspace.

### Scenario AE: Abrupt Termination Orphan Scavenger Recovery
- **Goal:** Verify cleanup of orphaned staging files after unexpected engine termination.
- **Fixture:** Artificially created `temp_mat_12345/` staging directory and stale `.lease` file with PID 999999.
- **Success Criteria:** Pipeline startup runner identifies defunct PID, removes temporary files, unlinks lease, and records recovery in log.

---

## 5. Test Suite Verification Invariant

All 36 scenarios (A through AJ) must achieve a 100% pass rate in synthetic test harnesses before Stage M specifications can be declared implementation-ready.
