# Form Validation and Recovery Test Plan

**Document ID:** NLE-PLN-FVR-001  
**Version:** 1.0.0  
**Status:** Approved — Design-Level Specification  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Classification:** UI Architecture — Test Plan  

---

## 1. Purpose and Scope

This test plan defines the future test cases for forms, validation, error presentation, unsaved-change handling, operation failures, retry behaviour, and error recovery across NexLudic Engine. All tests are **design-level specifications only** — no tests are executed, no test runner is configured, and no test infrastructure is authorized in this step.

**When these tests execute:** Phase 4 (Implementation) or the designated testing sub-phase for each component.

**Test ownership:** Each test case specifies the responsible level (Unit, Integration, UI/E2E) and the team owner.

---

## 2. Test Case Format

| Field | Description |
|---|---|
| **Test ID** | Unique: `TVR-[group]-[seq]` |
| **Objective** | What is being verified |
| **Preconditions** | Required state before test begins |
| **Input / Failure** | What is entered or what failure is induced |
| **Steps** | Ordered actions |
| **Expected State** | Form state after test |
| **Expected Message** | Message category and approximate content |
| **Data Preservation** | Whether user input is preserved |
| **Recovery** | Recovery path expected |
| **Accessibility** | Screen-reader and keyboard expectations |
| **Test Level** | Unit / Integration / UI/E2E |
| **Required Evidence** | What must be captured to pass |
| **Related AC** | Acceptance criteria from Step 23 (AC-01 through AC-28) |

---

## 3. Valid Input Tests

### TVR-VAL-001: Project Name — Valid Entry

| Field | Value |
|---|---|
| **Test ID** | TVR-VAL-001 |
| **Objective** | Confirm valid project name accepted without error |
| **Preconditions** | Project creation wizard Stage 1 open |
| **Input** | "My Test Project 2026" |
| **Steps** | 1. Enter name. 2. Tab away. 3. Observe. |
| **Expected State** | `EDITING` or `DIRTY`; no inline error shown |
| **Expected Message** | None |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | No error announced; character count announced at ≥90 chars |
| **Test Level** | UI/E2E |
| **Required Evidence** | Screenshot — no NleInlineValidation visible |
| **Related AC** | AC-01, AC-02 |

### TVR-VAL-002: UNKNOWN Game Version Accepted (Epistemic Honesty)

| Field | Value |
|---|---|
| **Test ID** | TVR-VAL-002 |
| **Objective** | Confirm UNKNOWN game version is NOT an error — VR-VER-001 |
| **Preconditions** | Stage 4 of project creation open; game pair selected |
| **Input** | Leave version field empty (UNKNOWN state) |
| **Steps** | 1. Leave version field empty. 2. Tab away. 3. Attempt stage exit. |
| **Expected State** | Stage exits successfully; `UNKNOWN` advisory shown (CAT-02 Info) — not an Error |
| **Expected Message** | "Version not specified. NexLudic Engine will use available detection data." (Info level only) |
| **Data Preservation** | N/A |
| **Recovery** | None required |
| **Accessibility** | Announced Polite only — not Assertive |
| **Test Level** | UI/E2E |
| **Required Evidence** | Stage 5 reached; no Error-level validation visible for empty version |
| **Related AC** | AC-05, AC-11 |

### TVR-VAL-003: Capability with Exploratory Mode

| Field | Value |
|---|---|
| **Test ID** | TVR-VAL-003 |
| **Objective** | Confirm exploratory flag satisfies VR-CAP-001 without requiring a capability entry |
| **Preconditions** | Stage 6 open; zero capabilities declared |
| **Input** | Check "Exploratory Mode" checkbox |
| **Steps** | 1. Check exploratory. 2. Attempt stage exit. |
| **Expected State** | Stage exits successfully |
| **Expected Message** | None |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Checkbox toggle announced; no error announced |
| **Test Level** | UI/E2E |
| **Required Evidence** | Stage 7 reached |
| **Related AC** | AC-03 |

---

## 4. Invalid Input Tests

### TVR-INV-001: Project Name — Empty on Stage Exit

