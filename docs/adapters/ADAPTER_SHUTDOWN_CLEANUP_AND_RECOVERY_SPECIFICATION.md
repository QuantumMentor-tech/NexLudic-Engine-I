# ADAPTER SHUTDOWN, CLEANUP, AND RECOVERY SPECIFICATION
**Document ID:** `NLE-SPC-SCR-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 40 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Software Architect, Reliability Engineer, Security Architect, Senior QA Engineer  
**Classification:** Core Runtime Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the authoritative, deterministic protocol for game adapter shutdown, resource reclamation, fault classification, automated crash recovery, and security quarantine within NexLudic Engine. Game adapters execute as isolated, out-of-process worker processes under the supervision of the Core Supervisor (`CMP-08`). Because adapters interact directly with target game memory, hook libraries, and live IPC streams, unmanaged termination risks memory leakage, orphaned OS handles, game instability, and anti-cheat false positives. 

This document formally specifies:
1. The two-tier teardown sequence (orderly graceful drain vs. ungraceful forced termination).
2. The exact JSON-RPC 2.0 handshake for adapter shutdown (`adapter.shutdown`).
3. Win32 Job Object handle and IPC pipe reclamation procedures.
4. The failure taxonomy classifying crashes into recoverable, conditionally recoverable, and non-recoverable categories.
5. The exponential backoff recovery algorithm and state re-establishment pipeline.
6. The crash circuit breaker and quarantine escalation policy governing unrecoverable worker failures.

### 1.2 Architectural Scope and System Boundaries
```
+--------------------------------------------------------------------------------------------------------+
|                                    CORE SUPERVISOR (CMP-08)                                            |
|                                                                                                        |
|  +-----------------------------+     +-------------------------------+     +------------------------+  |
|  |   Teardown Orchestrator     |     |   Failure Classifier Engine   |     | Crash Circuit Breaker  |  |
|  | - Dispatch adapter.shutdown |     | - Transient IPC vs Fatal Kill |     | - Sliding 60s Window   |  |
|  | - 5000ms Grace Period Timer |     | - Anti-Cheat Driver Check     |     | - Trip at 3 Failures   |  |
|  | - Win32 TerminateProcess    |     | - Integrity Escalation Check  |     | - Quarantine Registry  |  |
|  +--------------+--------------+     +---------------+---------------+     +-----------+------------+  |
+-----------------|------------------------------------|---------------------------------|---------------+
                  | JSON-RPC 2.0 (Named Pipe)          | Signal Inspection               | State Lockdown
                  v                                    v                                 v
+--------------------------------------------------------------------------------------------------------+
|                                  OUT-OF-PROCESS ADAPTER WORKER                                         |
|                                                                                                        |
|  [ In-Flight Queue Drain ] --> [ Hook Unregistration ] --> [ Shared Buffer Free ] --> [ ExitProcess ]  |
+--------------------------------------------------------------------------------------------------------+
```

This specification governs all adapter workers across all three integration strategies defined in `ADR-0002` and Step 34 (`NLE-SPC-SDK-001`):
- Strategy 1: Host-Game Modification Adapters
- Strategy 2: Live Interoperability Bridging Adapters
- Strategy 3: Hybrid Dual-Process Adapters

---

## 2. ORDERLY TEARDOWN PROTOCOL (GRACEFUL SHUTDOWN)

### 2.1 The Two-Tier Teardown Architecture
To guarantee that neither the host game nor the NexLudic platform suffers resource corruption, teardown is split into two strictly bounded tiers:
1. **Tier 1 (Graceful Teardown):** A cooperative, multi-phase drain initiated by the Core Supervisor via a synchronous JSON-RPC 2.0 request. The worker is granted a strict grace period of 5,000 ms (`T_SHUTDOWN_GRACE = 5000ms`) to flush buffers, unhook libraries, detach memory probes, and terminate its child threads.
2. **Tier 2 (Forced Termination):** An unconditional, supervisor-enforced termination executed via Win32 `TerminateProcess` if the worker fails to exit cleanly before `T_SHUTDOWN_GRACE` expires.

### 2.2 Graceful Teardown Lifecycle Flow
When the Core Supervisor or the session manager requests an adapter shutdown (due to session conclusion, user pause, configuration reload, or graceful reconfiguration), the following sequential steps MUST execute:

```
Core Supervisor                                                 Adapter Worker
      |                                                               |
      | 1. Dispatch "adapter.shutdown" Request                        |
      |-------------------------------------------------------------->|
      |    (Start T_SHUTDOWN_GRACE = 5000ms countdown)                |
      |                                                               | 2. Halt inbound work ingest
      |                                                               | 3. Complete/reject in-flight IPC calls
      |                                                               | 4. Unhook game detours / detach engine
      |                                                               | 5. Release shared memory / mutexes
      |                                                               | 6. Flush telemetry & audit buffer
      |                                                               | 7. Serialize recovery state (if any)
      | 8. Receive "adapter.shutdown" Response (OK)                   |
      |<--------------------------------------------------------------|
      |                                                               | 8b. Worker calls ExitProcess(0)
      | 9. Process Handle signaled (ExitCode = 0)                     |
      |    (Cancel T_SHUTDOWN_GRACE timer)                            |
      v                                                               v
