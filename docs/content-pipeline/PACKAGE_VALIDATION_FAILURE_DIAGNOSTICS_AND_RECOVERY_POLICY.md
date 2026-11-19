# Package Validation Failure, Diagnostics & Recovery Policy

**Document Identifier:** `NLE-POL-PVF-001`  
**Classification:** Reliability Engineering & Diagnostics Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-POL-GMF-001`, `NLE-CON-CPS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Diagnostic Architecture

This policy defines the comprehensive failure taxonomy, diagnostic bundle format, and safe recovery workflows for package validation and dependency resolution.

---

## 2. Package Validation Failure Taxonomy (34 Failure Codes)

| Failure Code | Severity | Description | Downstream Impact | Recovery Path |
| :--- | :--- | :--- | :--- | :--- |
| `ERR_PVA_ART_CORRUPT` | FATAL_ASSET | Member artifact bytes corrupted or truncated. | Assembly Aborted | Re-run Stage H transcode |
| `ERR_PVA_META_MISSING` | RECOVERABLE | Required metadata missing from member record.| Candidate Blocked | Supply missing metadata |
| `ERR_PVA_SCHEMA_UNSUPP` | RECOVERABLE | Manifest schema version unsupported. | Halted | Upgrade manifest parser |
| `ERR_PVA_INTEG_MISMATCH`| FATAL_ASSET | Member SHA-256 does not match digest record.| Member Rejected | Re-transcode or re-download |
| `ERR_PVA_PRV_INVALID` | FATAL | Provenance chain incomplete or unverified. | Assembly Halted | Provide signed attestation |
| `ERR_PVA_AUTH_REVOKED` | FATAL | User revoked Gate G5 packaging authorization.| Assembly Halted | Re-authorize task contract |
| `ERR_PVA_EVID_STALE` | RECOVERABLE | Member validation evidence expired/stale. | Invalidate & Retest| Re-run Stage I validation |
| `ERR_PVA_DEP_MISSING` | RECOVERABLE | Required dependency cannot be found. | Stage J Paused | Download required package |
| `ERR_PVA_DEP_INCOMPAT` | RECOVERABLE | Dependency version constraint unsatisfied. | Stage J Paused | Adjust SemVer range |
| `ERR_PVA_DEP_CONFLICT` | RECOVERABLE | Two members demand mutually exclusive mods.| Stage J Halted | Select non-conflicting mod |
| `ERR_PVA_DAG_CYCLE` | FATAL | Dependency loop detected in DAG. | Stage J Aborted | Break circular dependency |
| `ERR_PVA_PROVIDER_AMBIG`| RECOVERABLE | Multiple providers equally valid without rule.| Paused | Explicitly select candidate |
| `ERR_PVA_DUP_IDENTITY` | RECOVERABLE | Duplicate package ID in local registry. | Halted | Namespace package ID |
| `ERR_PVA_TARGET_MISMATCH`|RECOVERABLE | Member compiled for different game build. | Member Blocked | Re-target asset compilation |
| `ERR_PVA_DEST_CONFLICT` | RECOVERABLE | Two members claim identical relative path. | Stage K Halted | Define explicit override |
| `ERR_PVA_ORDER_INVALID` | RECOVERABLE | Violated topological ordering constraint. | Re-order Members | Enforce topological sort |
| `ERR_PVA_EXT_UNSAFE` | FATAL_SEC | Manifest extension contains banned script.| Output Quarantined | Remove executable extension |
| `ERR_PVA_RES_EXCEEDED` | RECOVERABLE | Package size exceeds 10 GB container limit.| Assembly Halted | Split package into chunks |
| `ERR_PVA_VAL_TIMEOUT` | RECOVERABLE | Validation worker timed out after 120s. | Worker Reset | Increase timeout / retry |
| `ERR_PVA_INPUT_CHANGED` | RECOVERABLE | Member artifact mutated during assembly scan.| Invalidate & Retry | Re-freeze member revisions |
| `ERR_PVA_INV_INCOMPLETE`| RECOVERABLE | Manifest lists member not present in archive.| Stage L Failed | Include missing member file |
| `ERR_PVA_POLICY_BLOCK` | FATAL | Transitive license incompatibility (GPL/Prop).| Assembly Blocked | Align licensing terms |
| `ERR_PVA_ZIP_SLIP` | FATAL_SEC | Member path contains `..` directory escape.| Immediate Quarantine| Sanitize relative path |
| `ERR_PVA_ZIP_BOMB` | FATAL_SEC | Container compression ratio exceeds 100:1. | Archive Aborted | Lower compression ratio |
| `ERR_PVA_EXEC_PAYLOAD` | FATAL_SEC | Member contains unapproved `.exe` or `.dll`.| Immediate Quarantine| Remove executable binary |
| `ERR_PVA_WAIVER_DENIED` | FATAL | Attempted warning waiver on critical check. | Gate Lock | Comply with mandatory rule |
| `ERR_PVA_RUNTIME_UNPROV`| WARNING | Package valid statically; runtime unverified.| Valid with Warning | Run game smoke test |
| `ERR_PVA_DIAMOND_FAIL` | RECOVERABLE | Diamond dependency resolution found no inter.| Stage J Halted | Unify dependency versions |
| `ERR_PVA_OPT_INCOMPAT` | RECOVERABLE | Optional member conflicts with required one. | Member Excluded | Emit PackageExclusionRecord |
| `ERR_PVA_CANCELLED` | RECOVERABLE | User cancelled assembly mid-operation. | Staging Cleaned | User re-initiates assembly |
| `ERR_PVA_CONCURRENCY` | RECOVERABLE | Concurrent process altered staging files. | Abort & Retry | Re-run in clean workspace |
| `ERR_PVA_AUDIT_FAIL` | FATAL | Storage write error recording audit record. | Pipeline Halted | Verify AppData permissions |
| `ERR_PVA_MIGRATION_FAIL`|RECOVERABLE | Manifest schema migration transform failed. | Halted | Correct schema syntax |
| `ERR_PVA_GATE_LOCKED` | FATAL | Downstream handoff attempted while BLOCKED. | Halted | Resolve blocking failures |

---

## 3. Diagnostic Dossier Specification (`.nlepvf`)

When package validation fails or encounters non-fatal warnings, the validator packages a `.nlepvf` archive containing:
- `validation_trace.json`: Detailed step-by-step evaluation log of all 15 assembly steps.
- `conflict_graph.json`: Visualizable JSON representation of conflicting dependency paths.
- `manifest_diff.json`: Comparison between declared manifest inventory and actual staged files.
- `sanitized_error_report.json`: Formatted error messages with all personal user paths redacted.
