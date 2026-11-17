# Game Metadata Provider Normalization & Failure Specification

**Document ID:** NLE-SPC-NRM-001  
**Version:** 1.0.0  
**Status:** Approved — Normalization & Failure Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** Data Transformation, Ingestion Pipelines & Error Recovery  

---

## 1. Purpose and Architectural Scope

This specification defines the data transformation pipelines, normalization boundaries, defensive schema parsing, error recovery, caching lifecycles, and failure handling protocols for external metadata providers integrated into the **NexLudic Engine**.

In strict accordance with the Step 25 and Step 26 baselines:
- External provider Data Transfer Objects (DTOs) are strictly quarantined at the provider boundary.
- Provider-specific schemas, field names, and wire formats never leak into the universal domain model.
- Network outages, API rate limits, malformed payloads, and provider schema drift are handled deterministically without crashing the engine, corrupting stored identities, or fabricating default values.

---

## 2. Ingestion Pipeline & Normalization Architecture

The ingestion pipeline converts disparate external payloads into unified, provenance-backed canonical domain entities:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        PROVIDER DATA INGESTION & NORMALIZATION                         │
└────────────────────────────────────────────────────────────────────────────────────────┘
          │                                  │                                  │
   [Steam App Details]             [PCGW Cargo Response]               [IGDB Game Payload]
          │                                  │                                  │
          ▼                                  ▼                                  ▼
┌──────────────────┐               ┌──────────────────┐               ┌──────────────────┐
│ Steam DTO Parser │               │ PCGW DTO Parser  │               │ IGDB DTO Parser  │
│ (Raw Storefront) │               │ (Raw Cargo JSON) │               │ (Raw Apicalypse) │
└─────────┬────────┘               └─────────┬────────┘               └─────────┬────────┘
          │                                  │                                  │
          ▼                                  ▼                                  ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        PROVIDER NORMALIZATION MAPPERS                                  │
│  • Strips HTML tags  • Decodes Unicode  • Normalizes Enums  • Generates Claim Provenance│
└──────────────────────────────────────────┬─────────────────────────────────────────────┘
                                           │
                                           ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                       CANONICAL DOMAIN AGGREGATOR                                      │
│  • Field Authority Arbitration  • NormalizedKey Derivation  • Cache Persistence on D:   │
└──────────────────────────────────────────┬─────────────────────────────────────────────┘
                                           │
                                           ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        PERSISTED CANONICAL GAME ENTITY                                 │
│  CanonicalGame ──► GameEdition ──► GameReleaseDistribution ──► GameVersionBuild        │
│  (100% Decoupled from External Vendor Schemas)                                         │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Provider-to-Canonical Field Mapping Rules

### 3.1 Steamworks Storefront DTO Mapping

| Steam Raw DTO Field | Canonical Target Field | Transformation & Normalization Rules | Missing Value Handling |
|---|---|---|---|
| `data.name` | `canonical_title` (if primary) | UTF-8 decoded, trimmed. Feeds `RULE-ID-003` normalized key generator. | Rejects record if null/empty. |
| `data.steam_appid` | `storefront_app_id` | Converted to invariant string. `storefront_channel = STEAM`. | Rejects record if null. |
| `data.release_date.date` | `initial_release_year` | Parses 4-digit year regex `\b(19\|20)\d{2}\b`. Stored as integer. | Sets `initial_release_year = null`. |
| `data.developers[]` | `developer_name` | First array element; joins multiple with `", "`. | Sets `developer_name = null`. |
| `data.publishers[]` | `publisher_name` | First array element; joins multiple with `", "`. | Sets `publisher_name = null`. |
| `data.platforms.windows` | `PlatformExecutionEnvironment` | If true, instantiates `PlatformId = "win-x64"` / `"win-x86"`. | Ignores if false. |
| `data.demos`, `data.dlc` | `GameEdition` entitlements | Flags edition with DLC entitlement array; does not create distinct edition. | Defaults to empty array. |

### 3.2 PCGamingWiki Cargo DTO Mapping

