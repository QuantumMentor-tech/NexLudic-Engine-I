# NEXLUDIC ENGINE — TECHNOLOGY EVALUATION & TRADE-OFF ANALYSIS
## Document ID: NLE-ARCH-EVAL-001
**Phase:** 1 — Product Definition & Research  
**Step:** 5 of 100 — Universal Platform Technology Evaluation & Architecture Decision Records  
**Date:** November 16, 2026  
**Auditor / Architect:** Principal Software Architect & Systems Engineering Lead  

---

## 1. Evaluation Methodology & Architectural Criteria

To ensure NexLudic Engine achieves its objective as a universal, extensible game-mashup platform, candidate technologies were evaluated across 12 architectural criteria established in the project mandate:
1. **Maintainability:** Clean separation of concerns, strong typing, mature developer tooling.
2. **Extensibility:** Ability to introduce new game adapters without modifying or recompiling core platform assemblies.
3. **Strong Module Boundaries:** Enforceable architectural encapsulation between presentation, workflow orchestration, adapters, and agent workers.
4. **Reliability & Recoverability:** Robust exception handling, crash isolation, and atomic rollback capabilities.
5. **Security & Controlled Execution:** Operating under least-privilege Windows user permissions; sandboxing untrusted file mutations and command execution.
6. **Platform Compatibility:** Deep, native integration with Windows 10/11 x64 systems (window handles, named pipes, process jobs).
7. **Development & Testing Workflows:** Fast inner-loop compilation, headless unit testing, mock adapter support.
8. **Resource Footprint:** Minimal idle CPU/RAM overhead, ensuring the platform does not contend with heavy commercial game runtimes.
9. **Long-Term Support (LTS) & Supply Chain:** Backed by stable, maintained ecosystems with predictable support horizons.
10. **Cost & Licensing:** Permissive open-source licenses (MIT, Apache 2.0); zero proprietary runtime fees or telemetry lock-in.
11. **AI Agent Ergonomics:** Compatibility with standard coding-agent tooling (CLI execution, Model Context Protocol [MCP], unified diff parsing).
12. **Simplicity:** Lowest sufficient complexity; avoiding premature microservice or distributed-system overhead.

---

## 2. Core Language & Runtime Evaluation

The universal core engine requires high-performance process management, Win32 API interoperability, native IPC capabilities, and robust asynchronous task orchestration.

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                        CORE LANGUAGE & RUNTIME TRADE-OFF MATRIX                                         |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Candidate Runtime  | Key Strengths               | Critical Weaknesses         | Security & Risk  | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **C# / .NET 8.0**  | - Native Windows IPC/Win32  | - Windows-first focus       | - Strong memory  | **RECOMMENDED**     |
| **(LTS Baseline)** | - First-class async/await   | - Cross-platform UI requires|   safety         | - Verified active   |
|                    | - Bundled WPF desktop engine|   Avalonia if expanded      | - Zero runtime   |   on dev host       |
|                    | - Verified installed on host|                             |   elevation req. | - LTS to late 2026  |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **C# / .NET 10.0** | - Active LTS (through 2028) | - Not yet globally default  | - Strong memory  | **MIGRATION TARGET**|
| **(Future LTS)**   | - Enhanced JIT throughput   |   on older Windows 10 hosts |   safety         | - Upgrade path from |
|                    | - C# 13 language features   |                             |                  |   .NET 8 in Phase 2 |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Node.js / TS**   | - Massive npm ecosystem     | - Heavy V8 memory footprint | - High supply-   | **REJECTED (Core)** |
|                    | - Dynamic JSON parsing      | - Single-threaded event loop|   chain risk     | - Retained for web  |
|                    | - Native MCP SDK support    |   contends under heavy IPC  | - Script policy  |   tooling only      |
|                    |                             | - Fragile Win32 API binding |   execution lag  |                     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Python 3.14**    | - Rapid scripting/prototyping| - Global Interpreter Lock   | - Slow IPC loops | **REJECTED (Core)** |
|                    | - Rich CV/data libraries    | - Difficult binary packaging| - Fragile virtual| - Retained for CV / |
|                    |                             | - Lack of native desktop UI |   env management |   offline analysis  |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Rust**           | - Zero-cost abstractions    | - Steep developer curve     | - Maximum memory | **DEFERRED**        |
|                    | - Deterministic memory      | - Slow compile times        |   safety         | - Overkill for core |
|                    | - Excellent low-level IPC   | - Immature GUI ecosystem    |                  |   orchestration     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Detailed Finding on .NET 8.0 vs. .NET 10.0 LTS
- **Empirical Baseline:** Step 3 verified that the development workstation currently runs `.NET SDK 8.0.425` with `Microsoft.WindowsDesktop.App 8.0.31`. Microsoft officially supports .NET 8.0 LTS through November 10, 2026.
- **Migration Path:** .NET 10 LTS was released in November 2025 and is supported through November 14, 2028. Because .NET 8 and .NET 10 share complete backward compatibility for C# console and WPF codebases, **.NET 8 is adopted as the immediate baseline**, with .NET 10 documented as the natural forward upgrade path.

