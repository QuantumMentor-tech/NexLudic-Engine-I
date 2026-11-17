# Step 31 Compatibility Rule Validation & Regression Test Plan

**Document ID:** `NLE-PLN-CRV-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED`  
**Workstream:** `Compatibility Rule Architecture, Rule Taxonomy, Domain Schema, Lifecycle Governance, and Evaluation Contracts`  
**Phase:** `4 of 12 — Game Discovery & Compatibility Intelligence`  
**Step:** `31 of 100`  
**Related Documents:**
- [COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md) (`NLE-SPC-CRD-001`)
- [EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md) (`NLE-SPC-EPE-001`)
- [COMPATIBILITY_RULE_DEPENDENCY_AND_CONFLICT_RESOLUTION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_RULE_DEPENDENCY_AND_CONFLICT_RESOLUTION_SPECIFICATION.md) (`NLE-SPC-RDC-001`)
- [COMPATIBILITY_EVALUATION_EXPLAINABILITY_AND_TRACE_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_EVALUATION_EXPLAINABILITY_AND_TRACE_SPECIFICATION.md) (`NLE-SPC-EET-001`)

---

## 1. Test Strategy & Architectural Mandates

This test plan defines the design-level validation specifications for the compatibility rule catalog, execution pipeline, conflict resolution logic, and explainability generators in **NexLudic Engine**.

### 1.1 Synthetic Fixture Strategy
In strict compliance with Step 31 governance:
- **Zero code execution:** Specifications represent test contracts to guide future Phase 5/8 implementations.
- **Pure synthetic data:** Tests operate on synthetic mock contexts (`MockGameContext`, `MockEnvironmentProfile`, `MockRequirementSet`, `MockRuleCatalog`).
- **Zero process execution:** No local game files or processes are inspected or launched during testing.

---

## 2. Validation Test Suite (35 Design-Level Test Specifications)

### 2.1 Group 1: Rule Definition & Schema Tests (TCRV-DEF)

| Test ID | Test Title | Objective | Input Fixture | Expected Outcome | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TCRV-DEF-001` | Valid Approved Rule Ingestion | Verify schema parsing of approved rule. | Valid `RULE-IDN-001` definition. | Ingested successfully into catalog. | All required fields valid. |
| `TCRV-DEF-002` | Draft Rule Execution Exclusion | Verify draft rule excluded from official run. | Rule in `DRAFT` lifecycle state. | Pipeline skips rule; logs exclusion. | Draft rule never fires. |
| `TCRV-DEF-003` | Missing Required Metadata Rejection | Verify rejection of rule lacking rationale. | Rule definition with `rationale = null`. | Validation fails with `ERR_MISSING_METADATA`.| Catalog rejection. |
| `TCRV-DEF-004` | Circular Dependency Cycle Rejection | Verify detection of $A \rightarrow B \rightarrow A$ cycle. | Rule A depends on B; Rule B depends on A. | Rejected with `ERR_CIRCULAR_RULE_DEPENDENCY`.| Topological sort fails cleanly. |
| `TCRV-DEF-005` | Deprecated Rule Exclusion | Verify deprecated rule excluded from selection. | Rule in `DEPRECATED` state. | Pipeline skips rule; logs deprecation note. | Deprecated rule inactive. |
| `TCRV-DEF-006` | Invalid Outcome Code Rejection | Verify rule output constrained to valid codes. | Rule returning arbitrary string `"OK"`. | Rejected with `ERR_INVALID_OUTCOME_CODE`. | Output constrained to enum. |
| `TCRV-DEF-007` | Mandatory Constraint Categorization | Verify `RULE-SEC-*` flagged as mandatory. | Rule in Security category. | `nature = MANDATORY_CONSTRAINT`. | Constraint flag verified. |

### 2.2 Group 2: Evaluation Pipeline Stage Tests (TCRV-STG)

| Test ID | Test Title | Objective | Input Fixture | Expected Outcome | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TCRV-STG-001` | Stage A Context Failure Short-Circuit | Verify unresolvable game identity halts pipe. | Game B canonical ID is unresolvable. | Aborts at Stage A; yields `INSUFFICIENT_EVIDENCE`. | Skips downstream stages. |
| `TCRV-STG-002` | Stage B Evidence Hash Validation | Verify payload hash mismatch discards evidence. | Evidence record with corrupt SHA-256. | Discarded at Stage B; marked `HASH_INVALID`. | Untrusted evidence pruned. |
| `TCRV-STG-003` | Stage C Strategy Filter | Verify Host Mod rules skipped on Bridge project. | Strategy selected is `LIVE_BRIDGE`. | Host Mod rules marked `NOT_APPLICABLE`. | Strategy isolation verified. |
| `TCRV-STG-004` | Stage D Hard Blocker Short-Circuit | Verify anti-cheat trips immediate short-circuit.| Participant has BattlEye anti-cheat. | Aborts at Stage D; status `PROHIBITED_BLOCKED`. | Skips Stages E, F, G. |
| `TCRV-STG-005` | Stage E Dependency Version Mismatch | Verify incompatible loader version detected. | SKSE 2.2.3 on game patch 1.6.1170. | Stage E outputs `DEPENDENCY_VERSION_MISMATCH`. | Actionable gap recorded. |
| `TCRV-STG-006` | Stage F Strategy Isolation | Verify Host Mod failure does not fail Bridge. | Host Mod fails bitness; Bridge succeeds. | Host Mod: `UNSUPPORTED`; Bridge: `SUPPORTED`. | Strategies decoupled. |
| `TCRV-STG-007` | Stage J Reassessment Registration | Verify timestamp hooks saved to Volume D. | Completed assessment. | Invalidation hooks saved to `latest_assessment.json`.| Volume D persistence valid. |

