# NexLudic Engine: AI Agent Failure, Resource & Recovery Policy

**Document ID:** `NLE-POL-AFR-001`  
**Phase:** 7 of 12 (AI Coding-Agent Orchestration)  
**Step:** 52 of 100  
**Classification:** Reliability, Resource Management & Disaster Recovery Policy  
**Status:** IMPLEMENTATION-READY SPECIFICATION  

---

## 1. Failure Classification Taxonomy

The orchestration engine defines an exhaustive taxonomy of failure modes and specifies deterministic recovery behaviors:

```mermaid
flowchart TD
    Fail["Failure Event Detected"] --> TypeCheck{"Classify Failure Type"}
    
    TypeCheck -->|Transient Network / 429 Rate Limit| Retry["Transient: Exponential Backoff Retry (Max 3x)"]
    TypeCheck -->|Context Window Overflow| HaltContext["Fatal Context: Abort & Emit ERR_CONTEXT_EXCEEDED"]
    TypeCheck -->|Diff Scope Whitelist Breach| HaltScope["Security Breach: Rollback & Emit ERR_SCOPE_VIOLATION"]
    TypeCheck -->|Test Runner Failure (Tests Failed)| Feedback["Validation Defect: Package trx Log & Transition REVISION_REQUESTED"]
    TypeCheck -->|Execution Timeout (>300s)| HaltTimeout["Watchdog Timeout: Force Kill Process & Rollback Workspace"]
```

### 1.1 Canonical Failure Response Matrix

| Failure Key | Description | Immediate Action | Task State Transition | Retry Allowed? |
| :--- | :--- | :--- | :--- | :---: |
| `ERR_NETWORK_DISCONNECT` | TLS socket drop or cloud API timeout. | Retry with exponential backoff (2s, 4s, 8s). | Retain in `RUNNING` | Yes (up to 3x) |
| `ERR_RATE_LIMIT_EXCEEDED`| Provider HTTP 429 quota reached. | Parse `Retry-After` header; pause queue. | Retain in `QUEUED` | Yes (after backoff) |
| `ERR_CONTEXT_LIMIT_EXCEEDED`| Input prompt exceeds model context window. | Abort attempt; notify user to trim files. | Transition to `FAILED` | No |
| `ERR_MALFORMED_OUTPUT` | Agent emitted invalid JSON or broken diff. | Send feedback prompt requesting re-serialization. | Retain in `RUNNING` | Yes (up to 2x) |
| `ERR_TEST_EXECUTION_FAILED`| Unit tests executed and reported failures. | Capture test output; package feedback for agent.| `REVISION_REQUESTED` | Yes (up to 3 attempts) |
| `ERR_SCOPE_VIOLATION` | Patch touches unwhitelisted file. | Discard patch; abort execution attempt. | Transition to `FAILED` | No |
| `ERR_TASK_TIMEOUT` | Task active duration exceeds 300s. | Terminate process tree; restore Git baseline. | Transition to `CANCELLED` | No |
| `ERR_PERMISSION_REVOKED` | User clicked Cancel or revoked auth token. | Terminate process tree; atomic rollback. | Transition to `CANCELLED` | No |

---

## 2. Retry Safety & Idempotency Rules

Retrying an AI task is not an unconstrained loop. The engine enforces strict idempotency boundaries:

1. **Stateful Write Rollback Precondition:**  
   If an agent execution attempt produced partial file writes before a timeout or crash, the orchestrator **must execute a complete atomic rollback to the pre-task baseline** before initiating a retry.
2. **Exponential Backoff Formula:**  
   For transient network or rate-limit retries:
   $$T_{\text{wait}} = \min(T_{\text{max}}, T_{\text{base}} \times 2^{\text{attempt}} + \text{jitter})$$
   Where $T_{\text{base}} = 2.0\text{ s}$, $T_{\text{max}} = 60.0\text{ s}$, and $\text{jitter} \in [0.1, 0.5]\text{ s}$.
3. **Attempt Ceiling Invariant:**  
   A coding task cannot exceed **3 execution attempts**. If a task fails verification on its 3rd attempt, it transitions permanently to `FAILED` and requires human engineer intervention.

---

## 3. Resource Governance & Cost Control

To protect host CPU/memory resources and prevent runaway AI API billing, the orchestrator imposes hard resource ceilings:

```
+----------------------------------------------------------------------------------------------------+
| RESOURCE GOVERNANCE LIMITS & CEILINGS                                                              |
+----------------------------------------------------------------------------------------------------+
| 1. Concurrency Ceiling:                                                                            |
|    - Maximum 2 concurrent running coding tasks across the entire desktop application.              |
| 2. Token Budget Ceilings:                                                                          |
|    - Task Attempt Limit: 64,000 input tokens | 8,192 output tokens.                                |
|    - Cumulative Task Session Ceiling: 250,000 total tokens.                                        |
| 3. Execution Timeouts:                                                                              |
|    - Agent LLM Response Watchdog: 180 seconds.                                                     |
|    - Sandboxed Test Runner Execution: 60 seconds.                                                  |
|    - Total End-to-End Task Timeout: 600 seconds (10 minutes).                                      |
| 4. Storage Quotas on Volume D:                                                                     |
|    - Maximum 50 MB disk space allocated per task sandbox in scratch/tasks/<task_id>/.             |
|    - Sandboxes for completed tasks automatically pruned after 24 hours.                            |
+----------------------------------------------------------------------------------------------------+
```

---

## 4. Observability & Tamper-Evident Audit Logging

Every interaction across the AI coding-agent lifecycle is recorded in a tamper-evident, append-only JSON journal located on Volume D at:  
`D:\App Winrar Files\Nexludic Engine (working)\projects\<project_id>\logs\ai_orchestration_audit.jsonl`

### 4.1 Log Entry Schema
Each log entry contains:
- `timestamp_utc`: RFC 3339 timestamp.
- `task_id` & `attempt_id`: Unique tracking references.
- `event_type`: One of `TASK_AUTHORIZED`, `PROMPT_DISPATCHED`, `TOOL_INVOKED`, `DIFF_INSPECTED`, `TEST_EXECUTED`, `HUMAN_APPROVED`, `ROLLBACK_EXECUTED`.
- `payload_hash_sha256`: Cryptographic hash of the event payload.
- `previous_event_hash_sha256`: Cryptographic hash chaining to ensure tamper evidence.
- Redaction guarantee: Zero plaintext passwords, API keys, or private user credentials appear in audit logs.
