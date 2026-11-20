# LIVE BRIDGE SYNTHETIC QA TEST PLAN

**Document ID:** `NLE-CAT-LBA-001`  
**Classification:** Quality Assurance & Synthetic Verification Test Catalog  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 70)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-MOD-CRB-001`, `NLE-CON-DEC-001`, `NLE-REQ-TIC-001`, `NLE-SPC-PME-001`, `NLE-POL-SLC-001`, `NLE-POL-GPE-001`, `NLE-CON-HIC-001`, `NLE-POL-SRS-001`, `NLE-REQ-FDR-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), Implementation Verification Suites  

---

## 1. Test Architecture & Synthetic Execution Strategy

In strict adherence to the **Design-Only Milestone Constraint** (`P8-C02`), zero runtime code or active game process hooks are deployed during Step 70. 

Verification is achieved through a comprehensive **Synthetic QA Test Catalog** containing **36 rigorously designed scenarios (Scenarios A through AJ)**. Each scenario specifies precise input fixtures, channel conditions, sequence triggers, mathematical assertions, and expected state transitions across synthetic mock adapters and channel emulators.

---

## 2. Test Scenario Matrix Overview

```
+-------+------------------------------------------+--------------------+-----------------------+
| Group | Domain Area                              | Scenarios          | Coverage Scope        |
+-------+------------------------------------------+--------------------+-----------------------+
| 1     | Transport & Wire Framing                 | Scenarios A - F    | 6 Scenarios           |
| 2     | Handshake & Identity Attestation         | Scenarios G - L    | 6 Scenarios           |
| 3     | Capability Negotiation                   | Scenarios M - R    | 6 Scenarios           |
| 4     | Message Routing & Flow Control           | Scenarios S - X    | 6 Scenarios           |
| 5     | Hybrid Integration & Cross-Subsystems    | Scenarios Y - AD   | 6 Scenarios           |
| 6     | Stress, Fault Recovery & Boundary Safety | Scenarios AE - AJ  | 6 Scenarios           |
+-------+------------------------------------------+--------------------+-----------------------+
| TOTAL | Comprehensive Test Suite                 | 36 Scenarios       | 100% Phase 9 Scope    |
+-------+------------------------------------------+--------------------+-----------------------+
```

---

## 3. Detailed Scenario Specifications (Scenarios A through AJ)

### Group 1: Transport & Wire Framing (Scenarios A – F)

#### Scenario A: Standard Length-Prefixed Frame Demarcation
- **Objective:** Verify proper frame parsing across fragmented byte reads.
- **Fixture:** Stream split into 3 fragments: `[Magic (4B) + Length (4B)]`, `[Payload Chunk 1 (100B)]`, `[Payload Chunk 2 (200B)]`.
- **Assertion:** Envelope parser yields exactly 1 complete frame matching length 300B; zero premature parsing.

#### Scenario B: Oversized Frame Transport Rejection
- **Objective:** Verify immediate rejection when frame length header exceeds maximum allowed frame ceiling.
- **Fixture:** Frame header specifying length `17,000,000` bytes (ceiling: 16 MB).
- **Assertion:** Transport immediately aborts frame, closes connection, and records `ERR_PME_002`.

#### Scenario C: Invalid Magic Bytes Stream Abort
- **Objective:** Verify transport aborts connection when stream lacks `0x4E4C4542` magic bytes.
- **Fixture:** Byte stream starting with `0xDEADBEEF`.
- **Assertion:** Transport connection immediately severed; zero bytes enqueued to message router; `ERR_PME_001` logged.

#### Scenario D: Named Pipe DACL Security Validation
- **Objective:** Verify Named Pipe rejects cross-user connection attempts on Windows.
- **Fixture:** Mock client impersonating secondary local user SID attempting `CreateFileW` on bridge pipe.
- **Assertion:** OS returns `ERROR_ACCESS_DENIED`; pipe listener unaffected; zero bridge state mutation.

#### Scenario E: Loopback TCP Interface Binding Conformance
- **Objective:** Verify TCP transport listener strictly refuses non-loopback bindings.
- **Fixture:** Configuration attempting to bind to `0.0.0.0:8080` or `192.168.1.50:8080`.
- **Assertion:** Socket initialization throws `InvalidOperationException`; listener binds strictly to `127.0.0.1`.

