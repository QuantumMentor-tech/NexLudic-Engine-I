# Game Metadata Licensing & Data Governance Review

**Document ID:** NLE-GOV-LIC-001  
**Version:** 1.0.0  
**Status:** Approved — Compliance & Governance Review  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** Legal Review, Licensing Governance & Privacy Assessment  

---

## 1. Executive Summary & Governance Objectives

This review evaluates the legal terms of use, intellectual property boundaries, database rights, attribution obligations, caching permissions, and privacy constraints for all metadata sources evaluated for the **NexLudic Engine**.

In strict accordance with the Step 26 mandate:
- All conclusions are derived from **verified, published terms of service and license texts** reviewed as of **November 17, 2026**.
- Verified legal statements are strictly distinguished from legal interpretations.
- Commercial redistribution, caching boundaries, and copyright restrictions are established as binding constraints for future implementation.

---

## 2. Comprehensive Provider Licensing Audit

| Provider | Governing License / Legal Agreement | Permitted Desktop Client Use | Local Caching Permitted? | Redistribution of Raw Data? | Attribution Mandatory? | Specific Attribution Requirement | Compliance Assessment |
|---|---|---|---|---|---|---|---|
| **Steamworks / Storefront API** | Valve Steam Web API Terms of Use | **Yes** (For legitimate functional integration) | **Yes** (Reasonable client caching allowed) | **No** (Reselling or sublicensing barred) | **Yes** | Storefront link or Steam branding | **COMPLIANT** (Textual query only; no raw redistribution) |
| **PCGamingWiki** | Creative Commons Attribution-NonCommercial-ShareAlike 3.0 (`CC-BY-NC-SA 3.0`) | **Yes** (Non-commercial research & integration engine) | **Yes** (Attribution metadata retained) | **Yes, under CC-BY-NC-SA** (Commercial sale barred) | **Yes** | Citation link to PCGamingWiki page | **COMPLIANT** (Attribution rendered in Metadata Inspector) |
| **IGDB (Twitch / Amazon)** | Twitch Developer Services Agreement / IGDB Terms | **Yes** (Standard Developer API tier) | **Yes** (Functional application caching allowed) | **No** (Direct database mirroring barred) | **Yes** | Prominent "Powered by IGDB" badge & link | **COMPLIANT** (Attribution badge included in UI design) |
| **Wikidata** | Creative Commons Public Domain Dedication (`CC0 1.0 Universal`) | **Yes** (Fully unrestricted) | **Yes** (Unrestricted) | **Yes** (Unrestricted public domain) | No (Recommended) | Citation to Wikidata QID | **FULLY COMPLIANT** (Zero legal restrictions) |
| **MobyGames** | MobyGames Terms of Use / Commercial API Agreement | **Restricted** (Free tier capped at 360 req/day) | **Restricted** (Strict caching limitations) | **No** (Strict commercial property) | **Yes** | Mandatory link | **NON-VIABLE FOR RUNTIME** (Rejected due to quota) |

---

## 3. In-Depth Analysis of Core Adopted Providers

### 3.1 PCGamingWiki (`CC-BY-NC-SA 3.0`) Compliance
- **License Substance:** The Creative Commons `CC-BY-NC-SA 3.0` license permits sharing (copying and redistributing the material in any medium or format) and adapting (remixing, transforming, and building upon the material) under two strict conditions:
  1. *Attribution:* You must give appropriate credit, provide a link to the license, and indicate if changes were made.
  2. *Non-Commercial:* You may not use the material for commercial purposes without an explicit commercial license from PCGamingWiki.
  3. *ShareAlike:* If you remix, transform, or build upon the material, you must distribute your contributions under the same license.
