# Host-Game Execution Synthetic QA Test Plan

**Document Identifier:** `NLE-CAT-ETA-001`  
**Lifecycle Stage:** Content Pipeline Stage E Quality Assurance  
**Parent Document:** [HOST_GAME_EXECUTION_AUTHORIZATION_PRECONDITION_REVALIDATION_AND_TRANSACTIONAL_APPLICATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_EXECUTION_AUTHORIZATION_PRECONDITION_REVALIDATION_AND_TRANSACTIONAL_APPLICATION_SPECIFICATION.md) (`NLE-SPC-ETA-001`)  
**Test Suite Coverage:** Scenarios A through AM (39 Exhaustive Test Scenarios)  
**System Status:** Authoritative Architectural Test Specification  
**Classification:** Verification Procedures & Synthetic Test Suite  

---

## 1. Executive Summary & Test Governance

The **Host-Game Execution Synthetic QA Test Plan** establishes the comprehensive synthetic QA test matrix for Stage E of the Content Pipeline. It validates execution authorization, last-moment preflight revalidation, operation sequencing, pseudo-transactional application, atomic renaming, pre-mutation backups, concurrency locking, cancellation handling, partial-failure governance, and downstream verification handoff.

This suite defines 39 discrete, reproducible synthetic test scenarios (**Scenarios A through AM**). All tests execute within isolated mock harnesses using synthetic test files; zero operations are executed against real game installations.

---

## 2. Synthetic Test Scenario Matrix (Scenarios A through AM)

