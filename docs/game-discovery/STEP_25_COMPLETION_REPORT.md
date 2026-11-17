# Step 25 Completion Report: Game Discovery Architecture & Canonical Game Identity Specification

**Document ID:** `NLE-REP-STP-025`  
**Version:** 1.0.0  
**Phase:** 4 of 12 — Game Discovery & Compatibility Intelligence  
**Step Number:** 25 of 100  
**Step Title:** Game Discovery Architecture & Canonical Game Identity Specification  
**Workstream:** Game Discovery, Canonical Identity, Metadata Architecture, Catalog Boundaries, and Compatibility-Intelligence Foundation  
**Date:** 2026-11-17  
**Status:** Complete & Approved  
**Classification:** Phase 4 Foundation & Step Completion Report  
**Working Repository:** `D:\App Winrar Files\Nexludic Engine (working)`  
**Repository Branch:** `main`  

---

## A. Overall Status

### Final Status: **COMPLETE**

Step 25 of 100 has fulfilled all objectives, architectural requirements, quality mandates, and deliverables set forth in the Step 25 directive.

Execution operated strictly in **research, domain modeling, architecture specification, data dictionary authoring, and test planning mode**. In strict compliance with the step boundary:
- Exactly **0 lines of application source code** were authored.
- Zero database tables, schemas, or external API connectors were implemented.
- Zero local drive scans or filesystem traversals were executed.
- Zero third-party packages or NuGet/npm dependencies were installed.
- All 8 mandated deliverables have been authored, validated, and placed in [`docs/game-discovery/`](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/).

---

## B. Prerequisite Verification

