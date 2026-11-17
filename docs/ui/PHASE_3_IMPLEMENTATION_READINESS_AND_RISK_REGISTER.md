# Phase 3 Implementation Readiness and Risk Register

**Document ID:** NLE-REG-RSK-001  
**Version:** 1.0.0  
**Status:** Approved — Formal Risk Register  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 24 of 100  
**Date:** 2026-11-17  
**Parent Framework:** NexLudic Engine System Architecture & Development Foundation  
**Classification:** Quality, Risk Management & Implementation Readiness  

---

## 1. Purpose and Scope

This register documents all technical risks, architectural gaps, and implementation dependencies identified during the Phase 3 Desktop Interface & Workspace Management readiness review.

Future engineering teams entering Phase 4 ("Core Engine Implementation & Project Engine Baseline") must use this register to understand:
- Explicit design limitations that cannot be assumed in code.
- Dependencies on Phase 4 infrastructure (such as idempotency keys and storage contracts).
- Verification requirements that must be executed during runtime development.

---

## 2. Risk Evaluation Criteria

In alignment with the Phase 2 risk architecture (`NLE-ARC-RSK-001`), risks are evaluated across:
- **Severity:** `Critical` (blocks architecture/safety), `High` (impacts core user data/recovery), `Medium` (impacts user experience or deferred capability), `Low` (minor procedural or aesthetic gap).
- **Likelihood:** `High` (certain to occur unless mitigated), `Medium` (occurs under specific operational failures), `Low` (rare edge case).
- **Classification:** `Blocking` (must be resolved before phase exit) vs. `Non-Blocking` (safe to proceed with documented mitigation and conditions).

---

## 3. Comprehensive Risk and Dependency Register

### RSK-UI-001: Absence of Draft Persistence Across Unexpected Process Termination

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-001 (associated with `GAP-FVR-001`) |
| **Description** | NexLudic Engine does not implement background draft auto-save across unexpected OS process termination (power cut, task kill). Unsaved in-memory form edits will be lost. |
| **Evidence** | `NLE-SPC-FVR-001 §14`, `NLE-SPC-FSM-001 §8` (INV-004). |
| **Affected Steps** | Step 20 (Wizard), Step 21 (Workspace), Step 23 (Forms). |
| **Consequence** | User must re-enter uncommitted wizard or form data after a sudden application crash. Already-saved project files are unaffected. |
| **Severity / Likelihood**| **Medium** / **Low** |
| **Required Mitigation** | The navigation guard (`IsDirty`) prevents accidental UI exit. Epistemic honesty: the application does not falsely claim draft recovery. Crash recovery auto-save is scheduled for Phase 5 persistence enhancements. |
| **Decision Authority** | Application Architecture Team |
| **Classification** | **Non-Blocking** (Acceptable operational limitation for early development phases). |
| **Exit Condition** | Phase 5 implements `.recovery/drafts/` journaling. |
| **Current Status** | Documented & Accepted. |

---

### RSK-UI-002: Undefined Idempotency Tokens for Side-Effect Operations

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-002 (associated with `GAP-FVR-002`) |
| **Description** | The persistence and task execution layers do not yet generate idempotency tokens for file-write, adapter installation, or AI diff applications. |
| **Evidence** | `NLE-SPC-REC-001 §3.1`, `NLE-SPC-FVR-001 §14`. |
| **Affected Steps** | Step 22 (Actions), Step 23 (Recovery). |
| **Consequence** | Blind automatic retry after `PARTIAL` or `UNCERTAIN` outcomes could potentially duplicate side effects (e.g., duplicate file creation or double-applied diffs). |
| **Severity / Likelihood**| **High** / **Medium** |
| **Required Mitigation** | The UI specification strictly prohibits automatic retry for `PARTIAL` and `UNCERTAIN` outcomes (VR-GLB-004, R-01–R-05). All retries require explicit manual user acknowledgement and review of diagnostic logs. |
| **Decision Authority** | Core Persistence & Task Architecture Team |
| **Classification** | **Non-Blocking for Phase 3** (Mitigated by strict UI safety guards; requires Phase 4 data layer contract). |
| **Exit Condition** | Phase 4 persistence implements transactional idempotency keys. |
| **Current Status** | Documented & Guarded by UI Contracts. |

