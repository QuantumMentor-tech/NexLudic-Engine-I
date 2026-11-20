# LIVE BRIDGE MESSAGE EXCHANGE SYNTHETIC QA TEST PLAN

**Document ID:** `NLE-CAT-MRD-001`  
**Classification:** Quality Assurance & Synthetic Test Catalog  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 73 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-MRD-001` through `NLE-CON-MDA-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), Test Automation Framework  

---

## 1. Testing Strategy & Execution Boundary

In accordance with the **Design-Only Milestone Constraint (`P8-C02`)**, zero live game processes are hooked, zero network probes are executed, and zero runtime memory injection occurs.

All verification is conducted via **Deterministic Synthetic Test Fixtures**, exercising the message router, validation pipeline, request-response correlator, deduplication guard, and queue controllers against simulated endpoints and mock channel pipes.

---

## 2. Test Scenario Matrix (Scenarios A through AN — 40 Scenarios)

```
+-------+------------------------------------------+--------------------+---------------------+
| Group | Domain Area                              | Scenarios          | Scenario Count      |
+-------+------------------------------------------+--------------------+---------------------+
| 1     | Envelope & Multi-Stage Validation        | Scenarios A - G    | 7 Scenarios         |
| 2     | Routing & Authorization Gates            | Scenarios H - M    | 6 Scenarios         |
| 3     | Request-Response Correlation             | Scenarios N - S    | 6 Scenarios         |
| 4     | Delivery Guarantees & Uncertainty        | Scenarios T - Y    | 6 Scenarios         |
| 5     | Duplicate Detection, Ordering & Aborts   | Scenarios Z - AF   | 7 Scenarios         |
| 6     | Resource, Security & Hybrid Boundaries   | Scenarios AG - AN  | 8 Scenarios         |
+-------+------------------------------------------+--------------------+---------------------+
| TOTAL | Comprehensive Test Suite                 | Scenarios A - AN   | 40 Test Scenarios   |
+-------+------------------------------------------+--------------------+---------------------+
```

---

## 3. Detailed Synthetic Scenario Specifications

### Group 1: Envelope & Multi-Stage Validation (Scenarios A – G)

#### Scenario A: Valid Request Message Progression
- **Objective:** Verify valid `COMMAND_REQUEST` passes all 7 validation stages.
- **Fixture:** Authenticated peer sends well-formed envelope with valid digest, sequence number, and authorized topic.
- **Assertion:** Frame dispatched to recipient queue; state -> `VALIDATED_AND_DISPATCHED`.

#### Scenario B: Missing Mandatory Envelope Field
- **Objective:** Verify rejection when mandatory field (`message_id`) is missing.
- **Fixture:** Frame JSON payload omitting `message_id`.
- **Assertion:** Rejection at Stage 2 with `ERR_PME_SYNTAX`; error signal returned to sender.

#### Scenario C: Unsupported Schema Revision
- **Objective:** Verify rejection when payload declares unknown schema version.
- **Fixture:** Payload specifies `schema_version: "99.0.0"`.
- **Assertion:** Rejection at Stage 6 with `ERR_PAYLOAD_SCHEMA_VIOLATION`.

#### Scenario D: Sender Identity Mismatch (Spoofing Attempt)
- **Objective:** Verify detection when sender ID does not match channel's authenticated peer.
- **Fixture:** Channel authenticated as Endpoint A; frame declares `sender_endpoint_id: "valve:half-life-2"`.
- **Assertion:** Immediate socket reset at Stage 4; `ERR_AUTH_SPOOF` logged in `TDJ-08`.

#### Scenario E: Message Referencing Inactive Session
- **Objective:** Verify rejection when frame presents an expired or un-admitted session ID.
- **Fixture:** Session ID points to a session in `CLOSED` state.
- **Assertion:** Rejection at Stage 3 with `ERR_SESSION_INACTIVE`.