---

## 3. Desktop UI Presentation Framework Evaluation

The UI must present game library discovery, intake questionnaires, blueprint summaries, live streaming logs from coding agents, side-by-side colorized diffs, and verification checklists.

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                         DESKTOP UI FRAMEWORK TRADE-OFF MATRIX                                           |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Framework Candidate| Key Strengths               | Critical Weaknesses         | Resource Use     | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **WPF (.NET 8)**   | - Native Windows framework  | - Windows-only presentation | - Very low idle  | **RECOMMENDED**     |
|                    | - Bundled in WindowsDesktop | - XAML learning curve       |   RAM (40–70 MB) | - Verified active   |
|                    | - Full MVVM & community tool|                             | - Instant launch | - Zero extra bloat  |
|                    | - Hardware accelerated D3D  |                             |   time (<200 ms) |                     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **WinUI 3 / AppSDK**| - Modern Fluent Design controls| - Separate AppSDK runtime | - Moderate RAM   | **REJECTED**        |
|                    | - Modern Windows 11 styling | - Deployment friction       |   (100–150 MB)   | - Premature tooling |
|                    |                             | - Fragile unpacking on Win10| - Packaging bugs |   instability       |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Avalonia UI**    | - Cross-platform (Win/Linux)| - Third-party community     | - Low RAM        | **CONTINGENCY**     |
|                    | - Familiar XAML syntax      |   framework dependency      |   (60–90 MB)     | - Adopt if Linux    |
|                    | - .NET 8 compatible         | - Extra abstraction layer   |                  |   scope is approved |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Tauri (v2)**     | - Modern web frontend (Vue) | - Requires Rust toolchain   | - Low RAM        | **REJECTED**        |
|                    | - Lightweight webview       | - Dev host lacks Rust       |   (50–80 MB)     | - Missing Rust      |
|                    | - Multi-process sandbox     | - FFI bridge overhead to C# |                  |   toolchain on host |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Electron**       | - Rapid web UI development  | - Heavy Chromium runtime    | - High RAM       | **REJECTED**        |
|                    | - Huge widget ecosystem     | - 150MB+ bundle overhead    |   (250–450 MB)   | - Excessive resource|
|                    |                             | - Contends with games for RAM|                  |   footprint for PC  |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Recommendation
**WPF (Windows Presentation Foundation) under .NET 8** is recommended. It runs natively on the inspected host without installing secondary SDKs, uses hardware-accelerated DirectX rendering, consumes under 70 MB of RAM, and handles high-frequency virtualized log rendering with zero UI lag.

---

## 4. Adapter Execution & Isolation Model Evaluation

