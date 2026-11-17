# Forms, Validation & Error-Recovery Specification

**Document ID:** NLE-SPC-FVR-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Applies To:** All NexLudic Engine forms, structured input controls, submission flows, and error-recovery patterns  
**Classification:** UI Architecture — Design Specification  

---

## 1. Purpose and Scope

This document establishes the authoritative specification for forms, user-input validation, error presentation, unsaved-change handling, operation failures, retry behaviour, and error-recovery across NexLudic Engine. It defines:

- The four-tier validation ownership model and the precise boundaries between tiers.
- Error taxonomy, severity classifications, and blocking rules.
- Form lifecycle state machine governing all structured data entry flows.
- Error-presentation principles and mandatory redaction rules.
- Input-preservation invariants that protect user work from silent data loss.
- Submission semantics and duplicate-submission prevention.
- Recovery principles and eligibility rules.
- Accessibility and security requirements that apply to all form contexts.

**Scope boundary:** This specification governs every form in NexLudic Engine — wizard stages, workspace panels, configuration screens, requirements management, and project-settings forms. Single-action buttons and status-only panels without user input fields are excluded from §4–§7 but remain subject to §8 (error presentation) and §9 (operation recovery).

**Implementation note:** This document is design-and-documentation only. No application code, validator implementations, schema changes, or UI framework modifications are authorized by this document.

---

## 2. Authoritative References

| Ref | Document | Relevance |
|-----|----------|-----------|
| `ADR-0003` | Technology Stack Decision | WPF/XAML, .NET 8.0 LTS, CommunityToolkit.Mvvm |
| `NLE-DOM-SPEC-001` | Core Domain Model | Epistemic tiers, Operation Status enum, entity lifecycle |
| `NLE-SPC-FRM-001` | UI Controls, Forms & Validation Patterns | FormField, NleInlineValidation, timing rules |
| `NLE-SPC-CTX-002` | Project Context & Navigation State | IsDirty model, unsaved-change navigation |
| `NLE-SPC-DLG-001` | Dialog, Notification & Contextual Interaction | ConfirmationDialog, unsaved-change modal |
| `NLE-CAT-WIZ-001` | Project Creation Field & Validation Catalog | Wizard-specific field rules (non-duplicated here) |
| `NLE-SPC-WIZ-001` | Project Creation Wizard Specification | Wizard stage flow and pre-flight integration |
| `REQ-SEC-001` | Security Requirements Baseline | Medium IL, DPAPI, no raw credentials in UI |
| `REQ-SAFE-001` | Safety Requirements Baseline | Anti-cheat hard block semantics |
| `DEC-009` | Volume D Storage Decision | D:\ path enforcement |

---

## 3. Foundational Principles

### 3.1 Epistemic Honesty

Validation rules must respect the domain epistemic tier model (`NLE-DOM-SPEC-001`):

- **`DECLARED`** ≠ **`DETECTED`** ≠ **`PROBED`** ≠ **`VERIFIED`** — different tiers carry different authority.
- **`UNKNOWN`** version is a valid, explicitly permitted state; it must never be treated as an input error.
- **`UNEVALUATED`** feasibility is not a failure condition; it must not block project creation or stage completion unless a specific rule requires evaluation.
- **`UNCERTAIN`** operation outcome must never be silently collapsed into success or failure.

### 3.2 Input Preservation

User input is preserved through all recoverable failure paths:

- **INV-001:** Saving failure must not clear or reset field values.
- **INV-002:** Validation failure must not reset field values.
- **INV-003:** Navigation-away with unsaved changes must prompt before discarding, not silently discard.
- **INV-004:** Application crash recovery is a future capability; this architecture does not guarantee draft persistence across unexpected shutdown (gap documented).
- **INV-005:** Pre-flight rejections (Tier 4) must not destroy wizard stage data already entered.
- **INV-006:** Async validation stale responses must not overwrite the user's current input state.
- **INV-007:** Partial save success must preserve the state of unsaved portions as dirty.

### 3.3 Separation of Concerns

UI validation layers must never usurp domain authority:

- Presentation (Tier 1) may format-check; it may not make feasibility or domain-business decisions.
- Application (Tier 2) may cross-validate; it may not override domain service uniqueness or path permission results.
- Domain (Tier 3) has final authority on business-rule correctness.
- Persistence (Tier 4 / Pre-flight) has final authority on infrastructure constraints.

### 3.4 Actionability

Every error message surfaced to the user must include at minimum:
1. What went wrong (in user-intelligible terms).
2. Why it matters (consequence or context).
3. What the user can do (at least one clear recovery action).

