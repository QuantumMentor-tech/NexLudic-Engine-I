# NexLudic Engine — Architecture Risk and Open-Question Register

**Document ID:** `NLE-REG-ARC-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 9 of 100)  
**Status:** Approved Technical Register  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Architecture  

---

## 1. Register Overview & Risk Governance

The **Architecture Risk and Open-Question Register** tracks critical architectural uncertainties, design trade-offs, potential failure points, and open engineering questions that impact the system architecture of **NexLudic Engine**.

Every registered risk is evaluated for its impact on feasibility, safety, extensibility, and implementation complexity. Items are actively managed through concrete mitigations and validation gates across Phase 2 and subsequent phases.

---

## 2. Master Architecture Risk Register

```
+-----------------------------------------------------------------------------------------------------------------------------+
|                                              MASTER ARCHITECTURE RISK REGISTER                                              |
+------------+------------------------------------+----------+---------------------+-------------------+------------+---------+
| Risk ID    | Risk Summary & Core Concern        | Severity | Affected Components | Mitigation Action | Owner      | Status  |
+------------+------------------------------------+----------+---------------------+-------------------+------------+---------+
| ARC-RSK-01 | Universal Core SDK Coupling        | High     | Core, Domain        | ADR-0002 Traits   | Lead Arch  | Mitig.  |
| ARC-RSK-02 | In-Process Adapter Crash Cascade   | Critical | App, Infrastructure | Out-of-Proc Worker| Systems Eng| Mitig.  |
| ARC-RSK-03 | Single-Language Mono-Stack Lock-In | High     | Scaffolding Drivers | Multi-Toolchain   | Modding Eng| Mitig.  |
| ARC-RSK-04 | IPC Latency Exceeds 60 Hz Budget   | High     | Named Pipe Host     | Protobuf/Fast Pipe| QA / Perf  | Tracked |
| ARC-RSK-05 | Breaking Game Patches Invalidate Ad| High     | Adapter Registry    | Hash Pinning      | Release Eng| Mitig.  |
| ARC-RSK-06 | AI Agent Escape from Sandbox Jail  | Critical | Win32 Job Object    | Staging Sandbox   | Security   | Mitig.  |
| ARC-RSK-07 | Cross-Process Dual Game Desync     | Moderate | IPC Bridge Host     | Dead-Reckoning    | Lead Arch  | Tracked |
| ARC-RSK-08 | Unnecessary Cloud Lock-In          | Moderate | AI Provider Layer   | Local-First Design| Product Mgr| Mitig.  |
| ARC-RSK-09 | LiteDB Document Size / Corruption  | Moderate | Persistence Engine  | JSON Sidecars     | Backend Eng| Mitig.  |
| ARC-RSK-10 | Over-Engineering Abstractions      | Moderate | Core Scaffolding    | YAGNI Enforcement | Lead Arch  | Mitig.  |
| ARC-RSK-11 | Insufficient Boundaries / Leakage  | High     | Domain Layer        | Clean Arch Rules  | Lead Arch  | Mitig.  |
| ARC-RSK-12 | Excessive Micro-Process Overhead   | Moderate | Adapter Supervisor  | Pooled Workers    | Systems Eng| Tracked |
| ARC-RSK-13 | Evidence Detachment from Reality   | Critical | Feasibility Engine  | Epistemic Tiers   | QA Lead    | Mitig.  |
| ARC-RSK-14 | Unjustified Hybrid Complexity      | Moderate | Strategy Engine     | ARB Justification | Product Mgr| Mitig.  |
| ARC-RSK-15 | Architecture Mistaken for Feasibil | Critical | All Documentation   | Strict Gating     | ARB Board  | Mitig.  |
+------------+------------------------------------+----------+---------------------+-------------------+------------+---------+
```

---

## 3. In-Depth Architecture Risk Profiles

---

### Risk ID: `ARC-RSK-01`
- **Title:** Universal Core Becoming Coupled to Specific Game SDKs.
- **Description:** Early implementation convenience may tempt engineers to reference game-specific DLLs (e.g. `FabricLoader.jar` or `UnityEngine.dll`) directly within `NexLudic.Core`. This would destroy platform universality and force the entire platform to recompile whenever a game SDK changes.
- **Affected Components:** `NexLudic.Core`, `NexLudic.Domain`.
- **Severity & Impact:** **High.** Architectural collapse into a game-specific tool.
- **Mitigation Action:** Strictly enforce Architecture Rule 3 (`Clean Architecture Dependency Boundary`). Core assemblies have zero references to game-specific libraries. All game interactions occur via `IGameAdapter` traits and out-of-process worker processes (`ADR-0002`).
- **Owner:** Principal Software Architect.
- **Status:** **MITIGATED BY ARCHITECTURE DESIGN.**

---

### Risk ID: `ARC-RSK-02`
- **Title:** In-Process Adapter Execution Creating an Unsafe Failure Boundary.
- **Description:** Loading third-party game modding libraries or native C++ hooks in-process into the WPF desktop shell risks crashing the entire desktop application whenever an adapter encounters an access violation (`0xC0000005`) or unhandled exception.
- **Affected Components:** `NexLudic.App`, `NexLudic.Infrastructure`.
- **Severity & Impact:** **Critical.** Application instability, user data corruption.
- **Mitigation Action:** Formally adopt the **Out-of-Process Adapter Worker Model** (`ADR-0002`). Adapters execute as separate Windows child processes monitored by the core engine. A crash in an adapter is detected via exit codes and isolated without affecting the desktop shell.
- **Owner:** Senior Systems Design Engineer.
- **Status:** **MITIGATED BY ARCHITECTURE DESIGN.**

---

### Risk ID: `ARC-RSK-03`
- **Title:** Single-Language Toolchain Assumption Blocking Viable Game Adapters.
- **Description:** Assuming all game integrations can be scaffolded in C#/.NET would permanently lock out mature ecosystems like Minecraft (Java/Fabric) or native titles (C++/UE4SS, Lua/CET).
- **Affected Components:** `NexLudic.Core.Scaffolding`.
- **Severity & Impact:** **High.** Inability to expand beyond Unity games.
- **Mitigation Action:** Structure scaffolding drivers behind generic `IBuildToolchainDriver` interfaces. The platform drives external CLI build systems (Gradle, MSBuild, CMake) natively within staging jails (`UNIVERSAL_CORE_AND_GAME_BOUNDARIES.md`).
- **Owner:** Senior Game-Modding Research Engineer.
- **Status:** **MITIGATED BY ARCHITECTURE DESIGN.**

---

### Risk ID: `ARC-RSK-04`
- **Title:** IPC Telemetry Latency Exceeds 60 Hz Frame Budget ($>16.6\text{ ms}$).
- **Description:** In Strategy B (Live Bridge), high serialization overhead, OS pipe buffer contention, or thread scheduling delays could cause round-trip latency to exceed $16.6\text{ ms}$, introducing visible gameplay desync and frame stutter.
- **Affected Components:** `NexLudic.Infrastructure.IPC`.
- **Severity & Impact:** **High.** Degraded user experience during live bridge play.
- **Mitigation Action:** Utilize Windows Named Pipes in asynchronous message mode with binary Protocol Buffers serialization (`ADR-0003`). If benchmarks reveal JSON overhead, enforce binary structs. Fallback: shared memory ring buffers (`System.IO.MemoryMappedFiles`).
- **Owner:** Senior QA Automation & Performance Engineer.
- **Status:** **TRACKED (Benchmarking scheduled in Phase 3).**

---

### Risk ID: `ARC-RSK-05`
- **Title:** Game Updates Silently Invalidate Adapter Compatibility.
- **Description:** PC games receive frequent automatic updates on Steam, Epic, and launcher stores. A minor patch can alter binary memory offsets or bytecode signatures, causing an adapter to fail catastrophically.
- **Affected Components:** `NexLudic.Core.Registry`.
- **Severity & Impact:** **High.** Broken user mashups post-update.
- **Mitigation Action:** Enforce **Binary SHA-256 Hash Verification** at every project launch (`REQ-F-002`). If an executable hash mismatch is detected, the engine flags `UnverifiedVersion` and requires adapter re-qualification before execution.
- **Owner:** Build & Release Engineer.
- **Status:** **MITIGATED BY ARCHITECTURE DESIGN.**

---

### Risk ID: `ARC-RSK-06`
- **Title:** AI Coding Agent Attempting Unauthorized OS Operations.
- **Description:** An LLM or autonomous coding agent generating build scripts or mod source code may hallucinate dangerous commands (e.g. registry edits, downloading unauthorized binaries, writing outside the project root).
- **Affected Components:** `NexLudic.Core.AI`, `NexLudic.Infrastructure.Sandbox`.
- **Severity & Impact:** **Critical.** Security compromise of the host system.
- **Mitigation Action:** Confinment inside **Win32 Job Objects** (`ADR-0009`) with hard CPU, memory, and process quotas. Staging jail isolation (`<project>/staging/`) and the **Mandatory Human Diff Review Gate** (`REQ-F-009`) prevent unauthorized file deployment.
- **Owner:** Senior Security & Privacy Analyst.
- **Status:** **MITIGATED BY ARCHITECTURE DESIGN.**

---

### Risk ID: `ARC-RSK-15`
- **Title:** Architecture Documentation Being Mistaken for Proof of Feasibility.
- **Description:** Stakeholders or engineers might assume that because a component or sequence is cleanly diagrammed, the integration is technically proven and ready for immediate deployment.
- **Affected Components:** Entire Documentation Baseline.
- **Severity & Impact:** **Critical.** False confidence, unevidenced delivery commitments.
- **Mitigation Action:** Enforce **Epistemic Tier Labeling** across all architecture artifacts. Architecture diagrams represent *design intent*, not empirical proof. Every integration capability requires clearance through Gate G4 (Behavioral Validation) before being represented as supported.
- **Owner:** Architecture Review Board (ARB).
- **Status:** **MITIGATED BY GOVERNANCE POLICY.**

---

*End of Architecture Risk and Open-Question Register (NLE-REG-ARC-001).*

