# Capability Match Result and Explainability Contract Specification

**Document Identifier:** `NLE-CON-MRE-001`  
**Phase:** Phase 5 — Universal Adapter SDK & Capability System  
**Step:** Step 38 of 100 — Capability Matching & Requirement Resolution Specification  
**Status:** Approved Architectural Contract  
**Classification:** Result Data Schema, Explainable Diagnostics & Downstream Handoff Protocol  
**Repository Path:** `docs/adapters/CAPABILITY_MATCH_RESULT_AND_EXPLAINABILITY_CONTRACT.md`  

---

## 1. Executive Summary & Purpose

### 1.1 Purpose
This specification establishes the standardized data payload, diagnostic structure, and explainability contract for capability matching results within the NexLudic Engine.

When the matching engine processes a `CapabilityMatchingRequest` (`NLE-MOD-CRM-001`), it emits a comprehensive `CapabilityMatchResultPayload`. This payload provides a machine-readable, fully auditable trace of every requirement evaluation, detailing selected candidate adapters, disqualified alternatives, resolved dependency trees, and actionable next steps—without relying on unverified assumptions or opaque numerical scores.

### 1.2 Anti-Deception & Explainability Invariants
- **Twelve-Question Explainability Standard:** Every requirement resolution finding must answer the twelve (12) standard architectural explainability questions (Section 3). Natural language guessing or AI-generated hallucinations are strictly prohibited.
- **Explicit Gaps Over Synthetic Summaries:** The result payload never condenses findings into a single percentage (e.g., "85% Ready"). Instead, it presents discrete tallies: mandatory requirements resolved, mandatory requirements blocked, and optional features deferred.
- **Traceable Handoff to Compatibility Assessment:** Every finding references the exact `requirementId`, `registryRecordId`, `manifestSha256`, rule version, and evidence records (`EVD-xxxx`), providing the downstream Cross-Game Compatibility Assessment Engine (`docs/game-discovery/`) with an immutable evidentiary audit trail.

---

