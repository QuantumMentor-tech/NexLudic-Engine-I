# Game Discovery Architecture & Canonical Game Identity Specification

**Document ID:** NLE-SPC-DSC-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Specification Baseline  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 25 of 100  
**Date:** 2026-11-17  
**Parent Framework:** NexLudic Engine System Architecture & Development Foundation  
**Classification:** Core Architecture & Discovery Specification  

---

## 1. Executive Summary & Architectural Scope

The **Game Discovery Architecture & Canonical Game Identity Specification** establishes the foundational architecture for identifying, discovering, cataloging, and contextualizing games within the **NexLudic Engine** platform.

NexLudic Engine is a universal, AI-assisted desktop platform designed to evaluate, design, develop, validate, and manage integrations between games across three integration archetypes:
1. **Host-Game Modification:** Introducing compatible external functionality into a host game via runtime or asset modification.
2. **Live Interoperability Bridge:** Coordinating separate game processes concurrently through validated inter-process communication (IPC).
3. **Hybrid Integration:** Combining live interoperability and local modification where justified by requirements and evidence.

To support these workflows, the engine requires an **evidence-aware, implementation-neutral, and extensible canonical game identity system**. A user may refer to a game by its casual commercial name, storefront listing, publisher edition, executable filename, modded runtime, or platform SKU. These representations often refer to related but technically distinct entities. This specification formalizes the canonical identity model, separation of concerns, boundaries between universal catalog metadata and game-specific adapters, and strict data-provenance rules.

### Implementation Authorization Notice
> [!IMPORTANT]
> **Specification Mode Only:** In strict accordance with the Step 25 mandate, this document establishes architectural models, conceptual contracts, and data boundaries. Zero application code, database tables, external API connectors, local drive scanners, or compatibility scoring engines are implemented.

---

## 2. Upstream Architecture & Phase 3 Gate Verification

