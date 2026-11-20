# DERIVED SEMANTIC MAPPING AND DEPENDENCY SPECIFICATION

**Document Identifier:** `NLE-SPC-DMD-001`  
**Lifecycle Stage:** Authoritative Specification  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Deterministic Execution  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-STD-001` (Step 74), `NLE-POL-TRU-001` (Step 74)  
**Downstream Dependents:** `NLE-POL-LAF-001` (Step 74), `NLE-CON-MVF-001` (Step 74)

---

## 1. Purpose and Scope

This specification defines the syntax, evaluation engine, execution model, and dependency resolution algorithms for **Derived Semantic Mappings**.

A Derived Mapping computes a target property from multiple source properties using deterministic mathematical and logical operators (e.g., computing linear speed from a 3D velocity vector, computing vehicle tire slip from wheel speed and vehicle chassis velocity, or calculating damage absorption from armor rating and attack damage).

---

## 2. Inviolable Execution Invariants

1. **Zero Dynamic Code Execution:** Derived mappings **shall never** use Python `eval()`, JavaScript `eval()`, dynamic scripting engines, or unverified bytecode. All derivations run through a sandboxed, statically-typed Abstract Syntax Tree (AST) evaluator.
2. **Acyclic Dependency Graph:** All derivation dependencies between properties must form a Directed Acyclic Graph (DAG). Cycles are detected at registration time via Kahn's Algorithm and strictly prohibited.
3. **Constant Time / Bounded Steps:** Derivation expressions must execute in $O(1)$ time complexity with a maximum AST depth of 8 and maximum operation count of 32 instructions per derived field.
4. **Deterministic Arithmetic:** Floating-point operations use IEEE 754 strict rounding (round-to-nearest-even) to guarantee bitwise reproducibility across architectures.

---

## 3. Derivation Expression Specification & Operators

### 3.1 Supported Operator Set
- **Arithmetic:** `+`, `-`, `*`, `/`, `POW`, `SQRT`, `ABS`, `MIN`, `MAX`, `CLAMP`
- **Vector Operations:** `VEC3_LENGTH`, `VEC3_NORMALIZE`, `VEC3_DOT`, `VEC3_CROSS`, `VEC3_DISTANCE`
- **Quaternion Operations:** `QUAT_ROTATE_VECTOR`, `QUAT_INVERSE`, `QUAT_SLERP`
- **Logical / Conditional:** `IF_THEN_ELSE`, `AND`, `OR`, `NOT`, `EQUALS`, `LESS_THAN`, `GREATER_THAN`

### 3.2 Formal JSON Expression Schema
```json
{
  "target_property": "linear_speed_mps",
  "derivation_rule": {
    "operation": "VEC3_LENGTH",
    "inputs": [
      { "property_ref": "source.velocity_vector" }
    ],
    "post_clamp": {
      "min": 0.0,
      "max": 1000.0
    }
  }
}
```

---

## 4. Topological Dependency Resolution and Cycle Detection

Before a Semantic Mapping Record containing derivations can be transitioned to `APPROVED` state, its dependency graph $G = (V, E)$ is analyzed:

```
Algorithm 1: Kahn's Topological Sort & Cycle Detection
Input: Vertices V (properties), Edges E (property dependencies)
Output: Sorted Evaluation Order OR Error (Cycle Detected)

1. Compute in-degree for each vertex v in V.
2. Initialize Queue Q with all vertices having in-degree = 0 (raw source properties).
3. Initialize empty list L (evaluation order).
4. While Q is not empty:
     a. Remove u from Q.
     b. Append u to L.
     c. For each vertex v such that (u, v) is in E:
          i. Decrement in-degree(v).
          ii. If in-degree(v) == 0:
                Add v to Q.
5. If length(L) != length(V):
     Emit ERR_SEM_DERIVATION_CYCLE with list of cyclic dependencies.
     Fail validation immediately.
6. Return L as authoritative execution schedule.
```

---

## 5. Execution Pipeline & Error Handling

```
Raw Source Payload
       |
       v
Phase 1: Direct & Normalized Property Extraction (In-degree = 0)
       |
       v
Phase 2: Topological Stage Execution (Level-by-Level DAG evaluation)
       |
       +---> For each derived node:
       |       - Check input readiness
       |       - Evaluate AST expression
       |       - Validate output against Target Property Range
       |       - On division-by-zero or math fault: Fall back to safe rule or emit ERR_SEM_DERIVATION_FAULT
       |
       v
Phase 3: Assembly into Final Target Payload
```
