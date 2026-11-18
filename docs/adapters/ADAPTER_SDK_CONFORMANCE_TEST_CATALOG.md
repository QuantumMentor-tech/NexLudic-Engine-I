# ADAPTER SDK CONFORMANCE TEST CATALOG
**Document ID:** `NLE-CAT-CTC-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior QA Automation Engineer, Testing Architect, SDK Engineer, Systems Verification Specialist  
**Classification:** Conformance Test Catalog  

---

## 1. PURPOSE AND CATALOG STRUCTURE

### 1.1 Purpose
This catalog defines the authoritative, comprehensive test inventory used by the NexLudic Universal Adapter SDK Conformance Test Harness (`CMP-41`). Every test case in this catalog evaluates a specific, traceable contract requirement established in Phase 5 (Steps 34–40) or architectural decisions from Phase 2 (`ADR-0002`, `DEC-002`, `DEC-009`, `BLK-SEC-01`).

### 1.2 Test Catalog Organization
The test suite is organized into 9 primary contract categories encompassing **84 formal conformance checks**:

```
+--------------------------------------------------------------------------------------------------------+
|                               CONFORMANCE TEST CATALOG (84 CHECKS)                                     |
|                                                                                                        |
|  [ CAT 1 ] Package, Archive & Descriptor Integrity               (10 Checks: CONF-PKG-001..010)       |
|  [ CAT 2 ] SDK Wire Protocol & IPC Contract                      (10 Checks: CONF-PROT-001..010)      |
|  [ CAT 3 ] Canonical Capability Declaration & Semantics          (10 Checks: CONF-CAP-001..010)       |
|  [ CAT 4 ] Registry Eligibility & Discovery Contracts             (8 Checks: CONF-REG-001..008)       |
|  [ CAT 5 ] Capability Matching & Resolution Behavior              (8 Checks: CONF-MAT-001..008)       |
|  [ CAT 6 ] Negotiation & Contextual Availability Behavior         (8 Checks: CONF-NEG-001..008)       |
|  [ CAT 7 ] Lifecycle, Handshake, Health & Teardown               (12 Checks: CONF-LFC-001..012)       |
|  [ CAT 8 ] Cross-Contract Boundaries & Degradation Regressions    (8 Checks: CONF-XCON-001..008)      |
|  [ CAT 9 ] Security Sandboxing, Tokens & Anti-Cheat Coexistence  (10 Checks: CONF-SEC-001..010)       |
+--------------------------------------------------------------------------------------------------------+
```

---

## 2. CATEGORY 1: PACKAGE, ARCHIVE, AND DESCRIPTOR INTEGRITY (`CONF-PKG`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-PKG-001` | Archive Format Verification | Static | Mandatory | Packaging Spec | Archive MUST be unencrypted ZIP/TAR; valid headers; zero corruption. | `ERR_PKG_CORRUPT_ARCHIVE` |
| `CONF-PKG-002` | Package Size Cap Enforcement | Static | Mandatory | `NLE-REQ-SRA-001` | Uncompressed archive MUST NOT exceed 256 MB; compressed $\le 64$ MB. | `ERR_PKG_OVERSIZED_ARCHIVE` |
| `CONF-PKG-003` | Path Traversal Vulnerability Scan | Static | Mandatory | Security Baseline | Archive MUST NOT contain absolute paths, drive letters, or `..` sequences. | `CRIT_SEC_PATH_TRAVERSAL` |
| `CONF-PKG-004` | Descriptor Presence and Location | Static | Mandatory | `NLE-SPC-SDK-001` | Archive MUST contain `adapter.json` located strictly at root directory. | `ERR_PKG_MISSING_DESCRIPTOR` |
| `CONF-PKG-005` | Descriptor Syntax Validation | Static | Mandatory | `NLE-SCH-ACD-001` | `adapter.json` MUST parse as valid UTF-8 JSON without trailing commas. | `ERR_PKG_INVALID_JSON_SYNTAX` |
| `CONF-PKG-006` | Canonical Adapter Identity Format | Static | Mandatory | `NLE-MOD-CCT-001` | `adapter_id` MUST conform to regex `^[a-z0-9-]+(\.[a-z0-9-]+)+$`. | `ERR_PKG_INVALID_ADAPTER_ID` |
| `CONF-PKG-007` | Semantic Versioning Syntax | Static | Mandatory | `NLE-MOD-CCT-001` | `version` string MUST conform strictly to SemVer 2.0.0 (`X.Y.Z`). | `ERR_PKG_INVALID_SEMVER` |
| `CONF-PKG-008` | Binary Checksum Presence | Static | Mandatory | `NLE-SPC-VCR-001` | Manifest MUST provide SHA-256 digests for all referenced binaries. | `ERR_PKG_MISSING_CHECKSUM` |
| `CONF-PKG-009` | Prohibited Executable Formats | Static | Mandatory | Security Baseline | Package MUST NOT contain `.bat`, `.cmd`, `.ps1`, `.vbs`, or kernel `.sys`. | `CRIT_SEC_FORBIDDEN_FILETYPE` |
| `CONF-PKG-010` | Unknown Property Rejection | Static | Mandatory | `NLE-SCH-ACD-001` | `adapter.json` root MUST have `additionalProperties: false`. | `ERR_PKG_UNKNOWN_PROPERTY` |

