# SYNTHETIC COMMUNICATION AND MESSAGE VALIDATION SPECIFICATION

**Document Identifier:** `NLE-SPEC-SIM-002`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 80 of 100  
**Status:** AUTHORITATIVE SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior Distributed-Systems Engineer, Senior QA Automation Engineer  
**Upstream Dependencies:** `NLE-ARCH-SIM-001` (Simulation Architecture), `NLE-CON-MEC-001` (Message Envelope Contract), `NLE-SPC-MRD-001` (Message Exchange Specification)  
**Downstream Consumed By:** Steps 81–87 (Capability Conformance, State Sync Simulation, Fault Injection)

---

## 1. Executive Summary and Scope

Step 80 establishes the synthetic communication plane for NexLudic Engine, enabling reproducible scenario execution, envelope validation, session-bound routing, and controlled failure injection without requiring live game processes or third-party network stacks.

The implementation strictly reflects the authoritative data plane established in **Phase 9 Step 73 (`NLE-CON-MEC-001` & `NLE-SPC-MRD-001`)**:
$$\text{Transport Delivery} \neq \text{Application Operation} \neq \text{Verified Gameplay Outcome}$$
$$\text{Created} \neq \text{Validated} \neq \text{Accepted} \neq \text{Transmitted} \neq \text{Received} \neq \text{Processing} \neq \text{Completed} \neq \text{Verified}$$

---

## 2. Hub-and-Spoke In-Memory Routing Architecture (`CMR-02`)

Direct peer-to-peer IPC between simulated game endpoints is strictly prohibited. All communication routes centrally through the **Central Message Router (`CentralMessageRouter`)**, which enforces sequential validation, rate limits, session boundaries, and audit logging.

```
+----------------------------------------------------------------------------------------------------+
|                                    CENTRAL MESSAGE ROUTER (CMR-02)                                 |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|    +-----------------------+                         +-----------------------+                     |
|    |  Synthetic Endpoint A |                         |  Synthetic Endpoint B |                     |
|    |      (Process A)      |                         |      (Process B)      |                     |
|    +-----------+-----------+                         +-----------^-----------+                     |
|                | Length-Prefixed Wire Frame                      | Verified Envelope               |
|                v                                                 |                                 |
|    +-----------+-----------+                         +-----------+-----------+                     |
|    | Inbound Ingestion     |                         | Outbound Dispatch     |                     |
|    +-----------+-----------+                         +-----------^-----------+                     |
|                |                                                 |                                 |
|                v                                                 |                                 |
|    +-------------------------------------------------------------+-----------------------------+   |
|    |                           SEVEN-STAGE SEQUENTIAL VALIDATION PIPELINE                      |   |
|    |  Stage 1: Framing & Header Invariants        Stage 5: Topic & Capability Authorization    |   |
|    |  Stage 2: Session State & Active Epoch       Stage 6: SHA-256 Digest Integrity            |   |
|    |  Stage 3: Sender Identity Attestation        Stage 7: Virtual Clock TTL Verification      |   |
|    |  Stage 4: Recipient Resolution                                                            |   |
|    +-------------------------------------------------------------------------------------------+   |
|                |                                                 ^                                 |
|                v                                                 |                                 |
|    +-----------------------+                         +-----------+-----------+                     |
|    | Deduplication Guard   |                         | Correlator & Journal  |                     |
|    | (Monotonic Seq Check) | ----------------------> | (RPC Request Matching)|                     |
|    +-----------------------+                         +-----------------------+                     |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

---

## 3. Seven-Stage Message Validation Pipeline

Every inbound `BridgeMessageEnvelope` passes through seven sequential validation gates:

| Stage | Name | Checked Invariants | Error Code |
| :---: | :--- | :--- | :---: |
| **1** | Envelope Framing & Structure | `envelope_version == "1.0"`, `message_id` non-empty with `msg-` prefix, non-empty endpoint IDs. | `ERR_PME_001`, `ERR_PME_002` |
| **2** | Session State & Epoch | `session_id` non-empty, session actively registered, session not expired, epoch matches active epoch. | `ERR_PME_005` |
| **3** | Sender Attestation | `sender_endpoint_id` registered in active session contract. Prevents sender spoofing. | `ERR_PME_006` |
| **4** | Recipient Resolution | `recipient_endpoint_id` registered in session, or `*` for authorized multicast. | `ERR_PME_007` |
| **5** | Topic Authorization | `topic` non-empty and explicitly authorized within session contract. | `ERR_PME_008` |
| **6** | Payload Digest Integrity | Computes SHA-256 digest of `payload_json` and compares bit-for-bit with `payload_digest`. | `ERR_PME_003` |
| **7** | Temporal Validity & TTL | `ttl_ms > 0` and `(Clock.UtcNow - TimestampUtc) <= ttl_ms`. | `ERR_PME_009` |

---

## 4. Message Category & Lifecycle Contract

Messages are classified into six discrete categories:
1. **`CONTROL_LIFECYCLE`:** Handshakes, heartbeat signals, session epoch invalidations.
2. **`COMMAND_REQUEST`:** RPC invocations requiring unique `correlation_id` and bounded `ttl_ms`.
3. **`COMMAND_RESPONSE`:** Return values mapped to matching `correlation_id`.
4. **`DATA_EVENT`:** Asynchronous state telemetry, inventory updates, world changes.
5. **`ACKNOWLEDGEMENT`:** Explicit receipt or processing confirmation envelopes.
6. **`ERROR_SIGNAL`:** Terminal protocol failure and exception reports.

---

## 5. Deduplication & Replay Guard (`DED-04`)

The router maintains an in-memory `DeduplicationGuard` enforcing:
1. **Message ID Uniqueness:** Every message UUID is checked against an in-memory hash set. Duplicates trigger `ERR_PME_004`.
2. **Monotonic Sequence Numbers:** Sequence numbers per sender must increase monotonically ($seq_{k+1} > seq_k$). Regressions trigger `ERR_PME_004`.
3. **Sequence Gap Detection:** Out-of-order sequence arrivals ($seq_{k+1} > seq_k + 1$) are flagged in diagnostic traces for downstream re-ordering.