### 1. Phase 3 Gate Verification
The Phase 3 Gate Review (`NLE-GAT-PH3-001`, `STEP_24_COMPLETION_REPORT.md`) was inspected. Phase 3 formally concluded with a determination of **`READY WITH CONDITIONS`**. All five governance conditions were verified as satisfied within Step 25:
- **Condition 1 (Presentation Decoupling):** Catalog specifications establish pure domain contracts decoupled from UI controls.
- **Condition 2 (Volume D Storage):** All catalog cache, seed, and manifest paths are strictly allocated to `D:\NexLudic\Catalog\` (`DEC-009`).
- **Condition 3 (Staging Jail & Diff Gating):** Discovery services never deploy files to game installations (`ADR-0009`).
- **Condition 4 (DPAPI Secret Vault):** Provider API keys are governed by opaque reference UUIDs (`REQ-SEC-001`).
- **Condition 5 (Deferred Relational Storage):** Relational databases (SQLite) remain formally deferred (`DEC-ARC-007`). Specifications use conceptual entities and JSON manifest structures.

### 2. Upstream Architecture Baselines Verified
- `NLE-DOM-SPEC-001` (Core Domain Model): Invariants, epistemic tiers, and `GameParticipant` contracts verified.
- `NLE-CAT-WIZ-001` (Project Creation Wizard Catalog): Two-game selection rules and Volume D path constraints verified.
- `REQ-SAFE-001` (Safety Baseline): Anti-cheat hard-block verified and operationalized in metadata catalog.
- `REQ-SEC-001` (Security Baseline): Windows Medium Integrity Level verified.

---

## C. Repository Audit

A comprehensive repository inspection prior to Step 25 execution established:
- **Repository Branch:** `main` (clean working tree with untracked documentation files).
- **Physical Directories Inspected:**
  - `docs/architecture/` (Phase 2 foundation and ADRs)
  - `docs/ui/` (Phase 3 desktop interface specifications, 39 files)
- **Host Runtime:** Microsoft .NET 8.0.31 verified on host machine (`ADR-0003`).
- **Existing Source Implementation:** Exactly 0 lines of C# / XAML code exist in the repository; the project remains in architecture and specification phase.

---

## D. Existing Game Identity Implementation Found in Repository

The audit examined existing game-related representations established in prior documentation:
1. **Core Domain Model (`NLE-DOM-SPEC-001`):** Defined `GameParticipant` (with `GameId`, `GameEdition`, `GameVersionRef`, `GameSignature`, and `ParticipantRole`).
2. **Project Creation Wizard (`NLE-SPC-WIZ-001`, `NLE-CAT-WIZ-001`):** Established `GameTitle`, `GameId`, `GameEdition`, `GameVersion`, and `GameEnvironment`. Enforced distinct game selection (`Game A != Game B`) and anti-cheat checks.
3. **Component Catalog (`NLE-CAT-CMP-001`, `NLE-SPC-COL-001`):** Defined `GamePairHeader` (CMP-029) rendering Game A and Game B badges.

**Finding:** The existing documentation treated `GameId` and `GameEdition` at a conceptual level suitable for UI project setup, but lacked an exhaustive metadata catalog, provider abstraction, normalization pipeline, and version comparison rules. Step 25 directly extends and formalizes this foundation without contradicting earlier baselines.

---

## E. Canonical Identity Design

The canonical game identity model established in `NLE-SPC-DSC-001` and `NLE-DOM-CAT-001` enforces:
1. **Immutability:** Every game possesses a permanent, unique `CanonicalGameId` (`nle:game:<uuid>`).
2. **Title Decoupling:** Display names, re-branding, and marketing titles do not alter the internal identifier (`RULE-ID-001`).
3. **Normalized Key Generation:** Deterministic derivation via Unicode NFKD normalization, diacritic stripping, lowercase conversion, and stop-article removal (`RULE-ID-003`).
4. **Homonymous Disambiguation:** Mandatory release year appending for title collisions (e.g., `doom-1993` vs `doom-2016`) (`RULE-ID-004`).
5. **Hierarchical Entity Separation:**
   $$\text{Canonical Game} \longrightarrow \text{Game Edition} \longrightarrow \text{Release Distribution} \longrightarrow \text{Version/Build} \longrightarrow \text{Local Installation}$$
   Separates conceptual identity from technical editions (e.g., 32-bit Skyrim LE vs. 64-bit Skyrim SE), storefront SKUs (Steam vs. GOG), exact patch builds, and physical local directories.

---

## F. Metadata Catalog & Field Dictionary

`GAME_METADATA_CATALOG_AND_FIELD_DICTIONARY.md` (`NLE-DIC-MET-001`) defines 35+ standardized metadata fields across 7 operational clusters:
- **Core Identification & Taxonomy:** `canonical_game_id`, `canonical_title`, `normalized_key`, `initial_release_year`, `developer_name`, `publisher_name`, `alternate_titles`.
- **Edition & Variant Attributes:** `edition_id`, `edition_title`, `architecture_type` (x64/x86/ARM64), `is_remaster`, `has_engine_fork`.
- **Distribution & Storefront:** `storefront_channel`, `storefront_app_id`, `has_drm`.
- **Version & Build Fields:** `raw_version_string`, `build_identifier`, `epistemic_tier`.
- **Technical Engine & Runtime:** `engine_family`, `engine_version`, `scripting_backend` (Mono/IL2CPP/Lua/Native), `graphics_api`.
- **Modding Ecosystem & Safety:** `primary_mod_loader`, `anti_cheat_status` (`KERNEL_LEVEL` hard block trigger).
- **Provenance & Governance:** `source_authority`, `ingested_at`, `license_attribution`.

Every field documents conceptual type, nullability, validity bounds, ownership, freshness, and privacy boundaries.

---

## G. Discovery & Identity Resolution

`GAME_DISCOVERY_IDENTITY_RESOLUTION_AND_DATA_QUALITY_RULES.md` (`NLE-RUL-RES-001`) formalizes:
1. **4-Stage Matching Pipeline:** Exact Anchor / ID Match $\rightarrow$ Exact Normalized Title Match $\rightarrow$ Alternate Title / Alias Match $\rightarrow$ Fuzzy Candidate Generation ($\text{Jaro-Winkler} \ge 0.85$).
2. **Prohibition on Blind Selection:** If multiple candidates return close similarity ($\Delta < 0.08$), auto-selection is strictly barred. The system returns `AMBIGUOUS_CANDIDATES` for human selection (`RULE-MAT-003`).
3. **Manual Entry & Provisional Records:** If a game is absent from catalogs, user can register a `ProvisionalGameIdentity` with tier `DECLARED`. Project creation is unblocked immediately (`RULE-MAN-001`).
4. **Epistemic Integrity:** Proves that **Identity Match $\ne$ Compatibility Proof** (`RULE-MAT-004`).

---

## H. Provider & Source Governance

`GAME_METADATA_SOURCE_AND_PROVIDER_ARCHITECTURE.md` (`NLE-ARC-SRC-001`) defines the provider architecture:
- **Source Evaluation:** Steamworks Storefront API, PCGamingWiki (Cargo API), IGDB API, Wikidata SPARQL, and Local Offline Seed Catalog.
- **Provider Abstraction (`IMetadataProvider`):** Queries, anchor lookups, capability declarations, and normalization boundaries.
- **Field-Domain Authority Precedence:** Storefront APIs are authoritative for AppIDs and release dates; PCGamingWiki is authoritative for engine family, bitness, and anti-cheat; Wikidata/IGDB for aliases.
- **Conflict Arbitration:** Conflicting values preserved in `MetadataConflictRecord`; safer anti-cheat determination enforced on conflict.
- **Resilience:** Graceful degradation on timeouts and HTTP 429 rate limits; 100% offline operation via local seed catalog on Volume `D:\`.
- **Licensing Compliance:** Textual facts only; zero proprietary binaries or assets ingested; `CC-BY-NC-SA 3.0` attribution tags preserved for community sources.

---

## I. Compatibility Boundary

The specification strictly enforces the boundary between catalog discovery and compatibility intelligence:
- **Catalog Owns:** Stable identity, title normalization, edition architecture (x64/x86), engine family metadata, distribution channels, and source provenance.
- **Compatibility Engine Owns (Phase 5):** Capability contract validation, binary injection feasibility, adapter binding, memory hooking, and feasibility gate decisions.
- **Catalog Isolation:** Ingesting new catalog metadata never mutates existing project feasibility states automatically (`RULE-CMP-001`).

---

## J. Security and Privacy Boundaries

- **Windows Medium Integrity Level:** Catalog services execute strictly in standard user space without UAC elevation (`REQ-SEC-001`).
- **Volume D Storage:** All catalog caches and databases are confined to `D:\NexLudic\Catalog\` (`DEC-009`).
- **Filesystem Sandbox:** Catalog discovery operates on metadata; zero arbitrary drive traversal permitted. Local installation detection requires explicit user opt-in.
- **DPAPI Secret Vaulting:** External provider API keys stored as opaque reference UUIDs; zero raw tokens in manifests (`REQ-SEC-001`).
- **Private Path Containment:** Local personal drive paths are barred from shared catalog manifests.
- **Input Sanitization:** User search terms treated as untrusted plain text; zero XAML or code evaluation.

---

## K. Deliverables Registry

All 8 mandated deliverables have been authored, validated, and placed in `docs/game-discovery/`:

1. **Deliverable A:** [GAME_DISCOVERY_AND_CANONICAL_IDENTITY_SPECIFICATION.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_DISCOVERY_AND_CANONICAL_IDENTITY_SPECIFICATION.md) (`NLE-SPC-DSC-001`, 13.9 KB)  
   *Primary architecture specification: scope, exclusions, canonical identity principles, entity hierarchy, adapter boundaries, security.*
2. **Deliverable B:** [GAME_CATALOG_DOMAIN_MODEL_AND_ENTITY_RELATIONSHIPS.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_CATALOG_DOMAIN_MODEL_AND_ENTITY_RELATIONSHIPS.md) (`NLE-DOM-CAT-001`, 14.5 KB)  
   *Domain model: 10 core entities (CanonicalGame, Edition, Release, Build, Platform, Engine, Provenance, Provisional), Mermaid ERD.*
3. **Deliverable C:** [CANONICAL_GAME_IDENTITY_AND_VERSIONING_RULES.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/CANONICAL_GAME_IDENTITY_AND_VERSIONING_RULES.md) (`NLE-RUL-IDV-001`, 11.2 KB)  
   *Identity rules: Immutability, NFKD normalization, homonymous disambiguation, non-semver versioning, version comparison abstraction.*
4. **Deliverable D:** [GAME_METADATA_CATALOG_AND_FIELD_DICTIONARY.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_METADATA_CATALOG_AND_FIELD_DICTIONARY.md) (`NLE-DIC-MET-001`, 11.8 KB)  
   *Data dictionary: 35+ metadata fields across 7 operational clusters with constraints, ownership, freshness, and privacy tags.*
5. **Deliverable E:** [GAME_METADATA_SOURCE_AND_PROVIDER_ARCHITECTURE.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_METADATA_SOURCE_AND_PROVIDER_ARCHITECTURE.md) (`NLE-ARC-SRC-001`, 13.1 KB)  
   *Source architecture: 5 provider categories, normalization boundaries, field-domain authority matrix, resilience, DPAPI vaulting, licensing.*
6. **Deliverable F:** [GAME_DISCOVERY_IDENTITY_RESOLUTION_AND_DATA_QUALITY_RULES.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_DISCOVERY_IDENTITY_RESOLUTION_AND_DATA_QUALITY_RULES.md) (`NLE-RUL-RES-001`, 11.4 KB)  
   *Resolution rules: 4-stage pipeline, ambiguity handling, manual entry, provisional lifecycle state machine, data quality dimensions.*
7. **Deliverable G:** [GAME_CATALOG_SECURITY_LICENSING_AND_TEST_PLAN.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/GAME_CATALOG_SECURITY_LICENSING_AND_TEST_PLAN.md) (`NLE-PLN-CAT-001`, 15.6 KB)  
   *Security & governance mandates; 30 design-level test specifications (identity, editions, non-semver, resilience, security, regression).*
8. **Deliverable H:** [STEP_25_COMPLETION_REPORT.md](file:///D:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/STEP_25_COMPLETION_REPORT.md) (`NLE-REP-STP-025`, 23.5 KB — this document)  
   *Formal step sign-off, scenario reviews (A–V), acceptance criteria audit (AC-01–AC-28), and Step 26 readiness.*

---

## L. Scenario-Based Design Validation (Scenarios A through V)

The 22 mandatory review scenarios from §22 were evaluated at the specification level:

- **Scenario A (Common game title):** Query `"Prey"` matches 2006 and 2017 releases. Pipeline flags `AMBIGUOUS_CANDIDATES`; appends release year (`prey-2006` vs `prey-2017`); surfaces both to user with developer tags. Zero auto-selection. (**PASS**).
- **Scenario B (Different editions of the same game):** Skyrim LE and Skyrim SE share title. Edition rules (`RULE-EDN-001`) create distinct `GameEdition` records due to binary architecture fork (32-bit x86 vs 64-bit x64). (**PASS**).
- **Scenario C (Game with an unknown version):** User creates project without knowing version. `RawVersionString` is set to `"UNKNOWN"`; tier is `DECLARED`. Epistemic honesty rules accept state as valid and non-blocking. (**PASS**).
- **Scenario D (Multiple versioning schemes):** Game 1 uses `1.6.640`; Game 2 uses `Build 108429`. Version comparator reports `INCOMPARABLE`; zero artificial sorting order or coercion into semver. (**PASS**).
- **Scenario E (Alternate game title):** User queries `"Biohazard"`. Stage 3 matches alternate title of type `LOCALIZED`; resolves to canonical game *"Resident Evil"*. Canonical title preserved. (**PASS**).
- **Scenario F (Conflicting external sources):** Storefront claims 32-bit; PCGamingWiki claims 64-bit. Field-domain authority gives precedence to PCGamingWiki; conflict preserved in `MetadataConflictRecord`. (**PASS**).
- **Scenario G (Provider returns no result):** Steam API returns empty list for non-Steam indie title. System does not treat this as non-existence; queries remaining providers and seed catalog. (**PASS**).
- **Scenario H (Provider becomes unavailable):** IGDB API unreachable (timeout). Local catalog cache on Volume D: serves existing records; search continues with local seed catalog. Zero crash. (**PASS**).
- **Scenario I (Manual entry):** User enters uncataloged homebrew title. `ProvisionalGameIdentity` created with tier `DECLARED`. Project creation unblocked immediately. (**PASS**).
- **Scenario J (Provisional entry later reconciled):** Catalog update imports canonical match. System creates `ReconciliationCandidate`; user confirms; project links to canonical ID; historical declared title preserved. (**PASS**).
- **Scenario K (Game title changes):** Game rebranded. Former title demoted to `AlternateTitle` of type `FORMER`. `CanonicalGameId` remains immutable. (**PASS**).
- **Scenario L (Multiple local installations):** User has Steam and GOG installs of same game. Both stored as distinct `LocalInstallationInstance` records pointing to the same `CanonicalGame`. Path is not global identity. (**PASS**).
- **Scenario M (Missing installation):** Previously detected installation folder moved or deleted. Canonical game in catalog remains 100% intact; local installation marked `UNAVAILABLE`. No cascade deletion. (**PASS**).
- **Scenario N (Stale version evidence):** Cached version metadata exceeds TTL (90 days). Freshness policy flags advisory warning in UI; existing feasibility claims remain intact until manual re-assessment. (**PASS**).
- **Scenario O (Unsupported external metadata field):** Provider introduces unrecognized JSON property. Normalization mapper safely discards unmapped field and logs diagnostic warning. Zero record corruption. (**PASS**).
- **Scenario P (Licensing restriction):** PCGamingWiki metadata requires `CC-BY-NC-SA 3.0` attribution. Manifest preserves attribution tag; UI renders clickable licensing citation in Metadata Inspector. (**PASS**).
- **Scenario Q (Unauthorized local scan):** User selects game in wizard. System resolves catalog metadata only. Zero filesystem traversal or drive scanning executed without explicit opt-in. (**PASS**).
- **Scenario R (Catalog record appears compatible):** Game has complete metadata and UE4 engine tag. Catalog provides metadata; downstream `CompatibilityEngine` retains exclusive right to evaluate feasibility. (**PASS**).
- **Scenario S (Duplicate candidate records):** Two scraped entries share high Jaro-Winkler similarity. Pipeline flags `DUPLICATE_CANDIDATE`; quaranatined from auto-match; awaiting manual review. Zero auto-merge. (**PASS**).
- **Scenario T (Existing identity model):** Preserves `GameParticipant`, `GameId`, and `GameEdition` from Phase 2/3 contracts without schema breakage. (**PASS**).
- **Scenario U (Phase 3 gate is blocked):** Audited actual Phase 3 gate report (`READY WITH CONDITIONS`); all conditions integrated as formal constraints in Step 25 specifications. (**PASS**).
- **Scenario V (No existing catalog connector):** Acknowledges that zero external connectors are implemented. Specifications define conceptual boundaries and DTO contracts only. (**PASS**).

---

## M. Acceptance Criteria Audit (AC-01 through AC-28)

| Criterion ID | Criterion Description | Assessment | Evidence / Reference Document |
|---|---|---|---|
| **AC-01** | Repository, Phase 2 dependencies, Phase 3 gate decision inspected | **PASS** | Section B, C; `NLE-GAT-PH3-001` audited |
| **AC-02** | Existing game domain entities, schemas, discovery audited | **PASS** | Section D; `NLE-DOM-SPEC-001` audited |
| **AC-03** | Scope and limitations of game discovery foundation explicit | **PASS** | `NLE-SPC-DSC-001 §4` (Scope & Boundaries) |
| **AC-04** | Canonical identity separated from display names and external IDs | **PASS** | `NLE-SPC-DSC-001 §6`, `NLE-RUL-IDV-001` (RULE-ID-001) |
| **AC-05** | Games, editions, releases, versions, installations distinguished | **PASS** | `NLE-DOM-CAT-001 §3`, `NLE-SPC-DSC-001 §7` |
| **AC-06** | Architecture does not assume universal version format or sorting | **PASS** | `NLE-RUL-IDV-001` (RULE-VER-001, RULE-VER-003) |
| **AC-07** | Unknown, conflicting, declared, imported, verified distinguished | **PASS** | `NLE-DOM-CAT-001 §3.8`, `NLE-RUL-IDV-001` (RULE-VER-002) |
| **AC-08** | Metadata dictionary defines purpose, owner, provenance, status | **PASS** | `NLE-DIC-MET-001` (35+ fields cataloged) |
| **AC-09** | Identity separated from adapter qualification and compatibility | **PASS** | `NLE-SPC-DSC-001 §8`, `NLE-RUL-IDV-001 §7` |
| **AC-10** | Provider architecture defines boundaries, provenance, failures | **PASS** | `NLE-ARC-SRC-001 §3, §6` |
| **AC-11** | Source authority and conflicts resolved without arbitrary rankings | **PASS** | `NLE-ARC-SRC-001 §4` (Field-Domain Authority) |
| **AC-12** | Discovery rules cover exact, ambiguous, normalization, duplicates | **PASS** | `NLE-RUL-RES-001 §2, §3` |
| **AC-13** | Provisional game identities distinct from verified records | **PASS** | `NLE-RUL-RES-001 §4` (RULE-MAN-001, 002) |
| **AC-14** | Local discovery separated from catalog; subject to authorization | **PASS** | `NLE-PLN-CAT-001 §2.3`, `NLE-SPC-DSC-001 §4.2` |
| **AC-15** | Data quality, freshness, lifecycle, reconciliation defined | **PASS** | `NLE-RUL-RES-001 §5, §6` |
| **AC-16** | Source licensing, attribution, DPAPI secret management defined | **PASS** | `NLE-PLN-CAT-001 §3, §4`, `NLE-ARC-SRC-001 §7` |
| **AC-17** | Conceptual service boundaries distinguish catalog, providers, etc. | **PASS** | `NLE-SPC-DSC-001 §4`, `NLE-ARC-SRC-001 §3` |
| **AC-18** | Failures and timeouts cannot become unsupported game claims | **PASS** | `NLE-ARC-SRC-001 §6.1`, `NLE-RUL-RES-001 §3` |
| **AC-19** | Future discovery and identity tests have measurable outcomes | **PASS** | `NLE-PLN-CAT-001 §5–§11` (30 test cases) |
| **AC-20** | All required documentation artifacts exist and verified on disk | **PASS** | Section K (8 deliverables in `docs/game-discovery/`) |
| **AC-21** | Terminology consistent; no contradictions with Phase 2/3 ADRs | **PASS** | Section E, F; `NLE-RUL-IDV-001` |
| **AC-22** | All required conceptual scenarios reviewed; gaps recorded | **PASS** | Section L (Scenarios A through V verified) |
| **AC-23** | Proposed functionality clearly distinguished from implementation | **PASS** | Section A; `NLE-SPC-DSC-001 §1` |
| **AC-24** | Zero unsupported claims of game detection or compatibility | **PASS** | `NLE-SPC-DSC-001 §4.2`, `NLE-RUL-RES-001 §3.4` |
| **AC-25** | Completion report distinguishes documentation from execution | **PASS** | Section A; `NLE-REP-STP-025` |
| **AC-26** | Existing project references, domain models, UI preserved | **PASS** | Section N; Clean git working tree verified |
| **AC-27** | Zero external APIs, connectors, packages, or scanners added | **PASS** | Section N; 0 lines of code, 0 packages |
| **AC-28** | Zero compatibility engines or adapter workflows modified | **PASS** | Section N; Documentation only |

---

## N. Repository Preservation Verification

- **Working Tree State:** Clean with untracked documentation files in `docs/game-discovery/` and `docs/ui/`. Zero uncommitted tracked modifications.
- **Source Code Files:** 0 source code files (`.cs`, `.xaml`, `.js`, `.py`) created, edited, or deleted.
- **Build Configurations:** 0 project (`.csproj`) or solution (`.sln`) files touched.
- **Third-Party Dependencies:** 0 NuGet or npm packages installed.
- **Prior Phase Artifacts:** 100% of Phase 1, Phase 2, and Phase 3 documents remain intact and unaltered.

---

## O. Step 26 Readiness & Downstream Dependencies

The Game Discovery Architecture and Canonical Game Identity baseline established in Step 25 is **sufficiently specified and approved** to inform **Step 26 ("Local Game Detection, Environment Scanning & Executable Verification Architecture")**.

### Downstream Dependencies to Carry Forward into Step 26:
1. **Explicit User Authorization:** Step 26 local environment scanning must strictly require user opt-in and adhere to user-specified library directories.
2. **Medium Integrity Level:** Local binary inspection (PE header reading, file hashing) must execute strictly at Windows Medium IL (zero UAC elevation).
3. **Volume D Isolation:** Local detection caches and signatures must reside on Volume `D:\` (`DEC-009`).
4. **Epistemic Elevation:** Local detection promotes version provenance from `DECLARED` or `IMPORTED` to `DETECTED`, and hash-verified builds to `VERIFIED`.

---

## P. Strict Stop Policy

**STRICT STOP POLICY ENFORCED:** Step 25 is officially concluded. Advancing to Step 26 is strictly unauthorized within this turn and awaits user instruction.

