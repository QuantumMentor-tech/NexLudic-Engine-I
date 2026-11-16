# ADR-0006: Inter-Process Communication (IPC) and Shared Data Contracts
## Document ID: NLE-ADR-0006
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Systems Engineer & Interoperability Architect  

---

### 1. Context and Problem
NexLudic Engine coordinates communication between the desktop core, isolated adapter workers, and active live game bridges. The communication mechanism must be low-latency (<5 ms), cross-language compatible, highly secure on Windows, and immune to port-binding conflicts or local network firewall prompts.

### 2. Requirements Addressed
- `REQ-BRIDGE-001`: Local High-Speed IPC.
- `REQ-BRIDGE-004`: Heartbeat & Process Health Watchdog.
- `REQ-SAFE-002`: Local-Only Telemetry & Privacy.

### 3. Constraints and Assumptions
- Network ports (TCP/UDP) can trigger Windows Defender Firewall elevation dialogs, creating user friction.
- Data contracts must be human-readable, schema-validatable, and portable across C#, C++, Java, and Python.

### 4. Options Considered
- **Option 1: Windows Named Pipes (`\\.\pipe\nexludic_*`) with Length-Prefixed JSON Framing.**
- **Option 2: Localhost Loopback WebSockets / HTTP REST.** Requires port binding; triggers firewall alerts.
- **Option 3: Shared Memory (Memory-Mapped Files).** Microsecond fast but complex synchronization; high risk of deadlocks.
- **Option 4: gRPC over Named Pipes / HTTP/2.** Heavy protocol buffer build tooling; high complexity.

### 5. Evaluation Criteria
- Message latency and throughput on Windows desktop.
- Security isolation (local kernel-level ACLs).
- Freedom from firewall alerts and port collision risks.
- Developer simplicity and multi-language support.

### 6. Evidence and Source Links
- Step 3 Audit (`PLATFORM_AND_TOOLCHAIN_COMPATIBILITY_MATRIX.md`).
- Step 5 Technology Evaluation (`TECHNOLOGY_EVALUATION.md`).
- Microsoft Win32 Named Pipes API documentation.

### 7. Decision
NexLudic Engine adopts **Windows Named Pipes (`System.IO.Pipes`) with 4-byte length-prefixed JSON framing and JSON Schema Draft 2020-12 data contracts** for all local inter-process communication.

### 8. Rationale
Named pipes are native to the Windows kernel. They operate in memory without traversing the TCP network stack, produce zero firewall warnings, require no open network ports, enforce native Windows Access Control Lists (restricting communication to the current user token), and achieve <1 ms roundtrip latency under load.

### 9. Advantages
- Sub-millisecond latency (<1 ms), easily satisfying the sub-5 ms target.
- Zero network exposure; completely private and impervious to external network scanning.
- Portable; every major language (C#, Java, C++, Python) supports Windows named pipe I/O natively.

### 10. Disadvantages and Risks
- Length-prefixed framing must be handled explicitly in stream decoders.

### 11. Consequences for Other Components
The `NexLudic.Infrastructure` layer will implement a reusable `NamedPipeServer` and `NamedPipeClient` library with heartbeat watchdog support.

### 12. Dependencies
- Windows Win32 Named Pipe subsystem; `System.Text.Json`.

### 13. Conditions That Would Invalidate the Decision
Expansion to non-Windows operating systems where Named Pipes are unavailable (Unix domain sockets would be the equivalent fallback).

### 14. Follow-Up Work
Author the formal IPC packet framing and heartbeat specification in Phase 2.

### 15. Owner Approval Required
Pending Project Owner formal approval.