#### Scenario F: Inbound Queue Backpressure Saturation
- **Objective:** Verify transport pauses read operations when queue exceeds 75% high watermark.
- **Fixture:** Fast sender emitting 1000 frames/sec; slow mock consumer processing 50 frames/sec.
- **Assertion:** Transport read loop halts when buffer hits 750 frames; sender socket buffer fills; resumes at 250 frames.

---

### Group 2: Handshake & Identity Attestation (Scenarios G – L)

#### Scenario G: Standard Handshake & Session Token Authentication
- **Objective:** Verify successful connection, identity attestation, and token verification.
- **Fixture:** Valid `HANDSHAKE_INIT` containing matching 256-bit token and canonical `EndpointIdentifier`.
- **Assertion:** Engine replies with `HANDSHAKE_ACK`; state transitions from `PROBING` to `AUTHENTICATING` to `NEGOTIATING`.

#### Scenario H: Ephemeral Token Mismatch Rejection
- **Objective:** Verify immediate connection termination on incorrect handshake token.
- **Fixture:** `HANDSHAKE_INIT` containing corrupt or stale 256-bit token.
- **Assertion:** Engine drops connection within 50ms; session invalidated; security event logged in `TDJ-08`.

#### Scenario I: Handshake Timeout Enforcement
- **Objective:** Verify unauthenticated connection is terminated after 2000ms idle probe window.
- **Fixture:** Transport connection established, but client sends zero bytes for 2001ms.
- **Assertion:** Transport closed by coordinator; state resets to `DISCONNECTED`; resources reclaimed.

#### Scenario J: Canonical Endpoint ID Syntax Validation
- **Objective:** Verify rejection of non-conforming endpoint identifier strings.
- **Fixture:** Endpoint ID containing invalid characters: `Valve/Half Life 2:Build #4!`.
- **Assertion:** Handshake rejected with `ERR_INVALID_ENDPOINT_ID`; regex constraint enforced.

#### Scenario K: Simultaneous Duplicate Instance Disambiguation
- **Objective:** Verify two concurrent running instances of same game title receive distinct sessions.
- **Fixture:** Two connections with identical `provider:title_id:build_hash`, but differing `instance_id`.
- **Assertion:** Both sessions created concurrently with distinct `session_id` and isolated pipe endpoints.

#### Scenario L: Unsolicited Pre-Handshake Message Rejection
- **Objective:** Verify data events arriving prior to handshake completion are dropped.
- **Fixture:** Client sends `DATA_EVENT` immediately upon socket connection without sending `HANDSHAKE_INIT`.
- **Assertion:** Message dropped; connection severed immediately; `ERR_ILLEGAL_STATE` recorded.

---

### Group 3: Capability Negotiation (Scenarios M – R)

#### Scenario M: Successful Multi-Topic Capability Intersection
- **Objective:** Verify computation of shared capabilities between two symmetric manifests.
- **Fixture:** Manifest A offers `{spatial: v1, inventory: v1}`; Manifest B offers `{spatial: v1, inventory: v1, chat: v2}`.
- **Assertion:** `NegotiatedSessionContract` contains exactly `{spatial: v1, inventory: v1}`; chat omitted; state -> `ACTIVE`.

#### Scenario N: Schema Version Discrepancy Rejection
- **Objective:** Verify rejection when endpoints offer non-overlapping schema major versions.
- **Fixture:** Endpoint A offers `inventory: 2.0.0`; Endpoint B offers `inventory: 1.5.0`.
- **Assertion:** Capability negotiation excludes `inventory`; if no other shared capabilities exist, session terminated.

#### Scenario O: Directional Mismatch Resolution
- **Objective:** Verify handling of incompatible directionalities (both endpoints declare `OUTBOUND`).
- **Fixture:** Endpoint A offers `telemetry.spatial (OUTBOUND)`; Endpoint B offers `telemetry.spatial (OUTBOUND)`.
- **Assertion:** Capability excluded from contract; diagnostic warning `WARN_DIRECTION_CONFLICT` emitted.

