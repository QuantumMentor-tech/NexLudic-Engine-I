# UI COMPONENT CATALOG & CONTRACTS

**Document Identifier:** `NLE-CAT-CMP-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Component Catalog, Contracts & State Matrix  
**Parent Document:** `NLE-SPC-CMP-001`

> **Status Legend:** `Existing` = implemented in current repo | `Approved` = specified, authorized for implementation | `Provisional` = proposed, pending upstream decision | `Deferred` = out of current scope

---

## 1. Layer 2: Basic Interface Controls

### CMP-001 · NleButton
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Primary interaction trigger for all application actions (navigation, form submission, operation dispatch, confirmation).
- **Reuse Justification:** Most frequently used control; must have identical loading, disabled, and keyboard semantics across all screens.
- **Required Inputs:** `Label` (string), `Command` (ICommand)
- **Optional Inputs:** `VisualRole` (Primary | Secondary | Tertiary | Destructive | IconOnly), `IsLoading` (bool), `IconSource` (geometry/path), `ToolTipText` (string), `AccessibleLabel` (string override)
- **Output Events:** `Executed` (routed through ICommand)
- **State Ownership:** `IsLoading` driven by ViewModel AsyncRelayCommand state; `IsEnabled` driven by ICommand.CanExecute
- **Visual Variants:** Primary (filled accent), Secondary (outlined), Tertiary (text-only), Destructive (danger-red outlined), IconOnly (icon + accessible label, no visible text)
- **Supported States:** Default, Hovered, Pressed, Focused, Disabled, Loading, DisabledWithReason
- **Accessibility:** `AutomationProperties.Name` = `AccessibleLabel ?? Label`; role = Button; announces loading and disabled states
- **Error Behaviour:** If AsyncRelayCommand faults, button returns to default enabled state; ViewModel propagates error to notification layer
- **Keyboard:** `Enter` / `Space` activates; `Tab` cycles focus; `Shift+Tab` reverses
- **Known Limitations:** Does not independently guard against duplicate submissions — AsyncRelayCommand's `CanExecute=false` during execution is the guard

---

### CMP-002 · NleIconButton
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Compact icon-only action control (toolbar actions, panel toggles, inline record actions).
- **Required Inputs:** `IconSource` (geometry), `AccessibleLabel` (string — required, never optional)
- **Optional Inputs:** `VisualRole`, `IsLoading`, `ToolTipText`
- **Accessibility:** `AutomationProperties.Name` = `AccessibleLabel`; visible tooltip always present; never relies solely on icon color for semantic meaning
- **Known Limitations:** Must not be used for destructive actions unless accompanied by a confirmation dialog

---

### CMP-003 · NleTextInput
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Single-line text entry for project names, slugs, game titles, version strings.
- **Required Inputs:** `Value` (two-way bound string), `AccessibleLabel`
- **Optional Inputs:** `Placeholder`, `MaxLength`, `IsReadOnly`, `IsDisabled`, `ValidationState` (None | Valid | Warning | Error), `ValidationMessage`
- **Supported States:** Default, Focused, Filled, ReadOnly, Disabled, Error, Warning
- **Value Preservation:** Value retained on focus loss; cleared only by explicit ViewModel command

---

### CMP-004 · NleMultilineInput
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Multi-line text capture (mashup thesis, descriptions, custom requirement notes).
- **Required Inputs:** `Value`, `AccessibleLabel`
- **Optional Inputs:** `Placeholder`, `MaxLength`, `MinLines`, `MaxLines`, `ShowCharacterCount`, `IsReadOnly`
- **Notes:** Raw creator wording must never be altered by the component; only the ViewModel may decide to clear or preserve text

---

### CMP-005 · NleSearchField
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Search input with debounced query emission and clear action.
- **Required Inputs:** `QueryText` (two-way), `AccessibleLabel`
- **Optional Inputs:** `Placeholder`, `IsLoading`, `DebounceMs` (default 300)
- **Output Events:** `QueryChanged` (debounced string), `Cleared`
- **Accessibility:** Live region announces "Searching…" during load; "N results found" on completion; "No results" on empty
- **Stale Result Protection:** QueryChanged carries a monotonic sequence token; consumers must discard responses with older tokens

---

### CMP-006 · NleSelector
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Single-value dropdown selection from a bounded list (distribution platform, strategy type, sync model, etc.).
- **Required Inputs:** `SelectedValue` (two-way), `ItemsSource` (IEnumerable of display/value pairs), `AccessibleLabel`
- **Optional Inputs:** `Placeholder`, `IsDisabled`, `GroupBy`
- **Supported States:** Default, Open, Selected, Disabled

---

### CMP-007 · NleCheckbox
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Boolean toggle for acknowledgement, consent, and feature flags (e.g. single-player compliance ack).
- **Required Inputs:** `IsChecked` (two-way bool), `Label`
- **Optional Inputs:** `IsDisabled`, `AccessibleDescription`
- **Keyboard:** `Space` toggles; `Tab` focuses

---

### CMP-008 · NleRadioGroup
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Mutually exclusive option selection within a bounded set (e.g. complexity appetite, sync model).
- **Required Inputs:** `SelectedValue` (two-way), `Options` (list of label/value pairs), `GroupLabel`
- **Keyboard:** Arrow keys navigate within group; `Tab` exits group

---

### CMP-009 · NleToggle
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** On/Off setting toggle for binary configuration options.
- **Required Inputs:** `IsOn` (two-way bool), `Label`
- **Supported States:** On, Off, Disabled

---

### CMP-010 · NleInlineValidation
- **Category:** Basic Control | **Status:** Approved
- **Purpose:** Inline validation message display below a form field.
- **Required Inputs:** `Message` (string), `Severity` (Error | Warning | Info | None)
- **Accessibility:** Associated with parent input via `AutomationProperties.DescribedBy`; severity announced on focus entry when message is present

---

## 2. Layer 3: Composite Components

### CMP-020 · FormField
- **Category:** Composite | **Status:** Approved
- **Purpose:** Canonical composition of Label + Input Control + Required Indicator + Helper Text + Validation Message. Ensures all form fields have identical layout across wizard stages and project destination forms.
- **Required Inputs:** `Label`, `IsRequired`, child control slot
- **Optional Inputs:** `HelperText`, `ValidationState`, `ValidationMessage`, `IsReadOnly`
- **Reuse Justification:** Appears across 7 wizard stages, 12 workspace destinations, and all project configuration forms; inconsistent validation layout is a Level 5 regression risk
- **Layout:** `Label [Required*] / [Child Input Control] / [HelperText] / [ValidationMessage]`

---

### CMP-021 · GameSelectorControl
- **Category:** Composite | **Status:** Approved
- **Purpose:** Domain-specialized selection control for designating a participant game (Game A or Game B). Combines catalog search, manual entry path, identity disambiguation, and version provenance display.
- **Required Inputs:** `SelectedGame` (two-way, GameIdentitySummary?), `Role` (PRIMARY_HOST | DONOR_SECONDARY), `AccessibleLabel`
- **Optional Inputs:** `ExcludeGameId` (prevents duplicate game selection), `IsReadOnly`
- **Output Events:** `GameSelected` (GameIdentitySummary), `ManualEntryRequested`
- **Reuse Justification:** Requires distinct identity, edition, distribution, and provenance metadata handling not achievable with generic NleSelector; anti-duplicate guard (`ExcludeGameId`) is domain-specific

---

### CMP-022 · SearchResultList
- **Category:** Composite | **Status:** Approved
- **Purpose:** Virtualized list of search results with loading, empty, and error states; keyboard navigable.
- **Required Inputs:** `Items` (IEnumerable), `IsLoading`, `HasError`
- **Optional Inputs:** `EmptyMessage`, `ErrorMessage`, `MaxVisibleItems`
- **States:** Loading (skeleton rows), Empty (message), Populated (virtual list), Error (message + retry action)

---

### CMP-023 · ConfirmationDialog
- **Category:** Composite | **Status:** Approved
- **Purpose:** Modal dialog for consequential action confirmation (delete, discard, rollback, archive).
- **Required Inputs:** `Title`, `Message`, `ConfirmLabel`, `CancelLabel`, `Severity` (Info | Warning | Destructive)
- **Output Events:** `Confirmed`, `Cancelled`
- **Focus:** Enters dialog on open (focus on Cancel by default for Destructive); returns to trigger control on close
- **Keyboard:** `Escape` = Cancel; `Enter` = focused button activation

---

### CMP-024 · EmptyStatePanel
- **Category:** Composite | **Status:** Approved
- **Purpose:** Standardized empty-state presentation with icon, title, description, and optional primary action.
- **Required Inputs:** `Title`, `Description`
- **Optional Inputs:** `IllustrationIcon`, `PrimaryActionLabel`, `PrimaryActionCommand`
- **Reuse Justification:** 12 destinations + wizard stages all need distinct but visually consistent empty states

---

### CMP-025 · FilterBar
- **Category:** Composite | **Status:** Approved
- **Purpose:** Horizontal filter row for collections (Requirements list, Task queue, Artifact list).
- **Required Inputs:** `Filters` (IEnumerable of FilterDefinition)
- **Optional Inputs:** `ShowClearAll`, `ActiveFilterCount`
- **Output Events:** `FilterChanged`, `FilterCleared`

---

### CMP-026 · OperationProgressCard
- **Category:** Composite | **Status:** Approved
- **Purpose:** Summary card for a background operation (AI task, adapter scan, test run) with status, indeterminate/determinate progress, and cancel action.
- **Required Inputs:** `OperationTitle`, `Status` (OperationStatus enum), `ProjectId`
- **Optional Inputs:** `ProgressValue` (nullable double 0.0–1.0; null = indeterminate), `CancelCommand`, `DetailLink`
- **State Ownership:** Status sourced from authoritative OperationRecord; never self-calculated

---

### CMP-027 · ContextualActionMenu
- **Category:** Composite | **Status:** Approved
- **Purpose:** Overflow menu (kebab or ellipsis button) for secondary and tertiary record-level actions.
- **Required Inputs:** `Actions` (IEnumerable of ActionDefinition)
- **Keyboard:** `F10` / `Shift+F10` / `Apps` key opens; arrow keys navigate; `Enter` activates; `Escape` closes

---

### CMP-028 · SummaryCard
- **Category:** Composite | **Status:** Approved
- **Purpose:** Bounded card presenting a primary heading, status badge, secondary metadata, and optional click/navigate action.
- **Required Inputs:** `Title`, `Status` (nullable)
- **Optional Inputs:** `Subtitle`, `Metadata` (up to 3 key-value pairs), `NavigateCommand`, `IsLoading`
- **Clickability:** When `NavigateCommand` is set, the entire card surface is a single keyboard-accessible button; no nested interactive controls permitted inside a navigable card

---

### CMP-029 · CollectionPanel
- **Category:** Composite | **Status:** Approved
- **Purpose:** Bounded, virtualized wrapper for record lists with loading, empty, error, and partial-data states.
- **Required Inputs:** `ItemsSource`, `IsLoading`
- **Optional Inputs:** `HasError`, `EmptyState` (EmptyStatePanel config), `MaxItemCount`

---

## 3. Layer 4: Domain-Aware Presentation Components

> These components bind only to ViewModel summary types — they do not compute domain results.

### CMP-040 · ProjectLifecycleBadge
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Displays the authoritative project lifecycle state as a colored chip with label.
- **Required Inputs:** `LifecycleState` (enum: CREATED | RESEARCH | PLANNING | PROTOTYPE | VALIDATED | BLOCKED | ARCHIVED)
- **Authoritative Source:** `ProjectManifest.LifecycleState` via ViewModel
- **Supported Values & Labels:**

| Enum Value | Display Label | Visual Treatment |
|:---|:---|:---|
| `CREATED` | "Created" | Neutral grey |
| `RESEARCH` | "Research" | Blue |
| `PLANNING` | "Planning" | Indigo |
| `PROTOTYPE` | "Prototype" | Orange |
| `VALIDATED` | "Validated" | Green |
| `BLOCKED` | "Blocked" | Red |
| `ARCHIVED` | "Archived" | Muted grey |
| `UNKNOWN` | "Unknown" | Yellow warning |

- **Accessibility:** Text label always present; color not sole indicator

---

### CMP-041 · FeasibilityOutcomeBadge
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Displays the authoritative feasibility evaluation result without implying that any compatibility decision has been confirmed.
- **Required Inputs:** `FeasibilityOutcome` (enum)
- **Authoritative Source:** `FeasibilityAssessment.Outcome` from `FeasibilityAssessmentService`

| Enum Value | Display Label | Visual Treatment |
|:---|:---|:---|
| `UNEVALUATED` | "Not Assessed" | Yellow/Amber |
| `FEASIBLE` | "Feasible (Conditional)" | Green outlined |
| `PARTIAL` | "Partially Feasible" | Orange |
| `BLOCKED` | "Blocked — Evidence Required" | Red |
| `IMPOSSIBLE` | "Not Feasible" | Dark red |
| `STALE` | "Stale — Reassessment Needed" | Yellow warning |
| `UNKNOWN` | "Unknown" | Grey |

- **Critical Rule:** `FEASIBLE` does not mean "compatible in all conditions". Tooltip must display: *"Feasibility is conditional. Requires Phase 5 adapter probing and integration testing to confirm."*

---

### CMP-042 · CapabilityStatusRow
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Single-row display of one declared capability requirement with its current evidence tier and coverage status.
- **Required Inputs:** `CapabilityName`, `EpistemicTier` (DECLARED | DETECTED | PROBED | VERIFIED), `CoverageStatus`
- **Authoritative Source:** `CapabilityRequirement` record from `RequirementsManagementService`
- **Critical Rule:** `DECLARED` tier must never render identically to `VERIFIED` tier

---

### CMP-043 · AdapterQualificationCard
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Summary card for an adapter's identity, qualification status, supported game versions, and known limitations.
- **Required Inputs:** `AdapterName`, `QualificationStatus` (UNSCANNED | QUALIFIED | PARTIAL | INCOMPATIBLE | UNSUPPORTED)
- **Optional Inputs:** `SupportedVersionRange`, `LimitationSummary`, `DetailCommand`
- **Critical Rule:** `QUALIFIED` status must display supported version range prominently; must never imply universal compatibility

---

### CMP-044 · OperationStatusRow
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Single-row summary of an operation (AI task, build step, adapter scan) for use in lists and history views.
- **Required Inputs:** `OperationTitle`, `OperationStatus` (QUEUED | AWAITING_APPROVAL | RUNNING | COMPLETED | PARTIAL | FAILED | CANCELLED | UNCERTAIN), `ProjectId`
- **Authoritative Source:** `OperationRecord` from operation service

| Status Value | Display Label | Visual |
|:---|:---|:---|
| `QUEUED` | "Queued" | Grey |
| `AWAITING_APPROVAL` | "Awaiting Your Review" | Blue pending |
| `RUNNING` | "Running…" | Spinner + Blue |
| `COMPLETED` | "Completed" | Green |
| `PARTIAL` | "Partially Completed" | Orange |
| `FAILED` | "Failed" | Red |
| `CANCELLED` | "Cancelled" | Muted |
| `UNCERTAIN` | "Outcome Unknown" | Yellow warning |

- **Critical Rule:** `UNCERTAIN` must never be presented as `COMPLETED` or `FAILED`; external side effects may have occurred.

---

### CMP-045 · ApprovalSummaryCard
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Displays a pending or resolved AI diff approval request with requested scope, available review action, and decision provenance.
- **Required Inputs:** `ApprovalTitle`, `RequestedScope`, `ApprovalStatus` (PENDING | APPROVED | REJECTED | EXPIRED)
- **Critical Rule:** Displaying this card does NOT approve the request. Only an explicit `Confirmed` event from `ConfirmationDialog` triggers the approval command.

---

### CMP-046 · TestResultBadge
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Displays the outcome of a test execution record.
- **Required Inputs:** `TestOutcome` (NOT_RUN | PASSED | FAILED | INCOMPLETE | SKIPPED | BLOCKED)
- **Critical Rule:** `NOT_RUN` renders visibly distinct from `PASSED`. An empty test record collection is NOT equivalent to a passing result.

---

### CMP-047 · EvidenceReferenceRow
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Single-row display of a research evidence citation with source type, provenance tier, and freshness indicator.
- **Required Inputs:** `ClaimSummary`, `SourceType` (USER_DECLARED | DOCUMENTATION | PROTOTYPE | INTEGRATION_TEST | REGRESSION), `EpistemicTier`, `IsFresh`
- **Critical Rule:** `USER_DECLARED` must render visually distinct from `INTEGRATION_TEST`. Stale evidence (`IsFresh = false`) must display a freshness warning.

---

### CMP-048 · ArtifactSummaryRow
- **Category:** Domain-Aware | **Status:** Approved
- **Purpose:** Compact summary row for a generated workspace artifact (mod package, config file, diff output).
- **Required Inputs:** `ArtifactName`, `ArtifactType`, `ValidationState` (UNVALIDATED | VALIDATED | FAILED | MISSING)
- **Optional Inputs:** `DetailCommand`
- **Critical Rule:** `MISSING` must render as an explicit unavailable state, not as an empty row or zero-byte file.

---

## 4. Layer 5: Page-Composition Patterns

### CMP-060 · StandardPageLayout
- **Category:** Page Pattern | **Status:** Approved
- **Purpose:** Standard two-row page composition (Header Row with title + primary actions; Content Body with optional sections).
- **Used By:** All 12 project workspace destinations

### CMP-061 · MasterDetailLayout
- **Category:** Page Pattern | **Status:** Approved
- **Purpose:** Split view with a navigable record list on the left and a detail panel on the right. Collapses to single-panel on narrower windows.

### CMP-062 · ReviewConfirmLayout
- **Category:** Page Pattern | **Status:** Approved
- **Purpose:** Read-only review summary followed by a primary confirm action and disclaimer. Used by Wizard Stage 7 and AI diff approval screens.

### CMP-063 · EmptyAreaLayout
- **Category:** Page Pattern | **Status:** Approved
- **Purpose:** Full-area centered EmptyStatePanel when a destination has no records. Prevents blank white page confusion.

### CMP-064 · DetailSectionGroup
- **Category:** Page Pattern | **Status:** Approved
- **Purpose:** Visually grouped section within a page with a section heading, divider, and contained content area.

