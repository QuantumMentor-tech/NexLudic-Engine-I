# CROSS-GAME CONFLICT RESOLUTION SYNTHETIC QA TEST PLAN

**Document Identifier:** `NLE-CAT-CRR-001`  
**Lifecycle Stage:** Authoritative Test Plan  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Synthetic Verification  
**Upstream Dependencies:** `NLE-SPC-CRR-001` through `NLE-CON-COA-001` (Step 76)  
**Downstream Dependents:** Step 77 (Observability), Step 78 (Phase 9 Gate)

---

## 1. Scope, Methodology and Synthetic Harness Standard

This test plan defines **50 formal synthetic test scenarios (C01 through C50)** covering all aspects of cross-game conflict detection, authority arbitration, resolution policy matching, merge semantics, invariant enforcement, concurrency protection, rollback/compensation, and security.

### Test Execution Principles:
- **Zero Live Game Execution:** All tests run inside an isolated in-memory simulation harness. No running game binaries, no live network sockets, no anti-cheat interference.
- **Deterministic Evaluation:** Inputs are synthetic JSON payloads with fixed seeds (`SEED = 0x4352523736`).
- **Status Reporting Standard:** All scenarios in this document are **SPECIFIED, NOT EXECUTED** pending runtime test harness implementation in subsequent phases.

---

## 2. Master Synthetic Test Matrix (Scenarios C01 through C50)

### Group 1: Conflict Detection and Evidence (C01 – C10)
- **C01 (Legitimate temporal difference):** Two updates differ in value because they were captured at monotonic ticks 100 and 101. Expected: Evaluated as monotonic progression; dismissed as NOT a conflict.
- **C02 (Confirmed conflicting revisions):** Two updates claim incompatible state for revision 105 in the same epoch. Expected: Confirmed conflict; quarantined immediately.
- **C03 (Missing authority evidence):** Neither endpoint possesses a registered ownership token. Expected: Authority cannot be established; quarantined as `UNRESOLVED`.
- **C04 (Stale observation):** One candidate update exceeds maximum freshness limit ($> 500$ ms). Expected: Flagged as `STALE`; disqualified from arbitration.
- **C05 (Different semantic mappings):** Endpoints emit values under mismatched SMR versions (`v1.0` vs `v2.0`). Expected: Semantic conflict confirmed; mapping reconciliation requested.
- **C06 (Unsupported normalization):** Candidate value cannot be normalized by Step 74 CIR. Expected: Ingestion rejected with `ERR_SEM_NORMALIZATION_FAILED`.
- **C07 (False-positive detection):** Partitioned fields (Endpoint A updates health, Endpoint B updates armor). Expected: Classified as disjoint partitioned updates; both accepted.
- **C08 (Insufficient causal information):** Updates lack vector clocks and arrive simultaneously. Expected: Ordering cannot be assumed; flagged as `CONFLICT_CAUSALITY_UNKNOWN`.
- **C09 (Conflicting source claims):** Both endpoints present valid certificates claiming exclusive write rights. Expected: Ambiguous authority confirmed; escalated to human review; NO LWW.
- **C10 (Invalid conflict metadata):** Conflict record contains malformed UUID and invalid epoch. Expected: Rejected at schema validation gate; audit alert emitted.

### Group 2: Authority and Policy Selection (C11 – C20)
- **C11 (Single verified authority):** Endpoint A holds exclusive registered write token. Expected: Endpoint A's value accepted; Endpoint B's update rejected.
- **C12 (Disconnected authority):** Authoritative endpoint temporarily unplugs. Expected: Authority is NOT reassigned; domain frozen until reconnect or session reset.
- **C13 (Authority change in progress):** Ownership transfer token in flight while old update arrives. Expected: Drain rule applied; updates before cutoff accepted; post-cutoff rejected.
- **C14 (Unauthorized writer):** Endpoint with read-only permission emits mutation. Expected: Rejected immediately with `ERR_SYNC_UNAUTHORIZED_WRITER`.
- **C15 (Policy unavailable):** No approved resolution policy matches the domain and category. Expected: Automatic resolution refused; domain quarantined in `UNRESOLVED`.
- **C16 (Multiple compatible policy candidates):** Concept-level and Domain-level policies both match. Expected: More specific Concept-level policy selected via Precedence Rule.
- **C17 (Contradictory policies):** Two equally specific policies prescribe conflicting strategies. Expected: Ambiguity detected; execution halted; escalated to admin.
- **C18 (Policy expired):** Policy definition exceeds its valid TTL. Expected: Policy disqualified as expired; fresh policy negotiation required.
- **C19 (Stale approval):** User approval references revision 100, but domain moved to 102. Expected: Stale approval rejected; re-approval requested.
- **C20 (Unsupported target capability):** Policy requires atomic CAS, but target lacks capability. Expected: Policy disqualified; fallback to snapshot reset.

