# CROSS-GAME MERGE SEMANTICS AND INVARIANT PROTECTION POLICY

**Document Identifier:** `NLE-POL-MSI-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Semantic Integrity  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-SPC-CSM-001` (Step 74), `NLE-CON-MDP-001` (Step 74)  
**Downstream Dependents:** `NLE-SPC-CSW-001` (Step 76), `NLE-CON-RAV-001` (Step 76)

---

## 1. Purpose and Scope

This policy governs the semantic rules, domain invariant checks, and lossiness constraints that must be satisfied whenever two conflicting state values are merged or reconciled.

A candidate resolution that is structurally valid (e.g. valid JSON) can still be semantically disastrous (e.g. creating two players with the same unique sword, or spawning an entity inside solid geometry). This policy ensures that semantic integrity and domain invariants are preserved across all reconciliation operations.

---

## 2. Inviolable Merge Invariants

1. **Structural Match $\neq$ Semantic Compatibility:** Merging two values requires verification against Step 74 Domain Concept Descriptors. Merging differing units or coordinate frames without explicit normalization is strictly prohibited.
2. **Conservation Laws:** In economy, inventory, and resource domains, a merge **shall never** fabricate or destroy resources:
   $$\sum \text{Balance}_{target} == \sum \text{Balance}_{source} \quad (\text{No phantom currency or duped items})$$
3. **Mutual Exclusion Invariants:** Physical spaces cannot be occupied by two distinct colliders simultaneously. A candidate merge placing two actors at identical coordinates triggers an invariant failure.
4. **State Machine Transition Invariants:** A merge cannot transition an entity to an illegal state (e.g. transitioning directly from `DEAD` to `ATTACKING` without `RESPAWN`).

---

## 3. Handling Lossy Transformations & Quantization

When reconciliation requires mapping data between asymmetric fidelity representations (e.g. high-precision float to 8-bit quantized int):
1. **Explicit Lossiness Declaration:** The resolution record must set `is_lossy: true` and record the precision delta ($\Delta v$).
2. **Opaque Metadata Vaulting (`vault_token`):** The unquantized high-precision source value must be preserved in an encrypted `vault_token` (Step 74) to enable lossless round-trip replication.
3. **Prohibited Approximations:** Converting an unknown status or missing item into a convenient default without explicit policy approval is strictly prohibited.

---

## 4. Treatment of Missing, Null, Unknown, and Defaulted Fields

```
+-----------------------------------+-------------------------------------------------------------------+
| State Condition                   | Mandatory Policy Handling                                         |
+-----------------------------------+-------------------------------------------------------------------+
| 1. Missing Field                  | Preserved as missing; evaluated against Default Value Hierarchy.  |
| 2. Explicit Null                  | Treated as intentional absence; NOT replaced by 0 or default.     |
| 3. Unknown Value                  | Quarantined in `vault_token`; never guessed or hallucinated.       |
| 4. Defaulted Field                | Explicitly attributed with PROVENANCE_DEFAULTED in audit dossier. |
+-----------------------------------+-------------------------------------------------------------------+
```
