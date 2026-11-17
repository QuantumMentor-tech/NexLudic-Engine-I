# Form Submission and Unsaved Change State Model

**Document ID:** NLE-SPC-FSM-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Classification:** UI Architecture — State Model Specification  

---

## 1. Purpose and Scope

This document defines the authoritative form submission state machine, dirty-state tracking model, navigation-away guard contract, unsaved-change dialog semantics, duplicate submission prevention, concurrent-change detection, partial-save handling, and application-shutdown behaviour for all NexLudic Engine forms.

**Scope:** Every form in NexLudic Engine that can produce a dirty state (i.e., any form whose ViewModel exposes `IsDirty`). This includes wizard stages, project workspace panels, requirements management forms, and project configuration screens.

**Authoritative references:** `NLE-SPC-FVR-001`, `NLE-SPC-CTX-002`, `NLE-SPC-DLG-001`, `NLE-DOM-SPEC-001`, `ADR-0003`.

---

## 2. IsDirty State Tracking Model

### 2.1 Definition

`IsDirty` is a boolean property on every form ViewModel that tracks whether the user has made changes that have not yet been persisted.

```
IsDirty = true   ↔   The user has unsaved changes
IsDirty = false  ↔   No unsaved changes exist
```

### 2.2 Events That Set IsDirty = true

| Event | Notes |
|---|---|
| Any text field value changed from its loaded or last-saved value | Character-level detection; initial load value is the baseline |
| Any selection changed (dropdown, radio, checkbox) | Comparison against loaded value |
| Any item added to or removed from a list (capabilities, adapters) | List identity, not deep equality |
| Any file path field changed | String comparison |
| Any credential reference field changed | String comparison — never log the value |

### 2.3 Events That Set IsDirty = false

| Event | Notes |
|---|---|
| Successful save (COMPLETED outcome) | Only after confirmed success; not on submit initiation |
| User selects Discard Changes in navigation-away dialog | Explicit user action |
| Form closed after successful save | IsDirty already false from save |

### 2.4 Events That Must NOT Change IsDirty

| Event | Invariant |
|---|---|
| Save failure (FAILED outcome) | INV-001 — IsDirty stays true |
| Partial save (PARTIAL outcome) | INV-007 — IsDirty stays true for unsaved portion |
| Uncertain outcome (UNCERTAIN) | INV-001 — IsDirty stays true |
| Cancellation (CANCELLED) | INV-001 — IsDirty stays true |
| Validation failure | IsDirty stays true; validation does not reset dirty state |
| Async stale result received | VR-GLB-003 — stale result discarded; dirty state unaffected |

### 2.5 Partial-Dirty Model (Multi-Section Forms)

For forms with independent saveable sections (e.g., Project Configuration with General, AI Provider, Output Paths sections), each section maintains its own `IsDirty` flag. The form-level `IsDirty` is the logical OR of all section flags. Navigation guard fires when any section is dirty.

**Gap (GAP-FSM-001):** Granular per-section dirty tracking across saves is a future implementation detail. The implementation must not collapse a partially-dirty form to fully-clean after a partial save.

---

## 3. Full Form Lifecycle State Machine

### 3.1 State Definitions

| State | Description | IsDirty | CanSubmit |
|---|---|---|---|
| `INITIAL` | Form just opened; no user input made | false | false |
| `EDITING` | User has focused a field but not yet changed values | false | false |
| `DIRTY` | User has changed at least one value | true | false |
| `VALIDATION_REQUESTED` | Stage-exit or Submit triggered; validation in progress | true | false |
| `INVALID` | Validation completed; at least one error present | true | false |
| `READY_TO_SUBMIT` | All validation tiers passed | true | true |
| `SUBMITTING` | Async operation in progress | true | false (IsRunning) |
| `SAVED` | Operation confirmed `COMPLETED` | false | — |
| `FAILED` | Operation confirmed `FAILED` | true | true (retry eligible) |
| `PARTIAL_COMPLETE` | Operation returned `PARTIAL` | true (partial) | true (partial retry) |
| `UNCERTAIN` | Operation returned `UNCERTAIN` | true | false (no auto-retry) |
| `CANCELLED` | Operation cancelled | true | true |

