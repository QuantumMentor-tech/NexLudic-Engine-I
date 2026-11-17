# Phase 4 Master Requirements Traceability Matrix

**Document ID:** `NLE-MAT-PH4-001`  
**Version:** 1.0.0  
**Phase:** 4 of 12 — Game Discovery & Compatibility Intelligence  
**Step:** 33 of 100 — Phase 4 Game Discovery & Compatibility Intelligence Readiness Review  
**Classification:** Phase-Gate Deliverable / Bidirectional Requirements Traceability Matrix  
**Status:** Approved & Formally Sealed  
**Authoritative Reference:** Phase 1 Baselines, Phase 2 ADRs (`DEC-002`, `DEC-009`), Phase 3 UI Contracts, Steps 25–32 Deliverables

---

## 1. Traceability Architecture & Coverage Standard

This Master Requirements Traceability Matrix establishes complete bidirectional verification between the foundational requirements defined in Phase 1 & 2, the Phase 4 domain specifications (Steps 25–32), their formal acceptance criteria, and their corresponding validation test plans.

### 1.1 Traceability Coverage Summary
- **Total Master Requirements Evaluated:** 52 Requirements across 9 Domains.
- **Forward Traceability:** 100% of requirements map to an authoritative specification, acceptance criterion, and QA test scenario.
- **Backward Traceability:** 100% of domain schemas, decision tables, rule taxonomies, and report widgets trace back to an authorized architectural requirement.
- **Traceability Status:** **100% FULLY TRACED**. Zero unmapped requirements; zero orphan specifications.

---

## 2. Master Requirements Traceability Matrix

### 2.1 Domain 1: Foundational Architectural & Security Constraints

| Req ID | Requirement Statement | Source Phase / ADR | Implementing Step 25–32 Deliverables | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-SEC-001` | Sandboxing: Windows Medium Integrity Level; zero UAC elevation; local path redaction. | Phase 2 Baseline / `ADR-003` | `NLE-SPC-DAP-001`, `NLE-PLN-EPF-001`, `NLE-PLN-ASP-001` | AC-SEC-01 (No elevation) | `TC-REP-SEC-01`, `TC-REP-SEC-04` | FULLY TRACED |
| `REQ-SAFE-001`| Safety: Strictly read-only; zero DLL injection; zero memory hooking (`OpenProcess` banned). | Phase 2 Baseline / `ADR-003` | `NLE-SPC-LID-001`, `NLE-SPC-CAC-001`, `NLE-SPC-CRD-001` | AC-SAF-01 (Zero process hooks)| `TC-REP-BASE-05`, `TC-REP-SEC-05` | FULLY TRACED |
| `DEC-002` | Modular Boundaries: Universal Catalog decoupled from Game-Specific Adapters. | Phase 2 ADR / `DEC-002` | `NLE-SPC-GDC-001`, `NLE-SPC-RUX-001` | AC-MOD-01 (Loose coupling) | `TC-REP-BASE-01`, `TC-MAT-STP-031-01`| FULLY TRACED |
| `DEC-009` | Storage Isolation: Project storage scoped strictly to Volume D (`D:\NexLudic\Projects\<id>\`). | Phase 2 ADR / `DEC-009` | `NLE-CAT-RIC-001`, `NLE-POL-RVG-001` | AC-STR-01 (Volume D scoping) | `TC-REP-SEC-01`, `TC-EVD-STOR-01` | FULLY TRACED |

---

### 2.2 Domain 2: Game Discovery & Canonical Identity (Step 25)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-IDN-01` | Unambiguous Canonical Game Identity separate from marketing titles. | Step 25 | `NLE-SPC-GDC-001`, `NLE-MOD-GCD-001` | AC-25-01 (Canonical entity schema) | `TC-GDC-001` to `005` | FULLY TRACED |
| `REQ-IDN-02` | Strict separation of Game, Edition, Release, Build, and Installation instances. | Step 25 | `NLE-MOD-GCD-001`, `NLE-RUL-CGV-001` | AC-25-02 (Hierarchy isolation) | `TC-GDC-006` to `010` | FULLY TRACED |
| `REQ-IDN-03` | Unified Game Metadata Dictionary containing technical modding fields. | Step 25 | `NLE-DIC-GMF-001` | AC-25-03 (58 field dictionary) | `TC-GDC-011` to `015` | FULLY TRACED |
| `REQ-IDN-04` | Multi-Source Metadata Provider Ingestion Architecture. | Step 25 | `NLE-ARC-GSP-001` | AC-25-04 (Multi-source contracts) | `TC-GDC-016` to `020` | FULLY TRACED |
| `REQ-IDN-05` | Deterministic Identity Resolution & Duplicate Handling. | Step 25 | `NLE-RUL-IDR-001` | AC-25-05 (String normalization) | `TC-GDC-021` to `025` | FULLY TRACED |