### Group 3: Merge and Invariant Validation (C21 – C30)
- **C21 (Valid deterministic merge):** Pure merge function combines two orthogonal player stat sets. Expected: Merged state passes all domain invariants; deterministic outcome.
- **C22 (Non-deterministic merge behavior):** Merge function evaluated multiple times with identical inputs produces differing outputs. Expected: Invariant gate catches discrepancy; merge aborted.
- **C23 (Duplicate additive operation):** Additive XP delta delivered twice. Expected: Deduplication filter catches repeated `delta_id`; applied exactly once.
- **C24 (Non-commutative operations):** Sequence of multiplication and subtraction reordered. Expected: Non-commutativity detected; reordering strictly blocked.
- **C25 (Cross-field invariant violation):** Candidate merge sets player speed to 500 m/s while in `CROUCHED` state. Expected: Domain invariant violated; merge rejected.
- **C26 (Invalid derived value):** Derived kinetic energy does not match $0.5 \cdot m \cdot v^2$. Expected: Mathematical consistency check fails; derived value recalculated.
- **C27 (Lossy transformation):** Merging 64-bit float to 8-bit int loses precision. Expected: Lossiness flagged; original 64-bit float sealed in `vault_token`.
- **C28 (Unknown value versus default):** Unmapped enemy faction ID in payload. Expected: Preserved as unknown; NOT silently replaced by default faction.
- **C29 (Field-level authority):** Writer authorized for inventory attempts to modify health. Expected: Health mutation stripped; inventory mutation processed.
- **C30 (Unsupported automatic merge):** High-stakes quest completion state in conflict. Expected: Automated merge prohibited by policy; routed to human gate.

### Group 4: Concurrency and Application (C31 – C40)
- **C31 (Target changes before write):** Target advances revision between evaluation and dispatch. Expected: TOCTOU check catches mismatch; mutation aborted.
- **C32 (Stale-write prevention):** Conditional write fails at target because target is at R=105, not expected R=104. Expected: `ERR_CONFLICT_STALE_WRITE_PREVENTED` returned.
- **C33 (Duplicate resolution request):** Same `reconcile_op_id` received again. Expected: Recognized in dedup buffer; previous result returned idempotently.
- **C34 (Application succeeds without acknowledgment):** Target applies mutation but network drops ack. Expected: Status set to `OUTCOME_UNCERTAIN`; read-back initiated.
- **C35 (Explicit target rejection):** Target game engine throws internal physics out-of-bounds exception. Expected: Status marked `OUTCOME_TARGET_REJECTED`; recovery triggered.
- **C36 (Partial multi-item application):** Gold deducted in Game A, item spawn fails in Game B. Expected: `OUTCOME_PARTIALLY_APPLIED` flagged; compensation pipeline triggered.
- **C37 (Read-back disagreement):** Read-back query returns position differing from written value. Expected: `OUTCOME_VERIFICATION_FAILED` logged; domain frozen.
- **C38 (Unknown application outcome):** Target lacks read-back API and timed out. Expected: Domain frozen; speculative writes blocked; snapshot required.
- **C39 (Session epoch changes during application):** Reconnection increments epoch while write in flight. Expected: In-flight write discarded; stale session rejected.
- **C40 (New conflict during reconciliation):** Secondary conflict detected while first is resolving. Expected: Active resolution completed under domain lock; secondary queued.

### Group 5: Recovery and Reversibility (C41 – C47)
- **C41 (Safe supported rollback):** Target engine supports state snapshot; cleanly reverts to R=100. Expected: Target confirmed at R=100; domain resumes.
- **C42 (Rollback would overwrite newer state):** Target advanced 10 revisions ahead of conflict. Expected: Rollback refused; forward compensation requested.
- **C43 (Compensation unavailable):** Operation consumed rare consumable item; no inverse API exists. Expected: Classified as `IRREVERSIBLE`; escalated to human review.
- **C44 (Compensation is not an inverse):** Refunding gold does not restore market price state. Expected: Partial compensation recorded; residual variance logged.
- **C45 (Replay outcome uncertain):** Previous spawn attempt had unknown outcome; replay could duplicate actor. Expected: Replay blocked; read-back mandatory.
- **C46 (Recovery snapshot resolves baseline uncertainty):** Authoritative fresh snapshot published. Expected: Clean baseline established; conflict resolved via reset.
- **C47 (Recovery cannot establish authority):** Source endpoint refuses to declare authoritative baseline. Expected: Domain remains `UNRESOLVED`; bridge session marked degraded.

### Group 6: Security, Audit, and Isolation (C48 – C50)
- **C48 (Unauthorized policy modification):** Peer attempts to upload resolution policy granting itself write rights. Expected: Signature verification fails; peer quarantined.
- **C49 (Cross-session conflict leakage):** Conflict packet from Session Alpha delivered to Session Beta. Expected: Session UUID check fails; packet dropped.
- **C50 (Audit completeness):** Conflict dossier inspected post-resolution. Expected: Verified facts, rationale, and hashes fully present; no leaked memory pointers.