| Field | Value |
|---|---|
| **Test ID** | TVR-INV-001 |
| **Objective** | Confirm required-field error on empty project name at stage exit |
| **Preconditions** | Stage 1 open; name field empty |
| **Input** | Attempt stage exit with name empty |
| **Steps** | 1. Leave name empty. 2. Click Next. |
| **Expected State** | `INVALID`; inline error below Name field; focus on Name field |
| **Expected Message** | "Project name is required." |
| **Data Preservation** | Yes |
| **Recovery** | Enter name; re-attempt stage exit |
| **Accessibility** | Error announced Assertive; focus on Name field |
| **Test Level** | UI/E2E |
| **Required Evidence** | NleInlineValidation visible; focus on Name; no stage advance |
| **Related AC** | AC-01, AC-07 |

### TVR-INV-002: Slug Format Violation

| Field | Value |
|---|---|
| **Test ID** | TVR-INV-002 |
| **Objective** | Confirm slug format error on invalid characters |
| **Preconditions** | Settings > Slug field |
| **Input** | "My Project!@#" |
| **Steps** | 1. Enter slug. 2. Blur. |
| **Expected State** | `INVALID`; inline error on Slug field |
| **Expected Message** | "Project ID must use only lowercase letters, numbers, and hyphens..." |
| **Data Preservation** | Yes |
| **Recovery** | Correct slug format |
| **Accessibility** | Error announced Polite on blur |
| **Test Level** | Integration |
| **Required Evidence** | NleInlineValidation visible; stage not advanced |
| **Related AC** | AC-01 |

### TVR-INV-003: Path Not on Volume D — Hard Block

| Field | Value |
|---|---|
| **Test ID** | TVR-INV-003 |
| **Objective** | Confirm hard-block error when path is not on D:\ |
| **Preconditions** | Stage 1 or project settings path field |
| **Input** | "C:\Users\test\NexLudic" |
| **Steps** | 1. Enter path. 2. Blur. 3. Attempt stage exit. |
| **Expected State** | `INVALID`; Error-severity (non-dismissible) on path field |
| **Expected Message** | "Workspace location must be on volume D:\\..." |
| **Data Preservation** | Yes |
| **Recovery** | Enter a D:\ path |
| **Accessibility** | Error announced Assertive; focus on path field |
| **Test Level** | UI/E2E |
| **Required Evidence** | Error shown; stage not advanced; no D:\ path: blocked |
| **Related AC** | AC-04, AC-13 |

### TVR-INV-004: Anti-Cheat Hard Block — Non-Dismissible

| Field | Value |
|---|---|
| **Test ID** | TVR-INV-004 |
| **Objective** | Confirm anti-cheat Critical block is non-dismissible and Confirm button is hidden |
| **Preconditions** | Stage 3; a known anti-cheat game selected |
| **Input** | Select anti-cheat game as source or target |
| **Steps** | 1. Select anti-cheat game. 2. Attempt stage exit. |
| **Expected State** | Critical banner shown; Confirm button hidden; no stage advance |
| **Expected Message** | "[Game name] uses active anti-cheat protection…" |
| **Data Preservation** | Yes |
| **Recovery** | Select different game |
| **Accessibility** | Announced Assertive; focus on Critical banner or Back button |
| **Test Level** | UI/E2E |
| **Required Evidence** | Critical banner visible; Confirm hidden; no advance possible |
| **Related AC** | AC-04, AC-12 |

### TVR-INV-005: Same Game Selected for Both Slots

| Field | Value |
|---|---|
| **Test ID** | TVR-INV-005 |
| **Objective** | Confirm VR-GAME-002 fires when Source = Target |
| **Preconditions** | Stage 3 open |
| **Input** | Select identical game for both slots |
| **Steps** | 1. Select Game A for source. 2. Select same Game A for target. 3. Stage exit. |
| **Expected State** | `INVALID`; Error on second game selector |
| **Expected Message** | "Source game and target game must be different titles." |
| **Data Preservation** | Yes |
| **Recovery** | Select different target game |
| **Accessibility** | Error announced; focus on second selector |
| **Test Level** | Integration |
| **Required Evidence** | Error shown; stage not advanced |
| **Related AC** | AC-03, AC-07 |

---

## 5. Cross-Field Validation Tests