### 3.2 State Transition Table

| From State | Trigger | To State | Guard Condition |
|---|---|---|---|
| `INITIAL` | User focuses any input | `EDITING` | — |
| `EDITING` | First value change committed | `DIRTY` | — |
| `DIRTY` | Stage-exit or Submit pressed | `VALIDATION_REQUESTED` | — |
| `DIRTY` | Additional edits (while dirty) | `DIRTY` | No state change needed |
| `VALIDATION_REQUESTED` | Any tier returns error | `INVALID` | — |
| `VALIDATION_REQUESTED` | All tiers pass | `READY_TO_SUBMIT` | — |
| `INVALID` | User corrects field; re-validate | `DIRTY` or `VALIDATION_REQUESTED` | Only the corrected field re-validates at Tier 1; full re-validate on next stage-exit |
| `READY_TO_SUBMIT` | Confirm/Submit action | `SUBMITTING` | `AsyncRelayCommand.CanExecute = true` |
| `SUBMITTING` | Operation returns COMPLETED | `SAVED` | — |
| `SUBMITTING` | Operation returns FAILED | `FAILED` | — |
| `SUBMITTING` | Operation returns PARTIAL | `PARTIAL_COMPLETE` | — |
| `SUBMITTING` | Operation returns UNCERTAIN | `UNCERTAIN` | — |
| `SUBMITTING` | User cancels operation | `CANCELLED` | Cancellation token honoured |
| `FAILED` | User edits and re-submits | `DIRTY` → `VALIDATION_REQUESTED` | User must edit before re-submit |
| `UNCERTAIN` | User explicitly decides to retry | `DIRTY` → `VALIDATION_REQUESTED` | User decision only; no auto-retry |
| `CANCELLED` | User re-attempts | `DIRTY` → `VALIDATION_REQUESTED` | User decision |
| `SAVED` | User reopens / navigates back | `INITIAL` | Fresh load |

### 3.3 State Machine Diagram

```
                    ┌──────────┐
                    │  INITIAL │◄──────────────────────────────────────────────────────────┐
                    └────┬─────┘                                                           │
                         │ Focus input                                                     │
                         ▼                                                                 │
                    ┌──────────┐                                                           │
                    │ EDITING  │                                                           │
                    └────┬─────┘                                                           │
                         │ First change                                                    │
                         ▼                                                                 │
                    ┌──────────┐                                                           │
                    │  DIRTY   │◄──────────────────────────────────────┐                  │
                    └────┬─────┘                                       │                  │
                         │ Stage-exit / Submit                         │                  │
                         ▼                                             │                  │
              ┌──────────────────────┐                                 │                  │
              │ VALIDATION_REQUESTED │                                 │                  │
              └──────────┬───────────┘                                 │                  │
                         │                                             │                  │
              ┌──────────┴───────────┐                                 │                  │
              ▼                     ▼                                  │                  │
         ┌─────────┐    ┌────────────────────┐                        │                  │
         │ INVALID │    │  READY_TO_SUBMIT   │                        │                  │
         └────┬────┘    └────────┬───────────┘                        │                  │
              │ Correct          │ Confirm                             │                  │
              └──────────────────┤                                     │                  │
                                 ▼                                     │                  │
                          ┌─────────────┐                             │                  │
                          │ SUBMITTING  │                             │                  │
                          └──────┬──────┘                             │                  │
                                 │                                     │                  │
              ┌──────────────────┼──────────────────────────┐         │                  │
              ▼                  ▼              ▼            ▼         │                  │
         ┌────────┐       ┌───────────┐  ┌─────────┐  ┌──────────┐   │                  │
         │ SAVED  │       │  FAILED   │  │PARTIAL_ │  │UNCERTAIN │   │                  │
         └────┬───┘       └────┬──────┘  │COMPLETE │  └────┬─────┘   │                  │
              │                │         └────┬────┘       │          │                  │
              │                │              │             │          │                  │
              │                └──────────────┴─────────────┘          │                  │
              │                              │ User edits / retries    │                  │
              │                              └─────────────────────────┘                  │
              │                                                                           │
              │                Also from SUBMITTING:                                     │
              │                       ▼                                                   │
              │                  ┌──────────┐                                             │
              │                  │CANCELLED │──────────────────────────────────────────────┘
              │                  └──────────┘  User re-attempt → DIRTY
              │
              └──────────────────────────────────────────────────────────────────────────► INITIAL (reload)
```