| PCGW Raw Cargo Field | Canonical Target Field | Transformation & Normalization Rules | Missing Value Handling |
|---|---|---|---|
| `_pageName` | `AlternateTitle` (Category: WIKI) | MediaWiki title string, stripped of wiki formatting. | Fallback to query slug. |
| `Infobox_game_engine.Engine` | `engine_family` | Mapped to canonical engine taxonomy (e.g., *"Unreal Engine 4"* $\rightarrow$ `Unreal Engine`). | Sets `engine_family = "UNKNOWN"`. |
| `Infobox_game_engine.Build` | `engine_version` | Strips leading `v` or `version`; captures patch string (e.g., `4.27.2`). | Sets `engine_version = null`. |
| `Infobox_game_engine.64-bit_executable` | `architecture_type` | `"true"` $\rightarrow$ `X64`; `"false"` $\rightarrow$ `X86`; `"hackable"` $\rightarrow$ `X86`. | Sets `architecture_type = UNKNOWN`. |
| `Infobox_game_engine.DirectX` | `graphics_api` | Regex extracts highest API version (`12` $\rightarrow$ `DIRECTX_12`, `11` $\rightarrow$ `DIRECTX_11`). | Sets `graphics_api = UNKNOWN`. |
| `Infobox_game_engine.Anti-cheat` | `anti_cheat_status` | Token match: `Denuvo` $\rightarrow$ `CLIENT_SIDE_INTEGRITY`; `Easy Anti-Cheat`/`BattlEye`/`Vanguard` $\rightarrow$ `KERNEL_LEVEL`. | Sets `anti_cheat_status = NONE`. |

### 3.3 IGDB Game DTO Mapping

| IGDB Raw DTO Field | Canonical Target Field | Transformation & Normalization Rules | Missing Value Handling |
|---|---|---|---|
| `name` | `canonical_title` (if primary) | UTF-8 decoded string. | Mandatory non-null. |
| `alternative_names[].name` | `alternate_titles[]` | Mapped to `AlternateTitle` collection with category `LOCALIZED` or `ACRONYM`. | Defaults to empty list. |
| `first_release_date` | `initial_release_year` | Converts Unix epoch timestamp to UTC year integer. | Sets `initial_release_year = null`. |
| `involved_companies[]` | `developer_name` / `publisher_name` | Filters by `developer == true` or `publisher == true`. | Sets to null if unlisted. |

---

## 4. Defensive Schema Parsing and Drift Protection

External APIs periodically alter JSON payload schemas, add new properties, or deprecate existing attributes:

### RULE-DRF-001: Strict Tolerance of Unknown Properties
- All JSON deserializers in the ingestion pipeline must configure:
  `JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip`
- The addition of new or unexpected fields by a provider must **never** cause a serialization exception or abort payload processing.

### RULE-DRF-002: Diagnostic Anomaly Logging
- If a mandatory provider field is missing or contains an unexpected data type (e.g., array instead of string), the item is flagged:
  - An informational diagnostic event is logged to Serilog (`"Provider schema drift detected for {ProviderName} on field {FieldName}"`).
  - The single malformed field is set to `UNKNOWN`.
  - Processing of all remaining valid fields proceeds uninterrupted.

---

## 5. Resilience, Failure Handling & Fallback Protocols

```
[Query Initiated]
       │
       ├──► [Provider HTTP Call (5-second timeout)]
       │         │
       │         ├──► Success (HTTP 200) ──► Normalization ──► Update Cache on D:
       │         │
       │         ├──► Timeout / Network Down (5s elapsed)
       │         │         │
       │         │         └──► Log Warning ──► Consult Local Cache on D: (TTL Ignored)
       │         │
       │         ├──► Rate Limited (HTTP 429)
       │         │         │
       │         │         └──► Engage Exponential Backoff ──► Fallback to Seed Catalog
       │         │
       │         └──► HTTP 5xx Server Error
       │                   │
       │                   └──► Fallback to Secondary Active Providers
```

### 5.1 Provider Failure Modes
1. **HTTP 429 Rate Limiting:** Triggers exponential backoff protocol (`NLE-REQ-API-001 §3`). The provider is paused; queries are routed to secondary providers or local cache.
2. **Network Timeout (5-second limit):** HTTP clients enforce a hard 5.0-second timeout. When exceeded, the call is aborted, an advisory diagnostic is logged, and the query falls back to cached data.
3. **HTTP 5xx Server Outage:** If an external service returns HTTP 500 or 503, the provider is temporarily marked `DEGRADED` for a 5-minute cooldown. Search proceeds seamlessly using the remaining active providers.
4. **All External Providers Offline:** If the host machine has zero internet connectivity, the engine immediately serves results from `D:\NexLudic\Catalog\Seed\seed_catalog.json`.

### 5.2 Stale Cache Degradation Invariant
- If an external provider is unreachable, previously cached metadata on **Volume `D:\`** (`D:\NexLudic\Catalog\Cache\<Provider>\`) is served to the user, even if its 90-day TTL has expired.
- The UI attaches an advisory badge: `CAT-03 Warning: "Using cached metadata. Last updated [Date]. External provider currently unreachable."`

---

## 6. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-SPC-NRM-001 |
| Created | 2026-11-17 |
| Classification | Data Transformation, Ingestion Pipelines & Error Recovery |
| Implementation Authorization | Transformation rules and failure specification only |

