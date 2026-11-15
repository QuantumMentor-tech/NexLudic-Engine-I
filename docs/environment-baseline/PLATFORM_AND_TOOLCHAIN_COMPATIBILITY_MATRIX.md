# NEXLUDIC ENGINE — PLATFORM & TOOLCHAIN COMPATIBILITY MATRIX
## Document ID: NLE-DOC-MATRIX-001
**Phase:** 1 — Product Definition & Research  
**Step:** 3 of 100 — Development Environment Audit, Toolchain Compatibility, and Initial Game-Pair Baseline  
**Date:** November 15, 2026  
**Auditor / Architect:** Principal Software Architect & Systems Compatibility Engineer  

---

## 1. Operating System Baseline & Scope Boundaries

### 1.1 Proposed Platform: Windows 10 & 11 (x64)
- **Primary Target:** Windows desktop (x64 architecture).
- **Justification:** Over 96% of commercial PC gaming and modding ecosystems (Steam, DirectX, mod loaders, game hooks) operate primarily on Windows. Windows provides robust, low-latency inter-process communication (Named Pipes, Memory-Mapped Files) and comprehensive user-mode windowing and input APIs (`SendInput`).
- **Target OS Versions Supported:**
  - Windows 11 (Versions 21H2, 22H2, 23H2, 24H2) — Full support.
  - Windows 10 (Versions 21H2, 22H2) — Full support with .NET 8 desktop runtime.
- **Explicit Cross-Platform Boundary:**
  - Linux and macOS are **unsupported for the initial MVP**.
  - Windows-first development does not imply or guarantee cross-platform execution. Linux support via Proton/Wine or native ports introduces complex namespace and IPC translation challenges, and is deferred to post-v2.0 milestones.

---

## 2. Technology Responsibility Matrix

The following matrix documents the proposed architectural technologies, their designated subsystem responsibilities, and their governance classification:

| Technology | Proposed Subsystem Responsibility | Platform Scope | Required / Optional | Maintenance & Lifecycle Status |
| :--- | :--- | :--- | :--- | :--- |
| **C# / .NET 8.0** | Core Desktop Platform, Application Services, Discovery Engine, AI Orchestration, File Watchdogs, Safety Gates | **Universal Core** | **Required** | .NET 8 LTS (Supported through Nov 2026). Seamless upgrade path to .NET 10 LTS (Supported through Nov 2028). |
| **WPF (Windows Desktop)** | Native Windows desktop user interface, MVVM presentation layer, unified diff viewer, logs console | **Universal Core** | **Required (Desktop UI)**| Mature, native Windows UI framework bundled in `Microsoft.WindowsDesktop.App`. Zero third-party runtime bloat. |
| **Java (JDK 21+)** | Compilation runtime for Minecraft Java Edition modding projects | **Game-Specific** | **Conditional** (Minecraft only) | Required solely when generating/building Fabric or Forge mods. Strictly decoupled from core engine. |
| **FabricMC / Loom** | Mod loader, API, and Gradle plugin for Minecraft Java mod projects | **Game-Specific** | **Conditional** (Minecraft only) | Maintained by FabricMC community. Supports Minecraft 1.20+ through 1.21+. |
| **Local IPC (Named Pipes)**| High-throughput, sub-millisecond local inter-process message broker between core and bridge adapters | **Core Bridge** | **Required (Strategy B/C)**| Native Windows OS kernel transport (`\\.\pipe\...`). Ultra low latency (<2 ms), zero external dependencies. |
| **JSON & JSON Schema** | Data contracts for project blueprints (`mashup-blueprint.json`), adapter manifests (`adapter.json`), and test logs | **Universal Core** | **Required** | Portable, human-readable, machine-verifiable specification standard using `System.Text.Json`. |
| **Python (3.14+)** | Lightweight offline log analysis, telemetry visualization, and optional computer vision prototyping | **Experimental Tool** | **Optional / Deferred** | Present on dev host; strictly non-mandatory for core runtime. No packages required for MVP. |
| **Native C / C++** | Game-specific hook DLLs or low-level telemetry taps (e.g. BakkesMod SDK plugins) | **Game-Specific** | **Optional / Conditional**| Not part of core. Used only in specialized compiled adapter DLLs. |
| **SQLite (Local DB)** | Structured local caching of scanned game libraries, file checksums, and session audit history | **Core Data Layer**| **Optional / Phase 3** | File-based local storage (`System.Data.SQLite` or `Microsoft.Data.Sqlite`). No external server daemon. |
| **AI Coding Agent (CLI/API)**| Context-driven code generation, script authoring, and compiler error diagnostics under sandbox isolation | **AI Orchestrator** | **Required (Strategy A/C)**| Provider-agnostic orchestration via CLI process wrapping or Model Context Protocol (MCP). |

---

## 3. Technology Separation & Architectural Decoupling