## 2. Capability Match Result Payload Schema (`NLE-SCH-MRP-001`)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/v1/capability-match-result-payload.json",
  "title": "CapabilityMatchResultPayload",
  "description": "Standardized output payload emitted by the NexLudic Capability Matching Engine.",
  "type": "object",
  "required": [
    "requestId",
    "parentProjectId",
    "evaluatedAt",
    "matcherVersion",
    "ruleCatalogVersion",
    "overallResolutionState",
    "summaryMetrics",
    "requirementFindings"
  ],
  "properties": {
    "requestId": { "type": "string", "format": "uuid" },
    "parentProjectId": { "type": "string", "format": "uuid" },
    "evaluatedAt": { "type": "string", "format": "date-time" },
    "matcherVersion": { "type": "string" },
    "ruleCatalogVersion": { "type": "string" },
    "overallResolutionState": {
      "type": "string",
      "enum": [
        "FULLY_RESOLVED",
        "RESOLVED_WITH_LIMITATIONS",
        "UNRESOLVED_MANDATORY_GAPS",
        "BLOCKED_BY_POLICY",
        "INCOMPLETE_EVALUATION"
      ]
    },
    "summaryMetrics": {
      "type": "object",
      "required": [
        "totalRequirements",
        "mandatoryResolved",
        "mandatoryUnresolved",
        "mandatoryBlocked",
        "optionalResolved",
        "optionalUnresolved",
        "conditionalEvaluated",
        "allMandatoryResolved"
      ],
      "properties": {
        "totalRequirements": { "type": "integer", "minimum": 0 },
        "mandatoryResolved": { "type": "integer", "minimum": 0 },
        "mandatoryUnresolved": { "type": "integer", "minimum": 0 },
        "mandatoryBlocked": { "type": "integer", "minimum": 0 },
        "optionalResolved": { "type": "integer", "minimum": 0 },
        "optionalUnresolved": { "type": "integer", "minimum": 0 },
        "conditionalEvaluated": { "type": "integer", "minimum": 0 },
        "allMandatoryResolved": { "type": "boolean" },
        "durationMs": { "type": "number", "minimum": 0 }
      }
    },
    "requirementFindings": {
      "type": "array",
      "items": { "$ref": "#/$defs/RequirementFinding" }
    }
  },
  "$defs": {
    "RequirementFinding": {
      "type": "object",
      "required": [
        "requirementId",
        "canonicalCapabilityId",
        "criticality",
        "resolutionStatus",
        "matchRationale",
        "nextSafeAction"
      ],
      "properties": {
        "requirementId": { "type": "string", "format": "uuid" },
        "canonicalCapabilityId": { "type": "string" },
        "criticality": {
          "type": "string",
          "enum": ["MANDATORY", "OPTIONAL", "CONDITIONAL", "ALTERNATIVE"]
        },
        "resolutionStatus": {
          "type": "string",
          "enum": [
            "RESOLVED",
            "RESOLVED_WITH_LIMITATIONS",
            "UNRESOLVED",
            "BLOCKED",
            "NOT_APPLICABLE"
          ]
        },
        "selectedCandidate": {
          "$ref": "#/$defs/SelectedCandidateSummary"
        },
        "alternativeCandidates": {
          "type": "array",
          "items": { "$ref": "#/$defs/SelectedCandidateSummary" }
        },
        "excludedCandidates": {
          "type": "array",
          "items": { "$ref": "#/$defs/ExcludedCandidateRecord" }
        },
        "dependencySummary": {
          "type": "object",
          "required": ["totalDependencies", "resolvedDependencies", "unresolvedDependencies", "hasCycles"],
          "properties": {
            "totalDependencies": { "type": "integer" },
            "resolvedDependencies": { "type": "integer" },
            "unresolvedDependencies": { "type": "integer" },
            "hasCycles": { "type": "boolean" },
            "topologicalOrder": { "type": "array", "items": { "type": "string" } }
          }
        },
        "matchRationale": {
          "type": "object",
          "required": [
            "rulesEvaluated",
            "conditionsPassed",
            "conditionsFailed",
            "confidenceSummary",
            "caveats"
          ],
          "properties": {
            "rulesEvaluated": { "type": "array", "items": { "type": "string" } },
            "conditionsPassed": { "type": "array", "items": { "type": "string" } },
            "conditionsFailed": { "type": "array", "items": { "type": "string" } },
            "confidenceSummary": { "type": "string" },
            "caveats": { "type": "array", "items": { "type": "string" } }
          }
        },
        "nextSafeAction": {
          "type": "object",
          "required": ["actionType", "description"],
          "properties": {
            "actionType": {
              "type": "string",
              "enum": ["PROCEED", "INSTALL_ADAPTER", "UPGRADE_GAME", "RESOLVE_DEPENDENCY", "SELECT_ALTERNATIVE", "ABORT"]
            },
            "description": { "type": "string" }
          }
        }
      }
    },
    "SelectedCandidateSummary": {
      "type": "object",
      "required": [
        "adapterId",
        "registryRecordId",
        "manifestSha256",
        "candidateOutcome",
        "assignedVerificationState"
      ],
      "properties": {
        "adapterId": { "type": "string" },
        "registryRecordId": { "type": "string", "format": "uuid" },
        "manifestSha256": { "type": "string" },
        "adapterRole": { "type": "string" },
        "candidateOutcome": {
          "type": "string",
          "enum": ["FULL_MATCH", "PARTIAL_MATCH", "CONDITIONAL_MATCH", "UNKNOWN"]
        },
        "assignedVerificationState": { "type": "string" },
        "versionScope": { "type": "object" },
        "limitations": { "type": "array", "items": { "type": "string" } }
      }
    },
    "ExcludedCandidateRecord": {
      "type": "object",
      "required": ["adapterId", "registryRecordId", "exclusionReason", "diagnosticCode", "message"],
      "properties": {
        "adapterId": { "type": "string" },
        "registryRecordId": { "type": "string", "format": "uuid" },
        "exclusionReason": { "type": "string" },
        "diagnosticCode": { "type": "string" },
        "message": { "type": "string" }
      }
    }
  }
}
```

---

## 3. The 12-Question Explainability Standard

To guarantee complete auditability, every `RequirementFinding` must be capable of answering twelve explicit questions:

| # | Explainability Dimension | Location in Finding Schema | Concrete Example |
|:---:|:---|:---|:---|
| **1** | Which requirement was evaluated? | `requirementId`, `purpose` | `"Sync player inventory items between Skyrim and OpenMW"` |
| **2** | Which candidate was considered? | `selectedCandidate.adapterId` | `"adapter.skyrim.inventory_bridge"` |
| **3** | Which canonical capability applies? | `canonicalCapabilityId` | `"entities.inventory.item_transfer"` |
| **4** | Which context was compared? | `gamePairContext` | `Skyrim v1.6.640 (windows-x64) vs OpenMW v0.48 (windows-x64)` |
| **5** | Which matching rules were applied? | `matchRationale.rulesEvaluated` | `["MTR-IDN-001", "MTR-CTX-002", "MTR-STR-001", "MTR-OUT-001"]` |
| **6** | Which conditions passed? | `matchRationale.conditionsPassed` | `["Canonical ID Exact Match", "SemVer In Scope (1.5.0-1.6.640)", "Anti-Cheat Clean"]` |
| **7** | Which conditions failed / unknown? | `matchRationale.conditionsFailed` | `["Optional particle sync capability not found"]` |
| **8** | Which dependencies affected outcome? | `dependencySummary` | `Requires SKSE v2.2.3 (Resolved); Requires OpenMW-Lua (Resolved)` |
| **9** | What evidence was consumed? | `selectedCandidate.evidenceCitations`| `EVD-2026-0412 (Local observation: Item transfer verified)` |
| **10** | Why was the candidate selected? | `matchRationale.confidenceSummary`| `"Exact version match with empirical evidence and zero missing prerequisites."` |
| **11** | What would be required to change finding? | `nextSafeAction` | `"No action required; requirement is fully resolved."` |
| **12** | What does this result NOT establish? | `matchRationale.caveats` | `"Does NOT establish live wire-protocol synchronization latency (Step 39 required)."` |

---

## 4. Traceability and Cross-Reference Anchors

- `NLE-SPC-CMR-001`: Primary Capability Matching Specification.
- `NLE-MOD-CRM-001`: Capability Requirement Model & Input Contract.
- `NLE-CAT-MTR-001`: Matching Rule Catalog & Decision Tables.
- `NLE-RUL-CDR-001`: Capability Dependency Resolution & Composition Rules.
- `NLE-POL-CCR-001`: Candidate Comparison & Resolution Policy.
- `docs/game-discovery/`: Step 29/31 Compatibility Assessment Pipeline.
- `SCR-CMP-001`: Desktop UI Compatibility Assessment View.
