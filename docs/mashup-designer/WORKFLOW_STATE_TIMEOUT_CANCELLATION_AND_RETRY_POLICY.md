# Workflow State, Timeout, Cancellation & Retry Policy

**Document ID:** `NLE-POL-WTC-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED ARCHITECTURAL POLICY`  
**Phase:** `Phase 6 of 12 — Mashup Designer & Integration Planning`  
**Step:** `Step 48 of 100`  
**Classification:** `Execution Semantics, Timeouts, Cancellation & Retry Policy`  
**Related Documents:**
- [CROSS_GAME_INTERACTION_WORKFLOW_AND_SEQUENCING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_INTERACTION_WORKFLOW_AND_SEQUENCING_SPECIFICATION.md) (`NLE-SPC-IWS-001`)
- [CROSS_GAME_WORKFLOW_STEP_AND_INTERACTION_CONTRACT.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_WORKFLOW_STEP_AND_INTERACTION_CONTRACT.md) (`NLE-CON-WSI-001`)
- [ADAPTER_LIFECYCLE_INITIALIZATION_AND_OPERATIONAL_STATE_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/adapters/ADAPTER_LIFECYCLE_INITIALIZATION_AND_OPERATIONAL_STATE_SPECIFICATION.md) (`NLE-SPC-ALC-001`)

---

## 1. Step-Level Operational State Taxonomy

An interaction step advances through twelve formal operational states during its lifecycle:

```mermaid
stateDiagram-v2
    [*] --> NOT_EVALUATED: Initialized
    NOT_EVALUATED --> AWAITING_PRECONDITIONS: Workflow Scheduled
    AWAITING_PRECONDITIONS --> READY_FOR_EXECUTION: Preconditions Verified
    AWAITING_PRECONDITIONS --> BLOCKED: Precondition Failed / Security Blocker
    
    READY_FOR_EXECUTION --> EXECUTING: Dispatched by Orchestrator
    EXECUTING --> COMPLETED: Success Verified (Postconditions True)
    EXECUTING --> COMPLETED_PARTIAL: Partial Result Produced
    EXECUTING --> FAILED: Error Returned / Timeout Exceeded
    EXECUTING --> CANCELLED: Aborted by User / System Event
    
    FAILED --> READY_FOR_EXECUTION: Idempotent Retry Permitted
    COMPLETED --> STALE: Upstream Data Invalidated
    READY_FOR_EXECUTION --> SKIPPED: Preceding Branch Evaluated False
    
    BLOCKED --> [*]
    COMPLETED --> [*]
    CANCELLED --> [*]
    SKIPPED --> [*]
```

---

## 2. Timeout & Bounded Execution Policy

Every step in an interaction workflow must specify a deterministic timeout limit (`timeout_ms`), constrained within $[10\text{ ms}, 60,000\text{ ms}]$:

```mermaid
flowchart TD
    Dispatch["Step Dispatched (Timer Started)"] --> PollCheck{"Response Received within timeout_ms?"}
    
    PollCheck -->|"Yes"| EvalPost["Evaluate Postconditions & Acknowledge"]
    PollCheck -->|"No (Timer Expired)"| TimeoutAction["1. Cancel Pending IPC Call<br/>2. Transition State: FAILED (ERR_STEP_TIMEOUT)<br/>3. Evaluate Potential Side Effects"]
    
    TimeoutAction --> IdemCheck{"Is Step Marked is_idempotent == true?"}
    IdemCheck -->|"Yes"| RetryCheck{"Retry Count < Max Retries (3)?"}
    IdemCheck -->|"No"| Uncertain["State: UNCERTAIN_COMPLETION<br/>(Halt Workflow; Trigger Reconciliation)"]
    
    RetryCheck -->|"Yes"| ExecRetry["Exponential Backoff Retry<br/>(wait = 50ms * 2^attempt + jitter)"]
    RetryCheck -->|"No"| FailStop["Dispatch FAILURE_FALLBACK / Abort"]

    style Dispatch fill:#ebf8ff,stroke:#2b6cb0,stroke-width:1px
    style EvalPost fill:#f0fff4,stroke:#22543d,stroke-width:2px
    style TimeoutAction fill:#fff5f5,stroke:#9b2c2c,stroke-width:1px
    style Uncertain fill:#742a2a,color:#fff,stroke:#e53e3e,stroke-width:2px
```

### Invariant Rules:
1. **Rule RUL-WTC-01 (Mandatory Timeout Bounds):** Unbounded steps with infinite timeouts are prohibited (`timeout_ms == null` is rejected by schema `NLE-SCH-IWS-001`).
2. **Rule RUL-WTC-02 (Uncertain Completion Isolation):** If a non-idempotent operation (e.g. inventory item deduction) times out after the dispatch frame was transmitted, the system **must never blindly repeat the action**. The state is classified as `UNCERTAIN_COMPLETION`, and the engine triggers an authoritative snapshot query to verify whether the side effect occurred.

---

## 3. Cancellation Propagation & Cleanup Governance

Workflows can be cancelled at any point by creator intervention, game process crash, or security violation:

```mermaid
flowchart LR
    CancelEvent["Cancellation Event Raised"] --> CoreCancel["Core Orchestrator Broadcasts CANCEL"]
    CoreCancel --> WorkerA["Cancel Active Step in Adapter A"]
    CoreCancel --> WorkerB["Cancel Active Step in Adapter B"]
    CoreCancel --> BridgeCancel["Drain Bridge Transmit Buffers"]
    
    WorkerA --> ReleaseLocks["Release Temporary File Locks & Resources"]
    WorkerB --> ReleaseLocks
    BridgeCancel --> ReleaseLocks
    
    ReleaseLocks --> LogState["Record CANCELLED in Transaction Log (DEC-009)"]

    style CancelEvent fill:#fff5f5,stroke:#9b2c2c,stroke-width:1px
    style CoreCancel fill:#ebf8ff,stroke:#2b6cb0,stroke-width:1px
    style ReleaseLocks fill:#fefcbf,stroke:#d69e2e,stroke-width:1px
    style LogState fill:#f0fff4,stroke:#22543d,stroke-width:2px
```

### Invariant Rules:
1. **Rule RUL-WTC-03 (Immediate Resource De-allocation):** Upon receiving a cancellation event, components must immediately release temporary staging file handles on Volume D, drop unacknowledged queue buffers, and revert volatile in-memory mirrors.
2. **Rule RUL-WTC-04 (Safe Process State):** Cancelling a cross-game workflow must never terminate or destabilize participating commercial game processes. The engine detaches cleanly without injecting crash signals.

---

## 4. Retry Safety & Idempotency Tokens

Automatic step retries are strictly constrained by the **Idempotency Verification Invariant (`RUL-WTC-05`)**:

| Step Characteristic | `is_idempotent` | Permitted Automated Retries | Maximum Retry Limit | Backoff Algorithm |
| :--- | :---: | :---: | :---: | :--- |
| Read-Only State Query | `true` | **YES** | 3 attempts | Exponential: $50\text{ ms} \times 2^{\text{attempt}} + \text{jitter}$ |
| Telemetry Heartbeat Poll | `true` | **YES** | 3 attempts | Fixed: $16\text{ ms}$ interval |
| Transactional Item Grant | `false` | **NO (Prohibited)** | 0 attempts | Requires manual reconciliation / query |
| Discrete Entity Spawning | `false` | **NO (Prohibited)** | 0 attempts | Requires unique entity ID query before retry |
| UI Overlay Toast Display | `true` | **YES** | 2 attempts | Fixed: $100\text{ ms}$ |

---
*End of Policy — Document `NLE-POL-WTC-001`.*
