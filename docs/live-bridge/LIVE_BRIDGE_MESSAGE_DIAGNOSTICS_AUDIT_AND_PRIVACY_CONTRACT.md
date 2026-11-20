# LIVE BRIDGE MESSAGE DIAGNOSTICS, AUDIT & PRIVACY CONTRACT

**Document ID:** `NLE-CON-MDA-001`  
**Classification:** Operational Logging & Data Privacy Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 73 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-REQ-FDR-001`, `NLE-SPC-MRD-001`, `NLE-POL-QBR-001`  
**Downstream Consumed By:** Step 74 (Event Routing), Step 75 (Health Monitoring)  

---

## 1. Scope & Privacy Mandate

Operating in real-time across third-party games requires robust diagnostic observability without violating player privacy, leaking cryptographic secrets, or bloating system storage.

This contract defines the structured schema, privacy scrubbing rules, and audit retention policies governing message exchange logging in the **Telemetry & Diagnostic Journal (`TDJ-08`)**.

---

## 2. Structured Diagnostic Record Schema

Diagnostic events, routing errors, gate failures, and timeouts are journaled into `bridge-audit.jsonl` using a standardized JSON schema:

```json
{
  "$schema": "https://schemas.nexludic.dev/v1/bridge/diagnostic-record.json",
  "record_id": "diag-01938a4b-9999-7000-8801-abcdef987654",
  "timestamp_utc": "2026-11-20T17:40:02.456Z",
  "session_id": "sess-01938a4b-7777-7000-8801-abcdef998877",
  "message_id": "msg-01938a4b-7c12-7000-8801-abcdef123456",
  "sender_endpoint_id": "mojang:minecraft-java:99b1c43f01:inst-01938a4c",
  "recipient_endpoint_id": "valve:half-life-2:a8f9c12e5d:inst-01938a4b",
  "topic": "inventory.item.transfer",
  "event_category": "GATE_FAILURE",
  "severity": "WARNING",
  "error_code": "ERR_TOPIC_UNAUTHORIZED",
  "diagnostics": {
    "failed_gate": "STAGE_5_CAPABILITY_AUTHORIZATION",
    "requested_topic": "inventory.item.transfer",
    "authorized_topics": ["telemetry.player.spatial"],
    "action_taken": "FRAME_DROPPED_ERROR_SIGNAL_RETURNED"
  }
}
```

---

## 3. Privacy Safeguards & Secret Redaction Rules

1. **Payload Data Minimization:** Full payload bodies are **NEVER** logged in production diagnostic journals. Payloads are represented strictly by their SHA-256 `payload_digest` and byte length.
2. **Secret Scrubbing:** Handshake tokens, session keys, and authentication nonces are completely redacted or replaced with one-way salted hashes before writing to disk.
3. **Safe String Sanitization:** Untrusted text strings originating from game memory or malformed frames are stripped of control characters, shell escapes, and ANSI escape sequences to prevent log injection or console corruption.
4. **Permanent Audit Retention:** Diagnostic failure records, authentication rejections, and session closure events are preserved in the append-only log for audit traceability, while ephemeral telemetry events are discarded after session termination.
