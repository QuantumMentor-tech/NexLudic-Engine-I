# STATE SYNCHRONIZATION SYNTHETIC QA TEST PLAN

**Document Identifier:** `NLE-CAT-LSS-001`  
**Lifecycle Stage:** Authoritative Test Plan  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Synthetic Verification  
**Upstream Dependencies:** `NLE-SPC-LSS-001` through `NLE-CON-SOD-001` (Step 75)  
**Downstream Dependents:** Step 76 (Gameplay Orchestration), Step 77 (Observability)

---

## 1. Scope and Synthetic Testing Architecture

This test plan defines **54 formal synthetic test scenarios (S01 through S54)** covering all aspects of cross-game state synchronization, snapshot exchange, delta sequencing, continuity, freshness, ownership, recovery, and security.

### Test Execution Principles:
- **Zero Live Game Execution:** All tests run inside an isolated in-memory simulation test harness. No running game binaries, no live network sockets, no anti-cheat interference.
- **Deterministic Evaluation:** Inputs are synthetic JSON payloads with fixed seeds (`SEED = 0x554E4954`).
- **Status Reporting Standard:** All scenarios in this document are **SPECIFIED, NOT EXECUTED** pending runtime test harness implementation in subsequent phases.

---

## 2. Master Synthetic Test Matrix (Scenarios S01 through S54)

### Group 1: Snapshot Establishment (S01 – S08)
- **S01 (Valid initial snapshot):** Complete, authorized snapshot establishing valid baseline. Pre: Handshake complete. Input: Snapshot R=100. Expected: 12-stage validation passes; domain transitions to `SYNCHRONIZING`.
- **S02 (Partial snapshot):** Snapshot transfer ends before completeness reached. Expected: Timeout occurs; partial snapshot rejected; baseline remains unestablished.
- **S03 (Incorrect mapping version):** Snapshot references unavailable mapping URN `map:hero:v9.9.9`. Expected: Validation fails at Stage 5; error `ERR_SYNC_MAPPING_INCOMPATIBLE` logged.
- **S04 (Stale initial snapshot):** Snapshot timestamp exceeds maximum allowable age ($> 5.0$ s). Expected: Validation fails at Stage 11; error `ERR_SYNC_STALE_STATE` emitted.
- **S05 (Snapshot identity collision):** Two distinct snapshots reuse identical UUID. Expected: Second snapshot rejected as duplicate; warning logged.
- **S06 (Snapshot transfer interruption):** Connection drops while chunk 3 of 5 is in flight. Expected: Staging buffer discarded; domain resets to `NOT_SYNCHRONIZED`.
- **S07 (Non-atomic source observation):** Source advertises `BEST_EFFORT_SINGLE_FRAME` for high-precision physics domain. Expected: Refused promotion to authoritative baseline.
- **S08 (Unsupported state value):** Snapshot contains NaN in position vector. Expected: Layer 8 rejects payload with `ERR_SYNC_INVALID_SNAPSHOT`.

### Group 2: Delta Ordering and Application (S09 – S18)
- **S09 (Continuous ordered updates):** In-order deltas R=101, 102, 103 arrive. Expected: Revisions advance monotonically: $R_{app} = 101 \to 102 \to 103$.
- **S10 (Duplicate delta):** Delta R=102 delivered twice. Expected: Second instance identified in dedup window; dropped idempotently.
- **S11 (Out-of-order delta):** Delta R=103 arrives before R=102. Expected: R=103 placed in out-of-order queue; applied only after R=102 arrives.
- **S12 (Missing revision):** Stream jumps from R=101 to R=104 (missing 102, 103). Expected: Gap detected; `ERR_SYNC_REVISION_GAP` emitted; recovery triggered.
- **S13 (Expired-session delta):** Delta arrives with epoch 1 when active epoch is 2. Expected: Dropped immediately with `ERR_SYNC_STALE_EPOCH`.
- **S14 (Unknown baseline):** Delta references unknown snapshot cutoff R=50. Expected: Rejected with `ERR_SYNC_INVALID_DELTA`.
- **S15 (Invalid delta payload):** Delta contains string in integer field. Expected: Schema validation fails; delta dropped.
- **S16 (Application outcome unknown):** Target adapter cannot verify whether update committed. Expected: Status marked `VALIDATED_BUT_APPLICATION_UNCONFIRMED`.
- **S17 (Conflicting acknowledgments):** Target reports ack for R=105 while highest applied is R=103. Expected: Invariant violation logged; ack ignored.
- **S18 (Unsupported delta coalescing):** Coalescing attempted on `ENTITY_SPAWN`. Expected: Coalescing refused; spawn executed in strict order.

