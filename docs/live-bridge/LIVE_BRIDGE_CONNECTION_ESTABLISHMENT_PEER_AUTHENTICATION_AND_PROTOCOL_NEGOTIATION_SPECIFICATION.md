# LIVE BRIDGE CONNECTION ESTABLISHMENT, PEER AUTHENTICATION & PROTOCOL NEGOTIATION SPECIFICATION

**Document ID:** `NLE-SPC-CPN-001`  
**Classification:** Core Engineering & Security Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001` (Live Game Bridge Architecture), `NLE-SPC-PME-001` (Protocol & Message Envelope Foundation), `NLE-POL-SLC-001` (Session Lifecycle & Capability Negotiation), `NLE-CON-DEC-001` (Domain Model & Canonical Identity), `NLE-SPC-EDR-001` (Endpoint Discovery & Registration), `NLE-CON-ERR-001` (Registration Contract), `NLE-POL-EIV-001` (Identity Verification Policy)  
**Downstream Consumed By:** Step 73 (Cross-Game Semantic Translation Engine), Step 74 (Bidirectional Event Routing), Step 76 (Hybrid Integration Orchestrator)  

---

## 1. Executive Summary & Purpose

This specification defines the authoritative engineering and security protocol governing the transition from registered, identity-assessed endpoints (Step 71) to an authenticated, compatible, explicitly authorized, and active bridge session (Step 70).

Establishing cross-game live interoperability involves complex interactions between heterogeneous game processes, local IPC transports, and sandboxed shims. To prevent security vulnerabilities, process instability, session confusion, and unauthorized state mutation, this specification formalizes the strict boundaries separating the seven distinct operational states of session establishment:

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC ENGINE PLATFORM                                        |
|                                                                                                    |
|   +--------------------------------------------------------------------------------------------+   |
|   |                        SESSION ESTABLISHMENT SEPARATION OF CONCERNS                        |   |
|   |                                                                                            |   |
|   |   +------------------+       +-------------------+       +--------------------+            |   |
|   |   |    CONNECTION    | ----> |       PEER        | ----> |      PROTOCOL      |            |   |
|   |   |   ESTABLISHED    |       |   AUTHENTICATED   |       |     NEGOTIATED     |            |   |
|   |   | Physical pipe/IPC|       | 4-Tier proof check|       | Wire version match |            |   |
|   |   +------------------+       +-------------------+       +---------+----------+            |   |
|   |                                                                    |                       |   |
|   |                                                                    v                       |   |
|   |   +------------------+       +-------------------+       +--------------------+            |   |
|   |   |     SESSION      | <---- |      SESSION      | <---- |     CAPABILITY     |            |   |
|   |   |      ACTIVE      |       |    AUTHORIZED     |       |       AGREED       |            |   |
|   |   | Handoff to router|       | Scope grant check |       | Effective subset   |            |   |
|   |   +------------------+       +-------------------+       +--------------------+            |   |
|   +--------------------------------------------------------------------------------------------+   |
|                                                                                                    |
|   ================================ INVIOLABLE VERIFICATION BOUNDARY ==============================  |
|    - An Active Session does NOT imply that runtime operations have been verified.                   |
|    - Operation verification requires independent empirical evidence (telemetry / state receipts).   |
+----------------------------------------------------------------------------------------------------+
```

$$\text{Connection Established} \neq \text{Peer Authenticated} \neq \text{Protocol Negotiated} \neq \text{Capabilities Agreed} \neq \text{Session Authorized} \neq \text{Session Active} \neq \text{Operation Verified}$$

---

## 2. Inviolable Architectural & Security Tenets

1. **Strict Non-Implication:**
   - Physical transport reachability does not imply peer authenticity.
   - Successful peer authentication does not imply wire protocol compatibility.
   - Compatible wire protocols do not imply capability agreement.
   - Capability agreement does not grant authorization to execute those capabilities.
   - Session activation does not imply that subsequent game-state operations are verified.
2. **Deterministic Preflight Verification:**
   Before any OS transport pipe or socket is established, both candidate endpoints must pass preflight validation confirming that their registration records in Step 71 are `REGISTERED_VERIFIED`, fresh, unrevoked, and bound to the exact target game builds.
