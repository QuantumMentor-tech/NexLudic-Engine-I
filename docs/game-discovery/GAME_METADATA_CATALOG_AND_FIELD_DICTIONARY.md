# Game Metadata Catalog & Field Dictionary

**Document ID:** NLE-DIC-MET-001  
**Version:** 1.0.0  
**Status:** Approved — Data Dictionary Baseline  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 25 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-SPC-DSC-001  
**Classification:** Metadata Architecture & Field Dictionary  

---

## 1. Purpose and Scope

This document establishes the comprehensive **Game Metadata Catalog & Field Dictionary** for the NexLudic Engine. It defines the complete inventory of standardized metadata attributes utilized across game discovery, canonical catalog records, technical classification, modding ecosystem identification, and provenance tracking.

Each entry documents:
- **Concept & Purpose:** Why the field exists and what technical decision it informs.
- **Data Shape & Constraints:** Conceptual type, nullability, and validity boundaries.
- **Ownership & Provenance:** Which subsystem owns the data and what evidence tier is required.
- **Freshness & Privacy:** Invalidation policies and privacy/licensing restrictions.
- **Governance Status:** Approved, Provisional, or Unresolved.

---

## 2. Field Dictionary Structure

Fields are organized into seven operational clusters:
1. **Cluster 1: Core Identification & Taxonomy** (Canonical Game Entity)
2. **Cluster 2: Edition & Variant Attributes** (Game Edition Entity)
3. **Cluster 3: Distribution & Storefront Metadata** (Release Distribution Entity)
4. **Cluster 4: Version & Build Technical Fields** (Version Build Entity)
5. **Cluster 5: Technical Engine & Runtime Metadata** (Game Runtime Entity)
6. **Cluster 6: Modding Ecosystem & Extensibility Attributes** (Ecosystem Boundary)
7. **Cluster 7: Provenance & Governance Metadata** (Claim Tracking)

---

## 3. Cluster 1: Core Identification & Taxonomy

### FLD-ID-001: `canonical_game_id`
- **Concept:** Immutable unique identifier for a canonical game.
- **Purpose:** Stable foreign key for all project manifests, research logs, and adapter bindings.
- **Proposed Owner:** `CanonicalGameCatalog` (Core Domain).
- **Data Shape:** String (UUIDv5 formatted with prefix `nle:game:<uuid>`).
- **Required / Optional:** **Mandatory** (Non-null).
- **Validity Constraints:** Must match regex `^nle:game:[a-f0-9]{8}-[a-f0-9]{4}-5[a-f0-9]{3}-[89ab][a-f0-9]{3}-[a-f0-9]{12}$`.
- **Provenance Requirement:** Generated deterministically from canonical anchor or random UUIDv4.
- **Freshness:** Immutable. Never expires.
- **Privacy / Licensing:** Public metadata. Unrestricted.
- **Status:** **Approved** (`NLE-DOM-SPEC-001`).

### FLD-ID-002: `canonical_title`
- **Concept:** Authoritative international English commercial release title.
- **Purpose:** Primary display label for user interface, search indexing, and reports.
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** String (UTF-8, plain text).
- **Required / Optional:** **Mandatory** (Non-null, non-whitespace).
- **Validity Constraints:** 1 to 200 characters. No HTML or formatting markup.
- **Provenance Requirement:** Sourced from primary publisher listing or verified catalog.
- **Freshness:** Updated on official commercial re-branding only.
- **Privacy / Licensing:** Public commercial title.
- **Status:** **Approved**.

### FLD-ID-003: `normalized_key`
- **Concept:** Diacritic-stripped, lowercase, hyphenated representation of the title.
- **Purpose:** Fast indexed querying, deduplication, and file directory slug derivation.
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** String (ASCII, lowercase alphanumeric with hyphens).
- **Required / Optional:** **Mandatory** (Non-null).
- **Validity Constraints:** Must match `^[a-z0-9]+(-[a-z0-9]+)*$`. Maximum 100 characters.
- **Provenance Requirement:** Derived deterministically via `RULE-ID-003`.
- **Freshness:** Synchronized with `canonical_title`.
- **Status:** **Approved**.

### FLD-ID-004: `initial_release_year`
- **Concept:** Four-digit calendar year of the game's initial commercial release.
- **Purpose:** Vital disambiguator for reboots, remakes, and homonymous titles (`RULE-ID-004`).
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** Integer (Nullable).
- **Required / Optional:** Optional (Recommended).
- **Validity Constraints:** $1970 \le \text{Year} \le \text{CurrentYear} + 2$.
- **Provenance Requirement:** Verified catalog metadata (IGDB, Wikidata, Steam).
- **Freshness:** Historical constant. Never expires.
- **Status:** **Approved**.

