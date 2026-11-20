# HYBRID INTEGRATION SYNTHETIC QA TEST PLAN

**Document Identifier:** `NLE-CAT-HIO-001`  
**Lifecycle Stage:** Authoritative Test Plan  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Synthetic Verification  
**Upstream Dependencies:** `NLE-SPC-HIO-001` through `NLE-CON-HOA-001` (Step 77)  
**Downstream Dependents:** Step 78 (Phase 9 Gate)

---

## 1. Scope, Methodology and Test Harness Architecture

This test plan defines **50 formal synthetic test scenarios (H01 through H50)** covering all aspects of multi-strategy hybrid orchestration, plan validation, capability selection, dependency ordering, handoff lifecycles, duplicate prevention, fallback, and resource isolation.

### Synthetic Test Principles:
- **Zero Live Game Execution:** All tests run inside an isolated in-memory simulation test harness. No running game binaries, no live network sockets, no anti-cheat interference.
- **Deterministic Evaluation:** Inputs are synthetic JSON payloads with fixed seeds (`SEED = 0x48494F3737`).
- **Status Reporting Standard:** All scenarios in this document are **SPECIFIED, NOT EXECUTED** pending runtime test harness implementation in subsequent phases.

---

## 2. Master Synthetic Test Matrix (Scenarios H01 through H50)

### Group 1: Plan Validation and Capability Selection (H01 – H10)
- **H01 (Valid single-strategy plan):** Plan contains one approved strategy with all required capabilities. Expected: Plan passes all 14 gates; transitions to `ELIGIBLE_FOR_ACTIVATION`.
- **H02 (Valid hybrid plan):** Two approved strategies (Phase 8 Mod + Phase 9 Bridge) provide complementary capabilities. Expected: Validation passes; execution graph created.
- **H03 (Missing required capability):** Plan requires `CAP_SNAPSHOT_CREATE` which neither strategy provides. Expected: Validation fails at Gate 4 with `ERR_HYBRID_CAPABILITY_UNSATISFIED`.
- **H04 (Declared but unverified capability):** Capability present only as Level 1 (Declared). Expected: Rejection; Level 3 (Verified) mandatory for activation.
- **H05 (Unauthorized strategy):** Strategy lacks required Phase 8 content modification token. Expected: Gate 3 fails; rejected as unauthorized.
- **H06 (Incompatible versions):** Strategy uses SMR `v2.0` while target requires `v1.0`. Expected: Gate 8 fails; mapping incompatibility reported.
- **H07 (Invalid dependency graph):** Strategy references non-existent prerequisite node. Expected: Graph validation fails; plan rejected.
- **H08 (Cyclic initialization dependencies):** Strategy A depends on Strategy B, which depends on Strategy A. Expected: Kahn's algorithm detects cycle; plan rejected with `ERR_HYBRID_DEPENDENCY_CYCLE`.
- **H09 (Ambiguous responsibility):** Two strategies claim unpartitioned write authority over identical domain. Expected: Gate 7 fails; plan rejected.
- **H10 (Plan invalidation):** Host game executable build hash changes post-validation. Expected: Invalidation trigger trips; plan reverts to `INVALID`.

### Group 2: Coordination and Lifecycle (H11 – H20)
- **H11 (Correct startup order):** Phase 8 mod verified applied before live bridge connector begins handshake. Expected: Orderly phase progression strictly maintained.
- **H12 (Premature activation):** Live bridge attempts delta transmission before initial snapshot baseline established. Expected: Operation blocked; prerequisite enforced.
- **H13 (Component starts but remains unready):** Adapter process launches but fails internal capability check. Expected: Orchestrator holds state; does NOT advance plan to `ACTIVE`.
- **H14 (Independent component failure):** Telemetry supporting strategy crashes while primary bridge continues. Expected: Telemetry isolated; primary domain remains `ACTIVE`.
- **H15 (Required dependency failure):** Content package verification revoked. Expected: Dependent live bridge immediately suspended.
- **H16 (Session epoch change):** Transport reconnection forces epoch increment ($1 \to 2$). Expected: Cached deltas from epoch 1 purged; fresh baseline requested.
- **H17 (Mapping change during activation):** SMR major version bumped during startup. Expected: Incremental stream halted; fresh snapshot requested.
- **H18 (Authentication expires):** Session token TTL elapses during active stream. Expected: Privileged mutations blocked; re-authentication triggered.
- **H19 (Strategy replacement):** Crashed adapter instance replaced by fresh instance. Expected: Preconditions revalidated before instance granted role.
- **H20 (Controlled shutdown):** Shutdown requested. Expected: In-flight operations drained in reverse-topological order within 3.0s limit.

