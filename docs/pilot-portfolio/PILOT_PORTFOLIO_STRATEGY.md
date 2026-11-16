# NexLudic Engine — Pilot Portfolio Strategy

**Document ID:** `NLE-POL-PORT-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 7 of 100)  
**Status:** Approved Architecture Policy  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Game-Integration Architecture  

---

## 1. Executive Summary & Strategic Purpose

The **Pilot Portfolio Strategy** defines the methodology for selecting, structuring, evaluating, and sequencing representative game pairs to validate the universal integration architecture of **NexLudic Engine**.

NexLudic Engine is not a fixed bridge between two specific games (e.g., Minecraft and Spider-Man), nor is it a bespoke mod-launcher for a single title. It is a universal AI-assisted desktop platform designed to help users evaluate, design, scaffold, test, and manage cross-game mashups across fundamentally different PC games, engines, and modification ecosystems.

A single flagship demo cannot validate a universal platform. Demonstrating that an AI agent can inject a custom weapon into Minecraft proves only that Minecraft is moddable; it proves nothing about whether the same core architecture can interface with Unreal Engine, coordinate live telemetry with a proprietary simulation engine, or isolate out-of-process worker crashes.

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC ENGINE CORE RUNTIME                                    |
|                (Universal Monolith Core: Evaluation, Staging, Scaffolding, Governance)              |
+-------------------------------------------------+--------------------------------------------------+
                                                  |
                        +-------------------------+-------------------------+
                        |                                                   |
                        v                                                   v
     +------------------------------------+              +------------------------------------+
     |    STRATEGY A: HOST-GAME MOD       |              |    STRATEGY B: LIVE IPC BRIDGE     |
     |  (In-Engine Execution / Extensibility) |              | (Dual-Process Event Synchronization) |
     +-----------------+------------------+              +-----------------+------------------+
                       |                                                   |
                       v                                                   v
     +------------------------------------+              +------------------------------------+
     |      PILOT ARCHETYPE A & B         |              |         PILOT ARCHETYPE C          |
     | A: JVM / Fabric (Minecraft + RDR2) |              | Named Pipe / CET / BakkesMod       |
     | B: Mono / BepInEx (Unity / ROR2)   |              | (Rocket League + Cyberpunk 2077)   |
     +------------------------------------+              +------------------------------------+
                                                  |
                                                  v
                                 +------------------------------------+
                                 |    STRATEGY C: HYBRID INTEGRATION  |
                                 |        (PILOT ARCHETYPE D)         |
                                 | In-Engine Mod + Out-of-Process IPC |
                                 +------------------------------------+
```

---

## 2. Why NexLudic Engine Requires a Representative Portfolio

The pilot portfolio is an engineering validation suite designed to stress-test the universal platform. It must systematically answer ten core architectural questions:

1. **Workflow Universality:** Can the platform apply a single, uniform feasibility evaluation workflow ([TECHNICAL_FEASIBILITY_FRAMEWORK.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/TECHNICAL_FEASIBILITY_FRAMEWORK.md)) to radically different game pairs without tailoring the evaluation rules to a specific game?
2. **Surface Heterogeneity:** Can the platform accurately discover and represent the fundamentally divergent integration surfaces exposed by different games (e.g., JVM bytecode mixins vs. native DLL injection vs. memory-mapped telemetry)?
3. **Capability Distinction:** Can the platform cleanly distinguish content adaptation (textures/models) from gameplay mechanic replication, event-based RPCs, state transfer, and continuous telemetry synchronization?
4. **Toolchain & Language Independence:** Can the modular architecture support game-specific toolchains (Java/Gradle, C#/MSBuild, C++/CMake, Lua) without polluting the core .NET 8.0 platform with game-specific SDK dependencies?
5. **Contract Extensibility:** Can the core adapter interfaces (`IGameAdapter`, `IModScaffoldable`, `ITelemetrySource`, `IInputTarget`) represent different integration styles without architectural refactoring?
6. **Risk-Gated Rejection:** Can the validation gates (Gates G0–G6) catch infeasible, unstable, or policy-violating requirements early, saving expensive engineering effort?
7. **Transparent Partial Support:** Can the platform clearly articulate and enforce partial compatibility boundaries rather than making binary all-or-nothing claims?
8. **Graceful Fault Containment:** When a game or adapter crashes or exceeds memory quotas, does the out-of-process architecture contain the failure without destabilizing the host engine or operating system?
9. **Knowledge Decoupling:** Does a successful pilot create reusable scaffolding components and adapter traits rather than embedding game-specific hacks into core platform assemblies?
10. **Evidence-Based Expansion:** What concrete empirical evidence is required before the engine can declare support for an entire engine family (e.g., all Unreal Engine 4 titles or all Unity Mono titles)?

### The Four Tiers of Portfolio Purpose

A portfolio can be designed with four distinct intents. NexLudic Engine explicitly rejects Tier 1 and demands Tiers 2 through 4:

| Portfolio Level | Description | Engineering Validity for NexLudic |
| :--- | :--- | :--- |
| **Tier 1: Demo Collection** | Showcasing several visually impressive mods using the exact same underlying mechanism. | **REJECTED.** Produces marketing artifacts without validating architectural universality. |
| **Tier 2: Technique Testing** | Validating the three distinct technical strategies: Strategy A (Host Mod), Strategy B (Live Bridge), and Strategy C (Hybrid). | **MANDATORY.** Proves the engine can handle different structural integration topologies. |
| **Tier 3: Architectural Extensibility** | Validating adapter isolation, multi-language scaffolding, out-of-process IPC, and crash isolation. | **MANDATORY.** Proves that new game ecosystems can be integrated without modifying the core monolith. |
| **Tier 4: Compatibility Generalization** | Generating empirical evidence to establish defensible compatibility boundaries across game engine archetypes. | **STRATEGIC GOAL.** Enables the platform to generalize across broad game categories. |

---

## 3. Core Selection Principles & Mandatory Exclusions

### 3.1 Selection Principles

1. **Evidence Over Intuition:** Every candidate pair must be backed by documented extension surfaces, active community tooling, and clear technical evidence. Unverified claims are relegated to research backlogs.
2. **Technological Orthogonality:** Each shortlisted pilot must introduce at least one distinct technical dimension (engine runtime, programming language, communication bus, or modification surface).
3. **Falsifiable Hypotheses:** Every pilot must be structured around a single, measurable engineering hypothesis with predeclared success and failure thresholds.
4. **Sandboxed Reversibility:** Every pilot must operate in an isolated sandbox and provide a verified, automated rollback script (`cleanup.ps1`) that leaves zero residual files on the host system.
5. **Honest Scope Bounding:** The scope of a pilot must be the absolute minimum necessary to prove the technical hypothesis. Cosmetic polishing, complex level design, and extraneous assets are strictly excluded.

### 3.2 Mandatory Exclusion Rules (Hard Blockers)

Any candidate game pair exhibiting any of the following characteristics is **immediately disqualified** from the pilot shortlist:

- ❌ **Anti-Cheat Violation (`RSK-01`):** Requires memory hooking, process injection, or synthetic input in a game protected by active kernel-level anti-cheat (Easy Anti-Cheat, BattlEye, Vanguard, Ricochet) or running in an online multiplayer session (`REQ-SAFE-001`).
- ❌ **Proprietary Asset Redistribution (`RSK-06`):** Requires redistributing copyrighted game binaries, models, audio, or textures. All asset extraction must be performed locally via user-owned files (`REQ-MOD-003`, `DEC-006`).
- ❌ **Kernel-Mode Driver Dependency (`RSK-08` / `RSK-10`):** Requires kernel-mode drivers (such as the archived ViGEmBus), unsigned driver loading, or disabling Windows Driver Signature Enforcement (`DEC-003`).
- ❌ **Administrative Elevation (`RSK-09`):** Requires running games, adapters, or scaffolding tools with elevated administrative privileges (`Run as Administrator`).
- ❌ **DRM Circumvention (`RSK-05`):** Requires tampering with, cracking, or bypassing commercial DRM systems (Denuvo, SteamStub, Arxan, VMProtect).
- ❌ **Unbounded Maintenance Sink (`RSK-03`):** Relies exclusively on volatile, undocumented memory offsets in games receiving weekly binary patches without a stable mod loader abstraction.
- ❌ **Portfolio Redundancy:** Replicates the exact engine, language, and integration strategy of an already qualified candidate without providing new architectural learning.

---

## 4. Portfolio Dimension Model

To ensure diversity, candidates are classified across seven orthogonal engineering dimensions:

```
+----------------------------------------------------------------------------------------------------+
|                                    SEVEN PORTFOLIO DIMENSIONS                                      |
+----------------------------------------------------------------------------------------------------+
|  1. Game Technology Diversity   : JVM (Java) vs. Mono/.NET (C#) vs. Native C++ vs. Scripting (Lua) |
|  2. Integration Approach        : Strategy A (Host Mod) vs. Strategy B (Live Bridge) vs. C (Hybrid)|
|  3. Capability Diversity        : Content Import vs. Mechanic Simulation vs. RPC Telemetry vs. Input|
|  4. Runtime & Process Topology  : Single In-Process vs. Dual Out-of-Process vs. Headless Daemon    |
|  5. Operational & Permissions   : Standard User Mode (Medium IL) vs. Win32 Job Object Limits       |
|  6. Maintenance & Patch Exposure: Stable Mod API vs. Semi-Stable Decompilation vs. Offset Drift    |
|  7. Evidence & Epistemic Tier   : Tier 2 (Official SDK) vs. Tier 3 (Maintained Community Tools)    |
+----------------------------------------------------------------------------------------------------+
```

### 4.1 Game Technology Diversity
The portfolio must encompass diverse runtime environments:
- **Java Virtual Machine (JVM):** Java 21, bytecode manipulation, mixins, custom classloaders (e.g., Minecraft / FabricMC).
- **Mono / Common Language Runtime (.NET):** Unity C#, Harmony runtime patching, BepInEx (e.g., Risk of Rain 2, Cities: Skylines, Valheim).
- **Native Unmanaged (C / C++):** Native compiled code, DLL loading, assembly hooking, Unreal Engine / REDengine / custom engines (e.g., Cyberpunk 2077 / REDengine, Marvel's Spider-Man / Insomniac Engine, Rocket League / UE3).
- **Embedded Scripting:** Lua, Python, or proprietary domain-specific scripting engines (e.g., Cyber Engine Tweaks Lua, GTA ScriptHook).

### 4.2 Integration-Approach Diversity
The portfolio must rigorously test the three core architectural strategies:
- **Strategy A (Host-Game Modification):** Game B mechanics/assets are adapted and executed directly within Game A's engine. Game B does not run concurrently.
- **Strategy B (Live Interoperability Bridge):** Game A and Game B run concurrently as independent processes, exchanging telemetry and events over local Windows Named Pipes.
- **Strategy C (Hybrid Integration):** Game A executes custom in-engine mod logic while maintaining an active IPC telemetry link with Game B or an external helper daemon.

### 4.3 Capability Diversity
The portfolio must exercise diverse functional capabilities:
- **Content & Asset Adaptation:** Mesh conversion, texture atlas repacking, sound event registration.
- **Gameplay Mechanic Recreation:** Custom physics solvers, raycasting, state machines, tick rate dilation.
- **Event & Telemetry Streaming:** 60 Hz coordinate streaming, discrete event dispatch (kills, goals, damage).
- **Input Forwarding:** Synthesized keyboard/mouse inputs via Win32 `SendInput` or in-engine event queues.
- **State Synchronization:** Bidirectional entity position lerping and health synchronization.

---

## 5. Candidate Discovery & Screening Process

Candidate discovery follows a disciplined 5-stage screening pipeline:

```
+------------------+     +------------------+     +------------------+     +------------------+     +------------------+
|     STAGE A      |     |     STAGE B      |     |     STAGE C      |     |     STAGE D      |     |     STAGE E      |
|    Scope & ID    | --> | Technical Surface| --> | Feasibility &    | --> |    Portfolio     | --> |    Readiness     |
|   Completeness   |     |    Discovery     |     |   Safety Review  |     |   Contribution   |     |  Classification  |
+------------------+     +------------------+     +------------------+     +------------------+     +------------------+
```

### Stage A: Identity & Scope Completeness
- Game A and Game B are identified by exact executable names, platforms (Steam, GOG, Epic, Standalone), and target version numbers.
- Proposed gameplay mashup is bounded to concrete, observable interactions (e.g., "Trigger visual screen distortion in Game B upon scoring in Game A") rather than vague aspirational goals ("Combine both games into one").

### Stage B: Technical Surface Discovery
- Extension surfaces are documented: official modding SDKs, established community loaders (Fabric, Forge, BepInEx, Reloaded.II, CET), or debug event hooks.
- Toolchains, compilers, and dependencies are identified and checked for active maintenance.

### Stage C: Feasibility & Safety Review
- Screened against `FEASIBILITY_RISK_REGISTER.md` and the 6 Mandatory Exclusion Rules.
- Zero kernel-mode anti-cheat interference; zero administrative elevation; zero proprietary asset bundling.

### Stage D: Portfolio Contribution
- Evaluated for architectural novelty: Does this candidate test a new engine runtime, language, or communication topology not covered by existing pilots?

### Stage E: Readiness Classification
Assigned one of six formal readiness states:
1. **Candidate Identified:** Initial concept documented; technical surface unverified.
2. **Research Qualified:** Technical surfaces, versions, and toolchains documented with credible evidence.
3. **Conditionally Suitable:** Feasible in principle; gated by specific hardware, toolchain, or scope constraints.
4. **Pilot Shortlist:** Recommended for formal Gate G3 PoC specification and ARB authorization.
5. **Deferred:** Technically valid, but temporarily postponed due to resource constraints or portfolio redundancy.
6. **Rejected:** Permanently disqualified due to policy, security, anti-cheat, or architectural incompatibility.

---

## 6. Pilot Archetype Framework

The pilot portfolio is structured into four core archetypes plus an optional research challenge:

```
+----------------------------------------------------------------------------------------------------+
|                                    PILOT ARCHETYPE SPECIFICATIONS                                  |
+-------------------+-----------------------------------+--------------------------------------------+
| Archetype         | Focus & Engineering Hypothesis    | Representative Pilot Candidate             |
+-------------------+-----------------------------------+--------------------------------------------+
| **Archetype A**   | Bounded, documentation-rich host  | **Pilot 1:** Minecraft Java (Fabric 1.21)  |
|                   | modding in managed runtime (JVM). | + Red Dead Redemption 2 (Western Mechanics)|
+-------------------+-----------------------------------+--------------------------------------------+
| **Archetype B**   | Managed C# / Unity ecosystem with | **Pilot 2:** Risk of Rain 2 (BepInEx/Unity)|
|                   | distinct compiler & hook model.   | + Call of Duty Zombies (Wave Survival)     |
+-------------------+-----------------------------------+--------------------------------------------+
| **Archetype C**   | Dual out-of-process live bridge   | **Pilot 3:** Rocket League (BakkesMod/C++) |
|                   | over local Named Pipe IPC.        | + Cyberpunk 2077 (CET / Lua)               |
+-------------------+-----------------------------------+--------------------------------------------+
| **Archetype D**   | Hybrid integration: host mod +    | **Pilot 4:** Minecraft Java (Fabric)      |
|                   | live telemetry bridge to external.| + Marvel's Spider-Man Remastered (Physics) |
+-------------------+-----------------------------------+--------------------------------------------+
| **Research R&D**  | Advanced native hooking in closed | **Research R1:** Source Engine (TF2)       |
|                   | proprietary engines (Gated).      | + Unreal Engine 4 (Deep Rock Galactic)     |
+-------------------+-----------------------------------+--------------------------------------------+
```

### Archetype A: Bounded, Documentation-Rich Host Modification
- **Objective:** Validate the core engine's scaffolding and mod-packaging pipeline in a mature, highly documented, managed runtime environment.
- **Key Characteristics:** Single running game process; managed execution (JVM); stable, mature mod loader (FabricMC); zero concurrent process overhead; zero anti-cheat risk.
- **Hypothesis Validated:** The universal core can generate valid build scripts, source scaffolding, and asset configurations for an extensible host game without manual developer intervention.

### Archetype B: Distinct Engine Runtime & Adapter Language (Unity / C#)
- **Objective:** Prove language and toolchain independence by executing an integration within a completely different game engine (Unity) using C# and Harmony bytecode patching.
- **Key Characteristics:** Unity Engine / Mono runtime; BepInEx mod framework; MSBuild / C# 12 compilation toolchain; proves that the platform is not coupled to Java or Minecraft.
- **Hypothesis Validated:** The core adapter interface can control a .NET/Mono build pipeline and scaffold Harmony patches with the same architectural lifecycle used for Fabric mods.

### Archetype C: Live Interoperability Bridge (Out-of-Process IPC)
- **Objective:** Validate real-time, low-latency inter-process communication between two concurrently running games built on disparate native engines.
- **Key Characteristics:** Two separate operating system processes; native unmanaged runtimes (Unreal Engine 3 custom + REDengine 4); Windows Named Pipe transport; asynchronous telemetry streaming; fault containment under process termination.
- **Hypothesis Validated:** The platform can establish a resilient, bi-directional IPC telemetry bridge between two separate running games with round-trip latency $\le 16.6\text{ ms}$ without destabilizing either game loop.

### Archetype D: Hybrid Integration (Host Mod + Telemetry Bridge)
- **Objective:** Validate combined in-engine gameplay mechanics with external helper or session telemetry synchronization.
- **Key Characteristics:** Host game runs a custom internal mod; guest game (or helper daemon) communicates via local IPC; combines Strategy A and Strategy B.
- **Hypothesis Validated:** The platform can orchestrate a dual-channel integration where rendering/physics occur inside the host while state events are coordinated externally.

---

## 7. Pilot Sequencing Principles

The pilot portfolio must be executed in a strict conditional progression to retire foundational risks before introducing compounding complexity:

```
+----------------------------------------------------------------------------------------------------+
|                                    PILOT VALIDATION PROGRESSION                                    |
+----------------------------------------------------------------------------------------------------+
|  Phase 1: Bounded Host Scaffolding (Pilot 1: Fabric / JVM)                                         |
|    - Validates: Staging jail, build automation, file rollback, human-in-the-loop review.          |
+-------------------------------------------------+--------------------------------------------------+
                                                  | Gate G4 Cleared
                                                  v
|  Phase 2: Multi-Toolchain Extensibility (Pilot 2: BepInEx / Unity C#)                              |
|    - Validates: Toolchain independence, C# / MSBuild scaffolding, adapter contract consistency.   |
+-------------------------------------------------+--------------------------------------------------+
                                                  | Gate G4 Cleared
                                                  v
|  Phase 3: Out-of-Process IPC Bridge (Pilot 3: Named Pipe / BakkesMod / CET)                        |
|    - Validates: IPC latency, Win32 Job Object sandbox, connection loss resilience, fault isolation.|
+-------------------------------------------------+--------------------------------------------------+
                                                  | Gate G4 Cleared
                                                  v
|  Phase 4: Hybrid Integration Architecture (Pilot 4: In-Engine Mod + Out-of-Process Telemetry)      |
|    - Validates: Composite adapter coordination, cross-game state synchronization.                  |
+----------------------------------------------------------------------------------------------------+
```

### Fallback & Pivot Rules
- If Pilot 1 (Fabric/JVM) fails, the platform core cannot reliably scaffold build systems. Stop and refactor `NexLudic.Core.Scaffolding`.
- If Pilot 2 (BepInEx/Unity) fails due to toolchain issues, verify whether the adapter contract is overly JVM-centric and refactor adapter abstraction before attempting live bridges.
- If Pilot 3 (Live Bridge) encounters performance or memory limits on dev hardware, pivot to headless telemetry mock testing before re-attempting dual-game execution.

---

## 8. Cross-Pilot Success & Acceptance Metrics

Portfolio performance is measured through quantitative engineering metrics across four categories:

| Metric Category | Target Identifier | Metric Definition | Target Threshold | Validation Phase |
| :--- | :--- | :--- | :--- | :--- |
| **Architectural Universality** | `MET-ARCH-01` | Number of distinct engine ecosystems supported | $\ge 3$ (JVM, Unity, REDengine/Native) | Pilot Phase 1–3 |
| **Language Independence** | `MET-ARCH-02` | Number of distinct adapter toolchains exercised | $\ge 3$ (Java/Gradle, C#/MSBuild, Lua/CET) | Pilot Phase 1–3 |
| **IPC Performance** | `MET-PERF-01` | 99th percentile round-trip IPC latency | $\le 16.6\text{ ms}$ (Target $\le 4.0\text{ ms}$) | Pilot Phase 3 |
| **Process Stability** | `MET-STAB-01` | Host crash rate upon guest termination | $0.0\%$ (Zero unhandled host crashes) | Pilot Phase 3 |
| **System Hygiene** | `MET-HYG-01` | File rollback cleanliness after `cleanup.ps1` | $100.0\%$ (Zero orphaned files/registry) | All Pilots |
| **Safety Compliance** | `MET-SAFE-01` | Anti-cheat or DRM circumvention violations | $0$ (Zero policy violations) | All Pilots |

---

## 9. Governance & Review Authority

- **Architecture Review Board (ARB):** Retains sole authority to approve pilot selection, authorize Gate G3 PoC specifications, and clear Gate G4 validation results.
- **Human-in-the-Loop Requirement:** No AI agent may authorize, compile, or execute a pilot integration without explicit written review and sign-off by a designated human engineer.
- **Currency & Maintenance:** This strategy document must be reviewed and re-baselined at the conclusion of each project phase.

---

*End of Pilot Portfolio Strategy (NLE-POL-PORT-001).*

