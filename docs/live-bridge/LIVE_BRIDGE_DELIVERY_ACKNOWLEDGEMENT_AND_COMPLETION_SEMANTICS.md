# LIVE BRIDGE DELIVERY, ACKNOWLEDGEMENT & COMPLETION SEMANTICS

**Document ID:** `NLE-SPC-DAC-001`  
**Classification:** Delivery Protocol & Reliability Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 73 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-SPC-MRD-001`, `NLE-MOD-RRC-001`  
**Downstream Consumed By:** `NLE-POL-DRI-001`, Steps 74–78  

---

## 1. Scope & Core Verification Distinction

This specification establishes the formal delivery, acknowledgement, and completion semantics for all message categories within the NexLudic Engine bridge.

To eliminate ambiguity across cross-game interactions, the platform enforces an inviolable separation between communication milestones and verified gameplay effects:

$$\text{Created} \neq \text{Transmitted} \neq \text{Received} \neq \text{Accepted} \neq \text{Completed} \neq \text{Verified}$$

```
+-------------------+---------------------------------------------------------------------------------+
| Delivery State    | Precise Technical Definition                                                    |
+-------------------+---------------------------------------------------------------------------------+
| CREATED           | Message frame constructed locally by authorized caller.                         |
| TRANSMITTED       | OS transport reported successful write into physical pipe/socket buffer.       |
| RECEIVED          | Recipient endpoint transport layer has read the complete framed byte stream.    |
| ACCEPTED          | Recipient validation pipeline passed; message admitted to dispatch queue.       |
| PROCESSING        | Recipient handler has initiated internal game-state operation.                  |
| COMPLETED         | Recipient handler has concluded operation and returned terminal result frame.   |
| UNCERTAIN         | Timeout or disconnect occurred while remote execution was pending.              |
| VERIFIED          | Independent telemetry / audit receipts confirm intended gameplay state change.  |
+-------------------+---------------------------------------------------------------------------------+
```

---

## 2. Acknowledgement Contract (`CAT-ACK`)

When Quality-of-Service tiers (QoS 1 or QoS 2) require confirmed transmission, the recipient emits structured `ACKNOWLEDGEMENT` frames:

```json
{
  "$schema": "https://schemas.nexludic.dev/v1/bridge/acknowledgement.json",
  "version": "1.0",
  "message_type": "ACKNOWLEDGEMENT",
  "session_id": "sess-01938a4b-7777-7000-8801-abcdef998877",
  "referenced_message_id": "msg-01938a4b-7c12-7000-8801-abcdef123456",
  "ack_stage": "ACCEPTED_FOR_PROCESSING",
  "timestamp_utc": "2026-11-20T17:40:01.050Z",
  "recipient_endpoint_id": "valve:half-life-2:a8f9c12e5d:inst-01938a4b"
}
```

### 2.1 Supported Acknowledgement Stages
- **`ACK_STAGE_RECEIVED`:** Confirms frame framing and envelope syntax passed at recipient boundary.
- **`ACK_STAGE_ACCEPTED`:** Confirms capability authorization and payload schema validation passed.
- **`ACK_STAGE_PROCESSED`:** Optional progress marker before terminal `COMMAND_RESPONSE`.

---

## 3. Delivery Guarantees by Category

| Category | Protocol QoS Tier | Guarantee Definition | Loss Handling |
| :--- | :--- | :--- | :--- |
| **`DATA_EVENT` (Telemetry)** | **QoS 0 (Best-Effort)** | Fire-and-forget. Zero ACK required. Frame dropped under queue saturation. | Discarded silently or logged in rate limiter metrics. |
| **`COMMAND_REQUEST` (RPC)** | **QoS 1 (At-Least-Once)** | Sender tracks pending request; expects ACK / Response within `ttl_ms`. | Retransmit on timeout (if idempotent) or fail with UNCERTAIN. |
| **`CONTROL_LIFECYCLE`** | **QoS 1 (At-Least-Once)** | Mandatory handshake/heartbeat ACK. Missing 3 heartbeats triggers termination. | Reconnect or session termination. |
| **Transactional Mutation** | **QoS 2 (Exactly-Once)** | Coordinated two-phase commit handshake with deduplication journal. | Rollback / state reconciliation. |
