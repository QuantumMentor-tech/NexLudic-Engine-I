# Capability Negotiation Result and Explainability Contract Specification

**Document Identifier:** `NLE-CON-NRE-001`  
**Phase:** Phase 5 — Universal Adapter SDK & Capability System  
**Step:** Step 39 of 100 — Capability Negotiation & Contextual Availability Resolution Specification  
**Status:** Approved Architectural Contract  
**Classification:** Result Data Schema, Explainable Diagnostics & Downstream Handoff Protocol  
**Repository Path:** `docs/adapters/CAPABILITY_NEGOTIATION_RESULT_AND_EXPLAINABILITY_CONTRACT.md`  

---

## 1. Executive Summary & Purpose

### 1.1 Purpose
This specification establishes the standardized data payload, explainable diagnostics, and downstream handoff protocol for capability negotiation and contextual availability results within the NexLudic Engine.

When the negotiation engine finishes evaluating a `CapabilityNegotiationRequest` (`NLE-CON-CNC-001`), it emits a comprehensive `CapabilityNegotiationResultPayload`. This payload provides an immutable, machine-readable record detailing:
1. The exact joint capability configuration agreed upon by the participants.
2. The negotiated operational parameters (ranges, tick rates, coordinate transformations).
3. The real-world contextual availability state of each capability in the local host environment.
4. An actionable, human-readable explainability rationale outlining remaining limitations, unverified assumptions, and next steps for runtime test harness execution (Step 40).

---

