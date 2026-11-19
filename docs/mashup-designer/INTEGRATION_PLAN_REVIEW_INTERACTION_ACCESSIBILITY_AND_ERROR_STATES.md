# NexLudic Engine: Review Interaction, Accessibility & Error States

**Document ID:** `NLE-UX-IPE-001`  
**Phase:** 6 of 12 (Mashup Designer & Integration Planning)  
**Step:** 50 of 100  
**Classification:** Interaction Design, Accessibility & State Handling Specification  
**Status:** IMPLEMENTATION-READY SPECIFICATION  

---

## 1. User Interactions & Confirmation Boundaries

The Integration Plan Review experience operates under strict non-destructive principles:

### 1.1 Non-Destructive Review Principle
- **Passive Read-Only by Default:** Browsing tabs, zooming diagrams, filtering requirements, and expanding inspector drawers perform zero disk writes or descriptor mutations.
- **Explicit Modal Confirmations:** Any action that mutates project state (e.g., recording an architectural decision, triggering a re-assessment pipeline, or archiving a plan) requires an explicit modal dialog explaining the exact consequence:

```
+--------------------------------------------------------------------+
| CONFIRM PLAN RE-ASSESSMENT & SNAPSHOT REVISION                     |
+--------------------------------------------------------------------+
| You are about to re-assemble the integration plan against updated  |
| upstream source files.                                             |
|                                                                    |
| Actions that will occur:                                           |
| 1. Live SHA-256 hashes will be bound to a new snapshot.            |
| 2. Dependency DAG and Quality Gates will be re-evaluated.          |
| 3. Current revision (v1.0.0) will be archived as SUPERSEDED.       |
| 4. New revision (v1.1.0) will become the active plan.              |
|                                                                    |
| [✓] Proceed with Re-assessment             [ Cancel ]              |
+--------------------------------------------------------------------+
```

### 1.2 The Non-Authorization Security Barrier
In strict adherence to `INV-IPU-01`, the interface contains **no buttons** labeled "Execute Plan", "Launch Mashup", "Inject Hooks", or "Start Bridging". All downstream execution remains locked behind Phase 7+ runtime staging gates.

---

## 2. Accessibility Specification (WCAG 2.1 AA Conformance)

The desktop interface conforms to **WCAG 2.1 AA** standards across keyboard navigation, visual contrast, screen reader integration, and semantic UI Automation:

### 2.1 Full Keyboard Navigation Matrix

| Key / Shortcut | Context | Target UI Behavior |
| :--- | :--- | :--- |
| `Tab` / `Shift+Tab` | Global | Moves keyboard focus sequentially through interactive elements. |
| `Arrow Keys` (↑/↓) | Matrix / List | Traverses requirement rows or issue list items. Updates Inspector. |
| `Arrow Keys` (←/→) | Sub-Nav Bar | Switches between sub-views (`Overview` $\leftrightarrow$ `Requirements` $\leftrightarrow$ `Architecture`). |
| `Enter` / `Space` | Any Control | Activates selected button, expands collapsible section, or selects node. |
| `Escape` | Modals / Drawers | Closes Contextual Inspector drawer, dismisses dialog, or clears filter. |
| `Alt + 1..9` | Global Review | Instant keyboard shortcut jumping to Sub-Views 1 through 9. |
| `Ctrl + F` | Requirements / Maps | Focuses instant search/filter input field. |

### 2.2 Visual Focus & Non-Color Contrast Rules
- **Visible Focus Ring:** All focused controls display a distinct 2px outer outline with a minimum contrast ratio of 4.5:1 against the background.
- **Non-Color Reliance Invariant:** Color is never the sole conveyor of information. Every status badge pairs:
  1. A unique icon symbol (`[✓]`, `[▲]`, `[✕]`, `[!]`, `[○]`);
  2. An unambiguous text label (`FULLY_COVERED`, `PARTIAL`, `UNCOVERED`, `INVALID`);
  3. A descriptive accessibility label read by screen readers.

### 2.3 Screen Reader & UI Automation Contracts
All visual graphs, diagrams, and matrices expose equivalent Microsoft UI Automation (UIA) tree structures:
- Architecture and dependency diagrams expose an accessible tree view (`ControlType.Tree`) with parent-child relationships clearly declared.
- Nodes declare `AutomationProperties.Name` containing the item ID, role, and prerequisite list.

---

## 3. System Presentation States: Empty, Loading, Error & Partial

### 3.1 Empty States
Displayed when upstream steps have not yet generated planning artifacts:

```
+--------------------------------------------------------------------+
| [○] NO INTEGRATION PLAN ASSEMBLED YET                              |
| This mashup project does not currently have an active plan.        |
|                                                                    |
| Prerequisites Status:                                              |
| - Step 43 Project Domain: [✓] Complete                             |
| - Step 44 Requirements:   [✓] Complete                             |
| - Step 45 Strategy:       [✕] Missing Strategy Decision            |
|                                                                    |
| Next Action: Evaluate and select an integration strategy in Step 45|
| before assembling an integration plan.                             |
| [ Navigate to Step 45 Strategy Evaluation ]                        |
+--------------------------------------------------------------------+
```

### 3.2 Loading States
During assembly pipeline execution or cryptographic hash checks:
- The main canvas displays deterministic **Skeleton Screens** replicating table headers and cards.
- The status bar displays a pulsing indicator: `Validating 6 source file hashes against Volume D...`
- Interactive buttons are disabled to prevent race conditions.

### 3.3 Error States
When an unrecoverable failure occurs (e.g., corrupted JSON or unauthorized elevation request):

```
+--------------------------------------------------------------------+
| [✕] PLAN LOAD ERROR: Corrupted Descriptor File                     |
| Error Code: ERR_PLAN_DESCRIPTOR_MALFORMED                          |
| File: projects/orion-atlas/plans/active_plan.json                  |
| Reason: JSON Schema validation failed at line 42 (missing 'is_dag')|
|                                                                    |
| Remediation: The file may have been edited externally. Re-run the  |
| Step 49 assembly pipeline to regenerate a valid descriptor.        |
| [ Re-run Plan Assembly ]              [ View Error Log ]           |
+--------------------------------------------------------------------+
```

### 3.4 Partial-Data States
If non-critical sections (e.g., optional telemetry mappings) fail to load:
- The interface renders available sections normally.
- The affected tab displays an amber caution pill: `Mappings (Partial Data)`.
- The overview clearly notes: *"3 of 4 sub-views verified. Data mapping analysis is partial due to missing target schema."*
