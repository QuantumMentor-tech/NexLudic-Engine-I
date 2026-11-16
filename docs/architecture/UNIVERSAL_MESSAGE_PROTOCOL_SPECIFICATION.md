# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Universal Message Protocol Specification

**Document ID:** `NLE-SPC-MSG-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 13 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Protocol Specification  
**Traceability:** `ADR-0002` (Out-of-Process Adapters), `ADR-0003` (Dual-Channel IPC), `NLE-ARC-IPC-001` (IPC Architecture)

---

## 1. Protocol Purpose & Scope

The **Universal Message Protocol (UMP)** defines the standardized, transport-independent messaging format and state machine governing all communication between the NexLudic Engine Core Host and external game adapters, worker processes, and sidecar integration tools.

UMP decouples application-level coordination from physical transport mechanics. Whether a message travels over standard input/output streams (`stdio`), Windows Named Pipes, Unix domain sockets, or an in-memory queue, the message envelope, routing metadata, correlation semantics, version negotiation, and error handling follow identical logical invariants.

---

## 2. Logical Message Envelope Model

Every message transmitted within the NexLudic Engine ecosystem conforms to the standard **Universal Message Envelope**. The envelope encapsulates transport-independent metadata required for routing, correlation, auditing, and payload validation:

```
+-----------------------------------------------------------------------------------+
|                           UNIVERSAL MESSAGE ENVELOPE                              |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | ENVELOPE HEADER                                                             |  |
|  | - protocol_version : string (SemVer, e.g. "1.0.0")                          |  |
|  | - message_id       : string (UUIDv4, globally unique)                       |  |
|  | - correlation_id   : string? (UUIDv4, null for root requests/events)        |  |
|  | - message_type     : string (Hierarchical dotted string)                    |  |
|  | - timestamp_utc    : string (ISO 8601 UTC timestamp, nanosecond precision)  |  |
|  | - source_endpoint  : string (e.g. "core.supervisor", "adapter.skyrim")      |  |
|  | - target_endpoint  : string (e.g. "adapter.skyrim", "core.supervisor")      |  |
|  | - channel_type     : enum ("CONTROL" | "DATA" | "DIAGNOSTIC")               |  |
|  | - sequence_number  : uint64 (Monotonically increasing per-endpoint counter) |  |
|  +-----------------------------------------------------------------------------+  |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | EXECUTION CONTEXT (Optional / Request-Scoped)                               |  |
|  | - timeout_ms       : uint32? (Milliseconds allocated for completion)       |  |
|  | - deadline_utc     : string? (Absolute ISO 8601 cancellation cutoff)       |  |
|  | - trace_id         : string? (Distributed W3C TraceContext trace ID)       |  |
|  +-----------------------------------------------------------------------------+  |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | MESSAGE PAYLOAD (Polymorphic Body)                                          |  |
|  | - Type-specific parameters, data entities, state snapshots, or result objects|  |
|  +-----------------------------------------------------------------------------+  |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | ERROR PAYLOAD (Populated on Failure Responses)                              |  |
|  | - code, category, message, details, retryable, trace_id                       |  |
|  +-----------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------+
```

---

## 3. The 11 Standard Message Categories

All messages transmitted within UMP belong to one of 11 strictly defined functional categories:

```
                                  UMP MESSAGE TAXONOMY
                                           |
    +-------------------+------------------+-------------------+-------------------+
    |                   |                  |                   |                   |
 1. SESSION          2. HEALTH          3. CAPABILITY       4. OPERATION        5. NOTIFICATION
 - Handshake Req     - Ping             - Query             - Invoke Req        - Telemetry Event
 - Handshake Resp    - Pong             - Manifest          - Invoke Accepted   - State Changed
                     - Status Report    - Update Notify     - Invoke Progress   - Target Focus
                                                            - Invoke Completed
    +-------------------+------------------+-------------------+-------------------+
    |                   |                  |                   |                   |
 6. CANCELLATION     7. DIAGNOSTICS     8. FLOW CONTROL     9. SHUTDOWN        10. STREAMING
 - Cancel Req        - Log Event        - Ack Frame         - Terminate Req     - Data Chunk
 - Cancel Resp       - Trace Event      - Backpressure Signal- Terminate Resp   - Stream Control
                     - Dump Capture                                            11. EXTENSION
                                                                                - Vendor/Custom