```
+----------------------------------------------------------------------------------------------------+
|                                    TECHNOLOGY ISOLATION DOMAINS                                    |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [UNIVERSAL PLATFORM DOMAIN] (Strictly Game-Agnostic)                                              |
|  - Language: C# 12 / .NET 8.0 LTS                                                                  |
|  - Presentation: Windows Presentation Foundation (WPF) with modern Fluent styling                  |
|  - IPC Engine: System.IO.Pipes (Named Pipes) with framing protocol                                 |
|  - Data Contracts: JSON Schema (Draft 2020-12) via System.Text.Json                                |
|  - Storage: Flat-file JSON manifests & optional local SQLite database                              |
|                                                                                                    |
|  [GAME-SPECIFIC ADAPTER DOMAIN] (Pluggable, Dynamically Loaded)                                    |
|  - Minecraft Adapter: Java 21, Gradle 8.x, Fabric Loader 0.16.x, Fabric API                        |
|  - Rocket League Adapter: C++20, BakkesMod SDK headers, Windows DLL injection hooks                |
|  - Cyberpunk 2077 Adapter: Lua 5.1 / Cyber Engine Tweaks (CET) script bindings                     |
|  - Black Ops 3 Adapter: GSC scripting syntax, Steam Workshop Mod Tools                             |
|                                                                                                    |
|  [AI ORCHESTRATION & SANDBOX DOMAIN]                                                              |
|  - Execution: Windows Process Subsystem with Job Objects & restricted tokens                        |
|  - Working Directory: <project_dir>/staging/ (Strict boundary enforcement)                         |
|  - Communication: Standard stdin/stdout streaming with ANSI parsing and diff calculation          |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

---

## 4. Toolchain Version Compatibility & Maintenance Risks

### 4.1 .NET 8.0 vs. .NET 10.0 LTS Lifecycle
- **Current Baseline:** .NET 8.0.425 is verified active on the development host. Supported by Microsoft with security patches through November 10, 2026.
- **Future Migration Path:** .NET 10 LTS was released in November 2025 (supported through November 2028). Because .NET maintains strict backward compatibility for C# console and WPF projects, developing against .NET 8 guarantees immediate execution on the current workstation while allowing a zero-friction upgrade to .NET 10 in future phases.

### 4.2 Java & Gradle Version-Matching Matrix (for Minecraft Adapters)
When Minecraft Java Edition is targeted as a host game, strict version alignment is required across five distinct layers:

```
+------------------------------------------------------------------------------------+
|                        MINECRAFT FABRIC VERSION MATRIX                             |
+---------------------+-------------------+-------------------+----------------------+
| Minecraft Target    | Required JDK      | Required Gradle   | Fabric Loader / Loom |
+---------------------+-------------------+-------------------+----------------------+
| 1.20.1              | Java 17 (LTS)     | Gradle 8.1 - 8.5  | Loader 0.14+ / Loom 1.2|
| 1.20.4              | Java 17 (LTS)     | Gradle 8.3 - 8.6  | Loader 0.15+ / Loom 1.4|
| 1.20.5 - 1.20.6     | Java 21 (LTS)     | Gradle 8.6 - 8.8  | Loader 0.15+ / Loom 1.6|
| 1.21.0 - 1.21.4     | Java 21 (LTS)     | Gradle 8.8 - 8.10 | Loader 0.16+ / Loom 1.7|
+---------------------+-------------------+-------------------+----------------------+
```

- **Risk Mitigation:** The NexLudic Minecraft Adapter must not assume a single hardcoded Java version. The adapter manifest (`adapter.json`) must declare its exact required JDK version, and the engine scanner must verify local JDK compliance before initiating mod builds.

---

## 5. Technology Determinations & Open Choices

### 5.1 Confirmed Determinations
1. **Core Runtime:** C# / .NET 8.0 SDK is confirmed as the primary development platform for the universal core engine.
2. **Desktop UI Framework:** WPF (Windows Presentation Foundation) is confirmed as the native presentation baseline for the Windows desktop client.
3. **IPC Transport:** Local Named Pipes (`\\.\pipe\nexludic_*`) confirmed as the high-throughput, low-latency inter-process bridge transport.
4. **Data Formats:** JSON with strict JSON Schema contracts confirmed for all blueprints, manifests, and test outputs.

### 5.2 Open Architectural Decisions (Deferred to Step 4 & Phase 2)
1. **Agent Protocol Standard:** Evaluation between direct CLI process wrapping (spawning `claude` or `gemini` in a subshell) vs. Model Context Protocol (MCP) server integration.
2. **Local Logging Framework:** Evaluation between `Serilog` vs. native `Microsoft.Extensions.Logging` for structured file and UI streaming logs.
3. **Data Storage:** Decision on whether to introduce SQLite in Phase 2 or maintain flat-file JSON manifests for the initial MVP.