How game-specific logic is hosted and executed is the central architectural question determining whether NexLudic Engine remains modular or becomes brittle.

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                      ADAPTER EXECUTION MODEL TRADE-OFF MATRIX                                           |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Architecture Model | Isolation & Fault Tolerance | Language Independence       | IPC Overhead     | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **1. In-Process**  | **ZERO ISOLATION.**         | **None.** Restricted to .NET| None (Direct     | **REJECTED**        |
| **Plugins (ALC)**  | An uncaught adapter crash or| assemblies. Cannot run Java,| in-memory calls) | - Destroys platform |
|                    | memory corruption kills the | C++, or Python directly.    |                  |   stability         |
|                    | entire NexLudic Engine app. |                             |                  |                     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **2. Out-of-Process**| **COMPLETE ISOLATION.**   | **High.** Workers can run in| Minimal (<1 ms   | **RECOMMENDED**     |
| **Workers (IPC)**  | Adapter crashes terminate   | C#, Java, Python, or native | over local named | - Fault-tolerant    |
|                    | only the worker process;    | C++ with standard IPC.      | pipes)           | - Safe crash recovery|
|                    | UI remains responsive.      |                             |                  |                     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **3. CLI Tool**    | **Complete Isolation.**     | **High.** Invoked via shell | Batch execution  | **ADOPTED FOR**     |
| **Adapters**       | Clean process execution     | args and exit codes.        | only; poor for   | **BUILD/SCAFFOLD**  |
|                    | boundary per command.       |                             | continuous event | - Ideal for Gradle/ |
|                    |                             |                             | streams.         |   compiler execution|
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **4. Generated**   | **Complete Independence.**  | **Universal.** AI generates | File-based       | **PRIMARY MOD**     |
| **Projects with**  | Project is built and tested | native project files in the | artifact handoff | **DELIVERY MODEL**  |
| **Manifests**      | in an isolated directory.   | game's native ecosystem.    |                  | - Follows community |
|                    |                             |                             |                  |   mod standards     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Recommendation
A **Hybrid Multi-Tier Isolation Architecture**:
1. **Host-Game Mods (Strategy A):** Follows **Model 4 (Generated Projects)**. The AI agent generates an idiomatic project (e.g. Fabric Gradle) in `<project>/staging/`. Build and test commands are executed via **Model 3 (Controlled CLI Workers)** with captured logs.
2. **Live Bridges (Strategy B / C):** Follows **Model 2 (Out-of-Process Workers)**. The NexLudic bridge daemon spawns lightweight, isolated worker processes communicating over local Windows Named Pipes.

---

