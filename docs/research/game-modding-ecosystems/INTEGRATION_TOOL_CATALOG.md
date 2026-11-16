# NEXLUDIC ENGINE — INTEGRATION TOOL CATALOG
## Document ID: NLE-RES-CAT-001
**Phase:** 1 — Product Definition & Research  
**Step:** 4 of 100 — Research Game-Modding Ecosystems, Integration Technologies, and Universal Adapter Opportunities  
**Date:** November 16, 2026  
**Auditor / Architect:** Senior Game-Modding Engineer & Toolchain Analyst  

---

## 1. Catalog Governance & Inclusion Criteria

This catalog records third-party tools, mod loaders, decompilers, SDKs, and libraries evaluated for potential integration with NexLudic Engine.
- **Evaluation Status:** Candidate tools are assessed for capability, license compliance, maintenance vitality, and security risks.
- **Strict Boundary:** Inclusion in this catalog **does not constitute adoption as an approved dependency**. All dependencies remain uninstalled and subject to formal architectural sign-off.

---

## 2. Structured Integration Tool Directory

---

### Category 1: Community Mod Loaders & Bytecode Patchers

#### Tool 1.1: FabricMC Ecosystem (Fabric Loader, Fabric API, Fabric Loom)
- **Official Repository / URL:** [https://github.com/FabricMC/fabric](https://github.com/FabricMC/fabric) | [https://fabricmc.net](https://fabricmc.net)
- **Primary Purpose:** Lightweight, modular modding toolchain for Minecraft Java Edition utilizing SpongePowered Mixin bytecode transformation.
- **Supported Platforms:** Windows, Linux, macOS (JVM-based).
- **Target Game / Engine:** Minecraft Java Edition (Versions 1.14 through 1.21+).
- **Programming Languages:** Java, Kotlin.
- **Integration Strategy Role:** Strategy A (Host-Game Modification Reference Target).
- **Maintenance / Vitality:** Highly active; maintained by dedicated core team with immediate updates for every Minecraft release.
- **Software License:** Apache License 2.0 (Fabric Loader / API). Permissive commercial and open-source reuse.
- **Build / Runtime Dependencies:** JDK 17 / 21, Gradle 8.x.
- **Known Limitations:** Minecraft-specific; cannot be generalized to native C++ games.
- **Role in NexLudic Engine:** Primary reference host mod template for Phase 5 MVP.

#### Tool 1.2: BepInEx
- **Official Repository / URL:** [https://github.com/BepInEx/BepInEx](https://github.com/BepInEx/BepInEx)
- **Primary Purpose:** Unified plugin framework and patcher for Unity (Mono & IL2CPP) and .NET framework games.
- **Supported Platforms:** Windows, Linux, macOS.
- **Target Game / Engine:** Unity Engine games (e.g. Valheim, Risk of Rain 2, Cities: Skylines II).
- **Programming Languages:** C# (.NET).
- **Integration Strategy Role:** Strategy A (Host-Game Mod) & Strategy B (Telemetry Hook).
- **Maintenance / Vitality:** Active; standard framework for Unity modding worldwide.
- **Software License:** GNU Lesser General Public License v2.1 (LGPL-2.1).
- **Build / Runtime Dependencies:** .NET SDK, HarmonyX, MonoMod.
- **Known Limitations:** IL2CPP games require unhollower / Cpp2IL symbol dumping, which breaks across game patches.
- **Role in NexLudic Engine:** Candidate adapter framework for Unity-based games in Release 2.0.

#### Tool 1.3: RE-UE4SS (Unreal Engine 4/5 Scripting System)
- **Official Repository / URL:** [https://github.com/UE4SS-RE/RE-UE4SS](https://github.com/UE4SS-RE/RE-UE4SS)
- **Primary Purpose:** Injectable C++ modding framework, live UObject property dumper, and Lua scripting engine for Unreal Engine 4 and 5.
- **Supported Platforms:** Windows x64.
- **Target Game / Engine:** Unreal Engine 4.7 up to 5.x titles (e.g. Palworld, Hogwarts Legacy).
- **Programming Languages:** C++, Lua.
- **Integration Strategy Role:** Strategy A (Host Mod) & Strategy B (Live Bridge Hook).
- **Maintenance / Vitality:** Active open-source development community.
- **Software License:** MIT License.
- **Build / Runtime Dependencies:** CMake, MSVC C++ runtime.
- **Known Limitations:** Games with custom modified Unreal headers or custom obfuscation can crash during automatic SDK dumping.
- **Role in NexLudic Engine:** Primary candidate adapter for Unreal Engine games in Release 1.1 / 2.0.

---

### Category 2: Embedded Scripting & Runtime Loaders

#### Tool 2.1: Cyber Engine Tweaks (CET)
- **Official Repository / URL:** [https://github.com/maximegmd/CyberEngineTweaks](https://github.com/maximegmd/CyberEngineTweaks)
- **Primary Purpose:** Framework hooking REDengine 4 in Cyberpunk 2077, exposing an embedded Lua 5.1 environment and ImGui overlay.
- **Supported Platforms:** Windows 10/11 x64 (DirectX 12).
- **Target Game / Engine:** Cyberpunk 2077 (CD Projekt Red REDengine 4).
- **Programming Languages:** C++, Lua.
- **Integration Strategy Role:** Strategy A (Host Mod) & Strategy B (Event Receiver Hook).
- **Maintenance / Vitality:** Actively maintained for Cyberpunk 2077 patch 2.1+.
- **Software License:** MIT License.
- **Build / Runtime Dependencies:** DirectX 12 runtime, MSVC C++.
- **Known Limitations:** Tightly bound to Cyberpunk 2077 binary layouts; major game updates break offsets until recompiled.
- **Role in NexLudic Engine:** Candidate adapter hook for Cyberpunk 2077 in Pair 3 (Rocket League + Cyberpunk).

#### Tool 2.2: Script Hook RDR2 (Alexander Blade)
- **Official Repository / URL:** [http://www.dev-c.com/rdr2/scripthookrdr2/](http://www.dev-c.com/rdr2/scripthookrdr2/)
- **Primary Purpose:** ASI script loader and native function invoker for Red Dead Redemption 2.
- **Supported Platforms:** Windows x64.
- **Target Game / Engine:** Red Dead Redemption 2 (Rockstar RAGE Engine).
- **Programming Languages:** C++, Assembly.
- **Integration Strategy Role:** Strategy A (Single-player modding).
- **Maintenance / Vitality:** Maintained as closed-source freeware by author; updated after major Rockstar title updates.
- **Software License:** Freeware (Closed source binary distribution).
- **Build / Runtime Dependencies:** `dinput8.dll` ASI loader, MSVC runtime.
- **Known Limitations:** Closed-source; redistribution of modified binaries is restricted; automatically shuts down if online mode is accessed.
- **Role in NexLudic Engine:** Analyzed for understanding RDR2 native scripting; not suitable as a core bundled dependency.

---

### Category 3: Official Game Modding Suites

#### Tool 3.1: Call of Duty: Black Ops III Mod Tools
- **Official Repository / URL:** [Steam Store - BO3 Mod Tools](https://store.steampowered.com/app/545990/)
- **Primary Purpose:** Official developer suite containing Radiant Level Editor, Asset Property Editor (APE), and GSC script compiler.
- **Supported Platforms:** Windows 7/8/10/11 x64.
- **Target Game / Engine:** Call of Duty: Black Ops III (Treyarch Engine).
- **Programming Languages:** GSC, CSC, C++.
- **Integration Strategy Role:** Strategy A (Host Mod Scaffolding).
- **Maintenance / Vitality:** Stable official legacy toolchain; Steam Workshop integration remains active.
- **Software License:** Proprietary Activision / Treyarch End User License Agreement.
- **Build / Runtime Dependencies:** Base game ownership on Steam, ~100 GB disk space for full asset packs.
- **Known Limitations:** Massive disk footprint; strictly tied to Steam ecosystem; cannot be redistributed.
- **Role in NexLudic Engine:** Explored as a candidate for automated GSC script generation in Pair 2.

---

### Category 4: Asset Extraction & Container Tools

#### Tool 4.1: SMPC Tool (Spider-Man PC Modding Tool) & Overstrike
- **Official Repository / URL:** [https://github.com/Phew/SMPCTool-src](https://github.com/Phew/SMPCTool-src) | [Overstrike on GitHub](https://github.com/overstrike)
- **Primary Purpose:** Asset container inspector and `.toc` (Table of Contents) index patcher for Marvel's Spider-Man PC ports.
- **Supported Platforms:** Windows x64.
- **Target Game / Engine:** Insomniac proprietary engine (Spider-Man Remastered, Miles Morales).
- **Programming Languages:** C#, C++.
- **Integration Strategy Role:** Asset extraction and custom suit replacement.
- **Maintenance / Vitality:** Community maintained.
- **Software License:** GPL-3.0 / Open Source.
- **Build / Runtime Dependencies:** .NET 6 / 7 runtime.
- **Known Limitations:** Limited to asset replacement; **does not provide runtime scripting or external telemetry APIs**.
- **Role in NexLudic Engine:** Evaluated in Pair 4; confirms Spider-Man live integration cannot rely on asset tools alone.

#### Tool 4.2: FModel & UnrealPak
- **Official Repository / URL:** [https://fmodel.app](https://fmodel.app) | [https://github.com/4sval/FModel](https://github.com/4sval/FModel)
- **Primary Purpose:** Open-source Unreal Engine archive reader and asset visualizer (PAK, UTOC, UCAS).
- **Supported Platforms:** Windows x64.
- **Target Game / Engine:** Unreal Engine 4 and 5 games.
- **Programming Languages:** C# (.NET).
- **Integration Strategy Role:** Local asset format inspection.
- **Maintenance / Vitality:** Actively maintained.
- **Software License:** GPL-3.0.
- **Role in NexLudic Engine:** Evaluated for future asset extraction workflows; non-core.

---

### Category 5: Native Plugin & Game-State Hooking SDKs

#### Tool 5.1: BakkesMod SDK
- **Official Repository / URL:** [https://github.com/bakkesmodorg/BakkesModSDK](https://github.com/bakkesmodorg/BakkesModSDK)
- **Primary Purpose:** C++ SDK for authoring plugins that attach to Rocket League's process, hooking game loop events safely.
- **Supported Platforms:** Windows x64.
- **Target Game / Engine:** Rocket League (Psyonix / Unreal Engine 3 custom).
- **Programming Languages:** C++20.
- **Integration Strategy Role:** Strategy B (Live Telemetry & Event Hook).
- **Maintenance / Vitality:** Highly active; permitted by Psyonix for custom/training games.
- **Software License:** MIT License.
- **Build / Runtime Dependencies:** MSVC C++ compiler, Windows SDK.
- **Known Limitations:** Restricted to PC; plugins must respect anti-cheat rules during competitive online matches.
- **Role in NexLudic Engine:** Primary candidate hook for Rocket League in Pair 3.

---

### Category 6: Input Emulation & Virtual Gamepad Status

#### Tool 6.1: Windows User-Mode `SendInput` API
- **Official Documentation:** [Microsoft Learn - SendInput function](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput)
- **Primary Purpose:** Standard Win32 user-mode API for synthesizing keystrokes, mouse motions, and button clicks.
- **Supported Platforms:** Windows (all versions).
- **Maintenance / Vitality:** Permanent, official Windows OS subsystem.
- **Software License:** Microsoft Windows OS API (Native).
- **Security & Integrity:** 100% user-mode; requires zero kernel drivers, zero UAC elevation, and zero third-party binaries.
- **Known Limitations:** Target game window must have active input focus; some RawInput games bypass synthetic mouse deltas.
- **Role in NexLudic Engine:** Core verified input dispatch mechanism for the desktop controller (`REQ-BRIDGE-003`).

#### Tool 6.2: ViGEmBus (Retired) vs. Modern Alternatives
- **Historical Reference:** `nefarius/ViGEmBus` on GitHub.
- **Current Status:** **Archived / Retired** as of November 2023.
- **Technical Barrier:** Required Windows kernel-mode driver (`.sys`) with administrative elevation.
- **Architectural Determination:** Formally rejected for NexLudic Engine core (`DEC-003`). Virtual gamepad requirements will prioritize user-mode HID or mod-level input injection.

---

### Category 7: AI Coding Agent Orchestration Technologies

#### Tool 7.1: Model Context Protocol (MCP) SDK
- **Official Repository / URL:** [https://github.com/modelcontextprotocol](https://github.com/modelcontextprotocol)
- **Primary Purpose:** Open protocol standardizing how AI models and coding agents discover tools, inspect resources, and edit workspaces.
- **Supported Platforms:** Cross-platform (Node.js, Python, C# SDKs).
- **Programming Languages:** TypeScript, Python, C#.
- **Maintenance / Vitality:** Rapidly growing industry standard backed by major AI labs.
- **Software License:** MIT License.
- **Role in NexLudic Engine:** Optimal candidate protocol for connecting NexLudic Engine core to external AI coding agents (Claude Code, Gemini CLI, local SLMs) in Phase 5.

