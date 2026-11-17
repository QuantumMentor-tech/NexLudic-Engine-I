# Operation Retry and Error Recovery Specification

**Document ID:** NLE-SPC-REC-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Classification:** UI Architecture — Recovery Specification  

---

## 1. Purpose and Scope

This document defines the authoritative retry eligibility rules, failure classification matrix, resume vs. retry distinction, recovery workflows, and forbidden recovery actions for NexLudic Engine. It governs every recoverable failure path across all forms and operation surfaces.

**Scope:** All operations with outcomes in the OperationStatus enum (`FAILED`, `PARTIAL`, `UNCERTAIN`, `CANCELLED`). Input validation errors (CAT-01 through CAT-04) are recovery-destination concerns; their resolution is field correction, not retry of an operation.

**Authoritative references:** `NLE-SPC-FVR-001`, `NLE-SPC-FSM-001`, `NLE-DOM-SPEC-001`, `REQ-SEC-001`, `REQ-SAFE-001`.

---

## 2. Failure Classification Matrix

Before offering any recovery action, the failure must be classified by what is known about the operation's start and end state.

| Class | Code | Definition | Retry Safe? |
|---|---|---|---|
| Definitively Not Started | `DNS` | The operation did not begin; no state was modified | Yes — if input is valid |
| Started, Not Committed | `SNC` | The operation began but no durable write occurred | Yes — equivalent to DNS |
| Confirmed Failed | `CF` | The operation failed and all side effects were rolled back (or never occurred) | Yes — idempotency required |
| Partially Completed | `PC` | Some sub-operations succeeded and some failed | Partial — targeted retry of failed items only; blocked by GAP-FVR-002 |
| Confirmed Completed | `CC` | The operation succeeded | No retry needed |
| Outcome Unknown | `OU` | The result cannot be determined | No — blind retry prohibited; user decides |

### 2.1 Classification at Submission Boundaries

| NexLudic Operation | Typical Class on Failure |
|---|---|
| Project creation (pre-flight fails before first write) | `DNS` |
| Project creation (write begins, fails midway) | `PC` or `OU` |
| Capability requirement save | `DNS` or `CF` (depends on write atomicity) |
| Configuration section save | `CF` or `PC` |
| Adapter install | `PC` or `OU` |
| AI-assisted diff generation | `CF` or `OU` |
| AI-assisted analysis | `CF` |

**Architectural Note:** The specific classification of each NexLudic operation at the persistence layer depends on whether atomic writes or rollback capability is provided. This classification table is a design guide; implementation must confirm each classification against the actual persistence architecture (`NLE-PER-SPEC-001`).

---

## 3. Retry Eligibility Rules

An operation may be offered a Retry action **only** when all of the following conditions are met:

| # | Condition | Rationale |
|---|---|---|
| R-01 | Failure class is `DNS`, `SNC`, or `CF` | Only known-safe starting states |
| R-02 | No conflicting state change has occurred between failure and retry | Stale state check required |
| R-03 | The operation is idempotent, OR the architecture explicitly guarantees safe retry | Required to avoid duplicate side effects |
| R-04 | The user's current input is valid (passes Tier 1 + 2 validation) | No point retrying with invalid data |
| R-05 | The underlying resource or condition has been resolved (for CAT-05) | Retrying into the same resource failure is pointless |

**If any condition fails:** Do not offer a Retry button. Instead, present the appropriate non-retry recovery path (§7).

### 3.1 Idempotency Gap (GAP-FVR-002)

NexLudic Engine does not yet define idempotency tokens for file-write operations, adapter installs, or AI-generated diff applications. Until this gap is resolved:

- **Blind retry after `PARTIAL` or `UNCERTAIN` outcomes is prohibited.**
- Targeted retry after `PARTIAL` (class `PC`) is offered only when the architecture can confirm which sub-operations did not start (class `DNS`).
- Retry eligibility for `PARTIAL` and `UNCERTAIN` will be fully defined when idempotency tokens are introduced.

---

## 4. Resume vs. Retry Distinction

| Term | Definition | When to Use |
|---|---|---|
| **Retry** | Re-execute the entire operation from the beginning | Failure class is `DNS`, `SNC`, or `CF`; operation is idempotent |
| **Resume** | Continue an operation from the last confirmed checkpoint | Only when the architecture provides explicit checkpoint semantics |

**Current limitation:** NexLudic Engine does not define operation checkpoints in the current architecture. Resume is not an available recovery option in Phase 3. Retry (from start) is the only implemented recovery mechanism where eligible.

---

## 5. Retry Eligibility by Operation Category

### 5.1 Project Creation — Pre-flight Failure (CAT-04, CAT-05)

| Condition | Class | Retry Eligible |
|---|---|---|
| Anti-cheat hard block | `DNS` | No — not a transient failure; user must select different games |
| Disk space insufficient | `DNS` | Yes — after user frees space (R-05 required) |
| Path not writeable | `DNS` | Yes — after user corrects permissions (R-05 required) |
| Path not on D:\ | `DNS` | No — user must correct path (validation fix, not retry) |