## 5. Inter-Process Communication (IPC) Protocol Evaluation

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                           IPC TRANSPORT TRADE-OFF MATRIX                                                |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Transport Option   | Throughput & Latency        | Windows Security Model      | Cross-Language   | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Windows Named**  | **Sub-millisecond**         | - Win32 security descriptors| Excellent        | **RECOMMENDED**     |
| **Pipes (OS)**     | (<1 ms roundtrip). High     | - Localhost kernel boundary | (C#, C++, Java,  | - Native Windows OS |
|                    | memory throughput.          | - Zero open network ports   | Python support)  | - High security     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Loopback TCP /** | Low latency (1–3 ms).       | - Requires binding local port| Universal        | **CONTINGENCY**     |
| **WebSockets**     | Higher CPU network stack    | - Firewall alerts on bind   |                  | - For browser-based |
|                    | overhead.                   | - Port conflict risks       |                  |   or remote adapters|
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Shared Memory**  | **Microsecond** (<0.1 ms).  | - Complex synchronization   | C/C++ only;      | **DEFERRED (R&D)**  |
| **(MMF)**          | Zero-copy memory buffer.    | - No built-in event queue   | complex bindings | - Excessive         |
|                    |                             | - High crash-state risk     | in managed runtimes|   complexity for MVP|
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **gRPC over HTTP/2**| Moderate (2–5 ms).         | - Heavy protocol buffer and | Universal        | **REJECTED**        |
|                    | Binary serialization.       |   HTTP/2 framing overhead   |                  | - Premature bloat   |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Recommendation
**Windows Named Pipes (`System.IO.Pipes`)** with length-prefixed JSON framing. Named pipes run entirely within the Windows kernel, require zero firewall permissions, produce no open TCP ports, support native Access Control Lists (ACLs), and achieve sub-millisecond roundtrip latency.

---

## 6. Data Storage & Persistence Evaluation

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                           DATA STORAGE TRADE-OFF MATRIX                                                 |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Storage Mechanism  | Use Cases & Suitability     | Concurrency & Migration     | Portability      | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Versioned JSON** | - Project configurations    | - Single-writer safe        | - 100% human-    | **RECOMMENDED (MVP)**|
| **Manifests**      | - Mashup blueprints         | - Schema migration via      |   readable       | - Portable, simple  |
|                    | - Capability declarations   |   `System.Text.Json`        | - Zero external  | - Easy git diffing  |
|                    | - Adapter manifests         |                             |   dependencies   |                     |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **SQLite (Local)** | - Historical telemetry logs | - Multi-read single-write   | - Single binary  | **DEFERRED (PHASE 3)|
|                    | - Deep indexed game caches  | - Requires schema migration |   file (`.db`)   | - Unnecessary for   |
|                    | - Session replay analytics  |   tooling (EF Core / Dapper)|                  |   MVP flat metadata |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Structured Flat**| - Build compiler output     | - Append-only sequential    | - Text files     | **RECOMMENDED**     |
| **Logs (Serilog)** | - AI agent thought traces   | - Thread-safe file sink     |   (`.log`)       | - Essential for     |
|                    | - Verification results      |                             |                  |   diagnostics       |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Recommendation
**Tiered Flat-File JSON & Structured Logging for MVP**. Mashup projects represent self-contained directories. Storing project configurations in versioned JSON manifests (`manifest.json`, `mashup-blueprint.json`) provides instant human readability, easy backup/restore, zero database corruption risk, and zero external database dependencies. SQLite is deferred to Phase 3.

---

## 7. AI Coding-Agent Orchestration Protocol Evaluation

```
+-------------------------------------------------------------------------------------------------------------------------+
|                                      AI ORCHESTRATION PROTOCOL TRADE-OFF MATRIX                                         |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| Integration Route  | Provider Independence       | Security & Execution Control| Tool Calling     | Recommendation      |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Model Context**  | **UNIVERSAL STANDARD.**     | - Fine-grained tool access  | - Native JSON-RPC| **RECOMMENDED**     |
| **Protocol (MCP)** | Decouples engine from specific| - Human approval gating   |   tool schemas   | - Industry standard |
|                    | LLM vendors.                | - Controlled resource URIs  | - Context driven | - Vendor-neutral    |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Controlled CLI** | **High.** Wraps existing    | - Process isolation via     | - Text / diff    | **RECOMMENDED**     |
| **Process Wrapper**| command-line tools (Claude  |   Windows Job Objects       |   streaming      | - Direct support for|
|                    | Code, Gemini CLI, Aider).   | - Working directory locked  | - Stdout parsing |   local dev tools   |
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
| **Direct Vendor**  | **POOR.** Tightly binds the | - Requires storing user API | - Vendor-specific| **REJECTED**        |
| **Cloud REST APIs**| engine to proprietary APIs  |   keys in the app           |   tool bindings  | - Violates privacy  |
|                    | (Anthropic / OpenAI).       | - Recurring cloud costs     |                  |   and vendor-neutral|
+--------------------+-----------------------------+-----------------------------+------------------+---------------------+
```

### Recommendation
A **Dual-Layer Agent Abstraction**:
1. **Primary Interface:** **Model Context Protocol (MCP)** server architecture. NexLudic Engine acts as an MCP Host and Tool Provider, exposing safe project inspection, file authoring, and build validation tools to any compatible AI agent.
2. **Execution Runner:** **Controlled Subprocess Wrapper**. The engine spawns the user's preferred local coding agent CLI within an isolated Windows Job Object, capturing stdout/stderr and confining write operations to `<project>/staging/`.

