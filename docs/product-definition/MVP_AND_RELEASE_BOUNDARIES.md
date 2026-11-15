# NEXLUDIC ENGINE — MVP & RELEASE BOUNDARIES
## Document ID: NLE-DOC-REL-001
**Phase:** 1 — Product Definition & Research  
**Step:** 2 of 100 — Universal Product Scope, User Journeys, Mashup Workflow, and Release Boundaries  
**Date:** November 15, 2026  
**Auditor / Architect:** Principal Product Architect & QA Lead  

---

## 1. Release Strategy & Milestone Overview

To balance ambition with rigorous software engineering, NexLudic Engine implements a phased release cadence. Rather than attempting to support every PC game and experimental synchronization technique at initial launch, the Minimum Viable Product (MVP) focuses on delivering an **end-to-end, trustworthy, and verified workflow** across representative, highly extensible game environments.

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC ENGINE RELEASE PHASING                                 |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [PHASE I: MVP (v1.0-alpha)]                                                                       |
|  - Universal Desktop Shell & Project Manager (Windows 10/11)                                       |
|  - Multi-source Game Scanner (Steam, Registry, Manual Exe Picker)                                  |
|  - Natural Language Intake & Clarification Engine                                                  |
|  - Capability Matrix & Feasibility Assessor                                                        |
|  - Strategy A (Host-Game Modding) Engine with FabricMC (Minecraft Java) Reference Adapter          |
|  - Sandboxed AI Coding Agent Orchestration with Unified Diff Review                                |
|  - Multi-Tier Verification Runner (Tiers 1 & 2 Automated, Tiers 3 & 4 Guided)                       |
|  - Atomic Backup, Rollback, and Non-Distributive Local Exporter                                    |
|                                                                                                    |
|  [PHASE II: EXPANDED ADAPTERS & LIVE IPC (v1.1)]                                                   |
|  - Strategy B Live Interoperability Bridge (Local Named Pipes, Heartbeat, E-Stop)                  |
|  - Additional Mod Loaders: Cyber Engine Tweaks (Cyberpunk 2077) & BakkesMod (Rocket League)        |
|  - User-Mode Synthetic Input Controller (SendInput / Virtual HID)                                  |
|  - Automated In-Game Headless Smoke Test Runners                                                   |
|                                                                                                    |
|  [PHASE III: ADVANCED HYBRID & COMMUNITY PLATFORM (v2.0)]                                          |
|  - Strategy C Hybrid Telemetry Pipelines                                                           |
|  - Community Adapter SDK & Manifest Package Manager                                                |
|  - Visual HUD Overlay Injector (DirectX/Vulkan Hook via standard OBS/ReShade-style layer)          |
|  - Local LLM Execution Harness (Offline AI Agent Support)                                          |
|                                                                                                    |
|  [PHASE IV: EXPERIMENTAL R&D (Long-Term)]                                                          |
|  - Screen Observation Telemetry via Computer Vision / OCR                                          |
|  - Cross-Process 3D Physics Mesh Synchronization (Pending Feasibility Proof)                       |
|  - Dynamic Real-Time Neural Physics Tuning                                                         |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Evaluation of Candidate MVP Capabilities

Each of the 12 candidate MVP capabilities proposed in the product mandate has been formally evaluated:

