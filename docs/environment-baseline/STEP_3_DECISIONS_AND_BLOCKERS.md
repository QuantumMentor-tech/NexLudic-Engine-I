# NEXLUDIC ENGINE — STEP 3 DECISIONS & BLOCKERS
## Document ID: NLE-DOC-BLOCK-001
**Phase:** 1 — Product Definition & Research  
**Step:** 3 of 100 — Development Environment Audit, Toolchain Compatibility, and Initial Game-Pair Baseline  
**Date:** November 15, 2026  
**Auditor / Architect:** Principal Software Architect & QA Lead  

---

## 1. Architectural Decisions Recorded in Step 3

### DEC-008: Formal Adoption of .NET 8.0 SDK & WPF for Windows Desktop Core
- **Topic:** Core Platform Implementation Stack
- **Status:** Architectural Baseline Established
- **Context:** The development workstation contains .NET 8.0.425 SDK and `Microsoft.WindowsDesktop.App` (WPF runtime) pre-installed and fully verified. .NET 8 is an active LTS release supported through late 2026, offering native Windows IPC, thread-safe asynchronous task execution, and robust process sandboxing.
- **Evidence:** Workstation audit verified `dotnet --info` showing 8.0.425 SDK and MSBuild 17.11.
- **Options Considered:**
  1. *Option A:* Shift to web-based desktop wrapper (Electron/Tauri). Requires installing Rust toolchain or bundling heavy Node/Chromium runtimes.
  2. *Option B:* Adopt native C#/.NET 8.0 with WPF. Zero additional runtime installation required; native Windows IPC and memory performance.
- **Consequences:** Option B adopted. Core platform will be authored in C# 12 / .NET 8.0.
- **Further Authorization:** Owner approval of Phase 2 solution architecture.

---

