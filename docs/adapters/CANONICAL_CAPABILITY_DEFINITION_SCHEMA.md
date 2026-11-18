# Canonical Capability Definition Schema

**Document ID:** `NLE-SCH-CCD-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 35 of 100 — Canonical Capability Taxonomy & Declaration Schema Specification  
**Classification:** Canonical Contract Schema & Semantic Definition Specification  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-MOD-CCT-001`, `NLE-CAT-DRC-001`, `NLE-CAT-RIA-001`, `NLE-DOM-CAP-001`

---

## 1. Specification Scope & Authority

This document defines the formal schema and field dictionary for **Canonical Capability Definitions (`CCD`)**. 

Canonical capability definitions are maintained exclusively in the universal platform catalog. They establish the precise operational semantics, schema contracts, side-effect rules, and security constraints that govern a recognized capability. They are completely decoupled from any single game title or programming language.

---

## 2. Canonical Capability Definition Schema (`canonical-capability.json`)

```json
{
  "$schema": "https://schemas.nexludic.org/v1/canonical-capability.json",
  "capabilityId": "CAP-EXT-HOOK-DETOUR-001",
  "canonicalName": "Native C++ Function Detour Injection",
  "shortDescription": "Injects native machine-code detours into target game binary routines via an approved mod framework.",
  "definitionMarkdown": "Allows an adapter to detour specified native engine functions to intercept parameters, modify execution paths, or observe state transitions.",
  "taxonomyCategory": "CAT-EXTENSION",
  "assessmentDimension": "MEMORY_HOOKING",
  "contractRevision": 1,
  "lifecycleStatus": "ACTIVE",
  "scopeAndApplicability": {
    "intendedAdapterRoles": ["HOST_MOD_SCAFFOLD", "HYBRID_COORDINATOR"],
    "applicableStrategies": ["STRATEGY_A_HOST_MOD", "STRATEGY_C_HYBRID"],
    "targetPlatformConstraints": ["windows-x64", "windows-x86"],
    "runtimePrerequisites": ["Approved_Native_Mod_Loader"],
    "exclusions": ["Games_With_Active_Kernel_AntiCheat"]
  },
  "operationalSemantics": {
    "behaviorSpecification": "Executes memory patch or detours target symbol; registers trampoline for original function invocation.",
    "inputSchema": {
      "type": "object",
      "required": ["targetSymbol", "detourPayloadPath"],
      "properties": {
        "targetSymbol": { "type": "string" },
        "detourPayloadPath": { "type": "string" },
        "executionPhase": { "type": "string", "enum": ["PRE_EXECUTION", "POST_EXECUTION", "REPLACE"] }
      }
    },
    "outputSchema": {
      "type": "object",
      "required": ["hookStatus", "trampolineAddress"],
      "properties": {
        "hookStatus": { "type": "string", "enum": ["HOOK_ATTACHED", "HOOK_FAILED"] },
        "trampolineAddress": { "type": "string" }
      }
    },
    "preconditions": [
      "Target process must be running in standard user space (Medium IL).",
      "Target game must not load kernel anti-cheat drivers (BLK-SEC-01 check passed)."
    ],
    "postconditions": [
      "Target symbol invokes detour payload upon invocation.",
      "Original function remains callable via trampoline."
    ],
    "invariants": [
      "Host game process thread affinity must be preserved.",
      "Stack alignment must conform to x64 calling conventions."
    ],
    "sideEffects": "EPHEMERAL_MEMORY",
    "failureModes": [
      "Rejected_PreconditionFail",
      "Failed_BeforeSideEffects",
      "Fatal_ProcessCrash"
    ],
    "idempotency": "STRICT"
  },
  "constraintsAndNonGuarantees": {
    "knownLimitations": [
      "Function detours are invalidated if the game executable is patched to a new build hash."
    ],
    "unsupportedModes": ["Multiplayer_AntiCheat_Protected_Modes"],
    "requiredPermissions": {
      "processElevation": "None",
      "fileSystem": "ScopedProjectStaging",
      "processMemory": "LoopbackSharedMemory"
    },
    "resourceConstraints": {
      "maxExecutionTimeMs": 5000,
      "maxMemoryBytes": 67108864
    },
    "explicitNonGuarantees": [
      "Does NOT guarantee detour stability across undocumented game patches.",
      "Does NOT guarantee thread-safety inside concurrent game engine tick loops."
    ]
  },
  "evidenceAndReferences": {
    "evidenceCitations": ["EVD-8021-SKSE-HEADERS"],
    "sourceDocumentation": "https://docs.nexludic.org/capabilities/CAP-EXT-HOOK-DETOUR-001.md",
    "rationale": "Required for Strategy A host-game modifications where native logic replacement is necessary."
  }
}
```

---

## 3. Field Dictionary & Semantic Validation Rules

