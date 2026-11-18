# ADAPTER LIFECYCLE EVENTS, DIAGNOSTICS, AND AUDIT CONTRACT
**Document ID:** `NLE-CON-LEA-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 40 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Software Architect, Distributed-State Designer, Evidence & Provenance Engineer, Senior QA Engineer  
**Classification:** Core Runtime Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This contract establishes the formal specification for adapter lifecycle observability, diagnostic telemetry, event ordering, and tamper-evident audit logging within NexLudic Engine. Because game adapters operate in hostile, multi-process desktop environments interacting with live target game hooks, real-time debugging, crash forensics, and security audits require an immutable, structured historical record of all state transitions and operational health shifts.

This specification details:
1. The authoritative 18-event lifecycle taxonomy capturing all runtime transitions.
2. The `LifecycleEventRecord` JSON Schema (`NLE-SCH-LER-001`).
3. Monotonic ordering, high-resolution timestamps, and high-frequency event deduplication rules.
4. The in-memory lock-free ring buffer architecture.
5. Persistent audit storage on Volume D (`DEC-009`) featuring rolling SHA-256 tamper-evident integrity hashes.
6. The standardized diagnostic error and warning code catalog.

### 1.2 Architectural Integration
```
+--------------------------------------------------------------------------------------------------------+
|                                  EVENT GENERATION SUB-SYSTEMS                                          |
|                                                                                                        |
|  [ Core Supervisor ]       [ Watchdog Health Timer ]       [ Adapter Worker IPC ]     [ User / UI ]    |
+----------+----------------------------+-------------------------------+---------------------+----------+
           |                            |                               |                     |
           +----------------------------+---------------+---------------+---------------------+
                                                        |
                                                        v
                                   +-----------------------------------------+
                                   |      LIFECYCLE EVENT CAPTURE HUB        |
                                   |  - Assign Monotonic sequence_no         |
                                   |  - Nanosecond QueryPerformanceCounter   |
                                   |  - Deduplication Filter                 |
                                   +--------------------+--------------------+
                                                        |
                                +-----------------------+-----------------------+
                                |                                               |
                                v                                               v
              +-----------------------------------+           +-----------------------------------+
              |     IN-MEMORY RING BUFFER         |           |       VOLUME D AUDIT WRITER       |
              | - Lock-free 4096-slot ring        |           | - JSONL Append-only stream        |
              | - Real-time UI telemetry feed     |           | - Rolling SHA-256 chain hash      |
              | - Memory-mapped read interface    |           | - Anchored to DEC-009 Project Dir |
              +-----------------------------------+           +-----------------------------------+