### TVR-XFV-001: Capability Missing — No Exploratory Flag

| Field | Value |
|---|---|
| **Test ID** | TVR-XFV-001 |
| **Objective** | Confirm VR-CAP-001 fires when no capability and no exploratory flag |
| **Preconditions** | Stage 6 open; zero capabilities; exploratory unchecked |
| **Input** | Attempt stage exit |
| **Steps** | 1. Leave capability list empty. 2. Leave exploratory unchecked. 3. Next. |
| **Expected State** | `INVALID`; Error on capability section |
| **Expected Message** | "Add at least one capability requirement, or mark this project as exploratory…" |
| **Data Preservation** | Yes |
| **Recovery** | Add capability or check exploratory |
| **Accessibility** | Error announced; focus on capability area |
| **Test Level** | UI/E2E |
| **Required Evidence** | Error shown; stage not advanced |
| **Related AC** | AC-03 |

### TVR-XFV-002: Strategy Disclaimer Not Acknowledged

| Field | Value |
|---|---|
| **Test ID** | TVR-XFV-002 |
| **Objective** | Confirm VR-STRAT-001 blocks stage exit if disclaimer not checked |
| **Preconditions** | Stage 6; disclaimer checkbox unchecked |
| **Input** | Attempt stage exit |
| **Steps** | 1. Leave disclaimer unchecked. 2. Next. |
| **Expected State** | `INVALID`; Error on disclaimer |
| **Expected Message** | "Please acknowledge the strategy disclaimer before continuing." |
| **Data Preservation** | Yes |
| **Recovery** | Check disclaimer |
| **Accessibility** | Error announced; focus on checkbox |
| **Test Level** | UI/E2E |
| **Required Evidence** | Error shown; stage not advanced |
| **Related AC** | AC-03 |

---

## 6. Conditional Validation Tests

### TVR-COND-001: UNEVALUATED Feasibility Is Not an Error

| Field | Value |
|---|---|
| **Test ID** | TVR-COND-001 |
| **Objective** | Confirm UNEVALUATED feasibility does not block project creation |
| **Preconditions** | Pre-flight audit with feasibility status = UNEVALUATED |
| **Input** | Proceed with no feasibility evaluation performed |
| **Steps** | 1. Complete wizard stages 1–6. 2. Enter Stage 7 with feasibility = UNEVALUATED. |
| **Expected State** | Pre-flight proceeds; UNEVALUATED shown as advisory (CAT-02) only |
| **Expected Message** | "Feasibility has not yet been evaluated…" (Info level only) |
| **Data Preservation** | N/A |
| **Recovery** | None required |
| **Accessibility** | Announced Polite only |
| **Test Level** | UI/E2E |
| **Required Evidence** | Stage 7 reached; no Error on feasibility; Info/Advisory visible |
| **Related AC** | AC-05, AC-11 |

### TVR-COND-002: Version UNKNOWN with DECLARED Environment Still Valid

| Field | Value |
|---|---|
| **Test ID** | TVR-COND-002 |
| **Objective** | Confirm UNKNOWN version + selected environment is a valid combination |
| **Preconditions** | Stage 4; version blank; environment selected (Steam) |
| **Input** | UNKNOWN version, selected environment |
| **Steps** | 1. Leave version blank. 2. Select environment. 3. Stage exit. |
| **Expected State** | Stage 5 reached; version shown as UNKNOWN advisory |
| **Expected Message** | Advisory only — no Error |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | No Error announced |
| **Test Level** | UI/E2E |
| **Required Evidence** | Stage 5 loaded; no blocking error on version |
| **Related AC** | AC-05 |

---

## 7. Async Validation Stale Response Tests

### TVR-ASYNC-001: Stale Slug Uniqueness Result Discarded