| Scenario ID | Title | Domain Area | Input / Condition | Expected Behavior & Diagnostic Code | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **A** | Fully Authorized Request | Golden Path | Valid grant, matching package & plan, verified preflight | Pass; reaches EXECUTION_READY (no live mutation) | Designed |
| **B** | No Execution Authorization | Authorization | Execution requested without signed grant | Abort; `ERR_EAR_AUTHORIZATION_INVALID` | Designed |
| **C** | Approval for Other Plan Revision | Scope Binding | Grant signed for Plan Rev 1, request has Rev 2 | Abort; `ERR_EAR_SCOPE_MISMATCH` | Designed |
| **D** | Approval for Other Target | Scope Binding | Grant signed for Skyrim Inst A, applied to Inst B | Abort; `ERR_EAR_SCOPE_MISMATCH` | Designed |
| **E** | Authorization Revoked Pre-Exec | Revocation | User revokes grant before preflight completes | Abort; `ERR_EAR_GRANT_REVOKED` | Designed |
| **F** | Destructive Change Lacks Approval | Authorization | Plan has REPLACE, but grant omits op ID | Abort; `ERR_EAR_UNAUTHORIZED_DESTRUCTIVE` | Designed |
| **G** | Operation List Expanded Post-Approval| Scope Binding | Request contains 483 ops, grant authorized 482 | Abort; `ERR_EAR_SCOPE_MISMATCH` | Designed |
| **H** | Physical Package Altered Post-Verif | Revalidation | Package container modified on disk | Abort; `ERR_LPR_PACKAGE_DRIFT` | Designed |
| **I** | Plan Altered Post-Assessment | Revalidation | Plan JSON modified after authorization | Abort; `ERR_LPR_PLAN_DRIFT` | Designed |
| **J** | Target Release Changed | Revalidation | Game executable patched (version drift) | Abort; `ERR_LPR_TARGET_BUILD_DRIFT` | Designed |
| **K** | Target Resource Changed Post-Plan | Revalidation | Destination file modified by external actor | Abort; `ERR_LPR_BASELINE_MISMATCH` | Designed |
| **L** | Target Identity Ambiguous | Revalidation | Target instance cannot be uniquely verified | Abort; `ERR_TIS_AMBIGUOUS_TARGET` | Designed |
| **M** | Safety Validator Unavailable | Preflight Gate | Hash verifier service offline / uncontactable | Abort; `ERR_LPR_VALIDATOR_UNAVAILABLE` | Designed |
| **N** | Destination Escapes Boundary | Containment | Operation path targets `Data/../../Windows/` | Abort; `ERR_TPC_TRAVERSAL_DETECTED` | Designed |
| **O** | Valid Dependency Ordering | Sequencing | Operations topologically ordered (dirs -> files) | Pass; valid deterministic execution sequence | Designed |
| **P** | Circular Operation Prerequisites | Sequencing | Op 1 depends on Op 2, and Op 2 depends on Op 1 | Abort; `ERR_APD_CYCLE_DETECTED` | Designed |
| **Q** | Required Operation Omitted | Completeness | Request attempts to drop a mandatory change | Abort; `ERR_ERL_INPUT_REVISION_MISMATCH` | Designed |
| **R** | Unapproved Extra Operation | Scope Enforcement | Request injects unapproved file addition | Abort; `ERR_EAR_SCOPE_MISMATCH` | Designed |
| **S** | Prior Content Differs from Baseline | Content Protection | Destination file hash differs from expected | Abort; `ERR_LPR_BASELINE_MISMATCH` (No overwrite) | Designed |
| **T** | Multiple Ops on Same Destination | Sequencing | Two ops in same session target identical path | Abort; `ERR_OST_DESTINATION_COLLISION` | Designed |
| **U** | Crash Before First Mutation | Resilience | Process crash simulated during preflight | Target remains 100% pristine; journal logged | Designed |
| **V** | Operation Fails Post-Mutation | Partial Failure | Op 1 succeeds, Op 2 fails on I/O error | Halt; status `EXECUTION_PARTIALLY_APPLIED` | Designed |
| **W** | Operation Partially Changes Resource | Partial Failure | File truncated mid-write before rename | Target intact (atomic rename); error recorded | Designed |
| **X** | Cancel Before First Modification | Cancellation | SIG_CANCEL received during preflight | Transition to `EXECUTION_CANCELLED`; 0 writes | Designed |
| **Y** | Cancel After One Operation Committed | Cancellation | SIG_CANCEL received after Op 1 applied | Stop between ops; status `PARTIALLY_APPLIED` | Designed |
| **Z** | External Mutation During Execution | Concurrency | External process alters file between ops | Detect drift; halt with `ERR_CCP_EXTERNAL_MUTATION`| Designed |
| **AA**| Concurrent Requests on Same Target | Concurrency | Session 2 attempts lock while Session 1 active | Reject Session 2: `ERR_CCP_CONCURRENT_SESSION` | Designed |
| **AB**| Operation Timeout (Unknown Result) | Fault Handling | File write hangs; timeout fires | Halt; mark `EXECUTION_OUTCOME_UNCERTAIN` | Designed |
| **AC**| Journal Storage Unavailable | Audit Integrity | Disk full on journal volume during write | Halt; block further ops (`ERR_EJR_JOURNAL_WRITE`) | Designed |
| **AD**| Recovery Prerequisite Missing | Recovery Gate | REPLACE operation lacks backup storage space | Abort before mutation (`ERR_ERP_BACKUP_MISSING`) | Designed |
| **AE**| Recovery Attempt Outcome Uncertain | Recovery | Recovery script fails mid-restoration | Retain original execution journal; flag UNRESOLVED | Designed |
| **AF**| Success Pending Independent Verif | Handoff | All ops committed; Stage V not yet run | Emit `EXECUTION_COMPLETED_PENDING_VERIF` | Designed |
| **AG**| Stage V Verification Finds Mismatch | Downstream Gate | Post-verification detects altered file hash | Execution logged; discrepancy routed to Stage V | Designed |
| **AH**| Attempted Package Code Execution | Security Boundary | Package attempts to launch `install.bat` | Block code execution; `ERR_ESP_CODE_EXECUTION` | Designed |
| **AI**| AI-Generated Request Exceeds Scope | AI Safety | AI submits request with extra operations | Deterministic rejection: `ERR_EAR_SCOPE_MISMATCH` | Designed |
| **AJ**| Exit 0 Without Complete Journal | Audit Integrity | Process returns 0, but journal missing commit | Reject completion: `ERR_EJR_INCOMPLETE_JOURNAL` | Designed |
| **AK**| Package Valid, Target Incompatible | Compatibility | Valid `.nlpkg` applied to mismatched target | Block execution: `ERR_TVP_TARGET_INCOMPATIBLE` | Designed |
| **AL**| Approved Plan Stale | Freshness | Target snapshot lease expired (> 1800s) | Invalidate: `ERR_TEF_LEASE_EXPIRED` | Designed |
| **AM**| Interruption with Uncertain Operations| Fault Handling | Power loss simulated during atomic rename | Mark `EXECUTION_OUTCOME_UNCERTAIN`; reconcile | Designed |

---

## 3. Test Harness Invariant

All 39 scenarios must achieve 100% specification compliance in synthetic mock harnesses before Stage E execution can be declared production-ready.
