# CROSS-GAME AUTHORITY ARBITRATION AND OWNERSHIP CONTRACT

**Document Identifier:** `NLE-CON-CAA-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Authority Governance  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-POL-SOC-001` (Step 75), `NLE-CON-EDR-001` (Step 71)  
**Downstream Dependents:** `NLE-POL-CEC-001` (Step 76), `NLE-CON-CRF-001` (Step 76)

---

## 1. Purpose and Scope

This contract specifies the formal rules, cryptographic evidence, scoping levels, and arbitration algorithms used by NexLudic Engine to evaluate competing authority claims over synchronized state concepts.

Authority determines which participant is permitted to mutate state. Without verified authority, all write attempts are rejected or quarantined.

---

## 2. Granular Authority Scoping Hierarchy

Authority is never evaluated as a crude binary switch across an entire game. It is declared and enforced across five granular scopes:

```
+-----------------------------------+-------------------------------------------------------------------+
| Scoping Level                     | Definition & Granularity Rules                                     |
+-----------------------------------+-------------------------------------------------------------------+
| 1. Domain-Level Authority         | Entire domain (e.g. `vehicle_physics`) owned by single endpoint.  |
| 2. Concept-Level Authority        | Specific concept within domain (e.g. `chassis_transform`).         |
| 3. Field-Level Authority          | Individual scalar property (e.g. `current_mana` vs `max_mana`).    |
| 4. Partitioned Exclusive Authority| Endpoint A owns Attributes [0..7]; Endpoint B owns [8..15].        |
| 5. Derived Coordinator Authority  | Coordinator owns calculated metric (e.g. relative distance).      |
+-----------------------------------+-------------------------------------------------------------------+
```

---

## 3. Required Authority Evidence

An endpoint claiming write authority over a domain or field must provide verifiable evidence satisfying three criteria:

1. **Session Capability Registration:** The endpoint's active `NegotiatedSessionContract` (Step 72) must include the `CAP_STATE_WRITE_AUTHORITY` capability token.
2. **Domain Ownership Binding:** The domain registration in the Session Registry must explicitly list the endpoint's canonical ID (`endpoint_ceid`).
3. **Current Epoch Validation:** The claim must reference the active session epoch $E_{active}$. Claims from prior epochs ($E < E_{active}$) are instantly disqualified.

**Inviolable Invariant:** Payload contents (e.g. an embedded `"is_authoritative": true` JSON tag) **shall never** serve as evidence of authority. Authority is established solely by external session contracts.

---

## 4. The 8-Stage Arbitration Algorithm

When competing or conflicting updates arrive for an overlapping concept, the coordinator executes deterministic arbitration:

```
Algorithm 1: Deterministic Authority Arbitration
Input: Competing Update Records U_1, U_2; Active Session Registry S
Output: Arbitration Verdict (SELECT_U1, SELECT_U2, ESCALATE_UNRESOLVED, REJECT_BOTH)

1. Verify Session Epoch:
     If U_1.epoch != S.active_epoch and U_2.epoch == S.active_epoch -> Return SELECT_U2.
     If U_2.epoch != S.active_epoch and U_1.epoch == S.active_epoch -> Return SELECT_U1.
     If both epochs invalid -> Return REJECT_BOTH.

2. Verify Scoped Ownership:
     O_1 = S.GetOwnership(U_1.endpoint_id, U_1.property_ref)
     O_2 = S.GetOwnership(U_2.endpoint_id, U_2.property_ref)
     If O_1 == AUTHORITATIVE and O_2 == READ_ONLY -> Return SELECT_U1.
     If O_2 == AUTHORITATIVE and O_1 == READ_ONLY -> Return SELECT_U2.

3. Evaluate Partitioning:
     If U_1 and U_2 target disjoint fields -> Return MERGE_PARTITIONED (Both valid).

4. Check Token Passing (Multi-Writer Domains):
     If domain uses TOKEN_PASSING:
        Validate which update holds active cryptographic token T_active.
        If U_1 holds T_active -> Return SELECT_U1; else Return SELECT_U2.

5. Authority Disputed / Ambiguous:
     If both endpoints claim valid authority over identical unpartitioned fields:
        Freeze domain at R_clean.
        Quarantine both U_1 and U_2.
        Emit ERR_SYNC_UNAUTHORIZED_WRITER / ERR_CONFLICT_AMBIGUOUS_AUTHORITY.
        Return ESCALATE_UNRESOLVED (NO Last-Write-Wins!).
```

---

## 5. Authority Transfer Protocols

Authority may transfer dynamically during a session (e.g. a player enters a vehicle, transferring control from character controller to vehicle simulator):
1. **Explicit Token Handshake:** The current authority emits an `AUTHORITY_RELEASE` message; the coordinator acknowledges and issues an `AUTHORITY_GRANT` token to the new authority.
2. **In-Flight Update Drain:** Any updates emitted by the former authority prior to the release token are applied if $R \le R_{release}$; subsequent updates are rejected.
3. **Disconnected Authority Handling:** If an authority disconnects ungracefully, authority does **not** silently fall back to another peer. The domain freezes until an authorized administrative transfer or session epoch increment occurs.
