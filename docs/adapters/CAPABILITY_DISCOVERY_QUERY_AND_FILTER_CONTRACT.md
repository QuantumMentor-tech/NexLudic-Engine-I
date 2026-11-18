# Capability Discovery Query and Filter Contract Specification

**Document Identifier:** `NLE-CON-CDQ-001`  
**Phase:** Phase 5 — Universal Adapter SDK & Capability System  
**Step:** Step 37 of 100 — Validated Capability Declaration Registry & Discovery Workflow Specification  
**Status:** Approved Architectural Contract  
**Classification:** Universal Core API-Neutral Query Contract & Filter Semantics  
**Repository Path:** `docs/adapters/CAPABILITY_DISCOVERY_QUERY_AND_FILTER_CONTRACT.md`  

---

## 1. Executive Summary & Discovery Purpose

### 1.1 Purpose
This specification establishes the formal, implementation-neutral query and filtering contract for discovering registered capability declarations within the NexLudic Engine.

The discovery subsystem allows core platform components—specifically the Cross-Game Compatibility Assessment Pipeline (`docs/game-discovery/`), Adapter Management UI (`SCR-ADM-001`), and Mashup Composition Planner—to locate candidate adapters that declare capabilities relevant to a specific game, version, platform, or mashup objective.

### 1.2 Anti-Deception Invariants
- **Discovery Match Is Not a Compatibility Conclusion:** A query match confirms solely that a registered adapter's declared scope intersects the search criteria. It does **not** establish that the adapter is currently installed, that the game environment is properly configured, that prerequisite mods exist, or that two games can interoperate.
- **Strict Separation of Discovery from Authorization:** Capability discovery is a read-only metadata search. Returning an adapter in discovery results does not authorize the system to start the adapter process or inject hooks.
- **Default Suppression of Inactive State:** Ordinary discovery queries strictly return declarations in `ACTIVE` or `ACTIVE_WITH_LIMITATIONS` states. Records in `PENDING_REVIEW`, `STALE`, `SUPERSEDED`, `WITHDRAWN`, or `REJECTED` states are suppressed by default and accessible only via explicit administrative/audit queries.

---

## 2. Discovery Query Modes

The discovery interface supports three distinct operational modes, each with tailored input parameters, performance budgets, and result scopes:

```mermaid
flowchart TD
    CALLER[Caller / Subsystem] --> MODE{Query Mode}
    MODE -->|Exact Identifier| EXACT[1. Exact Lookup<br>O(1) Record Retrieval]
    MODE -->|Context Criteria| FILTER[2. Filtered Discovery<br>Targeted Candidate Search]
    MODE -->|Broad Catalog| EXPLORE[3. Exploratory Discovery<br>Paginated Catalog Browsing]
    EXACT --> RESULT[DiscoveryResultPayload]
    FILTER --> RESULT
    EXPLORE --> RESULT
```

### 2.1 Mode Definitions

| Query Mode | Description | Primary Consumers | Latency SLA | Result Cardinality |
|:---|:---|:---|:---:|:---:|
| **1. Exact Lookup** | Direct point lookup using unique `registryRecordId` or `(adapterId, manifestSha256)`. | Diagnostic inspector, direct adapter loader pre-flight. | $\le 10\text{ ms}$ | Exactly 0 or 1 record |
| **2. Filtered Discovery** | Context-aware search specifying target game, version scope, required capabilities, and adapter roles. | Cross-Game Assessment Engine, mashup feasibility planner. | $\le 50\text{ ms}$ | $0 \dots N$ candidate matches (capped) |
| **3. Exploratory Discovery** | Broad exploration across capability categories or adapter registries with loose criteria. | Developer catalog explorer, Desktop UI adapter browser. | $\le 250\text{ ms}$ | Strict pagination (default 50, max 200) |

---

## 3. Query Input Contract (`CapabilityDiscoveryQuery`)