Error messages must never expose: raw exception class names, stack traces, file system paths that reveal internal installation layout, API tokens, DPAPI GUIDs, or OS-level credential store identifiers.

---

## 4. Four-Tier Validation Ownership Model

### 4.1 Tier 1 — Presentation Layer (FormField / NleInlineValidation)

**Owner:** View / XAML code-behind  
**Authority:** Format, length, character-set, regex pattern  
**Triggers:** Character input (progressive, format-only), field blur (format + length)  
**Blocking:** Prevents stage-exit when errors present  
**Example rules:** Name length ≤ 100 chars; slug matches `[a-z0-9-]+`; path not empty string  

Tier 1 must not query domain services or perform I/O.

### 4.2 Tier 2 — Stage-Level ViewModel (ValidateAsync)

**Owner:** ViewModel (`ValidateAsync()` on `IRelayCommand`)  
**Authority:** Cross-field consistency, anti-cheat hard-block evaluation, conditional-field logic  
**Triggers:** Stage-exit (Next/Finish button), explicit Validate action  
**Blocking:** Prevents stage advance when errors present  
**Example rules:** Game A ≠ Game B; anti-cheat hard-block (non-dismissible); at least one capability or exploratory flag  

Tier 2 must not perform file I/O or network calls. It may invoke in-memory domain service evaluation on cached data.

### 4.3 Tier 3 — Domain Service

**Owner:** Application/Domain Service layer  
**Authority:** Slug uniqueness, path permission verification, business-rule correctness  
**Triggers:** Stage 7 entry (pre-flight initiation), explicit Validate on configuration screens  
**Blocking:** Prevents project creation commit when errors present  
**Example rules:** Slug not already in use; `D:\` path exists and is writeable; project path unique  

Tier 3 performs I/O. Results must be scoped with correlation tokens (monotonic counter or `ProjectId`) to discard stale async responses.

### 4.4 Tier 4 — Pre-flight Application Service

**Owner:** Application Service (ProjectCreationService or equivalent)  
**Authority:** Disk space adequacy, final anti-cheat confirmation, infrastructure readiness  
**Triggers:** Project-creation commit action (Stage 7 Confirm)  
**Blocking:** Prevents irreversible operations when pre-flight fails  
**Example rules:** Available disk space ≥ 10 GB on D:\; anti-cheat hard-block confirmed at infrastructure level  

Tier 4 is the last gate before irreversible state change. Failure at Tier 4 is classified as an Operation Rejection (§5.3).

### 4.5 Validation Timing Rules

| Timing Event | Tier Executed | Scope |
|---|---|---|
| Character typed | 1 (format-only subset) | Current field — no length enforcement during typing |
| Field blur | 1 (full Tier 1 rules) | Current field |
| Stage-exit button | 1 + 2 | Full stage |
| Stage 7 entry | 1 + 2 + 3 (async) | Full project data |
| Commit (Confirm) | 1 + 2 + 3 + 4 | Full project + infrastructure |
| Config save | 1 + 2 + 3 | Affected section |

---

## 5. Error Taxonomy Overview

Errors are classified into 13 canonical categories. Full message catalog is in `NLE-CAT-ERR-001` (Deliverable C).

### 5.1 Input Validation Error (CAT-01)

User input violates a field or cross-field rule. Blocking. Inline presentation. Correctable by user.

### 5.2 Incomplete but Permitted (CAT-02)

Input is in a structurally valid but incomplete epistemic state (e.g., `UNKNOWN` version). Non-blocking. Advisory presentation only. Must never produce a validation error.

### 5.3 Warning — Non-Blocking (CAT-03)

Input or configuration carries a risk or advisory note that does not prevent progress. Shown with `Warning` icon. User may proceed.

### 5.4 Operation Rejection (CAT-04)

A requested operation cannot proceed due to a business rule, pre-flight failure, or authorization constraint. Non-dismissible. User must either correct the condition or abandon the operation. Anti-cheat hard-block is the canonical CAT-04 example.

### 5.5 Resource Unavailable (CAT-05)

A required infrastructure resource (disk path, file, service) is not accessible. Non-blocking on input forms but blocks pre-flight. Recovery: resolve resource then retry pre-flight.

### 5.6 Persistence Failure (CAT-06)

A save, write, or storage operation did not complete. Input preserved (INV-001). Retry is eligibility-dependent (§E).

### 5.7 Unexpected Internal Error (CAT-07)

An unhandled or unanticipated failure occurred. Non-dismissible diagnostic message shown. No stack trace or internal path exposed. User instructed to retry or report.

### 5.8 Partial Completion (CAT-08)

An operation succeeded for some items and failed for others. Status is `PARTIAL`. User shown what succeeded and what failed. Recovery per succeeded and failed sub-items separately.

### 5.9 Uncertain Outcome (CAT-09)

The result of an operation cannot be determined. Status is `UNCERTAIN`. Never collapsed to success or failure. User shown `UNCERTAIN` state clearly. No blind retry.

### 5.10 Cancellation (CAT-10)

An operation was cancelled by the user or by the system (e.g., timeout). Status is `CANCELLED`. User input preserved. Re-attempt is a user decision.

### 5.11 Stale / Conflicting Information (CAT-11)

A record or configuration was changed externally while the user's form was open. The form's view of the data is stale. User must review current state before proceeding.

### 5.12 Access Denied (CAT-12)

An operation requires a permission or authorization level not held by the current context. Medium Integrity Level enforced. UAC elevation is not permitted (`REQ-SEC-001`).

### 5.13 Dependency / Capability Conflict (CAT-13)

A capability declaration, adapter combination, or project configuration contains a constraint conflict that the domain model cannot resolve. Requires user clarification or scope reduction.

---

## 6. Severity and Blocking Classification

| Severity | Blocking | Dismissible | Presentation Component | Focus Behaviour |
|---|---|---|---|---|
| `Critical` | Yes — prevents commit | No | Pre-flight banner + NleInlineValidation | Focus returned to offending field |
| `Error` | Yes — prevents stage advance | No (inline) | NleInlineValidation below field | Focus on first invalid field |
| `Warning` | No | Yes | NleStatusBanner (Warning level) | No forced focus change |
| `Info` | No | Yes | NleStatusBanner (Info level) | No forced focus change |
| `Advisory` | No | Yes | Helper text or tooltip | No forced focus change |

Anti-cheat hard-block is classified `Critical` and is non-dismissible under any user action.

---

## 7. Form Lifecycle State Machine

```
┌──────────────┐
│   INITIAL    │  Form opened; no user input yet; IsDirty = false
└──────┬───────┘
       │ User begins typing / selecting
       ▼
