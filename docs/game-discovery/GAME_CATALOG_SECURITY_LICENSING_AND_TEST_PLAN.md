# Game Catalog Security, Licensing & Test Plan

**Document ID:** NLE-PLN-CAT-001  
**Version:** 1.0.0  
**Status:** Approved — Security, Governance & Test Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 25 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-SPC-DSC-001  
**Classification:** Security Architecture, Licensing Governance & Verification Plan  

---

## 1. Purpose and Scope

This document establishes the authoritative security boundaries, data privacy rules, source licensing compliance standards, and comprehensive design-level verification plans for the **NexLudic Engine Game Discovery and Catalog Architecture**.

In strict accordance with the Step 25 mandate:
- All security and licensing controls are formalized as binding architectural requirements.
- All test specifications are **design-level test cases** to guide future implementation in Phase 4. Zero test code, test runners, or live network calls are executed in this step.

---

## PART I: SECURITY, PRIVACY & LICENSING GOVERNANCE

## 2. Platform Security Boundaries

### 2.1 Windows Medium Integrity Level (MIL) Enforcement
In strict compliance with `REQ-SEC-001`:
- All catalog services, query resolvers, cache managers, and provider connectors must execute within the standard user space at **Windows Medium Integrity Level**.
- The discovery subsystem must **never** request UAC elevation, invoke administrative helper binaries, or attempt to modify files in protected directories (`C:\Program Files`, `C:\Windows`).
- If an operation requires elevated access, it is hard-blocked and logged as a security violation.

