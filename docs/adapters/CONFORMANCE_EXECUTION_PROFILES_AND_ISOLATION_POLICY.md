# CONFORMANCE EXECUTION PROFILES AND ISOLATION POLICY
**Document ID:** `NLE-POL-CPI-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Security Architect, Systems Engineer, Testing Architect, Reliability Engineer  
**Classification:** Operational Security & Test Policy  

---

## 1. PURPOSE AND POLICY SCOPE

### 1.1 Purpose
This policy formally defines the execution profiles, sandboxing constraints, authorization requirements, and isolation boundaries governing the NexLudic Universal Adapter SDK Conformance Test Harness (`CMP-41`). 

Testing third-party game adapters requires balancing rapid developer feedback against the imperative to protect host system stability, operating system tokens, and game account standing. This policy specifies:
1. The three standardized test execution profiles.
2. The rules prohibiting the bypass of mandatory safety checks.
3. The Win32 Job Object and Medium Integrity Level sandboxing parameters for dynamic testing.
4. Resource caps, timeout limits, and clean-up invariants.

---

## 2. STANDARDIZED EXECUTION PROFILES

The harness supports three discrete execution profiles:

```
+--------------------------------------------------------------------------------------------------------+
|                                    EXECUTION PROFILES TAXONOMY                                         |
|                                                                                                        |
|  [ PROFILE 1: STATIC_MINIMAL ]                                                                         |
|  - Scope: Package archive integrity, path traversal scans, descriptor schema validation (L01-L02).     |
|  - Worker Execution: STRICTLY PROHIBITED (Zero native processes spawned).                              |
|  - Use Case: Pre-commit git hooks, pull request CI/CD bots, initial developer syntax sanity.           |
|                                                                                                        |
|  [ PROFILE 2: CONTRACT_SIMULATION (DEFAULT) ]                                                          |
|  - Scope: Full Static + Semantic Validation (DAG cycles) + Synthetic Registry, Matching & Negot.      |
|  - Worker Execution: STRICTLY PROHIBITED (Evaluated via in-memory synthetic doubles).                 |
|  - Use Case: Standard developer validation, capability composition verification, offline CI runs.     |
|                                                                                                        |
|  [ PROFILE 3: DYNAMIC_FULL ]                                                                           |
|  - Scope: Full Simulation + Real Out-of-Process Worker Spawn + Handshake + Ping/Pong + Teardown.       |
|  - Worker Execution: PERMITTED under Win32 Job Object sandbox at Medium IL (DEC-002).                  |
|  - Use Case: Pre-release verification, lifecycle conformance, memory ceiling & crash circuit testing.  |
|  - Requirement: Explicit Local Developer Consent Required (--authorize-dynamic-execution).             |
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 Profile Capabilities and Restrictions Matrix

| Dimension | `STATIC_MINIMAL` | `CONTRACT_SIMULATION` | `DYNAMIC_FULL` |
| :--- | :---: | :---: | :---: |
| **Worker Process Spawned** | **NO** (0 processes) | **NO** (0 processes) | **YES** (1 process in Job Object) |
| **Execution Mechanism** | In-memory AST check | In-memory synthetic evaluation | Win32 `CreateProcessW` under Medium IL |
| **Target Game Required** | **NO** | **NO** | **NO** (Uses synthetic protocol double) |
| **Network Access** | **PROHIBITED** | **PROHIBITED** | **PROHIBITED** (Local named pipe only) |
| **Filesystem Write** | None (In-memory) | None (In-memory) | Sandbox on Volume D (`DEC-009`) |
| **Developer Authorization**| Default (Unrestricted) | Default (Unrestricted) | **Explicit Consent Flag Required** |
| **Registry Evidence Level** | Candidate Syntax Valid | Candidate Semantic Valid | Full Lifecycle Readiness Verified |

---

## 3. POLICY CONTROL AND NON-BYPASS INVARIANTS

### 3.1 Prohibition of Security Check Bypassing
1. **No Profile Downgrading for Safety:** Selecting a reduced profile (e.g. `STATIC_MINIMAL`) strictly narrows test scope to static layers; it CANNOT mark unexecuted dynamic tests as `PASSED`. Unexecuted checks MUST be marked `SKIPPED`.
2. **Mandatory Security Rule Invariant:** All security checks relevant to the selected profile (`CONF-PKG-003` path traversal, `CONF-SEC-002` UAC manifest rejection, `CONF-CAP-009` memory injection declarations) are unconditionally **MANDATORY**. A profile selection filter CANNOT exclude or disable security checks.
3. **No Average Scoring:** Results must not use weighted scores or percentages. Any single failing mandatory check produces an overall status of `NON_CONFORMANT` or `SECURITY_VIOLATION`.

