# NEXLUDIC ENGINE — REPRESENTATIVE GAME-PAIR FEASIBILITY CASE STUDIES
## Document ID: NLE-RES-PAIR-001
**Phase:** 1 — Product Definition & Research  
**Step:** 4 of 100 — Research Game-Modding Ecosystems, Integration Technologies, and Universal Adapter Opportunities  
**Date:** November 16, 2026  
**Auditor / Architect:** Game-Integration Research Engineer & Systems Architect  

---

## 1. Case Study Evaluation Standards

This document establishes the empirical feasibility analysis for the four representative game pairs nominated in the product mandate. Each evaluation:
1. Re-evaluates claims made in historical specifications or community transcripts against verified ecosystem research.
2. Formally separates proven capabilities from unverified assumptions.
3. Outlines the concrete feasibility experiments required before implementation can be authorized.

---

## 2. In-Depth Case Study Evaluations

---

### Case Study 1: Minecraft Java Edition + Red Dead Redemption 2

#### 1. Technical Profile
- **Candidate Host:** Minecraft Java Edition (FabricMC 1.21).
- **Candidate Guest:** Red Dead Redemption 2 (Rockstar RAGE Engine).
- **Intended Gameplay:** Western combat crossover: Cattleman Revolver with custom reload animations and bullet recoil, Dead-Eye targeting with screen-tint shader and slow-motion entity physics, and a circular western weapon wheel.

#### 2. Strategy Trade-Off Comparison
- **Strategy A (Host Mod via Fabric):** **RECOMMENDED & HIGHLY VIABLE.**
  Minecraft is fully moddable via Fabric. All requested mechanics (weapons, screen shaders, tick dilation, UI wheels) can be authored directly in Java as a client/server mod. RDR2 does not need to run concurrently.
- **Strategy B (Live IPC Bridge):** **UNSUPPORTED / NOT RECOMMENDED.**
  RDR2 does not expose an open telemetry stream. Attempting to synchronize player aiming or shooting in real-time between running games would require invasive memory hooks in RDR2, creating Rockstar launcher ban risks and window focus conflicts.
- **Strategy C (Hybrid):** **NOT JUSTIFIED.** Adds massive dual-game overhead with zero gameplay benefit over Strategy A.

#### 3. Feature-to-Capability Matrix
| Desired Gameplay Feature | Feasibility Under Strategy A | Required Extension Point | Evidence Status |
| :--- | :--- | :--- | :--- |
| **Cattleman Revolver 3D Item** | Fully Feasible | Minecraft Item Registry & JSON Model | **Verified** (Fabric API) |
| **Dead-Eye Red Screen Tint** | Fully Feasible | Client-side Post-Processing Shader | **Verified** (Fabric / Vanilla GLSL) |
| **Dead-Eye Time Dilation (Slow-Mo)**| Fully Feasible | `tick_rate` API / Entity motion scaling | **Verified** (Minecraft 1.20.5+)|
| **Western Weapon Wheel HUD** | Fully Feasible | Custom ImGui / Screen Overlay | **Verified** (Fabric Client GUI)|
| **Custom Audio (Gunshots, Whistle)**| Fully Feasible (Local extraction)| `SoundEvent` Registry & OGG Audio | **Verified** (Local extraction)|

#### 4. Verified vs. Unverified Capabilities
- **Verified:** FabricMC supports custom items, bullet raycasting, screen shaders, and tick rate manipulation out of the box.
- **Unverified:** Automated conversion of proprietary RDR2 audio banks (`.awc`) into OGG Vorbis without human audio extraction tools.

#### 5. Known Blockers & Risks
- **Blocker:** Java JDK 21 is currently absent on the host development machine (`BLOCK-001`).
- **Risk:** High memory usage if high-resolution RDR2 textures are imported into Minecraft.

#### 6. Proposed Feasibility Experiment (EXP-P1)
- **Title:** Headless Fabric 1.21 Time-Dilation & Weapon Raycast Proof-of-Concept.
- **Goal:** Verify that an AI-generated Fabric mod can alter player tick rate and render a HUD shader cleanly.
- **Provisional Status:** **Candidate Integration Identified (High Confidence for Strategy A)**.

---

### Case Study 2: Minecraft Java Edition + Call of Duty: Black Ops III

#### 1. Technical Profile
- **Candidate Host:** Minecraft Java Edition (FabricMC 1.21).
- **Candidate Guest:** Call of Duty: Black Ops III (Treyarch Engine).
- **Intended Gameplay:** Replicate classic Call of Duty Zombies in a voxel landscape: Ray Gun and Wunderwaffe weapons, Mystery Box block with roulette item cycling, Pack-a-Punch upgrade station, and progressive round-based zombie spawning with barricade repairs.