### FLD-ID-005: `developer_name`
- **Concept:** Primary development studio credited with building the software.
- **Purpose:** Disambiguation, catalog filtering, and engine family inference.
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** String (Nullable).
- **Required / Optional:** Optional.
- **Validity Constraints:** 1 to 150 characters.
- **Status:** **Approved**.

### FLD-ID-006: `publisher_name`
- **Concept:** Primary commercial publishing entity.
- **Purpose:** Commercial rights identification and platform storefront correlation.
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** String (Nullable).
- **Required / Optional:** Optional.
- **Status:** **Approved**.

### FLD-ID-007: `alternate_titles`
- **Concept:** Collection of aliases, regional localizations, acronyms, and former titles.
- **Purpose:** Enhances discovery query matching without creating duplicate records.
- **Proposed Owner:** `CanonicalGameCatalog`.
- **Data Shape:** List of `AlternateTitle` objects (`Title`, `Category`, `LocaleCode`).
- **Required / Optional:** Optional (Defaults to empty list).
- **Status:** **Approved**.

---

## 4. Cluster 2: Edition & Variant Attributes

### FLD-EDN-001: `edition_id`
- **Concept:** Unique identifier for a technical or commercial edition family.
- **Purpose:** Distinguishes editions requiring different binary adapters or engine bindings.
- **Proposed Owner:** `GameEdition` entity.
- **Data Shape:** String (`nle:edition:<slug>`).
- **Required / Optional:** **Mandatory** for every edition.
- **Status:** **Approved**.

### FLD-EDN-002: `edition_title`
- **Concept:** Distinctive title of the edition (e.g., *"Special Edition"*, *"Remastered"*).
- **Purpose:** User selection and UI disambiguation in game pair setup.
- **Data Shape:** String (1 to 100 characters).
- **Status:** **Approved**.

### FLD-EDN-003: `architecture_type`
- **Concept:** Primary binary processor architecture targeted by the edition's executable.
- **Purpose:** Crucial compatibility gate; 32-bit (x86) and 64-bit (x64) binaries cannot share native DLL injection hooks.
- **Data Shape:** Enum: `X64`, `X86`, `ARM64`, `UNKNOWN`.
- **Required / Optional:** **Mandatory** (Defaults to `UNKNOWN`).
- **Status:** **Approved**.

### FLD-EDN-004: `is_remaster`
- **Concept:** Boolean indicating whether the edition is a technical remaster.
- **Purpose:** Alerts downstream compatibility intelligence that original asset formats and hooks may be incompatible.
- **Data Shape:** Boolean.
- **Status:** **Approved**.

### FLD-EDN-005: `has_engine_fork`
- **Concept:** Boolean indicating that the edition runs on a substantially modified or forked engine codebase.
- **Purpose:** Enforces adapter boundary isolation (`RULE-EDN-001`).
- **Data Shape:** Boolean.
- **Status:** **Approved**.

---

## 5. Cluster 3: Distribution & Storefront Metadata

### FLD-REL-001: `storefront_channel`
- **Concept:** The distribution platform or storefront delivering the software package.
- **Purpose:** Determines executable packaging, DRM wrapper presence, and launcher parameters.
- **Data Shape:** Enum: `STEAM`, `GOG`, `EPIC_GAMES`, `MICROSOFT_STORE`, `STANDALONE`, `PHYSICAL_MEDIA`, `CUSTOM`.
- **Status:** **Approved**.

### FLD-REL-002: `storefront_app_id`
- **Concept:** Storefront-specific identifier (e.g., Steam AppID, GOG Product ID).
- **Purpose:** Enables programmatic launch commands and store manifest cross-referencing.
- **Data Shape:** String (Nullable, e.g., *"480"*, *"1207658924"*).
- **Status:** **Approved**.

### FLD-REL-003: `has_drm`
- **Concept:** Flag indicating whether the executable is protected by an active DRM wrapper.
- **Purpose:** Informs adapter whether memory injection or binary modification is impeded.
- **Data Shape:** Boolean (Nullable).
- **Status:** **Approved**.

---

## 6. Cluster 4: Version & Build Technical Fields

### FLD-VER-001: `raw_version_string`
- **Concept:** Verbatim version string as exposed by the game executable, UI, or manifest.
- **Purpose:** Preserves exact version identity without forcing artificial semver conformity (`RULE-VER-001`).
- **Data Shape:** String (1 to 60 characters).
- **Required / Optional:** **Mandatory** (May contain `"UNKNOWN"`).
- **Status:** **Approved**.

