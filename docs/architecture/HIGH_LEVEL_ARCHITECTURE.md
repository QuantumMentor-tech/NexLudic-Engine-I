# NexLudic Engine — High-Level Architecture Specification

**Document ID:** `NLE-ARC-HLA-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 9 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Principal Software Architecture  

---

## 1. Architectural Vision & Core Paradigm

The **High-Level Architecture (HLA)** establishes the structural organization, component responsibilities, dependency rules, and communication topologies of **NexLudic Engine**.

NexLudic Engine is architected as a **C# 12 / .NET 8.0 LTS Modular Monolith** (`ADR-0001`) operating on Windows 10/11 x64 (`ADR-0005`). It incorporates an **Out-of-Process Adapter Worker Model** (`ADR-0002`), **Windows Named Pipes IPC** (`ADR-0003`), **Win32 Job Object Sandboxing** (`ADR-0009`), and an **Embedded NoSQL Store** (`ADR-0007`).

```
+----------------------------------------------------------------------------------------------------+
|                                    CLEAN MODULAR MONOLITH CORE                                     |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [ PRESENTATION LAYER ]       NexLudic.App (WPF 8.0 Desktop Shell, MVVM, Views, ViewModels)       |
|                                       |                                                            |
|                                       v                                                            |
|  [ APPLICATION LAYER ]        NexLudic.Core (Workflow Orchestration, Feasibility Engine,          |
|                                              AI Agent Sandbox Jail, Scaffolding Drivers)           |
|                                       |                                                            |
|                                       v                                                            |
|  [ DOMAIN LAYER ]             NexLudic.Domain (Capability Contracts, Project Entities,             |
|                                                Epistemic Tiers, Strategy Rules - ZERO DEPENDENCIES)|
|                                       ^                                                            |
|                                       |                                                            |
|  [ INFRASTRUCTURE LAYER ]     NexLudic.Infrastructure (Named Pipe IPC Server, LiteDB Store,        |
|                                                        Win32 Job Objects, Git / Rollback Engine)   |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
                                       |
                   +-------------------+-------------------+
                   | (Named Pipe IPC)                      | (Process Execution)
                   v                                       v
