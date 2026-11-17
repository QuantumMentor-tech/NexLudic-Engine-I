# Compatibility Report Accessibility, Security, and Performance Plan

**Document ID:** `NLE-PLN-ASP-001`  
**Version:** 1.0.0  
**Phase:** 4 of 12 — Game Discovery & Compatibility Intelligence  
**Step:** 32 of 100 — Game-Pair Compatibility Report & Recommendation Experience Specification  
**Classification:** Non-Functional Quality, Security, and Accessibility Specification  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-DOM-SPEC-001`, `REQ-SEC-001`, `REQ-SAFE-001`, `DEC-009`, `UI_COMPONENT_ACCESSIBILITY_PERFORMANCE_AND_REGRESSION_PLAN.md`

---

## 1. Accessibility Architecture (WCAG 2.1 AA / Section 508)

The Compatibility Report view (`NAV-PRJ-FEA`) is an information-dense technical dashboard. Accessibility must ensure that keyboard-only users, screen-reader users, and visually impaired engineers can navigate, understand, and audit complex compatibility intelligence with equal fidelity.

### 1.1 Keyboard Navigation & Focus Order Contract
- **Logical Tab Order:**
  1. Main Workspace Navigation Bar -> 2. Report Breadcrumbs -> 3. Reassessment Action (if stale) -> 4. Executive Summary Actions -> 5. Strategy Cards (`Tab` moves between cards; `Space`/`Enter` activates `[Inspect Route]`) -> 6. Requirements Filter Chips -> 7. Requirement Rows -> 8. Evidence Drawer (when open).
- **Focus Trap in Modals & Drawers:** When the `EvidenceDrilldownDrawer` is open, keyboard focus is constrained within the drawer using standard WAI-ARIA dialog practices. Pressing `Escape` closes the drawer and restores focus to the exact triggering element on the report surface.
- **Visible Focus Indicator:** High-contrast 2px solid cyan/indigo outline with 2px offset (`var(--focus-ring-color)`) on all interactive buttons, cards, filter chips, and citation links.
- **Skip Links:** Top of report provides `"Skip to Strategy Comparison"` and `"Skip to Requirements Grid"` shortcuts for rapid keyboard navigation.

### 1.2 Screen-Reader Semantics & ARIA Landmarks
- **Semantic Structure:**
  - Header: `<header role="region" aria-label="Assessment Context & Environment">`
  - Executive Summary: `<section aria-labelledby="exec-summary-heading">` with `<h2>`
  - Strategy Comparison: `<section aria-labelledby="strategy-compare-heading">` with `<h2>`, containing three `<article role="region" aria-labelledby="strategy-x-title">`
  - Requirements Grid: `<div role="table" aria-label="Capability Requirements Coverage">` with semantic row and column headers.
- **Live Announcements (`aria-live="polite"`):**
  - Background reassessments announce: *"Reassessment complete. 4 of 5 requirements satisfied. Strategy A is recommended."*
  - Evidence drawer open: *"Evidence Forensics drawer opened for citation EVD-8021."*
  - Error occurrences announce: *"Error: Rule execution timeout. Click Retry with Core RuleSet."*

### 1.3 Color Contrast & Visual Token Compliance
- **Text Contrast:** All text content achieves a minimum contrast ratio of 4.5:1 against its background. Large headings (18pt+) achieve 3.0:1.
- **Non-Color Coding:** Every status badge pairs color tokens with explicit text and distinct SVG iconography:
  - `FEASIBLE_CONDITIONAL`: Green outline + `icon-check-circle` + Text: "Feasible (Conditional)"
  - `PARTIAL`: Amber outline + `icon-slash-circle` + Text: "Partially Feasible"
  - `UNSUPPORTED`: Red outline + `icon-x-circle` + Text: "Technically Unsupported"
  - `BLOCKED`: Solid dark red + `icon-shield-alert` + Text: "Blocked by Policy"

---

## 2. Security, Privacy & Sandboxing Boundaries

The Compatibility Report view operates strictly under the Windows Medium Integrity Level and Volume D isolation contracts established in `REQ-SEC-001`, `REQ-SAFE-001`, and `DEC-009`.

### 2.1 Path Redaction & Host Environment Privacy
Local game installations and mod directories frequently contain user account names or sensitive filesystem structures. The report view enforces mandatory client-side path redaction:
- `C:\Users\<user>\...` is automatically normalized to `~` or `<UserHome>`.
- Project storage paths on `D:\NexLudic\Projects\<id>\` are normalized to `<ProjectRoot>\`.
- File sizes and disk statistics are rounded to readable units without exposing exact sector layouts or volume serials.

### 2.2 Untrusted Source Content Handling
Evidence records often include raw text from community mod repositories, API documentation, or forum threads:
- **No Raw HTML Execution:** All evidence excerpts are parsed and rendered via a strict Markdown-to-AST parser. Raw `<script>`, `<iframe>`, `<object>`, and `style` attributes are discarded.
- **External URL Defense:** Links to external sites (e.g. NexusMods, PCGamingWiki) must never be executed automatically. They require an explicit user click, display an external navigation warning dialog (`CMP-015`), and open in the default OS browser using `rel="noopener noreferrer"`.
- **Zero Binary Execution:** The report UI has zero capability to trigger binary execution, process injection, DLL loading, or registry editing. Viewing an evidence record that references a DLL does NOT load or inspect the DLL binary directly.

---

## 3. Large-Report Performance & Resource Budgets

In complex projects involving extensive total-conversion mods, a compatibility report may encompass dozens of requirements, hundreds of rules, and extensive evidence excerpts.

### 3.1 Performance Budgets & SLA Targets

| Operation / Metric | SLA Target | Measurement Methodology |
|:---|:---|:---|
| **Initial Report Render** | $< 150\text{ ms}$ | Time from ViewModel data arrival to first contentful paint of report grid. |
| **Drawer Slide-In Animation** | $60\text{ fps}$ ($16.6\text{ ms}$/frame) | Hardware-accelerated CSS transform; zero layout thrashing. |
| **Requirements Filtering (100 items)** | $< 16\text{ ms}$ | Instant in-memory filter across 8 dimensions without server round-trip. |
| **Memory Footprint (Report View)** | $< 35\text{ MB}$ additional RAM | Heap memory delta for report ViewModel, DOM nodes, and cached traces. |

### 3.2 UI Virtualization & Progressive Loading
- **Virtualized Requirements Table:** When a project defines $> 25$ requirements, the requirement coverage list utilizes virtualized row rendering (`CMP-010 VirtualizedList`), rendering only visible DOM nodes plus a 5-item buffer.
- **Deferred Evidence Body Loading:** The primary report payload contains only evidence headers (`EvidenceCitationSummary`: ID, title, tier, hash). Full evidence text, Markdown bodies, and AST previews are fetched on-demand only when the user opens the `EvidenceDrilldownDrawer`.
- **Stable Component Identity:** All rows and cards utilize stable keys (`RequirementId`, `StrategyId`, `EvidenceId`) to ensure zero-flicker differential DOM patching during background status updates.

