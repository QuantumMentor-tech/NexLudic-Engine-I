# Cross-Game Workflow Validation & Reassessment Policy

**Document ID:** `NLE-POL-WVR-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED ARCHITECTURAL POLICY`  
**Phase:** `Phase 6 of 12 — Mashup Designer & Integration Planning`  
**Step:** `Step 48 of 100`  
**Classification:** `Workflow Quality Gates, Invalidation Matrix & Reassessment Specification`  
**Related Documents:**
- [CROSS_GAME_INTERACTION_WORKFLOW_AND_SEQUENCING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_INTERACTION_WORKFLOW_AND_SEQUENCING_SPECIFICATION.md) (`NLE-SPC-IWS-001`)
- [WORKFLOW_DEPENDENCY_ORDERING_AND_BRANCHING_RULES.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/WORKFLOW_DEPENDENCY_ORDERING_AND_BRANCHING_RULES.md) (`NLE-RUL-WOB-001`)
- [CROSS_GAME_MAPPING_REVISION_AND_INVALIDATION_POLICY.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/mashup-designer/CROSS_GAME_MAPPING_REVISION_AND_INVALIDATION_POLICY.md) (`NLE-POL-MRI-001`)

---

## 1. The 10 Workflow Quality Gates (`GATE-WFL-01..10`)

Before an interaction workflow descriptor can transition to `VALIDATED` or be accepted for topological planning, it must satisfy ten automated Quality Gates:

```mermaid
flowchart TD
    In["Draft Interaction Workflow Ingestion"] --> G1{"Gate 1: Schema Conformance<br/>(Strict JSON NLE-SCH-IWS-001)"}
    G1 -->|"Fail"| F1["Status: STRUCTURALLY_INVALID"]
    
    G1 -->|"Pass"| G2{"Gate 2: Blueprint & Component Alignment<br/>(Responsible Components in Blueprint)"}
    G2 -->|"Fail"| F2["Status: INCOMPLETE (Unassigned Component)"]
    
    G2 -->|"Pass"| G3{"Gate 3: Requirement Traceability Gate<br/>(All Mandatory Req Steps Covered)"}
    G3 -->|"Fail"| F3["Status: INCOMPLETE_REQUIREMENT_DEFICIT"]
    
    G3 -->|"Pass"| G4{"Gate 4: Acyclic Control Flow (Kahn DAG)<br/>(Forward Edges Have Zero Cycles)"}
    G4 -->|"Fail"| F4["Status: CIRCULAR_DEPENDENCY"]
    
    G4 -->|"Pass"| G5{"Gate 5: Contract Reference Resolution<br/>(All Steps Bind to Approved CTR-*)"}
    G5 -->|"Fail"| F5["Status: CONTRACT_UNRESOLVED"]
    
    G5 -->|"Pass"| G6{"Gate 6: Data Mapping Validation Gate<br/>(Referenced Mappings VALIDATED)"}
    G6 -->|"Fail"| F6["Status: MAPPING_DEPENDENCY_DEFECT"]
    
    G6 -->|"Pass"| G7{"Gate 7: Precondition & Postcondition Logic<br/>(Zero Unverifiable Assumed Prerequisites)"}
    G7 -->|"Fail"| F7["Status: NEEDS_CLARIFICATION"]
    
    G7 -->|"Pass"| G8{"Gate 8: Concurrency & Fork/Join Safety<br/>(Disjoint State Mutation Invariant)"}
    G8 -->|"Fail"| F8["Status: CONCURRENCY_RACE_DETECTED"]
    
    G8 -->|"Pass"| G9{"Gate 9: Security & Anti-Cheat Boundary<br/>(Medium IL, Zero Kernel Hooks BLK-SEC-01)"}
    G9 -->|"Fail"| F9["Status: BLOCKED_SECURITY_VIOLATION"]
    
    G9 -->|"Pass"| G10{"Gate 10: Fault Isolation & Compensation<br/>(Side-Effects Paired with SAGA Handlers)"}
    G10 -->|"Fail"| F10["Status: COMPENSATION_UNRESOLVED"]
    
    G10 -->|"All Passed"| Success["Status: VALIDATED<br/>(Ready for Creator Review & Planning Approval)"]

    style In fill:#f7fafc,stroke:#4a5568,stroke-width:1px
    style Success fill:#f0fff4,stroke:#22543d,stroke-width:2px
    style F1 fill:#fff5f5,stroke:#9b2c2c,stroke-width:1px
    style F4 fill:#fff5f5,stroke:#e53e3e,stroke-width:1px
    style F9 fill:#742a2a,color:#fff,stroke:#e53e3e,stroke-width:2px
```

---

## 2. The 8-Event Workflow Invalidation Cascade Matrix

When an upstream dependency is modified, the engine evaluates the change against the **Workflow Invalidation Cascade Matrix**:

| Event Code | Upstream Change Event Description | Impact on Workflow | Invalidation Action | Downstream Planning Impact |
| :--- | :--- | :--- | :--- | :--- |
| **`W-INV-01`** | Participating Game Added, Removed, or Swapped | Participant topology fundamentally altered | **`FULL_REASSESSMENT_MANDATORY`** | Clears workflow approval; halts Step 49. |
| **`W-INV-02`** | Game Executable Build Updated (Steam/Epic patch)| IPC hooks or mod offsets may shift | **`PARTIAL_REASSESSMENT_REQUIRED`** | Re-validates component readiness; pauses Step 49. |
| **`W-INV-03`** | Mandatory Requirement Added, Removed, or Edited | Scope of required workflow interactions changed | **`FULL_REASSESSMENT_MANDATORY`** | Re-runs Gate 3 (Requirement Traceability). |
| **`W-INV-04`** | Architecture Blueprint Revised (Step 46) | Component topology or responsibilities shifted | **`FULL_REASSESSMENT_MANDATORY`** | Re-evaluates Gate 2 (Blueprint Alignment). |
| **`W-INV-05`** | Referenced Cross-Game Mapping Modified or Stale | Underlying data translation contract changed | **`PARTIAL_REASSESSMENT_REQUIRED`** | Re-runs Gate 6 for affected transformation steps. |
| **`W-INV-06`** | Kernel Anti-Cheat Detected on Game Executable | Memory hooking or modding prohibited | **`WORKFLOW_REVOKED_BLOCKED`** | Immediately terminates hook-based workflows. |
| **`W-INV-07`** | Evidence Exceeds 90-Day Freshness Window | Provenance validity expired | **`MARK_STALE_ADVISORY`** | Surfaces advisory warning in UI; non-blocking. |
| **`W-INV-08`** | Non-Normative Description or Title Edit | Metadata only | **`NO_ACTION_VALID`** | Workflow remains valid and accepted. |

---

## 3. Historical Snapshot Archiving

1. **Monotonic Workflow Revision:** Every edit increments `workflow_revision` by 1.
2. **Historical Snapshot Archiving (`DEC-009`):** Superseded workflow descriptors are archived to:
   ```
   D:\NexLudic\Projects\<project-id>\workflows\history\<workflow-id>_rev<revision>.json
   ```
3. **Re-Validation Protocol:** Re-validating a `STALE` workflow re-runs the 10 Quality Gates. If all pass, the workflow state is restored to `VALIDATED`.

---
*End of Policy — Document `NLE-POL-WVR-001`.*
