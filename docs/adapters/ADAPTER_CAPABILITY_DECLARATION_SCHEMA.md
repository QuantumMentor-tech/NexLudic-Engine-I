# Adapter Capability Declaration Schema

**Document ID:** `NLE-SCH-ACD-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 35 of 100 — Canonical Capability Taxonomy & Declaration Schema Specification  
**Classification:** Adapter Manifest Schema & Declaration Contract Specification  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-MOD-CCT-001`, `NLE-SCH-CCD-001`, `NLE-CAT-ACM-001`, `NLE-CAT-RIA-001`

---

## 1. Specification Scope & Boundary

This document defines the formal schema and field dictionary for **Adapter Capability Declarations (`ACD`)**. 

An Adapter Capability Declaration is the concrete assertion embedded within an adapter’s `adapter.json` descriptor manifest. While a **Canonical Capability Definition** (`CCD`) defines what a capability means across the universe of games, an **Adapter Capability Declaration** (`ACD`) defines what a specific adapter claims it can deliver for an identified target game, build range, and execution context.

> [!WARNING]
> **Anti-Deception Rule:**  
> An Adapter Capability Declaration is an **author claim**, NOT an empirical proof. Publishing a valid declaration does NOT automatically certify that the capability functions on the user's computer or that it satisfies integration criteria.

---

## 2. Declaration Schema Specification (`capability-declaration.json`)

```json
{
  "$schema": "https://schemas.nexludic.org/v1/capability-declaration.json",
  "declarationId": "decl-skyrimse-skse-detour-01",
  "adapterId": "adapter-skyrimse-skse-host",
  "adapterVersion": "1.2.0",
  "contractVersion": "1.0.0",
  "capabilityId": "CAP-EXT-HOOK-DETOUR-001",
  "capabilityRevision": 1,
  "declarationRevision": 1,
  "targetContext": {
    "canonicalGameId": "game-tesv-skyrimse",
    "targetPlatform": "windows-x64",
    "supportedVersionRanges": [
      {
        "minimumVersion": "1.5.97",
        "maximumVersion": "1.6.640",
        "testedBuildHash": "sha256:7f83b1657ff1fc53b92dc18148a1d65dfc2d4b1fa3d677284b"
      }
    ]
  },
  "supportScope": {
    "declaredSupportLevel": "SUPPORTED",
    "partialScopeDescription": null,
    "operationalConditions": [
      "Target game must be started via skse64_loader.exe",
      "Address Library for SKSE Plugins must be installed for runtime offset resolution"
    ]
  },
  "dependencies": [
    {
      "dependencyType": "HARD_PREREQUISITE",
      "targetCapabilityId": "CAP-DISC-VERSION-MANIFEST-001",
      "conditionDescription": "Must detect executable patch level prior to detouring vtable entries."
    },
    {
      "dependencyType": "CONDITIONAL_PREREQUISITE",
      "targetCapabilityId": "CAP-EXT-ADDR-LIBRARY-001",
      "conditionDescription": "Required for game builds >= 1.6.0 (Anniversary Edition)."
    }
  ],
  "resourceFootprint": {
    "allocatedMemoryBytes": 16777216,
    "maxTimeoutMs": 3000
  },
  "provenanceAndEvidence": {
    "evidenceReferences": ["EVD-8021-SKSE-HEADERS", "EVD-8044-DETOUR-BENCHMARK"],
    "technicalNotes": "Uses CommonLibSSE detour trampolines allocating from 2GB memory boundary.",
    "knownLimitations": [
      "Detour injection fails if another active mod injects incompatible trampolines at identical offsets."
    ]
  }
}
```

---

## 3. Declaration Field Dictionary & Requiredness Contracts

### 3.1 Declaration Identity & Version References

| Field Path | Data Type | Requiredness | Validation Constraints | Authoritative Source |
|:---|:---|:---:|:---|:---|
| `declarationId` | `String` | **MANDATORY** | Unique key within adapter scope (`^[a-z0-9-]+$`). | Adapter Author |
| `adapterId` | `String` | **MANDATORY** | Must match root `adapterId` in `adapter.json`. | Adapter Descriptor |
| `adapterVersion` | `SemVer` | **MANDATORY** | Must match root `version` in `adapter.json`. | Adapter Descriptor |
| `contractVersion` | `SemVer` | **MANDATORY** | Must match root `contractVersion` in `adapter.json`. | Universal SDK |
| `capabilityId` | `String` | **MANDATORY** | Must match an existing canonical `CapabilityId` or accepted `PROPOSED:` ID. | Canonical Catalog |
| `capabilityRevision`| `Integer`| **MANDATORY** | Revision of canonical capability targeted ($\ge 1$). | Canonical Catalog |
| `declarationRevision`| `Integer`| **MANDATORY** | Incremental revision of this specific declaration ($\ge 1$). | Adapter Author |

