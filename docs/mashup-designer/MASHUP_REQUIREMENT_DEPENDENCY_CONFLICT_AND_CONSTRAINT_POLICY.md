# MASHUP REQUIREMENT DEPENDENCY, CONFLICT, AND CONSTRAINT POLICY
**Document ID:** `NLE-POL-RDC-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Systems Analyst, Principal Software Architect, Rules-System Designer, Senior QA Engineer  
**Classification:** Core Governance Policy & Algorithms  

---

## 1. PURPOSE AND POLICY SCOPE

### 1.1 Purpose
This policy establishes the formal mathematical rules, graph algorithms, conflict detection mechanisms, and constraint taxonomies governing **Requirement Dependencies, Conflicts, Constraints, and Assumptions** within NexLudic Engine. 

Cross-game mashups involve interlocking mechanical systems. If Requirement A depends on Requirement B, but B contradicts Requirement C, an integration planner or AI coding agent cannot construct a stable integration plan without deterministic dependency resolution. This document specifies:
1. The formal catalog of 8 dependency edge types.
2. The Kahn topological sort algorithm detecting circular dependency deadlocks.
3. The conflict classification and non-destructive resolution protocols.
4. The management of technical constraints, explicit assumptions, and open questions.

---

## 2. THE EIGHT REQUIREMENT DEPENDENCY TYPES

Requirements relate to one another via typed, directed edges:

```
+--------------------------------------------------------------------------------------------------------+
|                                    REQUIREMENT DEPENDENCY RELATIONSHIPS                                |
|                                                                                                        |
|  [ 1. REQUIRES ]        ---> Hard prerequisite: A cannot function without B.                           |
|  [ 2. DEPENDS_ON ]      ---> Architectural dependency: A consumes data produced by B.                  |
|  [ 3. PRECEDES ]        ---> Temporal ordering: A must initialize/execute before B.                   |
|  [ 4. CONDITIONAL_ON ]  ---> Dynamic prerequisite: A is active only when condition C is met.           |
|  [ 5. ALTERNATIVE_TO ]  ---> Design alternative: Choose A OR B to fulfill feature.                     |
|  [ 6. CONFLICTS_WITH ]  ---> Mutual exclusion: A and B cannot be active simultaneously.               |
|  [ 7. REFINES ]         ---> Specialization: B provides fine-grained detail for high-level A.          |
|  [ 8. SUPERSEDES ]      ---> Version replacement: New requirement B replaces obsolete A.              |
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 Dependency Type Matrix

| Edge Type | Direction | Invalidation Cascade | Cyclic Permitted? | Impact on Downstream Planning |
| :--- | :---: | :---: | :---: | :--- |
| **`REQUIRES`** | $A \rightarrow B$ | If B fails, A is blocked. | **NO (Hard DAG)** | Blocks plan if B is unfulfilled. |
| **`DEPENDS_ON`** | $A \rightarrow B$ | If B changes, A is marked dirty. | **NO (Hard DAG)** | Re-evaluates A if B modifies data shape. |
| **`PRECEDES`** | $A \rightarrow B$ | Execution order sequence. | **NO (Hard DAG)** | Dictates initialization order in Phase 6/7. |
| **`CONDITIONAL_ON`** | $A \rightarrow C$ | Evaluated dynamically. | Conditional | A remains idle until condition evaluates true. |
| **`ALTERNATIVE_TO`** | $A \leftrightarrow B$ | Selecting A deactivates B. | Symmetric | Planner picks highest-priority viable alternative. |
| **`CONFLICTS_WITH`** | $A \leftrightarrow B$ | Cannot both be `ACCEPTED`. | Symmetric | Blocks plan approval until user resolves conflict. |
| **`REFINES`** | $B \rightarrow A$ | Inherits parent intent. | No | Refines acceptance criteria for parent A. |
| **`SUPERSEDES`** | $B \rightarrow A$ | A is marked `SUPERSEDED`. | No | A is archived; B becomes active. |

