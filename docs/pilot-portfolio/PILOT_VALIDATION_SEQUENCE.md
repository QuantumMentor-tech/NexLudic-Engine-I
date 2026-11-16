# NexLudic Engine — Pilot Validation Sequence

**Document ID:** `NLE-SEQ-PILOT-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 7 of 100)  
**Status:** Approved Architecture Plan  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Engineering  

---

## 1. Executive Summary & Authorization Boundary

The **Pilot Validation Sequence** defines the conditional, risk-gated execution roadmap for advancing shortlisted pilot candidates through future validation phases.

> [!IMPORTANT]
> **Strict Execution Boundary Notice:**  
> This document establishes a **provisional planning sequence only**. It does **NOT** authorize the execution of any proof-of-concept, runtime experiment, game modification, or software implementation during Phase 1. Execution of each pilot stage requires explicit, future authorization at Gate G3 by the Architecture Review Board.

The sequence enforces a **dependency-driven progression**: foundational risks in local mod scaffolding and rollback must be eliminated before the platform attempts multi-toolchain compilation or live dual-process inter-process communication.

```
+----------------------------------------------------------------------------------------------------+
|                                    PILOT VALIDATION ROADMAP                                        |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
     +--------------------------------------------------------------------------+
     | STAGE 1: BOUNDED HOST MOD SCAFFOLDING (PILOT 1: MINECRAFT + RDR2)        |
     | Objectives: JVM/Gradle scaffolding, staging jail isolation, rollback.    |
     +------------------------------------+-------------------------------------+
                                          | Gate G4 Cleared (Pass)
                                          v
     +--------------------------------------------------------------------------+
     | STAGE 2: MULTI-TOOLCHAIN EXTENSIBILITY (PILOT 2: ROR2 + COD ZOMBIES)     |
     | Objectives: Unity / Mono / C# 12 scaffolding, BepInEx, toolchain decouple|
     +------------------------------------+-------------------------------------+
                                          | Gate G4 Cleared (Pass)
                                          v
     +--------------------------------------------------------------------------+
     | STAGE 3: LIVE INTEROPERABILITY BRIDGE (PILOT 3: ROCKET LEAGUE + CP2077)  |
     | Objectives: Out-of-process Named Pipe IPC, 60 Hz telemetry, fault isol.  |
     +------------------------------------+-------------------------------------+
                                          | Gate G4 Cleared (Pass)
                                          v
     +--------------------------------------------------------------------------+
     | STAGE 4: HYBRID INTEGRATION ARCHITECTURE (PILOT 4: MC + SPIDER-MAN)      |
     | Objectives: Composite adapter orchestration, in-engine mod + IPC sync.   |
     +--------------------------------------------------------------------------+
