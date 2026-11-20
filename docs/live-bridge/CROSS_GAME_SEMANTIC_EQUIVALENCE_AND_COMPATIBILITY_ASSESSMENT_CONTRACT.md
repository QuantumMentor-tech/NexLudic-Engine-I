# CROSS-GAME SEMANTIC EQUIVALENCE AND COMPATIBILITY ASSESSMENT CONTRACT

**Document Identifier:** `NLE-CON-SEC-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Safety Assessment  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-STD-001` (Step 74), `NLE-POL-CAP-001` (Step 70)  
**Downstream Dependents:** `NLE-POL-TRU-001` (Step 74), `NLE-POL-LAF-001` (Step 74)

---

## 1. Purpose and Scope

This contract specifies the multi-dimensional semantic equivalence criteria, compatibility scoring algorithms, and automated assessment protocols used to determine whether data exchanged between two distinct game integrations can be safely and meaningfully reconciled.

Compatibility is not a binary flag. Two games may be fully compatible regarding 3D transforms, partially compatible regarding character inventory, and completely incompatible regarding physics mass simulation. This contract provides a formal mathematical framework for evaluating compatibility per semantic category.

---

## 2. Multi-Dimensional Equivalence Dimensions

To establish semantic equivalence, two domain concepts are evaluated across six orthogonal dimensions:

```
+---------------------------------------------------------------------------------------+
| 1. STRUCTURAL DIMENSION: Data primitive, bit-width, signedness, array dimensionality  |
+---------------------------------------------------------------------------------------+
| 2. PHYSICAL / SPATIAL DIMENSION: Coordinate handedness, axis alignment, unit scale    |
+---------------------------------------------------------------------------------------+
| 3. DYNAMIC / TEMPORAL DIMENSION: Update rate, time representation, derivative order   |
+---------------------------------------------------------------------------------------+
| 4. RANGE & BOUNDARY DIMENSION: Clamping limits, singularity points, valid domain      |
+---------------------------------------------------------------------------------------+
| 5. BEHAVIORAL / STATE DIMENSION: State machine transitions, event trigger causality   |
+---------------------------------------------------------------------------------------+
| 6. GOVERNANCE & INTEGRITY DIMENSION: Authority model, immutability, security tier     |
+---------------------------------------------------------------------------------------+
```

---

## 3. Compatibility Tier Classification

| Tier | Designation | Equivalence Score ($S_{eq}$) | Information Fidelity | Operational Permission |
| :---: | :--- | :---: | :--- | :--- |
| **T0** | **Full Semantic Equivalence** | $S_{eq} = 1.0$ | 100% Lossless, Isomorphic | Unrestricted bidirectional replication permitted. |
| **T1** | **Normalized Equivalence** | $0.90 \le S_{eq} < 1.0$ | Deterministic conversion, $\le 10^{-6}$ float precision error | Bidirectional normalized replication permitted. |
| **T2** | **Bounded Approximate** | $0.70 \le S_{eq} < 0.90$ | Range clamping, scale quantization, or projection | Permitted with explicit session agreement and audit logging. |
| **T3** | **Lossy / Partial Projection** | $0.50 \le S_{eq} < 0.70$ | Unidirectional subset mapping; non-invertible | Permitted only for display/telemetry; forbidden for physics authority. |
| **T4** | **Incompatible / Rejected** | $S_{eq} < 0.50$ | Semantic mismatch, incompatible physics, missing concepts | **Strictly Forbidden.** Session rejects mapping of this concept. |

---

## 4. Quantitative Compatibility Scoring Algorithm

The Semantic Equivalence Score ($S_{eq}$) is computed as the weighted sum of dimensional scores:

$$S_{eq} = \sum_{i=1}^{6} w_i \cdot s_i$$

Where weights $w_i$ satisfy $\sum w_i = 1.0$, with default baseline weights:
- $w_{struct} = 0.15$ (Structural compatibility)
- $w_{phys} = 0.25$ (Physical & spatial transformation correctness)
- $w_{temp} = 0.15$ (Temporal alignment & frequency stability)
- $w_{range} = 0.20$ (Range and boundary overlap)
- $w_{behav} = 0.15$ (State semantics and side-effect parity)
- $w_{gov} = 0.10$ (Authority and security compliance)

### Mandatory Fail-Closed Conditions:
Regardless of the aggregate score $S_{eq}$, if any single critical dimensional score drops to zero ($s_i = 0$), the entire assessment is automatically forced to **T4 (Incompatible)**. For example:
- A spatial concept where handedness cannot be inverted without geometry distortion.
- An event concept where Game A requires atomic financial consistency but Game B is an unverified peer client.

---

## 5. Assessment Contract Output Schema

The output of an automated compatibility assessment is an immutable `SemanticCompatibilityAssessmentReport`:

```json
{
  "report_id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "evaluated_at": "2026-11-20T18:00:00Z",
  "source_concept_uri": "nexludic://concepts/ue5/character_stats",
  "target_concept_uri": "nexludic://concepts/unity/player_vitals",
  "aggregate_score": 0.885,
  "assigned_tier": "T2_BOUNDED_APPROXIMATE",
  "dimensional_scores": {
    "structural": 1.0,
    "physical_spatial": 1.0,
    "temporal": 0.9,
    "range_boundary": 0.75,
    "behavioral": 0.8,
    "governance": 1.0
  },
  "identified_discrepancies": [
    {
      "dimension": "range_boundary",
      "property": "mana_points",
      "issue": "Source max mana is 1000.0; Target max mana is 255 (uint8). Requires quantization.",
      "lossiness_flag": true
    }
  ],
  "verdict": "APPROVED_WITH_CONDITIONS",
  "required_mitigations": [
    "ATTACH_OPAQUE_VAULT_TOKEN_FOR_UNQUANTIZED_MANA"
  ]
}
```
