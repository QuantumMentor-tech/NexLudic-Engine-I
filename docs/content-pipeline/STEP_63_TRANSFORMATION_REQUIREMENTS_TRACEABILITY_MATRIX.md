# Step 63 Transformation Requirements Traceability Matrix

**Document Identifier:** `NLE-MAT-STP-063`  
**Classification:** Requirements Engineering & Traceability Matrix  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-MAT-STP-062`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Traceability Methodology

This matrix establishes bidirectional traceability between foundational architectural principles, upstream phase decisions, the 32 material Step 63 requirements (`TRF-REQ-001` through `TRF-REQ-032`), the transformation contracts, and the 26 synthetic test scenarios (Scenarios A through Z).

---

## 2. Requirements Traceability Table (`TRF-REQ-001` to `TRF-REQ-032`)

| Req ID | Requirement Summary | Authoritative Document | Affected Domain Entity | Security / Governance Rule | Related Synthetic Scenario | Coverage Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `TRF-REQ-001` | Transformation Definition Schema & Versioning | `NLE-MOD-TRF-001` Sec 2 | `TransformationDefinition` | Strict versioning, approved state | Scenario A, L | `COVERED_IN_SPEC` |
| `TRF-REQ-002` | Transformation Request Contract & Task Binding| `NLE-MOD-TRF-001` Sec 3 | `TransformationRequest` | Bounded execution scope | Scenario A, Z | `COVERED_IN_SPEC` |
| `TRF-REQ-003` | Job Lifecycle & Attempt State Machine | `NLE-MOD-TRF-001` Sec 5 | `TransformationJob` | Strict state transitions | Scenario Y, Z | `COVERED_IN_SPEC` |
| `TRF-REQ-004` | Intermediate Representation (IR) Schemas | `NLE-SPC-CNI-001` Sec 1 | Canonical IR (glTF, EXR)| Engine-neutral normalization | Scenario J | `COVERED_IN_SPEC` |
| `TRF-REQ-005` | Versioned Semantic Mapping Specifications | `NLE-POL-SML-001` Sec 2 | `MappingSpecification` | Explicit property mapping | Scenario D, F | `COVERED_IN_SPEC` |
| `TRF-REQ-006` | Output Artifact Provenance & Integrity Model | `NLE-MOD-TRF-001` Sec 4 | `TransformationOutputArtifact`| SHA-256 and byte-size tracking | Scenario A, N | `COVERED_IN_SPEC` |
| `TRF-REQ-007` | Ten Mandatory Preconditions (`PRE-TRF-01..10`)| `NLE-POL-TEP-001` Sec 2 | Precondition Engine | Multi-factor gate before transcode| Scenario B, U | `COVERED_IN_SPEC` |
| `TRF-REQ-008` | Capability vs Proof Verification Doctrine | `NLE-POL-TEP-001` Sec 3 | Capability Matcher | Declared != verified support | Scenario M | `COVERED_IN_SPEC` |
| `TRF-REQ-009` | Unsupported Transformation Degradation & Halts| `NLE-POL-TEP-001` Sec 4 | Pipeline Controller | Skip optional, halt critical | Scenario C, I | `COVERED_IN_SPEC` |
| `TRF-REQ-010` | Eight-Category Format Conversion Contracts | `NLE-CON-FCC-001` Sec 1 | Format Converters | Strict schemas per category | Scenario A, G | `COVERED_IN_SPEC` |
| `TRF-REQ-011` | Input & Output Binary Syntactic Completeness | `NLE-CON-FCC-001` Sec 2 | Transcoder Worker | Reject truncated buffers | Scenario B, S | `COVERED_IN_SPEC` |
| `TRF-REQ-012` | Unsupported Property Omission & Record Rules | `NLE-CON-FCC-001` Sec 3 | `OmissionRecord` | Never silently discard fields | Scenario I, P | `COVERED_IN_SPEC` |
| `TRF-REQ-013` | Unit & Coordinate Transformation Matrix | `NLE-SPC-CNI-001` Sec 2 | Normalization Engine | Meters standard, matrix transform | Scenario G | `COVERED_IN_SPEC` |
| `TRF-REQ-014` | Polygon Winding Order & Chirality Preservation | `NLE-SPC-CNI-001` Sec 2 | Geometry Normalizer | Invert winding on axis reflection | Scenario G | `COVERED_IN_SPEC` |
| `TRF-REQ-015` | Permitted Defaults & Schema Invariants | `NLE-SPC-CNI-001` Sec 3 | Normalization Invariants | Zero arbitrary fabrication | Scenario D, H | `COVERED_IN_SPEC` |
| `TRF-REQ-016` | Four Semantic Mapping Classifications | `NLE-POL-SML-001` Sec 2 | Semantic Mapper | Direct, conditional, lossy, unmap | Scenario D, F | `COVERED_IN_SPEC` |
| `TRF-REQ-017` | Four Lossiness Tiers & Mandatory Accounting | `NLE-POL-SML-001` Sec 3 | `LossAccountingRecord` | Explicit user prompt on Tier 3 | Scenario E, P | `COVERED_IN_SPEC` |
| `TRF-REQ-018` | Approximation Governance & Error Bounds | `NLE-POL-SML-001` Sec 3 | Approximation Engine | Bounded approximations only | Scenario E | `COVERED_IN_SPEC` |
| `TRF-REQ-019` | Transformation Graph (DAG) Representation | `NLE-SPC-TGD-001` Sec 1 | Transformation DAG | Acyclic graph of operations | Scenario J, K | `COVERED_IN_SPEC` |
| `TRF-REQ-020` | Topological Execution & Failure Isolation | `NLE-SPC-TGD-001` Sec 2 | DAG Scheduler | Cancel children on parent failure | Scenario K, Y | `COVERED_IN_SPEC` |
| `TRF-REQ-021` | Multi-Stage Conversion Chains ($A \rightarrow \text{IR} \rightarrow B$)| `NLE-SPC-TGD-001` Sec 3 | Chain Orchestrator | Intermediates validated in Stage I| Scenario J | `COVERED_IN_SPEC` |
| `TRF-REQ-022` | Three Determinism Tiers & Reproducibility | `NLE-POL-TDP-001` Sec 2 | Determinism Engine | Bit-exact, numerical, non-determ | Scenario A | `COVERED_IN_SPEC` |
| `TRF-REQ-023` | Transformation Provenance Dossier Generation | `NLE-POL-TDP-001` Sec 3 | Provenance Manager | Cryptographic build record | Scenario A, N | `COVERED_IN_SPEC` |
| `TRF-REQ-024` | Semantic Output Comparison & Normalization | `NLE-POL-TDP-001` Sec 2 | Artifact Comparator | Compare semantic value not zip bit| Scenario V | `COVERED_IN_SPEC` |
| `TRF-REQ-025` | Out-of-Process Worker Sandboxing | `NLE-POL-TSA-001` Sec 2 | Process Manager | Isolated unprivileged child proc | Scenario R, S | `COVERED_IN_SPEC` |
| `TRF-REQ-026` | Untrusted Input Defenses & Fuzzing Mitigations| `NLE-POL-TSA-001` Sec 3 | Binary Parser | Integer overflow / buffer bounds | Scenario S | `COVERED_IN_SPEC` |
| `TRF-REQ-027` | Zero-Script Execution During Transformation | `NLE-POL-TSA-001` Sec 3 | Parser Sandbox | Ban execution during transcode | Scenario T | `COVERED_IN_SPEC` |
| `TRF-REQ-028` | Licensing Inheritance & Retail Asset Lockdown | `NLE-POL-TSA-001` Sec 4 | License Policy | Retail game content locked local | Scenario U | `COVERED_IN_SPEC` |
| `TRF-REQ-029` | Four Stage-I Validation Gates | `NLE-POL-TVC-001` Sec 2 | Stage I Validator | Pre-flight, IR, struct, semantic | Scenario A, O | `COVERED_IN_SPEC` |
| `TRF-REQ-030` | Geometric Metric Invariants ($\pm 0.001$ Tol) | `NLE-POL-TVC-001` Sec 3 | Metric Invariant Checker | Bounds, normals, winding check | Scenario A, G | `COVERED_IN_SPEC` |
| `TRF-REQ-031` | Downstream Stage J/K Handoff Dossier | `NLE-CON-TOH-001` Sec 2 | Handoff Controller | Formal dossier to Stage J/K | Scenario A, Z | `COVERED_IN_SPEC` |
| `TRF-REQ-032` | 34-Code Transformation Failure Taxonomy | `NLE-POL-TFD-001` Sec 2 | Diagnostic Engine | Comprehensive error handling | Scenario B, R | `COVERED_IN_SPEC` |

---

## 3. Decision & Governance Summary

All 32 material requirements are mapped to authoritative specification sections, explicit security rules, and synthetic validation scenarios. No unverified assumptions or placeholder claims are permitted.
