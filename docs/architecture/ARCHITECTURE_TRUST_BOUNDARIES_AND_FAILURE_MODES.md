# NexLudic Engine — Architecture Trust Boundaries and Failure Modes

**Document ID:** `NLE-ARC-SEC-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 9 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Security Architecture  

---

## 1. Executive Summary & Purpose

The **Architecture Trust Boundaries and Failure Modes** specification establishes the security boundaries, threat mitigations, and fault-containment mechanisms governing **NexLudic Engine**.

Because NexLudic Engine coordinates external game processes, third-party modding frameworks, compilers, and AI coding agents, security cannot be retrofitted as an afterthought. This document formalizes the platform's **zero-trust execution boundaries** and details how the architecture detects, isolates, contains, and recovers from failures without endangering the host operating system or existing game installations.

---

## 2. Formal Trust Boundaries Specification

```
+----------------------------------------------------------------------------------------------------+
|                                    FORMAL TRUST BOUNDARIES MODEL                                   |
+----------------------------------------------------------------------------------------------------+
                                                  |
     +--------------------------------------------+--------------------------------------------+
     | [ PERIMETER: UNTRUSTED / EXTERNAL ]                                                     |
     |                                                                                         |
     |  TB-01: AI Agent Providers (LLM Cloud APIs)     --> Untrusted Code Proposals            |
     |  TB-02: Target Game Processes (Game A / Game B) --> Untrusted Memory / Named Pipe IPC   |
     |  TB-03: External Mod Loaders & Compilers        --> Untrusted Child Process Executables |
     |  TB-04: User Input & Staging Files              --> Untrusted User Prompts & Paths      |
     +--------------------------------------------+--------------------------------------------+
                                                  |
                                                  v [ GATEWAY: VALIDATION & SANITIZATION ]
                                                  |
     +--------------------------------------------+--------------------------------------------+
     | [ SECURED CORE: TRUSTED ZONE (MEDIUM IL) ]                                              |
     |                                                                                         |
     |  - NexLudic.Core Workflow Engine & Feasibility Assessor                                 |
     |  - Win32 Job Object Execution Jail (Memory & CPU Quotas)                                |
     |  - Named Pipe Server with Windows Current-User SID ACLs                                 |
     |  - LiteDB Persistence Engine & File Rollback Verifier                                   |
     +-----------------------------------------------------------------------------------------+
