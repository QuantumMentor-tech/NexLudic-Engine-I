# UNIVERSAL ADAPTER SDK CONFORMANCE HARNESS SPECIFICATION
**Document ID:** `NLE-SPC-ACH-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Principal Software Architect, Testing Architect, Developer Tooling Engineer, Senior QA Automation Engineer  
**Classification:** Core Runtime Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the authoritative, implementation-ready architectural blueprint for the **Universal Adapter SDK Conformance Test Harness** (`CMP-41`) within NexLudic Engine. 

NexLudic Engine is a universal game-mashup and interoperability platform that evaluates and facilitates integrations between pairs of games using host-game modification, live interoperability bridging, or hybrid execution. For third-party and first-party game adapters to interoperate reliably and safely without endangering host system stability, corrupting game state, or tripping anti-cheat mechanisms, an adapter MUST undergo deterministic, repeatable, and contract-first conformance verification before it can be registered or admitted into an active mashup session.

This document formally specifies:
1. The purpose, core principles, and boundaries of the conformance harness.
2. The separation of validation states (preventing the collapse of structural, semantic, behavioral, and compatibility dimensions into a naive "pass/fail").
3. The relationship between the harness and authoritative Phase 5 contracts (Steps 34–40).
4. The multi-tiered execution modes (Static Inspection, Contract Simulation, and Controlled Dynamic Verification).
5. The overall conformance evaluation policy and result determination rules.

### 1.2 Architectural Scope and System Boundaries
```
+--------------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC CONFORMANCE HARNESS (CMP-41)                               |
|                                                                                                        |
|  +--------------------------------------------------------------------------------------------------+  |
|  |                             CONFORMANCE TEST COORDINATOR (CTC-01)                                |  |
|  |  - Orchestrates Suite Execution        - Enforces Profile Boundaries     - Aggregates Findings   |  |
|  +------------------+--------------------------------+-------------------------------+--------------+  |
|                     |                                |                               |                 |
|                     v                                v                               v                 |
|  +----------------------+      +-------------------------+      +-----------------------------------+  |
|  |   STATIC INSPECTION  |      |   CONTRACT SIMULATION   |      |   SEPARATELY AUTHORIZED DYNAMIC   |  |
|  |  (Default, Zero-Run) |      | (Mock IPC & Protocol)   |      |  (Medium IL Sandbox, DEC-002)     |  |
|  | - Package & Manifest |      | - JSON-RPC 2.0 Ingest   |      | - Worker Spawn & Init Handshake   |  |
|  | - Schema Validation  |      | - Synthetic Registry    |      | - Ping/Pong Freshness Timing      |  |
|  | - Path Traversal Scan|      | - Matching / Negot. Sim |      | - Orderly Teardown Drain          |  |
|  +----------------------+      +-------------------------+      +-----------------------------------+  |
+--------------------------------------------------------------------------------------------------------+
                                                 |
                                                 v
