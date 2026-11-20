# LIVE CROSS-GAME STATE SYNCHRONIZATION SPECIFICATION

**Document Identifier:** `NLE-SPC-LSS-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Strict Sandboxed Execution  
**Upstream Dependencies:** `NLE-SPC-LGB-001` (Step 70), `NLE-SPC-EDR-001` (Step 71), `NLE-SPC-CPA-001` (Step 72), `NLE-SPC-MVR-001` (Step 73), `NLE-SPC-CSM-001` (Step 74)  
**Downstream Dependents:** Step 76 (Cross-Game Gameplay Orchestration), Step 77 (Bridge Observability & Diagnostics)

---

## 1. Executive Summary & Core Objective

The **Live Cross-Game State Synchronization Specification** defines the authoritative architecture, state models, session lifecycle, consistency contracts, and synchronization guarantees governing how NexLudic Engine exchanges, maintains, and reconciles state between two or more heterogeneous game runtimes across an active live interoperability bridge.

Heterogeneous games do not share memory layouts, tick rates, simulation models, state architectures, or physics engines. Direct point-to-point state replication without strict synchronization semantics inevitably leads to ghosting, teleportation, desynchronization loops, and engine deadlocks.

This specification establishes a robust, bounded, observable, and recoverable synchronization layer that operates on the fundamental engineering principle:
$$\textbf{NexLudic Engine must distinguish what it has received, what it has validated, what it has applied,}$$
$$\textbf{what it can prove is current, and what remains unknown.}$$
$$\textbf{Synchronization must never create the illusion of consistency where evidence does not support it.}$$

---

## 2. Inviolable Architectural Axioms & Principles

### Axiom 1: The Multi-Layer Integration Non-Equivalence Hierarchy
$$\text{Transport Connection} \neq \text{Protocol Agreement} \neq \text{Semantic Compatibility} \neq \text{State Consistency} \neq \text{Game-Pair Compatibility}$$
- A healthy TCP/IPC connection does not mean data is understood.
- Valid semantic mapping does not mean state is synchronized.
- Successful application of an update does not imply the overall game pair is fundamentally compatible.

### Axiom 2: Strict State Progression Disambiguation
$$\text{Received Update} \neq \text{Validated Update} \neq \text{Accepted Update} \neq \text{Applied Update} \neq \text{Acknowledged Update}$$
- These five lifecycle milestones **shall never** be collapsed into a single status. If a receiving adapter cannot verify that an update was applied inside the host game's simulation, it must report `VALIDATED_BUT_APPLICATION_UNCONFIRMED`.

### Axiom 3: Functional Decoupling from Upstream and Downstream Layers
$$\text{Message Transport (Step 73)} \neq \text{Semantic Mapping (Step 74)} \neq \text{State Synchronization (Step 75)} \neq \text{Gameplay Orchestration (Step 76)}$$
- **Step 73** provides envelope packaging, routing, and delivery guarantees.
- **Step 74** transforms individual payloads between source, canonical, and target schemas.
- **Step 75 (This Step)** tracks state baselines, manages snapshots, sequences deltas, monitors freshness, and detects divergence.
- **Step 76** enforces mashup rules, win/loss conditions, and cross-game mechanics.

### Axiom 4: Anti-Cheat & Runtime Boundary Inviolability (`BLK-SEC-01`, `DEC-002`)
- NexLudic Engine operates strictly in user-space at Medium Integrity Level (IL).
- State synchronization **shall never** employ DLL injection, thread hijacking, process memory scanning (`ReadProcessMemory`/`WriteProcessMemory`), kernel drivers, or anti-cheat tampering.
- All state flows exclusively through authorized, authenticated bridge communication channels (local loopback sockets or named pipes).

### Axiom 5: Absolute Honesty of Unknown and Inconsistent State
- If a source engine cannot provide an atomic snapshot, the engine **shall not** fabricate one.
- If a delta sequence gap cannot be resolved, the domain **shall not** silently guess missing values.
- Unknown states are explicitly represented and surfaced to consumers and audit logs.

---

## 3. Authoritative Synchronization Terminology

To prevent semantic conflation across heterogeneous engineering teams, the following 15 terms are authoritatively defined:

```
+---------------------------+----------------------------------------------------------------------------------------------------+
| Term                      | Authoritative Engineering Definition                                                               |
+---------------------------+----------------------------------------------------------------------------------------------------+
| 1. Game-Local State       | State internally maintained within a specific game's engine memory or simulation world.            |
| 2. Exposed State          | The authorized subset of game-local state that an integration adapter exposes to the bridge.       |
| 3. Canonical Representation| The standardized, SI-metric, Right-Handed Y-Up data model used internally by NexLudic Engine.       |
| 4. Mapped State           | State transformed into target-engine representation via a Step 74 Semantic Mapping Record.         |
| 5. Published Update       | An update serialized and emitted by an authorized source endpoint into the bridge pipeline.        |
| 6. Received Update        | An update accepted by the bridge transport layer and delivered to the synchronization coordinator. |
| 7. Validated Update       | An update that has cleared all 14 layers of schema, type, range, and security validation.          |
| 8. Applied Update         | An update verified to have taken effect within the receiving game's simulation world.              |
| 9. Observed State         | The most recently validated and recorded state known to the bridge synchronization coordinator.   |
| 10. Authoritative State   | State designated as the singular source of truth for a domain under an explicit ownership contract.|
| 11. Derived State         | State computed deterministically from other state attributes rather than independently emitted.    |
| 12. Stale State           | Valid state whose age exceeds the declared freshness limit of its synchronization domain.          |
| 13. Inconsistent State    | State or metadata that violates declared structural, temporal, or behavioral invariants.          |
| 14. Synchronization Gap   | An unrecovered missing sequence number or revision between observed updates in an active stream.  |
| 15. Resynchronization     | The formal recovery protocol for restoring an endpoint or domain to a verified valid baseline.     |
+---------------------------+----------------------------------------------------------------------------------------------------+
```

---

## 4. Logical Synchronization Session Model

A **Synchronization Session** represents a bounded, stateful agreement between two or more authenticated endpoints to exchange state within designated domains under an active `NegotiatedSessionContract` (Step 72).

### 4.1 Session Identifiers and Epochs
- **Session Identifier (`sync_session_id`):** A canonical UUIDv4 identifying the continuous synchronization context.
- **Session Epoch (`session_epoch`):** A monotonically increasing 64-bit integer (`uint64`). The epoch increments whenever a session reconnects, resets, or recovers from a major transport break.
- **Stale Epoch Rejection:** Any update or snapshot bearing an epoch $E_{msg} < E_{active}$ is immediately dropped as a stale remnant from a prior incarnation (`ERR_SYNC_STALE_EPOCH`).

### 4.2 Synchronization Session Lifecycle FSM

```
                   +-----------------------------------+
                   |         NOT_SYNCHRONIZED          |
                   +-----------------------------------+
                                     |
                                     | Handshake & Capability Agreement (Step 72)
                                     v
                   +-----------------------------------+
                   |       ESTABLISHING_BASELINE       | <--------------------+
                   +-----------------------------------+                      |
                                     |                                        |
                                     | Snapshot Transfer & Validation (Sec 5) |
                                     v                                        |
                   +-----------------------------------+                      |
                   |           SYNCHRONIZING           |                      |
                   +-----------------------------------+                      |
                                     |                                        |
                                     | Catch-up Deltas Applied (Sec 7)        |
                                     v                                        |
                   +-----------------------------------+                      |
                   |              ACTIVE               |                      |
                   +-----------------------------------+                      |
                        /             |             \                         |
         Freshness Lag /   Gap / OOS  |              \ Major Failure /        |
                      v    Detected   v               \ Divergence            |
          +---------------+   +----------------+       v                      |
          |    DEGRADED   |   | RESYNC_REQUIRED| ---> +---------------------+ |
          +---------------+   +----------------+      |     RECOVERING      | -+
                 |                    ^               +---------------------+
                 +--------------------+                          |
                               |                                 | Recovery Failed
                               v                                 v
                   +-----------------------------------+  +---------------------+
                   |             SUSPENDED             |  |       FAILED        |
                   +-----------------------------------+  +---------------------+
                                     |                               |
                                     +---------------+---------------+
                                                     |
                                                     v
                                          +---------------------+
                                          |     TERMINATED      |
                                          +---------------------+
