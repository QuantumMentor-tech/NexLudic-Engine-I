# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Configuration Security & Validation Test Plan

**Document ID:** `NLE-PLN-CFG-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Quality & Security Test Plan  
**Traceability:** `ADR-0007` (Storage Baseline), `ADR-0008` (AI Abstraction), `ADR-0009` (Workspace Isolation), `ADR-0010` (Testing Strategy), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Quality Scope

The **Configuration Security and Validation Test Plan** establishes the automated testing suites, security verification checks, leak detection tests, and migration validation procedures governing the configuration and secret management subsystems of NexLudic Engine.

It also defines the **10 AI Coding Agent Rules for Configuration Changes** to ensure future autonomous coding agents respect configuration boundaries, secret segregation, and mandatory security policies.

---

## 2. The 10-Level Configuration Validation Suite

```
+----------------------------------------------------------------------------------------------------+
|                             THE 10-LEVEL CONFIGURATION TEST PLAN                                   |
|                                                                                                    |
|  Level 1: Configuration Schema Validation (JSON Schema Draft 2020-12, Types, Enums, Ranges)       |
|  Level 2: Deterministic Precedence & Conflict Tests (Mandatory Security Policy vs Project vs User)|
|  Level 3: Environment-Profile Isolation Tests (Resource Segregation, Mock Vault in Testing)        |
|  Level 4: Secrets Access & Least Privilege Tests (DPAPI Decryption, Reference Validation)         |
|  Level 5: Secret-Leakage & Redaction Tests (Serilog Interceptors, Prompt Scrubbing, Export Check) |
|  Level 6: Provider Configuration & Offline Tests (Model Compatibility, Fallback Handoff)           |
|  Level 7: Configuration Migration Tests (Schema V1.0 -> V1.1 Pipeline, Future Version Lock)       |
|  Level 8: Import, Export & Portability Tests (.nxpkg Sanitization, Machine-Specific Path Remap)   |
|  Level 9: Permission-Boundary Tests (Staging Jail Traversal Rejection, Approval Bypass Rejection) |
|  Level 10: Recovery & Corrupt Configuration Tests (Rollback to Safe Defaults, Error Diagnostics)   |
+----------------------------------------------------------------------------------------------------+
```

---

### Detailed Test Specifications:

#### Level 1: Configuration Schema Validation
* `TC-CFG-L1-01`: Valid `project.json` matches JSON Schema Draft 2020-12.
* `TC-CFG-L1-02`: Malformed JSON or type mismatch (e.g. string for integer port) rejected with `VALIDATION_SCHEMA_VIOLATION`.

#### Level 2: Deterministic Precedence & Conflict Tests
* `TC-CFG-L2-01`: Project-specific setting overrides User Global Preference.
* `TC-CFG-L2-02`: Project setting attempting to disable staging jail is **OVERRIDDEN & REJECTED** by Mandatory Security Policy (Layer 1).

#### Level 3: Environment-Profile Isolation Tests
* `TC-CFG-L3-01`: Testing profile enforces synthetic mock vault; real DPAPI vault is inaccessible.
* `TC-CFG-L3-02`: Development profile renders explicit visual environment banner in UI shell.

#### Level 4: Secrets Access & Least Privilege Tests
* `TC-CFG-L4-01`: DPAPI vault successfully encrypts and decrypts token under current user context.
* `TC-CFG-L4-02`: External game adapter worker requesting credential reference is denied access.

#### Level 5: Secret-Leakage & Redaction Tests
* `TC-CFG-L5-01`: Serilog logging of simulated exception containing `Bearer sk-ant-api...` replaces token with `[REDACTED_API_TOKEN]`.
* `TC-CFG-L5-02`: Assembled LLM prompt text inspected; confirms zero tokens or DPAPI contents present.

#### Level 6: Provider Configuration & Offline Tests
* `TC-CFG-L6-01`: AI task with missing credential reference gracefully pauses and reports `CREDENTIAL_UNAVAILABLE`.
* `TC-CFG-L6-02`: Complete network disconnection triggers seamless fallback to `manual_prompt_handoff` markdown export.

#### Level 7: Configuration Migration Tests
* `TC-CFG-L7-01`: Outdated schema v1.0.0 migrated to v1.1.0 with automatic snapshot in `backups/`.
* `TC-CFG-L7-02`: Manifest with schema v2.0.0 forces read-only mode in v1.x Core; write operations blocked.

#### Level 8: Import, Export & Portability Tests
* `TC-CFG-L8-01`: Exported `.nxpkg` package verified; confirms `credential_ref` is stripped and game paths are relative IDs.
* `TC-CFG-L8-02`: Importing `.nxpkg` on machine with missing game executable prompts interactive path mapping dialog.

#### Level 9: Permission-Boundary Tests
* `TC-CFG-L9-01`: Project configuration attempting path traversal (`staging/../../Windows`) rejected at boundary.
* `TC-CFG-L9-02`: Project configuration attempting `"auto_deploy": true` blocked by mandatory human approval gate.

#### Level 10: Recovery & Corrupt Configuration Tests
* `TC-CFG-L10-01`: Corrupted `appsettings.json` detected; Core restores `.bak` file or resets to safe system defaults with warning.

---

## 3. AI Coding Agent Rules for Configuration Changes

Future autonomous coding agents modifying configuration subsystems MUST adhere to these strict rules:

1. **No Plaintext Secrets:** Agents MUST NEVER place real API keys or tokens in source code, configuration files, test fixtures, or documentation.
2. **Never Print Secrets in Logs:** Agents MUST NOT output environment variables or credential values to console or log streams.
3. **Respect the Precedence Hierarchy:** Agents MUST NOT implement configuration overrides that bypass Mandatory Security Policy (Layer 1).
4. **Mandatory Schema Validation:** Every new configuration property added to Core models MUST have a corresponding JSON Schema definition.
5. **Preserve Unknown Settings:** When migrating or updating configuration files, unrecognized properties MUST be preserved.
6. **No File Writes Outside Staging:** Configuration changes must never direct tool outputs outside `<project>/staging/`.
7. **Isolate Test Credentials:** Automated tests MUST use synthetic mock tokens, never requesting live user DPAPI stores.
8. **Enforce Read-Only on Future Versions:** If a configuration file has a newer `MAJOR` version, agents must respect the read-only lock.
9. **Fail-Closed on Auth Errors:** If a provider credential fails, agents must pause the task, not retry blindly in an infinite loop.
10. **Stop and Consult:** If a requested feature requires bypassing the DPAPI vault or relaxing staging isolation, agents must stop and request explicit human engineering approval.

