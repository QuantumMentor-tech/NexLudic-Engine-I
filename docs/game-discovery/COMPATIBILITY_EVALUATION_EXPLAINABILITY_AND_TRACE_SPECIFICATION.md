# Compatibility Evaluation Explainability & Trace Specification

**Document ID:** `NLE-SPC-EET-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED`  
**Workstream:** `Compatibility Rule Architecture, Rule Taxonomy, Domain Schema, Lifecycle Governance, and Evaluation Contracts`  
**Phase:** `4 of 12 — Game Discovery & Compatibility Intelligence`  
**Step:** `31 of 100`  
**Related Documents:**
- [COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md) (`NLE-SPC-CRD-001`)
- [EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md) (`NLE-SPC-EPE-001`)
- [COMPATIBILITY_ASSESSMENT_OUTPUT_AND_REASSESSMENT_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_ASSESSMENT_OUTPUT_AND_REASSESSMENT_SPECIFICATION.md) (`NLE-SPC-CAO-001`)

---

## 1. Executive Summary & Purpose

This specification governs the audit trace schema, explainability generation contracts, and counterfactual change explanations produced by the compatibility evaluation engine within **NexLudic Engine**.

A compatibility verdict is useless to an engineer or user if presented as an opaque black box. Every material finding must be explainable in plain human language, traceable to specific evidence items and executed rules, and capable of explaining exactly why an outcome changed when environment conditions shifted. This document defines the `CompatibilityEvaluationTrace` schema and establishes strict semantic generation rules for user-facing conclusions.

---

## 2. Evaluation Trace Schema: `CompatibilityEvaluationTrace`

Every execution of the 10-stage evaluation pipeline emits a structured `CompatibilityEvaluationTrace` archived alongside the assessment on Volume D:

```json
{
  "trace_id": "a9b1c2d3-e4f5-6789-0123-abcdef012345",
  "assessment_id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "rule_set_version": "v1.0.0-approved",
  "evaluated_at_utc": "2026-11-17T19:23:00Z",
  "inputs_digest_sha256": "4f53cda18c2baa0c0354bb5f9a3ecbe5ed12ab4d8e11ba873c2f11161202b945",
  "executed_rule_results": [
    {
      "rule_id": "RULE-IDN-001",
      "rule_revision": 1,
      "outcome": "OUTCOME_SATISFIED",
      "supporting_evidence": ["ev-98a1-b2c3"]
    },
    {
      "rule_id": "RULE-SEC-001",
      "rule_revision": 1,
      "outcome": "OUTCOME_SATISFIED",
      "supporting_evidence": []
    },
    {
      "rule_id": "RULE-STR-LB-001",
      "rule_revision": 1,
      "outcome": "OUTCOME_SATISFIED",
      "supporting_evidence": ["ev-77d4-e5f6"]
    }
  ],
  "skipped_rules": [
    {
      "rule_id": "RULE-STR-HM-001",
      "reason": "NOT_APPLICABLE_STRATEGY_IS_LIVE_BRIDGE"
    }
  ],
  "triggered_blockers": [],
  "user_explanation": {
    "headline": "Integration Supported via Live Interoperability Bridge",
    "rationale": "Both game installations are verified on disk. Named Pipes IPC communication route is feasible with low latency.",
    "capability_breakdown": [
      {
        "capability_key": "cap:sync_player_gold",
        "status": "SUPPORTED",
        "explanation": "Verified via SKSE64 Named Pipe emitter and Baldur's Gate 3 Extender receiver.",
        "evidence_citations": ["SKSE64 API Spec v2.2.3", "BG3 Extender Release Notes v18"]
      }
    ],
    "actionable_next_step": "Configure bridge adapter port and launch validation sandbox."
  }
}
```

---

## 3. User-Facing Explanation Generation Contracts

Explanations must adhere to strict semantic generation templates based on the synthesized outcome:

| Outcome Status | Headline Template | Rationale Template | Actionable Next Step Template |
| :--- | :--- | :--- | :--- |
| **`SUPPORTED`** | *"Full Integration Supported via [Strategy]"* | *"[Game A] and [Game B] are verified. 100% of requested capabilities have proven technical evidence."* | *"Proceed to adapter configuration and sandbox testing."* |
| **`PARTIALLY_SUPPORTED`** | *"Partial Integration Supported ([M] of [N] capabilities met)"* | *"[Mandatory capabilities] are feasible. [Optional capabilities] lack documented hooks in [Game B]."* | *"Review optional capabilities or proceed with reduced scope."* |
| **`INSUFFICIENT_EVIDENCE`** | *"Exploratory Assessment: Build Verification Needed"* | *"[Game A] installation files have not been verified on disk. Technical constraints cannot be finalized."* | *"Locate game executable in Environment Settings to verify build."* |
| **`UNSUPPORTED`** | *"Integration Unsupported under Current Builds"* | *"[Strategy] cannot proceed due to [Technical Blocker, e.g. 32-bit / 64-bit architecture mismatch]."* | *"Select an alternative strategy (e.g. Live Bridge) or change game builds."* |
| **`PROHIBITED_BLOCKED`** | *"Integration Prohibited by Security Policy"* | *"[Game] employs active [Security Control, e.g. BattlEye anti-cheat]. Interfacing is strictly barred."* | *"Select an offline single-player game pair without active anti-cheat."* |

---

## 4. Counterfactual & Change Explanations

When an assessment re-evaluates an existing project and the outcome changes, the engine must generate an explicit **What Changed** diff explanation:

```
[Assessment Diff: Revision 1 -> Revision 2]
Status Change: SUPPORTED -> STALE_REASSESSMENT_REQUIRED

Change Explanation:
"Skyrim Special Edition was modified on disk by Steam (build changed from 1.6.640 to 1.6.1170).
Mod loader SKSE64 v2.2.3 is incompatible with this new executable build.
Previous compatibility conclusions for capability 'cap:sync_player_gold' are no longer valid."

Recommended Action:
"Update SKSE64 to version 2.2.6 (compatible with build 1.6.1170) and revalidate."
```

### 4.1 Invariants on Change Explanations
1. **Rule RUL-EET-01 (Factual Attribution):** The explanation must cite the exact input field or evidence record that changed (e.g. `file_timestamp_utc` or `build_id`). It must **never** hypothesize speculative causes.
2. **Rule RUL-EET-02 (Audit Retention):** The evaluation trace for every revision is preserved append-only in `D:\NexLudic\Projects\<id>\Environment\assessment_history.jsonl`.

---

## 5. Document Sign-Off & Verification

| Role | Reviewer | Verdict | Date |
| :--- | :--- | :--- | :--- |
| **Principal Software Architect** | Engine Architecture Team | Approved | 2026-11-17 |
| **Lead Desktop UX Architect** | Workspace Experience Team | Approved | 2026-11-17 |
| **Senior QA Automation Architect** | Test & Quality Assurance | Approved | 2026-11-17 |