#### Scenario P: Rate Limit Clamping to Conservative Minimum
- **Objective:** Verify negotiated rate limit clamps to lowest common denominator.
- **Fixture:** Endpoint A requests 120 msgs/sec; Endpoint B requests 30 msgs/sec.
- **Assertion:** Active contract clamps `rate_limit_per_sec` to `30`.

#### Scenario Q: Missing Phase 8 Prerequisite Capability Exclusion
- **Objective:** Verify capability requiring unverified Phase 8 package is rejected during negotiation.
- **Fixture:** Capability declares `requires_content_package: true`, but target package is in `UNVERIFIED` state.
- **Assertion:** Negotiation excludes capability; `CAPABILITY_REJECT` emitted with code `ERR_HYBRID_PRECONDITION_FAILED`.

#### Scenario R: Dynamic Mid-Session Capability Extension
- **Objective:** Verify runtime addition of newly unlocked capabilities without tearing down session.
- **Fixture:** Active session receives valid `CAPABILITY_OFFER` adding `audio.sync: v1`.
- **Assertion:** Coordinator re-evaluates contract, issues updated `CAPABILITY_ACCEPT`, routes new topic seamlessly.

---

### Group 4: Message Routing & Flow Control (Scenarios S – X)

#### Scenario S: Valid Data Event Routing & Payload Delivery
- **Objective:** Verify end-to-end routing of valid `DATA_EVENT` from Endpoint A to Endpoint B.
- **Fixture:** Conforming envelope with valid sequence number, valid SHA-256 digest, and authorized topic.
- **Assertion:** Endpoint B receives identical payload; digest matches; sequence counter advances.

#### Scenario T: Correlated RPC Command-Response Dispatch
- **Objective:** Verify `COMMAND_REQUEST` accurately pairs with `COMMAND_RESPONSE` via correlation ID.
- **Fixture:** A sends `COMMAND_REQUEST` with `correlation_id: 0x1234`; B replies with `COMMAND_RESPONSE` `0x1234`.
- **Assertion:** Router matches correlation ID, fulfills pending async completion task in initiator within timeout.

#### Scenario U: Sequence Regression & Replay Attack Defense
- **Objective:** Verify out-of-order or duplicate sequence numbers are dropped.
- **Fixture:** Session at sequence 105; sender dispatches duplicate frame with sequence 104.
- **Assertion:** Router drops frame; emits `ERR_PME_004`; state unaffected.

#### Scenario V: Token-Bucket Rate Limiter Overflow
- **Objective:** Verify frames exceeding burst capacity are dropped or throttled.
- **Fixture:** Capability capped at 10 msgs/sec; sender emits 25 frames in a 100ms burst.
- **Assertion:** First 10 frames pass; subsequent 15 frames receive `ERR_PME_007` rate limit rejection.

#### Scenario W: Payload SHA-256 Digest Tamper Detection
- **Objective:** Verify envelope is rejected if payload bytes do not match header digest.
- **Fixture:** Payload modified in-transit by 1 bit, altering hash.
- **Assertion:** Router detects digest mismatch; drops frame; emits `ERR_PME_003`; logs security alert.

#### Scenario X: Clock Skew Beyond Window Rejection
- **Objective:** Verify messages with timestamps outside allowed ±5000ms drift are rejected.
- **Fixture:** Envelope timestamp set to 10 minutes in the past.
- **Assertion:** Frame dropped; `ERR_PME_005` returned to sender.

---

### Group 5: Hybrid Integration & Cross-Subsystem Coupling (Scenarios Y – AD)

#### Scenario Y: Hybrid Session Activation with Verified Phase 8 Package
- **Objective:** Verify full bridge activation when Phase 8 package is verified.
- **Fixture:** Package `pkg_adapter_v1` verified with matching SHA-256 digest; bridge requests dependent capability.
- **Assertion:** Hybrid Arbiter approves binding; session transitions to `ACTIVE`.

