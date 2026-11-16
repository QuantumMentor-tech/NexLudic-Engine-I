# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Phase 2 Architecture Foundation Baseline

**Document ID:** `NLE-ARC-PH2-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 16 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Core Architectural Baseline Specification  
**Traceability:** Phase 1 Exit Review (`PHASE_1_EXIT_REVIEW.md`), `ADR-0001` through `ADR-0011`, Steps 9–15 Specifications

---

## 1. Executive Summary & Architecture Vision

The **NexLudic Engine** is an evidence-backed, Windows-first, AI-assisted platform designed to evaluate, plan, scaffold, validate, and execute integrations ("game mashups") between heterogeneous PC games.

Phase 2 ("Architecture & Development Foundation", Steps 9–16) establishes the complete structural blueprint for the platform. It defines the universal host architecture, component responsibilities, implementation-neutral domain models, capability lifecycles, out-of-process adapter isolation, dual-channel inter-process communication, tiered file-based persistence, and a 7-layer configuration and secrets hierarchy.

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC ENGINE SYSTEM ARCHITECTURE                             |
|                                                                                                    |
|  +----------------------------------------------------------------------------------------------+  |
|  | PRESENTATION LAYER (NexLudic.App) - WPF / XAML / .NET 8.0 LTS (ADR-0003)                     |  |
|  | - Desktop UI Shell, Workflow Wizards, Diff Review Dialogs, Live Telemetry Visualizer         |  |
|  +----------------------------------------------+-----------------------------------------------+  |
|                                                 | Commands / Reactive ViewModel Bindings           
|                                                 v                                                  
|  +----------------------------------------------------------------------------------------------+  |
|  | CORE ORCHESTRATION LAYER (NexLudic.Core) - C# 12 / .NET 8.0 LTS (ADR-0002, ADR-0004)         |  |
|  | - Project Coordinator         - Compatibility Assessment Engine   - Validation Workflow      |  |
|  | - AI Task Orchestrator (MCP)  - Adapter Process Supervisor        - Configuration Resolver   |  |
|  +----------------------------------------------+-----------------------------------------------+  |
|                                                 | Enforces Domain Rules & Invariants               
|                                                 v                                                  
|  +----------------------------------------------------------------------------------------------+  |
|  | UNIVERSAL DOMAIN LAYER (NexLudic.Domain) - Pure DDD Business Logic (NLE-DOM-SPEC-001)        |  |
|  | - Aggregates: GamePairProject, CapabilityRegistry, EvidenceCitation, RiskRecord, AIApproval   |  |
|  | - Contracts: IGameAdapterContract, IProjectRepository, IEvidenceRepository, IArtifactStore  |  |
|  +----------------------------------------------+-----------------------------------------------+  |
|                                                 | Implements Abstractions                          
|                                                 v                                                  
|  +----------------------------------------------------------------------------------------------+  |
|  | INFRASTRUCTURE & PERSISTENCE (NexLudic.Infrastructure) - (ADR-0007, ADR-0008, ADR-0009)     |  |
|  | - Atomic JSON Manifest Repo (File.Replace)    - Windows DPAPI Encrypted Vault (credentials.dat)|
|  | - Staging Sandbox Jail Manager                - Serilog Structured Logging Sinks (ndjson)      |  |
|  | - Win32 Job Object Process Runner             - Model Context Protocol (MCP) Server / Runner   |  |
|  +----------------------------------------------+-----------------------------------------------+  |
+-------------------------------------------------|--------------------------------------------------+
                                                  | Dual-Channel IPC (ADR-0003, NLE-ARC-IPC-001)     
                         +------------------------+------------------------+                         
                         | Channel 1: Control (stdio JSON-RPC)             | Channel 2: Data (Pipe)  
                         v                                                 v                         
+----------------------------------------------------------------------------------------------------+
|                     OUT-OF-PROCESS ADAPTER WORKERS (ADR-0002, ADR-0005, ADR-0009)                  |
|                     (Win32 Job Object Sandbox: Medium IL, 2 GB Commit Cap, Kill-On-Close)          |
|                                                                                                    |
|  +------------------------------------+   +------------------------------------+                   |
|  | Adapter A (e.g. Skyrim CreationKit)|   | Adapter B (e.g. Subnautica BepInEx)|                   |
|  | - Language: C# / Python / C++      |   | - Language: C# / .NET 8 Worker     |                   |
|  | - Traits: IModScaffoldableTrait    |   | - Traits: ITelemetrySourceTrait    |                   |
|  | - Staging: <proj>/staging/game_a/  |   | - Staging: <proj>/staging/game_b/  |                   |
|  +------------------------------------+   +------------------------------------+                   |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Foundational Architecture Decisions & Baseline Tenets

The Phase 2 baseline is anchored on eight immutable structural tenets:

1. **Clean Separation of Universal Core vs. Game Adapters (`ADR-0004`):** The universal core contains zero game-specific logic, proprietary header files, or modding engine hacks. Game-specific operations interface via standardized capability traits (`IModScaffoldableTrait`, `ITelemetrySourceTrait`, `IInputTargetTrait`).
2. **Out-of-Process Worker Model (`ADR-0002`, `ADR-0005`):** External adapters run in isolated OS child processes bounded by Win32 Job Objects (`ADR-0009`). A crash, memory leak, or invalid memory pointer in a modding tool cannot crash the primary engine.
3. **Dual-Channel IPC Architecture (`ADR-0003`, `NLE-ARC-IPC-001`):**
   * **Control Channel:** Standard I/O (`stdin`/`stdout`) via Newline-Delimited JSON-RPC 2.0 for commands, lifecycle, and progress events.
   * **Telemetry Data Channel:** Windows Named Pipes (`\\.\pipe\nexludic_*`) in Message Mode using Protocol Buffers (`proto3`) for 60 Hz real-time state streaming.
4. **Tiered File-Based Persistence on Volume `D:` (`ADR-0007`, `DEC-009`):** Projects reside as self-contained, human-readable, git-friendly folders on Volume `D:`. Manifests update atomically via `File.Replace`. Relational database engines (SQLite) are formally evaluated and deferred to Phase 3+.
5. **Mandatory Security Policy & Zero Elevation (`REQ-SEC-001`, `ADR-0009`):** All processes run under standard user permissions (Medium Integrity Level). Administrator elevation (`requireAdministrator`) is hard-blocked. All file mutations are confined to `<project>/staging/` with mandatory human diff review before deployment.
6. **Opaque Secrets Vault via Windows DPAPI (`NLE-ARC-SEC-001`):** AI provider API keys are encrypted via Windows DPAPI (`DataProtectionScope.CurrentUser`) and referenced in project manifests solely by non-secret UUID pointers (`credential_ref`).
7. **Evidence-First Epistemic Provenance (`NLE-DOM-SPEC-001`):** Capability support claims require immutable evidence citations (Tiers 1–4). Storing an adapter claim does NOT mark it validated; validation requires empirical test execution runs.
8. **Vendor-Neutral AI Provider Model (`ADR-0008`):** AI agent interactions are decoupled through Model Context Protocol (MCP) and the `IAgentRunner` abstraction, supporting Gemini, Claude, local SLMs (Ollama/ONNX), or 100% offline manual prompt copy-paste.

---

## 3. High-Level Subsystems & Responsibilities

| Subsystem Identifier | Namespace | Architectural Responsibility | Excluded Responsibilities |
| :--- | :--- | :--- | :--- |
| **Presentation Shell** | `NexLudic.App` | Desktop UI rendering, project wizards, diff review dialogs, visual health monitors. | No direct disk I/O; no direct process spawning; no business rule calculation. |
| **Core Orchestration** | `NexLudic.Core` | Coordinates project lifecycle, compatibility workflows, AI agent tasks, and adapter supervision. | No UI framework dependencies; no direct native Win32 P/Invoke calls. |
| **Domain Logic** | `NexLudic.Domain` | Pure domain entities, capability state machines, evidence models, business invariants. | Zero external dependencies; zero file-system or database references. |
| **Infrastructure** | `NexLudic.Infrastructure`| Concrete persistence, DPAPI crypto, Serilog logging, Job Object sandbox, MCP server. | Does not define business policies; strictly implements domain interfaces. |
| **Adapter Workers** | External Binaries | Executes game-specific mod scaffolding, compilation, script reflection, and telemetry. | Cannot access parent memory; cannot access unauthorized filesystem paths. |

---

## 4. Phase 2 Architectural Tenet Summary

The Phase 2 architecture establishes an airtight, defensible foundation that enables Phase 3 (Desktop UI & Workspace Management) to proceed with complete certainty regarding data ownership, process isolation, security boundaries, and communication protocols.

