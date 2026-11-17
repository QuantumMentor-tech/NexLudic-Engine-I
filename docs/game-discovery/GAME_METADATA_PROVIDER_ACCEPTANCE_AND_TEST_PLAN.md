# Game Metadata Provider Acceptance & Test Plan

**Document ID:** NLE-PLN-PRV-001  
**Version:** 1.0.0  
**Status:** Approved — Quality, Acceptance & Test Specification  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** Provider Acceptance Gates & Verification Test Plan  

---

## 1. Purpose and Scope

This document specifies the formal **Provider Acceptance Gates (Gates A through J)** and the comprehensive design-level test plan required before any external metadata provider connector is authorized for implementation in subsequent Phase 4 steps.

In strict accordance with the Step 26 mandate:
- Acceptance gates operate as hard binary criteria. A provider that fails a mandatory licensing or security gate cannot be promoted through an aggregate score.
- All 30 test cases specify **design-level test requirements** to guide future implementation. Zero test code, test runners, or live network calls are executed in this step.

---

## PART I: FORMAL PROVIDER ACCEPTANCE GATES

Before any external provider is authorized for code scaffolding, it must pass all ten mandatory acceptance gates:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        TEN MANDATORY PROVIDER ACCEPTANCE GATES                         │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ [Gate A: Identity & Relevance]    Verified primary documentation; relevant to engine   │
│ [Gate B: Docs Completeness]       Endpoints, quotas, and error formats documented      │
│ [Gate C: Licensing & Legal]       Desktop client querying, local caching permitted     │
│ [Gate D: Canonical Mapping]       Maps cleanly to Step 25 domain model without loss    │
│ [Gate E: Provenance Fidelity]     Sufficient attribution data for field-level claims   │
│ [Gate F: Security & Privacy]      DPAPI vaulting supported; zero path/data leakage     │
│ [Gate G: Failure & Fallback]      Deterministic behavior on timeouts, 429, and outages │
│ [Gate H: Testability]             Deterministic mock payloads can be constructed       │
│ [Gate I: Lifecycle & Deprecation] Invalidation and cache TTL policies supported        │
│ [Gate J: Governance Approval]     Formally approved by Architecture Review Board       │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## PART II: DESIGN-LEVEL VERIFICATION TEST SUITE

The following 30 test cases define the expected behaviors, failure handling, and validation criteria for future provider implementations.

---

## 2. Documentation & Contract Verification Tests

### TPRV-DOC-001: Provider Primary Endpoint Reachability
- **Objective:** Verify documented endpoint URL returns valid HTTPS response.
- **Preconditions:** Network interface active; official endpoint target (e.g., Steam store API).
- **Action:** Dispatch HTTPS GET request with valid headers.
- **Expected Result:** HTTP 200 OK; response Content-Type is `application/json`.
- **Related AC:** AC-04, AC-05.

### TPRV-DOC-002: User-Agent Header Compliance
- **Objective:** Confirm MediaWiki `Api-User-Agent` header prevents HTTP 403 blocks.
- **Preconditions:** PCGamingWiki Cargo endpoint target.
- **Action:** Send query with custom `Api-User-Agent: NexLudicEngine/1.0.0...`.
- **Expected Result:** HTTP 200 OK; Cargo JSON returned without blocking.
- **Related AC:** AC-05, AC-10.

---

## 3. Normalization & Mapping Tests

### TPRV-MAP-001: Steam App Details Normalization to CanonicalGame
- **Objective:** Verify Steam `appdetails` JSON maps to `CanonicalGame` and `GameEdition`.
- **Preconditions:** Mock Steam response for AppID 480 (*"Spacewar"*).
- **Action:** Execute Steam normalization mapper.
- **Expected Result:** `canonical_title == "Spacewar"`; `storefront_channel == STEAM`; `storefront_app_id == "480"`.
- **Related AC:** AC-07, AC-13.

### TPRV-MAP-002: PCGamingWiki 64-bit Architecture Extraction
- **Objective:** Confirm PCGW `64-bit_executable` maps to `architecture_type = X64`.
- **Preconditions:** Mock PCGW payload with `"64-bit_executable": "true"`.
- **Action:** Execute PCGW normalization mapper.
- **Expected Result:** Output entity has `architecture_type == X64`; `source_authority == "provider:pcgamingwiki"`.
- **Related AC:** AC-07, AC-13.

### TPRV-MAP-003: Defensive Schema Parsing on Unexpected JSON Keys
- **Objective:** Confirm unmapped provider properties are skipped without throwing exceptions (`RULE-DRF-001`).
- **Preconditions:** Injected JSON payload containing unexpected key `"unknown_custom_vendor_field": "test"`.
- **Action:** Deserialize payload via provider parser.
- **Expected Result:** Serialization completes with zero exceptions; valid fields populate cleanly.
- **Related AC:** AC-13, AC-14.

