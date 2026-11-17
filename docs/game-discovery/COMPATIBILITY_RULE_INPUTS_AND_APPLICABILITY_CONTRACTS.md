# Compatibility Rule Inputs & Applicability Contracts

**Document ID:** `NLE-CAT-RIA-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED`  
**Workstream:** `Compatibility Rule Architecture, Rule Taxonomy, Domain Schema, Lifecycle Governance, and Evaluation Contracts`  
**Phase:** `4 of 12 — Game Discovery & Compatibility Intelligence`  
**Step:** `31 of 100`  
**Related Documents:**
- [COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_RULE_CATALOG_AND_DOMAIN_SPECIFICATION.md) (`NLE-SPC-CRD-001`)
- [EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/EXPLAINABLE_COMPATIBILITY_EVALUATION_PIPELINE_SPECIFICATION.md) (`NLE-SPC-EPE-001`)
- [COMPATIBILITY_RULE_DEPENDENCY_AND_CONFLICT_RESOLUTION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/COMPATIBILITY_RULE_DEPENDENCY_AND_CONFLICT_RESOLUTION_SPECIFICATION.md) (`NLE-SPC-RDC-001`)
- [GAME_ENVIRONMENT_PROFILE_AND_COMPATIBILITY_CONTEXT_MODEL.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_ENVIRONMENT_PROFILE_AND_COMPATIBILITY_CONTEXT_MODEL.md) (`NLE-DOM-ENV-001`)

---

## 1. Executive Summary & Purpose

This contract establishes the formal specifications for the input domains, input validation states, applicability preconditions, and standardized evaluation outcome codes consumed and produced by compatibility rules within **NexLudic Engine**.

Rules must never execute in an unconstrained environment or read arbitrary ambient variables. Every rule operates against a strictly bounded set of authorized inputs. This document defines the behavior of rules when inputs are missing, unknown, conflicting, or stale, and enforces deterministic distinctions between rules that are not applicable, rules that fail, and rules that cannot proceed due to upstream blockers.

---

## 2. Permitted Input Domain Catalog

Rules are permitted to consume inputs strictly from nine authorized domain contracts:

```mermaid
flowchart TD
    subgraph AuthorizedInputs ["The Nine Permitted Input Domains"]
        I1["1. Canonical Game Metadata (CanonicalGame)"]
        I2["2. Version & Build Snapshot (GameVersionBuild)"]
        I3["3. Local Installation State (LocalInstallationInstance)"]
        I4["4. Host Environment Profile (GameEnvironmentProfile)"]
        I5["5. User Mashup Requirements (UserMashupRequirements)"]
        I6["6. Evidence Items & Provenance (CompatibilityEvidenceRecord)"]
        I7["7. Claim Linkage Findings (EvidenceClaimLink)"]
        I8["8. Candidate Strategy Selection (StrategyType)"]
        I9["9. Security & Anti-Cheat Profile (SecurityPolicyProfile)"]
    end
    
    style I1 fill:#f9f0ff,stroke:#6b46c1,stroke-width:1px
    style I2 fill:#ebf8ff,stroke:#2b6cb0,stroke-width:1px
    style I3 fill:#e6fffa,stroke:#234e52,stroke-width:1px
    style I4 fill:#fffaf0,stroke:#7b341e,stroke-width:1px
    style I5 fill:#f0fff4,stroke:#22543d,stroke-width:1px
    style I6 fill:#edf2f7,stroke:#4a5568,stroke-width:1px
    style I7 fill:#fffaf0,stroke:#dd6b20,stroke-width:1px
    style I8 fill:#edf2f7,stroke:#4a5568,stroke-width:1px
    style I9 fill:#fff5f5,stroke:#9b2c2c,stroke-width:2px
```

### 2.1 Input Data Invariants
1. **Rule RUL-RIA-01 (No Undocumented Inputs):** Rules are strictly prohibited from consuming environment variables, arbitrary filesystem paths outside the profile, host usernames, or undocumented network APIs.
2. **Rule RUL-RIA-02 (Local Path Isolation):** Rules evaluate architectural attributes (bitness, build IDs, DLL presence); they never evaluate or display raw host drive paths (`install_path`).

---

## 3. Formal Input Validation States

When an input is supplied to a rule, the evaluation engine classifies the input across six formal validation states:

| Input Validation State | Meaning & Definition | Engine Behavior |
| :--- | :--- | :--- |
| **`INPUT_CONFIRMED_VERIFIED`** | Input value is cryptographically verified (SHA-256 match) or confirmed via authoritative manifest. | Full rule evaluation authorized. |
| **`INPUT_DECLARED_UNVERIFIED`**| Input is asserted by user without local filesystem verification. | Rule evaluates under `PROVISIONAL` mode; cannot authorize execution. |
| **`INPUT_UNKNOWN`** | Field is unpopulated or undetectable (e.g. unknown build version). | Triggers non-blocking unknown handling; rule yields `INSUFFICIENT_EVIDENCE`. |
| **`INPUT_CONFLICTING`** | Two contradictory values supplied for the same field. | Suspends deterministic evaluation; sets rule state to `EVIDENCE_CONFLICT`. |
| **`INPUT_STALE`** | Value was verified in past, but disk timestamp indicates recent file modification. | Flags input as stale; triggers reassessment notice. |
| **`INPUT_CORRUPTED`** | Value violates structural schema or fails type constraints. | Rejects input immediately; logs input corruption warning. |

---

## 4. Rule Applicability Preconditions

Before executing a rule, the pipeline evaluates its **Applicability Contract**. A rule does not run simply because it exists in the catalog.

```mermaid
flowchart TD
    Rule["Candidate Rule"] --> AppCheck{"Evaluate Applicability Preconditions"}
    
    AppCheck -->|"Strategy Mismatch (e.g. Host Mod rule on Bridge project)"| NA["Status: NOT_APPLICABLE<br/>• Silently skipped<br/>• Does NOT count as failure"]
    AppCheck -->|"Prerequisites Met & Inputs Verified"| Eval["Status: APPLICABLE_AND_EVALUATED<br/>• Executes evaluation logic<br/>• Returns deterministic outcome"]
    AppCheck -->|"Prerequisites Met but Required Input Missing"| Incomplete["Status: APPLICABLE_BUT_INCOMPLETE<br/>• Returns INSUFFICIENT_EVIDENCE<br/>• Identifies missing prerequisite"]
    AppCheck -->|"Upstream Hard Blocker Active (e.g. Anti-Cheat)"| Blocked["Status: APPLICABLE_BUT_BLOCKED<br/>• Evaluation bypassed<br/>• Inherits upstream blocker"]
    
    style NA fill:#edf2f7,stroke:#4a5568,stroke-width:1px
    style Eval fill:#f0fff4,stroke:#22543d,stroke-width:2px
    style Incomplete fill:#fffaf0,stroke:#7b341e,stroke-width:1px
    style Blocked fill:#fff5f5,stroke:#9b2c2c,stroke-width:2px
```

### 4.1 Applicability Invariants
1. **`NOT_APPLICABLE` $\ne$ Failure:** If a rule assessing in-process 32-bit/64-bit memory injection is skipped because the user selected *Live Interoperability Bridge*, the rule is marked `NOT_APPLICABLE`. It does **not** indicate a failure.
2. **Missing Input $\ne$ Incompatibility:** If a rule requires an exact game build number, and the build is currently `INPUT_UNKNOWN`, the rule outputs `OUTCOME_INSUFFICIENT_EVIDENCE`. It must **never** conclude that the game is incompatible.

---

## 5. Standardized Rule Outcome Codes

To ensure cross-system determinism, all rules must return one of six standardized outcome codes:

| Outcome Code | Meaning & Significance | Downstream Effect |
| :--- | :--- | :--- |
| **`OUTCOME_SATISFIED`** | Preconditions and evaluation logic fully met with adequate evidence. | Contributes positive finding to requirement coverage. |
| **`OUTCOME_PARTIALLY_SATISFIED`**| Primary conditions met, but non-critical limitations or optional gaps identified. | Contributes partial finding; flags advisory notice. |
| **`OUTCOME_UNSATISFIED`** | Technical conditions not met; required capability is demonstrably absent. | Flags capability as unsupported under current build. |
| **`OUTCOME_BLOCKED_BY_POLICY`** | Mandatory security, anti-cheat, or licensing restriction triggered. | **Critical Blocker:** Hard-blocks live execution. |
| **`OUTCOME_INSUFFICIENT_EVIDENCE`**| Required inputs or evidence items are unknown or unverified. | Preserves open research state; prompts user for input. |
| **`OUTCOME_EVIDENCE_CONFLICT`** | Contradictory evidence items exist within same authority tier. | Flags claim for Human Review Queue triage. |

---

## 6. Document Sign-Off & Verification

| Role | Reviewer | Verdict | Date |
| :--- | :--- | :--- | :--- |
| **Principal Software Architect** | Engine Architecture Team | Approved | 2026-11-17 |
| **Senior Compatibility Intelligence Engineer**| Platform Engineering | Approved | 2026-11-17 |
| **Senior QA Automation Architect** | Test & Quality Assurance | Approved | 2026-11-17 |

