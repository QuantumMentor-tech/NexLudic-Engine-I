# NEXLUDIC ENGINE — GAME-MODDING ECOSYSTEM TAXONOMY
## Document ID: NLE-RES-TAX-001
**Phase:** 1 — Product Definition & Research  
**Step:** 4 of 100 — Research Game-Modding Ecosystems, Integration Technologies, and Universal Adapter Opportunities  
**Date:** November 16, 2026  
**Auditor / Architect:** Principal Game-Technology Research Architect & Systems Analyst  

---

## 1. Executive Summary & Taxonomy Purpose

To enable NexLudic Engine to operate as a universal platform across arbitrary PC game pairings, the platform cannot assume that games share identical extension mechanisms. PC games span radically different architectures: compiled native C++ binaries, managed JVM bytecode, .NET CLR assemblies, embedded scripting runtimes (Lua, GSC, Papyrus), and closed proprietary engines.

This document establishes an **11-family taxonomy of game-modding and integration mechanisms**. NexLudic Engine's discovery and feasibility engines will map any candidate game pair against these capability families to determine valid integration paths without hardcoding assumptions.

```
+----------------------------------------------------------------------------------------------------+
|                                    INTEGRATION CAPABILITY FAMILIES                                 |
+----+----------------------------------------+------------------------------------------------------+
| #  | Capability Family                      | Core Mechanism & Typical Runtime Architecture        |
+----+----------------------------------------+------------------------------------------------------+
| 01 | Official SDK / Modding Development Kit | Publisher-provided tools, level editors & API suites |
| 02 | Community Mod Loader / Bytecode Patcher| Runtime bytecode instrumentation (JVM Mixin, Harmony)|
| 03 | Embedded Scripting & Config System     | In-engine script hooks (Lua, GSC, AngelScript, Python)|
| 04 | Asset Inspection & Archive Replacement | Archive repackaging / TOC redirection (.pak, .toc)   |
| 05 | Custom Content / Add-on Injection      | Data-driven entity, item, and recipe registration     |
| 06 | Native Binary Hook / Proxy DLL         | Dynamic link library proxying (dinput8, winhttp.dll) |
| 07 | External Local Telemetry / IPC Port    | Native telemetry UDP/TCP broadcast or IPC sockets    |
| 08 | Screen Observation & Synthetic Input   | Desktop window capture, OCR/CV, user-mode SendInput  |
| 09 | Cross-Process State & Event Bridge     | Shared memory / local named pipes linking two games  |
| 10 | Hybrid Multi-Tier Integration          | Host mod combined with live telemetry link           |
| 11 | Closed / Blocked / Anti-Cheat Gated    | Kernel anti-cheat, encrypted DRM, zero mod access    |
+----+----------------------------------------+------------------------------------------------------+
```

---

## 2. In-Depth Family Analysis

---

### Family 01: Official SDK or Extension Interface
- **Definition:** The game publisher or engine developer explicitly ships official developer tools, level editors, compilers, or documented plugin APIs for public modding.
- **Representative Examples:**
  - *Call of Duty: Black Ops III Mod Tools* (Steam Workshop, Radiant level editor, APE, GSC compiler).
  - *Bethesda Creation Kit* (Skyrim, Fallout 4 — Papyrus scripting, ESP/ESM plugin files).
  - *Arma 3 Tools* (Bohemia Interactive SQF scripting, terrain builder).
- **Prerequisites:** Ownership of the base game on supported store platforms (e.g. Steam); installation of the official tool DLC.
- **Potential Capabilities:** High-level gameplay creation, official asset import/compilation pipelines, native multiplayer server hosting (unranked), custom game modes.
- **Common Limitations:** Tools are tightly bound to one specific game version; multiplayer usage is restricted to unranked lobbies; proprietary editor learning curves.
- **Implementation Languages:** C++, C#, Python, proprietary scripting languages (GSC, Papyrus).
- **Platform Implications:** NexLudic Engine can orchestrate official compilers headlessly (e.g. executing Black Ops 3 linker or Radiant scripts) when installed.

