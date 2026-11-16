# NEXLUDIC ENGINE — GAME ENGINE & FRAMEWORK RESEARCH
## Document ID: NLE-RES-ENG-001
**Phase:** 1 — Product Definition & Research  
**Step:** 4 of 100 — Research Game-Modding Ecosystems, Integration Technologies, and Universal Adapter Opportunities  
**Date:** November 16, 2026  
**Auditor / Architect:** Senior Game-Technology Research Architect  

---

## 1. Research Scope & Evaluation Methodology

This document provides structured, evidence-backed research across eight primary PC game engines and modding frameworks. The objective is to understand how disparate engines expose extension hooks, handle asset formats, and support integration strategies, establishing the boundary between engine-level capabilities and title-specific constraints.

---

## 2. In-Depth Engine & Ecosystem Profiles

---

### Ecosystem 1: Minecraft Java Edition (FabricMC Ecosystem)
- **1. Game / Ecosystem Name:** Minecraft Java Edition (Fabric Loader & Fabric API).
- **2. Publisher / Maintainer:** Mojang Studios / Microsoft (Base game); FabricMC Project Community (Modding framework).
- **3. Game Engine / Runtime:** Java Virtual Machine (OpenJDK 21 for Minecraft 1.20.5+; OpenJDK 17 for 1.18–1.20.4). Lightweight Java Game Library (LWJGL 3) for OpenGL rendering and OpenAL audio.
- **4. Extension Mechanism:** Dynamic bytecode manipulation at class-loading time via SpongePowered Mixin framework. Mixins inject callbacks directly into Minecraft's obfuscated class methods.
- **5. Status:** Community modding framework; unofficial but widely recognized and permitted under Mojang's Minecraft Commercial Usage Guidelines.
- **6. Development Language:** Java (primary), Kotlin, Scala.
- **7. Available Tooling:** Fabric Loom (Gradle plugin), Official Yarn deobfuscation mappings, ProGuard/Tiny Remapper, IntelliJ IDEA / VS Code integration.
- **8. Asset Format Support:** JSON data-driven models, PNG textures, OGG Vorbis audio, JSON animation and particle descriptors.
- **9. Gameplay State & Control Capabilities:** Full, unrestricted programmatic access to tick loops, world blocks, entity state machines, player inventory, client-side rendering overlays, and network packet handlers.
- **10. Required Dependencies:** Java Development Kit (JDK 21), Gradle 8.8+, Fabric Loader, Fabric API.
- **11. Supported Versions:** Highly granular support from 1.14 through active release (1.21.x).
- **12. License & Redistribution:** Fabric Loader and API are Apache 2.0. Minecraft assets and compiled game code remain proprietary to Mojang; modders may distribute only their own mod JARs, not modified vanilla binaries.
- **13. Maintenance & Compatibility Concerns:** High version drift. Each minor Minecraft patch (e.g. 1.21.1 to 1.21.2) shifts internal obfuscation names, requiring rebuilds against updated Yarn mappings.
- **14. Applicable Integration Strategy:** **Strategy A (Host-Game Modification)** — Optimal host platform for testing mechanics imported from other games.
- **15. Evidence References:** [FabricMC GitHub Organization](https://github.com/FabricMC); [Fabric Documentation](https://fabricmc.net/wiki/).
- **16. Unresolved Questions:** Managing client-side tick dilation (Dead-Eye slow motion) without desynchronizing single-player server loops.

---

### Ecosystem 2: Unreal Engine 4 & 5 (RE-UE4SS Modding Ecosystem)
- **1. Game / Ecosystem Name:** Unreal Engine 4 / 5 Generic Modding (RE-UE4SS).
- **2. Publisher / Maintainer:** Epic Games (Engine); UE4SS-RE Community (Modding framework).
- **3. Game Engine / Runtime:** Native compiled C++ with proprietary UObject reflection system and garbage collection.
- **4. Extension Mechanism:** DLL injection proxy (`dxgi.dll` or `dwmapi.dll`) that intercepts engine initialization, reconstructs internal `UObjectArray` and `GNames` global pointers, and dynamically binds an embedded Lua virtual machine.
- **5. Status:** Unofficial community framework.
- **6. Development Language:** C++20, Lua 5.4.
- **7. Available Tooling:** RE-UE4SS GUI property editor, C++ SDK generator (dumps headers), UnrealPak, FModel.
- **8. Asset Format Support:** `.pak`, `.utoc`, `.ucas` container archives containing serialized `.uasset` and `.uexp` binaries.
- **9. Gameplay State & Control Capabilities:** Can dynamically iterate active actors, read/write reflected UObject properties, hook UFunctions (e.g. `TakeDamage`, `BeginPlay`), and invoke Blueprint functions from Lua.
- **10. Required Dependencies:** Visual C++ Redistributable, CMake, compatible Windows x64 host.
- **11. Supported Versions:** Broad compatibility from UE 4.7 up to modern UE 5.4+.
- **12. License & Redistribution:** RE-UE4SS is open-source under the MIT License.
- **13. Maintenance & Compatibility Concerns:** Games with custom engine forks or stripped reflection metadata require manual memory pattern scanning (`AobScanner`).
- **14. Applicable Integration Strategy:** **Strategy B (Telemetry Source)** or **Strategy A (Host-Game Mod)** for supported offline Unreal titles.
- **15. Evidence References:** [UE4SS GitHub Repository](https://github.com/UE4SS-RE/RE-UE4SS); [Modding.wiki UE4SS Guide](https://modding.wiki).
- **16. Unresolved Questions:** Detecting whether a commercial Unreal title has stripped UFunction names before launching the injector.

---

### Ecosystem 3: Unity Engine (BepInEx & Harmony Ecosystem)
- **1. Game / Ecosystem Name:** Unity Engine Modding (BepInEx).
- **2. Publisher / Maintainer:** Unity Technologies (Engine); BepInEx Organization (Modding framework).
- **3. Game Engine / Runtime:** C# over Mono runtime (older titles) or IL2CPP ahead-of-time compiled native C++ binaries (modern titles).
- **4. Extension Mechanism:** Doorstop DLL proxying (`winhttp.dll`) that bootstraps a custom Mono domain, loading `0Harmony.dll` to patch method prologues and epilogues in memory.
- **5. Status:** Unofficial community framework; widely accepted across hundreds of commercial indie and AA games.
- **6. Development Language:** C# (.NET Standard 2.0 / .NET Framework 4.x / .NET 6+).
- **7. Available Tooling:** AssetStudio (asset viewer), Il2CppDumper / Cpp2IL (for IL2CPP games), Visual Studio / Rider.
- **8. Asset Format Support:** Unity AssetBundles, `.sharedAssets`, serialized Unity YAML.
- **9. Gameplay State & Control Capabilities:** Full access to all public and private fields via Harmony reflection; ability to hook `MonoBehaviour.Update()`, inspect physics colliders, and manipulate game save states.
- **10. Required Dependencies:** .NET SDK, HarmonyX, MonoMod.
- **11. Supported Versions:** Unity 5.x up to Unity 2023 / Unity 6.
- **12. License & Redistribution:** BepInEx is LGPL-2.1; Harmony is MIT.
- **13. Maintenance & Compatibility Concerns:** Games compiled with IL2CPP require regenerating dummy DLL assemblies after every game patch.
- **14. Applicable Integration Strategy:** **Strategy A (Host Mod)** and **Strategy B (Telemetry Hook)**.
- **15. Evidence References:** [BepInEx GitHub](https://github.com/BepInEx/BepInEx); [Harmony Documentation](https://harmony.pardeike.net/).
- **16. Unresolved Questions:** Developing an automated pipeline for generating C# dummy DLLs from IL2CPP titles without human intervention.

---

### Ecosystem 4: Cyberpunk 2077 (REDengine 4 / Cyber Engine Tweaks)
- **1. Game / Ecosystem Name:** Cyberpunk 2077 (Cyber Engine Tweaks & REDmod).
- **2. Publisher / Maintainer:** CD Projekt Red (Game & REDmod); yamashi / maximegmd & CET Team (Modding framework).
- **3. Game Engine / Runtime:** REDengine 4 (DirectX 12 native C++ 64-bit engine).
- **4. Extension Mechanism:** `version.dll` proxy that hooks DirectX 12 render loop, binds ImGui overlay, and exposes internal game engine functions (Rtti system) to an embedded Lua 5.1 environment.
- **5. Status:** Community framework (CET) operating alongside official modding DLC (REDmod).
- **6. Development Language:** Lua (CET scripts), Redscript, C++ (native plugins).
- **7. Available Tooling:** WolvenKit (open-source REDengine modding toolkit), Cyber Engine Tweaks console, REDmod CLI.
- **8. Asset Format Support:** Proprietary `.archive` containers containing compiled CR2W binary resources (meshes, textures, audio banks).
- **9. Gameplay State & Control Capabilities:** Reading and invoking thousands of registered engine functions via the RTTI dump: player coordinates, weapon stats, visual glitch effects, weather control, vehicle physics spawning.
- **10. Required Dependencies:** Cyberpunk 2077 PC installation, DirectX 12 hardware, Visual C++ Redistributable.
- **11. Supported Versions:** Cyberpunk 2077 v2.0+ (Phantom Liberty patch era).
- **12. License & Redistribution:** CET is MIT licensed. REDmod is proprietary CD Projekt Red freeware.
- **13. Maintenance & Compatibility Concerns:** Major game updates alter RTTI hash tables and function memory offsets.
- **14. Applicable Integration Strategy:** **Strategy B (Live Event Receiver)** or **Strategy A (Host-Game Mod)**.
- **15. Evidence References:** [Cyber Engine Tweaks Repository](https://github.com/maximegmd/CyberEngineTweaks); [WolvenKit GitHub](https://github.com/WolvenKit/WolvenKit).
- **16. Unresolved Questions:** Establishing low-latency local named pipe communication directly from inside CET's Lua environment without blocking the main game render thread.

---

### Ecosystem 5: Red Dead Redemption 2 (Rockstar RAGE Engine / Script Hook)
- **1. Game / Ecosystem Name:** Red Dead Redemption 2 (RAGE Engine / Script Hook RDR2).
- **2. Publisher / Maintainer:** Rockstar Games / Take-Two Interactive (Game); Alexander Blade (Script Hook library).
- **3. Game Engine / Runtime:** Rockstar Advanced Game Engine (RAGE) with native x64 compiled code and custom script bytecode threads.
- **4. Extension Mechanism:** `dinput8.dll` ASI loader that attaches to the game process and exposes native script functions (`native.h`) to custom `.asi` C++ libraries.
- **5. Status:** Unofficial community framework; strictly prohibited in online multiplayer.
- **6. Development Language:** C++, Assembly.
- **7. Available Tooling:** OpenIV (archive viewer for `.rpf`), Lenny's Mod Loader (LML), Script Hook RDR2 SDK.
- **8. Asset Format Support:** Encrypted `.rpf` archives containing `.ytd` (textures), `.ydr` (drawables), `.awc` (audio).
- **9. Gameplay State & Control Capabilities:** Full invocation of thousands of native script calls: ped spawning, weapon recoil, weather, slow-motion (Dead-Eye), camera controls.
- **10. Required Dependencies:** Base game installation, Alexander Blade `ScriptHookRDR2.dll`.
- **11. Supported Versions:** Tied to specific Rockstar build versions (breaks on game updates).
- **12. License & Redistribution:** Closed-source freeware. Redistribution of modified binaries or original `.rpf` archives is strictly restricted by Rockstar EULA.
- **13. Maintenance & Compatibility Concerns:** Single-player only. Script Hook contains hardcoded shutdown checks if Red Dead Online is detected.
- **14. Applicable Integration Strategy:** **Guest Content Source for Strategy A** (extracting mechanics/audio into Minecraft) or **Strategy B** (Single-player only).
- **15. Evidence References:** [Alexander Blade Script Hook RDR2](http://www.dev-c.com/rdr2/scripthookrdr2/).
- **16. Unresolved Questions:** Avoiding Rockstar Launcher integrity check failures when modifying local game folders.

---

### Ecosystem 6: Call of Duty: Black Ops III (Treyarch Engine / Official Mod Tools)
- **1. Game / Ecosystem Name:** Call of Duty: Black Ops III Mod Tools.
- **2. Publisher / Maintainer:** Activision / Treyarch.
- **3. Game Engine / Runtime:** Treyarch proprietary engine (derived from IW Engine). Compiled native x64 C++ runtime with custom GSC/CSC virtual machine.
- **4. Extension Mechanism:** Official Steam Workshop Mod Tools suite compiling custom maps, game modes, and assets into standalone mod packages loaded via the in-game "Mods" menu.
- **5. Status:** Official publisher-supported development kit.
- **6. Development Language:** GSC (Game Script), CSC (Client Script), C++.
- **7. Available Tooling:** Radiant (3D level editor), APE (Asset Property Editor), Linker, Mod Tools Launcher.
- **8. Asset Format Support:** Custom `.xpak` and `.zone` compiled files.
- **9. Gameplay State & Control Capabilities:** Scripting full game modes, custom zombie rounds, perk machines, mystery box spawning, HUD overlays, weapon handling.
- **10. Required Dependencies:** Steam base game, BO3 Mod Tools DLC (~100 GB storage).
- **11. Supported Versions:** Black Ops III PC Steam version.
- **12. License & Redistribution:** Proprietary Activision EULA. Workshop distribution permitted; commercial redistribution prohibited.
- **13. Maintenance & Compatibility Concerns:** Official toolchain is frozen (no longer updated); highly stable on Windows 10/11.
- **14. Applicable Integration Strategy:** **Content & Logic Source for Strategy A** (replicating Zombies in Minecraft) or **Host Strategy** (building custom BO3 mods via AI-generated GSC scripts).
- **15. Evidence References:** [Steam Store - BO3 Mod Tools](https://store.steampowered.com/app/545990/).
- **16. Unresolved Questions:** Automating the headless compilation of GSC scripts without launching the full Radiant editor UI.

---

### Ecosystem 7: Marvel's Spider-Man Remastered (Insomniac Engine)
- **1. Game / Ecosystem Name:** Marvel's Spider-Man Remastered PC.
- **2. Publisher / Maintainer:** Sony Interactive Entertainment / Insomniac Games / Nixxes Software.
- **3. Game Engine / Runtime:** Insomniac proprietary engine. Compiled native x64 DirectX 12 binary.
- **4. Extension Mechanism:** Asset archive replacement via `.toc` (Table of Contents) re-indexing using SMPC Tool or Overstrike.
- **5. Status:** Unofficial community asset modding. **Zero official SDK or native scripting APIs.**
- **6. Development Language:** Binary asset editing, Python/C# extractor scripts.
- **7. Available Tooling:** SMPC Tool (jedijosh920), Overstrike Mod Manager.
- **8. Asset Format Support:** Proprietary `.stage` and `.assetarchive` containers; models and textures.
- **9. Gameplay State & Control Capabilities:** **Severely Limited.** No public scripting engine. Can replace visual models (suits, textures), but cannot programmatically spawn custom entities, read live player coordinates via an API, or inject new physics loops without reverse-engineered memory hooking.
- **10. Required Dependencies:** Base game installation, SMPC Tool.
- **11. Supported Versions:** PC Remastered release on Steam/Epic.
- **12. License & Redistribution:** Proprietary Sony EULA. Redistribution of modified game archives is prohibited.
- **13. Maintenance & Compatibility Concerns:** High risk of archive corruption if `.toc` files are modified improperly.
- **14. Applicable Integration Strategy:** **Guest Visual Reference for Strategy A** (recreating Spider-Man mechanics inside Minecraft host) or **Strategy B (Restricted)** via external user-mode synthetic input.
- **15. Evidence References:** [SMPC Tool on Nexus Mods](https://www.nexusmods.com/marvelsspidermanremastered/mods/51); [Overstrike GitHub](https://github.com/overstrike).
- **16. Unresolved Questions:** Confirming whether any stable user-mode telemetry hook exists without triggering anti-tamper memory protection.

---

### Ecosystem 8: Rocket League (Psyonix / BakkesMod SDK)
- **1. Game / Ecosystem Name:** Rocket League (BakkesMod SDK).
- **2. Publisher / Maintainer:** Psyonix / Epic Games (Game); BakkesMod Development Team (Modding framework).
- **3. Game Engine / Runtime:** Custom Unreal Engine 3 binary with proprietary PhysX vehicle simulation.
- **4. Extension Mechanism:** C++ DLL injector that attaches to Rocket League, hooks internal game loop delegates, and exposes a high-level C++ plugin SDK (`BakkesModPlugin`).
- **5. Status:** Community framework explicitly whitelisted and permitted by Psyonix for offline/freeplay use.
- **6. Development Language:** C++20.
- **7. Available Tooling:** BakkesMod SDK (`bakkesmodorg/BakkesModSDK`), MSVC compiler, BakkesMod Injector.
- **8. Asset Format Support:** Unreal Engine 3 `.upk` packages; custom textures and skins via uMod.
- **9. Gameplay State & Control Capabilities:** Rich, high-precision access to ball position, vehicle velocity, boost amount, score, demo events, and match tick callbacks.
- **10. Required Dependencies:** Base game (Steam or Epic Games Store), BakkesMod runtime, Visual C++ Redistributable.
- **11. Supported Versions:** Continuously maintained for latest Epic Games Store / Steam releases.
- **12. License & Redistribution:** BakkesMod SDK is open-source under the MIT License.
- **13. Maintenance & Compatibility Concerns:** Major Rocket League title updates require updating the BakkesMod core injector before third-party plugins load.
- **14. Applicable Integration Strategy:** **Strategy B (Telemetry Source)** — The gold standard for real-time live event extraction.
- **15. Evidence References:** [BakkesMod SDK GitHub](https://github.com/bakkesmodorg/BakkesModSDK); [BakkesMod Documentation](https://wiki.bakkesplugins.com/).
- **16. Unresolved Questions:** Packaging BakkesMod C++ plugin compilation into a clean, zero-configuration pipeline for non-technical users.

