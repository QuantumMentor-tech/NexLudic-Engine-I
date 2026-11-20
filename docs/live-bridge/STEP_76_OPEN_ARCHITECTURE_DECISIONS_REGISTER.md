# STEP 76 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-076`  
**Lifecycle Stage:** Authoritative Architecture Register  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Architectural Governance  
**Upstream Dependencies:** `NLE-SPC-CRR-001` through `NLE-CON-COA-001`  
**Downstream Dependents:** Phase 9 Baseline Architecture

---

## 1. Summary of Architectural Decision Records (ADRs)

| ADR Identifier | Mandatory Decision Area | Status | Impact Area |
| :--- | :--- | :---: | :--- |
| `ADR-CRR-001` | Approved Authority Model & Granularity | **ACCEPTED** | Authority Scoping |
| `ADR-CRR-002` | Definition of Confirmed Conflict | **ACCEPTED** | Detection Rigor |
| `ADR-CRR-003` | Mechanism for Evaluating Authority Evidence | **ACCEPTED** | Identity & Trust |
| `ADR-CRR-004` | Policy Precedence and Ambiguity Handling | **ACCEPTED** | Resolution Policy |
| `ADR-CRR-005` | Domains Permitting Automatic Reconciliation | **ACCEPTED** | Automation Safety |
| `ADR-CRR-006` | Operations Requiring Human Approval | **ACCEPTED** | Human-in-the-Loop |
| `ADR-CRR-007` | Criteria for Deterministic Merge Eligibility | **ACCEPTED** | Functional Merge |
| `ADR-CRR-008` | Requirements for Conditional Writes / Preconditions | **ACCEPTED** | Concurrency Safety |
| `ADR-CRR-009` | Handling of Concurrent Updates During Application | **ACCEPTED** | TOCTOU Protection |
| `ADR-CRR-010` | Rules for Unknown Application Outcomes | **ACCEPTED** | Uncertainty Protocol |
| `ADR-CRR-011` | Required Verification Strength for Success Reporting | **ACCEPTED** | Verification Rigor |
| `ADR-CRR-012` | Reversibility and Compensation Support | **ACCEPTED** | State Restoration |
| `ADR-CRR-013` | Handling of Multi-Domain Partial Completion | **ACCEPTED** | Composite Safety |
| `ADR-CRR-014` | Policy Versioning and Invalidation | **ACCEPTED** | Version Governance |
| `ADR-CRR-015` | Boundaries on AI-Assisted Conflict Analysis | **ACCEPTED** | AI Governance |
| `ADR-CRR-016` | Security Approval for State-Changing Integrations | **ACCEPTED** | Security Sandboxing |
| `ADR-CRR-017` | Conflict Retention and Diagnostic Privacy | **ACCEPTED** | Observability & PII |

---

## 2. Detailed Architectural Decision Records

### ADR-CRR-001: Approved Authority Model & Granularity
- **Decision:** Authority is scoped down to **individual fields and partitioned property sets**, defaulting to single authoritative writer. Global game-level authority switches are rejected.
- **Consequences:** Prevents coarse overrides from clobbering orthogonal, non-conflicting state.

### ADR-CRR-002: Definition of Confirmed Conflict
- **Decision:** A conflict is confirmed **only when** competing claims target overlapping authority scopes in the same epoch with incompatible revisions or contradictory mutations. Legitimate temporal lag and partitioned updates are dismissed.
- **Consequences:** Eliminates false-positive desynchronization storms.

### ADR-CRR-003: Mechanism for Evaluating Authority Evidence
- **Decision:** Authority evidence is validated strictly via external **Session Contracts and X.509 endpoint certificates**. Self-declared authority tags in message payloads are discarded.
- **Consequences:** Prevents malicious or misconfigured peers from claiming authority via payload tampering.

### ADR-CRR-004: Policy Precedence and Ambiguity Handling
- **Decision:** Specific policies override general policies; conservative modes override automated modes; equally specific contradictory policies trigger **fail-closed refusal and containment**.
- **Consequences:** Guarantees deterministic, safe outcomes in ambiguous configuration scenarios.

