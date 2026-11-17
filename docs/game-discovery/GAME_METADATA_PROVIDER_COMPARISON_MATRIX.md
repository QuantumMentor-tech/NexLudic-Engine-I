# Game Metadata Provider Comparison Matrix

**Document ID:** NLE-MAT-PRV-001  
**Version:** 1.0.0  
**Status:** Approved — Comparative Evaluation Matrix  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** Provider Research & Comparative Evaluation  

---

## 1. Overview and Methodology

This document provides a structured, side-by-side comparative analysis of the seven investigated metadata sources.

Each provider was evaluated against sixteen standardized evaluation dimensions using current primary documentation reviewed as of **November 17, 2026**.

In accordance with mandatory research principles:
- Numerical ratings that disguise missing facts are strictly prohibited.
- Licensing and legal permissions are evaluated as hard operational gates.
- Missing capabilities are explicitly designated as `UNAVAILABLE` or `UNVERIFIED`.

---

## 2. Comprehensive Provider Comparison Matrix

| Evaluation Dimension | Steamworks / Storefront API | PCGamingWiki (Cargo API) | IGDB API (v4) | Wikidata (SPARQL / REST) | MobyGames API | RAWG Database API | NexLudic Seed Catalog |
|---|---|---|---|---|---|---|---|
| **Official Documentation** | `partner.steamgames.com` | `pcgamingwiki.com/wiki/API` | `api-docs.igdb.com` | `wikidata.org/wiki/Data_access` | `mobygames.com/info/api/` | `rawg.io/apidocs` | `D:\NexLudic\Catalog\Seed\` |
| **Source Category** | Commercial Storefront API | Community Technical Wiki | Commercial Catalog API | Open Semantic Knowledgebase | Commercial Historical DB | Commercial Catalog API | Curated Platform Asset |
| **Primary Identifier** | Steam AppID (Numeric) | Page Title / MediaWiki ID | IGDB ID (Numeric) | Wikidata QID (`Q[0-9]+`) | MobyGames ID | RAWG ID / Slug | `CanonicalGameId` (UUIDv5) |
| **Title & Alias Coverage** | Official store title only | Canonical + limited aliases | Canonical + localized + aliases | Extensive multilingual labels | English title + variants | Title + common slugs | Curated pilot titles |
| **Edition Distinction** | Packages / SubIDs / Bundles | Dedicated pages for SE/Remaster | Parent-game edition linking | Statements (P144 based on) | Physical / Regional releases | Basic edition tags | Curated edition splits |
| **Version & Build Depth** | Manifest Build IDs (authenticated) | Major patch history & branches | Sparse release version notes | Release date statements | Release dates by country | Patch notes (limited) | Pinned target versions |
| **Technical Engine Data** | **Unavailable** | **High** (Engine, version, flags) | **Low** (Coarse engine tags) | **Moderate** (Engine P408) | **Low** (Historical engine) | **Low** | **High** (Verified engine) |
| **Binary Bitness (x86/x64)** | **Unavailable** | **High** (Indexed field in Cargo) | **Unavailable** | **Sparse** | **Unavailable** | **Unavailable** | **High** (Verified x86/x64) |
| **Anti-Cheat Detection** | Coarse (Valve Anti-Cheat flag) | **High** (EAC, BattlEye, Vanguard) | **Unavailable** | **Sparse** | **Unavailable** | **Unavailable** | **High** (Verified safety) |
| **Mod Loader Metadata** | Steam Workshop tag only | **High** (SKSE, UE4SS, BepInEx) | **Unavailable** | **Unavailable** | **Unavailable** | **Unavailable** | **High** (Primary loader) |
| **Access Model & Auth** | Public REST / API Key (`ISteamApps`)| Public REST (`Api-User-Agent`) | OAuth2 Client Credentials | Public SPARQL / REST | API Key Required | API Key Required | 100% Local File Access |
| **Published Rate Limits** | ~200 req / 5 min (public) | 1 req / second (recommended) | 4 req / second (free tier) | 60s query execution cap | **Strict: 360 req / day** | 20,000 req / month | **Unlimited (0ms latency)**|
| **Usage Costs** | Free (requires Steam account) | Free | Free tier (Twitch Dev Account) | Free (Public Domain) | Paid commercial tier | Paid commercial tier | Zero cost |
| **Licensing Terms** | Steam Web API Terms of Use | `CC-BY-NC-SA 3.0` | Twitch Dev Terms (Attribution) | `CC0 1.0 Universal` | Restrictive Commercial | Commercial License Req. | NexLudic Core License |
| **Caching Permission** | Permitted for client caching | Permitted with attribution | Permitted for functional use | Unrestricted (`CC0`) | Strictly limited caching | Limited caching | Full local persistence |
| **Redistribution Rights** | No raw data redistribution | Non-commercial share-alike | No raw database redistribution | Unrestricted (`CC0`) | Strictly prohibited | Strictly prohibited | Proprietary distribution |
| **Operational Reliability** | High (Global Valve CDN) | Moderate (Community hosted) | High (AWS / Twitch infra) | High (Wikimedia Foundation) | Moderate | Moderate | Absolute (Offline local) |
| **Security / DPAPI Req.** | DPAPI key vaulting for Web API | None (Zero secrets needed) | DPAPI Client Secret vaulting | None (Zero secrets needed) | DPAPI API Key vaulting | DPAPI API Key vaulting | None |
| **Review Date** | 2026-11-17 | 2026-11-17 | 2026-11-17 | 2026-11-17 | 2026-11-17 | 2026-11-17 | 2026-11-17 |
| **Recommendation Status** | **Adopted: Primary Storefront** | **Adopted: Primary Technical** | **Adopted: Supplemental Search**| **Adopted: Semantic Anchor** | **Rejected: Quota Unusable** | **Deferred: Secondary** | **Adopted: Mandatory Seed** |

---

## 3. Evaluation of Recommendation Decisions

### 3.1 Adopted Core Providers (Multi-Source Strategy)
1. **NexLudic Seed Catalog (Mandatory Baseline):** Immediate offline lookup for pilot game pairs. Zero external dependencies.
2. **PCGamingWiki (Primary Technical Authority):** The **only** surveyed provider offering structured, indexed data for binary bitness (x86 vs x64), game engine versions, DirectX/Vulkan APIs, and anti-cheat middleware. Compliance secured under `CC-BY-NC-SA 3.0` via non-commercial desktop attribution.
3. **Steamworks Storefront API (Primary Storefront Authority):** Authoritative resolution for Steam AppIDs, official packages, store URLs, and official release dates.
4. **IGDB API (Supplemental Search & Taxonomy Authority):** Broad game catalog discovery, international localized titles, developer/publisher credits, and franchise taxonomy.
5. **Wikidata (Semantic Cross-Reference Anchor):** Open, permanent semantic IDs (QIDs) linking Steam AppIDs and PCGamingWiki slugs under public domain (`CC0`).

### 3.2 Rejected / Deferred Providers
- **MobyGames (Rejected for Runtime Discovery):** The free tier quota of **360 requests per day** is mathematically unviable for interactive desktop discovery across multiple game pairs. Retained solely for offline historical research.
- **RAWG (Deferred):** Provides redundant capabilities to IGDB with less comprehensive engine metadata and more restrictive commercial terms.

---

## 4. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-MAT-PRV-001 |
| Created | 2026-11-17 |
| Classification | Provider Research & Comparative Evaluation Matrix |
| Implementation Authorization | Research, evaluation, and documentation only |