---

## 3. CATEGORY 2: SDK WIRE PROTOCOL & IPC CONTRACT (`CONF-PROT`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-PROT-001`| JSON-RPC 2.0 Header Conformance | Sim/Dyn | Mandatory | `NLE-SPC-SDK-001` | All RPC payloads MUST declare `"jsonrpc": "2.0"` and valid request `id`. | `ERR_PROT_INVALID_JSONRPC` |
| `CONF-PROT-002`| Control Pipe Naming Convention | Sim/Dyn | Mandatory | `NLE-CON-AIR-001` | Pipe path MUST match `\\.\pipe\nexludic_ctl_<session>_<adapter>`. | `ERR_PROT_INVALID_PIPE_NAME` |
| `CONF-PROT-003`| Standard Method Ingest Handling | Sim/Dyn | Mandatory | `NLE-SPC-SDK-001` | Worker MUST handle `adapter.initialize`, `adapter.ping`, `adapter.shutdown`.| `ERR_PROT_UNHANDLED_METHOD` |
| `CONF-PROT-004`| Standard Error Code Conformance | Sim/Dyn | Mandatory | JSON-RPC 2.0 Spec | Negative error codes MUST use standard JSON-RPC range (`-32700..-32603`). | `ERR_PROT_NONSTANDARD_ERROR` |
| `CONF-PROT-005`| Unknown Method Response | Sim/Dyn | Mandatory | `NLE-SPC-SDK-001` | Unrecognized methods MUST return error code `-32601` (`Method not found`). | `ERR_PROT_METHOD_NOT_FOUND` |
| `CONF-PROT-006`| Payload Framing and Delimitation | Sim/Dyn | Mandatory | `NLE-CON-AIR-001` | Messages over stream channels MUST be newline-delimited (`\n`). | `ERR_PROT_FRAMING_ERROR` |
| `CONF-PROT-007`| Bidirectional Request ID Correlation | Sim/Dyn | Mandatory | JSON-RPC 2.0 Spec | Response `id` MUST exactly match the inbound request `id`. | `ERR_PROT_CORRELATION_MISMATCH`|
| `CONF-PROT-008`| Malformed Inbound Message Handling | Sim/Dyn | Mandatory | Robustness Baseline| Ingestion of malformed JSON MUST yield error `-32700` without process crash. | `ERR_PROT_PARSE_ERROR` |
| `CONF-PROT-009`| Message Buffer Length Limit | Sim/Dyn | Mandatory | `NLE-REQ-SRA-001` | Messages exceeding 16 MB MUST be rejected with error `-32600`. | `ERR_PROT_MESSAGE_OVERFLOW` |
| `CONF-PROT-010`| Notification Handling Semantics | Sim/Dyn | Mandatory | JSON-RPC 2.0 Spec | Notifications (requests lacking `id`) MUST NOT produce a response packet. | `ERR_PROT_ILLEGAL_RESPONSE` |

---