### FLD-VER-002: `build_identifier`
- **Concept:** Underlying manifest build ID or revision hash.
- **Purpose:** Precise build disambiguation when two releases share the same public version label.
- **Data Shape:** String (Nullable, e.g., *"Build 108429"*).
- **Status:** **Approved**.

### FLD-VER-003: `epistemic_tier`
- **Concept:** The verification pedigree of the recorded version.
- **Purpose:** Epistemic integrity; distinguishes unverified user declarations from hash-verified builds.
- **Data Shape:** Enum: `DECLARED`, `IMPORTED`, `DETECTED`, `VERIFIED`.
- **Status:** **Approved** (`NLE-DOM-SPEC-001`).

---

## 7. Cluster 5: Technical Engine & Runtime Metadata

### FLD-ENG-001: `engine_family`
- **Concept:** Foundational software framework or commercial engine.
- **Purpose:** Selects candidate universal adapters (e.g., Unreal Engine adapter, Unity adapter).
- **Data Shape:** String (e.g., *"Unreal Engine"*, *"Unity"*, *"Creation Engine"*, *"Source 2"*, *"Custom Native"*).
- **Status:** **Approved**.

### FLD-ENG-002: `engine_version`
- **Concept:** Specific version of the underlying engine.
- **Purpose:** Evaluates memory layout stability and known symbol offsets.
- **Data Shape:** String (Nullable, e.g., *"4.27.2"*, *"5.3.1"*).
- **Status:** **Approved**.

### FLD-ENG-003: `scripting_backend`
- **Concept:** Virtual machine or language runtime used for game gameplay logic.
- **Purpose:** Determines logic bridge integration vectors (Mono vs. IL2CPP vs. Lua vs. Native).
- **Data Shape:** Enum: `MONO_CLR`, `IL2CPP`, `NATIVE_C_PLUS_PLUS`, `LUA`, `PYTHON`, `PAPIN`, `UNKNOWN`.
- **Status:** **Approved**.

### FLD-ENG-004: `graphics_api`
- **Concept:** Primary rendering pipeline API utilized by the game binary.
- **Purpose:** Dictates graphics hook injection feasibility (DirectX 9 vs 11 vs 12 vs Vulkan).
- **Data Shape:** Enum: `DIRECTX_9`, `DIRECTX_11`, `DIRECTX_12`, `VULKAN`, `OPENGL`, `UNKNOWN`.
- **Status:** **Approved**.

---

## 8. Cluster 6: Modding Ecosystem & Extensibility Fields

### FLD-MOD-001: `primary_mod_loader`
- **Concept:** Dominant community-standard extension framework or script loader.
- **Purpose:** Informs adapter whether it can hook through an existing mod loader (e.g., UE4SS, SKSE, SMAPI, BepInEx).
- **Data Shape:** String (Nullable).
- **Status:** **Approved**.

### FLD-MOD-002: `anti_cheat_status`
- **Concept:** Presence and category of anti-cheat / anti-tamper technologies.
- **Purpose:** Enforces mandatory safety hard-block (`REQ-SAFE-001`).
- **Data Shape:** Enum: `NONE`, `CLIENT_SIDE_INTEGRITY`, `SERVER_VERIFIED`, `KERNEL_LEVEL`, `ONLINE_COMPETITIVE_ACTIVE`.
- **Validity Constraints:** If `KERNEL_LEVEL` or `ONLINE_COMPETITIVE_ACTIVE`, triggers non-dismissible `Critical` block in project wizard.
- **Status:** **Approved** (`REQ-SAFE-001`).

---

## 9. Cluster 7: Provenance & Governance Metadata

### FLD-PRV-001: `source_authority`
- **Concept:** Name or URI of the entity that provided the metadata claim.
- **Purpose:** Complete traceability and conflict arbitration.
- **Data Shape:** String (e.g., *"provider:pcgamingwiki"*, *"provider:steam_api"*, *"user:declared"*).
- **Status:** **Approved**.

### FLD-PRV-002: `ingested_at`
- **Concept:** UTC ISO 8601 timestamp when the metadata was fetched or recorded.
- **Purpose:** Freshness evaluation and stale cache invalidation.
- **Data Shape:** DateTime (UTC).
- **Status:** **Approved**.

### FLD-PRV-003: `license_attribution`
- **Concept:** Required copyright, licensing, or attribution tag for the ingested metadata.
- **Purpose:** Legal compliance with community API terms (`CC-BY-NC-SA`, etc.).
- **Data Shape:** String (Nullable).
- **Status:** **Approved**.

---

## 10. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 25 — Game Discovery Architecture & Canonical Game Identity Specification |
| Document ID | NLE-DIC-MET-001 |
| Created | 2026-11-17 |
| Classification | Metadata Architecture & Field Dictionary |
| Implementation Authorization | Architecture and data dictionary only |

