# STATE SYNCHRONIZATION BUFFERING, BACKPRESSURE AND RESOURCE POLICY

**Document Identifier:** `NLE-POL-SBB-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Resource Governance  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-CON-SID-001` (Step 75), `NLE-POL-QBR-001` (Step 73)  
**Downstream Dependents:** `NLE-CON-SOD-001` (Step 75), `NLE-CAT-LSS-001` (Step 75)

---

## 1. Purpose and Scope

This policy defines the authoritative limits, bounded buffer allocations, backpressure flow-control signals, and overload management rules for cross-game state synchronization.

State updates in high-tick simulations (e.g. 60Hz or 120Hz) can rapidly generate megabytes of telemetry. If a receiving engine or bridge translation layer cannot keep pace, unconstrained memory allocation will cause out-of-memory crashes. This policy mandates deterministic bounding across all queues.

---

## 2. Mandatory Queue & Buffer Allocation Limits

```
+-----------------------------------+-----------------------------------+-----------------------------------+
| Resource / Queue Description      | Hard Upper Bound                  | Enforcement Action on Saturation  |
+-----------------------------------+-----------------------------------+-----------------------------------+
| 1. Pending Delta Ingestion Queue  | 512 deltas / 8 MiB per domain     | Apply backpressure signal; drop   |
| 2. Snapshot Staging Catch-Up Buffer| 1024 deltas / 16 MiB per domain   | Abort snapshot; trigger RESYNC    |
| 3. Max Single Snapshot Size       | 64 MiB total payload              | Reject snapshot as oversized      |
| 4. Deduplication Sliding Window   | 1024 tuples per stream            | Evict oldest in FIFO order        |
| 5. Diagnostic Quarantine Buffer   | 64 records per domain             | Ring buffer overwrite with log    |
| 6. Concurrent Sync Sessions       | 4 active bridge sessions          | Refuse new session admission      |
| 7. Max In-Flight Recovery Replays | 1 active replay stream per domain | Queue request or reject           |
+-----------------------------------+-----------------------------------+-----------------------------------+
```

---

## 3. Backpressure and Flow-Control Mechanisms

When a receiving coordinator's queue utilization reaches critical thresholds, it activates explicit flow-control protocols:

```
Queue Utilization:
[0% ------------------------ 70% ------------------------ 90% ------------------------ 100%]
         Normal                    Warning: Throttling           Critical: Drop & Resync
                                   - Send BACKPRESSURE_SLOWDOWN  - Emit ERR_SYNC_QUEUE_OVERFLOW
                                   - Coalesce safe values        - Transition to RESYNC_REQUIRED
```

### Flow-Control Signals:
1. **`SIGNAL_SYNC_SLOWDOWN`:** Instructs source publisher to throttle update frequency (e.g., from 60Hz to 30Hz or downsample transform stream).
2. **`SIGNAL_SYNC_PAUSE`:** Requests publisher to temporarily freeze streaming deltas while receiver processes backlog.

---

## 4. Safe Coalescing vs. Ordered Event Preservation

To mitigate overload without breaking simulation integrity, update types are treated differently:

```
+----------------------------------------------------------------------------------------------------+
| 1. COALESCEABLE VALUE UPDATES (Permitted):                                                         |
|    - Operation: SET_ATTRIBUTES (e.g. continuous vehicle position, player health float).            |
|    - Rule: If Queue contains [Delta_1: Pos=10, Delta_2: Pos=11, Delta_3: Pos=12],                 |
|            the receiver MAY discard Delta_1 and Delta_2, advancing directly to Delta_3.            |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
+----------------------------------------------------------------------------------------------------+
| 2. NON-COALESCEABLE ORDERED EVENTS (Strictly Prohibited from Coalescing):                          |
|    - Operations: ENTITY_SPAWN, ENTITY_DESPAWN, STATE_TRANSITION, DISCRETE_EVENT.                   |
|    - Rule: Discarding an intermediate spawn or state transition corrupts the target simulation.   |
|            Under overload, these updates MUST NOT be coalesced; if buffer is exhausted, the       |
|            domain MUST drop into RESYNC_REQUIRED rather than skipping intermediate operations.     |
+----------------------------------------------------------------------------------------------------+
```
