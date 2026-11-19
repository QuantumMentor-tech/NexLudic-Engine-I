# Execution Authorization Scope, Approval and Revocation Policy

**Document Identifier:** `NLE-POL-EAR-001`  
**Lifecycle Stage:** Content Pipeline Stage E Authorization Governance  
**Parent Document:** [HOST_GAME_EXECUTION_AUTHORIZATION_PRECONDITION_REVALIDATION_AND_TRANSACTIONAL_APPLICATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_EXECUTION_AUTHORIZATION_PRECONDITION_REVALIDATION_AND_TRANSACTIONAL_APPLICATION_SPECIFICATION.md) (`NLE-SPC-ETA-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Authorization Governance & Cryptographic Policy  

---

## 1. Authority Model & Non-Delegable Responsibilities

The **Execution Authorization Scope, Approval and Revocation Policy** governs who may grant permission to modify a game installation and establishes the binding rules for execution grants.

### 1.1 Permitted Authorities
1. **Certified Human Operator:** An authenticated user who has reviewed the application plan, accepted destructive change disclosures, and explicitly confirmed execution via UI/CLI.
2. **Certified Automated Integration Policy:** A cryptographically signed CI/CD or sandbox test runner possessing an approved automation certificate.

> **AI Agent Prohibition Invariant:** AI coding agents, subagents, or automated planning algorithms CANNOT authorize execution requests. An AI agent proposing an application plan has zero authority to approve or execute it.

---

## 2. Cryptographic Execution Authorization Grant (`ExecutionAuthorizationGrant`)

Execution requires an immutable, signed cryptographic grant:

```json
{
  "$schema": "https://schemas.nexludic.net/pipeline/v1/execution-authorization-grant.json",
  "grant_id": "grant-20261119-9941",
  "grant_version": "1.0.0",
  "bound_scope": {
    "package_id": "pkg-skyrim-nordic-armory-v1.0.0",
    "package_digest": "sha256:9a8b7c6d5e4f3a2b1c0d9e8f7a6b5c4d3e2f1a0b9c8d7e6f5a4b3c2d1e0f9a8b",
    "plan_id": "plan-skyrim-nordic-armory-v1.0.0",
    "plan_digest": "sha256:4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b",
    "target_installation_instance_id": "inst-7788-9900-aabb",
    "target_snapshot_token": "sha256:d8b2e104f981237a6b91c0e9d1a8e97f012b3c4d5e6f7a8b9c0d1e2f3a4b5c6d",
    "authorized_operation_count": 482,
    "authorized_destructive_operations": ["chg-20261119-0042"],
    "permitted_destination_prefix": "Data/"
  },
  "validity_window": {
    "issued_at": "2026-11-19T19:30:00Z",
    "expires_at": "2026-11-19T19:40:00Z"
  },
  "issuer": {
    "identity": "user:operator_01",
    "role": "CERTIFIED_MOD_MANAGER",
    "public_key": "ed25519:33445566778899aabbcc...",
    "signature": "ed25519:abcdef0123456789..."
  }
}
```

---

## 3. Strict Scope-Bound Authorization Checks

Before execution begins, the authorization engine evaluates six mandatory checks:
1. **Signature Validity:** Grant signature must be valid against the issuer's trusted public key.
2. **Expiration Check:** Current UTC timestamp must fall strictly within `issued_at` and `expires_at` (maximum window: 10 minutes).
3. **Exact Plan Digest Match:** `grant.bound_scope.plan_digest == request.plan_digest`.
4. **Exact Target Instance Match:** `grant.bound_scope.target_installation_instance_id == request.target_installation_instance_id`.
5. **Exact Operation Count Match:** Number of operations in plan must equal `authorized_operation_count`.
6. **Destructive Authorization:** Every operation classified as `REPLACE` or `REMOVE` must be explicitly listed in `authorized_destructive_operations`.

---

## 4. Revocation & Invalidation Triggers

An authorization grant is instantly **revoked** if:
- The user issues a cancellation command before mutation starts.
- Any change is made to the physical package artifact on disk.
- Any change is detected in target game files (target snapshot token drift).
- The grant validity window expires.

---

## 5. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_EAR_GRANT_EXPIRED` | FATAL | Authorization grant exceeded validity window | Reject execution. Re-request authorization. |
| `ERR_EAR_SCOPE_MISMATCH` | FATAL | Grant parameters do not match plan or target | Reject execution. Audit authorization scope. |
| `ERR_EAR_UNAUTHORIZED_DESTRUCTIVE` | FATAL | Plan contains unapproved REPLACE/REMOVE | Block execution. Require explicit approval. |
| `ERR_EAR_AI_SELF_AUTHORIZATION` | FATAL | AI agent attempted to issue execution grant | Hard abort. High-severity security alert. |