```

---

## 2. STANDARDIZED LIFECYCLE EVENT TAXONOMY

The NexLudic platform defines exactly 18 standardized lifecycle event types. Every state transition, health shift, supervisor action, and fatal condition MUST map to one of these discrete events:

| Index | Event Type Identifier | Severity | Emitter / Actor | Description / Trigger Condition |
| :---: | :--- | :--- | :--- | :--- |
| **01** | `ADMISSION_REQUESTED` | `INFO` | `SESSION_MANAGER` | Dynamic admission evaluation requested for an adapter. |
| **02** | `ADMISSION_GRANTED` | `INFO` | `CORE_SUPERVISOR` | All 6 runtime admission gates passed successfully. |
| **03** | `ADMISSION_REJECTED` | `ERROR` | `CORE_SUPERVISOR` | Admission denied due to failed gate (anti-cheat, hash, consent). |
| **04** | `SPAWN_INITIATED` | `INFO` | `CORE_SUPERVISOR` | Worker process creation invoked via `CreateProcessW` under Medium IL. |
| **05** | `SPAWN_FAILED` | `ERROR` | `CORE_SUPERVISOR` | OS failed to spawn worker process (bad executable, missing DLLs). |
| **06** | `HANDSHAKE_INITIATED` | `INFO` | `CORE_SUPERVISOR` | `adapter.initialize` request dispatched across control IPC pipe. |
| **07** | `HANDSHAKE_COMPLETED` | `INFO` | `ADAPTER_WORKER` | Worker accepted initialization parameters and returned success. |
| **08** | `HANDSHAKE_TIMED_OUT` | `ERROR` | `WATCHDOG` | Worker failed to acknowledge handshake within 10,000 ms. |
| **09** | `READY_DECLARED` | `INFO` | `ADAPTER_WORKER` | Worker finished hook setup and emitted valid `ReadyReport`. |
| **10** | `EXECUTION_STARTED` | `INFO` | `SESSION_MANAGER` | Lifecycle transitioned to `RUNNING`; active data loops engaged. |
| **11** | `EXECUTION_PAUSED` | `INFO` | `SESSION_MANAGER` | Worker instructed to suspend active game memory polling. |
| **12** | `HEALTH_STATE_CHANGED` | `WARN` / `INFO` | `WATCHDOG` | Operational health shifted (e.g., `HEALTHY` $\rightarrow$ `DEGRADED`). |
| **13** | `DEGRADATION_TRIGGERED` | `WARN` | `CORE_SUPERVISOR` | Non-critical capability faulted; capability marked `UNAVAILABLE`. |
| **14** | `TEARDOWN_INITIATED` | `INFO` | `CORE_SUPERVISOR` | Orderly shutdown dispatched (`adapter.shutdown`); grace timer started. |
| **15** | `TEARDOWN_COMPLETED` | `INFO` | `ADAPTER_WORKER` | Worker cleanly unhooked and signaled readiness to exit. |
| **16** | `PROCESS_EXITED` | `INFO` / `WARN` | `CORE_SUPERVISOR` | Worker process terminated; exit code captured and evaluated. |
| **17** | `FORCED_TERMINATION` | `CRITICAL` | `CORE_SUPERVISOR` | Grace period expired or violation detected; `TerminateProcess` executed. |
| **18** | `CIRCUIT_BREAKER_TRIPPED` | `CRITICAL` | `CORE_SUPERVISOR` | Crash limit exceeded or anti-cheat detected; adapter `QUARANTINED`. |

---

## 3. LIFECYCLE EVENT RECORD SCHEMA (`NLE-SCH-LER-001`)

Every emitted event MUST validate strictly against the following JSON schema:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/adapters/lifecycle-event-record.schema.json",
  "title": "LifecycleEventRecord",
  "type": "object",
  "required": [
    "event_id",
    "sequence_no",
    "timestamp_utc",
    "timestamp_qpc",
    "adapter_id",
    "session_id",
    "event_type",
    "previous_lifecycle_state",
    "new_lifecycle_state",
    "previous_health_state",
    "new_health_state",
    "actor",
    "severity",
    "diagnostic_code"
  ],
  "properties": {
    "event_id": { "type": "string", "format": "uuid" },
    "sequence_no": { "type": "integer", "minimum": 1 },
    "timestamp_utc": { "type": "string", "format": "date-time" },
    "timestamp_qpc": { "type": "integer", "description": "High-resolution QueryPerformanceCounter tick" },
    "adapter_id": { "type": "string", "pattern": "^[a-z0-9-]+(\\.[a-z0-9-]+)+$" },
    "session_id": { "type": "string", "format": "uuid" },
    "correlation_id": { "type": "string", "format": "uuid" },
    "event_type": {
      "type": "string",
      "enum": [
        "ADMISSION_REQUESTED",
        "ADMISSION_GRANTED",
        "ADMISSION_REJECTED",
        "SPAWN_INITIATED",
        "SPAWN_FAILED",
        "HANDSHAKE_INITIATED",
        "HANDSHAKE_COMPLETED",
        "HANDSHAKE_TIMED_OUT",
        "READY_DECLARED",
        "EXECUTION_STARTED",
        "EXECUTION_PAUSED",
        "HEALTH_STATE_CHANGED",
        "DEGRADATION_TRIGGERED",
        "TEARDOWN_INITIATED",
        "TEARDOWN_COMPLETED",
        "PROCESS_EXITED",
        "FORCED_TERMINATION",
        "CIRCUIT_BREAKER_TRIPPED"
      ]
    },
    "previous_lifecycle_state": {
      "type": "string",
      "enum": [
        "NOT_ADMITTED", "ADMISSION_PENDING", "ADMITTED", "INITIALIZING",
        "READY", "RUNNING", "STOPPING", "STOPPED", "FAILED", "QUARANTINED"
      ]
    },
    "new_lifecycle_state": {
      "type": "string",
      "enum": [
        "NOT_ADMITTED", "ADMISSION_PENDING", "ADMITTED", "INITIALIZING",
        "READY", "RUNNING", "STOPPING", "STOPPED", "FAILED", "QUARANTINED"
      ]
    },
    "previous_health_state": {
      "type": "string",
      "enum": ["NOT_MONITORED", "HEALTHY", "DEGRADED", "UNHEALTHY", "UNKNOWN", "STALE"]
    },
    "new_health_state": {
      "type": "string",
      "enum": ["NOT_MONITORED", "HEALTHY", "DEGRADED", "UNHEALTHY", "UNKNOWN", "STALE"]
    },
    "actor": {
      "type": "string",
      "enum": ["CORE_SUPERVISOR", "ADAPTER_WORKER", "SESSION_MANAGER", "WATCHDOG", "USER"]
    },
    "severity": {
      "type": "string",
      "enum": ["INFO", "WARN", "ERROR", "CRITICAL"]
    },
    "diagnostic_code": { "type": "string", "pattern": "^(OK|[A-Z]{3,4}_[A-Z0-9_]+)$" },
    "diagnostic_details": {
      "type": "object",
      "properties": {
        "message": { "type": "string" },
        "exit_code": { "type": "integer" },
        "capability_id": { "type": "string" },
        "latency_ms": { "type": "number" },
        "memory_bytes": { "type": "integer" },
        "violation_type": { "type": "string" }
      },
      "additionalProperties": true
    }
  },
  "additionalProperties": false
}
```

