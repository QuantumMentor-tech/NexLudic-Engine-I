# ADAPTER LIFECYCLE SECURITY, RESOURCE, AND AUTHORIZATION REQUIREMENTS
**Document ID:** `NLE-REQ-SRA-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 40 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Security Architect, Systems Engineer, Reliability Architect, Senior QA Engineer  
**Classification:** Core Runtime Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the mandatory security boundaries, operating system token restrictions, anti-cheat coexistence rules, quantitative resource ceilings, and capability authorization constraints governing game adapter execution in NexLudic Engine. Game adapters require low-latency interaction with target game processes, graphics pipelines, and shared memory buffers. However, executing third-party adapter code on Windows desktop environments introduces severe risks: privilege escalation, host system instability, anti-cheat bans, unauthorized file modification, and resource exhaustion.

This document formally defines:
1. The Windows Medium Integrity Level execution boundary and token restrictions (`REQ-SEC-001`, `DEC-002`).
2. The Anti-Cheat Dominance rule (`BLK-SEC-01`) and detection safeguards.
3. Quantitative operating limits: CPU quotas, memory ceilings, IO constraints, and execution timeouts.
4. Win32 Job Object sandboxing parameters.
5. Storage isolation anchored to Volume D (`DEC-009`).
6. Capability authorization tokens and real-time consent revocation protocols.

### 1.2 Architectural Security Boundary Model
```
+--------------------------------------------------------------------------------------------------------+
|                                    WINDOWS HOST SECURITY BOUNDARY                                      |
|                                                                                                        |
|  [ SYSTEM / HIGH IL ] ---> [ STRICTLY PROHIBITED FOR ALL ADAPTERS (DEC-002) ]                         |
|                                                                                                        |
|  +--------------------------------------------------------------------------------------------------+  |
|  |                             MEDIUM INTEGRITY LEVEL (STANDARD USER)                               |  |
|  |                                                                                                  |  |
|  |  +-----------------------------+           +--------------------------------------------------+  |  |
|  |  |    CORE SUPERVISOR (CMP-08) |           |         WIN32 JOB OBJECT SANDBOX CONTAINER       |  |  |
|  |  |  - Process Controller       |           |                                                  |  |  |
|  |  |  - Job Object Manager       |           |  +--------------------------------------------+  |  |  |
|  |  |  - Anti-Cheat Scanner       |==========>|  |       OUT-OF-PROCESS ADAPTER WORKER        |  |  |  |
|  |  |  - Pipe Security Descriptor |           |  | - Restricted Token (No Elevation)          |  |  |  |
|  |  +-----------------------------+           |  | - 512 MB Max Private Working Set           |  |  |  |
|  |                                            |  | - Child Process Spawning Prohibited        |  |  |  |
|  |                                            |  | - Volume D Storage Only (DEC-009)          |  |  |  |
|  |                                            |  +--------------------------------------------+  |  |  |
|  +--------------------------------------------------------------------------------------------------+  |  |
|                                                                                                        |
|  [ LOW IL / APPCONTAINER ] -> Reserved for untrusted sandboxed web widgets; Adapters run at Medium IL  |
+--------------------------------------------------------------------------------------------------------+
```

---

## 2. WINDOWS SECURITY MODEL & INTEGRITY LEVEL ENFORCEMENT

### 2.1 Medium Integrity Level Mandate (`DEC-002`, `REQ-SEC-001`)
In accordance with platform security invariants:
1. **Standard User Token Execution:** All adapter worker processes MUST execute under the interactive user's standard Windows token at **Medium Integrity Level** (`SECURITY_MANDATORY_MEDIUM_RID`).
2. **Elevation Prohibition:** An adapter worker MUST NEVER request, prompt for, or inherit High Integrity Level (`SECURITY_MANDATORY_HIGH_RID`) or `SYSTEM` privileges.
3. **UAC Shield Rejection:** If an adapter executable or script manifests a `requireAdministrator` execution level, the Core Supervisor MUST fail admission at Gate `ADM-GAT-005` and transition the adapter to `FAILED`.
4. **Token Filtering:** When spawning the worker process via `CreateProcessAsUserW` or `CreateProcessW`, the Core Supervisor strips administrative SIDs and dangerous privileges (`SeDebugPrivilege`, `SeTcbPrivilege`, `SeLoadDriverPrivilege`, `SeTakeOwnershipPrivilege`) using `CreateRestrictedToken`.

### 2.2 Win32 Job Object Sandboxing Configuration
Every adapter worker process is unconditionally assigned to a dedicated Win32 Job Object upon creation:

```c
// Architectural Reference: Job Object Configuration
JOBOBJECT_EXTENDED_LIMIT_INFORMATION jeli = {0};
jeli.BasicLimitInformation.LimitFlags = 
    JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE          | // Guarantee process reaping on supervisor exit
    JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION  | // Prevent Windows Error Reporting UI hangs
    JOB_OBJECT_LIMIT_JOB_MEMORY                 | // Hard ceiling on aggregate commit charge
    JOB_OBJECT_LIMIT_ACTIVE_PROCESS;              // Ban unauthorized child processes