---

### RSK-UI-003: Absence of Three-Way Visual Merge for External Edit Conflicts

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-003 (associated with `GAP-FVR-003`) |
| **Description** | If an underlying project file is modified by an external tool while open in the workspace, the conflict resolution UX is currently limited to Reload (adopt external) or Overwrite (keep mine). Interactive diff-based merge is undefined. |
| **Evidence** | `NLE-CAT-ERR-001 §13` (CAT-11), `NLE-SPC-FSM-001 §6`. |
| **Affected Steps** | Step 21 (Workspace), Step 23 (Validation). |
| **Consequence** | Users cannot perform granular line-by-line conflict resolution when external tools concurrently edit project files. |
| **Severity / Likelihood**| **Low** / **Low** |
| **Required Mitigation** | Conflict detection prevents silent overwrites. Users are given clear binary options to preserve their changes or reload. |
| **Decision Authority** | UI/UX Architecture Team |
| **Classification** | **Non-Blocking** (Single-user local-first desktop application makes concurrent external edits rare). |
| **Exit Condition** | Phase 5 workspace enhancements add interactive visual merge editor. |
| **Current Status** | Documented & Accepted. |

---

### RSK-UI-004: Procedural Omission of Standalone Step 17 and Step 19 Files

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-004 (associated with `DISC-01`, `DISC-02`) |
| **Description** | Deliverables for Step 17 (Design System Foundation) and Step 19 (Dashboard) were absorbed into Steps 18, 21, and 22 rather than created as standalone documents with matching roadmap filenames. |
| **Evidence** | `NLE-AUD-PH3-001 §3.1, §3.3`, `STEP_18_COMPLETION_REPORT.md §2`. |
| **Affected Steps** | Step 17, Step 19. |
| **Consequence** | Reviewers expecting specific filenames from the preliminary roadmap prompt must look at surrounding specifications. |
| **Severity / Likelihood**| **Low** / **High** |
| **Required Mitigation** | `PHASE_3_ARTIFACT_INVENTORY_AND_COMPLETION_AUDIT.md` and `PHASE_3_REQUIREMENTS_TRACEABILITY_MATRIX.md` establish the explicit cross-mapping proving that 100% of technical requirements were satisfied in adjacent deliverables. |
| **Decision Authority** | Documentation & Quality Review Board |
| **Classification** | **Non-Blocking** (Documentation naming nuance; technical scope completely fulfilled). |
| **Exit Condition** | Phase 3 Gate formalizes the consolidation. |
| **Current Status** | Audited & Cross-Referenced. |

---

### RSK-UI-005: Runtime Accessibility & Screen Reader Verification Dependency

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-005 (associated with `GAP-AVS-001`) |
| **Description** | Accessibility specifications (WCAG 2.1 AA) are authored to high architectural standards, but runtime verification using NVDA and Windows Narrator cannot occur until source code is implemented. |
| **Evidence** | `NLE-REV-UXS-001 §4.2`, `NLE-PLN-FVR-001 §14`. |
| **Affected Steps** | Step 18, Step 20, Step 21, Step 22, Step 23. |
| **Consequence** | Potential accessibility defects could be introduced during XAML implementation if developers omit specified UI Automation attributes. |
| **Severity / Likelihood**| **Medium** / **Medium** |
| **Required Mitigation** | Detailed test cases (`TVR-A11Y-001` through `TVR-A11Y-004`) and AI Coding Agent Rules (`NLE-PLN-SHL-001 §3`) mandate accessibility attributes for all generated XAML. Physical assistive tech verification is a Phase 4 implementation gate. |
| **Decision Authority** | Accessibility & QA Engineering Lead |
| **Classification** | **Non-Blocking for Phase 3** (Design is complete; testing requires code). |
| **Exit Condition** | Phase 4 executes live automated (Accessibility Insights) and manual screen reader test suites. |
| **Current Status** | Test Specifications Formally Authored in `NLE-PLN-FVR-001`. |

---

