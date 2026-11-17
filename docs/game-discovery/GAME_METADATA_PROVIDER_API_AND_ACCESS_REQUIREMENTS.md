# Game Metadata Provider API & Access Requirements

**Document ID:** NLE-REQ-API-001  
**Version:** 1.0.0  
**Status:** Approved — Technical Access Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** API Specifications, Access Protocols & Security Requirements  

---

## 1. Purpose and Scope

This document specifies the technical interface requirements, endpoints, query parameters, authentication flows, rate limits, error handling protocols, and credential security boundaries for the verified metadata providers adopted in the **NexLudic Engine Multi-Source Strategy**.

All specifications reflect **official primary documentation reviewed as of November 17, 2026**.

---

## 2. Provider Technical Access Specifications

### 2.1 Steamworks Storefront API & Web API (Valve Corporation)
- **Official Documentation:** `partner.steamgames.com/doc/webapi_overview` (Reviewed: 2026-11-17).
- **Access Protocol:** HTTPS REST (JSON).
- **Documented Endpoints:**
  1. *Storefront App Details (Public):*  
     `GET https://store.steampowered.com/api/appdetails?appids={app_id}&cc=us&l=en`  
     *Payload:* Returns official display title, detailed description, developers, publishers, release date, platform support (`windows`, `mac`, `linux`), package groups, DRM notices.
  2. *Application List (Public / Web API):*  
     `GET https://api.steampowered.com/ISteamApps/GetAppList/v2/`  
     *Payload:* Full mapping table of all Steam AppIDs to official store title strings.
- **Authentication:**
  - Storefront `appdetails` endpoint: **Unauthenticated** (No API key required).
  - Steamworks Partner Web API: Authenticated via query parameter `?key={STEAM_WEB_API_KEY}`.
- **Published Rate Limits:** Public storefront endpoint enforces IP-based rate limiting (~200 requests per 5-minute rolling window). HTTP 429 returned on breach.
- **Query & Lookup Capabilities:** Direct numeric AppID lookup; bulk query supported up to ~20 AppIDs per request (comma-delimited), but single AppID queries are recommended for caching stability.
- **Security & Secret Handling:** Steam Web API key (if configured for enhanced depot queries) must be stored in the Windows DPAPI store (`REQ-SEC-001`). Opaque reference `credential_ref` passed to provider.
- **Failure Responses:** HTTP 429 (Too Many Requests), HTTP 500 (Internal Store Error), or JSON `{"success": false}` for unlisted/region-locked AppIDs.

---

### 2.2 PCGamingWiki MediaWiki Cargo Query API
- **Official Documentation:** `pcgamingwiki.com/wiki/PCGamingWiki:API`, MediaWiki Extension:Cargo documentation (Reviewed: 2026-11-17).
- **Access Protocol:** HTTPS GET / POST against MediaWiki Action API (JSON).
- **Documented Endpoint:**  
  `GET https://www.pcgamingwiki.com/api.php?action=cargoquery&tables={tables}&fields={fields}&where={conditions}&format=json`
- **Primary Data Tables & Fields:**
  - `Infobox_game`: `_pageName`, `Developers`, `Publishers`, `Release_dates`, `Available_on`.
  - `Infobox_game_engine`: `Engine`, `Build`, `DirectX`, `Vulkan`, `64-bit_executable`, `Anti-cheat`.
  - `Infobox_game_middleware`: Third-party middleware, audio engines, physics engines.
- **Authentication:** **Unauthenticated**. No API key or user account required for read-only queries.
- **Mandatory Request Headers:** MediaWiki policy strictly requires a descriptive `Api-User-Agent` identifying the application and version:
  ```http
  Api-User-Agent: NexLudicEngine/1.0.0 (https://github.com/nexludic/engine; support@nexludic.local)
  ```
- **Published Rate Limits:** No rigid numerical cap published, but MediaWiki bot guidelines mandate polite queuing: maximum **1 request per second**; burst queries strictly prohibited.
- **Query Capabilities:** SQL-like `WHERE` clauses on indexed Cargo fields (e.g., `where=_pageName="The Elder Scrolls V: Skyrim"` or `where=Steam_AppID HOLDS "480"`). Pagination via `limit` (max 500 per query) and `offset`.
- **Failure Responses:** Standard MediaWiki error blocks: `{"error": {"code": "ratelimited", "info": "You've exceeded the rate limit."}}`.

---

