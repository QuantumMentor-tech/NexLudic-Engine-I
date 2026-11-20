# LIVE BRIDGE CONNECTION SECURITY & SECRET HANDLING POLICY

**Document ID:** `NLE-POL-CSS-001`  
**Classification:** Core Security & Cryptographic Handling Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-POL-SRS-001` (Step 70 Security Policy), `DEC-002`, `BLK-SEC-01`  
**Downstream Consumed By:** `NLE-POL-TCR-001`, Steps 73–78  

---

## 1. Scope & Security Objectives

Connection establishment occurs across local inter-process boundaries where unauthorized applications, rogue background services, or tampered game scripts could attempt to intercept, manipulate, or impersonate cross-game communications.

This policy defines the cryptographic channel protections, anti-replay mechanisms, downgrade defenses, and strict secret-handling standards required during connection negotiation.

---

## 2. Inviolable Security Controls

### 2.1 Replay Attack Defenses
1. **Monotonically Increasing Sequence Counters:** Every frame in the handshake and active session carries a 32-bit monotonic sequence counter starting at `1`. Duplicate or regressive sequence numbers trigger immediate frame drops (`ERR_PME_004`).
2. **Timestamp Skew Tolerance:** Frame timestamps must be within $\pm 5000\,\text{ms}$ of coordinator UTC time. Outside this window, frames are rejected (`ERR_PME_005`).
3. **Session Nonce Binding:** Handshake frames bind client and server nonces. Replaying a recorded handshake packet in a subsequent session fails because the server nonce differs.

### 2.2 Protocol Downgrade Defenses
1. **Strict Version Lockout:** The coordinator strictly enforces `NLE-WIRE-v1`. Any handshake offer requesting deprecated, unauthenticated, or plaintext fallback protocols is rejected (`ERR_PROTO_DOWNGRADE_ATTEMPT`).
2. **Feature Downgrade Protection:** Mandatory features (`feat.wire.digest.sha256`, `feat.wire.framing.length_prefix`) cannot be disabled under any circumstances.

### 2.3 Peer Substitution Defenses
1. **Continuous PID & Socket Binding:** Once an endpoint authenticates from a specific Process ID on a Named Pipe, the transport channel binds strictly to that OS file handle. If the pipe is severed or the client PID exits, the session is invalidated.
2. **No Reconnection Hijacking:** A newly spawned process cannot resume an existing `session_id`. It must initiate a fresh connection request with a new instance ID.

---

## 3. Ephemeral Secret Token Handling Standards

Ephemeral handshake secret tokens represent sensitive cryptographic material and must be handled under strict memory hygiene rules:

```
+-----------------------------------------------------------------------------------------+
|                                  SECRET HANDLING RULES                                  |
|                                                                                         |
|   1. MEMORY-ONLY PINNED STORAGE:                                                        |
|      - Tokens are stored in unmanaged pinned byte arrays or ProtectedMemory.            |
|      - Zero-wiped (SecureZeroMemory / Array.Clear) immediately upon consumption.        |
|                                                                                         |
|   2. ZERO DISK PERSISTENCE:                                                             |
|      - Tokens are NEVER written to configuration files, registry keys, or temp files.   |
|                                                                                         |
|   3. LOG REDACTION & SCRUBBING:                                                         |
|      - Diagnostic journals (TDJ-08) log ONLY the SHA-256 hash digest of the token.      |
|      - Raw token strings are strictly redacted from exception logs and stack traces.    |
|                                                                                         |
|   4. SHORT-LIVED VALIDITY WINDOW:                                                       |
|      - Ephemeral tokens expire exactly 5000ms after preflight generation if unused.     |
|      - Once consumed in HANDSHAKE_INIT, the token cannot be reused.                     |
+-----------------------------------------------------------------------------------------+
```

---

## 4. Privacy & Data Minimization

Handshake frames must contain **only** data required to establish the technical channel:
- Endpoint canonical identifier.
- Protocol versions and feature flags.
- Capability descriptors and schema URIs.
- Ephemeral tokens and nonces.

Handshake frames **MUST NEVER** transmit:
- User gamer tags, personal names, or email addresses.
- File paths outside designated mod directories.
- System hardware serial numbers or MAC addresses.
- Raw memory addresses or binary pointer values.
