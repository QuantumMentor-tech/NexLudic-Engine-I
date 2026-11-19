# Host Game Content Compatibility & Evidence Policy

**Document Identifier:** `NLE-POL-GCC-001`  
**Classification:** Quality Assurance & Compatibility Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`, `NLE-POL-GMS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Objective & Compatibility Principles

A content modification package must never be applied to a host game based on speculative claims. This policy establishes the required compatibility dimensions, evidence standards, verification state lifecycle, and invalidation rules.

---

## 2. Six Dimensions of Compatibility

Every modification plan must demonstrate compatibility across six independent dimensions:

| Dimension | Description | Verification Method |
| :--- | :--- | :--- |
| **1. Game Build Compatibility** | Match against exact target executable and data patch version. | Executable SHA-256 / Version Resource |
| **2. Engine Subsystem Compatibility** | Compatibility with underlying rendering/physics runtime. | Adapter Capability Match Matrix |
| **3. Mod Loader Compatibility** | Compatibility with detected loader (e.g. SKSE64 2.2.6, UE4SS 3.0).| Dynamic Loader API Probe / Version Check |
| **4. Asset Schema Compatibility**| Match between package asset format and engine format version. | Asset Header & Magic Byte Validation |
| **5. Platform & OS Compatibility**| Windows architecture, Medium IL permissions, graphics API (DX11/12/Vulkan).| OS & Driver Capability Query |
| **6. Coexistence Compatibility**| Non-interference with existing user modifications. | Filesystem Conflict Matrix Analysis |

---

## 3. Compatibility Evidence Standards

Compatibility claims must be supported by empirical evidence classified by source authority:

```
[Tier 1: Empirical Execution Evidence] (HIGHEST AUTHORITY)
   - Successful automated smoke test execution logs.
   - Zero crash reports, valid rendering frame capture.

[Tier 2: Adapter Structural Static Verification]
   - Successful deep binary validation of generated asset archives.
   - Header validation, schema validation, reference integrity checks.

[Tier 3: Declared Vendor / Community Capability Matrix]
   - Published compatibility profiles from adapter maintainers.

[Tier 4: Speculative / Unverified Claim] (INSUFFICIENT FOR AUTO-APPROVAL)
   - Purely heuristic or untested AI agent declarations.
```

---

## 4. Compatibility State Lifecycle

```
             [UNKNOWN]
                 |
        +--------+--------+
        |                 |
  (Static Checks    (Static Checks
      Passed)           Failed)
        |                 |
        v                 v
[COMPATIBLE_UNTESTED]  [INCOMPATIBLE_BLOCKED]
        |
   (Smoke Test
    Conducted)
        |
  +-----+-----+
  |           |
(Passed)   (Failed)
  |           |
  v           v
[COMPATIBLE_VERIFIED]  [CONDITIONAL_DEGRADED]
```

### State Definitions:
- **`UNKNOWN`:** Default initial state. No compatibility analysis has been conducted.
- **`INCOMPATIBLE_BLOCKED`:** Definitively incompatible (e.g. game patch mismatch, anti-cheat conflict). Modification application prohibited.
- **`COMPATIBLE_UNTESTED`:** Passed static structural validation, but live runtime execution has not been observed. Requires explicit user acknowledgment to deploy.
- **`COMPATIBLE_VERIFIED`:** Supported by concrete Tier 1 or Tier 2 verification evidence.
- **`CONDITIONAL_DEGRADED`:** Functional with known non-fatal limitations (e.g., lower resolution textures or missing audio voiceovers).

---

## 5. Invalidation Triggers

A previously verified compatibility state is IMMEDIATELY invalidated and reset to `UNKNOWN` or `INCOMPATIBLE_BLOCKED` upon:
1. **Game Patch / Update:** The target game executable SHA-256 or build number changes.
2. **Adapter Update:** The registered game adapter version is incremented.
3. **Mod Loader Change:** The mod loader is updated, disabled, or removed by the user.
4. **Filesystem Collision:** New user modifications are detected in the target directory.