┌──────────────┐
│   EDITING    │  User is entering data; IsDirty = false until first change
└──────┬───────┘
       │ First field change committed
       ▼
┌──────────────┐
│    DIRTY     │  IsDirty = true; unsaved-change guard active
└──────┬───────┘
       │ Stage-exit or Submit triggered
       ▼
┌──────────────────────┐
│  VALIDATION_REQUESTED │  ValidateAsync() executing
└──────┬───────────────┘
       │
  ┌────┴─────┐
  │          │
  ▼          ▼
┌──────┐  ┌───────────────┐
│INVALID│  │ READY_TO_SUBMIT│  All tiers pass
└──┬───┘  └──────┬────────┘
   │              │
   │ User corrects│ Commit action
   │              ▼
   │       ┌────────────┐
   │       │ SUBMITTING │  Async operation in progress; guard active
   │       └──────┬─────┘
   │              │
   │    ┌─────────┼──────────────────┬────────────────┐
   │    ▼         ▼                  ▼                ▼
   │  ┌──────┐ ┌──────────┐  ┌────────────┐ ┌──────────────┐
   │  │SAVED │ │  FAILED  │  │PARTIAL_    │ │  UNCERTAIN   │
   │  │      │ │          │  │COMPLETE    │ │              │
   │  └──────┘ └──────────┘  └────────────┘ └──────────────┘
   │           IsDirty kept  IsDirty partial IsDirty kept
   │           INV-001       INV-007         INV-001
   │
   │  Also from SUBMITTING:
   │    ▼
   │  ┌───────────┐
   └─▶│CANCELLED  │  User cancelled; IsDirty kept; re-attempt optional
      └───────────┘
