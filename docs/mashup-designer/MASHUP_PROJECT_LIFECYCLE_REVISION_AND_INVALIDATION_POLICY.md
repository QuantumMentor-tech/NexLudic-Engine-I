# MASHUP PROJECT LIFECYCLE, REVISION, AND INVALIDATION POLICY
**Document ID:** `NLE-POL-MPL-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Principal Software Architect, Distributed-State Designer, Systems Integration Engineer, Senior QA Lead  
**Classification:** Operational Policy & State Model  

---

## 1. PURPOSE AND POLICY SCOPE

### 1.1 Purpose
This policy formally defines the lifecycle state machine, state transition rules, revision snapshotting architecture, and change invalidation cascades governing **Mashup Projects** (`MashupProject`) within NexLudic Engine. 

A project evolves over time: creators experiment with game pairs, add feature requirements, adjust version constraints, and refine integration strategies. This policy ensures that:
1. The project's lifecycle remains clearly distinct from adapter runtime lifecycles and runtime execution states.
2. Every save operation creates an immutable, verifiable revision history on Volume D (`DEC-009`).
3. Modifying project inputs deterministically marks downstream assessments, capability matches, and integration plans as stale or invalid.

---

## 2. THE EIGHT PROJECT LIFECYCLE STATES

The Mashup Project domain defines exactly eight discrete lifecycle states:

```
+--------------------------------------------------------------------------------------------------------+
|                                    PROJECT LIFECYCLE STATE SPACE                                       |
|                                                                                                        |
|  [ STATE 1: PROJECT_DRAFT ]                  - Transient creation session; partially populated fields. |
|  [ STATE 2: PROJECT_INCOMPLETE ]              - Saved project missing mandatory fields (<2 games).     |
|  [ STATE 3: PROJECT_READY_FOR_ASSESSMENT ]    - Complete baseline; ready for compatibility evaluation. |
|  [ STATE 4: PROJECT_ASSESSMENT_PENDING ]      - Automated compatibility or matching scans running.     |
|  [ STATE 5: PROJECT_PLANNING_IN_PROGRESS ]    - Assessments complete; integration graph being authored.|
|  [ STATE 6: PROJECT_PLAN_AVAILABLE ]          - Integration plan validated; ready for code generation. |
|  [ STATE 7: PROJECT_ARCHIVED ]                - Project placed in read-only cold storage.              |
|  [ STATE 8: PROJECT_DELETED ]                 - Project marked for deletion; workspace cleanup pending.|
+--------------------------------------------------------------------------------------------------------+
```

---

## 3. FORMAL STATE TRANSITION MATRIX

| Current State | Trigger / Event | Actor | Preconditions & Validation Rules | Target State | Invalidation Consequence |
| :--- | :--- | :---: | :--- | :---: | :--- |
| `PROJECT_DRAFT` | `INIT_PROJECT` | User | Unique display name provided; workspace allocated. | `PROJECT_DRAFT` | None (Initial state) |
| `PROJECT_DRAFT` | `SAVE_PARTIAL` | User | Project has valid `project_id`; missing participants. | `PROJECT_INCOMPLETE` | None |
| `PROJECT_DRAFT` | `SAVE_COMPLETE` | User | $\ge 2$ participants, valid objective, strategy set. | `PROJECT_READY_FOR_ASSESSMENT` | Triggers Phase 4/5 evaluation |
| `PROJECT_INCOMPLETE` | `ADD_REQUIRED_FIELDS`| User | Validated participants $\ge 2$; valid objective. | `PROJECT_READY_FOR_ASSESSMENT` | Unlocks assessment actions |
| `PROJECT_READY_FOR_ASSESSMENT` | `RUN_ASSESSMENT` | System/User | Phase 4/5 discovery query engines invoked. | `PROJECT_ASSESSMENT_PENDING` | Locks project from editing |
| `PROJECT_ASSESSMENT_PENDING` | `ASSESSMENT_COMPLETED` | System | Compatibility report emitted; matching complete. | `PROJECT_PLANNING_IN_PROGRESS` | Unlocks Integration Designer |
| `PROJECT_ASSESSMENT_PENDING` | `ASSESSMENT_FAILED` | System | Engine encountered fatal unhandled scan error. | `PROJECT_READY_FOR_ASSESSMENT` | Logs diagnostic error |
| `PROJECT_PLANNING_IN_PROGRESS` | `APPROVE_PLAN` | User | Integration plan verified; all mandatory reqs mapped. | `PROJECT_PLAN_AVAILABLE` | Unlocks Phase 7 Code Agent |
| `ANY_ACTIVE_STATE` | `MODIFY_PARTICIPANTS` | User | Game ID or Edition changed by creator. | `PROJECT_READY_FOR_ASSESSMENT` | **INVALIDATES ALL DERIVED PLANS** |
| `ANY_ACTIVE_STATE` | `ARCHIVE_PROJECT` | User | User selects archive option. | `PROJECT_ARCHIVED` | Freezes editing & background scans |
| `PROJECT_ARCHIVED` | `RESTORE_PROJECT` | User | User selects unarchive option. | Previous State | Reassesses evidence freshness |
| `ANY_STATE` | `DELETE_PROJECT` | User | User explicitly confirms project deletion. | `PROJECT_DELETED` | Marks Volume D workspace for purge |

---

## 4. DECOUPLING PROJECT, ADAPTER, AND RUNTIME LIFECYCLES

A critical architectural failure mode is conflating the project state with the adapter or game execution state. NexLudic Engine enforces strict domain independence:

| State Space | Controlling Entity | Lifecycle States | Governing Specification |
| :--- | :--- | :--- | :--- |
| **Project Lifecycle** | `MashupProject` | `DRAFT`, `INCOMPLETE`, `READY`, `PLANNING`, `ARCHIVED` | `NLE-POL-MPL-001` (Step 43) |
| **Adapter Registry Lifecycle** | Registry Engine | `REGISTERED`, `ACTIVE`, `QUARANTINED`, `DEPRECATED` | `NLE-POL-REC-001` (Step 37) |
| **Adapter Process Lifecycle** | Core Supervisor | `ADMITTED`, `INITIALIZING`, `READY`, `RUNNING`, `STOPPED` | `NLE-MOD-LSM-001` (Step 40) |
| **Operational Health** | Health Watchdog | `NOT_MONITORED`, `HEALTHY`, `DEGRADED`, `UNHEALTHY` | `NLE-MOD-LSM-001` (Step 40) |
| **Runtime Execution State** | Session Manager | `SESSION_IDLE`, `SESSION_RUNNING`, `SESSION_PAUSED` | Phase 7 / Phase 10 |

> [!IMPORTANT]
> A project reaching `PROJECT_PLAN_AVAILABLE` certifies that an integration plan has been designed. It does NOT assert that the adapters are running, that games are launched, or that execution is authorized.

---

## 5. REVISION SNAPSHOTTING AND VOLUME D AUDIT REPOSITORY

In accordance with `DEC-009`, all project modifications are tracked via immutable revision snapshots:
1. **Monotonic Sequence:** Every save operation increments `revision_number` ($1, 2, 3, \dots$).
2. **Snapshot Persistence:** The full project state is written to:  
   `D:\NexLudic\Projects\<projectId>\revisions\rev_<number>.json`
3. **Cryptographic Sealing:** Each snapshot computes a SHA-256 digest (`snapshot_sha256`).
4. **Audit Hash Chain:** A persistent append-only log `revisions.audit.jsonl` tracks author, timestamp, change summary, and rolling hash.

---

## 6. CHANGE INVALIDATION MATRIX

When a creator modifies an existing project, downstream artifacts are invalidated according to the following deterministic rules:

| Modified Field / Attribute | Immediate Project Impact | Compatibility Report Status (Step 32) | Capability Match Status (Step 38) | Integration Plan Status (Phase 6) |
| :--- | :--- | :---: | :---: | :---: |
| **`canonical_game_id`** | Reset to `READY_FOR_ASSESSMENT` | **INVALIDATED (Purged)** | **INVALIDATED (Purged)** | **INVALIDATED (Purged)** |
| **`edition_id`** | Reset to `READY_FOR_ASSESSMENT` | **INVALIDATED (Purged)** | **INVALIDATED (Purged)** | **INVALIDATED (Purged)** |
| **`version_constraint`** | Retains current state | **STALE (Re-evaluation needed)** | **STALE (Re-evaluation needed)** | **STALE** |
| **`role` Assignment** | Retains current state | Unaffected | Unaffected | **INVALIDATED (Re-wiring needed)** |
| **`StrategySelection`** | Retains current state | Unaffected | **STALE** | **INVALIDATED (Re-wiring needed)** |
| **Add Optional Requirement** | Retains current state | Unaffected | Unaffected | **DIRTY (Appended)** |
| **Add Mandatory Requirement**| Reset to `READY_FOR_ASSESSMENT` | Unaffected | **STALE (Requires re-match)** | **STALE (Unfulfilled)** |
| **Remove Requirement** | Retains current state | Unaffected | Unaffected | **DIRTY (Pruned)** |
| **Project Renaming** | None (Metadata update only) | Unaffected | Unaffected | Unaffected |

---
*End of Policy — Document `NLE-POL-MPL-001`.*
