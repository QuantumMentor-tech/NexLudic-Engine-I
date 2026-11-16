# ADR-0002: Universal Core Language and Runtime Selection
## Document ID: NLE-ADR-0002
**Status:** Accepted  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Software Architect & Project Owner  

---

### 1. Context and Problem
The universal core application requires a high-performance, strongly typed programming language and runtime capable of asynchronous workflow coordination, process management, structured JSON processing, and native Windows desktop interoperability.

### 2. Requirements Addressed
- `REQ-CORE-002`: Pluggable Engine Architecture.
- `REQ-CORE-004`: Project Lifecycle Management.
- `REQ-SAFE-003`: Non-administrative desktop execution.

### 3. Constraints and Assumptions
- The development workstation has `.NET SDK 8.0.425` pre-installed and verified.
- The host does not contain Rust, Go, or full Visual Studio C++ compilers.
- Long-term support (LTS) stability is paramount.

### 4. Options Considered
- **Option 1: C# 12 / .NET 8.0 LTS (with .NET 10 LTS forward path).**
- **Option 2: Node.js / TypeScript.** Requires bundling V8/Chromium; high memory overhead.
- **Option 3: Python 3.14.** Dynamic typing; GIL limits concurrency; packaging friction.
- **Option 4: Rust.** Toolchain absent on host; steep learning curve for rapid application authoring.

### 5. Evaluation Criteria
- Immediate host availability.
- Windows API integration and IPC performance.
- Memory safety and execution speed.
- Microsoft LTS support lifecycle.

### 6. Evidence and Source Links
- Step 3 Environment Audit (`DEVELOPMENT_ENVIRONMENT_AUDIT.md`) verifying .NET 8 SDK.
- Microsoft .NET Support Policy confirming .NET 8 LTS support through November 2026 and .NET 10 LTS through November 2028.

### 7. Decision
NexLudic Engine adopts **C# 12 / .NET 8.0 (LTS)** as the core programming language and runtime for the universal desktop engine. .NET 10 LTS is designated as the seamless forward upgrade path.

### 8. Rationale
C# and .NET 8 provide world-class asynchronous task primitives (`Task`, `Channel<T>`), high-throughput JSON processing (`System.Text.Json`), native Win32 interop via source-generated P/Invoke, and zero additional runtime installation on the host.

### 9. Advantages
- Zero setup friction; ready for development immediately on the host.
- Single-file publish capabilities for simple distribution.
- Memory safe, garbage collected, and enterprise supported.

### 10. Disadvantages and Risks
- Adapters in non-.NET languages (Java, Lua, C++) cannot be linked in-process and must run out-of-process (which aligns with our safety isolation model).

### 11. Consequences for Other Components
All core services, scanners, blueprint compilers, and IPC bridges will be authored in modern C#.

### 12. Dependencies
- .NET 8.0 SDK (Development); .NET 8.0 Desktop Runtime (End-user).

### 13. Conditions That Would Invalidate the Decision
End-of-life of .NET 8 requiring immediate migration to .NET 10 LTS.

### 14. Follow-Up Work
Initialize the base .NET 8 solution structure in Phase 2.

### 15. Owner Approval Required
Approved via architectural consensus (`DEC-008`).

