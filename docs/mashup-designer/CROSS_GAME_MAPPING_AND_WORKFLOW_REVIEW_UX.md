# NexLudic Engine: Cross-Game Mapping & Workflow Review UX

**Document ID:** `NLE-UX-IPM-001`  
**Phase:** 6 of 12 (Mashup Designer & Integration Planning)  
**Step:** 50 of 100  
**Classification:** Semantic Mapping & Operational Workflow UX Specification  
**Status:** IMPLEMENTATION-READY SPECIFICATION  

---

## 1. Cross-Game Data Mapping Review Experience

The **Cross-Game Mappings** sub-view (`?view=mappings`) enables creators and engineers to audit the data contracts defined in Step 47 that translate information concepts between participating games.

```
+----------------------------------------------------------------------------------------------------+
| DATA MAPPINGS VIEWPORT                                                                             |
|                                                                                                    |
| Total Mappings: 8 | Lossless: 6 | Approximate: 2 | Truncated: 0 | Unmapped: 0                      |
+----------------------------------------------------------------------------------------------------+
| MAPPING INVENTORY TABLE                                                                            |
|                                                                                                    |
| MAPPING ID       | SOURCE CONCEPT       | TARGET CONCEPT       | FIDELITY    | FORMULA / TRANSFORM |
| -----------------+----------------------+----------------------+-------------+---------------------|
| MAP-VELOCITY-01  | Orion: VehicleSpeed  | Atlas: HudSpeedometer| [✓] LOSSLESS| speed_mps * 2.23694 |
| MAP-COORDS-02    | Orion: PosVector3D   | Atlas: WorldPosition | [▲] APPROX  | AffineMatrixOffset  |
| MAP-HEALTH-03    | Orion: EngineDamage  | Atlas: VehicleHealth | [✓] LOSSLESS| 100.0 - (dmg * 10.0)|
| MAP-LIGHTING-04  | Orion: SunDirection  | Atlas: DirectionLight| [▲] APPROX  | SphericalHarmonicMap|
+----------------------------------------------------------------------------------------------------+
```

### 1.1 Fidelity Loss Classification Presentation
Data loss or precision drift across game engines is classified into 4 distinct tiers:

| Fidelity Tier | Visual Presentation | Meaning | User Guidance / Consequence |
| :--- | :--- | :--- | :--- |
| **`LOSSLESS`** | `[✓] Solid Green Badge` | Exact mathematical mapping; 100% reversible; zero precision loss. | Safe for bidirectional physics mirroring. |
| **`APPROXIMATE`**| `[▲] Amber Warning Badge`| Minor precision drift (e.g., float64 to float32 or grid offset). | Tolerable for cosmetic/environmental cues. |
| **`TRUNCATED`** | `[!] Orange Alert Badge` | Multi-dimensional state collapsed to scalar; irreversible. | Displays warning card explaining loss scope. |
| **`UNMAPPED`** | `[✕] Solid Red Error` | Target concept expects data that source does not supply. | Blocks plan acceptance (`ERR_UNMAPPED_DATA_HANDOFF`). |

### 1.2 Mapping Detail Inspector Card
Selecting a mapping row opens the **Contextual Inspector** to show field-level transformation math:

```
+--------------------------------------------------------------------+
| CONTEXTUAL INSPECTOR: Mapping Specification                        |
+--------------------------------------------------------------------+
| Mapping ID: MAP-COORDS-02                                          |
| Source: Game Orion (custom engine, double-precision Cartesian XYZ) |
| Target: Game Atlas (Unreal Engine 5.3, single-precision L-Hand Z-Up)|
| Status: [▲] APPROXIMATE FIDELITY (Precision Warning)               |
+--------------------------------------------------------------------+
| MATHEMATICAL TRANSFORMATION:                                       |
| target.X = (source.X - OriginOffsetX) * 100.0f;                    |
| target.Y = (source.Z - OriginOffsetZ) * 100.0f;  // Swap Y/Z       |
| target.Z = (source.Y - OriginOffsetY) * 100.0f;                    |
+--------------------------------------------------------------------+
| PRECISION BOUNDS & CLAMPING:                                       |
| - Source Range: [-50000.0, +50000.0] meters                        |
| - Target Range: [-5000000, +5000000] Unreal centimeters            |
| - Drift Ceiling: <= 0.005 cm relative drift per frame              |
+--------------------------------------------------------------------+
| REVERSIBILITY: Non-Reversible (Z-axis offset truncation)           |
| TRACEABILITY: Supports [REQ-MAND-002: World Position Sync]         |
+--------------------------------------------------------------------+
```

