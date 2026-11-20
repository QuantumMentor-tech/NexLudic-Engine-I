# CROSS-GAME CONFLICT OBSERVABILITY, DIAGNOSTICS AND AUDIT CONTRACT

**Document Identifier:** `NLE-CON-COA-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Observability & Audit  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-RAV-001` (Step 76), `NLE-CON-SOD-001` (Step 75)  
**Downstream Dependents:** `NLE-CAT-CRR-001` (Step 76), `NLE-MAT-STP-076` (Step 76)

---

## 1. Purpose and Scope

This contract specifies the structured audit records, sixteen diagnostic event types, explainability standards, and telemetry metrics for cross-game conflict resolution.

Reconciliation actions must be fully transparent, explainable to human operators, and preserved in tamper-evident diagnostic records without leaking sensitive player credentials or memory pointers.

---

## 2. Authoritative Conflict Audit Record Schema

Every resolved, unresolved, or escalated conflict produces an immutable JSON-LD **Conflict Audit Record**:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/conflict-audit-record-v1.json",
  "title": "ConflictAuditRecord",
  "type": "object",
  "required": [
    "audit_record_id",
    "conflict_id",
    "timestamp_utc",
    "sync_session_id",
    "session_epoch",
    "domain_id",
    "competing_endpoints",
    "detected_category",
    "severity_tier",
    "evidence_dossier_summary",
    "selected_policy_id",
    "applied_strategy",
    "approval_provenance",
    "reconciliation_outcome",
    "verification_strength",
    "human_explanation"
  ],
  "properties": {
    "audit_record_id": { "type": "string", "format": "uuid" },
    "conflict_id": { "type": "string", "format": "uuid" },
    "timestamp_utc": { "type": "string", "format": "date-time" },
    "sync_session_id": { "type": "string", "format": "uuid" },
    "session_epoch": { "type": "integer" },
    "domain_id": { "type": "string" },
    "competing_endpoints": { "type": "array", "items": { "type": "string" } },
    "detected_category": { "type": "string" },
    "severity_tier": { "type": "string" },
    "selected_policy_id": { "type": "string" },
    "applied_strategy": { "type": "string" },
    "approval_provenance": { "type": "string" },
    "reconciliation_outcome": { "type": "string" },
    "verification_strength": { "type": "string" },
    "human_explanation": {
      "type": "object",
      "required": ["verified_facts", "policy_rationale", "uncertainties"],
      "properties": {
        "verified_facts": { "type": "array", "items": { "type": "string" } },
        "policy_rationale": { "type": "string" },
        "uncertainties": { "type": "array", "items": { "type": "string" } }
      }
    }
  }
}
```

---

## 3. Sixteen Structured Diagnostic Event Types

```
+------------------------------------+-------------------------------------------------------------------+
| Event Type Identifier              | Semantic Trigger Condition                                        |
+------------------------------------+-------------------------------------------------------------------+
| 1. `EVT_CONFLICT_DETECTED`         | Ingestion filter identifies competing incompatible observations. |
| 2. `EVT_CONFLICT_CONFIRMED`        | Evidence dossier proves invariant or authority violation.         |
| 3. `EVT_CONFLICT_DISMISSED`        | Discrepancy confirmed as expected temporal/partitioned difference.|
| 4. `EVT_AUTHORITY_DISPUTED`        | Competing endpoints hold contradictory or ambiguous authority.    |
| 5. `EVT_POLICY_INELIGIBLE`         | Candidate policy fails one or more eligibility gates.             |
| 6. `EVT_RESOLUTION_PROPOSED`       | Automated or AI-advisory proposal generated for review.          |
| 7. `EVT_APPROVAL_REQUESTED`        | High-stakes conflict routed to human approval gate.               |
| 8. `EVT_APPROVAL_GRANTED`          | Cryptographic signature received; operation unlocked.             |
| 9. `EVT_APPROVAL_REJECTED`         | Human operator or policy timeout rejected candidate resolution.   |
| 10. `EVT_PRECONDITION_FAILED`      | TOCTOU race detected; target revision advanced in interim.        |
| 11. `EVT_RESOLUTION_DISPATCHED`    | State mutation dispatched to target adapter.                      |
| 12. `EVT_OUTCOME_APPLIED`          | Target adapter reported successful mutation execution.           |
| 13. `EVT_OUTCOME_UNCERTAIN`        | Adapter timed out or returned ambiguous execution outcome.        |
| 14. `EVT_VERIFICATION_PASSED`      | Independent read-back and domain invariants confirmed valid.      |
| 15. `EVT_VERIFICATION_FAILED`      | Read-back disagreed or domain invariant failed post-write.        |
| 16. `EVT_CONFLICT_ESCALATED`       | Automated recovery exhausted; conflict permanently quarantined.   |
+------------------------------------+-------------------------------------------------------------------+
```

---

## 4. Telemetry Metrics Matrix

- `conflict_detected_total{domain="..."}`: Counter of all candidate conflicts.
- `conflict_confirmed_total{category="..."}`: Counter of confirmed conflicts by category.
- `conflict_resolved_total{strategy="...", outcome="..."}`: Counter of resolution outcomes.
- `conflict_stale_writes_prevented_total{domain="..."}`: Counter of TOCTOU races blocked.
- `conflict_quarantine_active_gauge{domain="..."}`: Gauge of currently quarantined domains.
