# STATE SYNCHRONIZATION CONSISTENCY, FRESHNESS AND MONITORING POLICY

**Document Identifier:** `NLE-POL-SFM-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Temporal Governance  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-CON-SID-001` (Step 75), `NLE-CON-MVF-001` (Step 74)  
**Downstream Dependents:** `NLE-POL-SOC-001` (Step 75), `NLE-CON-SOD-001` (Step 75)

---

## 1. Purpose and Scope

This policy governs the evaluation, classification, and monitoring of **State Freshness**, temporal degradation, and cross-domain consistency across participating game engines.

A core principle of NexLudic Engine is that **a healthy transport connection does not imply fresh state**. A socket may be actively sending heartbeats while a game simulation is frozen in a loading screen or paused in a menu. This policy establishes strict criteria for distinguishing connection liveness from state timeliness.

---

## 2. Decoupled Health and Freshness Indicators

The synchronization coordinator maintains eight independent telemetry indicators for each active domain:

```
+-----------------------------------+-------------------------------------------------------------------+
| Telemetry Indicator               | Evaluation Method & Semantics                                     |
+-----------------------------------+-------------------------------------------------------------------+
| 1. Transport Connectivity         | Socket/Pipe heartbeat status (L4 connection liveness).            |
| 2. Last Message Receipt           | Timestamp of most recent envelope delivered by bridge transport.  |
| 3. Last Valid Update Observation  | Timestamp of most recent update passing 14-layer validation.      |
| 4. Last Successful Application    | Timestamp of most recent update confirmed applied in game world.  |
| 5. Last Confirmed Source Revision | Monotonic revision counter emitted by authoritative source engine.|
| 6. Latest Known State Age         | Measured delta: t_eval_monotonic - t_update_monotonic.            |
| 7. Declared Freshness Limit       | Configured SLA threshold (Delta t_freshness_limit) for domain.    |
| 8. Domain Synchronization Health  | Compound state evaluated from indicators 1 through 7.            |
+-----------------------------------+-------------------------------------------------------------------+
```

---

## 3. Five-State Freshness Classification Model

Every active synchronization domain is classified into one of five formal freshness states:

```
+---------------+-------------------------------------------------------------------+-----------------------------------+
| State         | Entry Condition                                                   | Operational Impact                |
+---------------+-------------------------------------------------------------------+-----------------------------------+
| **CURRENT**   | Age <= Delta t_freshness_limit (default: 250 ms for physics).    | Unrestricted state consumption.   |
| **AGING**     | Delta t_freshness_limit < Age <= 2 * Delta t_freshness_limit.      | Warning logged; dead reckoning.   |
| **STALE**     | Age > 2 * Delta t_freshness_limit.                                | Gameplay integrations flagged;    |
|               |                                                                   | physics extrapolation throttled.  |
| **UNKNOWN**   | Monotonic clock synchronization missing or unverified.            | Displayed as UNVERIFIED; no sync. |
| **INVALID**   | State timestamp is in the future (> 100ms clock skew) or corrupt. | Payload dropped; error logged.    |
+---------------+-------------------------------------------------------------------+-----------------------------------+
```

### Stale State Impact Rules:
1. **Downstream Gating:** State marked `STALE` **shall never** be passed to high-stakes combat or physics solvers.
2. **User Visibility:** The UI plugin surfaces a degraded status pill (`SYNC_DEGRADED_STALE`) when a domain enters `STALE`.
3. **No Silent Freezing:** The engine does not freeze or pause the game automatically; it isolates the affected domain and refers to the session failure policy.

---

## 4. Cross-Domain Consistency & Dependency Invariants

When multiple domains have physical or logical dependencies (e.g., Domain A: Vehicle Chassis Transform, Domain B: Wheel Suspension Positions, Domain C: Passenger Actor Transforms):

1. **Independent vs. Coordinated Baselines:** Unless a multi-domain snapshot is explicitly coordinated at the same revision $R$, domains are assumed to be **independently consistent**.
2. **Skew Limit:** If dependent domains exhibit revision skew $\Delta R = |R_A - R_B| > 5$ ticks, dependent child domains (e.g. Passengers) are held in intermediate interpolation buffers until the parent domain catches up.
3. **Partial Synchronization Transparency:** If Domain A is `CURRENT` but Domain B is `STALE`, the coordinator reports:
   ```json
   {
     "composite_health": "PARTIALLY_SYNCHRONIZED",
     "domains": {
       "chassis_transform": "CURRENT",
       "wheel_suspension": "STALE"
     }
   }
   ```
   The engine **shall never** average or falsely claim global consistency when child domains are desynchronized.
