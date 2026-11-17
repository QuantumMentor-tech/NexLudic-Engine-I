# Form Validation Accessibility and Security Specification

**Document ID:** NLE-SPC-AVS-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Classification:** UI Architecture — Accessibility & Security Specification  

---

## 1. Purpose and Scope

This document specifies the accessibility requirements, security controls, and data-integrity safeguards that apply to all NexLudic Engine forms, validation presentations, error messages, and recovery flows. It establishes the mandatory accessibility attributes, keyboard interaction requirements, screen-reader announcement rules, sensitive-data handling policies, untrusted-input treatment, and the seven data-integrity invariants that every form implementation must satisfy.

**Standards references:** WCAG 2.1 Level AA, Windows UI Automation specification, WPF AccessibilityProperties, CommunityToolkit.Mvvm MVVM pattern (`ADR-0003`), `REQ-SEC-001`.

**Implementation note:** This document is design-and-documentation only. No application code is authorized.

---

## 2. Field-Level Accessibility

### 2.1 Label Association

Every form field must have its label associated via `AutomationProperties.LabeledBy`:

```
// Design contract (not code)
AutomationProperties.LabeledBy(inputControl) → labelElement
```

This association ensures that screen readers announce the field label when the input control receives focus. Plain text `Label` elements must reference the corresponding input control in their `Target` property.

For `ComboBox`, `CheckBox`, and `RadioButton` controls, the accessible name must be set to the visible label text.

### 2.2 Required Field Indication

Required fields must carry two indicators:
1. A visible required marker (e.g., asterisk with colour contrast ≥ 4.5:1 against background).
2. An accessible description: `AutomationProperties.HelpText` = "Required field" on the input control.

The required marker must not rely solely on colour to convey meaning (WCAG 1.4.1).

### 2.3 Helper Text

Helper text below a field (instructional, not error) must be associated with the input via `AutomationProperties.DescribedBy` referencing the helper text element ID.

### 2.4 Character Count Indicators

When a character count indicator is shown (e.g., "87 / 100 characters"):
1. The indicator must be in a live region: `AutomationProperties.LiveSetting = Polite`.
2. Announcements must not fire on every keystroke — announce at defined thresholds only (90% of limit, at limit).

---

## 3. Error Association and Announcement

### 3.1 Inline Error Association

`NleInlineValidation` (CMP-010) errors must be associated with the offending input via:

```
// Design contract
AutomationProperties.DescribedBy(inputControl) → nleInlineValidationElement
```

This ensures the error message is read by screen readers when the input control has focus, without requiring the user to navigate away.

### 3.2 Live Region Settings for Errors

| Error Category | LiveSetting | Rationale |
|---|---|---|
| CAT-01 (Input Validation) — blur | `Polite` | Does not interrupt ongoing input |
| CAT-04 (Operation Rejection) — Critical | `Assertive` | Blocking; must be announced immediately |
| CAT-01 (Input Validation) — stage-exit failure | `Assertive` | Form submit attempt; user expects feedback |
| CAT-05 through CAT-09 (operation failures) | `Assertive` | Operation result; user is waiting |
| CAT-03 (Warning) | `Polite` | Non-blocking advisory |
| CAT-02 (Incomplete but Permitted) | `Polite` | Non-error advisory |
| Form-level error summary | `Assertive` | Multiple errors after submit attempt |
| Character count at threshold | `Polite` | Background advisory |

### 3.3 Form-Level Error Summary

When two or more fields fail stage-exit validation:
1. A focusable error summary is rendered at the top of the form region.
2. Each entry in the summary is a focusable link that moves keyboard focus to the offending field.
3. The summary container must be a live region with `LiveSetting = Assertive`.
4. Summary announcement: "[n] errors found. Review the highlighted fields to continue."
5. The error summary is in addition to, not a replacement for, inline `NleInlineValidation` messages.

---

## 4. Focus Management on Validation Failure

### 4.1 Stage-Exit Validation Failure

When stage-exit or form-submit validation fails:
1. Focus moves to the **first invalid field** in document order.
2. If using `FocusManager.SetFocusedElement` (WPF): target the input control, not the label or error element.
3. The invalid field's inline error is associated via `DescribedBy` and therefore announced automatically.

### 4.2 Pre-flight Critical Failure (CAT-04)

