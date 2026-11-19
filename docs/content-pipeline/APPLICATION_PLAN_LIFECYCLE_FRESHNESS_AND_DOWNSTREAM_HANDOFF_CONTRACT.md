# Application Plan Lifecycle, Freshness and Downstream Handoff Contract

**Document Identifier:** `NLE-CON-ALH-001`  
**Lifecycle Stage:** Content Pipeline Stage I Handoff Contract  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**Downstream Interface:** Stage E (Execution Engine) / Stage V (Verification Engine)  
**System Status:** Authoritative Architectural Contract  
**Classification:** Inter-Stage Contracts & Handoff Governance  

---

## 1. Purpose & Scope

The **Application Plan Lifecycle, Freshness and Downstream Handoff Contract** governs the state progression of an `ApplicationPlan` and defines the authoritative data contract required to hand off a fully reviewed and approved plan to downstream pipeline stages (Stage E / Change Execution or Stage V / Verification).

---

## 2. Application Plan Formal Document Schema

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/application-plan.json",
  "plan_id": "plan-skyrim-nordic-armory-v1.0.0",
  "plan_version": "1.0.0",
  "status": "APPROVED",
  "source_package": {
    "package_id": "pkg-skyrim-nordic-armory-v1.0.0",
    "package_digest": "sha256:9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b",
    "receipt_id": "mpr-20261119-48192"
  },
  "target_environment": {
    "game_id": "game-skyrim-se",
    "release_id": "release-1.6.640-steam",
    "installation_instance_id": "inst-7788-9900-aabb",
    "target_snapshot_token": "sha256:d8b2e104f981237a6b91c0e9d1a8e97f012b3c4d5e6f7a8b9c0d1e2f3a4b5c6d"
  },
  "planned_changes": [
    {
      "change_id": "chg-001",
      "operation": "ADD",
      "destination_relative_path": "Data/Textures/Armor/Nordic/armor_d.dds",
      "package_member_id": "mem-001-diffuse",
      "source_sha256": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  "dependency_dag": {
    "root_nodes": ["chg-001"],
    "edges": []
  },
  "conflict_assessment": {
    "blocking_conflicts_count": 0,
    "warnings_count": 0
  },
  "approval_attestation": {
    "approval_token_id": "appr-20261119-9941",
    "signer_public_key": "ed25519:33445566778899aabbcc...",
    "signature": "ed25519:abcdef0123456789..."
  },
  "plan_digest": "sha256:4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b"
}
```

---

## 3. Downstream Handoff Receipt Schema

Upon successful approval, Stage I emits the **Plan Handoff Receipt**:

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/plan-handoff-receipt.json",
  "receipt_id": "rcpt-handoff-20261119-102",
  "plan_id": "plan-skyrim-nordic-armory-v1.0.0",
  "plan_digest": "sha256:4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b",
  "downstream_authorization": {
    "ready_for_execution": true,
    "requires_pre_execution_revalidation": true,
    "requires_isolated_backup_journal": true
  },
  "non_execution_warranty": "This receipt certifies the completion of static application planning. Zero files have been modified. Execution requires Stage E authorization.",
  "timestamp": "2026-11-19T19:20:00Z"
}
```

---

## 4. Downstream Invariants

1. **Pre-Execution Revalidation:** Downstream Stage E MUST verify that `target_snapshot_token` matches current target filesystem state immediately prior to applying changes.
2. **Immutable Handoff:** Once issued, an `ApplicationPlan` is permanently sealed; any modification requires a new plan ID and re-approval.
