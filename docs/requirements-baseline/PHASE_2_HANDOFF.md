# NexLudic Engine — Phase 2 Handoff Package

**Document ID:** `NLE-DOC-HANDOFF-P2-001`  
**Version:** 1.0.0  
**Phase Transition:** Phase 1 (Product Definition & Research) $\to$ Phase 2 (Architecture & Development Foundation)  
**Status:** Approved Handoff Package  
**Date:** November 16, 2026  
**Authority:** Architecture Review Board (ARB) & Systems Engineering  

---

## 1. Executive Summary & Authorization Scope

The **Phase 2 Handoff Package** formalizes the transfer of verified specifications, architectural constraints, technology baselines, and feasibility gates from **Phase 1** into **Phase 2: Architecture & Development Foundation** (Steps 9 through 20).

> [!IMPORTANT]
> **Strict Phase Boundary Notice:**  
> This handoff package defines the engineering parameters for future Phase 2 work. It does **NOT** authorize Phase 2 implementation, solution scaffolding, or dependency installation during Step 8. Phase 2 activities commence only after explicit authorization by the project owner.

---

## 2. Transferred Assets & Authoritative Baselines

Phase 2 engineering must build strictly upon the authoritative artifacts established in Phase 1:

| Handoff Component | Authoritative Document Reference | Key Constraints Transferred to Phase 2 |
| :--- | :--- | :--- |
| **Product Requirements** | [NEXLUDIC_PRODUCT_REQUIREMENTS_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/requirements-baseline/NEXLUDIC_PRODUCT_REQUIREMENTS_SPECIFICATION.md) | 13 Functional, 7 Non-Functional, 7 Security, and 5 Compatibility requirements. Scope strictly bounded to Tier 1 MVP. |
| **Requirements Traceability**| [REQUIREMENTS_TRACEABILITY_MATRIX.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/requirements-baseline/REQUIREMENTS_TRACEABILITY_MATRIX.md) | 100% bidirectional links between Goals, Needs, Requirements, ADRs, Risks, Gates, and Pilots. |
| **Technology Baseline** | [RECOMMENDED_TECHNOLOGY_BASELINE.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/RECOMMENDED_TECHNOLOGY_BASELINE.md) | C# 12 / .NET 8.0 LTS Modular Monolith (`NexLudic.App`, `Core`, `Domain`, `Infrastructure`). |
| **Architecture Decisions** | [ADR-0001 through ADR-0011](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/architecture/decisions/) | Out-of-Process Worker Model (`ADR-0002`), Named Pipes IPC (`ADR-0003`), Win32 Job Object sandbox (`ADR-0009`). |
| **Feasibility & Quality Gates**| [RISK_GATED_VALIDATION_PLAN.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/RISK_GATED_VALIDATION_PLAN.md) | Gates G0 through G6. Mandatory 20-field PoC specification required for Gate G3 clearance. |
| **Risk Register** | [FEASIBILITY_RISK_REGISTER.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/FEASIBILITY_RISK_REGISTER.md) | 24 active platform risks (`RSK-01` to `RSK-24`) with prevention controls and rollback requirements. |
| **Pilot Portfolio Strategy** | [PILOT_PORTFOLIO_STRATEGY.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/pilot-portfolio/PILOT_PORTFOLIO_STRATEGY.md) | 4 shortlisted pilot archetypes (JVM Fabric, Unity BepInEx, Native Named Pipe, Composite Hybrid). |

---

## 3. Mandatory Invariants for Phase 2 Architecture Design

During Phase 2, the development team must adhere to seven inviolable architectural invariants:

1. **Zero Core Coupling to Game Binaries (`REQ-NF-007`):**  
   `NexLudic.Core` and `NexLudic.Domain` must never reference Minecraft, Fabric, Unity, Unreal, or any other proprietary game assembly. All game-specific logic must be implemented via the Out-of-Process Adapter Worker Model (`ADR-0002`) implementing `IGameAdapter` traits.
2. **Win32 Job Object Sandboxed Staging Jail (`ADR-0009`):**  
   All AI coding agent file creation, compilation, and scaffolding execution must be confined strictly to `<project>/staging/`. The host platform must enforce CPU/memory quotas and prevent child process breakout.
3. **Mandatory Human-in-the-Loop Diff Gate (`REQ-F-009`):**  
   The platform must require explicit human approval of file diffs before any staged mod, script, or configuration is applied to target game directories.
4. **Driverless, User-Mode Input Injection (`DEC-003`):**  
   Kernel-mode drivers (such as the archived ViGEmBus) are permanently prohibited. Input forwarding must use standard Win32 `SendInput` or in-engine event injection.
5. **Anti-Cheat Hard Block (`REQ-SAFE-001`, `RSK-01`):**  
   The platform must prevent any memory scanning, DLL injection, or debugger attachment to games protected by active kernel-level anti-cheat (EAC, BattlEye, Vanguard, Ricochet, VAC).
6. **Non-Distributive Asset Policy (`REQ-MOD-003`, `DEC-006`):**  
   Zero raw proprietary textures, audio banks, models, or executables may be bundled in the repository or core release packages. All asset extraction must occur locally via user-side scripts.
7. **Automated Clean Rollback (`REQ-F-010`, `REQ-NF-006`):**  
   Every generated integration scaffold must include an automated `cleanup.ps1` script that restores target game installations to byte-for-byte pristine hash baseline.

---

## 4. Phase 2 Scope & Initial Objectives (Step 9 Preview)

Upon formal authorization, Phase 2 will focus on establishing the architectural foundation and core solution structure:

- **Step 9:** Solution Structure, Dependency Management & Project Scaffolding (.NET 8.0 Solution initialization: `NexLudic.App`, `NexLudic.Core`, `NexLudic.Domain`, `NexLudic.Infrastructure`).
- **Step 10:** Domain Model & Capability Contract Architecture (Defining `IGameAdapter`, `IModScaffoldable`, `ITelemetrySource`, `IInputTarget`).
- **Step 11:** Win32 Job Object Sandboxing & Process Boundary Implementation.
- **Step 12:** Windows Named Pipes IPC Transport & Protocol Buffer Serialization Layer.
- **Step 13:** Workspace Manager & File Staging Jail Engine.
- **Step 14:** Automated Scaffolding Engine for Strategy A (JVM Fabric & Unity BepInEx).

---

## 5. Tracked Prerequisites for Phase 2 Execution

| Prerequisite ID | Prerequisite Description | Target Step for Resolution | Owner |
| :--- | :--- | :--- | :--- |
| **`PREREQ-01`** | Install OpenJDK 21 LTS & Gradle on test machine (`BLOCK-001`). | Required prior to Step 14 (Fabric Scaffolding). | Lead Environment Engineer |
| **`PREREQ-02`** | Install MSVC v143 C++ build tools (`BLOCK-002`). | Required prior to Step 17 (Native Hooks). | Build & Release Engineer |
| **`PREREQ-03`** | Implement Headless Mock Bridge Harness for Strategy B (`REQ-F-012`).| Scheduled in Step 12 (IPC Implementation). | Systems Architect |

---

*End of Phase 2 Handoff Package (NLE-DOC-HANDOFF-P2-001).*