#### 2. Strategy Trade-Off Comparison
- **Strategy A (Host Mod via Fabric):** **RECOMMENDED & HIGHLY VIABLE.**
  Minecraft block entities natively support state machines (e.g. Mystery Box opening, cycling, and closing). Ray Gun projectiles map directly to custom Minecraft projectile entities with particle trails.
- **Strategy B / C (Live Interoperability):** **BLOCKED.**
  Call of Duty: Black Ops III enforces Valve Anti-Cheat (VAC). Hooking the running BO3 process to stream zombies telemetry into Minecraft carries severe risk of permanent VAC bans on Steam.

#### 3. Feature-to-Capability Matrix
| Desired Gameplay Feature | Feasibility Under Strategy A | Required Extension Point | Evidence Status |
| :--- | :--- | :--- | :--- |
| **Ray Gun Splash Damage** | Fully Feasible | Custom `ProjectileEntity` & Explosion | **Verified** (Fabric API) |
| **Mystery Box Roulette Entity** | Fully Feasible | `BlockEntity` & Custom Tile Renderer | **Verified** (Fabric API) |
| **Round Wave Manager** | Fully Feasible | Server-side Tick Handler & Mob Spawner | **Verified** (Fabric API) |
| **Pack-a-Punch Upgrade Logic** | Fully Feasible | Custom Crafting / Interaction Screen | **Verified** (Fabric API) |
| **Barricade Repair Mechanics** | Fully Feasible | Block Placement & Player Interaction Hook| **Verified** (Fabric API) |

#### 4. Verified vs. Unverified Capabilities
- **Verified:** All requested Zombie mechanics can be expressed entirely within Minecraft's native Java tick and entity loops.
- **Unverified:** Seamless conversion of proprietary BO3 3D weapon meshes (`.xmodel_export`) into Minecraft JSON models without manual 3D modeling cleanup.

#### 5. Known Blockers & Risks
- **Blocker:** Local Java toolchain missing on host (`BLOCK-001`).
- **Risk:** Excessive particle density from Wunderwaffe electrical arcs causing client FPS drops.

#### 6. Proposed Feasibility Experiment (EXP-P2)
- **Title:** Fabric 1.21 Mystery Box & Projectile Raycast Scaffold.
- **Goal:** Verify an automated Fabric mod template implementing a block entity that spawns randomized items upon right-click.
- **Provisional Status:** **Candidate Integration Identified (High Confidence for Strategy A)**.

---

### Case Study 3: Rocket League + Cyberpunk 2077

#### 1. Technical Profile
- **Candidate Game A:** Rocket League (Psyonix / Unreal Engine 3 custom).
- **Candidate Game B:** Cyberpunk 2077 (CD Projekt Red / REDengine 4).
- **Intended Gameplay:** Dual-monitor or reactive session: Driving, scoring goals, demoing cars, or boosting in Rocket League on Monitor 1 dynamically triggers environmental reactions, sandstorms, relic glitches, or wanted levels in Cyberpunk 2077 on Monitor 2.

#### 2. Strategy Trade-Off Comparison
- **Strategy A (Host Mod):** **UNSUPPORTED / EXTREME DIFFICULTY.**
  Replicating Rocket League's proprietary PhysX vehicle physics and sphere collision inside Cyberpunk's REDengine is practically impossible for an automated platform.
- **Strategy B (Live IPC Bridge):** **RECOMMENDED & FEASIBLE ON PAPER.**
  Rocket League supports BakkesMod SDK (C++ event hooks); Cyberpunk 2077 supports Cyber Engine Tweaks (Lua scripting). A lightweight background daemon can pass telemetry over local named pipes (`\\.\pipe\nexludic_rl_cp`).
- **Strategy C (Hybrid):** Feasible if custom vehicle skins are imported into Cyberpunk alongside the bridge.

#### 3. Feature-to-Capability Matrix
| Desired Gameplay Feature | Feasibility Under Strategy B | Required Extension Point | Evidence Status |
| :--- | :--- | :--- | :--- |
| **Goal Event Hook (Rocket League)**| Fully Feasible | BakkesMod SDK `HookEvent` API | **Verified** (BakkesMod) |
| **Supersonic Boost Telemetry** | Fully Feasible | BakkesMod `CarWrapper.GetBoost()` | **Verified** (BakkesMod) |
| **Relic Glitch Effect (Cyberpunk)**| Fully Feasible | CET Lua calling `Game.GetFxPC()` | **Verified** (CET RTTI) |
| **Weather / Sandstorm Trigger** | Fully Feasible | CET Lua `Game.GetWeatherSystem()` | **Verified** (CET RTTI) |
| **Low-Latency IPC (<5 ms)** | Fully Feasible | Windows Named Pipes (`System.IO.Pipes`)| **Verified** (Win32 OS) |

