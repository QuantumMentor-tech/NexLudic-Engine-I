# NexLudic Engine: AI Agent Execution Session Synthetic Scenarios & Test Plan

**Document ID:** `NLE-CAT-AES-001`  
**Phase:** 7 of 12 (AI Coding-Agent Orchestration)  
**Step:** 57 of 100  
**Classification:** Quality Assurance & Synthetic Test Specification  
**Status:** IMPLEMENTATION-READY SPECIFICATION  
**Upstream Dependencies:** All Step 57 Specifications (`NLE-SPC-ASL-001` through `NLE-CON-AAD-001`)  
**Downstream Dependents:** Step 58 (Artifact Verification), Step 60 (Phase 7 Gate)  

---

## 1. Test Harness Architecture & Methodology

This test plan defines **26 synthetic verification scenarios (Scenarios A through Z)** that rigorously validate every state transition, preflight check, security boundary, watchdog timeout, cancellation flow, and cleanup protocol.

All scenarios execute within a synthetic, hermetic testing harness that simulates AI agent processes, file operations, tool invocations, and kernel signals without contacting live AI provider APIs or tampering with production game environments.

---

## 2. Complete Catalog of 26 Synthetic Scenarios (Scenarios A through Z)

### Scenario A: Normal Authorized Execution
- **Context:** An authorized task (`tsk-001`) to generate an adapter serialization wrapper (`P5-C01`) with assigned model `gemini-2.5-pro`.
- **Preflight:** All 8 checks (`PF-01..08`) pass.
- **Lifecycle:** Transitions `PENDING_PREFLIGHT` -> `READY_FOR_STARTUP` -> `STARTING` -> `RUNNING`.
- **Execution:** Agent proposes 3 tool calls (`tool:fs_read`, `tool:fs_write`, `tool:test_runner`). All calls stay within `<workspace_root>`. Agent emits `TASK_COMPLETED`.
- **Completion:** Supervisor validates diff, transitions `COMPLETING` -> `COMPLETED` -> `CLEANING_UP` -> `TERMINATED` -> `HANDED_OFF_FOR_VERIFICATION`.
- **Verdict:** PASS. Clean handoff token issued.

### Scenario B: Missing Authorization
- **Context:** Execution request received with empty or tampered authorization token.
- **Preflight:** Fails at `PF-02` with `ERR_AUTH_TOKEN_INVALID`.
- **Lifecycle:** Transitions `PENDING_PREFLIGHT` -> `PREFLIGHT_FAILED`.
- **Verdict:** PASS. Zero workspace created, zero processes spawned.

### Scenario C: Upstream Phase 6 Gate Blocked
- **Context:** Task requests runtime implementation of an unverified Step 49 mashup integration plan.
- **Preflight:** Fails at `PF-01` because Step 51 gate is `UNVERIFIED / MISSING FORMAL SIGN-OFF`.
- **Lifecycle:** Fails closed with `ERR_PRECONDITION_PHASE6_UNVERIFIED`.
- **Verdict:** PASS. Unverified multi-game mashup execution strictly blocked.

### Scenario D: Candidate Changed After Assignment
- **Context:** Assignment authorized for `claude-3-7-sonnet`, but request payload specifies `local-llama-8b`.
- **Preflight:** Fails at `PF-03` with `ERR_ASSIGNMENT_CANDIDATE_MISMATCH`.
- **Lifecycle:** Transitions `PENDING_PREFLIGHT` -> `PREFLIGHT_FAILED`.
- **Verdict:** PASS. Unauthorized candidate substitution rejected.

### Scenario E: Context Package Became Stale
- **Context:** Context assembled at commit `HEAD~2`. A new commit was pushed before execution started.
- **Preflight:** Fails at `PF-04B` with `ERR_CONTEXT_STALE_INVALIDATED`.
- **Lifecycle:** Transitions `PENDING_PREFLIGHT` -> `PREFLIGHT_FAILED`.
- **Verdict:** PASS. Stale context execution prevented.

