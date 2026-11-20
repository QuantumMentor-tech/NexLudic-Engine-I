# SIMULATION FRAMEWORK ARCHITECTURE AND TEST LAYERS SPECIFICATION

**Document Identifier:** `NLE-ARCH-SIM-001`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** AUTHORITATIVE ARCHITECTURAL SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior Test Automation Architect, Senior Simulation Engineer  
**Upstream Dependencies:** `NLE-CON-PH9-010` (Phase 9 Handoff Contract), `NLE-GAT-PH9-001` (Phase 9 Gate), `NLE-ARCH-BASE-001` (.NET 8 Baseline)  
**Downstream Consumed By:** Steps 80 through 87 (Simulation Harnesses, Automated Tests, Fault Injection)

---

## 1. Executive Summary and Architectural Mission

Phase 10 of the NexLudic Engine roadmap marks the formal transition from specification-only architecture (Phases 1–9) to executable, reproducible, automated simulation and testing. 

NexLudic Engine is designed to support diverse games, modding frameworks, adapters, and cross-game mashups. Its validation infrastructure must therefore provide:
1. **Extensibility Without Uniformity:** It cannot assume all games share the same memory layouts, engines, programming languages, or runtimes.
2. **Strict Test/Production Boundary:** Test primitives must never leak into production code, change runtime defaults, or grant unauthorized process privileges.
3. **Deterministic Simulation:** Real-time sleeps and race conditions must be replaced with virtual clocks and reproducible seed providers.
4. **Evidence Truthfulness:** A completed document is not code; a mocked adapter is not a live game integration; a passing unit test is not proof of end-to-end multi-game compatibility.

---

## 2. Testing Layer Boundaries and Responsibility Hierarchy

The Phase 10 simulation architecture divides verification into six strictly bounded layers:

```
+----------------------------------------------------------------------------------------------------+
|                                    NEXLUDIC TESTING HIERARCHY                                      |
+----------------------------------------------------------------------------------------------------+
| Layer 6: End-to-End Game-Pair Tests (E2E)                                                          |
| - Requires: Live game installations, approved adapters, user consent tokens.                      |
| - Boundary: Full desktop application workflow from game launch to live mashup state exchange.      |
+----------------------------------------------------------------------------------------------------+
| Layer 5: Integration Tests (Real Components)                                                       |
| - Requires: Real engine modules (Named Pipes, JSON Serializer, Registry, State Engine).            |
| - Boundary: Verifies IPC and component interaction without faking internal communication channels. |
+----------------------------------------------------------------------------------------------------+
| Layer 4: Simulation Tests (Modeled Environments)                                                  |
| - Requires: Test Harness Engine, Synthetic Endpoints, Virtual Clock, Seed Provider.                |
| - Boundary: Exercises complex multi-agent/multi-game protocols using high-fidelity test doubles.   |
+----------------------------------------------------------------------------------------------------+
| Layer 3: Contract Tests (Interface & Schema Conformance)                                          |
| - Requires: Formal JSON-LD schemas (DCD v1, SMR v1, Envelope v1, Snapshot v1, Delta v1).          |
| - Boundary: Validates schema parsing, serialization invariants, and backward compatibility.        |
+----------------------------------------------------------------------------------------------------+
| Layer 2: Component Tests (Subsystem Boundary)                                                     |
| - Requires: Subsystem implementations with controlled dependencies (e.g. Memory Stream, Mock Clock)|
| - Boundary: Verifies behavior of a single module (e.g. LifecycleManager, DiagnosticsCollector).    |
+----------------------------------------------------------------------------------------------------+
| Layer 1: Unit Tests (Isolated Functions & Classes)                                                 |
| - Requires: Zero external services, zero disk I/O, zero network sockets.                           |
| - Boundary: Pure algorithmic validation (e.g. Seed generation, state dictionary, outcome matching).|
+----------------------------------------------------------------------------------------------------+
```

### 2.1 Layer Definitions