---

### 2.3 Domain 3: Metadata Sources & Provider Authority (Step 26)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-MET-01` | Comprehensive Provider Evaluation (IGDB, PCGamingWiki, Steam, Mobygames). | Step 26 | `NLE-RPT-MET-001`, `NLE-MAT-PRV-001` | AC-26-01 (Provider scorecard) | `TC-PAT-001` to `005` | FULLY TRACED |
| `REQ-MET-02` | Field-Level Source-of-Truth Authority Matrix. | Step 26 | `NLE-MAT-FSA-001` | AC-26-02 (Definitive field rank) | `TC-PAT-006` to `010` | FULLY TRACED |
| `REQ-MET-03` | Provider API Access Contracts & Caching Policies. | Step 26 | `NLE-SPC-API-001` | AC-26-03 (Rate-limit contracts) | `TC-PAT-011` to `014` | FULLY TRACED |
| `REQ-MET-04` | Licensing & Data Governance Review (CC-BY-SA compliance). | Step 26 | `NLE-REV-LIC-001` | AC-26-04 (Attribution compliance) | `TC-PAT-015` to `017` | FULLY TRACED |
| `REQ-MET-05` | Provider Normalization & Offline Degradation Handling. | Step 26 | `NLE-SPC-PNF-001` | AC-26-05 (Graceful degradation) | `TC-PAT-018` to `020` | FULLY TRACED |

---

### 2.4 Domain 4: Local Game Installation Discovery (Step 27)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-LOC-01` | Sandboxed, Consent-Based Local Installation Scanning. | Step 27 | `NLE-SPC-LID-001`, `NLE-SPC-DAP-001` | AC-27-01 (Explicit user consent) | `TC-DSV-001` to `005` | FULLY TRACED |
| `REQ-LOC-02` | Detection Source Evaluation (Steam, GOG, Epic, Custom Paths). | Step 27 | `NLE-CAT-LDS-001` | AC-27-02 (Manifest parsing) | `TC-DSV-006` to `010` | FULLY TRACED |
| `REQ-LOC-03` | 13-State Installation Lifecycle Model (`DETECTED` to `VERIFIED`). | Step 27 | `NLE-MOD-IVS-001` | AC-27-03 (13 discrete states) | `TC-DSV-011` to `015` | FULLY TRACED |
| `REQ-LOC-04` | PE Header & Version Verification without Execution. | Step 27 | `NLE-RUL-VIM-001` | AC-27-04 (Binary header hashing) | `TC-DSV-016` to `020` | FULLY TRACED |
| `REQ-LOC-05` | Discovery Failure, Unreadable Drives & Path Relocation Recovery. | Step 27 | `NLE-SPC-DFR-001` | AC-27-05 (Fault tolerance) | `TC-DSV-021` to `025` | FULLY TRACED |

---

