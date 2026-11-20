# LIVE BRIDGE CONNECTION & NEGOTIATION SYNTHETIC QA TEST PLAN

**Document ID:** `NLE-CAT-CPN-001`  
**Classification:** Quality Assurance & Synthetic Test Catalog  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-CPN-001` through `NLE-POL-TCR-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), Test Automation Framework  

---

## 1. Testing Strategy & Synthetic Execution Boundary

In accordance with the **Design-Only Milestone Constraint (`P8-C02`)**, zero live game processes are hooked, zero network probes are executed, and zero memory injection APIs are called.

All verification is achieved via **Deterministic Synthetic Test Fixtures**, exercising preflight validators, token exchange handshakes, protocol negotiators, capability agreement engines, and admission gates against simulated mock endpoints and in-memory IPC channels.

---

## 2. Test Scenario Matrix (Scenarios A through AL — 38 Scenarios)

```
+-------+------------------------------------------+--------------------+---------------------+
| Group | Domain Area                              | Scenarios          | Scenario Count      |
+-------+------------------------------------------+--------------------+---------------------+
| 1     | Preflight & Transport Setup              | Scenarios A - F    | 6 Scenarios         |
| 2     | Peer Authentication & Trust Binding      | Scenarios G - L    | 6 Scenarios         |
| 3     | Protocol Negotiation & Version Selection | Scenarios M - T    | 8 Scenarios         |
| 4     | Capability Agreement & Authorization     | Scenarios U - Z    | 6 Scenarios         |
| 5     | Lifecycle, Replay & Failure Recovery     | Scenarios AA - AF  | 6 Scenarios         |
| 6     | Hybrid Integration & Security Guardrails | Scenarios AG - AL  | 6 Scenarios         |
+-------+------------------------------------------+--------------------+---------------------+
| TOTAL | Comprehensive Test Suite                 | Scenarios A - AL   | 38 Test Scenarios   |
+-------+------------------------------------------+--------------------+---------------------+
```

---

## 3. Detailed Synthetic Scenario Specifications

### Group 1: Preflight & Transport Setup (Scenarios A – F)

#### Scenario A: Valid Connection Request Preflight Success
- **Objective:** Verify preflight passes when both registered participants satisfy all validation checks.
- **Fixture:** Both endpoints `REGISTERED_VERIFIED`, revisions match, target game build hashes confirmed.
- **Assertion:** `pfr-001` generated with status `PREFLIGHT_PASSED`; transport factory allocates Named Pipes.

#### Scenario B: Stale Endpoint Registration Preflight Block
- **Objective:** Verify preflight blocks connection if an endpoint registration is stale or unverified.
- **Fixture:** Endpoint A registration status is `REGISTERED_UNVERIFIED` (pending identity proofs).
- **Assertion:** Preflight fails with `ERR_PRE_ENDPOINT_UNVERIFIED`; zero transport listeners allocated.

#### Scenario C: Revoked Endpoint Connection Rejection
- **Objective:** Verify immediate rejection if either participant has been revoked.
- **Fixture:** Endpoint B registration state is `REVOKED`.
- **Assertion:** Preflight fails with `ERR_PRE_ENDPOINT_INACTIVE`; rejection logged to audit journal.

#### Scenario D: Contradictory Target Game Build Context
- **Objective:** Verify rejection when request specifies build hash differing from registered host binary.
- **Fixture:** Request specifies build `1122334455`, but verified executable on disk has digest `99b1c43f01`.
- **Assertion:** Preflight fails with `ERR_PRE_BUILD_HASH_STALE`; request aborted.

#### Scenario E: Unsupported Transport Profile Request
- **Objective:** Verify error handling when request demands an unavailable transport profile.
- **Fixture:** Request on Windows desktop environment specifies `transport_preference: "UNIX_DOMAIN_SOCKET"`.
- **Assertion:** Preflight fails with `ERR_PRE_TRANSPORT_UNAVAILABLE`; engine refuses fallback without explicit policy.