**Recovery action for pre-flight failures:** Correct the condition → Re-run pre-flight → Confirm.

### 5.2 Project Metadata Save (CAT-06)

| Condition | Class | Retry Eligible |
|---|---|---|
| General write failure | `CF` or `DNS` | Yes — if R-01 through R-05 met |
| Disk full | `DNS` or `CF` | Yes — after user frees space |
| File locked | `DNS` | Yes — after process releases lock |
| Partial save | `PC` | Partial — per sub-item; blocked by GAP-FVR-002 |

**Retry action:** Re-submit the same form without requiring user to re-enter data (IsDirty preserved per INV-001).

### 5.3 Requirement Save (CAT-06)

Same eligibility as Project Metadata Save. Each requirement save is a discrete operation; partial saves list failed requirements.

### 5.4 Configuration Save (CAT-06)

Each configuration section saves independently. Failed section: eligible for retry if class `CF` or `DNS`. Partial configuration save: list which sections succeeded and which failed (CAT-08).

### 5.5 Adapter Install (CAT-06, CAT-05)

| Condition | Class | Retry Eligible |
|---|---|---|
| Adapter archive not found | `DNS` | Yes — after resolving file location |
| Install begins, fails midway | `PC` or `OU` | No — blocked by GAP-FVR-002 until idempotency tokens defined |
| Install confirmed failed with no side effects | `CF` | Yes |

### 5.6 AI-Assisted Analysis (CAT-06, CAT-07)

| Condition | Class | Retry Eligible |
|---|---|---|
| AI provider credential invalid | `DNS` | No — user must fix credential in settings |
| Analysis timed out | `OU` | Conditional — user decides after reviewing UNCERTAIN state |
| Analysis returned unexpected error | `CF` or `OU` | Per classification at time of failure |

### 5.7 AI-Assisted Diff Application (CAT-06, CAT-07, CAT-08)

**High-risk operation.** Diff application modifies project artifacts.

| Condition | Class | Retry Eligible |
|---|---|---|
| Diff generation fails (no write) | `DNS` | Yes |
| Diff application fails partway through | `PC` | No — blind retry may double-apply diffs; blocked by GAP-FVR-002 |
| Diff application outcome unknown | `OU` | No — UNCERTAIN must not auto-retry |

**Additional constraint:** Diff application results require user approval (`AWAITING_APPROVAL`) before commit. Retry of a partially applied diff requires explicit user review of current artifact state.

---

## 6. UNCERTAIN Outcome Handling

### 6.1 Mandatory Rules

1. `UNCERTAIN` must never be relabelled as `COMPLETED` or `FAILED` (VR-GLB-004).
2. No automatic retry may be initiated when outcome is `UNCERTAIN`.
3. IsDirty must remain unchanged (INV-001).
4. The user must be shown the `UNCERTAIN` status clearly, with a correlation ID.

### 6.2 User Options When Outcome is UNCERTAIN

| Option | Action | Guard |
|---|---|---|
| View Operation Log | Open operation log panel; review last known state | Always available |
| Retry (if safe) | Transition to DIRTY → VALIDATION_REQUESTED | Only if user confirms they understand the risk; only after consulting operation log |
| Accept Current State | Accept that the operation may have succeeded; set form to reflect current saved state via reload | Reload required to establish ground truth |
| Report / Export Diagnostics | Export correlation ID and log to file | Always available |

**Critical:** The "Retry" option for UNCERTAIN is a user-explicit decision after reviewing the log. It must not be an automatic button that fires without user acknowledgement. The acknowledgement dialog must clearly state: "The previous operation's outcome could not be confirmed. Retrying may duplicate the operation. Are you sure?"

---

## 7. Recovery Workflows by Category

### 7.1 CAT-01: Input Validation Error Recovery

1. `NleInlineValidation` shown below offending field.
2. Focus moved to first invalid field.
3. User corrects value.
4. Field re-validates on blur (Tier 1) and on next stage-exit attempt (Tier 2+).
5. When all errors resolved: form transitions from `INVALID` to `READY_TO_SUBMIT`.

### 7.2 CAT-02: Incomplete but Permitted — No Recovery Required

CAT-02 is not an error. No recovery workflow. User may optionally complete the field.

### 7.3 CAT-03: Warning — Non-Blocking

User reads warning and decides whether to proceed. No forced recovery action. Warning persists until dismissed or condition resolves.

### 7.4 CAT-04: Operation Rejection Recovery

1. Non-dismissible Critical banner shown.
2. For anti-cheat hard-block: user must select different games. No workaround.
3. For other rejections: user corrects the blocking condition or abandons the operation.
4. No retry mechanism for rejection conditions — the rejection itself is deterministic.

### 7.5 CAT-05: Resource Unavailable Recovery

