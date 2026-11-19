# Host-Game Content Transformation, Format Conversion & Normalization Specification

**Document Identifier:** `NLE-SPC-TRF-001`  
**Classification:** Technical Architecture & Transformation Governance Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-GAT-PH7-001` (Phase 7 Gate Decision: READY WITH CONDITIONS), `NLE-SPC-GMS-001` (Host Modification Architecture), `NLE-CON-CPS-001` (16-Stage Content Pipeline), `NLE-SPC-CID-001` (Ingestion Specification), `NLE-CON-AEP-001` (Asset Eligibility Contract)  
**Security Level:** Internal Engineering Architecture  

---

## 1. Executive Summary & Transformation Philosophy

Cross-game interoperability and host-game modification require converting assets between divergent proprietary engines (e.g. Unreal Engine, Unity, Creation Engine, Source, Godot) while maintaining geometric, visual, auditory, and logical fidelity.

This specification (`NLE-SPC-TRF-001`) defines how future implementations will execute Stages G, H, and I of the Universal Content Pipeline (`NLE-CON-CPS-001`):
- **Stage G:** Transformation Planning
- **Stage H:** Controlled Transformation
- **Stage I:** Intermediate Artifact Validation

The architecture enforces a critical distinction: **a successful file format conversion does not guarantee semantic equivalence, game compatibility, or authorization to install or redistribute**. Converting bytes into a target file structure without verifying coordinate frames, bounding volumes, material bindings, or game engine constraints produces silent in-game rendering failures, physics crashes, or game state corruption.

---

## 2. Sixteen Distinct Asset & Transformation States

To prevent dangerous conflation of distinct engineering stages, the transformation pipeline strictly models sixteen separate conditions:

```
[1. Discovered Source Asset] ---------> Found during authorized discovery scan (Stage C)
[2. Validated Source Asset] ----------> Structural grammar and integrity verified (Stage E)
[3. Transformation-Authorized Asset] -> Explicitly cleared for conversion gate G4 (Step 62)
[4. Declared Source Format] ----------> MIME type claimed in container or manifest
[5. Verified Source Format] ----------> Magic bytes and binary signature confirmed
[6. Intermediate Representation (IR)] -> Normalized canonical schema (glTF, OpenEXR, WAV)
[7. Target-Specific Representation] --> Proprietary engine format (.uasset, .nif, .dds)
[8. Transformation Definition] -------> Registered capability and contract (Stage G)
[9. Transformation Execution Request] -> Bounded execution parameters for a specific run
[10. Authorized Transformation] ------> User-approved and cryptographic token bound
[11. Executed Transformation] --------> Sandcoded transcode worker completed run
[12. Structurally Valid Transformed] -> Header, chunks, and grammar pass binary check
[13. Semantically Valid Transformed] -> Invariants, bounding box, tangents verified (Stage I)
[14. Target-Compatible Artifact] -----> Compatible with specific target game build/engine
[15. Package-Ready Artifact] ---------> Bundled with manifest for dependency DAG (Stage K)
[16. Approved Application Artifact] ---> Explicit human sign-off for installation (Stage M)
```

---

## 3. Upstream Gate Verification & Inherited Governance

### 3.1 Phase 7 Gate Audit (`NLE-GAT-PH7-001`)
The Phase 7 Gate Assessor rendered the decision: **`READY WITH CONDITIONS`**. Step 63 inherits and strictly enforces all three binding conditions:
- **`P7-C01` (Phase 6 Mashup Planning Gate Hold):** Automated mashup workflow execution remains frozen until formal sign-off. Consequently, transformation planning and execution in Step 63 are strictly offline, design-time, and human-authorized. No transformation may automatically trigger game modification or background live-patching.
- **`P7-C02` (Universal Adapter SDK Wire Protocol & Memory Safety):** All format transcoding and native binary conversions must occur within isolated worker processes communicating via protobuf over standard IPC channels (`P5-C01`). Unmanaged memory safety must be strictly enforced (`P5-C02`). No raw C++ pointers or shared memory are permitted across the core-adapter boundary.
- **`P7-C03` (Windows Medium IL & Anti-Cheat Inviolability):** Per `DEC-002` and `BLK-SEC-01`, transformation tools run strictly at Windows Medium Integrity Level. Converters are prohibited from inspecting game process memory, injecting DLLs, or modifying protected system libraries.

---

## 4. Core Transformation Principles & Invariants

```
+-----------------------------------------------------------------------------------------+
|                                    NEXLUDIC CORE                                        |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
|  | Transformation Graph  |   | Canonical Intermediate  |   | Quality, Fidelity &     |  |
|  | Planner (Stage G)     |-->| Representation (IR)     |-->| Loss Accounting Gate    |  |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
+-------------------------------------------|---------------------------------------------+
                                            | (Strict Canonical Protobuf Wire Protocol)
                                            v
