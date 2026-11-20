# STATE SYNCHRONIZATION SNAPSHOT EXCHANGE CONTRACT

**Document Identifier:** `NLE-CON-SSS-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Baseline Exchange  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-CON-STD-001` (Step 74), `NLE-CON-SEC-001` (Step 74)  
**Downstream Dependents:** `NLE-CON-SID-001` (Step 75), `NLE-SPC-SDC-001` (Step 75)

---

## 1. Purpose and Scope

This contract defines the schema, observation semantics, validation pipeline, and publication boundaries for **Initial State Snapshots**. 

A Snapshot establishes the authoritative initial state baseline for a synchronization domain before incremental deltas can be processed. Without an authorized and validated snapshot, no incremental state progression is permitted.

---

## 2. State Snapshot Data Schema

A valid State Snapshot conforms to the following formal JSON Schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/state-snapshot-v1.json",
  "title": "StateSnapshotPayload",
  "type": "object",
  "required": [
    "snapshot_id",
    "sync_session_id",
    "session_epoch",
    "domain_id",
    "source_endpoint_id",
    "mapping_record_urn",
    "cutoff_revision",
    "observation_semantics",
    "clock_semantics",
    "timestamp_utc_us",
    "monotonic_timestamp_ns",
    "completeness_status",
    "payload_entities",
    "integrity_hash"
  ],
  "properties": {
    "snapshot_id": { "type": "string", "format": "uuid" },
    "sync_session_id": { "type": "string", "format": "uuid" },
    "session_epoch": { "type": "integer", "minimum": 1 },
    "domain_id": { "type": "string" },
    "source_endpoint_id": { "type": "string" },
    "mapping_record_urn": { "type": "string" },
    "cutoff_revision": { "type": "integer", "minimum": 0 },
    "observation_semantics": {
      "type": "string",
      "enum": [
        "ATOMIC_SIMULATION_PAUSE",
        "REVISION_BOUND_TRANSACTION",
        "BEST_EFFORT_SINGLE_FRAME",
        "PARTIAL_CHUNKED_OBSERVATION"
      ]
    },
    "clock_semantics": {
      "type": "string",
      "enum": ["UTC_WALLCLOCK_AND_MONOTONIC_TICKS", "MONOTONIC_TICKS_ONLY"]
    },
    "timestamp_utc_us": { "type": "integer" },
    "monotonic_timestamp_ns": { "type": "integer" },
    "completeness_status": {
      "type": "string",
      "enum": ["COMPLETE_BASELINE", "PARTIAL_SEGMENT", "DEGRADED_COVERAGE"]
    },
    "segment_index": { "type": "integer", "minimum": 0 },
    "total_segments": { "type": "integer", "minimum": 1 },
    "payload_entities": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["entity_ceid", "state_properties"],
        "properties": {
          "entity_ceid": { "type": "string", "format": "uuid" },
          "state_properties": { "type": "object" },
          "vault_token": { "type": "string" }
        }
      }
    },
    "integrity_hash": {
      "type": "string",
      "pattern": "^sha256:[a-f0-9]{64}$"
    }
  }
}
```

---

## 3. Source Observation Semantics

The coordinator must evaluate the source game's observation capability before accepting a snapshot:

| Observation Semantic | Technical Meaning | Consistency Level | Permitted Usage |
| :--- | :--- | :---: | :--- |
| **ATOMIC_SIMULATION_PAUSE** | Game engine paused; entire world captured in single tick. | Highest | Unrestricted baseline creation. |
| **REVISION_BOUND_TRANSACTION** | Engine captures state matching specific tick $R$ without pausing. | High | Unrestricted baseline creation. |
| **BEST_EFFORT_SINGLE_FRAME** | State read across multiple subsystems during live rendering. | Bounded | Permitted only for non-physics telemetry; flagged as lossy. |
| **PARTIAL_CHUNKED_OBSERVATION**| Large world transferred in multiple segments over several ticks. | Asynchronous | Permitted only after all segments received and stitched. |

**Inviolable Rule:** If a source provides only `BEST_EFFORT_SINGLE_FRAME`, the coordinator **shall never** advertise it as an atomic snapshot.

---

## 4. The 12-Stage Snapshot Validation Pipeline

Every snapshot must clear twelve deterministic validation gates before becoming an active baseline:

```
[Raw Snapshot Payload Received]
             |
             v
1. Format & Schema Validation (JSON-LD Schema Draft 2020-12)
2. Session & Epoch Validation (sync_session_id matches active; epoch == current_epoch)
3. Endpoint Authorization Check (Source possesses WRITE permission for domain_id)
4. Domain Registration Match (domain_id declared in NegotiatedSessionContract)
5. Semantic Mapping Compatibility (mapping_record_urn matches approved active version)
6. Cutoff Revision Verification (cutoff_revision >= highest known prior baseline)
7. Completeness Verification (completeness_status == COMPLETE_BASELINE or all segments present)
8. Range & Physical Invariant Check (Coordinates clamped to limits; no NaNs)
9. Canonical Entity ID Resolution (All entity_ceid strings valid in EntityRegistry)
10. Vault Token Cryptographic Check (HMAC validation on embedded vault tokens)
11. Freshness & Age Verification (|t_now - t_snapshot| <= max_snapshot_age_ms)
12. Checksum Integrity Hash Verification (SHA-256 matches computed payload hash)
             |
             v
[Snapshot VALIDATED & Ready for Publication]
```

---

## 5. Snapshot Publication Boundary

To prevent partial or corrupt state from leaking to target game consumers, snapshots follow strict publication states:

```
  +-------------------------------------------------------+
  |                     TRANSFERRING                      |
  |  (Payload or chunks being received across transport)  |
  +-------------------------------------------------------+
                              |
                              | All chunks received & assembled
                              v
  +-------------------------------------------------------+
  |                      VALIDATING                       |
  |      (Executing 12-Stage Validation Pipeline)         |
  +-------------------------------------------------------+
               /                             \
  Validation  /                               \ Validation
    Pass     /                                 \ Fail
            v                                   v
  +--------------------+             +--------------------+
  |     PUBLISHED      |             |      REJECTED      |
  | (Active Baseline)  |             | (Audit Log Emitted)|
  +--------------------+             +--------------------+
```

Consumers are notified of a new baseline **only when** the snapshot reaches `PUBLISHED` state.