| Capability Item | Description | MVP Evaluation | Classification | Justification & Dependencies |
| :--- | :--- | :--- | :--- | :--- |
| **1. Project Creation & Isolation** | Create, name, and configure an isolated project workspace. | **Approved for MVP** | Mandatory Owner Req (`REQ-CORE-004`) | Fundamental foundation for non-destructive mod development and safe rollback. |
| **2. Game Selection & Discovery** | Scan installed games (Steam, Registry) and support manual path configuration. | **Approved for MVP** | Mandatory Owner Req (`REQ-DISC-001`, `002`) | Essential for universal game-pair intake. Safe, read-only filesystem operations. |
| **3. Version & Anti-Cheat Audit** | Extract version metadata, file checksums, and detect invasive anti-cheat systems. | **Approved for MVP** | Mandatory Owner Req (`REQ-DISC-003`, `005`) | Critical safety gate to prevent account bans and invalid offset injection. |
| **4. Natural Language Intake** | Accept free-form prompt and conduct structured interactive clarification. | **Approved for MVP** | Mandatory Owner Req (`REQ-MASH-001`, `002`) | Core user experience differentiating NexLudic from manual IDE coding. |
| **5. Feasibility Assessment** | Produce automated capability report cross-referencing game pair capabilities. | **Approved for MVP** | Mandatory Owner Req (`REQ-CORE-005`, `REQ-MASH-003`)| Prevents user frustration by honestly identifying unsupported features early. |
| **6. Strategy Recommendation** | Propose Host-Mod, Live-IPC, or Hybrid based on empirical adapter capabilities. | **Approved for MVP** | Architectural Consensus (`DEC-004`) | Ensures platform defaults to the most stable, low-friction technical path. |
| **7. Blueprint Compilation** | Compile approved requirements into machine-readable `mashup-blueprint.json`. | **Approved for MVP** | Mandatory Owner Req (`REQ-MASH-004`, `REQ-AI-001`) | Provides structured, deterministic boundaries for AI coding agents. |
| **8. Staging Workspace Sandbox** | Confine coding agent file operations to `<project>/staging` with diff tracking. | **Approved for MVP** | Architectural Consensus (`DEC-005`, `REQ-AI-002`) | Crucial security requirement protecting developer host system from rogue agents. |
| **9. Controlled Agent Execution** | Orchestrate sequential AI agent tasks with mandatory human-in-the-loop review. | **Approved for MVP** | Mandatory Owner Req (`REQ-AI-003`, `REQ-AI-004`) | Prevents unreviewed script execution or unintended file overwrites. |
| **10. Reference Mod Build & Test** | Build and validate a representative reference integration (FabricMC host). | **Approved for MVP** | Architectural Consensus (`REQ-MOD-001`, `REQ-TEST-002`) | Proves end-to-end viability on a real-world, highly extensible game engine. |
| **11. Multi-Tier Test Reporting** | Present clear validation progress across Build OK, Launch OK, and Interaction. | **Approved for MVP** | Mandatory Owner Req (`REQ-TEST-003`) | Eliminates false success claims based merely on clean compilation. |
| **12. Backup, Rollback & Packaging** | One-click atomic rollback and non-distributive standalone package export. | **Approved for MVP** | Mandatory Owner Req (`REQ-PKG-001`, `002`, `003`)| Guarantees user system safety, reversibility, and copyright compliance. |

---

## 3. Explicitly Blocked MVP Features

The following capabilities are **strictly prohibited** from the MVP scope:

1. **Custom Kernel-Mode Drivers (`.sys`):**
   - *Status:* Blocked / Rejected (`DEC-003`).
   - *Reason:* Prevents system blue-screens (BSOD), kernel attack surfaces, and anti-cheat driver flags.
2. **Dynamic 3D Cross-Process Collision Mesh Injection:**
   - *Status:* Blocked / Deferred to Experimental R&D (`ISSUE-004`).
   - *Reason:* Lacks public API support in proprietary closed-source games; unverified feasibility and extreme crash risks.
3. **Anti-Cheat Evasion, DRM Bypassing, or Memory Spoofing:**
   - *Status:* Blocked / Permanently Prohibited (`REQ-SAFE-001`, `DEC-006`).
   - *Reason:* Violates security guidelines, platform terms, and user safety.
4. **Automated Bundling of Copyrighted Game Binaries/Assets:**
   - *Status:* Blocked / Permanently Prohibited (`REQ-MOD-003`, `DEC-006`).
   - *Reason:* Strict copyright compliance. Distributables must contain only open code and local extraction scripts.
5. **Unconstrained, Headless AI Shell Execution:**
   - *Status:* Blocked / Strictly Gated (`REQ-AI-002`, `REQ-AI-004`).
   - *Reason:* AI agents must never execute unreviewed shell commands outside the sandboxed staging directory.