[ Lifecycle -> STOPPED ]                                      [ Process Terminated ]
[ Health    -> NOT_MONITORED ]
```

### 2.3 JSON-RPC 2.0 Shutdown Request Schema (`NLE-SCH-ASR-001`)
The Core Supervisor dispatches the shutdown command over the primary control channel (`stdin` or control pipe).

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/shutdown-request.schema.json",
  "title": "AdapterShutdownRequest",
  "type": "object",
  "required": ["jsonrpc", "id", "method", "params"],
  "properties": {
    "jsonrpc": { "type": "string", "const": "2.0" },
    "id": { "type": "string", "format": "uuid" },
    "method": { "type": "string", "const": "adapter.shutdown" },
    "params": {
      "type": "object",
      "required": ["session_id", "reason", "grace_period_ms", "preserve_state"],
      "properties": {
        "session_id": { "type": "string", "format": "uuid" },
        "reason": {
          "type": "string",
          "enum": [
            "USER_TERMINATED_SESSION",
            "MASHUP_COMPLETED",
            "RECONFIGURATION_REQUESTED",
            "ADAPTER_SUPERSEDED",
            "HOST_GAME_EXITING",
            "SYSTEM_SHUTDOWN"
          ]
        },
        "grace_period_ms": { "type": "integer", "minimum": 1000, "maximum": 10000, "default": 5000 },
        "preserve_state": { "type": "boolean", "default": false },
        "state_snapshot_path": { "type": "string" }
      },
      "additionalProperties": false
    }
  },
  "additionalProperties": false
}
```