3. **Identity-Bound Handshake Cryptography:**
   All handshake frames must cryptographically bind the ephemeral 256-bit handshake secret token, the canonical endpoint identifier (`provider:title-id:build-hash:instance-id`), and the transport channel descriptor. Session token reuse across different locators or PIDs triggers immediate quarantine.
4. **Non-Invasive Security Inviolability (`BLK-SEC-01`, `DEC-002`):**
   - The connection subsystem operates strictly at **Windows Medium Integrity Level**.
   - Zero DLL injection, zero API hooking, zero virtual memory reading/writing (`ReadProcessMemory`), zero anti-cheat or DRM circumvention.
   - Transports are restricted to local Named Pipes with explicit user SID DACLs and loopback TCP (`127.0.0.1`) with ephemeral token authentication.
5. **Fail-Closed Admission Policy:**
   If any preflight check, authentication proof, protocol negotiation gate, or authorization grant fails or times out, session admission is blocked immediately, transport channels are closed (`SO_LINGER = 0`), and provisional resources are deterministically reclaimed.

---

## 3. Handshake Architecture & Subsystem Decomposition

Connection establishment is orchestrated by the **Central Bridge Coordinator (`CBC-01`)** utilizing four specialized components:

```mermaid
flowchart TD
    subgraph Preflight_Stage["1. Preflight & Eligibility Engine"]
        ConnReq["Connection Request (Endpoint A + Endpoint B)"] --> PreflightEval["Preflight Validator (CPE-01)
(Verify Step 71 Records, Builds, Targets)"]
        PreflightEval -->|Pass| ChannelSetup["Transport Factory (CTF-02)
(Bind Named Pipe / Loopback TCP)"]
        PreflightEval -->|Fail| PreflightReject["Reject Connection Request"]
    end

    subgraph Handshake_Stage["2. Handshake & Authentication Engine"]
        ChannelSetup --> PipeOpen["Physical Pipe Connected"]
        PipeOpen --> HandshakeEngine["Peer Authentication Handler (PAH-03)
(Evaluate Token Proof, TokenUser SID)"]
        HandshakeEngine -->|Auth OK| ProtoEngine["Protocol & Capability Negotiator (PCN-04)
(Reconcile NLE-WIRE-v1 & Capability Schemas)"]
        HandshakeEngine -->|Auth Fail| TerminateChannel["Close Channel + Log TDJ-08"]
    end

    subgraph Admission_Stage["3. Session Admission & Handoff"]
        ProtoEngine -->|Negotiated| AuthCheck{"Authorization Guard (ASG-05)
Verify User Granted Scope"}
        AuthCheck -->|Approved| SessionMint["Mint NegotiatedSessionContract
(Step 70 Active State)"]
        AuthCheck -->|Denied| DropSession["Drop Session (Scope Denied)"]
        SessionMint --> ActiveHandoff["Handoff to Central Message Router (CMR-02)"]
    end
```

### 3.1 Subsystem Role Breakdown
- **`CPE-01` Connection Preflight Engine:** Validates the static inputs of the connection request, confirming that both endpoint registrations are active, identity evidence is fresh, and game target definitions match.
- **`CTF-02` Connection Transport Factory:** Instantiates the approved OS IPC transport channel (Named Pipe or Loopback TCP) with strict DACL security descriptors.
- **`PAH-03` Peer Authentication Handler:** Executes the two-way cryptographic token exchange and validates process ownership against the local user token SID.
- **`PCN-04` Protocol & Capability Negotiator:** Evaluates protocol versions, selects wire format options, computes the mathematical intersection of supported capabilities, and clamps rate limits.
- **`ASG-05` Authorization & Scope Guard (Step 70):** Enforces user security grants, validating that the negotiated capability set does not exceed user-approved boundaries.

---

## 4. End-to-End Handshake Message Flow

The full handshake sequence from preflight to active session handoff proceeds through seven deterministic phases:

```mermaid
sequenceDiagram
    autonumber
    participant UI as Orchestrator / Mashup UI
    participant CBC as Central Bridge Coordinator
    participant GEC_A as Game Endpoint A (Process A)
    participant GEC_B as Game Endpoint B (Process B)

    UI->>CBC: Submit Connection Request (Endpoint A, Endpoint B, Scope)
    Note over CBC: Phase 1: Preflight Validation (CPE-01)<br/>Verify Step 71 registrations, build hashes, target contexts
    CBC->>CBC: Allocate Ephemeral Session Tokens (Token A, Token B)
    CBC->>CBC: Open Named Pipes / Loopback TCP Listeners (CTF-02)
    
    CBC-->>GEC_A: Launch / Bootstrap with Token A & Pipe Locator
    CBC-->>GEC_B: Launch / Bootstrap with Token B & Pipe Locator

    Note over CBC,GEC_A: Phase 2: Transport Connection & Peer Authentication
    GEC_A->>CBC: Transport Connect
    GEC_A->>CBC: HANDSHAKE_INIT (Token A, Endpoint A Canonical ID)
    CBC->>CBC: Validate Token A & Client Process User SID (PAH-03)
    CBC->>GEC_A: HANDSHAKE_ACK (Engine Version, Auth OK)

    Note over CBC,GEC_B: Phase 2 (Parallel): Endpoint B Handshake
    GEC_B->>CBC: Transport Connect
    GEC_B->>CBC: HANDSHAKE_INIT (Token B, Endpoint B Canonical ID)
    CBC->>CBC: Validate Token B & Client Process User SID (PAH-03)
    CBC->>GEC_B: HANDSHAKE_ACK (Engine Version, Auth OK)

    Note over CBC,GEC_B: Phase 3: Protocol & Capability Negotiation (PCN-04)
    GEC_A->>CBC: CAPABILITY_OFFER (Manifest A, Protocols: [NLE-WIRE-v1])
    GEC_B->>CBC: CAPABILITY_OFFER (Manifest B, Protocols: [NLE-WIRE-v1])
    CBC->>CBC: Reconcile Protocol Version & Compute Capability Intersection
    CBC->>CBC: Verify Phase 8 Content Prerequisites (Hybrid Arbiter HIBA-07)
    
    Note over CBC: Phase 4: Session Authorization & Admission Gate
    CBC->>CBC: Check User Authorization Scope (ASG-05)
    CBC->>CBC: Mint NegotiatedSessionContract (UUIDv4)
    
    CBC->>GEC_A: CAPABILITY_ACCEPT (NegotiatedSessionContract)
    CBC->>GEC_B: CAPABILITY_ACCEPT (NegotiatedSessionContract)
    
    Note over CBC,GEC_B: Phase 5: Active Bridge Session Activated
    CBC-->>UI: Session Established & Active (Handoff to Router)
```

---

## 5. Document Roadmap & Traceability

This specification coordinates with the remaining Step 72 documents:
- **`NLE-CON-CRP-001`:** Connection Request, Preflight Contracts & Participant Validation.
- **`NLE-POL-PAI-001`:** Peer Authentication, Proof Evaluation & Identity-Binding Policies.
- **`NLE-POL-PNV-001`:** Protocol Negotiation, Version Selection & Feature Governance Policies.
- **`NLE-CON-CAS-001`:** Capability Agreement, Effective Set Derivation & Session Admission Contracts.
- **`NLE-SPC-CSF-001`:** 22-State Connection Lifecycle, Failure Classification & State Transitions.
- **`NLE-POL-CSS-001`:** Channel Security, Replay Defense, Downgrade Protection & Secret Handling.
- **`NLE-POL-TCR-001`:** Handshake Timeouts, Cancellation, Retry Safety & Concurrency Governance.
- **`NLE-CAT-CPN-001`:** Comprehensive Synthetic QA Test Plan (Scenarios A through AL: 38 scenarios).
- **`NLE-MAT-STP-072`:** Requirements Traceability Matrix (`CPN-REQ-001..038`).
- **`NLE-REG-STP-072`:** Open Architecture Decisions Register (`ADR-CPN-001..010`).
- **`NLE-REP-STP-072`:** Step 72 Completion Report.
