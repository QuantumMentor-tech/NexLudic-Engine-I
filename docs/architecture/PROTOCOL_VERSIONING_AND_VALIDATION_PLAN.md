# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Protocol Versioning & Validation Plan

**Document ID:** `NLE-PLN-PRT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 13 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Quality & Validation Plan  
**Traceability:** `NLE-SPC-MSG-001` (Universal Message Protocol), `NLE-STD-ERR-001` (Errors & Timeouts), `NLE-POL-TRN-001` (Transport Policy)

---

## 1. Executive Summary & Validation Scope

The **Protocol Versioning and Validation Plan** establishes the governance policies for protocol evolution, semantic versioning rules, forward and backward compatibility contracts, and an **8-Level Automated Testing Suite** designed to verify every communication path between the Core Host and external game adapters before code implementation.

---

## 2. Protocol vs. Contract Versioning Governance

NexLudic Engine enforces a strict architectural distinction between **Protocol Versioning** and **Capability Contract Versioning**:

```
+-----------------------------------------------------------------------------------+
|                        VERSIONING ARCHITECTURE HIERARCHY                          |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  | LEVEL 1: UNIVERSAL MESSAGE PROTOCOL (UMP) VERSION                           |  |
|  | - Governs: Envelope structure, framing, handshake state machine, error     |  |
|  |   taxonomy, and physical transport semantics.                               |  |
|  | - Example: UMP "1.0.0" -> "1.1.0"                                           |  |
|  | - Scope: Entire platform wide. Every adapter must speak a compatible UMP.   |  |
|  +-----------------------------------------------------------------------------+  |
|                                         |                                         |
|                                         v                                         |
|  +-----------------------------------------------------------------------------+  |
|  | LEVEL 2: CAPABILITY & TRAIT CONTRACT VERSION                                |  |
|  | - Governs: Payload schemas for specific operations (e.g. `scaffold_mod`,      |  |
|  |   `compile_scripts`), domain models, and Protobuf telemetry messages.       |  |
|  | - Example: `IModScaffoldableTrait` schema "2.0.0", `PlayerState` proto v1    |  |
|  | - Scope: Scoped per capability trait. Decoupled from transport UMP version.|  |
|  +-----------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------+
```

### 2.1 Semantic Versioning Rules (SemVer 2.0.0)
* **MAJOR (Breaking Changes):** 
  - Modifying or deleting existing mandatory envelope fields.
  - Changing physical framing rules (e.g. replacing `ndjson` with binary-only framing).
  - Altering the mandatory handshake state machine sequence.
  - Incompatible payload changes for an existing capability without fallback.
* **MINOR (Additive & Backward-Compatible Changes):**
  - Introducing new optional envelope fields.
  - Adding new message categories or message types.
  - Adding optional parameters to existing operation payloads.
  - Adding new Protobuf telemetry message types with new field tags.
* **PATCH (Non-Breaking Maintenance):**
  - Documentation updates, clarification of error descriptions, internal bug fixes that do not modify schema or wire format.

### 2.2 Forward & Backward Compatibility Invariants
1. **Unknown Field Tolerance:** All consumers (Core and Workers) MUST ignore unrecognized properties in incoming JSON envelopes and payloads.
2. **Protobuf Tag Invariant:** In Protobuf contracts, field numbers MUST NEVER be reassigned or reused. Deprecated fields must be reserved using `reserved <tag_number>;`.
3. **No Breaking Wire Modifications:** No protocol change may silently convert an existing required field into an incompatible data type.

---

## 3. The 8-Level Protocol Validation Plan

To guarantee total reliability across diverse runtimes (C#, Python, C++, Rust), all IPC communication implementations must pass the following 8-Level Validation Suite:

```
+-----------------------------------------------------------------------------------+
|                        THE 8-LEVEL IPC VALIDATION SUITE                           |
|                                                                                   |
|  Level 1: Envelope & Framing Syntax Validation (JSON Schema, ndjson, Length Prefix|
|  Level 2: Protocol Conformance & State Machine Validation (Handshake, Ping-Pong)  |
|  Level 3: Version Negotiation & Fallback Testing (Major vs Minor compatibility)   |
|  Level 4: Failure Injection & Boundary Testing (Broken pipes, timeouts, fuzzing)  |
|  Level 5: Security & Access Control Validation (User SID DACL, Sandbox evasion)   |
|  Level 6: Endpoint Lifecycle & Resource Cleanup Testing (Job Object kill-on-close)|
|  Level 7: Cross-Language Interoperability Testing (C# Host vs Py/C++/Rust Workers)|
|  Level 8: End-to-End Game Integration Workflow Validation (Mock Game Adapter)     |
+-----------------------------------------------------------------------------------+
```

---

### 3.1 Level 1: Envelope & Framing Syntax Validation
* **Objective:** Verify physical framing parsing and JSON schema compliance.
* **Test Cases:**
  - `TC-L1-01`: Valid single-line JSON with trailing `\n` parsed successfully.
  - `TC-L1-02`: Message containing embedded `\r\n` correctly stripped.
  - `TC-L1-03`: Message exceeding 16 MB Control Channel buffer limit triggers `TRANSPORT_BUFFER_OVERFLOW` and closes stream.
  - `TC-L1-04`: Control Channel message missing mandatory `message_id` or `timestamp_utc` rejected with `VALIDATION_SCHEMA_VIOLATION`.
  - `TC-L1-05`: Protobuf length-prefixed frame decoded correctly on Named Pipe.

### 3.2 Level 2: Protocol Conformance & State Machine Validation
* **Objective:** Ensure all endpoints respect valid message sequencing and correlation.
* **Test Cases:**
  - `TC-L2-01`: Core successfully executes Handshake (`session.handshake.request` -> `response`).
  - `TC-L2-02`: Core verifies correlation between `operation.invoke.request` and subsequent `progress` and `completed` events.
  - `TC-L2-03`: Worker rejects commands arriving prior to successful handshake with `PROTOCOL_UNEXPECTED_MESSAGE_TYPE`.
  - `TC-L2-04`: Heartbeat cycle maintains `Healthy` status over 100 consecutive pings at 5 Hz.

### 3.3 Level 3: Version Negotiation & Fallback Testing
* **Objective:** Prove compatibility across minor version increments and clean rejection across major breaking differences.
* **Test Cases:**
  - `TC-L3-01`: Core `["1.0.0", "1.1.0"]` + Worker `["1.0.0"]` negotiates `1.0.0`.
  - `TC-L3-02`: Core `["1.1.0"]` + Worker `["1.0.0"]` negotiates `1.0.0` (minor backward compatibility).
  - `TC-L3-03`: Core `["2.0.0"]` + Worker `["1.0.0"]` fails handshake with `PROTOCOL_UNSUPPORTED_VERSION`.
  - `TC-L3-04`: Worker sending unexpected extra fields does not crash Core deserializer.

### 3.4 Level 4: Failure Injection & Boundary Testing
* **Objective:** Ensure unhandled crashes, abrupt disconnects, and malformed data do not hang the Core.
* **Test Cases:**
  - `TC-L4-01`: Abrupt worker process kill (`kill -9`) triggers instant `TRANSPORT_CONNECTION_LOST` detection within 50 ms.
  - `TC-L4-02`: Worker sleeping indefinitely triggers `TIMEOUT_OPERATION_EXPIRED` at exact deadline budget.
  - `TC-L4-03`: Fuzzed garbage bytes injected into `stdin` cleanly handled as `PROTOCOL_MALFORMED_MESSAGE`.
  - `TC-L4-04`: Pipe buffer full (stalled consumer) triggers `flow.backpressure` signal.

### 3.5 Level 5: Security & Access Control Validation
* **Objective:** Verify that local privilege escalation and cross-user tampering are strictly blocked.
* **Test Cases:**
  - `TC-L5-01`: Named pipe rejects connection attempt originating from a different local Windows User SID.
  - `TC-L5-02`: Worker attempting to escape staging jail (`../../Windows/System32`) blocked with `SECURITY_SANDBOX_VIOLATION`.
  - `TC-L5-03`: Worker with embedded UAC Administrator manifest refused launch with `SECURITY_UNAUTHORIZED_CALLER`.
  - `TC-L5-04`: Anonymous logon pipe connection denied by kernel DACL.

### 3.6 Level 6: Endpoint Lifecycle & Resource Cleanup Testing
* **Objective:** Guarantee zero orphaned background processes or leaked file handles.
* **Test Cases:**
  - `TC-L6-01`: Killing Core host process immediately terminates all child worker processes via Win32 Job Object `KILL_ON_JOB_CLOSE`.
  - `TC-L6-02`: Worker exceeding 2.0 GB memory limit aborted by OS Job Object memory commit ceiling.
  - `TC-L6-03`: Staging directory locks cleanly released within 200 ms of worker shutdown.
  - `TC-L6-04`: 1,000 rapid spawn/terminate cycles complete without handle exhaustion.

### 3.7 Level 7: Cross-Language Interoperability Testing
* **Objective:** Validate communication parity across diverse programming language runtimes.
* **Test Cases:**
  - `TC-L7-01`: C# Core Host <-> C# Worker (`dotnet run`) passes all L1-L4 tests.
  - `TC-L7-02`: C# Core Host <-> Python Worker (`python worker.py`) passes all L1-L4 tests.
  - `TC-L7-03`: C# Core Host <-> Rust Worker (`cargo run`) passes all L1-L4 tests.
  - `TC-L7-04`: C# Core Host <-> C++ Native Worker passes all L1-L4 tests.

### 3.8 Level 8: End-to-End Game Integration Workflow Validation
* **Objective:** Simulate a realistic multi-step game mashup integration workflow using a Mock Game Adapter.
* **Test Cases:**
  - `TC-L8-01`: End-to-end mock workflow: Spawn -> Handshake -> Capability Manifest -> Mod Scaffold (with 3 progress events) -> Completion -> 60 Hz Telemetry Stream (1,000 frames) -> Clean Shutdown.
  - `TC-L8-02`: Mid-operation user cancellation: Trigger scaffold -> issue cancel at 50% -> verify clean rollback -> verify `CANCELLED` status response.

---

## 4. Acceptance Criteria for Future Implementation Phase

Future code implementation of the IPC subsystem (Phase 3+) will be gated against these explicit criteria:
1. **Zero Elevation:** Code MUST run cleanly under a non-elevated standard Windows user account.
2. **Zero Core Hangs:** No worker crash, timeout, or infinite loop may freeze the Core UI thread or supervisor dispatch loop.
3. **100% Test Pass Rate:** All 32 automated test cases in Levels 1 through 8 must achieve a passing status.
4. **Latency Budget:** Control Channel round-trip RPC latency must remain below 2.0 ms on standard desktop hardware; Data Channel telemetry frame transit must remain below 0.5 ms.

