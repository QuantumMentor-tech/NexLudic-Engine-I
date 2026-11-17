# UI DIALOG, NOTIFICATION & CONTEXTUAL INTERACTION SPECIFICATION

**Document Identifier:** `NLE-SPC-DLG-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Dialog Architecture, Notification System & Overlay Interaction Contracts  
**Parent Document:** `NLE-SPC-CMP-001`

---

## 1. Dialog Architecture & Type Catalog

The NexLudic Engine uses WPF modal dialogs (`Window` or `ContentDialog`-equivalent UserControl hosted in an overlay layer). The following dialog types are approved:

### 1.1 Dialog Type Matrix

| Dialog Type | Component | Trigger | Severity | Closeable via Escape |
|:---|:---|:---|:---|:---|
| **Information** | `InfoDialog` | Informational message requiring acknowledgement | Info | Yes → closes |
| **Confirmation** | `ConfirmationDialog` (CMP-023) | Action requiring explicit intent | Warning | Yes → Cancel |
| **Destructive Confirmation** | `ConfirmationDialog` (Destructive) | Irreversible data-modifying operation | Destructive | Yes → Cancel |
| **Form Dialog** | `FormDialog` | Lightweight data entry within a contextual task | Neutral | Yes → triggers unsaved-change check |
| **Approval Review** | `ApprovalReviewDialog` | AI diff or operation approval gate | Warning | Yes → leaves PENDING |
| **Error Detail** | `ErrorDetailDialog` | Expandable error with diagnostic detail | Critical | Yes |
| **Operation Progress** | `OperationProgressOverlay` | Long-running atomic operation (project creation) | Neutral | No (blocks until complete or error) |

### 1.2 When NOT to Use a Dialog
- Routine navigation between destinations → use workspace navigation.
- Inline validation errors → use `NleInlineValidation` on the field.
- Transient status updates → use toast notifications.
- Extensive multi-step workflows → use a full wizard or dedicated workspace destination.

---

## 2. ConfirmationDialog Behavioural Contract

The `ConfirmationDialog` (CMP-023) is the canonical destructive-action gate for all project operations.

### 2.1 Standard Properties
- **Title:** Specific action name (e.g. "Delete Requirement REQ-002?"), never generic ("Are you sure?")
- **Message:** One or two sentences explaining what will be changed and that it may be irreversible.
- **ConfirmLabel:** Specific verb (e.g. "Delete", "Discard Changes", "Rollback"); never "Yes" or "OK" for destructive actions.
- **CancelLabel:** "Cancel" (always available, always the safe default focus for Destructive dialogs).
- **Severity:** Sets visual treatment and default focus assignment.

### 2.2 Focus & Keyboard Contract
1. Dialog opens → focus placed on **Cancel** button (Destructive severity) or **Confirm** button (Info/Warning severity).
2. `Tab` cycles between Cancel ↔ Confirm.
3. `Escape` activates Cancel.
4. `Enter` activates the focused button.
5. Dialog closes → focus returns to the button or control that triggered the dialog.

### 2.3 Operations That Always Require ConfirmationDialog
- Deleting any project record (requirement, capability, adapter binding).
- Discarding unsaved wizard changes.
- Purging staging cache.
- Snapshot rollback.
- Approving AI-generated diff (uses `ApprovalReviewDialog` variant).
- Project archival.

### 2.4 Operations That Do NOT Require ConfirmationDialog
- Saving project (immediate `Ctrl+S` without prompt).
- Navigating to a different workspace destination.
- Collapsing a sidebar or inspector panel.
- Running a read-only feasibility audit.
- Refreshing a data view.

---

## 3. ApprovalReviewDialog

The `ApprovalReviewDialog` enforces the human-in-the-loop requirement from `ADR-0009`.

### 3.1 Contract
- **Opens from:** `ApprovalSummaryCard` → "Review Diff" action; or from pending approval notification.
- **Content:** Read-only diff viewer (before/after text), affected files list, requested scope description, permission summary.
- **Actions:** `[Approve & Apply]` (Primary), `[Reject & Discard]` (Secondary), `[Request Revision]` (Tertiary), `[Cancel]`.
- **Critical Invariant:** Opening this dialog does NOT approve anything. The `[Approve & Apply]` button routes to a secondary `ConfirmationDialog` (Destructive) before the approval command is dispatched.
- **Loading State:** While applying approved diff, the dialog shows `OperationProgressOverlay`; the apply button is disabled.
- **Post-Action:** On successful apply → dialog closes → OperationStatusRow updates to `COMPLETED`; focus returns to the triggering `ApprovalSummaryCard`.

---

## 4. OperationProgressOverlay

Used for atomic operations that must not be interrupted (project creation, snapshot rollback, staging purge).

### 4.1 Contract
- **Presentation:** Full-screen semi-transparent overlay with centered progress card.
- **Content:** Operation title, animated indeterminate `ProgressBar`, status message.
- **Close Behaviour:** Not user-dismissible while operation is running. If the operation errors: overlay transitions to error state with `[View Diagnostics]` and `[Retry]` options.
- **Escape Key:** Disabled during active operation (prevent accidental dismissal).

---

## 5. Toast Notifications

Toasts are transient, non-blocking announcements for low-to-medium importance events. They use the global notification model from `NLE-SPC-STS-001`.

### 5.1 Notification Severity Levels

| Level | Use Case | Auto-Dismiss | Persist in History |
|:---|:---|:---|:---|
| **Info** | Successful save, task queued | 4 seconds | No |
| **Warning** | Feasibility stale, adapter version mismatch | 8 seconds | Yes |
| **Success** | AI task completed, test suite passed | 4 seconds | No |
| **Error** | Save failed, adapter bind error | Persistent (manual dismiss) | Yes |
| **Critical** | Project manifest corrupt, disk full | Persistent + system tray alert | Yes |

### 5.2 Notification Dismissal Rules
- Clicking the notification itself navigates to the relevant destination (if `NavigationTarget` is set).
- Dismissing a notification clears it from the toast queue; it does NOT resolve the underlying issue.
- Dismissing an Error/Critical toast does NOT approve an operation, cancel a task, or restore data.
- Persistent notifications remain in the Shell Status Tray notification drawer until manually cleared.

### 5.3 Accessibility
- Toasts use `AutomationProperties.LiveSetting = LiveSetting.Assertive` for Error/Critical; `LiveSetting.Polite` for Info/Warning/Success.
- Each toast is keyboard-reachable via `F6` pane cycling to the notification tray area.
- Keyboard shortcut `Alt+N` focuses the notification tray.

---

## 6. Tooltips

### 6.1 Usage Rules
- Tooltips clarify secondary or supplementary information (e.g. what a feasibility badge means, what a strategy shorthand stands for).
- Tooltips must NOT be the only way to access critical information. If a limitation or warning is actionable, it must appear inline or in a persistent indicator.
- Tooltips appear on **pointer hover** AND **keyboard focus** (`ToolTipService.ShowOnDisabled = true` for disabled controls with prerequisites).

### 6.2 Content Constraints
- Max 100 characters for single-line tooltips; use a small popover for longer explanatory content.
- Must not contain raw file paths, internal IDs, or stack traces.

---

## 7. Context Menus

Context menus (`ContextualActionMenu`, CMP-027) provide overflow secondary actions for records.

### 7.1 Access Methods
- Right-click on a record row or card.
- Keyboard: Focus item → `Shift+F10` or `Apps` key.
- Icon ellipsis button (always visible for keyboard discoverability — not hidden on hover only).

### 7.2 Invariants
- Context menus must NEVER be the **only** way to perform an essential action.
- Destructive items (Delete, Archive) appear last in the menu, separated by a divider, with a warning icon.
- Menu items respect `CanExecute` and show disabled items with reason tooltips where applicable.

---

## 8. Popovers

Popovers (flyout content panels) are used for inline supplementary content that is too long for a tooltip but doesn't warrant a full dialog.

### 8.1 Approved Popover Use Cases
- Expanded evidence citation detail from an `EvidenceReferenceRow`.
- Full capability limitation description from a `CapabilityStatusRow`.
- Game version disambiguation detail in `GameSelectorControl`.

### 8.2 Constraints
- Popovers are keyboard-triggered from the activating button/link.
- `Escape` dismisses the popover; focus returns to trigger.
- Popovers do not contain form submission actions. Use `FormDialog` instead.
- Never nest a popover inside a modal dialog.

---

## 9. Unsaved Changes Dialog

The unsaved changes confirmation follows the authoritative model from `NLE-SPC-CTX-002` Section 2.1.

Standard pattern across all surfaces:
- **Title:** "Unsaved Changes"
- **Message:** "You have unsaved changes to [Project Name / Form Name]. What would you like to do?"
- **Actions:** `[Save]` (Primary), `[Discard Changes]` (Destructive Secondary), `[Cancel — Stay Here]` (Tertiary)
- **Default Focus:** `Cancel — Stay Here` (conservative default prevents accidental data loss)
- This dialog is triggered by:
  - Project switching while `IsDirty = true`
  - Navigating away from a form with unsaved edits
  - Closing the application window
  - Wizard `Cancel` when form has been modified

The `ConfirmationDialog` component does not independently decide whether data is dirty — the ViewModel's `IsDirty` property is the authoritative source.

