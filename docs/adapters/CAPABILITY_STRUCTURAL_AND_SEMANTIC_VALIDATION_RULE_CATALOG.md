# Capability Structural and Semantic Validation Rule Catalog

**Document ID:** `NLE-CAT-VRC-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 36 of 100 — Capability Declaration Validation & Conformance Rules Specification  
**Classification:** Validation Rule Catalog & Diagnostic Error Dictionary  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-SPC-CVC-001`, `NLE-SCH-ACD-001`, `NLE-MOD-CCT-001`, `ADR-0002`

---

## 1. Catalog Scope & Governance

This catalog defines the formal, deterministic rules evaluated during **Stage B (Structural Validation)** and **Stage D (Semantic Validation)** of the Capability Declaration Validation Pipeline (`NLE-SPC-CVC-001`).

Every rule specifies:
- Stable Rule Identifier (`VLD-STR-###` or `VLD-SEM-###`).
- Rule Title, Purpose, and Subsystem Authority.
- Target Fields and Preconditions.
- Algorithmic Validation Logic.
- Finding Severity (`FATAL`, `ERROR`, `WARNING`, `INFO`).
- Diagnostic Error Code, Template Message, and Remediation Guidance.
- Traceability link to planned QA test specifications.

---

## 2. Stage B: Structural Validation Rules (`VLD-STR`)

Stage B verifies that the declaration strictly adheres to the Draft-07 JSON Schema defined in `NLE-SCH-ACD-001`.

```
+----------------------------------------------------------------------------------------------------+
|                                    STAGE B: STRUCTURAL RULES SUMMARY                               |
+-------------+------------------------------------+----------+------------------+-------------------+
| Rule ID     | Rule Title                         | Severity | Blocks Admission | Diagnostic Code   |
+-------------+------------------------------------+----------+------------------+-------------------+
| VLD-STR-001 | Mandatory Manifest Fields Present  | FATAL    | YES              | `ERR-VAL-STR-001` |
| VLD-STR-002 | Semantic Version Syntax Compliance | ERROR    | YES              | `ERR-VAL-STR-002` |
| VLD-STR-003 | Out-of-Process Execution Mode      | FATAL    | YES              | `ERR-VAL-STR-003` |
| VLD-STR-004 | Identifier Syntax & Naming Rules   | ERROR    | YES              | `ERR-VAL-STR-004` |
| VLD-STR-005 | Support Level Enum Recognition     | ERROR    | YES              | `ERR-VAL-STR-005` |
| VLD-STR-006 | Non-Empty Target Version Scope     | ERROR    | YES              | `ERR-VAL-STR-006` |
| VLD-STR-007 | Prohibited Extra / Unknown Fields  | WARNING  | NO (Ignored)     | `WRN-VAL-STR-007` |
+-------------+------------------------------------+----------+------------------+-------------------+
```

### 2.1 Rule `VLD-STR-001`: Mandatory Manifest Fields Present
- **Purpose:** Ensure all foundational metadata fields exist before semantic processing begins.
- **Authority:** `NLE-SCH-ACD-001` (Declaration Schema).
- **Target Fields:** `adapterId`, `adapterVersion`, `contractVersion`, `capabilityId`, `targetContext`, `supportScope`.
- **Validation Logic:** Assert that all target fields are present and non-null in the root JSON object.
- **Severity:** `FATAL` (Blocks further pipeline execution).
- **Diagnostic Code:** `ERR-VAL-STR-001`
- **Error Message:** *"Mandatory manifest field '{fieldName}' is missing from capability declaration."*
- **Remediation:** Provide the required field in `adapter.json` conforming to the schema specification.
- **Test ID:** `TC-CAP-VAL-001`

### 2.2 Rule `VLD-STR-002`: Semantic Version Syntax Compliance
- **Purpose:** Validate that version strings follow standard SemVer (`MAJOR.MINOR.PATCH`).
- **Authority:** `NLE-SPC-AVR-001` (Adapter Versioning Specification).
- **Target Fields:** `adapterVersion`, `contractVersion`.
- **Validation Logic:** Match string against regex `^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$`.
- **Severity:** `ERROR` (Blocks declaration qualification).
- **Diagnostic Code:** `ERR-VAL-STR-002`
- **Error Message:** *"Version string '{versionString}' in field '{fieldName}' violates SemVer syntax."*
- **Remediation:** Format version using three dot-separated integers (e.g., `1.2.0`).
- **Test ID:** `TC-CAP-VAL-002`

### 2.3 Rule `VLD-STR-003`: Out-of-Process Execution Mode Enforcement
- **Purpose:** Enforce process sandboxing architecture (`ADR-0002`).
- **Authority:** `NLE-CAT-ACM-001` Section 3; `ADR-0002`.
- **Target Field:** `executionMode`.
- **Validation Logic:** Assert that `executionMode == "OutOfProcessWorker"`. Any other string (e.g. `"InProcessDLL"`, `"EmbeddedAssembly"`) is invalid.
- **Severity:** `FATAL` (Blocks adapter admission).
- **Diagnostic Code:** `ERR-VAL-STR-003`
- **Error Message:** *"Execution mode '{executionMode}' is prohibited. Adapters must execute as 'OutOfProcessWorker'."*
- **Remediation:** Configure adapter to launch as an isolated worker process communicating via Named Pipes.
- **Test ID:** `TC-CAP-VAL-003`

