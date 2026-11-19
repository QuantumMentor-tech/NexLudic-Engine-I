# Host-Game Content Ingestion, Asset Discovery & Provenance Validation Specification

**Document Identifier:** `NLE-SPC-CID-001`  
**Classification:** Technical Architecture & Ingestion Governance Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-GAT-PH7-001` (Phase 7 Gate Decision: READY WITH CONDITIONS), `NLE-SPC-GMS-001` (Host Modification Architecture), `NLE-CON-CPS-001` (16-Stage Content Pipeline), `NLE-SPC-ADA-001` (Adapter SDK)  
**Security Level:** Internal Engineering Architecture  

---

## 1. Executive Summary & Ingestion Philosophy

NexLudic Engine provides cross-game interoperability and controlled modification experiences across diverse host games, engines, and platforms. Content ingestion is the foundational entry gate of the content pipeline: it determines which source assets exist, where they originated, whether they are authentic and licensed, whether their structure is valid, and whether they are authorized for downstream processing.

This specification (`NLE-SPC-CID-001`) defines how future implementations will execute Stages C, D, E, and F of the Universal Content Pipeline (`NLE-CON-CPS-001`):
- **Stage C:** Source-Content Discovery
- **Stage D:** Source Authorization & Provenance Validation
- **Stage E:** Source Validation & Integrity Checking
- **Stage F:** Format & Schema Resolution

A fundamental invariant of NexLudic Engine is that **file presence does not imply authorization, authenticity, compatibility, or safety**. The presence of an asset on disk or within a game directory does not grant permission to read, transcode, package, or redistribute it.

---

## 2. Fourteen Distinct Status Dimensions

To avoid dangerous over-simplification, the ingestion architecture strictly forbids collapsing asset evaluations into a single boolean `is_valid` flag. The system models fourteen orthogonal dimensions:

```
[1. Source Location] ------------> An approved physical or logical address/pointer
[2. Discovered File] ------------> Raw filesystem or container entry observed
[3. Candidate Asset] ------------> Filtered entry meeting preliminary content criteria
[4. Identified Asset] -----------> Assigned canonical identity and logical category
[5. Validated Asset] -----------> Passed structural schema and magic byte validation
[6. Authorized Asset] ----------> Explicitly permitted for use in the current task
[7. Verified Provenance] -------> Complete cryptographic chain-of-custody established
[8. Unresolved Provenance] -----> Origin, author, or build attestation incomplete
[9. Recognized Format] ---------> Format matches known MIME type or magic signature
[10. Supported Format] ---------> Registered adapter capable of parsing/transcoding
[11. Structurally Valid] -------> Adheres to format grammar without binary corruption
[12. Semantically Valid] -------> Internal references and coordinate bounds are valid
[13. Target Compatible] --------> Verified compatible with target game build/engine
[14. Downstream Eligible] ------> Explicitly cleared for specific pipeline transformation
```

---

## 3. Upstream Gate Verification & Inherited Governance

### 3.1 Phase 7 Gate Audit (`NLE-GAT-PH7-001`)
The Phase 7 Gate Assessor rendered the decision: **`READY WITH CONDITIONS`**. Step 62 inherits and strictly enforces all three binding conditions:
- **`P7-C01` (Phase 6 Mashup Planning Gate Hold):** Automated mashup workflow execution remains frozen until formal sign-off. Consequently, content ingestion in Step 62 is strictly defined as an offline, design-time, and human-authorized capability. No ingestion operation may automatically trigger physical game modification.
- **`P7-C02` (Universal Adapter SDK Wire Protocol & Memory Safety):** All asset inspection, format probing, and metadata extraction conducted by game-specific adapters must occur through isolated IPC processes adhering to strict protobuf wire serialization (`P5-C01`) and memory safety (`P5-C02`). Ingestion workers must never load unverified C++ DLLs or execute raw binary parsers in the main platform process.
- **`P7-C03` (Windows Medium IL & Anti-Cheat Inviolability):** Discovery and ingestion run strictly at standard user privileges (Windows Medium Integrity Level). Ingestion workers are prohibited from accessing protected memory, attaching debuggers, inspecting kernel anti-cheat structures, or bypassing file system access controls.

---

## 4. Ingestion Architectural Boundary & Core Invariants

```
+-----------------------------------------------------------------------------------------+
|                                    NEXLUDIC CORE                                        |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
|  | Source Definition &   |   | Canonical Asset         |   | Provenance, Trust &     |  |
|  | Discovery Controller  |-->| Identity Manager        |-->| License Gate            |  |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
+-------------------------------------------|---------------------------------------------+
                                            | (Isolated Sandbox / Read-Only Channel)
                                            v