+-----------------------------------------------------------------------------------------+
|                                GAME-SPECIFIC ADAPTERS                                   |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
|  | Unreal Transcoder     |   | Creation Engine NIF     |   | Custom Engine Adapters  |  |
|  | (Sandboxed Worker)    |   | (Sandboxed Worker)      |   | (Sandboxed Worker)      |  |
|  +-----------------------+   +-------------------------+   +-------------------------+  |
+-------------------------------------------|---------------------------------------------+
                                            | (Isolated Output Staging Area)
                                            v
+-----------------------------------------------------------------------------------------+
|                            STAGED TRANSFORMATION OUTPUTS                                |
|   [%LOCALAPPDATA%/NexLudic/staging/<run_id>/intermediate/]                             |
|   (Zero Write Access to Host Game Directories during Stages G, H, and I)                |
+-----------------------------------------------------------------------------------------+
```

### Invariant 1: Isolated Sandbox Transformation (`INV-TRF-01`)
All transcoding operations must execute inside an ephemeral, isolated workspace directory (`.staging/transcode_<run_id>/`). Adapters are NEVER granted write access to live host game installations during transformation.

### Invariant 2: Explicit Loss Accounting (`INV-TRF-02`)
Any transformation that reduces geometric precision, discards animation bones, downsamples textures, or drops shader parameters must explicitly record a `LossAccountingRecord`. Silent loss of material gameplay or visual properties is strictly prohibited.

### Invariant 3: Semantic Invariant Preservation (`INV-TRF-03`)
Mesh bounding volumes, coordinate origins, and normal vectors must satisfy metric preservation tolerances ($\pm 0.001$ units). Rotations and scale transformations must preserve chirality (handedness) and avoid polygon inversion.

### Invariant 4: Universal Core vs. Game-Specific Adapter Separation (`INV-TRF-04`)
The Universal Core manages DAG scheduling, intermediate representations (glTF, OpenEXR, WAV), provenance, and quality gates. Game-specific adapters implement proprietary binary serialization (e.g. `.uasset`, `.nif`, `.dds`, `.ba2`) behind strictly typed capability interfaces.

---

## 5. Document Roadmap for Step 63 Deliverables

The specification framework for Step 63 consists of 15 interdependent technical documents in `docs/content-pipeline/`:
1. `NLE-SPC-TRF-001`: Primary Transformation & Conversion Specification (This Document).
2. `NLE-MOD-TRF-001`: Content Transformation Domain Model & Lifecycle Contract.
3. `NLE-POL-TEP-001`: Content Transformation Eligibility & Precondition Policy.
4. `NLE-CON-FCC-001`: Content Format Conversion Source-Target Contract.
5. `NLE-SPC-CNI-001`: Content Normalization & Intermediate Representation Specification.
6. `NLE-POL-SML-001`: Cross-Game Content Semantic Mapping & Lossiness Policy.
7. `NLE-SPC-TGD-001`: Content Transformation Graph & Dependency Ordering Specification.
8. `NLE-POL-TDP-001`: Content Transformation Determinism, Reproducibility & Provenance Policy.
9. `NLE-POL-TSA-001`: Content Transformation Security, Authorization & Untrusted-Input Policy.
10. `NLE-POL-TVC-001`: Content Transformation Validation & Target Compatibility Policy.
11. `NLE-CON-TOH-001`: Content Transformation Output Artifact & Downstream Handoff Contract.
12. `NLE-POL-TFD-001`: Content Transformation Failure, Resource & Diagnostic Policy.
13. `NLE-CAT-TRF-001`: Content Transformation Synthetic Scenarios & Test Plan (Scenarios A through Z).
14. `NLE-MAT-STP-063`: Requirements Traceability Matrix (`TRF-REQ-001..032`).
15. `NLE-REP-STP-063`: Step 63 Completion Report (`AC-01..24`).