### 2.2 Volume D Storage Containment
In strict compliance with `DEC-009`:
- All physical storage allocated for catalog databases, caches, provider DTO dumps, and seed manifests must reside strictly on **Volume `D:\`** under:
  - `D:\NexLudic\Catalog\Database\`
  - `D:\NexLudic\Catalog\Cache\`
  - `D:\NexLudic\Catalog\Seed\`
- Any attempt by an ingestion provider to cache metadata on system drive `C:\` is intercepted and rejected.

### 2.3 Filesystem Sandbox & Local Detection Boundary
- **Zero Arbitrary Scanning:** Catalog discovery operates on structured metadata. It is strictly forbidden from traversing local file systems, scanning drives, or searching for game executables during discovery.
- **Explicit Authorization Mandate:** Future local installation detection (downstream Phase 4) requires explicit, affirmative user opt-in and must be confined to user-specified library folders (e.g., specific Steam library folders).

---

## 3. Privacy, Sensitive Data & Credential Governance

### 3.1 DPAPI Secret Vaulting
In compliance with `REQ-SEC-001` and `ADR-0003`:
- External metadata providers requiring authentication (e.g., IGDB Twitch client credentials) must retrieve API keys exclusively from the Windows DPAPI credential store using opaque references (`credential_ref`).
- Raw API tokens, client secrets, and bearer tokens must **never** appear in catalog JSON files, git repositories, logs, or UI diagnostics.

### 3.2 Private Local Path Containment
- Local installation paths (e.g., `D:\Games\Steam\steamapps\common\...`) and host environment usernames are strictly private to the local user.
- Local paths must **never** be included in shared catalog manifests, exportable project blueprints, or telemetry payloads.

### 3.3 Untrusted User Input Containment
- All search strings, manual game titles, and external metadata payloads are treated as untrusted input.
- User input must be parsed as plain text and rendered in WPF `TextBlock` controls with `TextWrapping`. Script execution, XAML markup evaluation, and SQL injection vectors are neutralized by design (`NLE-SPC-AVS-001 §8`).

---

## 4. Licensing, Copyright & Source Attribution Governance

### 4.1 Pure Textual Ingestion Mandate
- The universal catalog is authorized to ingest and store **structured technical facts only** (titles, release years, engine families, DirectX versions, AppIDs).
- **Prohibition on Proprietary Assets:** Downloading, storing, or redistributing proprietary game binaries, dynamic link libraries (`.dll`), mod scripts, proprietary game audio, or high-resolution box art from external sources is **strictly prohibited**.

### 4.2 Community Attribution Obligations
- In compliance with open and community database licenses:
  - Metadata ingested from PCGamingWiki must carry a permanent attribution tag citing `CC-BY-NC-SA 3.0`.
  - Metadata ingested from IGDB must retain the required attribution link per Twitch developer terms.
  - Wikidata claims must be marked with public domain source attribution (`CC0`).
- The desktop UI must provide a clickable *"Source & Licensing Attribution"* dialog in the Metadata Inspector displaying required community acknowledgments.

---

## PART II: DESIGN-LEVEL VERIFICATION TEST PLAN

The following 30 test cases specify the expected validation criteria for future Phase 4 implementation. All tests define initial conditions, actions, observable outcomes, and traceability to Step 25 acceptance criteria.

---

## 5. Canonical Identity & Title Tests

### TCAT-ID-001: CanonicalGameId Immutability Across Title Rename
- **Objective:** Verify `CanonicalGameId` remains identical when commercial title is updated.
- **Preconditions:** Game record exists with ID `nle:game:a1b2...` and title *"Original Title"*.
- **Action:** Update record title to *"Rebranded Title"*.
- **Expected Outcome:** Title updates; `CanonicalGameId` is unchanged; former title saved to `AlternateTitles` as `FORMER`.
- **Related AC:** AC-04.

### TCAT-ID-002: Deterministic UUIDv5 Key Generation
- **Objective:** Confirm identical external anchors yield identical canonical IDs.
- **Preconditions:** External anchor `steam:480`.
- **Action:** Generate `CanonicalGameId` across two independent runs.
- **Expected Outcome:** Both executions produce the exact same UUIDv5 string.
- **Related AC:** AC-04.

### TCAT-ID-003: Homonymous Title Disambiguation by Release Year
- **Objective:** Verify titles with identical names resolve to distinct records using release year.
- **Preconditions:** Ingest *"DOOM (1993)"* and *"DOOM (2016)"*.
- **Action:** Execute query `"DOOM"`.
- **Expected Outcome:** Pipeline generates normalized keys `doom-1993` and `doom-2016`; returns `AMBIGUOUS_CANDIDATES` with year badges; zero collision.
- **Related AC:** AC-04, AC-12.

### TCAT-ID-004: Normalized Key Generation Strips Diacritics and Articles
- **Objective:** Verify `RULE-ID-003` normalization algorithm.
- **Preconditions:** Query string `"The Pokémon Adventures: Special Edition!"`.
- **Action:** Execute normalization pipeline.
- **Expected Outcome:** Output is exactly `"pokemon-adventures-special-edition"`.
- **Related AC:** AC-12.

---

## 6. Edition & Variant Separation Tests

### TCAT-EDN-001: Distinct Editions for 32-bit vs. 64-bit Binaries
- **Objective:** Confirm Skyrim LE (x86) and Skyrim SE (x64) are stored as distinct `GameEdition` records.
- **Preconditions:** Canonical game *"The Elder Scrolls V: Skyrim"*.
- **Action:** Ingest metadata for Original release and Special Edition.
- **Expected Outcome:** Single `CanonicalGame` with two child `GameEdition` records; `ArchitectureType` reflects `X86` and `X64` respectively.
- **Related AC:** AC-05.

### TCAT-EDN-002: Promotional Bundle Collapsed to Base Edition
- **Objective:** Verify GOTY edition with identical binaries does not create a new `GameEdition`.
- **Preconditions:** Base edition exists. Ingest *"Game of the Year Edition"* with matching executable hash.
- **Action:** Process ingestion.
- **Expected Outcome:** Reused base `GameEdition`; promotional bundle tag recorded in release metadata.
- **Related AC:** AC-05.

---

## 7. Versioning & Epistemic State Tests

### TCAT-VER-001: Non-Semantic Version Preservation
- **Objective:** Confirm non-semver strings are preserved verbatim without coercion.
- **Preconditions:** Game reports version `"Patch 4 Hotfix 2 (Build 108429)"`.
- **Action:** Store version build record.
- **Expected Outcome:** `RawVersionString` exactly matches input; no parsing error; no semver coercion.
- **Related AC:** AC-06.

### TCAT-VER-002: UNKNOWN Version Accepted as Valid
- **Objective:** Verify missing version does not block project creation or produce errors.
- **Preconditions:** Game record created with empty version input.
- **Action:** Inspect `GameVersionBuild` record.
- **Expected Outcome:** `RawVersionString == "UNKNOWN"`; `EpistemicTier == DECLARED`; status is valid.
- **Related AC:** AC-07.

### TCAT-VER-003: Incomparable Version Evaluation
- **Objective:** Confirm version comparison returns `INCOMPARABLE` for disparate schemes.
- **Preconditions:** Version A = `"v1.4"`, Version B = `"Build 2023-11-04"`.
- **Action:** Invoke `Compare(VersionA, VersionB)`.
- **Expected Outcome:** Returns `INCOMPARABLE`; zero fabricated sorting order.
- **Related AC:** AC-06.

---

## 8. Provider Failure, Resilience & Fallback Tests

### TCAT-SRC-001: External Provider Timeout Handled Gracefully
- **Objective:** Confirm network timeout does not crash search or block local catalog.
- **Preconditions:** Remote provider simulated to drop connections (HTTP timeout).
- **Action:** User searches for a game title.
- **Expected Outcome:** Remote provider logged as unavailable; local seed catalog results returned; UI displays advisory banner.
- **Related AC:** AC-10, AC-18.

### TCAT-SRC-002: HTTP 429 Rate Limiting Enforces Backoff
- **Objective:** Confirm rate-limited provider pauses without failing application.
- **Preconditions:** Provider returns HTTP 429 Too Many Requests.
- **Action:** Execute query against provider.
- **Expected Outcome:** Provider marked paused; query falls back to local cache; diagnostic event emitted.
- **Related AC:** AC-10, AC-18.

### TCAT-SRC-003: 100% Offline Search via Bundled Seed Catalog
- **Objective:** Verify catalog search functions with zero network connectivity.
- **Preconditions:** Host network adapter disabled.
- **Action:** Search for pilot game pairs (e.g., *"Skyrim"*).
- **Expected Outcome:** Search resolves against `D:\NexLudic\Catalog\Seed\seed_catalog.json` immediately.
- **Related AC:** AC-10.

---

## 9. Manual Entry & Provisional Lifecycle Tests

### TCAT-MAN-001: Manual Game Registration Generates Provisional Record
- **Objective:** Confirm unrecognized game creates `ProvisionalGameIdentity`.
- **Preconditions:** User enters obscure homebrew title *"My Custom Game 2026"*.
- **Action:** Click "Add Game Manually".
- **Expected Outcome:** Created record with ID prefix `nle:provisional:`, tier `DECLARED`, and status `PROVISIONAL`.
- **Related AC:** AC-13.

### TCAT-MAN-002: Project Creation Unblocked with Provisional Game
- **Objective:** Confirm wizard advances to Stage 7 with provisional game.
- **Preconditions:** Game A is provisional; Game B is verified.
- **Action:** Advance wizard to Stage 7 Confirm.
- **Expected Outcome:** Wizard advances cleanly; pre-flight confirms non-anti-cheat; project created successfully.
- **Related AC:** AC-13.

---

## 10. Security & Privacy Tests

### TCAT-SEC-001: Medium Integrity Level Execution Verified
- **Objective:** Confirm catalog processes run without administrative privileges.
- **Preconditions:** NexLudic process executing.
- **Action:** Inspect process token integrity level.
- **Expected Outcome:** Token is `SECURITY_MANDATORY_MEDIUM_RID`; zero elevation tokens.
- **Related AC:** AC-14.

### TCAT-SEC-002: Local Installation Paths Barred from Shared Metadata
- **Objective:** Ensure user local drive paths never leak into canonical manifests.
- **Preconditions:** Game detected at `D:\Games\CustomFolder\game.exe`.
- **Action:** Export canonical game catalog definition.
- **Expected Outcome:** Exported JSON contains zero instances of `D:\Games\...`; paths confined to local app data.
- **Related AC:** AC-14.

### TCAT-SEC-003: Volume D Storage Enforcement on Catalog Files
- **Objective:** Verify all catalog caches are rejected if targeted at C:\.
- **Preconditions:** Catalog initialization routine.
- **Action:** Attempt to configure catalog cache path on `C:\NexLudic`.
- **Expected Outcome:** Throws configuration validation error; enforces `D:\NexLudic\Catalog\`.
- **Related AC:** AC-14.

---

## 11. Downstream Compatibility Boundary Tests

### TCAT-REG-001: Catalog Ingestion Does Not Alter Project Feasibility
- **Objective:** Confirm catalog metadata updates do not mutate existing project feasibility decisions.
- **Preconditions:** Project X has `FeasibilityAssessment == FEASIBLE`.
- **Action:** Catalog ingests new engine patch for Game A.
- **Expected Outcome:** Project X retained intact; feasibility status unchanged; advisory badge surfaced in UI.
- **Related AC:** AC-09, AC-26.

### TCAT-REG-002: Kernel Anti-Cheat Hard Block Triggered
- **Objective:** Verify anti-cheat metadata triggers non-dismissible Critical block in project wizard.
- **Preconditions:** Ingest game with `anti_cheat_status == KERNEL_LEVEL`.
- **Action:** Select game in Project Creation Wizard Stage 3.
- **Expected Outcome:** Wizard displays Critical banner; Confirm button hidden; project creation blocked (`REQ-SAFE-001`).
- **Related AC:** AC-09, AC-28.

---

## 12. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 25 — Game Discovery Architecture & Canonical Game Identity Specification |
| Document ID | NLE-PLN-CAT-001 |
| Created | 2026-11-17 |
| Classification | Security Architecture, Licensing Governance & Verification Plan |
| Implementation Authorization | Security, governance and test planning only |