#### Scenario F: Oversized Frame Payload Rejection
- **Objective:** Verify parser drops frames exceeding length ceiling.
- **Fixture:** Inbound frame specifies length `2,000,000` bytes (default limit: 1 MB).
- **Assertion:** Socket severed immediately at Stage 1 with `ERR_PME_002_FRAME_LENGTH`.

#### Scenario G: Command Injection in Payload Text Neutralized
- **Objective:** Verify payload strings containing shell commands are treated strictly as passive text data.
- **Fixture:** Item custom name set to `"; rm -rf /; calc.exe"`.
- **Assertion:** String deserialized as passive string; zero subprocess execution; digest validated.

---

### Group 2: Routing & Authorization Gates (Scenarios H – M)

#### Scenario H: Valid Direct Unicast Routing
- **Objective:** Verify router dispatches frame exclusively to specified recipient channel.
- **Fixture:** Endpoint A sends unicast command to Endpoint B.
- **Assertion:** Endpoint B outbound queue receives frame; zero leaking to other endpoints.

#### Scenario I: Recipient Endpoint Unavailable
- **Objective:** Verify error response when target endpoint is temporarily disconnected.
- **Fixture:** Target endpoint channel in `DISCONNECTED` state.
- **Assertion:** Router returns `ERROR_SIGNAL` with `ERR_ROUTE_RECIPIENT_UNAVAILABLE`.

#### Scenario J: Recipient Registered but Incompatible
- **Objective:** Verify routing rejection when recipient lacks capability schema.
- **Fixture:** Recipient registered, but does not support requested topic.
- **Assertion:** Router rejects frame with `ERR_ROUTE_INCOMPATIBLE`.

#### Scenario K: Capability Not Negotiated in Session
- **Objective:** Verify rejection of topic not present in active session contract.
- **Fixture:** Endpoint sends frame on un-negotiated topic `chat.global`.
- **Assertion:** Stage 5 rejects frame with `ERR_TOPIC_UNAUTHORIZED`.

#### Scenario L: User Security Grant Revocation Mid-Session
- **Objective:** Verify router immediately blocks topic when user revokes permission during session.
- **Fixture:** User revokes `inventory.item.transfer` permission while session is active.
- **Assertion:** Subsequent frames on this topic rejected at Stage 5 with `ERR_ACCESS_DENIED`.

#### Scenario M: Multicast Event Fan-Out with Partial Authorization
- **Objective:** Verify broadcast event delivers only to authorized endpoints.
- **Fixture:** Broadcast `DATA_EVENT`; Endpoint B authorized, Endpoint C unauthorized.
- **Assertion:** Frame routed to B; frame withheld from C; partial delivery recorded.

---

### Group 3: Request-Response Correlation (Scenarios N – S)

#### Scenario N: Valid Response Matches Outstanding Request
- **Objective:** Verify response pairs with pending request and completes interaction.
- **Fixture:** B sends `COMMAND_RESPONSE` matching active `correlation_id: req-01`.
- **Assertion:** Correlator matches request; marks `COMPLETED`; resolves caller task.

#### Scenario O: Response with Unknown Correlation ID
- **Objective:** Verify handling of response referencing non-existent request.
- **Fixture:** Response arrives with `correlation_id: req-unknown-999`.
- **Assertion:** Frame dropped; logged as `WARN_CORRELATION_UNKNOWN_ID`; zero state mutation.

#### Scenario P: Response Arriving from Wrong Participant
- **Objective:** Verify rejection when response sender does not match original target.
- **Fixture:** Request sent to B; response arrives from C with matching correlation ID.
- **Assertion:** Correlator rejects match; logs security alert `ERR_CORRELATION_PEER_MISMATCH`.

#### Scenario Q: Late Response Arriving After Timeout
- **Objective:** Verify response arriving after request timed out does not mutate state.
- **Fixture:** Request timed out at $t = 3000\text{ms}$; response arrives at $t = 3200\text{ms}$.
- **Assertion:** Response logged as `INFO_LATE_RESPONSE_RECEIVED`; request remains `TIMED_OUT`.

