# Target Identity, Selection Evidence and Compatibility Policy

**Document Identifier:** `NLE-POL-TIS-001`  
**Lifecycle Stage:** Content Pipeline Stage I Policy & Governance  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Target Environment Modeling & Compatibility Rules  

---

## 1. Multi-Tier Target Identity Modeling

To prevent accidental misapplication across disparate game releases, editions, or local directories, NexLudic Engine enforces a **Four-Tier Target Identity Model**:

```
+--------------------------------------------------------------------------------------------------+
|                                FOUR-TIER TARGET IDENTITY MODEL                                   |
+--------------------------------------------------------------------------------------------------+
|  Tier 1: Canonical Game Identity (game_id)                                                       |
|  - Universal game family identifier (e.g., "game-skyrim-se", "game-witcher-3")                   |
|  - Independent of platform, distribution store, or local path                                    |
+--------------------------------------------------------------------------------------------------+
|  Tier 2: Target Release & Build Identity (release_id / build_id)                                  |
|  - Specific version, content patch, or binary edition (e.g., "release-1.6.640-steam")           |
|  - Bound to specific executable checksums and engine revisions                                   |
+--------------------------------------------------------------------------------------------------+
|  Tier 3: Platform & Distribution Channel (platform / store_channel)                              |
|  - OS architecture (e.g., "windows-x64") and storefront (e.g., "steam", "gog", "epic")          |
|  - Determines filesystem layout conventions and DRM / launcher constraints                       |
+--------------------------------------------------------------------------------------------------+
|  Tier 4: Target Installation Instance (installation_instance_id)                                 |
|  - Unique UUID bound to a specific local filesystem root hash and user authorization             |
|  - Guaranteed distinct across multiple parallel installations on the same machine                |
+--------------------------------------------------------------------------------------------------+
```

---

## 2. Target Selection Modes & Evidence Provenance

Target selection must be explicit, verifiable, and authorized. The engine recognizes four selection modes:

1. **User-Authorized Installation Instance (`MODE_EXPLICIT_INSTANCE`):** The user explicitly selects an authorized, registered installation profile managed by the Universal Adapter SDK.
2. **Target Profile Specification (`MODE_TARGET_PROFILE`):** Planning executes against a validated offline target profile without inspecting local filesystem state (dry-run design mode).
3. **Synthetic Test Fixture (`MODE_SYNTHETIC_FIXTURE`):** Planning executes against mock target descriptors in automated CI/CD test harnesses.
4. **Adapter-Asserted Discovery (`MODE_ADAPTER_DISCOVERY`):** The target identity is asserted by an authorized game adapter plugin that has verified local installation markers.

### 2.1 Evidence Hierarchy & Trust Levels
Target claims are classified under four distinct trust levels:
- `TRUST_LEVEL_4 (CRYPTOGRAPHIC_OBSERVATION)`: Direct read-only observation of binary hashes (e.g., SHA-256 of main executable).
- `TRUST_LEVEL_3 (ADAPTER_ASSERTED)`: Signed claim from a certified Universal Adapter SDK plugin.
- `TRUST_LEVEL_2 (MANIFEST_DECLARED)`: Target claims recorded in game distribution metadata files.
- `TRUST_LEVEL_1 (UNVERIFIED_USER_CLAIM)`: Raw user input without corroborating system evidence. Planning cannot proceed on Level 1 evidence alone.

---

## 3. Compatibility Evaluation Dimensions

Compatibility evaluation tests the package's requirements against target evidence across five dimensions:

| Dimension | Evaluated Target Property | Policy Rule |
| :--- | :--- | :--- |
| **Game Family** | `target.game_id` | Must strictly equal `package.compatibility.target_game_id` |
| **Build Version Range** | `target.build_version` | Must fall within SemVer range `package.compatibility.supported_versions` |
| **Platform Architecture** | `target.platform` | Must match target OS and CPU architecture (e.g., `windows-x64`) |
| **Loader / Hook Subsystem** | `target.mod_loader` | Mod loader version must satisfy minimum required capability |
| **Adapter Capabilities** | `target.adapter_capabilities` | Target must support required modification mechanisms (`VFS`, `OVERLAY`) |

---

## 4. Evidence Disagreement & Disambiguation Rules

When multiple evidence sources yield conflicting information regarding target identity or version:
1. **No Speculative Averaging:** The engine must never guess, average, or select a "most likely" version.
2. **Contradiction Halting:** Any unresolved contradiction between evidence sources triggers `ERR_TIS_EVIDENCE_CONFLICT` and suspends planning.
3. **Explicit User Disambiguation:** The system prompts the user or calling workflow to provide verified clarifying evidence or select a definitive installation profile.

---

## 5. Policy Invariants

- Under no circumstances may a package declared for one canonical game be planned against a different game.
- Target selection metadata does NOT constitute filesystem write permission.
