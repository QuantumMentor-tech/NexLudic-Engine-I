# Step 34 SDK Foundation Requirements Traceability Matrix

**Document ID:** `NLE-MAT-STP-034`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 34 of 100 — Universal Adapter SDK Foundation & Contract Reconciliation Specification  
**Classification:** Requirements Traceability Matrix & QA Test Plan  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-GAT-PH4-001` (Phase 4 Gate), `REQ-SEC-001`, `REQ-SAFE-001`, `DEC-002`, `DEC-009`, `ADR-0002`

---

## 1. Upstream Requirements & Architecture Traceability

This matrix links foundational architecture decisions, Phase 4 gate conditions, and the Step 34 SDK specifications.

| Requirement ID | Architectural Mandate | Source Phase / ADR | Implementing Step 34 Deliverable | Acceptance Criteria | QA Test Reference | Traceability Status |
|:---|:---|:---|:---|:---|:---|:---|
| `REQ-SEC-001` | Sandboxing: Windows Medium Integrity Level; zero UAC elevation; local path redaction. | Phase 2 Baseline / `ADR-003` | `NLE-CAT-ACM-001` Section 3; `NLE-SPC-ATF-001` Section 2 | AC-34-SEC-01 (Elevation rejected) | `TC-SDK-SEC-01`, `TC-SDK-SEC-04` | FULLY TRACED |
| `REQ-SAFE-001`| Safety: Strictly read-only; zero DLL injection; zero memory hooking (`OpenProcess` banned). | Phase 2 Baseline / `ADR-003` | `NLE-SPC-UAR-001` Section 2; `NLE-SPC-ATF-001` Section 5 | AC-34-SAF-01 (Zero process hooks) | `TC-SDK-SEC-02`, `TC-SDK-ROL-03` | FULLY TRACED |
| `DEC-002` | Modular Boundaries: Universal Catalog decoupled from Game-Specific Adapters. | Phase 2 ADR / `DEC-002` | `NLE-SPC-SDK-001` Section 1.1; `NLE-SPC-UAR-001` Section 1 | AC-34-MOD-01 (Decoupled boundaries)| `TC-SDK-ROL-01`, `TC-SDK-ROL-02` | FULLY TRACED |
| `DEC-009` | Storage Isolation: Project storage scoped strictly to Volume D (`D:\NexLudic\Projects\<id>\`). | Phase 2 ADR / `DEC-009` | `NLE-SPC-UAR-001` Section 4.3; `NLE-CAT-ACM-001` Invariant 6 | AC-34-STR-01 (Volume D scoping) | `TC-SDK-SEC-03`, `TC-SDK-MAN-05` | FULLY TRACED |
| `ADR-0002` | Process Isolation: Adapters execute as Out-of-Process Workers (`OutOfProcessWorker`). | Phase 2 ADR / `ADR-0002` | `NLE-CAT-ACM-001` Section 3; `NLE-SPC-AVR-001` Section 3.2 | AC-34-ISO-01 (Out-of-process only)| `TC-SDK-MAN-03`, `TC-SDK-LIF-01` | FULLY TRACED |
| `GAT-PH4-C01` | Condition C-01: Adapter Schema Alignment (`RuleInputDomain`, `CapabilityRequirement`). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-SPC-SCI-001` Section 1, Section 5 | AC-34-CAP-01 (Input domain mapping)| `TC-SDK-CAP-01`, `TC-SDK-CAP-02` | FULLY TRACED |
| `GAT-PH4-C02` | Condition C-02: Safety & Integrity Enforcement (Medium IL, zero injection, Volume D). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-SPC-SDK-001` Section 2.2; `NLE-SPC-ATF-001` Section 2 | AC-34-SEC-02 (Medium IL sandboxing)| `TC-SDK-SEC-01`, `TC-SDK-SEC-05` | FULLY TRACED |
| `GAT-PH4-C03` | Condition C-03: Advisory Boundary Maintenance (`CMP-045` Approval Required). | Phase 4 Gate / `NLE-GAT-PH4-001` | `NLE-SPC-SDK-001` Section 2.3; `NLE-SPC-ATF-001` Section 2.2 | AC-34-ADV-01 (Human confirmation) | `TC-SDK-ROL-05`, `TC-SDK-SEC-04` | FULLY TRACED |

---

## 2. Planned QA Conformance Test Specifications (30 Test Cases)

The following 30 test specifications define the verification criteria for future SDK implementation and adapter validator tooling:

### 2.1 Group 1: Contract Design & Manifest Validation
- **TC-SDK-MAN-01 (Minimum Valid Manifest):** Valid `adapter.json` with all required fields parses successfully; assigns status `QUALIFIED`.
- **TC-SDK-MAN-02 (Missing Mandatory Identity):** Manifest missing `adapterId` or `version` fails schema validation; emits `ERR-ADP-MAN-01`.
- **TC-SDK-MAN-03 (In-Process Execution Rejection):** Manifest declaring `executionMode = "InProcessDLL"` is rejected immediately under `ADR-0002`.
- **TC-SDK-MAN-04 (Invalid Canonical Game Reference):** Manifest referencing an unknown `canonicalGameId` fails Stage 3 validation with `ERR-ADP-IDN-01`.
- **TC-SDK-MAN-05 (Volume D Path Compliance):** Staging paths outside `D:\NexLudic\Projects\<id>\` fail descriptor checks with `ERR-ADP-STR-01`.

### 2.2 Group 2: Role & Responsibility Boundaries
- **TC-SDK-ROL-01 (Core Catalog Isolation):** Verifies adapter worker cannot alter or inject records directly into the Universal Catalog database.
- **TC-SDK-ROL-02 (Core Assessment Protection):** Verifies adapter worker cannot directly invoke the 10-stage compatibility rule engine or alter outcome badges.
- **TC-SDK-ROL-03 (Read-Only Safety Default):** Discovery and inspection calls execute with zero filesystem mutations on game installation folders.
- **TC-SDK-ROL-04 (Approved Role Enforcement):** Manifest declaring an unapproved role (e.g. `AUTONOMOUS_CRACKER`) is rejected with `ERR-ADP-ROL-01`.
- **TC-SDK-ROL-05 (Advisory Boundary Confirmation):** Binding an adapter to a project workspace halts until `CMP-045` confirmation event is received.

### 2.3 Group 3: Capability System & Dimension Integration
- **TC-SDK-CAP-01 (8-Dimension Categorization):** Every declared capability must map to one of the 8 approved dimensions (`NLE-CAT-DRC-001`).
- **TC-SDK-CAP-02 (Input Domain Mapping):** `SUPPORTED` capability declarations map deterministically to `INPUT_CONFIRMED_VERIFIED` in `NLE-CAT-RIA-001`.
- **TC-SDK-CAP-03 (Proposed Capability Prefixing):** Novel capabilities not present in canonical catalog receive `PROPOSED:` prefix and `CAPABILITY_PROVISIONAL`.
- **TC-SDK-CAP-04 (Dependency Cascade on Prerequisite Failure):** If a prerequisite capability fails, all dependent capabilities evaluate to `UNSUPPORTED`.
- **TC-SDK-CAP-05 (Anti-Deception Level Separation):** System verifies that Level 1 (Declared) capabilities cannot satisfy requirement rows in `NAV-PRJ-FEA`.

### 2.4 Group 4: Versioning & Protocol Invariants
- **TC-SDK-VER-01 (Contract Version Match):** Adapter with `contractVersion == "1.0.0"` passes version admission.
- **TC-SDK-VER-02 (Unsupported Contract Version):** Adapter with `contractVersion == "2.0.0"` on a v1.0.0 host is rejected with `ERR-ADP-VER-01`.
- **TC-SDK-VER-03 (Target Game Version Bounds):** Game build within `[minimumVersion, maximumVersion]` evaluates as supported; outside evaluates as unverified.
- **TC-SDK-VER-04 (Wire Protocol Handshake):** Core and worker establish JSON-RPC 2.0 session; confirm protocol version parity (`1.0.0`).
- **TC-SDK-VER-05 (Heartbeat Liveness Detection):** Missed 3 consecutive pings triggers `Degraded` status and initiates worker recovery.

### 2.5 Group 5: Trust, Permissions & Security Sandboxing
- **TC-SDK-SEC-01 (Process Elevation Rejection):** Manifest requesting `processElevation = "UAC_Elevated"` is rejected immediately (`REQ-SEC-001`).
- **TC-SDK-SEC-02 (Anti-Cheat Blocker Short-Circuit):** Adapter targeting game with active Vanguard/EAC kernel driver emits `BLK-SEC-01` and halts.
- **TC-SDK-SEC-03 (Filesystem Security Scoping):** Worker attempting to open `C:\Windows\System32` for write triggers Win32 access denied.
- **TC-SDK-SEC-04 (Anti-Self-Authorization):** Core ignores undeclared permission claims; worker denied access to ungranted resources.
- **TC-SDK-SEC-05 (Loopback IPC Enforcement):** Worker network traffic constrained to loopback (`127.0.0.1`); external sockets blocked.

### 2.6 Group 6: Conformance, Failure & Lifecycle Supervision
- **TC-SDK-LIF-01 (10-Second Boot Timeout SLA):** Worker failing to complete protocol handshake within 10,000ms is terminated with `StartupTimeout`.
- **TC-SDK-LIF-02 (12-Outcome Taxonomy Enforcement):** Invocations returning non-standard status codes are coerced to `Fatal_ProcessCrash`.
- **TC-SDK-LIF-03 (Non-Retry Mandate on Unknown Outcome):** `TimedOut_OutcomeUnknown` response prohibits automated retry; alerts user.
- **TC-SDK-LIF-04 (Worker Crash Recovery):** Worker crashing with `0xC0000005` transitions to `Crashed`; supervisor cleans up IPC pipes cleanly.
- **TC-SDK-LIF-05 (Clean Teardown Contract):** `ShutdownAsync()` invocation prompts worker to flush buffers, release file locks, and exit with code 0.

---

## 3. Non-Blocking Gap Register

| Gap ID | Identified Condition | Mitigating Architectural Pattern | Follow-Up Step |
|:---|:---|:---|:---|
| `GAP-STP34-01` | Adapter binary signing and PKI trust infrastructure is scheduled for Step 38. | Step 34 enforces SHA-256 package hashing and manifest validation in the interim. | Step 38 (Adapter Signing & Security) |
| `GAP-STP34-02` | Cross-adapter shared memory ring buffer schemas will be finalized in Phase 6. | Step 34 restricts IPC transport to Win32 Named Pipes and loopback sockets. | Step 41 (Universal IPC Broker) |