#### Scenario R: Contradictory Duplicate Terminal Responses
- **Objective:** Verify second terminal response for completed request is rejected.
- **Fixture:** Request already `COMPLETED`; second response arrives for same correlation ID.
- **Assertion:** Second frame rejected with `ERR_CORRELATION_DUPLICATE_RESPONSE`.

#### Scenario S: Duplicate Acknowledgement Received
- **Objective:** Verify duplicate ACK frame is safely absorbed without corrupting state.
- **Fixture:** Sender receives two identical `ACKNOWLEDGEMENT (ACCEPTED)` frames.
- **Assertion:** First ACK updates state to `ACK_RECEIVED`; second ACK absorbed idempotently.

---

### Group 4: Delivery Guarantees & Uncertainty (Scenarios T – Y)

#### Scenario T: Message Accepted Locally but Never Transmitted
- **Objective:** Verify local queue admission is reported separately from transmission.
- **Fixture:** Message admitted to queue; mock transport fails before socket write.
- **Assertion:** State reported as `ACCEPTED_FOR_TRANSMISSION` then `TRANSMISSION_FAILED`.

#### Scenario U: Ambiguous Transport Send Result
- **Objective:** Verify delivery uncertainty when socket resets during write.
- **Fixture:** OS reports pipe broken mid-write.
- **Assertion:** Interaction marked `DELIVERY_UNCERTAIN`; logged to `TDJ-08`.

#### Scenario V: Recipient Receives Message but Rejects Authorization
- **Objective:** Verify recipient-side gate failure reports rejection, not delivery loss.
- **Fixture:** Recipient receives frame but fails Stage 5 authorization check.
- **Assertion:** Recipient returns `ERROR_SIGNAL (ERR_TOPIC_UNAUTHORIZED)`; state -> `REJECTED`.

#### Scenario W: Recipient Accepts Request but Internal Handler Fails
- **Objective:** Verify distinction between transport delivery and application handler failure.
- **Fixture:** Recipient game mod throws internal NullPointerException during command.
- **Assertion:** Recipient returns `COMMAND_RESPONSE (Status: FAILED, Error: HANDLER_EXCEPTION)`.

#### Scenario X: Operation Completes but Response Frame Lost
- **Objective:** Verify timeout handling when execution occurred but response was lost.
- **Fixture:** Recipient completes operation, but outbound response socket drops.
- **Assertion:** Sender times out; status marked `UNCERTAIN`; idempotency key prevents re-execution.

#### Scenario Y: Request Timeout with Remote Execution Pending
- **Objective:** Verify state is classified as UNCERTAIN when timeout expires during processing.
- **Fixture:** Recipient processing takes 4000ms; client `ttl_ms` set to 3000ms.
- **Assertion:** Client marks request `TIMED_OUT (UNCERTAIN)`; logs diagnostic alert.

---

### Group 5: Duplicate Detection, Ordering & Aborts (Scenarios Z – AF)

#### Scenario Z: Retransmission of Identical Logical Message
- **Objective:** Verify deduplication guard drops identical retransmission.
- **Fixture:** Sender retransmits frame with same `message_id` and sequence number.
- **Assertion:** Duplicate detected; frame dropped; cached ACK re-emitted.

#### Scenario AA: Conflicting Payload on Reused Message ID
- **Objective:** Verify alarm triggered when same message ID carries altered payload.
- **Fixture:** Inbound frame has same `message_id` but altered `payload_digest`.
- **Assertion:** Security alert logged; frame rejected with `ERR_REPLAY_CONFLICTING_PAYLOAD`.

#### Scenario AB: Out-of-Order Message Progression Detection
- **Objective:** Verify sequence gap handling for sequenced interactions.
- **Fixture:** Channel receives sequence 104 when sequence 103 was expected.
- **Assertion:** Frame 104 buffered; retransmit requested; or dropped per QoS tier.

#### Scenario AC: Stale Event Dropped on Newer Observation
- **Objective:** Verify older telemetry dropped when newer sample has already arrived.
- **Fixture:** Telemetry sample with $t = 100$ arrives after sample with $t = 110$.
- **Assertion:** Stale sample discarded; router processes only freshest update.

