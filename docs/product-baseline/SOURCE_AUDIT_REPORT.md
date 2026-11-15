# NEXLUDIC ENGINE — PRODUCT BASELINE AUDIT
## Document ID: NLE-DOC-AUDIT-001
**Phase:** 1 — Product Definition & Research  
**Step:** 1 of 100 — Universal Product Source Audit, Evidence Verification, and Requirements Reconciliation  
**Audit Date:** November 15, 2026  
**Auditor:** Senior Software Architect, Game-Integration Engineer, AI-Assisted Development Systems Architect, Requirements Analyst, Technical Researcher, Security Engineer & QA Lead  
**Workspace Location:** `D:\App Winrar Files\Nexludic Engine (working)`  
**Product Name:** NexLudic Engine  
**Product Category:** Universal AI-Assisted Game Mashup & Interoperability Platform  

---

## 1. Executive Summary & Audit Scope

This document establishes the initial source audit, workspace inspection, toolchain baseline, and evidence verification for **NexLudic Engine**.

### 1.1 Purpose
The purpose of this audit is to provide a grounded, traceable, and empirically verified foundation for NexLudic Engine as a **reusable platform for creating diverse PC game mashups** assisted by AI coding agents. This audit establishes clear boundaries between:
1. Requirements explicitly established by the project owner.
2. Conceptual ideas and proposals in historical specification drafts (GameWeaver Bridge QPIS and Spider-Craft AI QPIS).
3. Narrated workflows in community demonstration transcripts (Claude Code + Melty mashups).
4. Independently verified technical facts and ecosystem capabilities.
5. Engineering recommendations and experimental options requiring future validation.

### 1.2 Non-Goals of this Step
- No code generation, skeleton implementation, or application compilation.
- No third-party dependency installation or environment modification.
- No direct interaction with or modification of installed PC games or game processes.
- No speculative design hardcoding to a specific two-game pair.

---

## 2. Workspace & Repository State Inspection

A comprehensive read-only audit of the working directory was conducted on November 15, 2026.

### 2.1 Workspace Findings
- **Current Directory:** `D:\App Winrar Files\Nexludic Engine (working)`
- **Filesystem State:** The workspace contains no prior application source code, configuration manifests, build scripts, or modding assets.
- **Pre-existing Files Found:**
  - `NexLudic Engine Neon Logo Set.png` (502,964 bytes, SHA-256: `99F53DB612233D6D3E2973DCCFB5558A3E889B745127F13962B201403ADD64F4`): High-resolution branding asset depicting the neon planetary/orbital "N" logo of NexLudic Engine.
  - User uploaded asset: `media_1794755036384_47a48f7e.png` (595,634 bytes, SHA-256: `732266CA4C066AFF012D078A242A8EF4A248494259F35F9E8BE0DDA07068ADB7`): Matching official logo artwork for the project.
- **Git Repository State:**
  - `git status` check: Resulted in `fatal: not a git repository (or any of the parent directories): .git`.
  - In strict compliance with Section 13 of the project directives, Git initialization was **not performed** during this read-only audit step.

### 2.2 Host Environment & Toolchain Discovery
Read-only probes were executed across the Windows host environment to identify pre-existing runtimes and development tools:

| Tool / Runtime | Command Executed | Version Detected | Operational Status | Notes |
| :--- | :--- | :--- | :--- | :--- |
| **Git** | `git --version` | `2.56.0.windows.4` | Verified Installed | Available in system `PATH`. |
| **.NET SDK** | `dotnet --version` | `8.0.425` (LTS) | Verified Installed | .NET 8.0 SDK available; suitable for cross-platform desktop / C# architecture. |
| **Node.js** | `node -v` | `v26.8.2` | Verified Installed | Available in system `PATH`. |
| **npm** | `cmd /c npm -v` | `11.19.1` | Verified Installed | Available via `npm.cmd` (PowerShell `.ps1` restricted by script execution policy). |
| **Python** | `python --version` | `3.14.7` | Verified Installed | Standard Python runtime available in `PATH`. |
| **Java JDK** | `java -version` | Not Found | Not in `PATH` | Neither standard JDK nor JRE detected in `Program Files` or environment path. |
| **Gradle** | `gradle -v` | Not Found | Not in `PATH` | Build tool for JVM/Fabric modding not globally installed. |
| **Rust / Cargo** | `rustc --version` | Not Found | Not in `PATH` | Native compilation toolchain not installed. |
| **VS Code CLI** | `code --version` | Not Found | Not in `PATH` | CLI shim not present in `PATH`. |

