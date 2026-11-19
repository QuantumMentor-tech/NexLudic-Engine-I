# Step 62 Content Ingestion Requirements Traceability Matrix

**Document Identifier:** `NLE-MAT-STP-062`  
**Classification:** Requirements Engineering & Traceability Matrix  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-MAT-STP-061`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Traceability Methodology

This matrix establishes bidirectional traceability between foundational architectural principles, upstream phase decisions, the 32 material Step 62 requirements (`CID-REQ-001` through `CID-REQ-032`), the discovery contracts, and the 26 synthetic test scenarios (Scenarios A through Z).

---

## 2. Requirements Traceability Table (`CID-REQ-001` to `CID-REQ-032`)

| Req ID | Requirement Summary | Authoritative Document | Affected Domain Entity | Security / Governance Rule | Related Synthetic Scenario | Coverage Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `CID-REQ-001` | Content Source Taxonomy & Registration | `NLE-CON-CSD-001` Sec 2 | `ContentSourceDefinition` | Whitelist approved sources only | Scenario A, B | `COVERED_IN_SPEC` |
| `CID-REQ-002` | Source Authorization & Boundary Scope | `NLE-CON-CSD-001` Sec 3 | `AuthorizationScope` | Bounded read-only access | Scenario T, X | `COVERED_IN_SPEC` |
| `CID-REQ-003` | Source Reference vs Content Invariant | `NLE-CON-CSD-001` Sec 4 | `SourceReference` | Presence != authorization | Scenario C, K | `COVERED_IN_SPEC` |
| `CID-REQ-004` | Structured Discovery Request Grammar | `NLE-MOD-CDR-001` Sec 2 | `DiscoveryRequest` | Task & plan reference binding | Scenario A, X | `COVERED_IN_SPEC` |
| `CID-REQ-005` | Discovered Candidate Data Model | `NLE-MOD-CDR-001` Sec 3 | `DiscoveredCandidate` | Preliminary unvalidated status | Scenario D, G | `COVERED_IN_SPEC` |
| `CID-REQ-006` | Canonical Asset Identity & Namespacing | `NLE-SPC-CAI-001` Sec 2 | `CanonicalAssetRecord` | Namespaced grammar, no filenames | Scenario D, E | `COVERED_IN_SPEC` |
| `CID-REQ-007` | Logical Asset vs Content Revision Model| `NLE-SPC-CAI-001` Sec 3 | `ContentRevision` | Immutable SHA-256 revision IDs | Scenario E, R | `COVERED_IN_SPEC` |
| `CID-REQ-008` | Ingestion Result Auditable Ledger | `NLE-MOD-CDR-001` Sec 5 | `IngestionResult` | Complete audit trail & metrics | Scenario Y | `COVERED_IN_SPEC` |
| `CID-REQ-009` | Filesystem Traversal & Root Confinement| `NLE-CON-CSD-001` Sec 5 | Discovery Controller | Canonical path prefix validation | Scenario P, T | `COVERED_IN_SPEC` |
| `CID-REQ-010` | Symlink & Junction Escape Prevention | `NLE-CON-CSD-001` Sec 5 | Path Resolver | Disallow symlinks outside root | Scenario P | `COVERED_IN_SPEC` |
| `CID-REQ-011` | External Registry & Network Access Bounds| `NLE-CON-CSD-001` Sec 2 | Network Fetcher | Read-only signed registry access | Scenario C, L | `COVERED_IN_SPEC` |
| `CID-REQ-012` | Official Modding Interface Adaptation | `NLE-CON-CSD-001` Sec 3 | Adapter Binding | Official mod tools integration | Scenario W | `COVERED_IN_SPEC` |
| `CID-REQ-013` | Multi-Stage Metadata Levels (L0-L5) | `NLE-SPC-CAI-001` Sec 4 | Metadata Pipeline | Inferred metadata requires flag | Scenario H, O | `COVERED_IN_SPEC` |
| `CID-REQ-014` | Category-Specific Metadata Schemas | `NLE-SPC-CAI-001` Sec 5 | Mesh / Tex Schemas | Strict typing and bounding boxes | Scenario H | `COVERED_IN_SPEC` |
| `CID-REQ-015` | Metadata Safety & Anti-Injection Guards | `NLE-SPC-CAI-001` Sec 6 | Sanitizer Gate | Disallow eval, sanitize strings | Scenario G, O | `COVERED_IN_SPEC` |
| `CID-REQ-016` | Eight-Stage Format Detection Hierarchy | `NLE-POL-AFD-001` Sec 1 | Format Detector | Extension is lowest authority | Scenario G | `COVERED_IN_SPEC` |
| `CID-REQ-017` | Magic Byte & Binary Signature Validation| `NLE-POL-AFD-001` Sec 2 | Binary Inspector | Reject extension spoofing | Scenario F, G | `COVERED_IN_SPEC` |
| `CID-REQ-018` | Structural Binary Validation Contracts | `NLE-POL-AFD-001` Sec 3 | Structural Validator | Buffer, accessor, and syntax check| Scenario H | `COVERED_IN_SPEC` |
| `CID-REQ-019` | Semantic Invariant & Bound Checks | `NLE-POL-AFD-001` Sec 3 | Semantic Validator | Reject NaN, zero-area, seams | Scenario H | `COVERED_IN_SPEC` |
| `CID-REQ-020` | Unsupported Format Degradation & Halts | `NLE-POL-AFD-001` Sec 4 | Pipeline Controller | Skip optional, halt critical | Scenario F, W | `COVERED_IN_SPEC` |
| `CID-REQ-021` | Six-Tier Source Authority Hierarchy | `NLE-POL-API-001` Sec 2 | Authority Gate | AI claims have zero authority | Scenario O, Z | `COVERED_IN_SPEC` |
| `CID-REQ-022` | Cryptographic Provenance Attestation | `NLE-POL-API-001` Sec 3 | Provenance Attestation | Ed25519 signature verification | Scenario A, J | `COVERED_IN_SPEC` |
| `CID-REQ-023` | SHA-256 Content Digest Verification | `NLE-POL-API-001` Sec 4 | Integrity Engine | Stream and verify file checksum | Scenario E, R | `COVERED_IN_SPEC` |
| `CID-REQ-024` | Nine Granular Operation Authorization | `NLE-POL-ALO-001` Sec 2 | Authorization Gate | Distinct discover/read/trans/share| Scenario K, X | `COVERED_IN_SPEC` |
| `CID-REQ-025` | SPDX Licensing Metadata & Governance | `NLE-POL-ALO-001` Sec 3 | License Validator | Explicit SPDX license tags | Scenario K | `COVERED_IN_SPEC` |
| `CID-REQ-026` | Proprietary Game Asset Redistribution Ban| `NLE-POL-ALO-001` Sec 2 | IP Protection Gate | Banned public redistribution | Scenario K | `COVERED_IN_SPEC` |
| `CID-REQ-027` | Asset Dependency Graph & DAG Cycle Check| `NLE-SPC-ADD-001` Sec 4 | Dependency Engine | Cycle detection DFS algorithm | Scenario M, N | `COVERED_IN_SPEC` |
| `CID-REQ-028` | Duplicate Identification & Collision Res| `NLE-POL-ADC-001` Sec 2, 3 | Duplicate Manager | Deduplicate bytes, prompt collision| Scenario D, V | `COVERED_IN_SPEC` |
| `CID-REQ-029` | Revalidation & Invalidation Triggers | `NLE-POL-ADC-001` Sec 4 | Cache / State Manager | Invalidate on byte/schema change | Scenario R, S | `COVERED_IN_SPEC` |
| `CID-REQ-030` | Safe Archive Inspection (Zip Bomb/Slip) | `NLE-POL-CIS-001` Sec 3 | Archive Scanner | 100:1 ratio ceiling, 10 GB limit | Scenario P, Q | `COVERED_IN_SPEC` |
| `CID-REQ-031` | Zero-Execution Script Inspection & AST | `NLE-POL-CIS-001` Sec 4 | AST Analyzer | Ban execution, inspect AST | Scenario L, U | `COVERED_IN_SPEC` |
| `CID-REQ-032` | Twelve-Dimension Asset Downstream Gate | `NLE-CON-AEP-001` Sec 2 | Eligibility Controller | Multi-factor clearance to Stage G| Scenario A, X | `COVERED_IN_SPEC` |

---

## 3. Decision & Governance Summary

All 32 material requirements are mapped to authoritative specification sections, explicit security rules, and synthetic validation scenarios. No unverified assumptions or placeholder claims are permitted.
