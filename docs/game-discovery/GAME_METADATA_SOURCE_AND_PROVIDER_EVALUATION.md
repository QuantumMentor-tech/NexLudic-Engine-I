# Game Metadata Sources & Provider Evaluation Specification

**Document ID:** NLE-EVA-SRC-001  
**Version:** 1.0.0  
**Status:** Approved — Research & Evaluation Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Framework:** NexLudic Engine System Architecture & Development Foundation  
**Classification:** External Source Research & Provider Evaluation  

---

## 1. Executive Summary & Research Mandate

This document establishes the authoritative technical research, provider evaluation, and source-selection specification for game metadata within the **NexLudic Engine**.

In strict accordance with the Step 26 mandate:
- All evaluations are backed by **current, verified primary documentation** from official platform and provider portals reviewed as of **November 17, 2026**.
- The evaluation explicitly distinguishes **general game identification** (commercial title, developer, publisher) from **version-specific technical integration evidence** (binary architecture, game engine version, anti-cheat middleware, graphics APIs).
- The specification establishes an evidence-backed, multi-source ingestion strategy without implementing runtime connectors, creating live catalog databases, or making unauthorized production API calls.

### Core Research Conclusion
No single metadata provider satisfies all requirements for NexLudic Engine. Commercial storefronts (Steam) excel at store IDs and official release packaging but lack deep engine and anti-cheat metadata. General databases (IGDB, RAWG) offer broad taxonomy but lack binary bitness and mod loader details. Technical community wikis (PCGamingWiki) provide unmatched technical architecture data but operate under copyleft non-commercial licensing (`CC-BY-NC-SA 3.0`). Consequently, NexLudic Engine must adopt a **layered, multi-source authority architecture** anchored by a curated local offline seed catalog.

---

## 2. Upstream Verification & Boundary Audit