---

## 4. DYNAMIC ISOLATION AND WIN32 JOB OBJECT SANDBOX

When `DYNAMIC_FULL` is authorized, the harness isolates the adapter worker process within a restricted environment:

### 4.1 Win32 Job Object Configuration Parameters
The harness controller assigns the worker to an isolated Job Object before thread execution commences:

```c
// Architectural Invariant: Harness Dynamic Sandbox
JOBOBJECT_EXTENDED_LIMIT_INFORMATION jeli = {0};
jeli.BasicLimitInformation.LimitFlags = 
    JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE          | // Auto-reap worker if harness exits/crashes
    JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION  | // Prevent OS error dialog hangs
    JOB_OBJECT_LIMIT_JOB_MEMORY                 | // Hard commit ceiling
    JOB_OBJECT_LIMIT_ACTIVE_PROCESS;              // Ban child process creation

jeli.BasicLimitInformation.ActiveProcessLimit = 1; // Strictly 1 process
jeli.JobMemoryLimit = 512 * 1024 * 1024;          // 512 MB hard ceiling
```

### 4.2 Windows Medium Integrity Level Enforcement (`DEC-002`, `REQ-SEC-001`)
1. **Restricted Token Creation:** The harness spawns the worker using `CreateProcessW` passing a restricted token where administrative SIDs and dangerous privileges (`SeDebugPrivilege`, `SeTcbPrivilege`, `SeLoadDriverPrivilege`) are explicitly stripped.
2. **UAC Elevation Refusal:** If an adapter executable contains a Windows UAC manifest requesting `requireAdministrator` or `highestAvailable`, the harness unconditionally refuses execution and flags `CRIT_SEC_UAC_MANIFEST_BANNED`.

### 4.3 Storage Isolation Anchored to Volume D (`DEC-009`)
All scratch files, temporary named pipes, and audit trails generated during dynamic runs are anchored strictly to Volume D:
`D:\NexLudic\Projects\<projectId>\Adapters\<adapterId>\.conformance_sandbox\`  
Writing to Drive C, Windows system directories, or user home directories is blocked by path canonicalization filters.

---

## 5. RESOURCE BOUNDARIES AND TIMEOUT BUDGETS

Dynamic conformance tests are bounded by deterministic execution ceilings:

| Dimension | Hard Limit | Enforcement Point | Action on Violation |
| :--- | :--- | :--- | :--- |
| **Max Private Memory** | 512 MB | OS Job Object | Immediate worker termination (`STATUS_NO_MEMORY`) |
| **Max Execution Time** | 60 seconds total | Harness Run Watchdog | Hard abort of test run; mark `ERROR` |
| **Init Handshake Timeout**| 10,000 ms (`T_BOOT_MAX`) | Supervisor Timer | Terminate worker; fail with `ERR_LFC_HANDSHAKE_TIMEOUT` |
| **Shutdown Grace Period** | 5,000 ms (`T_SHUTDOWN_GRACE`)| Supervisor Timer | Force kill via `TerminateProcess`; fail teardown |
| **Ping/Pong Heartbeat** | 50 ms max roundtrip | Harness IPC Timer | Log warning or shift health state to `STALE` |
| **Max Output Log Volume** | 16 MB per run | Pipe Buffer Monitor | Truncate output; log buffer overflow warning |

---

## 6. CLEAN-UP AND RECOVERY INVARIANTS

Upon completion or interruption of a test run:
1. **Unconditional Process Reaping:** The harness guarantees that no orphaned worker processes or child threads remain running. Closing the Win32 Job Object handle forces the operating system to terminate all contained processes.
2. **IPC Pipe Cleanup:** Named Pipe server instances are disconnected (`DisconnectNamedPipe`) and closed (`CloseHandle`).
3. **Sandbox Directory Wipe:** Ephemeral sandbox files in `.conformance_sandbox/` are wiped, while persistent audit logs in `.audit/` are flushed and sealed with the SHA-256 rolling digest.

---
*End of Policy — Document `NLE-POL-CPI-001`.*