```

---

## 2. Stage-by-Stage Validation Specifications

---

### Stage 1: Bounded Host-Game Scaffolding & Packaging
- **Candidate Focus:** **Pilot 1 (`CAND-PAIR-001`)** — *Minecraft: Java Edition (Fabric 1.21) + Red Dead Redemption 2 Mechanics*.
- **Primary Objective:** Validate that NexLudic Engine's core scaffolding subsystem (`NexLudic.Core.Scaffolding`) can automatically synthesize, compile, and package a working host-game mod in a sandboxed staging directory, verify clean rollback via `cleanup.ps1`, and log structured build telemetry.
- **Entry Criteria:**
  - Local Java 21 JDK and Gradle build environment installed and verified on test machine.
  - Formal PoC specification (`POC-MCJAVA-RDR2-001`) approved at Gate G3 by ARB.
  - Staging sandbox confirmed active under `<project_root>/staging/poc/POC-MCJAVA-RDR2-001/`.
- **Key Architectural Validations:**
  - `ADR-0001` (Modular Monolith Core separation).
  - `ADR-0008` (Sandboxed file staging and diff generation).
  - `REQ-MOD-001` (Non-destructive mod installation).
  - `REQ-MOD-003` (Non-distributive local asset extraction).
- **Exit Criteria (Gate G4 Approval):**
  - Mod compiles cleanly via automated CLI without human syntax correction.
  - Headless test harness confirms weapon raycasting, tick dilation (`tick_rate`), and HUD shader hooks execute without crashing Minecraft.
  - `cleanup.ps1` returns Minecraft installation to byte-for-byte pristine hash.
- **Fallback & Recovery Path:**
  - If automated Gradle scaffolding fails, halt progression and refactor the code-generation templates in `NexLudic.Core.Scaffolding`. Do not advance to Stage 2 until JVM scaffolding achieves 100% deterministic compilation.

---

### Stage 2: Toolchain & Engine Runtime Independence
- **Candidate Focus:** **Pilot 2 (`CAND-PAIR-002`)** — *Risk of Rain 2 (Unity 2021 LTS Mono / BepInEx) + CoD Zombies Survival Loop*.
- **Primary Objective:** Prove that the platform's adapter contract (`IGameAdapter`, `IModScaffoldable`) is genuinely engine-agnostic and language-independent by compiling and deploying a C# / MSBuild BepInEx plugin into a Unity game without modifying core engine interfaces.
- **Entry Criteria:**
  - Stage 1 successfully cleared at Gate G4.
  - .NET 8.0 SDK and MSBuild toolchain verified.
  - Formal PoC specification (`POC-ROR2-CODZ-001`) approved at Gate G3.
- **Key Architectural Validations:**
  - `ADR-0002` (Out-of-Process Adapter Worker Model).
  - Multi-language scaffolding (C# MSBuild vs. Java Gradle).
  - HarmonyX runtime bytecode hook management and safe detachment.
- **Exit Criteria (Gate G4 Approval):**
  - BepInEx plugin compiled and staged via CLI.
  - Round wave manager and Mystery Box roulette entity execute inside RoR2 without memory leaks ($<5\text{ MB/min}$) or assembly version conflicts.
  - Zero residual DLLs left in game directory after teardown.
- **Fallback & Recovery Path:**
  - If BepInEx scaffolding reveals JVM-specific coupling in `NexLudic.Core`, refactor adapter interfaces to abstract away build tools behind generic `IBuildToolchainDriver` abstractions.

---

### Stage 3: Live Interoperability Bridge & Fault Containment
- **Candidate Focus:** **Pilot 3 (`CAND-PAIR-003`)** — *Rocket League (BakkesMod / C++) + Cyberpunk 2077 (CET / Lua)*.
- **Primary Objective:** Validate out-of-process, bi-directional IPC telemetry streaming over local Windows Named Pipes between two concurrently executing native games, while proving strict fault containment under process termination.
- **Entry Criteria:**
  - Stage 2 successfully cleared at Gate G4.
  - Test machine equipped with compatible GPU hardware ($\ge 6\text{ GB}$ VRAM) or validated headless mock harness (`BLOCK-003`).
  - Formal PoC specification (`POC-RL-CP2077-001`) approved at Gate G3.
- **Key Architectural Validations:**
  - `ADR-0003` (Windows Named Pipes Transport Protocol).
  - `ADR-0009` (Win32 Job Object process boundary & CPU/RAM quotas).
  - `RSK-07` (Guest process crash isolation).
- **Exit Criteria (Gate G4 Approval):**
  - Continuous 60 Hz telemetry stream achieves round-trip latency $\le 16.6\text{ ms}$ (99th percentile $\le 4.0\text{ ms}$).
  - Abrupt termination (`kill -9`) of Cyberpunk 2077 or the CET plugin does not crash Rocket League or cause frame drops ($<1.0\text{ FPS}$ delta).
  - Named pipe disconnect is handled gracefully with automatic exponential backoff reconnection.
- **Fallback & Recovery Path:**
  - If IPC latency exceeds $16.6\text{ ms}$, transition pipe serialization from JSON to Protocol Buffers or shared memory mapped buffers (`System.IO.MemoryMappedFiles`).
  - If test machine hardware remains constrained, execute Stage 3 using the headless mock bridge harness until dedicated test hardware is provisioned.

---

### Stage 4: Hybrid Integration Architecture
- **Candidate Focus:** **Pilot 4 (`CAND-PAIR-004`)** — *Minecraft Java (Fabric) + Marvel's Spider-Man Remastered Mechanics*.
- **Primary Objective:** Validate composite adapter orchestration where in-engine game modifications (web-swinging physics, HUD) operate simultaneously with an external telemetry reader without causing thread deadlocks or focus contention.
- **Entry Criteria:**
  - Stage 3 successfully cleared at Gate G4.
  - Formal PoC specification (`POC-MCJAVA-SMREM-001`) approved at Gate G3.
- **Key Architectural Validations:**
  - Composite adapter lifecycle orchestration (`NexLudic.Core.Adapters.CompositeAdapter`).
  - Strategy C (Hybrid) architectural viability.
  - Non-invasive user-mode window inspection (`User32.dll`).
- **Exit Criteria (Gate G4 Approval):**
  - In-engine traversal momentum runs smoothly at $\ge 60\text{ FPS}$ inside Minecraft.
  - External telemetry hook operates in non-blocking background thread with zero administrative elevation.
  - Full system rollback verified.
- **Fallback & Recovery Path:**
  - If hybrid orchestration introduces unacceptable thread contention, decouple telemetry polling to an asynchronous queue worker with a fixed 10 Hz refresh cap.

---

## 3. Sequence Interruption & Termination Criteria

The validation sequence must be paused or permanently terminated upon the occurrence of any of the following triggers:

1. **Safety or Policy Violation:** Immediate termination if any pilot attempt triggers an anti-cheat detection, account warning, DRM alert, or system instability event (`RSK-01`).
2. **Core Architectural Invalidation:** Immediate pause if a pilot demonstrates that the out-of-process adapter model (`ADR-0002`) cannot meet real-time latency thresholds, requiring ARB architectural re-evaluation.
3. **Repeated Stage Failure:** If a stage fails Gate G4 validation across three successive iterations, the candidate pair must be marked **DEFERRED** or **REJECTED**, and an alternative archetype candidate from `GAME_PAIR_CANDIDATE_REGISTER.md` must be promoted.

---

*End of Pilot Validation Sequence (NLE-SEQ-PILOT-001).*