### 2.1 Verification of Prior Gates and Baselines
- **Phase 3 Gate Review (`NLE-GAT-PH3-001`):** Formally audited as **`READY WITH CONDITIONS`**. All five conditions (WPF presentation decoupling, Volume `D:\` storage, staging jail containment, DPAPI credential protection, and deferred SQLite) are strictly inherited.
- **Step 25 Baseline (`NLE-SPC-DSC-001`, `NLE-DOM-CAT-001`):** Establishes the canonical identity model (`CanonicalGameId`), the 5-tier entity hierarchy (Game $\rightarrow$ Edition $\rightarrow$ Release $\rightarrow$ Version $\rightarrow$ Installation), non-semver versioning preservation, and the 35+ field metadata dictionary (`NLE-DIC-MET-001`).

### 2.2 Operational Step Boundaries
- **Research and Specification Only:** Zero lines of C# / XAML code, zero third-party packages, zero database tables, and zero live automated scrapers are implemented.
- **Medium Integrity & Volume D:** In compliance with `REQ-SEC-001` and `DEC-009`, all future provider caches and manifests must reside on **Volume `D:\`** (`D:\NexLudic\Catalog\`) and operate at Windows Medium Integrity Level (zero UAC elevation).

---

## 3. Metadata Requirements Analysis

To support NexLudic Engine's three integration archetypes (Host Modification, Live Bridge, Hybrid Integration), metadata sources must satisfy four distinct information tiers:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        NEXLUDIC METADATA REQUIREMENT TIERS                             │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 1: CANONICAL IDENTITY & SEARCH                                                    │
│ • Canonical Title  • Normalized Slug  • Release Year  • Developer/Publisher  • Aliases │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 2: STOREFRONT & DISTRIBUTION CONTEXT                                              │
│ • Storefront Channel (Steam/GOG/Epic)  • Storefront AppID  • Packaging  • DRM Wrapper  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 3: TECHNICAL ENGINE & BINARY ARCHITECTURE                                         │
│ • Engine Family & Version  • Bitness (x86 vs x64)  • Scripting (Mono/IL2CPP)  • API    │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ TIER 4: MODDING ECOSYSTEM & SAFETY                                                     │
│ • Primary Mod Loader (SKSE/UE4SS)  • Anti-Cheat Middleware (EAC/BattlEye/Vanguard)     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### Critical Epistemic Rule
> [!IMPORTANT]
> **Metadata is NOT Proof of Compatibility:** A provider declaring that a game uses *"Unreal Engine 4.27"* provides technical context. It does **not** prove that a specific community adapter works, that a memory injection will succeed, or that two games can interoperate. Compatibility determination belongs exclusively to downstream feasibility intelligence (`CompatibilityEngine`, Phase 5).

---

## 4. Evaluated Candidate Providers

Seven candidate metadata sources were investigated against official documentation, API specifications, and current terms of service:

### 4.1 Steamworks Web API / Storefront API (Valve Corporation)
- **Primary References:** `partner.steamgames.com/doc/webapi_overview`, `store.steampowered.com/api/appdetails` (Reviewed: 2026-11-17).
- **Access Model:** Unauthenticated REST endpoint for `appdetails` (JSON payload); authenticated Web API requires a Steam Web API Key (`ISteamApps`). Rate limit: ~200 requests / 5 minutes on public endpoint.
- **Technical Strengths:** Absolute authority for Steam AppIDs, official release dates, package/DLC manifests, supported OS platforms, and Steam Workshop integration.
- **Technical Limitations:** Exposes zero engine family data, zero binary bitness (x86/x64), and coarse anti-cheat tags (often mentions VAC, but omits third-party kernel drivers like Easy Anti-Cheat or BattlEye in basic payloads).
- **Licensing & Terms:** Steam Web API Terms of Use. Permitted for desktop client queries; caching allowed for performance; redistribution of raw customer data or proprietary game assets is prohibited.
- **Evaluation Status:** **Recommended as Primary Authority for Steam Distribution & Storefront AppIDs**.

### 4.2 PCGamingWiki (PCGamingWiki Community)
- **Primary References:** `pcgamingwiki.com/wiki/PCGamingWiki:API`, MediaWiki / Cargo query endpoint `pcgamingwiki.com/api.php` (Reviewed: 2026-11-17).
- **Access Model:** REST API via MediaWiki Cargo queries. Requires descriptive `Api-User-Agent` header. Rate limit: 1 request / second recommended.
- **Technical Strengths:** Unrivaled technical granularity. Formally indexes: game engine family & version, 32-bit vs. 64-bit binary bitness, DirectX / Vulkan rendering APIs, anti-cheat & anti-tamper middleware (Denuvo, EAC, BattlEye, Vanguard), config/save paths, and community mod loaders.
- **Technical Limitations:** Community-maintained wiki; niche or unreleased titles may have incomplete pages; schema changes in Cargo tables can occur.
- **Licensing & Terms:** Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported (`CC-BY-NC-SA 3.0`). Non-commercial use within desktop tool permitted with visible attribution; cannot be bundled into a closed-source proprietary commercial database resale.
- **Evaluation Status:** **Recommended as Primary Authority for Technical Engine, Binary Bitness, Anti-Cheat, and Modding Framework Metadata**.

### 4.3 IGDB — Internet Game Database (Twitch / Amazon)
- **Primary References:** `api-docs.igdb.com` (IGDB API v4) (Reviewed: 2026-11-17).
- **Access Model:** Authenticated REST API. Requires Twitch Developer Portal account, OAuth2 Client Credentials flow generating short-lived App Access Tokens. Rate limit: 4 requests / second on free tier.
- **Technical Strengths:** Broad global catalog coverage (300,000+ titles), excellent franchise taxonomies, rich alternative and localized titles, developer/publisher company records, and cross-platform release dates.
- **Technical Limitations:** Technical engine fields are coarse or frequently unpopulated; lacks binary bitness and mod loader data; requires OAuth2 client credentials management.
- **Licensing & Terms:** Twitch Developer Services Agreement / IGDB API Terms. Requires public attribution link (*"Powered by IGDB"*); caching permitted for functional application needs; commercial redistribution prohibited without custom license.
- **Evaluation Status:** **Recommended as Supplemental Authority for Broad Catalog Search, Disambiguation & Localized Aliases**.

### 4.4 Wikidata (Wikimedia Foundation)
- **Primary References:** `wikidata.org/wiki/Wikidata:Data_access`, SPARQL Query Service `query.wikidata.org` (Reviewed: 2026-11-17).
- **Access Model:** Open SPARQL endpoint and MediaWiki REST API. Zero API keys required. Descriptive `User-Agent` required. 60-second execution timeout on complex SPARQL queries.
- **Technical Strengths:** Permanent semantic entity IDs (QIDs), extensive multi-lingual labels, and cross-catalog entity mapping properties (P1733 Steam AppID, P5794 PCGamingWiki ID, P856 official site).
- **Technical Limitations:** Querying SPARQL in real-time desktop UI can suffer latency spikes; technical engine versions are sparse.
- **Licensing & Terms:** Creative Commons Public Domain Dedication (`CC0 1.0 Universal`). Fully open public domain data; zero copyright restrictions on caching, embedding, or redistribution.
- **Evaluation Status:** **Recommended as Cross-Reference & Permanent Semantic Disambiguation Anchor**.

### 4.5 MobyGames (Atari Corporation / MobyGames)
- **Primary References:** `mobygames.com/info/api/` (Reviewed: 2026-11-17).
- **Access Model:** Authenticated API requiring API key. Free tier enforces an extremely restrictive limit of **1 request / second and a maximum of 360 requests / day**.
- **Technical Strengths:** Meticulous historical credit tracking, physical box editions, historical platform coverage.
- **Technical Limitations:** Free tier rate limit (360 requests/day) makes it unusable for interactive desktop search across active users; paid commercial tier requires enterprise contract.
- **Licensing & Terms:** Restrictive commercial licensing.
- **Evaluation Status:** **Rejected for Runtime Discovery / Reserved for Offline Manual Research Only**.

### 4.6 RAWG Video Games Database
- **Primary References:** `rawg.io/apidocs` (Reviewed: 2026-11-17).
- **Access Model:** API key required. Free tier rate limited (20,000 requests/month for personal use).
- **Technical Strengths:** Fast JSON responses, clean storefront mappings.
- **Technical Limitations:** Technical engine metadata is largely absent; commercial license required for commercial distributions.
- **Evaluation Status:** **Deferred as Secondary Fallback to IGDB**.

### 4.7 NexLudic Local Bundled Seed Catalog
- **Specification:** Stored locally on Volume `D:\` as `D:\NexLudic\Catalog\Seed\seed_catalog.json`.
- **Access Model:** 100% offline, zero network latency, zero credentials, zero rate limits.
- **Technical Strengths:** Curated specifically for NexLudic pilot portfolio game pairs (Skyrim SE, Fallout 4, Portal 2, Witcher 3, Cyberpunk 2077, Street Fighter 6, Tekken 8). All engine families, bitness, and anti-cheat tags are 100% verified.
- **Technical Limitations:** Static; requires application updates or downloaded manifest updates to expand.
- **Licensing & Terms:** Native platform asset under NexLudic Engine governance.
- **Evaluation Status:** **Adopted as Mandatory Offline Baseline & Immediate First-Launch Provider**.

---

## 5. Multi-Source Layered Authority Architecture

To eliminate single points of failure and maximize technical accuracy, NexLudic Engine adopts a **Layered Domain Authority Matrix**:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                          LAYERED METADATA RESOLUTION PIPELINE                          │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 1. LOCAL OFFLINE SEED CATALOG (Priority 0 — Instant 0ms Lookup)                        │
│    • Resolves Pilot Portfolio Game Pairs immediately without network access.           │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 2. CANONICAL IDENTITY & DISAMBIGUATION (Priority 1)                                    │
│    • Query: IGDB (Broad search) + Wikidata (QID anchor & cross-links).                 │
│    • Outputs: CanonicalTitle, NormalizedKey, ReleaseYear, Developer, AlternateTitles.  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 3. DISTRIBUTION & STOREFRONT MAPPING (Priority 2)                                      │
│    • Query: Steamworks API (AppID ↔ Package ↔ Release Date).                           │
│    • Outputs: StorefrontChannel, StorefrontAppId, OfficialPackaging, DRM wrapper tag.  │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 4. TECHNICAL ENGINE & SAFETY AUDIT (Priority 3)                                        │
│    • Query: PCGamingWiki (Cargo API).                                                  │
│    • Outputs: EngineFamily, EngineVersion, ArchitectureType (x86/x64), AntiCheatStatus │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 5. PROVENANCE RECORDING & CACHING ON VOLUME D:                                         │
│    • Assembles CanonicalGame entity with MetadataClaimProvenance records.              │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 6. Security, Privacy & Secret Management

1. **DPAPI Credential Protection:** All external API secrets (Twitch/IGDB client secrets) must be vaulted in the Windows DPAPI store (`REQ-SEC-001`). The application code refers exclusively to opaque UUID references (`credential_ref`).
2. **Zero Plaintext Tokens:** Raw API keys must never be written to project manifests, catalog caches on disk, git repositories, or diagnostic logs.
3. **Privacy Isolation:** User search terms and local file paths are strictly isolated. External provider queries transmit only sanitized game title strings or external AppIDs.

---

## 7. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-EVA-SRC-001 |
| Created | 2026-11-17 |
| Classification | External Source Research & Provider Evaluation |
| Implementation Authorization | Research, evaluation, and documentation only |

