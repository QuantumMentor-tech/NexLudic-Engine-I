# NexLudic Engine: AI Agent Session Resource, Timeout & Watchdog Policy

**Document ID:** `NLE-POL-ARW-001`  
**Phase:** 7 of 12 (AI Coding-Agent Orchestration)  
**Step:** 57 of 100  
**Classification:** Resource Governance & Reliability Policy  
**Status:** IMPLEMENTATION-READY SPECIFICATION  
**Upstream Dependencies:** `NLE-MOD-ASL-001` (Domain Model), `NLE-SPC-ASM-001` (State Machine)  
**Downstream Dependents:** `NLE-POL-APT-001` (Termination Policy), `NLE-CON-AAD-001` (Audit Contract)  

---

## 1. Resource Governance Architecture

AI coding sessions can consume immense system resources, hang indefinitely waiting on unresponsive remote models, enter infinite tool-calling loops, or deplete desktop RAM needed by gaming processes.

NexLudic Engine implements an active, multi-dimensional **Resource Watchdog Subsystem** that continuously measures resource consumption and enforces strict limits.

```mermaid
flowchart TD
    subgraph Monitored_Dimensions [Monitored Resource Dimensions]
        T_Wall[Wall-Clock Session Timer<br/>Max 30 min]
        T_Heartbeat[Agent Heartbeat Timer<br/>Max 60 sec]
        T_Tool[Individual Tool Timer<br/>Max 60 sec]
        M_RAM[Committed RAM<br/>Max 2048 MB via Job Object]
        D_Disk[Cumulative Disk Writes<br/>Max 1024 MB]
        C_Tokens[Provider Token Budget<br/>Max 100,000 Tokens]
    end

    subgraph Watchdog_Engine [Supervisor Watchdog Thread (1 Hz)]
        Sampler[Metric Sampler & Correlator]
        ThresholdCheck{Threshold Exceeded?}
    end

    subgraph Actions [Enforcement Actions]
        Warn[80% Threshold: Emit WARNING_RESOURCE_USAGE]
        Abort[100% Threshold: Force Transition to TIMED_OUT / FAILED]
    end

    T_Wall --> Sampler
    T_Heartbeat --> Sampler
    T_Tool --> Sampler
    M_RAM --> Sampler
    D_Disk --> Sampler
    C_Tokens --> Sampler

    Sampler --> ThresholdCheck
    ThresholdCheck -->|>= 80% & < 100%| Warn
    ThresholdCheck -->|>= 100%| Abort
```

---

## 2. Resource Dimensions & Operational Limits

| Dimension | Default Limit | Maximum Permitted | Enforcement Mechanism | Failure Action |
| :--- | :--- | :--- | :--- | :--- |
| **Session Wall-Clock** | 1800 seconds (30m) | 7200 seconds (120m) | Monotonic hardware timer | Transition to `TIMED_OUT`; terminate process tree. |
| **Inactivity / Heartbeat** | 60 seconds | 180 seconds | Event timestamp delta | Probe agent; terminate if unresponsive after 15s. |
| **Single Tool Execution** | 60 seconds | 300 seconds | Async execution timeout | Cancel tool worker process; return `ERR_TOOL_TIMEOUT`. |
| **Process Memory (RAM)** | 2048 MB | 8192 MB | Windows Job Object memory cap | Windows terminates allocating process (`STATUS_QUOTA_EXCEEDED`). |
| **Scratch Disk Space** | 1024 MB | 4096 MB | Supervisor directory write quota | Block further disk writes; return `ERR_DISK_QUOTA_EXCEEDED`. |
| **Cumulative Tool Calls** | 100 calls | 500 calls | Monotonic counter | Transition to `FAILED` (`ERR_TOOL_CALL_BUDGET_EXHAUSTED`). |
| **Provider Token Budget** | 100,000 tokens | 500,000 tokens | Upstream provider token meter | Pause session; alert user for budget expansion. |

---

## 3. Watchdog Thread & Telemetry Emission

1. **Sampling Frequency:** The supervisor executes an internal watchdog loop every 1,000 milliseconds (1 Hz).
2. **Warning Threshold (80% Ceiling):**  
   When any resource reaches 80% of its hard limit (e.g. 24 minutes elapsed, or 1638 MB RAM committed), the supervisor emits a `WATCHDOG_RESOURCE_WARNING` event.
3. **Hard Ceiling (100% Ceiling):**  
   When any resource reaches 100%:
   - The supervisor transitions the session state directly to `TIMED_OUT` (for time limits) or `FAILED` (for memory/disk limits).
   - The supervisor issues a termination signal to the Windows Job Object.
   - A `WATCHDOG_LIMIT_BREACHED` event is committed to the immutable audit ledger.