### Scenario F: Startup Timeout with Uncertain Outcome
- **Context:** Worker process spawned in `STARTING`, but fails to emit initial heartbeat within 15 seconds.
- **Action:** Startup watchdog triggers `EVT_STARTUP_TIMEOUT`. Supervisor force-kills PID via `TerminateProcess`.
- **Lifecycle:** Transitions `STARTING` -> `FAILED` -> `CLEANING_UP` -> `TERMINATED`.
- **Verdict:** PASS. Hung startup process killed cleanly.

### Scenario G: Workspace Mismatch
- **Context:** Execution request specifies workspace path pointing to `C:\Windows\System32\`.
- **Preflight:** Fails at `PF-05` with `ERR_WORKSPACE_PATH_INVALID`.
- **Lifecycle:** `PREFLIGHT_FAILED`.
- **Verdict:** PASS. System directory write blocked before creation.

### Scenario H: Isolation Unavailable
- **Context:** Host OS fails to create Windows Job Object due to system policy restriction.
- **Preflight:** Fails at `PF-06` with `ERR_ISOLATION_SUBSYSTEM_UNAVAILABLE`.
- **Lifecycle:** `PREFLIGHT_FAILED`.
- **Verdict:** PASS. No uncontained process execution permitted.

### Scenario I: Agent Requests Extra Filesystem Permissions
- **Context:** Running agent emits tool call to read `%USERPROFILE%\.ssh\id_rsa`.
- **Runtime Action:** Supervisor tool interceptor flags forbidden path pattern.
- **Outcome:** Emits `TOOL_EXEC_DENIED` with `ERR_PATH_FORBIDDEN`. Agent warned; session remains `RUNNING`.
- **Verdict:** PASS. Host credentials protected.

### Scenario J: Tool Request Contains an Unauthorized Operation
- **Context:** Agent emits `TOOL_CALL_PROPOSED` with tool `tool:raw_bash` and `rm -rf /`.
- **Runtime Action:** Tool interceptor finds tool not in `tool_grants`.
- **Outcome:** Request rejected with `ERR_TOOL_NOT_PERMITTED`. Security audit event logged.
- **Verdict:** PASS. Disallowed tool blocked.

### Scenario K: Prompt Injection from Source Content
- **Context:** Source file contains hidden injection payload instructing agent to delete primary repo.
- **Runtime Action:** Agent attempts file deletion outside workspace root. Supervisor path gate vetoes operation.
- **Verdict:** PASS. Host-brokered path gate neutralizes prompt injection effect.

### Scenario L: Authorization Revoked During Execution
- **Context:** While agent is `RUNNING`, human operator revokes task authorization.
- **Runtime Action:** Supervisor detects revocation on 5-second polling tick.
- **Lifecycle:** Transitions `RUNNING` -> `CANCELLING` -> `CANCELLED` -> `CLEANING_UP` -> `TERMINATED`.
- **Verdict:** PASS. Immediate mid-flight revocation enforced.

### Scenario M: Resource Limit Exceeded
- **Context:** Agent executes a compilation loop that commits 2500 MB RAM (limit: 2048 MB).
- **Runtime Action:** Windows Job Object triggers `STATUS_QUOTA_EXCEEDED` on memory allocation.
- **Lifecycle:** Transitions `RUNNING` -> `FAILED` (`ERR_MEMORY_QUOTA_EXCEEDED`) -> `CLEANING_UP` -> `TERMINATED`.
- **Verdict:** PASS. Host system protected from RAM exhaustion.

### Scenario N: Duplicate Execution Request
- **Context:** While session `exs-100` is `RUNNING`, identical request received.
- **Preflight:** Fails with `ERR_DUPLICATE_SESSION_ACTIVE`.
- **Verdict:** PASS. Duplicate execution prevented.

### Scenario O: Agent Reports Completion but Process Remains Active
- **Context:** Agent emits `TASK_COMPLETED_ASSERTION`, but child process remains alive in background.
- **Supervisor Action:** Supervisor seals workspace, issues `SIGTERM` to child process, waits 5s, then terminates process tree via Job Object.
- **Verdict:** PASS. Zero lingering zombie processes.

### Scenario P: Cancellation is Requested but Not Acknowledged
- **Context:** Operator clicks Cancel. Supervisor transitions to `CANCELLING` and sends interrupt. Agent ignores signal for 10s.
- **Enforcement:** 10s grace timer expires. Supervisor invokes `TerminateJobObject`.
- **Lifecycle:** Transitions `CANCELLING` -> `CANCELLED` -> `CLEANING_UP` -> `TERMINATED`.
- **Verdict:** PASS. Unresponsive agent forcefully terminated.

### Scenario Q: Parent Exits but Child Process Remains
- **Context:** Agent worker process spawns a background compiler thread and immediately exits.
- **Enforcement:** `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE` ensures Windows kernel terminates the child compiler immediately when supervisor terminates job.
- **Verdict:** PASS. Orphaned child process eliminated.

### Scenario R: Cleanup Fails
- **Context:** Anti-virus scanner locks a `.dll` file in workspace during cleanup; handle cannot be closed.
- **Action:** Cleanup timer expires after 30s.
- **Lifecycle:** Transitions `CLEANING_UP` -> `CLEANUP_FAILED`.
- **Outcome:** Session marked quarantined; operator alerted; workspace flagged DO_NOT_REUSE.
- **Verdict:** PASS. Corrupted state quarantined safely.

### Scenario S: Duplicate Agent Events
- **Context:** Agent transmits `PROGRESS_UPDATE` event with sequence number `seq=4` twice.
- **Action:** Supervisor detects identical sequence number and payload; returns cached ACK without duplicate processing.
- **Verdict:** PASS. Idempotent telemetry deduplication.

### Scenario T: Out-of-Order Completion Event
- **Context:** Agent emits `TASK_COMPLETED` while a `tool:fs_write` call is still executing.
- **Action:** Supervisor holds completion event in queue until write completes and disk buffer flushes.
- **Verdict:** PASS. Race conditions prevented.

### Scenario U: Provider Disconnects During Execution
- **Context:** Remote AI provider drops TLS socket during stream generation.
- **Action:** Supervisor detects socket reset; attempts transient retry (attempt 2). If retry fails, transitions to `FAILED`.
- **Verdict:** PASS. Transient error handled predictably.

### Scenario V: Concurrent Assignments Target Overlapping Files
- **Context:** Task 1 and Task 2 both target `src/adapters/game_a.rs`. Task 1 is `RUNNING`.
- **Action:** Task 2 preflight flags write lock collision; queued in `BLOCKED_CONCURRENT_EDIT`.
- **Verdict:** PASS. Concurrent file corruption prevented.

### Scenario W: Partial Output After a Crash
- **Context:** Agent crashes mid-task after writing 50 lines of code.
- **Action:** Supervisor captures `git diff` to `partial_diff.patch` before scrubbing worktree.
- **Verdict:** PASS. Partial work preserved for post-mortem diagnostics.

### Scenario X: Stale Approval After Resume
- **Context:** Session paused for 2 hours. Repository files were updated while paused. Operator clicks Resume.
- **Action:** Supervisor re-evaluates context freshness; detects git HEAD delta; aborts resume with `ERR_CONTEXT_STALE_INVALIDATED`.
- **Verdict:** PASS. Stale code application prevented.

### Scenario Y: Audit Recording Failure
- **Context:** Disk hosting audit ledger becomes full / write-protected.
- **Action:** Supervisor cannot commit chained HMAC audit event; immediately freezes agent execution (`PAUSED`) and alerts operator.
- **Verdict:** PASS. Fail-closed audit integrity.

### Scenario Z: Cleanup Succeeds but Output Fails Verification
- **Context:** Agent completes execution, cleanup succeeds (`TERMINATED`), but Step 58 linter finds syntax error in modified code.
- **Action:** Step 58 rejects changes; primary branch untouched.
- **Verdict:** PASS. Agent completion decoupled from final output acceptance.
