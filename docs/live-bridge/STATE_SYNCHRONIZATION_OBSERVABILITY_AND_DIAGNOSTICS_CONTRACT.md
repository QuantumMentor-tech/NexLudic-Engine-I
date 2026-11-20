# STATE SYNCHRONIZATION OBSERVABILITY AND DIAGNOSTICS CONTRACT

**Document Identifier:** `NLE-CON-SOD-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Telemetry & Diagnostics  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-POL-SFM-001` (Step 75), `NLE-CON-MDA-001` (Step 73)  
**Downstream Dependents:** `NLE-CAT-LSS-001` (Step 75), `NLE-MAT-STP-075` (Step 75)

---

## 1. Purpose and Scope

This contract specifies the structured telemetry metrics, diagnostic events, correlation headers, and authoritative error taxonomy for cross-game state synchronization.

---

## 2. Authoritative Error Taxonomy (15 Formal Error Codes)

```
+------------------------------------+-------------------------------------------------------------------+
| Error Identifier                   | Diagnostic Description & Remediation Action                       |
+------------------------------------+-------------------------------------------------------------------+
| 1. `ERR_SYNC_INVALID_SNAPSHOT`     | Snapshot failed 12-stage validation. Drop and log error.          |
| 2. `ERR_SYNC_INVALID_DELTA`        | Delta payload malformed or fails schema. Drop delta.              |
| 3. `ERR_SYNC_UNKNOWN_SESSION`      | Update refers to non-existent session ID. Reject immediately.     |
| 4. `ERR_SYNC_STALE_EPOCH`          | Update epoch < active session epoch. Drop as obsolete message.    |
| 5. `ERR_SYNC_UNAUTHORIZED_WRITER`  | Endpoint lacks write authority for domain. Quarantine payload.   |
| 6. `ERR_SYNC_REVISION_GAP`         | Sequence gap in delta stream. Trigger Bounded Replay or Resync.   |
| 7. `ERR_SYNC_DUPLICATE_DELTA`      | Duplicate delta sequence observed. Drop or ignore idempotently.   |
| 8. `ERR_SYNC_MAPPING_INCOMPATIBLE` | Mapping URN not found or incompatible. Halt incremental stream.   |
| 9. `ERR_SYNC_SCHEMA_INCOMPATIBLE`  | Domain Concept Descriptor version mismatch. Require fresh snapshot|
| 10. `ERR_SYNC_STALE_STATE`         | State age exceeds 2x freshness limit. Flag domain as STALE.       |
| 11. `ERR_SYNC_STATE_DIVERGENCE`    | Inconsistent state detected across participants. Force resync.    |
| 12. `ERR_SYNC_QUEUE_OVERFLOW`      | Ingestion or staging queue exhausted. Discard and trigger resync. |
| 13. `ERR_SYNC_APPLICATION_UNKNOWN` | Target cannot confirm state applied. Log unconfirmed status.      |
| 14. `ERR_SYNC_RECOVERY_UNAVAILABLE`| Source cannot provide replay or fresh snapshot. Suspend domain.   |
| 15. `ERR_SYNC_RECOVERY_EXHAUSTED`  | Recovery failed 3 consecutive times. Transition domain to FAILED. |
+------------------------------------+-------------------------------------------------------------------+
```

---

## 3. Structured Diagnostic Event Schema

Every synchronization event is emitted as a structured, machine-parsable JSON-LD document:

```json
{
  "event_id": "7d9b2e1a-4567-89ab-cdef-0123456789ab",
  "timestamp_utc": "2026-11-20T18:45:00.123456Z",
  "sync_session_id": "5f3c7e42-1234-4567-89ab-cdef01234567",
  "session_epoch": 3,
  "domain_id": "domain:hero_position",
  "event_type": "SYNC_REVISION_GAP_DETECTED",
  "severity": "WARNING",
  "watermarks": {
    "R_observed": 1045,
    "R_validated": 1042,
    "R_applied": 1042,
    "gap_size": 2
  },
  "action_taken": "REQUESTED_BOUNDED_DELTA_REPLAY_RANGE_1043_1044",
  "correlation_id": "corr:live_bridge:ue5_unity:tick_1045"
}
```

---

## 4. Telemetry Metrics Matrix

The engine publishes real-time Prometheus-compatible metrics for bridge monitoring:
- `sync_domain_freshness_age_ms{domain="..."}`: Gauge of latest state age.
- `sync_deltas_applied_total{domain="..."}`: Counter of successfully applied updates.
- `sync_revision_gaps_total{domain="..."}`: Counter of detected sequence gaps.
- `sync_queue_utilization_ratio{queue="..."}`: Gauge of queue saturation ($0.0 - 1.0$).
- `sync_recovery_events_total{domain="...", outcome="..."}`: Counter of resynchronization attempts.
