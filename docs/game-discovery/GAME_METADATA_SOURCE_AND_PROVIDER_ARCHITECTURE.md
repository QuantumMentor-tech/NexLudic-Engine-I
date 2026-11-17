# Game Metadata Source & Provider Architecture

**Document ID:** NLE-ARC-SRC-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 25 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-SPC-DSC-001  
**Classification:** Source Architecture, Provider Contracts & Governance  

---

## 1. Purpose and Architectural Objectives

This document specifies the architecture, abstraction boundaries, integration patterns, and data governance policies for external and local metadata providers supplying the **NexLudic Engine Game Catalog**.

The primary architectural objective is to establish an **extensible, provider-agnostic metadata ingestion pipeline** that:
- Decouples the universal game catalog from the proprietary data structures of any single commercial storefront or web database.
- Enforces rigorous data provenance, field-level attribution, and conflict arbitration without relying on arbitrary numerical rankings.
- Safeguards user privacy and API credentials in strict compliance with the platform security architecture (`REQ-SEC-001`, `ADR-0003`).
- Maintains continuous local-first operation on **Volume `D:\`** (`DEC-009`) even when external network providers are unreachable.

---

## 2. Source Categories & Evaluation Matrix

Metadata sources are classified into five distinct categories based on their technical utility, reliability, licensing terms, and authority:

| Category | Primary Examples | Core Strengths | Technical Limitations | Licensing & Redistribution | Authoritative Domain |
|---|---|---|---|---|---|
| **Platform / Storefront APIs** | Steamworks Web API, GOG Store API, Epic Games Catalog | Highly reliable storefront AppIDs, official release dates, patch notes. | Zero deep engine architecture metadata; often lacks anti-cheat details. | Commercial terms apply; rate-limited; no asset redistribution. | Storefront SKUs, AppIDs, official distribution channels. |
| **Technical Community Databases** | PCGamingWiki (Cargo API / MediaWiki API) | Exceptional technical granularity: engine family, 32/64-bit architecture, DirectX API, anti-cheat tags. | Community-edited; potential vandalism or delayed patch updates. | Creative Commons Attribution-NonCommercial-ShareAlike (`CC-BY-NC-SA 3.0`). | Technical engine, binary bitness, mod loaders, anti-cheat. |
| **Comprehensive Game Catalogs** | IGDB (Twitch/Amazon API), RAWG | Broad catalog coverage, franchise taxonomies, localized titles, cover art metadata. | Requires OAuth credentials; rate-limited; technical engine fields are coarse. | Free tier with attribution requirements; commercial restrictions. | Alternate titles, international names, release year taxonomy. |
| **Open Semantic Datasets** | Wikidata (SPARQL endpoint / JSON dumps) | Open domain identifiers (QIDs), permanent semantic links, zero API key required. | Sparse technical metadata for niche indie games. | Public Domain (`CC0 1.0 Universal`). | Stable external cross-references, global semantic links. |
| **Local Bundled Seed Catalog** | NexLudic Embedded Catalog (`D:\NexLudic\Catalog\seed\`) | 100% offline, zero latency, guaranteed availability for pilot game pairs. | Static; requires engine updates or patch downloads to refresh. | Proprietary curated manifest under NexLudic platform license. | Baseline offline fallback for pilot portfolio titles. |

---

## 3. Provider Abstraction Architecture

The metadata ingestion subsystem interacts with external sources strictly through an implementation-neutral provider boundary:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                 CANONICAL GAME CATALOG                                 │
└──────────────────────────────────────────▲─────────────────────────────────────────────┘
                                           │
                                           │ Normalization & Provenance Boundary
                                           │
┌──────────────────────────────────────────┴─────────────────────────────────────────────┐
│                           METADATA PROVIDER AGGREGATOR                                 │
│  • Dispatches Queries  • Normalizes External DTOs  • Ingests Provenance  • Caches on D: │
└──────▲─────────────────────▲─────────────────────────▲───────────────────────▲─────────┘
       │                     │                         │                       │
┌──────┴──────────────┐┌─────┴──────────────────┐┌─────┴────────────────┐┌─────┴─────────┐
│ Steam Store Provider││  PCGamingWiki Provider ││    IGDB Provider     ││ Local Seed    │
│ (Storefront AppIDs) ││  (Technical Engine)    ││ (Taxonomy & Aliases) ││ (Offline JSON)│
└─────────────────────┘└────────────────────────┘└──────────────────────┘└───────────────┘
```

### 3.1 Conceptual Provider Contract (`IMetadataProvider`)
Each provider implements standard conceptual responsibilities (design-level contract):
- **`SearchByQueryAsync(query)`:** Queries the external source using normalized title or keywords; returns candidate summaries.
- **`GetDetailsByAnchorAsync(sourceAnchor)`:** Retrieves exhaustive entity record using source-native key (e.g., Steam AppID).
- **`GetProviderCapabilities()`:** Reports supported data fields, rate limits, and authentication requirements.
- **`GetFreshnessPolicy()`:** Declares recommended cache TTL (Time-To-Live).

### 3.2 Normalization Boundary
External providers emit source-specific Data Transfer Objects (DTOs). These DTOs must **never** be stored directly in canonical game entities. A dedicated provider mapper translates external DTOs into internal domain value objects, stripping HTML, normalizing string encodings, and converting proprietary enumerations into canonical domain types.

