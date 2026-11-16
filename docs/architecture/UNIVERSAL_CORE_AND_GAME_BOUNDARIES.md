# NexLudic Engine — Universal Core and Game-Specific Boundaries

**Document ID:** `NLE-ARC-BND-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 9 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Architecture  

---

## 1. Executive Summary & Purpose

The **Universal Core and Game-Specific Boundaries** specification defines the strict decoupling between the universal platform core (`NexLudic.Core`, `NexLudic.Domain`) and game-specific integration components (Adapters, Mod Scaffolds, IPC Bridges).

The defining architectural challenge of NexLudic Engine is **universality without homogenizing game-specific toolchains**. Rather than forcing all modding into C# or creating bespoke hardcoded bridges, the platform establishes generic capability contracts that allow game-specific components to utilize their native programming languages, compilers, and mod loaders while communicating through standardized, versioned interfaces.

```
+----------------------------------------------------------------------------------------------------+
|                                    ARCHITECTURAL RESPONSIBILITY BOUNDARIES                         |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  +----------------------------------------------------------------------------------------------+  |
|  | 1. UNIVERSAL CORE RESPONSIBILITIES (Managed C# 12 / .NET 8.0)                                |  |
|  | - Workspace Orchestration, Feasibility Evaluation, Risk Gating, AI Agent Jail, IPC Server   |  |
|  +-----------------------------------------------+----------------------------------------------+  |
|                                                  | Dynamic Adapter Loading (Out-of-Process)        |
|                                                  v                                                 |
|  +----------------------------------------------------------------------------------------------+  |
|  | 2. GAME-SPECIFIC ADAPTER RESPONSIBILITIES (Decoupled Worker Processes)                       |  |
|  | - Implements IGameAdapter traits (IModScaffoldable, ITelemetrySource, IInputTarget)          |  |
|  | - Game A: Java 21 / FabricMC (JVM)        | Game B: C# 12 / BepInEx (Unity Mono)             |  |
|  | - Game C: C++ 20 / BakkesMod (Native UE3) | Game D: Lua 5.1 / Cyber Engine Tweaks (REDengine)|  |
|  +-----------------------------------------------+----------------------------------------------+  |
|                                                  | Process Boundary (Named Pipe / In-Engine Hook)  |
|                                                  v                                                 |
|  +----------------------------------------------------------------------------------------------+  |
|  | 3. EXTERNAL GAME RUNTIME RESPONSIBILITIES (Proprietary Closed Executables)                     |  |
|  | - Game simulation loops, physics engines, internal rendering pipelines, official game servers |  |
|  +----------------------------------------------------------------------------------------------+  |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Allocation of System Responsibilities

### 2.1 Universal Core Responsibilities
The universal core (`NexLudic.Core`, `NexLudic.Domain`, `NexLudic.Infrastructure`) owns platform-wide governance and orchestration:
1. **Workspace & Project Management:** Creation, validation, persistence, and cleanup of mashup project directories (`REQ-F-001`).
2. **Feasibility Assessment Engine:** Automated execution of the 14-question evaluation framework (`Q1` to `Q14`) and calculation of Epistemic Tiers.
3. **Quality Gate Governance:** Enforcement of entry and exit criteria across Gates G0 through G6 (`NLE-FEAS-GATES-001`).
4. **AI-Agent Staging Jail:** Execution of untrusted code generation tools inside Win32 Job Objects with memory and CPU quota enforcement (`ADR-0009`).
5. **Human-in-the-Loop Diff Gate:** Mandatory presentation of file diffs and human approval before file deployment (`REQ-F-009`).
6. **Inter-Process Communication Host:** Management of local Windows Named Pipe servers (`\\.\pipe\nexludic_*`) and packet deserialization (`ADR-0003`).
7. **System Rollback Verifier:** Verification of automated `cleanup.ps1` execution and post-teardown hash validation (`REQ-F-010`).

### 2.2 Game-Specific Adapter Responsibilities
Game-specific adapters own all translation between the universal contracts and the target game's native environment:
1. **Toolchain Invocation:** Invoking game-specific compilers (e.g. `gradlew build`, `dotnet build`, `cmake --build`) within the staging sandbox.
2. **Mod File Scaffolding:** Generating game-compliant directory structures, manifest files (`fabric.mod.json`, BepInEx plugin manifests), and source templates.
3. **Native Event Hooking:** Attaching in-engine hooks to capture gameplay telemetry (e.g. Fabric tick mixin, BakkesMod `HookEvent`, CET Lua function hook).
4. **Data Normalization:** Converting game-specific coordinate frames, tick rates, and physics units into standardized SI units (meters, seconds, radians) before transmitting across the IPC pipe.
5. **Local Asset Transcoding:** Executing user-side scripts to convert proprietary local audio banks (`.awc`) or textures into open formats (`.ogg`, `.png`).

### 2.3 External Responsibilities (Outside Platform Scope)
1. **Game Process Execution:** Internal rendering loops, scene graph traversal, and physics integration remain entirely within the proprietary game executable.
2. **Mod Loader Frameworks:** Fabric, BepInEx, CET, and UE4SS maintain their own open-source release cycles and bootstrapping logic.
3. **Operating System Facilities:** Windows process scheduling, memory virtualization, and hardware driver access.

---

## 3. Compositional Adapter Trait Architecture

To avoid forcing every game into a rigid, monolithic interface, the platform utilizes **Compositional Capability Traits** (`ADR-0002`):

```csharp
// Base marker interface implemented by all game adapters
public interface IGameAdapter
{
    string AdapterId { get; }
    string GameName { get; }
    SemanticVersion SupportedGameVersion { get; }
    AdapterExecutionMode ExecutionMode { get; }
    Task<AdapterHealthReport> CheckHealthAsync(CancellationToken ct);
}

// Trait implemented by adapters capable of scaffolding host-game mods (Strategy A)
public interface IModScaffoldable
{
    Task<ScaffoldResult> ScaffoldModProjectAsync(ScaffoldContext context, CancellationToken ct);
    Task<BuildResult> CompileModAsync(StagingDirectory stagingDir, CancellationToken ct);
    Task<RollbackScript> GenerateRollbackScriptAsync(ProjectContext context, CancellationToken ct);
}

// Trait implemented by adapters that stream live telemetry over Named Pipes (Strategy B/C)
public interface ITelemetrySource
{
    string PipeEndpointName { get; }
    TelemetrySchema ProtocolSchema { get; }
    Task<bool> StartTelemetryStreamAsync(CancellationToken ct);
    Task StopTelemetryStreamAsync(CancellationToken ct);
}

// Trait implemented by adapters that accept synthesized player inputs
public interface IInputTarget
{
    Task<InputDispatchResult> SendInputAsync(SyntheticInputPacket packet, CancellationToken ct);
}

// Trait implemented by adapters supporting local asset extraction
public interface IAssetExtractable
{
    Task<ExtractionResult> ExtractLocalAssetsAsync(GameInstallPath path, CancellationToken ct);
}
```

---

## 4. Multi-Language Toolchain Participation Rules

The platform accommodates heterogeneous programming languages across game ecosystems:

| Game Ecosystem | Target Language | Native Build System | Decoupled Core Driver | Adapter Execution Model |
| :--- | :--- | :--- | :--- | :--- |
| **Minecraft Java** | Java 21 LTS | Gradle 8.x / Loom | `JavaGradleToolchainDriver` | Out-of-Process CLI Worker |
| **Unity Engine** | C# 12 | MSBuild / .NET 8 SDK | `DotNetToolchainDriver` | Out-of-Process CLI Worker |
| **Unreal Engine 3/4** | C++ 20 | MSVC v143 / CMake | `NativeCMakeToolchainDriver`| Out-of-Process CLI Worker |
| **REDengine 4** | Lua 5.1 | CET Script Engine | `LuaScriptToolchainDriver` | In-Engine Script Injection |

### Coupling Prevention Rules:
1. **Rule CPL-01:** No game-specific binary dependency may be compiled into `NexLudic.Core.dll` or `NexLudic.Domain.dll`.
2. **Rule CPL-02:** Build toolchains must be driven exclusively via non-interactive command-line interfaces (`gradlew`, `dotnet`, `cmake`) wrapped by Win32 Job Objects.
3. **Rule CPL-03:** Adapter workers must communicate with the core engine via standard JSON-RPC over stdin/stdout or local Windows Named Pipes.

---

## 5. Unresolved Limitations & Boundary Exceptions

1. **Direct Dynamic In-Memory Hooking:** Live runtime hooking of proprietary closed engines without an established mod loader remains an unresolved research topic (`RSK-04`). All initial pilots require an established mod loader (Fabric, BepInEx, CET, BakkesMod).
2. **Automated 3D Collision Mesh Conversion:** Dynamic extraction of 3D geometry from one engine and real-time injection into another is **formally excluded** from MVP boundaries due to proprietary vertex format differences.

---

*End of Universal Core and Game-Specific Boundaries (NLE-ARC-BND-001).*