+--------------------------------------+   +--------------------------------------+
|  GAME ADAPTER WORKER (OUT-OF-PROCESS)|   |    AI CODING AGENT STAGING JAIL      |
|  - Isolated external process         |   |  - Confined strictly to staging/     |
|  - Implements IGameAdapter traits    |   |  - Win32 Job Object quota limits     |
|  - Game-specific runtimes (JVM/C#)   |   |  - Mandatory human diff review       |
+--------------------------------------+   +--------------------------------------+
```

---

## 2. Principal Architecture Drivers

The architecture is explicitly shaped by twelve primary architectural drivers:

| Driver ID | Driver Name | Architectural Implication | Addressed By |
| :--- | :--- | :--- | :--- |
| **DRV-01** | **Universality** | Core engine must not couple to any specific game engine, mod loader, or proprietary asset format. | Clean Domain contracts (`ADR-0001`), out-of-process workers (`ADR-0002`). |
| **DRV-02** | **Extensibility** | New game ecosystems must be supported via decoupled adapter traits without core recompilation. | Compositional adapter interfaces (`IGameAdapter`, `IModScaffoldable`). |
| **DRV-03** | **Capability Transparency**| Engine must explicitly report partial, unverified, or prohibited capabilities (`CAP-###`). | 4 Epistemic Tiers, 14-question feasibility engine (`NLE-FEAS-FRAMEWORK-001`).|
| **DRV-04** | **Adapter Isolation** | A crash or memory leak in a game adapter must never crash the core desktop shell. | Out-of-process worker model (`ADR-0002`), Win32 Job Objects (`ADR-0009`). |
| **DRV-05** | **AI Agent Containment** | AI-generated code is untrusted and must be quarantined before deployment. | Sandboxed staging jail (`<project>/staging/`), human diff gate (`ADR-0008`). |
| **DRV-06** | **Security & Policy** | Zero kernel drivers, zero administrative elevation, permanent anti-cheat hard block. | User-mode Win32 `SendInput` (`DEC-003`), EAC/VAC blocker (`REQ-SAFE-001`). |
| **DRV-07** | **Reproducibility** | All feasibility decisions, test procedures, and telemetry logs must be fully repeatable. | LiteDB evidence store (`ADR-0007`), standardized 20-field PoC template. |
| **DRV-08** | **Maintainability** | Breaking game patches must invalidate only the specific adapter, not unrelated tools. | Adapter version pinning (`REQ-CMP-005`), semantic version boundaries. |
| **DRV-09** | **Reliability & Recovery**| The system must guarantee clean rollback to pristine game file hashes upon teardown. | Automated `cleanup.ps1` generation and verification (`REQ-F-010`). |
| **DRV-10** | **Usability** | The desktop UI must clearly expose risk-gated progress, warnings, and error diagnostics. | WPF desktop shell (`ADR-0006`) with MVVM presentation architecture. |
| **DRV-11** | **Performance** | IPC transport must operate within 60 Hz frame synchronization budgets ($\le 16.6\text{ ms}$). | Windows Named Pipes transport with Protocol Buffer serialization (`ADR-0003`).|
| **DRV-12** | **Distribution Lifecycle**| Zero raw proprietary assets bundled; user-side local extraction scripts only. | Non-distributive asset policy (`REQ-MOD-003`, `DEC-006`). |

---

## 3. High-Level Building Blocks (Logical Components)

The platform is decomposed into twelve logical components across four architectural layers:

```
+----------------------------------------------------------------------------------------------------+
|                                    LOGICAL COMPONENT ARCHITECTURE                                  |
+-------------------+-----------------------------------+--------------------------------------------+
| Layer             | Component Name                    | Primary Responsibility                     |
+-------------------+-----------------------------------+--------------------------------------------+
| **Presentation**  | 1. Desktop Presentation (`App.UI`)| WPF MVVM views, project dashboard, diff    |
|                   |                                   | viewer, real-time telemetry HUD.           |
+-------------------+-----------------------------------+--------------------------------------------+
| **Application**   | 2. Application Orchestration      | Coordinates project workflows, command     |
|                   |                                   | dispatching, and view-model queries.       |
|                   | 3. Feasibility & Evidence Engine  | Executes 14-question evaluation; manages   |
|                   |                                   | Epistemic Tiers and Gate G0–G6 clearance.  |
|                   | 4. AI-Agent Orchestration Jail    | Manages agent sessions, prompts, and Win32 |
|                   |                                   | Job Object execution quotas.               |
|                   | 5. Mod Scaffolding Driver         | Drives external build systems (Gradle,     |
|                   |                                   | MSBuild, CMake) to compile staged code.    |
|                   | 6. Testing & Validation Coord.    | Generates test harnesses, executes fault   |
|                   |                                   | injection, logs pass/fail thresholds.      |
+-------------------+-----------------------------------+--------------------------------------------+
| **Domain**        | 7. Domain Model & Capability State| Universal domain entities (`Project`,      |
|                   |                                   | `GameSignature`, `CapabilityContract`).    |
|                   | 8. Adapter Trait Contracts        | Defines `IGameAdapter`, `ITelemetrySource`,|
|                   |                                   | `IModScaffoldable`, `IInputTarget`.        |
+-------------------+-----------------------------------+--------------------------------------------+
| **Infrastructure**| 9. Named Pipe IPC Host            | Windows Named Pipe server; coordinates     |
|                   |                                   | binary packet transport and reconnection.  |
|                   | 10. Persistence & Config (LiteDB) | Embedded NoSQL document store for project  |
|                   |                                   | metadata, evidence, and risk registers.    |
|                   | 11. Staging & Rollback Engine     | Manages `<project>/staging/` workspace     |
|                   |                                   | and automated `cleanup.ps1` verification.  |
|                   | 12. Diagnostics & Telemetry Sink  | High-resolution Stopwatch timer, ETW       |
|                   |                                   | logger, and crash dump capture.            |
+-------------------+-----------------------------------+--------------------------------------------+
```

---

## 4. Architectural Dependency Inversion & Rules

To ensure long-term maintainability and prevent circular dependencies, four inviolable structural dependency rules are enforced:

```
[ NexLudic.App ] --------> [ NexLudic.Core ] --------> [ NexLudic.Domain ]
       |                            |                           ^
       |                            v                           |
       +-------------------> [ NexLudic.Infrastructure ] -------+
```

1. **Rule 1 (The Domain Invariance Rule):**  
   `NexLudic.Domain` has **zero external project dependencies** and zero third-party package dependencies. It contains purely POCO entities, value objects, and adapter trait interfaces.
2. **Rule 2 (Core Decoupling):**  
   `NexLudic.Core` depends only on `NexLudic.Domain`. It coordinates business logic using dependency inversion (interfaces implemented in `Infrastructure`).
3. **Rule 3 (Zero Game Assembly References):**  
   Neither `NexLudic.Core`, `Domain`, nor `App` may reference game-specific DLLs (e.g. `FabricLoader.dll`, `UnityEngine.dll`). All game-specific code lives in out-of-process adapter workers.
4. **Rule 4 (Infrastructure Isolation):**  
   Low-level OS operations (Win32 P/Invoke, Named Pipes, LiteDB, File IO) are confined to `NexLudic.Infrastructure`.

---

## 5. Conceptual Communication Models

The platform employs three distinct communication patterns matched to specific operational requirements:

| Pattern | Mechanism | Boundary Crossed | Use Case | Latency Budget |
| :--- | :--- | :--- | :--- | :--- |
| **Pattern A: In-Process Mediator** | C# MediatR / In-Memory Events | None (Inside Core process) | UI command dispatching, view-model synchronization. | $<0.1\text{ ms}$ |
| **Pattern B: Local Out-of-Process IPC** | Windows Named Pipes (`\\.\pipe\nexludic_*`) | Process boundary (Core $\leftrightarrow$ Game) | Live telemetry streaming, coordinate forwarding, event RPCs. | $\le 16.6\text{ ms}$ (Target $\le 4.0\text{ ms}$) |
| **Pattern C: File-Based Staging Handoff** | Structured JSON / Staged Code Files | Storage boundary (Core $\leftrightarrow$ Build Tool) | AI code generation, compiler invocations, test reports. | Asynchronous (seconds) |

---

## 6. High-Level Data Ownership & Storage Principles

In accordance with `ADR-0007`, data persistence is partitioned based on lifecycle, mutability, and durability:

```
+----------------------------------------------------------------------------------------------------+
|                                    DATA OWNERSHIP & PERSISTENCE MODEL                              |
+----------------------+--------------------+--------------------+-----------------------------------+
| Data Category        | Storage Target     | Authoritative Owner| Mutation & Access Policy          |
+----------------------+--------------------+--------------------+-----------------------------------+
| Project Metadata     | LiteDB (`.db`)     | NexLudic.Core      | Durable, user-editable via UI.    |
| Game Signatures      | LiteDB (`.db`)     | NexLudic.Core      | Read-only once verified (SHA-256).|
| Feasibility Evidence | LiteDB (`.db`)     | Feasibility Engine | Append-only evidence log.         |
| Staged Code Diffs    | File System        | Staging Jail       | Disposable; human approval req.   |
| Live IPC Telemetry   | Memory (In-Flight) | Named Pipe Host    | Ephemeral; sampled to CSV if debug|
| Rollback Manifest    | JSON File          | Staging Engine     | Mandatory for cleanup.ps1.        |
+----------------------+--------------------+--------------------+-----------------------------------+
```

---

## 7. Failure Containment & Recovery Architecture

```
+----------------------------------------------------------------------------------------------------+
|                                    FAULT CONTAINMENT TOPOLOGY                                      |
+----------------------------------------------------------------------------------------------------+
                                                  |
     +--------------------------------------------+--------------------------------------------+
     |                                                                                         |
     v                                                                                         v
+-------------------------------------------------+       +-------------------------------------------------+
|          ADAPTER OR GUEST PROCESS CRASH         |       |            COMPILER OR BUILD STALL              |
| - Named Pipe detects broken pipe (EOF / Error)  |       | - Win32 Job Object CPU quota detects timeout    |
| - Core logs disconnect event & timestamp        |       | - Core terminates hung compiler process         |
| - Host game continues execution unimpeded       |       | - Staging jail purged; error returned to user   |
| - Automatic exponential backoff reconnection    |       | - Zero corrupt files written to game directory  |
+-------------------------------------------------+       +-------------------------------------------------+
```

1. **Host Game Protection:** If an external adapter or guest game process crashes during a live bridge session, the core engine's Named Pipe listener catches the disconnection, safely releases OS pipe handles, and isolates the failure. The host game's main loop continues uninterrupted (`REQ-NF-005`, `RSK-07`).
2. **Build Staging Isolation:** If an external compiler (e.g. Gradle or MSBuild) hangs or exceeds resource limits, the Win32 Job Object automatically terminates the process tree after a configurable timeout ($60\text{ seconds}$).

---

*End of High-Level Architecture Specification (NLE-ARC-HLA-001).*

