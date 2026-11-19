# Step 61 Host-Game Modification Requirements Traceability Matrix

**Document Identifier:** `NLE-MAT-STP-061`  
**Classification:** Requirements Engineering & Traceability Matrix  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-GAT-PH7-001`, `NLE-ADR-0001`..`0011`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Traceability Methodology

This matrix establishes bidirectional traceability between foundational architectural principles, upstream phase decisions, the 32 material Step 61 requirements (`GMS-REQ-001` through `GMS-REQ-032`), the 16 content pipeline stages (Stages A through P), security controls, and the 26 synthetic test scenarios (Scenarios A through Z).

---

## 2. Requirements Traceability Table (`GMS-REQ-001` to `GMS-REQ-032`)

| Req ID | Requirement Summary | Authoritative Document | Affected Entity / Stage | Security / Safety Rule | Related Synthetic Scenario | Coverage Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GMS-REQ-001` | Canonical host game identity reference | `NLE-MOD-GMS-001` Sec 2 | `CanonicalHostGameRef` / Stage B | Anti-spoofing identity match | Scenario B, J | `COVERED_IN_SPEC` |
| `GMS-REQ-002` | Installed game instance fingerprinting | `NLE-MOD-GMS-001` Sec 3 | `InstalledGameInstance` / Stage B | Executable SHA-256 validation | Scenario B, P | `COVERED_IN_SPEC` |
| `GMS-REQ-003` | Modification mechanism taxonomy | `NLE-MOD-GMS-001` Sec 4 | `ModificationMechanism` / Stage B | Reversibility rating enforcement | Scenario C, Y | `COVERED_IN_SPEC` |
| `GMS-REQ-004` | Declarative modification plan model | `NLE-MOD-GMS-001` Sec 5 | `ModificationPlan` / Stage G, M | Explicit operation manifest | Scenario A, Q | `COVERED_IN_SPEC` |
| `GMS-REQ-005` | Content artifact data contract | `NLE-MOD-GMS-001` Sec 6 | `ContentArtifact` / Stage H, I | Format & hash validation | Scenario E, K | `COVERED_IN_SPEC` |
| `GMS-REQ-006` | Applied modification ledger record | `NLE-MOD-GMS-001` Sec 7 | `AppliedModificationRecord` / Stage P | Immutable audit logging | Scenario A, P | `COVERED_IN_SPEC` |
| `GMS-REQ-007` | Strategy evaluation multi-factor inputs | `NLE-POL-GMS-001` Sec 2 | Strategy Selector / Stage B | Anti-cheat & privilege audit | Scenario B, C | `COVERED_IN_SPEC` |
| `GMS-REQ-008` | Non-destructive strategy hierarchy | `NLE-POL-GMS-001` Sec 3 | Strategy Selector / Stage B | Mod folder precedence | Scenario A, C | `COVERED_IN_SPEC` |
| `GMS-REQ-009` | Human-in-the-loop approval workflow | `NLE-POL-GMS-001` Sec 5 | User Approval Gate / Stage M | Signed authorization token | Scenario Q | `COVERED_IN_SPEC` |
| `GMS-REQ-010` | Prohibited strategy immediate rejection | `NLE-POL-GMS-001` Sec 3 | Policy Gate / Stage B | Ban on binary patching & injection | Scenario Y | `COVERED_IN_SPEC` |
| `GMS-REQ-011` | 16-stage universal pipeline definition | `NLE-CON-CPS-001` Sec 2 | Stages A through P | Sequential stage gates | Scenario A, W | `COVERED_IN_SPEC` |
| `GMS-REQ-012` | Pipeline state machine transitions | `NLE-CON-CPS-001` Sec 3 | Engine State Machine / All Stages| Automated rollback on failure | Scenario R, T | `COVERED_IN_SPEC` |
| `GMS-REQ-013` | Universal core vs adapter separation | `NLE-SPC-CFT-001` Sec 1 | Architecture Boundary / Stage H | Memory-safe wire protocol | Scenario E | `COVERED_IN_SPEC` |
| `GMS-REQ-014` | Canonical intermediate format (IR) specs | `NLE-SPC-CFT-001` Sec 2 | Pipeline Core / Stage F, H | glTF 2.0 / OpenEXR / WAV | Scenario E, K | `COVERED_IN_SPEC` |
| `GMS-REQ-015` | Lossless & bound preservation invariants | `NLE-SPC-CFT-001` Sec 3 | Validation Gate / Stage I | Metric tolerance $\pm 0.001$ | Scenario K | `COVERED_IN_SPEC` |
| `GMS-REQ-016` | ContentTranscodeService protocol | `NLE-SPC-CFT-001` Sec 4 | Adapter Interface / Stage H | Sandboxed transcode process | Scenario E, H | `COVERED_IN_SPEC` |
| `GMS-REQ-017` | Unsupported format graceful degradation | `NLE-SPC-CFT-001` Sec 5 | Pipeline Resolver / Stage F, W | Non-fatal optional asset skip | Scenario E, W | `COVERED_IN_SPEC` |
| `GMS-REQ-018` | Six dimensions of game compatibility | `NLE-POL-GCC-001` Sec 2 | Compatibility Engine / Stage B, J| Comprehensive matrix match | Scenario B, J | `COVERED_IN_SPEC` |
| `GMS-REQ-019` | Four-tier compatibility evidence model | `NLE-POL-GCC-001` Sec 3 | Evidence Evaluator / Stage B | Reject unverified agent claims | Scenario X | `COVERED_IN_SPEC` |
| `GMS-REQ-020` | Compatibility state lifecycle | `NLE-POL-GCC-001` Sec 4 | Lifecycle Manager / Stage B | Transition to BLOCKED on fail | Scenario B, V | `COVERED_IN_SPEC` |
| `GMS-REQ-021` | Dynamic invalidation triggers | `NLE-POL-GCC-001` Sec 5 | Background Monitor / Runtime | Game update invalidates cache | Scenario P, U | `COVERED_IN_SPEC` |
| `GMS-REQ-022` | Content package archive layout (.nlpkg) | `NLE-CON-CPM-001` Sec 2 | Package Container / Stage K | Standard directory layout | Scenario F | `COVERED_IN_SPEC` |
| `GMS-REQ-023` | Signed package manifest schema | `NLE-CON-CPM-001` Sec 3 | Manifest Parser / Stage L | SemVer and hash verification | Scenario F, O | `COVERED_IN_SPEC` |
| `GMS-REQ-024` | Provenance dossier & supply-chain audit | `NLE-CON-CPM-001` Sec 4 | Provenance Verifier / Stage D | Cryptographic Ed25519 signature | Scenario Z | `COVERED_IN_SPEC` |
| `GMS-REQ-025` | Dependency grammar & DAG cycle rejection| `NLE-CON-CPM-001` Sec 5 | Dependency Resolver / Stage J | Cycle detection algorithm | Scenario G, V | `COVERED_IN_SPEC` |
| `GMS-REQ-026` | Pre-modification baseline snapshot | `NLE-POL-GIP-001` Sec 2 | Snapshot Engine / Stage N | Fingerprint pre-modification state| Scenario I, S | `COVERED_IN_SPEC` |
| `GMS-REQ-027` | Transactional staging & journaling | `NLE-POL-GIP-001` Sec 3 | Staging Manager / Stage N | Atomic deployment protocol | Scenario R | `COVERED_IN_SPEC` |
| `GMS-REQ-028` | Preservation of existing user mods | `NLE-POL-GIP-001` Sec 4 | Conflict Detector / Stage N | Isolated namespace / user prompt | Scenario I | `COVERED_IN_SPEC` |
| `GMS-REQ-029` | Three-tier rollback & disaster recovery | `NLE-POL-GIP-001` Sec 5 | Recovery Engine / Stage P | Bit-for-bit baseline restoral | Scenario R, T | `COVERED_IN_SPEC` |
| `GMS-REQ-030` | Content trust tiers & archive inspection | `NLE-SEC-CSG-001` Sec 2, 3 | Security Scanner / Stage C, E | Zip slip / bomb protection | Scenario L, N | `COVERED_IN_SPEC` |
| `GMS-REQ-031` | Script-bearing content sandbox & AST | `NLE-SEC-CSG-001` Sec 4 | Script Verifier / Stage L | Ban OS calls / isolate sandbox | Scenario L | `COVERED_IN_SPEC` |
| `GMS-REQ-032` | IP, licensing & prompt injection defense| `NLE-SEC-CSG-001` Sec 5 | IP & Prompt Guard / Stage D | SPDX validation & sanitization | Scenario H, M | `COVERED_IN_SPEC` |

---

## 3. Decision & Governance Summary

All 32 material requirements are mapped to authoritative specification sections, explicit security rules, and synthetic validation scenarios. No unverified assumptions or placeholder claims are permitted.
