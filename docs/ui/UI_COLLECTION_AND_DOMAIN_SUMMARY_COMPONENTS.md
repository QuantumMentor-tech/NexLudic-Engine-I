# UI COLLECTION & DOMAIN SUMMARY COMPONENTS

**Document Identifier:** `NLE-SPC-COL-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Collection Patterns, Domain Summary Components & Selection Contracts  
**Parent Document:** `NLE-SPC-CMP-001`

---

## 1. Layout Selection Guide: When to Use Each Collection Pattern

| Data Type | Volume | Interaction | Recommended Pattern |
|:---|:---|:---|:---|
| Project list (all projects) | 1–100 | Click to open | `CollectionPanel` with `SummaryCard` rows |
| Requirement records | 1–50 | View + edit + delete | `MasterDetailLayout` (list left, detail right) |
| Capability chips | 3–20 | Multi-select | Chip group within `DetailSectionGroup` |
| Feasibility evidence | 1–30 | View + filter | `CollectionPanel` with `EvidenceReferenceRow` rows |
| AI task queue | 1–20 | View + approve | `CollectionPanel` with `OperationStatusRow` rows |
| Test results | 1–200 | Filter + inspect | `CollectionPanel` with `TestResultBadge` rows + `FilterBar` |
| Artifact files | 1–500 | Browse + action | `CollectionPanel` with `ArtifactSummaryRow` + `FilterBar` |
| Project history events | 1–1000 | Scroll + filter | Virtualized `CollectionPanel` with timeline rows |
| Adapter catalog | 5–50 | Browse + bind | Grid of `AdapterQualificationCard` |

**Decision Rule:** Use a table (`DataGrid`) only when sorting multiple columns simultaneously is a core user workflow. Prefer structured row components for most project record types.

---

## 2. CollectionPanel (CMP-029) — Detailed Contract

### 2.1 States
```
CollectionPanel
├── [IsLoading = true] → SkeletonRows (3 animated placeholder rows)
├── [HasError = true] → InlineErrorCard (message + [Retry] command)
├── [Items.Count = 0, IsLoading = false] → EmptyStatePanel (CMP-024)
└── [Items.Count > 0] → VirtualizingStackPanel of item components
```

### 2.2 Virtualization
For collections exceeding 50 items, `VirtualizingStackPanel` with `VirtualizationMode.Recycling` is used. Items outside the visible viewport are recycled. This prevents memory growth in long activity history views.

### 2.3 Asynchronous Refresh Protection
The `CollectionPanel` ViewModel binding must carry a `ProjectId` correlation token. If a data response arrives carrying a `ProjectId` that does not match the currently active project, the response is discarded and the collection is not updated.

---

## 3. SummaryCard (CMP-028) — Detailed Contract

A `SummaryCard` is a compact, optionally navigable record summary combining a title, status badge, and up to three key-value metadata pairs.

### 3.1 Layout Anatomy
```
+----------------------------------------------------------+
| [Status Badge]  Title (bold, truncated at 1 line)        |
|                 Subtitle (secondary, truncated at 1 line) |
| Metadata Key 1: Value 1    Metadata Key 2: Value 2        |
| [Primary Action Button — optional]                        |
+----------------------------------------------------------+
```

### 3.2 Clickable Card Rules
- When `NavigateCommand` is set, the entire card surface is a single `Button` control (not nested buttons inside a `Button`).
- **No nested interactive controls** inside a navigable `SummaryCard` surface. Secondary actions appear in a `ContextualActionMenu` triggered from an ellipsis `NleIconButton` anchored to the card corner.
- `Tab` focuses the card; `Enter` / `Space` activates navigation.
- Accessible name: `"{Title} — {StatusLabel}"` via `AutomationProperties.Name`.

### 3.3 Overflow & Long Content
- Title: truncated with ellipsis at 1 line; full title exposed via tooltip on focus or hover.
- Subtitle: truncated at 1 line; full value via tooltip.
- Metadata values: truncated at field width; ellipsis with tooltip.

---

## 4. GameSelectorControl (CMP-021) — Detailed Contract

The game selector is the most domain-specific composite control in the system.

### 4.1 Selection Modes
1. **Catalog Search Mode (default):** `NleSearchField` → `SearchResultList` populated from known-games registry.
2. **Manual Entry Mode:** Triggered by "Define Custom / Unlisted Game…" item at the bottom of the search results list; opens a `FormDialog` with fields for title, executable name, engine type, and architecture.

### 4.2 Search Result Item Display
Each result row in the catalog search shows:
- Official game title (bold)
- Developer / publisher
- Distribution platform badge (Steam / GOG / Epic / Custom)
- Known engine tag (Creation Engine / idTech 7 / Unity / etc.)
- Known anti-cheat flag indicator (red ⚠ icon if `AntiCheat = true`)

### 4.3 Disambiguation for Similar Titles
When two or more results share identical or very similar titles (e.g. "The Elder Scrolls V: Skyrim" vs "Skyrim Special Edition" vs "Skyrim Anniversary Edition"):
- Each item must display edition and distribution channel as distinct visible metadata.
- The selector never silently selects one edition when multiple editions match a fuzzy query.

### 4.4 Anti-Duplicate Enforcement
The `ExcludeGameId` property is provided by the ViewModel owning the two-game selection. The selector hides or disables items whose `CanonicalId` matches `ExcludeGameId`. The selector does not independently calculate project rules — it enforces exclusion purely at the presentation level based on the ViewModel-provided value.

### 4.5 Selected State Display
After selection:
```
+------------------------------------------+
| [Platform Icon]  Game Title (bold)        |
|                  Edition · Distribution   |
|                  Version: [Declared]      |
| [Change Selection]                        |
+------------------------------------------+
```

---

## 5. MasterDetailLayout (CMP-061) — Record Lists with Inline Detail

### 5.1 Layout Behaviour
- **Wide (≥ 1200px):** Left list (280px fixed) + Right detail panel (remaining width).
- **Narrow (< 1200px):** List fills full width; selecting a record navigates to a detail view; detail has a `[← Back to List]` control.

### 5.2 Selection State
- Selected row is highlighted with `NleSelection.Active` color token.
- Keyboard: Arrow keys navigate rows; `Enter` opens the detail panel; `Escape` returns focus to list.
- When the selected record is deleted, focus moves to the next available row; if none, focus returns to the list heading.

---

## 6. FilterBar (CMP-025) — Filter Interaction Contract

### 6.1 Filter Chip States
- **Inactive:** Outlined chip, light background.
- **Active:** Filled chip with accent color + ✕ clear icon.
- **Clearing:** Clicking ✕ on an active chip clears that filter and refreshes the list.
- **Clear All:** Visible only when ≥ 2 filters are active.

### 6.2 Filter and Stale Result Protection
When a filter changes, the ViewModel issues a new query with a fresh sequence token. Responses carrying older tokens are discarded. The filter chip enters a brief "searching" state (spinner) until fresh results arrive.

---

## 7. Table / DataGrid Usage Rules

DataGrid (`DataGrid` XAML control) is reserved for:
- Side-by-side multi-column comparison (e.g. capability matrix: Game A coverage vs. Game B coverage).
- Sortable logs/event tables (Diagnostic Log view, History view).

### 7.1 Accessible Column Headers
All column headers use `AutomationProperties.Name` with the column's semantic meaning (not the internal property name). Sort state is announced: *"Title column, sorted ascending"*.

### 7.2 Row Actions
Inline row action buttons appear in a dedicated "Actions" column. Destructive row actions (Delete) always use `ContextualActionMenu` (CMP-027) rather than an inline `NleButton` to prevent accidental activation.

---

## 8. Domain-Specific Summary Components — Context of Use

| Component | Used In | Key Domain Rule |
|:---|:---|:---|
| `ProjectLifecycleBadge` | Overview, Projects list, Shell header | Never default to `VALIDATED` for new projects |
| `FeasibilityOutcomeBadge` | Overview, Feasibility destination, Cards | Always show tooltip; `UNEVALUATED` for new projects |
| `CapabilityStatusRow` | Requirements list | `DECLARED` ≠ `VERIFIED`; always render distinct |
| `AdapterQualificationCard` | Adapters destination | `QUALIFIED` always scoped to version range |
| `OperationStatusRow` | AI Tasks list, Shell status tray | `UNCERTAIN` never merged with success or failure |
| `ApprovalSummaryCard` | AI Tasks approval queue | Card display does NOT approve |
| `TestResultBadge` | Testing destination | Empty collection = `NOT_RUN`, not `PASSED` |
| `EvidenceReferenceRow` | Feasibility research list | `USER_DECLARED` distinct from `INTEGRATION_TEST` |
| `ArtifactSummaryRow` | Workspace artifacts list | `MISSING` renders as unavailable, not empty |

