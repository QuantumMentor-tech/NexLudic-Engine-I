# UI CONTROLS, FORMS & VALIDATION PATTERNS

**Document Identifier:** `NLE-SPC-FRM-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Form Controls, Validation Architecture & Interaction Patterns  
**Parent Document:** `NLE-SPC-CMP-001`

---

## 1. Button Hierarchy & Behavioural Contract

### 1.1 Semantic Role Definitions

| Role | WPF Style Key | Use Case | Keyboard |
|:---|:---|:---|:---|
| **Primary** | `NleButtonPrimary` | One dominant action per view/stage (e.g. "Create Project", "Save") | `Enter` when button has focus |
| **Secondary** | `NleButtonSecondary` | Supportive actions alongside Primary (e.g. "Back", "Cancel") | `Enter` / `Space` |
| **Tertiary** | `NleButtonTertiary` | Low-priority auxiliary links (e.g. "Learn More", "Skip") | `Enter` / `Space` |
| **Destructive** | `NleButtonDestructive` | Irreversible operations (e.g. "Delete Project", "Purge Staging") | `Enter` / `Space` — always with ConfirmationDialog |
| **IconOnly** | `NleIconButtonStyle` | Compact toolbar controls (e.g. collapse sidebar, close panel) | `Enter` / `Space` |

**Rule:** No screen may have two Primary buttons visible simultaneously in the same action area. Hierarchy communicates relative action importance.

### 1.2 Button States & Loading Behaviour

```
[Default] → [Hovered] → [Pressed] → (invokes AsyncRelayCommand)
                                         ↓
                                    [Loading: disabled + spinner overlay]
                                         ↓
                              ┌─────────────────────────┐
                              │ CanExecute = true again  │
                              └─────────────────────────┘
                              ↙                         ↘
                     [Default]                    [Fault → ViewModel error notification]
```

- **Loading State:** While `AsyncRelayCommand.IsRunning = true`, the button shows an inline `ProgressRing` (small, 16px), label is hidden, and `IsEnabled = false`. This is the **sole mechanism** preventing duplicate submissions — the ViewModel command owns this, not the Button.
- **No Optimistic Success:** The button never transitions to a "success" or "complete" visual without an authoritative result from the ViewModel.

### 1.3 Disabled vs. Unavailable vs. Blocked

| Condition | Presentation | Explanation Tooltip |
|:---|:---|:---|
| `CanExecute = false` (prerequisite not met) | Greyed, disabled | Required: "Complete game selection first" |
| Feature not in scope / deferred | Hidden or not rendered | N/A — not shown |
| Running operation (another task active) | Loading state on that task's control | N/A for other buttons |
| Authorization insufficient | Hidden or disabled with reason | "Requires explicit approval" |

**Rule:** Disabled buttons with meaningful prerequisites should expose a `ToolTip` explaining the blocking condition. Silent greying without explanation reduces usability.

---

## 2. Form Field Composition Contract

Every form field uses `FormField` (CMP-020) as its wrapper. The composition is:

```
FormField
├── Label Row
│   ├── NleLabel (field label text)
│   └── RequiredIndicator (*) — only when IsRequired = true
├── [Child Control Slot]  ← NleTextInput / NleSelector / NleCheckbox / etc.
├── HelperText (optional, appears below control in secondary style)
└── NleInlineValidation (ValidationState + ValidationMessage)
```

### 2.1 Read-Only Fields
When a field is in read-only state (e.g. auto-generated slug, system-detected version):
- Replace editable input with a styled read-only text display (`NleReadOnlyValue`).
- Display a lock icon or "(auto-generated)" annotation.
- Do NOT render an editable `TextBox` with `IsReadOnly=true` without visual distinction — this is deceptive.

### 2.2 Sensitive Fields
Credential references (DPAPI opaque UUIDs) are displayed as masked strings: `••••••••-••••-••••`. They are **never** shown as plain text in any form field. Direct editing of raw API tokens is prohibited in project-scoped forms (`REQ-SEC-001`).

---

## 3. Validation Architecture

### 3.1 Ownership Separation

| Validation Layer | Owner | Examples |
|:---|:---|:---|
| **Presentation (Tier 1)** | `FormField` + `NleInlineValidation` | Character count, regex format check, min length |
| **Stage-Level (Tier 2)** | ViewModel (`ValidateStage()`) | Cross-field dependencies, anti-cheat flag logic |
| **Domain (Tier 3)** | Domain Service | Slug uniqueness against disk, path write permissions |
| **Pre-flight (Tier 4)** | Application Service | Disk space audit, anti-cheat hard block |

The UI only **presents** Tier 1 results inline and receives Tier 2–4 results from the ViewModel. Tier 3–4 never execute in the UI thread.

### 3.2 Validation Timing Rules

| Trigger | Timing | Rule |
|:---|:---|:---|
| Character-by-character typing | Real-time (Tier 1 only) | Only for obviously invalid characters (e.g. reserved chars in slug); never disrupt mid-typing |
| Field blur (`LostFocus`) | On exit | Show Tier 1 format/length errors |
| "Next Stage" / "Continue" | On demand | Execute Tier 2 stage validation for all fields in current stage |
| Stage 7 "Review" entry | Pre-flight | Execute Tier 3–4 async checks; show aggregate results |

### 3.3 Error vs. Warning Distinction

| Condition | Severity | Blocks Progress |
|:---|:---|:---|
| Project name contains reserved char | **Error** | Yes |
| Project path not on Volume D: | **Error** | Yes |
| Game executable path not found on disk | **Warning** | No — user may continue with unverified path |
| Version string set to "UNKNOWN" | **Info** | No — explicitly allowed |
| Anti-cheat flag detected | **Error (Hard Block)** | Yes — non-dismissible |

### 3.4 Value Preservation Rules
- Navigating **Back** in the wizard: All field values in the abandoned stage are retained in ViewModel state.
- Game selection change (Stage 2 → 3 invalidation): Stage 3 version fields for the **changed** game are cleared; Stage 3 fields for the unchanged game are retained.
- Save failure: All in-memory values remain; user must correct the error before retrying.
- Dialog close (Cancel): Values in a form dialog are discarded on explicit Cancel; never discarded on Escape without confirmation if data was entered.

---

## 4. Search Field Interaction Contract

### 4.1 Query Lifecycle

```
[User types query] 
    → Debounce 300ms
    → Emit QueryChanged(query, sequenceToken)
    → ViewModel: SearchAsync(query, sequenceToken)
    → If response.SequenceToken < currentToken: discard (stale)
    → If response.SequenceToken == currentToken: update results
