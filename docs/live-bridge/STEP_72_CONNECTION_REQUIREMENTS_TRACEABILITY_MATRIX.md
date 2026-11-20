# STEP 72 CONNECTION REQUIREMENTS TRACEABILITY MATRIX

**Document ID:** `NLE-MAT-STP-072`  
**Classification:** Requirements Traceability & Verification Matrix  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-CPN-001` through `NLE-CAT-CPN-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), System Audit Teams  

---

## 1. Overview & Verification Methodology

This matrix establishes 100% bidirectional traceability for all **38 Architectural Requirements (`CPN-REQ-001` through `CPN-REQ-038`)** governing Live Bridge Connection Establishment, Peer Authentication, and Protocol Negotiation.

Each requirement maps directly to its authoritative specification document, implementing architectural component, verifying synthetic test scenarios (from `NLE-CAT-CPN-001`), and compliance status.

---

## 2. Requirements Traceability Matrix (`CPN-REQ-001` to `CPN-REQ-038`)

| Req ID | Requirement Title & Description | Authoritative Specification | Implementing Component | Synthetic Test Verification | Compliance Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`CPN-REQ-001`** | **Seven-Stage Lifecycle Separation:** Connection != Auth != Protocol != Capabilities != Scope != Admission != Verified. | `NLE-SPC-CPN-001` §1 | Architecture Wide | Scenarios A, F, G, M, U, AL | **100% Specified** |
| **`CPN-REQ-002`** | **Non-Implication Invariant:** Transport connectivity conveys zero peer trust; auth conveys zero operation rights. | `NLE-SPC-CPN-001` §2 | `CBC-01`, `ASG-05` | Scenarios F, G, U | **100% Specified** |
| **`CPN-REQ-003`** | **Windows Medium IL Enforcement:** Platform operates strictly at Medium IL without UAC elevation (`DEC-002`). | `NLE-SPC-CPN-001` §2 | Engine Host | Scenario AL | **100% Specified** |
| **`CPN-REQ-004`** | **Non-Invasive Security Inviolability:** Zero DLL injection, zero memory hooks (`ReadProcessMemory`) (`BLK-SEC-01`). | `NLE-SPC-CPN-001` §2 | Subsystem Boundary | Scenario AL | **100% Specified** |
| **`CPN-REQ-005`** | **Fail-Closed Admission Policy:** Missing proofs, expired tokens, or unverified packages abort session admission. | `NLE-SPC-CPN-001` §2 | `SEG-04` | Scenarios B, C, I, Y, AH | **100% Specified** |
| **`CPN-REQ-006`** | **Structured Connection Request:** Canonical request schema binding endpoints, mashup definitions, and scope. | `NLE-CON-CRP-001` §2 | `CPE-01` | Scenarios A, D | **100% Specified** |
| **`CPN-REQ-007`** | **Eight-Check Preflight Validation:** Mandatory evaluation of registration, build hashes, and targets. | `NLE-CON-CRP-001` §3 | `CPE-01` | Scenarios A–E | **100% Specified** |
| **`CPN-REQ-008`** | **Preflight Result Freshness:** Passed preflight records expire after strictly 5000ms. | `NLE-CON-CRP-001` §4 | `CPE-01` | Scenario J | **100% Specified** |
| **`CPN-REQ-009`** | **Transport Factory Isolation:** Sockets and pipes allocated with restrictive user DACLs strictly upon preflight pass. | `NLE-CON-CRP-001` §4 | `CTF-02` | Scenarios A, E | **100% Specified** |
| **`CPN-REQ-010`** | **Physical Transport Failure Cleanup:** OS handles immediately disposed upon transport connect failures. | `NLE-CON-CRP-001` §4 | `CTF-02` | Scenarios F, AF | **100% Specified** |
| **`CPN-REQ-011`** | **Multi-Factor Peer Identity Binding:** Process SID attestation + 256-bit secret token + Canonical ID binding. | `NLE-POL-PAI-001` §2 | `PAH-03` | Scenarios G, H, L | **100% Specified** |
| **`CPN-REQ-012`** | **OS Process SID Attestation:** Windows token inspection via `GetNamedPipeClientProcessId`; blocks cross-user IPC. | `NLE-POL-PAI-001` §2.1 | `PAH-03` | Scenarios G, H | **100% Specified** |
| **`CPN-REQ-013`** | **Ephemeral Handshake Secret Token:** 256-bit CSPRNG token passed to authorized connector; must echo in 500ms. | `NLE-POL-PAI-001` §2.1 | `PAH-03` | Scenarios G, J | **100% Specified** |
| **`CPN-REQ-014`** | **Canonical ID & Build Hash Binding:** Claimed endpoint ID must match pre-authorized preflight ID. | `NLE-POL-PAI-001` §2.1 | `PAH-03` | Scenarios H, AK | **100% Specified** |
| **`CPN-REQ-015`** | **Standardized Handshake Envelopes:** Binary-framed `HANDSHAKE_INIT` and `HANDSHAKE_ACK` schemas. | `NLE-POL-PAI-001` §3 | `PAH-03` | Scenarios G, J | **100% Specified** |
| **`CPN-REQ-016`** | **Anti-Replay Nonce Exchange:** Client and server nonces exchanged and cryptographically bound to session. | `NLE-POL-PAI-001` §4 | `PAH-03` | Scenario G | **100% Specified** |
| **`CPN-REQ-017`** | **NLE-WIRE Protocol Standardization:** Wire communication standardizes on length-prefixed `NLE-WIRE-v1`. | `NLE-POL-PNV-001` §2 | `PCN-04` | Scenarios M, N | **100% Specified** |
| **`CPN-REQ-018`** | **Protocol Version Reconciliation:** Automated selection of common supported wire version. | `NLE-POL-PNV-001` §2.1 | `PCN-04` | Scenarios M, N | **100% Specified** |
| **`CPN-REQ-019`** | **Mandatory Feature Enforcement:** Mandatory framing and SHA-256 digest features cannot be disabled. | `NLE-POL-PNV-001` §3 | `PCN-04` | Scenario O | **100% Specified** |
| **`CPN-REQ-020`** | **Optional Feature Negotiation:** Optional features (e.g. compression) enabled only if both peers offer them. | `NLE-POL-PNV-001` §3.1 | `PCN-04` | Scenario P | **100% Specified** |
| **`CPN-REQ-021`** | **CAPABILITY_OFFER Schema:** Structured offer frame exchanging protocols, features, and topics. | `NLE-POL-PNV-001` §4 | `PCN-04` | Scenarios M, P | **100% Specified** |
| **`CPN-REQ-022`** | **Protocol Incompatibility Rejection:** Emits `CAPABILITY_REJECT` and terminates channel on version mismatch. | `NLE-POL-PNV-001` §4.2 | `PCN-04` | Scenarios N, O | **100% Specified** |
| **`CPN-REQ-023`** | **Effective Capability Set Derivation:** Mathematical intersection of offered, supported, and authorized topics. | `NLE-CON-CAS-001` §2 | `PCN-04`, `ASG-05` | Scenarios U, V | **100% Specified** |
| **`CPN-REQ-024`** | **Directionality Matching Rules:** Unidirectional and bidirectional compatibility rules strictly enforced. | `NLE-CON-CAS-001` §2.1 | `PCN-04` | Scenario AL | **100% Specified** |
| **`CPN-REQ-025`** | **Phase 8 Content Prerequisite Check:** Capabilities requiring packages validated against Phase 8 journal. | `NLE-CON-CAS-001` §3 | `HIBA-07` | Scenarios AG, AH | **100% Specified** |
| **`CPN-REQ-026`** | **User Authorization Scope Enforcement:** Effective set clamped strictly to user-granted security boundaries. | `NLE-CON-CAS-001` §3 | `ASG-05` | Scenarios U, X, Y | **100% Specified** |
| **`CPN-REQ-027`** | **NegotiatedSessionContract Minting:** Immutable session contract containing UUID, effective set, and timeouts. | `NLE-CON-CAS-001` §4 | `CBC-01` | Scenarios AL | **100% Specified** |
| **`CPN-REQ-028`** | **Router Table Handoff:** Session contract registered in `CMR-02` routing table; starts 1000ms heartbeat. | `NLE-CON-CAS-001` §5 | `CMR-02` | Scenario AL | **100% Specified** |
| **`CPN-REQ-029`** | **Deterministic 22-State FSM:** Formal state machine governing session progression and error transitions. | `NLE-SPC-CSF-001` §1 | `CBC-01` | Scenarios A–AL | **100% Specified** |
| **`CPN-REQ-030`** | **Structured Failure Codes:** Explicit error taxonomy (`ERR_CONN_*`, `ERR_AUTH_*`, `ERR_PROTO_*`, `ERR_ADM_*`). | `NLE-SPC-CSF-001` §3 | `CBC-01` | Scenarios B–E, H, N, O | **100% Specified** |
| **`CPN-REQ-031`** | **Sequence & Skew Protection:** 32-bit sequence numbers, ±5000ms clock skew window. | `NLE-POL-CSS-001` §2.1 | `CMR-02` | Scenario AL | **100% Specified** |
| **`CPN-REQ-032`** | **Downgrade Attack Rejection:** Strictly bars downgrade to plaintext, unauthenticated, or obsolete protocols. | `NLE-POL-CSS-001` §2.2 | `PCN-04` | Scenario N | **100% Specified** |
| **`CPN-REQ-033`** | **Peer Substitution Defenses:** Continuous PID and channel binding; new processes cannot inherit sessions. | `NLE-POL-CSS-001` §2.3 | `PAH-03` | Scenario L | **100% Specified** |
| **`CPN-REQ-034`** | **Memory Hygiene & Secret Scrubbing:** Ephemeral tokens wiped from memory; redacted in diagnostic logs. | `NLE-POL-CSS-001` §3 | `PAH-03`, `TDJ-08` | Scenario AL | **100% Specified** |
| **`CPN-REQ-035`** | **Multi-Phase Timeout Limits:** Timeouts enforced per stage; total establishment ceiling capped at 5000ms. | `NLE-POL-TCR-001` §1 | `CBC-01` | Scenarios F, J, AA | **100% Specified** |
| **`CPN-REQ-036`** | **Cooperative Cancellation Abort:** Handshakes cancel cleanly within 20ms without residual orphaned pipes. | `NLE-POL-TCR-001` §2 | `CBC-01` | Scenario AD | **100% Specified** |
| **`CPN-REQ-037`** | **Retry Safety & Circuit Breakers:** Exponential backoff on transient errors; retrying security failures barred. | `NLE-POL-TCR-001` §3 | `CBC-01` | Scenario AA | **100% Specified** |
| **`CPN-REQ-038`** | **Concurrency & Revocation Governance:** Per-endpoint mutex locks; revocation interrupts handshakes immediately. | `NLE-POL-TCR-001` §4 | `CBC-01` | Scenarios AB, AC | **100% Specified** |
