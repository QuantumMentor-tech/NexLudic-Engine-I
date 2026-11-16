# NEXLUDIC ENGINE — INTEGRATION RESEARCH RISKS & DECISIONS
## Document ID: NLE-RES-RISK-001
**Phase:** 1 — Product Definition & Research  
**Step:** 4 of 100 — Research Game-Modding Ecosystems, Integration Technologies, and Universal Adapter Opportunities  
**Date:** November 16, 2026  
**Auditor / Architect:** Senior Security Engineer & Licensing Analyst  

---

## 1. Tooling & Ecosystem Risk Register

The structured research conducted in Step 4 identified five material risk domains that govern NexLudic Engine's architecture:

```
+----------------------------------------------------------------------------------------------------+
|                                    MATERIAL RISK DOMAINS                                           |
+----+----------------------------+----------+-------------------------------------------------------+
| ID | Risk Description           | Severity | Mitigation Strategy & Platform Boundary               |
+----+----------------------------+----------+-------------------------------------------------------+
| R1 | Anti-Cheat Account Bans    | CRITICAL | Hard block on games with kernel anti-cheat; prohibit  |
|    | (VAC, BattlEye, Vanguard)  |          | memory injection in online multiplayer modes.         |
| R2 | Copyright / EULA Infringement| HIGH   | Strict Non-Distributive Asset Policy (REQ-MOD-003);   |
|    | (Redistributing raw assets)|          | local user-driven extraction scripts only.             |
| R3 | Deprecated Kernel Drivers  | HIGH     | Rejection of ViGEmBus (.sys); prioritize user-mode    |
|    | (ViGEmBus archived)        |          | SendInput and mod-level input hooks (DEC-003).        |
| R4 | Unverified Physics Claims  | HIGH     | Defer cross-process 3D mesh injection to Phase 12     |
|    | (Spider-Craft 3D injection)|          | Experimental R&D; prioritize Strategy A host mods.    |
| R5 | Rogue AI Agent Execution   | CRITICAL | Staged workspace sandbox (<project>/staging), unified |
|    | (Unrestricted shell calls) |          | diff tracking, human-in-the-loop approval gates.      |
+----+----------------------------+----------+-------------------------------------------------------+
```

---

## 2. In-Depth Risk Assessments & Legal Analysis

### 2.1 Anti-Cheat & Process Integrity Risks
- **Threat Vector:** Commercial PC games increasingly deploy aggressive anti-cheat systems running at ring-0 (kernel level), such as Riot Vanguard, Easy Anti-Cheat, and BattlEye, or process integrity monitors such as Valve Anti-Cheat (VAC) and Rockstar Arvato/integrity checks.
- **Risk Assessment:** Injecting custom DLLs, opening external process handles (`OpenProcess` with `PROCESS_VM_READ/WRITE`), or synthesizing rapid inputs into a protected game can result in immediate, permanent account bans, hardware HWID blacklisting, and user backlash.
- **Architectural Policy:**
  1. The platform scanner (`GameScannerRegistry`) must inspect game binaries and running services for known anti-cheat signatures.
  2. If a protected multiplayer mode is detected, **Strategy B (Live Hooking) is strictly disabled**.
  3. Mashups for games with anti-cheat must be restricted to **offline, unranked, or private server environments**, or executed purely via Strategy A on mod-friendly hosts.

### 2.2 Intellectual Property, Copyright & Asset Redistribution
- **Threat Vector:** Commercial games contain copyrighted intellectual property: 3D models, textures, animations, voice lines, sound effects, and music. Packaging these assets into downloadable mashup bundles and distributing them online infringes publisher copyrights and EULAs.
- **Risk Assessment:** DMCA takedown notices, litigation against the platform or mod creators, and distribution platform bans.
- **Architectural Policy (`REQ-MOD-003`, `DEC-006`):**
  1. **Zero Raw Binary Redistribution:** NexLudic Engine will never package or distribute raw proprietary assets (`.pak`, `.rpf`, `.awc`, `.toc`) from any commercial game.
  2. **Local Extraction Architecture:** Mashups distributed through NexLudic Engine consist purely of open source code (Java/C#/Lua), configuration diffs, and recipe manifests.
  3. When guest game assets (such as sound effects or icons) are needed, the platform provides automated local extraction scripts that instruct the user's local machine to unpack the assets directly from their legally purchased, installed game copy.

### 2.3 Obsolete Dependencies: The ViGEmBus Paradigm
- **Research Finding:** Historical planning documents (Spider-Craft QPIS) assumed the use of ViGEmBus for emulating virtual Xbox/PlayStation controllers. Research confirmed that ViGEmBus was **permanently retired and archived by its author (nefarius) in November 2023**. Furthermore, ViGEmBus required a custom Windows kernel-mode driver (`.sys`) and administrative UAC installation.
- **Architectural Determination (`DEC-003`):** ViGEmBus is formally rejected. NexLudic Engine core will rely on:
  1. Standard Win32 user-mode `SendInput` APIs (zero kernel drivers, standard user permissions).
  2. In-engine mod input hooks where available (e.g. simulating key presses inside Minecraft or CET directly).
  3. Modern open-source user-mode virtual HID frameworks if analog gamepad emulation becomes essential in future milestones.

### 2.4 Separation of Unverified Claims: 3D Voxel Collision Injection
- **Research Finding:** Gemini Spider-Craft AI QPIS asserted that 3D voxel terrain from Minecraft could be extracted at 60 fps and injected into Marvel's Spider-Man Remastered's physics simulation as dynamic collision geometry in sub-16 ms.
- **Technical Reality:** Insomniac's proprietary engine has no public collision injection API. Injecting collision meshes into closed C++ game physics threads requires reverse-engineered memory pointers, hooking internal Havok/proprietary physics threads, and bypasses memory integrity protections, guaranteeing extreme crash risks and driver freezes.
- **Architectural Determination (`ISSUE-004`):** Reclassified from core scope to **Experimental R&D (Phase 12)**. The universal platform will not depend on cross-process 3D collision injection for its core value proposition.

---

## 3. Required Feasibility Experiments Summary

Before implementing adapter modules in later phases, four isolated proof-of-concept experiments must be executed:
1. **EXP-P1 (Minecraft Time Dilation & Shader):** Verify client-side tick dilation and GLSL post-processing shaders inside a headless Fabric 1.21 mod (Phase 5).
2. **EXP-P2 (Mystery Box State Machine):** Verify custom block entity roulette state machines in Fabric 1.21 (Phase 5).
3. **EXP-P3 (Named Pipe Latency Benchmark):** Benchmark sub-millisecond local IPC packet transfer between a standalone sender and receiver under heavy simulated GPU/CPU load (Phase 6).
4. **EXP-P4 (Web-Swinging Spring Math Solver):** Verify spring-constraint momentum math in a standalone Java/C# physics simulator before mod deployment (Phase 5).

---

## 4. Pending Project Owner Authorizations

1. **Adoption of Compositional Trait Model:** Formal approval of the `IGameAdapter` compositional capability trait model (`IModScaffoldable`, `ITelemetrySource`, `IInputTarget`, `IAssetExtractable`) as the design foundation for Phase 2.
2. **Strategy Prioritization Confirmation:** Reconfirmation that Strategy A (Host-Game Modding) serves as the primary default for initial release milestones, with Strategy B (Live Bridge) reserved for explicitly capable pairs (e.g. Rocket League + Cyberpunk).