### 2.4 Rule `VLD-STR-004`: Identifier Syntax & Naming Rules
- **Purpose:** Ensure stable, hyphenated alphanumeric identifiers across platform catalogs.
- **Authority:** `NLE-MOD-CCT-001` Section 5.
- **Target Fields:** `declarationId`, `adapterId`.
- **Validation Logic:** Assert match against `^[a-z0-9-]+$` with length between 3 and 64 characters.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-STR-004`
- **Error Message:** *"Identifier '{identifier}' contains invalid characters. Must be lowercase alphanumeric with hyphens."*
- **Remediation:** Remove uppercase letters, spaces, or underscores from identifier.
- **Test ID:** `TC-CAP-VAL-004`

### 2.5 Rule `VLD-STR-005`: Support Level Enum Recognition
- **Purpose:** Ensure adapter support assertions map cleanly into the 3 recognized states.
- **Authority:** `NLE-SCH-ACD-001` Section 3.3.
- **Target Field:** `supportScope.declaredSupportLevel`.
- **Validation Logic:** Assert value is in `["SUPPORTED", "CONDITIONALLY_SUPPORTED", "UNSUPPORTED"]`.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-STR-005`
- **Error Message:** *"Unrecognized declaredSupportLevel '{value}'. Must be SUPPORTED, CONDITIONALLY_SUPPORTED, or UNSUPPORTED."*
- **Remediation:** Update manifest to use one of the three standardized enum keys.
- **Test ID:** `TC-CAP-VAL-005`

### 2.6 Rule `VLD-STR-006`: Non-Empty Target Version Scope
- **Purpose:** Prevent unbounded declarations that claim universal support without version scoping.
- **Authority:** `NLE-SCH-ACD-001` Section 3.2.
- **Target Field:** `targetContext.supportedVersionRanges`.
- **Validation Logic:** Assert that `supportedVersionRanges` is an array with `length >= 1`.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-STR-006`
- **Error Message:** *"Target version scope array is empty. Adapter must declare at least one supported version range."*
- **Remediation:** Specify explicit `minimumVersion` and `maximumVersion` for target game.
- **Test ID:** `TC-CAP-VAL-006`

### 2.7 Rule `VLD-STR-007`: Prohibited Extra / Unknown Fields
- **Purpose:** Maintain strict schema hygiene while supporting explicit extension points.
- **Authority:** `NLE-SCH-ACD-001` Section 1.
- **Target Field:** Root object and sub-objects.
- **Validation Logic:** If unrecognized fields exist outside `x-extensions` namespace, emit warning diagnostic.
- **Severity:** `WARNING` (Non-blocking).
- **Diagnostic Code:** `WRN-VAL-STR-007`
- **Warning Message:** *"Unrecognized field '{fieldName}' detected in declaration. Field will be ignored."*
- **Remediation:** Relocate custom vendor properties into `x-extensions` or remove deprecated fields.
- **Test ID:** `TC-CAP-VAL-007`

---

## 3. Stage D: Semantic and Cross-Field Validation Rules (`VLD-SEM`)

Stage D verifies that fields within the declaration are logically coherent and do not contradict each other or the adapter's declared role.

```
+----------------------------------------------------------------------------------------------------+
|                                    STAGE D: SEMANTIC RULES SUMMARY                                 |
+-------------+------------------------------------+----------+------------------+-------------------+
| Rule ID     | Rule Title                         | Severity | Blocks Admission | Diagnostic Code   |
+-------------+------------------------------------+----------+------------------+-------------------+
| VLD-SEM-001 | Role-Capability Compatibility      | ERROR    | YES              | `ERR-VAL-SEM-001` |
| VLD-SEM-002 | Conditional Scope Explanation Req. | ERROR    | YES              | `ERR-VAL-SEM-002` |
| VLD-SEM-003 | Condition Non-Negation Check       | ERROR    | YES              | `ERR-VAL-SEM-003` |
| VLD-SEM-004 | Memory Budget Sanity Ceiling       | WARNING  | NO (Clamped)     | `WRN-VAL-SEM-004` |
| VLD-SEM-005 | Timeout Budget Operating Bounds    | ERROR    | YES              | `ERR-VAL-SEM-005` |
| VLD-SEM-006 | Tested Build Hash Formatting       | ERROR    | YES              | `ERR-VAL-SEM-006` |
+-------------+------------------------------------+----------+------------------+-------------------+
```

### 3.1 Rule `VLD-SEM-001`: Role-Capability Compatibility
- **Purpose:** Ensure an adapter does not declare capabilities outside its architectural role.
- **Authority:** `NLE-CAT-ACM-001` Section 4; `NLE-SCH-CCD-001` Section 3.2.
- **Preconditions:** `Stage C` passed; canonical definition loaded.
- **Validation Logic:** Assert that root `adapterRole` is present in the target capability's `intendedAdapterRoles` array.
  - *Example Violation:* An adapter with role `ASSET_TRANSCODER` declaring `CAP-EXT-HOOK-DETOUR` (which requires `HOST_MOD_SCAFFOLD` or `HYBRID_COORDINATOR`).
- **Severity:** `ERROR` (Blocks capability qualification).
- **Diagnostic Code:** `ERR-VAL-SEM-001`
- **Error Message:** *"Adapter role '{adapterRole}' is not permitted to declare capability '{capabilityId}'. Permitted roles: [{permittedRoles}]."*
- **Remediation:** Remove capability from manifest or update adapter architecture to an approved role.
- **Test ID:** `TC-CAP-VAL-008`

### 3.2 Rule `VLD-SEM-002`: Conditional Scope Explanation Required
- **Purpose:** Prevent ambiguous conditional declarations lacking technical explanations.
- **Authority:** `NLE-SCH-ACD-001` Section 3.3.
- **Preconditions:** `supportScope.declaredSupportLevel == "CONDITIONALLY_SUPPORTED"`.
- **Validation Logic:** Assert that `supportScope.partialScopeDescription` is non-null, non-empty, and has string length $\ge 10$ characters.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-SEM-002`
- **Error Message:** *"Capability marked CONDITIONALLY_SUPPORTED but 'partialScopeDescription' is missing or too brief."*
- **Remediation:** Provide clear description of limitations (e.g. *"Supported only in single-player sandbox mode"*).
- **Test ID:** `TC-CAP-VAL-009`