### TPRV-MAP-004: Missing Optional Field Handled as UNKNOWN
- **Objective:** Verify missing engine version in provider payload sets `engine_version = null`.
- **Preconditions:** Provider payload omits engine build string.
- **Action:** Process normalization.
- **Expected Result:** `engine_version == null`; zero crash; zero fabricated default version.
- **Related AC:** AC-07, AC-13.

### TPRV-MAP-005: HTML Markup Stripped from Text Fields
- **Objective:** Confirm description or title fields containing HTML entities are sanitized.
- **Preconditions:** Provider title string `"<b>Game Title</b> &amp; Edition"`.
- **Action:** Normalize title string.
- **Expected Result:** Output string is plain text `"Game Title & Edition"`.
- **Related AC:** AC-10, AC-13.

---

## 4. Identity Resolution & Ambiguity Tests

### TPRV-ID-001: Exact Steam AppID Matches Canonical Anchor
- **Objective:** Confirm querying by `steam:480` resolves directly to single canonical game.
- **Preconditions:** Catalog contains mapped Steam AppID 480.
- **Action:** Execute search with query `"steam:480"`.
- **Expected Result:** Immediate O(1) resolution; returns `IDENTIFIED` status; zero candidate ambiguity.
- **Related AC:** AC-12.

### TPRV-ID-002: Ambiguous Candidate Set Returned on Close Similarity
- **Objective:** Confirm close similarity scores ($\Delta < 0.08$) trigger `AMBIGUOUS_CANDIDATES` (`RULE-MAT-003`).
- **Preconditions:** Two candidate records with similarity scores 0.91 and 0.89.
- **Action:** Execute search query.
- **Expected Result:** System returns `AMBIGUOUS_CANDIDATES`; zero auto-selection; candidate list surfaced for user selection.
- **Related AC:** AC-12.

### TPRV-ID-003: Alternate Title Resolves to Canonical Parent
- **Objective:** Confirm regional title *"Biohazard"* resolves to *"Resident Evil"*.
- **Preconditions:** Alternate title mapping exists in IGDB provider.
- **Action:** Query search for `"Biohazard"`.
- **Expected Result:** Output resolves to `CanonicalGameId` for Resident Evil; canonical title preserved.
- **Related AC:** AC-12.

---

## 5. Versioning & Technical Claim Tests

### TPRV-VER-001: Non-Semantic Version String Preserved Verbatim
- **Objective:** Confirm non-semver version `"Build 108429"` is stored unmodified.
- **Preconditions:** Storefront reports build identifier.
- **Action:** Ingest version build record.
- **Expected Result:** `raw_version_string == "Build 108429"`; no semver coercion.
- **Related AC:** AC-06, AC-07.

### TPRV-VER-002: Incomparable Version Evaluation
- **Objective:** Verify comparison of disparate version strings returns `INCOMPARABLE`.
- **Preconditions:** Version A = `"v1.4"`, Version B = `"Build 2023-11-04"`.
- **Action:** Execute `Compare(VersionA, VersionB)`.
- **Expected Result:** Returns `INCOMPARABLE`; zero artificial sorting order.
- **Related AC:** AC-06.

---

## 6. Provider Failure, Resilience & Fallback Tests

### TPRV-FLR-001: 5-Second HTTP Timeout Handled Gracefully
- **Objective:** Verify that a hanging external request aborts after exactly 5.0 seconds.
- **Preconditions:** Provider simulated to delay response for 10.0 seconds.
- **Action:** Dispatch query via provider client.
- **Expected Result:** Task cancels at 5.0s with `TimeoutException`; query falls back to local cache; zero application freeze.
- **Related AC:** AC-10, AC-18.

### TPRV-FLR-002: HTTP 429 Triggers Exponential Backoff with Jitter
- **Objective:** Confirm HTTP 429 response pauses provider queries according to backoff protocol.
- **Preconditions:** Provider returns HTTP 429 Too Many Requests.
- **Action:** Dispatch query.
- **Expected Result:** Provider enters backoff delay; query rerouted to secondary provider / local cache.
- **Related AC:** AC-10, AC-18.

### TPRV-FLR-003: Graceful Degradation on Complete Internet Loss
- **Objective:** Confirm catalog search functions when network interface is disconnected.
- **Preconditions:** Network interface disabled.
- **Action:** Search for pilot game pairs (*"Skyrim"*).
- **Expected Result:** Ingestion engine immediately serves results from `D:\NexLudic\Catalog\Seed\seed_catalog.json`; zero crash.
- **Related AC:** AC-10, AC-18.

### TPRV-FLR-004: Stale Cached Metadata Served with Advisory Warning
- **Objective:** Confirm expired cache data is served during external outages with `CAT-03` warning.
- **Preconditions:** Cache entry on Volume D: is 120 days old (TTL = 90 days); provider is offline.
- **Action:** Query game details.
- **Expected Result:** Stale cached data returned; UI displays warning: *"Using cached metadata. Provider unreachable."*
- **Related AC:** AC-14, AC-18.

