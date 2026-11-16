# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Project Data & Artifact Lifecycle Specification

**Document ID:** `NLE-LFC-DAT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Lifecycle Specification  
**Traceability:** `NLE-DOM-SPEC-001` (Core Domain Model), `NLE-ARC-DAT-001` (Persistence Architecture), `ADR-0009` (Workspace Isolation)

---

## 1. Executive Summary & Purpose

NexLudic Engine manages complex, multi-stage workflows that span months of iterative mod development, testing, and AI-assisted refinement. To prevent orphan files, accidental overwrites, stale evidence claims, and data corruption, all entities and artifacts must adhere to formal, deterministic **Lifecycle State Machines**.

This document defines the lifecycle states, transitions, guard conditions, side effects, durability guarantees, and cleanup policies for seven core system domains:
1. Game-Pair Project Lifecycle
2. Adapter & Compatibility Record Lifecycle
3. Evidence & Epistemic Grounding Lifecycle
4. Test Specification & Execution History Lifecycle
5. AI Task, Approval & Scaffolding Lifecycle
6. Generated Artifact & Staging File Lifecycle
7. Recovery, Snapshot & Rollback Lifecycle

---

## 2. Project Lifecycle State Machine (`DAT-PRJ-001`)

A game-pair project represents the overarching aggregate root within the universal platform core.

```
       [Created]
           |
           v
      +---------+     Games Validated & Strategy Chosen     +------------+
      |  DRAFT  | ----------------------------------------> | CONFIGURED |
      +---------+                                           +------------+
           |                                                       |
           | Cancelled                                             | Launch Workflows
           v                                                       v
     +-----------+                                           +------------+
     | DESTROYED | <---------------------------------------- |   ACTIVE   | <---+
     +-----------+                                           +------------+     |
           ^                                                  |          |      | Resume
           | Hard Purge (Explicit Confirm)                    | Pause    | Stop |
           |                                                  v          |      |
     +-----------+         Archive Project                 +---------+   |      |
     |  TRASHED  | <-------------------------------------- | PAUSED  | --+------+
     +-----------+                                         +---------+   |
           ^                                                             | Archive
           | User Deletes Project                                        v
           +-------------------------------------------------------+------------+
                                                                   |  ARCHIVED  |
                                                                   +------------+
```

### 2.1 State Definitions & Invariants:
* **DRAFT:** Initial project creation. Name assigned, but target games, editions, or integration strategies are incomplete or unvalidated.
* **CONFIGURED:** Target game installations confirmed, executable hashes verified, and an approved integration strategy (A, B, or C) assigned.
* **ACTIVE:** Actively participating in scaffolding, mod compilation, or live telemetry sessions. Staging workspace unlocked.
* **PAUSED:** Active execution halted. Worker processes stopped; staging workspace locked against concurrent edits; manifests durably flushed.
* **ARCHIVED:** Read-only historical state. Retains all requirements, blueprints, evidence citations, and approved artifacts. Staging caches cleaned.
* **TRASHED (Soft-Deleted):** Moved to `.trash/` folder. Retained for 30 days before purge. Can be restored with a single click.
* **DESTROYED (Purged):** Permanent disk removal of project directory. Unrecoverable.

---

## 3. Adapter & Compatibility Record Lifecycle (`DAT-ADP-001`, `DAT-CMP-001`)

```
      +------------+        Qualification Pass (Tests & Checks)      +-----------+
      | DISCOVERED | ----------------------------------------------> | QUALIFIED |
      +------------+                                                 +-----------+
            |                                                              |
            | Missing Tools / Incompatible OS                              | Registered
            v                                                              v
      +--------------+      Game Patch / Version Mismatch Break      +-----------+
      | INCOMPATIBLE | <-------------------------------------------- |  ACTIVE   |
      +--------------+                                               +-----------+
            |                                                              |
            | Permanent Deprecation                                        | Superceded
            v                                                              v
      +------------+ <-----------------------------------------------------+
      |  RETIRED   |
      +------------+
```

### 3.1 State Definitions & Invariants:
* **DISCOVERED:** Adapter binary or manifest detected in plugin directory. Unverified.
* **QUALIFIED:** Core has verified adapter manifest schema, dependency prerequisites, and sandbox compatibility tests (`TC-L7-01`).
* **ACTIVE:** Currently assigned to one or more active projects. Process supervisor authorized to launch worker instances.
* **INCOMPATIBLE:** Target game updated (hash mismatch), or required tool missing. Adapter prevented from launching.
* **RETIRED:** Adapter version superseded or permanently deprecated. **Crucial Rule:** Historical validation evidence generated by this adapter remains preserved in dependent projects.

---

## 4. Evidence & Epistemic Grounding Lifecycle (`DAT-EVD-001`)

Evidence provides verifiable auditability for compatibility claims. Contradictory evidence never erases history.

```
      +-----------+         Peer / Automated Review Passes           +----------+
      | COLLECTED | -----------------------------------------------> | VERIFIED |
      +-----------+                                                  +----------+
            |                                                              |
            | Unverifiable / Invalid Citation                              | Grounded
            v                                                              v
      +-------------+        Newer Contradictory Test Run            +----------+
      | INVALIDATED | <--------------------------------------------- |  ACTIVE  |
      +-------------+                                                +----------+
            |                                                              |
            | Retained for Audit                                           | New Evidence
            v                                                              v
      +------------------------------------------------------------> +------------+
                                                                     | SUPERSEDED |
                                                                     +------------+