### 3.3 Rule `VLD-SEM-003`: Operational Condition Non-Negation Check
- **Purpose:** Ensure declared conditions do not completely negate the capability's primary function.
- **Authority:** `NLE-MOD-CCT-001` Section 1.2.
- **Preconditions:** `supportScope.operationalConditions` is populated.
- **Validation Logic:** Evaluate string patterns; assert condition text does not contain negation phrases that render the capability universally unusable (e.g. *"Feature is disabled in all releases"*).
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-SEM-003`
- **Error Message:** *"Operational condition '{conditionText}' directly negates capability support. Mark capability as UNSUPPORTED instead."*
- **Remediation:** Set `declaredSupportLevel` to `UNSUPPORTED` if the feature is completely disabled.
- **Test ID:** `TC-CAP-VAL-010`

### 3.4 Rule `VLD-SEM-004`: Memory Budget Sanity Ceiling
- **Purpose:** Prevent excessive memory allocation declarations that exceed host quotas.
- **Authority:** `NLE-CAT-ACM-001` Section 3; `DEC-009`.
- **Target Field:** `resourceFootprint.allocatedMemoryBytes`.
- **Validation Logic:** If declared, assert `allocatedMemoryBytes <= 1073741824` (1 GB). If $> 1\text{ GB}$, emit warning and clamp to default maximum quota (512 MB).
- **Severity:** `WARNING` (Non-blocking).
- **Diagnostic Code:** `WRN-VAL-SEM-004`
- **Warning Message:** *"Declared memory footprint ({bytes} bytes) exceeds standard 512 MB worker budget. Clamping to maximum permitted."*
- **Remediation:** Optimize adapter memory consumption or request dedicated multi-process resource grant.
- **Test ID:** `TC-CAP-VAL-011`

### 3.5 Rule `VLD-SEM-005`: Timeout Budget Operating Bounds
- **Purpose:** Enforce responsive IPC communication and prevent worker thread hangs.
- **Authority:** `NLE-SPC-AVR-001` Section 4.2.
- **Target Field:** `resourceFootprint.maxTimeoutMs`.
- **Validation Logic:** Assert that `1000 <= maxTimeoutMs <= 30000` (Between 1 second and 30 seconds).
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-SEM-005`
- **Error Message:** *"Declared timeout budget '{timeoutMs} ms' outside allowed bounds [1000, 30000]."*
- **Remediation:** Set timeout within standard operational range (e.g. 5000 ms).
- **Test ID:** `TC-CAP-VAL-012`

### 3.6 Rule `VLD-SEM-006`: Tested Build Hash Formatting
- **Purpose:** Ensure executable build hashes conform to standard SHA-256 formatting.
- **Authority:** `NLE-SCH-ACD-001` Section 3.2; `NLE-SPC-LID-001`.
- **Target Field:** `targetContext.supportedVersionRanges[].testedBuildHash`.
- **Validation Logic:** If present, assert string matches regex `^sha256:[a-f0-9]{64}$`.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-SEM-006`
- **Error Message:** *"Tested build hash '{hash}' invalid. Must be formatted as 'sha256:' followed by 64 lowercase hex characters."*
- **Remediation:** Calculate SHA-256 hash using `Get-FileHash` or standard OpenSSL tools.
- **Test ID:** `TC-CAP-VAL-013`
