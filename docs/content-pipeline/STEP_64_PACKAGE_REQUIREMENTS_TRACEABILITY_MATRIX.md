# Step 64 Package Requirements Traceability Matrix

**Document Identifier:** `NLE-MAT-STP-064`  
**Classification:** Requirements Engineering & Traceability Matrix  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-MAT-STP-063`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Traceability Methodology

This matrix establishes bidirectional traceability between foundational architectural principles, upstream phase decisions, the 32 material Step 64 requirements (`PVA-REQ-001` through `PVA-REQ-032`), the packaging contracts, and the 34 synthetic test scenarios (Scenarios A through AH).

---

## 2. Requirements Traceability Table (`PVA-REQ-001` to `PVA-REQ-032`)

| Req ID | Requirement Summary | Authoritative Document | Affected Domain Entity | Security / Governance Rule | Related Synthetic Scenario | Coverage Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `PVA-REQ-001` | Offline Logical Assembly Planning | `NLE-SPC-PVA-001` Sec 3 | `PackageAssemblyPlan` | Invariant `INV-PVA-01` | Scenario A, AD | `COVERED_IN_SPEC` |
| `PVA-REQ-002` | Deterministic Conflict Resolution | `NLE-SPC-PVA-001` Sec 3 | Resolver Engine | Invariant `INV-PVA-02` (No last-writer-wins) | Scenario Q, R | `COVERED_IN_SPEC` |
| `PVA-REQ-003` | Complete Manifest & Cryptographic Coverage| `NLE-SPC-PVA-001` Sec 3 | `manifest.json` | Invariant `INV-PVA-03` | Scenario V, Y | `COVERED_IN_SPEC` |
| `PVA-REQ-004` | Strict Application Boundary Invariant | `NLE-SPC-PVA-001` Sec 3 | Handoff Gate | Invariant `INV-PVA-04` (No implicit apply) | Scenario AH | `COVERED_IN_SPEC` |
| `PVA-REQ-005` | Layer A Identity & Digest Verification | `NLE-CON-AEF-001` Sec 1 | Layer A Validator | SHA-256 recomputation | Scenario A, C | `COVERED_IN_SPEC` |
| `PVA-REQ-006` | Layer B Format & Container Structure | `NLE-CON-AEF-001` Sec 1 | Layer B Validator | Native adapter binary parse | Scenario B, F | `COVERED_IN_SPEC` |
| `PVA-REQ-007` | Layer C Semantic & Target Build Match | `NLE-CON-AEF-001` Sec 1 | Layer C Validator | Metric bounds tolerance ($\pm 0.001$) | Scenario F, S | `COVERED_IN_SPEC` |
| `PVA-REQ-008` | Layer D Policy & Gate G5 Packaging Auth | `NLE-CON-AEF-001` Sec 1 | Layer D Validator | Explicit Gate G5 token check | Scenario E, AC | `COVERED_IN_SPEC` |
| `PVA-REQ-009` | Layer E Evidence Freshness & Invalidation| `NLE-CON-AEF-001` Sec 1 | Layer E Validator | Invalidate on source/def change | Scenario D, U | `COVERED_IN_SPEC` |
| `PVA-REQ-010` | Nine Dependency Categories & Schema | `NLE-POL-PDG-001` Sec 2 | `PackageDependencyRecord`| Typed dependency classification | Scenario G, L | `COVERED_IN_SPEC` |
| `PVA-REQ-011` | SemVer 2.0 Version Range Evaluation | `NLE-POL-PDG-001` Sec 3 | Version Resolver | Strict range bounding | Scenario G, I | `COVERED_IN_SPEC` |
| `PVA-REQ-012` | Deterministic Candidate Tie-Breaking | `NLE-POL-PDG-001` Sec 3 | Candidate Ranker | Highest SemVer, earliest revision | Scenario J, K | `COVERED_IN_SPEC` |
| `PVA-REQ-013` | Diamond Dependency Resolution | `NLE-POL-PDG-001` Sec 3 | Graph Resolver | Unify intersection bounds | Scenario G | `COVERED_IN_SPEC` |
| `PVA-REQ-014` | Dependency DAG & Cycle Detection | `NLE-POL-PDG-001` Sec 4 | Cycle Detector | Three-color DFS (`ALG-DAG-CYCLE`)| Scenario M | `COVERED_IN_SPEC` |
| `PVA-REQ-015` | Self-Dependency Detection & Rejection | `NLE-POL-PDG-001` Sec 4 | Graph Validator | Reject reflexive self-dependency | Scenario N | `COVERED_IN_SPEC` |
| `PVA-REQ-016` | Destination Path Collision Detection | `NLE-POL-PDG-001` Sec 5 | Collision Engine | Unique `target_relative_path` | Scenario Q | `COVERED_IN_SPEC` |
| `PVA-REQ-017` | Cross-Artifact Target Release Homogeneity| `NLE-POL-PMC-001` Sec 2 | Target Validator | Disallow mixed target releases | Scenario S | `COVERED_IN_SPEC` |
| `PVA-REQ-018` | Transitive Licensing Compatibility Check | `NLE-POL-PMC-001` Sec 2 | License Validator | Disallow GPL/Proprietary clash | Scenario O | `COVERED_IN_SPEC` |
| `PVA-REQ-019` | Required vs Optional Member Exclusions | `NLE-POL-PMC-001` Sec 3 | Member Controller | Required members can never omit | Scenario L, T | `COVERED_IN_SPEC` |
| `PVA-REQ-020` | Package Completeness Verification | `NLE-POL-PMC-001` Sec 4 | Completeness Validator | Zero dangling material/mesh refs | Scenario H, V | `COVERED_IN_SPEC` |
| `PVA-REQ-021` | Package Manifest Schema & Versioning | `NLE-POL-PMS-001` Sec 2 | `manifest.json` | Explicit versioning contracts | Scenario V, W | `COVERED_IN_SPEC` |
| `PVA-REQ-022` | Manifest Forward Compatibility & Unknown | `NLE-POL-PMS-001` Sec 3 | Manifest Parser | Fail closed on unknown mandatory | Scenario W | `COVERED_IN_SPEC` |
| `PVA-REQ-023` | Fifteen-Step Deterministic Assembly | `NLE-CON-PAD-001` Sec 2 | Assembly Sequencer | Auditable 15-step pipeline | Scenario A, AD | `COVERED_IN_SPEC` |
| `PVA-REQ-024` | Canonical Manifest Serialization | `NLE-CON-PAD-001` Sec 3 | JSON Serializer | Sorted keys, LF newlines | Scenario X, Y | `COVERED_IN_SPEC` |
| `PVA-REQ-025` | Timestamp Exclusion from Integrity Hash | `NLE-CON-PAD-001` Sec 3 | Digest Engine | Exclude `generated_at` from hash | Scenario Y | `COVERED_IN_SPEC` |
| `PVA-REQ-026` | Path Sanitization & Virtual Staging | `NLE-POL-PST-001` Sec 2 | Path Sanitizer | Disallow `..`, absolute, devices | Scenario Z | `COVERED_IN_SPEC` |
| `PVA-REQ-027` | Six Granular Packaging Authorization | `NLE-POL-PST-001` Sec 3 | Policy Gate | Separate package from install/pub| Scenario E, AF | `COVERED_IN_SPEC` |
| `PVA-REQ-028` | AI Coding-Agent Guardrails (Untrusted) | `NLE-POL-PST-001` Sec 4 | Guardrail Engine | AI suggestions require validation| Scenario AG | `COVERED_IN_SPEC` |
| `PVA-REQ-029` | 34-Code Package Validation Failure Taxon | `NLE-POL-PVF-001` Sec 2 | Diagnostic Engine | Comprehensive error taxonomy | Scenario B, AB | `COVERED_IN_SPEC` |
| `PVA-REQ-030` | Transient Safe Retries & Resource Ceils | `NLE-POL-PVF-001` Sec 3 | Worker Supervisor | 4 GB RAM, 10 GB container limit | Scenario AA, AB | `COVERED_IN_SPEC` |
| `PVA-REQ-031` | Eight Readiness Classification States | `NLE-CON-PRH-001` Sec 2 | State Machine | Strict progression gates | Scenario AH | `COVERED_IN_SPEC` |
| `PVA-REQ-032` | Downstream Stage M Package Handoff Dossier| `NLE-CON-PRH-001` Sec 3 | Handoff Controller | Formal dossier to Stage M | Scenario A, AH | `COVERED_IN_SPEC` |

---

## 3. Decision & Governance Summary

All 32 material requirements are mapped to authoritative specification sections, explicit security rules, and synthetic validation scenarios. No unverified assumptions or placeholder claims are permitted.
