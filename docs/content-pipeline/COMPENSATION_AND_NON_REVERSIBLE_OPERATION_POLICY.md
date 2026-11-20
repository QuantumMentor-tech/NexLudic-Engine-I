# Compensation and Non-Reversible Operation Policy

**Document Identifier:** `NLE-POL-CNR-001`  
**Lifecycle Stage:** Content Pipeline Stage R Compensation Governance  
**Parent Document:** [HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_MODIFICATION_RECOVERY_ROLLBACK_RESTORATION_AND_RECONCILIATION_SPECIFICATION.md) (`NLE-SPC-MRR-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Non-Reversible Operations & Compensation Semantics  

---

## 1. Objective & Semantic Boundary

The **Compensation and Non-Reversible Operation Policy** defines the governance rules, schemas, and safety boundaries for operations that cannot be cleanly undone byte-for-byte.

### Strict Semantic Distinction
$$\text{Compensating Action} \neq \text{Exact Restoration}$$
A compensating action modifies the target to reach a safe, functionally acceptable alternative state. It does NOT recreate the exact historical bitstream. The engine must never report compensation as an exact restoration.

---

## 2. Non-Reversible Operation Taxonomy

Stage R categorizes non-reversible operations under three classes:

1. **Merged Configuration Files:** INI, XML, or JSON files where mod settings were merged with existing user configurations. Direct file restoration would overwrite legitimate user customizations made prior to modification.
2. **Virtual Filesystem Mounts & Cache Entries:** Cached texture indices, shader caches, or mod loader registration tables that cannot be restored via simple file copy.
3. **Lossy Conversions & Semantic Mappings:** Content transformations that discarded source attributes during format normalization (Step 63).

---

## 3. Declarative Compensation Plan Schema

Compensating actions must be declared in a structured compensation plan:

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/compensation-plan.json",
  "compensation_id": "cmp-20261120-0042",
  "target_resource": "Data/Config/settings.ini",
  "compensation_mechanism": "CONFIG_KEY_REMOVAL",
  "actions": [
    {
      "section": "NexLudic_NordicArmory",
      "action": "REMOVE_SECTION",
      "expected_residual_state": "ORIGINAL_USER_KEYS_PRESERVED"
    }
  ],
  "fidelity_rating": "FUNCTIONAL_ALTERNATIVE_STATE",
  "authorizer": "user:operator_01"
}
```

---

## 4. Policy Constraints & Prohibitions

1. **No External Side Effects:** Compensation must never attempt cross-system synchronization, database modifications, or online account mutations.
2. **Explicit User Disclosure:** The user must be explicitly informed when a recovery plan utilizes compensation rather than exact rollback, highlighting any residual configuration differences.

---

## 5. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_CNR_COMPENSATION_UNSUPPORTED` | FATAL | Resource requires unsupported compensation logic | Escalate to manual intervention. |
| `ERR_CNR_MISLABELED_RESTORATION` | FATAL | System attempted to report compensation as exact restoration | Reject report. Enforce accurate labeling. |