```

### 4.1 State Definitions & Invariants:
* **COLLECTED:** Raw citation or test output ingested into registry.
* **VERIFIED:** Citation validated (valid URL/DOI, valid SHA-256 test run log, or decompiled code snippet).
* **ACTIVE:** Currently cited as primary epistemic justification for an active capability claim.
* **SUPERSEDED:** A newer, higher-fidelity test or newer game version observation replaces this claim. Old claim remains intact with `superseded_by_id` pointer.
* **INVALIDATED:** Proven incorrect or tainted. Tagged as invalid; capability downgraded to `Unknown` or `Unsupported`.

---

## 5. Test History Lifecycle (`DAT-TST-001`)

```
      +---------+              Execution Triggered                   +-----------+
      | PLANNED | -------------------------------------------------> | EXECUTING |
      +---------+                                                    +-----------+
                                                                           |
                                            +------------------------------+------------------------------+
                                            | All Asserts Pass             | Assertion Fails              | Crash / Timeout
                                            v                              v                              v
                                     +---------------+              +---------------+              +---------------+
                                     | PASSED_VERIFY |              | FAILED_ASSERT |              | ERROR_TIMEOUT |
                                     +---------------+              +---------------+              +---------------+
                                            |                              |                              |
                                            +------------------------------+------------------------------+
                                                                           |
                                                                           v
                                                                    +-------------+
                                                                    |  ARCHIVED   |
                                                                    +-------------+
```

### 5.1 Invariants:
1. **Append-Only Test History:** Test run records (`validation-runs.json`) are strictly append-only.
2. **Separation of Spec from Run:** A `TestSpecification` is a template; a `TestExecution` is an immutable, timestamped record of an actual execution.

---

## 6. AI Task & Scaffolding Lifecycle (`DAT-TSK-001`)

```
      +---------+           User Configures Scope & Prompts          +-------------------+
      | DRAFTED | -------------------------------------------------> | AWAITING_APPROVAL |
      +---------+                                                    +-------------------+
                                                                               |
                                            +----------------------------------+-------------------+
                                            | User Approves Plan & Diff                            | User Rejects
                                            v                                                      v
                                     +---------------+                                      +---------------+
                                     |  IN_PROGRESS  |                                      |   REJECTED    |
                                     +---------------+                                      +---------------+
                                            |
                         +------------------+------------------+
                         | Generation OK                       | Crash / Abort
                         v                                     v
                  +-------------+                       +-------------+
                  |  COMPLETED  |                       |  CANCELLED  |
                  +-------------+                       +-------------+
                         |                                     |
                         +------------------+------------------+
                                            v
                                     +-------------+
                                     |   AUDITED   |
                                     +-------------+
```

### 6.1 Invariants:
* **Mandatory Human-in-the-Loop (`REQ-F-009`, `ADR-0009`):** No AI-generated code or file-system modification can transition to `IN_PROGRESS` or write outside staging without explicit human approval.

---

## 7. Generated Artifact Lifecycle (`DAT-ART-001`)

```
      [Worker Scaffolds]
              |
              v
        +-----------+           Linter & Syntax Check Pass            +----------+
        |  STAGED   | ----------------------------------------------> |  LINTED  |
        +-----------+                                                 +----------+
              |                                                             |
              | Linter Errors                                               | Diff Review Pass
              v                                                             v
        +-----------+                                                 +----------+
        | QUARANTINE|                                                 | APPROVED |
        +-----------+                                                 +----------+
              ^                                                             |
              | Tampering Detected                                          | Integration
              |                                                             v
        +-----------+            Superseded by New Build              +----------+
        |  PRUNED   | <---------------------------------------------- |  ACTIVE  |
        +-----------+                                                 +----------+
```

### 7.1 Invariants:
* **Staging Jail Containment:** Staged artifacts live strictly in `<project_root>/staging/`. They cannot enter the game installation directory until user-approved.
* **Hash Integrity:** Every approved artifact records its SHA-256 hash in `artifacts-manifest.json`.

---

## 8. Recovery, Snapshot & Rollback Lifecycle

```
      +----------+              Pre-Operation Checkpoint Created             +--------+
      | PROPOSED | --------------------------------------------------------> | STAGED |
      +----------+                                                           +--------+
                                                                                  |
                                                                                  | Restore Invoked
                                                                                  v
      +-----------+           Verification Passes (Hashes Match)            +----------+
      | RESTORED  | <------------------------------------------------------ | APPLYING |
      +-----------+                                                         +----------+
            |                                                                     |
            | Verified Usable                                                     | Verification Fails
            v                                                                     v
      +-----------+                                                         +----------+
      | VERIFIED  |                                                         |  FAILED  |
      +-----------+                                                         +----------+
```

### 8.1 Invariants:
1. **Never Assume Restored Means Verified:** A restore operation is marked `RESTORED` when files are extracted, but MUST undergo structural and checksum validation before transitioning to `VERIFIED`.
2. **Atomic Rollback Journal:** Rollbacks use `.recovery/in_flight.journal`. If interrupted, the Core re-evaluates the journal on next boot.