#### Scenario F: Transport Connected but Handshake Not Initiated
- **Objective:** Verify timeout and cleanup if client connects to pipe but fails to send `HANDSHAKE_INIT`.
- **Fixture:** Mock client establishes physical pipe connection, then remains silent for 501ms.
- **Assertion:** Auth timer fires; pipe reset (`SO_LINGER = 0`); session marked `FAILED (ERR_AUTH_TIMEOUT)`.

---

### Group 2: Peer Authentication & Trust Binding (Scenarios G – L)

#### Scenario G: Peer Authentication Success Under Approved Fixture
- **Objective:** Verify successful two-way handshake and token validation.
- **Fixture:** Client sends valid `HANDSHAKE_INIT` containing matching 256-bit token and identical User SID.
- **Assertion:** Coordinator replies with `HANDSHAKE_ACK`; state transitions to `PEER_AUTHENTICATED`.

#### Scenario H: Peer Identity Mismatch Rejection
- **Objective:** Verify detection and rejection when client claims an identity differing from preflight.
- **Fixture:** Preflight approved for Endpoint A; client transmits `claimed_endpoint_id` of Endpoint C.
- **Assertion:** Coordinator severs pipe immediately; emits `ERR_AUTH_IDENTITY_MISMATCH`; logs security alert.

#### Scenario I: Authentication Service Unavailable Fail-Closed
- **Objective:** Verify fail-closed behavior when identity verification service is unreachable.
- **Fixture:** Simulated internal fault in `PAH-03` token validator.
- **Assertion:** Connection dropped immediately; state transitions to `FAILED`; zero session admission.

#### Scenario J: Handshake Token Expiration / Timeout
- **Objective:** Verify late handshake tokens (> 500ms) are strictly rejected.
- **Fixture:** Client sends `HANDSHAKE_INIT` 520ms after physical socket connection.
- **Assertion:** Coordinator rejects frame with `ERR_AUTH_TIMEOUT`; pipe closed; token invalidated.

#### Scenario K: Revoked Endpoint Reconnection Prevention
- **Objective:** Verify endpoint revoked during active session cannot authenticate on a new connection.
- **Fixture:** Client reconnects using a stale token after administrative revocation has taken place.
- **Assertion:** Authentication fails with `ERR_AUTH_TOKEN_MISMATCH`; connection severed.

#### Scenario L: Unchanged Locator with Mutated Identity Evidence
- **Objective:** Verify identity substitution detection on an existing Named Pipe (`CONF-03`).
- **Fixture:** Process with PID 9999 opens pipe previously bound to PID 1000.
- **Assertion:** OS PID check fails; pipe reset; full re-authentication enforced.

---

### Group 3: Protocol Negotiation & Version Selection (Scenarios M – T)

#### Scenario M: Successful Common Protocol Version Selection
- **Objective:** Verify selection of mutually supported `NLE-WIRE-v1` protocol.
- **Fixture:** Both endpoints offer `supported_wire_protocols: ["NLE-WIRE-v1"]`.
- **Assertion:** Protocol reconciled as `NLE-WIRE-v1`; state transitions to `PROTO_COMPATIBLE`.

#### Scenario N: No Mutually Supported Protocol Version
- **Objective:** Verify clean failure when endpoints offer non-overlapping protocol versions.
- **Fixture:** Endpoint A offers `["NLE-WIRE-v1"]`; Endpoint B offers `["NLE-WIRE-v2"]`.
- **Assertion:** Negotiation fails with `ERR_PROTO_NO_COMMON_VERSION`; coordinator emits `CAPABILITY_REJECT`.

#### Scenario O: Missing Mandatory Protocol Feature
- **Objective:** Verify rejection if client fails to support mandatory wire framing or digest features.
- **Fixture:** Endpoint A offers protocol without `feat.wire.digest.sha256`.
- **Assertion:** Negotiation fails with `ERR_PROTO_MANDATORY_FEATURE_MISSING`; channel closed.

#### Scenario P: Missing Optional Feature Graceful Degradation
- **Objective:** Verify optional feature is disabled if not supported by both endpoints.
- **Fixture:** Endpoint A offers `feat.wire.compression.lz4`; Endpoint B omits it.
- **Assertion:** Protocol negotiated successfully; compression flag disabled in final contract.

