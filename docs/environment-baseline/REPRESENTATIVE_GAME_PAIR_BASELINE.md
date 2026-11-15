# NEXLUDIC ENGINE — REPRESENTATIVE GAME-PAIR COMPATIBILITY BASELINE
## Document ID: NLE-DOC-PAIR-001
**Phase:** 1 — Product Definition & Research  
**Step:** 3 of 100 — Development Environment Audit, Toolchain Compatibility, and Initial Game-Pair Baseline  
**Date:** November 15, 2026  
**Auditor / Architect:** Game-Integration Research Engineer & Compatibility Lead  

---

## 1. Compatibility Status Taxonomy

In strict accordance with Step 3 specifications, all game-pair evaluations employ an 8-state evidence-based status model:

```
+----------------------------------------------------------------------------------------------------+
|                                    COMPATIBILITY STATUS TAXONOMY                                   |
+------------------------------------+---------------------------------------------------------------+
| Status State                       | Operational Meaning & Evidence Requirement                    |
+------------------------------------+---------------------------------------------------------------+
| 1. Not Inspected                   | Pair has not been analyzed or indexed by the platform.        |
| 2. Discovery Incomplete            | Partial files located; version/launcher metadata unresolved.  |
| 3. Candidate Integration Identified| Technical mechanism identified on paper; no tests executed.   |
| 4. Feasibility Experiment Required | High uncertainty; isolated proof-of-concept required.         |
| 5. Partially Validated             | Scaffolding or sub-mechanic compiles; gameplay unverified.    |
| 6. Validated for Specific Config   | Verified end-to-end (Build, Launch, Interact) on tested build.|
| 7. Unsupported                     | Modification or bridge prohibited by technical barriers/DRM.  |
| 8. Blocked                         | Missing critical information, toolchain, or anti-cheat check. |
+------------------------------------+---------------------------------------------------------------+
```

---

## 2. Representative Game-Pair In-Depth Baselines

---

### Pair 1: Minecraft Java Edition + Red Dead Redemption 2
- **Source of Example:** YouTube Demonstration Transcript (Source C: Claude Code + Melty mashup).
- **Intended Mashup Outcome:** Introduce iconic Red Dead Redemption 2 western elements into Minecraft: Cattleman Revolver with custom reload animations, Dead-Eye targeting with screen-tint shader and slow-motion entity physics, and a western weapon-selection wheel.

```
+-----------------------------------------------------------------------------+
|               PAIR 1 ARCHITECTURAL STRATEGY EVALUATION                      |
+-----------------------------------------------------------------------------+
|                                                                             |
|  [STRATEGY A: HOST-GAME MODIFICATION (RECOMMENDED & VIABLE)]                |
|  - Host Runtime: Minecraft Java Edition (Fabric Loader 1.21)                |
|  - Guest Source: RDR2 (Audio and weapon asset specifications)               |
|  - Execution: Mod runs 100% inside Minecraft; RDR2 process NOT running.     |
|  - Feasibility: High. Fabric API provides custom entity, item, sound,       |
|    and client-side post-processing shader hooks.                            |
|                                                                             |
|  [STRATEGY B: LIVE INTEROPERABILITY (NOT RECOMMENDED)]                      |
|  - Execution: Dual running games. RDR2 lacks an open telemetry API;         |
|    reading RDR2 player aim/shooting in real-time requires invasive memory    |
|    hooks, risking Rockstar launcher anti-cheat/account flags.              |
|                                                                             |
|  [STRATEGY C: HYBRID (NOT RECOMMENDED)]                                     |
|  - High complexity with zero gameplay justification over Strategy A.        |
|                                                                             |
+-----------------------------------------------------------------------------+
```

- **Toolchain Requirements:** JDK 21 (LTS), Gradle 8.8+, Fabric Loader 0.16+, Fabric API, local audio extraction tool.
- **Known Technical Uncertainty:** Client-side tick manipulation in multiplayer servers; GLSL post-processing shader performance across diverse player GPUs.
- **Available Evidence:** Verified FabricMC API documentation; community transcript narration of working prototype.
- **Missing Evidence:** Source repository commits from YouTube demonstration; exact prompt chains used by original creator.
- **Proposed Next Feasibility Experiment:** Scaffold a minimal headless Fabric 1.21 mod with a custom `RevolverItem` that applies a FOV change and client-side time dilator (`tick_rate` adjustment in Minecraft 1.20.5+).
- **Current Compatibility Status:** **Candidate Integration Identified** (Strategy A).