---

### Family 02: Community Mod Loader or Managed Bytecode Framework
- **Definition:** Open-source community runtimes that intercept game execution at startup to dynamically inject class loaders, patch bytecode, or hook function calls at runtime.
- **Representative Examples:**
  - *FabricMC & NeoForge / Forge* (Minecraft Java Edition — ASM / Mixin bytecode transformation).
  - *BepInEx* (Unity Mono/IL2CPP games — Harmony runtime patcher, Doorstop preloader).
  - *RE-UE4SS* (Unreal Engine 4/5 — UObject hooking, SDK dumping, Lua bindings).
- **Prerequisites:** Clean game installation; matching mod loader runtime installed into game directory or launch profile.
- **Potential Capabilities:** Direct access to game tick loops, memory structures, rendering pipelines, custom entity spawners, and event listeners.
- **Common Limitations:** Game updates frequently alter internal signatures or class mappings (e.g. Mojang obfuscation maps), requiring loader updates.
- **Implementation Languages:** Java (Minecraft), C# (.NET / Unity), C++ / Lua (Unreal).
- **Platform Implications:** Primary default target for Strategy A (Host-Game Modification). Highly amenable to AI coding agents producing structured source files.

---

### Family 03: Embedded Scripting or Configuration Engine
- **Definition:** The game engine includes an internal scripting virtual machine (Lua, Python, JavaScript, AngelScript) that can be intercepted or extended via script drops into designated folders.
- **Representative Examples:**
  - *Cyber Engine Tweaks (CET)* (Cyberpunk 2077 — embedded Lua 5.1 runtime hooking REDengine 4).
  - *Grand Theft Auto V / RDR2 Script Hook* (Alexander Blade ASI loader executing native script threads).
  - *World of Warcraft / Factorio* (Native Lua API sandboxes).
- **Prerequisites:** Script loader DLL placed in executable directory.
- **Potential Capabilities:** High-level gameplay logic manipulation, spawning vehicles/weapons, altering game rules, binding hotkeys, rendering custom UI overlays via ImGui.
- **Common Limitations:** Scripting APIs are bounded by what native functions the loader exposes; cannot easily rewrite low-level physics engines.
- **Implementation Languages:** Lua, GSC, AngelScript, Python.
- **Platform Implications:** Exceptionally rapid turnaround for AI coding agents. Scripts can be authored as plain text files without requiring heavy compilation toolchains.

---