jeli.BasicLimitInformation.ActiveProcessLimit = 1; // Exactly 1 process permitted in job
jeli.JobMemoryLimit = 512 * 1024 * 1024;          // 512 MB Hard Memory Limit
```

Child Process Creation Ban:
- The `ActiveProcessLimit = 1` constraint strictly forbids adapter workers from spawning background daemons, command shells (`cmd.exe`, `powershell.exe`), or secondary worker threads disguised as separate processes. Spawning attempts trigger immediate termination of the offending process.

---

## 3. ANTI-CHEAT DOMINANCE & INTEROPERABILITY BOUNDARIES (`BLK-SEC-01`)

### 3.1 Anti-Cheat Dominance Invariant
NexLudic Engine prioritizes user account integrity and fair-play compliance above all integration capabilities. Under no circumstances may an adapter compromise a user's standing with anti-cheat software:
1. **Zero Evasion:** Adapters MUST NOT employ anti-cheat evasion techniques (e.g., driver hiding, DKOM, rootkit hooks, code virtualization, obfuscated injection).
2. **Active Driver Detection:** Prior to admitting an adapter or hooking game memory, the Core Supervisor scans the host system for active kernel-mode anti-cheat drivers:
   - EasyAntiCheat (`EasyAntiCheat.sys`, `EasyAntiCheat_EOS.sys`)
   - BattlEye (`BEDaisy.sys`)
   - Riot Vanguard (`vgk.sys`)
   - Call of Duty Ricochet (`atvi-randgrid.sys`)
   - Valve Anti-Cheat / CS2 Protected Services
3. **Execution Block (`BLK-SEC-01`):** If an active anti-cheat driver is detected protecting the target host game, and the adapter requires runtime memory injection or hook redirection (Integration Strategy 1 or Strategy 3), the Core Supervisor MUST refuse admission (`ADM-GAT-004`), transition the adapter to `NOT_ADMITTED`, and present an explicit warning explaining that the game's anti-cheat policy prohibits live memory interoperability.

---

## 4. QUANTITATIVE RESOURCE CEILINGS & EXECUTION BUDGETS

To safeguard host desktop responsiveness, every adapter worker is subjected to deterministic, hard limits:

| Resource Dimension | Standard Limit | Maximum Upper Bound | Enforcement Mechanism | Violation Consequence |
| :--- | :--- | :--- | :--- | :--- |
| **Private Working Set Memory** | 256 MB | **512 MB** | Win32 Job Object `JobMemoryLimit` | OS terminates worker; Exit code `0xC0000017` (`STATUS_NO_MEMORY`). |
| **CPU Core Utilization** | 50% single-core | **100% single-core** | Job Object Rate Limit (`JOB_OBJECT_CPU_RATE_CONTROL_ENABLE`) | OS throttles thread scheduling; Telemetry flags latency warning. |
| **Boot / Handshake Timeout** | 5,000 ms | **10,000 ms** (`T_BOOT_MAX`) | Core Supervisor Watchdog Timer | Supervisor kills process; transitions to `FAILED`. |
| **Teardown Grace Timeout** | 3,000 ms | **5,000 ms** (`T_SHUTDOWN_GRACE`) | Supervisor Teardown Watchdog | Forced kill via `TerminateProcess`; transitions to `FAILED`. |
| **IPC Invocation Timeout** | 2,000 ms | **10,000 ms** | JSON-RPC Client Request Timeout | Supervisor aborts transaction; flags capability degraded. |
| **Persistent Storage Quota** | 128 MB | **256 MB** | Volume D Directory Quota Watcher | File write calls fail (`ERROR_DISK_FULL`); Adapter logs warning. |
| **Open File / Pipe Handles** | 64 handles | **128 handles** | Win32 Handle Table Monitor | Inbound handle requests rejected; Supervisor alerts. |

---

## 5. STORAGE ISOLATION AND VOLUME D ANCHORING (`DEC-009`)

### 5.1 Storage Sandbox Boundary
Adapters are strictly prohibited from writing arbitrary files across the host filesystem. All persistent and scratch storage is anchored to Volume D:

```
D:\NexLudic\
    └── Projects\
        └── <projectId>\
            └── Adapters\
                └── <adapterId>\
                    ├── cache\             <-- Ephemeral adapter cache (Max 128 MB)
                    ├── config\            <-- Adapter configuration files (Read-only at runtime)
                    ├── state\             <-- State snapshots for session recovery (Max 64 MB)
                    └── .audit\            <-- Immutable lifecycle event logs & diagnostics