#### Scenario Q: Conflicting Protocol Declarations Rejection
- **Objective:** Verify detection of contradictory framing or encoding declarations.
- **Fixture:** Endpoint declares conflicting framing modes (`LENGTH_PREFIX` and `DELIMITED`).
- **Assertion:** Negotiation rejected with `ERR_PROTO_FEATURE_CONFLICT`.

#### Scenario R: Unknown Mandatory Schema Field in Control Frame
- **Objective:** Verify schema validation catches unversioned or unexpected control frame attributes.
- **Fixture:** Client sends `CAPABILITY_OFFER` with invalid property `unexpected_override_flag: true`.
- **Assertion:** Schema validation fails; frame rejected with `ERR_SCHEMA_VIOLATION`.

#### Scenario S: Mid-Handshake Protocol Version Mutation
- **Objective:** Verify rejection if client changes protocol version offer between frames.
- **Fixture:** Client offers `v1` in handshake init, then sends `v2` in capability offer.
- **Assertion:** Coordinator detects state drift; aborts handshake with `ERR_PROTO_STATE_DRIFT`.

#### Scenario T: Interrupted Negotiation Frame Stream
- **Objective:** Verify timeout handling if client disconnects while negotiation is in flight.
- **Fixture:** Client sends partial JSON frame, then closes socket.
- **Assertion:** Frame parser reports EOF; coordinator marks session `FAILED`; reclaims state.

---

### Group 4: Capability Agreement & Authorization (Scenarios U – Z)

#### Scenario U: Advertised Capability Lacking Authorization Scope
- **Objective:** Verify capabilities not covered by user security grant are excluded from effective set.
- **Fixture:** Both endpoints offer `admin.process.restart`; user granted only `telemetry.player.spatial`.
- **Assertion:** Admin capability excluded; effective set contains strictly spatial telemetry.

#### Scenario V: Mandatory Capability Missing from One Participant
- **Objective:** Verify connection aborts if a mandatory capability requested in preflight is unsupported.
- **Fixture:** Preflight requires `inventory.item.transfer`; Endpoint B does not offer it.
- **Assertion:** Admission fails with `ERR_CAP_MANDATORY_MISSING`; coordinator emits `CAPABILITY_REJECT`.

#### Scenario W: Un-Negotiated Capability Inbound Dispatch Block
- **Objective:** Verify router drops frames targeting topics not present in session contract.
- **Fixture:** Active session established without chat capability; client transmits frame on `chat.message`.
- **Assertion:** Router Gate 3 rejects frame with `ERR_PME_006_UNKNOWN_TOPIC`.

#### Scenario X: Authorization Scope Bound to Mismatched Game Pair
- **Objective:** Verify security grant scoped to Game A + Game B cannot be used for Game A + Game C.
- **Fixture:** Request presents auth token scoped to `minecraft` + `skyrim` for `minecraft` + `half-life-2`.
- **Assertion:** Preflight / Admission fails with `ERR_ADM_SCOPE_DENIED`.

#### Scenario Y: Authorization Revocation Prior to Session Admission
- **Objective:** Verify race condition where user revokes permission while handshake is in flight.
- **Fixture:** User triggers permission revocation during `CAP_ASSESS_PENDING` phase.
- **Assertion:** Admission gate detects revoked grant; session admission aborted (`ERR_AUTH_REVOKED`).

#### Scenario Z: Stale Cross-Game Semantic Compatibility Evidence
- **Objective:** Verify admission blocks if Phase 4 semantic compatibility record has expired.
- **Fixture:** Compatibility assessment record older than 24 hours without refresh.
- **Assertion:** Admission fails with `ERR_COMPAT_EVIDENCE_STALE`; reassessment prompted.

---

### Group 5: Lifecycle, Replay & Failure Recovery (Scenarios AA – AF)

#### Scenario AA: Timeout During Delivery of Final Capability Accept
- **Objective:** Verify uncertain state handling when final ACK delivery is ambiguous.
- **Fixture:** Coordinator sends `CAPABILITY_ACCEPT`, but socket resets before TCP ACK.
- **Assertion:** Session marked `UNCERTAIN`; logged to `TDJ-08`; client required to re-authenticate.

