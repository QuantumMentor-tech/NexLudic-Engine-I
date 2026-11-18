# MASHUP REQUIREMENT VERSIONING, CHANGE, AND INVALIDATION POLICY
**Document ID:** `NLE-POL-RVC-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Systems Integration Architect, Distributed-State Designer, Software Reliability Specialist  
**Classification:** Operational Policy & Invalidation Rules  

---

## 1. PURPOSE AND POLICY SCOPE

### 1.1 Purpose
This policy establishes the formal versioning standards, revision tracking mechanics, supersession lifecycles, and downstream invalidation cascades governing **Feature Requirements** (`FeatureRequirement`) within NexLudic Engine.

Requirements are not static; creators iteratively refine statements, adjust priority tiers, alter participant targets, and sharpen acceptance criteria. If requirements mutate without deterministic invalidation, downstream integration plans, capability matches, and code-generation pipelines will execute on obsolete assumptions. This policy specifies:
1. The requirement revision tracking model.
2. The rules governing requirement supersession and deprecation.
3. The deterministic downstream invalidation cascades triggered by requirement edits.

---

## 2. REQUIREMENT REVISION TRACKING

Each individual requirement maintains a local version counter integrated with the parent project revision hierarchy:
- **`req_version`:** Monotonically increasing integer ($\ge 1$). Initialized at 1 upon creation; incremented on any substantive field modification (`statement`, `criticality`, `priority`, `participant_id`, `acceptance_criteria`).
- **Revision Snapshot Binding:** When a project save occurs, the updated requirement state is captured in the project's immutable snapshot (`revisions/rev_<N>.json`) on Volume D (`DEC-009`).
- **Audit Diff Generation:** The system computes a structured JSON patch diff between `rev_<N-1>` and `rev_<N>`, appending a change record to `.audit/revisions.audit.jsonl`.

---

## 3. SUPERSESSION AND DEPRECATION LIFECYCLE

When a creator replaces an existing requirement with a fundamentally new approach:
1. **The `SUPERSEDES` Edge:** The new requirement declares a `SUPERSEDES` dependency pointing to the obsolete requirement.
2. **Archival State Transition:** The obsolete requirement transitions its quality status to `SUPERSEDED`. It is hidden from active planning views but retained in historical snapshots.
3. **Traceability Preservation:** Historical integration plans and compatibility reports maintain immutable references to the superseded requirement version, ensuring that past evaluation decisions remain fully explainable.

---

## 4. DOWNSTREAM INVALIDATION DECISION MATRIX

When a requirement is modified, the engine evaluates the change against the following deterministic invalidation matrix:

| Field Modified | Immediate Requirement Impact | Dependent Requirements (`REQUIRES`) | Integration Plan Status (Phase 6) | Capability Match Status (Phase 5) |
| :--- | :--- | :---: | :---: | :---: |
| **`statement` / Intent** | Quality status resets to `DRAFT` | Stamped `STALE_DEPENDENCY` | **INVALIDATED (Re-planning required)**| **STALE (Re-evaluation needed)** |
| **`category`** | Quality status resets to `DRAFT` | Stamped `STALE_DEPENDENCY` | **INVALIDATED (Topology broken)** | **INVALIDATED (Re-match needed)** |
| **`criticality` (Opt $\rightarrow$ Mand)** | Re-evaluates Quality Gates | Unaffected | **STALE (Requires mandatory fulfillment)** | **STALE** |
| **`criticality` (Mand $\rightarrow$ Opt)** | Re-evaluates Quality Gates | Unaffected | **DIRTY (Graceful degrade enabled)**| Unaffected |
| **`priority`** | Metadata update | Unaffected | **DIRTY (Reschedule priority)** | Unaffected |
| **`target_participant_id`**| Resets to `DRAFT` | Stamped `STALE_DEPENDENCY` | **INVALIDATED (Rerouting required)** | **INVALIDATED (Re-match needed)** |
| **`acceptance_criteria`** | Resets to `DRAFT` | Unaffected | **STALE (Re-verification required)** | Unaffected |
| **`assumptions`** | Triggers assumption review | Unaffected | Stamped `NEEDS_REVIEW` | Unaffected |
| **Requirement Deleted** | Purged from active project | Any dependent req shifts to `BLOCKED`| **INVALIDATED (Broken dependency)** | Unaffected |

---

## 5. REVALIDATION AND RECOVERY PROTOCOL

1. **State Reset Enforcement:** Any requirement experiencing a substantive edit unconditionally resets its quality status from `READY_FOR_ANALYSIS` back to `DRAFT`.
2. **Automated Quality Re-Evaluation:** The engine re-executes the 10 Quality Gates (`NLE-SPC-ACV-001`).
3. **Promoting Back to Ready:** Once all gates pass and all dependent open questions are resolved, the requirement returns to `READY_FOR_ANALYSIS`, signaling the downstream Integration Planner to re-compile the integration plan.

---
*End of Policy — Document `NLE-POL-RVC-001`.*