```

Prohibited Storage Paths:
- Writing to `%USERPROFILE%`, `%APPDATA%`, `%LOCALAPPDATA%`, or `%TEMP%` on Drive C is strictly prohibited.
- Writing to `C:\Windows\`, `C:\Program Files\`, or system directories is blocked at OS token level and by path sanity filters.
- Modifying target game installation directories without explicit project backup and manifest verification is prohibited.

---

## 6. CAPABILITY AUTHORIZATION AND CONSENT ENFORCEMENT

### 6.1 Cryptographic Authorization Tokens
Runtime capability execution is gated by a cryptographically signed Authorization Token emitted during the capability negotiation phase (Step 39):
1. **Token Scope:** The token contains the unique `adapter_id`, `session_id`, expiration timestamp, and an array of explicitly authorized `capability_id` values.
2. **Handshake Presentation:** The token is delivered to the worker inside the `InitializeAdapterRequest` payload.
3. **Inter-Process Verification:** When the worker invokes Core Supervisor services or data exchange channels, it MUST present the authorization token. Requests targeting undeclared or unconsented capabilities are rejected with `ERR_SEC_UNAUTHORIZED_CAPABILITY`.

### 6.2 Real-Time Consent Revocation Protocol
If the user revokes capability consent in the NexLudic Desktop UI while an adapter is actively `RUNNING`:
1. The Core Supervisor immediately invalidates the adapter's Authorization Token.
2. If the revoked capability is **Optional**:
   - The Supervisor dispatches a capability disable command to the worker.
   - The capability transitions to `UNAVAILABLE` in the Contextual Availability Engine (`NLE-MOD-CAN-001`).
   - The adapter continues running in `DEGRADED` health mode.
3. If the revoked capability is **Mandatory**:
   - The Supervisor immediately triggers the Orderly Teardown sequence (`adapter.shutdown`).
   - The mashup session is halted safely to prevent gameplay desynchronization.

---

## 7. SECURITY INCIDENT CLASSIFICATION AND AUDIT TRACEABILITY

| Incident Type | Detection Mechanism | Immediate Action | Audit Classification |
| :--- | :--- | :--- | :--- |
| **Token Elevation Attempt** | Process Token Query during spawn | Terminate process immediately; do not run | `CRIT_SEC_INTEGRITY_VIOLATION` |
| **Anti-Cheat Driver Detected** | Kernel driver scan at Admission | Deny admission; present UI warning | `ERR_ADM_ANTI_CHEAT_DETECTED` |
| **Child Process Creation** | Job Object Violation Hook | Hard kill by OS Job Object Manager | `CRIT_SEC_CHILD_PROCESS_BLOCKED` |
| **Quota Overrun (>512MB)** | Job Object Memory Limit | Hard kill by Windows Memory Manager | `ERR_RES_MEMORY_LIMIT_EXCEEDED` |
| **Out-of-Sandbox Disk Write** | Path Canonicalization Filter | Reject write; log file violation | `WARN_SEC_OUT_OF_BOUNDS_WRITE` |

---
*End of Specification — Document `NLE-REQ-SRA-001`.*
