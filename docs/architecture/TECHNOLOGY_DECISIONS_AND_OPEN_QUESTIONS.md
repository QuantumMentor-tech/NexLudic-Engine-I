# NEXLUDIC ENGINE — TECHNOLOGY DECISIONS & OPEN QUESTIONS
## Document ID: NLE-ARCH-DEC-001
**Phase:** 1 — Product Definition & Research  
**Step:** 5 of 100 — Universal Platform Technology Evaluation & Architecture Decision Records  
**Date:** November 16, 2026  
**Auditor / Architect:** Principal Software Architect & QA Lead  

---

## 1. Summary of Architecture Decision Records (ADRs)

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC ENGINE ADR REGISTER                                    |
+----------+-------------------------------------------------------------+-----------+---------------+
| ADR ID   | Title                                                       | Status    | Governance    |
+----------+-------------------------------------------------------------+-----------+---------------+
| ADR-0001 | Initial Platform and Operating-System Support Boundary      | Accepted  | Owner Mandate |
| ADR-0002 | Universal Core Language and Runtime (.NET 8.0 LTS)          | Accepted  | Consensual BL |
| ADR-0003 | Desktop Presentation UI Framework (WPF)                     | Accepted  | Consensual BL |
| ADR-0004 | Universal Core Architecture & Modular Monolith Boundaries   | Proposed  | Ready for Sign|
| ADR-0005 | Adapter Isolation & Out-of-Process Execution Model          | Proposed  | Ready for Sign|
| ADR-0006 | Inter-Process Communication (IPC) & Shared Data Contracts   | Proposed  | Ready for Sign|
| ADR-0007 | Configuration, Persistent Storage & Diagnostic Logging      | Proposed  | Ready for Sign|
| ADR-0008 | AI Coding-Agent Provider Abstraction (MCP + CLI Runner)     | Proposed  | Ready for Sign|
| ADR-0009 | Workspace Isolation, Diff Tracking & Command-Approval Model| Proposed  | Ready for Sign|
| ADR-0010 | Testing, Quality Assurance & Multi-Tier Validation Strategy | Proposed  | Ready for Sign|
| ADR-0011 | Packaging, Dependency Management & Atomic Rollback Strategy | Proposed  | Ready for Sign|
+----------+-------------------------------------------------------------+-----------+---------------+
```

---

## 2. Formally Rejected or Deferred Technologies

The following technologies were evaluated during Step 5 and formally rejected or deferred:

1. **In-Process Adapter Plugins (`AssemblyLoadContext`):**
   - *Status:* **REJECTED (`ADR-0005`).**
   - *Reason:* Unhandled native crashes or memory leaks in third-party game mods would bring down the entire NexLudic Engine host application. Out-of-process workers provide 100% crash containment.
2. **Electron & Web-Based Desktop Wrappers:**
   - *Status:* **REJECTED (`ADR-0003`).**
   - *Reason:* Excessive memory footprint (300–450 MB RAM) and Chromium CPU contention; conflicts directly with heavy commercial PC games.
3. **WinUI 3 / Windows App SDK:**
   - *Status:* **REJECTED (`ADR-0003`).**
   - *Reason:* Unpacking bugs on Windows 10, complex deployment requirements, and unnecessary runtime bloat compared to native WPF.
4. **Kernel-Mode Controller Drivers (ViGEmBus):**
   - *Status:* **REJECTED (`DEC-003`).**
   - *Reason:* Officially retired in November 2023; requires administrative UAC elevation; risks blue-screens (BSOD) and anticheat flags.
5. **Cross-Process 3D Voxel Collision Injection:**
   - *Status:* **DEFERRED TO PHASE 12 EXPERIMENTAL R&D (`ISSUE-004`).**
   - *Reason:* Insomniac engine has no public collision injection API; unverified feasibility and extreme game crash risk.
6. **Relational Database (SQLite) in Core MVP:**
   - *Status:* **DEFERRED TO PHASE 3 (`ADR-0007`).**
   - *Reason:* Flat-file JSON manifests provide complete human-inspectability, zero corruption risk, and bit-for-bit git diffability for MVP project metadata.

---

## 3. Remaining Evidence Gaps & Empirical Uncertainties

1. **Named Pipe IPC Latency under Dual Heavy GPU Load:**
   - *Description:* While Win32 named pipes consistently achieve <1 ms roundtrip latency in synthetic benchmarks, empirical throughput must be measured when the Windows kernel scheduler is heavily loaded by a running 3D game.
   - *Resolution Action:* Execute experiment `EXP-P3` in Phase 6.
2. **Background Window Input Focus for `SendInput`:**
   - *Description:* Win32 `SendInput` synthesizes keystrokes to the currently active foreground window. Testing is required to verify whether games accept synthetic inputs in borderless windowed mode when focus changes.
   - *Resolution Action:* Execute user-mode input validation in Phase 6.
3. **Automated Headless Mod Compilation on Fresh Host:**
   - *Description:* Compiling an AI-generated Fabric 1.21 Gradle mod requires JDK 21 and Gradle, which are currently absent on the development workstation (`BLOCK-001`).
   - *Resolution Action:* Verify headless compilation via a portable JDK in Phase 5.

---

## 4. Active Blockers & Risk Mitigation Summary

- **`BLOCK-001` (Java JDK Absent on Dev Workstation):** Does not block Step 6 (Architecture Design); addressed in Phase 5 via portable JDK bundle.
- **`BLOCK-002` (Target Games Absent on Dev Workstation):** Mitigated by implementing a comprehensive **Mock Game Adapter Test Suite** (`REQ-TEST-001`, `ADR-0010`) for 100% automated CI testing.
- **`BLOCK-003` (2 GB VRAM GPU Constraint on Dev Host):** Mitigated by prioritizing Strategy A (Host-Game Modification), which executes only a single game engine at a time.
- **`BLOCK-004` (Unverified 3D Mesh Injection):** Mitigated by formally scoping the MVP Spider-Man integration to Strategy A (in-engine Minecraft web-swinging mechanics).

---

## 5. Pending Owner Authorizations Before Step 6

1. **Formal Sign-off on Proposed ADRs:** Confirmation of `ADR-0004` through `ADR-0011` as the binding architectural standard for Phase 2 implementation.
2. **Authorization to Proceed to Step 6:** Formal confirmation that Step 5 research, technology trade-off evaluations, and ADRs are accepted and that the project can advance to Step 6.