### DEC-009: Strict Working Workspace Storage Allocation to Volume D:
- **Topic:** Development Storage Management & Failure Prevention
- **Status:** Mandatory Operational Constraint
- **Context:** System drive `C:` has only 9.79 GB free disk space (<9% capacity remaining). Compiling mod projects, downloading Gradle dependencies, and backing up game directories will rapidly exhaust `C:`, risking system freezes or build aborts. Drive `D:` possesses 417.6 GB free space.
- **Evidence:** Filesystem drive discovery confirmed `C:` (9.79 GB free) vs `D:` (417.6 GB free).
- **Options Considered:**
  1. *Option A:* Store projects in `%APPDATA%` on `C:`. Severe risk of disk exhaustion.
  2. *Option B:* Enforce default workspace and project paths targeting `D:\App Winrar Files\Nexludic Engine (working)` or `D:\NexLudicProjects\`.
- **Consequences:** Option B adopted. Engine configuration defaults will target Volume D:.
- **Further Authorization:** None; operational safety requirement.

---

### DEC-010: Decoupling of Java & Modding Toolchains from Core Platform Dependencies
- **Topic:** Dependency Isolation
- **Status:** Architectural Baseline Established
- **Context:** Neither Java JDK nor Gradle is installed on the development host. Because NexLudic Engine is a universal platform, Java must never be introduced as a global prerequisite for running the core desktop engine.
- **Evidence:** System probe confirmed Java and Gradle absent from `PATH`.
- **Consequences:** Java and Gradle remain strictly conditional, game-adapter-specific dependencies. The universal core engine runs purely on .NET 8 without requiring Java.
- **Further Authorization:** None; satisfies `REQ-CORE-002`.

---

## 2. Active Technical Blockers & Mitigation Strategies

```
+----------------------------------------------------------------------------------------------------+
|                                      ACTIVE BLOCKERS REGISTER                                      |
+-------------+------------------------------------+----------+--------------------------------------+
| Blocker ID  | Description                        | Severity | Resolution Path & Target Step        |
+-------------+------------------------------------+----------+--------------------------------------+
| BLOCK-001   | Java JDK & Gradle absent on host   | Medium   | Required only when building JVM mods |
|             | machine.                           |          | (Minecraft). Address in Phase 5 via  |
|             |                                    |          | portable JDK or guided install.      |
+-------------+------------------------------------+----------+--------------------------------------+
| BLOCK-002   | Target PC games not installed on   | Low      | Build mock game adapter harnesses    |
|             | development workstation.           |          | for Phase 2/3 unit and IPC testing.  |
+-------------+------------------------------------+----------+--------------------------------------+
| BLOCK-003   | Workstation GPU (2GB VRAM) cannot  | Medium   | Focus initial milestones on Strategy |
|             | run dual AAA games concurrently.   |          | A (Host Modding). Defer heavy dual   |
|             |                                    |          | game live tests to Phase 6.          |
+-------------+------------------------------------+----------+--------------------------------------+
| BLOCK-004   | Unverified 3D collision injection  | High     | Reclassified as Experimental R&D.    |
|             | into closed game processes.        |          | Excluded from MVP baseline.          |
+-------------+------------------------------------+----------+--------------------------------------+
```

### Detailed Blocker Analysis

#### BLOCK-001: Absence of Java Development Kit (JDK 21) & Gradle
- **Impact:** While the core .NET platform compiles without Java, generating and compiling a real FabricMC mod project (as planned in the MVP) requires JDK 21+ and Gradle.
- **Mitigation:**
  1. For Step 4 and Phase 2 (Architecture), mock adapter schemas will be used.
  2. In Phase 5 (Mod Generation), a pre-flight environment check will detect missing JDK and provide an automated script or clean user guide to install Eclipse Adoptium Temurin OpenJDK 21 without administrator UAC elevation.

#### BLOCK-002: PC Games Not Installed on Host Development Machine
- **Impact:** Live game process hooks and in-game window verification cannot be tested against live game clients directly on this development workstation.
- **Mitigation:** The platform will build a comprehensive **Mock Game Adapter Test Suite** (`REQ-TEST-001`) that simulates game process lifecycles, event streams, and named pipe responses in headless automated tests.

#### BLOCK-003: Workstation Hardware Constraints (2 GB VRAM on Quadro K420)
- **Impact:** Attempting to launch two heavy games simultaneously (e.g. Rocket League + Cyberpunk) will trigger an out-of-memory driver crash.
- **Mitigation:** Prioritizing Strategy A (Host-Game Modification), which runs only a single lightweight host engine (Minecraft) during development and validation.

#### BLOCK-004: Unverified Reverse-Engineered Physics Injection
- **Impact:** The claim from Spider-Craft AI QPIS that real-time voxel meshes can be injected into Marvel's Spider-Man memory space lacks empirical verification and poses severe game stability risks.
- **Mitigation:** Formally deferred to Phase 12 (Experimental R&D). The Spider-Man reference integration for the MVP is strictly restricted to Strategy A (implementing Spider-Man web-swinging mechanics inside the Minecraft host mod).

---

## 3. Required Compatibility Experiments for Subsequent Steps

Before committing to live IPC and modding implementations in later phases, the following isolated proof-of-concept experiments must be executed:
1. **EXP-001 (Named Pipe Latency Benchmark):** A standalone C# console utility measuring roundtrip packet latency and throughput across local named pipes on Windows 11 under simulated CPU load (Scheduled for Phase 6).
2. **EXP-002 (Headless Fabric Project Compilation):** Verifying that an AI-generated Fabric 1.21 Gradle mod project can be compiled headlessly via command line when JDK 21 is present (Scheduled for Phase 5).
3. **EXP-003 (User-Mode Input Injection Verification):** Testing `SendInput` event receipt across focused and non-focused Windows desktop game windows (Scheduled for Phase 6).

---

## 4. Pending Project Owner Authorizations

1. **Phase 2 Technical Architecture Kickoff:** Confirmation that the technical architecture design in Phase 2 should proceed based on .NET 8.0 / C# / WPF.
2. **Volume Allocation Confirmation:** Formal approval of Volume `D:` as the dedicated storage location for all NexLudic Engine development artifacts and project staging.