---

## 3. Authoritative Source Documents Audit

The audit evaluated four core sources referenced in the project mandate.

### 3.1 Inventory & Access Audit

```
+----------------------------------------------------------------------------------------------------+
|                                    SOURCE INVENTORY STATUS TABLE                                   |
+-------------------+------------------------------------------+-----------------+-------------------+
| Identifier        | Expected File / Document Title           | Location State  | Evaluation Method |
+-------------------+------------------------------------------+-----------------+-------------------+
| Source A          | GameWeaver_Bridge_QPIS_v1.0(1).md        | Absent on disk  | Prompt Spec Basis |
| Source B          | spider_craft_ai_qpis_specification.md    | Absent on disk  | Prompt Spec Basis |
| Source C          | Pasted text(20261011-110220).txt         | Absent on disk  | Prompt Spec Basis |
| Source D          | Project Owner Direct Mandate             | Active Prompt   | Primary Directive |
| Branding / Media  | NexLudic Engine Neon Logo Set.png        | Present in Repo | File & Image Hash |
+-------------------+------------------------------------------+-----------------+-------------------+
```

#### Detailed Document Analysis

#### Source A: GameWeaver Bridge QPIS (`GameWeaver_Bridge_QPIS_v1.0(1).md`)
- **Physical Availability:** Not present as a standalone file in the local filesystem.
- **Substantive Scope Analyzed:** Detailed architecture for an inter-process bridge connecting Minecraft Java Edition and Marvel's Spider-Man Remastered.
- **Key Concepts Extracted:**
  - Event extraction from Minecraft (e.g., Fabric mod capturing block placements, player jumps, swing actions).
  - Local IPC mechanisms (named pipes, localhost sockets).
  - Deterministic mapping engine translating player actions into target game inputs.
  - Desktop controller and bridge orchestration daemon.
  - Adapter health monitoring, keep-alive heartbeats, and emergency kill switches (E-stop).
  - Safety, logging, privacy, and defensive process isolation.
- **Critical Audit Distinction:** Valuable for its safety architecture, fail-safe mechanisms, and IPC bridge patterns, but fundamentally constrained to a single, bespoke pair. Must be generalized into a pluggable adapter contract for NexLudic Engine.

#### Source B: Gemini Spider-Craft AI QPIS (`spider_craft_ai_qpis_specification.md`)
- **Physical Availability:** Not present as a standalone file in the local filesystem.
- **Substantive Scope Analyzed:** An ambitious real-time synchronization specification between Minecraft and Marvel's Spider-Man.
- **Key Concepts Extracted:**
  - Real-time voxel terrain extraction and 3D coordinate frame translation.
  - Dynamic anchor point generation for Spider-Man web-swinging based on Minecraft geometry.
  - Virtual gamepad emulation (ViGEmBus / vJoy).
  - Computer vision and OCR screen observation for game state tracking.
  - AI-assisted real-time telemetry adjustment.
- **Critical Audit Distinction:** Contains numerous unverified technical assumptions, including direct memory reading/writing without anti-cheat impact, sub-16 ms cross-process world geometry rendering, and reliance on deprecated kernel drivers (ViGEmBus). Classified largely as *Experimental* and *Unverified* pending empirical validation.

#### Source C: YouTube Demonstration Transcript (`Pasted text(20261011-110220).txt`)
- **Physical Availability:** Not present as a standalone file in the local filesystem.
- **Substantive Scope Analyzed:** Transcript describing a creator using Claude Code and Melty to generate working game mashups:
  1. Minecraft with Red Dead Redemption 2.
  2. Minecraft with Call of Duty: Black Ops 3.
  3. Rocket League with Cyberpunk 2077.
