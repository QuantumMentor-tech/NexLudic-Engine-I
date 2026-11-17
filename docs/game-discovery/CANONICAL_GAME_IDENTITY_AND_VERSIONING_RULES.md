# Canonical Game Identity & Versioning Rules

**Document ID:** NLE-RUL-IDV-001  
**Version:** 1.0.0  
**Status:** Approved — Specification Rules Baseline  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 25 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-SPC-DSC-001  
**Classification:** Technical Rules & Invariant Specification  

---

## 1. Purpose and Scope

This document specifies the authoritative rules for establishing, preserving, and updating game identities, titles, editions, releases, versions, and build references within the **NexLudic Engine**.

These rules guarantee that game identification remains stable, deterministic, and evidence-backed, ensuring that downstream compatibility analysis, adapter bindings, and project histories are protected from silent data corruption.

---

## 2. Canonical Identity Rules

### RULE-ID-001: Immutability of CanonicalGameId
- The `CanonicalGameId` is permanently assigned upon record creation.
- Under no circumstances may a `CanonicalGameId` be altered, reassigned, or repurposed.
- If a game changes its commercial name, developer, or publisher, the `CanonicalGameId` remains identical.
- **Enforcement:** Enforced at domain entity instantiation; `CanonicalGameId` setter is private/read-only.

### RULE-ID-002: Deterministic ID Generation
- For canonical games with a verified authoritative external anchor (such as a Wikidata QID, Steam AppID, or official publisher registry), the internal identifier is derived via UUIDv5:
  $$\text{Id} = \text{"nle:game:"} + \text{UUIDv5}(\text{Namespace\_DNS}, \text{"nexludic.engine/game/"} + \text{AnchorKey})$$
- Where no single external anchor exists, a cryptographically secure random UUIDv4 is assigned.

### RULE-ID-003: Normalized Key Generation
- Every canonical game has an indexed `NormalizedKey` derived deterministically from its English canonical title:
  1. Convert text to Unicode Normalization Form KD (NFKD).
  2. Strip diacritical marks and accents (e.g., *"Pokémon"* $\rightarrow$ *"Pokemon"*).
  3. Convert to lowercase ASCII.
  4. Strip common leading stop articles: `^the\s+`, `^a\s+`, `^an\s+`.
  5. Replace all non-alphanumeric characters with a single hyphen (`-`).
  6. Collapse consecutive hyphens and trim leading/trailing hyphens.
- Example: *"The Elder Scrolls V: Skyrim®"* $\rightarrow$ `elder-scrolls-v-skyrim`.

### RULE-ID-004: Homonymous Title Disambiguation
- When two distinct games produce identical normalized keys (e.g., original releases vs. reboots), the initial commercial release year must be appended:
  - *"DOOM"* (1993) $\rightarrow$ `doom-1993`
  - *"DOOM"* (2016) $\rightarrow$ `doom-2016`
  - *"Prey"* (2006) $\rightarrow$ `prey-2006`
  - *"Prey"* (2017) $\rightarrow$ `prey-2017`
- A title without a release year collision retains the base normalized key.

---

## 3. Title, Localized Name & Alias Rules

### RULE-TTL-001: Single Canonical Title Anchor
- Every canonical game record possesses exactly one `CanonicalTitle`.
- The canonical title is designated as the international English commercial release title.
- Regional or localized variations must be cataloged as `AlternateTitle` records, never as independent canonical games.

### RULE-TTL-002: Taxonomy of Alternate Titles
Alternate titles linked to a `CanonicalGame` are categorized into four explicit types:
1. `LOCALIZED`: Official regional translations (e.g., *"Biohazard"* for *"Resident Evil"*).
2. `ACRONYM`: Common community or industry abbreviations (e.g., *"TF2"*, *"CS:GO"*, *"FFVII"*).
3. `FORMER`: Historical titles used during development or prior to commercial re-branding.
4. `COLLOQUIAL`: Widely accepted informal titles (e.g., *"Skyrim"* for *"The Elder Scrolls V: Skyrim"*).

### RULE-TTL-003: Non-Destructive Title Updates
- If the official publisher updates the canonical title (e.g., re-branding), the previous title is automatically demoted to an `AlternateTitle` of type `FORMER`.
- The `CanonicalGameId` and `NormalizedKey` remain untouched.

---

## 4. Edition vs. Variant Differentiation Rules

A primary failure mode in game modding platforms is confusing product editions with game titles. The following rules govern edition boundaries:

### RULE-EDN-001: Criteria for Distinct GameEdition
A separate `GameEdition` entity under the same `CanonicalGame` is **mandated** if and only if any of the following technical conditions are met:
1. **Binary Architecture Change:** The execution target changes (e.g., 32-bit x86 vs. 64-bit x64).
2. **Major Engine Generation Fork:** The underlying engine is upgraded to a different major version (e.g., Unreal Engine 3 to Unreal Engine 4).
3. **Core Binary Re-compilation / Remaster:** Executable memory layout, symbol tables, or asset packaging structures are fundamentally altered.
4. **Platform-Exclusive Technical Branch:** The game on Platform X has a distinct codebase from Platform Y that breaks binary adapter parity.

