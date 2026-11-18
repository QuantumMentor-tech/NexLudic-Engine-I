# Step 35 Capability Requirements Traceability Matrix

**Document ID:** `NLE-MAT-STP-035`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 35 of 100 — Canonical Capability Taxonomy & Declaration Schema Specification  
**Classification:** Requirements Traceability Matrix & QA Test Plan  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-GAT-PH4-001`, `REQ-SEC-001`, `REQ-SAFE-001`, `DEC-002`, `DEC-009`, `NLE-MOD-CCT-001` through `NLE-PLN-CSV-001`

---

## 1. Upstream Requirements & Architecture Traceability

This matrix links foundational architectural mandates, Phase 4 gate conditions, and Step 34/35 capability specifications.

| Requirement ID | Architectural Mandate | Source Phase / ADR | Implementing Step 35 Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-SEC-001` | Sandboxing: Windows Medium Integrity Level; zero UAC elevation; local path redaction. | Phase 2 Baseline / `ADR-003` | `NLE-SCH-CCD-001` Section 3.4; `NLE-POL-CCG-001` Section 4 | AC-35-SEC-01 (Elevation rejected) | `TC-CAP-SEC-01`, `TC-CAP-SEC-03` | FULLY TRACED |
| `REQ-SAFE-001`| Safety: Strictly read-only; zero DLL injection; zero memory hooking (`OpenProcess` banned). | Phase 2 Baseline / `ADR-003` | `NLE-MOD-CCT-001` Section 4; `NLE-SCH-CCD-001` Section 4 | AC-35-SAF-01 (Side-effect guarding)| `TC-CAP-SEC-02`, `TC-CAP-SEC-05` | FULLY TRACED |
| `DEC-002` | Modular Boundaries: Universal Catalog decoupled from Game-Specific Adapters. | Phase 2 ADR / `DEC-002` | `NLE-MOD-CCT-001` Section 2; `NLE-POL-CCG-001` Section 3 | AC-35-MOD-01 (Catalog decoupling) | `TC-CAP-DEF-01`, `TC-CAP-DEC-01` | FULLY TRACED |
| `DEC-009` | Storage Isolation: Project storage scoped strictly to Volume D (`D:\NexLudic\Projects\<id>\`). | Phase 2 ADR / `DEC-009` | `NLE-SCH-CCD-001` Section 4; `NLE-SCH-ACD-001` Section 3 | AC-35-STR-01 (Volume D scoping) | `TC-CAP-SEC-03`, `TC-CAP-STA-04` | FULLY TRACED |
| `GAT-PH4-C01` | Condition C-01: Adapter Schema Alignment (`RuleInputDomain`, `CapabilityRequirement`). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-MOD-CCT-001` Section 3; `NLE-SCH-CCD-001` Section 2 | AC-35-CAP-01 (8-Dimension mapping) | `TC-CAP-DEF-03`, `TC-CAP-STA-01` | FULLY TRACED |
| `GAT-PH4-C02` | Condition C-02: Safety & Integrity Enforcement (Medium IL, zero injection, Volume D). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-SCH-CCD-001` Section 3.4; `NLE-MOD-CDV-001` Section 4 | AC-35-SEC-02 (Medium IL sandboxing)| `TC-CAP-SEC-01`, `TC-CAP-SEC-02` | FULLY TRACED |
| `GAT-PH4-C03` | Condition C-03: Advisory Boundary Maintenance (`CMP-045` Approval Required). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-MOD-CDV-001` Section 3, Section 4 | AC-35-ADV-01 (Human confirmation) | `TC-CAP-STA-04`, `TC-CAP-SEC-05` | FULLY TRACED |
| `STEP-34-SDK` | Reconcile SDK contracts, manifest schema, and 12-outcome invocation taxonomy. | Step 34 Specification / `NLE-SPC-SDK-001`| `NLE-SCH-ACD-001`, `NLE-MOD-CDV-001` | AC-35-SDK-01 (Manifest alignment) | `TC-CAP-DEC-01`, `TC-CAP-DEC-05` | FULLY TRACED |

---

## 2. Backward Traceability Verification

All high-impact capability concepts specified in Step 35 were audited for backward legitimacy:
1. **The 10 Taxonomy Categories:** Trace directly to Candidate Categories in Step 35 Prompt and map to the 8 Assessment Dimensions (`NLE-CAT-DRC-001`).
2. **Canonical Capability Schema (`NLE-SCH-CCD-001`):** Extends `NLE-DOM-CAP-001` (Step 11) with Phase 4's operational semantics and 12-outcome invocation taxonomy.
3. **Adapter Capability Declaration Schema (`NLE-SCH-ACD-001`):** Directly implements the `capabilitiesDeclared[]` schema established in `NLE-CAT-ACM-001` (Step 34).
4. **The 7 Verification States (`NLE-MOD-CDV-001`):** Formally operationalizes the 5 epistemic levels defined in `NLE-SPC-SCI-001`.
5. **DAG Cycle Rejection via Kahn's Algorithm:** Traces to `NLE-SPC-RDC-001` (Step 31) cycle prevention standards.

Zero orphan specifications or ungrounded architectural inventions were detected.

---

## 3. Non-Blocking Gap Register

| Gap ID | Identified Condition | Mitigating Architectural Pattern | Follow-Up Step |
|:---|:---|:---|:---|
| `GAP-STP35-01` | Runtime capability negotiation algorithms are scheduled for Step 36. | Step 35 provides the formal schemas and DAG contracts required for negotiation. | Step 36 (Capability Negotiation) |
| `GAP-STP35-02` | Automated JSON Schema CLI validation tooling will be authored in Step 39. | Step 35 provides the complete Draft-07 JSON Schemas for validation tooling. | Step 39 (Conformance Tooling) |
| `GAP-STP35-03` | Cross-adapter shared memory ring buffer schemas will be finalized in Phase 6. | Step 35 models interop capabilities abstractly under `CAT-INTEROP`. | Step 41 (Universal IPC Broker) |