### 3.2 Target Context & Version Scope

| Field Path | Data Type | Requiredness | Validation Constraints | Authoritative Source |
|:---|:---|:---:|:---|:---|
| `targetContext.canonicalGameId` | `String` | **MANDATORY** | Must resolve to valid `CanonicalGameIdentity` (`Step 25`). | Universal Catalog |
| `targetContext.targetPlatform` | `String` | **MANDATORY** | `windows-x64` or `windows-x86`. | SDK Architecture |
| `targetContext.supportedVersionRanges`| `List<Range>`| **MANDATORY** | Array of version bounds with `minimumVersion` and `maximumVersion`. | Adapter Author |
| `targetContext.supportedVersionRanges[].testedBuildHash`| `String`| OPTIONAL | Truncated hex `sha256:...` of tested binary build. | Local Verification |

### 3.3 Support Scope & Declared Status

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `supportScope.declaredSupportLevel` | `Enum` | **MANDATORY** | Must be: `SUPPORTED`, `CONDITIONALLY_SUPPORTED`, or `UNSUPPORTED`. |
| `supportScope.partialScopeDescription`| `String` | CONDITIONAL | **MANDATORY** if `declaredSupportLevel == "CONDITIONALLY_SUPPORTED"`. Max 300 chars. |
| `supportScope.operationalConditions` | `List<String>`| OPTIONAL | Explicit operational conditions required for execution. |

### 3.4 Dependencies & Prerequisites

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `dependencies` | `List<Object>` | OPTIONAL | Array of dependency objects. Empty if standalone capability. |
| `dependencies[].dependencyType` | `Enum` | **MANDATORY** | One of: `HARD_PREREQUISITE`, `OPTIONAL_ENHANCEMENT`, `CONDITIONAL_PREREQUISITE`, `ALTERNATIVE_PATH`, `MUTUALLY_EXCLUSIVE`. |
| `dependencies[].targetCapabilityId`| `String` | **MANDATORY** | Valid `CapabilityId` of dependent capability. |
| `dependencies[].conditionDescription`| `String`| CONDITIONAL | **MANDATORY** if `dependencyType == "CONDITIONAL_PREREQUISITE"`. |

### 3.5 Provenance & Evidence Citations

| Field Path | Data Type | Requiredness | Validation Constraints |
|:---|:---|:---:|:---|
| `provenanceAndEvidence.evidenceReferences`| `List<String>`| OPTIONAL | List of valid evidence record keys (`EVD-xxxx`) in catalog. |
| `provenanceAndEvidence.technicalNotes` | `String` | OPTIONAL | Implementation notes explaining the game-specific mechanism. |
| `provenanceAndEvidence.knownLimitations`| `List<String>`| **MANDATORY** | Explicit disclosure of known bugs, conflicts, or game limits. |

---

## 4. Declared Support Levels vs. Assessment Mappings

The declared support level represents an adapter's self-assessment. It maps deterministically into the Phase 4 rule evaluation pipeline:

```
+----------------------------------------------------------------------------------------------------+
|                               SUPPORT LEVEL MAPPING TO RULE INPUTS                                 |
+----+-----------------------------+-------------------------------+---------------------------------+
| #  | Adapter Declared Level      | Rule Input State (NLE-CAT-RIA)| Report Display Treatment        |
+----+-----------------------------+-------------------------------+---------------------------------+
| 01 | `SUPPORTED`                 | `INPUT_CONFIRMED_VERIFIED`    | Evaluated against evidence;     |
|    |                             | (If backed by Tier 1/2 EVD)   | displays green check if verified|
+----+-----------------------------+-------------------------------+---------------------------------+
| 02 | `CONDITIONALLY_SUPPORTED`   | `INPUT_CONFIRMED_PARTIAL`     | Requires prerequisite checks;   |
|    |                             |                               | displays amber conditional icon |
+----+-----------------------------+-------------------------------+---------------------------------+
| 03 | `UNSUPPORTED`               | `INPUT_UNSUPPORTED`           | Evaluated as unfeasible;        |
|    |                             |                               | displays red X-circle icon      |
+----+-----------------------------+-------------------------------+---------------------------------+
```

### Anti-Deception Invariant:
If an adapter declares `SUPPORTED` but provides **zero supporting evidence records** (`evidenceReferences` is empty), the rule engine downgrades the input state to `INPUT_UNKNOWN` (`COVERAGE_UNRESOLVED_UNKNOWN`). A bare declaration without evidence cannot satisfy a mandatory requirement.

