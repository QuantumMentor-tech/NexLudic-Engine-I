# NexLudic Engine — Component Interaction and Failure Ownership

**Document ID:** `NLE-ARC-INT-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 10 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Architecture  

---

## 1. Executive Summary & Purpose

The **Component Interaction and Failure Ownership** specification defines the dynamic relationships, authorization gates, and operational failure responsibilities among the 15 logical components of **NexLudic Engine**.

In complex distributed and multi-process desktop systems, unassigned error ownership leads to unhandled exceptions, zombie processes, orphaned mod files, and application deadlocks. This specification establishes an explicit **Failure Ownership Matrix**, defining which component is responsible for detecting, classifying, containing, reporting, and recovering from every failure condition.

---

## 2. Master Failure Ownership Matrix

```
+--------------------------------------------------------------------------------------------------------------------+
|                                              MASTER FAILURE OWNERSHIP MATRIX                                       |
+---------+--------------------------+---------------------+-------------------+------------------+------------------+
| Fail ID | Failure Scenario         | Detecting Component | Owning Component  | Failure Category | Recovery Action  |
+---------+--------------------------+---------------------+-------------------+------------------+------------------+
| ERR-001 | Game Executable Missing  | CMP-02 (Registry)   | CMP-01 (Project)  | User-Correctable | Hold Gate G0     |
| ERR-002 | Executable Hash Mismatch | CMP-02 (Registry)   | CMP-02 (Registry) | Version Warning  | Flag Unverified  |
| ERR-003 | Anti-Cheat Policy Breach | CMP-04 (Feas Engine)| CMP-04 (Feas Eng) | Security Policy  | Hard Stop Gate G1|
| ERR-004 | Circular Capability Dep  | CMP-03 (Capability) | CMP-03 (Capab.)   | Domain Invariant | Reject Graph Def |
| ERR-005 | LLM API Quota / Timeout  | CMP-10 (AI Orchestr)| CMP-10 (AI Orch)  | External Dep     | Retry w/ Backoff |
| ERR-006 | Compiler Build Timeout   | CMP-12 (Tool Driver)| CMP-12 (Tool Drv) | Operational Tool | Job Tree Kill    |
| ERR-007 | Runaway Memory (>2 GB)   | CMP-12 (Tool Driver)| CMP-12 (Tool Drv) | Security Quota   | Terminate Build  |
| ERR-008 | Human Diff Rejection     | CMP-01 (Project/UI) | CMP-01 (Project)  | Human Gate       | Purge Staging    |
| ERR-009 | Broken Pipe (Crash)      | CMP-09 (IPC Host)   | CMP-08 (Lifecycle)| Process Crash    | Isolate Host     |
| ERR-010 | IPC Telemetry Spike      | CMP-09 (IPC Host)   | CMP-13 (Testing)  | Perf Degradation | Drop Stale Pack  |
| ERR-011 | Rollback Hash Mismatch   | CMP-15 (Rollback)   | CMP-15 (Rollback) | System Integrity | Alert User Diff  |
| ERR-012 | LiteDB Disk Lock / Corrup| CMP-14 (Persistence)| CMP-14 (Persist)  | Storage Defect   | Restore Backup   |
+---------+--------------------------+---------------------+-------------------+------------------+------------------+
```

---

## 3. Side-Effect Ownership & Transaction Boundaries

To maintain system integrity, side effects are strictly partitioned across three transaction boundaries:

```
+----------------------------------------------------------------------------------------------------+
|                                    TRANSACTION & SIDE-EFFECT BOUNDARIES                            |
+--------------------------+---------------------+-------------------------+-------------------------+
| Transaction Boundary     | Owning Component    | Scope of Side Effects   | Rollback Mechanism      |
+--------------------------+---------------------+-------------------------+-------------------------+
| **1. Project Metadata**  | CMP-01 / CMP-14     | LiteDB database files   | Atomic LiteDB commits / |
|                          |                     | (`.db`) in project dir. | automated JSON backups. |
+--------------------------+---------------------+-------------------------+-------------------------+
| **2. Staged Scaffolding**| CMP-10 / CMP-11     | Files strictly inside   | Purge staging folder    |
|                          |                     | `<project>/staging/`.   | (`Directory.Delete`).   |
+--------------------------+---------------------+-------------------------+-------------------------+
| **3. Game Mod Deploy**   | CMP-01 / CMP-15     | Copying mod binaries to | Automated `cleanup.ps1` |
|                          |                     | external game folders.  | post-test restoration.  |
+--------------------------+---------------------+-------------------------+-------------------------+
```

### Invariant:
Under no circumstances may an AI agent (`CMP-10`) or compiler driver (`CMP-12`) cross from Boundary 2 to Boundary 3 directly. Transition to Boundary 3 requires explicit human confirmation via the UI diff viewer.

---

## 4. Key Component Interaction Sequences

### 4.1 Live Bridge Fault Containment & Recovery Workflow

```mermaid
sequenceDiagram
    autonumber
    participant UI as Desktop Presentation (CMP-01)
    participant IPC as Named Pipe Host (CMP-09)
    participant Super as Adapter Supervisor (CMP-08)
    participant GameA as Game A (Rocket League)
    participant GameB as Game B (Cyberpunk 2077)

    Note over GameA,GameB: 60 Hz Live Telemetry Streaming Active
    GameA->>IPC: SendTelemetry(Speed, Boost)
    IPC->>GameB: ForwardEvent(GlitchDistortion)

    alt Abrupt Fault: Game B Process Crashes (0xC0000005)
        GameB-xIPC: Connection Lost (Broken Pipe)
        IPC->>IPC: Catch BrokenPipeException; Close Pipe Handle
        IPC->>Super: NotifyAdapterDisconnect(GameB_AdapterId)
        Super->>Super: Log Worker Exit Code; Classify Crash
        IPC->>GameA: SendBridgeDegradedNotification()
        Note over GameA: Game A continues running without crashing
        Super->>UI: Update Status: Game B Crashed (Auto-Reconnecting...)
        Super->>Super: Initiate Exponential Backoff Restart (Attempt 1/3)
    end