## 4. CATEGORY 3: CANONICAL CAPABILITY DECLARATIONS & SEMANTICS (`CONF-CAP`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-CAP-001` | Canonical Taxonomy Membership | Static | Mandatory | `NLE-MOD-CCT-001` | Declared capability IDs MUST exist in canonical capability catalog. | `ERR_CAP_UNRECOGNIZED_ID` |
| `CONF-CAP-002` | Capability Semantic Version Range | Static | Mandatory | `NLE-MOD-CCT-001` | `target_version` MUST declare valid SemVer range (e.g. `^1.0.0`). | `ERR_CAP_INVALID_VERSION_SPEC` |
| `CONF-CAP-003` | Strategy Compatibility Match | Static | Mandatory | `NLE-SPC-SDK-001` | Declared strategy MUST be compatible with capability execution mode. | `ERR_CAP_STRATEGY_MISMATCH` |
| `CONF-CAP-004` | DAG Dependency Cycle Detection | Static | Mandatory | `NLE-SPC-CVC-001` | Kahn DAG topological sort MUST confirm zero cyclic dependencies. | `ERR_CAP_CYCLIC_DEPENDENCY` |
| `CONF-CAP-005` | Required Parameters Schema Validity| Static | Mandatory | `NLE-MOD-CCT-001` | Declared configuration parameter schemas MUST be valid JSON Schema. | `ERR_CAP_INVALID_PARAM_SCHEMA` |
| `CONF-CAP-006` | Evidence Provenance Completeness | Static | Mandatory | `NLE-SPC-CVC-001` | Declared evidence references MUST include valid test ID and SHA-256 hash. | `ERR_CAP_INCOMPLETE_PROVENANCE`|
| `CONF-CAP-007` | Absence-of-Evidence State Fallback | Static | Mandatory | `NLE-SPC-CVC-001` | Unsubstantiated declarations MUST downgrade to `PROPOSED_UNVERIFIED`. | `WARN_CAP_UNVERIFIED_EVIDENCE` |
| `CONF-CAP-008` | Resource Consumption Bounds | Static | Mandatory | `NLE-REQ-SRA-001` | Declared memory footprint MUST be declared $\le 512$ MB. | `ERR_CAP_EXCESSIVE_RESOURCE` |
| `CONF-CAP-009` | Hook Injection Safety Declarations| Static | Mandatory | `BLK-SEC-01` | Capabilities using memory injection MUST declare target game requirements. | `ERR_CAP_UNGUARDED_INJECTION` |
| `CONF-CAP-010` | Deprecated Capability Warning | Static | Advisory | `NLE-MOD-CCT-001` | Declarations referencing deprecated capabilities MUST emit warning. | `WARN_CAP_DEPRECATED_REFERENCE`|

---

## 5. CATEGORY 4: REGISTRY ELIGIBILITY & DISCOVERY CONTRACTS (`CONF-REG`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-REG-001` | Registry Record Schema Conformance | Static | Mandatory | `NLE-SCH-RRC-001` | Adapter record MUST conform to `RegistryRecord` schema. | `ERR_REG_SCHEMA_INVALID` |
| `CONF-REG-002` | Cryptographic Hash Binding | Static | Mandatory | `NLE-SPC-VCR-001` | Registered SHA-256 MUST match binary hash computed at verification. | `ERR_REG_HASH_MISMATCH` |
| `CONF-REG-003` | Duplicate Registration Idempotence| Sim | Mandatory | `NLE-SPC-VCR-001` | Re-registering identical version MUST return existing record without error.| `WARN_REG_DUPLICATE_REGISTRATION`|
| `CONF-REG-004` | Version Immutability Enforcement | Sim | Mandatory | `NLE-SPC-VCR-001` | Mutating files under an existing `(adapter_id, version)` MUST be rejected. | `ERR_REG_VERSION_MUTATION` |
| `CONF-REG-005` | Discovery Query Resolution | Sim | Mandatory | `NLE-SCH-CDQ-001` | Adapter MUST be accurately discoverable via capability, game ID, or strategy.| `ERR_REG_DISCOVERY_MISSING` |
| `CONF-REG-006` | Lifecycle State Registry Binding | Sim | Mandatory | `NLE-SPC-VCR-001` | Newly registered adapter MUST enter `STATUS_REGISTERED`. | `ERR_REG_INVALID_INITIAL_STATE`|
| `CONF-REG-007` | Provenance Audit Record Generation | Sim | Mandatory | `DEC-009` | Registry write MUST emit audit event to Volume D `.audit/registry/`. | `ERR_REG_AUDIT_LOG_MISSING` |
| `CONF-REG-008` | Quarantine Registry Exclusion | Sim | Mandatory | `NLE-SPC-SCR-001` | Adapters flagged `STATUS_QUARANTINED` MUST NOT appear in active queries. | `ERR_REG_QUARANTINE_LEAK` |

---

