# STEP 79 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-079`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** ALL DECISIONS EVALUATED AND ACCEPTED  
**Authoring Roles:** Principal Software Architect, Senior Test Automation Architect  

---

## 1. Architectural Decisions Summary

| ADR ID | Decision Title | Decision Status | Summary of Resolution |
| :--- | :--- | :---: | :--- |
| **ADR-SIM-001** | Core Runtime Selection | **ACCEPTED** | C# 12 / .NET 8 LTS adopted; compiles natively with `dotnet 8.0.425`. |
| **ADR-SIM-002** | In-Memory Scenario Lifecycle | **ACCEPTED** | 6-stage lifecycle in `ScenarioLifecycleManager` without heavy container overhead. |
| **ADR-SIM-003** | Virtual Clock vs Real Sleeps | **ACCEPTED** | `IDeterministicClock` adopted; zero `Thread.Sleep` in simulation harnesses. |
| **ADR-SIM-004** | Seven-State Outcome Taxonomy | **ACCEPTED** | Distinct Passed, Failed, Error, Skipped, Blocked, NotExecuted, Inconclusive states. |
| **ADR-SIM-005** | Security & Anti-Cheat Boundaries| **ACCEPTED** | Strict Medium IL enforced; `DisallowProcessMemoryAccess = true` mandatory. |
| **ADR-SIM-006** | Teardown Failure Preservation | **ACCEPTED** | `AggregateException` preserves both execution and teardown errors. |
| **ADR-SIM-007** | Modular Test Double Taxonomy | **ACCEPTED** | Precise separation: Stub, Mock, Fake, Simulator, Synthetic Endpoint, Real. |
| **ADR-SIM-008** | Seed-Based Failure Replay | **ACCEPTED** | `ReproducibleSeedProvider` embedded in result metadata for bit-for-bit replay. |
| **ADR-SIM-009** | Test/Production Assembly Separation| **ACCEPTED** | Simulation code isolated in `NexLudic.Simulation`; excluded from release desktop binary. |
| **ADR-SIM-010** | Phase 9 Master Suite Adoption | **ACCEPTED** | Handoff of 244 synthetic scenarios scheduled across Steps 80–86. |

---

## 2. Detailed Decision Records

### ADR-SIM-001: Core Runtime Selection (.NET 8 LTS)
- **Context:** Phase 10 requires an extensible, cross-game testing harness.
- **Decision:** Adopt C# 12 and .NET 8 LTS as specified in `NLE-ARCH-BASE-001`.
- **Consequences:** Pre-installed runtime on dev host; high-performance memory semantics; full xUnit test runner compatibility.

### ADR-SIM-003: Virtual Clock Abstraction (`IDeterministicClock`)
- **Context:** Protocol and network simulation requires simulating latencies without slowing test execution or introducing race conditions.
- **Decision:** Use `IDeterministicClock` to advance virtual time monotonically.
- **Consequences:** Tests execute in sub-millisecond CPU time while modeling multi-second distributed timeouts accurately.

### ADR-SIM-005: Anti-Cheat & Process Isolation Inviolability
- **Context:** Testing live bridge adapters must never jeopardize player accounts or violate anti-cheat protections.
- **Decision:** Enforce Medium Integrity Level and prohibit live process memory scanning (`ReadProcessMemory`) or DLL injection in all simulation doubles.
- **Consequences:** Guarantees 100% compliance with game security policies; prevents false-positive anti-cheat flags.\n