- **Key Concepts Extracted:**
  - Workflow: Identify installed games -> Prompt engineering / requirement drafting -> Clarification questionnaire -> Coding agent file generation -> In-game testing -> Iterative debugging.
  - Architecture: Primary reliance on **host-game modification** (e.g., creating a Minecraft mod that adds RDR2 or BO3 weapons, UI, or mechanics, or modifying Cyberpunk/Rocket League using community mod loaders).
- **Critical Audit Distinction:** Demonstrates that the host-game modding strategy produces immediate, compelling gameplay results when supported modding toolchains exist. However, it does not prove universal compatibility across any arbitrary game pairing.

#### Source D: Project Owner Direct Mandate
- **Physical Availability:** Fully available and active within the current system prompt.
- **Primary Directives:**
  - Official product name: **NexLudic Engine**.
  - General-purpose platform: Must support creating mashups across diverse PC game pairs.
  - Not locked to Minecraft, Spider-Man, RDR2, BO3, Cyberpunk, or Rocket League.
  - Implementation roadmap: 100 sequential steps organized into 12 phases.
  - Rigorous engineering standards: Sequential execution, explicit boundaries, no speculative coding, zero regression.

---

## 4. Evidence Verification & Technical Research Findings

To ground the platform's architectural choices in current, verified engineering reality, external technical research was conducted.