### ADR-CRR-005: Domains Permitting Automatic Reconciliation
- **Decision:** Automatic reconciliation is restricted to **kinematic transforms, display telemetry, and additive metrics**. High-stakes domains (inventory, progression) strictly forbid unverified automation.
- **Consequences:** Protects game economy and player progression from automated corruption.

### ADR-CRR-006: Operations Requiring Human Approval
- **Decision:** Irreversible operations, economy currency grants, unique item creation, and permanent quest transitions require **explicit human cryptographic sign-off** when in conflict.
- **Consequences:** Ensures human oversight over consequential state mutations.

### ADR-CRR-007: Criteria for Deterministic Merge Eligibility
- **Decision:** Merge functions must be pure, closed under domain invariants, deterministic across architectures, and registered in approved policies.
- **Consequences:** Eliminates arbitrary heuristic merges.

### ADR-CRR-008: Requirements for Conditional Writes / Preconditions
- **Decision:** All resolution writes require **Precondition Tokens** carrying expected target revision and baseline hash. Blind overwrites are prohibited.
- **Consequences:** Prevents overwriting newer legitimate state.

### ADR-CRR-009: Handling of Concurrent Updates During Application
- **Decision:** Enforce **per-domain mutual exclusion locks** during reconciliation; if revision advances during evaluation, TOCTOU abort triggers.
- **Consequences:** Linearizes concurrent updates per domain without global pipeline blocking.

### ADR-CRR-010: Rules for Unknown Application Outcomes
- **Decision:** If application outcome is uncertain, the domain is **frozen immediately**. Speculative retries and automatic reversals are strictly forbidden until read-back or snapshot verification.
- **Consequences:** Prevents double-applying operations or reversing changes that actually succeeded.

### ADR-CRR-011: Required Verification Strength for Success Reporting
- **Decision:** Reporting `OUTCOME_VERIFIED_SUCCESS` requires **independent read-back (Level 4) or invariant verification (Level 5)**. Transport acks cannot claim verified success.
- **Consequences:** Upholds absolute truthfulness in synchronization health reporting.

### ADR-CRR-012: Reversibility and Compensation Support
- **Decision:** Operations are explicitly classified into 4 reversibility tiers. Compensation is recognized as distinct from true rollback and does not guarantee inverse equivalence.
- **Consequences:** Prevents invalid assumptions of transactional undo.

### ADR-CRR-013: Handling of Multi-Domain Partial Completion
- **Decision:** Multi-domain operations detect partial application, halt execution, attempt registered compensation, and escalate to human review if compensation fails.
- **Consequences:** Prevents cross-game state duplication or resource leaks.

### ADR-CRR-014: Policy Versioning and Invalidation
- **Decision:** Policies are versioned via SemVer 2.0.0; major bumps require re-negotiation; expired or superseded policies immediately invalidate active reconciliation.
- **Consequences:** Maintains strict alignment between bridge policies and runtime capabilities.

### ADR-CRR-015: Boundaries on AI-Assisted Conflict Analysis
- **Decision:** AI agents are restricted to **read-only diagnostic advisory roles**. AI agents hold zero write authority and cannot bypass domain invariants.
- **Consequences:** Harnesses AI analysis speed while maintaining human-controlled safety.

### ADR-CRR-016: Security Approval for State-Changing Integrations
- **Decision:** State-changing writes are restricted to authorized local loopback / named pipe adapters. Memory injection and anti-cheat tampering remain permanently blocked.
- **Consequences:** Protects system integrity and adheres to platform security invariants.

### ADR-CRR-017: Conflict Retention and Diagnostic Privacy
- **Decision:** Conflict audit dossiers redact player PII and raw memory addresses, retaining only canonical IDs, revisions, and operational hashes.
- **Consequences:** Enables full post-incident auditability while complying with data privacy standards.