### Group 3: Freshness and Consistency (S19 – S26)
- **S19 (Connected but stale endpoint):** Transport healthy but source simulation stops emitting updates. Expected: Age exceeds 500ms; domain transitions to `STALE`.
- **S20 (Clock disagreement):** Source and target clocks differ by 500ms. Expected: Wall-clock disregarded; monotonic tick counters used for ordering.
- **S21 (Partial domain synchronization):** Parent chassis is `CURRENT`, child wheel is `STALE`. Expected: Composite health reported as `PARTIALLY_SYNCHRONIZED`.
- **S22 (Revision regression):** Source emits delta with R=99 after R=105 applied. Expected: Rejected as regression; audit alert emitted.
- **S23 (Divergent state):** Invariant check detects overlapping physical volumes. Expected: `ERR_SYNC_STATE_DIVERGENCE` logged; resync requested.
- **S24 (Unknown freshness):** Source clock metadata missing. Expected: Domain freshness classified as `UNKNOWN`.
- **S25 (Mapping update during synchronization):** Mapping URN bumped during active streaming. Expected: Incremental stream suspended; fresh snapshot requested.
- **S26 (Capability regression):** Endpoint drops write capability. Expected: Domain write permissions revoked; status transitions to `DEGRADED`.

### Group 4: Recovery and Resource Protection (S27 – S36)
- **S27 (Successful delta replay):** Small gap of 3 deltas recovered via replay buffer. Expected: Missing deltas applied in order; domain restored to `ACTIVE`.
- **S28 (Replay unavailable):** Source cannot provide missing deltas. Expected: Falls back to fresh snapshot acquisition.
- **S29 (Fresh snapshot recovery):** Clean snapshot acquired after buffer overflow. Expected: New baseline published; domain resumes streaming.
- **S30 (Recovery failure):** Fresh snapshot fails validation during recovery. Expected: Domain transitions to `FAILED`.
- **S31 (Queue overflow):** Delta ingestion queue exceeds 512 items. Expected: Overflow error emitted; oldest unapplied deltas dropped; resync triggered.
- **S32 (Backpressure activation):** Ingestion reaches 80% capacity. Expected: `SIGNAL_SYNC_SLOWDOWN` emitted to publisher.
- **S33 (Repeated invalid updates):** Endpoint emits 5 invalid deltas sequentially. Expected: Endpoint quarantined; domain halted.
- **S34 (Retry amplification):** Recovery fails repeatedly. Expected: Aborted after 3 attempts; retry storm prevented.
- **S35 (Concurrent recovery requests):** Multiple triggers initiate recovery simultaneously. Expected: Deduplicated into single recovery task.
- **S36 (Session epoch change during recovery):** Transport reconnects while recovery is executing. Expected: Stale recovery aborted; fresh session initialized.

### Group 5: Ownership, Isolation, and Security (S37 – S45)
- **S37 (Unauthorized writer):** Non-authoritative endpoint emits update. Expected: Dropped with `ERR_SYNC_UNAUTHORIZED_WRITER`.
- **S38 (Ambiguous ownership):** Domain registered without designated writer. Expected: Writes refused until ownership declared.
- **S39 (Concurrent writers):** Two endpoints write to same attribute. Expected: Both updates quarantined; conflict logged; no LWW winner chosen.
- **S40 (Cross-session leakage):** Delta from Session A presented to Session B. Expected: Session UUID mismatch; payload rejected.
- **S41 (Oversized snapshot):** Snapshot payload is 75 MiB (limit 64 MiB). Expected: Rejected at transport boundary.
- **S42 (Malformed nested payload):** Payload nested 10 levels deep. Expected: Rejected at depth check (limit 4).
- **S43 (Sensitive diagnostic data):** Corrupted payload contains credentials. Expected: Redacted in diagnostic error logs.
- **S44 (Mapping payload attempts execution):** Payload contains JavaScript expression. Expected: Evaluator refuses execution; error emitted.
- **S45 (Authority change):** Ownership transferred while updates in flight. Expected: In-flight updates from old owner rejected; new owner baseline required.

### Group 6: Interoperability and Observability (S46 – S54)
- **S46 (Source has no snapshot capability):** Source lacks snapshot API. Expected: Classified as `STREAM_ONLY_LOSSY`; requires external baseline.
- **S47 (Source has no reliable revision capability):** Source cannot emit sequence numbers. Expected: Classified as `UNSEQUENCED_BEST_EFFORT`.
- **S48 (Endpoints use different schema versions):** Endpoint A (v2.1) connects to Endpoint B (v2.0). Expected: Reconciled via Step 74 SMR or rejected if breaking.
- **S49 (Bridge restart):** Bridge restarts while games remain running. Expected: Epoch incremented; full resync required.
- **S50 (Recovery diagnostics):** Failed recovery preserves diagnostic record. Expected: Record inspectable via telemetry API.
- **S51 (Healthy transport with failed application):** Socket alive but game engine throws exception on apply. Expected: `ERR_SYNC_APPLICATION_UNKNOWN` logged.
- **S52 (Snapshot and delta baseline mismatch):** First delta expects R=100 but snapshot was R=95. Expected: Baseline mismatch detected; catch-up aborted.
- **S53 (Isolation between domains):** Inventory domain fails recovery while transform domain continues. Expected: Transform domain remains `ACTIVE`.
- **S54 (End-to-end qualification):** Protocol valid, state synchronized, but game mechanics incompatible. Expected: Engine reports `SYNC_VALID_BUT_GAME_PAIR_INCOMPATIBLE`.