## 6. CATEGORY 5: CAPABILITY MATCHING & REQUIREMENT RESOLUTION (`CONF-MAT`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-MAT-001` | Exact Capability Identity Match | Sim | Mandatory | `NLE-SPC-CMR-001` | Evaluator MUST match identical canonical capability IDs. | `ERR_MAT_EXACT_MATCH_FAILED` |
| `CONF-MAT-002` | Version Constraint Resolution | Sim | Mandatory | `NLE-SPC-CMR-001` | Adapter within SemVer range matches; adapter outside range is rejected. | `ERR_MAT_VERSION_MISMATCH` |
| `CONF-MAT-003` | Multi-Tier Criticality Handling | Sim | Mandatory | `NLE-SCH-CRQ-001` | Missing mandatory capability fails match; missing optional yields degraded. | `ERR_MAT_CRITICALITY_EVAL_ERROR`|
| `CONF-MAT-004` | Transitive Dependency Resolution | Sim | Mandatory | `NLE-RUL-CDR-001` | Dependencies of matched capabilities MUST resolve transitively. | `ERR_MAT_UNRESOLVED_DEPENDENCY` |
| `CONF-MAT-005` | Candidate Ranking Determinism | Sim | Mandatory | `NLE-POL-CCR-001` | When multiple candidates exist, tier-ranking rules MUST break ties. | `ERR_MAT_NONDETERMINISTIC_RANK` |
| `CONF-MAT-006` | Incompatible Strategy Exclusion | Sim | Mandatory | `NLE-CAT-MTR-001` | Pairing incompatible strategies (e.g. mutually exclusive hooks) rejected. | `ERR_MAT_STRATEGY_CONFLICT` |
| `CONF-MAT-007` | Explainability Standard Compliance | Sim | Mandatory | `NLE-CON-MRE-001` | Match result payload MUST answer all 12 explainability standard fields. | `ERR_MAT_INCOMPLETE_EXPLANATION`|
| `CONF-MAT-008` | Match Result Schema Conformance | Sim | Mandatory | `NLE-SCH-MRP-001` | Result payload MUST validate strictly against `CapabilityMatchResultPayload`.| `ERR_MAT_PAYLOAD_SCHEMA_INVALID`|

---

## 7. CATEGORY 6: NEGOTIATION & CONTEXTUAL AVAILABILITY (`CONF-NEG`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-NEG-001` | Joint Capability Set Resolution | Sim | Mandatory | `NLE-RUL-JSC-001` | Intersection of supported features between two adapters computed accurately.| `ERR_NEG_INTERSECTION_FAILED` |
| `CONF-NEG-002` | Shared Resource Lock Exclusion | Sim | Mandatory | `JSC-EXC-001` | Two adapters requesting identical exclusive hook points MUST be rejected. | `ERR_NEG_RESOURCE_CONFLICT` |
| `CONF-NEG-003` | Contract Parameter Convergence | Sim | Mandatory | `NLE-SPC-CCN-001` | Polling frequency and buffer sizes MUST converge on overlapping ranges. | `ERR_NEG_PARAMETER_DIVERGENCE` |
| `CONF-NEG-004` | Non-Overlapping Parameter Rejection| Sim | Mandatory | `NLE-SPC-CCN-001` | Mutually disjoint parameter ranges MUST fail negotiation gracefully. | `ERR_NEG_DISJOINT_PARAMETERS` |
| `CONF-NEG-005` | Graceful Degradation Selection | Sim | Mandatory | `JSC-DEG-001` | If optional capability fails parameter convergence, optional is dropped. | `WARN_NEG_OPTIONAL_DROPPED` |
| `CONF-NEG-006` | Contextual Availability State Map | Sim | Mandatory | `NLE-MOD-CAN-001` | Capabilities correctly mapped into 7-state availability taxonomy. | `ERR_NEG_INVALID_AVAIL_STATE` |
| `CONF-NEG-007` | Negotiation Timeout Handling | Sim/Dyn | Mandatory | `NLE-SPC-CNA-001` | Unresponsive negotiation party MUST timeout at 10,000 ms. | `ERR_NEG_TRANSACTION_TIMEOUT` |
| `CONF-NEG-008` | Negotiation Result Schema Conformance| Sim | Mandatory | `NLE-SCH-NRP-001` | Result payload MUST validate strictly against `CapabilityNegotiationResult`. | `ERR_NEG_PAYLOAD_SCHEMA_INVALID`|

---