#### Scenario Z: Bridge Blocked on Unapplied / Missing Package
- **Objective:** Verify bridge refuses connection if Phase 8 package was never applied.
- **Fixture:** Target directory lacks required Phase 8 mod files.
- **Assertion:** Arbiter blocks session; UI alerted to missing content mod prerequisite.

#### Scenario AA: Runtime Target Drift Suspension
- **Objective:** Verify bridge immediately suspends when host game directory files are altered externally.
- **Fixture:** Active session; filesystem watcher detects unauthorized change in host game folder.
- **Assertion:** Session transitions immediately to `SUSPENDED`; message routing halts; reconciliation required.

#### Scenario AB: Bridge Disconnection Preserves Phase 8 Mod Files
- **Objective:** Verify abrupt socket disconnection NEVER triggers filesystem rollback.
- **Fixture:** Live bridge TCP socket forcefully killed via `RST`.
- **Assertion:** Bridge session transitions to `TERMINATED`; Phase 8 files remain untouched and `VERIFIED`.

#### Scenario AC: Phase 8 Package Rollback Revokes Active Bridge Capability
- **Objective:** Verify user-initiated rollback in Phase 8 terminates dependent live bridge sessions.
- **Fixture:** User triggers Phase 8 rollback of `pkg_adapter_v1` while bridge session is active.
- **Assertion:** Arbiter revokes capability; coordinator emits `SESSION_TERMINATE` and closes bridge socket.

#### Scenario AD: Dual Independent Authorization Verification
- **Objective:** Verify granting Phase 8 authorization does NOT auto-grant Phase 9 live bridge authority.
- **Fixture:** User approves Phase 8 plan; engine attempts live bridge activation without Phase 9 prompt.
- **Assertion:** Bridge activation fails with `ERR_UNAUTHORIZED_LIVE_BRIDGE`; explicit prompt enforced.

---

### Group 6: Stress, Fault Recovery & Boundary Safety (Scenarios AE – AJ)

#### Scenario AE: Host Game Crash Detection via Heartbeat Timeout
- **Objective:** Verify engine detects game CTD (Crash-to-Desktop) within 3000ms.
- **Fixture:** Active session; mock game process terminated abruptly without closing socket.
- **Assertion:** Coordinator misses 3 heartbeats (1s, 2s, 3s); marks session `TERMINATED`; reclaims handles.

#### Scenario AF: Rapid Reconnect Storm Circuit Breaker
- **Objective:** Verify circuit breaker trips after 5 rapid consecutive connection failures.
- **Fixture:** Broken client connects and disconnects every 20ms for 5 iterations.
- **Assertion:** Circuit breaker transitions to `OPEN`; rejects subsequent connection attempts for 15s.

#### Scenario AG: 24-Hour Soak Test Memory Stability
- **Objective:** Verify zero memory growth across 1,000,000 synthetic message frames.
- **Fixture:** Synthetic harness streaming 1M frames with ArrayPool buffer recycling.
- **Assertion:** Process private working set memory delta < 1 MB across entire run; zero unmanaged leaks.

#### Scenario AH: Non-Invasive Anti-Cheat Compliance Audit
- **Objective:** Verify bridge execution makes zero prohibited Win32 memory or thread calls.
- **Fixture:** Static and dynamic API hook tracer monitoring `OpenProcess`, `WriteProcessMemory`, `CreateRemoteThread`.
- **Assertion:** Exactly 0 prohibited calls recorded; 100% compliance with `BLK-SEC-01`.

#### Scenario AI: Windows Medium Integrity Level Enforcement Audit
- **Objective:** Verify process token operates at Medium IL and refuses elevation.
- **Fixture:** Token integrity query on running bridge coordinator process.
- **Assertion:** Process token integrity level equals `SECURITY_MANDATORY_MEDIUM_RID` (0x2000); UAC not invoked.

#### Scenario AJ: Multi-Endpoint Cross-Game Synchronization
- **Objective:** Verify simultaneous 3-way synchronization across distinct games without race conditions.
- **Fixture:** Game A, Game B, and Companion App all connected; concurrent state updates routed through coordinator.
- **Assertion:** All endpoints receive consistent, serialized state snapshots; zero deadlocks; latency < 5ms.