```

### 3.1 Category 1: Session Negotiation
* `session.handshake.request`: Transmitted by Core to initiate the communication session. Delivers protocol capabilities, session security token, assigned worker ID, and physical data pipe endpoint.
* `session.handshake.response`: Returned by the worker acknowledging session establishment, declaring runtime version, selected protocol version, and adapter initialization status (`READY` | `FAILED`).

### 3.2 Category 2: Readiness & Health
* `health.ping`: Transmitted periodically (every 5s) by Core to verify worker liveness and responsiveness.
* `health.pong`: Transmitted immediately by worker confirming liveness, carrying CPU usage, private memory working set, and current queue depth.
* `health.status_report`: Unsolicited notification emitted by worker if it detects internal degradation (e.g. underlying game crash, resource starvation).

### 3.3 Category 3: Capability Discovery
* `capability.query`: Dispatched by Core to inspect the adapter's supported operations, schema versions, and game integration traits.
* `capability.manifest`: Returned by worker containing the full capability descriptor (traits supported, mod scaffold templates, schema versions, file extensions).
* `capability.update`: Emitted dynamically by worker if capabilities change during runtime (e.g., game runtime injected successfully, enabling live telemetry trait).

### 3.4 Category 4: Operation Invocation
* `operation.invoke.request`: Core orders the worker to execute a concrete task (e.g. `scaffold_mod_workspace`, `compile_scripts`, `read_game_save`).
* `operation.invoke.accepted`: Worker confirms acceptance of request, providing an `operation_id` and estimated duration.
* `operation.invoke.progress`: Worker emits incremental progress reports (percentage, stage, status message).
* `operation.invoke.completed`: Final response delivering output artifacts, execution metrics, and status code (`SUCCESS` | `FAILURE`).

### 3.5 Category 5: Asynchronous Notifications
* `event.telemetry.batch`: Asynchronous bundle of game state observations, coordinates, and health values.
* `event.state.changed`: Notification of state transition (e.g., Game Engine changed state from `MENU` to `INGAME`).
* `event.filesystem.modified`: Notification that a staging file was altered or generated by an external mod tool.

### 3.6 Category 6: Cancellation & Interruption
* `operation.cancel.request`: Core requests immediate cooperative cancellation of an active `operation_id`.
* `operation.cancel.response`: Worker acknowledges cancellation status (`CANCELLED`, `ALREADY_COMPLETED`, `NOT_CANCELLABLE`).

### 3.7 Category 7: Diagnostic & Log Forwarding
* `diagnostic.log.event`: Worker forwards structured logs (`TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL`) to the Core central logging pipeline.
* `diagnostic.trace.event`: Worker transmits distributed tracing spans.
* `diagnostic.dump.capture`: Worker reports location of a crash dump or memory diagnostic file.

### 3.8 Category 8: Acknowledgement & Flow Control
* `flow.ack`: Confirms receipt and persistence of sequence-critical frames.
* `flow.backpressure`: Transmitted by either endpoint when internal consumer queues exceed high-water marks (e.g. 80% capacity), instructing sender to throttle transmission rate.

### 3.9 Category 9: Graceful Shutdown & Termination
* `session.terminate.request`: Core instructs worker to cease operations, flush buffers, release file locks, and exit cleanly within the specified grace period.
* `session.terminate.response`: Worker confirms clean shutdown readiness immediately prior to calling process exit.

### 3.10 Category 10: Streaming Telemetry & Data Frames
* `stream.data.chunk`: High-frequency binary or compact JSON telemetry frame transmitted across Channel 2.
* `stream.control`: Pause, resume, or adjust sampling frequency (e.g. throttle from 60 Hz to 10 Hz when game is paused).

### 3.11 Category 11: Vendor & Extension Messages
* `extension.custom`: Namespaced custom extensions (`extension.<vendor>.<action>`), allowed only when capability flags explicitly declare experimental or third-party features.

---

## 4. Message Framing & Physical Delimitation

Physical stream parsing requires unambiguous framing to prevent buffer overflows, message truncation, and desynchronization:

### 4.1 Stdio Control Channel Framing (Newline-Delimited JSON)
On the standard input and output streams, messages are framed as **Newline-Delimited JSON (ndjson)**:
* Each complete message envelope is serialized as a single-line UTF-8 JSON object.
* The message MUST be terminated by a single ASCII Line Feed character (`\n`, `0x0A`).
* Leading or trailing Carriage Return characters (`\r`, `0x0D`) MUST be stripped by the receiver.
* **Maximum Message Length:** The maximum permitted single-line message size on the Control Channel is **16 Megabytes (16,777,216 bytes)**. Any message exceeding this threshold triggers a fatal `TRANSPORT_BUFFER_OVERFLOW` error and stream disconnect.
* Empty lines and whitespace-only lines are ignored as keep-alive padding.

### 4.2 Named Pipe Data Channel Framing (Message Mode & Length-Prefixed Frames)
On the Windows Named Pipe Data Channel, messages are framed using native Win32 Message Mode combined with a 4-byte length header:
* **Pipe Configuration:** `PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE`.
* **Frame Header:** 4-byte unsigned integer in Little-Endian byte order (`uint32_le`) denoting the exact length of the trailing payload bytes.
* **Maximum Frame Size:** The maximum frame size on the Data Channel is **64 Megabytes (67,108,864 bytes)**, accommodating high-resolution asset chunks or bulk telemetry snapshots.
* If a frame exceeds the buffer, Windows returns `ERROR_MORE_DATA`; the consumer reads consecutive chunks until the entire frame is assembled.

---

## 5. Correlation & Operation Identity

To support concurrent, non-blocking asynchronous operations, UMP enforces strict correlation invariants:

```
Core Host                                                   Adapter Worker
   |                                                              |
   | 1. operation.invoke.request                                  |
   |    [message_id: M-101, correlation_id: null]                 |
   |    [payload: { operation_id: OP-501, action: "scaffold" }]   |
   | -----------------------------------------------------------> |
   |                                                              |
   | 2. operation.invoke.accepted                                 |
   |    [message_id: M-102, correlation_id: M-101]                |
   |    [payload: { operation_id: OP-501, status: "ACCEPTED" }]   |
   | <----------------------------------------------------------- |
   |                                                              |
   | 3. operation.invoke.progress                                 |
   |    [message_id: M-103, correlation_id: M-101]                |
   |    [payload: { operation_id: OP-501, progress_pct: 45.0 }]   |
   | <----------------------------------------------------------- |
   |                                                              |
   | 4. operation.invoke.completed                                |
   |    [message_id: M-104, correlation_id: M-101]                |
   |    [payload: { operation_id: OP-501, status: "SUCCESS" }]    |
   | <----------------------------------------------------------- |
