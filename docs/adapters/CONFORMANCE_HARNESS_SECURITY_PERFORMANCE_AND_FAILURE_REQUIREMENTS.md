# CONFORMANCE HARNESS SECURITY, PERFORMANCE, AND FAILURE REQUIREMENTS
**Document ID:** `NLE-REQ-SPF-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Security Architect, Reliability Engineer, Systems Architect, Lead QA Automation Engineer  
**Classification:** Non-Functional Requirements Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the mandatory security controls, threat mitigations, quantitative performance criteria, and fault tolerance requirements governing the NexLudic Universal Adapter SDK Conformance Test Harness (`CMP-41`). 

Because the harness ingests and evaluates third-party archive packages, custom JSON descriptors, and native adapter executables, it represents a high-value attack surface. This document specifies the non-bypassable constraints required to guarantee host system security, predictable performance, and fail-safe recovery under all operational conditions.

---

## 2. SECURITY CONTROLS AND THREAT MITIGATIONS

```
+--------------------------------------------------------------------------------------------------------+
|                                    HARNESS THREAT MODEL & MITIGATIONS                                  |
|                                                                                                        |
|  [ THREAT 1: Decompression Bombs ]      ---> 64MB Archive Cap; 256MB Unpack Cap; Max 10:1 Ratio Cap     |
|  [ THREAT 2: Path Traversal Exploits ]  ---> Reject '..', Drive Letters, Symlinks, Relative Slashing   |
|  [ THREAT 3: Parser Recursion Attacks ] ---> Max JSON Depth: 32; Max String: 64KB; Defensive Parser   |
|  [ THREAT 4: Privilege Escalation ]     ---> Medium IL Enforcement; Restricted Token; Ban UAC Manifest  |
|  [ THREAT 5: Rogue Child Processes ]    ---> Win32 Job Object ActiveProcessLimit = 1                   |
|  [ THREAT 6: Memory Exhaustion (OOM) ]  ---> 512 MB Job Object Memory Limit; OS-Enforced Kill          |
|  [ THREAT 7: Network Exfiltration ]     ---> Outbound Network Sockets Blocked in Dynamic Mode           |
|  [ THREAT 8: Anti-Cheat Collisions ]    ---> BLK-SEC-01 Invariant; Immediate Block on Active AC Drivers|
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 Untrusted Archive and Package Parsing Safeguards
1. **Archive Size Ceilings:** The harness MUST reject any compressed archive exceeding **64 MB** (`ERR_PKG_OVERSIZED_ARCHIVE`).
2. **Decompression Expansion Limit:** Uncompressed archive content MUST NOT exceed **256 MB**. Decompression aborts immediately if total bytes written exceed this limit.
3. **Compression Ratio Cap:** Archives with an overall compression ratio exceeding **10:1** are rejected as potential zip bombs.
4. **Strict Path Canonicalization:** Every file entry path is sanitized. Entries containing `..`, leading slashes (`/`, `\`), Windows drive letters (`C:`), or symbolic link targets are rejected with `CRIT_SEC_PATH_TRAVERSAL`.

### 2.2 Defensive JSON and Manifest Parsing
1. **Recursion Depth Ceiling:** JSON parsers MUST enforce a maximum nesting depth of **32 levels**. Exceeding depth aborts parsing with `ERR_PKG_PARSER_DEPTH_EXCEEDED`.
2. **Token and String Length Limits:** Individual string values in `adapter.json` MUST NOT exceed **64 KB**. Total descriptor file size MUST NOT exceed **1 MB**.
3. **Disallow Unsafe Formats:** Descriptors containing binary blobs, unescaped control characters, or trailing bytes past the root object are rejected.

### 2.3 Dynamic Sandbox Security Boundaries
When running in `DYNAMIC_FULL` mode:
1. **Medium Integrity Level Mandate (`DEC-002`):** The worker process runs strictly under the standard user token. High/SYSTEM tokens are prohibited.
2. **Win32 Job Object Limits:** The process is locked to a Job Object configured with `ActiveProcessLimit = 1` and `JobMemoryLimit = 512MB`.
3. **Network Socket Block:** The sandbox prohibits outbound TCP/UDP connections. Communication is restricted strictly to local named pipes (`\\.\pipe\nexludic_*`).
4. **Anti-Cheat Dominance (`BLK-SEC-01`):** Active kernel anti-cheat drivers (EasyAntiCheat, BattlEye, Vanguard) trigger immediate admission refusal for memory-hooking adapters.

---

## 3. PERFORMANCE AND RESOURCE BUDGETS

To guarantee fast feedback during developer authoring and CI/CD pipelines, the harness enforces the following quantitative performance targets:

| Test Mode / Operation | Target Latency | Maximum Hard Timeout | Resource Ceiling |
| :--- | :---: | :---: | :---: |
| **Static Inspection Mode** | $\le 200\text{ ms}$ | **500 ms** | $\le 64\text{ MB RAM}$ |
| **Contract Simulation Mode** | $\le 800\text{ ms}$ | **2,000 ms** | $\le 128\text{ MB RAM}$ |
| **Dynamic Handshake Phase** | $\le 1,500\text{ ms}$ | **10,000 ms** (`T_BOOT_MAX`) | $\le 512\text{ MB RAM}$ |
| **Dynamic Teardown Phase** | $\le 500\text{ ms}$ | **5,000 ms** (`T_SHUTDOWN_GRACE`) | Reclaimed to 0 MB |
| **Full Dynamic Conformance Run**| $\le 12\text{ seconds}$ | **30,000 ms** | $\le 512\text{ MB RAM}$ |

### 3.1 Caching and Invalidation Invariants
1. **In-Memory AST Caching:** Parsed descriptor ASTs and schema validation models may be cached in-memory during a single CLI session, keyed by the archive's SHA-256 digest.
2. **Zero Disk Caching of Unverified Code:** Dynamic execution results are NEVER cached if the adapter binary, descriptor, or rule catalog changes.
3. **Deterministic Clean-Up:** All temporary named pipe buffers and scratch memory maps are purged upon test conclusion.

---

## 4. FAULT TOLERANCE AND FAILURE RECOVERY

### 4.1 Handling Interrupted Runs (User Abort / SIGINT)
If a developer interrupts the harness (via `Ctrl+C` or CI timeout):
1. **Immediate Job Object Closure:** The harness immediately terminates the Win32 Job Object handle, guaranteeing the operating system reaps the worker process.
2. **Pipe Disconnection:** All named pipe instances are disconnected and closed.
3. **Partial Report Emission:** A partial report is written with status `INCOMPLETE` and diagnostic code `ERR_RUN_ABORTED_BY_USER`.

### 4.2 Handling Worker Crashes and Unhandled Exceptions
1. **Non-Crashing Harness:** A crash, null pointer dereference, or buffer overflow inside an adapter worker MUST NEVER crash the harness runner.
2. **Crash Forensic Capture:** The harness captures the worker's Win32 exit code (e.g. `0xC0000005` Access Violation), records a `CONF-LFC` failure, and transitions the test outcome to `FAIL`.

---

## 5. SPECIFICATION TRACEABILITY MATRIX

| Requirement ID | Upstream Authority | Specification Section | Verification Method |
| :--- | :--- | :--- | :--- |
| **REQ-SPF-001** | Security Baseline | Section 2.1 (Archive Safeguards) | `CONF-PKG-002`, `CONF-PKG-003` |
| **REQ-SPF-002** | `DEC-002` (Medium IL) | Section 2.3 (Sandbox Boundaries) | `CONF-SEC-001`, `CONF-SEC-002` |
| **REQ-SPF-003** | `BLK-SEC-01` (Anti-Cheat) | Section 2.3 (Anti-Cheat Dominance) | `CONF-SEC-003`, `CONF-SEC-010` |
| **REQ-SPF-004** | `DEC-009` (Volume D) | Section 2.3 (Storage Sandboxing) | `CONF-SEC-006` |
| **REQ-SPF-005** | Reliability Baseline | Section 3 (Performance Targets) | Benchmark timing verification |
| **REQ-SPF-006** | Robustness Baseline | Section 4 (Fault Recovery) | `CONF-LFC-009`, `CONF-LFC-010` |

---
*End of Specification — Document `NLE-REQ-SPF-001`.*
