# LIVE BRIDGE ROUTING & RECIPIENT RESOLUTION CONTRACT

**Document ID:** `NLE-CON-RRR-001`  
**Classification:** Interface Contract & Message Routing Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 73 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-CON-MEC-001`, `NLE-POL-MVA-001`  
**Downstream Consumed By:** `NLE-MOD-RRC-001`, Steps 74–78  

---

## 1. Routing Architecture & Gateway Topology

All cross-game message exchange in NexLudic Engine operates via a **Hub-and-Spoke Gateway Routing Architecture** governed by the **Central Message Router (`CMR-02`)**.

Direct peer-to-peer IPC between game executables is prohibited. Centralized routing guarantees that all traffic is validated, authorized, rate-limited, and journaled.

```
+-----------------------------------------------------------------------------------------+
|                              CENTRAL MESSAGE ROUTER (CMR-02)                            |
|                                                                                         |
|       +------------------------------------+   +------------------------------------+   |
|       |         INBOUND CHANNEL A          |   |         INBOUND CHANNEL B          |   |
|       |     (Game Endpoint A - Process A)  |   |     (Game Endpoint B - Process B)  |   |
|       +-----------------+------------------+   +-----------------+------------------+   |
|                         |                                        |                      |
|                         v                                        v                      |
|                  [Inbound Queue A]                        [Inbound Queue B]             |
|                         |                                        |                      |
|                         \--------------------\  /----------------/                      |
|                                               v v                                       |
|                               +--------------------------------+                        |
|                               |  ROUTING & DISPATCH ENGINE     |                        |
|                               | - Recipient Resolution Table   |                        |
|                               | - Unicast / Multicast Dispatch |                        |
|                               | - Flow Control / Backpressure  |                        |
|                               +---------------+----------------+                        |
|                                               |                                         |
|                         /--------------------/ \----------------\                      |
|                         v                                        v                      |
|                  [Outbound Queue A]                       [Outbound Queue B]            |
|                         |                                        |                      |
|                         v                                        v                      |
|       +-----------------+------------------+   +-----------------+------------------+   |
|       |        OUTBOUND CHANNEL A          |   |        OUTBOUND CHANNEL B          |   |
|       |     (Game Endpoint A - Process A)  |   |     (Game Endpoint B - Process B)  |   |
|       +------------------------------------+   +------------------------------------+   |
+-----------------------------------------------------------------------------------------+
```

---

## 2. Recipient Resolution Contract

The router resolves the destination channel for every message based strictly on `recipient_endpoint_id`:

### 2.1 Unicast Resolution ($1 \rightarrow 1$)
- When `recipient_endpoint_id` specifies a canonical endpoint ID (e.g. `valve:half-life-2:a8f9c12e5d:inst-01938a4b`):
  1. Router inspects the active session routing table.
  2. If the recipient matches an active participant in the session, the frame is enqueued into the recipient's outbound channel queue.
  3. If the recipient is not part of the session, the frame is rejected with `ERR_ROUTE_UNKNOWN_RECIPIENT`.
  4. If the recipient's channel is suspended or disconnected, the frame is rejected with `ERR_ROUTE_RECIPIENT_UNAVAILABLE`.

### 2.2 Multicast Fan-Out Resolution ($1 \rightarrow N$)
- When `recipient_endpoint_id` equals `*` (Broadcast/Multicast):
  1. The message must be of category `DATA_EVENT`. Commands and requests **CANNOT** use broadcast routing.
  2. The router evaluates each registered participant in the session independently.
  3. Each participant must independently possess authorization for the event topic.
  4. Frames are enqueued to all authorized recipients. Partial delivery is tracked per recipient.

---

## 3. Origin Traceability & Non-Rewriting Invariant

1. **Preservation of Origin Identity:** The Central Message Router **NEVER** rewrites the `sender_endpoint_id` or `message_id` of a routed frame. The recipient always sees the exact original sender identity.
2. **Intermediary Signatures:** If routing telemetry is attached, it is appended to an explicit routing metadata header (`x-router-hop`) without altering the canonical envelope body or payload digest.
3. **No Unapproved Forwarding:** Endpoints cannot instruct the router to relay messages to third-party network addresses or unverified local processes.