---

## 4. EVENT ORDERING, DEDUPLICATION, AND BUFFER PROTOCOL

### 4.1 Strict Monotonic Ordering
Distributed time across separate Windows processes is susceptible to clock skew and NTP step corrections. To guarantee causality:
1. **Monotonic Sequence:** The Core Supervisor assigns a 64-bit strictly increasing unsigned integer (`sequence_no`) to every captured event. Causality is evaluated by `sequence_no`, not UTC clock.
2. **Nanosecond Counters:** Every event captures the Win32 `QueryPerformanceCounter` tick (`timestamp_qpc`) alongside UTC ISO-8601 to measure exact execution latency intervals down to sub-microsecond precision.

### 4.2 High-Frequency Deduplication Filter
Certain operational conditions (such as 60Hz frame watcher errors or repeated missed heartbeats) threaten to saturate event logs. The event capture hub enforces deduplication rules:
- **Rule DEDUP-01 (Continuous Health State):** A heartbeat tick that does NOT change the current `health_state` is NOT emitted as an event. Only transition events (`HEALTH_STATE_CHANGED`) are logged.
- **Rule DEDUP-02 (Repeated Telemetry Warnings):** If a worker emits identical telemetry frame warnings, subsequent identical warnings within a 1,000 ms window are collapsed into an aggregate counter rather than generating distinct events.

### 4.3 In-Memory Lock-Free Ring Buffer
For high-speed inter-thread communication and immediate UI diagnostics updates:
- Capacity: Exactly `4,096` event slots per active adapter instance.
- Concurrency: Single-producer (Event Capture Hub) multi-consumer (UI telemetry feed, audit flush thread) lock-free ring buffer implemented using atomic head/tail pointer increments (`InterlockedIncrement64`).
- Overwrite Semantics: If consumer threads fall behind, the buffer wraps and overwrites oldest events, but the persistent disk writer retains the complete authoritative log.

---

## 5. AUDIT STORAGE AND TAMPER-EVIDENT SPECIFICATION

### 5.1 Storage Hierarchy on Volume D
In accordance with `DEC-009`, all lifecycle audit logs are anchored to Volume D:
```
D:\NexLudic\Projects\<projectId>\Adapters\<adapterId>\.audit\lifecycle\
    ├── events_<sessionId>_0001.jsonl
    ├── events_<sessionId>_0002.jsonl
    └── checkpoint_<sessionId>.sha256
```

### 5.2 Tamper-Evident SHA-256 Rolling Hash Chain
To ensure that post-incident forensics cannot be falsified or manipulated:
1. **Line-Oriented Chain:** Each line in `events_<sessionId>.jsonl` contains a single JSON string formatted according to `NLE-SCH-LER-001`, terminated by `\n`.
2. **Chain Hash Calculation:** For event line $k$, the hash $H_k$ is computed over the previous hash and the current line content:

$$H_0 = \text{SHA256}(\text{"NEXLUDIC_LIFECYCLE_GENESIS\_"} \parallel \text{adapter\_id} \parallel \text{session\_id})$$

$$H_k = \text{SHA256}(H_{k-1} \parallel \text{EventLineJSON}_k)$$

3. **Checkpoint Verification:** Upon session conclusion, the final digest $H_N$ is written to `checkpoint_<sessionId>.sha256`. During verification, the validator re-computes the chain. Any single mutated, inserted, or deleted event breaks the hash chain.

---

## 6. STANDARDIZED DIAGNOSTIC ERROR & WARNING CODE CATALOG

All events with severity `WARN`, `ERROR`, or `CRITICAL` MUST supply a standardized diagnostic code from the following authoritative catalog:

| Code Category | Diagnostic Code | Severity | Description and Remediation |
| :--- | :--- | :--- | :--- |
| **Admission** | `ERR_ADM_HASH_MISMATCH` | `ERROR` | Binary SHA-256 does not match registry. Re-validate adapter package. |
|  | `ERR_ADM_ANTI_CHEAT_DETECTED` | `CRITICAL` | Kernel anti-cheat detected. Adapter admission denied immediately. |
|  | `ERR_ADM_ELEVATION_ATTEMPT` | `CRITICAL` | High/System integrity token requested. Refused under Medium IL rule. |
|  | `ERR_ADM_CONSENT_MISSING` | `ERROR` | User consent token absent or revoked for required capabilities. |
| **Init & Handshake**| `ERR_HND_IPC_CONNECT_FAILED` | `ERROR` | Core Supervisor could not open Named Pipe connection to worker. |
|  | `ERR_HND_TIMEOUT_EXPIRED` | `ERROR` | Worker failed to respond to `adapter.initialize` within 10,000 ms. |
|  | `ERR_HND_SCHEMA_VALIDATION` | `ERROR` | Handshake payload violated JSON schema specification. |
| **Runtime & Health** | `WARN_HLT_MISSED_HEARTBEAT` | `WARN` | Ping probe missed. Health shifted to `STALE`. |
|  | `ERR_HLT_FATAL_HEARTBEAT_LOSS`| `ERROR` | 3 consecutive pings missed (>6000ms). Teardown or restart initiated. |
|  | `WARN_HLT_FRAME_LATENCY_SPIKE`| `WARN` | 60Hz telemetry frame processing exceeded 33.3ms budget. |
|  | `ERR_HLT_CAPABILITY_FAULT` | `ERROR` | Subsystem capability crashed. Fallback to `DEGRADED` mode. |
| **Teardown & Crash**| `ERR_SHT_GRACE_PERIOD_EXPIRED` | `ERROR` | Worker hung during shutdown; forced kill via `TerminateProcess`. |
|  | `CRIT_CRASH_CIRCUIT_TRIPPED` | `CRITICAL` | Crash limit exceeded (3 in 60s). Adapter locked in `QUARANTINED`. |
|  | `CRIT_SEC_INTEGRITY_VIOLATION`| `CRITICAL` | Process attempted unauthorized memory tampering. Hard kill executed. |

---

## 7. TRACEABILITY AND VERIFICATION MATRIX

| Requirement ID | Trace Source | Implementation Section | Verification Method |
| :--- | :--- | :--- | :--- |
| **REQ-LEA-001** | Phase 2 Step 12 (`NLE-SPC-ADP-001`) | Section 2 (18-Event Lifecycle Taxonomy) | Verified all state transitions produce discrete events. |
| **REQ-LEA-002** | Step 37 (`NLE-SPC-VCR-001`) | Section 3 (Lifecycle Event Record Schema) | Verified schema enforcement for diagnostic logging. |
| **REQ-LEA-003** | Step 39 (`NLE-MOD-CAN-001`) | Section 2 (Events 12 & 13: Degradation) | Verified capability availability shift tracing. |
| **REQ-LEA-004** | `DEC-009` (Volume D Storage) | Section 5.1 (Persistent Audit Logging) | Verified directory hierarchy on Volume D. |
| **REQ-LEA-005** | Security Baseline (`REQ-SEC-001`) | Section 5.2 (SHA-256 Rolling Hash Chain) | Verified cryptographic tamper detection in JSONL logs. |

---
*End of Specification — Document `NLE-CON-LEA-001`.*
