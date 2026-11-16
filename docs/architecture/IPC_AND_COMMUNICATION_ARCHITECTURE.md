# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Inter-Process Communication & Communication Architecture

**Document ID:** `NLE-ARC-IPC-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 13 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Architecture Specification  
**Traceability:** `ADR-0002` (Out-of-Process Adapter Architecture), `ADR-0003` (Dual-Channel IPC), `ADR-0009` (Sandboxed Adapter Execution), `REQ-SEC-001` (Zero Elevation), `REQ-SAFE-001` (Anti-Cheat Invariant)

---

## 1. Executive Summary & Purpose

The NexLudic Engine platform coordinates heterogeneous game modification tools, telemetry engines, input handlers, and AI orchestration pipelines. Because target games vary radically in runtime technology (Unity/Mono/.NET, Unreal/C++, Godot/GDScript, proprietary native binaries, Java), NexLudic Engine explicitly forbids running untrusted third-party game modding scripts or runtime injection hooks inside the primary application process (`ADR-0002`).

Instead, the platform adopts an **Out-of-Process Worker Model** supervised by the Core Host Application (`NexLudic.App` / `NexLudic.Core`). This document establishes the **Inter-Process Communication (IPC) and Communication Architecture** governing how the Core platform discovers, supervises, exchanges data with, and terminates external game adapters. It defines the communication patterns, candidate transport evaluations, dual-channel physical baseline, endpoint lifecycle, security boundaries, and failure isolation guarantees.

```
+-----------------------------------------------------------------------------------+
|                            NEXLUDIC CORE APPLICATION HOST                         |
|                                                                                   |
|  +--------------------+   +-----------------------+   +------------------------+  |
|  |  Adapter Super-    |   | Communication Gateway |   | Telemetry Dispatcher   |  |
|  |  visor & Lifecycle |   | (JSON-RPC Router)     |   | (Protobuf Stream Sink) |  |
|  +---------+----------+   +-----------+-----------+   +-----------+------------+  |
+------------|--------------------------|---------------------------|---------------+
             | Stdio Management         | Control Messages          | Named Pipe
             | (Process Standard Handles)| (Requests / Responses)   | (High-Throughput)
             v                          v                           v
+-----------------------------------------------------------------------------------+
|               SECURE IPC CHANNELS (Restricted User SID Access Control)            |
|                                                                                   |
|    Channel 1: Control Channel (Standard Input / Standard Output, JSON-RPC 2.0)    |
|    Channel 2: Telemetry Data Channel (Windows Named Pipe, Protobuf Streaming)     |
+-----------------------------------------------------------------------------------+
             ^                          ^                           ^
             | Handle Inheritance       | Control Parser            | Named Pipe Client
             | (Restricted to Worker)   | (Command Dispatch)        | (Stream Producer)
