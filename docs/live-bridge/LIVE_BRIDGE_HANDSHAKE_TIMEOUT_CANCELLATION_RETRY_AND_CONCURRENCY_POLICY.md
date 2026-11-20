# LIVE BRIDGE HANDSHAKE TIMEOUT, CANCELLATION, RETRY & CONCURRENCY POLICY

**Document ID:** `NLE-POL-TCR-001`  
**Classification:** Operational Governance & Reliability Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-CPN-001`, `NLE-SPC-CSF-001`, `NLE-REQ-FDR-001` (Step 70 Reliability)  
**Downstream Consumed By:** Step 73 (Semantic Translation), Step 75 (Health Monitoring)  

---

## 1. Multi-Phase Handshake Timeout Policy

Connection establishment involves discrete operational phases. To prevent hanging threads, pipe starvation, and resource exhaustion, each phase operates under strict, non-negotiable timeouts:

```
+------------------------------------+-------------------+--------------------+-----------------------+
| Operational Phase                  | Standard Timeout  | Hard Ceiling       | Action on Expiry      |
+------------------------------------+-------------------+--------------------+-----------------------+
| 1. Preflight Validation            | 500 ms            | 1,000 ms           | Abort request (`ERR_PREFLIGHT_TIMEOUT`). |
| 2. Transport Channel Binding       | 1,000 ms          | 2,000 ms           | Close socket (`ERR_TRANSPORT_TIMEOUT`).  |
| 3. Peer Handshake & Auth Token     | 500 ms            | 1,000 ms           | Sever pipe (`ERR_AUTH_TIMEOUT`).         |
| 4. Protocol & Capability Neg       | 1,000 ms          | 1,500 ms           | Emit reject (`ERR_NEGOTIATION_TIMEOUT`). |
| 5. Session Admission Gate          | 300 ms            | 500 ms             | Abort admission (`ERR_ADMISSION_TIMEOUT`).|
+------------------------------------+-------------------+--------------------+-----------------------+
| TOTAL ESTABLISHMENT CEILING        | 3,000 ms          | 5,000 ms           | Forceful cleanup of all handles.        |
+------------------------------------+-------------------+--------------------+-----------------------+
```

---

## 2. Cancellation Semantics

1. **Cooperative Cancellation Tokens:** Every asynchronous operation in the connection pipeline takes a `CancellationToken`.
2. **Immediate Abort on User Cancel:** If the user cancels session establishment via the UI, the coordinator triggers immediate cancellation:
   - Closes all pending transport listeners.
   - Nullifies ephemeral tokens.
   - Purges allocated memory buffers.
   - Sets session state to `CANCELLED`.
3. **No Phantom Sessions:** An interrupted or cancelled handshake **CANNOT** leave an active session in the router. If cancellation occurs while admission is in flight, the admission transaction is rolled back cleanly.

---

## 3. Retry Safety & Circuit Breakers

### 3.1 Retry Eligibility
- **Retryable Faults:** Transient transport binding timeouts (`ERR_TRANSPORT_TIMEOUT`), initial socket connect blips, or clean disconnects prior to authentication.
- **Non-Retryable Faults:** Authentication failures (`ERR_AUTH_CROSS_USER`, `ERR_AUTH_TOKEN_MISMATCH`), protocol version incompatibility, missing mandatory capabilities, or revoked endpoints. Retrying security rejections is strictly forbidden.

### 3.2 Jittered Exponential Backoff
When retrying eligible connection attempts:
$$t_{\text{retry}} = \min\left(5000\,\text{ms},\ 1000\,\text{ms} \times 2^{\text{attempt}} + \text{rand}(0, 250\,\text{ms})\right)$$
- Maximum retries allowed: **2 retries** (3 attempts total).
- If attempt 3 fails, the endpoint circuit breaker transitions to `OPEN` for 15 seconds.

---

## 4. Concurrency Governance & Race Condition Defenses

To prevent conflicting handshakes, split-brain routing, or races against administrative revocation:

1. **Per-Endpoint Connection Mutex:** A single canonical endpoint identity can participate in only **one active connection establishment handshake at any given instant**. Subsequent requests targeting the same endpoint while a handshake is in flight are queued or rejected (`ERR_CONCURRENCY_LOCKED`).
2. **Revocation Interruption Race:** If an administrative revocation event arrives while a handshake is in progress, the revocation takes immediate priority. The handshake cancellation token is triggered, and all connection resources are torn down immediately.
3. **Deterministic Memory Release:** In-flight frame buffers, preflight cache allocations, and transport handles are released via `finally` blocks, guaranteeing zero resource leakage on cancellation, timeout, or failure.
