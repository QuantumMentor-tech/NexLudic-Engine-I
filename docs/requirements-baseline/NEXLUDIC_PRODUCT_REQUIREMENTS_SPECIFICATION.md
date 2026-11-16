# NexLudic Engine — Product Requirements Specification (PRS)

**Document ID:** `NLE-SRS-001`  
**Version:** 1.0.0 (Phase 1 Baseline)  
**Phase:** 1 — Product Definition & Research (Step 8 of 100)  
**Status:** Approved Requirements Baseline  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Product Architecture  

---

## 1. Product Purpose, Vision, and Scope Boundaries

### 1.1 Product Vision & Problem Statement
Creating cross-game mashups and integrations on PC has historically required ad-hoc, fragile, and game-specific reverse engineering. Modders and developers face severe obstacles:
1. **Toolchain Fragmentation:** Every game engine (JVM, Unity Mono, Unreal C++, REDengine) requires completely separate SDKs, decompilers, build systems, and hooking frameworks.
2. **Fragility & Crash Risks:** Ad-hoc DLL injections and memory hacks frequently cause game crashes, access violations (`0xC0000005`), or system instability.
3. **Account & Legal Jeopardy:** Unvetted memory tampering in games with active anti-cheat (VAC, EAC, BattlEye) leads to permanent account bans, while unauthorized bundling of proprietary textures, models, or audio exposes creators to copyright infringement.
4. **Architectural Lock-In:** Most game mashups are hardcoded as one-off hacks between two specific games, producing zero reusable engineering knowledge.

**NexLudic Engine** is a universal, AI-assisted desktop platform designed to help users evaluate, design, scaffold, test, and manage cross-game mashups across fundamentally different PC games, engines, and modification ecosystems.

### 1.2 What NexLudic Engine Is vs. What It Is NOT

| What NexLudic Engine IS | What NexLudic Engine IS NOT |
| :--- | :--- |
| A universal orchestration, evaluation, and scaffolding desktop platform. | A single-game mod, mod-pack, or fixed bridge between two specific games. |
| An out-of-process manager enforcing Win32 Job Object sandboxes and clean rollback. | A universal game emulator, hypervisor, or general-purpose 3D rendering engine. |
| An evidence-first feasibility analyzer that verifies integration surfaces before coding. | A magic "black box" that promises any two arbitrary games can be merged. |
| A toolchain driver supporting C#, Java, C++, and Lua through common adapter traits. | A closed mono-stack that forces all game mods into a single programming language. |
| A non-distributive platform operating strictly on user-owned, locally installed files. | A pirate tool, asset redistribution site, DRM bypasser, or anti-cheat circumvention tool. |

### 1.3 Architectural Boundaries: Universal Core vs. Game-Specific Projects
The platform strictly decouples universal domain logic from game-specific tooling (`ADR-0001`, `ADR-0002`):

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC UNIVERSAL CORE                                         |
|    - Project Management & Workspace Orchestration       - 14-Question Feasibility Assessment Engine|
|    - Gate G0-G6 Risk-Gated Workflow Engine             - Sandboxed AI Agent Execution Jail         |
|    - Windows Named Pipe IPC Host (\\.\pipe\nexludic_*) - Universal Telemetry & Packet Deserializer|
|    - Automated Git / Staging Diff & Rollback Verifier   - Capability Contract Manager               |
+-------------------------------------------------+--------------------------------------------------+
                                                  |
                  +-------------------------------+-------------------------------+
                  |                                                               |
                  v                                                               v