### 2.5 Domain 5: Version, Edition & Environment Mapping (Step 28)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-VER-01` | Formal Version & Edition Relationship Mapping. | Step 28 | `NLE-SPC-VEC-001`, `NLE-CAT-VER-001` | AC-28-01 (Patch level hierarchy) | `TC-VEM-001` to `005` | FULLY TRACED |
| `REQ-VER-02` | Environment Profile Schema (OS, GPU, DirectX, Runtimes). | Step 28 | `NLE-MOD-GEP-001` | AC-28-02 (Profile attributes) | `TC-VEM-006` to `010` | FULLY TRACED |
| `REQ-VER-03` | Scoping Evidence strictly to Evaluated Build Ranges. | Step 28 | `NLE-RUL-VCE-001` | AC-28-03 (Version applicability) | `TC-VEM-011` to `015` | FULLY TRACED |
| `REQ-VER-04` | Environment Change Detection & Assessment Invalidation Policy. | Step 28 | `NLE-POL-ECS-001` | AC-28-04 (Staleness triggers) | `TC-VEM-016` to `020` | FULLY TRACED |
| `REQ-VER-05` | Game-Pair Dependency Intersection & Conflict Detection. | Step 28 | `NLE-SPC-GED-001` | AC-28-05 (Dependency mapping) | `TC-VEM-021` to `025` | FULLY TRACED |

---

### 2.6 Domain 6: Cross-Game Compatibility Assessment Framework (Step 29)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-ASS-01` | Multi-Strategy Assessment (Strategy A: Mod, B: Bridge, C: Hybrid). | Step 29 | `NLE-SPC-CAC-001`, `NLE-RUL-ISF-001` | AC-29-01 (Equal 3-way evaluation) | `TC-CST-001` to `005` | FULLY TRACED |
| `REQ-ASS-02` | 8-Dimension Compatibility Taxonomy & Anti-Aggregation Invariant. | Step 29 | `NLE-CAT-DRC-001` | AC-29-02 (Anti-averaging rule) | `TC-CST-006` to `010` | FULLY TRACED |
| `REQ-ASS-03` | Mandatory Blocker Dominance (Kernel Anti-Cheat `BLK-SEC-01`, DRM `BLK-SEC-02`). | Step 29 | `NLE-SPC-CAC-001`, `NLE-TAB-CDT-001` | AC-29-03 (Blocker short-circuit) | `TC-CST-011` to `015` | FULLY TRACED |
| `REQ-ASS-04` | Epistemic Quality & Conflict Resolution Scoring. | Step 29 | `NLE-RUL-EQC-001` | AC-29-04 (Confidence calculation)| `TC-CST-016` to `020` | FULLY TRACED |
| `REQ-ASS-05` | Assessment Output Schema & Revision Invalidation Lifecycle. | Step 29 | `NLE-SPC-AOR-001` | AC-29-05 (Revision tracking) | `TC-CST-021` to `030` | FULLY TRACED |

---

### 2.7 Domain 7: Evidence Collection, Verification & Provenance (Step 30)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-EVD-01` | Cryptographic Evidence Record Schema (SHA-256 Content Hashing). | Step 30 | `NLE-SPC-EPS-001` | AC-30-01 (Cryptographic schema) | `TC-EPF-001` to `005` | FULLY TRACED |
| `REQ-EVD-02` | 4-Tier Epistemic Hierarchy (`REPRODUCED` to `INDIRECT`). | Step 30 | `NLE-SPC-ECL-001`, `NLE-RUL-ECV-001` | AC-30-02 (Maturity ranking) | `TC-EPF-006` to `010` | FULLY TRACED |
| `REQ-EVD-03` | Deterministic Conflict Resolution & Duplicate Elimination. | Step 30 | `NLE-WRK-QCD-001` | AC-30-03 (Perceptual hashing) | `TC-EPF-011` to `015` | FULLY TRACED |
| `REQ-EVD-04` | Freshness Monitoring & 90-Day Evidence Expiration Policies. | Step 30 | `NLE-POL-FCD-001` | AC-30-04 (Expiration watermarking)| `TC-EPF-016` to `020` | FULLY TRACED |
| `REQ-EVD-05` | Untrusted Evidence Content Sandboxing & AST Parsing. | Step 30 | `NLE-PLN-EPF-001` | AC-30-05 (Script stripping) | `TC-EPF-021` to `030` | FULLY TRACED |

---

