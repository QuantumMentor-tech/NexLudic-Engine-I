# CROSS-STRATEGY CONTRACTS, RESPONSIBILITY AND SHARED OPERATION SPECIFICATION

**Document Identifier:** `NLE-SPC-CSR-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 77 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Architectural Boundaries  
**Upstream Dependencies:** `NLE-SPC-HIO-001` (Step 77), `NLE-CON-HIP-001` (Step 77), `NLE-SPC-MVR-001` (Step 73)  
**Downstream Dependents:** `NLE-SPC-CSH-001` (Step 77), `NLE-POL-HDC-001` (Step 77)

---

## 1. Purpose and Scope

This specification defines the inter-component interface contracts, single responsibility rules for state changes, shared operation identity models, and failure ownership boundaries across cooperating integration strategies.

When multiple mechanisms (e.g. content mods and live bridge adapters) participate in a hybrid integration, ambiguity regarding which component owns a given operation or state mutation leads to duplicate execution or unassigned failure recovery. This document codifies strict, unambiguous boundaries.

---

## 2. Inviolable Single Responsibility for State Changes

For every synchronized domain or logical operation, exactly one component is designated as the **Authoritative Executor**:

```
+-----------------------------------+-------------------------------------------------------------------+
| Operational Responsibility        | Authoritative Component Designation Rules                         |
+-----------------------------------+-------------------------------------------------------------------+
| 1. State Observation & Ingress    | Native Source Game Adapter (Reads state; emits raw payload).      |
| 2. Normalization & Transformation | Semantic Normalization Engine (Applies Step 74 SMR into CIR).     |
| 3. Routing & Envelope Packaging   | Message Exchange Gateway (Wraps Step 73 Envelope; signs HMAC).   |
| 4. Conflict Arbitration & Policy  | Authority Arbitrator (Evaluates Step 76 Resolution Policy).       |
| 5. State Mutation Execution       | Target Game Adapter (Applies mutation to target simulation).      |
| 6. Outcome Verification           | Verification Engine (Performs Step 76 read-back / check).         |
| 7. Audit Dossier Sealing          | Audit Logger (Writes tamper-evident diagnostic records).          |
+-----------------------------------+-------------------------------------------------------------------+
```

**Prohibition:** Under no circumstance shall two strategy instances independently apply the same logical operation to a target game runtime.

---

## 3. Shared Operation Identity Across Strategy Transitions

To trace operations crossing multiple strategy boundaries without losing causality or duplicating effects, all components reuse the authoritative Step 73 Operation Descriptor:

```json
{
  "operation_id": "op:hybrid:4a7e9b21-1234-5678-9abc-def012345678",
  "correlation_id": "corr:hybrid:ue5_unity:tick_1042",
  "plan_id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "session_epoch": 3,
  "source_strategy_id": "strat:live_bridge:ue5_hero:v1.0.0",
  "target_strategy_id": "strat:live_bridge:unity_proxy:v1.0.0",
  "originating_timestamp_utc": "2026-11-20T19:45:00.123456Z",
  "hop_history": [
    { "component": "ue5_adapter", "action": "INGRESS_CAPTURED" },
    { "component": "semantic_mapper", "action": "NORMALIZED_TO_CIR" },
    { "component": "hybrid_orchestrator", "action": "HANDOFF_ROUTED" },
    { "component": "unity_adapter", "action": "APPLIED_AND_VERIFIED" }
  ]
}
```

---

## 4. Boundary Failure Ownership

When a failure occurs at an inter-component boundary:
1. **Detection Ownership:** The receiving component at the boundary detects the failure (e.g. timeout, schema mismatch, corrupt payload).
2. **Containment Ownership:** The detecting component immediately halts the operation, quarantine the payload, and prevents downstream propagation.
3. **Escalation Ownership:** The detecting component emits a structured failure event to the Hybrid Orchestrator (`EVT_HYBRID_BOUNDARY_FAULT`).
4. **Recovery Ownership:** The Hybrid Orchestrator alone determines whether retry, fallback, or domain suspension is authorized under the active plan.