## 8. CATEGORY 7: LIFECYCLE, HEALTH, AND TEARDOWN (`CONF-LFC`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-LFC-001` | Runtime Admission Gate Evaluation | Dyn | Mandatory | `NLE-CON-AIR-001` | Worker MUST pass all 6 admission gates before process execution. | `ERR_LFC_ADMISSION_DENIED` |
| `CONF-LFC-002` | Worker Process Spawn under Medium IL| Dyn | Mandatory | `DEC-002` | Worker process MUST spawn at Windows Medium Integrity Level. | `CRIT_SEC_ELEVATION_DETECTED` |
| `CONF-LFC-003` | JSON-RPC Handshake Response | Dyn | Mandatory | `NLE-SCH-AIR-001` | Worker MUST reply to `adapter.initialize` within 10,000 ms (`T_BOOT_MAX`). | `ERR_LFC_HANDSHAKE_TIMEOUT` |
| `CONF-LFC-004` | Ready Report Schema Validation | Dyn | Mandatory | `NLE-SCH-ARD-001` | Worker MUST emit valid `ReadyReport` payload upon internal initialization. | `ERR_LFC_INVALID_READY_REPORT` |
| `CONF-LFC-005` | Active Ping/Pong Heartbeat Timing | Dyn | Mandatory | `NLE-POL-HSF-001` | Worker MUST answer `adapter.ping` with `adapter.pong` in $< 50\text{ ms}$. | `ERR_LFC_HEARTBEAT_TIMEOUT` |
| `CONF-LFC-006` | Health Freshness Degradation to STALE| Dyn | Mandatory | `NLE-POL-HSF-001` | Missing 2 consecutive pings MUST transition health state to `STALE`. | `WARN_LFC_HEALTH_STALE` |
| `CONF-LFC-007` | Fatal Health Escalation to UNHEALTHY| Dyn | Mandatory | `NLE-POL-HSF-001` | Missing 3 consecutive pings MUST transition health state to `UNHEALTHY`. | `ERR_LFC_HEALTH_UNHEALTHY` |
| `CONF-LFC-008` | Orderly Shutdown Grace Period Drain | Dyn | Mandatory | `NLE-SPC-SCR-001` | Worker MUST unhook, flush buffers, and exit cleanly in $\le 5,000\text{ ms}$. | `ERR_LFC_SHUTDOWN_HANG` |
| `CONF-LFC-009` | Forced Termination on Timeout | Dyn | Mandatory | `NLE-SPC-SCR-001` | If worker hangs past 5,000 ms, Supervisor executes `TerminateProcess`. | `WARN_LFC_FORCED_KILL_EXECUTED`|
| `CONF-LFC-010` | Resource Handle Reclamation Post-Exit| Dyn | Mandatory | `ADR-0002` | Named Pipe and Job Object handles MUST be cleanly closed post-termination. | `ERR_LFC_HANDLE_LEAK` |
| `CONF-LFC-011` | Crash Circuit Breaker Quarantine | Dyn | Mandatory | `NLE-SPC-SCR-001` | $\ge 3$ crashes within 60s MUST lock adapter into `QUARANTINED` state. | `CRIT_LFC_CIRCUIT_TRIPPED` |
| `CONF-LFC-012` | Lifecycle Event Audit Chain Logging| Dyn | Mandatory | `NLE-CON-LEA-001` | All state transitions MUST be logged to SHA-256 rolling audit log. | `ERR_LFC_AUDIT_CHAIN_BROKEN` |

---

