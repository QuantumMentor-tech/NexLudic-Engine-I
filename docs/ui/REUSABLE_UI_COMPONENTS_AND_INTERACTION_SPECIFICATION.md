# REUSABLE UI COMPONENTS & INTERACTION SPECIFICATION

**Document Identifier:** `NLE-SPC-CMP-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Component Architecture, Taxonomy, Reuse Rules & Design Contract  
**UI Framework (Authoritative):** WPF / XAML on .NET 8.0 LTS, CommunityToolkit.Mvvm, Fluent Design (`ADR-0003`)  
**Related Documents:**
- `NLE-SPC-SHL-001`: Application Shell & Layout Specification
- `NLE-SPC-WKS-001`: Project Workspace Layout & Navigation Specification
- `NLE-SPC-WIZ-001`: Project Creation Wizard Specification
- `NLE-SPC-CTX-002`: Project Context & Navigation State Specification
- `NLE-DOM-SPEC-001`: Core Domain Model & Capability Contract
- `ADR-0003`: Desktop UI Framework (WPF .NET 8)
- `ADR-0009`: Workspace Filesystem Isolation & Safety Sandbox

---

## 1. Component System Purpose & Mandate

The NexLudic Engine component system governs the **presentation and interaction layer** of the WPF desktop application. It is the contractual specification that ensures every screen, wizard stage, project destination, and contextual panel shares consistent visual language, interaction patterns, accessibility behaviour, and error handling.

The component system is **not** a business logic layer. It does not:
- Evaluate game compatibility or feasibility.
- Authorize AI task execution or file writes.
- Calculate adapter qualification scores.
- Own project lifecycle transitions.
- Manage persistence, IPC, or configuration secrets.

It **does** provide:
- Consistent, testable, accessible building blocks.
- Shared presentation contracts for domain-sourced state.
- Predictable interaction patterns that can be audited and regression-tested.
- A vocabulary that lets AI coding agents build screens without reinventing controls.

---

## 2. Component Taxonomy

The component system is organized into five layers. Components at higher layers compose those below; they may not introduce circular or upward dependencies.

```
+----------------------------------------------------------------------------------------------------+
| LAYER 5: PAGE-COMPOSITION PATTERNS                                                                 |
|   StandardPageLayout, DetailSectionGroup, MasterDetailLayout, ReviewConfirmLayout, EmptyAreaLayout  |
+----------------------------------------------------------------------------------------------------+
| LAYER 4: DOMAIN-AWARE PRESENTATION COMPONENTS                                                      |
|   ProjectLifecycleBadge, FeasibilityOutcomeBadge, CapabilityStatusRow, AdapterQualificationCard,   |
|   OperationStatusRow, ApprovalSummaryCard, TestResultBadge, EvidenceReferenceRow, ArtifactSummaryRow|
+----------------------------------------------------------------------------------------------------+
| LAYER 3: COMPOSITE COMPONENTS                                                                      |
|   FormField, GameSelectorControl, SearchResultList, ConfirmationDialog, FilterBar,                 |
|   EmptyStatePanel, OperationProgressCard, ContextualActionMenu, PaginatedBoundedList,              |
|   SummaryCard, CollectionPanel                                                                      |
+----------------------------------------------------------------------------------------------------+
| LAYER 2: BASIC INTERFACE CONTROLS                                                                  |
|   NleButton, NleTextInput, NleMultilineInput, NleSearchField, NleSelector, NleCheckbox,            |
|   NleRadioGroup, NleToggle, NleLabel, NleTooltip, NleIconButton, NleLink, NleInlineValidation      |
+----------------------------------------------------------------------------------------------------+
| LAYER 1: DESIGN PRIMITIVES (References Established Design System Only)                            |
|   Typography Styles, Semantic Color Tokens, Spacing/Sizing Scale, Icon Presentation,               |
|   Focus Indicator Style, Dividers, Surface Elevations, Border Radii                               |
+----------------------------------------------------------------------------------------------------+
```

---

## 3. Design and API Contract Principles

### 3.1 Single Responsibility
Each component has one clearly stated responsibility. `NleButton` manages activation and loading state; it does not decide whether the user is authorized to perform the underlying action — that belongs to the ViewModel's command preconditions (`ICommand.CanExecute`).

### 3.2 Explicit Configuration, No Hidden Assumptions
Every component expresses its configuration requirements explicitly through documented dependency properties or constructor parameters. No component silently reads global state, navigates to a destination, or modifies project data without an explicit binding from its consumer.

### 3.3 Controlled vs. Self-Managed State

| State Type | Ownership | Examples |
|:---|:---|:---|
| **Authoritative Domain State** | ViewModel → Domain Service | Feasibility outcome, Operation status, Task approval state |
| **Local Ephemeral UI State** | Component-local | Dialog visibility, Tooltip hover, Scroll position |
| **Derived Presentation State** | Component (from bound input) | Validation error display, Loading spinner visibility |
| **Persisted Application State** | Application Layer | Last selected project, Window size |

UI-local state must never override or mask an authoritative domain result. If the ViewModel says an operation failed, the component must present failure — not a cached "last success" state.

### 3.4 Composition Rules
- Layer N components may compose Layer N−1 and below.
- Layer 4 (domain-aware) components bind to ViewModel summaries; they do not reach into Layer 1 token constants for domain-specific meanings.
- Circular composition is prohibited.
- Page-level layouts (Layer 5) must not expose specific game, adapter, or operation details as hardcoded content.

### 3.5 Naming Conventions
- **Basic controls:** `Nle` prefix + noun describing the control role (e.g. `NleButton`, `NleSearchField`).
- **Composite components:** Descriptive noun phrase without prefix (e.g. `FormField`, `GameSelectorControl`, `EmptyStatePanel`).
- **Domain-aware components:** Domain concept + visual role suffix (e.g. `FeasibilityOutcomeBadge`, `OperationStatusRow`).
- **Page patterns:** Descriptive layout noun (e.g. `StandardPageLayout`, `MasterDetailLayout`).
- Names describe the **responsibility**, never a specific game, adapter, or integration method.

### 3.6 Dependency Direction
```
Page-Composition Patterns
       ↓
