# CROSS-GAME RESOLUTION APPLICATION AND VERIFICATION CONTRACT

**Document Identifier:** `NLE-CON-RAV-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / State Verification  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-SPC-RSC-001` (Step 76), `NLE-SPC-CSW-001` (Step 76)  
**Downstream Dependents:** `NLE-POL-RIU-001` (Step 76), `NLE-SPC-RCR-001` (Step 76)

---

## 1. Purpose and Scope

This contract specifies the **13-Stage Reconciliation Execution Lifecycle**, the nine formal application outcome categories, and the six distinct verification strength levels.

A resolution operation cannot be declared successful merely because it was dispatched or acknowledged over the network. This contract mandates rigorous, independent verification before a domain can resume active synchronization.

---

## 2. The 13-Stage Reconciliation Lifecycle

```
+----------------------------------------------------------------------------------------------------+
| STAGE 1: DETECT CANDIDATE CONFLICT (Ingestion filter identifies discrepancy)                      |
+----------------------------------------------------------------------------------------------------+
| STAGE 2: COMPILE EVIDENCE DOSSIER (Gather revisions, epochs, mapping URNs, capability flags)       |
+----------------------------------------------------------------------------------------------------+
| STAGE 3: CLASSIFY CONFLICT (Authority, Concurrent-Write, Stale-Write, Semantic, Invariant)        |
+----------------------------------------------------------------------------------------------------+
| STAGE 4: ARBITRATE SOURCE AUTHORITY (Evaluate scoped ownership; verify active epoch)               |
+----------------------------------------------------------------------------------------------------+
| STAGE 5: MATCH ELIGIBLE RESOLUTION POLICY (Evaluate 8 policy eligibility gates)                    |
+----------------------------------------------------------------------------------------------------+
| STAGE 6: DECISION SELECTION OR ESCALATION (Automated selection or route to Human Approval Gate)    |
+----------------------------------------------------------------------------------------------------+
| STAGE 7: EVALUATE CONCURRENCY PRECONDITIONS (Check expected target revision and token TTL)         |
+----------------------------------------------------------------------------------------------------+
| STAGE 8: ISSUE RESOLUTION AUTHORIZATION (Issue cryptographically signed execution permit)           |
+----------------------------------------------------------------------------------------------------+
| STAGE 9: DISPATCH AND APPLY OPERATION (Transmit resolution mutation to target adapter)             |
+----------------------------------------------------------------------------------------------------+
| STAGE 10: OBSERVE TARGET RESULT (Capture adapter execution return code and status payload)         |
+----------------------------------------------------------------------------------------------------+
| STAGE 11: INDEPENDENT VERIFICATION (Perform read-back or domain invariant validation)              |
+----------------------------------------------------------------------------------------------------+
| STAGE 12: RECORD AUDIT DOSSIER (Write immutable reconciliation record to audit log)                |
+----------------------------------------------------------------------------------------------------+
| STAGE 13: RESUME, DEGRADE OR SUSPEND DOMAIN (Update FSM; notify bridge session manager)            |
+----------------------------------------------------------------------------------------------------+
```

---

## 3. Nine Authoritative Application Outcomes

Every reconciliation attempt terminates in exactly one of nine formal outcome categories:

```
+----------------------------------------+-------------------------------------------------------------------+
| Application Outcome Category           | Authoritative System Definition                                   |
+----------------------------------------+-------------------------------------------------------------------+
| 1. `OUTCOME_NOT_ATTEMPTED`             | Conflict dismissed or classified as expected difference.          |
| 2. `OUTCOME_REFUSED`                   | No eligible policy or safety invariants violated; writes refused. |
| 3. `OUTCOME_AWAITING_APPROVAL`         | Operation held pending required human cryptographic signature.    |
| 4. `OUTCOME_TARGET_REJECTED`           | Target adapter or host game explicitly refused mutation.          |
| 5. `OUTCOME_PARTIALLY_APPLIED`         | Multi-item operation succeeded in part; inconsistent state exists.|
| 6. `OUTCOME_UNCERTAIN`                 | Operation dispatched; target cannot confirm whether applied.      |
| 7. `OUTCOME_APPLIED_UNVERIFIED`        | Target confirmed receipt; independent read-back unavailable.       |
| 8. `OUTCOME_VERIFIED_SUCCESS`          | Operation applied and independently confirmed via read-back/check.|
| 9. `OUTCOME_VERIFICATION_FAILED`       | Operation applied, but subsequent read-back failed invariant test.|
+----------------------------------------+-------------------------------------------------------------------+
```

---

## 4. Hierarchy of Verification Strengths

```
[Level 1: Transport Ack] (Weakest: Packet delivered to host machine)
          |
          v
[Level 2: Adapter Acceptance Ack] (Adapter received mutation request)
          |
          v
[Level 3: Target Revision Advance] (Adapter increments internal tick)
          |
          v
[Level 4: Independent Read-Back] (Target state queried and matched)
          |
          v
[Level 5: Invariant Validation Pass] (Domain conservation & physical laws hold)
          |
          v
[Level 6: End-to-End Pair Qualification] (Strongest: Full cross-game semantic test)
```

**Honesty Mandate:** The engine **shall never** report `OUTCOME_VERIFIED_SUCCESS` if only Level 1, 2, or 3 evidence is available. If independent read-back (Level 4) is not supported by the adapter, the result must be reported honestly as `OUTCOME_APPLIED_UNVERIFIED`.
