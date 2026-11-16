# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Phase 3 Architecture Handoff

**Document ID:** `NLE-HND-PH3-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 16 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Inter-Phase Engineering Handoff Specification  
**Traceability:** Phase 2 Gate Review (`PHASE_2_GATE_REVIEW.md`), Phase 3 Scope (Steps 17–24)

---

## 1. Executive Summary & Purpose

This document provides the formal **Architecture Handoff** from Phase 2 ("Architecture & Development Foundation") to **Phase 3 ("Desktop Interface & Workspace Management", Steps 17 through 24)**.

Phase 3 is responsible for designing and implementing the desktop presentation shell (`NexLudic.App`), user interface workflows, project creation wizards, unified diff inspection tools, and workspace directory management. This specification establishes the non-negotiable architectural boundaries, data models, persistence contracts, and security rules that Phase 3 must respect and build upon.

---

## 2. Phase 3 Architectural Inputs from Phase 2

```
+----------------------------------------------------------------------------------------------------+
|                               PHASE 3 ARCHITECTURAL INPUT PACKAGE                                  |
+--------------------------+-------------------------------------+-----------------------------------+
| Input Domain             | Authoritative Phase 2 Document      | Key Constraints & Directives      |
+--------------------------+-------------------------------------+-----------------------------------+
| Desktop UI Technology    | `ADR-0003`, `HIGH_LEVEL_ARCH.md`    | WPF / .NET 8.0 LTS / MVVM pattern |
| Component Boundaries     | `CORE_COMPONENT_ARCHITECTURE.md`    | Presentation decoupled from Core  |
| Domain Models & States   | `CORE_DOMAIN_MODEL_SPECIFICATION.md`| Epistemic tiers, 8-state caps     |
| Persistence & Storage    | `PERSISTENCE_AND_DATA_MANAGEMENT.md`| Atomic JSON writes, Volume D:     |
| Workspace & Staging Jail | `ADR-0009`, `NLE-POL-SEC-001`       | <project>/staging/ containment    |
| Configuration Precedence | `CONFIGURATION_INVENTORY_POLICY.md` | 7-layer hierarchy; Security supreme|
| Secrets & DPAPI Vault    | `SECRETS_AND_CREDENTIAL_ARCH.md`    | Opaque refs; DPAPI encryption     |
| Error & Cancel Semantics | `PROTOCOL_ERRORS_TIMEOUTS.md`       | 18-category taxonomy, 2-phase halt|
+--------------------------+-------------------------------------+-----------------------------------+
```

---

## 3. Phase 3 Design Freedoms vs. Architectural Constraints

To guide the Phase 3 engineering team:

### 3.1 What Phase 3 Can Design & Implement Freely:
* **Visual UI / UX Layout:** Design XAML views, themes (Dark/Light/System), typography, window controls, and responsiveness.
* **MVVM Architecture:** Select and configure MVVM view models (e.g. `CommunityToolkit.Mvvm`), reactive bindings, and command bindings.
* **Diff Viewer Presentation:** Implement unified diff visualization, side-by-side syntax-highlighted code editors, and change inspectors.
* **Project Wizard Workflows:** Build interactive, user-friendly step-by-step wizards for game-pair selection, feasibility summaries, and blueprint authoring.
* **Workspace Tree Explorer:** Build file explorer UI components for navigating project staging folders.

### 3.2 Non-Negotiable Constraints Phase 3 MUST Preserve:
1. **Zero Domain Logic in UI:** The presentation layer (`NexLudic.App`) must delegate all business operations to `NexLudic.Core`. ViewModels must not calculate feasibility or validate capabilities directly.
2. **Strict Volume D: Placement (`DEC-009`):** The default project directory must be initialized to `D:\NexLudic\Projects\`. Phase 3 cannot place workspaces on System Volume `C:`.
3. **Staging Containment (`ADR-0009`):** Workspace management must strictly enforce `<project>/staging/`. Any attempt to deploy files to game directories requires an explicit human-in-the-loop confirmation modal.
4. **Opaque Secret References:** Settings screens must never display, log, or export plaintext API keys. All token inputs must immediately enter the DPAPI vault.
5. **Zero Administrative Elevation (`REQ-SEC-001`):** The UI process must execute exclusively at Medium Integrity Level without requesting UAC Administrator privileges.

---

## 4. Phase 3 Handoff Acceptance Criteria

Phase 3 design and implementation tasks (Steps 17–24) will be validated against the following gates:
* **HND-AC-01:** UI project displays project states using the exact domain terminology defined in `NLE-DOM-SPEC-001`.
* **HND-AC-02:** Project directory initialization strictly creates the directory hierarchy specified in `NLE-ARC-DAT-001` Section 6.
* **HND-AC-03:** Diff approval dialog renders unified diffs with explicit [Approve & Deploy] and [Reject & Rollback] actions.
* **HND-AC-04:** AI Settings screen interfaces strictly via `credential_ref` UUIDs backed by Windows DPAPI.
* **HND-AC-05:** Zero circular dependencies between `NexLudic.App` and `NexLudic.Infrastructure`.

