# STEP 77 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-077`  
**Lifecycle Stage:** Authoritative Architecture Register  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Architectural Governance  
**Upstream Dependencies:** `NLE-SPC-HIO-001` through `NLE-CON-HOA-001`  
**Downstream Dependents:** Phase 9 Baseline Architecture

---

## 1. Summary of Architectural Decision Records (ADRs)

| ADR Identifier | Mandatory Decision Area | Status | Impact Area |
| :--- | :--- | :---: | :--- |
| `ADR-HIO-001` | Approved Hybrid Plan Representation | **ACCEPTED** | Plan Contract |
| `ADR-HIO-002` | Authority Responsible for Accepting Plan | **ACCEPTED** | Plan Governance |
| `ADR-HIO-003` | Minimum Evidence for Capability Eligibility | **ACCEPTED** | Capability Model |
| `ADR-HIO-004` | Required Dependency Representation | **ACCEPTED** | Dependency Graph |
| `ADR-HIO-005` | Handling of Cyclic Dependencies | **ACCEPTED** | Graph Validation |
| `ADR-HIO-006` | Strategy & Component Responsibility Boundaries | **ACCEPTED** | Architecture Boundaries |
| `ADR-HIO-007` | Shared Operation Identity Across Transitions | **ACCEPTED** | Operation Tracing |
| `ADR-HIO-008` | Supported Handoff Types | **ACCEPTED** | Handoff Lifecycles |
| `ADR-HIO-009` | Conditions for Strategy Changes in Session | **ACCEPTED** | Dynamic Handoff |
| `ADR-HIO-010` | Ownership Transfer and Dual-Writer Prevention | **ACCEPTED** | Concurrency Safety |
| `ADR-HIO-011` | Conditions Permitting Fallback | **ACCEPTED** | Resilience & Fallback |
| `ADR-HIO-012` | Approvals Required for Privilege Changes | **ACCEPTED** | Security & Auth |
| `ADR-HIO-013` | Degraded-Mode Behavior & Status Reporting | **ACCEPTED** | Degraded Operation |
| `ADR-HIO-014` | Handling of Unknown Operation Outcomes | **ACCEPTED** | Fault Tolerance |
| `ADR-HIO-015` | Recovery and Failback Prerequisites | **ACCEPTED** | Recovery Coordination |
| `ADR-HIO-016` | Resource Allocation and Queue Bounds | **ACCEPTED** | Resource Governance |
| `ADR-HIO-017` | Orchestration Audit Retention and Privacy | **ACCEPTED** | Observability & Audit |
| `ADR-HIO-018` | Phase 9 Integration Qualification Evidence | **ACCEPTED** | End-to-End Readiness |

---

## 2. Detailed Architectural Decision Records

### ADR-HIO-001: Approved Hybrid Plan Representation
- **Decision:** Standardize Hybrid Integration Plans as immutable JSON Schema Draft 2020-12 documents with canonical SHA-256 integrity hashes.
- **Consequences:** Guarantees machine-validatable, portable, and tamper-evident plan definitions.

### ADR-HIO-002: Authority Responsible for Accepting Plan
- **Decision:** Plan acceptance requires **dual cryptographic signatures**: the User/Administrator security token and the Platform Verification Authority key.
- **Consequences:** Eliminates unauthorized automatic activation of hybrid integrations.

### ADR-HIO-003: Minimum Evidence for Capability Eligibility
- **Decision:** Strategies require **Level 3 (Verified)** in synthetic harnesses and **Level 4 (Authorized)** security tokens before inclusion in an active plan. Level 1 (Declared) claims are strictly rejected.
- **Consequences:** Prevents unverified adapters from failing in production sessions.

### ADR-HIO-004: Required Dependency Representation
- **Decision:** Dependencies are represented as a directed graph $G = (V, E)$ with typed, explicit dependency relationships.
- **Consequences:** Enables formal topological scheduling and unambiguous startup ordering.