---

### Pair 2: Minecraft Java Edition + Call of Duty: Black Ops 3
- **Source of Example:** YouTube Demonstration Transcript (Source C: Claude Code + Melty mashup).
- **Intended Mashup Outcome:** Replicate Call of Duty Zombies mechanics within a Minecraft world: Ray Gun and Wunderwaffe weapons, procedural Mystery Box item spawning with roulette cycle, Pack-a-Punch weapon upgrade system, and round-based zombie horde spawning algorithms.

```
+-----------------------------------------------------------------------------+
|               PAIR 2 ARCHITECTURAL STRATEGY EVALUATION                      |
+-----------------------------------------------------------------------------+
|                                                                             |
|  [STRATEGY A: HOST-GAME MODIFICATION (RECOMMENDED & VIABLE)]                |
|  - Host Runtime: Minecraft Java Edition (Fabric Loader 1.21)                |
|  - Execution: Custom Fabric mod implementing custom entity projectile rays, |
|    mystery box block entity with animated roulette, and wave manager.       |
|  - Feasibility: High. Supported natively by standard Minecraft block entity |
|    state machines and custom projectile entity classes.                     |
|                                                                             |
|  [STRATEGY B / C: LIVE INTEROPERABILITY (BLOCKED)]                          |
|  - BO3 on PC features VAC (Valve Anti-Cheat) in protected lobbies.          |
|    Cross-process memory hooking or synthetic input injection is prohibited. |
|                                                                             |
+-----------------------------------------------------------------------------+
```

- **Toolchain Requirements:** JDK 21 (LTS), Gradle 8.8+, Fabric Loader, Block Entity renderer models.
- **Known Technical Uncertainty:** Particle physics density for laser weapons causing client-side frame drops.
- **Available Evidence:** Verified Fabric block entity documentation; community video transcript evidence.
- **Missing Evidence:** Exact 3D models and sound format converters used in the demonstration.
- **Proposed Next Feasibility Experiment:** Generate a headless Fabric mod containing a custom `MysteryBoxBlock` that cycles through registered item IDs upon right-click.
- **Current Compatibility Status:** **Candidate Integration Identified** (Strategy A).

---

### Pair 3: Rocket League + Cyberpunk 2077
- **Source of Example:** YouTube Demonstration Transcript (Source C: Claude Code + Melty mashup).
- **Intended Mashup Outcome:** Interactive telemetry-driven mashup: Scoring a goal, demoing an opponent, or activating supersonic rocket boost in Rocket League immediately triggers environmental visual reactions, sandstorms, relic glitches, or wanted levels in an active Cyberpunk 2077 session.

```
+-----------------------------------------------------------------------------+
|               PAIR 3 ARCHITECTURAL STRATEGY EVALUATION                      |
+-----------------------------------------------------------------------------+
|                                                                             |
|  [STRATEGY A: HOST-GAME MODIFICATION (HIGH DIFFICULTY)]                     |
|  - Recreating Rocket League's complex PhysX vehicle sphere-physics inside   |
|    Cyberpunk's REDengine vehicle framework is extremely difficult.          |
|                                                                             |
|  [STRATEGY B: LIVE INTEROPERABILITY (RECOMMENDED & VIABLE)]                 |
|  - Game A: Rocket League running with BakkesMod C++ plugin.                 |
|  - Game B: Cyberpunk 2077 running with Cyber Engine Tweaks (CET).           |
|  - Bridge: NexLudic Named Pipe daemon routing match events to CET Lua hook. |
|  - Feasibility: High. BakkesMod SDK officially supports match event hooks;  |
|    CET Lua supports triggering game events and HUD distortions.             |
|                                                                             |
|  [STRATEGY C: HYBRID (OPTIONAL)]                                            |
|  - Adding a custom Rocket League vehicle skin in Cyberpunk alongside bridge.|
|                                                                             |
+-----------------------------------------------------------------------------+
```

