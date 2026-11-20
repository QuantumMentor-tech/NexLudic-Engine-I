# CROSS-GAME RESOLUTION STRATEGY CATALOGUE SPECIFICATION

**Document Identifier:** `NLE-SPC-RSC-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Strategy Governance  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-CRF-001` (Step 76), `NLE-POL-CEC-001` (Step 76)  
**Downstream Dependents:** `NLE-POL-MSI-001` (Step 76), `NLE-CON-RAV-001` (Step 76)

---

## 1. Purpose and Scope

This specification provides the authoritative catalogue of the **Nine Resolution Strategy Families** supported by NexLudic Engine. 

Every strategy family is defined with explicit applicability conditions, required evidentiary inputs, operational limitations, verification requirements, and mandatory rejection criteria.

---

## 2. Master Catalogue of Resolution Strategy Families

### Strategy 1: Authoritative-Source Reconciliation (`STRAT_AUTH_SOURCE_OVERRIDE`)
- **Applicability:** Domain governed by `SINGLE_AUTHORITATIVE_WRITER` where source authority is verified.
- **Mechanism:** The value from the verified authority is accepted as ground truth; target state is overridden.
- **Required Evidence:** Valid `NegotiatedSessionContract`, active epoch matching, uncorrupted source payload.
- **Limitations:** Target must possess write capability; cannot override local invariants without validation.
- **Rejection Criteria:** Source authority unverified, target revision changed since evaluation, or payload fails schema.

### Strategy 2: Conditional Replacement (`STRAT_CONDITIONAL_REPLACEMENT`)
- **Applicability:** Target supports atomic Compare-And-Swap (CAS) or revision-gated update APIs.
- **Mechanism:** Update is dispatched with precondition: `expected_target_revision == R_target_current`. If target revision advanced in the interim, update is rejected at target without mutation.
- **Required Evidence:** Target adapter declares `CAP_ATOMIC_CONDITIONAL_WRITE`.
- **Limitations:** Only viable if target adapter provides optimistic concurrency checking.
- **Rejection Criteria:** Target lacks revision verification capability; revision token expired.

### Strategy 3: Field-Level Independent Reconciliation (`STRAT_FIELD_INDEPENDENT`)
- **Applicability:** State domain with partitioned, orthogonal properties (e.g. Health vs Inventory).
- **Mechanism:** Conflicting field is isolated and reconciled independently without mutating non-conflicting fields.
- **Required Evidence:** Proof of zero cross-field physical or logical dependencies.
- **Limitations:** Inadmissible if fields share mathematical coupling (e.g. position and velocity).
- **Rejection Criteria:** Cross-field invariants exist or are unknown.

### Strategy 4: Deterministic Merge (`STRAT_DETERMINISTIC_MERGE`)
- **Applicability:** Both endpoints submit valid partial updates that can be reconciled via a pure, functional merge.
- **Mechanism:** Evaluates pure function $f(S_1, S_2) \to S_{merged}$ satisfying commutativity and determinism.
- **Required Evidence:** Formally registered merge function in active Resolution Policy; zero external I/O.
- **Limitations:** Requires mathematical closure within the domain's valid type and range bounds.
- **Rejection Criteria:** Merge produces out-of-bounds scalar or breaks referential integrity.

### Strategy 5: Commutative or Monotonic Updates (`STRAT_COMMUTATIVE_MONOTONIC`)
- **Applicability:** Additive counters (e.g. total score, XP accumulation, resource gathering) or monotonically advancing state flags.
- **Mechanism:** Applies delta additively or advances high-watermark flag ($S_{new} = \max(S_1, S_2)$).
- **Required Evidence:** Deduplication record verifying the delta has not been applied previously.
- **Limitations:** Non-invertible; vulnerable to duplicate application if deduplication window lapses.
- **Rejection Criteria:** Delta duplicate check fails; operation contains negative decrement violating monotonicity.

### Strategy 6: Source-Directed Replay (`STRAT_SOURCE_REPLAY`)
- **Applicability:** Receiver experienced temporary sequence gap ($\le 64$ deltas) and source retains buffered history.
- **Mechanism:** Coordinator requests bounded replay range $[R_{missing\_start} \dots R_{missing\_end}]$; applies in strict sequence.
- **Required Evidence:** Source declares `CAP_DELTA_REPLAY_BUFFER`; gap size $\le 64$.
- **Limitations:** All intermediate deltas must be available; target must be able to absorb catch-up burst.
- **Rejection Criteria:** Source buffer missing deltas; gap exceeds quota; non-idempotent operations present without tombstones.

### Strategy 7: Fresh-Snapshot Reset (`STRAT_SNAPSHOT_RESET`)
- **Applicability:** State divergence, large sequence gap ($> 64$), or corrupted baseline.
- **Mechanism:** Incremental deltas halted; authoritative fresh snapshot requested, validated, and published.
- **Required Evidence:** Source provides `ATOMIC_SIMULATION_PAUSE` or `REVISION_BOUND_TRANSACTION` snapshot.
- **Limitations:** High bandwidth and latency cost; may cause visual hitch in target client.
- **Rejection Criteria:** Source snapshot capability unavailable or snapshot fails 12-stage validation.

### Strategy 8: Manual or Game-Specific Escalation (`STRAT_MANUAL_ESCALATION`)
- **Applicability:** High-value state (player progression, inventory items, quest completion) with ambiguous authority.
- **Mechanism:** Conflict frozen; structured escalation payload rendered to user/admin UI for explicit resolution selection.
- **Required Evidence:** Human cryptographic signature or authenticated user session action.
- **Limitations:** Asynchronous latency; cannot resolve real-time 60Hz physics.
- **Rejection Criteria:** User dismisses prompt without choice; timeout expires without response.

### Strategy 9: Refusal and Safe Containment (`STRAT_REFUSAL_CONTAINMENT`)
- **Applicability:** Default fail-closed fallback whenever no other strategy is eligible or safe.
- **Mechanism:** Operation rejected; target domain frozen at last clean revision $R_{clean}$; audit log recorded.
- **Required Evidence:** Recorded failure of all candidate policy eligibility gates.
- **Limitations:** Halts synchronization progression for affected domain until manual intervention or session reset.
- **Rejection Criteria:** None (Universal Fail-Closed Fallback).