+--------------------------------------------------------------------------------------------------------+
|                                   DOWNSTREAM PLATFORM GATES (READ-ONLY)                                |
|                                                                                                        |
|  [ Step 37 Registry ] <----- (Evidence Artifacts) -----> [ Step 38 Matcher ] <----> [ Step 40 Supv ]  |
+--------------------------------------------------------------------------------------------------------+
```

### 1.3 Strict Non-Goals and Explicit Exclusions
The conformance test harness is strictly an automated verification tool. It is NOT:
- A compiler, package builder, or code generator.
- A live game process injector, memory dumper, or kernel debugger.
- A bypass mechanism for anti-cheat software (`BLK-SEC-01`), DRM, or Windows security controls.
- An automatic publishing or distribution pipeline (registry admission is governed independently by Step 37).
- An end-to-end game interoperability certification engine (passing SDK conformance certifies contract compliance, NOT that two specific games will mash up seamlessly).

---

## 2. GOVERNING PRINCIPLES

### 2.1 Contract-First Verification
The conformance harness is strictly an evaluator of contracts, never an alternate source of truth:
- Every test assertion MUST trace back to an approved JSON Schema, protocol definition, state machine rule, or security mandate from Steps 34 through 40.
- If an expected behavior lacks an authoritative specification, the harness MUST log a `SPECIFICATION_GAP` finding rather than inventing an ad-hoc pass/fail criterion.

### 2.2 Language and Runtime Neutrality
In accordance with `NLE-SPC-SDK-001` (Step 34):
- The harness interacts with adapters strictly across standardized boundary protocols: package archive inspection, schema validation, and JSON-RPC 2.0 over standard I/O and Named Pipes (`\\.\pipe\nexludic_*`).
- The harness makes ZERO assumptions about the adapter's implementation language (C++, Rust, C#, Python, Zig, or Go). Any adapter adhering to the wire protocol and manifest contracts is fully testable.

### 2.3 Strict Separation of Validation States
A primary architectural failure mode in extension ecosystems is collapsing heterogeneous checks into a single boolean "valid" or "compatible" flag. The harness strictly distinguishes 10 orthogonal validation layers:

| Layer ID | Validation Dimension | Evaluated By | Meaning of Passing |
| :---: | :--- | :--- | :--- |
| **L01** | **Package & Archive Integrity** | Package Reader | Archive is uncorrupted, under size caps, and free of path traversal attacks. |
| **L02** | **Descriptor Structural Validity** | JSON Schema Validator | Manifest conforms syntactically to `NLE-SCH-ACD-001`. |
| **L03** | **Capability Semantic Validity** | Step 36 Validator (`CVC-001`)| Declarations resolve against canonical taxonomy; DAG dependencies valid. |
| **L04** | **SDK Protocol Conformance** | Contract Simulator | Adapter correctly speaks JSON-RPC 2.0 wire protocol and error structures. |
| **L05** | **Registry Eligibility** | Step 37 Fixture (`VCR-001`) | Adapter satisfies static registration prerequisites (hash binding, provenance). |
| **L06** | **Capability Matching Behavior** | Step 38 Evaluator (`CMR-001`)| Adapter satisfies requirement models and comparison ranking rules. |
| **L07** | **Negotiation Protocol** | Step 39 Engine (`CNA-001`) | Adapter negotiates parameters and handles graceful degradation without crash. |
| **L08** | **Lifecycle & Health State** | Step 40 Supervisor (`ALC-001`)| Worker handles admission, handshake, ping/pong, and orderly teardown. |
| **L09** | **Security & Sandboxing** | Security Inspector | Process respects Medium IL (`DEC-002`), zero elevation, 512MB RAM, Volume D. |
| **L10** | **End-to-End Interoperability** | Target Integration Tests | Game mashup synchronizes live gameplay (OUTSIDE HARNESS SCOPE). |

> [!IMPORTANT]
> A 100% pass across Layers L01–L09 certifies that the adapter is **SDK Contract Conforming**. It does NOT guarantee Layer L10 (End-to-End Gameplay Compatibility).

### 2.4 Least Privilege and Safe-by-Default
1. **Default Mode is Non-Executing:** Routine conformance checks operate strictly in **Static Inspection Mode** or **Contract-Test Simulation Mode** using synthetic fixtures.
2. **Explicit Authorization for Dynamic Runs:** Executing an adapter executable requires explicit developer invocation, runs strictly at Windows Medium Integrity Level (`DEC-002`) inside a Win32 Job Object, and operates under strict CPU, memory, and timeout caps.
3. **Zero Game Injection:** The harness NEVER launches, injects into, or modifies target game binaries.

---

## 3. HARNESS EXECUTION MODES AND ENVIRONMENT TAXONOMY

The harness supports three strictly bounded execution modes:

```
+--------------------------------------------------------------------------------------------------------+
|                                    HARNESS EXECUTION MODES                                             |
|                                                                                                        |
|  [ MODE 1: STATIC INSPECTION ]                                                                         |
|  - Operations: Archive unpack (sandboxed), schema validation, DAG cycle checks, hash verification.     |
|  - Worker Process Execution: STRICTLY PROHIBITED (0 processes spawned).                                |
|  - Required Trust Level: Zero trust (Safe for untrusted / third-party PRs).                            |
|                                                                                                        |
|  [ MODE 2: CONTRACT SIMULATION ]                                                                       |
|  - Operations: Mock supervisor feeds synthetic JSON-RPC requests to adapter descriptors; offline math. |
|  - Worker Process Execution: STRICTLY PROHIBITED (Simulated via harness test doubles).                 |
|  - Required Trust Level: Zero trust.                                                                   |
|                                                                                                        |
|  [ MODE 3: CONTROLLED DYNAMIC VERIFICATION ]                                                           |
|  - Operations: Worker spawned out-of-process; exercises real handshake, ping/pong, and teardown.       |
|  - Worker Process Execution: PERMITTED under Win32 Job Object, Medium IL, 512MB RAM, 10s boot timeout. |
|  - Required Trust Level: Explicit Local Developer Authorization Required. Zero network / Zero game.    |
+--------------------------------------------------------------------------------------------------------+
```

---

## 4. OVERALL CONFORMANCE DECISION LOGIC

### 4.1 Result Classification Vocabulary
Every test case evaluated by the harness MUST resolve to exactly one of the following six normalized states:
- **`PASS`**: All applicable assertions in the test case executed and evaluated to true against authoritative contracts.
- **`FAIL`**: At least one mandatory assertion evaluated to false or returned an illegal payload.
- **`BLOCKED`**: The test could not execute because an essential prerequisite, contract schema, or admission gate failed.
- **`SKIPPED`**: The test was intentionally omitted under an approved profile filter (e.g., skipping dynamic tests in static mode).
- **`NOT_APPLICABLE`**: The test targets a capability, integration strategy, or optional feature not declared by the adapter.
- **`ERROR`**: The harness itself encountered an internal unhandled exception, resource failure, or environmental fault.

### 4.2 Suite-Level Conformance Decision Table
The overall conformance determination is computed deterministically using the following decision rules:

| Condition Check | Priority | Overall Conformance Status | Registry Admission Eligibility |
| :--- | :---: | :---: | :---: |
| Any mandatory test fails (`FAIL > 0`) | 1 | **`NON_CONFORMANT`** | **Ineligible** (Hard Block) |
| Any security / sandbox check fails | 1 | **`SECURITY_VIOLATION`** | **Ineligible** (Quarantine Recommended) |
| Any test errors internally (`ERROR > 0`) | 2 | **`INCONCLUSIVE`** | **Ineligible** (Harness Fault) |
| Any mandatory test blocked (`BLOCKED > 0`) | 3 | **`INCOMPLETE`** | **Ineligible** (Prerequisites Unmet) |
| Only advisory / optional warnings exist | 4 | **`CONFORMANT_WITH_WARNINGS`** | **Eligible** (Subject to Step 37 Gates) |
| All mandatory & applicable tests pass | 5 | **`FULLY_CONFORMANT`** | **Eligible** (Subject to Step 37 Gates) |

> [!CAUTION]
> The harness MUST NEVER compute an average "conformance score" or percentage to obscure a mandatory contract failure. A single failing mandatory check unconditionally yields `NON_CONFORMANT`.

---

## 5. RELATIONSHIP TO DOWNSTREAM PLATFORM CAPABILITIES

```
+--------------------------------------------------------------------------------------------------------+
|                               CONFORMANCE HARNESS TRACEABILITY CHAIN                                   |
|                                                                                                        |
|  [ Step 34: Universal Adapter SDK Foundation ] ----------> Baseline Wire Protocols & Interfaces       |
|  [ Step 35: Canonical Capability Taxonomy ] ------------> Canonical Capability Identifiers             |
|  [ Step 36: Capability Validation Engine ] --------------> 11-Stage Structural & Semantic Rules         |
|  [ Step 37: Validated Declaration Registry ] -----------> Registration Eligibility Evidence Baseline   |
|  [ Step 38: Capability Matching Engine ] ---------------> Requirement Resolution Test Doubles          |
|  [ Step 39: Capability Negotiation Engine ] ------------> Contextual Availability & Degradation Scenarios|
|  [ Step 40: Adapter Lifecycle & Health Contract ] ------> Two-Dimensional State Machine Invariants     |
|                                                                                                        |
|                                                  |                                                     |
|                                                  v                                                     |
|                                 [ STEP 41: CONFORMANCE TEST HARNESS ]                                  |
|                                                  |                                                     |
|                                                  v                                                     |
|                        (Generates Verified Test Evidence Artifacts & Audit Logs)                       |
|                                                  |                                                     |
|                                                  v                                                     |
|                     [ Step 42: Phase 5 SDK & Capability System Readiness Review ]                      |
+--------------------------------------------------------------------------------------------------------+
```

---

## 6. SPECIFICATION SUMMARY AND INVARIANTS

1. **Deterministic Execution:** Given identical adapter packages, harness versions, and synthetic fixtures, test outcomes MUST be 100% reproducible.
2. **Immutable Audit Trails:** Conformance runs emit structured JSON evidence files signed by a rolling SHA-256 digest on Volume D (`DEC-009`).
3. **Fail-Safe Defaults:** If an input package or manifest contains unknown fields with mandatory semantics, the harness MUST reject the package with `ERR_UNSUPPORTED_MANDATORY_EXTENSION`.

---
*End of Specification — Document `NLE-SPC-ACH-001`.*