### 2.8 Domain 8: Compatibility Rules & Explainable Pipeline (Step 31)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-RUL-01` | 9-Category Rule Taxonomy & Input Applicability Contracts. | Step 31 | `NLE-SPC-CRD-001`, `NLE-CAT-RIA-001` | AC-31-01 (Taxonomy schema) | `TC-CRV-001` to `005` | FULLY TRACED |
| `REQ-RUL-02` | 10-Stage Sequential Evaluation Pipeline (Stage A–J). | Step 31 | `NLE-SPC-EPE-001` | AC-31-02 (Sequential execution) | `TC-CRV-006` to `012` | FULLY TRACED |
| `REQ-RUL-03` | DAG Dependency Cycle Prevention via Kahn's Algorithm. | Step 31 | `NLE-SPC-RDC-001` | AC-31-03 (Static cycle detection)| `TC-CRV-013` to `018` | FULLY TRACED |
| `REQ-RUL-04` | Strategy-Specific Rule Specifications (Bitness, IPC, Headroom). | Step 31 | `NLE-SPC-SRC-001` | AC-31-04 (Deterministic rules) | `TC-CRV-019` to `024` | FULLY TRACED |
| `REQ-RUL-05` | Explainable Decision Trace Schema & Counterfactual Diff Engine. | Step 31 | `NLE-SPC-EET-001` | AC-31-05 (Human-readable trace) | `TC-CRV-025` to `030` | FULLY TRACED |
| `REQ-RUL-06` | Rule Versioning Governance & Rollback Quarantine Protocol. | Step 31 | `NLE-POL-RVG-001` | AC-31-06 (Rollback protocol) | `TC-CRV-031` to `035` | FULLY TRACED |

---

### 2.9 Domain 9: Game-Pair Compatibility Report UX (Step 32)

| Req ID | Requirement Statement | Originating Step | Authoritative Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-REP-01` | 7-Region Report Layout & Progressive Disclosure Model. | Step 32 | `NLE-SPC-RUX-001`, `NLE-CAT-RIC-001` | AC-32-01 (7 layout regions) | `TC-REP-BASE-01` to `04` | FULLY TRACED |
| `REQ-REP-02` | Non-Color Status Semantics & Anti-Deception Wording Dictionary. | Step 32 | `NLE-GDE-SVL-001` | AC-32-02 (WCAG non-color) | `TC-REP-STAT-01` to `05` | FULLY TRACED |
| `REQ-REP-03` | 3-Way Strategy Comparison Cards & Justified Recommendations. | Step 32 | `NLE-SPC-SCR-001` | AC-32-03 (No forced winners) | `TC-REP-STAT-04`, `TC-REP-INT-05`| FULLY TRACED |
| `REQ-REP-04` | Forensic Evidence Drill-Down Drawer (`EvidenceDrilldownDrawer`). | Step 32 | `NLE-SPC-EDT-001` | AC-32-04 (Bidirectional trace) | `TC-REP-EVD-01` to `05` | FULLY TRACED |
| `REQ-REP-05` | Complete Interaction Lifecycle (Empty, In-Progress, Stale, Errors). | Step 32 | `NLE-SPC-RIS-001` | AC-32-05 (State machine completeness)| `TC-REP-INT-01` to `04` | FULLY TRACED |
| `REQ-REP-06` | WCAG 2.1 AA Accessibility, Medium Integrity Security & SLAs. | Step 32 | `NLE-PLN-ASP-001`, `NLE-SPC-WSR-001` | AC-32-06 (Keyboard & SLA budgets)| `TC-REP-A11Y-01` to `PERF-05` | FULLY TRACED |

---

## 3. Backward Traceability Verification

All high-impact implementation concepts specified in Phase 4 were audited for backward legitimacy:
1. **`RuleSetVersion` & Kahn's DAG:** Traces directly to `REQ-RUL-03` and `Phase 2 High-Level Architecture`.
2. **`EvidenceDrilldownDrawer`:** Traces to `REQ-REP-04`, `REQ-EVD-01`, and `CMP-018 ContextualSideSheet`.
3. **`PE Header Hashing`:** Traces to `REQ-LOC-04` and `REQ-SAFE-001` (read-only binary inspection).
4. **`Anti-Aggregation Invariant`:** Traces to `REQ-ASS-02` and `Phase 1 Feasibility Baseline`.

Zero orphan specifications or ungrounded architectural inventions were detected.