When a Critical pre-flight banner is rendered:
1. Focus moves to the banner itself (or its first actionable element if the banner is focusable).
2. The banner is announced as `Assertive`.
3. If the Confirm button is hidden (anti-cheat hard-block): focus moves to the Cancel/Back button.

### 4.3 Navigation-Away Dialog

When the unsaved-changes ConfirmationDialog (CMP-023) opens:
1. Focus is set to the **Cancel** button (default for Destructive severity dialogs per `NLE-SPC-DLG-001`).
2. Focus is trapped within the dialog while it is open.
3. Escape key is equivalent to Cancel.

### 4.4 Stale-Record Conflict Dialog

Same focus rules as navigation-away dialog. Cancel is the default-focused button.

---

## 5. Keyboard-Only Interaction

All form operations must be completable using keyboard alone. This includes:

| Operation | Keyboard Path |
|---|---|
| Tab through all fields | Standard Tab order following document/logical order |
| Activate dropdown | Alt+Down or Enter when focused |
| Toggle checkbox | Space when focused |
| Select radio button | Arrow keys within group |
| Trigger Submit / Next | Enter or Space on focused button |
| Trigger Cancel | Escape or Tab to Cancel and Enter |
| Navigate error summary links | Tab to link, Enter to jump to field |
| Dismiss dismissible banner | Tab to close button, Enter |
| Activate Retry | Tab to Retry button, Enter |

Keyboard shortcuts must not conflict with system or screen-reader reserved key combinations.

No operation in NexLudic Engine forms requires a mouse. Hover-only content (tooltips) must have equivalent keyboard-accessible alternatives (e.g., via focus or a dedicated help button).

---

## 6. Text Scaling and High DPI

All form layouts must support DPI scaling from 100% to 200%:

1. Use relative sizing (`*`, `Auto`) in XAML grids; avoid hard-coded pixel dimensions for label widths.
2. Minimum touch/click target: 44×44 pixels at 100% DPI (WCAG 2.5.5); scales proportionally.
3. Text must remain readable at 200% without truncation or layout overflow.
4. Error messages must not truncate at any supported DPI level.
5. Character count indicators must remain visible at all DPI levels.
6. Form sections must scroll rather than clip when layout overflows.

---

## 7. Reduced Motion

When the system reduced-motion preference is active (`SystemParameters.MinimizeAnimation = false` or equivalent):

1. No animated entry or exit for error messages — render immediately.
2. No animated progress spinners — use static loading indicator.
3. No animated validation state transitions.
4. Static indicators only for error and success states.

This applies to all `NleInlineValidation`, `NleStatusBanner`, and dialog transitions.

---

## 8. Untrusted Input Treatment

### 8.1 Definition

All form field input is considered **untrusted** until validated by the domain service. This includes:
- Project name, description, slug, intent text
- Game version strings
- File paths entered by the user
- Requirement titles and body text
- Configuration values

### 8.2 Safe Rendering Rules

1. User-supplied text must be rendered only in `TextBlock` with `TextWrapping`. Never in:
   - `RichTextBox` with `IsReadOnly = false` and content-binding
   - Any XAML element that evaluates markup (e.g., `ContentPresenter` with dynamic template)
   - Any binding that evaluates expressions at runtime from user input
2. File paths entered by user must be parsed as filesystem paths, never as URIs or command strings.
3. Requirement body text must be rendered as plain text; no markup language, no HTML rendering.
4. Version strings must be sanitised before display (strip control characters).

### 8.3 No Code Evaluation

User input must never be passed to:
- `Eval()` or equivalent in any scripting layer
- `Process.Start()` as argument without strict validation and allowlist check
- Any templating engine that executes embedded expressions

---

## 9. Sensitive Information Handling

### 9.1 Credential Fields

1. All credential reference fields must use masked rendering: displayed value = `••••••••-••••-••••-••••-••••••••••••`.
2. Credential fields must use `PasswordBox` or equivalent masked input.
3. The actual credential reference UUID must never be written to any log visible in the UI.
4. Clipboard operations on credential fields must be blocked or masked.

### 9.2 Error Message Redaction

As specified in `NLE-SPC-FVR-001 §8.4` and `NLE-CAT-ERR-001 §16`. Mandatory redaction applies to:
- Stack traces → correlation ID only
- Internal paths → omitted or relative-to-workspace-root
- Raw exception names → human-readable description
- API tokens → `[credential reference]`
- DPAPI GUIDs → `••••••••-••••-••••-••••-••••••••••••`
- OS usernames → omitted