- **Toolchain Requirements:** C++20 compiler (MSVC), BakkesMod SDK, Lua 5.1 runtime, Cyber Engine Tweaks (CET) installed in Cyberpunk. High-end multi-core PC with dedicated GPU (>=6GB VRAM).
- **Known Technical Uncertainty:** Severe GPU/VRAM bottleneck on entry-level hardware (e.g. dev workstation with 2GB VRAM cannot run both simultaneously); window focus and audio contention between games.
- **Available Evidence:** Verified BakkesMod SDK repository (`bakkesmodorg/BakkesModSDK`); verified Cyber Engine Tweaks repository (`maximegmd/CyberEngineTweaks`).
- **Missing Evidence:** Transcript does not detail the exact IPC transport or latency measurements achieved.
- **Proposed Next Feasibility Experiment:** Author a standalone mock C++ telemetry sender simulating BakkesMod events over a Windows Named Pipe to a CET Lua receiver script.
- **Current Compatibility Status:** **Feasibility Experiment Required** (Strategy B; gated by hardware capacity).

---

### Pair 4: Minecraft Java Edition + Marvel's Spider-Man Remastered
- **Source of Example:** GameWeaver Bridge QPIS (Source A) & Gemini Spider-Craft AI QPIS (Source B).
- **Intended Mashup Outcome:** Web-slinging traversal and superhero combat mechanics operating in a voxel landscape, combined with dynamic telemetry exchange between processes.

```
+-----------------------------------------------------------------------------+
|               PAIR 4 ARCHITECTURAL STRATEGY EVALUATION                      |
+-----------------------------------------------------------------------------+
|                                                                             |
|  [STRATEGY A: HOST-GAME MODIFICATION (HIGHLY FEASIBLE)]                     |
|  - Host Runtime: Minecraft Java Edition (Fabric Loader).                    |
|  - Execution: Custom web-shooter item raycasts to block surfaces, attaches   |
|    a visual spring-constraint entity, applies momentum vectors to player,    |
|    and dilates FOV. Single game execution.                                   |
|                                                                             |
|  [STRATEGY B: LIVE INTEROPERABILITY (RESTRICTED FEASIBILITY)]               |
|  - Spider-Man Remastered has no official telemetry or modding API.          |
|    Synthetic input dispatch (`SendInput`) works only while Spider-Man window |
|    has focus. Window focus conflicts prevent dual active play.              |
|                                                                             |
|  [STRATEGY C: HYBRID (HIGH COMPLEXITY)]                                     |
|  - Fabric mod in Minecraft receives combat event flags from Spider-Man hook.|
|                                                                             |
+-----------------------------------------------------------------------------+
```

- **Toolchain Requirements:** JDK 21, Gradle, Fabric Loom, Windows user-mode input libraries.
- **Known Technical Uncertainty:** Historical claim of real-time 3D voxel terrain injection into Spider-Man physics engine is classified as **Unverified & High-Risk** (`ISSUE-004`).
- **Available Evidence:** FabricMC physics manipulation APIs verified; SMPC Tool exists for Spider-Man asset modding.
- **Missing Evidence:** Zero public evidence of Spider-Man Remastered accepting dynamic collision mesh injection without crashing.
- **Proposed Next Feasibility Experiment:** Implement Strategy A purely within FabricMC (custom web-swinging item with spring physics) as the reference implementation for Step 5 / MVP.
- **Current Compatibility Status:**
  - Strategy A (Host Mod): **Candidate Integration Identified**.
  - Strategy B/C (Live Bridge / Mesh Injection): **Feasibility Experiment Required (Deferred to R&D)**.

---

## 3. Summary Assessment Matrix

| Game Pair | Dominant Strategy | Core Host / Target | Toolchain Needed | Hardware Tier | Feasibility Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Minecraft + RDR2** | Strategy A (Host Mod) | Minecraft (Fabric 1.21) | JDK 21, Gradle | Standard (4GB RAM) | Candidate Integration Identified |
| **2. Minecraft + BO3** | Strategy A (Host Mod) | Minecraft (Fabric 1.21) | JDK 21, Gradle | Standard (4GB RAM) | Candidate Integration Identified |
| **3. Rocket League + CP2077**| Strategy B (Live Bridge) | Dual: BakkesMod + CET | C++20, Lua, IPC | High-End (>=6GB VRAM)| Feasibility Experiment Required |
| **4. Minecraft + Spider-Man**| Strategy A (Host Mod) | Minecraft (Fabric 1.21) | JDK 21, Gradle | Standard (4GB RAM) | Candidate Integration Identified |

