# STEP 73 OPEN ARCHITECTURE DECISIONS REGISTER

**Document ID:** `NLE-REG-STP-073`  
**Classification:** Architectural Decision Records (ADRs)  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 73 of 100)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-MRD-001`, `ADR-BRG-001..010`, `ADR-EDR-001..010`, `ADR-CPN-001..010`  
**Downstream Consumed By:** Phase 9 Steps 74–78  

---

## 1. Overview

This register formally documents the ten fundamental architectural decisions (`ADR-MRD-001` through `ADR-MRD-010`) established during Step 73 for Message Exchange, Routing, Validation, and Delivery Semantics.

---

## 2. Architectural Decision Records (`ADR-MRD-001` to `ADR-MRD-010`)

### `ADR-MRD-001`: Multi-Stage Delivery Separation (Delivery != Operation != Verified)
- **Context:** Distributed systems often equate message delivery with operation completion or verified gameplay state.
- **Decision:** Explicitly separate communication milestones: Created != Validated != Transmitted != Received != Accepted != Processing != Completed != Verified.
- **Rationale:** Prevents callers from assuming an item was spawned or state changed merely because the transport layer delivered the message.
- **Consequences:** Systems requiring verified game-state changes must await independent empirical telemetry receipts.

### `ADR-MRD-002`: Hub-and-Spoke Gateway Routing via Central Message Router (`CMR-02`)
- **Context:** Should game adapters communicate peer-to-peer or centrally through NexLudic Engine?
- **Decision:** Mandate that all cross-game live traffic routes through `CMR-02`. P2P IPC between games is forbidden.
- **Rationale:** Centralized routing ensures consistent authorization enforcement, rate limiting, audit logging, and schema validation.
- **Consequences:** Negligible loopback routing latency (< 0.5ms) provides complete security containment.

### `ADR-MRD-003`: Seven-Stage Sequential Message Validation Pipeline (`MVP-01`)
- **Context:** When and how should incoming message frames be validated?
- **Decision:** Enforce a strict 7-stage sequential pipeline: Framing -> Syntax -> Session State -> Sender Attestation -> Capability Authorization -> Payload Schema -> Semantic Preconditions.
- **Rationale:** Early rejection of malformed or unauthorized frames protects downstream handlers from processing untrusted inputs.
- **Consequences:** Frames failing any stage are dropped immediately with structured error signals.

### `ADR-MRD-004`: Origin Identity Preservation & Immutability of Envelope Headers
- **Context:** Can routing intermediaries modify envelope headers or rewrite sender IDs?
- **Decision:** Strictly bar intermediaries from altering `sender_endpoint_id`, `message_id`, `sequence_number`, or `payload_digest`.
- **Rationale:** Guarantees end-to-end traceability and prevents proxy impersonation attacks.
- **Consequences:** Routing telemetry is appended strictly to outer routing metadata headers (`x-router-hop`).

### `ADR-MRD-005`: Request-Response Correlation via Ephemeral `correlation_id`
- **Context:** How should asynchronous RPC requests and responses be paired?
- **Decision:** Require unique `correlation_id` (UUIDv4) in requests, echoed in responses, tracked by `RRC-03`.
- **Rationale:** Decouples send and receive threads, handles out-of-order responses, and detects late/duplicate replies.
- **Consequences:** Requests maintain deterministic lifecycles with configurable timeouts (`ttl_ms`).

### `ADR-MRD-006`: QoS-Tiered Delivery Guarantees (QoS 0 vs QoS 1 / QoS 2)
- **Context:** Should all messages receive guaranteed delivery and retransmission?
- **Decision:** Differentiate QoS tiers: QoS 0 (Best-Effort) for spatial telemetry; QoS 1 (At-Least-Once) for commands; QoS 2 (Exactly-Once) for transactions.
- **Rationale:** Retransmitting stale high-frequency telemetry wastes bandwidth and causes queue bloat; state-changing commands require reliability.
- **Consequences:** Telemetry frames may be discarded under backpressure without breaking reliability for commands.

### `ADR-MRD-007`: Scoped Idempotency Keys and Monotonic Inbound Sequence Enforcement
- **Context:** Protecting against double-execution when commands are retransmitted after timeouts.
- **Decision:** Support optional `idempotency_key` cached for 60 seconds; enforce monotonic sequence numbers per channel.
- **Rationale:** Eliminates duplicate execution of state mutations (e.g. duplicating items or double-spending currency).
- **Consequences:** Senders can safely retry idempotent commands after uncertain timeouts.

### `ADR-MRD-008`: High-Water Backpressure (75%) and Socket Read Pausing
- **Context:** Preventing fast game simulations from overwhelming slower runtimes or engine memory.
- **Decision:** Pause socket/pipe read loops when inbound queues reach 75% capacity; resume when drained to 25%.
- **Rationale:** Leverages native OS TCP/Pipe flow control to push backpressure directly to the emitting game process without dropping commands.
- **Consequences:** Senders will experience blocked write calls if they exceed recipient consumption capacity.

### `ADR-MRD-009`: Payload Minimization & Secret Redaction in Telemetry Journals (`TDJ-08`)
- **Context:** Logging diagnostic events without violating player privacy or leaking secrets.
- **Decision:** Record only SHA-256 payload digests and lengths in diagnostic logs; scrub tokens and private data.
- **Rationale:** Complies with privacy standards and prevents sensitive gameplay data or credentials from leaking into log files.
- **Consequences:** Full payloads are not persisted in disk logs; verification relies on digest matching.

### `ADR-MRD-010`: Design-Only Specification Milestone (`P8-C02`)
- **Context:** Step 73 establishes message exchange, routing, validation, and delivery specifications.
- **Decision:** Restrict Step 73 strictly to engineering specifications, domain models, contracts, and synthetic QA test designs. **Zero runtime code or live routing brokers are implemented.**
- **Rationale:** Strict adherence to project roadmap governance. Prevents premature implementation before architectural contracts are approved.
- **Consequences:** Runtime message router and event bus implementation begin in subsequent Phase 9 steps.