### 2.4 JSON-RPC 2.0 Shutdown Response Schema (`NLE-SCH-ASP-001`)
The worker MUST respond within the allocated grace period prior to terminating its process.

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/shutdown-response.schema.json",
  "title": "AdapterShutdownResponse",
  "type": "object",
  "required": ["jsonrpc", "id", "result"],
  "properties": {
    "jsonrpc": { "type": "string", "const": "2.0" },
    "id": { "type": "string", "format": "uuid" },
    "result": {
      "type": "object",
      "required": ["status", "cleanup_duration_ms", "unregistered_hooks", "resources_released"],
      "properties": {
        "status": { "type": "string", "enum": ["CLEANUP_SUCCESSFUL", "CLEANUP_PARTIAL"] },
        "cleanup_duration_ms": { "type": "number", "minimum": 0.0 },
        "unregistered_hooks": { "type": "integer", "minimum": 0 },
        "resources_released": {
          "type": "array",
          "items": { "type": "string" }
        },
        "state_persisted": { "type": "boolean" },
        "warnings": {
          "type": "array",
          "items": { "type": "string" }
        }
      },
      "additionalProperties": false
    }
  },
  "additionalProperties": false
}
```

---

## 3. FORCED TERMINATION AND RESOURCE RECLAMATION (TIER 2)

### 3.1 Forced Kill Invariants
When an adapter worker violates the cooperative shutdown contract, the Core Supervisor MUST terminate it preemptively to preserve host stability:
1. **Timeout Trigger:** If `T_SHUTDOWN_GRACE` (5,000 ms) elapses after dispatching `adapter.shutdown` and the worker process handle remains unsignaled, Tier 2 termination triggers unconditionally.
2. **Crash on Teardown:** If the worker faults or generates an unhandled OS exception during cleanup, Tier 2 termination triggers immediately.
3. **Execution Primitive:** The Core Supervisor executes the Win32 `TerminateProcess` API passing the worker process handle and exit code `0xC000013A` (`STATUS_CONTROL_C_EXIT`) or `0xE0000001` (`NLE_ERR_FORCED_KILL`).
4. **Lifecycle State Assignment:** Upon forced termination, the worker lifecycle transitions to `FAILED` (or `QUARANTINED` if anti-cheat/integrity violations precipitated the kill), never `STOPPED`.

### 3.2 Supervisor Resource Reclamation Checklist
Following a Tier 2 forced kill, the Core Supervisor MUST execute a deterministic cleanup of all platform-side resources associated with the worker:
1. **Named Pipe Teardown:** Disconnect the pipe server instance (`DisconnectNamedPipe`), close pipe handles (`CloseHandle`), and unmap any named pipe buffers.
2. **Job Object Handle Cleanup:** Ensure the Win32 Job Object terminating assignment (`JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`) has reaped any child threads or lingering processes.
3. **Shared Memory Unmapping:** Unmap views of shared memory sections (`UnmapViewOfFile`) and close the file mapping objects (`CloseHandle`).
4. **Synchronization Mutex Release:** Release or force-close inter-process mutexes and semaphore handles.
5. **Project Lock Release:** Release file write locks held in `D:\NexLudic\Projects\<id>\Adapters\<adapterId>\`.
6. **Telemetry Channel Purge:** Discard pending frames in the 60Hz telemetry consumer buffer and flush the lifecycle audit log.

---

## 4. FAILURE TAXONOMY AND RECOVERY DECISION ENGINE

### 4.1 Failure Classification Hierarchy
Not all adapter worker failures are equal. The Core Supervisor evaluates crash events through a deterministic 5-category failure taxonomy:

| Category | Failure Description | Root Causes | Supervisor Reaction | Recoverability Class |
| :--- | :--- | :--- | :--- | :--- |
| **Category A** | **Transient IPC / Channel Loss** | Pipe buffer overflow, momentary socket stall, context switch latency. | Attempt immediate channel reconnect; if failed within 1000ms, proceed to process restart. | **Recoverable** |
| **Category B** | **Worker Soft Crash / Null Dereference** | Bug in adapter script, unhandled native exception in non-hook thread, divide-by-zero. | Log exception dump; initiate automatic restart using exponential backoff. | **Conditionally Recoverable** (up to 3x) |
| **Category C** | **Anti-Cheat Trigger / Driver Detection** | Host game anti-cheat (EasyAntiCheat, BattlEye, Vanguard) detected memory scan or hook injection. | Unconditional instant kill; zero retries; log security alert; transition to `QUARANTINED`. | **Non-Recoverable (Security Block)** |
| **Category D** | **Contract Tampering / Integrity Violation** | Adapter hash mismatch, medium IL escape attempt, child process spawn attempt. | Immediate kill; log security alert; revoke registration; transition to `QUARANTINED`. | **Non-Recoverable (Security Block)** |
| **Category E** | **Resource Budget Exceeded** | Memory consumption > 512 MB, CPU utilization 100% for > 5000ms, disk leak. | Terminate worker; if memory exceeded, attempt restart with halved buffer cache; if persistent, transition to `FAILED`. | **Conditionally Recoverable** (1 retry) |

### 4.2 Recovery Decision Matrix
```
                    [ Adapter Failure Event Detected ]
                                   |
         +-------------------------+-------------------------+
         |                                                   |
[ Category C or D: Security / Anti-Cheat ]      [ Category A, B, or E: Operational ]
         |                                                   |
         v                                                   v
