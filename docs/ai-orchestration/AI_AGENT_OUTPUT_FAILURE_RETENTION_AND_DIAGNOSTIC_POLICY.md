# NexLudic Engine: AI Agent Output Failure, Retention & Diagnostic Policy

**Document ID:** `NLE-POL-AOF-001`  
**Phase:** 7 of 12 (AI Coding-Agent Orchestration)  
**Step:** 59 of 100  
**Classification:** Diagnostic Forensics & Data Retention Policy  
**Status:** IMPLEMENTATION-READY SPECIFICATION  
**Upstream Dependencies:** `NLE-SPC-AOC-001` (Master Spec), `NLE-CON-ATA-001` (Step 58 Audit Contract)  
**Downstream Dependents:** Step 60 Phase Gate  

---

## 1. Output Failure Taxonomy (Failures F-01 through F-28)

The verification subsystem defines an exhaustive catalog of 28 structured failure classifications:

| Failure ID | Failure Classification | Detection Point | Root Cause & Diagnostic Indicator |
| :--- | :--- | :--- | :--- |
| **F-01** | `ERR_MISSING_FINAL_RESULT` | Result Ingestion | Session exited without emitting a formal result submission message. |
| **F-02** | `ERR_INVALID_SUBMISSION_STRUCTURE` | Schema Validation | Submission JSON payload violates Draft 2020-12 schema. |
| **F-03** | `ERR_SESSION_IDENTITY_MISMATCH` | Identity Gate | Submission session ID does not match active supervisor session. |
| **F-04** | `ERR_UNSUPPORTED_ARTIFACT_TYPE` | Artifact Ingest | Agent submitted unknown file type or disallowed binary format. |
| **F-05** | `ERR_ARTIFACT_NOT_FOUND_ON_DISK` | Resolution Gate | Claimed file does not exist in worktree directory. |
| **F-06** | `ERR_ARTIFACT_DIGEST_MISMATCH` | Integrity Check | Physical file hash does not match claimed hash. |
| **F-07** | `ERR_UNKNOWN_ARTIFACT_PROVENANCE` | Provenance Audit | Artifact cannot be linked to an authorized tool invocation. |
| **F-08** | `ERR_WORKSPACE_BOUNDARY_BREACH` | Path Containment | Submitted file path points outside workspace staging directory. |
| **F-09** | `ERR_MANDATORY_DELIVERABLE_MISSING` | Completeness Gate | Required deliverable from task contract not submitted. |
| **F-10** | `ERR_UNAUTHORIZED_SCOPE_EXPANSION` | Diff Audit | Modifications made to files outside task `write_roots`. |
| **F-11** | `ERR_INVALID_TEST_REPORT` | Evidence Ingest | Test report XML/JSON corrupted or missing execution metadata. |
| **F-12** | `ERR_UNEXECUTED_TEST_CLAIMED` | Evidence Audit | Agent reported test passed, but supervisor logs confirm no execution. |
| **F-13** | `ERR_STALE_VERIFICATION_EVIDENCE` | Freshness Check | Test evidence generated on older commit SHA than current files. |
| **F-14** | `ERR_UNIT_TEST_FAILURE` | Test Evaluation | Test runner exited with non-zero status or failed assertions. |
| **F-15** | `ERR_COMPILER_BUILD_FAILURE` | Static Analysis | `cargo check` or `mypy` reported compilation errors. |
| **F-16** | `ERR_LINTER_STYLE_VIOLATION` | Static Analysis | Code violates mandatory engine formatting rules. |
| **F-17** | `ERR_CONTRADICTORY_REFERENCES` | Document Audit | Submitted document contradicts approved ADR or specification. |
| **F-18** | `ERR_ACCEPTANCE_CRITERIA_UNMET` | Criteria Eval | Mandatory acceptance criterion evaluated to `NOT_SATISFIED`. |
| **F-19** | `ERR_SECURITY_SECRET_LEAK` | Secret Scanner | Submitted code/document contains API keys or passwords. |
| **F-20** | `ERR_SECURITY_PROHIBITED_BEHAVIOR` | Security Audit | Submitted code contains process injection or anti-cheat tampering. |
| **F-21** | `ERR_DEPENDENCY_LICENSE_VIOLATION` | License Check | Dependency introduced with viral copyleft license (GPLv3). |
| **F-22** | `ERR_VERIFICATION_ENV_UNAVAILABLE` | Pre-Check | Host compiler or test framework missing from host environment. |
| **F-23** | `ERR_INDEPENDENT_REVIEW_PENDING` | Review Gate | Required human review has not been completed. |
| **F-24** | `ERR_UNCERTAIN_EXECUTION_OUTCOME` | Outcome Gate | External tool side-effect remained unconfirmed. |
| **F-25** | `ERR_AUDIT_LEDGER_WRITE_FAILURE` | Audit Ingest | Local audit ledger full or write-protected; fail-closed. |
| **F-26** | `ERR_PRECONDITION_PHASE6_UNVERIFIED` | Gate Check | Task implements unverified Phase 6 mashup integration plan. |
| **F-27** | `ERR_PARTIAL_EXECUTION_LEAK` | Cleanup Check | Child process lingered after test execution completed. |
| **F-28** | `ERR_REMEDIATION_LIMIT_EXCEEDED` | Lifecycle Gate | Task failed verification across 3 consecutive attempts. |

---

## 2. Diagnostic Bundle Packaging & Evidence Retention

Upon failure or rejection:
1. **Automated Diagnostic Bundle:** The supervisor packages:
   - `submission_payload.json`
   - `git_diff_full.patch`
   - `compiler_stderr.log` / `test_runner.xml`
   - `acceptance_evaluation_report.json`
   into `.nexludic/diagnostics/verification/diag_<task_id>_<submission_id>.zip`.
2. **Local Retention Period:** Retained for **30 days** on local disk (`%LOCALAPPDATA%\NexLudic\diagnostics\`).
3. **Zero Secret Leakage:** Diagnostic logs are sanitized by the secret redaction engine before bundle creation.