### RULE-EDN-002: Rejection of Marketing-Only Editions
- A commercial release that bundles DLCs, soundtracks, or digital cosmetics but executes against the **exact same binary executable and memory layout** (e.g., *"Game of the Year Edition"*, *"Deluxe Edition"*) does **NOT** warrant a distinct `GameEdition`.
- Such releases are cataloged under the base edition with DLC entitlement flags.

---

## 5. Version and Build Representation Rules

### RULE-VER-001: Acceptance of Non-Semantic Version Strings
- The catalog must **never** enforce or assume that game versions follow Semantic Versioning (SemVer: `MAJOR.MINOR.PATCH`).
- Games utilize wildly divergent versioning schemes:
  - Date-based: `2023.11.04`
  - Build numbers: `Build 108429`
  - Incremental letters: `v1.2b`, `v1.2c`
  - Commit hashes: `git-a7f39b1`
  - Composite strings: `1.6.640.0-Steam`
- All version representations must preserve the `RawVersionString` verbatim as emitted by the source.

### RULE-VER-002: Unknown Version Is a Valid Epistemic State
- If a game has no exposed version number or the user leaves the version field empty:
  - `RawVersionString` is set to `"UNKNOWN"`.
  - `EpistemicTier` is set to `DECLARED` or `UNVERIFIED`.
- In strict adherence to `NLE-DOM-SPEC-001`, an unknown version is a valid, permitted domain state. It must **never** block project creation, search, or browsing.

### RULE-VER-003: Version Comparison Abstraction
Because games use non-uniform versioning, universal numeric comparisons are prohibited. A version comparator must evaluate versions according to their specific `VersioningScheme`:

$$\text{Compare}(V_1, V_2) \rightarrow \begin{cases} \text{EQUAL} & \text{if } V_1.\text{Raw} == V_2.\text{Raw} \\ \text{ORDERABLE} & \text{if both parse cleanly into declared scheme} \\ \text{INCOMPARABLE} & \text{otherwise} \end{cases}$$

- If versions are `INCOMPARABLE`, the system must report `INCOMPARABLE`. It must **never** fabricate a sort order or assume that $V_1 > V_2$.

### RULE-VER-004: Version Provenance Tiers
Every version reference carries an explicit `EpistemicTier`:
- `DECLARED`: Stated by the user or an untrusted manifest without verification.
- `IMPORTED`: Extracted from a structured third-party catalog (e.g., Steam store API).
- `DETECTED`: Discovered on the local host by reading PE headers, file metadata, or local manifest files.
- `VERIFIED`: Cryptographically confirmed via binary hashing (SHA-256) matching an approved reference signature.

---

## 6. Duplicate Detection and Record Reconciliation

### RULE-DUP-001: Duplicate Candidate Triggers
A record is flagged as a potential duplicate candidate (`DUPLICATE_CANDIDATE`) if:
1. `NormalizedKey` matches an existing record, OR
2. High string similarity ($\text{Jaro-Winkler} \ge 0.92$) exists between titles **AND** both records share the exact same `InitialReleaseYear` and `Developer`.

### RULE-DUP-002: Human-in-the-Loop Merge Mandate
- The catalog system is strictly forbidden from executing automatic, unattended record merges.
- All potential duplicates must be surfaced for administrative or developer review.
- Distinct editions or re-releases must never be collapsed into a single entity.

### RULE-REC-001: Historical Preservation on Merge
If two records $A$ and $B$ are formally merged:
1. Surviving record $A$ retains its `CanonicalGameId`.
2. Retired record $B$ is set to status `SUPERSEDED`, and its `CanonicalGameId` is permanently redirected to $A$.
3. All existing projects referencing $B$ continue to resolve via the redirection pointer without breaking historical links.

---

## 7. Downstream Compatibility Invalidation Rules

### RULE-CMP-001: Catalog Update Isolation
- Changes to catalog metadata (e.g., newly detected patch version, updated engine classification) must never automatically invalidate or rewrite active project compatibility decisions.
- Downstream compatibility intelligence (`CompatibilityEngine`) retains exclusive authority over feasibility invalidation.

### RULE-CMP-002: Version Drift Advisory
- If a project's targeted game version differs from the latest catalog version or the locally installed version:
  - An informational advisory badge (`CAT-03` Warning or `CAT-02` Info) is surfaced in the Project Overview dashboard.
  - The project's existing feasibility assessment remains intact until the user explicitly triggers a Re-Assessment.

---

## 8. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 25 — Game Discovery Architecture & Canonical Game Identity Specification |
| Document ID | NLE-RUL-IDV-001 |
| Created | 2026-11-17 |
| Classification | Technical Rules & Invariant Specification |
| Implementation Authorization | Architecture and rules definition only |