### 3.1 JSON Schema Specification (`NLE-SCH-CDQ-001`)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/v1/capability-discovery-query.json",
  "title": "CapabilityDiscoveryQuery",
  "description": "API-neutral input query schema for searching the Validated Capability Declaration Registry.",
  "type": "object",
  "properties": {
    "queryId": { "type": "string", "format": "uuid" },
    "queryMode": {
      "type": "string",
      "enum": ["EXACT_LOOKUP", "FILTERED_DISCOVERY", "EXPLORATORY_DISCOVERY"]
    },
    "exactLookup": {
      "type": "object",
      "properties": {
        "registryRecordId": { "type": "string", "format": "uuid" },
        "adapterId": { "type": "string" },
        "manifestSha256": { "type": "string", "pattern": "^[a-f0-9]{64}$" }
      }
    },
    "contextFilters": {
      "type": "object",
      "properties": {
        "targetGameId": { "type": "string" },
        "targetGameVersion": { "type": "string" },
        "targetPlatform": {
          "type": "string",
          "enum": ["windows-x64", "windows-x86"]
        },
        "environmentProfile": { "type": "string" }
      }
    },
    "capabilityFilters": {
      "type": "object",
      "properties": {
        "capabilityIds": {
          "type": "array",
          "items": { "type": "string" },
          "maxItems": 50
        },
        "category": {
          "type": "string",
          "enum": [
            "core.state",
            "core.events",
            "world.environment",
            "world.spatial",
            "world.physics",
            "entities.actors",
            "entities.inventory",
            "ui.hud",
            "audio.sfx",
            "network.session"
          ]
        },
        "matchAllCapabilities": { "type": "boolean", "default": false },
        "minimumVerificationState": {
          "type": "string",
          "enum": [
            "DECLARED_ONLY",
            "STRUCTURALLY_CONFORMANT",
            "CONTRACT_VERIFIED",
            "EVIDENCED_COMPATIBLE",
            "EMPIRICALLY_VALIDATED"
          ]
        }
      }
    },
    "adapterFilters": {
      "type": "object",
      "properties": {
        "adapterIds": { "type": "array", "items": { "type": "string" } },
        "adapterRole": {
          "type": "string",
          "enum": ["SOURCE", "TARGET", "BIDIRECTIONAL"]
        },
        "integrationModes": {
          "type": "array",
          "items": {
            "type": "string",
            "enum": ["HOST_MOD", "LIVE_BRIDGE", "HYBRID"]
          }
        }
      }
    },
    "lifecycleFilter": {
      "type": "object",
      "properties": {
        "allowedStates": {
          "type": "array",
          "items": {
            "type": "string",
            "enum": [
              "ACTIVE",
              "ACTIVE_WITH_LIMITATIONS",
              "PENDING_REVIEW",
              "SUSPENDED",
              "STALE",
              "SUPERSEDED",
              "WITHDRAWN",
              "RETIRED",
              "REJECTED"
            ]
          },
          "default": ["ACTIVE", "ACTIVE_WITH_LIMITATIONS"]
        },
        "includeHistorical": { "type": "boolean", "default": false }
      }
    },
    "pagination": {
      "type": "object",
      "properties": {
        "limit": { "type": "integer", "minimum": 1, "maximum": 200, "default": 50 },
        "offset": { "type": "integer", "minimum": 0, "default": 0 },
        "sortBy": {
          "type": "string",
          "enum": ["RELEVANCE", "ADAPTER_ID", "REGISTERED_AT", "VERIFICATION_STATE"],
          "default": "RELEVANCE"
        }
      }
    }
  },
  "required": ["queryId", "queryMode"]
}
```

---

## 4. Contextual Scope Resolution & Semantics

When evaluating a `CapabilityDiscoveryQuery`, the registry applies deterministic scope resolution rules aligned with Step 28 (`Game Version & Environment Mapping`):

### 4.1 Game Version Scope Matching
1. **SemVer Range Intersection:** If `targetGameVersion` is supplied (e.g., `"1.6.640"`), the engine evaluates:
   $$\text{Match} = \left(\texttt{minVersion} \le \texttt{targetGameVersion} \le \texttt{maxVersion}\right) \land \left(\texttt{targetGameVersion} \notin \texttt{excludedVersions}\right)$$
2. **Wildcard & Unspecified Scope:** If the caller provides no `targetGameVersion`, declarations targeting any version of the specified `targetGameId` are matched, but returned with `versionScopeQualifier: "UNBOUNDED"`.
3. **Non-Comparable Versions:** If the target game uses non-SemVer build IDs (e.g., Unity build hashes), exact hash equality is required per Step 28. Cross-version interpolation is prohibited.

### 4.2 Multi-Capability Matching (`matchAllCapabilities`)
- When `matchAllCapabilities: false` (default / OR search): Any declaration that matches **at least one** of the requested `capabilityIds` is included.
- When `matchAllCapabilities: true` (AND search): Only declarations whose manifest asserts **all** requested capability IDs within a single adapter are returned.

### 4.3 Verification State Floor Filtering (`minimumVerificationState`)
- Callers may require a minimum confidence floor. For example, setting `minimumVerificationState: "EVIDENCED_COMPATIBLE"` automatically filters out any declaration where the capability verdict is `DECLARED_ONLY` or `STRUCTURALLY_CONFORMANT` (i.e., unevidenced claims).

---

## 5. Security, Input Sanitization & Anti-DoS Protections

To protect the registry from hostile queries or resource exhaustion:
1. **Array Length Caps:** `capabilityIds` capped at 50; `adapterIds` capped at 20. Oversized arrays are rejected with `ERR_QUERY_FILTER_LIMIT_EXCEEDED`.
2. **Execution Timeout:** All discovery queries enforce a strict timeout of **250 milliseconds**. If a broad exploratory search exceeds this ceiling, execution halts and returns a partial result payload flagged `isTruncated: true`.
3. **Query Parameter Sanitization:** String filters are sanitized against regex injection and path traversal tokens (`..`, `/`, `\`).
4. **Authorization Isolation:** Ordinary non-admin callers requesting restricted states (`PENDING_REVIEW`, `REJECTED`, or `audit: true`) receive an `ERR_UNAUTHORIZED_LIFECYCLE_QUERY` exception.

---

## 6. Traceability and Cross-Reference Anchors

- `NLE-SPC-VCR-001`: Primary Registry Specification.
- `NLE-MOD-RRM-001`: Registry Record Data Schema.
- `NLE-POL-REL-001`: Lifecycle Policy and State Enumeration.
- `NLE-MOD-CCT-001`: Canonical Capability Taxonomy.
- `docs/game-discovery/`: Step 28 Game Version & Environment Mapping rules.
- `SCR-ADM-001`, `SCR-CMP-001`: Desktop UI Adapter and Compatibility Views.