| Field | Value |
|---|---|
| **Test ID** | TVR-ASYNC-001 |
| **Objective** | Confirm stale Tier 3 slug-uniqueness result does not update ViewModel |
| **Preconditions** | Stage 7 pre-flight; slug validation async in progress |
| **Input** | User changes slug before async result arrives; stale result then arrives |
| **Steps** | 1. Initiate pre-flight (slug check starts). 2. User edits slug mid-flight. 3. Original slug check result arrives with old correlation token. |
| **Expected State** | Old result discarded; new slug's validation state shown; ViewModel reflects current input |
| **Expected Message** | Validation state for new slug only |
| **Data Preservation** | Yes — current slug value unchanged |
| **Recovery** | N/A |
| **Accessibility** | No erroneous announcement from stale result |
| **Test Level** | Integration |
| **Required Evidence** | Correlation token mismatch logged; ViewModel state reflects current slug |
| **Related AC** | AC-14 |

---

## 8. Form Submission Lifecycle Tests

### TVR-SUB-001: Successful Save Clears IsDirty

| Field | Value |
|---|---|
| **Test ID** | TVR-SUB-001 |
| **Objective** | Confirm IsDirty = false after COMPLETED save |
| **Preconditions** | Requirements form with unsaved changes |
| **Input** | Valid requirement data |
| **Steps** | 1. Enter valid data. 2. Click Save. 3. Wait for COMPLETED response. |
| **Expected State** | `SAVED`; IsDirty = false; no navigation guard on subsequent navigate |
| **Expected Message** | Success confirmation |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Success announced Polite |
| **Test Level** | Integration |
| **Required Evidence** | IsDirty = false confirmed; navigation proceeds without dialog |
| **Related AC** | AC-09 |

### TVR-SUB-002: Duplicate Submission Prevented

| Field | Value |
|---|---|
| **Test ID** | TVR-SUB-002 |
| **Objective** | Confirm second Submit while first is in-flight has no effect |
| **Preconditions** | Form in SUBMITTING state; Save operation async in progress |
| **Input** | Attempt to click Save again during SUBMITTING |
| **Steps** | 1. Click Save. 2. While SUBMITTING, attempt second Save click. |
| **Expected State** | Second click has no effect; IsRunning = true; CanExecute = false |
| **Expected Message** | None |
| **Data Preservation** | Yes |
| **Recovery** | N/A |
| **Accessibility** | Save button announced as disabled |
| **Test Level** | Integration |
| **Required Evidence** | Only one operation in progress; no duplicate events |
| **Related AC** | AC-15 |

---

## 9. Unsaved Change Tests

### TVR-UNS-001: Navigation Away Triggers Guard

| Field | Value |
|---|---|
| **Test ID** | TVR-UNS-001 |
| **Objective** | Confirm ConfirmationDialog shown when navigating away with IsDirty = true |
| **Preconditions** | Requirements form; at least one field changed |
| **Input** | Click navigation tab / section link |
| **Steps** | 1. Enter a change. 2. Navigate to different section. |
| **Expected State** | ConfirmationDialog shown; focus on Cancel; navigation blocked |
| **Expected Message** | "You have unsaved changes. What would you like to do?" |
| **Data Preservation** | Yes |
| **Recovery** | Save / Discard / Cancel |
| **Accessibility** | Dialog announced; Cancel focused |
| **Test Level** | UI/E2E |
| **Required Evidence** | Dialog shown; Cancel focused; navigation did not proceed |
| **Related AC** | AC-10, AC-11 |

### TVR-UNS-002: Save in Guard — Save Failure Returns to Form

| Field | Value |
|---|---|
| **Test ID** | TVR-UNS-002 |
| **Objective** | Confirm that if user selects Save in guard dialog but save fails, navigation does not proceed |
| **Preconditions** | Navigation guard dialog open; persistence layer simulated to fail |
| **Input** | Select Save in dialog |
| **Steps** | 1. Open guard dialog. 2. Click Save. 3. Save fails (CAT-06). |
| **Expected State** | Dialog closes; form returns to FAILED state; navigation did NOT proceed; IsDirty unchanged |
| **Expected Message** | CAT-06 error banner |
| **Data Preservation** | Yes — INV-001 |
| **Recovery** | Retry save or Discard |
| **Accessibility** | Error announced; focus on form/error area |
| **Test Level** | Integration |
| **Required Evidence** | Form remains open; error banner visible; user is still on original form |
| **Related AC** | AC-10, AC-16 |

### TVR-UNS-003: Discard in Guard — Navigation Proceeds