#### Scenario AB: Duplicate Connection Request Arrives for Active Session
- **Objective:** Verify second connection request for existing active session is rejected.
- **Fixture:** Request arrives targeting Endpoint A while Endpoint A is in `ACTIVE_ROUTING`.
- **Assertion:** Request rejected with `ERR_CONCURRENCY_LOCKED`; active session undisturbed.

#### Scenario AC: Handshake Request Races Against Administrative Revocation
- **Objective:** Verify revocation event wins race against in-flight connection establishment.
- **Fixture:** Revocation signal received during `PROTO_NEG_PENDING`.
- **Assertion:** Cancellation token fires immediately; socket closed; state -> `REVOKED`.

#### Scenario AD: User Cancellation During Handshake Authentication
- **Objective:** Verify clean teardown when user clicks "Cancel" in UI during connection.
- **Fixture:** `CancellationToken` cancelled during `PEER_AUTH_PENDING`.
- **Assertion:** Handshake aborts in < 20ms; handles closed; state -> `CANCELLED`.

#### Scenario AE: Session Admitted Without Complete Audit Trail
- **Objective:** Verify admission fails if audit journal fails to commit session record.
- **Fixture:** Simulated disk write failure in `TDJ-08` logger during contract minting.
- **Assertion:** Admission rolls back; socket closed; error logged to emergency console.

#### Scenario AF: Cleanup of Provisional Resources on Connection Failure
- **Objective:** Verify zero handle or memory leakage across failed connection attempts.
- **Fixture:** 50 consecutive failed handshakes simulated against transport factory.
- **Assertion:** Handle count and memory allocation remain completely flat; all handles disposed.

---

### Group 6: Hybrid Integration & Security Guardrails (Scenarios AG – AL)

#### Scenario AG: Phase 8 Mod Verified but Live Bridge Authentication Fails
- **Objective:** Verify live bridge auth failure does not trigger Phase 8 filesystem rollback.
- **Fixture:** Phase 8 package verified; bridge handshake fails with token mismatch.
- **Assertion:** Bridge session aborted; Phase 8 installed files remain completely untouched.

#### Scenario AH: Bridge Authentication Succeeds but Host Modification Unverified
- **Objective:** Verify hybrid capability is excluded if Phase 8 package is unverified.
- **Fixture:** Bridge auth succeeds; capability `requires_content_package = true`; package unverified.
- **Assertion:** Capability excluded from contract; flagged `PREREQUISITE_PENDING`.

#### Scenario AI: Handshake Metadata Attempting Arbitrary Command Injection
- **Objective:** Verify metadata containing command injection payloads is neutralized.
- **Fixture:** Client offers capability with schema URI containing `cmd.exe /c calc.exe`.
- **Assertion:** Schema URI fails regex check; payload rejected; zero command execution.

#### Scenario AJ: Oversized Negotiation Frame Rejection
- **Objective:** Verify parser drops negotiation payload exceeding maximum frame size.
- **Fixture:** Client sends 2MB `CAPABILITY_OFFER` (handshake max frame limit: 64KB).
- **Assertion:** Transport severs connection immediately with `ERR_PME_002_FRAME_LENGTH`.

#### Scenario AK: Authenticated Peer Incompatible with Target Game Release
- **Objective:** Verify authentication does not override engine version incompatibility.
- **Fixture:** Client proves identity for Half-Life 2 Build 100, but mashup requires Build 200.
- **Assertion:** Admission blocked by Compatibility Engine; status -> `REJECTED`.

#### Scenario AL: Complete End-to-End Synthetic Handshake & Activation
- **Objective:** Verify seamless progression across all 7 phases using synthetic fixtures only.
- **Fixture:** Mock Minecraft and Half-Life 2 adapters with synthetic tokens.
- **Assertion:** Complete state transition: `REQUESTED` -> `PREFLIGHT` -> `CONNECTED` -> `AUTH` -> `NEGOTIATED` -> `ADMITTED` -> `ACTIVE_ROUTING`. Zero real game processes or sockets invoked.