---

## 4. Navigation-Away Guard Contract

### 4.1 Trigger Conditions

The navigation-away guard fires when ALL of the following are true:

1. `IsDirty = true`
2. User attempts one of: tab switch, section change within workspace, project switch, project close, window close (X), application exit (Alt+F4), or keyboard shortcut close

### 4.2 Dialog Contract

When the guard fires, `ConfirmationDialog` (CMP-023) is shown with:

| Property | Value |
|---|---|
| Title | "Unsaved Changes" |
| Body | "You have unsaved changes. What would you like to do?" |
| Primary Action | **Save** — attempts save; if save fails, return to form in FAILED state |
| Secondary Action | **Discard Changes** — sets IsDirty = false; proceeds with navigation |
| Cancel Action | **Cancel** — dismisses dialog; remains on current form |
| Default Focus | **Cancel** (Destructive severity — `NLE-SPC-DLG-001` §Destructive) |
| Dismissal (Escape) | Equivalent to Cancel |

### 4.3 Save-Failure During Guard

If the user selects Save and the save fails:
1. Navigation-away dialog closes.
2. Form returns to `FAILED` state with the save-failure error banner (`CAT-06`).
3. User input is preserved (INV-001).
4. User may retry save or choose Discard Changes from the new failure state.
5. Navigation does not proceed.

### 4.4 Application-Level Shutdown (Forced)

When the application is terminated by the OS (task kill, shutdown, power loss):

- **Current architecture does not guarantee unsaved-change recovery** (GAP-FVR-001).
- Unsaved form data is lost.
- Project files that were already saved before shutdown are unaffected.
- This is a documented limitation, not a defect in the state model.
- Future implementation may add crash-recovery drafts; this spec does not define that behaviour.

---

## 5. Duplicate Submission Prevention

### 5.1 Mechanism

`AsyncRelayCommand` (CommunityToolkit.Mvvm) is the canonical duplicate-submission guard. When the command is executing:
- `IsRunning = true`
- `CanExecute()` returns `false`
- Submit button is rendered disabled via standard `Command` binding

### 5.2 Rules

1. The ViewModel must not manually reset `IsRunning` before the operation completes or fails.
2. The ViewModel must not use timers or delays as a substitute for the `IsRunning` guard.
3. If the operation transitions to `UNCERTAIN`, `IsRunning` must be set to false (operation is no longer running) but no success state is set.
4. If a stale async result arrives after the form transitions to `SAVED`, it must be discarded (VR-GLB-003).

### 5.3 UI Feedback During Submission

| Element | State During Submission |
|---|---|
| Submit button | Disabled (CanExecute = false) |
| Progress indicator | Shown (`NleLoadingIndicator` or inline spinner) |
| Field inputs | Optionally read-only during submission (implementation detail) |
| Cancel button | Remains active if operation supports cancellation |

---

## 6. Concurrent-Change Detection (Stale Record)

### 6.1 Problem

A project record may be modified by another process (or future multi-instance scenario) while the user has the form open. The form's data is then stale relative to the persisted state.

### 6.2 Detection Mechanism

The persistence layer is expected to provide a version token (record ETag, timestamp, or sequence number) with each loaded record. When the user submits, the application service compares the form's version token against the current persisted token.