| Field | Value |
|---|---|
| **Test ID** | TVR-UNS-003 |
| **Objective** | Confirm Discard Changes in guard dialog proceeds with navigation and IsDirty = false |
| **Preconditions** | Navigation guard dialog open |
| **Input** | Select Discard Changes |
| **Steps** | 1. Open guard dialog. 2. Click Discard Changes. |
| **Expected State** | Navigation proceeds; IsDirty = false; changes lost |
| **Expected Message** | None |
| **Data Preservation** | No — user chose to discard |
| **Recovery** | N/A |
| **Accessibility** | Navigation destination loaded; announced |
| **Test Level** | UI/E2E |
| **Required Evidence** | Navigation proceeds; IsDirty = false |
| **Related AC** | AC-10 |

---

## 10. Persistence Failure Tests

### TVR-FAIL-001: Save Failure Preserves Input (INV-001)

| Field | Value |
|---|---|
| **Test ID** | TVR-FAIL-001 |
| **Objective** | Confirm all field values preserved when save fails |
| **Preconditions** | Requirements form; persistence layer simulated to fail |
| **Input** | Valid requirement data; save fails (CAT-06) |
| **Steps** | 1. Enter data. 2. Click Save. 3. Simulate write failure. |
| **Expected State** | `FAILED`; all field values unchanged; IsDirty = true |
| **Expected Message** | "Your changes could not be saved. Your work is preserved." |
| **Data Preservation** | Yes — INV-001 |
| **Recovery** | Retry (if eligible) |
| **Accessibility** | Error announced Assertive |
| **Test Level** | Integration |
| **Required Evidence** | Field values identical to pre-save; IsDirty = true |
| **Related AC** | AC-16 |

### TVR-FAIL-002: Partial Save — Dirty for Unsaved Portion (INV-007)

| Field | Value |
|---|---|
| **Test ID** | TVR-FAIL-002 |
| **Objective** | Confirm IsDirty stays true for unsaved portion after PARTIAL outcome |
| **Preconditions** | Multi-section config form; section A saves; section B fails |
| **Input** | Valid data in both sections; section B write fails |
| **Steps** | 1. Edit both sections. 2. Save all. 3. Section A: COMPLETED; Section B: FAILED. |
| **Expected State** | `PARTIAL_COMPLETE`; Section A IsDirty = false; Section B IsDirty = true |
| **Expected Message** | CAT-08 partial banner with per-section breakdown |
| **Data Preservation** | Yes — INV-007 |
| **Recovery** | Targeted retry for section B |
| **Accessibility** | Partial outcome announced; per-section indicators |
| **Test Level** | Integration |
| **Required Evidence** | Section A: clean; Section B: dirty; partial banner visible |
| **Related AC** | AC-16, AC-17 |

---

## 11. UNCERTAIN Outcome Tests

### TVR-UNC-001: UNCERTAIN — No Auto-Collapse to Success or Failure

| Field | Value |
|---|---|
| **Test ID** | TVR-UNC-001 |
| **Objective** | Confirm UNCERTAIN outcome never relabelled as COMPLETED or FAILED |
| **Preconditions** | Any operation; response simulated as UNCERTAIN |
| **Input** | Trigger operation; simulate UNCERTAIN result |
| **Steps** | 1. Submit operation. 2. Receive UNCERTAIN response. |
| **Expected State** | `UNCERTAIN`; UNCERTAIN banner shown; IsDirty unchanged |
| **Expected Message** | "The outcome of this operation could not be confirmed…" |
| **Data Preservation** | Yes — INV-001 |
| **Recovery** | User reviews log; makes explicit retry decision |
| **Accessibility** | Announced Assertive; UNCERTAIN label present |
| **Test Level** | Integration |
| **Required Evidence** | UNCERTAIN banner visible; no success or failure label; no auto-navigation |
| **Related AC** | AC-18 |

### TVR-UNC-002: UNCERTAIN — No Auto-Retry

