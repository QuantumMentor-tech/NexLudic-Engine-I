# NexLudic Engine — Pilot Success Metrics

**Document ID:** `NLE-MET-PILOT-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 7 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior QA Engineering  

---

## 1. Executive Summary & Epistemic Grounding

The **Pilot Success Metrics** standard defines the quantitative, reproducible criteria for measuring the performance, stability, extensibility, and safety of the NexLudic Engine pilot portfolio.

### Critical Epistemic Rule (Zero Fabricated Results)
In strict adherence to Step 7 engineering mandates (`AC-12`), this document enforces absolute distinction between:
- **Proposed Target:** A predeclared engineering design objective based on technical standards (e.g., $16.6\text{ ms}$ for 60 Hz frame synchronization).
- **Provisional Target:** A working threshold subject to future empirical calibration once hardware test rigs are active.
- **Observed Measurement:** An actual, measured data point gathered from reproducible test execution logs. *(All observed fields currently read `N/A (Pre-Implementation)` — hypothetical numbers are strictly prohibited).*
- **Unavailable Measurement:** Metrics that cannot be sampled until runtime experiments are authorized.

---

## 2. Portfolio-Level Success Metrics Dashboard

```
+---------------------------------------------------------------------------------------------------------------------------------+
|                                              PORTFOLIO-LEVEL SUCCESS METRICS                                                    |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| Metric ID     | Metric Name & Scope               | Metric Status      | Target Threshold       | Observed Measurement          |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-01   | Integration Strategy Coverage     | Proposed Target    | $\ge 3$ Strategies     | N/A (Pre-Implementation)      |
|               | (Strategies A, B, and C evaluated)|                    | (100% of defined types)| (3 Planned in Portfolio)      |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-02   | Distinct Engine Architectures     | Proposed Target    | $\ge 3$ Engine Types   | N/A (Pre-Implementation)      |
|               | (JVM, Unity Mono, REDengine / UE) |                    | (JVM, Unity, Native)   | (4 Planned in Portfolio)      |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-03   | Multi-Language Toolchain Coverage | Proposed Target    | $\ge 3$ Toolchains     | N/A (Pre-Implementation)      |
|               | (Java/Gradle, C#/MSBuild, Lua)    |                    | (Zero JVM-only lock-in)| (3 Planned in Portfolio)      |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-04   | Universal Contract Reusability    | Provisional Target | $\ge 80.0\%$ Interface | N/A (Pre-Implementation)      |
|               | (Shared adapter interfaces reused)| (Requires Calib.)  | Code Reuse across pairs| (Evaluation Pending Gate G5)  |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-05   | Pre-Implementation Blocker Catch  | Proposed Target    | $100.0\%$ of Policy/   | 100.0% (Step 7 Register)      |
|               | (Infeasible pairs stopped at G0/G1|                    | Anti-Cheat Blockers    | (CAND-005, CAND-008 Rejected) |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
| MET-PORT-06   | Zero Binary Asset Bundling        | Mandatory Standard | $0$ Copyrighted Assets | 100.0% Compliant              |
|               | (Non-distributive asset extraction|                    | Bundled in Core Engine | (REQ-MOD-003 Enforced)        |
+---------------+-----------------------------------+--------------------+------------------------+-------------------------------+
```

---

## 3. Pilot-Level Engineering Performance Thresholds

The following performance thresholds must be satisfied during future authorized pilot test runs at Gate G4:

### 3.1 Pilot 1: Bounded Host-Game Scaffolding (`CAND-PAIR-001` — Minecraft + RDR2)
- **Scaffolding Compilation Time (`MET-P1-01`):** Automated CLI compilation from scaffold generation to final `.jar` must complete in $\le 60.0\text{ seconds}$ on standard developer hardware. *(Status: Proposed Target)*.
- **Client Frame Rate Impact (`MET-P1-02`):** Minecraft frame rate degradation while Dead-Eye shader and Cattleman weapon raycasting are active must be $\le 3.0\text{ FPS}$ compared to vanilla baseline. *(Status: Proposed Target)*.
- **Java Heap Working Set Delta (`MET-P1-03`):** Memory overhead introduced by mod classes and assets must be $\le 128.0\text{ MB}$. *(Status: Proposed Target)*.
- **Rollback Cleanliness (`MET-P1-04`):** File system check following `cleanup.ps1` must show $100.0\%$ pristine match against pre-test hash. *(Status: Mandatory Standard)*.

### 3.2 Pilot 2: Multi-Toolchain Extensibility (`CAND-PAIR-002` — Risk of Rain 2 + CoD Zombies)
- **MSBuild Compilation Time (`MET-P2-01`):** BepInEx plugin C# compilation to `.dll` must complete in $\le 15.0\text{ seconds}$. *(Status: Proposed Target)*.
- **Hook Attachment Latency (`MET-P2-02`):** HarmonyX dynamic patch injection during game bootstrap must take $\le 500.0\text{ ms}$ total startup overhead. *(Status: Proposed Target)*.
- **Memory Leak Rate (`MET-P2-03`):** Memory growth rate during continuous 30-minute wave survival loop must be $\le 2.0\text{ MB/hour}$ (indicating zero uncollected entity references). *(Status: Proposed Target)*.
- **Assembly Collision Rate (`MET-P2-04`):** Zero unhandled `TypeLoadException` or `FileLoadException` events in Unity output log. *(Status: Mandatory Standard)*.

### 3.3 Pilot 3: Live Interoperability Bridge (`CAND-PAIR-003` — Rocket League + Cyberpunk 2077)
- **Round-Trip IPC Latency (`MET-P3-01`):** Windows Named Pipe round-trip packet transport time (Rocket League telemetry send $\to$ bridge daemon $\to$ Cyberpunk CET receive) must achieve:
  - Mean Latency: $\le 4.0\text{ ms}$.
  - 99th Percentile: $\le 16.6\text{ ms}$ (1 frame boundary at 60 FPS). *(Status: Proposed Target)*.
- **Continuous Sampling Throughput (`MET-P3-02`):** Sustained packet throughput must maintain $60.0\text{ Hz} \pm 1.0\text{ Hz}$ across a continuous 10-minute test run. *(Status: Proposed Target)*.
- **Host Crash Isolation (`MET-P3-03`):** Abrupt termination (`kill -9`) of Cyberpunk 2077 or the CET plugin during continuous telemetry streaming must result in **$0$ host crashes** in Rocket League and $\le 1.0\text{ FPS}$ transient frame jitter. *(Status: Mandatory Standard)*.
- **Reconnection Recovery Time (`MET-P3-04`):** Automatic pipe reconnection following receiver restart must re-establish communications within $\le 2.0\text{ seconds}$. *(Status: Proposed Target)*.

### 3.4 Pilot 4: Hybrid Integration Architecture (`CAND-PAIR-004` — Minecraft + Spider-Man)
- **Momentum Physics Solver Overhead (`MET-P4-01`):** Web-swinging spring-damping calculation inside the client tick loop must consume $\le 0.5\text{ ms}$ per tick ($20\text{ Hz}$ tick loop budget is $50.0\text{ ms}$). *(Status: Proposed Target)*.
- **Non-Blocking Telemetry Polling (`MET-P4-02`):** External window state and input polling thread must consume $\le 1.5\%$ single-core CPU utilization. *(Status: Proposed Target)*.

---

## 4. Calibration & Review Protocol

1. **Pre-Execution Baseline Sampling:** Prior to executing any pilot test harness, baseline system performance (unmodded game FPS, CPU usage, RAM working set) must be sampled for 10 minutes to calibrate environmental deltas.
2. **Post-Pilot Target Recalibration:** Following the completion of Stage 1 and Stage 2 pilots, provisional targets will be recalibrated against empirical telemetry to refine Stage 3 and Stage 4 thresholds.

---

*End of Pilot Success Metrics (NLE-MET-PILOT-001).*