#### Scenario AD: Cancellation Before Transmission
- **Objective:** Verify request cancelled in queue is purged cleanly without transmission.
- **Fixture:** Caller issues cancel while request is in outbound queue.
- **Assertion:** Frame purged from queue; state -> `CANCELLED`; zero bytes sent over wire.

#### Scenario AE: Cancellation During Remote Processing
- **Objective:** Verify cancelling in-flight request marks state UNCERTAIN if unconfirmed.
- **Fixture:** Caller cancels request while recipient is executing handler.
- **Assertion:** State transitions to `CANCELLED (UNCERTAIN)`; cancellation signal forwarded.

#### Scenario AF: Session Closure with Pending Outstanding Requests
- **Objective:** Verify clean accounting when session terminates with active requests.
- **Fixture:** Session terminated while 5 RPC requests are pending responses.
- **Assertion:** All 5 requests fail with `ERR_SESSION_TERMINATED`; handles deallocated.

---

### Group 6: Resource, Security & Hybrid Boundaries (Scenarios AG – AN)

#### Scenario AG: Channel Queue Backpressure Triggering
- **Objective:** Verify transport read loop pauses when queue hits 75% high watermark.
- **Fixture:** Fast producer enqueues 750 frames into 1000-frame queue.
- **Assertion:** Read loop pauses; resumes when consumer drains queue to 250 frames (25%).

#### Scenario AH: Token-Bucket Rate Limiter Overflow
- **Objective:** Verify frame throttling when message rate exceeds burst capacity.
- **Fixture:** Producer emits 25 frames in 50ms (rate limit: 60/s, burst: 10).
- **Assertion:** First 10 frames admitted; subsequent 15 frames receive `ERR_RATE_LIMITED`.

#### Scenario AI: Diagnostic Text Sanitization & Escape
- **Objective:** Verify malformed strings in diagnostics are sanitized.
- **Fixture:** Malformed frame contains ANSI escape sequences and null bytes.
- **Assertion:** Text sanitized in `TDJ-08`; zero log injection; valid JSONL format.

#### Scenario AJ: Audit Persistence Failure Fail-Closed Handling
- **Objective:** Verify routing halts if audit logger fails to commit records.
- **Fixture:** Disk write fault simulated in audit logger during state-changing RPC.
- **Assertion:** Operation rejected; error logged to console; fail-closed behavior verified.

#### Scenario AK: Authentication Token Invalidation Mid-Session
- **Objective:** Verify message routing halts if session token is invalidated.
- **Fixture:** Security watchdog revokes session token during active messaging.
- **Assertion:** Session transitions to `SUSPENDED`; subsequent frames dropped.

#### Scenario AL: Operation Completed but Gameplay Effect Unverified
- **Objective:** Verify report distinguishes completed RPC from verified gameplay change.
- **Fixture:** `COMMAND_RESPONSE` reports success for item spawn; telemetry pending.
- **Assertion:** Status reported as `COMPLETED_UNVERIFIED`; verification withheld.

#### Scenario AM: Hybrid Integration Bridge Loss Preserves Phase 8 Files
- **Objective:** Verify live bridge disconnect NEVER rolls back Phase 8 mod files.
- **Fixture:** Bridge connection severed during cross-game mashup session.
- **Assertion:** Bridge session terminates; Phase 8 installed files remain `VERIFIED`.

#### Scenario AN: Complete Synthetic Message Lifecycle
- **Objective:** Traverse complete message lifecycle using synthetic fixtures only.
- **Fixture:** Full mock pipeline (Endpoint A $\rightarrow$ CMR $\rightarrow$ Endpoint B).
- **Assertion:** Complete sequence: `CREATED` $\rightarrow$ `VALIDATED` $\rightarrow$ `TRANSMITTED` $\rightarrow$ `RECEIVED` $\rightarrow$ `COMPLETED`. Zero real game processes or sockets invoked.
