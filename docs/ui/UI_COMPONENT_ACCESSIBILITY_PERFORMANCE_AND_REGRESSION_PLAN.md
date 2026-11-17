# UI COMPONENT ACCESSIBILITY, PERFORMANCE & REGRESSION PLAN

**Document Identifier:** `NLE-PLN-CMP-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Accessibility Contract, Performance Requirements & Visual Regression Plan  
**Parent Document:** `NLE-SPC-CMP-001`

> **Important:** This document specifies **future implementation requirements**. No accessibility or performance tests have been executed against application code. No compliance claims are made for unimplemented components.

---

## 1. Component-Level Accessibility Contracts

### 1.1 Universal Accessibility Requirements (All Interactive Components)

| Requirement | Implementation Approach |
|:---|:---|
| Accessible name | `AutomationProperties.Name` on every focusable control |
| Role semantics | Correct `AutomationPeer` subclass or `AutomationProperties.AutomationControlType` |
| Keyboard activation | `Enter` / `Space` for all button-like controls; arrow keys for lists and radio groups |
| Visible focus indicator | 2px minimum outline using `NleFocus.Outline` token; NEVER `FocusVisualStyle={x:Null}` |
| Disabled state announced | `AutomationProperties.HelpText` on disabled controls with prerequisites |
| Color independence | Every status badge and severity indicator has an accessible text label; color is supplementary only |
| Text scaling | All layouts tested at 100%, 125%, 150%, 175%, and 200% DPI scaling |

### 1.2 Accessibility Contract per Component Category

| Component | Role | Key Accessibility Rule |
|:---|:---|:---|
| `NleButton` | Button | `Name` = label or `AccessibleLabel` override; loading state announced |
| `NleIconButton` | Button | `Name` = `AccessibleLabel` (required, non-optional); tooltip always visible |
| `NleTextInput` | Edit | Linked to `FormField` label via `AutomationProperties.LabeledBy` |
| `NleSearchField` | Edit | Live region announces "Searching…", result count, "No results" |
| `NleSelector` | ComboBox | Selected value announced on change |
| `NleCheckbox` | CheckBox | `Name` = label text; checked state announced on toggle |
| `NleRadioGroup` | Group of RadioButton | Group name via `GroupBox`; arrow key navigation within group |
| `ConfirmationDialog` | Dialog | Focus trapped inside modal; `Escape` = Cancel; announced as dialog region |
| `SummaryCard` (navigable) | Button | `Name` = `"{Title} — {StatusLabel}"`; entire surface is single focusable target |
| `CollectionPanel` | List | ARIA-equivalent: `AutomationProperties.ItemCount` updated on load |
| `FilterBar` | Toolbar | Filter chip active/inactive state announced on change |
| `FeasibilityOutcomeBadge` | Text + Image | Text label always visible; color not sole indicator; tooltip always present |
| `OperationStatusRow` | ListItem | Status changes announced via `LiveSetting.Polite` on update |
| `EmptyStatePanel` | Region | `AutomationProperties.Name` describes the absent content clearly |

---

## 2. Focus Management Rules

### 2.1 Focus Entry on Dialog Open
- `ConfirmationDialog` (Destructive) → focus on **Cancel** button.
- `ConfirmationDialog` (Info/Warning) → focus on **Confirm** button.
- `FormDialog` → focus on first editable input field.
- `ApprovalReviewDialog` → focus on **Reject** button (conservative default).
- `OperationProgressOverlay` → focus on the progress card's status text (read-only).

### 2.2 Focus Return on Close
- All dialogs and popovers: focus returns to the **exact control** that triggered the open action.
- Project context switch: focus returns to the top of the new project's Overview page heading.
- Contextual panel close: focus returns to the item in the main list that opened the panel.

### 2.3 Focus Never Lost
- If the triggering control is no longer present (e.g. a deleted record), focus falls to the next logical parent container heading, then to the first focusable control within the destination.

---

## 3. Screen Reader Semantic Requirements

### 3.1 Live Region Specifications

| Region | LiveSetting | Content |
|:---|:---|:---|
| Search results | Polite | "N results found for [query]" / "No results" / "Search failed" |
| Toast notifications (Info/Warning) | Polite | Full notification message |
| Toast notifications (Error/Critical) | Assertive | Full notification message |
| Form validation failure (stage exit) | Assertive | "Validation failed: [first error field label] — [error message]" |
| Operation status update | Polite | "[Operation name] status changed to [new status]" |
| Pre-flight audit result | Assertive | "Pre-flight check failed: [blocking reason]" |

### 3.2 Status Badges — Screen Reader Text
All `ProjectLifecycleBadge`, `FeasibilityOutcomeBadge`, `TestResultBadge`, and `CapabilityStatusRow` components expose their display label plus supplementary description via `AutomationProperties.HelpText`. Example:
- Visible: "Not Assessed" (badge)
- Screen reader: "Feasibility status: Not Assessed. Run a feasibility audit to evaluate declared capabilities."

---

## 4. Motion & Animation

- **Transition duration:** Maximum 200ms for page transitions; 150ms for panel expansion/collapse; 100ms for tooltip/popover show.
- **Reduced Motion:** All component transitions must respect `SystemParameters.ClientAreaAnimation` / Windows "Reduce motion" accessibility setting. When disabled: transitions are instant (no fade, no slide).
- **Motion as sole indicator:** Prohibited. A spinner must be accompanied by a text label ("Running…"); an animated skeleton must be accompanied by an `AutomationProperties.Name` of "Loading content".

---

## 5. Performance Requirements

### 5.1 Rendering Performance
- **Navigation transition:** < 200ms from button click to destination content skeleton visible.
- **Project context switch:** < 500ms from switch confirmation to new project Overview skeleton.
- **Dialog open:** < 50ms from trigger to dialog visible.
- **Search result first render:** < 300ms from debounced query emit to skeleton → results transition.

### 5.2 Virtualization Requirements
- Collections of 50+ items: `VirtualizingStackPanel` with `VirtualizationMode.Recycling`.
- Collections of 200+ items (History, Artifact list): additionally requires `IsVirtualizing=true` on `ItemsControl`.
- Large DataGrid tables (Diagnostic logs): row height must be fixed for efficient virtualization.

### 5.3 Stale Asynchronous Update Protection
Every async data fetch must carry a **correlation token** (monotonic counter per destination load, or `ActiveProjectId` as discriminator). Components reject updates whose token does not match the current active request.

```
// Conceptual ViewModel pattern (pseudocode — not an implementation authorization)
var token = Interlocked.Increment(ref _currentToken);
var result = await _service.FetchAsync(query);
if (result.Token != _currentToken) return; // Discard stale result
UpdateDisplayCollection(result.Items);
```

### 5.4 Event Handler Lifecycle
- All ViewModel subscriptions to service events (task completion, approval received) must be released in `IDisposable.Dispose()` or `CommunityToolkit.Mvvm.Messaging` weak reference patterns.
- Dialogs, contextual panels, and popovers must unsubscribe from parent events when closed.

### 5.5 Expensive Work Prohibition
The following operations are prohibited inside any component's rendering or data-binding code:
- Filesystem access (game path existence checks → move to async ViewModel command).
- Registry queries.
- IPC calls.
- AI prompt dispatch.
- Feasibility calculations.
- Adapter qualification evaluation.

---

## 6. Future Component Validation Requirements

### 6.1 Component-Level Tests (per shared component)
When authorized implementation steps build shared components, each component requires:
1. **Unit tests** for all state transitions (default → loading → success; default → loading → error).
2. **Property validation tests** (required inputs missing → component gracefully renders an error/null state without throwing).
3. **Accessibility verification** (UI Automation tree correct element structure, name, role, state).
4. **Keyboard-only interaction test** (activate, navigate, dismiss without mouse).
5. **High contrast mode visual check** (all text and borders legible).
6. **Text scaling test** at 150% DPI (no content clipping, action buttons reachable).

### 6.2 Visual Regression Requirements
- Baseline screenshots captured per component variant per theme (Light, Dark, High Contrast) during first implementation.
- Any change to a shared component triggers visual diff review against baselines for all known consumer screens.
- Spacing, typography, and color must not change between component versions without an explicit design system update and cross-consumer impact review.

### 6.3 Consumer Impact Analysis (Before Any Shared Component Change)
Before modifying any Layer 2–4 component, the implementer must:
1. Run a cross-reference search across all XAML files consuming the component.
2. List every consumer screen and variant.
3. Confirm all tests for affected consumers pass with the proposed change.
4. Obtain explicit authorization if any breaking change to public API, state semantics, or layout is required.

### 6.4 Security & Permission-Boundary Presentation Tests
- Verify: `ApprovalReviewDialog` does not apply changes without explicit Confirm event.
- Verify: Disabled buttons expose `CanExecute=false` reasons via tooltip; they do NOT silently succeed.
- Verify: Sensitive field values (DPAPI refs) do not appear in `AutomationProperties.Name` or tooltip text.
- Verify: Error detail dialogs do not expose raw stack traces, file paths beyond the project root, or internal service exception messages.

---

## 7. AI Coding Agent Component Implementation Rules

When AI coding agents implement these components in future authorized steps, the following rules are mandatory:

1. **Inspect the catalog first:** Check `NLE-CAT-CMP-001` before creating any new control.
2. **No business logic in components:** ViewModel provides state; component presents it.
3. **Required `AccessibleLabel` for IconOnly:** Never omit `AccessibleLabel` on `NleIconButton`.
4. **Status badge tooltips:** `FeasibilityOutcomeBadge` and `CapabilityStatusRow` tooltips are non-optional.
5. **Stale result token check:** Every async fetch in a ViewModel must implement correlation token discard.
6. **No inline `FocusVisualStyle={x:Null}`:** Focus outlines are never removed.
7. **`ConfirmationDialog` before all destructive commands:** No destructive `RelayCommand` executes without routing through `ConfirmationDialog` first.
8. **Domain status enums are sealed:** Do not add new enum values to `FeasibilityOutcome`, `OperationStatus`, or `TestOutcome` without a domain model update approved in a dedicated architecture step.