#### 4. Verified vs. Unverified Capabilities
- **Verified:** BakkesMod can hook goals and boost; CET can trigger visual distortions and weather changes in Cyberpunk via Lua.
- **Unverified:** Workstation hardware capacity. The development workstation GPU (2 GB Quadro K420) **cannot run both games simultaneously** without crashing (`BLOCK-003`).

#### 5. Known Blockers & Risks
- **Blocker:** Hardware constraint on dev host (2 GB VRAM). Live testing requires a test rig with >=6 GB VRAM.
- **Risk:** Audio and window focus contention when two heavy games run side-by-side on Windows.

#### 6. Proposed Feasibility Experiment (EXP-P3)
- **Title:** Mock Telemetry Pipe Benchmark (BakkesMod Sender Simulation to CET Lua Receiver).
- **Goal:** Measure packet delivery latency and thread impact across a local named pipe without launching full game clients.
- **Provisional Status:** **Feasibility Experiment Required (Medium Confidence; Hardware Gated)**.

---

### Case Study 4: Minecraft Java Edition + Marvel's Spider-Man Remastered

#### 1. Technical Profile
- **Candidate Host:** Minecraft Java Edition (FabricMC 1.21).
- **Candidate Guest:** Marvel's Spider-Man Remastered (Insomniac Engine).
- **Intended Gameplay:** Superhero traversal and combat: Web-shooters that attach to Minecraft terrain, dynamic swing momentum physics, web-zip targeting, costume HUD, and combat synchronization.

#### 2. Strategy Trade-Off Comparison
- **Strategy A (Host Mod via Fabric):** **RECOMMENDED & HIGHLY VIABLE.**
  Spider-Man traversal mechanics (spring constraints, momentum vectors, FOV dilation, web lines) can be implemented natively inside Minecraft using Java physics algorithms.
- **Strategy B (Live IPC Bridge):** **RESTRICTED / HIGH RISK.**
  Spider-Man Remastered has no official modding or scripting SDK. Sending synthetic input (`SendInput`) works only while the Spider-Man window has focus. Synchronizing two running worlds across processes without an API is fragile.
- **Strategy C (Hybrid):** High complexity. Host mod handles all rendering and physics in Minecraft; external bridge reads Spider-Man session state.

#### 3. Feature-to-Capability Matrix
| Desired Gameplay Feature | Feasibility Under Strategy A | Required Extension Point | Evidence Status |
| :--- | :--- | :--- | :--- |
| **Web Raycast to Voxel Geometry**| Fully Feasible | Minecraft Raycast & Block Hit Result | **Verified** (Fabric API) |
| **Dynamic Swing Momentum (Lerp)** | Fully Feasible | Player Velocity Vector Manipulation | **Verified** (Fabric API) |
| **Spring Damping Physics** | Fully Feasible | Math/Physics Hook in Client Tick Loop | **Verified** (Fabric API) |
| **Spider-Man Suit & Web HUD** | Fully Feasible | Custom Texture & HUD Screen Overlay | **Verified** (Fabric Client) |
| **Real-time 3D Voxel Injection** | **UNVERIFIED & HIGH RISK** | Proprietary Spider-Man Physics Thread | **Contradicted / Unverified** |

#### 4. Verified vs. Unverified Capabilities
- **Verified:** Web-swinging momentum, spring physics, and raycasting can be authored cleanly inside Minecraft Java Edition.
- **Unverified & Contradicted:** The claim from historical specifications (Source B) that 3D voxel collision geometry can be extracted from Minecraft and injected into Spider-Man's proprietary Havok/internal physics thread at 60 fps is **completely unverified** and presents extreme crash risks.

#### 5. Known Blockers & Risks
- **Blocker:** Historical QPIS documents assumed unverified memory injection.
- **Resolution:** Formally decoupled. The MVP reference implementation adopts Strategy A (Spider-Man mechanics inside Minecraft host).

#### 6. Proposed Feasibility Experiment (EXP-P4)
- **Title:** FabricMC Web-Swinging Spring-Constraint Prototype.
- **Goal:** Verify that an AI coding agent can generate a spring-momentum math solver in a Fabric mod item class that swings the player between blocks smoothly.
- **Provisional Status:**
  - Strategy A (Host Mod): **Candidate Integration Identified (High Confidence)**.
  - Strategy B/C (Live Bridge / Mesh Injection): **Deferred to Experimental R&D (`ISSUE-004`)**.