### 4.1 Input Emulation & Virtual Gamepads (ViGEmBus Research)
- **Source:** [nefarius/ViGEmBus on GitHub](https://github.com/nefarius/ViGEmBus)
- **Verified Date:** November 2026 (Historical archive date: November 2023).
- **Direct Findings:**
  - ViGEmBus was officially **retired and archived** by its author (*nefarius*) in late 2023.
  - ViGEmBus requires a Windows kernel-mode driver (`.sys`) with administrative installation privileges.
  - Kernel driver installation creates significant security, stability, and anti-cheat risks.
- **Implications for NexLudic Engine:** Virtual input emulation cannot be assumed as a universal, lightweight mechanism. Desktop-level user-mode input (`SendInput`, virtual HID, or game-specific mod APIs) must be preferred over kernel driver dependencies.

### 4.2 Game Modding Frameworks Research
- **Minecraft Java Edition:**
  - **Toolchain:** FabricMC (`fabric-loader`, `fabric-api`, `fabric-loom`).
  - **Verified Findings:** Well-documented Java/Gradle ecosystem. Allows comprehensive event extraction, custom entity injection, rendering hooks, and network/IPC integration.
- **Rocket League:**
  - **Toolchain:** BakkesMod SDK (`bakkesmodorg/BakkesModSDK` on GitHub).
  - **Verified Findings:** C++ plugin architecture attaching to Rocket League process. Provides event hooking, game state manipulation, and custom UI rendering without violating game terms in offline/custom matches.
- **Cyberpunk 2077:**
  - **Toolchain:** Cyber Engine Tweaks (CET), REDmod, Redscript.
  - **Verified Findings:** CET provides an embedded Lua runtime and ImGui overlay, hooking the REDengine 4 pipeline for scripting and event handling.
- **Call of Duty: Black Ops 3:**
  - **Toolchain:** Official Steam Mod Tools (Radiant, APE, GSC scripting engine).
  - **Verified Findings:** Official modding support exists on PC, allowing custom maps, weapons, and game modes via GSC scripts.
- **Marvel's Spider-Man Remastered:**
  - **Toolchain:** Community modding tools (SMPC Tool, asset archives).
  - **Verified Findings:** Primarily supports asset, mesh, texture, and suit replacement. Does *not* have an official C++/scripting SDK or documented API for real-time external telemetry. Live interoperability requires either reverse-engineered DLL hooking or synthetic desktop input.

### 4.3 Windows Game Discovery Research
Game installations on Windows are distributed across multiple store ecosystems, each requiring dedicated discovery strategies:
1. **Steam:** Reads `libraryfolders.vdf` and `appmanifest_<appid>.acf` files in plain text VDF format. Provides installation path, app ID, build ID, and executable name deterministically.
2. **Windows Registry:** Keys in `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` and `HKCU` counterpart provide display names, install locations, and uninstall strings.
3. **Epic Games Store:** JSON manifests stored in `C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests\*.item`.
4. **GOG Galaxy:** Registry keys under `HKLM\SOFTWARE\GOG.com\Games\<game_id>`.
5. **Xbox / Microsoft Store (UWP/MSIX):** Packaged via `Windows.Management.Deployment.PackageManager`. Heavily sandboxed with restricted filesystem access (`WindowsApps`), requiring specialized permission considerations.

---

## 5. Universal Product Boundaries & Core Principles

Based on the audit, the following engineering principles govern the NexLudic Engine architecture:

```
+-----------------------------------------------------------------------------+
|                     NEXLUDIC ENGINE ARCHITECTURE LAYERS                     |
+-----------------------------------------------------------------------------+
|                                                                             |
|   +---------------------------------------------------------------------+   |
|   |                       NEXLUDIC UNIVERSAL CORE                       |   |
|   |  - Discovery Engine (Registry/Steam/Epic/GOG/Custom)                |   |
|   |  - Mashup Specification & Clarification Engine                      |   |
|   |  - AI Coding Agent Orchestrator & Sandboxed Workspace Manager        |   |
|   |  - Universal Adapter Contract & Registry                            |   |
|   |  - Validation, Build, Test & Recovery Engine                        |   |
|   |  - Safety, Access Control & Emergency Stop (E-Stop)                 |   |
|   +---------------------------------------------------------------------+   |
|                                      |                                      |
|            +-------------------------+-------------------------+            |
|            |                                                   |            |
|   +-----------------------+                           +-----------------+   |
|   | STRATEGY A:           |                           | STRATEGY B:     |   |
|   | Host-Game Mod Project |                           | Live IPC Bridge |   |
|   | (Fabric/CET/GSC/etc.) |                           | (Pipes/Sockets) |   |
|   +-----------------------+                           +-----------------+   |
|            |                                                   |            |
|            +-------------------------+-------------------------+            |
|                                      |                                      |
|   +---------------------------------------------------------------------+   |
|   |                     GAME ADAPTER IMPLEMENTATIONS                    |   |
|   |  [Minecraft Adapter]   [Spider-Man Adapter]   [Cyberpunk Adapter]   |   |
|   |  [RDR2 Adapter]        [BO3 Adapter]          [RocketLeague Adapter]|   |
|   +---------------------------------------------------------------------+   |
|                                                                             |
+-----------------------------------------------------------------------------+
```

1. **Universal Core, Pair-Specific Adapters:** Shared platform components (discovery, AI orchestration, project scaffolding, validation, safety) must remain completely decoupled from game-specific mod loaders, compilers, or hook libraries.
2. **Capability-Driven Availability:** The engine must query adapter and game capabilities dynamically. If a game does not support live telemetry, live bridge options must be disabled in the UI.
3. **Honest Capability Reporting:** Unsupported or high-risk integrations (e.g. games protected by kernel anti-cheat like Vanguard or Easy Anti-Cheat in multiplayer) must be reported immediately as blocked.
4. **AI as a Sandboxed Assistant:** AI coding agents will generate code within isolated staging folders. No agent may execute unreviewed commands, tamper with system files, or bypass safety guardrails.
5. **No Proprietary Asset Redistribution:** NexLudic Engine will generate code, scripts, patches, and configurations, but will never package or redistribute copyrighted game binaries or assets.

---

## 6. Audit Conclusions & Next Steps

1. The workspace is currently pristine, containing only project logo artwork.
2. Git is not yet initialized and must remain uninitialized until authorized in the roadmap.
3. .NET 8.0 SDK, Node.js v26.8, and Python 3.14 are verified installed on the host machine. Java and Gradle are absent and will need documented handling when Java-based modding (such as Minecraft Fabric) is tested.
4. The product direction is firmly established as a **universal platform**, rendering the earlier two-game specifications as reference case studies rather than architectural boundaries.
5. All deliverables for Step 1 are ready to be established under `docs/product-baseline/`.

