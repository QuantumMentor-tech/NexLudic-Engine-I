# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Persistence Validation & Failure Test Plan

**Document ID:** `NLE-PLN-DAT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Quality & Verification Plan  
**Traceability:** `ADR-0007` (Storage Baseline), `ADR-0010` (Testing Strategy), `NLE-ARC-DAT-001` (Persistence Architecture)

---

## 1. Executive Summary & Quality Scope

The **Persistence Validation and Failure Test Plan** establishes the automated testing suites, failure injection scenarios, regression baselines, and quality gates required to verify the persistence layer of NexLudic Engine before application implementation.

It also formalizes strict **AI Coding Agent Rules for Persistence Changes** to prevent future automated agents from performing unauthorized destructive file operations, breaking schema contracts, or introducing unapproved database engines.

---

## 2. The 10-Level Persistence Validation Suite

```
+----------------------------------------------------------------------------------------------------+
|                                THE 10-LEVEL PERSISTENCE TEST PLAN                                  |
|                                                                                                    |
|  Level 1: Data-Model & Schema Invariant Tests (JSON Schema Draft 2020-12, Domain Rules)           |
|  Level 2: Storage Contract & Repository Mock Tests (IProjectRepository, IEvidenceRepository)       |
|  Level 3: Transaction Atomicity & Concurrency Tests (File.Replace, OCC Revision Tokens)           |
|  Level 4: Schema Evolution & Migration Tests (V1.0 -> V1.1 Pipeline, Unknown Version Rejection)    |
|  Level 5: Backup & Restore Verification Tests (Round-trip .nxpkg, Identity Collision Handling)     |
|  Level 6: Artifact Lifecycle & Staging Jail Tests (Path Containment, SHA-256 Checksums)           |
|  Level 7: Security, Privacy & Secret Redaction Tests (Path Traversal, DPAPI Storage, Scrubbing)    |
|  Level 8: Failure-Injection & Hardware Tests (Disk Full, Power Cut Simulation, Locked Handles)     |
|  Level 9: Historical Regression Tests (Long-term Compatibility across Game Engine Updates)         |
|  Level 10: Disaster Recovery & Rollback Tests (Crash Journal Replay, In-Flight Rollback)          |
+----------------------------------------------------------------------------------------------------+
```

---

### Detailed Test Specifications:

#### Level 1: Data-Model & Schema Invariant Tests
* `TC-DAT-L1-01`: Validate `project.json` matches JSON Schema Draft 2020-12.
* `TC-DAT-L1-02`: Reject project creation with empty name or invalid UUIDv4.
* `TC-DAT-L1-03`: Ensure capability claim without linked evidence cannot transition to `VALIDATED`.

#### Level 2: Storage Contract & Repository Tests
* `TC-DAT-L2-01`: Verify `JsonProjectRepository.SaveAsync` produces formatted, readable UTF-8 JSON.
* `TC-DAT-L2-02`: Verify `JsonEvidenceRepository.AppendAsync` guarantees immutable append ordering.

#### Level 3: Transaction Atomicity & Concurrency Tests
* `TC-DAT-L3-01`: Atomic write-rename replaces destination file without intermediate empty state.
* `TC-DAT-L3-02`: Save rejected with `PERSISTENCE_CONCURRENCY_CONFLICT` when disk revision exceeds memory revision.

#### Level 4: Schema Evolution & Migration Tests
* `TC-DAT-L4-01`: Migrate project schema `v1.0.0` to `v1.1.0` with pre-migration backup creation.
* `TC-DAT-L4-02`: Opening schema `v2.0.0` in a `v1.x` Core forces read-only mode and blocks write operations.

#### Level 5: Backup & Restore Verification Tests
* `TC-DAT-L5-01`: Export project to `.nxpkg`, delete project from disk, restore from `.nxpkg`, and verify all SHA-256 hashes match.
* `TC-DAT-L5-02`: Restore existing project ID prompts for collision resolution (Overwrite, Fork, Cancel).

#### Level 6: Artifact Lifecycle & Staging Jail Tests
* `TC-DAT-L6-01`: Write attempt outside `<project_root>/staging/` rejected with `SECURITY_SANDBOX_VIOLATION`.
* `TC-DAT-L6-02`: Missing generated file detected and flagged during artifact verification.

#### Level 7: Security, Privacy & Secret Redaction Tests
* `TC-DAT-L7-01`: Path traversal attempt (`staging/../../Windows/System32`) blocked by canonicalization.
* `TC-DAT-L7-02`: Confirm API keys are never written to `project.json` or Serilog log files.
* `TC-DAT-L7-03`: Encrypt AI provider token using DPAPI; verify recovery on current user account and rejection on other accounts.

#### Level 8: Failure-Injection Tests
* `TC-DAT-L8-01`: Disk Full simulation during manifest write: ensure `.tmp` fails, original manifest remains uncorrupted.
* `TC-DAT-L8-02`: Process killed mid-write: verify `.recovery/in_flight.journal` recovers previous state on restart.

#### Level 9: Historical Regression Tests
* `TC-DAT-L9-01`: Verify projects created under older pilot milestones remain readable without data loss.

#### Level 10: Disaster Recovery & Rollback Tests
* `TC-DAT-L10-01`: Aborted mod compilation rolls back staging workspace to pre-operation snapshot within 2 seconds.

---

## 3. AI Coding Agent Rules for Persistence Changes

Future AI agents modifying the persistence subsystem MUST adhere to these strict behavioral rules:

1. **No Unapproved Storage Engines:** Agents MUST NOT add NuGet or npm dependencies for SQLite, LiteDB, MongoDB, or EF Core without an approved ADR.
2. **Never Touch User Data in Tests:** Automated test fixtures MUST run inside isolated temporary directories (`<project_root>/temp_test/`), never touching live user projects.
3. **Mandatory Schema Migrations:** Never alter existing JSON manifest property names or types without authoring an explicit migration step.
4. **Preserve All Evidence Records:** Agents MUST NOT write code that prunes, edits, or deletes evidence records (`DAT-EVD-001`).
5. **Enforce Atomic Writes:** Never use `File.WriteAllText` directly on an existing manifest; always use the atomic write-rename pattern via `File.Replace`.
6. **Path Jail Containment:** Always canonicalize paths (`Path.GetFullPath`) and assert containment within staging directories before writing files.
7. **Secrets Out of Logs:** Never print configuration objects containing API tokens to console or Serilog sinks.
8. **Truthful Save Reporting:** Never return success to the caller before the underlying storage transaction has committed to disk.
9. **Fail-Closed on Unknown Future Schemas:** Refuse to serialize or mutate projects written by newer schema versions.
10. **Stop and Consult:** If an implementation task risks data loss or schema breaking, halt execution and request human engineering approval.

