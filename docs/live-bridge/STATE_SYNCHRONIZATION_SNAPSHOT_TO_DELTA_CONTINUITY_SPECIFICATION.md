# STATE SYNCHRONIZATION SNAPSHOT-TO-DELTA CONTINUITY SPECIFICATION

**Document Identifier:** `NLE-SPC-SDC-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Continuity Protocol  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-CON-SSS-001` (Step 75), `NLE-CON-SID-001` (Step 75)  
**Downstream Dependents:** `NLE-POL-SFM-001` (Step 75), `NLE-SPC-SRR-001` (Step 75)

---

## 1. Purpose and Scope

This specification defines the critical transition protocol from **Initial State Snapshot** establishment to **Incremental Delta Streaming**.

Because a snapshot represents state at a discrete cutoff revision $R_{cutoff}$, while delta streams flow continuously across live game loops, an uncoordinated transition risks creating an unobserved temporal gap or double-applying updates. This document defines the exact buffering, ordering, and activation rules to guarantee seamless continuity.

---

## 2. The Snapshot-to-Delta Continuity Problem

```
Source Game Timeline:
---[R=10]---[R=11]---[R=12]---[R=13]---[R=14]---[R=15]---[R=16]--->
             ^                  ^                         ^
             |                  |                         |
      Snapshot Cutoff     Snapshot En Route         Live Streaming
        Captured             Across Bridge              Head
        (R_cutoff=11)
```

If the receiver applies Snapshot $R=11$ and then immediately accepts Live Delta $R=16$, revisions $12, 13, 14, 15$ are permanently lost, causing severe state warping.

---

## 3. The 4-Phase Continuity Protocol

To guarantee continuity, the coordinator executes a 4-phase protocol:

```
+----------------------------------------------------------------------------------------------------+
| PHASE 1: SNAPSHOT INITIATION & DELTA BUFFERING                                                     |
| - Publisher announces Snapshot with cutoff_revision = R_cutoff.                                    |
| - Receiver creates bounded Delta Staging Buffer (size = 512 deltas).                               |
| - Incoming deltas with source_revision > R_cutoff are placed into Staging Buffer.                  |
| - Deltas with source_revision <= R_cutoff are discarded as already covered by snapshot.           |
+----------------------------------------------------------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
| PHASE 2: SNAPSHOT VALIDATION & BASELINE ESTABLISHMENT                                              |
| - Snapshot payload completes transfer and passes 12-Stage Validation Pipeline.                     |
| - Snapshot state is applied to target simulation world.                                            |
| - Baseline revision initialized: R_app = R_cutoff.                                                 |
+----------------------------------------------------------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
| PHASE 3: CATCH-UP DRAIN & GAP VERIFICATION                                                         |
| - Receiver drains Staging Buffer in strict sequence order starting at R_cutoff + 1.                |
| - If delta with source_revision == R_app + 1 exists: Apply delta, advance R_app = R_app + 1.       |
| - Repeat until Staging Buffer is empty or a revision gap is encountered.                           |
| - If a gap is detected (e.g. Buffer has R=12, then R=14, missing R=13):                           |
|     ABORT continuity catch-up; Transition domain to RESYNC_REQUIRED.                               |
+----------------------------------------------------------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
| PHASE 4: TRANSITION TO ACTIVE STREAMING                                                            |
| - Staging Buffer drained to live head.                                                             |
| - Domain state transitions from SYNCHRONIZING to ACTIVE.                                           |
| - Subsequent live deltas are processed directly on the hot path.                                  |
+----------------------------------------------------------------------------------------------------+
```

---

## 4. Continuity Invariants & Guardrails

1. **Strict Buffer Bounding:** The Staging Buffer has a hard capacity limit of $M = 1024$ updates or $16 \text{ MiB}$. If the snapshot transfer takes too long and the buffer fills, the coordinator **must not** drop deltas silently. It halts transfer, discards the stale snapshot, and transitions to `RESYNC_REQUIRED` (`ERR_SYNC_CONTINUITY_BUFFER_OVERFLOW`).
2. **First Delta Cutoff Alignment:** The very first delta applied after snapshot installation **must** have:
   $$\text{expected\_prior\_revision} == R_{cutoff}$$
   $$\text{source\_revision} == R_{cutoff} + 1$$
   Any deviation triggers an immediate continuity fault.
3. **Timeout Protection:** If Phase 3 catch-up does not complete within $T_{catchup} = 2.0 \text{ seconds}$, the catch-up is aborted as degraded.