```

**State transition rules:**

| From | To | Trigger | Guard |
|---|---|---|---|
| INITIAL | EDITING | Any input focus | — |
| EDITING | DIRTY | First field change | — |
| DIRTY | VALIDATION_REQUESTED | Stage-exit or Submit | IsDirty = true |
| VALIDATION_REQUESTED | INVALID | Any tier returns error | — |
| VALIDATION_REQUESTED | READY_TO_SUBMIT | All tiers pass | — |
| INVALID | DIRTY | User corrects field | Clears field error only |
| READY_TO_SUBMIT | SUBMITTING | Confirm action | AsyncRelayCommand.IsRunning guard |
| SUBMITTING | SAVED | Operation confirmed complete | IsDirty = false |
| SUBMITTING | FAILED | Operation confirmed failed | IsDirty unchanged |
| SUBMITTING | PARTIAL_COMPLETE | Some items succeeded | IsDirty = partial (INV-007) |
| SUBMITTING | UNCERTAIN | Outcome cannot be determined | IsDirty unchanged |
| SUBMITTING | CANCELLED | User cancels or timeout | IsDirty unchanged |
| FAILED / UNCERTAIN | DIRTY | User modifies and retries | Per recovery eligibility |

---

## 8. Error Presentation Principles

### 8.1 Inline Field Errors

- Rendered by `NleInlineValidation` (CMP-010) immediately below the offending field.
- Associated via `AutomationProperties.DescribedBy` for screen-reader accessibility.
- Displayed after blur (Tier 1) or after stage-exit (Tier 2+).
- Never displayed during active typing for format-only rules; character-limit proximity indicator only.

### 8.2 Form-Level Error Summary

- Triggered after a failed stage-exit or failed submit when two or more fields are invalid.
- Presented as a focusable list at the top of the form region.
- Each entry in the summary links (focus) to its corresponding field.
- Assistive technology: `LiveSetting = Assertive` on the summary container.

### 8.3 Pre-flight and Operation-Level Errors

- Presented via `NleStatusBanner` (Critical or Error level) above the action button row.
- Anti-cheat hard-block: rendered as non-dismissible Critical banner. The Confirm button is hidden (not disabled) for this state.
- Persistence failures: rendered as Error banner with specific retry or navigate-away guidance.

### 8.4 Mandatory Redaction Rules

The following must never appear in any user-visible error message, log displayed in UI, or diagnostic panel:

| Forbidden Content | Replacement |
|---|---|
| Raw API tokens or keys | `[credential reference]` or masked placeholder |
| DPAPI GUIDs | `••••••••-••••-••••-••••-••••••••••••` |
| Full internal file system paths | Relative path relative to workspace root, or omitted |
| Stack traces | "An unexpected error occurred. Reference: [correlation-id]" |
| Raw exception class names | User-intelligible description |
| OS-level usernames or account paths | Omitted |

---

## 9. Submission Semantics

### 9.1 Duplicate Submission Prevention

`AsyncRelayCommand` (CommunityToolkit.Mvvm) `IsRunning` guard is the canonical mechanism. When `IsRunning = true`, the command's `CanExecute` returns `false`, disabling the submit button. No manual state management supplement is required, but the ViewModel must not reset the guard before the operation completes or fails.

### 9.2 Stale Async Response Protection

Every async Tier 3/4 validation call and every form-submission operation must be paired with a correlation token (monotonic integer counter or the current `ProjectId`). Responses with a token different from the current token are silently discarded without updating ViewModel state.

### 9.3 Submission Outcome Handling

| Outcome | IsDirty After | User Presentation | Navigation |
|---|---|---|---|
| `COMPLETED` (Saved) | false | Success confirmation or status | Optional auto-navigation |
| `FAILED` | unchanged | Error banner; recovery action | Stay on form |
| `PARTIAL` | partial (INV-007) | Partial banner; per-item status | Stay on form |
| `UNCERTAIN` | unchanged | Uncertain banner; no auto-retry | Stay on form; user decides |
| `CANCELLED` | unchanged | Cancelled status | Stay on form |

---

## 10. Unsaved-Change Guard

Governed by `NLE-SPC-CTX-002` and `NLE-SPC-DLG-001`. Summary:

- When `IsDirty = true` and user triggers navigation away, project switch, window close, or application exit: show `ConfirmationDialog` (CMP-023) with options `[Save] [Discard Changes] [Cancel]`.
- Default focus: **Cancel** (Destructive severity).
- If user selects Save: attempt save; if save fails, return to form in FAILED state; do not navigate.
- If user selects Discard: clear dirty state and navigate.
- If user selects Cancel: dismiss dialog; remain on form.
- Application shutdown (forced): current architecture does not guarantee unsaved-change recovery. This is documented as a gap (GAP-FVR-001).

---

## 11. Recovery Principles

### 11.1 Recovery Eligibility Requirements

Recovery via retry is only offered when:
1. The failure is in a category where the operation's start state is known (Definitively Not Started, or Confirmed Failed without side effects).
2. The operation is idempotent or the architecture provides safe-retry guarantees.
3. No conflicting state change has occurred between failure and retry.

**Gap (GAP-FVR-002):** NexLudic Engine does not yet define server-side idempotency keys for file-write or adapter-install operations. Until idempotency is defined, blind retry after `PARTIAL` or `UNCERTAIN` outcomes is prohibited.

### 11.2 Forbidden Recovery Shortcuts

Under no circumstances may recovery flows:
- Bypass authorization checks or domain-service validation.
- Silently apply stale or unverified data.
- Elevate process integrity level (Medium IL enforced).
- Modify game installation files or protected game directories.
- Suppress or hide the `UNCERTAIN` status and present success instead.

### 11.3 Recovery Destination

| Failure Category | Recovery Destination |
|---|---|
| Input Validation (CAT-01) | Return focus to offending field |
| Operation Rejection (CAT-04) | Remain on rejection state; correct condition or abandon |
| Resource Unavailable (CAT-05) | Advise resolution; offer re-run pre-flight |
| Persistence Failure (CAT-06) | Remain on form; offer retry if eligible |
| Partial Completion (CAT-08) | Show per-item status; offer targeted retry for failed items |
| Uncertain Outcome (CAT-09) | Show UNCERTAIN; await user decision; no auto-retry |
| Internal Error (CAT-07) | Show correlation ID; offer retry; offer diagnostics export |
| Stale Information (CAT-11) | Reload record; merge or discard user changes (user choice) |

---

## 12. Security Requirements

Governed by `REQ-SEC-001`. All validation and error handling must:

1. Never execute user-supplied text as code, markup, or query (untrusted input).
2. Render user-supplied text in `TextBlock` with `TextWrapping` only — never in an evaluated context.
3. Treat all form field input as untrusted until validated by the domain service.
4. Store credentials exclusively as DPAPI opaque UUIDs; never in form state, error messages, or diagnostic logs visible in UI.
5. Enforce Medium Integrity Level; never request UAC elevation in response to a form error.
6. Volume D enforcement: path fields that do not begin with `D:\` are hard-blocked (Error severity, non-dismissible) per `DEC-009`.

---

## 13. Accessibility Requirements

Governed by `NLE-PLN-CMP-001`. All forms must:

1. Associate every error message with its field via `AutomationProperties.DescribedBy`.
2. Associate every field label with its input via `AutomationProperties.LabeledBy`.
3. Move focus to the first invalid field on stage-exit validation failure.
4. Use `LiveSetting = Assertive` for Critical/Error announcements; `Polite` for Warnings.
5. Support keyboard-only correction path with no mouse dependency.
6. Remain usable at DPI scaling 100%–200%.
7. Respect reduced-motion system settings; no animation required for error display.
8. Provide accessible descriptions for all required-field indicators.

---

## 14. Gaps and Open Issues

| Gap ID | Description | Impact | Status |
|---|---|---|---|
| GAP-FVR-001 | No draft persistence across unexpected application shutdown | Loss of unsaved wizard data on crash | Documented; future capability |
| GAP-FVR-002 | No idempotency tokens for file-write / adapter-install operations | Unsafe blind retry after PARTIAL/UNCERTAIN | Documented; blocks safe retry spec |
| GAP-FVR-003 | Stale-record merge UX not fully specified | User confusion when record changes during edit | Partial — CAT-11 covers detection; merge UI is future work |

---

## 15. Relationships to Other Specifications

```
NLE-SPC-FVR-001 (this document)
  ├── Extends: NLE-SPC-FRM-001 (validation timing, FormField, NleInlineValidation)
  ├── Extends: NLE-SPC-CTX-002 (IsDirty model, navigation guard)
  ├── Extends: NLE-SPC-DLG-001 (ConfirmationDialog, unsaved-change modal)
  ├── Extends: NLE-DOM-SPEC-001 (epistemic tiers, Operation Status enum)
  ├── Governed by: REQ-SEC-001 (security), REQ-SAFE-001 (anti-cheat)
  ├── Governed by: DEC-009 (Volume D path enforcement)
  ├── Refined by: NLE-CAT-FVR-001 (field rule catalog, Deliverable B)
  ├── Refined by: NLE-CAT-ERR-001 (error message catalog, Deliverable C)
  ├── Refined by: NLE-SPC-FSM-001 (submission state model, Deliverable D)
  ├── Refined by: NLE-SPC-REC-001 (retry & recovery spec, Deliverable E)
  ├── Refined by: NLE-SPC-AVS-001 (accessibility & security, Deliverable F)
  └── Tested by: NLE-PLN-FVR-001 (test plan, Deliverable G)
```

---

## 16. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Supersedes | None — first issue |
| Next Review | Phase 3 Gate Review |
| Implementation Authorization | Documentation only — no code authorized |