+-----------------------------------------------------------------------------------------+
|                                GAME-SPECIFIC ADAPTERS                                   |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
|  | Asset Parser / Prober |   | Metadata Extractor      |   | Dependency Analyzer     |  |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
+-------------------------------------------|---------------------------------------------+
                                            | (Strictly Read-Only Access)
                                            v
+-----------------------------------------------------------------------------------------+
|                                  AUTHORIZED SOURCES                                     |
|   [Project Workspace]    [Approved Mod Directory]    [Signed Content Package (.nlpkg)]  |
|   (Executable Game Binaries & Protected Memory: STRICTLY OFF-LIMITS FOR INGESTION)       |
+-----------------------------------------------------------------------------------------+
```

### Invariant 1: Zero-Execution Inspection (`INV-CID-01`)
Content discovery and metadata extraction must NEVER execute untrusted script code, load dynamic link libraries (`.dll`, `.so`), or run external executables embedded within candidate assets. All format detection and metadata extraction must use sandboxed static parsers or abstract syntax tree (AST) analyzers.

### Invariant 2: Read-Only Non-Destructiveness (`INV-CID-02`)
The discovery and ingestion subsystem is strictly read-only with respect to sources. It must never move, delete, touch, or alter files in user directories, game folders, or mod repositories during discovery.

### Invariant 3: Explicit Operation-Specific Authorization (`INV-CID-03`)
Authorization to discover an asset does NOT grant authorization to read it; authorization to read does NOT grant authorization to transform; authorization to transform does NOT grant authorization to redistribute. Each operation must have distinct, verifiable authorization.

### Invariant 4: Sandbox & Resource Containment (`INV-CID-04`)
All archive decompression, texture probing, and mesh parsing must execute within strictly bounded memory (maximum 4 GB per worker) and storage quotas (maximum 25 GB staging), with hard defenses against decompression bombs (100:1 ratio ceiling) and path traversal (`..` escape).

---

## 5. Document Roadmap for Step 62 Deliverables

The specification framework for Step 62 consists of 15 interdependent technical documents in `docs/content-pipeline/`:
1. `NLE-SPC-CID-001`: Primary Ingestion & Asset Discovery Specification (This Document).
2. `NLE-CON-CSD-001`: Content Source Definition & Authorized Discovery Contract.
3. `NLE-MOD-CDR-001`: Content Discovery Request & Candidate Domain Model.
4. `NLE-SPC-CAI-001`: Canonical Asset Identity & Metadata Schema Specification.
5. `NLE-POL-AFD-001`: Asset Format Detection & Structural Validation Policy.
6. `NLE-POL-API-001`: Asset Provenance, Integrity & Source Authority Policy.
7. `NLE-POL-ALO-001`: Asset License, Ownership & Operation-Specific Authorization Policy.
8. `NLE-SPC-ADD-001`: Asset Dependency Discovery & Relationship Validation Specification.
9. `NLE-POL-ADC-001`: Asset Duplicate, Conflict & Revision Management Policy.
10. `NLE-POL-CIS-001`: Content Ingestion Filesystem, Archive & Untrusted-Input Security Policy.
11. `NLE-CON-AEP-001`: Asset Eligibility for Downstream Content Processing Contract.
12. `NLE-POL-CDF-001`: Content Discovery Failure, Resource & Diagnostic Policy.
13. `NLE-CAT-CID-001`: Synthetic Scenarios & Test Plan (Scenarios A through Z).
14. `NLE-MAT-STP-062`: Requirements Traceability Matrix (`CID-REQ-001..032`).
15. `NLE-REP-STP-062`: Step 62 Completion Report (`AC-01..23`).
