# CONFORMANCE VERSION COMPATIBILITY AND RULE GOVERNANCE
**Document ID:** `NLE-POL-CVG-001`  
**Version:** 1.0.0  
**Phase:** Phase 5 (Universal Adapter SDK & Capability System)  
**Step:** Step 41 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Software Architect, API Contract Designer, Platform Governance Lead, Testing Architect  
**Classification:** Versioning & Governance Policy  

---

## 1. PURPOSE AND GOVERNANCE SCOPE

### 1.1 Purpose
This policy establishes the formal versioning relationships, compatibility matrices, deprecation schedules, rule evolution standards, and result invalidation criteria governing the NexLudic Universal Adapter SDK Conformance Test Harness (`CMP-41`).

As NexLudic Engine evolves, canonical capability taxonomies, wire protocols, and validation schemas will advance. This specification guarantees that:
1. Version relationships across the 7 interdependent contract dimensions remain predictable and strictly defined.
2. Older compliant adapters are not abruptly broken by non-breaking platform updates.
3. Test findings and registry eligibility are deterministically re-evaluated when breaking rule changes occur.

---

## 2. THE MULTI-TIER VERSION COMPATIBILITY MATRIX

The NexLudic platform tracks 7 distinct version dimensions:

```
+--------------------------------------------------------------------------------------------------------+
|                                    PLATFORM VERSION DIMENSIONS                                         |
|                                                                                                        |
|  [ V1 ] Adapter Release Version             - Declared by adapter author (SemVer 2.0.0)                |
|  [ V2 ] Target SDK Contract Version         - SDK wire protocol & RPC baseline (e.g. 1.0.0)            |
|  [ V3 ] Capability Declaration Schema Ver   - Structural ACD JSON Schema (NLE-SCH-ACD-001)            |
|  [ V4 ] Canonical Capability Catalog Ver    - Taxonomy identifiers & parameter specs (NLE-MOD-CCT-001) |
|  [ V5 ] Conformance Rule Catalog Version    - Assertion catalog & diagnostic rules (NLE-CAT-CTC-001)   |
|  [ V6 ] Conformance Test Harness Engine Ver - Test coordinator & runner executable                     |
|  [ V7 ] Conformance Report Schema Version   - Machine-readable result schema (NLE-SCH-CRR-001)         |
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 Compatibility Matrix (Phase 5 Baseline)

| Component Dimension | Version Baseline | Supported Range | Breaking Change Trigger |
| :--- | :---: | :---: | :--- |
| **Adapter Release Version** | `1.0.0` | Any valid SemVer | Any modification to binary code |
| **SDK Contract Version** | `1.0.0` | `^1.0.0` | Changing JSON-RPC method signatures |
| **Capability Schema Version** | `1.0.0` | `^1.0.0` | Modifying required descriptor fields |
| **Canonical Taxonomy Version**| `1.0.0` | `^1.0.0` | Modifying canonical capability IDs |
| **Rule Catalog Version** | `1.0.0` | `^1.0.0` | Adding or altering mandatory assertions |
| **Harness Engine Version** | `1.0.0` | `^1.0.0` | Internal execution architecture change |
| **Report Schema Version** | `1.0.0` | `^1.0.0` | Adding mandatory report fields |

---

## 3. SEMANTIC VERSIONING RULES FOR RULES & CONTRACTS

Changes to the Conformance Rule Catalog (`NLE-CAT-CTC-001`) follow strict SemVer 2.0.0 semantics:
1. **MAJOR Version Bump ($X.0.0$):**
   - Adding a new mandatory test case that can cause previously conformant adapters to fail.
   - Changing the pass/fail threshold or assertion logic of an existing mandatory test.
   - Removing support for an older SDK contract version or schema version.
2. **MINOR Version Bump ($X.Y.0$):**
   - Adding new advisory (`WARN`) tests or capability-specific checks for newly added capabilities.
   - Adding new synthetic test fixtures.
   - Expanding diagnostic error messages or remediation guidance.
3. **PATCH Version Bump ($X.Y.Z$):**
   - Bug fixes in harness parsing, internal formatting, or test double execution.
   - Non-functional documentation updates.

---

## 4. DEPRECATION LIFECYCLE AND SUNSET POLICY

To give adapter developers adequate time to adapt to evolving platform requirements:
1. **Deprecation Notice (Phase 1):** When a capability, parameter, or wire method is marked deprecated, the Rule Catalog updates the corresponding test to severity `WARN` (`WARN_CAP_DEPRECATED_REFERENCE`). The test remains advisory for a minimum of **two minor releases** (e.g. from v1.2.0 through v1.4.0).
2. **Mandatory Grace Period (Phase 2):** Notice is prominently featured in developer conformance reports with actionable migration links.
3. **Removal / Sunset (Phase 3):** At the next **MAJOR** version boundary, the deprecated feature is removed from the canonical catalog, and referencing it triggers a mandatory `FAIL` (`ERR_CAP_UNRECOGNIZED_ID`).

---

## 5. RESULT INVALIDATION AND RE-EVALUATION TRIGGERS

A previously successful conformance report (`FULLY_CONFORMANT`) is rendered **INVALID** under any of the following deterministic conditions:
- **Trigger INV-01 (Binary Mutation):** Any byte in the adapter's binary executable or library changes, altering the SHA-256 hash bound to the report.
- **Trigger INV-02 (Manifest Mutation):** Any modification to `adapter.json`.
- **Trigger INV-03 (Rule Catalog Major Bump):** The platform increments the Rule Catalog to a new major version ($X+1.0.0$).
- **Trigger INV-04 (Capability Taxonomy Mutation):** A canonical capability declared by the adapter is updated with incompatible parameter constraints.
- **Trigger INV-05 (Temporal Freshness Expiration):** Conformance evidence has a maximum validity period of **90 days** (`T_EVIDENCE_TTL = 90d`). Reports older than 90 days must be re-evaluated to confirm compatibility with current platform updates.

---

## 6. RULE GOVERNANCE AND RFC APPROVAL PROCESS

New conformance tests and modifications to mandatory assertions MUST undergo the formal NexLudic RFC (Request for Comments) governance process:
1. **Proposal Draft:** RFC authored specifying requirement source, assertion logic, and diagnostic code.
2. **Security & Interop Review:** Evaluated by Security Architecture for sandbox impacts and Game Interop specialists for anti-cheat risks.
3. **Reference Implementation:** Verified against the synthetic fixture test suite (`FX-BASE-001` through `FX-ERR-003`).
4. **Approval & Integration:** Merged into `NLE-CAT-CTC-001` with appropriate SemVer version increment.

---
*End of Policy — Document `NLE-POL-CVG-001`.*