### ADR-HIO-005: Handling of Cyclic Dependencies
- **Decision:** Kahn's algorithm is executed at plan validation; any cyclic dependency triggers **immediate fail-closed rejection**.
- **Consequences:** Eliminates runtime deadlocks between mutually dependent strategies.

### ADR-HIO-006: Strategy & Component Responsibility Boundaries
- **Decision:** Host-game modification is restricted to pre-launch filesystem assets; Live bridge is restricted to runtime socket IPC. Neither crosses into the other's domain.
- **Consequences:** Clean separation of concerns; prevents file corruption by runtime bridge components.

### ADR-HIO-007: Shared Operation Identity Across Transitions
- **Decision:** Reuse the Step 73 `operation_id` descriptor with an appended hop history array across all strategy transitions.
- **Consequences:** Full end-to-end traceability without fragmented or duplicate operational identities.

### ADR-HIO-008: Supported Handoff Types
- **Decision:** Codify eight formal handoff types (Initial Activation, Planned Transfer, Component Replacement, Capability-Driven, Recovery-Driven, Degraded Return, Reconfiguration, Planned Shutdown).
- **Consequences:** Every operational transition follows an explicit, verifiable lifecycle.

### ADR-HIO-009: Conditions for Strategy Changes in Session
- **Decision:** Strategy transitions require a verified cutoff revision, predecessor ingress quenching, and destination baseline establishment.
- **Consequences:** Prevents unobserved state gaps or dropped deltas during transitions.

### ADR-HIO-010: Ownership Transfer and Dual-Writer Prevention
- **Decision:** Exactly one strategy instance holds write authority for a domain at any time. Predecessor tokens are revoked before destination tokens are activated.
- **Consequences:** Mathematically eliminates split-brain dual-writer desynchronization.

### ADR-HIO-011: Conditions Permitting Fallback
- **Decision:** Fallbacks are permitted **only if** pre-approved and signed in the active plan; unlisted fallback destinations are strictly rejected.
- **Consequences:** Eliminates unpredictable, unverified failover paths.

### ADR-HIO-012: Approvals Required for Privilege Changes
- **Decision:** Silent privilege escalation is strictly forbidden. Any transition requiring higher OS or filesystem permissions requires explicit interactive user re-authorization.
- **Consequences:** Upholds least-privilege principles and prevents malicious privilege escalation attacks.

### ADR-HIO-013: Degraded-Mode Behavior & Status Reporting
- **Decision:** Degraded modes are explicitly surfaced in UI telemetry; downstream qualification cannot claim success while operating in degraded mode.
- **Consequences:** Absolute transparency regarding weakened guarantees.

### ADR-HIO-014: Handling of Unknown Operation Outcomes
- **Decision:** If an operation outcome is uncertain across a handoff, the domain is **frozen immediately**; speculative retries are prohibited until read-back or snapshot verification.
- **Consequences:** Prevents compounding state divergence.

### ADR-HIO-015: Recovery and Failback Prerequisites
- **Decision:** Failback to a primary strategy requires a 5.0-second soak test and full snapshot reconciliation.
- **Consequences:** Prevents rapid flapping between unstable primary and fallback instances.

### ADR-HIO-016: Resource Allocation and Queue Bounds
- **Decision:** Enforce hard quotas: 4 strategy instances max, 128 MiB aggregate memory, 1024 transition queue items, 2048 deduplication entries.
- **Consequences:** Prevents memory exhaustion attacks and host system starvation.

### ADR-HIO-017: Orchestration Audit Retention and Privacy
- **Decision:** Audit records redact player PII and raw memory pointers, preserving only canonical IDs, revisions, and operational hashes in JSON-LD format.
- **Consequences:** Complies with privacy standards while maintaining full post-incident auditability.

### ADR-HIO-018: Phase 9 Integration Qualification Evidence
- **Decision:** Qualification requires passing all 50 synthetic scenarios (H01–H50) in an isolated harness prior to production deployment.
- **Consequences:** Establishes rigorous evidence-backed quality gates for Phase 9 completion.