```

- **Fault Containment:** Game A's loop remains 100% stable; the broken pipe is caught at the IPC boundary (`CMP-09`), and the adapter supervisor (`CMP-08`) manages isolated recovery without destabilizing the host engine.

---

### 4.2 Sandboxed Staging & Human Diff Approval Workflow

```mermaid
sequenceDiagram
    autonumber
    actor User as Human Reviewer (User)
    participant UI as Diff Viewer (App)
    participant Core as Scaffolding Engine (CMP-01)
    participant Staging as Staging Manager (CMP-11)
    participant Driver as Toolchain Driver (CMP-12)
    participant GameDir as Target Game Directory

    Core->>Staging: Write Generated Mod Files to <project>/staging/
    Core->>Driver: ExecuteBuildCommand(WorkingDir=staging/)
    Driver->>Driver: Run Compiler inside Win32 Job Object Quota
    Driver-->>Core: Build Succeeded (Mod.jar generated)
    Core->>Staging: ComputeFileDiff(Staging vs. Game Directory)
    Staging-->>UI: Present Diff & Hashes to User
    Note over User,UI: Mandatory Human-in-the-Loop Review
    alt User Rejects Diff
        User->>UI: Reject Diff
        UI->>Staging: PurgeStagingDirectory()
        UI-->>User: Deployment Cancelled; Environment Unmodified
    else User Confirms Diff
        User->>UI: Approve Deployment
        UI->>Core: AuthorizeDeployment()
        Core->>GameDir: Copy Mod.jar to Game Mods Folder
        Core->>Core: Register cleanup.ps1 Manifest
        Core-->>UI: Deployment Succeeded; Rollback Ready
    end
```

- **Authorization Gate:** AI-generated artifacts cannot touch production game folders without human approval, preventing accidental file corruption or policy violations.

---

*End of Component Interaction and Failure Ownership (NLE-ARC-INT-001).*