### RSK-UI-006: Persistence Layer Concurrency Token Implementation

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-006 (associated with `GAP-FSM-002`) |
| **Description** | The UI specification defines conflict handling (CAT-11) based on entity version tokens or ETags. The exact schema for these tokens must be supplied by the Phase 4 persistence services. |
| **Evidence** | `NLE-SPC-FSM-001 §6.2`. |
| **Affected Steps** | Step 23 (Validation & State Model). |
| **Consequence** | If Phase 4 persistence does not include version sequence tracking in JSON manifests, optimistic concurrency detection in UI ViewModels cannot engage. |
| **Severity / Likelihood**| **Medium** / **Medium** |
| **Required Mitigation** | Phase 4 Data Access specification must mandate an `entity_version` or monotonic sequence field in all project and metadata manifests. |
| **Decision Authority** | Persistence Architecture Lead |
| **Classification** | **Non-Blocking for Phase 3** (UI contract defined; implementation dependency on Phase 4). |
| **Exit Condition** | Phase 4 data layer includes version tracking in entity contracts. |
| **Current Status** | Contract Dependency Formally Recorded. |

---

### RSK-UI-007: Visual Regression Tooling Dependency for Desktop WPF

| Field | Value |
|---|---|
| **Risk ID** | RSK-UI-007 |
| **Description** | Desktop WPF on .NET 8 has fewer standardized headless visual regression tools compared to web platforms. Tool selection for visual diffing is deferred. |
| **Evidence** | `NLE-PLN-CMP-001 §5`. |
| **Affected Steps** | Step 22 (Components), Step 23 (Regression). |
| **Consequence** | Visual drift across DPI scaling or theme changes might require manual inspection if automated screenshot diffing is not set up early in Phase 4. |
| **Severity / Likelihood**| **Medium** / **Medium** |
| **Required Mitigation** | Phase 4 scaffolding must evaluate FlaUI / WinAppDriver / Appium screenshot testing or render off-screen WPF Visuals for automated bitmap comparison. |
| **Decision Authority** | QA & Tooling Lead |
| **Classification** | **Non-Blocking for Phase 3** (Design specification complete; tooling belongs to Phase 4 build pipeline). |
| **Exit Condition** | Automated visual snapshot test runner configured in Phase 4 CI. |
| **Current Status** | Tooling Requirement Documented. |

---

## 4. Aggregate Implementation Readiness Matrix

```
+----------------------------------------------------------------------------------------------------+
|                                PHASE 3 IMPLEMENTATION READINESS SUMMARY                            |
+--------------------------------------+---------------+---------------------+-----------------------+
| Subsystem / Architectural Domain     | Readiness     | Primary Artifact    | Open Gaps / Blockers  |
+--------------------------------------+---------------+---------------------+-----------------------+
| 1. Shell & Window Architecture       | **READY**     | `NLE-SPC-SHL-001`   | Zero blockers.        |
| 2. Navigation & Layout Boundaries    | **READY**     | `NLE-CAT-NAV-001`   | Zero blockers.        |
| 3. Project Creation & Setup UX       | **READY**     | `NLE-SPC-WIZ-001`   | Zero blockers.        |
| 4. Workspace & Contextual Routes     | **READY**     | `NLE-CAT-WKS-001`   | Zero blockers.        |
| 5. Reusable Component Contracts (33) | **READY**     | `NLE-CAT-CMP-001`   | Zero blockers.        |
| 6. Validation Ownership & Invariants | **READY**     | `NLE-SPC-FVR-001`   | RSK-UI-001 (Crash gap)|
| 7. Error Presentation & Redaction    | **READY**     | `NLE-CAT-ERR-001`   | Zero blockers.        |
| 8. Recovery & Retry Semantics        | **READY**     | `NLE-SPC-REC-001`   | RSK-UI-002 (Idempot.) |
| 9. Security & Medium IL Boundaries   | **READY**     | `NLE-SPC-AVS-001`   | Zero blockers.        |
| 10. Accessibility Contracts (AA)     | **READY**     | `NLE-SPC-AVS-001`   | RSK-UI-005 (Live test)|
| 11. Verification & Test Plans        | **READY**     | `NLE-PLN-FVR-001`   | Implementation dep.   |
+--------------------------------------+---------------+---------------------+-----------------------+
```

---

## 5. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Quality Audit Team |
| Step | 24 — Phase 3 Desktop Interface & Workspace Management Readiness Review |
| Document ID | NLE-REG-RSK-001 |
| Created | 2026-11-17 |
| Classification | Formal Implementation Readiness & Risk Register |