| Field | Value |
|---|---|
| **Test ID** | TVR-UNC-002 |
| **Objective** | Confirm no automatic retry initiated after UNCERTAIN |
| **Preconditions** | Form in UNCERTAIN state |
| **Input** | None — observe behaviour after UNCERTAIN received |
| **Steps** | 1. Wait 30 seconds after UNCERTAIN response. |
| **Expected State** | Still UNCERTAIN; no retry attempt observed in logs |
| **Expected Message** | UNCERTAIN banner unchanged |
| **Data Preservation** | Yes |
| **Recovery** | Explicit user action only |
| **Accessibility** | No change in announcement |
| **Test Level** | Integration |
| **Required Evidence** | No retry events in operation log |
| **Related AC** | AC-18 |

---

## 12. Retry Safety Tests

### TVR-RET-001: Retry Offered Only for DNS/CF Class

| Field | Value |
|---|---|
| **Test ID** | TVR-RET-001 |
| **Objective** | Confirm Retry button shown only when failure class is DNS or CF |
| **Preconditions** | Various failure scenarios |
| **Input** | Simulate DNS failure (pre-flight fails before first write) |
| **Steps** | 1. Trigger DNS failure. 2. Observe Retry button presence. |
| **Expected State** | Retry button visible |
| **Expected Message** | CAT-06 or CAT-05 as applicable |
| **Data Preservation** | Yes |
| **Recovery** | Retry available |
| **Accessibility** | Retry button accessible via keyboard |
| **Test Level** | Integration |
| **Required Evidence** | Retry button visible; activatable |
| **Related AC** | AC-19 |

### TVR-RET-002: No Retry Offered for UNCERTAIN Without Acknowledgement

| Field | Value |
|---|---|
| **Test ID** | TVR-RET-002 |
| **Objective** | Confirm that if Retry is offered for UNCERTAIN, it requires explicit acknowledgement dialog |
| **Preconditions** | Form in UNCERTAIN state; architecture determines DNS-safe |
| **Input** | User clicks Retry (if offered) |
| **Steps** | 1. Click Retry on UNCERTAIN result. 2. Observe confirmation dialog. |
| **Expected State** | Acknowledgement dialog shown before retry proceeds |
| **Expected Message** | "The previous outcome could not be confirmed. Retrying may duplicate the operation…" |
| **Data Preservation** | Yes |
| **Recovery** | User confirms or cancels |
| **Accessibility** | Dialog announced; Cancel focused |
| **Test Level** | UI/E2E |
| **Required Evidence** | Dialog shown; retry proceeds only after explicit confirm |
| **Related AC** | AC-19 |

---

## 13. Error Presentation Tests

### TVR-PRES-001: Error Message Contains No Stack Trace

| Field | Value |
|---|---|
| **Test ID** | TVR-PRES-001 |
| **Objective** | Confirm no stack trace appears in any user-visible error |
| **Preconditions** | Any error scenario |
| **Input** | Trigger CAT-07 (unexpected error) |
| **Steps** | 1. Trigger unexpected error. 2. Inspect all visible UI text. |
| **Expected State** | Error banner visible; correlation ID visible; no stack trace |
| **Expected Message** | "An unexpected error occurred…Reference: [correlation-id]" |
| **Data Preservation** | Yes |
| **Recovery** | Retry / Diagnostics |
| **Accessibility** | Error announced |
| **Test Level** | UI/E2E |
| **Required Evidence** | UI text review: no exception class name, no stack trace, no internal path |
| **Related AC** | AC-21 |

### TVR-PRES-002: Credential Field Masked at All Times

| Field | Value |
|---|---|
| **Test ID** | TVR-PRES-002 |
| **Objective** | Confirm credential reference never visible as plain text |
| **Preconditions** | AI Provider configuration form |
| **Input** | Load form with existing credential reference |
| **Steps** | 1. Open AI Provider config. 2. Inspect credential field display. |
| **Expected State** | Field shows masked value only: `••••••••-••••-••••-••••-••••••••••••` |
| **Expected Message** | Masked display |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Announced as "sensitive field" |
| **Test Level** | UI/E2E |
| **Required Evidence** | Screenshot: masked credential; raw UUID not visible |
| **Related AC** | AC-21, AC-23 |

---

## 14. Accessibility Tests

### TVR-A11Y-001: Keyboard-Only Form Completion