### 3.1 Identity and Lifecycle Fields

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `capabilityId` | `String` | **MANDATORY** | Matches `^CAP-[A-Z]{3,5}-[A-Z0-9-]+-[0-9]{3}$`. Globally unique and immutable. |
| `canonicalName` | `String` | **MANDATORY** | Clear, unbranded title (5–80 chars). E.g., "Skeletal Mesh Transcoding". |
| `shortDescription` | `String` | **MANDATORY** | Summary text (10–250 chars). Displayed in tooltips and report cards. |
| `definitionMarkdown` | `Markdown` | **MANDATORY** | Full technical specification of the contract and operations. |
| `taxonomyCategory` | `Enum` | **MANDATORY** | Must be one of the 10 canonical categories from `NLE-MOD-CCT-001`. |
| `assessmentDimension`| `Enum` | **MANDATORY** | Must be one of the 8 approved dimensions from `NLE-CAT-DRC-001`. |
| `contractRevision` | `Integer` | **MANDATORY** | Positive integer starting at 1. Incremented on non-breaking schema evolution. |
| `lifecycleStatus` | `Enum` | **MANDATORY** | `DRAFT`, `ACTIVE`, `DEPRECATED`, or `RETIRED`. |

### 3.2 Scope & Applicability Fields

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `intendedAdapterRoles` | `List<Enum>` | **MANDATORY** | Subset of 5 approved roles (`HOST_MOD_SCAFFOLD`, `LIVE_BRIDGE_TELEMETRY`, etc.). |
| `applicableStrategies` | `List<Enum>` | **MANDATORY** | One or more: `STRATEGY_A_HOST_MOD`, `STRATEGY_B_LIVE_BRIDGE`, `STRATEGY_C_HYBRID`. |
| `targetPlatformConstraints`| `List<String>`| **MANDATORY** | Supported target platforms (e.g. `windows-x64`). |
| `runtimePrerequisites` | `List<String>` | OPTIONAL | Abstract prerequisites (e.g., `Approved_Native_Mod_Loader`). |
| `exclusions` | `List<String>` | OPTIONAL | Conditions that render capability inapplicable (e.g., `Kernel_AntiCheat`). |

### 3.3 Operational Semantics Fields

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `behaviorSpecification`| `String` | **MANDATORY** | Plain-language statement of operational guarantees and algorithm behavior. |
| `inputSchema` | `JSON Schema`| **MANDATORY** | Valid draft-07 JSON Schema defining required parameter types and bounds. |
| `outputSchema` | `JSON Schema`| **MANDATORY** | Valid draft-07 JSON Schema defining return payload structure. |
| `preconditions` | `List<String>` | **MANDATORY** | Minimum system/game state required before invocation can begin. |
| `postconditions` | `List<String>` | **MANDATORY** | Guaranteed system/game state upon `Completed_Success` outcome. |
| `invariants` | `List<String>` | **MANDATORY** | Properties that remain true throughout execution. |
| `sideEffects` | `Enum` | **MANDATORY** | One of: `NONE`, `READ_ONLY`, `EPHEMERAL_MEMORY`, `PERSISTENT_STAGING`, `GAME_STATE_MUTATION`. |
| `failureModes` | `List<Enum>` | **MANDATORY** | Valid subset of codes from the **12-Outcome Invocation Taxonomy** (`NLE-SPC-ATF-001`). |
| `idempotency` | `Enum` | **MANDATORY** | `STRICT` (safe to repeat), `AT_LEAST_ONCE`, or `NON_IDEMPOTENT` (mutation; non-retryable). |

### 3.4 Constraints and Explicit Non-Guarantees

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `knownLimitations` | `List<String>` | **MANDATORY** | Explicit technical constraints and boundaries. |
| `unsupportedModes` | `List<String>` | **MANDATORY** | Modes known to fail or be prohibited. |
| `requiredPermissions` | `Object` | **MANDATORY** | Minimum permissions required (Medium IL, Scoped Staging). |
| `resourceConstraints` | `Object` | OPTIONAL | Max execution time (`maxExecutionTimeMs`) and memory limits (`maxMemoryBytes`). |
| `explicitNonGuarantees`| `List<String>` | **MANDATORY** | Explicit statements of what the capability does **NOT** promise. Anti-deception guard. |

---

## 4. Side-Effect Taxonomy & Invariants

Every canonical capability must declare its side-effect category to ensure predictable rollback and execution isolation:

```
+----------------------------------------------------------------------------------------------------+
|                                    SIDE-EFFECT TAXONOMY                                            |
+----+-----------------------------+-----------------------------------------------------------------+
| #  | Side-Effect Enum            | Operational Behavior & System Isolation                         |
+----+-----------------------------+-----------------------------------------------------------------+
| 01 | `NONE`                      | Pure mathematical calculation or in-memory string formatting.   |
| 02 | `READ_ONLY`                 | Reads files or memory; zero disk writes, zero memory mutation.  |
| 03 | `EPHEMERAL_MEMORY`          | Hooks or allocates volatile memory; destroyed on process exit.  |
| 04 | `PERSISTENT_STAGING`        | Writes files strictly to Volume D project staging directory.    |
| 05 | `GAME_STATE_MUTATION`       | Modifies game save data or game directory files (Guarded).      |
+----+-----------------------------+-----------------------------------------------------------------+
```

### Safety Invariant:
Capabilities declaring `GAME_STATE_MUTATION` are strictly prohibited during initial discovery and compatibility assessment. They can only be invoked during active project builds after explicit user confirmation (`CMP-045`).