6. **Multiplayer Online Match Hooking:**
   - *Status:* Blocked / Prohibited in MVP.
   - *Reason:* Mashups in the MVP are strictly restricted to single-player, private server, or offline gameplay modes.

---

## 4. MVP Success Criteria (Observable Gates)

The MVP milestone will be deemed complete and validated when the following observable conditions are met:
1. **End-to-End Workflow Demonstration:** A user can create a project, select two games (e.g. Minecraft Java as Host and Marvel's Spider-Man as Guest reference), input a prompt ("Spider-Man web swinging in Minecraft"), complete clarification, and generate a validated blueprint.
2. **Clean Reference Build:** The AI coding agent, operating under sandbox isolation, generates an idiomatic FabricMC mod project that compiles cleanly with zero Gradle errors.
3. **Multi-Tier Verification Tracking:** The engine successfully runs Tier 1 (Headless Build) and Tier 2 (Launch OK), correctly displaying verified status in the UI without falsifying higher tiers.
4. **Honest Blocker Handling:** Selecting an unsupported game protected by kernel anti-cheat immediately triggers a clear, explanatory blocker dialog with zero false-positive claims.
5. **Atomic Reversibility Verified:** Deploying the generated mod into the host game and immediately triggering "Rollback" restores the original game directory with 100% hash parity.
6. **Zero Code Regressions:** The entire platform codebase maintains 100% passing unit and integration tests across core managers.

---

## 5. Later Release Roadmap & Dependencies

### 5.1 Release 1.1 (Live IPC & Multi-Mod Expansion)
- **Dependencies:** Stable MVP core and verified user-mode input handler.
- **Scope:**
  - Strategy B Live Interoperability Bridge with low-latency local named pipes.
  - Global Emergency Stop (E-Stop) system (`Ctrl+Alt+Escape`).
  - Rocket League adapter via BakkesMod SDK.
  - Cyberpunk 2077 adapter via Cyber Engine Tweaks (CET).

### 5.2 Release 2.0 (Hybrid Pipelines & Community SDK)
- **Dependencies:** Release 1.1 IPC bridge and adapter stability.
- **Scope:**
  - Strategy C Hybrid Integration workflows (linking host mod to secondary live game).
  - Open Community Adapter SDK (`@nexludic/adapter-sdk`).
  - Community adapter repository and package manager.
  - Offline local LLM inference runner (Ollama / ONNX Runtime).

### 5.3 Long-Term Experimental R&D (Unscheduled)
- **Dependencies:** Advanced academic research and dedicated graphics/physics reverse-engineering.
- **Scope:**
  - Real-time computer vision and OCR for HUD state extraction.
  - Cross-process 3D spatial coordinate synchronization and dynamic collision mapping.
  - Dynamic AI-driven physics damping algorithms.

---

## 6. Traceability to Confirmed Product Requirements

All MVP features directly satisfy verified requirements from the Step 1 Register:
- `REQ-CORE-001` through `REQ-CORE-005` -> MVP Core Platform & Lifecycle
- `REQ-DISC-001` through `REQ-DISC-005` -> MVP Game & Toolchain Scanner
- `REQ-ADAPT-001` through `REQ-ADAPT-002` -> MVP Pluggable Adapter Contracts
- `REQ-MASH-001` through `REQ-MASH-004` -> MVP Intake & Blueprint Engine
- `REQ-AI-001` through `REQ-AI-004` -> MVP Sandboxed Agent Orchestrator
- `REQ-MOD-001` through `REQ-MOD-004` -> MVP Reference Mod Generator (Fabric)
- `REQ-TEST-001` through `REQ-TEST-004` -> MVP Multi-Tier Test Runner
- `REQ-SAFE-001` through `REQ-SAFE-004` -> MVP Security, Non-Admin & Safety Policies
- `REQ-PKG-001` through `REQ-PKG-003` -> MVP Backup, Rollback & Packaging