---

## 3. DEPENDENCY GRAPH VALIDATION & KAHN DAG CYCLE REJECTION

To prevent infinite recursion during planning and execution:
1. **DAG Invariant:** The subgraph formed by all directed edges of types `REQUIRES`, `DEPENDS_ON`, and `PRECEDES` MUST form a Directed Acyclic Graph (DAG).
2. **Kahn Topological Sort Algorithm:**
   - The engine computes in-degrees for all requirement nodes.
   - Nodes with zero in-degree are enqueued and removed iteratively.
   - If the count of processed nodes does NOT equal the total node count, a circular dependency exists.
3. **Rejection Action:** The save operation is rejected with `ERR_REQ_CIRCULAR_DEPENDENCY`. The diagnostic payload returns the exact cyclic path (e.g., $Req_A \rightarrow Req_B \rightarrow Req_C \rightarrow Req_A$) with remediation instructions.

---

## 4. CONFLICT CLASSIFICATION AND RESOLUTION PROTOCOLS

When two requirements express incompatible demands, the engine categorizes the conflict:

```
+--------------------------------------------------------------------------------------------------------+
|                                    REQUIREMENT CONFLICT TAXONOMY                                       |
|                                                                                                        |
|  [ TYPE 1: LOGICAL CONTRADICTION ]   ---> Incompatible game rules (e.g. Permadeath vs Infinite Lives)  |
|  [ TYPE 2: RESOURCE LOCK CONFLICT ]  ---> Both reqs demand exclusive hook on same game subsystem       |
|  [ TYPE 3: PARTICIPANT ROLE CLASH ]  ---> Reqs assign mutually exclusive roles to same participant     |
|  [ TYPE 4: PERFORMANCE BUDGET CLASH] ---> Combined requirements exceed 512MB RAM or 16.6ms latency     |
+--------------------------------------------------------------------------------------------------------+
```

### 4.1 Non-Destructive Resolution Protocol
- **No Silent Overrides:** The engine strictly forbids automatically deleting or lowering the priority of a conflicting requirement.
- **`CONFLICTED` State Tagging:** Both conflicting requirements are tagged with quality status `CONFLICTED`.
- **User Resolution Tray:** The UI presents a dedicated conflict resolution panel offering three explicit options:
  1. *Designate Alternative:* Link them via `ALTERNATIVE_TO`, allowing the planner to select one based on adapter capability availability.
  2. *Set Precedence:* Demote one requirement to `OPTIONAL` with an explicit fallback condition.
  3. *Revoke Requirement:* Reject one requirement and record the creator's decision rationale.

---

## 5. CONSTRAINTS, ASSUMPTIONS, AND OPEN QUESTIONS MANAGEMENT

```
[ CONSTRAINTS ]   ---> Hard boundaries enforced by OS, anti-cheat, game engines (Non-negotiable).
       |
[ ASSUMPTIONS ]   ---> Presumed technical capabilities captured in AssumptionRecord (Needs verification).
       |
[ OPEN QUESTIONS ]---> Unresolved creator choices captured in OpenQuestionRecord (Needs user answer).
```

### 5.1 Assumption Lifecycle
Every `AssumptionRecord` tracks a formal verification state:
- `UNVERIFIED_ASSUMPTION`: Initial state when assumption is logged.
- `CONFIRMED_TRUE`: Verified by Phase 4 compatibility scan or Phase 5 adapter declaration.
- `DISPROVEN_FALSE`: Proved infeasible. Associated requirements must be adjusted.

### 5.2 Open Question Blocking Rules
- Questions tagged `is_blocking = true` prevent the parent requirement from transitioning to `READY_FOR_ANALYSIS`.
- Questions tagged `is_blocking = false` represent advisory polish choices and do not block initial integration planning.

---
*End of Policy — Document `NLE-POL-RDC-001`.*
