# REPRODUCIBILITY, DETERMINISM AND FAILURE REPLAY SPECIFICATION

**Document Identifier:** `NLE-SPEC-SIM-001`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** AUTHORITATIVE SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior Simulation Engineer, Senior QA Automation Engineer  
**Upstream Dependencies:** `NLE-ARCH-SIM-001`, `NLE-CON-SIM-001`  
**Downstream Consumed By:** Simulation runners and automated CI pipelines

---

## 1. Principles of Deterministic Simulation

Non-deterministic tests ("flaky tests") degrade confidence and waste engineering cycles. The NexLudic simulation framework enforces determinism across three pillars:

1. **Controlled Time:** Elimination of `Thread.Sleep()` and non-deterministic wall clocks.
2. **Controlled Randomness:** Pseudo-random seed propagation and tracking.
3. **Explicit Event Ordering:** Serialized state transitions and sequenced message processing.

---

## 2. Virtual Clock Abstraction (`IDeterministicClock`)

```csharp
public interface IDeterministicClock
{
    DateTimeOffset UtcNow { get; }
    void Advance(TimeSpan delta);
    void SetTime(DateTimeOffset time);
}
```

### 2.1 Rules of Time Simulation
- **Default Start Time:** Simulated tests initialize at a fixed epoch: `2026-11-20T12:00:00.000Z`.
- **Monotonic Progression:** `Advance(delta)` requires `delta >= TimeSpan.Zero`. Virtual clocks cannot move backward.
- **Zero Real-Time Delays:** Latency simulation is achieved by advancing the virtual clock, executing instantaneously on the CPU.

---

## 3. Seed Management & Pseudo-Random Data Generation

```csharp
public sealed class ReproducibleSeedProvider
{
    public int MasterSeed { get; }
    public ReproducibleSeedProvider(int? seed = null);
    public int Next();
    public int Next(int maxValue);
    public int Next(int minValue, int maxValue);
    public byte[] NextBytes(int count);
    public Random CreateChildRandom(string childContext);
}
```

### 3.1 Seed Governance
- Every scenario run is assigned a discrete seed (default: `42_79_100` or user-specified).
- The seed is embedded in the `ScenarioExecutionResult.Metadata["Seed"]`.
- Child contexts (e.g. packet jitter generators, payload fuzzers) derive independent, repeatable streams via `CreateChildRandom()`.

---

## 4. Failure Replay Contract

When a scenario fails, the framework preserves complete metadata to reproduce the failure on any developer machine:

```json
{
  "scenarioId": "RANDOM-FAIL-01",
  "runId": "run-a1b2c3d4e5f6",
  "seed": 10101,
  "clockEpoch": "2026-11-20T12:00:00.0000000+00:00",
  "configuration": {
    "environmentProfile": "SimulationIsolated",
    "timeoutMs": 30000
  },
  "failure": {
    "summary": "Deterministic failure on seed 10101 with value 42",
    "classification": "ProductDefect"
  }
}
```

### 4.1 Replay Procedure
To reproduce any failure:
1. Supply the recorded `seed` to `engine.RunScenarioAsync(scenarioId, overrideSeed: seed)`.
2. The harness initializes the exact same pseudo-random streams and virtual clock sequence.
3. The identical failure is reproduced deterministically without timing variance.\n