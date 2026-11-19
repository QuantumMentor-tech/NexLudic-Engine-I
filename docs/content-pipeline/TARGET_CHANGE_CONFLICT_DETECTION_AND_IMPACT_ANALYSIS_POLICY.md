# Target Change Conflict Detection and Impact Analysis Policy

**Document Identifier:** `NLE-POL-TCC-001`  
**Lifecycle Stage:** Content Pipeline Stage I Policy & Analysis  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Conflict Detection & Risk Assessment  

---

## 1. Purpose & Scope

The **Target Change Conflict Detection and Impact Analysis Policy** defines the formal mechanisms for detecting collisions, incompatibilities, and safety risks between proposed package modifications and the established target game environment.

It establishes:
1. A structured taxonomy of target conflicts.
2. Mandatory non-destructive resolution principles.
3. Impact and blast-radius analysis protocols.

---

## 2. Target Conflict Taxonomy

Conflicts detected during Stage I planning are categorized under six formal types:

| Conflict Code | Classification | Description | Blocking Severity |
| :--- | :--- | :--- | :--- |
| **`CONF_DEST_OCCUPIED`** | Destination Collision | Operation `ADD` planned, but destination is `CONFIRMED_PRESENT` | **BLOCKING** |
| **`CONF_BASELINE_MISMATCH`**| Baseline Drift | Existing file SHA-256 does not match expected prior state | **BLOCKING** |
| **`CONF_INTERNAL_COLLISION`**| Plan Collision | Multiple changes within the same plan target identical file | **BLOCKING** |
| **`CONF_HOOK_INCOMPATIBLE`** | Subsystem Conflict | Required game hook conflicts with installed mod loader | **BLOCKING** |
| **`CONF_MISSING_PREREQ`** | Dependency Missing | Plan requires base asset or patch not present in target | **BLOCKING** |
| **`CONF_CONCURRENT_MUTATION`**| State Tamper | File changed between snapshot capture and plan analysis | **BLOCKING** |

---

## 3. Conflict Resolution Principles

NexLudic Engine enforces deterministic, safety-first conflict resolution:

1. **No Silent "Last-Writer-Wins":** Overwriting existing files without explicit user review and approval is forbidden.
2. **No Arbitrary Collision Ignoring:** If two package members target the same file, the plan is blocked until an authoritative priority or exclusion rule is supplied.
3. **Preserve Contradictions:** Unresolved conflicts must remain prominent and blocking in the application plan; they must never be hidden or automatically bypassed.

---

## 4. Impact Analysis & Blast-Radius Assessment

The Planning Engine computes an **Impact Assessment Matrix** evaluating the blast radius of proposed changes:

```json
{
  "impact_summary": {
    "total_proposed_operations": 482,
    "additions_count": 478,
    "replacements_count": 4,
    "removals_count": 0,
    "no_op_count": 0,
    "overall_risk_classification": "MEDIUM_RISK",
    "blast_radius": {
      "modified_game_scripts": ["Data/Scripts/QuestScript.pex"],
      "modified_configs": ["Data/Config/settings.ini"],
      "requires_rollback_journal": true,
      "reversibility_rating": "100%_REVERSIBLE"
    }
  }
}
```

---

## 5. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_TCC_DESTINATION_OCCUPIED` | FATAL | Cannot ADD to occupied destination | Reclassify as REPLACE or abort. |
| `ERR_TCC_BASELINE_MISMATCH` | FATAL | Prior file content mismatch | Invalidate plan. Re-inspect target. |
| `ERR_TCC_INTERNAL_COLLISION` | FATAL | Two changes target same destination | Resolve member mapping collision. |
| `ERR_TCC_UNRESOLVED_CONFLICT` | FATAL | Unresolved blocking conflict in plan | Block plan progression to REVIEWABLE. |