```

### 4.2 States & Transitions

| State | Visual | Accessible Announcement |
|:---|:---|:---|
| Empty / Initial | Placeholder text | — |
| Typing | Live text in field | — |
| Searching | Inline spinner, results grayed | "Searching…" (assertive live region) |
| Results | Result list populated | "N results for [query]" |
| No Results | EmptyStatePanel ("No matches") | "No results for [query]" |
| Error | Error row with retry | "Search failed. [Retry] button available" |
| Cleared | Returns to Empty state | — |

---

## 5. File / Path Selection Contract

File path selection (for game executable paths, project workspace root) uses a **path input composite**:
1. `NleTextInput` for manual path entry with validation.
2. `NleIconButton` (folder icon) triggers `FolderBrowserDialog` via ViewModel command.
3. `NleInlineValidation` shows path existence/permission status.

**Constraints:**
- Project workspace paths must resolve to `D:\` (`DEC-009`); validation rejects other roots with `Error` severity.
- Executable paths accept missing files as `Warning` (not Error) — user may specify paths before the game is installed.
- Raw path strings are never stored in `AutomationProperties` or logs in formats that could expose private directory structures unnecessarily.

---

## 6. Multi-Stage Wizard Navigation Contracts

The wizard `Back` / `Next` / `Create` navigation tray is a **fixed footer component** (`WizardNavigationFooter`) with these invariants:
- `Back` is disabled on Stage 1 only.
- `Next` triggers `StageViewModel.ValidateAsync()` before advancing.
- If validation fails: stage does not advance; `NleInlineValidation` messages appear; first invalid field receives focus (`FocusManager.SetFocusedElement`).
- `Create` appears only on Stage 7; disabled until pre-flight audit passes and compliance checkboxes are checked.
- `Cancel` triggers `ConfirmationDialog` (Destructive severity) if any field has been modified (`IsDirty = true`).

### 6.1 Breadcrumb Stage Indicator
The wizard header displays a linear stage indicator: `[1 Basics] [2 Games] [3 Versions] [4 Goals] [5 Reqs] [6 Strategy] [7 Review]`
- Completed stages render as filled circles; active stage is highlighted; future stages are greyed and not clickable.
- Previously completed stages can be clicked to navigate back (triggers implicit `Back` chain validation on the current stage first).