[ Unconditional Kill ]                         [ Check Restart Budget (Max 3 / 60s) ]
         |                                                   |
[ Lifecycle: QUARANTINED ]                     +-------------+-------------+
[ Health: UNHEALTHY ]                          |                           |
[ Circuit Breaker: TRIPPED (PERMANENT) ]       v                           v
                                     [ Budget Available ]        [ Budget Exhausted (>= 3) ]
                                               |                           |
                                               v                           v
                                    [ Calculate Exponential ]     [ Trip Circuit Breaker ]
                                    [ Backoff Delay (1s, 2s, 4s)] |
                                               |                  [ Lifecycle: QUARANTINED ]
                                               v                  [ Health: UNHEALTHY ]
                                    [ Spawn New Worker Process ]  [ User Intervention Required ]
                                               |
                                    [ Run Handshake & Re-Init ]
```

---

## 5. EXPONENTIAL BACKOFF AND RESTART POLICY

### 5.1 Restart Budget and Sliding Window
To prevent infinite crash-restart loops from degrading host system responsiveness or spamming game process hooks:
- **Maximum Restart Count (`MAX_RESTARTS`):** `3` restart attempts.
- **Sliding Observation Window (`WINDOW_SEC`):** `60` seconds.
- **Circuit Breaker Window Reset:** If an adapter worker remains continuously in the `RUNNING` lifecycle state with `HEALTHY` operational health for at least 60 consecutive seconds, its crash count resets to `0`.

### 5.2 Mathematical Backoff Formula
The delay before attempting to spawn a new adapter worker process following a recoverable failure is governed by exponential backoff with zero jitter (deterministic execution):

$$T_{\text{delay}}(n) = T_{\text{base}} \times 2^{n - 1}$$

Where:
- $T_{\text{base}} = 1000\text{ ms}$
- $n = \text{restart attempt index } \in \{1, 2, 3\}$

Calculated delays:
- **Attempt 1 ($n = 1$):** $1000\text{ ms} \times 2^0 = 1000\text{ ms}$ (1.0 second)
- **Attempt 2 ($n = 2$):** $1000\text{ ms} \times 2^1 = 2000\text{ ms}$ (2.0 seconds)
- **Attempt 3 ($n = 3$):** $1000\text{ ms} \times 2^2 = 4000\text{ ms}$ (4.0 seconds)

### 5.3 State Re-Establishment Sequence
When restarting a recovered adapter worker:
1. **Spawn:** Supervisor executes `CreateProcessW` under Medium IL with clean arguments and environment.
2. **Runtime Admission:** Re-evaluate admission gates `ADM-GAT-001` through `ADM-GAT-005` in < 50 ms.
3. **Handshake (`InitializeAdapterRequest`):** Re-negotiate capabilities and supply negotiated configuration parameters.
4. **State Restoration (Optional):** If the adapter configured state preservation and wrote a clean `state_snapshot_path` prior to crashing, the Supervisor passes the snapshot path in the initialization request.
5. **Readiness Verification:** Worker executes internal setup and returns `ReadyReport` within 10,000 ms.
6. **Execution Resume:** Session manager transitions lifecycle from `READY` to `RUNNING` and resumes data stream.

---

## 6. CRASH CIRCUIT BREAKER AND QUARANTINE ESCALATION

### 6.1 Circuit Breaker Tripping Criteria
The crash circuit breaker transitions from `CLOSED` (normal operation) to `OPEN` (tripped/quarantined) under any of the following deterministic conditions:
- **Condition CB-01 (Crash Rate Exhaustion):** Adapter crashes 3 times within a rolling 60-second window ($n \ge 3$).
- **Condition CB-02 (Anti-Cheat Invalidation):** Any anti-cheat driver signature or scanning tripwire fires (`Category C`).
- **Condition CB-03 (Security Integrity Violation):** Hash tampering, binary mutation, or unauthorized privilege elevation detected (`Category D`).
- **Condition CB-04 (Repeated Init Timeout):** Adapter fails the 10,000 ms `INITIALIZING` handshake twice consecutively.

### 6.2 Quarantine State Invariants
When the circuit breaker trips:
1. **Lifecycle Transition:** Lifecycle state immediately transitions to `QUARANTINED` (`LFC-STA-010`).
2. **Health Transition:** Operational health transitions to `UNHEALTHY` (`OPH-STA-004`).
3. **Process Elimination:** Any running worker process or child threads are terminated unconditionally (`TerminateProcess`).
4. **Registry Lockdown:** The adapter's record in the Validated Capability Declaration Registry (`NLE-SPC-VCR-001`) is flagged as `STATUS_QUARANTINED`.
5. **UI Notification:** A high-priority warning banner is dispatched to the NexLudic Desktop UI detailing the trip condition, crash history, and error codes.
6. **Administrative Reset Required:** The adapter CANNOT be restarted or admitted dynamically by the session manager. Quarantine release requires explicit user acknowledgement and manual administrative re-validation.

### 6.3 Quarantine Audit Artifact Schema (`NLE-SCH-QAR-001`)
Whenever the circuit breaker trips, the Supervisor writes a persistent quarantine report to `D:\NexLudic\Projects\<id>\Adapters\<adapterId>\.audit\lifecycle\quarantine_<timestamp>.json`:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/quarantine-record.schema.json",
  "title": "AdapterQuarantineRecord",
  "type": "object",
  "required": [
    "quarantine_id",
    "adapter_id",
    "timestamp_iso8601",
    "trigger_condition",
    "failure_category",
    "crash_count_in_window",
    "last_exit_code",
    "diagnostic_summary"
  ],
  "properties": {
    "quarantine_id": { "type": "string", "format": "uuid" },
    "adapter_id": { "type": "string" },
    "session_id": { "type": "string", "format": "uuid" },
    "timestamp_iso8601": { "type": "string", "format": "date-time" },
    "trigger_condition": {
      "type": "string",
      "enum": ["CB-01_CRASH_RATE", "CB-02_ANTI_CHEAT", "CB-03_INTEGRITY", "CB-04_INIT_TIMEOUT"]
    },
    "failure_category": {
      "type": "string",
      "enum": ["CATEGORY_A", "CATEGORY_B", "CATEGORY_C", "CATEGORY_D", "CATEGORY_E"]
    },
    "crash_count_in_window": { "type": "integer", "minimum": 1 },
    "last_exit_code": { "type": "string" },
    "diagnostic_summary": { "type": "string" },
    "remediation_instructions": { "type": "string" }
  },
  "additionalProperties": false
}
```