+------------+--------------------------+---------------------------+---------------+
|  +---------+----------+   +-----------+-----------+   +-----------+------------+  |
|  | Worker Host Runtime|   | Adapter Contract Impl |   | Game Telemetry Engine  |  |
|  | (C# / Python / C++)|   | (Capability Provider) |   | (Observer / Hook Sink) |  |
|  +--------------------+   +-----------------------+   +------------------------+  |
|                                                                                   |
|                  EXTERNAL GAME ADAPTER WORKER (Win32 Job Sandbox)                 |
+-----------------------------------------------------------------------------------+
```

---

## 2. Communication Patterns & Interaction Styles

The NexLudic Engine architectural baseline requires six distinct communication patterns to satisfy the functional requirements of mod scaffolding, state observation, and live mashup execution:

### 2.1 Pattern 1: In-Process Direct Invocation (Core-Internal)
* **Application Scope:** Internal interactions between `NexLudic.App`, `NexLudic.Core`, `NexLudic.Domain`, and `NexLudic.Infrastructure` within the primary application process.
* **Mechanism:** Direct strongly-typed C# method invocation, C# 12 `System.Threading.Channels` for asynchronous pipeline decoupling, and asynchronous task execution via `System.Threading.Tasks.ValueTask<T>`.
* **Characteristics:** Zero serialization overhead, zero transport latency, guaranteed type safety, in-memory reference passing.

### 2.2 Pattern 2: Request / Synchronous Response (Two-Way RPC)
* **Application Scope:** Point-to-point command dispatch where the Core requires immediate deterministic confirmation or return data before continuing. Examples: `session.handshake`, `capability.query`, `workspace.validate_path`, `adapter.get_status`.
* **Mechanism:** Correlated two-way message exchange over the primary Control Channel. Each request carries a unique `message_id`; the responding worker returns a correlated response carrying `correlation_id = request.message_id`.
* **Characteristics:** Blocking or asynchronous await with strict deadline enforcement, deterministic timeout semantics, structured error propagation.

### 2.3 Pattern 3: Long-Running Asynchronous Operations with Progress Reporting
* **Application Scope:** Tasks taking from hundreds of milliseconds to several minutes. Examples: asset unpacking, code compilation, mod package scaffolding, multi-file diff computation, dependency indexing.
* **Mechanism:** Three-stage interaction:
  1. Core dispatches an `operation.invoke.request` with an assigned `operation_id`.
  2. Worker immediately acknowledges receipt with an `operation.invoke.accepted` response containing estimated execution parameters.
  3. Worker periodically emits one-way `operation.progress.event` messages carrying completed percentage, current phase, and diagnostic log entries.
  4. Upon completion or failure, the worker emits a final `operation.invoke.completed` message containing the operation result or error payload.
* **Characteristics:** Non-blocking to the caller, cancellable via correlated cancellation token, resilient against UI freezes.

### 2.4 Pattern 4: One-Way Asynchronous Notifications / Events
* **Application Scope:** Unsolicited, decoupled notifications emitted by the adapter to inform the Core of environmental changes or asynchronous lifecycle events. Examples: game process detected, game window focus changed, staging directory modified, worker health state change.
* **Mechanism:** Fire-and-forget notification message without a `correlation_id` requirement, routed through the Core event aggregator.
* **Characteristics:** Non-blocking, low-latency, ordered per sender channel, drop-safe or buffered.

### 2.5 Pattern 5: Task-Oriented Invocation with Cancellation Semantics
* **Application Scope:** Operations that must be interruptible by human user action or platform safety timeouts. Examples: user cancels asset extraction, compilation aborted due to syntax errors, emergency mashup halt.
* **Mechanism:** Two-phase cancellation:
  1. Core dispatches `operation.cancel.request` referencing the targeted `operation_id`.
  2. Worker receives the cancellation directive, signals its internal cancellation token, halts non-atomic I/O, cleans up intermediate artifacts, and responds with `operation.cancel.response` indicating whether the operation was successfully interrupted (`CANCELLED`) or had already completed (`ALREADY_COMPLETED`).
* **Characteristics:** Cooperative cancellation first; safety watchdog fallback to worker termination if unresponsive after the grace period.

### 2.6 Pattern 6: Streaming / High-Frequency Data Exchange
* **Application Scope:** Continuous, high-volume, low-latency telemetry feeds and input state synchronizations during live mashup runtime. Examples: game coordinate telemetry (60 Hz), vehicle velocity streams, health/stamina variables, virtual controller input frames.
* **Mechanism:** Dedicated binary stream over an independent high-throughput transport channel. Payloads use Protocol Buffers (`proto3`) binary encoding without text parsing overhead.
* **Characteristics:** Throughput exceeding 1,000 messages/sec, sub-millisecond serialization latency, backpressure-aware buffering, zero memory allocation on hot paths via memory pooling (`ArrayPool<byte>`).

---

## 3. Evaluation of Candidate Transports

To select an optimal, defensible IPC architecture, five transport candidates were evaluated against platform requirements:

| Evaluation Dimension | 1. In-Process (DLL / C#) | 2. Standard I/O (stdio) | 3. Windows Named Pipes | 4. Loopback TCP (127.0.0.1) | 5. Shared Memory (MMF) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Process Isolation** | None (crashes take down Core) | Complete (separate OS process) | Complete (separate OS process) | Complete (separate OS process) | Complete (separate OS process) |
| **Cross-Language Support** | Limited (C ABI / PInvoke) | Universal (any CLI runtime) | High (Win32 API / libuv) | Universal (standard sockets) | Moderate (manual layout sync) |
| **Throughput & Bandwidth** | Infinite (>10 GB/s) | Medium (~20-50 MB/s) | High (>500 MB/s) | High (~300-600 MB/s) | Maximum (>2 GB/s) |
| **Message Latency** | Sub-nanosecond | Low (<1 ms) | Very Low (<0.1 ms) | Moderate (~0.5-2 ms) | Microsecond (<0.01 ms) |
| **Message Framing** | Native object graph | Stream (requires framing/ndjson)| Message Mode or byte framing | Stream (requires framing) | Manual ring buffer framing |
| **Security & Access Control**| In-memory trust | Handle inheritance only | Win32 DACL (User SID) | Weak (any local process binds) | Win32 DACL (User SID) |
| **Firewall / Network Prompts**| None | None | None | Potential Windows Firewall prompt | None |
| **Port Conflicts** | Not applicable | Not applicable | Named endpoint (no collisions)| Frequent port collisions | Named handle |
| **Ease of Debugging** | IDE debugger attached | Direct text inspection/logging | Wireshark/NamedPipeCapture | Wireshark/pcap capture | Complex memory dump |
| **Platform Portability** | Windows / Linux / macOS | Universal POSIX / Win32 | Windows (POSIX via Domain Sockets)| Universal | POSIX shm / Windows MMF |

### 3.1 Transport Selection Analysis

1. **In-Process:** Rejected for third-party game adapters due to stability and safety risks (`ADR-0002`). A crash, memory leak, or illegal memory access in an adapter would crash the entire NexLudic Engine.
2. **Loopback TCP:** Rejected as the primary IPC mechanism. TCP on Windows requires binding to local ports, risking collisions with other developer tools or games. Furthermore, local TCP sockets lack native granular Windows User SID access control, allowing any process running under any user account on the same machine to probe open ports, and it frequently triggers intrusive Windows Firewall notification prompts.
3. **Shared Memory (Memory-Mapped Files):** Excellent throughput, but excessively complex for general RPC control flows. Lacks built-in synchronization, signaling, and message boundaries, requiring supplementary mutexes, semaphores, and manual ring-buffer management.
4. **Standard I/O (stdio):** Ideal for baseline process lifecycle, bootstrap, and control RPC. When the Core creates the child worker process, standard input and output handles are established directly by the OS kernel. No ports, no endpoint discovery, zero collision risk, and complete handle-level privacy.
5. **Windows Named Pipes:** The optimal high-performance channel for Windows desktop environments (`ADR-0003`). Supports kernel-level message framing (`PIPE_TYPE_MESSAGE`), sub-millisecond latencies, high throughput for binary telemetry, and rigorous Windows Security Identifiers (`DACL`) restricting access strictly to the current user's session token.

---

## 4. Recommended Architectural Baseline: The Dual-Channel Model

NexLudic Engine adopts a **Dual-Channel IPC Architecture** (`ADR-0003`), combining the structural simplicity of Standard I/O with the raw throughput and framing capabilities of Windows Named Pipes:

```
+-----------------------------------------------------------------------------------+
|                         DUAL-CHANNEL IPC ARCHITECTURAL MODEL                      |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | CHANNEL 1: CONTROL CHANNEL (Standard Input / Standard Output)               |  |
|  | - Protocol: JSON-RPC 2.0 / Universal Message Protocol (ndjson or length-pref)|  |
|  | - Responsibilities:                                                         |  |
|  |   * Process handshake, protocol version negotiation, and capability sync    |  |
|  |   * Synchronous RPC command execution (scaffold, validate, clean)           |  |
|  |   * Long-running operation dispatch, progress reporting, and cancellation   |  |
|  |   * Structured diagnostic log forwarding from worker to Core                |  |
|  |   * Dynamic pipe endpoint negotiation for Data Channel initialization       |  |
|  +-----------------------------------------------------------------------------+  |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | CHANNEL 2: TELEMETRY DATA CHANNEL (Windows Named Pipe: \\.\pipe\nexludic_*) |  |
|  | - Protocol: Protocol Buffers (proto3) over Pipe Message Mode                |  |
|  | - Responsibilities:                                                         |  |
|  |   * High-frequency game state telemetry streams (coordinates, physics, stats) |  |
|  |   * Input capture and injection forwarding packets                          |  |
|  |   * High-volume binary payload transfers (textures, meshes, audio snippets) |  |
|  |   * Frame tick synchronizations and time-critical mashup heartbeats         |  |
|  +-----------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------+
```

### 4.1 Channel 1: Control Channel Details
* **Physical Medium:** Child process standard input (`stdin`) and standard output (`stdout`). Standard error (`stderr`) is reserved exclusively for unhandled worker process crash dumps and raw runtime diagnostics.
* **Logical Protocol:** Universal Message Protocol formatted as JSON-RPC 2.0 compliant envelopes, serialized as UTF-8 newline-delimited JSON (`ndjson`).
* **Directionality:** Bidirectional full-duplex. Core writes requests to worker `stdin` and reads responses/events from worker `stdout`.
* **Framing:** Each JSON message is serialized on a single line terminated by the standard ASCII line feed (`\n`, `0x0A`). Carriage returns (`\r`) are stripped during parsing.

### 4.2 Channel 2: Telemetry Data Channel Details
* **Physical Medium:** Windows Named Pipe in Message Mode (`PIPE_TYPE_MESSAGE | PIPE_READMODE_MESSAGE`).
* **Endpoint Naming Pattern:** `\\.\pipe\nexludic_telemetry_{SESSION_ID}_{WORKER_ID}` where `SESSION_ID` and `WORKER_ID` are cryptographically random UUIDv4 identifiers generated by the Core during worker launch.
* **Security Descriptor:** Configured with a Win32 Security Descriptor Definition Language (`SDDL`) string restricting connection rights exclusively to the Current User Security Identifier (`TokenUser`). Network logon and remote access are explicitly denied (`DENY;NETWORK`).
* **Logical Protocol:** Protocol Buffers (`proto3`) binary frames.

---

## 5. Process Lifecycle & Supervision Architecture

External adapter worker processes are untrusted execution units that must be rigorously supervised by the Core platform:

```
+---------------+     Spawn Worker with Job Object     +---------------+
|               | -----------------------------------> |               |
|               |                                      |               |
|               |      session.handshake.request       |               |
|               | -----------------------------------> |               |
|               |                                      |               |
|               |      session.handshake.response      |               |
|               | <----------------------------------- |               |
|               |                                      |               |
|               |       capability.query.request       |               |
|   NEXLUDIC    | -----------------------------------> |    ADAPTER    |
|     CORE      |                                      |    WORKER     |
|  SUPERVISOR   |      capability.manifest.response    |    PROCESS    |
|               | <----------------------------------- |               |
|               |                                      |               |
|               |    Periodic Heartbeat (Ping / Pong)  |               |
|               | <==================================> |               |
|               |                                      |               |
|               |      session.terminate.request       |               |
|               | -----------------------------------> |               |
|               |                                      |               |
|               |      Worker Exit (ExitCode = 0)      |               |
|               | <----------------------------------- |               |
+---------------+                                      +---------------+
        |                                                      |
        | Heartbeat Timeout (>15s) or Unresponsive Shutdown    |
        +------------------------------------------------------+
        | Forceful Win32 Job Object Termination (TerminateJob) |
        v                                                      v
```

### 5.1 Process Creation & Sandbox Setup
1. **Creation Flags:** The Core creates the worker process via Win32 `CreateProcess` (or .NET `Process.Start`) with `bInheritHandles = FALSE` to prevent leaking open file descriptors, except for the specific redirected standard handles.
2. **Win32 Job Object Assignment (`ADR-0009`):** Before execution begins, the worker process is immediately assigned to an active Win32 Job Object configured with:
   * `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`: Guarantees that if the Core process exits, crashes, or is killed, Windows kernel will automatically terminate all child adapter processes.
   * `JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION`: Prevents interactive Windows crash dialogs from blocking process cleanup.
   * Memory Commit Limits: Caps peak private working set memory per worker (default 2 GB) to prevent system memory exhaustion.

### 5.2 Handshake & Initialization Sequence
1. Upon spawn, the worker initializes its local runtime and listens on `stdin`.
2. The Core transmits a `session.handshake.request` message over `stdin` within a strict **Handshake Deadline** (default: 10,000 ms).
3. The handshake request provides the worker's unique ID, protocol version, session security token, and the named pipe path for Channel 2.
4. The worker establishes its connection to the named pipe, initializes its internal modules, and returns `session.handshake.response` over `stdout`.
5. If the handshake response is not received before the deadline, or if version negotiation fails, the Core logs a fatal initialization error and immediately terminates the worker process.

### 5.3 Health Monitoring & Heartbeat Architecture
* **Interval:** The Core transmits a `health.ping` request every 5,000 ms over the Control Channel.
* **Response:** The worker must respond with `health.pong` containing current process metrics (CPU usage, working set memory, active operation count) within 2,500 ms.
* **Degraded State:** If two consecutive pings fail, the worker status is transitioned to `Degraded`.
* **Unresponsive State & Eviction:** If three consecutive pings fail (15,000 ms total silence), the worker is classified as `Unresponsive`. Active operations are failed with `TIMEOUT_HEARTBEAT_EXPIRED`, and the supervisor initiates forceful termination via the Job Object.

### 5.4 Graceful Shutdown vs. Forceful Termination
1. **Graceful Orderly Shutdown:**
   * Core dispatches `session.terminate.request` with a graceful termination grace period (default: 5,000 ms).
   * The worker cancels active operations, flushes telemetry buffers, closes the named pipe handle, and exits with code `0`.
   * Core monitors process termination via process handle wait (`WaitForSingleObject`).
2. **Forceful Eviction:**
   * If the worker does not exit within the 5,000 ms grace period, or if it experiences an unhandled memory fault, the Core invokes `TerminateJobObject` or `Process.Kill(entireProcessTree: true)`.
   * Staging locks and pipe handles are reclaimed by the operating system kernel.

---

## 6. Security Boundaries & Threat Mitigation

The IPC architecture is explicitly designed around zero-trust boundaries (`REQ-SEC-001`, `REQ-SAFE-001`):

1. **Zero Elevation Policy (`REQ-SEC-001`):** Both Core and worker processes execute under the standard user token (Medium Integrity Level). Neither Core nor workers will ever request, inherit, or require Administrator privilege (`UAC`). Any IPC request attempting to trigger elevated operations is rejected with `SECURITY_UNAUTHORIZED_CALLER`.
2. **Channel Access Control (`REQ-SEC-001`):** Stdio handles are kernel-isolated between parent and child. Windows Named Pipes are fortified with custom DACLs permitting access solely to the launching user account SID (`TokenUser`). Attempts by other local users to connect to `\\.\pipe\nexludic_*` are denied at the OS kernel level.
3. **Session Authentication Token:** Handshake payloads require validation of a 256-bit cryptographically random ephemeral session token generated by the Core. The named pipe will reject connection handshakes lacking this matching token.
4. **Anti-Cheat Isolation (`REQ-SAFE-001`):** No IPC message will ever transmit raw process memory injection commands, shellcode payloads, or kernel driver requests. Any adapter attempting to instruct the Core to bypass anti-cheat mechanisms is immediately terminated and blacklisted.

---

## 7. Architectural Decisions & Trade-Offs

| Decision Item | Chosen Approach | Rejected Alternative | Architectural Justification |
| :--- | :--- | :--- | :--- |
| **Primary RPC Transport** | Stdio (ndjson JSON-RPC) | Loopback TCP (127.0.0.1) | Eliminates port collisions, firewall popups, and multi-user TCP sniffing; native handle isolation. |
| **Telemetry Transport** | Windows Named Pipes | WebSockets / HTTP2 | Named Pipes provide lower latency, message-mode framing, native Windows security DACLs, and no socket bind overhead. |
| **Worker Boundary** | Out-of-Process Worker | In-Process Managed DLLs | Prevents unhandled native/managed crashes in game adapters from crashing the Core host; enforces clean isolation. |
| **Supervisor Enforcement** | Win32 Job Objects | Process-level polling | Job Objects provide kernel-guaranteed cleanup on parent crash (`KILL_ON_JOB_CLOSE`) and hardware resource limits. |
| **Serialization Strategy** | JSON (Control) + Protobuf (Data) | Pure JSON or Pure Protobuf | JSON provides human readability and effortless cross-language debugging for control RPC; Protobuf provides peak throughput for high-frequency telemetry. |

---

## 8. Summary of Traceability & Compliance

* **ADR-0002 Compliance:** Strict out-of-process isolation for all third-party adapter execution.
* **ADR-0003 Compliance:** Implements the Dual-Channel IPC architecture (stdio Control + Named Pipe Data).
* **ADR-0009 Compliance:** Integrates Win32 Job Object sandbox supervision with hard resource ceilings and automatic cleanup.
* **REQ-SEC-001 Compliance:** Zero administrative privileges required; strictly scoped user SID DACLs.
* **REQ-SAFE-001 Compliance:** Driverless, non-intrusive communication model with no memory injection vectors.

