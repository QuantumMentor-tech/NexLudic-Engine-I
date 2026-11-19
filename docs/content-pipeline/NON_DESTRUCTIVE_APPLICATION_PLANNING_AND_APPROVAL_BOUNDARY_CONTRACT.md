# Non-Destructive Application Planning and Approval Boundary Contract

**Document Identifier:** `NLE-CON-NDA-001`  
**Lifecycle Stage:** Content Pipeline Stage I Governance Contract  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**System Status:** Authoritative Architectural Contract  
**Classification:** Operational Boundaries & Cryptographic Approval Governance  

---

## 1. Objective & Non-Destructive Guarantees

The **Non-Destructive Application Planning and Approval Boundary Contract** establishes the immutable dry-run boundaries for Stage I of the Content Pipeline. It defines the formal protocols separating planning activities from execution authorities.

### Non-Destructive Planning Guarantees
Stage I planning guarantees:
1. **Zero Filesystem Mutation:** The planning engine must not create, modify, overwrite, rename, or delete any files within the target game directory.
2. **Zero Process Interaction:** The planning engine must not launch, inspect memory of, inject DLLs into, or attach debuggers to host-game processes.
3. **Zero Configuration Tampering:** Registry keys, launcher configurations, and mod loader global states must not be altered during plan generation.

---

## 2. Segregation of Permissions Matrix

To prevent privilege escalation or accidental modification, NexLudic Engine defines seven distinct, independently granted permissions:

```
+--------------------------------------------------------------------------------------------------+
|                                INDEPENDENT PERMISSION MATRIX                                     |
+--------------------------------------------------------------------------------------------------+
|  Permission Scope          | Allowed Activities                                | Can Mutate Disk?|
+----------------------------+---------------------------------------------------+-----------------+
|  PERM_PACKAGE_INSPECT      | Read package metadata, manifests, digests         | NO              |
|  PERM_TARGET_ASSESS        | Read target environment records & executable meta | NO              |
|  PERM_PLAN_GENERATE        | Compute mapping, detect conflicts, assemble plan  | NO              |
|  PERM_PLAN_REVIEW          | Inspect proposed changes and impact matrices      | NO              |
|  PERM_PLAN_APPROVE         | Issue cryptographic approval tokens for a plan    | NO              |
|  PERM_STAGING_WRITE        | Write backup files to isolated workspace staging  | YES (Workspace) |
|  PERM_TARGET_EXECUTE       | Modify target game files according to plan        | YES (Target)    |
+--------------------------------------------------------------------------------------------------+
```

> **Mandatory Rule:** `PERM_PLAN_GENERATE` or `PERM_PLAN_REVIEW` must NEVER imply or inherit `PERM_TARGET_EXECUTE`.

---

## 3. Cryptographic Plan Approval Scope & Token Schema

An application plan cannot progress to downstream execution stages without an authoritative **Plan Approval Token** signed by an authorized human operator or automated policy authority:

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/plan-approval-token.json",
  "approval_id": "appr-20261119-9941",
  "plan_id": "plan-skyrim-nordic-armory-v1",
  "plan_sha256": "sha256:4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b",
  "bound_package_digest": "sha256:9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b",
  "bound_target_snapshot": "sha256:d8b2e104f981237a6b91c0e9d1a8e97f012b3c4d5e6f7a8b9c0d1e2f3a4b5c6d",
  "approved_operations_count": 482,
  "accepted_destructive_operations": ["chg-20261119-0042"],
  "approver": {
    "identity": "user:operator_01",
    "role": "CERTIFIED_INTEGRATION_LEAD",
    "public_key": "ed25519:33445566778899aabbcc...",
    "signature": "ed25519:abcdef0123456789..."
  },
  "issued_at": "2026-11-19T19:15:00Z",
  "expires_at": "2026-11-19T20:15:00Z"
}
```

### 3.1 Approval Invalidation Rules
An issued approval token is automatically revoked if:
- The underlying physical package bytes change (`PackageDigest` mismatch).
- The target environment drifts (`TargetSnapshot` mismatch).
- Any proposed change in the plan is edited, reordered, or removed.

---

## 4. Contract Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_NDA_EXECUTION_ATTEMPT_IN_PLAN` | FATAL | System attempted file write during Stage I planning | Hard abort. Security audit event. |
| `ERR_NDA_UNAUTHORIZED_APPROVAL` | FATAL | Approval token invalid, forged, or expired | Reject handoff. Require re-approval. |
| `ERR_NDA_APPROVAL_SCOPE_DRIFT` | FATAL | Plan content changed after approval signature issued | Revoke approval. Plan reverted to DRAFT. |
