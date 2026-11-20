# STEP 74 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-074`  
**Lifecycle Stage:** Authoritative Architecture Register  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Architectural Governance  
**Upstream Dependencies:** `NLE-SPC-CSM-001` through `NLE-POL-SAU-001`  
**Downstream Dependents:** Phase 9 Architecture Baseline

---

## 1. Summary of Architectural Decision Records (ADRs)

| ADR Identifier | Title | Status | Impact Area |
| :--- | :--- | :---: | :--- |
| `ADR-SMR-001` | Canonical Intermediate Representation (CIR) Standard | **ACCEPTED** | Core Architecture |
| `ADR-SMR-002` | Right-Handed Y-Up SI Metric Coordinate Standard | **ACCEPTED** | Spatial Transformations |
| `ADR-SMR-003` | Sandboxed AST vs Dynamic Scripting for Derivations | **ACCEPTED** | Security & Performance |
| `ADR-SMR-004` | Two-Tier Identity Model (Local ID vs Canonical CEID) | **ACCEPTED** | Entity Tracking |
| `ADR-SMR-005` | Opaque Metadata Vaulting (`vault_token`) for Asymmetric Schemas | **ACCEPTED** | Data Preservation |
| `ADR-SMR-006` | Strict IEEE 754 NaN/Infinity Rejection Policy | **ACCEPTED** | Numerical Stability |
| `ADR-SMR-007` | Separation of Semantic Mapping from Continuous Synchronization | **ACCEPTED** | Boundary Architecture |
| `ADR-SMR-008` | Kahn's Algorithm for Registration-Time Cycle Detection | **ACCEPTED** | Graph Validation |
| `ADR-SMR-009` | Strict Medium IL User-Space Anti-Cheat Inviolability | **ACCEPTED** | Security & Anti-Cheat |
| `ADR-SMR-010` | Human-in-the-Loop Gate for AI-Proposed Mapping Definitions | **ACCEPTED** | AI Governance |

---

## 2. Detailed Architectural Decision Records

### ADR-SMR-001: Canonical Intermediate Representation (CIR) Standard
- **Context:** Point-to-point mappings between $N$ game engines require $O(N^2)$ mapping records. An intermediate canonical representation reduces this to $O(N)$.
- **Decision:** Adopt a unified Canonical Intermediate Representation (CIR). All source concepts normalize to CIR before target projection.
- **Consequences:** Eliminates combinatorial explosion; introduces small intermediate conversion step; easily amortized.

### ADR-SMR-002: Right-Handed Y-Up SI Metric Coordinate Standard
- **Context:** Unreal uses LH Z-Up (cm), Unity uses LH Y-Up (m), Blender uses RH Z-Up (m), Godot uses RH Y-Up (m).
- **Decision:** NexLudic Canonical Frame is standardized to Right-Handed, $+Y$ Up, $+Z$ Forward, meters ($1.0 \text{ unit} = 1.0 \text{ m}$).
- **Consequences:** Universal spatial baseline; conversion matrices are constant and precomputed.

### ADR-SMR-003: Sandboxed AST vs Dynamic Scripting for Derivations
- **Context:** Game mappings often need math derivations. Using Python/JS `eval()` creates severe arbitrary code execution risks.
- **Decision:** Implement a pure, sandboxed AST evaluator supporting only basic arithmetic, vector math, and conditionals with hard execution limits ($50 \ \mu\text{s}$, depth $\le 8$).
- **Consequences:** Zero vulnerability to remote code execution; deterministic performance.

### ADR-SMR-004: Two-Tier Identity Model (Local ID vs Canonical CEID)
- **Context:** Host games use native pointers or integer IDs that clash or change on respawn.
- **Decision:** Map native IDs to bridge-wide UUIDv4 Canonical Entity Identifiers (CEIDs) managed by a lock-free session registry.
- **Consequences:** Stable cross-game identity tracking independent of host game internal memory layouts.

### ADR-SMR-005: Opaque Metadata Vaulting (`vault_token`) for Asymmetric Schemas
- **Context:** Mapping high-dimensional source data through low-dimensional targets permanently loses unmapped attributes on return.
- **Decision:** Package unmapped fields into an HMAC-authenticated `vault_token` carried transparently by target payloads.
- **Consequences:** Full round-trip lossless preservation without forcing the target game to support foreign fields.

### ADR-SMR-006: Strict IEEE 754 NaN/Infinity Rejection Policy
- **Context:** A single `NaN` in a physics or transform vector can poison calculation trees and crash game engines.
- **Decision:** Any payload containing `NaN`, `+Infinity`, `-Infinity`, or subnormal floats is dropped at the normalization gate.
- **Consequences:** Total immunity to arithmetic poisoning cascades.

### ADR-SMR-007: Separation of Semantic Mapping from Continuous Synchronization
- **Context:** Conflating payload translation with temporal extrapolation, dead-reckoning, and networking degrades architectural cohesion.
- **Decision:** Semantic Mapping (Step 74) strictly handles static payload transformation. Temporal synchronization is deferred to Step 75.
- **Consequences:** High modularity; mapping engine is purely functional and easily testable.

### ADR-SMR-008: Kahn's Algorithm for Registration-Time Cycle Detection
- **Context:** Cyclic dependencies in derived mappings cause infinite loops or stack overflow at runtime.
- **Decision:** Execute Kahn's topological sort during mapping registration; reject cycles before activation.
- **Consequences:** Zero runtime cycle overhead; guarantees acyclic execution order.

### ADR-SMR-009: Strict Medium IL User-Space Anti-Cheat Inviolability
- **Context:** Anti-cheat systems (Easy Anti-Cheat, BattlEye, Vanguard) flag process injection and memory scanning.
- **Decision:** NexLudic runs strictly at Medium IL using documented loopback message interfaces. Zero memory scanning or DLL injection.
- **Consequences:** Safe, non-bannable, compliant interoperability.

### ADR-SMR-010: Human-in-the-Loop Gate for AI-Proposed Mapping Definitions
- **Context:** Phase 7 AI agents can draft semantic mappings, but AI hallucinations could inject bad transformations.
- **Decision:** AI-generated mappings remain in `DRAFT_AI_PROPOSED` and cannot be activated without human admin approval and signature.
- **Consequences:** Maximum safety; harnesses AI generation speed while preventing autonomous degradation.
