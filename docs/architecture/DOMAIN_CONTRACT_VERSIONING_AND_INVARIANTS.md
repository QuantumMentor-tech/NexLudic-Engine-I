# NexLudic Engine — Domain Contract Versioning and Invariants

**Document ID:** `NLE-STD-INV-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 11 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Architecture  

---

## 1. Executive Summary & Purpose

The **Domain Contract Versioning and Invariants** specification establishes the cross-cutting identifier policies, semantic versioning rules, business integrity invariants, security constraints, and error taxonomies governing all contracts in **NexLudic Engine**.

This document ensures that as the platform expands to support dozens of game adapters, build systems, and modding frameworks, interfaces evolve predictably without breaking existing mashups, corrupting user game files, or introducing security vulnerabilities.

---

## 2. Standardized Cross-Cutting Identifier Policy

To eliminate cross-project collisions and ensure deterministic tracing, all domain entities must utilize globally unique, prefixed identifiers:

```
+----------------------------------------------------------------------------------------------------+
|                                    STANDARDIZED IDENTIFIER PREFIXES                                |
+--------+--------------------------+----------------------------+-----------------------------------+
| Prefix | Entity / Concept         | Format Structure           | Canonical Example                 |
+--------+--------------------------+----------------------------+-----------------------------------+
| `PROJ-`| Game-Pair Project        | `PROJ-<slug>`              | `PROJ-mcjava-rdr2-western`        |
| `GAME-`| Target Game Identity     | `GAME-<slug>`              | `GAME-minecraft-java`             |
| `CAP-` | Capability Contract      | `CAP-<game>-<feature>-<NNN>`| `CAP-MC-DEADEYE-001`              |
| `ADPT-`| Adapter Identifier       | `ADPT-<game>-<ecosystem>`  | `ADPT-mcjava-fabric`              |
| `EVD-` | Evidence Record          | `EVD-<target>-<slug>-<NNN>`| `EVD-MC-TICKDILATE-001`           |
| `CLM-` | Compatibility Claim      | `CLM-<pair>-<NNN>`         | `CLM-MC-RDR2-001`                 |
| `GATE-`| Quality Gate             | `GATE-G<N>`                | `GATE-G3`                         |
| `TEST-`| Test Specification       | `TEST-<cap_id>-<NNN>`      | `TEST-CAP-MC-DEADEYE-001`         |
| `TSK-` | AI Agent Task            | `TSK-<proj>-<GUID>`        | `TSK-MC-7b3f-4e92-91a1`           |
| `APPR-`| Execution Approval       | `APPR-<proj>-<hash>`       | `APPR-MC-e3b0c442`                |
| `ART-` | Staged Artifact Record   | `ART-<file>-<SHA256_8>`    | `ART-weapon_mod_jar-a1b2c3d4`     |
| `CORR-`| Distributed Tracing ID   | `CORR-<UUID>`              | `CORR-4f9e8a7b-1234-5678`         |
+--------+--------------------------+----------------------------+-----------------------------------+
```

---

## 3. Contract Versioning & Evolution Policy

All platform contracts (Domain models, Adapter manifests, IPC Named Pipe schemas) adhere to **Semantic Versioning (`MAJOR.MINOR.PATCH`)**:

```
+----------------------------------------------------------------------------------------------------+
|                                    SEMANTIC VERSIONING GOVERNANCE                                  |
+---------------------+-------------------+---------------------+------------------------------------+
| Change Category     | Version Delta     | Permitted Actions   | Backward Compatibility Mandate     |
+---------------------+-------------------+---------------------+------------------------------------+
| **Additive Change** | `PATCH` increment | Adding optional     | 100% backward compatible; older   |
|                     | (`1.0.0` -> `1.0.1`)| fields, comments.   | adapters parse without error.      |
+---------------------+-------------------+---------------------+------------------------------------+
| **Feature Extension**| `MINOR` increment| Adding new traits,  | Backward compatible; older core    |
|                     | (`1.0.0` -> `1.1.0`)| new capabilities.   | ignores unknown new fields.        |
+---------------------+-------------------+---------------------+------------------------------------+
| **Breaking Change** | `MAJOR` increment | Altering required   | Breaking. Requires ARB approval &  |
|                     | (`1.0.0` -> `2.0.0`)| fields, wire layout.| 1-year deprecation window.         |
+---------------------+-------------------+---------------------+------------------------------------+
```

### Invariant Wire Rules:
1. **Additive Field Rule:** In Protocol Buffer schemas (`proto3`), field numbers cannot be modified or re-used. Removed fields must be marked `reserved`.
2. **Schema Rejection:** If an adapter presents an unknown `MAJOR` contract version, the core engine rejects the descriptor and refuses initialization.

---

## 4. Master Domain Invariants Catalog

The implementation must enforce the following non-negotiable invariants:

### 4.1 Identity & Data Integrity Invariants
- **`INV-ID-01` (Unique Slug Invariant):** No two projects may share the same `ProjectSlug`.
- **`INV-ID-02` (Immutable Hash Invariant):** Once a `GameSignature` is verified, its `SHA256` hash is immutable. If the file on disk changes, it is treated as a new unverified version.
- **`INV-DAT-01` (Single Authoritative Owner):** Derived reports and UI views cannot mutate authoritative entities. All mutations pass through domain commands.

### 4.2 Security & Authorization Invariants
- **`INV-SEC-01` (Zero Elevation Invariant):** The platform rejects any project, adapter, or toolchain configuration claiming to require elevated administrative privileges (`REQ-SEC-001`).
- **`INV-SEC-02` (Mandatory Diff Approval Invariant):** No file from `<project>/staging/` may be copied to game directories without an active, human-confirmed `ExecutionApproval` (`REQ-F-009`).
- **`INV-SEC-03` (Anti-Cheat Hard Block Invariant):** Any game process identified as running kernel anti-cheat is permanently prohibited from live memory hooking or injection (`REQ-SAFE-001`).

### 4.3 Lifecycle & Rollback Invariants
- **`INV-LIF-01` (Rollback Registration Invariant):** Deployment of an integration cannot begin until an automated `cleanup.ps1` script has been generated and validated (`REQ-F-010`).
- **`INV-LIF-02` (Pristine Hash Invariant):** System rollback is certified successful only when post-teardown file hashes match pre-test baselines byte-for-byte (`REQ-NF-006`).

---

## 5. Standardized Error and Outcome Taxonomy

To prevent ambiguous error reporting, all platform operations return errors mapped to eighteen standardized outcome categories:

```
+----------------------------------------------------------------------------------------------------+
|                                    18-CATEGORY ERROR TAXONOMY                                      |
+----+-----------------------------+-------------------+--------------------+------------------------+
| #  | Error Category              | Recoverable?      | Retry Permitted?   | Typical Trigger        |
+----+-----------------------------+-------------------+--------------------+------------------------+
| 01 | `Invalid_Input`             | Yes (User edit)   | Yes (Immediate)    | Malformed path / name. |
| 02 | `Invalid_Contract`          | Yes (Fix schema)  | No (Code defect)   | Missing schema field.  |
| 03 | `Unsupported_Capability`    | No (Scoped out)   | No (Unfeasible)    | 3D mesh injection.     |
| 04 | `Incompatible_Version`      | Yes (Downgrade)   | No (Patch needed)  | Game hash drift.       |
| 05 | `Missing_Dependency`        | Yes (Install SDK) | Yes (Post-install) | JDK 21 missing.        |
| 06 | `Permission_Denied`         | Yes (Fix ACLs)    | No (Policy block)  | File lock / no write.  |
| 07 | `Approval_Required`         | Yes (User prompt) | Yes (Post-approval)| Staged diff unapproved.|
| 08 | `Policy_Rejected`           | No (Permanent)    | No (Hard block)    | VAC / EAC anti-cheat.  |
| 09 | `External_System_Unavail`   | Yes (Transient)   | Yes (Exponential)  | LLM API rate limit.    |
| 10 | `Execution_Failed`          | Yes (Refine code) | Yes (Post-fix)     | Gradle build failure.  |
| 11 | `Partial_Completion`        | Yes (Partial OK)  | No (Deterministic) | 3/5 caps succeeded.    |
| 12 | `Cancelled_By_User`         | Yes (Normal abort)| No                 | User clicks cancel.    |
| 13 | `Outcome_Unknown`           | No (Manual audit) | **NO (Forbidden)** | Bridge timeout.        |
| 14 | `Validation_Failed`         | Yes (Tuning)      | Yes (Post-tune)    | Frame drop > 2 FPS.    |
| 15 | `Evidence_Insufficient`     | Yes (Run test)    | Yes (Run PoC)      | Tier 1 hypothesized.   |
| 16 | `Recovery_Required`         | Yes (Rollback)    | No (Run cleanup)   | Partial write crash.   |
| 17 | `Deprecated_Integration`    | No (Superseded)   | No (Upgrade)       | Loader retired.        |
| 18 | `Internal_Defect`           | No (Bug report)   | No                 | Null reference bug.    |
+----+-----------------------------+-------------------+--------------------+------------------------+
```

---

*End of Domain Contract Versioning and Invariants (NLE-STD-INV-001).*