+------------------------------------+                         +------------------------------------+
|     GAME-SPECIFIC ADAPTER A        |                         |     GAME-SPECIFIC ADAPTER B        |
| - Out-of-process worker process    |                         | - Out-of-process worker process    |
| - Native toolchain (e.g. Java/Loom)|                         | - Native toolchain (e.g. C#/MSBuild|
| - Implements IGameAdapter traits   |                         | - Implements IGameAdapter traits   |
| - Game-specific hooks / mixins     |                         | - Game-specific hooks / CET Lua    |
+------------------------------------+                         +------------------------------------+
```

---

## 2. Intended User Personas & Core Needs

The requirements baseline addresses five distinct user personas (`USER-001` through `USER-005`):

1. **Persona 1: The Creative Mashup Creator (`USER-001`)**
   - *Profile:* Experienced gamer with visionary mashup ideas but limited low-level C++/reverse-engineering expertise.
   - *Core Needs:* Plain-language mashup description, automated feasibility assessment, guided scaffolding, one-click sandbox testing, and zero risk to existing game installs.
2. **Persona 2: The Senior Game Modder (`USER-002`)**
   - *Profile:* Highly technical mod author fluent in Java/Fabric, C#/Harmony, or C++/UE4SS.
   - *Core Needs:* Precise control over generated source code, customizable build scripts, low-overhead IPC telemetry APIs, deterministic builds, and transparent error logs.
3. **Persona 3: The Cross-Toolchain Developer (`USER-003`)**
   - *Profile:* Software engineer building multi-language pipelines across C# 12, Java 21, and native C++.
   - *Core Needs:* Clean separation of build systems (MSBuild, Gradle, CMake), stable IPC protocol contracts, decoupled dependencies, and standard CI/CLI interfaces.
4. **Persona 4: The Technical Reviewer & QA Engineer (`USER-004`)**
   - *Profile:* Engineer responsible for verifying stability, performance, and reversibility.
   - *Core Needs:* Falsifiable test plans, automated telemetry benchmarks ($\le 16.6\text{ ms}$ IPC latency, $\le 2\text{ FPS}$ impact), memory leak monitors, and verified `cleanup.ps1` rollback scripts.
5. **Persona 5: The Security & Compliance Auditor (`USER-005`)**
   - *Profile:* Systems security analyst guarding against malware, anti-cheat bans, and copyright liability.
   - *Core Needs:* Mandatory zero-elevation execution (Medium IL), Win32 Job Object quotas, verified non-distributive asset extraction (`REQ-MOD-003`), and automated hard blocks on anti-cheat-protected games (`REQ-SAFE-001`).

---

## 3. Principal User Journey (14-Stage End-to-End Workflow)

The universal application guides users through a structured 14-stage journey:

```
+----------------------------------------------------------------------------------------------------+
|                                    PRINCIPAL USER JOURNEY                                          |
+----+--------------------------------+--------------------------------------------------------------+
| 01 | Project Creation               | User initializes project workspace with stable project slug. |
| 02 | Game & Version Specification   | User selects Game A & Game B with exact platform / hash.     |
| 03 | Mashup Intent Description      | User describes desired gameplay interaction in plain text.   |
| 04 | Capability Decomposition       | Engine maps intent into atomic testable capabilities (CAP-###|
| 05 | Technical Surface Discovery    | Engine identifies mod loaders, APIs, hooks, or telemetry sinks|
| 06 | Policy & Anti-Cheat Audit      | Gate G1 audit: verifies single-player offline, zero EAC/VAC. |
| 07 | Strategy Trade-Off Evaluation  | Compares Strategy A (Host Mod), B (Bridge), and C (Hybrid).  |
| 08 | Evidence & Uncertainty Report  | Surfaces Epistemic Tiers, known constraints, and blockers.   |
| 09 | Bounded Validation Plan (PoC)  | Authorizes 20-field PoC specification at Gate G3.            |
| 10 | Human Review Sign-Off          | Mandatory human-in-the-loop review of staging diffs and plan.|
| 11 | Sandboxed Scaffolding & Exec   | AI agent scaffolds mod/bridge in <project>/staging/ sandbox. |
| 12 | Telemetry & Artifact Capture   | Quantitative performance logged (IPC latency, FPS impact).   |
| 13 | Capability Status Reporting    | Engine outputs verified status: Supported / Partial / No-Go. |
| 14 | System Rollback & Teardown     | cleanup.ps1 executes; restores pristine game file hashes.    |
+----+--------------------------------+--------------------------------------------------------------+
```

---

## 4. Product Scope Tiers & Release Boundaries

```
+----------------------------------------------------------------------------------------------------+
|                                    PRODUCT SCOPE TIERS                                             |
+---------------------+------------------------------------------------------------------------------+
| Tier                | Scope & Capability Boundaries                                                |
+---------------------+------------------------------------------------------------------------------+
| **Tier 1:           | - Universal project workspace manager & 14-question feasibility engine.      |
| Foundation (MVP)**  | - Win32 Job Object sandboxed AI staging jail & human-in-the-loop diff review.|
|                     | - Windows Named Pipes IPC transport (\\.\pipe\nexludic_*) with Protobuf / JSON|
|                     | - Strategy A (Host Mod) scaffolding for JVM (Fabric) & Unity (.NET/BepInEx).  |
|                     | - Mandatory safety hard blocks (Anti-cheat, DRM, non-distributive assets).   |
|                     | - Automated rollback verifier (cleanup.ps1).                                 |
+---------------------+------------------------------------------------------------------------------+
| **Tier 2:           | - Strategy B (Live Bridge) dual-process execution with crash containment.     |
| Validated           | - Strategy C (Hybrid Integration) composite adapter orchestration.           |
| Integration**       | - Native C++ / UE4SS and REDengine 4 CET scripting integration.             |
|                     | - Headless telemetry mock harness for multi-game hardware testing.           |
|                     | - Quantitative telemetry profiler (real-time latency histogram & frame drops)|
+---------------------+------------------------------------------------------------------------------+
| **Tier 3:           | - Advanced 3D model conversion & texture atlas re-baking pipelines.          |
| Advanced /          | - Bidirectional state synchronization with dynamic dead-reckoning lerp.      |
| Expanded**          | - Cross-platform Linux / SteamOS Proton compatibility abstractions.          |
|                     | - Automated community adapter package repository & discovery registry.       |
+---------------------+------------------------------------------------------------------------------+
```

---

## 5. Comprehensive Requirements Register

### 5.1 Functional Requirements (`REQ-F-###`)

| ID | Title | Statement | Priority | Verification | Status |
| :--- | :--- | :--- | :---: | :---: | :---: |
| `REQ-F-001` | Project Management | The platform MUST create, open, validate, and manage isolated mashup project workspaces under `<workspace_root>/projects/<slug>/`. | **Must** | Test | Accepted |
| `REQ-F-002` | Game Signature Identification | The platform MUST detect and record target executable signatures, semantic versions, and SHA-256 hashes for both Game A and Game B. | **Must** | Test | Accepted |
| `REQ-F-003` | Intent-to-Capability Mapping | The platform MUST decompose unstructured user mashup requests into structured, atomic capability requirements (`CAP-###`). | **Must** | Inspection | Accepted |
| `REQ-F-004` | 3-Strategy Evaluation | The platform MUST evaluate every proposed pair against Strategy A (Host Mod), Strategy B (Live Bridge), and Strategy C (Hybrid). | **Must** | Analysis | Accepted |
| `REQ-F-005` | 14-Question Feasibility Assessment | The platform MUST evaluate candidates against the 14 core feasibility questions (`Q1` to `Q14`) before Gate G2 approval. | **Must** | Analysis | Accepted |
| `REQ-F-006` | Multi-Toolchain Scaffolding | The platform MUST support scaffolding build projects for Java 21/Gradle, C# 12/MSBuild, and C++/CMake via decoupled drivers. | **Must** | Demonstration | Accepted |
| `REQ-F-007` | Named Pipe IPC Coordination | The platform MUST host and coordinate inter-process communication over Windows Named Pipes (`\\.\pipe\nexludic_*`). | **Must** | Test | Accepted |
| `REQ-F-008` | Staged AI Code Generation | The platform MUST confine all AI-generated code, scripts, and build artifacts strictly to `<project>/staging/`. | **Must** | Test | Accepted |
| `REQ-F-009` | Human Diff Approval Gate | The platform MUST require explicit human confirmation before deploying staged modifications to game directories. | **Must** | Test | Accepted |
| `REQ-F-010` | Automated Rollback Execution | The platform MUST generate and verify an automated `cleanup.ps1` script for every project that returns game files to pristine state. | **Must** | Test | Accepted |
| `REQ-F-011` | Telemetry & Performance Capture | The platform MUST measure and log IPC round-trip latency, frame rate deltas, and process memory utilization during execution. | **Should** | Test | Accepted |
| `REQ-F-012` | Headless Mock Testing Harness | The platform SHOULD provide a mock sender/receiver daemon to test IPC protocols without launching full commercial game clients. | **Should** | Test | Accepted |
| `REQ-F-013` | Partial Capability Reporting | The platform MUST report capability support granularly (Supported, Partial, Unsupported, Prohibited) rather than all-or-nothing. | **Must** | Inspection | Accepted |

### 5.2 Non-Functional & Quality Requirements (`REQ-NF-###`)

| ID | Title | Statement | Target Metric | Priority | Status |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `REQ-NF-001` | IPC Latency Budget | Windows Named Pipe packet round-trip time must remain within real-time 60 Hz frame budgets. | Mean $\le 4.0\text{ ms}$; 99th % $\le 16.6\text{ ms}$ | **Must** | Accepted |
| `REQ-NF-002` | Game Frame Rate Impact | In-engine hooks and telemetry serialization must not degrade game client performance. | Frame rate delta $\le 2.0\text{ FPS}$ | **Must** | Accepted |
| `REQ-NF-003` | Host Memory Working Set | Core engine background processes must maintain a constrained memory footprint. | Working set $\le 150.0\text{ MB}$ | **Must** | Accepted |
| `REQ-NF-004` | Scaffolding Build Speed | Automated CLI compilation of generated mod projects must complete promptly. | Build time $\le 60.0\text{ seconds}$ | **Should** | Accepted |
| `REQ-NF-005` | Process Crash Isolation | Abrupt termination of an adapter or secondary game must never cause a crash in the host. | $0.0\%$ unhandled host crashes | **Must** | Accepted |
| `REQ-NF-006` | Clean Rollback Integrity | File system hash verification after teardown must show zero byte-level discrepancies. | $100.0\%$ hash match | **Must** | Accepted |
| `REQ-NF-007` | Modular Assembly Decoupling| The core monolith (`NexLudic.Core`) must not reference any game-specific binary assembly. | $0$ game references | **Must** | Accepted |

### 5.3 Security, Privacy & Safety Requirements (`REQ-SEC-###`)

| ID | Title | Statement | Priority | Verification | Status |
| :--- | :--- | :--- | :---: | :---: | :---: |
| `REQ-SEC-001`| Zero Administrative Elevation | The platform, adapters, and staging tools MUST execute entirely in standard user mode (Medium IL). | **Must** | Test | Accepted |
| `REQ-SEC-002`| Anti-Cheat Hard Block | The platform MUST permanently prohibit live memory hooking, injection, or synthetic input in games with active kernel anti-cheat (`RSK-01`). | **Must** | Test | Accepted |
| `REQ-SEC-003`| Non-Distributive Asset Policy | The platform MUST NEVER bundle or redistribute proprietary game assets; all extraction must be executed locally on user files (`REQ-MOD-003`). | **Must** | Inspection | Accepted |
| `REQ-SEC-004`| Zero Kernel-Mode Drivers | The platform MUST NOT require or install kernel-mode drivers (e.g. ViGEmBus) or test-signing modes (`DEC-003`, `RSK-08`). | **Must** | Test | Accepted |
| `REQ-SEC-005`| Win32 Job Object Sandboxing | All child processes, compilers, and AI agent scripts MUST execute inside Win32 Job Objects with strict CPU/memory limits (`ADR-0009`). | **Must** | Test | Accepted |
| `REQ-SEC-006`| Offline Single-Player Enforce | The platform MUST verify that game integrations execute only in single-player or local offline modes (`RSK-02`). | **Must** | Test | Accepted |
| `REQ-SEC-007`| Local IPC Named Pipe ACLs | Named Pipes MUST enforce Windows Access Control Lists (ACLs) restricting connections exclusively to the current user SID. | **Must** | Test | Accepted |

### 5.4 Compatibility & Ecosystem Requirements (`REQ-CMP-###`)

| ID | Title | Statement | Priority | Verification | Status |
| :--- | :--- | :--- | :---: | :---: | :---: |
| `REQ-CMP-001`| Windows OS Target | The platform MUST support 64-bit Windows 10 (Build 19045+) and Windows 11 (Build 22621+). | **Must** | Demonstration | Accepted |
| `REQ-CMP-002`| JVM Runtime Baseline | The platform MUST support Java 21 LTS (OpenJDK / Eclipse Temurin) and Gradle 8.x for Minecraft Fabric. | **Must** | Demonstration | Accepted |
| `REQ-CMP-003`| .NET Runtime Baseline | The platform MUST execute on .NET 8.0 LTS and support C# 12 MSBuild for BepInEx Unity mods. | **Must** | Demonstration | Accepted |
| `REQ-CMP-004`| Native Toolchain Baseline | The platform MUST interface with MSVC v143 (C++ 20) and CMake 3.28+ for native C++ hooks. | **Should** | Demonstration | Accepted |
| `REQ-CMP-005`| Semantic Version Pinning | Every adapter project MUST pin exact semantic versions for games, mod loaders, and libraries. | **Must** | Inspection | Accepted |

---

## 6. Integration Strategy Selection Principles

The platform dynamically assigns one of three strategies based on empirical feasibility:

1. **Strategy A (Host-Game Modification):**  
   - *Condition:* Recommended when Game A possesses a mature, highly extensible modding ecosystem (e.g., Minecraft Fabric, Risk of Rain 2 BepInEx) and the requested Game B features can be replicated as native assets and code within Game A.  
   - *Advantage:* Zero concurrent game execution overhead; zero dual-process synchronization complexity; 100% offline stability.
2. **Strategy B (Live Interoperability Bridge):**  
   - *Condition:* Recommended when both games must execute simultaneously to expose unique physics, simulation state, or camera views, and both expose documented, permitted event/telemetry hooks (e.g., BakkesMod C++ in Rocket League + CET Lua in Cyberpunk 2077).  
   - *Advantage:* Preserves proprietary game physics and visual fidelity in both engines without reverse-engineering internal shaders or rendering pipelines.
3. **Strategy C (Hybrid Integration):**  
   - *Condition:* Recommended only when in-engine modding is required for local player rendering/physics while an external process provides session telemetry or companion data. Requires explicit ARB complexity justification.

---

## 7. Feasibility Decisions & Epistemic Traceability Rules

In accordance with [TECHNICAL_FEASIBILITY_FRAMEWORK.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/TECHNICAL_FEASIBILITY_FRAMEWORK.md):
- **Epistemic Tier 1 (Hypothesized):** Unverified claim. Cannot be represented as feasible without an empirical PoC.
- **Epistemic Tier 2 (Sourced):** Sourced from official SDK/API documentation.
- **Epistemic Tier 3 (Verified Community):** Documented in active, maintained open-source modding frameworks.
- **Epistemic Tier 4 (Empirically Proven):** Validated through reproducible sandbox test execution logs.
- **Decision Determinations:** Every candidate pair receives one of six formal determinations: `FEASIBLE`, `CONDITIONALLY FEASIBLE`, `BLOCKED PENDING POC`, `UNFEASIBLE (TECHNICAL)`, `UNFEASIBLE (POLICY)`, or `DEFERRED`.

---

*End of Product Requirements Specification (NLE-SRS-001).*

