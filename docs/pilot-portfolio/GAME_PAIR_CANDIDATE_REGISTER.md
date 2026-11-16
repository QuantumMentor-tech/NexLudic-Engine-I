# NexLudic Engine — Game-Pair Candidate Register

**Document ID:** `NLE-REG-CAND-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 7 of 100)  
**Status:** Approved Technical Register  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Research Engineering  

---

## 1. Register Overview & Epistemic Grounding

The **Game-Pair Candidate Register** maintains the authoritative technical catalog of researched game pairs evaluated for potential inclusion in the NexLudic Engine pilot portfolio.

In accordance with [TECHNICAL_FEASIBILITY_FRAMEWORK.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/TECHNICAL_FEASIBILITY_FRAMEWORK.md) and [MINIMUM_PROOF_OF_CONCEPT_STANDARD.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/feasibility/MINIMUM_PROOF_OF_CONCEPT_STANDARD.md), all candidates in this register are evaluated against the 4 epistemic tiers:
- **Tier 1 (Hypothesized):** Unverified technical assumption requiring practical testing.
- **Tier 2 (Sourced):** Backed by official developer documentation, APIs, or published SDKs.
- **Tier 3 (Verified Community):** Supported by active, maintained open-source modding frameworks and tools.
- **Tier 4 (Empirically Proven):** Validated through reproducible execution logs in the NexLudic sandbox.

---

## 2. Summary Status Dashboard

```
+--------------------------------------------------------------------------------------------------------+
|                                    CANDIDATE PAIR EVALUATION MATRIX                                    |
+---------------+------------------------+------------------------+-------------+------------------------+
| Candidate ID  | Game A (Target)        | Game B (Target)        | Strategy    | Readiness Status       |
+---------------+------------------------+------------------------+-------------+------------------------+
| CAND-PAIR-001 | Minecraft Java (1.21)  | Red Dead Redemption 2  | Strategy A  | PILOT SHORTLIST (Arch A|
| CAND-PAIR-002 | Risk of Rain 2 (Unity) | Call of Duty (Zombies) | Strategy A  | PILOT SHORTLIST (Arch B|
| CAND-PAIR-003 | Rocket League (UE3)    | Cyberpunk 2077 (RED4)  | Strategy B  | PILOT SHORTLIST (Arch C|
| CAND-PAIR-004 | Minecraft Java (1.21)  | Spider-Man Remastered  | Strategy C  | PILOT SHORTLIST (Arch D|
| CAND-PAIR-005 | Minecraft Java (1.21)  | Call of Duty: BO3      | Strategy B  | REJECTED (VAC Blocker) |
| CAND-PAIR-006 | Deep Rock Galactic(UE4)| Left 4 Dead 2 (Source) | Strategy A  | RESEARCH QUALIFIED     |
| CAND-PAIR-007 | Cities: Skylines(Unity)| GTA V (RAGE Engine)    | Strategy B  | DEFERRED (Hardware)    |
| CAND-PAIR-008 | Cyberpunk 2077 (RED4)  | Spider-Man Remastered  | Strategy B  | REJECTED (Instability) |
+---------------+------------------------+------------------------+-------------+------------------------+
```

---

## 3. Researched Candidate Profiles

---

### Candidate ID: `CAND-PAIR-001`
- **Game A (Host):** *Minecraft: Java Edition* (Mojang / JVM) — Target Version: 1.21.x (Release).
- **Game B (Guest / Content Source):** *Red Dead Redemption 2* (Rockstar Games / RAGE Engine) — Version: 1491.50+ (Single-Player offline).
- **Proposed Integration Outcome:** Western Combat & Traversal Expansion: Add functional Cattleman Revolver with custom reload animations and recoil physics, Dead-Eye targeting system with visual GLSL screen-tint shader and slow-motion tick dilation (`tick_rate`), circular western weapon wheel HUD, and custom extracted audio effects.
- **Potential Host Game:** Game A (Minecraft Java Edition).
- **Candidate Integration Approach:** **Strategy A (Host-Game Modification)**.
- **Relevant Extension Surfaces:**
  - Minecraft: FabricMC 0.16.x Loader, Fabric API, Loom Gradle plugin, client-side post-processing GLSL shaders, `tick_rate` command API (`1.20.5+`).
  - RDR2: OpenIV / local audio bank decoders (offline file reading only; RDR2 process does not execute).
- **Required Languages & Toolchains:** Java 21 LTS, Gradle 8.x, GLSL, Loom, CFR/Vineflower decompiler.
- **Primary Dependencies & Maintenance:** FabricMC (Active, Tier 3), Fabric API (Active, Tier 3).
- **Evidence Quality & Sources:**
  - [FabricMC Documentation](https://fabricmc.net/wiki/) (Tier 2/3).
  - [Minecraft Official Tick Rate Feature](https://www.minecraft.net/en-us/article/minecraft-snapshot-23w43a) (Tier 2).
- **Known Constraints & Questions:**
  - Automated conversion of proprietary RDR2 audio banks (`.awc`) into OGG Vorbis without interactive GUI tools remains unverified (`ISSUE-AUDIO-01`).
- **Licensing & Policy:** RDR2 assets cannot be bundled (`REQ-MOD-003`); local extraction script required. Single-player Minecraft modding fully permitted under Mojang Commercial Usage Guidelines.
- **Major Technical Risks:** High memory consumption if high-resolution uncompressed textures are imported into Java heap (`RSK-14`).
- **Architectural Learning Value:** Validates the core engine's capability to scaffold, compile, package, and deploy a complete Gradle-based mod in a managed JVM runtime.
- **Portfolio Overlap:** None (Serves as baseline for Archetype A).
- **Preliminary Readiness Status:** **PILOT SHORTLIST (Archetype A Baseline)**.
- **Outstanding Research:** Automated CLI audio conversion script for `.awc` format.
- **Rationale for Advance:** Lowest technical risk, mature ecosystem, provides clean foundation to test AI scaffolding and rollback before introducing dual-process bridging.

---

### Candidate ID: `CAND-PAIR-002`
- **Game A (Host):** *Risk of Rain 2* (Hopoo Games / Unity Engine 2021.3 LTS Mono) — Target Version: Latest Steam Build.
- **Game B (Guest / Content Source):** *Call of Duty: Black Ops III* (Treyarch) — Single-Player Zombies Mode.
- **Proposed Integration Outcome:** CoD Zombies Survival Loop in Unity: Add round-based progressive zombie horde waves, functional Mystery Box entity with cycling item roulette, and Pack-a-Punch weapon upgrade shrine using RoR2 item interaction hooks.
- **Potential Host Game:** Game A (Risk of Rain 2).
- **Candidate Integration Approach:** **Strategy A (Host-Game Modification)**.
- **Relevant Extension Surfaces:**
  - Risk of Rain 2: BepInEx 5.4.x (.NET/Mono mod loader), R2API (ItemAPI, PrefabAPI, DirectorAPI), HarmonyX runtime bytecode patcher.
  - BO3: Local asset extraction (sound cues, particle effects) from offline game files.
- **Required Languages & Toolchains:** C# 12, .NET 8.0 SDK, MSBuild, HarmonyX, Unity Doorstop.
- **Primary Dependencies & Maintenance:** BepInEx 5.4+ (Active, Tier 3), R2API (Active, Tier 3).
- **Evidence Quality & Sources:**
  - [BepInEx GitHub & Documentation](https://docs.bepinex.dev/) (Tier 3).
  - [Risk of Rain 2 Modding Wiki & R2API](https://risk-of-rain-modding.github.io/R2Wiki/) (Tier 3).
- **Known Constraints & Questions:**
  - Harmony dynamic IL patching requires clean detachment upon mod reload to avoid memory leaks.
- **Licensing & Policy:** Single-player RoR2 modding permitted by Hopoo Games/Gearbox. Local user-owned audio extraction only. Zero VAC interaction (RoR2 does not use VAC; BO3 is not launched).
- **Major Technical Risks:** Unity assembly version conflicts between BepInEx plugins and game assemblies (`RSK-10`).
- **Architectural Learning Value:** **Critical.** Proves that NexLudic Engine's adapter contracts (`IGameAdapter`, `IModScaffoldable`) can scaffold and manage a .NET/C# MSBuild ecosystem completely independent of Java/JVM.
- **Portfolio Overlap:** None (Diverges completely in engine, language, and toolchain).
- **Preliminary Readiness Status:** **PILOT SHORTLIST (Archetype B Baseline)**.
- **Outstanding Research:** BepInEx scaffolding template integration into `NexLudic.Core.Scaffolding`.
- **Rationale for Advance:** Essential to prove language and engine independence (Unity C# vs. Java JVM).

---

### Candidate ID: `CAND-PAIR-003`
- **Game A:** *Rocket League* (Psyonix / Unreal Engine 3 Custom) — Target Version: Latest Epic/Steam Release (Offline Freeplay / Exhibition).
- **Game B:** *Cyberpunk 2077* (CD Projekt Red / REDengine 4) — Target Version: 2.1x (Offline Single-Player).
- **Proposed Integration Outcome:** Cross-Game Reactive Telemetry Bridge: Real-time dual-monitor reactive session. Player actions in Rocket League (supersonic boosting, aerial goals, demo explosions) stream telemetry at 60 Hz to Cyberpunk 2077, dynamically triggering in-game environmental distortion effects (Relic glitch, weather storms, wanted level escalation, vehicle spawning).
- **Potential Host Game:** Neither (Symmetric dual-game live bridge).
- **Candidate Integration Approach:** **Strategy B (Live Interoperability Bridge)**.
- **Relevant Extension Surfaces:**
  - Rocket League: BakkesMod C++ Plugin SDK (`HookEvent`, `CarWrapper.GetBoost()`, `BallWrapper.GetLocation()`).
  - Cyberpunk 2077: Cyber Engine Tweaks (CET 1.32+ Lua runtime, `Game.GetFxPC()`, `Game.GetWeatherSystem()`).
  - Transport: Windows Named Pipes (`\\.\pipe\nexludic_rl_cp`) managed by `NexLudic.Core.IPC`.
- **Required Languages & Toolchains:** C++ 20 (MSVC v143 for BakkesMod plugin), Lua 5.1 (CET script), C# 12 (.NET 8 daemon/engine).
- **Primary Dependencies & Maintenance:** BakkesMod SDK (Active, Tier 3), Cyber Engine Tweaks (Active, Tier 3).
- **Evidence Quality & Sources:**
  - [BakkesMod Programming Wiki & SDK](https://wiki.bakkesplugins.com/) (Tier 3).
  - [Cyber Engine Tweaks Wiki](https://wiki.redmodding.org/cyber-engine-tweaks/) (Tier 3).
- **Known Constraints & Questions:**
  - Workstation GPU hardware constraint: Running both Rocket League and Cyberpunk 2077 simultaneously requires $\ge 6\text{ GB}$ VRAM. Dev workstation currently has 2 GB Quadro K420 (`BLOCK-003`). Initial validation must use a mock headless sender/receiver harness.
- **Licensing & Policy:** BakkesMod is officially whitelisted by Psyonix for offline/freeplay use. Cyber Engine Tweaks is fully sanctioned by CD Projekt Red for single-player.
- **Major Technical Risks:** Window focus and audio contention between two fullscreen/borderless 3D games (`RSK-15`); process crash in Game B destabilizing the IPC pipe (`RSK-07`).
- **Architectural Learning Value:** **Massive.** Validates out-of-process IPC streaming, Win32 Job Object containment, named pipe latency ($\le 16.6\text{ ms}$), and disconnection fault tolerance.
- **Portfolio Overlap:** None (Only live dual-process bridge candidate on the shortlist).
- **Preliminary Readiness Status:** **PILOT SHORTLIST (Archetype C Baseline; Hardware Gated)**.
- **Outstanding Research:** Named Pipe latency benchmarking under simulated CPU load.
- **Rationale for Advance:** Indispensable for validating Strategy B (Live Bridge) architecture.

---

### Candidate ID: `CAND-PAIR-004`
- **Game A (Host):** *Minecraft: Java Edition* (Mojang / JVM) — Target Version: 1.21.x.
- **Game B (External Context):** *Marvel's Spider-Man Remastered* (Insomniac Games / Sony) — Version: v1.11+.
- **Proposed Integration Outcome:** Traversal & Momentum Mechanics in Voxel World: Implement Spider-Man web-swinging momentum, spring-damping physics, web-zip targeting, costume HUD, and directional velocity FOV dilation in Minecraft, with an optional background telemetry reader monitoring Spider-Man player state.
- **Potential Host Game:** Game A (Minecraft Java Edition).
- **Candidate Integration Approach:** **Strategy C (Hybrid Integration: Host Mod + Telemetry Hook)**.
- **Relevant Extension Surfaces:**
  - Minecraft: FabricMC 0.16.x, player motion vector manipulation, raycast collision physics.
  - Spider-Man Remastered: Window state inspection, user input state, memory telemetry (read-only single-player offline).
- **Required Languages & Toolchains:** Java 21 LTS, Fabric Loom, C# 12 helper daemon, Win32 API.
- **Primary Dependencies & Maintenance:** FabricMC (Active, Tier 3), Win32 `User32.dll` APIs.
- **Evidence Quality & Sources:**
  - [FabricMC Vector & Physics API](https://fabricmc.net/) (Tier 3).
  - Historical QPIS Reference Transcripts (Tier 1/2).
- **Known Constraints & Questions:**
  - Real-time 3D voxel injection into Spider-Man's proprietary engine at 60 FPS is **formally rejected** as unverified and unfeasible (`ADR-0004`, `RSK-04`). All rendering and traversal occur inside Minecraft.
- **Licensing & Policy:** Single-player offline use only. Spider-Man assets extracted locally from user install.
- **Major Technical Risks:** Unrealistic user expectations regarding bidirectional 3D mesh transfer.
- **Architectural Learning Value:** Validates composite adapter orchestration where an in-engine mod handles local rendering and physics while an out-of-process daemon synchronizes external session data.
- **Portfolio Overlap:** Partial overlap with `CAND-PAIR-001` on Minecraft host, but introduces hybrid external communication.
- **Preliminary Readiness Status:** **PILOT SHORTLIST (Archetype D Baseline)**.
- **Outstanding Research:** Bounding external session state hooks to non-invasive Win32 APIs.
- **Rationale for Advance:** Exercises Strategy C (Hybrid) without requiring impossible cross-engine memory injection.

---

### Candidate ID: `CAND-PAIR-005`
- **Game A:** *Minecraft: Java Edition* (1.21).
- **Game B:** *Call of Duty: Black Ops III* (Steam Release).
- **Proposed Integration Outcome:** Live memory hooking and event extraction directly from running BO3 multiplayer/co-op game process into Minecraft.
- **Candidate Integration Approach:** Strategy B (Live Bridge via direct memory hook).
- **Relevant Extension Surfaces:** Unofficial memory scanners / DLL injection.
- **Licensing & Policy Evaluation:** **FATAL POLICY VIOLATION.**
  - Call of Duty: Black Ops III runs Valve Anti-Cheat (VAC).
  - Attempting to attach debuggers, read live memory offsets, or inject DLLs into the running Steam BO3 process triggers automated, permanent VAC bans on the user's Steam account.
- **Preliminary Readiness Status:** ❌ **REJECTED (Mandatory Exclusion: Anti-Cheat Blocker `RSK-01`).**
- **Rationale for Rejection:** Direct violation of `REQ-SAFE-001` and Exclusion Rule 1. Cannot be mitigated.

---

### Candidate ID: `CAND-PAIR-006`
- **Game A (Host):** *Deep Rock Galactic* (Ghost Ship Games / Unreal Engine 4.27) — Target Version: Steam Release.
- **Game B (Source):** *Left 4 Dead 2* (Valve / Source Engine) — Target Version: Latest Steam.
- **Proposed Integration Outcome:** AI Director Swarm Mechanics in UE4: Implement dynamic horde pacing, stress-based mob spawning curves, and special infected audio cues inside Deep Rock Galactic.
- **Potential Host Game:** Game A (Deep Rock Galactic).
- **Candidate Integration Approach:** **Strategy A (Host-Game Modification)**.
- **Relevant Extension Surfaces:**
  - DRG: Unreal Engine Mod Loader (DRG Modding / Mod.io), UE4SS (Unreal Engine 4 Scripting System / C++ / Lua).
  - L4D2: VScript documentation, audio cues.
- **Required Languages & Toolchains:** C++, Unreal Engine 4.27 build tools, UE4SS, Lua.
- **Evidence Quality & Sources:**
  - [Unreal Engine Modding System / UE4SS Documentation](https://docs.ue4ss.com/) (Tier 3).
  - [DRG Modding Guide](https://mod.io/g/drg) (Tier 2/3).
- **Known Constraints & Questions:**
  - DRG utilizes an integrated modding menu via mod.io; sandbox verification needed for custom native DLL hooks.
- **Preliminary Readiness Status:** **RESEARCH QUALIFIED (Candidate for Phase 2 Portfolio Expansion).**
- **Rationale for Deferral:** High architectural value (Unreal Engine 4), but deferred to Phase 2 to allow Phase 1 pilots (JVM, Unity, REDengine) to establish the core baseline first.

---

### Candidate ID: `CAND-PAIR-007`
- **Game A:** *Cities: Skylines* (Colossal Order / Unity Engine) — Target Version: 1.17+.
- **Game B:** *Grand Theft Auto V* (Rockstar Games / RAGE Engine) — Target Version: Single-Player offline.
- **Proposed Integration Outcome:** Live Traffic Simulation Bridge: City traffic density, road grid layout, and emergency vehicle dispatch events simulated in Cities: Skylines stream live into GTA V to control ambient vehicle density and police response.
- **Candidate Integration Approach:** **Strategy B (Live Interoperability Bridge)**.
- **Relevant Extension Surfaces:** Cities: Skylines Modding API (C#), ScriptHookV (C++ native for GTA V single-player).
- **Preliminary Readiness Status:** **DEFERRED (Hardware & Resource Constraints).**
- **Rationale for Deferral:** Running Cities: Skylines (notorious for high RAM/CPU load) alongside GTA V requires $>32\text{ GB}$ system RAM and an 8-core CPU, far exceeding standard test workstation baselines (`BLOCK-003`). Deferred until hardware-agnostic mock harnesses are proven.

---

### Candidate ID: `CAND-PAIR-008`
- **Game A:** *Cyberpunk 2077* (REDengine 4).
- **Game B:** *Marvel's Spider-Man Remastered* (Insomniac Engine).
- **Proposed Integration Outcome:** Real-time dual-engine 3D mesh and physics synchronization: inject Spider-Man character model and web-swinging collision meshes directly into Cyberpunk 2077's native rendering and physics pipelines at 60 FPS.
- **Candidate Integration Approach:** Strategy B / Strategy C.
- **Preliminary Readiness Status:** ❌ **REJECTED (Technical Impossibility & Extreme Instability `RSK-04`).**
- **Rationale for Rejection:** Both engines are closed, proprietary native architectures with zero public APIs for runtime 3D physics mesh injection. Attempting dual-process frame-synchronized memory injection without source access causes immediate hard memory access violations (`0xC0000005`) and engine panics. Fails Mandatory Exclusion Rule 2.

---

## 4. Register Maintenance Protocol

1. **Review Frequency:** The Candidate Register is updated at every project gate milestone.
2. **Re-classification Triggers:** Any game patch that alters an integration surface, any deprecation of an underlying mod loader, or any change in anti-cheat enforcement triggers an immediate re-evaluation of candidate readiness status.

---

*End of Game-Pair Candidate Register (NLE-REG-CAND-001).*

