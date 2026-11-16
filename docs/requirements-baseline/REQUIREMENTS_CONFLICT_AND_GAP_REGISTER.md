# NexLudic Engine — Requirements Conflict and Gap Register

**Document ID:** `NLE-REG-GAP-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 8 of 100)  
**Status:** Approved Technical Register  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Systems Architecture  

---

## 1. Register Overview & Governance

The **Requirements Conflict and Gap Register** records all identified technical contradictions, scope ambiguities, unverified assumptions, toolchain dependencies, and architectural conflicts uncovered across the Phase 1 research baseline.

Every registered item is tracked with an explicit resolution path, evidence grounding, approving authority, and current status. Under NexLudic governance, no conflict may be resolved by silently editing documentation without ARB authorization.

```
+----------------------------------------------------------------------------------------------------+
|                                    CONFLICT & GAP RESOLUTION WORKFLOW                              |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |                 IDENTIFY CONFLICT / GAP                  |
                     |    (Audit cross-step documentation & technical claims)   |
                     +----------------------------+-----------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |               EVALUATE EVIDENCE & IMPACT                 |
                     |     (Determine scope, feasibility, and safety impact)    |
                     +----------------------------+-----------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |           PROPOSE EVIDENCE-GROUNDED RESOLUTION           |
                     |       (Formulate recommendation or mitigation path)      |
                     +----------------------------+-----------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |             ARB FORMAL REVIEW & DISPOSITION              |
                     |         (RESOLVED / TRACKED ACTION / HARD BLOCK)         |
                     +----------------------------------------------------------+