```

### State Definitions & Operational Rules:
1. **NOT_SYNCHRONIZED:** Initial state. No state exchange permitted.
2. **ESTABLISHING_BASELINE:** Initial snapshot transfer in progress. Streaming deltas are queued in a bounded buffer.
3. **SYNCHRONIZING:** Snapshot validated. Buffered deltas between snapshot cutoff and live head are being applied.
4. **ACTIVE:** Real-time continuous synchronization. In-order deltas applied as they arrive.
5. **DEGRADED:** Updates are arriving but violate freshness limits or experience backpressure. Application continues with warnings.
6. **RESYNC_REQUIRED:** Unrecoverable revision gap or packet loss detected. Normal delta processing halts; recovery triggered.
7. **RECOVERING:** Staged recovery procedure executing (replay or fresh baseline acquisition).
8. **SUSPENDED:** Administratively paused by user or host game pause event. State frozen; buffers retained.
9. **FAILED:** Recovery exhausted or security invariant violated. Synchronization halted permanently for this session.
10. **TERMINATED:** Clean shutdown; resources deallocated and tombstone records archived.

---

## 5. Synchronization Domains

State is not synchronized as a monolithic blob. It is partitioned into discrete **Synchronization Domains**:

```
+----------------------------------------------------------------------------------------------------+
|                                    SYNCHRONIZATION DOMAIN SCHEMA                                   |
+----------------------------------------------------------------------------------------------------+
| - domain_id: "domain:ue5_hero->unity_proxy:spatial_kinematics"                                     |
| - source_endpoint_id: "ue5-live-provider:title_alpha:bld_01:inst_01"                               |
| - target_endpoint_id: "unity-live-provider:title_beta:bld_02:inst_02"                              |
| - state_owner: "ue5-live-provider:title_alpha:bld_01:inst_01" (Strict Authoritative Single Writer)  |
| - direction: UNIDIRECTIONAL_OUTBOUND                                                               |
| - concept_uri: "nexludic://concepts/ue5_vehicle/suspension_telemetry"                              |
| - mapping_record_urn: "map:ue5_vehicle->unity_vehicle:v1.2.0"                                      |
| - schema_version: "2.1.0"                                                                          |
| - consistency_level: BOUNDED_STALENESS                                                             |
| - freshness_limit_ms: 250                                                                          |
| - supported_operations: ["SET_VALUE", "DELTA_PATCH"]                                               |
| - failure_policy: TRANSITION_TO_RESYNC_REQUIRED                                                    |
+----------------------------------------------------------------------------------------------------+
```

### Permitted Synchronization Directions:
- `UNIDIRECTIONAL_SOURCE_TO_TARGET`: Source owns and pushes state; target is read-only consumer.
- `UNIDIRECTIONAL_TARGET_TO_SOURCE`: Target owns and pushes state; source is read-only consumer.
- `PARTITIONED_SPLIT_OWNERSHIP`: Endpoint A owns sub-domain 1; Endpoint B owns sub-domain 2.
- **Strict Prohibition:** Unpartitioned bidirectional writes to the exact same property without external arbitration are strictly forbidden.

---

## 6. Synchronization Guarantees and Non-Guarantees

### 6.1 Supported Guarantees
1. **Per-Domain Monotonic Ordering:** Within a single domain and stream, updates are applied strictly in monotonic source revision order.
2. **Gap Detection Guarantee:** Any missing revision in a sequenced stream is detected prior to subsequent delta application.
3. **Stale Epoch Isolation:** Updates from older session epochs are guaranteed to be rejected without state mutation.
4. **Bounded Staleness:** Consumers can verify whether observed state is within $\Delta t_{freshness}$ of source time.
5. **Fail-Closed Baseline Integrity:** A snapshot is never published as an active baseline until it has passed all structural, semantic, and cryptographic validation gates.

### 6.2 Explicit Non-Guarantees (What the Bridge Does NOT Promise)
1. **NO Global Multi-Game Linearizability:** The engine does not make two independent game loops execute as a single atomic distributed computer.
2. **NO Lockstep Determinism:** Physics simulations running at different tick rates (e.g. 60Hz PhysX vs 120Hz Chaos) will diverge over time without continuous authoritative state overrides.
3. **NO Universal Two-Phase Commit:** A state change applied in Game A cannot be universally guaranteed to atomically commit in Game B simultaneously.
4. **NO Unobservable Application Proof:** If a receiving game adapter cannot confirm that an update was committed into the game world, the bridge guarantees only that the update was delivered and validated, not applied.
