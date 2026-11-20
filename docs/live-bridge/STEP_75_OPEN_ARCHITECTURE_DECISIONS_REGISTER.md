# STEP 75 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-075`  
**Lifecycle Stage:** Authoritative Architecture Register  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Architectural Governance  
**Upstream Dependencies:** `NLE-SPC-LSS-001` through `NLE-CON-SOD-001`  
**Downstream Dependents:** Phase 9 Baseline Architecture

---

## 1. Summary of Architectural Decision Records (ADRs)

| ADR Identifier | Mandatory Decision Area | Status | Impact Area |
| :--- | :--- | :---: | :--- |
| `ADR-SSR-001` | Supported Synchronization Consistency Guarantees | **ACCEPTED** | Consistency Model |
| `ADR-SSR-002` | Required Source Capabilities for Trustworthy Snapshots | **ACCEPTED** | Source Ingestion |
| `ADR-SSR-003` | Delta Identity and Revision Semantics | **ACCEPTED** | Delta Sequencing |
| `ADR-SSR-004` | Snapshot-to-Delta Continuity Strategy | **ACCEPTED** | State Transitions |
| `ADR-SSR-005` | State Freshness Evaluation Policy | **ACCEPTED** | Temporal Monitoring |
| `ADR-SSR-006` | Session Epoch and Revision Persistence Across Restarts | **ACCEPTED** | Session Lifecycle |
| `ADR-SSR-007` | Domain Ownership and Authority Model | **ACCEPTED** | Authority & Writes |
| `ADR-SSR-008` | Behavior When Target Application Outcome is Unknown | **ACCEPTED** | Application Safety |
| `ADR-SSR-009` | Recovery Choices When Replay is Unavailable | **ACCEPTED** | Resynchronization |
| `ADR-SSR-010` | Safe Treatment of Dependent Domains | **ACCEPTED** | Cross-Domain Safety |
| `ADR-SSR-011` | Buffer Allocation and Resource-Limit Policies | **ACCEPTED** | Resource Protection |
| `ADR-SSR-012` | Supported Handling of Multi-Writer Domains | **ACCEPTED** | Concurrency Control |
| `ADR-SSR-013` | Checksum and Integrity Verification Placement | **ACCEPTED** | Cryptographic Security |
| `ADR-SSR-014` | Compatibility Behavior During Mapping or Schema Changes | **ACCEPTED** | Version Governance |

---

## 2. Detailed Architectural Decision Records

### ADR-SSR-001: Supported Synchronization Consistency Guarantees
- **Decision:** NexLudic Engine provides **Per-Domain Monotonic Ordering** and **Bounded Staleness** for compatible domains. It does NOT promise global linearizability or lockstep multi-game determinism.
- **Consequences:** Eliminates unrealistic promises; allows asynchronous game loops to interoperate safely without deadlocks.

### ADR-SSR-002: Required Source Capabilities for Trustworthy Snapshots
- **Decision:** A snapshot is classified as authoritative only if the source provides `ATOMIC_SIMULATION_PAUSE` or `REVISION_BOUND_TRANSACTION`. `BEST_EFFORT_SINGLE_FRAME` is accepted only for non-authoritative display telemetry.
- **Consequences:** Prevents unverified observations from corrupting authoritative baselines.

### ADR-SSR-003: Delta Identity and Revision Semantics
- **Decision:** Delta sequence tracking is scoped strictly **per-domain and per-stream**. Global monotonic sequence counters across unrelated domains are rejected.
- **Consequences:** Prevents head-of-line blocking between independent subsystems (e.g. inventory vs transforms).

### ADR-SSR-004: Snapshot-to-Delta Continuity Strategy
- **Decision:** Adopt the **Staging Catch-Up Buffer with Cutoff Revision Alignment**. Deltas arriving during snapshot transfer are staged and drained in sequence.
- **Consequences:** Guarantees zero unobserved update gaps between snapshot creation and live streaming.

### ADR-SSR-005: State Freshness Evaluation Policy
- **Decision:** Freshness is evaluated using monotonic tick intervals ($\Delta t \le 250\text{ ms}$ baseline), completely decoupled from transport liveness.
- **Consequences:** Detects frozen or unresponsive game simulations even when network sockets remain open.

### ADR-SSR-006: Session Epoch and Revision Persistence Across Restarts
- **Decision:** Reconnection or bridge restart forces a monotonic **Session Epoch increment** and complete purge of cached unapplied deltas.
- **Consequences:** Old delayed messages from prior sessions are mathematically guaranteed to be dropped.

### ADR-SSR-007: Domain Ownership and Authority Model
- **Decision:** Default to **Single Authoritative Writer** per domain. Unpartitioned simultaneous multi-writer state is strictly prohibited.
- **Consequences:** Eliminates split-brain state divergence across game pairs.

### ADR-SSR-008: Behavior When Target Application Outcome is Unknown
- **Decision:** If the target adapter cannot verify that an update took effect, the watermark is marked `VALIDATED_BUT_APPLICATION_UNCONFIRMED` and $R_{app}$ does not advance.
- **Consequences:** Concealed state divergence is prevented; downstream systems are alerted to uncertainty.

### ADR-SSR-009: Recovery Choices When Replay is Unavailable
- **Decision:** When missing deltas cannot be replayed from the source buffer, the engine falls back to a **Fresh Snapshot Acquisition**. If snapshots are unsupported, the domain is suspended.
- **Consequences:** Guarantees that unrecoverable gaps never result in guessing or data fabrication.

### ADR-SSR-010: Safe Treatment of Dependent Domains
- **Decision:** Dependent domains permit a maximum revision skew of $\Delta R \le 5$. If exceeded, dependent child deltas are held in interpolation buffers until the parent catches up.
- **Consequences:** Prevents visual tearing (e.g. car wheels detaching from chassis).

### ADR-SSR-011: Buffer Allocation and Resource-Limit Policies
- **Decision:** Enforce hard bounds: Ingestion queue = 512 deltas (8 MiB); Staging buffer = 1024 deltas (16 MiB); Max snapshot = 64 MiB.
- **Consequences:** Prevents memory exhaustion attacks and runaway buffer bloat.

### ADR-SSR-012: Supported Handling of Multi-Writer Domains
- **Decision:** Multi-writer is permitted only via **Explicit Cryptographic Token Passing** or **Partitioned Attribute Sets**. Simultaneous uncoordinated writes are quarantined without choosing a winner.
- **Consequences:** Strict anti-split-brain enforcement without fabricating heuristic resolution.

### ADR-SSR-013: Checksum and Integrity Verification Placement
- **Decision:** Integrity verification occurs at the entry gate: SHA-256 for payloads, AES-GCM-256 HMAC for opaque vault tokens.
- **Consequences:** Tampered or corrupted state is rejected before reaching the normalization engine.

### ADR-SSR-014: Compatibility Behavior During Mapping or Schema Changes
- **Decision:** Major SemVer increments in mapping or schema invalidate the active baseline, triggering `RESYNC_REQUIRED`. Minor backward-compatible bumps hot-reload without interrupting live streams.
- **Consequences:** Prevents silent schema corruption while permitting non-breaking evolution.
