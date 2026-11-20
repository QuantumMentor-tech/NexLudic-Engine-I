# TEST OUTCOME MODEL AND STRUCTURED DIAGNOSTICS SPECIFICATION

**Document Identifier:** `NLE-MOD-SIM-001`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 79 of 100  
**Status:** AUTHORITATIVE SPECIFICATION  
**Authoring Roles:** Senior Software Architect, Senior QA Architect, Senior Reliability Engineer  
**Upstream Dependencies:** `NLE-ARCH-SIM-001`, `NLE-CON-SIM-001`  
**Downstream Consumed By:** CI runners, test aggregators, diagnostic analyzers

---

## 1. Authoritative Seven-State Outcome Taxonomy

The simulation harness distinguishes seven discrete outcome states to eliminate ambiguity between test failures, execution crashes, and unmet prerequisites:

```
+----------------------------------------------------------------------------------------------------+
|                               SEVEN-STATE TEST OUTCOME TAXONOMY                                    |
+---------------+------------------------------------------------------------+-----------------------+
| Outcome State | Semantic Definition & Trigger Condition                    | CI Status Interpretation|
+---------------+------------------------------------------------------------+-----------------------+
| Passed        | All verified assertions evaluated true.                    | Success (Green)       |
| Failed        | One or more verified assertions failed (expected != actual)| Defect Detected (Red) |
| Error         | Unhandled execution exception or harness fault.            | Test/Harness Error(Red)|
| Skipped       | Non-blocking criteria explicitly waived execution.         | Neutral (Yellow)      |
| Blocked       | Mandatory prerequisite or contract missing/unsatisfied.    | Blocked Prereq (Amber)|
| NotExecuted   | Registered scenario was not scheduled or executed.        | Pending (Gray)        |
| Inconclusive  | Execution finished but valid pass/fail could not be proven.| Review Required (Gray)|
+---------------+------------------------------------------------------------+-----------------------+
```

### 1.1 Strict Truthful Reporting Invariants
- An unexecuted test must NEVER be marked `Passed`.
- A missing contract dependency must be reported as `Blocked` or `UnresolvedContract`, NEVER silently converted to `Passed`.
- An execution crash must be reported as `Error`, NEVER conflated with an assertion `Failed`.

---

## 2. Root Cause Error Classification

When a scenario fails or throws an exception, the `DiagnosticsCollector` automatically maps it to a standard classification:

| Error Classification | Description | Typical Source Exception |
| :--- | :--- | :--- |
| **`ProductDefect`** | Engine logic produced an incorrect result. | `ScenarioAssertionException` |
| **`TestDefect`** | Scenario was malformed or improperly written. | `ScenarioValidationException`, `KeyNotFoundException` |
| **`EnvironmentIssue`** | Local host environment lacked required tools. | `FileNotFoundException`, `PlatformNotSupportedException` |
| **`UnresolvedContract`**| Prerequisite Phase 9 contract is missing. | `PrerequisiteUnmetException` |
| **`ResourceConstraint`**| Out of memory or disk space during test run. | `OutOfMemoryException`, `IOException` |
| **`SecurityBoundaryViolation`**| Attempted unauthorized process/memory access.| `UnauthorizedAccessException`, `InvalidOperationException` |
| **`TeardownFailure`** | Failure occurred during scenario cleanup. | `TeardownException` |

---

## 3. Structured Diagnostics & Sanitization

Each diagnostic event conforms to `DiagnosticEvent`:

```json
{
  "timestamp": "2026-11-20T12:00:01.0000000+00:00",
  "level": "INFO",
  "component": "SyntheticMockEndpoint",
  "message": "Received envelope msg-001 from harness-client",
  "metadata": {
    "sessionId": "ses-001",
    "messageType": "M01.STATE_SYNC"
  }
}
```

### 3.1 Sanitization Policy
1. Authentication tokens, session secrets, and symmetric encryption keys must be scrubbed (`[REDACTED]`).
2. Raw process memory buffers or pointers must never be dumped into diagnostic events.
3. Path strings must use relative or neutralized tokens to prevent revealing private user directory paths.\n