### 2.3 Group 3: Decision Integrity & Blockers Tests (TCRV-INT)

| Test ID | Test Title | Objective | Input Fixture | Expected Outcome | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TCRV-INT-001` | Blocker Overrides Positive Scores | Verify 99% positive rules cannot bypass blocker. | 10 capabilities pass; anti-cheat tripped. | Status: `PROHIBITED_BLOCKED` (Gate 0). | Blocker dominates. |
| `TCRV-INT-002` | Mandatory Requirement Failure | Verify 1 missing mandatory requirement blocks. | 4 mandatory met; 1 mandatory blocked. | Status: `UNSUPPORTED` (Gate 0). | No partial promotion. |
| `TCRV-INT-003` | Optional Requirement Gap Permitted | Verify missing optional yields partial. | 4 mandatory met; 1 optional unassessed. | Status: `PARTIALLY_SUPPORTED` (Gate 2). | Partial support awarded. |
| `TCRV-INT-004` | Unknown Version Non-Incompatibility | Verify unknown build yields exploratory gate. | Build version is unknown. | Status: `INSUFFICIENT_EVIDENCE` (Gate 1). | Not marked broken. |
| `TCRV-INT-005` | Hybrid Dual Prerequisite Failure | Verify Hybrid fails if one component fails. | Bridge valid; Host asset conversion fails. | Hybrid status: `UNSATISFIED`. | Conjunction enforced. |
| `TCRV-INT-006` | Anti-Aggregation Invariant | Verify percentage scoring is rejected. | Pipeline synthesis requested. | Generates discrete findings; zero percentages. | No averaging formulas. |
| `TCRV-INT-007` | Determinism Repetition Assertion | Verify 100 identical runs yield identical trace. | Fixed synthetic fixture run 100 times. | Exact identical outputs; zero variance. | 100% determinism. |

### 2.4 Group 4: Explainability & Trace Tests (TCRV-EXP)

| Test ID | Test Title | Objective | Input Fixture | Expected Outcome | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TCRV-EXP-001` | Trace Schema Conformity | Verify trace JSON conforms to schema. | Completed evaluation trace. | Validates against `CompatibilityEvaluationTrace`. | Syntax passes. |
| `TCRV-EXP-002` | User Headline Template Match | Verify headline matches status. | Status is `SUPPORTED`. | Headline: *"Full Integration Supported via..."*. | Template matches. |
| `TCRV-EXP-003` | Actionable Next Step Rendering | Verify next step advises concrete action. | Status is `INSUFFICIENT_EVIDENCE`. | Advises: *"Locate game folder in Environment..."*. | Actionable text. |
| `TCRV-EXP-004` | Counterfactual Diff Explanation | Verify "What Changed" diff explanation. | Build updated from 1.6.640 to 1.6.1170. | Cites exact build diff and incompatible loader. | Factual attribution. |
| `TCRV-EXP-005` | Critical Blocker Disclosure Clarity | Verify anti-cheat explanation names tool. | BattlEye anti-cheat blocker. | Explicitly states: *"Destiny 2 employs BattlEye..."*. | Transparent warning. |
| `TCRV-EXP-006` | Host Path Redaction in Traces | Verify raw drive paths scrubbed. | Input path: `D:\Users\Alice\Games\...`. | Output trace renders `<INSTALL_ROOT>\...`. | Path privacy preserved. |
| `TCRV-EXP-007` | Capability Breakdown Traceability | Verify every capability cites evidence. | Evaluated capability checklist. | Every item contains valid `evidence_citations`. | Full traceability. |

### 2.5 Group 5: Versioning, Rollback & Reassessment Tests (TCRV-VER)

| Test ID | Test Title | Objective | Input Fixture | Expected Outcome | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TCRV-VER-001` | Major Rule Bump Invalidates Projects | Verify rule v2.0.0 flags prior assessments. | Rule catalog bumped from 1.0 to 2.0. | Prior assessment marked `STALE_REASSESSMENT`. | Reassessment triggered. |
| `TCRV-VER-002` | Patch Rule Bump Preserves Assessment | Verify rule v1.0.1 does not invalidate projects. | Rule catalog bumped from 1.0.0 to 1.0.1. | Prior assessment remains `CURRENT`. | Backward compatible. |
| `TCRV-VER-003` | Quarantined Rollback Fallback | Verify engine falls back to v(n-1) on bug. | Active rule quarantined. | Engine evaluates using previous approved rev. | Safe degradation. |
| `TCRV-VER-004` | Historical Rule Revision Audit | Verify historical rule revision readable. | Audit query for rule revision 1. | Fetches archived revision from Volume D. | History preserved. |
| `TCRV-VER-005` | Unrelated Metadata Change Isolation | Verify unrelated game update ignores pair. | Game C updated; project is Game A + B. | Project A+B assessment untouched. | Scope isolated. |
| `TCRV-VER-006` | Local File Patch Triggers Stale | Verify game binary patch flags assessment. | Game A executable timestamp changed. | Assessment state transitions to `STALE_CHANGED`.| Reassessment flagged. |
| `TCRV-VER-007` | Rollback Preserves User Settings | Verify rule rollback does not wipe project. | Rule rolled back. | Project workspace configurations 100% intact. | Non-destructive. |

---

## 3. Document Sign-Off & Verification

| Role | Reviewer | Verdict | Date |
| :--- | :--- | :--- | :--- |
| **Principal Software Architect** | Engine Architecture Team | Approved | 2026-11-17 |
| **Lead QA Automation Architect** | Test & Quality Assurance | Approved | 2026-11-17 |
| **Senior Security & Compliance Officer**| Security Governance | Approved | 2026-11-17 |