**Gap (GAP-FSM-002):** Specific concurrency token implementation is not yet defined in the persistence architecture. When this gap is resolved, the state model extends to include a `CONFLICT` sub-state within `VALIDATION_REQUESTED` → `INVALID`.

### 6.3 Current Specification

When concurrent-change detection is implemented:
1. A detected conflict transitions the form to `INVALID` with a `CAT-11` (Stale Information) error.
2. The user is shown the conflict dialog (Reload / Keep Mine / Cancel).
3. If the user selects Reload: form reloads from persisted state; IsDirty = false; user changes are lost.
4. If the user selects Keep Mine: form retains user values; a forced overwrite is submitted (overwrite semantics defined by domain service).
5. If the user selects Cancel: form remains in conflict state; user may edit further.

---

## 7. Partial Save Handling

### 7.1 Definition

A `PARTIAL` save occurs when a multi-item or multi-section save operation succeeds for some items and fails for others.

### 7.2 State Rules

1. The form transitions to `PARTIAL_COMPLETE` state.
2. `IsDirty` remains true for all sections or items that were not saved (INV-007).
3. `IsDirty` is set to false only for sections or items that were confirmed saved.
4. The UI shows which items succeeded and which failed (CAT-08 presentation).
5. Targeted retry is offered for failed items only — subject to GAP-FVR-002 (idempotency).

### 7.3 User Decision After Partial Save

| User Action | Result |
|---|---|
| Retry failed items | Transition back to SUBMITTING for failed items only |
| Accept partial result | IsDirty = false (or partial-false); navigate away without warning for saved items |
| Edit and re-submit all | Form returns to DIRTY; full validation on next submit |

---

## 8. Draft Handling

**Current architectural limitation:** NexLudic Engine does not implement automatic draft persistence. There is no auto-save capability defined in the current architecture (`NLE-SPC-FVR-001` GAP-FVR-001).

The IsDirty model covers in-memory change tracking only. Closing the application or the form without saving results in permanent loss of in-memory changes.

**Future consideration:** A draft persistence layer would extend this model by adding a `DRAFT_SAVED` sub-state between `DIRTY` and `SAVED`. This is not authorized for the current phase.

---

## 9. ViewModel Contract Requirements

All form ViewModels that implement dirty-state tracking must satisfy the following contract:

```
// Required ViewModel properties (design contract — not code)
bool IsDirty { get; }
bool IsSubmitting { get; }  // = AsyncRelayCommand.IsRunning
OperationStatus LastOperationStatus { get; }
string? LastOperationCorrelationId { get; }
IRelayCommand SubmitCommand { get; }  // AsyncRelayCommand
IRelayCommand CancelCommand { get; }

// Required ViewModel behaviours (design contract)
ValidateAsync() → bool           // Executes Tier 1 + 2 (+ 3 async for config forms)
OnNavigatingAway() → bool        // Returns true if safe to navigate; fires guard if IsDirty
ResetToSaved()                   // Called only on confirmed COMPLETED; sets IsDirty = false
PreserveOnFailure()              // No-op — dirty state automatically preserved
```

---

## 10. Relationships to Other Specifications

| Reference | Relationship |
|---|---|
| `NLE-SPC-CTX-002` | Governs project-switch navigation guard; this doc extends to form-level |
| `NLE-SPC-DLG-001` | Defines ConfirmationDialog (CMP-023) used by navigation guard |
| `NLE-SPC-FVR-001` | Parent specification — defines IsDirty invariants INV-001–INV-007 |
| `NLE-SPC-REC-001` | Recovery specification — defines retry eligibility from FAILED/PARTIAL/UNCERTAIN |
| `ADR-0003` | Mandates CommunityToolkit.Mvvm AsyncRelayCommand as duplicate-submission guard |

---

## 11. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Parent | NLE-SPC-FVR-001 |
| Implementation Authorization | Documentation only |