## 2. Capability Negotiation Result Payload Schema (`NLE-SCH-NRP-001`)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/v1/capability-negotiation-result-payload.json",
  "title": "CapabilityNegotiationResultPayload",
  "description": "Standardized output payload emitted by the NexLudic Capability Negotiation Subsystem.",
  "type": "object",
  "required": [
    "negotiationId",
    "parentProjectId",
    "evaluatedAt",
    "negotiatorVersion",
    "overallOutcome",
    "summaryMetrics",
    "negotiatedCapabilities",
    "jointSelectionExplanation",
    "nextSafeAction"
  ],
  "properties": {
    "negotiationId": { "type": "string", "format": "uuid" },
    "parentProjectId": { "type": "string", "format": "uuid" },
    "evaluatedAt": { "type": "string", "format": "date-time" },
    "negotiatorVersion": { "type": "string" },
    "overallOutcome": {
      "type": "string",
      "enum": [
        "RESOLVED",
        "RESOLVED_WITH_LIMITATIONS",
        "PARTIALLY_RESOLVED",
        "NEEDS_INFORMATION",
        "INCOMPATIBLE",
        "BLOCKED",
        "STALE",
        "CANCELLED",
        "FAILED"
      ]
    },
    "summaryMetrics": {
      "type": "object",
      "required": [
        "totalCapabilitiesEvaluated",
        "mandatoryAvailable",
        "mandatoryUnavailable",
        "optionalAvailable",
        "optionalDegraded",
        "hasResourceCollisions",
        "allMandatoryAvailable"
      ],
      "properties": {
        "totalCapabilitiesEvaluated": { "type": "integer", "minimum": 0 },
        "mandatoryAvailable": { "type": "integer", "minimum": 0 },
        "mandatoryUnavailable": { "type": "integer", "minimum": 0 },
        "optionalAvailable": { "type": "integer", "minimum": 0 },
        "optionalDegraded": { "type": "integer", "minimum": 0 },
        "hasResourceCollisions": { "type": "boolean" },
        "allMandatoryAvailable": { "type": "boolean" },
        "durationMs": { "type": "number", "minimum": 0 }
      }
    },
    "negotiatedCapabilities": {
      "type": "array",
      "items": { "$ref": "#/$defs/NegotiatedCapabilityRecord" }
    },
    "jointSelectionExplanation": {
      "type": "object",
      "required": [
        "selectionRationale",
        "constraintsSatisfied",
        "omittedOptionalCapabilities",
        "unresolvedConditions",
        "caveats"
      ],
      "properties": {
        "selectionRationale": { "type": "string" },
        "constraintsSatisfied": {
          "type": "array",
          "items": { "type": "string" }
        },
        "omittedOptionalCapabilities": {
          "type": "array",
          "items": { "$ref": "#/$defs/OmittedCapabilityRecord" }
        },
        "unresolvedConditions": {
          "type": "array",
          "items": { "type": "string" }
        },
        "caveats": {
          "type": "array",
          "items": { "type": "string" }
        }
      }
    },
    "nextSafeAction": {
      "type": "object",
      "required": ["actionType", "description"],
      "properties": {
        "actionType": {
          "type": "string",
          "enum": [
            "PROCEED_TO_TEST_HARNESS",
            "INSTALL_MOD_LOADER",
            "RESOLVE_CONFIGURATION_DISPUTE",
            "SELECT_ALTERNATIVE_ADAPTER",
            "ABORT_MASHUP"
          ]
        },
        "description": { "type": "string" }
      }
    }
  },
  "$defs": {
    "NegotiatedCapabilityRecord": {
      "type": "object",
      "required": [
        "canonicalCapabilityId",
        "selectedAdapterId",
        "registryRecordId",
        "manifestSha256",
        "declaredRole",
        "negotiatedParameters",
        "contextualAvailabilityState"
      ],
      "properties": {
        "canonicalCapabilityId": { "type": "string" },
        "selectedAdapterId": { "type": "string" },
        "registryRecordId": { "type": "string", "format": "uuid" },
        "manifestSha256": { "type": "string" },
        "declaredRole": { "type": "string", "enum": ["SOURCE", "TARGET", "BIDIRECTIONAL"] },
        "negotiatedParameters": { "type": "object" },
        "contextualAvailabilityState": {
          "type": "string",
          "enum": [
            "AVAILABLE_UNDER_ASSESSED_CONDITIONS",
            "CONDITIONALLY_AVAILABLE",
            "UNAVAILABLE",
            "UNKNOWN",
            "STALE",
            "BLOCKED",
            "NOT_ASSESSED"
          ]
        },
        "outstandingLimitations": {
          "type": "array",
          "items": { "type": "string" }
        },
        "caveats": {
          "type": "array",
          "items": { "type": "string" }
        }
      }
    },
    "OmittedCapabilityRecord": {
      "type": "object",
      "required": ["canonicalCapabilityId", "adapterId", "omissionReason", "ruleId"],
      "properties": {
        "canonicalCapabilityId": { "type": "string" },
        "adapterId": { "type": "string" },
        "omissionReason": { "type": "string" },
        "ruleId": { "type": "string" }
      }
    }
  }
}
```

---

## 3. Explainability & Diagnostics Standard

The negotiation result guarantees complete evidentiary transparency:
1. **Constraint Satisfaction Trace:** `constraintsSatisfied` lists every joint rule (`JSC-EXC-001`, `JSC-ENV-001`, etc.) evaluated and passed, providing the technical justification for the joint selection.
2. **Graceful Degradation Ledger:** Any optional capability dropped to prevent a deadlock is recorded in `omittedOptionalCapabilities` with rule ID and plain-English explanation.
3. **Discrete Metric Governance:** Rejects arbitrary percentage scores; surfaces discrete counts of mandatory available vs. unavailable capabilities.
4. **Actionable Remediation:** `nextSafeAction` instructs the user or orchestrator on the precise next step (e.g., prompt user to install SKSE64 vs. proceed to runtime testing).

---

## 4. Traceability and Cross-Reference Anchors

- `NLE-SPC-CNA-001`: Primary Capability Negotiation Specification.
- `NLE-CON-CNC-001`: Negotiation Context & Input Contract.
- `NLE-RUL-JSC-001`: Joint Selection & Constraint Resolution Rules.
- `NLE-MOD-CAN-001`: Contextual Availability & Negotiation State Model.
- `SCR-CMP-001`: Desktop UI Compatibility Assessment View.
