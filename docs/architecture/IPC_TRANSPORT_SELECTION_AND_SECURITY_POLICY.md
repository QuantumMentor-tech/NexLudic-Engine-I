# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## IPC Transport Selection & Security Policy

**Document ID:** `NLE-POL-TRN-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 13 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Security Policy & Transport Standard  
**Traceability:** `ADR-0002` (Out-of-Process Isolation), `ADR-0003` (Dual-Channel IPC), `ADR-0009` (Sandboxed Execution), `REQ-SEC-001` (Zero Elevation), `REQ-SAFE-001` (Anti-Cheat Invariant)

---

## 1. Executive Summary & Policy Scope

NexLudic Engine coordinates diverse external toolchains, script runtimes, and telemetry agents. Because adapter code is untrusted and varies across native C++, .NET, Python, and external modding CLI binaries, communication transports must enforce rock-solid security boundaries, eliminate privilege escalation vectors, and guarantee deterministic resource cleanup.

This document defines the **IPC Transport Selection Rationale** and the **IPC Security Policy** governing endpoint isolation, access control lists (ACLs), handle inheritance, Win32 Job Object sandbox boundaries, and anti-cheat compliance.

---

## 2. Comprehensive 18-Dimension Transport Comparison Matrix

To justify the architectural baseline, six IPC mechanisms were evaluated across 18 rigorous engineering dimensions:

| Dimension # | Evaluation Criterion | 1. In-Process (Direct/DLL) | 2. Stdio (Standard I/O) | 3. Windows Named Pipes | 4. Loopback TCP (127.0.0.1) | 5. Unix Domain Sockets (AF_UNIX) | 6. Shared Memory (MMF) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | **Process Boundary Isolation** | None (Fatal crash kills Core) | Complete OS process boundary | Complete OS process boundary | Complete OS process boundary | Complete OS process boundary | Complete OS process boundary |
| **2** | **Latency Profile** | Sub-nanosecond (<1 ns) | Low (<0.8 ms) | Very Low (<0.15 ms) | Moderate (~0.8 - 2.5 ms) | Low (<0.3 ms) | Microsecond (<0.02 ms) |
| **3** | **Throughput Ceiling** | In-memory (>10 GB/s) | Medium (~30-60 MB/s) | High (>600 MB/s) | High (~300-500 MB/s) | High (~400 MB/s) | Maximum (>3 GB/s) |
| **4** | **Memory Overhead** | Zero serialization copy | Low (Stream buffers) | Low (Pinned pipe buffers) | Moderate (TCP socket buffers)| Low (Kernel buffers) | High (Pre-allocated block) |
| **5** | **OS Handles & File Descriptors** | Zero | 2 handles (stdin/stdout) | 1 kernel pipe handle | 1 socket handle + ephemeral | 1 socket handle | 1 file mapping handle |
| **6** | **Message Framing Support** | Native objects | Stream (requires ndjson/len) | Built-in (`PIPE_TYPE_MESSAGE`) | Stream (manual framing) | Stream (manual framing) | Manual ring buffer framing |
| **7** | **Flow Control & Backpressure**| Async await / Channels | OS pipe buffer backpressure | OS pipe buffer backpressure | TCP sliding window | Kernel socket buffer | Manual mutex/event signaling |
| **8** | **Bidirectional Full-Duplex** | Yes | Yes (Separate in/out streams)| Yes (Full duplex pipe) | Yes (Full duplex socket) | Yes (Full duplex socket) | Manual bidirectional rings |
| **9** | **Security DACL / User SID** | In-process trust | Kernel handle isolation | Native Win32 DACL (User SID) | None (Any local user connects)| File permissions (limited) | Native Win32 DACL (User SID) |
| **10** | **Localhost Network Stack Dep** | None | None | None | Heavy (Winsock TCP/IP stack) | Moderate (Winsock AF_UNIX) | None |
| **11** | **Windows Firewall Prompt Risk** | None | None | None | High (Triggered on bind/listen)| Low | None |
| **12** | **Port Collision Probability** | N/A | None (No ports) | None (Cryptographic pipe name) | Frequent port collisions | Low (File system path) | None |
| **13** | **Cross-Language Portability** | Poor (PInvoke/C ABI only) | Universal (All languages) | High (Native Win32, Node, Py) | Universal (Standard sockets) | Moderate (Win10+ only) | Complex (Layout synchronization) |
| **14** | **Debugging Ergonomics** | Attached debugger | Plain text line inspection | Wireshark NamedPipe capture | Wireshark loopback pcap | Hex dump / strace | Memory dump inspection |
| **15** | **Process Crash Teardown** | Crash is fatal | Instant EOF on pipe close | Instant `ERROR_BROKEN_PIPE` | TCP FIN/RST or timeout wait | Socket reset | Handle closure (no signal) |
| **16** | **Multi-User Isolation** | N/A | Total isolation | Enforced via User SID DACL | Vulnerable across accounts | Filesystem ACL | Enforced via User SID DACL |
| **17** | **Kernel Syscall Frequency** | Zero | Low per-line read | Very Low (Message mode coalesc) | Moderate (TCP protocol stack)| Low | Minimal (Direct memory) |
| **18** | **Job Object Sandbox Compliant** | N/A | 100% Compatible | 100% Compatible | Blockable by network filters | Compatible | 100% Compatible |

---

## 3. The Dual-Channel Rationale & Transport Roles

Based on the 18-dimension evaluation matrix, NexLudic Engine enforces a strict **Dual-Channel Transport Policy**:

1. **Standard I/O (`stdio`) as Primary Control Transport:**
   * Used for lifecycle supervision, handshake, capability exchange, synchronous RPC commands, and progress events.
   * *Rationale:* Standard I/O requires zero network configuration, has zero port collision risks, triggers zero firewall alerts, and provides 100% kernel handle isolation between Core and child workers.
2. **Windows Named Pipes as Primary Telemetry Data Transport:**
   * Used for high-frequency game telemetry, coordinate streams (60 Hz), input state forwarding, and large binary chunk transfers.
   * *Rationale:* Named Pipes in Message Mode provide sub-millisecond latency, zero manual TCP packet reassembly, native Win32 DACL user-level access security, and throughput exceeding 600 MB/s.
3. **Loopback TCP (127.0.0.1) Restricted to Fallback / Remote Diagnostics:**
   * Loopback TCP is strictly prohibited for standard desktop adapter communication. It is reserved solely for headless remote debugging tools or containerized mock environments.

---

## 4. Endpoint Access Control & Windows Security Policy

To prevent privilege escalation, cross-user tampering, and unauthorized IPC sniffing, all Named Pipe endpoints must enforce cryptographically random naming and restrictive Discretionary Access Control Lists (DACLs):

### 4.1 Endpoint Naming Standard
All Named Pipe endpoints created by NexLudic Engine must conform to the following pattern:
```
\\.\pipe\nexludic_{CHANNEL_TYPE}_{SESSION_ID}_{WORKER_ID}
```
* `CHANNEL_TYPE`: Must be `telemetry`, `input`, or `diagnostic`.
* `SESSION_ID`: A 128-bit cryptographically secure UUIDv4 generated at Core startup.
* `WORKER_ID`: A unique alphanumeric identifier assigned to the worker instance.

### 4.2 Security Descriptor Definition Language (SDDL) Specification
All Named Pipes instantiated by the Core platform MUST configure a Win32 Security Descriptor using the following strict SDDL string:
```
D:(A;;GA;;;PS)(A;;GA;;;<CURRENT_USER_SID>)(D;;GA;;;NU)(D;;GA;;;AN)
```
* `(A;;GA;;;PS)`: **Allow Generic All to Principal Self.**
* `(A;;GA;;;<CURRENT_USER_SID>)`: **Allow Generic All exclusively to the launching Windows User SID** (`TokenUser`).
* `(D;;GA;;;NU)`: **Explicitly DENY all access to Network Users** (`NETWORK_LOGON_USER_SID`).
* `(D;;GA;;;AN)`: **Explicitly DENY all access to Anonymous Logon** (`ANONYMOUS_LOGON_LUID`).

Any incoming connection from a process executing under another user account or over a network share is rejected by the Windows kernel before the pipe server accepts the connection.

---

## 5. Handle Inheritance & Process Creation Isolation

When spawning external adapter workers, the Core host strictly governs OS handle inheritance to eliminate handle leakage:

1. **Explicit Inheritance Prohibition:** The process creation call sets `bInheritHandles = FALSE`.
2. **Selective Handle List (`STARTUPINFOEX`):** Only the specific redirected `hStdInput`, `hStdOutput`, and `hStdError` handles are shared with the child process using `UpdateProcThreadAttribute` with `PROC_THREAD_ATTRIBUTE_HANDLE_LIST`.
3. **Zero Administrative Elevation (`REQ-SEC-001`):** Workers MUST be spawned with the standard user security token (Medium Integrity Level). If an adapter executable contains an embedded UAC manifest requesting `requireAdministrator`, the Core supervisor will detect this condition, refuse to launch the executable, and report a `SECURITY_UNAUTHORIZED_CALLER` policy violation.

---

## 6. Win32 Job Object Sandbox Constraints (`ADR-0009`)

Every external worker process is immediately assigned to an active Win32 Job Object upon launch. The Job Object enforces the following hardware and environment boundaries:

```
+-----------------------------------------------------------------------------------+
|                        WIN32 JOB OBJECT SANDBOX BOUNDARY                          |
|                                                                                   |
|  [ Job Object Configuration: NexLudic_WorkerJob_{WORKER_ID} ]                     |
|                                                                                   |
|  1. Lifecycle Coupling:                                                           |
|     - JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = TRUE                                   |
|       (If Core terminates for any reason, Windows kernel auto-kills all workers)   |
|                                                                                   |
|  2. Crash Dialog Suppression:                                                     |
|     - JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = TRUE                          |
|       (Prevents Windows Error Reporting dialogs from hanging headless workers)    |
|                                                                                   |
|  3. Memory Ceilings:                                                              |
|     - JobMemoryLimit = 2,147,483,648 bytes (2.0 GB Commit Limit)                   |
|     - Exceeding commit limit triggers instantaneous out-of-memory exception in    |
|       worker without starving the Core host or operating system.                  |
|                                                                                   |
|  4. Desktop & UI Isolation:                                                       |
|     - JOB_OBJECT_UILIMIT_HANDLES = TRUE                                           |
|       (Prevents worker from accessing or sending Win32 window messages to Core UI)|
|     - JOB_OBJECT_UILIMIT_READCLIPBOARD = TRUE                                     |
|     - JOB_OBJECT_UILIMIT_WRITECLIPBOARD = TRUE                                    |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

---

## 7. Anti-Cheat Hard Safety Invariant (`REQ-SAFE-001`)

To guarantee absolute compliance with gaming safety policies, commercial terms of service, and anti-cheat invariants:

1. **Prohibition of Native Injection:** The IPC layer will NEVER transmit memory injection instructions, DLL injection paths, remote thread creation directives (`CreateRemoteThread`), or raw kernel driver communications.
2. **Prohibition of Memory Scanning:** No adapter will read or scan the memory space of game processes protected by third-party anti-cheat engines (Easy Anti-Cheat, BattlEye, Vanguard, Ricochet).
3. **Approved Mod Staging Only:** Adapters interact with games solely via authorized file-system modding APIs (e.g. Creation Kit scripts, Unity BepInEx mods in offline staging, Godot project exports) or non-intrusive standard input simulation (`SendInput`) when games are run in offline single-player developer mode.
4. **Immediate Eviction:** Any worker process attempting to execute unapproved memory tampering is terminated immediately, and its manifest is permanently revoked.