1. **Unit Tests (Layer 1):** Validate isolated methods and classes using fixed inputs. Run completely in-memory with zero OS dependencies.
2. **Component Tests (Layer 2):** Validate interactions within a cohesive component boundary (e.g., `ScenarioLifecycleManager` coordinating setup, execution, and teardown).
3. **Contract Tests (Layer 3):** Validate that data producers and consumers adhere to Phase 9 schema specifications without inventing APIs.
4. **Simulation Tests (Layer 4):** Model multi-endpoint behavior using synthetic endpoints (`ISyntheticEndpoint`) and virtual time (`IDeterministicClock`). Output must explicitly tag simulated elements.
5. **Integration Tests (Layer 5):** Validate real inter-module communication (e.g., Named Pipes across local processes) without mock channels.
6. **End-to-End Tests (Layer 6):** Full user workflows across authorized game processes. Strictly blocked in Step 79 under Phase 9 condition `P9-C01`.

---

## 3. Technology Stack & Component Architecture

### 3.1 Runtime and Framework Selection
- **Core Runtime:** C# 12 / .NET 8.0 SDK (LTS) (`dotnet 8.0.425`).
- **Core Library:** `NexLudic.Simulation` (`src/NexLudic.Simulation/`).
- **Test Runner:** xUnit v2.8+ with Microsoft.NET.Test.Sdk (`tests/NexLudic.Simulation.Tests/`).
- **Isolation Mechanism:** Per-scenario context (`ScenarioContext`), in-memory queues, dedicated temporary scratch directories (`NexLudicSim_<ScenarioId>_<RunId>`), and virtual clocks.

### 3.2 Modular Component Breakdown

```
+---------------------------------------------------------------------------------------+
|                                  NexLudic.Simulation                                  |
+---------------------------------------------------------------------------------------+
|  [Contracts]                                                                          |
|   - IScenario: Authoritative scenario interface                                       |
|   - ScenarioContext: Isolated execution context, state store & disposable tracker     |
|   - ScenarioOutcome: 7-state authoritative outcome taxonomy                           |
|   - ScenarioExecutionResult: Enriched execution report with diagnostics and metadata  |
|   - IScenarioRegistry: Scenario discovery and lookup contract                         |
+---------------------------------------------------------------------------------------+
|  [Harness]                                                                            |
|   - TestHarnessEngine: Top-level entry point and runner                               |
|   - ScenarioLifecycleManager: 6-stage lifecycle, failure-safe teardown orchestrator   |
|   - TestHarnessConfiguration: Environment, seed, and timeout settings                 |
+---------------------------------------------------------------------------------------+
|  [Determinism]                                                                        |
|   - IDeterministicClock / DeterministicClock: Controllable virtual time               |
|   - ReproducibleSeedProvider: Deterministic pseudo-random generation                  |
+---------------------------------------------------------------------------------------+
|  [Diagnostics]                                                                        |
|   - IDiagnosticsCollector / DiagnosticsCollector: Sanitized event logging             |
|   - ErrorClassification: Root-cause taxonomy (ProductDefect, UnresolvedContract, etc)|
+---------------------------------------------------------------------------------------+
|  [Doubles]                                                                            |
|   - ISyntheticEndpoint: Simulation mock endpoint contract                             |
|   - SyntheticMockEndpoint: In-memory mock adapter (Ack, Reject, Echo, Drop, Fault)   |
|   - SyntheticMessageEnvelope: Step 73 compliant envelope structure                    |
+---------------------------------------------------------------------------------------+
```

---

## 4. Production Separation and Security Policy

1. **Packaging Separation:** `NexLudic.Simulation` is compiled as a distinct test and simulation assembly. It is never merged into the desktop client executable.
2. **Zero Game Modification:** Test runners must never call `OpenProcess` with write privileges, scan live process memory (`ReadProcessMemory`), or inject DLLs into third-party binaries.
3. **Execution Integrity:** All test doubles operate strictly at Windows Medium Integrity Level.
4. **Sanitized Diagnostics:** Logs, diagnostic events, and exception traces must never record user credentials, API keys, or unrestricted process memory dumps.

---

## 5. Extensibility Roadmap (Steps 80–87)

- **Step 80:** Multi-Game Simulation Test Harness Architecture & Mock Integration
- **Step 81:** Adapter Protocol & Mock Game Capability Conformance
- **Step 82:** In-Memory Message Bus & Simulated IPC Transport Testing
- **Step 83:** Cross-Game Semantic Translation & Normalization Test Suite
- **Step 84:** Distributed State Sync, Delta Replay & Clock Drift Simulation
- **Step 85:** Cross-Game Conflict Arbitration & Rollback Verification
- **Step 86:** Hybrid Mod-Bridge Orchestration & Fault Injection Suite
- **Step 87:** Phase 10 Simulation & Automated Testing Readiness Gate\n