---

## 7. TRACEABILITY AND SPECIFICATION COMPLIANCE

| Requirement ID | Trace Source | Implementation Section | Verification Method |
| :--- | :--- | :--- | :--- |
| **REQ-SCR-001** | Phase 2 Step 12 (`NLE-SPC-ADP-001`) | Section 2.1, 2.2 (Two-Tier Teardown Architecture) | Verified graceful drain sequence within 5,000ms. |
| **REQ-SCR-002** | `ADR-0002` (Process Isolation) | Section 3.1, 3.2 (Supervisor Resource Reclamation) | Verified Win32 Job Object and Named Pipe teardown. |
| **REQ-SCR-003** | `BLK-SEC-01` (Anti-Cheat Dominance) | Section 4.1, 6.1 (Category C Instant Quarantine) | Verified zero-retry policy on anti-cheat detection. |
| **REQ-SCR-004** | `DEC-002` (Medium IL Sandboxing) | Section 4.1, 6.1 (Category D Privilege Escalation) | Verified quarantine escalation on token manipulation. |
| **REQ-SCR-005** | Step 39 (`NLE-MOD-CAN-001`) | Section 5.3 (State Re-Establishment Sequence) | Verified capability state reconciliation after crash. |
| **REQ-SCR-006** | `DEC-009` (Volume D Storage) | Section 6.3 (Quarantine Audit Artifact Storage) | Verified persistent storage to project audit directory. |

---
*End of Specification — Document `NLE-SPC-SCR-001`.*
