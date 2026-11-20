# STATE SYNCHRONIZATION OWNERSHIP AND CONFLICT DETECTION POLICY

**Document Identifier:** `NLE-POL-SOC-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Authority Governance  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-POL-IES-001` (Step 74), `NLE-POL-REG-001` (Step 71)  
**Downstream Dependents:** `NLE-POL-SVI-001` (Step 75), `NLE-SPC-SRR-001` (Step 75)

---

## 1. Purpose and Scope

This policy governs the **Ownership Models**, authority validation, conflict detection protocols, and quarantine mechanisms for cross-game state synchronization.

In cross-game interoperability, allowing multiple independent game loops to write to the same logical state without explicit ownership causes split-brain state divergence. This policy establishes single-writer authority by default and mandates strict conflict quarantine without inventing unverified resolution rules.

---

## 2. Permitted State Ownership Models

Every synchronized property or domain must be registered under one of six explicit ownership models:

```
+------------------------------------+-------------------------------------------------------------------+
| Ownership Model                    | Authority & Directionality Rules                                  |
+------------------------------------+-------------------------------------------------------------------+
| 1. SINGLE_AUTHORITATIVE_WRITER     | Exactly one designated endpoint holds write rights; all others RO.|
| 2. SOURCE_OWNED_PROJECTED          | Source game pushes state; target creates ephemeral display proxy. |
| 3. PARTITIONED_EXCLUSIVE           | Endpoint A owns Property Set 1; Endpoint B owns Property Set 2.   |
| 4. READ_ONLY_MIRROR                | Bridge observes state for telemetry; zero writes to target game.  |
| 5. DERIVED_STATE_COORDINATOR_OWNED | Bridge coordinator computes state; neither game directly writes.   |
| 6. TOKEN_PASSING_MUTUALLY_EXCLUSIVE| Ownership transfers via explicit cryptographic token exchange.     |
+------------------------------------+-------------------------------------------------------------------+
```

**Prohibition:** Unpartitioned, simultaneous multi-writer state **is strictly prohibited** unless governed by an approved, specialized multi-writer conflict policy.

---

## 3. Conflict Detection Taxonomy

A conflict is detected whenever an incoming state update violates ownership, ordering, or invariant constraints:

```
+-----------------------------------+-------------------------------------------------------------------+
| Conflict Type                     | Detection Condition                                               |
+-----------------------------------+-------------------------------------------------------------------+
| 1. UNAUTHORIZED_WRITER            | Update originates from endpoint lacking write authority.          |
| 2. REVISION_REGRESSION            | Update advertises source_revision <= R_highest_applied.           |
| 3. INCOMPATIBLE_CONCURRENT_WRITES | Two endpoints emit updates for same property in same epoch.       |
| 4. EXPECTED_PRIOR_MISMATCH        | Delta expected_prior_revision != current target baseline.         |
| 5. OWNERSHIP_RACE_IN_FLIGHT       | Update emitted by previous owner arrives after ownership transfer.|
| 6. SEMANTIC_INVARIANT_VIOLATION   | Applied values produce physically impossible state (e.g. overlap).|
+-----------------------------------+-------------------------------------------------------------------+
```

---

## 4. Conflict Containment and Quarantine Protocol

When a conflict is detected, NexLudic Engine adheres to an inviolable safety mandate:
$$\textbf{Do NOT invent Last-Write-Wins (LWW), merge heuristics, or automatic overwrites.}$$
$$\textbf{Wall-clock timestamps alone must never pick a winner.}$$

### Standard Quarantine Procedure:
1. **Quarantine Payload:** The conflicting update is intercepted, tagged with `CONFLICT_QUARANTINED`, and prevented from entering the target game's simulation.
2. **Freeze Baseline:** The target domain freezes state progression at the last uncorrupted revision $R_{clean}$.
3. **Record Conflict Evidence:** An immutable diagnostic conflict record is generated:
   ```json
   {
     "conflict_id": "c7a8b9d0-1234-5678-9abc-def012345678",
     "detected_at": "2026-11-20T18:40:00.123456Z",
     "domain_id": "domain:hero_position",
     "conflict_type": "INCOMPATIBLE_CONCURRENT_WRITES",
     "claimed_writers": ["ue5_endpoint_01", "unity_endpoint_02"],
     "quarantined_revisions": [1042, 1043],
     "action_taken": "BASELINE_FROZEN_AT_R1041_RECOVERY_TRIGGERED"
   }
   ```
4. **Escalate to Recovery:** The domain transitions to `RESYNC_REQUIRED` to request a clean, authoritative baseline.