### 9.3 Diagnostic Logging

Serilog is the designated logging framework (`ADR-0003`). Logging rules:
1. Structured log entries must never include raw user-entered credential values.
2. User-entered file paths may be logged at Debug level only; not at Info or above in user-visible diagnostic exports.
3. Correlation IDs logged with every error entry — this is the only identifier surfaced to users.
4. Exception detail (stack trace, inner exception, SQL query) logged to file only — never rendered in UI.

---

## 10. Authorization Boundaries

### 10.1 UI Validation Is Not Authorization

Passing Tier 1–4 validation does not grant permission to perform an operation. Authorization is enforced by the domain service and application service layers independently of UI validation.

**Example:** A form that passes all validation may still receive a CAT-12 (Access Denied) response from the domain service. The UI must present the denial without attempting to bypass it.

### 10.2 Medium Integrity Level Enforcement

NexLudic Engine operates at Windows Medium Integrity Level. The UI must:
1. Never call APIs that require High IL (e.g., SCM, registry HKLM writes).
2. Never offer a "Run as Administrator" or UAC elevation prompt in response to any form or operation error.
3. If a resource requires higher privilege than Medium IL: present CAT-12 and advise the user to choose a different resource.

### 10.3 Anti-Cheat Hard-Block Authorization

The anti-cheat hard-block (VR-GAME-001, CAT-04) is enforced at both Tier 2 (ViewModel) and Tier 4 (pre-flight). There is no form state, user input, or configuration setting that can disable or override this block. The block is a safety constraint, not an authorization preference.

---

## 11. Data Integrity Safeguards

The following seven invariants must be satisfied by every form implementation:

| Invariant | Code | Statement |
|---|---|---|
| Save-failure preservation | INV-001 | Save failure must not clear or reset field values |
| Validation preservation | INV-002 | Validation failure must not reset field values |
| Navigation-away guard | INV-003 | Navigation with IsDirty = true must prompt; must not silently discard |
| Crash recovery gap | INV-004 | Architecture does not guarantee draft persistence across unexpected shutdown (documented gap) |
| Pre-flight data retention | INV-005 | Pre-flight rejections must not destroy wizard stage data already entered |
| Stale async isolation | INV-006 | Stale async results must not overwrite current input state |
| Partial-save dirty retention | INV-007 | Partial save success must preserve IsDirty for unsaved portion |

These invariants apply to ALL forms — there are no exceptions or override configurations.

---

## 12. Colour and Contrast

All form validation states must meet WCAG 2.1 AA contrast requirements:

| Element | Minimum Contrast Ratio |
|---|---|
| Body text on form background | 4.5:1 |
| Error icon and label against background | 3:1 (large UI component) |
| Field border in error state | 3:1 against adjacent background |
| Error message text | 4.5:1 |
| Warning text | 4.5:1 |
| Disabled state text | No contrast requirement (disabled) |
| Focus ring on input controls | 3:1 against adjacent colours |

Error and warning states must not rely solely on colour (red/yellow) to communicate the state; icon, text label, or pattern must accompany colour.

---

## 13. Screen-Reader Compatibility Requirements

NexLudic Engine targets NVDA and Windows Narrator as primary screen-reader targets on Windows (consistent with `NLE-PLN-CMP-001`).

| Requirement | Rule |
|---|---|
| Field labels announced on focus | Required via `LabeledBy` |
| Error messages announced without navigation | Required via `DescribedBy` + inline proximity |
| Required field announced | Via `HelpText = "Required field"` |
| Form-level error count announced | Via `Assertive` live region |
| Dialog role announced | WPF Window with `AutomationProperties.Name` set |
| Button state announced (disabled) | Standard WPF automation — disabled button not focusable |
| Progress state announced | Via `AutomationProperties.Name` on progress indicator |
| Uncertain state announced | Via `Assertive` live region on UNCERTAIN banner |

---

## 14. Future Accessibility Gap

**GAP-AVS-001:** Full WCAG 2.1 AA audit with NVDA and Narrator has not been conducted during the design phase. This audit is a Phase 3 Gate requirement. The test plan (`NLE-PLN-FVR-001`, Deliverable G) defines the accessibility test cases. Actual verification occurs during implementation.

---

## 15. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Parent | NLE-SPC-FVR-001 |
| Implementation Authorization | Documentation only |