---

## 4. Source Authority and Conflict Resolution Policy

Different sources possess differing levels of technical authority. The catalog rejects simplistic global rankings (e.g., *"Source A is always better than Source B"*). Instead, authority is established on a **field-domain basis**:

```
+----------------------------------------------------------------------------------------------------+
|                               FIELD-DOMAIN AUTHORITY MATRIX                                        |
+--------------------------+-----------------------+------------------------+------------------------+
| Field Domain             | Primary Authority     | Secondary Authority    | Fallback / Validation  |
+--------------------------+-----------------------+------------------------+------------------------+
| Storefront AppID & Channel| Storefront API (Steam)| PCGamingWiki           | User Declared          |
| Canonical English Title  | Storefront / Publisher| IGDB / Wikidata        | PCGamingWiki           |
| Engine Family & Version  | PCGamingWiki          | Local Detection (PE)   | IGDB                   |
| Binary Architecture (x64)| PCGamingWiki          | Local PE Header Probing| Unresolved / Unknown   |
| Anti-Cheat Status        | PCGamingWiki          | Local Binary Inspection| Storefront Manifest    |
| Release Year & Developer | Storefront API        | IGDB / Wikidata        | User Declared          |
| Alternate Titles / Aliases| IGDB / Wikidata       | PCGamingWiki           | User Declared          |
+--------------------------+-----------------------+------------------------+------------------------+
```

### 4.1 Conflict Arbitration Rules
When two providers disagree on a metadata field during ingestion:
1. **Domain Precedence:** The provider holding Primary Authority for that field domain is accepted by default.
2. **Provenance Retention:** The conflicting alternative value is preserved in a `MetadataConflictRecord` alongside its source authority and capture timestamp.
3. **Consequential Safety Invariant:** If two sources disagree on **`anti_cheat_status`** where one source claims `KERNEL_LEVEL` and another claims `NONE`, the **safer determination (`KERNEL_LEVEL`) is immediately enforced** until confirmed by binary inspection.
4. **Epistemic Transparency:** The UI surfaces unresolved technical conflicts as advisory warnings rather than concealing the ambiguity.

---

## 5. Provenance and Claim Verification

### 5.1 Field-Level Provenance Requirements
Every substantive technical claim ingested into the catalog must instantiate a `MetadataClaimProvenance` record:
- **`SourceAuthority`:** Explicit provider URI (e.g., `provider:pcgamingwiki`, `provider:steam:480`).
- **`CaptureTimestamp`:** UTC ISO 8601 timestamp of data retrieval.
- **`SourceRecordKey`:** The exact identifier or revision ID within the source system.
- **`EpistemicTier`:** Assigned tier (`DECLARED`, `IMPORTED`, `DETECTED`, `VERIFIED`).

---

## 6. Source Failure, Resilience, and Offline Operation

External network operations are inherently unreliable. The provider architecture enforces strict operational resilience:

### 6.1 Failure Modes and Graceful Degradation
- **Network Outage / Timeout:** If a provider times out, the search pipeline returns results from remaining active providers and the local seed catalog. A provider timeout **never** raises an application crash or blocks project creation.
- **HTTP 429 Rate Limiting:** The provider immediately pauses queries for that source, emits an informational diagnostic event, and falls back to local cache.
- **Malformed Source Payload:** The specific item is logged to Serilog diagnostics; valid items from the same payload continue processing.
- **Provider Retirement / Deprecation:** If a third-party service shuts down, stored catalog records retain their existing `MetadataClaimProvenance` referencing the historical source.

### 6.2 Volume D Cache Storage
In accordance with `DEC-009`, all provider cache files, raw payload dumps, and offline seed datasets are strictly housed on **Volume `D:\`** under:
- `D:\NexLudic\Catalog\Cache\<ProviderName>\`
- `D:\NexLudic\Catalog\Seed\`

Operating system drive `C:\` is strictly barred from catalog caching.

---

## 7. Security, Secrets Management, and Licensing Governance

### 7.1 DPAPI Secret Vaulting
In compliance with `REQ-SEC-001` and `ADR-0003`:
- Providers requiring API keys (e.g., IGDB client secret) must retrieve them exclusively from the Windows DPAPI credential store via opaque reference UUIDs (`credential_ref`).
- Raw API keys must **never** be written to catalog JSON files, git repositories, logs, or UI view models.

### 7.2 Licensing Compliance and Asset Attribution
- **Textual Data Ingestion Only:** Providers ingest structured textual facts (engine name, release year, AppID).
- **Prohibition on Proprietary Assets:** Ingesting, downloading, or redistributing proprietary game binaries, executable code, mod scripts, sound files, or high-resolution box art from external APIs is strictly prohibited.
- **Attribution Tags:** When metadata is displayed from sources requiring attribution (e.g., `CC-BY-NC-SA 3.0` for PCGamingWiki), the UI renders an appropriate source citation link in the metadata inspector.

---

## 8. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 25 — Game Discovery Architecture & Canonical Game Identity Specification |
| Document ID | NLE-ARC-SRC-001 |
| Created | 2026-11-17 |
| Classification | Source Architecture, Provider Contracts & Governance |
| Implementation Authorization | Architecture and specification only |

