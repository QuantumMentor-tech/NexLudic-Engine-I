# Content Discovery Failure, Resource & Diagnostic Policy

**Document Identifier:** `NLE-POL-CDF-001`  
**Classification:** Reliability Engineering & Diagnostics Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-CON-CPS-001`, `NLE-POL-GMF-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Diagnostic Architecture

This policy defines the comprehensive failure taxonomy, resource quotas, partial discovery reporting rules, safe retry semantics, and diagnostic bundle generation for content discovery and asset validation.

---

## 2. Ingestion & Discovery Failure Taxonomy (34 Failure Codes)

| Failure Code | Severity | Description | Downstream Effect | Recovery Path |
| :--- | :--- | :--- | :--- | :--- |
| `ERR_SRC_AUTH_MISSING` | FATAL | Source not authorized in task contract. | Discovery Aborted | Add source to task authorization |
| `ERR_SRC_UNAVAILABLE` | RECOVERABLE | Source directory or network URI unreachable.| Partial Discovery | Verify drive mount or network |
| `ERR_SRC_AMBIGUOUS` | RECOVERABLE | Multiple sources match ambiguous locator.| Halted | Specify exact canonical source ID |
| `ERR_SRC_MUTATED` | RECOVERABLE | Source file bytes changed during scan. | Invalidate & Retry | Re-run discovery from clean state |
| `ERR_SRC_DIR_ESCAPE` | FATAL_SEC | Source attempts traversal outside root. | Immediate Quarantine| Sanitize path; alert admin |
| `ERR_CAND_ID_AMBIGUOUS`| RECOVERABLE | Multiple candidates claim same filename.| Disambiguate | Apply package namespace prefix |
| `ERR_CAND_COLLISION` | RECOVERABLE | Two candidates claim same target path. | Halted at Stage J | User side-by-side diff resolution|
| `ERR_META_MISSING_REQ` | RECOVERABLE | Required metadata fields absent. | Candidate Blocked | Provide supplemental manifest |
| `ERR_META_MALFORMED` | RECOVERABLE | Syntax error in JSON/XML metadata chunk. | Candidate Rejected | Repair metadata formatting |
| `ERR_META_UNSUPPORTED` | RECOVERABLE | Schema version unrecognized by adapter. | Candidate Blocked | Upgrade adapter schema parser |
| `ERR_META_INJECTION` | FATAL_SEC | Malicious prompt/shell injection detected. | Immediate Quarantine| Reject asset; log incident |
| `ERR_FMT_UNKNOWN` | RECOVERABLE | Magic bytes do not match any known format.| Skip or Reject | Register custom adapter parser |
| `ERR_FMT_UNSUPPORTED` | RECOVERABLE | Format known but unsupported by target game.| Skip if Optional | Convert via intermediate format |
| `ERR_FMT_VERSION_MIS` | RECOVERABLE | Format version unsupported (e.g. UE 5.4). | Candidate Blocked | Downscale or retarget asset |
| `ERR_PARSER_UNAVAIL` | RECOVERABLE | Required adapter parser not installed. | Pipeline Paused | Install required adapter SDK |
| `ERR_STR_CORRUPT` | FATAL_ASSET | Binary chunk length or header corrupted. | Candidate Rejected | Re-export asset from DCC tool |
| `ERR_STR_DEGENERATE` | RECOVERABLE | Degenerate triangles or NaN coordinates. | Candidate Rejected | Clean mesh topology in Blender |
| `ERR_SEM_REF_BROKEN` | RECOVERABLE | Internal texture reference points to null.| Fallback Material | Provide missing texture asset |
| `ERR_PRV_MISSING` | RECOVERABLE | Source provenance or author unknown. | Needs Review | Provide author attestation |
| `ERR_PRV_INTEG_FAIL` | FATAL_ASSET | Computed SHA-256 does not match manifest. | Candidate Rejected | Re-download uncorrupted package |
| `ERR_PRV_SIG_INVALID` | FATAL_SEC | Ed25519 signature verification failed. | Quarantined | Re-sign with authorized release key|
| `ERR_AUTH_SCOPE_MIS` | FATAL | Asset authorized for local read, not pack. | Gate Lock | Request elevated user approval |
| `ERR_AUTH_LICENSE_UNR` | RECOVERABLE | SPDX license unresolved or ambiguous. | G8/G9 Locked | Add valid SPDX identifier |
| `ERR_DEP_UNRESOLVED` | RECOVERABLE | Required texture/mesh dependency missing. | Stage J Pause | Download required dependency |
| `ERR_DEP_CYCLE_DETECT` | FATAL_ASSET | Circular dependency loop in asset graph. | Stage J Abort | Break circular reference in DAG |
| `ERR_DEP_CONFLICT` | RECOVERABLE | Two dependencies require conflicting vers. | Stage J Pause | Align package SemVer constraints |
| `ERR_TGT_INCOMPATIBLE` | RECOVERABLE | Asset incompatible with target game build. | Candidate Blocked | Select compatible game build |
| `ERR_TGT_STALE_EVID` | RECOVERABLE | Game update invalidated compatibility ev. | Invalidate & Retest| Run automated smoke verification |
| `ERR_SEC_SCRIPT_UNTR` | FATAL_SEC | Script asset from untrusted Tier 3 source.| Immediate Rejection| Require Tier 1/2 signed origin |
| `ERR_SEC_AST_BANNED` | FATAL_SEC | Script contains banned OS/network calls. | Immediate Quarantine| Remove banned system calls |
| `ERR_SEC_ZIP_SLIP` | FATAL_SEC | Archive entry contains `..` path escape. | Archive Quarantined| Repackage archive with safe paths |
| `ERR_SEC_ZIP_BOMB` | FATAL_SEC | Compression ratio exceeds 100:1 ceiling. | Archive Aborted | Reject decompression bomb |
| `ERR_RES_SIZE_EXCEED` | RECOVERABLE | Asset file size exceeds 4 GB ceiling. | Candidate Blocked | Chunk or optimize asset |
| `ERR_RES_TIMEOUT` | RECOVERABLE | Metadata extraction exceeded worker timeout.| Worker Reset | Increase timeout or reduce LOD |

---

## 3. Partial Discovery Reporting Contract

When a discovery request inspects multiple sources and some fail:
1. **Never Report False Completeness:** The engine must explicitly record `result_status: PARTIAL_DISCOVERY_COMPLETED`.
2. **Isolation of Valid Candidates:** Validly discovered candidates from accessible sources are retained and assigned provisional status.
3. **Source Gap Accounting:** Every skipped, timed-out, or unreadable source is cataloged in `uninspected_sources[]` with error diagnostics.

---

## 4. Diagnostic Bundle Specification (`.nledisc`)

When discovery fails or encounters warnings, a diagnostic bundle is generated containing:
- `discovery_trace.json`: Full log of scanned paths, magic byte results, and timings.
- `candidate_manifest.json`: List of all identified candidates and their 12-dimension evaluation.
- `sanitized_error_report.json`: Cleaned error messages with all absolute user paths redacted to relative tokens.