```

---

## 2. Master Conflict and Gap Inventory

---

### Conflict ID: `CONF-001`
- **Title:** Universal Compatibility Vision vs. Game Engine Heterogeneity.
- **Conflicting Statements:** Early marketing vision suggested "integrate any two PC games automatically," whereas technical research (Step 4 & 6) proves that game integration is strictly bounded by available mod loaders, runtime environments, and anti-cheat policies.
- **Supporting Evidence:** `docs/product-baseline/SOURCE_AUDIT_REPORT.md` vs. `docs/feasibility/TECHNICAL_FEASIBILITY_FRAMEWORK.md`.
- **Impact:** High (Risk of misrepresenting platform capabilities to users).
- **Proposed Resolution:** Formally establish the **Universality Principle**: NexLudic Engine provides a universal *evaluation workflow, architecture, and adapter contract*, but explicitly rejects universal game-pair compatibility. Feasibility is determined per pair via the 14-question engine.
- **Resolution Grounding:** Evidence-Backed (`REQ-F-005`, `AC-02`).
- **Decision Owner:** Architecture Review Board (ARB).
- **Current Status:** **RESOLVED (Codified in PRS §1.2).**

---

### Conflict ID: `CONF-002`
- **Title:** Historical 3D Voxel Injection into Spider-Man Engine.
- **Conflicting Statements:** Historical QPIS transcripts (Source B) claimed dynamic injection of Minecraft 3D voxel collision meshes into Spider-Man's internal physics thread at 60 FPS. Technical research (Step 4 & 6) revealed Spider-Man has no public physics SDK and memory injection causes immediate hard access violations (`0xC0000005`).
- **Supporting Evidence:** Insomniac proprietary engine analysis in `docs/research/game-modding-ecosystems/REPRESENTATIVE_GAME_PAIR_FEASIBILITY.md` (`RSK-04`).
- **Impact:** Critical (Would lead to unrecoverable game crashes and wasted engineering).
- **Proposed Resolution:** Formally reject bidirectional 3D mesh injection into Spider-Man. Scope the Spider-Man mashup strictly to Strategy A/C: Spider-Man traversal mechanics and HUD shaders are executed natively inside Minecraft Java Edition, with an optional non-invasive external telemetry hook (`ADR-0004`).
- **Resolution Grounding:** Evidence-Backed (`ADR-0004`, `RSK-04`).
- **Decision Owner:** Senior Game-Integration Architect.
- **Current Status:** **RESOLVED (Mesh injection rejected; host mod adopted).**

---

### Conflict ID: `CONF-003`
- **Title:** Direct Memory Hooking vs. Valve Anti-Cheat (VAC) Enforcement.
- **Conflicting Statements:** Early candidate proposal (`CAND-PAIR-005`) explored live telemetry hooking into running Call of Duty: Black Ops III multiplayer/co-op. Modding research (Step 4 & 7) confirmed active VAC enforcement on Steam.
- **Supporting Evidence:** Valve developer documentation; `docs/pilot-portfolio/GAME_PAIR_CANDIDATE_REGISTER.md`.
- **Impact:** Fatal (Risk of permanent Steam VAC bans on user accounts).
- **Proposed Resolution:** Institute a permanent, hard blocker on live memory hooking in any game with active kernel anti-cheat or VAC (`REQ-SAFE-001`, `BLK-01`, `RSK-01`). Relegate Call of Duty content integration strictly to offline single-player asset extraction for a host mod (e.g. `CAND-PAIR-002` in Risk of Rain 2).
- **Resolution Grounding:** Evidence-Backed (`REQ-SAFE-001`).
- **Decision Owner:** Security & Compliance Auditor.
- **Current Status:** **RESOLVED (Candidate CAND-005 formally rejected).**

---

### Conflict ID: `CONF-004`
- **Title:** Kernel-Mode Virtual Controllers (ViGEmBus) vs. Modern Windows Security.
- **Conflicting Statements:** Early concept documents proposed ViGEmBus for synthetic gamepad emulation. Technical audit (Step 1 & 5) confirmed ViGEmBus was officially archived/retired in November 2023, requires kernel-mode driver installation, and introduces blue-screen (BSOD) risks.
- **Supporting Evidence:** Upstream ViGEmBus GitHub repository retirement notice; `docs/architecture/decisions/ADR-0004.md`.
- **Impact:** High (Security vulnerabilities, driver signing requirements).
- **Proposed Resolution:** Prohibit kernel-mode drivers entirely (`DEC-003`, `RSK-08`). Mandate driverless, standard user-mode Win32 `SendInput` and in-engine hook dispatch (`REQ-SEC-004`).
- **Resolution Grounding:** Evidence-Backed (`DEC-003`).
- **Decision Owner:** Senior Systems Architect.
- **Current Status:** **RESOLVED (ViGEmBus rejected; Win32 SendInput adopted).**

---

### Conflict ID: `CONF-005`
- **Title:** Host Workstation Hardware Constraint for Dual-Game Bridge Testing.
- **Conflicting Statements:** Pilot 3 (`CAND-PAIR-003`) requires running Rocket League and Cyberpunk 2077 concurrently to validate live IPC bridging. Host environment audit (Step 3) confirmed the local development machine has an NVIDIA Quadro K420 with 2 GB VRAM, which cannot run both games simultaneously without crashing.
- **Supporting Evidence:** `docs/environment-baseline/DEVELOPMENT_ENVIRONMENT_AUDIT.md` (`BLOCK-003`).
- **Impact:** Operational (Blocks local dual-game runtime validation on current machine).
- **Proposed Resolution:** Decouple development hardware from platform architecture. Validate Strategy B IPC protocol in Phase 2/3 using a **Headless Mock Bridge Harness** (`REQ-F-012`), simulating BakkesMod sender telemetry and CET receiver queues. Full commercial dual-game testing is gated on provisioning a dedicated test rig with $\ge 6\text{ GB}$ VRAM.
- **Resolution Grounding:** Engineering Recommendation.
- **Decision Owner:** Build & Release Engineer / Systems Architect.
- **Current Status:** **TRACKED ACTION (Non-blocking for Phase 2 architecture; mock harness specified).**

---

### Conflict ID: `CONF-006`
- **Title:** Missing Local Java and C++ Toolchains on Development Host.
- **Conflicting Statements:** FabricMC mod scaffolding requires Java 21 JDK (`BLOCK-001`), and native adapter compilation requires MSVC v143 (`BLOCK-002`), neither of which is currently installed on the development host.
- **Supporting Evidence:** `docs/environment-baseline/DEVELOPMENT_ENVIRONMENT_AUDIT.md`.
- **Impact:** Operational (Blocks local mod compilation during future Phase 3 PoC execution).
- **Proposed Resolution:** Document toolchain prerequisites for future Phase 3 PoC execution. Core Phase 2 architecture and scaffolding generator design (.NET 8.0 C#) can proceed immediately without local Java/C++ execution, as the .NET 8.0 SDK is fully verified. Toolchains must be installed prior to Gate G3 execution.
- **Resolution Grounding:** Engineering Recommendation.
- **Decision Owner:** Lead Software Engineer.
- **Current Status:** **TRACKED PREREQUISITE (Scheduled for installation prior to Phase 3 PoC).**

---

### Conflict ID: `CONF-007`
- **Title:** Automated Extraction of Proprietary Audio Banks (`.awc`).
- **Conflicting Statements:** Candidate `CAND-PAIR-001` (Minecraft + RDR2) includes custom western gunshot audio. While Minecraft `SoundEvent` registries support `.ogg` audio natively, fully automated CLI extraction of proprietary Rockstar `.awc` audio files without interactive GUI tools remains unverified.
- **Supporting Evidence:** `docs/pilot-portfolio/GAME_PAIR_CANDIDATE_REGISTER.md` (`ISSUE-AUDIO-01`).
- **Impact:** Low (Audio cosmetic feature; does not block core weapon raycast or tick dilation).
- **Proposed Resolution:** Classify `.awc` automated audio extraction as Epistemic Tier 1 (Hypothesized). In initial Pilot 1 scaffolding, use royalty-free placeholder audio or standard WAV-to-OGG converters. Relegate proprietary `.awc` decoders to secondary discovery.
- **Resolution Grounding:** Engineering Recommendation.
- **Decision Owner:** Senior Game-Modding Research Engineer.
- **Current Status:** **RESOLVED (Placeholder audio adopted for initial pilot).**

---

## 3. Register Summary & Phase 1 Exit Impact

All 7 identified conflicts and gaps have been systematically evaluated:
- **5 Critical Conflicts Resolved:** (`CONF-001`, `CONF-002`, `CONF-003`, `CONF-004`, `CONF-007`) have formal, evidence-backed resolutions codified in the architecture baseline.
- **2 Operational Gaps Tracked:** (`CONF-005` Hardware VRAM limit, `CONF-006` Toolchain installation) are non-blocking for Phase 2 architecture design and are managed through concrete mock harnesses and scheduled prerequisite provisioning.
- **Zero Blocking Contradictions Remain:** The Phase 1 research baseline is internally consistent, fully traceable, and structurally validated.

---

*End of Requirements Conflict and Gap Register (NLE-REG-GAP-001).*