---

## 7. Licensing & Attribution Compliance Tests

### TPRV-LIC-001: PCGamingWiki Attribution Rendered in UI
- **Objective:** Verify `CC-BY-NC-SA 3.0` attribution is rendered when displaying technical metadata.
- **Preconditions:** Game metadata sourced from PCGamingWiki.
- **Action:** Open Metadata Inspector in desktop UI.
- **Expected Result:** Displays clickable citation link: *"Technical data from PCGamingWiki (CC-BY-NC-SA 3.0)"*.
- **Related AC:** AC-08, AC-16.

### TPRV-LIC-002: IGDB "Powered by IGDB" Attribution Rendered
- **Objective:** Confirm mandatory Twitch developer attribution badge is visible.
- **Preconditions:** Metadata sourced from IGDB API.
- **Action:** Inspect metadata credits panel.
- **Expected Result:** Renders *"Powered by IGDB"* badge and link.
- **Related AC:** AC-08, AC-16.

### TPRV-LIC-003: Prohibition on Binary Asset Download Enforced
- **Objective:** Verify ingestion pipeline contains zero capability to download `.exe`, `.dll`, or audio assets.
- **Preconditions:** Provider response includes links to binary downloads or full-resolution media.
- **Action:** Execute ingestion pipeline.
- **Expected Result:** Binary links are discarded; zero filesystem write of executable code.
- **Related AC:** AC-08, AC-27.

---

## 8. Security, Privacy & Secret Management Tests

### TPRV-SEC-001: Windows Medium Integrity Level Enforcement
- **Objective:** Confirm provider client code executes at Medium IL without UAC elevation (`REQ-SEC-001`).
- **Preconditions:** Application running in standard user session.
- **Action:** Execute catalog query pipeline.
- **Expected Result:** Process token integrity is `SECURITY_MANDATORY_MEDIUM_RID`; zero elevation requests.
- **Related AC:** AC-10, AC-16.

### TPRV-SEC-002: DPAPI Credential Vaulting for API Keys
- **Objective:** Verify OAuth client secret is retrieved via opaque UUID reference.
- **Preconditions:** IGDB client secret vaulted in Windows DPAPI store under GUID `ref-igdb-01`.
- **Action:** Initialize IGDB provider.
- **Expected Result:** Provider loads secret from DPAPI; configuration files contain only the UUID reference; zero plaintext tokens in logs.
- **Related AC:** AC-10, AC-16.

### TPRV-SEC-003: Local File Path Leakage Barred
- **Objective:** Ensure outgoing HTTP headers and query strings never contain host file paths or usernames.
- **Preconditions:** Local game installed at `D:\Games\SteamLibrary\steamapps\common\...`.
- **Action:** Dispatch external metadata search.
- **Expected Result:** Outgoing HTTP request payload inspected; zero occurrences of local path strings or host usernames.
- **Related AC:** AC-10, AC-16.

### TPRV-SEC-004: Volume D Storage Enforcement for Provider Cache
- **Objective:** Confirm provider cache files are strictly rejected if targeted at C:\ (`DEC-009`).
- **Preconditions:** Cache manager initialization.
- **Action:** Attempt to configure cache path on `C:\NexLudic\Cache`.
- **Expected Result:** Throws configuration validation error; enforces `D:\NexLudic\Catalog\Cache\`.
- **Related AC:** AC-10, AC-16.

---

## 9. Downstream Boundary & Regression Tests

### TPRV-REG-001: Metadata Update Does Not Mutate Project Feasibility
- **Objective:** Confirm ingesting a newer game version does not alter existing project feasibility decisions.
- **Preconditions:** Project X has `FeasibilityAssessment == FEASIBLE` on version `1.5.97`.
- **Action:** Catalog ingests new patch `1.6.640` for the game.
- **Expected Result:** Project X retains its pinned `1.5.97` version; feasibility decision remains `FEASIBLE`; zero auto-recomputation.
- **Related AC:** AC-09, AC-25.

### TPRV-REG-002: Anti-Cheat Metadata Triggers Project Safety Block
- **Objective:** Verify anti-cheat metadata from PCGW triggers non-dismissible Critical block in project wizard (`REQ-SAFE-001`).
- **Preconditions:** PCGW returns `anti_cheat_status == KERNEL_LEVEL`.
- **Action:** Select game in Project Creation Wizard Stage 3.
- **Expected Result:** UI renders non-dismissible Critical banner; Confirm button is hidden; project creation blocked.
- **Related AC:** AC-09, AC-28.

---

## 10. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-PLN-PRV-001 |
| Created | 2026-11-17 |
| Classification | Provider Acceptance Gates & Verification Test Plan |
| Implementation Authorization | Test planning and acceptance criteria definition only |

