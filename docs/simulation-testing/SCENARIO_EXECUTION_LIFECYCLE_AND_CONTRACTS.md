# SCENARIO EXECUTION LIFECYCLE AND CONTRACTS SPECIFICATION

**Document Identifier:** `NLE-CON-SIM-001`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** AUTHORITATIVE SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior QA Automation Engineer, Senior Distributed-Systems Engineer  
**Upstream Dependencies:** `NLE-ARCH-SIM-001` (Simulation Architecture), `NLE-CON-PH9-010` (Handoff Contract)  
**Downstream Consumed By:** Test suites and scenario implementations across Phase 10

---

## 1. Scenario Interface Contract (`IScenario`)

Every test scenario in NexLudic Engine must implement `NexLudic.Simulation.Contracts.IScenario`.

```csharp
public interface IScenario
{
    string ScenarioId { get; }
    string Title { get; }
    string Description { get; }
    string Layer { get; }
    IReadOnlyList<string> RequirementIds { get; }
    IReadOnlyList<string> RequiredCapabilities { get; }
    IReadOnlyList<string> Prerequisites { get; }

    Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default);
    Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default);
    Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default);
}
```

### 1.1 Field Contracts
- **ScenarioId:** Globally unique identifier matching roadmap naming conventions (e.g. `T79-01`, `B01`, `G01`).
- **Title & Description:** Unambiguous human-readable metadata describing purpose and scope.
- **Layer:** Exactly one of: `Unit`, `Component`, `Contract`, `Simulation`, `Integration`, `E2E`.
- **RequirementIds:** One or more traceable requirement keys (e.g. `SIM-REQ-001`, `REQ-TEST-001`).
- **Prerequisites:** Explicit contractual or component prerequisites. If any prerequisite is missing in the test environment, the runner must report the scenario as `Blocked` without executing `SetupAsync`.

---

## 2. Six-Stage Scenario Execution Lifecycle

The `ScenarioLifecycleManager` enforces a deterministic, failure-safe 6-stage lifecycle:

```
  +-------------------------------------------------------------------------+
  |                   SIX-STAGE SCENARIO EXECUTION LIFECYCLE                |
  +-------------------------------------------------------------------------+
  |  [Stage 1: Validation & Registration]                                   |
  |   - Verify ScenarioId, Title, Layer metadata. Reject malformed defs.    |
  +-------------------------------------------------------------------------+
                                     |
                                     v
  +-------------------------------------------------------------------------+
  |  [Stage 2: Prerequisite Evaluation]                                     |
  |   - Evaluate mandatory prerequisites against available contracts.       |
  |   - If unsatisfied: Return BLOCKED immediately; skip Setup & Execute.   |
  +-------------------------------------------------------------------------+
                                     |
                                     v
  +-------------------------------------------------------------------------+
  |  [Stage 3: Context Initialization & Isolation]                          |
  |   - Generate RunId, assign DeterministicClock, DiagnosticsCollector,    |
  |     and isolated temporary scratch directory.                           |
  +-------------------------------------------------------------------------+
                                     |
                                     v
  +-------------------------------------------------------------------------+
  |  [Stage 4: Execution Pipeline (Setup -> Execute)]                       |
  |   - Execute SetupAsync; track allocated disposables.                    |
  |   - Execute ExecuteAsync; evaluate domain assertions.                   |
  |   - Catch ScenarioAssertionException (FAIL) or Exception (ERROR).       |
  +-------------------------------------------------------------------------+
                                     |
                                     v
  +-------------------------------------------------------------------------+
  |  [Stage 5: Guaranteed Failure-Safe Teardown]                            |
  |   - ALWAYS execute TeardownAsync, regardless of Stage 4 outcome.        |
  |   - If Teardown fails: Preserve both primary failure and teardown error.|
  +-------------------------------------------------------------------------+
                                     |
                                     v
  +-------------------------------------------------------------------------+
  |  [Stage 6: Context Cleanup & Result Compilation]                        |
  |   - Dispose all tracked IDisposable / IAsyncDisposable instances.       |
  |   - Delete temporary scratch directory.                                 |
  |   - Compile immutable ScenarioExecutionResult with complete diagnostics.|
  +-------------------------------------------------------------------------+
```

---

## 3. Failure-Safe Teardown Guarantees

Teardown guarantees are critical to prevent state pollution:
1. **Unconditional Invocation:** If `SetupAsync` begins, `TeardownAsync` is guaranteed to be invoked during the finally block.
2. **Partial Setup Protection:** Resources registered with `context.TrackDisposable(...)` before a setup failure are safely disposed during context cleanup.
3. **Dual Failure Preservation:** If an assertion fails during `ExecuteAsync` AND an error occurs during `TeardownAsync`, the runner captures both via an `AggregateException` rather than swallowing the primary assertion.
4. **Scratch Cleanup:** Dedicated scenario scratch directories (`NexLudicSim_<ScenarioId>_<RunId>`) are deleted upon context disposal.

---

## 4. Context State Isolation

The `ScenarioContext` provides complete operational isolation:
- **In-Memory State Bag:** `SetState<T>()` and `TryGetState<T>()` are scoped strictly to the scenario instance.
- **No Shared Singletons:** Test doubles, clocks, and diagnostics collectors are instantiated per context.
- **Thread Safety:** State storage utilizes thread-safe concurrent collections (`ConcurrentDictionary`, `ConcurrentBag`).\n