# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Phase 2 Conflict & Risk Register

**Document ID:** `NLE-REG-RSK-002`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 16 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Risk & Conflict Register  
**Traceability:** `FEASIBILITY_RISK_REGISTER.md` (Step 6), `ARCHITECTURE_RISK_AND_OPEN_QUESTION_REGISTER.md` (Step 9), Steps 9–15 Specifications

---

## 1. Executive Summary & Governance Scope

This register consolidates the architectural risks, technical trade-offs, evaluated contradictions, and mitigation strategies identified across Phase 2 (Steps 9 through 15).

Every identified risk is assigned an authoritative owner, a concrete technical mitigation, and a formal **Blocking Classification**:
* **Blocking:** Must be resolved before proceeding to the next step or phase.
* **Conditionally Blocking:** Work may proceed within a bounded scope; resolution required before implementing the affected subsystem.
* **Non-Blocking:** Managed via documented architecture standards and future automated validation testing.

---

## 2. Cross-Document Contradiction Analysis & Resolutions

During the Phase 2 reconciliation audit, four potential cross-document tensions were evaluated and authoritatively resolved:

```
+----------------------------------------------------------------------------------------------------+
|                               RESOLVED ARCHITECTURAL TENSIONS                                      |
+-----+---------------------------+-----------------------------------+------------------------------+
| ID  | Evaluated Tension         | Conflicting Perspectives          | Authoritative Resolution     |
+-----+---------------------------+-----------------------------------+------------------------------+
| T01 | Adapter Execution Model   | In-Process DLL vs. Out-of-Process | **RESOLVED: Out-of-Process** |
|     | Boundary                  | Worker Process                    | `ADR-0002` & `ADR-0005`      |
|     |                           |                                   | enforce separate processes.  |
+-----+---------------------------+-----------------------------------+------------------------------+
| T02 | Primary Storage Strategy  | Flat JSON vs. Mandatory SQLite    | **RESOLVED: Tiered Model**   |
|     | Baseline                  | Database on Day 1                 | `ADR-0007` & `NLE-ARC-DAT-001|
|     |                           |                                   | JSON for MVP; SQLite Phase 3.|
+-----+---------------------------+-----------------------------------+------------------------------+
| T03 | Configuration Authority   | Project settings override all vs. | **RESOLVED: 7-Layer Preced.**|
|     | and Permissions           | Hardcoded security invariants     | `NLE-POL-CFG-001` mandates   |
|     |                           |                                   | Security Policy is SUPREME.  |
+-----+---------------------------+-----------------------------------+------------------------------+
| T04 | Secret Storage Strategy   | Plaintext config / .env files vs. | **RESOLVED: DPAPI Vault**    |
|     |                           | OS-level encrypted vault          | `NLE-ARC-SEC-001` mandates   |
|     |                           |                                   | opaque refs + DPAPI store.   |
+-----+---------------------------+-----------------------------------+------------------------------+
```

---

## 3. Comprehensive Phase 2 Architecture Risk Register

| Risk ID | Risk Title & Description | Severity | Likelihood | Impact | Mitigating Architectural Strategy | Blocking Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **`RSK-ARC-01`** | **Anti-Cheat Conflict & Ban Risk:** Target games with anti-cheat flag live mashup telemetry as memory cheats. | **High** | Low | High | Offline single-player only; zero driver injection; zero memory scanning (`REQ-SAFE-001`). Driverless input via Win32 `SendInput` (`ADR-0001`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-02`** | **Staging Sandbox Jail Escape:** Malicious/buggy mod tools attempt to write outside `<project>/staging/`. | **Critical**| Low | Critical| Strict path canonicalization (`Path.GetFullPath`); containment assert; Win32 Job Object UI/file limit (`ADR-0009`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-03`** | **AI Credential & Secret Leakage:** Plaintext API keys leaked into git repos, export archives, or logs. | **High** | Moderate | High | Opaque UUID reference model (`credential_ref`); DPAPI encryption (`DataProtectionScope.CurrentUser`); Serilog redaction (`NLE-ARC-SEC-001`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-04`** | **Worker Process Hang / Zombie:** External adapter deadlocks, starving Core IPC dispatchers. | **Moderate**| Moderate | Moderate| 10s handshake deadline, 15s heartbeat eviction, Win32 Job Object `KILL_ON_JOB_CLOSE` forceful kill (`NLE-STD-ERR-001`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-05`** | **Manifest Corruption on Crash:** Power loss during save leaves half-written, unparseable project JSON. | **High** | Low | High | Atomic write-rename via Win32 `File.Replace`; temp write to `.tmp`; `.recovery/` journal recovery (`NLE-POL-MIG-001`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-06`** | **Drive C: Storage Exhaustion:** Volume C: has <10 GB free space; mod staging exhausts disk. | **High** | High | High | `DEC-009` mandates all project directories, staging jails, and artifact caches reside on Volume `D:`. | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-07`** | **False Capability Support Claims:** Unverified adapter claims mistaken for working integration. | **Moderate**| Moderate | Moderate| Epistemic grounding model; explicit `Unknown` state; immutable test execution evidence citations required (`NLE-DOM-SPEC-001`). | **Non-Blocking** (Mitigated by design) |
| **`RSK-ARC-08`** | **Telemetry Indexing Scaling:** High-frequency telemetry streams overwhelm flat file storage. | **Low** | Low | Moderate| In-memory queues for Phase 2; embedded SQLite formally evaluated and designated as Phase 3 candidate (`DEC-ARC-007`). | **Conditionally Blocking for Phase 4** (Deferred) |

---

## 4. Phase-Gate Risk Conclusion

Zero architectural risks remain **Blocking** for the transition to Phase 3 (Desktop Interface & Workspace Management). All high-severity failure modes possess concrete, documented structural mitigations verified against Phase 2 architecture specifications.