### 2.1 Phase 3 Gate Audit
The Phase 3 Gate Review (`NLE-GAT-PH3-001`, `STEP_24_COMPLETION_REPORT.md`) concluded with a formal determination of **`READY WITH CONDITIONS`**. All five mandatory governance conditions directly govern the design of this Phase 4 specification:
- **Condition 1 (Presentation Decoupling):** Catalog discovery models are pure domain entities, decoupled from WPF UI controls or XAML presentation.
- **Condition 2 (Storage & Drive Allocation):** All local catalog caches, manifests, and discovery indexes must reside strictly on **Volume `D:\`** (`DEC-009`) under `D:\NexLudic\`.
- **Condition 3 (Staging Jail & Diff Gating):** Discovery services never deploy files to game installations; all external files are isolated to staging (`ADR-0009`).
- **Condition 4 (Windows DPAPI Secret Vault):** External metadata provider credentials must interface through opaque DPAPI references (`REQ-SEC-001`). Raw API tokens must never be stored in catalog manifests.
- **Condition 5 (Deferred Relational Storage):** Relational databases (SQLite) remain formally deferred (`DEC-ARC-007`). Catalog specifications define conceptual models and JSON manifest structures without requiring an RDBMS.

### 2.2 Authoritative Upstream Contracts
- **`NLE-DOM-SPEC-001` (Core Domain Model):** Defines `GameId`, `GameEdition`, `GameVersionRef`, `GameSignature`, and epistemic tiers (`DECLARED`, `DETECTED`, `PROBED`, `VERIFIED`).
- **`NLE-CAT-WIZ-001` (Project Creation Catalog):** Establishes two-game selection rules (`Game A != Game B`), title search constraints, and Volume D path enforcement (`VR-PATH-002`).
- **`REQ-SAFE-001` (Safety Baseline):** Mandates immediate hard block of games protected by kernel-level drivers or active online competitive anti-cheat.
- **`REQ-SEC-001` (Security Baseline):** Enforces execution at Windows Medium Integrity Level (zero UAC elevation prompts).

---

## 3. Mandatory Engineering Principles

1. **Canonical Identity Invariance:** A game must possess an immutable, unique, internal canonical identifier (`CanonicalGameId`) that is permanently decoupled from display names, commercial titles, store URLs, or marketing rebranding.
2. **Entity Separation:** Titles, editions, releases, versions/builds, local installations, runtimes, modding frameworks, adapters, and compatibility outcomes must remain distinct concepts.
3. **Evidence Over Assumptions:** Metadata claims must record their source, capture context, and verification tier. Unknown is an explicitly valid epistemic state, never to be coerced into a false value.
4. **Universal Core Decoupling:** The universal catalog contains zero game-specific heuristics, modding hooks, or reverse-engineering logic. Game-specific intelligence belongs strictly inside isolated adapters (`NLE-ARC-ADP-001`).
5. **No Automatic Authorization:** Discovering or identifying a game record in a catalog implies zero permission to inspect, scan, modify, or execute game binaries on the host system.
6. **Licensing & Privacy Accountability:** Metadata sources must respect provider terms, attribution requirements, and copyright boundaries. Public metadata must never leak private local installation paths.

---

## 4. Scope, Responsibilities, and Boundary Matrix

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                               NEXLUDIC PLATFORM LAYERS                                 │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  1. PRESENTATION LAYER (Phase 3)                                                      │
│     • Game Selection UI  • Catalog Search  • Manual Game Add  • Version Indicator     │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  2. GAME DISCOVERY & CANONICAL CATALOG (Phase 4 — Step 25 Focus)                       │
│     • Canonical Identity Resolution  • Edition & Release Mapping  • Metadata Ingestion │
│     • Source Provenance Tracking     • Name Normalization         • Provisional Records│
├────────────────────────────────────────────────────────────────────────────────────────┤
│  3. LOCAL ENVIRONMENT DETECTION (Phase 4 — Downstream)                                │
│     • Authorized Filesystem Inspection  • Local Binary Probing  • Path Resolution     │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  4. COMPATIBILITY INTELLIGENCE & ADAPTER MATCHING (Phase 5+)                           │
│     • Capability Contracts  • Feasibility Gate Evaluation  • Mod Adapter Binding       │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### 4.1 Primary Responsibilities of Game Discovery
- Establish stable, collision-free canonical identities for games.
- Distinguish games with identical or overlapping display titles (e.g., reboots, remasters).
- Map editions, variant releases, and platform distributions to the appropriate canonical parent.
- Ingest and normalize metadata from diverse external sources while retaining source provenance.
- Manage the lifecycle of provisional, user-defined, and verified catalog entries.
- Expose standardized, implementation-neutral query and retrieval contracts to downstream consumers.

### 4.2 Explicit Exclusions (What Game Discovery Does NOT Do)
- **Does NOT Prove Installation:** A catalog entry does not establish that the game exists on the user's drive.
- **Does NOT Scan Filesystems:** Catalog discovery operates on metadata; it does not traverse drives or inspect executables.
- **Does NOT Determine Compatibility:** The catalog provides facts; it does not evaluate whether two games can be integrated.
- **Does NOT Qualify Adapters:** Adapter support declarations are evaluated by compatibility intelligence, not the catalog.
- **Does NOT Validate Ownership:** Catalog discovery does not inspect storefront licenses, DRM entitlements, or user purchases.
- **Does NOT Bypass Anti-Cheat:** Identifying an anti-cheat protected title flags it for downstream safety blocking; it never seeks to circumvent protections.

---

## 5. Conceptual Distinctions: Discovery vs. Selection vs. Verification

To maintain strict epistemic integrity across the engine, the following concepts are permanently separated:

| Operation / State | Definition | Authoritative Owner | Outcome Artifact |
|---|---|---|---|
| **Discovery Query** | Searching available metadata sources for matching game records. | `GameDiscoveryService` | Candidate List (`IReadOnlyList<GameMatchCandidate>`) |
| **Identity Resolution** | Resolving a query or external source record to a single canonical entity. | `GameIdentityResolver` | `CanonicalGameId` or Ambiguous Status |
| **Project Selection** | User explicitly binds a canonical game to a project slot (Host / Donor). | Project Creation Wizard | `GameParticipant` in `GamePairProject` manifest |
| **Installation Detection** | User-authorized local probing to locate game executables on disk. | `LocalInstallationDetector` (Future) | `LocalInstallationInstance` with physical path |
| **Version Verification** | Cryptographic or binary inspection establishing exact build identity. | `BinaryInspectionService` (Future) | `GameSignature` with Tier 4 Verification |
| **Compatibility Claim** | Evaluated determination that a capability can be achieved. | `CompatibilityEngine` (Phase 5) | `FeasibilityAssessment` Gate Decision |

---

## 6. The Canonical Game Identity Model

### 6.1 Canonical Game Identifier (`CanonicalGameId`)
Every canonical game in the NexLudic catalog is identified by an immutable `CanonicalGameId`:

$$\text{CanonicalGameId} = \text{Namespace} + \text{UUIDv5}(\text{Namespace\_DNS}, \text{NormalizedSourceAnchor})$$

- **Immutability:** Once assigned to a canonical title, the identifier never changes, even if the title is renamed, remastered, or acquired by a new publisher.
- **Collision Resistance:** UUIDv5 deterministic generation ensures identical source anchors produce identical IDs without distributed coordination, while distinct products generate collision-free keys.
- **Format:** Strongly-typed string identifier adhering to: `nle:game:[a-f0-9]{8}-[a-f0-9]{4}-5[a-f0-9]{3}-[89ab][a-f0-9]{3}-[a-f0-9]{12}`.

### 6.2 Display Name vs. Canonical Identity
A common failure mode in game catalogs is treating the display title as the primary key. In NexLudic Engine:
- **Canonical Title:** The official, internationally recognized base title in English (e.g., *"DOOM"*).
- **Normalized Key:** Stripped of casing, punctuation, and leading articles for search indexing (e.g., `doom`).
- **Release Year:** Serves as a vital disambiguator for reboots and homonymous titles (e.g., *"DOOM (1993)"* vs. *"DOOM (2016)"*).
- **Aliases & Localized Titles:** Linked 1-to-many from the canonical record; alternate titles never create duplicate canonical games.

### 6.3 Provisional vs. Verified Identity
When a user inputs a game not present in any official or community catalog:
- A **`ProvisionalGameIdentity`** is instantiated with status `PROVISIONAL`.
- It carries the user-supplied raw title, an ephemeral slug, and epistemic tier `DECLARED`.
- It allows immediate unblocking of project creation (`NLE-SPC-WIZ-001`).
- It remains strictly partitioned from verified catalog records until formally reconciled.

---

## 7. Hierarchical Game Domain Architecture

The relationship between conceptual titles, editions, releases, versions, and installations is structured as an explicit hierarchical tree:

```
[Canonical Game] (e.g., "The Elder Scrolls V: Skyrim", ID: nle:game:skyrim)
       │
       ├── [Game Edition] (e.g., "Special Edition", ID: nle:edition:skyrim-se)
       │         │
       │         ├── [Release / Distribution] (e.g., "Steam PC", Platform: Win64)
       │         │         │
       │         │         ├── [Version / Build] (e.g., "v1.6.640", BuildID: 9812451)
       │         │         │         │
       │         │         │         └── [Local Installation] (e.g., "D:\Games\Steam\...", Executable: SkyrimSE.exe)
       │         │         │
       │         │         └── [Version / Build] (e.g., "v1.5.97", BuildID: 4321560)
       │         │
       │         └── [Release / Distribution] (e.g., "GOG PC", Platform: Win64, DRM-free)
       │
       └── [Game Edition] (e.g., "Original / Legendary Edition", ID: nle:edition:skyrim-le, 32-bit x86)
                 │
                 └── [Release / Distribution] (e.g., "Steam PC", Platform: Win32)
