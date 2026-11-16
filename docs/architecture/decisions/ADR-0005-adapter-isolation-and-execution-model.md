# ADR-0005: Adapter Isolation and Out-of-Process Execution Model
## Document ID: NLE-ADR-0005
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Game-Technology Research Architect  

---

### 1. Context and Problem
Adapters for different games rely on disparate runtime technologies (Java/Fabric, C++/BakkesMod, Lua/CET). Loading third-party native DLLs or managed runtimes directly into the core NexLudic Engine process risks memory access violations, DLL conflicts, and unhandled native crashes that would instantly bring down the entire desktop application.

### 2. Requirements Addressed
- `REQ-CORE-002`: Decoupled Pluggable Core.
- `REQ-ADAPT-001`: Standardized Adapter Contract.
- `REQ-ADAPT-003`: Isolated Adapter Execution.
- `DEC-004`: Strategy A Prioritization.

### 3. Constraints and Assumptions
- Game mod code and live bridges must be treated as potentially unstable third-party code.
- Different games require different compilers (Gradle, MSBuild, CMake) and scripting interpreters.

### 4. Options Considered
- **Option 1: In-Process Assembly Loading (`AssemblyLoadContext`).** Unsafe; crashes the desktop app on any unhandled native exception; restricts adapters to .NET.
- **Option 2: Out-of-Process Worker Model via Compositional Capability Traits.** Isolates live hooks in independent worker processes; invokes compilers via controlled subprocesses.
- **Option 3: Pure Containerization (Docker/WSL2).** Excessive resource consumption; poor Windows GPU and game process visibility.

### 5. Evaluation Criteria
- Fault isolation and host stability.
- Polyglot language support (Java, C++, Lua, Python).
- Process lifecycle control and emergency termination.

### 6. Evidence and Source Links
- Step 4 Research (`UNIVERSAL_ADAPTER_CAPABILITY_BOUNDARIES.md` & `GAME_MODDING_ECOSYSTEM_TAXONOMY.md`).
- Step 5 Technology Evaluation (`TECHNOLOGY_EVALUATION.md`).

### 7. Decision
NexLudic Engine adopts an **Out-of-Process Worker & Compositional Trait Model**:
1. **Mod Project Scaffolding (Strategy A):** Adapters supply project templates (`IModScaffoldable`). Compilers (Gradle, CMake) execute as controlled child processes with stdout/stderr capture.
2. **Live Bridge Telemetry (Strategy B / C):** Adapters that attach to running game processes (`ITelemetrySource`, `IInputTarget`) execute in dedicated, isolated worker processes communicating with the core engine via Windows Named Pipes.
3. **Fault Containment:** If an adapter worker crashes, hangs, or experiences an access violation, the core engine detects process termination, flags the error in the UI, and remains fully operational without crashing.

### 8. Rationale
Eliminates all native crash propagation into the core application while providing true language independence. A Minecraft adapter can run in Java, a Rocket League adapter in C++, and a Cyberpunk adapter in Lua without conflicting with the .NET 8 core.

### 9. Advantages
- 100% crash resilience for the host desktop application.
- Supports any programming language that can write to a named pipe or execute from the CLI.
- Clean process-level lifecycle and memory reclamation upon session exit.

### 10. Disadvantages and Risks
- Slight inter-process communication serialization overhead (measured at <1 ms over named pipes).

### 11. Consequences for Other Components
The infrastructure layer will implement an `AdapterProcessManager` to supervise worker lifecycles and heartbeat monitoring.

### 12. Dependencies
- Windows Process Subsystem, `System.IO.Pipes`.

### 13. Conditions That Would Invalidate the Decision
Evidence demonstrating that inter-process communication latency is prohibitive for required gameplay events.

### 14. Follow-Up Work
Define the detailed JSON-RPC IPC message contracts in Phase 2.

### 15. Owner Approval Required
Pending Project Owner formal approval.