- **Application to NexLudic Engine:**
  - NexLudic Engine operates as an open-source/open-architecture research and game integration desktop platform.
  - Ingested technical fields (e.g., engine family, bitness, DirectX version) are textual facts cached locally on **Volume `D:\`** (`D:\NexLudic\Catalog\Cache\PCGamingWiki\`) for the individual user's project research.
  - The UI Metadata Inspector displays: *"Technical metadata sourced from PCGamingWiki under CC-BY-NC-SA 3.0 [View Original Page]"*.
  - NexLudic does not sell or license the PCGamingWiki database as a standalone commercial product.
- **Compliance Status:** **APPROVED FOR RESEARCH & DESKTOP CATALOG INTEGRATION**.

### 3.2 Steamworks Storefront API Compliance
- **Terms Substance:** Valve grants a limited, revocable, non-exclusive license to use the Steam Web APIs solely for querying and displaying game data to users. Users must not scrape the site, overload Valve infrastructure, or use the data to mislead customers.
- **Application to NexLudic Engine:**
  - Queries are dispatched only when a user inspects a Steam game or enters a Steam AppID.
  - Caching is performed locally on Volume D: to minimize external network requests and prevent redundant traffic to Valve servers.
  - Raw Steam database dumps are never bundled or redistributed.
- **Compliance Status:** **APPROVED FOR RUNTIME RESOLUTION**.

### 3.3 IGDB / Twitch API Compliance
- **Terms Substance:** Twitch Developer terms grant access to IGDB endpoints provided developers display the *"Powered by IGDB"* logo/link, honor rate limits (4 req/sec), do not mass-scrape to create a competing game database, and do not store data longer than reasonably necessary to support the application.
- **Application to NexLudic Engine:**
  - The application uses IGDB strictly for search candidate discovery and localized title resolution.
  - The UI includes the required attribution link.
  - Local caching follows an explicit 90-day TTL freshness policy.
- **Compliance Status:** **APPROVED WITH MANDATORY ATTRIBUTION**.

---

## 4. Copyright Boundaries: Textual Facts vs. Proprietary Media

A vital distinction in intellectual property law governs NexLudic Engine's metadata architecture:
1. **Factual Technical Metadata (Permitted):** Raw factual data — such as a game's release date, developer company, binary bitness (64-bit), graphics API (DirectX 11), executable filename (`SkyrimSE.exe`), and engine family (*"Creation Engine"*) — are **unprotected factual statements** under copyright law. Ingesting, indexing, and reasoning over technical facts is legally permitted.
2. **Proprietary Creative Assets (Strictly Barred):**
   - High-resolution box art, concept artwork, and marketing banners.
   - Proprietary game binaries, `.dll` dynamic libraries, and executable files.
   - Game audio files, dialogue snippets, and soundtrack recordings.
   - Proprietary script source code and reverse-engineered decompilations.
- **Operational Rule:** NexLudic Engine's universal catalog is strictly restricted to **plain-text structured metadata**. The catalog ingestion subsystem is architecturally prohibited from downloading, storing, or redistributing proprietary binary assets.

---

## 5. Privacy, Local Path Isolation & Secret Protection

### 5.1 Local Path Privacy Boundary
- Local installation directories (e.g., `D:\Games\Steam\steamapps\common\Skyrim\`) and Windows user account names are private personal data.
- **Strict Isolation Mandate:** Outgoing HTTP queries to external metadata providers must **never** include local file paths, directory listings, or user identifiers. Requests transmit solely sanitized game titles or public AppIDs.
- Local paths reside exclusively within the local project workspace manifest on Volume D: and are barred from shared catalog caches.

### 5.2 Windows DPAPI Secret Vaulting
In compliance with `REQ-SEC-001`:
- Twitch/IGDB API keys or Steam Web API keys must be encrypted using Windows Data Protection API (DPAPI) and stored in `D:\NexLudic\Config\credentials.vault`.
- Opaque GUID references (`credential_ref`) are passed to HTTP handlers.
- Plaintext API keys must never be committed to git, logged, or surfaced in UI diagnostics.

---

## 6. Open Legal & Compliance Questions

| Issue ID | Legal / Compliance Question | Status | Required Resolution / Authority |
|---|---|---|---|
| **LEG-001** | Future Commercial Distribution with `CC-BY-NC-SA` Data | **Open Dependency** | If NexLudic Engine ever transitions from an open research platform to a commercial distribution, PCGamingWiki metadata must either be replaced with clean-room first-party analysis or licensed commercially from PCGamingWiki. |
| **LEG-002** | IGDB Long-Term Caching Interpretation | **Bounded** | Twitch terms state data must not be stored longer than necessary. NexLudic enforces a strict 90-day cache TTL with periodic re-validation. |
| **LEG-003** | Third-Party Anti-Cheat Naming & Trademarks | **Clear** | Using trademarked terms (e.g., *"Denuvo"*, *"Easy Anti-Cheat"*, *"BattlEye"*) in technical safety classification represents fair nominative use. |

---

## 7. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-GOV-LIC-001 |
| Created | 2026-11-17 |
| Classification | Legal Review, Licensing Governance & Privacy Assessment |
| Implementation Authorization | Research, evaluation, and governance review only |