### Family 04: Asset Inspection, Conversion, and Archive Replacement
- **Definition:** The game loads content from proprietary archive containers (`.pak`, `.toc`, `.rpf`, `.ba2`). Modding is achieved by extracting assets, modifying them, and repacking or overriding the archive indexes.
- **Representative Examples:**
  - *SMPC Tool / Overstrike* (Marvel's Spider-Man Remastered — `.toc` table-of-contents asset redirection).
  - *OpenIV* (Grand Theft Auto V / RDR2 — `.rpf` archive inspection).
  - *UnrealPak / FModel* (Unreal Engine `.pak` / `.utoc` archive extraction and chunk loading).
- **Prerequisites:** Specialized archive extraction utilities; format conversion scripts.
- **Potential Capabilities:** Texture replacement, mesh swapping, custom character skins, audio/music replacement, string localization overrides.
- **Common Limitations:** **Zero live gameplay logic execution.** An archive extractor does *not* provide event extraction, memory hooks, or dynamic scripting.
- **Implementation Languages:** C++, C#, Python, binary hex templates.
- **Platform Implications:** Must be classified strictly as an asset pipeline capability, never confused with live runtime interoperability.

---

### Family 05: Data-Driven Content & Add-On Injection
- **Definition:** The game natively supports importing new gameplay items, recipes, quests, or blocks through structured data files (JSON, XML, YAML) without writing compiled code.
- **Representative Examples:**
  - *Minecraft Datapacks & Resource Packs* (JSON recipes, loot tables, advancements).
  - *Stardew Valley Content Patcher* (JSON patch manifests modifying game data dictionaries).
  - *Cities: Skylines / Paradox Games* (Paradox Script / XML object definitions).
- **Prerequisites:** Clean game installation; optional content loader.
- **Potential Capabilities:** High stability, 100% immune to binary crash bugs, official compatibility across game updates.
- **Common Limitations:** Restricted to parameter modifications; cannot introduce novel game physics or external network sockets.
- **Implementation Languages:** JSON, XML, YAML.
- **Platform Implications:** Safest, lowest-friction modding route. Ideal for AI coding agents generating declarative game content.

---

### Family 06: Native Binary Hooking & Proxy DLL Injection
- **Definition:** Placing a customized dynamic link library (DLL) into the game folder masquerading as an official system or DirectX library (e.g. `dinput8.dll`, `dxgi.dll`, `winhttp.dll`) to intercept function execution at startup.
- **Representative Examples:**
  - *BakkesMod* (Rocket League — C++ DLL injection into game process).
  - *Special K / ReShade* (DirectX / Vulkan rendering pipeline hooking).
  - *Script Hook RDR2* (`dinput8.dll` ASI loader).
- **Prerequisites:** Administrative permission to write to game folder; single-player or offline game mode.
- **Potential Capabilities:** Direct native process memory inspection, virtual table hooking, function detouring via Microsoft Detours or MinHook.
- **Common Limitations:** Fragile across game patches; trigger immediate account bans if loaded in online multiplayer games with anti-cheat.
- **Implementation Languages:** C, C++, Assembly (x64).
- **Platform Implications:** Must be deployed with extreme caution. Strictly prohibited in online multiplayer titles (`REQ-SAFE-001`).

---

### Family 07: External Local Telemetry & Network Ports
- **Definition:** The game natively exposes a loopback network port (UDP, TCP, WebSocket) broadcasting real-time match, vehicle, or player telemetry for third-party hardware (sim rigs, dashboards, companion apps).
- **Representative Examples:**
  - *Forza Motorsport / F1 Series / Assetto Corsa* (Standardized UDP telemetry streams).
  - *Flight Simulator (SimConnect API)* (Local TCP/IPC state exchange).
  - *Dota 2 / CS:GO (Game State Integration - GSI)* (Local HTTP JSON POST events).
- **Prerequisites:** Enabling telemetry broadcast in the game's settings menu or configuration file.
- **Potential Capabilities:** 100% safe, non-invasive, zero anti-cheat risk. Provides real-time speed, health, coordinates, and match events.
- **Common Limitations:** Read-only telemetry; does not accept control inputs or allow in-game visual modifications.
- **Implementation Languages:** Any network-capable language (C#, Node.js, Python).
- **Platform Implications:** Ideal data source for Strategy B (Live Interoperability Bridge).

---

### Family 08: Screen Observation & User-Mode Input Injection
- **Definition:** The platform treats the target game as a black box, observing its state via desktop window capture / OCR and sending synthetic user inputs (`SendInput`, virtual HID).
- **Representative Examples:**
  - External accessibility tools, macro utilities, stream overlays.
- **Prerequisites:** Windows Desktop environment; game running in borderless windowed mode.
- **Potential Capabilities:** Works with any closed, unmoddable game without modifying game files.
- **Common Limitations:** Fragile; sensitive to screen resolution, HUD scaling, and occlusion. Requires target game window to remain focused. Significant latency (16–50 ms).
- **Implementation Languages:** C# (`P/Invoke`), C++, Python (OpenCV).
- **Platform Implications:** Fallback mechanism when all internal modding options are unavailable.

---

### Family 09: Cross-Process State & Event Bridge
- **Definition:** A dedicated background service linking two active game processes via low-latency local IPC (Named Pipes, Shared Memory), coordinating bidirectional event dispatch.
- **Representative Examples:**
  - *NexLudic Live Bridge Daemon* (`\\.\pipe\nexludic_*`).
- **Prerequisites:** Both games running concurrently; each game equipped with an adapter hook (Family 02, 03, 06, or 07).
- **Potential Capabilities:** Sub-2 ms roundtrip latency, heartbeat monitoring, global E-Stop emergency abort.
- **Common Limitations:** High system resource consumption (running two heavy 3D games simultaneously); window focus and input routing friction.
- **Implementation Languages:** C# (.NET 8), C++20.
- **Platform Implications:** Core architectural foundation for Strategy B and Strategy C.

---

### Family 10: Hybrid Multi-Tier Integration
- **Definition:** A host game is modified using Family 02 (Mod Loader) or Family 03 (Scripting) to provide native visual and physics rendering, while simultaneously communicating with a second game via Family 09 (IPC Bridge) to ingest live telemetry.
- **Representative Examples:**
  - Minecraft Fabric mod rendering Spider-Man HUD and swinging physics, driven dynamically by an active secondary game's combat events.
- **Prerequisites:** Host mod loader installed; secondary game telemetry active; high-performance multi-core workstation.
- **Potential Capabilities:** Delivers the visual cohesion of Strategy A with the cross-process dynamics of Strategy B.
- **Common Limitations:** Highest development and testing complexity.
- **Platform Implications:** Reserved for advanced users and specialized mashups.

---

### Family 11: Closed, Blocked, or Anti-Cheat Protected
- **Definition:** The game enforces aggressive kernel-level anti-cheat (e.g. Riot Vanguard, BattlEye, Easy Anti-Cheat in kernel mode) or encrypted code virtualization (e.g. Denuvo) that prohibits DLL injection, memory inspection, or file tampering.
- **Representative Examples:**
  - *Valorant* (Vanguard), *Fortnite* (Easy Anti-Cheat / BattlEye), *Destiny 2* (BattlEye).
- **Prerequisites:** None.
- **Potential Capabilities:** **Zero.**
- **Platform Implications:** NexLudic Engine must immediately detect these games, refuse to attach mod hooks or live bridges, and report the blocker honestly to the user (`REQ-CORE-005`, `REQ-SAFE-001`).

---

## 3. Taxonomy Summary & Strategy Mapping

```
+----------------------------------------------------------------------------------------------------+
|                                TAXONOMY TO STRATEGY MAPPING MATRIX                                 |
+----+----------------------------------------+------------------+------------------+----------------+
| #  | Capability Family                      | Strategy A (Mod) | Strategy B (Live)| Strategy C (Hy)|
+----+----------------------------------------+------------------+------------------+----------------+
| 01 | Official SDK / MDK                     | Primary Candidate| Secondary Target | Supported      |
| 02 | Community Mod Loader (Fabric/BepInEx)  | Optimal Choice   | Adapter Hook     | Host Component |
| 03 | Embedded Scripting (CET / Lua / GSC)   | Optimal Choice   | Adapter Hook     | Host Component |
| 04 | Asset Inspection / Replacement (TOC)   | Asset Pipeline   | Unsupported      | Asset Source   |
| 05 | Data-Driven Content (Datapacks/JSON)   | Content Pipeline | Unsupported      | Content Source |
| 06 | Native Binary Hook (BakkesMod / ASI)   | High Risk        | Optimal Hook     | Bridge Hook    |
| 07 | External Telemetry Port (UDP / GSI)    | Ingestion Only   | Optimal Source   | Live Source    |
| 08 | Screen Observation & Synthetic Input   | Unsupported      | Fallback Only    | Secondary Link |
| 09 | Cross-Process IPC Bridge               | Not Needed       | Mandatory Core   | Mandatory Core |
| 10 | Hybrid Multi-Tier Integration          | Base Layer       | Secondary Layer  | Optimal Form   |
| 11 | Closed / Anti-Cheat Protected          | BLOCKED          | BLOCKED          | BLOCKED        |
+----+----------------------------------------+------------------+------------------+----------------+
```

