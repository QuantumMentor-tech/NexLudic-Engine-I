# STATE SYNCHRONIZATION INCREMENTAL DELTA CONTRACT

**Document Identifier:** `NLE-CON-SID-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Delta Exchange  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-CON-SSS-001` (Step 75), `NLE-CON-MVR-001` (Step 73)  
**Downstream Dependents:** `NLE-SPC-SDC-001` (Step 75), `NLE-POL-SFM-001` (Step 75)

---

## 1. Purpose and Scope

This contract specifies the schema, sequence tracking, revision advancement, operation types, deduplication rules, and replay bounds for **Incremental State Deltas**.

An Incremental Delta transmits state modifications that have occurred since a specified prior revision. It is the primary mechanism for continuous, bandwidth-efficient state replication.

---

## 2. Incremental Delta Data Schema

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/state-delta-v1.json",
  "title": "StateDeltaPayload",
  "type": "object",
  "required": [
    "delta_id",
    "sync_session_id",
    "session_epoch",
    "domain_id",
    "source_endpoint_id",
    "stream_id",
    "sequence_number",
    "source_revision",
    "expected_prior_revision",
    "operation_type",
    "timestamp_utc_us",
    "monotonic_timestamp_ns",
    "entities",
    "integrity_hash"
  ],
  "properties": {
    "delta_id": { "type": "string", "format": "uuid" },
    "sync_session_id": { "type": "string", "format": "uuid" },
    "session_epoch": { "type": "integer", "minimum": 1 },
    "domain_id": { "type": "string" },
    "source_endpoint_id": { "type": "string" },
    "stream_id": { "type": "string" },
    "sequence_number": { "type": "integer", "minimum": 1 },
    "source_revision": { "type": "integer", "minimum": 1 },
    "expected_prior_revision": { "type": "integer", "minimum": 0 },
    "operation_type": {
      "type": "string",
      "enum": [
        "SET_ATTRIBUTES",
        "DELTA_PATCH",
        "ENTITY_SPAWN",
        "ENTITY_DESPAWN",
        "STATE_TRANSITION",
        "DISCRETE_EVENT"
      ]
    },
    "is_coalesceable": { "type": "boolean" },
    "timestamp_utc_us": { "type": "integer" },
    "monotonic_timestamp_ns": { "type": "integer" },
    "entities": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["entity_ceid", "mutations"],
        "properties": {
          "entity_ceid": { "type": "string", "format": "uuid" },
          "mutations": { "type": "object" },
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

## 3. Delta Operation Semantics

| Operation Type | Semantic Meaning | Coalesceable? | Replay Safe? | Gap Behavior |
| :--- | :--- | :---: | :---: | :--- |
| **SET_ATTRIBUTES** | Complete replacement of declared attributes for entities. | YES | YES | Can skip intermediate if newer SET arrives. |
| **DELTA_PATCH** | Incremental modification relative to `expected_prior_revision`. | NO | CONDITIONAL | **Gap fatal:** Requires exact prior revision. |
| **ENTITY_SPAWN** | Introduces new entity into target simulation world. | NO | NO (Non-idempotent)| Gap fatal; requires ordered sequence. |
| **ENTITY_DESPAWN**| Removes entity from target simulation world. | NO | YES (Tombstone)| Gap fatal. |
| **STATE_TRANSITION**| Discrete state machine transition (e.g. Alive -> Dead). | NO | NO | Gap fatal. |
| **DISCRETE_EVENT**| One-off gameplay event (e.g. WeaponFired, DamageTaken). | NO | NO | Cannot be dropped or coalesced. |

---

## 4. State Revision Advancement Pipeline

To satisfy Axiom 2, the receiver tracks five distinct revision watermarks:

```
+----------------------------------------------------------------------------------------------------+
| 1. HIGHEST OBSERVED REVISION (R_obs): Highest revision delivered by network transport.             |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
+----------------------------------------------------------------------------------------------------+
| 2. HIGHEST VALIDATED REVISION (R_val): Highest revision passing 14-layer validation checks.        |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
+----------------------------------------------------------------------------------------------------+
| 3. HIGHEST ACCEPTED REVISION (R_acc): Highest revision staged in receiver's execution queue.       |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
+----------------------------------------------------------------------------------------------------+
| 4. HIGHEST APPLIED REVISION (R_app): Highest revision verified applied in host-game simulation.    |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
+----------------------------------------------------------------------------------------------------+
| 5. HIGHEST ACKNOWLEDGED REVISION (R_ack): Highest revision confirmed back to publisher.            |
+----------------------------------------------------------------------------------------------------+
```

### Inviolable Watermark Invariant:
$$R_{ack} \le R_{app} \le R_{acc} \le R_{val} \le R_{obs}$$
A revision watermark **shall never** advance past its preceding verification stage.

---

## 5. Duplicate Recognition & Replay Bounding

1. **Deduplication Sliding Window:** The receiver maintains a sliding ring buffer of the last $N = 1024$ processed `(delta_id, sequence_number)` tuples per stream.
2. **Duplicate Detection:**
   - If an incoming delta matches an entry in the deduplication window, it is identified as a duplicate.
   - For `SET_ATTRIBUTES`, it is dropped silently without state change.
   - For non-idempotent operations (`ENTITY_SPAWN`, `DISCRETE_EVENT`), the duplicate is rejected and an audit alert is emitted.
3. **Replay Bounding:** Replaying deltas is permitted **only** during an authorized resynchronization protocol (Section 11) using a signed `RecoveryReplayRequest`.