1. Error banner shown with specific resource description (without internal path).
2. User resolves the resource issue outside NexLudic Engine (e.g., reconnects drive, checks folder permissions).
3. User returns to NexLudic Engine and triggers Re-run Pre-flight (or Retry if on a non-pre-flight operation).
4. Operation re-attempts if R-01 through R-05 met.

### 7.6 CAT-06: Persistence Failure Recovery

1. Error banner shown. IsDirty preserved (INV-001).
2. Retry button shown if failure class `CF` or `DNS` and R-01 through R-05 met.
3. If Retry offered: user clicks Retry → form transitions to `SUBMITTING` → awaits result.
4. If Retry not offered (e.g., PARTIAL with unknown sub-state): user shown per-item status; targeted resolution described.
5. Diagnostics export always available.

### 7.7 CAT-07: Unexpected Internal Error Recovery

1. Error banner with correlation ID shown.
2. Retry button offered (operation re-execute from clean state, class `DNS` assumed).
3. Diagnostics export offered.
4. If retry also fails: advise restart of NexLudic Engine; note correlation ID for support.

### 7.8 CAT-08: Partial Completion Recovery

1. Warning banner shown with per-item success/failure breakdown.
2. For succeeded items: shown with success indicator.
3. For failed items: shown with failure indicator and reason (CAT-01–CAT-07 as applicable).
4. Targeted retry offered per failed item (subject to GAP-FVR-002).
5. User may accept partial result and proceed (IsDirty set to false for succeeded items only, per INV-007).

### 7.9 CAT-09: Uncertain Outcome Recovery

As specified in §6.2. User reviews log, makes explicit decision, may retry with explicit acknowledgement.

### 7.10 CAT-10: Cancellation Recovery

1. Info banner shown confirming cancellation.
2. User input preserved (INV-001).
3. User may re-attempt the operation immediately without re-entering data.
4. No special guard required — IsDirty still true; Retry/Submit available.

### 7.11 CAT-11: Stale Information Recovery

1. Warning banner shown with conflict description.
2. Options: Reload (adopt external state), Keep Mine (force overwrite), Cancel.
3. Merge UX not defined in current architecture (GAP-FVR-003).
4. If user selects Reload: form reloads from persisted state; IsDirty = false; in-memory changes lost.
5. If user selects Keep Mine: form retains user values; force-overwrite submitted.
6. If user selects Cancel: conflict banner remains; user may continue editing.

### 7.12 CAT-12: Access Denied Recovery

1. Error banner shown. No UAC elevation offered.
2. User advised to choose a different location or check folder permissions.
3. No retry until resource or permission condition changes.

### 7.13 CAT-13: Capability Conflict Recovery

1. Error shown inline (per-field) or in status banner (multi-item).
2. User must resolve the conflict by removing or modifying conflicting requirements, strategies, or adapter selections.
3. After conflict resolved: form re-validates at Tier 2.

---

## 8. Recovery Failure Handling

When a recovery attempt (retry) itself fails:

1. Apply the same failure classification matrix to the retry result.
2. Present the same error categories and recovery options.
3. If the operation fails multiple times: add a "Report / Export Diagnostics" call-to-action.
4. Do not enter an automatic retry loop. Each retry requires an explicit user action.
5. After three consecutive failures: surface a "Contact Support" advisory with the correlation ID.

---

## 9. Forbidden Recovery Actions

The following must never be used as recovery actions in NexLudic Engine, regardless of failure classification:

| Forbidden Action | Reason |
|---|---|
| Bypass domain service validation on retry | Validation ownership must not be circumvented |
| Silently overwrite game installation files | Safety boundary (`REQ-SAFE-001`); only authorised adapter operations may touch game files |
| Modify protected game directories | Same as above |
| Request UAC elevation to resolve access failure | Medium Integrity Level enforced (`REQ-SEC-001`) |
| Suppress UNCERTAIN status and present success | INV-001; epistemic honesty (`NLE-DOM-SPEC-001`) |
| Auto-retry after UNCERTAIN without user acknowledgement | VR-GLB-004 |
| Auto-retry after PARTIAL without idempotency guarantee | GAP-FVR-002 |
| Clear IsDirty after failed save | INV-001 |
| Use stale async result to update ViewModel state | VR-GLB-003 |
| Expose raw credentials or paths in recovery messages | REQ-SEC-001 |

---

## 10. Documented Gaps

| Gap ID | Description | Blocking What | Status |
|---|---|---|---|
| GAP-FVR-001 | No draft persistence across shutdown | Recovery from crash | Documented; future phase |
| GAP-FVR-002 | No idempotency tokens for file-write / adapter-install | Safe retry after PARTIAL/UNCERTAIN | Documented; blocks partial retry |
| GAP-FVR-003 | No merge UX for stale record conflicts | CAT-11 full resolution | Documented; future work |
| GAP-FSM-002 | Concurrency token implementation undefined | Stale-record detection | Documented; persistence layer dependency |

---

## 11. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Parent | NLE-SPC-FVR-001 |
| Implementation Authorization | Documentation only |