```

1. **Root Request:** A request generated by the Core assigns a new unique `message_id` (UUIDv4) and sets `correlation_id` to `null`.
2. **Correlated Response:** Every response message generated in answer to a request MUST set its `correlation_id` to the exact `message_id` of the originating request.
3. **Operation ID (`operation_id`):** For long-running asynchronous workflows, the payload contains a business-level `operation_id` (UUIDv4). All subsequent progress, cancellation, and completion messages carry this `operation_id` in their payload, allowing multiple operations to execute concurrently without interleaving conflicts.

---

## 6. Version Negotiation & Extensibility Rules

UMP is versioned using Semantic Versioning 2.0.0 (`MAJOR.MINOR.PATCH`):

1. **Protocol Version Matching:**
   * During session handshake, Core transmits `supported_protocol_versions: ["1.0.0", "1.1.0"]`.
   * Worker inspects the array and returns `selected_protocol_version: "1.0.0"` in its handshake response.
   * If there is no overlapping `MAJOR` version, the handshake fails with error `PROTOCOL_UNSUPPORTED_VERSION`.
2. **Additive Evolution & Unknown Fields:**
   * Receivers (both Core and Workers) MUST ignore unrecognized properties in the envelope or payload (`System.Text.Json` ignore extra attributes; Protobuf ignore unknown field tags).
   * Removing or renaming mandatory envelope fields requires a `MAJOR` version bump.
   * Adding optional envelope or payload fields requires a `MINOR` version bump.
   * Bug fixes and documentation adjustments require a `PATCH` version bump.

---

## 7. Traceability Matrix

* **NLE-ARC-IPC-001:** Adheres to the Dual-Channel IPC architecture and process supervision boundaries.
* **ADR-0002:** Supports cross-language, out-of-process isolation for all game adapters.
* **ADR-0003:** Specifies JSON-RPC / ndjson for Control Channel and binary framing for Data Channel.
* **REQ-MOD-001 / REQ-MOD-002:** Implements operation schemas for mod workspace scaffolding and compilation.
* **REQ-TEL-001:** Implements streaming and telemetry event message specifications.

