# Content Transformation Failure, Resource & Diagnostic Policy

**Document Identifier:** `NLE-POL-TFD-001`  
**Classification:** Reliability Engineering & Diagnostics Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-CON-CPS-001`, `NLE-POL-GMF-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Reliability Architecture

This policy defines the comprehensive failure taxonomy, resource quotas, safe retry semantics, partial output isolation, and diagnostic bundle formats for content transformation and format conversion.

---

## 2. Transformation Failure Taxonomy (34 Failure Codes)

| Failure Code | Severity | Description | Downstream Effect | Recovery Path |
| :--- | :--- | :--- | :--- | :--- |
| `ERR_TRF_SRC_INVALID` | FATAL | Source asset failed Step 62 validation. | Transformation Aborted | Re-run Stage E validation |
| `ERR_TRF_SRC_MUTATED` | RECOVERABLE | Source file hash changed mid-transcode. | Abort & Invalidate | Re-queue transcode job |
| `ERR_TRF_AUTH_MISSING` | FATAL | Gate G4 transformation auth missing. | Immediate Halt | Request task authorization |
| `ERR_TRF_FMT_UNSUPPORTED`| RECOVERABLE| Source format unrecognized by adapter. | Skip if Optional | Register compatible adapter |
| `ERR_TRF_FMT_VERSION` | RECOVERABLE | Source schema version unsupported. | Halted | Downgrade or normalize schema |
| `ERR_TRF_TGT_UNSUPPORTED`| RECOVERABLE| Target format cannot be generated. | Halted | Select alternative pathway |
| `ERR_TRF_SCHEMA_MISMATCH`| RECOVERABLE| Intermediate IR schema version conflict. | Halted | Upgrade intermediate schema |
| `ERR_TRF_DEF_UNAVAILABLE`| RECOVERABLE| Required transformation def not registered.| Paused | Register transformation definition|
| `ERR_TRF_ADAPTER_MISSING`| RECOVERABLE| Required adapter plugin not installed. | Paused | Install required adapter SDK |
| `ERR_TRF_MAP_MISSING` | RECOVERABLE | No mapping rule for required property. | Halted | Author missing semantic mapping |
| `ERR_TRF_MAP_AMBIGUOUS` | RECOVERABLE | Multiple conflicting mapping rules found.| Halted | Explicitly specify mapping ID |
| `ERR_TRF_META_INSUFFICIENT`|RECOVERABLE| Required source metadata missing. | Candidate Blocked | Supply supplemental metadata |
| `ERR_TRF_DEFAULT_INVALID`| RECOVERABLE | Computed default value violates schema. | Candidate Blocked | Correct default value formula |
| `ERR_TRF_DEP_UNAVAILABLE`| RECOVERABLE | Required material/texture missing in DAG.| Stage G Paused | Resolve missing dependency |
| `ERR_TRF_DAG_CYCLE` | FATAL | Circular dependency loop in transform DAG.| Stage G Aborted | Break circular dependency loop |
| `ERR_TRF_PATH_CONFLICT` | RECOVERABLE | Two conversion paths yield conflicting out.| Halted | Explicitly select conversion path |
| `ERR_TRF_STR_FAILED` | FATAL_ASSET | Generated output failed syntax/grammar. | Output Quarantined | Debug adapter transcode worker |
| `ERR_TRF_SEM_FAILED` | FATAL_ASSET | Transformed mesh bounds exceed tolerance.| Output Quarantined | Correct scaling/origin in mapping |
| `ERR_TRF_PROP_UNSUPPORTED`|RECOVERABLE| Critical property cannot be represented.| Halted | Re-export with supported feature |
| `ERR_TRF_LOSS_UNAPPROVED`| RECOVERABLE | Lossiness Tier 3 without user approval. | Paused at Stage I | Prompt user for loss approval |
| `ERR_TRF_INTEG_MISMATCH` | FATAL_ASSET | Output digest does not match worker hash.| Output Quarantined | Re-run conversion worker |
| `ERR_TRF_RES_EXCEEDED` | RECOVERABLE | Transcoder exceeded 4 GB RAM ceiling. | Worker Terminated | Chunk asset or optimize LOD |
| `ERR_TRF_TIMEOUT` | RECOVERABLE | Conversion exceeded 120-second timeout. | Worker Terminated | Increase timeout or reduce model |
| `ERR_TRF_PARTIAL_OUTPUT` | RECOVERABLE | Worker crashed leaving incomplete file. | Staging Cleaned | Inspect crash dump; retry |
| `ERR_TRF_OUTCOME_UNCERTAIN`|RECOVERABLE| Worker exited without status code. | State Quarantined | Re-verify staging directory |
| `ERR_TRF_OUTPUT_CORRUPT` | FATAL_ASSET | Generated file unparseable by target tools.| Output Quarantined | File bug report with adapter |
| `ERR_TRF_LICENSE_BLOCK` | FATAL | Source license restricts derivative work.| Conversion Denied | Check SPDX licensing terms |
| `ERR_TRF_SEC_VIOLATION` | FATAL_SEC | Worker attempted unauthorized socket/exec.| Immediate Quarantine| Audit adapter security sandbox |
| `ERR_TRF_DEF_CHANGED` | RECOVERABLE | Definition version incremented mid-run. | Invalidate & Retry | Re-run with active definition |
| `ERR_TRF_ADAPTER_CHANGED`| RECOVERABLE | Adapter binary updated during execution.| Invalidate & Retry | Re-run with updated adapter |
| `ERR_TRF_EVID_STALE` | RECOVERABLE | Target game patched invalidating output. | Re-validate Output | Run smoke test against new patch |
| `ERR_TRF_VERIFY_FAILED` | FATAL_ASSET | Independent verification check failed. | Output Blocked | Review validation report |
| `ERR_TRF_AUDIT_FAILED` | FATAL | Ledger failure writing provenance log. | Pipeline Halted | Verify local app data permissions |
| `ERR_TRF_ELIGIBILITY_DENIED`|FATAL | Asset failed 10 precondition checks. | Gate Lock | Review precondition failure list |

---

## 3. Safe Retries & Resource Ceilings

- **Transient I/O Retry:** Recoverable errors (`ERR_TRF_TIMEOUT`, worker crash) may be retried up to 2 times with exponential backoff ($2\text{s}, 4\text{s}$).
- **Deterministic Halt:** Syntax, semantic, or licensing errors are NEVER automatically retried.
- **Resource Hard Ceilings:** 4 GB RAM per worker, 25 GB staging cache, 4 GB per single output asset.
