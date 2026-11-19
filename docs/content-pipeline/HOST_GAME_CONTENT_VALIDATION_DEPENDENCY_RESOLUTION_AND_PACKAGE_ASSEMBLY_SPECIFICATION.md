# Host-Game Content Validation, Dependency Resolution & Package Assembly Specification

**Document Identifier:** `NLE-SPC-PVA-001`  
**Classification:** Technical Architecture & Packaging Governance Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-GAT-PH7-001` (Phase 7 Gate Decision: READY WITH CONDITIONS), `NLE-SPC-GMS-001` (Host Modification Architecture), `NLE-CON-CPS-001` (16-Stage Content Pipeline), `NLE-CON-TOH-001` (Transformation Handoff Contract)  
**Security Level:** Internal Engineering Architecture  

---

## 1. Executive Summary & Packaging Scope

Individual transformed artifacts produced by Stage H and validated by Stage I must be aggregated, cross-validated, checked for dependency consistency, and assembled into a coherent, verifiable package container (`.nlpkg`).

This specification (`NLE-SPC-PVA-001`) defines how future implementations will execute Stages J, K, and L of the Universal Content Pipeline (`NLE-CON-CPS-001`):
- **Stage J:** Dependency & Compatibility Resolution
- **Stage K:** Package Assembly Planning & Construction
- **Stage L:** Package Integrity & Security Checks

A fundamental principle of the NexLudic packaging architecture is that **a technically valid artifact, a target-compatible artifact, an internally consistent package, and an authorized package are distinct conditions**:

```
[Technically Valid Artifact] ---------> Conforms to format binary syntax and chunk grammar
           |
           v
[Target-Compatible Artifact] ---------> Compatible with specific game build and engine limits
           |
           v
[Internally Consistent Package] ------> Zero destination collisions, closed dependency DAG
           |
           v
[Authorized & Ready Package] ---------> Signed user approval, SPDX compliant, stage cleared
```

---

## 2. Upstream Gate Verification & Inherited Governance

### 2.1 Phase 7 Gate Audit (`NLE-GAT-PH7-001`)
The Phase 7 Gate Assessor rendered the decision: **`READY WITH CONDITIONS`**. Step 64 inherits and strictly enforces all three binding conditions:
- **`P7-C01` (Phase 6 Mashup Planning Gate Hold):** Automated mashup workflow execution remains frozen until formal sign-off. Consequently, package validation and assembly in Step 64 are strictly offline, logical planning operations. Assembling a package definition does NOT execute game modification or trigger live deployment.
- **`P7-C02` (Universal Adapter SDK Wire Protocol & Memory Safety):** All dependency resolution queries and manifest validation operations that interface with adapter capabilities must use isolated IPC processes with strict protobuf serialization (`P5-C01`) and memory safety (`P5-C02`).
- **`P7-C03` (Windows Medium IL & Anti-Cheat Inviolability):** Per `DEC-002` and `BLK-SEC-01`, package assembly and security scanning run strictly at Windows Medium Integrity Level. The packaging engine must NEVER require administrative elevation, kernel drivers, or process hooking.

---

## 3. Core Packaging Invariants

```
+-----------------------------------------------------------------------------------------+
|                                    NEXLUDIC CORE                                        |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
|  | Dependency Graph      |   | Package Manifest        |   | Package Integrity &     |  |
|  | Resolver (Stage J)    |-->| Assembler (Stage K)     |-->| Security Gate (Stage L) |  |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
+-------------------------------------------|---------------------------------------------+
                                            | (Strict Logical Package Definition)
                                            v
+-----------------------------------------------------------------------------------------+
|                              STAGED CONTENT PACKAGE (.nlpkg)                            |
|   manifest.json              <-- Authoritative Signed Package Manifest                  |
|   provenance.json            <-- Full Cryptographic Chain-of-Custody                    |
|   assets/                    <-- Transcoded Game-Ready Payloads (Meshes, Textures)      |
|   overrides/                 <-- Configuration & Virtual Path Overrides                 |
+-----------------------------------------------------------------------------------------+
                                            | (Strict Boundary Invariant: INV-PVA-04)
                                            v
+-----------------------------------------------------------------------------------------+
|                                   TARGET HOST GAME                                      |
|   [Game Filesystem] <--- UNTOUCHED (Requires Separate Stage M/N Human Authorization)    |
+-----------------------------------------------------------------------------------------+
```

### Invariant 1: Offline Logical Assembly (`INV-PVA-01`)
Package validation and assembly planning are strictly offline operations. Generating a package definition or staged `.nlpkg` archive must NEVER modify, create, or delete files within a live host game installation.

### Invariant 2: Deterministic Conflict Resolution (`INV-PVA-02`)
The package resolver must NEVER resolve resource destination conflicts using "last-writer-wins" or random selection. If two members claim the same relative path without an explicit, authorized override rule, assembly is halted.

### Invariant 3: Manifest Completeness & Cryptographic Coverage (`INV-PVA-03`)
Every asset bundled into a package must be cataloged in `manifest.json` with its exact byte size and SHA-256 digest. No untracked or wildcard files may exist within a `.nlpkg` container.

### Invariant 4: No Implicit Application Permission (`INV-PVA-04`)
A package that is validated, dependency-consistent, and marked `PACKAGE_READY` does NOT possess authorization to modify a host game. Physical deployment remains strictly locked behind Stage M (Pre-Application Review) and Stage N (Application Handoff).

---

## 4. Document Roadmap for Step 64 Deliverables

The specification framework for Step 64 consists of 12 interdependent technical documents in `docs/content-pipeline/`:
1. `NLE-SPC-PVA-001`: Primary Package Assembly Specification (This Document).
2. `NLE-CON-AEF-001`: Transformed-Artifact Eligibility & Final Validation Contract.
3. `NLE-POL-PDG-001`: Package Dependency Graph, Version Resolution & Conflict Policy.
4. `NLE-POL-PMS-001`: Package Manifest Schema & Compatibility Policy.
5. `NLE-POL-PMC-001`: Package Membership, Completeness & Cross-Artifact Consistency Policy.
6. `NLE-CON-PAD-001`: Package Assembly Determinism, Revisioning & Freshness Contract.
7. `NLE-POL-PST-001`: Package Security, Trust, Authorization & Provenance Policy.
8. `NLE-POL-PVF-001`: Package Validation Failure, Diagnostics & Recovery Policy.
9. `NLE-CON-PRH-001`: Package Readiness & Downstream Handoff Contract.
10. `NLE-CAT-PVA-001`: Package Validation & Dependency Resolution Synthetic Test Plan (Scenarios A through AH).
11. `NLE-MAT-STP-064`: Requirements Traceability Matrix (`PVA-REQ-001..032`).
12. `NLE-REP-STP-064`: Step 64 Completion Report (`AC-01..24`).