```

### 2.1 Detailed Trust Boundary Profiles

| Boundary ID | Interacting Parties | Data Crossing Boundary | Security Controls & Enforcement | Prohibited Operations |
| :--- | :--- | :--- | :--- | :--- |
| **TB-01** | AI Model $\to$ Core Engine | Code snippets, build scripts, hypotheses | Sanitized prompts; output written strictly to `<project>/staging/src/`. Zero direct OS execution. | Direct execution of model scripts; credential leakage. |
| **TB-02** | Running Game $\leftrightarrow$ Core Engine | 60 Hz telemetry, event RPCs, status | Windows Named Pipe ACLs (Current User SID only); Protocol Buffer schema deserialization with size limits ($\le 64\text{ KB}$). | Memory injection across processes; kernel driver hooks. |
| **TB-03** | Staging Jail $\to$ Game Directories | Compiled `.jar` / `.dll` mods, assets | Mandatory Human-in-the-Loop Diff Review Gate (`REQ-F-009`); SHA-256 hash verification. | Overwriting base game `.exe` or `.pak` files directly. |
| **TB-04** | User UI $\to$ Core Engine | Project paths, game selection, intent | Path canonicalization; checks ensuring paths are within user directories. Standard Medium IL. | Running as Administrator; elevated privilege requests. |
| **TB-05** | Compiler $\to$ Host OS | Child compiler process spawning | Win32 Job Objects enforcing hard memory limits ($\le 2\text{ GB}$) and 60-second timeouts. | Spawning detached background daemons outside Job Object. |

---

## 3. Comprehensive Architectural Failure Modes & Containment

The architecture evaluates and handles 23 distinct failure modes across four operational domains:

```
+----------------------------------------------------------------------------------------------------+
|                                    FAILURE DOMAIN TAXONOMY                                         |
+--------------------------+-------------------------------------------------------------------------+
| Domain                   | Scope & Failure Scenarios Included                                      |
+--------------------------+-------------------------------------------------------------------------+
| **A. Lifecycle & OS**    | Application crash, missing games, corrupted projects, permission denial.|
| **B. Adapter & Tool**    | Adapter crash, compiler timeout, missing toolchains, version drift.     |
| **C. IPC & Bridge**      | Broken pipes, packet corruption, high latency, desynchronization.       |
| **D. Security & Safety** | Anti-cheat detection, asset bundling violation, failed rollback.        |
+--------------------------+-------------------------------------------------------------------------+
```

### 3.1 Domain A: Lifecycle & OS Failure Modes

| ID | Failure Scenario | Detection Mechanism | Immediate System Response | Recovery & Rollback Action |
| :--- | :--- | :--- | :--- | :--- |
| **FL-01** | Missing Game Executable | File check at project open | Block project initialization; flag `GameNotFound`. | Prompt user to locate valid game installation path. |
| **FL-02** | Unsupported Game Version | Executable SHA-256 hash mismatch | Hold Gate G0; display `UnverifiedVersion` alert. | Allow user to request new adapter discovery or revert game. |
| **FL-03** | Corrupted Project Config | LiteDB deserialization failure | Quarantine corrupted `.db`; fail closed. | Offer to restore project from automated JSON backup manifest.|
| **FL-04** | Permission Denial (EACCES) | OS exception on file access | Catch exception; log access violation. | Verify user owns target directory; reject admin elevation. |

### 3.2 Domain B: Adapter & Toolchain Failure Modes

| ID | Failure Scenario | Detection Mechanism | Immediate System Response | Recovery & Rollback Action |
| :--- | :--- | :--- | :--- | :--- |
| **FL-05** | Adapter Worker Crash | Child process exit code $\ne 0$ | Catch process termination event; isolate failure. | Restart worker process with exponential backoff (max 3 tries).|
| **FL-06** | Compiler Build Timeout | Win32 Job Object timer ($>60\text{ s}$)| Terminate compiler process tree immediately. | Clean staging directory; report build error log to user. |
| **FL-07** | Missing Toolchain (e.g. JDK)| Environment check at Gate G3 | Block Gate G3 approval; flag missing dependency. | Guide user to install required SDK ([BLOCK-001](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/environment-baseline/STEP_3_DECISIONS_AND_BLOCKERS.md)). |
| **FL-08** | Incompatible Mod Loader Ver | Manifest inspection in staging | Reject staged mod build at Gate G4. | Re-scaffold mod with pinned compatible loader version. |

### 3.3 Domain C: IPC & Live Bridge Failure Modes

| ID | Failure Scenario | Detection Mechanism | Immediate System Response | Recovery & Rollback Action |
| :--- | :--- | :--- | :--- | :--- |
| **FL-09** | Abrupt Guest Process Crash | `BrokenPipeException` on Named Pipe | Catch pipe disconnect; log error with timestamp. | Host game loop continues unimpeded (`REQ-NF-005`); auto-reconnect.|
| **FL-10** | IPC Packet Corrupted | Protobuf deserialization exception | Drop corrupted packet; increment fault counter. | Reset pipe buffer; if $>10$ faults/sec, pause bridge session.|
| **FL-11** | Latency Spike ($>16.6\text{ ms}$)| High-resolution Stopwatch timer | Flag latency warning; drop non-critical packets. | Recalibrate queue size; warn user of frame drop risk. |
| **FL-12** | Window Focus Contention | Win32 `GetForegroundWindow()` | Route synthetic inputs only to active target window. | Pause input forwarding when neither game has focus. |

### 3.4 Domain D: Security, Policy & Safety Failure Modes

| ID | Failure Scenario | Detection Mechanism | Immediate System Response | Recovery & Rollback Action |
| :--- | :--- | :--- | :--- | :--- |
| **FL-13** | Anti-Cheat Detection Risk | Process scan detects EAC / VAC / BattlEye | **EMERGENCY HARD STOP:** Terminate bridge daemon. | Issue `UNFEASIBLE (POLICY)` determination permanently (`RSK-01`).|
| **FL-14** | Asset Piracy / Bundling | Pre-packaging file inspection | Reject build if proprietary binaries/assets detected. | Enforce non-distributive local extraction script (`REQ-MOD-003`).|
| **FL-15** | Failed Rollback Verification | SHA-256 hash mismatch after `cleanup.ps1` | Alert user immediately; display file diffs. | Offer automated restore from pristine backup copies. |
| **FL-16** | AI Hallucinated API Error | Compiler syntax / symbol error | Catch compilation error in staging sandbox. | Feed error trace back to AI agent for targeted refinement. |

---

## 4. Threat Model & Security Invariants

The platform strictly enforces five security invariants to protect the user environment:

1. **The Principle of Least Privilege:** NexLudic Engine operates exclusively in Windows Medium Integrity Level. If an operation claims to require `Run as Administrator`, it is rejected as an unsafe design (`REQ-SEC-001`).
2. **The Non-Distributive Asset Invariant:** The core platform and generated packages contain zero copyrighted assets. Asset transcoding occurs entirely locally on files already owned and installed by the user (`DEC-006`).
3. **The Anti-Cheat Boundary:** The platform enforces an absolute refusal to attach hooks, read process memory, or inject synthetic inputs into any game running in online multiplayer mode or protected by kernel anti-cheat (`REQ-SAFE-001`).
4. **The Sandboxed Execution Invariant:** Untrusted code generated by AI agents is quarantined inside `<project>/staging/` and bounded by Win32 Job Objects. Direct execution in production directories is prohibited (`ADR-0009`).
5. **The Deterministic Rollback Invariant:** Every modification made by NexLudic Engine must be 100% reversible via an automated `cleanup.ps1` script that restores pristine file hashes (`REQ-NF-006`).

---

*End of Architecture Trust Boundaries and Failure Modes (NLE-ARC-SEC-001).*

