# STEP 72 OPEN ARCHITECTURE DECISIONS REGISTER

**Document ID:** `NLE-REG-STP-072`  
**Classification:** Architectural Decision Records (ADRs)  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-CPN-001`, `ADR-BRG-001..010`, `ADR-EDR-001..010`, `DEC-002`, `BLK-SEC-01`  
**Downstream Consumed By:** Phase 9 Steps 73–78  

---

## 1. Overview

This register formally documents the ten fundamental architectural decisions (`ADR-CPN-001` through `ADR-CPN-010`) established during Step 72 for Connection Establishment, Peer Authentication, and Protocol Negotiation.

---

## 2. Architectural Decision Records (`ADR-CPN-001` to `ADR-CPN-010`)

### `ADR-CPN-001`: Seven-Stage Session Establishment Lifecycle Separation
- **Context:** Conflating transport connection with peer trust, capability negotiation, or session authorization creates severe security vulnerabilities.
- **Decision:** Enforce seven discrete operational stages: Connection != Authentication != Protocol != Capabilities != Scope != Admission != Verified.
- **Rationale:** Ensures every security and compatibility condition is independently evaluated. Physical connection conveys zero trust.
- **Consequences:** Implementation requires formal gate transitions before admitting an active session.

### `ADR-CPN-002`: Two-Phase Preflight Validation Prior to Transport Channel Allocation
- **Context:** Allocating OS pipes and sockets for unregistered or incompatible endpoints wastes system resources and enables denial-of-service.
- **Decision:** Perform static preflight validation (evaluating Step 71 records, build hashes, and scope) before allocating OS transport resources.
- **Rationale:** Invalid or revoked connection requests are rejected in memory within 5ms without touching the OS transport stack.
- **Consequences:** Transport listeners are created only for preflight-approved connection requests.

### `ADR-CPN-003`: OS User Token SID Attestation for Local Named Pipe Handshakes
- **Context:** How does the engine verify that a local process connecting to a Named Pipe belongs to the authorized user on Windows?
- **Decision:** Query `GetNamedPipeClientProcessId` and inspect the primary token SID (`TokenUser`). Reject connections where client SID != engine SID.
- **Rationale:** Prevents cross-user IPC attacks or unprivileged guest access on shared multi-user workstations.
- **Consequences:** Restricts Named Pipe live bridge interop strictly to processes running under the current interactive user account.

### `ADR-CPN-004`: Ephemeral Handshake Secret Token Generation & Injection
- **Context:** Named Pipes on Windows can be opened by any process running under the same user account.
- **Decision:** Generate an ephemeral 256-bit CSPRNG secret token per session, injected into the authorized connector environment and verified in `HANDSHAKE_INIT`.
- **Rationale:** Prevents arbitrary local processes running under the user's account from squatting or hijacking the bridge pipe.
- **Consequences:** Connecting clients must transmit the token within 500ms or face immediate disconnection.

### `ADR-CPN-005`: Strict Major Wire Protocol Version Selection (`NLE-WIRE-v1`)
- **Context:** Should protocol negotiation support backwards-compatibility fallback to unversioned or ad-hoc protocols?
- **Decision:** Mandate strict agreement on `NLE-WIRE-v1`. Fallback to unauthenticated or plaintext protocols is prohibited.
- **Rationale:** Protects against protocol downgrade attacks and ensures all bridge traffic uses binary framing and SHA-256 payload digests.
- **Consequences:** Endpoints lacking support for `NLE-WIRE-v1` are rejected during negotiation.

### `ADR-CPN-006`: Commutative Capability Intersection and Rate Limit Clamping
- **Context:** How should asymmetric capabilities and differing rate limit preferences be reconciled between two game endpoints?
- **Decision:** Derive effective capabilities via strict mathematical intersection: offered by both, compatible directionality, matching schema version, and rate clamped to $\min(\text{Rate}_A, \text{Rate}_B)$.
- **Rationale:** Prevents faster game engines from flooding slower runtimes, and eliminates runtime deserialization errors.
- **Consequences:** Capabilities not offered by both endpoints or lacking matching schemas are omitted from the session contract.

### `ADR-CPN-007`: Independent Dual Authorization & Scope Guarding for Live Sessions
- **Context:** Does authorization to install Phase 8 mod files convey authorization to run a live bridge session?
- **Decision:** Enforce independent authorization. The Authorization Guard (`ASG-05`) verifies user grants specifically for the live game pair and capability set.
- **Rationale:** Content installation is a static filesystem modification; live bridging involves inter-process runtime data exchange.
- **Consequences:** Users must authorize live cross-game data exchange explicitly.

### `ADR-CPN-008`: Single-Use Ephemeral Token Consumption & Anti-Replay Nonce Exchange
- **Context:** Protecting against replay attacks where an attacker captures and replays a valid `HANDSHAKE_INIT` frame.
- **Decision:** Mark ephemeral tokens as single-use (consumed immediately upon validation), and bind subsequent frames to client/server nonces.
- **Rationale:** Ensures that even if a local process captures a token, it cannot be reused to open a second connection.
- **Consequences:** Token reuse triggers immediate security quarantine of the endpoint.

### `ADR-CPN-009`: Cooperative Handshake Cancellation and Deterministic Resource Reclamation
- **Context:** Handling user cancellations, timeouts, or network resets during connection establishment.
- **Decision:** Pass `CancellationToken` across all asynchronous handshake tasks; enforce deterministic disposal of transport handles in `finally` blocks.
- **Rationale:** Prevents lingering orphaned pipes, leaked memory buffers, or phantom sessions in the router.
- **Consequences:** Handshakes abort cleanly in < 20ms upon cancellation.

### `ADR-CPN-010`: Design-Only Specification Milestone (`P8-C02`)
- **Context:** Step 72 establishes connection establishment, peer authentication, and protocol negotiation specifications.
- **Decision:** Restrict Step 72 strictly to engineering specifications, domain models, contracts, and synthetic QA test designs. **Zero runtime code or live sockets/injections are implemented.**
- **Rationale:** Strict adherence to project roadmap governance. Prevents premature implementation before architectural contracts are approved.
- **Consequences:** Runtime bridge handshake implementation begins in subsequent Phase 9 steps.