| Field | Value |
|---|---|
| **Test ID** | TVR-A11Y-001 |
| **Objective** | Confirm all wizard stages completable via keyboard alone |
| **Preconditions** | Project creation wizard open |
| **Input** | Keyboard only (no mouse) |
| **Steps** | 1. Tab through all fields. 2. Enter values. 3. Navigate Next via keyboard. 4. Complete all stages. |
| **Expected State** | All stages reached; project created |
| **Expected Message** | Standard stage messages |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | All fields reachable; no dead-end keyboard trap |
| **Test Level** | UI/E2E |
| **Required Evidence** | Full keyboard walkthrough recorded; no mouse interaction |
| **Related AC** | AC-24 |

### TVR-A11Y-002: Error Announced by Screen Reader

| Field | Value |
|---|---|
| **Test ID** | TVR-A11Y-002 |
| **Objective** | Confirm inline error announced by NVDA when field loses focus |
| **Preconditions** | Stage 1; NVDA running |
| **Input** | Leave required field empty; blur |
| **Steps** | 1. Focus name field. 2. Tab away without entering value. 3. Listen to NVDA output. |
| **Expected State** | NVDA announces field label + error message |
| **Expected Message** | "[Field label] is required." announced |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Error associated via DescribedBy; announced without manual navigation |
| **Test Level** | UI/E2E (manual) |
| **Required Evidence** | NVDA output log or screen recording |
| **Related AC** | AC-07, AC-24 |

### TVR-A11Y-003: Focus on First Invalid Field After Stage-Exit

| Field | Value |
|---|---|
| **Test ID** | TVR-A11Y-003 |
| **Objective** | Confirm focus moves to first invalid field after failed stage exit |
| **Preconditions** | Multiple invalid fields; stage exit attempted |
| **Input** | Leave multiple fields invalid; click Next |
| **Steps** | 1. Leave fields A, B, C invalid. 2. Click Next. 3. Observe focus location. |
| **Expected State** | Focus on field A (first in document order) |
| **Expected Message** | Field A error announced |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Focus on field A; NVDA announces field A error |
| **Test Level** | UI/E2E |
| **Required Evidence** | Focus documented as field A |
| **Related AC** | AC-24 |

### TVR-A11Y-004: 200% DPI Layout Integrity

| Field | Value |
|---|---|
| **Test ID** | TVR-A11Y-004 |
| **Objective** | Confirm all form layouts remain usable at 200% DPI |
| **Preconditions** | Windows DPI set to 200%; project creation open |
| **Input** | Navigate all stages at 200% DPI |
| **Steps** | 1. Set DPI to 200%. 2. Navigate wizard. 3. Inspect for truncation, overflow. |
| **Expected State** | All text readable; no clipping; buttons accessible |
| **Expected Message** | N/A |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | All UI elements reachable at 200% |
| **Test Level** | UI/E2E (manual) |
| **Required Evidence** | Screenshots at 200% DPI; no truncation |
| **Related AC** | AC-24 |

---

## 15. Security Tests

### TVR-SEC-001: Untrusted Input Not Executed

| Field | Value |
|---|---|
| **Test ID** | TVR-SEC-001 |
| **Objective** | Confirm user-entered text is never evaluated as markup, code, or query |
| **Preconditions** | Any text field |
| **Input** | `<Button>Click</Button>` and `'; DROP TABLE projects; --` |
| **Steps** | 1. Enter markup/injection text in name field. 2. Save and reload. 3. Inspect rendered output. |
| **Expected State** | Text rendered as-is in TextBlock; no XAML elements created; no SQL executed |
| **Expected Message** | Literal text displayed |
| **Data Preservation** | Yes |
| **Recovery** | N/A |
| **Accessibility** | Text announced as-is |
| **Test Level** | Integration |
| **Required Evidence** | UI screenshot: literal text; no injected controls |
| **Related AC** | AC-22, AC-23 |

---

## 16. Concurrency Tests

### TVR-CONC-001: Record Changed During Edit — CAT-11 Detected