### Group 3: Handoff and Ownership (H21 – H30)
- **H21 (Planned successful handoff):** Predecessor strategy drains operations; baseline transferred; destination assumes authority cleanly. Expected: Stage 1–15 complete; zero data loss.
- **H22 (Destination unavailable):** Handoff requested but destination adapter socket unreachable. Expected: Handoff aborted at Stage 2; predecessor retains authority.
- **H23 (Handoff fails before activation):** Predecessor drain times out. Expected: Handoff canceled; predecessor resumes active state.
- **H24 (Handoff fails during cutover):** Failure occurs at Stage 9 (destination initialization). Expected: State frozen; domain transitions to `HANDOFF_FAILED_UNCERTAIN`; snapshot reset required.
- **H25 (Former strategy remains active):** Predecessor emits mutation after cutoff revision. Expected: Guardrail catches write; rejected with `ERR_SYNC_UNAUTHORIZED_WRITER`.
- **H26 (Outstanding operation during handoff):** Non-idempotent operation in flight during transition. Expected: Operation held in transition buffer; applied once at destination.
- **H27 (Duplicate operation across strategies):** Shared operation ID delivered to both strategies. Expected: Deduplication sliding window drops second instance.
- **H28 (Missing state baseline):** Destination attempts to activate without verified cutoff snapshot. Expected: Activation blocked; baseline dependency enforced.
- **H29 (Ownership transfer):** Write token bound to new strategy in Session Registry. Expected: Single-writer invariant verified at all times.
- **H30 (Handoff cancellation after partial effects):** Handoff canceled after some fields written. Expected: Reversal refused; escalated to Step 76 compensation pipeline.

### Group 4: Fallback and Degraded Operation (H31 – H40)
- **H31 (Approved fallback):** Primary bridge fails; pre-approved read-only telemetry fallback activates. Expected: Transition succeeds; degraded mode entered.
- **H32 (Unauthorized fallback):** Primary bridge fails; unapproved fallback destination attempted. Expected: Refused fail-closed; session transitions to `FAILED`.
- **H33 (More privileged fallback):** Low-privilege bridge attempts to fall back to filesystem modification. Expected: Blocked by Anti-Escalation Invariant (`ERR_HYBRID_ESCALATION_PROHIBITED`).
- **H34 (Fallback lacks required capability):** Fallback adapter missing `CAP_DELTA_REPLAY`. Expected: Fallback rejected as ineligible.
- **H35 (Fallback mapping incompatibility):** Fallback uses incompatible SMR. Expected: Fallback rejected; session halts.
- **H36 (Degraded mode):** System enters `MODE_TELEMETRY_ONLY`. Expected: Real-time physics paused; display radar remains active; warning pill rendered.
- **H37 (Recovery during degraded mode):** Dependent domain attempts recovery. Expected: Bounded recovery follows Step 75 rules within degraded constraints.
- **H38 (Failback to primary):** Primary recovers; passes 5.0s soak test. Expected: Orderly handoff from fallback to primary executed.
- **H39 (Primary returns with stale state):** Primary attempts to resume using pre-failure cached baseline. Expected: Stale baseline rejected; fresh snapshot required.
- **H40 (Both strategies fail):** Primary and fallback strategies both crash. Expected: Session transitions to `FAILED`; resources released cleanly.

### Group 5: Recovery, Security, and Resource Boundaries (H41 – H50)
- **H41 (Bridge restart):** Bridge coordinator restarts while host games remain open. Expected: Full epoch increment; state baselines re-verified.
- **H42 (Repeated handoff failure):** Handoff fails 3 consecutive times. Expected: Bounded retry limit trips; domain marked `BLOCKED`.
- **H43 (Resource exhaustion):** Strategy memory allocation reaches 128 MiB. Expected: Backpressure activated; low-priority telemetry dropped.
- **H44 (Queue saturation):** Transition queue reaches 1024 items. Expected: Handoff aborted; fresh snapshot fallback triggered.
- **H45 (Revoked permission during handoff):** User revokes bridge consent mid-cutover. Expected: Cutover aborted immediately; IPC unlinked.
- **H46 (Fabricated capability claim):** Adapter claims Level 3 capability without test signature. Expected: Claim rejected during plan validation.
- **H47 (Cross-session isolation):** State update from Session Alpha delivered to Session Beta. Expected: Session UUID mismatch; payload quarantined.
- **H48 (Malformed integration plan):** Injected plan contains circular JSON references. Expected: Parser safely rejects with schema error.
- **H49 (Uncertain target application):** Target adapter times out during handoff mutation. Expected: Status marked `OUTCOME_UNCERTAIN`; domain frozen.
- **H50 (End-to-end status accuracy):** Component health, handoff completion, and qualification remain distinct. Expected: Telemetry preserves separate indicators without false success.