### 2.3 IGDB API v4 (Twitch / Amazon)
- **Official Documentation:** `api-docs.igdb.com` (Reviewed: 2026-11-17).
- **Access Protocol:** HTTPS POST using the Apicalypse query syntax (JSON payload).
- **Documented Endpoint:**  
  `POST https://api.igdb.com/v4/games`
- **Query Syntax Example (Apicalypse):**
  ```text
  fields name, slug, first_release_date, alternative_names.name, involved_companies.company.name;
  search "Skyrim";
  limit 10;
  ```
- **Authentication & OAuth2 Flow:**
  - Requires Twitch Developer Portal registration obtaining `Client ID` and `Client Secret`.
  - Mandatory OAuth2 Client Credentials flow:
    `POST https://id.twitch.tv/oauth2/token?client_id={ID}&client_secret={SECRET}&grant_type=client_credentials`
  - Returns bearer token (`access_token`) with typical expiration of ~60 days.
  - Subsequent requests pass headers:
    ```http
    Client-ID: {TWITCH_CLIENT_ID}
    Authorization: Bearer {APP_ACCESS_TOKEN}
    ```
- **Published Rate Limits:** **4 requests per second** per client ID on the standard tier.
- **Security & Secret Handling:** In strict compliance with `REQ-SEC-001`, `Client ID` and `Client Secret` are stored in the Windows DPAPI encrypted vault. The application stores only an opaque GUID reference in configuration manifests.
- **Failure Responses:** HTTP 401 Unauthorized (expired token $\rightarrow$ triggers automatic token refresh), HTTP 429 Too Many Requests (triggers exponential backoff).

---

### 2.4 Wikidata SPARQL Query Service & MediaWiki Action API
- **Official Documentation:** `wikidata.org/wiki/Wikidata:Data_access`, `query.wikidata.org` (Reviewed: 2026-11-17).
- **Access Protocol:** HTTPS GET / POST via SPARQL Protocol (JSON output).
- **Documented Endpoint:**  
  `GET https://query.wikidata.org/sparql?query={SPARQL_QUERY}&format=json`
- **Sample Semantic Query (Steam AppID to Wikidata QID & PCGW Slug):**
  ```sparql
  SELECT ?game ?gameLabel ?pcgw_id WHERE {
    ?game wdt:P1733 "480" .
    OPTIONAL { ?game wdt:P5794 ?pcgw_id . }
    SERVICE wikibase:label { bd:serviceParam wikibase:language "en". }
  }
  ```
- **Authentication:** **Unauthenticated**. No API key required.
- **Mandatory Request Headers:** Requires custom `User-Agent` header. Requests without a valid agent string are blocked with HTTP 403.
- **Published Rate Limits:** 60-second execution time limit per query. Concurrent connection limit enforced per IP address.
- **Failure Responses:** HTTP 429, HTTP 504 Gateway Timeout on complex joins.

---

### 2.5 NexLudic Local Bundled Seed Catalog
- **Location:** `D:\NexLudic\Catalog\Seed\seed_catalog.json` (Volume D mandate).
- **Access Protocol:** Local asynchronous file I/O (`System.IO.FileStream` with read sharing).
- **Authentication:** Local filesystem ACLs (Medium Integrity Level).
- **Rate Limits:** None. Sub-millisecond latency.

---

## 3. Rate-Limit Backoff and Resilience Protocol

To prevent IP bans and comply with provider fair-use terms, all HTTP provider clients must implement a standardized **Exponential Backoff with Jitter** protocol:

$$\text{BackoffDelay} = \min\left(\text{MaxDelay}, \text{BaseDelay} \times 2^{\text{Attempt}} + \text{UniformRandom}(0, \text{Jitter})\right)$$

| Provider | Base Delay | Max Delay | Max Retries | HTTP 429 Action |
|---|---|---|---|---|
| **Steam Storefront** | 2.0 seconds | 60.0 seconds | 3 | Pause requests for 60 seconds; fall back to local cache |
| **PCGamingWiki** | 1.0 second | 30.0 seconds | 3 | Enforce 1 req/sec spacing queue; backoff on 429 |
| **IGDB API** | 0.25 seconds| 10.0 seconds | 4 | Backoff on 429; refresh token on 401 |
| **Wikidata SPARQL** | 3.0 seconds | 60.0 seconds | 2 | Simplify query or abort on timeout |

---

## 4. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-REQ-API-001 |
| Created | 2026-11-17 |
| Classification | Technical Access Specification & API Protocols |
| Implementation Authorization | Technical requirements and interface definition only |