| Field | Value |
|---|---|
| **Test ID** | TVR-CONC-001 |
| **Objective** | Confirm stale-record conflict detected when record changes externally during edit |
| **Preconditions** | Requirements form open; record modified externally (simulated) |
| **Input** | User submits form after external modification |
| **Steps** | 1. Open form; note version token. 2. External process updates record. 3. User submits. |
| **Expected State** | CAT-11 conflict warning; Reload / Keep Mine / Cancel options |
| **Expected Message** | "This project was updated since you opened it…" |
| **Data Preservation** | Yes |
| **Recovery** | Reload, Keep Mine, or Cancel |
| **Accessibility** | Conflict announced; dialog options accessible |
| **Test Level** | Integration |
| **Required Evidence** | CAT-11 shown; conflict detected before overwrite |
| **Related AC** | AC-20 |

---

## 17. Regression Tests

### TVR-REG-001: FormField (CMP-020) Change Does Not Break Validation

| Field | Value |
|---|---|
| **Test ID** | TVR-REG-001 |
| **Objective** | Confirm that changes to shared FormField component do not break existing validations |
| **Preconditions** | FormField modified; all wizard and workspace forms loaded |
| **Input** | Navigate all validated forms |
| **Steps** | 1. After FormField change, open all forms. 2. Trigger known validations. |
| **Expected State** | All validation rules fire as specified in NLE-CAT-FVR-001 |
| **Expected Message** | Unchanged from pre-modification |
| **Data Preservation** | N/A |
| **Recovery** | N/A |
| **Accessibility** | Accessibility bindings intact |
| **Test Level** | UI/E2E (regression suite) |
| **Required Evidence** | All TVR test cases re-pass after component change |
| **Related AC** | AC-25 |

---

## 18. Test Coverage Summary

| Group | Test Count | Test Level |
|---|---|---|
| Valid input | 3 | UI/E2E |
| Invalid input | 5 | Integration / UI/E2E |
| Cross-field | 2 | UI/E2E |
| Conditional validation | 2 | UI/E2E |
| Async stale response | 1 | Integration |
| Submission lifecycle | 2 | Integration |
| Unsaved changes | 3 | Integration / UI/E2E |
| Persistence failure | 2 | Integration |
| UNCERTAIN outcome | 2 | Integration |
| Retry safety | 2 | Integration / UI/E2E |
| Error presentation | 2 | UI/E2E |
| Accessibility | 4 | UI/E2E (manual) |
| Security | 1 | Integration |
| Concurrency | 1 | Integration |
| Regression | 1 | UI/E2E |
| **Total** | **33** | — |

---

## 19. Acceptance Criteria Traceability

| AC | Covered By |
|---|---|
| AC-01 | TVR-VAL-001, TVR-INV-001, TVR-INV-002 |
| AC-02 | TVR-VAL-001 |
| AC-03 | TVR-VAL-003, TVR-XFV-001, TVR-XFV-002, TVR-INV-005 |
| AC-04 | TVR-INV-003, TVR-INV-004 |
| AC-05 | TVR-VAL-002, TVR-COND-001, TVR-COND-002 |
| AC-07 | TVR-INV-001, TVR-A11Y-002 |
| AC-09 | TVR-SUB-001 |
| AC-10 | TVR-UNS-001, TVR-UNS-002, TVR-UNS-003 |
| AC-11 | TVR-VAL-002, TVR-COND-001, TVR-UNS-001 |
| AC-12 | TVR-INV-004 |
| AC-13 | TVR-INV-003 |
| AC-14 | TVR-ASYNC-001 |
| AC-15 | TVR-SUB-002 |
| AC-16 | TVR-FAIL-001, TVR-UNS-002, TVR-FAIL-002 |
| AC-17 | TVR-FAIL-002 |
| AC-18 | TVR-UNC-001, TVR-UNC-002 |
| AC-19 | TVR-RET-001, TVR-RET-002 |
| AC-20 | TVR-CONC-001 |
| AC-21 | TVR-PRES-001 |
| AC-22 | TVR-SEC-001 |
| AC-23 | TVR-PRES-002, TVR-SEC-001 |
| AC-24 | TVR-A11Y-001, TVR-A11Y-002, TVR-A11Y-003, TVR-A11Y-004 |
| AC-25 | TVR-REG-001 |

---

## 20. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Parent | NLE-SPC-FVR-001 |
| Implementation Authorization | Documentation only — no tests executed |