---

## 2. Cross-Game Interaction Workflow Review Experience

The **Interaction Workflows** sub-view (`?view=workflows`) presents the temporal orchestration, step sequences, timeouts, and failure compensation rules established in Step 48.

```
+----------------------------------------------------------------------------------------------------+
| INTERACTION WORKFLOW VIEWPORT                                                                      |
|                                                                                                    |
| Workflow: WF-TELEMETRY-STREAM-01 | Steps: 4 | Max Latency Budget: 16.6ms | Compensation: Enabled   |
+----------------------------------------------------------------------------------------------------+
| WORKFLOW STEP SEQUENCE SWIMLANE                                                                    |
|                                                                                                    |
| [Step 1: Poll Orion Telemetry]                                                                     |
| Responsible: COMP-ORION-HOST-HOOK | Timeout: 5ms | Retry: None (Drop Frame)                        |
| Prerequisite: Shared memory ring buffer ready                                                      |
|           |                                                                                        |
|           v (Success)                                                                              |
| [Step 2: Transform Coordinates & Speed]                                                            |
| Responsible: COMP-PROTOCOL-TRANSLATOR | Transform: MAP-VELOCITY-01, MAP-COORDS-02                  |
| Execution: In-process C# memory buffer (<0.5ms)                                                    |
|           |                                                                                        |
|           v (Success)                                                                              |
| [Step 3: Dispatch IPC Packet to Atlas Mod]                                                         |
| Responsible: COMP-LIVE-IPC-BRIDGE | Transport: Win32 Named Pipe | Timeout: 8ms                      |
| Timeout Policy: Exponential backoff (max 3 retries)                                                |
|           |                                                                                        |
|           v (Success)                                                                              |
| [Step 4: Update Atlas Actor State]                                                                 |
| Responsible: COMP-ATLAS-MOD-PLUGIN | Method: SetActorLocationAndSpeed()                            |
| Failure Compensation: Revert to dead-reckoning extrapolation [COMPENSATE-DEAD-RECKON]              |
+----------------------------------------------------------------------------------------------------+
```

### 2.1 Workflow Failure & Compensation Presentation
When reviewing a workflow step, creators and reviewers can inspect the **Failure Behavior Matrix**:

```mermaid
flowchart TD
    Step["Workflow Step Execution"] --> Check{"Result?"}
    Check -->|Success| Next["Proceed to Next Step in Sequence"]
    Check -->|Timeout (>8ms)| Retry{"Retry Budget Remaining?"}
    Retry -->|Yes| DoRetry["Retry with Exponential Backoff (1x, 2x)"]
    Retry -->|Exhausted| Fail["Trigger Compensation Handler"]
    
    Fail --> Comp["COMPENSATE-DEAD-RECKON: Extrapolate trajectory from last known frame"]
    Comp --> Alert["Emit Diagnostic WARN_IPC_STREAM_DEGRADED"]
```

- **Clear Epistemic Distinction:** The interface explicitly indicates:
  > *"Notice: This diagram illustrates planned sequence and failure compensation logic. It does not indicate that this workflow has been executed in live game runtimes."*
- **Unmapped Data Detection:** If a workflow step references a cross-game data handoff that lacks an entry in Step 47, the step node is highlighted in red with badge `[✕] ERR_UNMAPPED_DATA_HANDOFF`, linking directly to the Mappings sub-view for resolution.