## 9. CATEGORY 8: CROSS-CONTRACT BOUNDARIES & REGRESSION (`CONF-XCON`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-XCON-001`| Structural Valid but Semantic Invalid| Sim | Mandatory | Step 35 vs Step 36 | Syntactically valid JSON referencing fake canonical capability fails at L03.| `ERR_XCON_SEMANTIC_REJECTION` |
| `CONF-XCON-002`| Semantic Valid but Registry Ineligible| Sim | Mandatory | Step 36 vs Step 37 | Valid capability declarations lacking binary hash fails registry admission.| `ERR_XCON_REGISTRY_INELIGIBLE` |
| `CONF-XCON-003`| Admitted Adapter Not Yet Initialized | Sim | Mandatory | Step 37 vs Step 40 | Process admitted to runtime cannot be queried for data before `READY`. | `ERR_XCON_PREMATURE_DATA_ACCESS`|
| `CONF-XCON-004`| Running Adapter with Stale Health | Sim | Mandatory | Step 39 vs Step 40 | Stale health signal prevents entering new negotiation sessions. | `ERR_XCON_STALE_NEGOTIATION_BLOCK`|
| `CONF-XCON-005`| Matched Capability Disjoint in Negot.| Sim | Mandatory | Step 38 vs Step 39 | Matched candidate rejected in negotiation when parameter bounds disjoint. | `ERR_XCON_NEGOTIATION_PRUNED` |
| `CONF-XCON-006`| Negotiated Feature Lacking Execution | Sim | Mandatory | Step 39 vs Step 40 | Negotiated feature cannot execute without cryptographically valid token. | `CRIT_XCON_UNAUTHORIZED_EXECUTION`|
| `CONF-XCON-007`| Runtime Fault Shifts Context Availability| Sim | Mandatory | Step 39 vs Step 40 | Worker reporting crash in optional hook transitions capability to UNAVAIL. | `WARN_XCON_AVAILABILITY_SHIFT` |
| `CONF-XCON-008`| Obsolete Contract Version Cache Purge| Sim | Mandatory | Step 34 vs Step 41 | Updating SDK contract version invalidates cached conformance results. | `WARN_XCON_CACHE_INVALIDATED` |

---

## 10. CATEGORY 9: SECURITY SANDBOXING, TOKENS & ANTI-CHEAT (`CONF-SEC`)

| Test ID | Test Title | Mode | Criticality | Target Contract | Verification Assertion | Failure Diagnostic |
| :--- | :--- | :---: | :---: | :--- | :--- | :--- |
| `CONF-SEC-001` | Medium Integrity Level Token Check | Dyn | Mandatory | `DEC-002` | Process token MUST NOT have High, System, or Admin SIDs. | `CRIT_SEC_INTEGRITY_ELEVATION` |
| `CONF-SEC-002` | UAC Shield Manifest Rejection | Static | Mandatory | `NLE-REQ-SRA-001` | Executable manifest containing `requireAdministrator` MUST fail admission. | `CRIT_SEC_UAC_MANIFEST_BANNED` |
| `CONF-SEC-003` | Anti-Cheat Driver Detection Block | Sim/Dyn | Mandatory | `BLK-SEC-01` | If kernel anti-cheat active, memory-hooking adapter admission blocked. | `ERR_SEC_ANTI_CHEAT_BLOCK` |
| `CONF-SEC-004` | Child Process Creation Ban | Dyn | Mandatory | `NLE-REQ-SRA-001` | Worker attempting to call `CreateProcessW` killed by Job Object limit. | `CRIT_SEC_CHILD_PROCESS_BLOCKED`|
| `CONF-SEC-005` | 512 MB Working Set Limit Enforcement | Dyn | Mandatory | `NLE-REQ-SRA-001` | Worker allocating $> 512\text{ MB}$ terminated by Job Object memory limit. | `ERR_SEC_MEMORY_LIMIT_EXCEEDED` |
| `CONF-SEC-006` | Filesystem Sandbox to Volume D | Dyn | Mandatory | `DEC-009` | Writes outside `D:\NexLudic\Projects\<id>\Adapters\<id>\` blocked. | `CRIT_SEC_OUT_OF_BOUNDS_WRITE` |
| `CONF-SEC-007` | Network Socket Opening Prohibition | Dyn | Mandatory | `NLE-REQ-SRA-001` | Worker attempting to open outbound TCP/UDP sockets blocked by sandbox. | `CRIT_SEC_NETWORK_ACCESS_BLOCKED`|
| `CONF-SEC-008` | Unauthorized Capability Request Reject| Dyn | Mandatory | `NLE-REQ-SRA-001` | IPC call targeting unconsented capability rejected with 403 Forbidden. | `ERR_SEC_UNAUTHORIZED_CAPABILITY`|
| `CONF-SEC-009` | Real-Time User Consent Revocation | Dyn | Mandatory | `NLE-REQ-SRA-001` | Revoking consent for mandatory capability triggers orderly session teardown.| `WARN_SEC_CONSENT_REVOKED` |
| `CONF-SEC-010` | Kernel Driver Injection Ban | Static | Mandatory | `BLK-SEC-01` | Adapter attempting to install `.sys` drivers rejected at admission. | `CRIT_SEC_KERNEL_DRIVER_BANNED` |

---
*End of Catalog — Document `NLE-CAT-CTC-001`.*