```

### Technical Significance for Integration:
- In the example above, **Skyrim LE** is a 32-bit DirectX 9 runtime, while **Skyrim SE** is a 64-bit DirectX 11 runtime.
- Treating them as the same entity would cause catastrophic adapter failure: a 64-bit DLL injector cannot hook a 32-bit executable.
- The hierarchy guarantees that downstream compatibility intelligence assesses the exact technical variant without polluting the top-level user search.

---

## 8. Downstream Adapter and Compatibility Boundaries

### 8.1 Universal Catalog vs. Adapter Responsibilities

| Information / Responsibility | Catalog Domain | Adapter SDK Domain (`NLE-ARC-ADP-001`) |
|---|---|---|
| Canonical Title, Publisher, Release Date | **Authoritative Owner** | Consumer (for display) |
| Known Distribution Channels & App IDs | **Authoritative Owner** | Consumer (for launch hooks) |
| Engine Family & Known Architecture (x64/x86) | **Authoritative Owner** | Validation Constraint |
| Binary Injection Vectors & Memory Offsets | **Excluded** | **Authoritative Owner** |
| Hooking Strategies & Detour Hooks | **Excluded** | **Authoritative Owner** |
| Modding Framework Requirements (SKSE, UE4SS) | Metadata Reference Only | **Authoritative Owner** |
| Capability Delivery Feasibility | **Excluded** | **Authoritative Co-Owner** |

### 8.2 Compatibility Invalidation Contracts
Catalog metadata updates must never silently corrupt or mutate existing project feasibility decisions:
1. **Catalog Update Isolation:** If an external metadata refresh alters a game's engine version or architecture tag, existing `GamePairProject` records retain their pinned historical snapshot.
2. **Flagged Re-Assessment:** The project is flagged with an advisory notice: *"Underlying game catalog metadata has changed. Re-run Feasibility Audit to verify integration plan."*
3. **No Automated Invalidation:** Re-assessment is an explicit user-initiated or policy-gated action; the catalog layer never modifies project status automatically.

---

## 9. Security, Privacy, and Licensing Boundaries

### 9.1 Windows Medium Integrity Level Enforcement
In strict compliance with `REQ-SEC-001`:
- All catalog caching, query processing, and metadata parsing execute at Windows Medium Integrity Level.
- No catalog discovery operation may request UAC elevation, invoke administrative helper processes, or inspect protected system directories.

### 9.2 Volume D Storage Mandate
In strict compliance with `DEC-009`:
- All imported catalog databases, search index caches, and provider manifests must be stored under `D:\NexLudic\Catalog\`.
- Operating system drive `C:\` must never be used for catalog storage or caching.

### 9.3 Data Privacy & Installation Isolation
- **No Path Leakage:** Local installation paths (e.g., `D:\SteamLibrary\steamapps\common\...`) belong exclusively to the local user's machine. They must never be embedded in shareable project blueprints, exported catalogs, or telemetry.
- **Explicit Authorization:** Scanning local storage for game installations requires affirmative, explicit user opt-in with user-specified directory scopes.

### 9.4 Licensing and Attribution Compliance
- **Attribution Preservation:** Metadata imported from community APIs (e.g., PCGamingWiki, IGDB) must retain source attribution flags and license tags (`CC-BY-NC-SA`, etc.) in the manifest.
- **Copyrighted Asset Exclusion:** The universal catalog stores textual metadata and structured specifications only. Binary game files, mod archives, proprietary game art, and copyrighted audio assets are strictly barred from catalog ingestion.

---

## 10. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 25 — Game Discovery Architecture & Canonical Game Identity Specification |
| Document ID | NLE-SPC-DSC-001 |
| Created | 2026-11-17 |
| Classification | Core Architecture & Discovery Specification |
| Implementation Authorization | Research, architecture, and documentation only |

