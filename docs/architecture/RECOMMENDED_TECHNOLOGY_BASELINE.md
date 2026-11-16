# NEXLUDIC ENGINE — RECOMMENDED TECHNOLOGY BASELINE
## Document ID: NLE-ARCH-BASE-001
**Phase:** 1 — Product Definition & Research  
**Step:** 5 of 100 — Universal Platform Technology Evaluation & Architecture Decision Records  
**Date:** November 16, 2026  
**Auditor / Architect:** Principal Software Architect & Systems Engineering Lead  

---

## 1. Architectural Stack Blueprint

The recommended technology baseline for **NexLudic Engine** adheres to the principle of **lowest sufficient complexity**: a high-performance, modular desktop monolith operating natively on Windows 10/11 x64, with strict out-of-process boundaries separating the universal core from failure-prone game hooks, external compilers, and AI coding agents.

```
+----------------------------------------------------------------------------------------------------+
|                                NEXLUDIC ENGINE RECOMMENDED STACK                                   |
+--------------------------+---------------------------------+---------------------------------------+
| Architectural Layer      | Selected Technology             | Governance / Support Horizon          |
+--------------------------+---------------------------------+---------------------------------------+
| Core Language & Runtime  | C# 12 / .NET 8.0 SDK (LTS)      | Microsoft LTS (Supported to late 2026)|
| Desktop UI Presentation  | Windows Presentation Foundation | Native `Microsoft.WindowsDesktop.App`  |
|                          | (WPF) + CommunityToolkit.Mvvm   | D3D Accelerated, XAML, MVVM           |
| Core Architecture Style  | Clean Modular Monolith          | Domain-Driven Design (DDD) boundaries |
| Adapter Execution Model  | Out-of-Process Worker Model     | Compositional Capability Trait Model  |
| Inter-Process Comm (IPC) | Windows Named Pipes             | `System.IO.Pipes` (<1 ms latency, OS) |
| Data Storage & Contracts | Versioned JSON + JSON Schema    | `System.Text.Json` (Draft 2020-12)    |
| AI Agent Orchestration   | MCP Server + Subprocess Runner  | Model Context Protocol + Job Objects  |
| Workspace Isolation      | Staging Directory Sandbox       | `<project>/staging/` + Diff Tracker   |
| Core Testing Framework   | xUnit + FluentAssertions + Moq  | Headless Mock Adapter Test Suite      |
| Logging & Observability  | Serilog                         | Structured file & UI streaming sinks  |
| Packaging & Distribution | .NET Single-File Publish / MSIX | Self-contained, non-administrative   |
| Backup & Rollback        | Atomic Directory Snapshot Engine| Bit-for-bit SHA-256 hash restoration  |
+--------------------------+---------------------------------+---------------------------------------+
```

---

## 2. Subsystem Architectural Specifications

### 2.1 Universal Desktop Core: C# 12 / .NET 8.0 LTS
- **Architecture Role:** Hosts the central workflow orchestration engine, game scanner, blueprint compiler, AI agent supervisor, and safety watchdogs.
- **Justification:** .NET 8 is pre-installed and verified on the development host (`dotnet 8.0.425`). It provides zero-overhead Win32 interoperability, native asynchronous task scheduling (`System.Threading.Channels`), and long-term enterprise stability. Upgrades to .NET 10 LTS (supported to late 2028) can be performed seamlessly in future phases without rewriting code.

### 2.2 Desktop UI: WPF (Windows Presentation Foundation) with MVVM
- **Architecture Role:** Desktop presentation layer rendering library scanners, intake wizards, diff viewers, and live logs.
- **Justification:** Bundled directly in `Microsoft.WindowsDesktop.App`. Consumes under 70 MB of RAM (compared to 300MB+ for Electron), launches instantly, and handles high-frequency virtualized log rendering with zero UI thread freezing via MVVM data binding (`CommunityToolkit.Mvvm`).

