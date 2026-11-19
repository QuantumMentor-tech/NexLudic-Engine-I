# Universal Content Pipeline Stage and State Contract

**Document Identifier:** `NLE-CON-CPS-001`  
**Classification:** Technical Contract & State Machine Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`, `NLE-POL-GMS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Pipeline Philosophy

The Universal Content Pipeline provides a deterministic, verifiable, and reversible workflow for processing raw or intermediate cross-game assets into game-ready modification packages. It guarantees that no asset reaches a host game installation without passing through rigorous validation, semantic transformation, dependency checks, and security isolation.

The pipeline comprises sixteen discrete, sequential stages designated **Stage A** through **Stage P**.

---

## 2. Sixteen-Stage Pipeline Definition

```
+-----------------------------------------------------------------------------------------+
|                                16-STAGE CONTENT PIPELINE                                |
|                                                                                         |
|  [Stage A: Request & Req Resolution] ---> [Stage B: Target & Env Resolution]            |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage C: Source Discovery]        ---> [Stage D: Source Auth & Provenance]            |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage E: Source Validation]       ---> [Stage F: Format & Schema Resolution]          |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage G: Transformation Planning] ---> [Stage H: Controlled Transformation]           |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage I: Intermediate Validation] ---> [Stage J: Dependency & Compatibility Res]      |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage K: Package Assembly]        ---> [Stage L: Package Integrity & Security]        |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage M: Pre-App Readiness]       ---> [Stage N: Application Handoff]                 |
|                   |                                     |                               |
|                   v                                     v                               |
|  [Stage O: Post-App Verification]   ---> [Stage P: Recovery & Provenance Closure]       |
+-----------------------------------------------------------------------------------------+
```

### Stage Summary & Interface Specifications

| Stage | Name | Input Contract | Output Contract | Primary Failure Code |
| :--- | :--- | :--- | :--- | :--- |
| **A** | Request & Requirement Resolution | Mashup Plan / User Request | Normalized Content Request Spec | `ERR_REQ_UNRESOLVABLE` |
| **B** | Target-Game & Environment Resolution| Canonical Game ID + Detected Inst | Target Environment Descriptor | `ERR_ENV_INCOMPATIBLE` |
| **C** | Source-Content Discovery | Content Reference IDs | Raw Source Asset Manifest | `ERR_SRC_NOT_FOUND` |
| **D** | Source Authorization & Provenance | Source Asset Manifest | Verified Provenance Dossier | `ERR_AUTH_UNVERIFIED` |
| **E** | Source Validation | Raw Source Assets | Source Structural Validity Cert | `ERR_SRC_MALFORMED` |
| **F** | Format & Schema Resolution | Target Engine Schema + Source Type| Transformation Blueprint | `ERR_FMT_UNSUPPORTED` |
| **G** | Transformation Planning | Blueprint + Adapter Capabilities | Concrete Stage-H Execution Plan | `ERR_PLAN_FAILED` |
| **H** | Controlled Transformation | Raw Assets + Adapter Plugin | Intermediate Transcoded Assets | `ERR_TRANSFORM_ABORT` |
| **I** | Intermediate Artifact Validation | Transcoded Assets | Intermediate Conformance Report | `ERR_ARTIFACT_INVALID` |
| **J** | Dependency & Compatibility Res | Transcoded Assets + Target Env | Resolved Dependency Graph | `ERR_DEP_UNRESOLVED` |
| **K** | Package Assembly | Assets + Dependency Graph | Staged Content Package | `ERR_PKG_ASSEMBLY` |
| **L** | Package Integrity & Security | Staged Package | Security & Malware Audit Cleared| `ERR_SEC_QUARANTINE` |
| **M** | Pre-Application Readiness Review | Package + User Approval Receipt | Signed Execution Authorization | `ERR_APPROVAL_MISSING` |
| **N** | Application Handoff | Target Instance + Signed Auth | Filesystem Transaction Journal | `ERR_HANDOFF_FAILED` |
| **O** | Post-Application Verification | Applied Filesystem State | Verification Evidence Report | `ERR_POST_VERIFY_FAIL` |
| **P** | Recovery & Provenance Closure | Journal + Evidence Report | Final Applied Modification Rec | `ERR_RECOVERY_ABORT` |

---

## 3. Pipeline State Machine & Transition Rules

```
       [IDLE]
         |
         | (Initiate Pipeline)
         v
    [PROCESSING] <---------------------------------------------+
         |                                                     |
         +------------+-----------------------+                |
         |            |                       |                |
   (Stage Error) (Stage Error)                |                |
   (Recoverable) (Fatal)                      | (Next Stage)   |
         |            |                       |                |
         v            v                       |                |
    [DEGRADED]   [FAILED]                     +----------------+
         |            |
   (User Bypass/      | (Trigger Rollback)
    Manual Fix)       v
         |       [ROLLING_BACK]
         |            |
         |            v
         +------> [ROLLED_BACK]
                      |
                      v
                 [TERMINATED]
```

### State Definitions:
1. `PIPELINE_INIT`: Environment initialized, baseline uncommitted.
2. `PIPELINE_PROCESSING`: Actively executing a specific stage $[A..P]$.
3. `PIPELINE_PAUSED`: Waiting for human-in-the-loop approval at Stage M.
4. `PIPELINE_COMPLETED`: Successfully executed through Stage P, all invariants satisfied.
5. `PIPELINE_DEGRADED`: Non-fatal warning encountered (e.g. optional audio transcode skipped).
6. `PIPELINE_FAILED`: Unrecoverable error encountered; execution halted.
7. `PIPELINE_ROLLING_BACK`: Active rollback in progress, restoring installation snapshot.
8. `PIPELINE_ROLLED_BACK`: Rollback verified bit-for-bit against pre-modification baseline.

---

## 4. Invariant Enforcement Across Stages

- **Stage Guard `GRD-STG-A`:** No stage may execute without the complete, verified output artifacts of all preceding stages.
- **Stage Guard `GRD-STG-H`:** All transformations must occur within an isolated sandbox directory (`.staging/transcode_<run_id>`). Adapters are never allowed direct write access to the host game directory.
- **Stage Guard `GRD-STG-M`:** Physical filesystem modification (Stage N) is physically impossible without a valid, cryptographically signed `UserApprovalReceipt` generated at Stage M.
- **Stage Guard `GRD-STG-O`:** If post-application verification fails, the pipeline MUST immediately enter `PIPELINE_ROLLING_BACK`.