Domain-Aware Presentations ←── ViewModel Summaries (from Application Layer)
       ↓
Composite Components
       ↓
Basic Interface Controls
       ↓
Design Primitives (Design System tokens only)
```
No component at any layer may import from `NexLudic.Core`, `NexLudic.Adapters`, or game-specific modules. Only ViewModels (in `NexLudic.App`) act as the bridge.

---

## 4. Reuse Threshold & Decision Framework

A UI pattern should become a shared component when **three or more** of the following criteria apply:

1. Used across **two or more distinct screens** with identical or near-identical visual and interaction behaviour.
2. Contains **non-trivial keyboard or accessibility behaviour** that is error-prone to reimplement.
3. Has **semantic status meaning** that must be consistent across the application (e.g. a feasibility badge that varies in a single screen violates epistemic hygiene).
4. Contains a **destructive confirmation gate** or **operation loading state**.
5. Displays **domain-authoritative data** where inconsistent presentation could mislead users into incorrect technical decisions.
6. Requires **centralized regression testing** because a visual or behavioral change in isolation could break many screens.

Simple two-line label-value pairs that appear twice with no special interaction do not need to become components.

---

## 5. Backward Compatibility & Change-Control Commitments

### 5.1 Additive Changes (Safe)
- Adding an optional property with a defined default.
- Adding a new supported `Status` enum value with a defined visual mapping.
- Adding a new accessible label override.

### 5.2 Breaking Changes (Require Migration Review)
- Removing or renaming a required property.
- Changing the meaning of an existing state (e.g. redefining what `Status.Unknown` renders).
- Altering default keyboard focus behaviour.
- Changing the event signature of a user-action notification.
- Introducing a visual treatment that shifts layout dimensions.

Before any breaking change to a shared component, the implementer must audit all known consumers, assess regression risk, update tests, and obtain explicit approval from the step authorizing that implementation task.

---

## 6. Framework-Specific Notes (WPF / .NET 8 / CommunityToolkit.Mvvm)

The following WPF primitives and conventions underpin the component system. These are confirmed by `ADR-0003` and the Shell specification (`NLE-SPC-SHL-001`).

- **Data binding:** `{Binding}` with `INotifyPropertyChanged` via `ObservableObject` from CommunityToolkit.Mvvm.
- **Commands:** `RelayCommand` / `AsyncRelayCommand` for button actions; `CanExecute` governs enabled state.
- **Styles and templates:** Defined in XAML `ResourceDictionary` entries; base styles extend Fluent Design theme.
- **Accessibility:** WPF UI Automation (`AutomationProperties`, custom `AutomationPeer` subclasses).
- **High DPI:** `PerMonitorV2` DPI awareness mode declared in app manifest.
- **Focus management:** Explicit `FocusManager.SetFocusedElement` on dialog open; `Keyboard.Focus` on close.
- **Live regions:** `AutomationProperties.LiveSetting = LiveSetting.Assertive` for critical status changes.

> **Important:** Component API specifications in this document use descriptive property names (e.g. `IsLoading`, `Status`, `ErrorMessage`). Actual XAML dependency property declarations are created only during authorized implementation steps — not in this specification.