### 2.3 Adapter Model: Out-of-Process Workers & Compositional Traits
- **Architecture Role:** Decouples game-specific mod loaders and process hooks from the core desktop process.
- **Justification:** Running adapters out-of-process ensures that an adapter crash or memory corruption in a third-party game mod never crashes the NexLudic Engine host application. Games declare capabilities through modular traits (`IModScaffoldable`, `ITelemetrySource`, `IInputTarget`, `IAssetExtractable`).

### 2.4 Inter-Process Communication: Windows Named Pipes
- **Architecture Role:** Bidirectional message bus between the desktop engine, adapter workers, and game hooks.
- **Justification:** Achieves sub-millisecond roundtrip latency (<1 ms) through Windows kernel memory, requires zero firewall permissions, opens no vulnerable network ports, and enforces native Win32 user access control lists (ACLs).

### 2.5 Data Storage: Versioned JSON Manifests & Structured Logging
- **Architecture Role:** Manages project state, blueprints, adapter configurations, and audit logs.
- **Justification:** Mashup projects are self-contained directory trees. Storing project configurations in human-readable JSON files (`manifest.json`, `mashup-blueprint.json`) guarantees portability, version control diffability, and immunity to binary database corruption. SQLite is deferred to Phase 3 when large historical telemetry indexing is required.

### 2.6 AI Agent Orchestration: Model Context Protocol (MCP) + Job Objects
- **Architecture Role:** Supervises external coding agents (Claude Code, Gemini CLI, local SLMs) to generate and modify files safely.
- **Justification:** Uses MCP for standardized tool calling and exposes safe workspace tools. Executes agent CLI processes inside a restricted Windows Job Object that enforces CPU/memory caps, terminates runaway processes, and strictly confines write operations to `<project>/staging/`.

### 2.7 Workspace Isolation & Rollback Engine
- **Architecture Role:** Guarantees non-destructive mod development and instant system recovery.
- **Justification:** All AI-generated files and compilation commands execute inside `<project>/staging/`. Nothing is written to actual game directories without user-reviewed diff approvals. Before deployment, a full timestamped backup is captured in `<project>/backups/`, enabling one-click rollback with 100% hash parity (`REQ-PKG-002`).

### 2.8 Quality Assurance & Headless Testing
- **Architecture Role:** Automated continuous testing of platform logic and mock game integrations.
- **Justification:** Core platform logic is tested via xUnit, FluentAssertions, and Moq. Comprehensive **Mock Game Adapters** simulate game process lifecycles and event streams, allowing 100% automated CI test execution on machines without game installations (`REQ-TEST-001`).

---

## 3. Technology Governance Status Matrix

| Subsystem Component | Selected Technology | Governance Status | Prerequisite / Action Required |
| :--- | :--- | :--- | :--- |
| **Core Runtime** | C# 12 / .NET 8.0 LTS | **Approved Baseline** | Active on host (`DEC-008`). |
| **Desktop UI** | WPF + CommunityToolkit.Mvvm | **Approved Baseline** | Bundled in .NET 8 Desktop (`DEC-008`). |
| **Adapter Model** | Compositional Out-of-Process | **Proposed (ADR-0005)**| Formal owner sign-off in Step 5. |
| **IPC Transport** | Windows Named Pipes | **Proposed (ADR-0006)**| Formal owner sign-off in Step 5. |
| **Storage Architecture**| Tiered JSON Manifests | **Proposed (ADR-0007)**| Formal owner sign-off in Step 5. |
| **AI Orchestrator** | MCP + Subprocess Runner | **Proposed (ADR-0008)**| Formal owner sign-off in Step 5. |
| **Workspace Sandbox** | Staging Directory + Job Objects| **Proposed (ADR-0009)**| Formal owner sign-off in Step 5. |
| **Testing Harness** | xUnit + Mock Adapters | **Proposed (ADR-0010)**| Formal owner sign-off in Step 5. |
| **Packaging & Rollback**| Single-File + Atomic Backup | **Proposed (ADR-0011)**| Formal owner sign-off in Step 5. |

