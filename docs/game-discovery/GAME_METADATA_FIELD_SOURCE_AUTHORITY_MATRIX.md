# Game Metadata Field Source Authority Matrix

**Document ID:** NLE-MAT-AUT-001  
**Version:** 1.0.0  
**Status:** Approved — Authority & Governance Matrix  
**Phase:** 4 — Game Discovery & Compatibility Intelligence  
**Step:** 26 of 100  
**Date:** 2026-11-17  
**Parent Specification:** NLE-EVA-SRC-001  
**Classification:** Source Authority & Conflict Governance Specification  

---

## 1. Purpose and Governance Authority

This matrix maps every standardized metadata attribute defined in the **Game Metadata Catalog & Field Dictionary** (`NLE-DIC-MET-001`, Step 25) to its authoritative source provider, secondary fallback, conflict resolution policy, and epistemic verification requirements.

In strict accordance with the core architecture:
- No single provider holds universal authority across all fields.
- Storefront APIs hold authority over commercial packaging and AppIDs.
- Technical community databases (PCGamingWiki) hold authority over engine runtimes, binary bitness, and anti-cheat tags.
- Local binary inspection holds final authority over installed file hashes and detected executables.
- General game catalogs (IGDB) hold authority over taxonomy and international aliases.

---

## 2. Field-to-Source Authority Mapping

| Field ID & Name | Purpose | Preferred Source Category | Primary Provider | Secondary / Fallback Provider | Evidentiary Limit | Conflict Resolution Rule | Licensing & Attribution Obligation |
|---|---|---|---|---|---|---|---|
| `FLD-ID-001` `canonical_game_id` | Immutable primary key | Internal Domain | **NexLudic Core Engine** | Deterministic UUIDv5 generator | Identity anchor only | N/A (Internal generated) | Internal Core Domain |
| `FLD-ID-002` `canonical_title` | Authoritative English title | Official Storefront / Publisher | **Steam Storefront API** | IGDB API / Wikidata | Commercial title only | Steam title wins for Steam PC; IGDB for non-Steam | Retain source link |
| `FLD-ID-003` `normalized_key` | Fast indexing slug | Internal Domain | **NexLudic Algorithm** | Derived via `RULE-ID-003` | Query key only | N/A (Deterministic) | Internal Core Domain |
| `FLD-ID-004` `initial_release_year`| Disambiguation anchor | Storefront / Knowledgebase | **Steam Storefront API** | IGDB API / Wikidata | Commercial release date | Oldest verified year wins across sources | Unrestricted |
| `FLD-ID-005` `developer_name` | Studio identification | Storefront / Catalog | **Steam Storefront API** | IGDB API / Wikidata | Corporate credit | Steam storefront listing wins; IGDB fallback | Retain source link |
| `FLD-ID-006` `publisher_name` | Publishing entity | Storefront / Catalog | **Steam Storefront API** | IGDB API / Wikidata | Commercial entity | Current storefront publisher wins | Retain source link |
| `FLD-ID-007` `alternate_titles` | Aliases and localizations | Multi-lingual Catalog | **IGDB API** | Wikidata (P1733, P5794) / PCGW | Aliases do not create new games | Union set of all verified aliases retained | IGDB Attribution ("Powered by IGDB") |
| `FLD-EDN-001` `edition_id` | Distinct technical edition | Internal Domain | **NexLudic Core Engine** | Derived via `RULE-EDN-001` | Technical boundary | N/A (Internal generated) | Internal Core Domain |
| `FLD-EDN-002` `edition_title` | User-facing edition name | Technical Wiki / Store | **PCGamingWiki** | Steam Storefront API | Release label | PCGamingWiki edition taxonomy wins | `CC-BY-NC-SA 3.0` |
| `FLD-EDN-003` `architecture_type` | Processor bitness (x86/x64)| Technical Community Wiki | **PCGamingWiki** | Local PE Header Inspection | Crucial compatibility constraint | Local PE header > PCGamingWiki > Unknown | `CC-BY-NC-SA 3.0` |
| `FLD-EDN-004` `is_remaster` | Major technical overhaul | Technical Community Wiki | **PCGamingWiki** | IGDB API | Informs adapter fork | PCGamingWiki technical tag wins | `CC-BY-NC-SA 3.0` |
| `FLD-EDN-005` `has_engine_fork` | Forked engine codebase | Technical Wiki / Adapter | **PCGamingWiki** | Adapter Descriptor (`ADR-0004`) | Demands adapter isolation | Adapter declaration > PCGW > False | Internal / `CC-BY-NC-SA` |
| `FLD-REL-001` `storefront_channel`| Packaging and launcher | Distribution Platform | **Storefront Ingestion** | User Selection in Wizard | Dictates launcher hooks | User selection establishes active channel | Unrestricted |
| `FLD-REL-002` `storefront_app_id` | Storefront numeric ID | Official Storefront | **Steam Storefront API** | GOG / Epic Storefront | Programmatic launch only | Official platform manifest is absolute authority | Store API Terms |
| `FLD-REL-003` `has_drm` | DRM wrapper presence | Technical Community Wiki | **PCGamingWiki** | Steam Store API | Injection impediment | PCGamingWiki detailed DRM field wins | `CC-BY-NC-SA 3.0` |
| `FLD-VER-001` `raw_version_string`| Unmodified patch string | Local Host / Storefront | **Local Executable Detection** | Steam Build ID / User Declared | Non-semver string preserved | Local detected binary > Manifest > Declared | Local Host (Private) |
| `FLD-VER-002` `build_identifier` | Internal revision hash | Official Store Manifest | **Steam Depot Manifest** | Local Binary PE Timestamp | Exact build identity | Official store manifest wins | Store API Terms |
| `FLD-VER-003` `epistemic_tier` | Verification pedigree | Internal Domain | **NexLudic Core Engine** | Derived from detection method | Epistemic integrity | Verified > Detected > Imported > Declared | Internal Core Domain |
| `FLD-ENG-001` `engine_family` | Core software framework | Technical Community Wiki | **PCGamingWiki** | Wikidata (P408) / IGDB | Selects candidate adapters | PCGamingWiki wins; Wikidata secondary | `CC-BY-NC-SA 3.0` |
| `FLD-ENG-002` `engine_version` | Engine patch level | Technical Community Wiki | **PCGamingWiki** | Local Memory Probe (Phase 4) | Memory offset stability | Local probe > PCGamingWiki > Unknown | `CC-BY-NC-SA 3.0` |
| `FLD-ENG-003` `scripting_backend`| Script execution VM | Technical Community Wiki | **PCGamingWiki** | Adapter SDK Binding | Dictates logic bridge hooks | Adapter binding analysis > PCGW | `CC-BY-NC-SA 3.0` |
| `FLD-ENG-004` `graphics_api` | Render pipeline (DX/VK) | Technical Community Wiki | **PCGamingWiki** | Local DXGI Query | Dictates graphics hook | Local runtime probe > PCGW | `CC-BY-NC-SA 3.0` |
| `FLD-MOD-001` `primary_mod_loader`| Standard community loader | Technical Community Wiki | **PCGamingWiki** | Community Documentation | Informs adapter prerequisite | PCGamingWiki curated field wins | `CC-BY-NC-SA 3.0` |
| `FLD-MOD-002` `anti_cheat_status`| Kernel/online anti-cheat | Technical Community Wiki | **PCGamingWiki** | Local Driver Detection (`REQ-SAFE`) | Triggers mandatory hard block | **Safety Invariant:** Most restrictive tag wins | `CC-BY-NC-SA 3.0` |
| `FLD-PRV-001` `source_authority` | Attesting entity URI | Ingestion Subsystem | **NexLudic Ingestion Engine** | Specific Provider Anchor | Complete traceability | N/A (Captures ingestion origin) | Internal Core Domain |
| `FLD-PRV-002` `ingested_at` | UTC capture timestamp | Ingestion Subsystem | **NexLudic Ingestion Engine** | System Clock (UTC) | Freshness evaluation | N/A (Immutable capture date) | Internal Core Domain |
| `FLD-PRV-003` `license_attribution`| Legal license citation | Ingestion Subsystem | **Provider Terms Parser** | Provider Policy Manifest | Legal compliance | Mandatory license tag preserved | Attribution Tag |

---

## 3. Conflict Resolution Protocols

When two or more providers return conflicting data for the same canonical entity:

### Protocol 1: Domain-Specific Authority Precedence
The provider designated as **Primary Provider** in the matrix above is adopted by default. Secondary providers are consulted only when the primary provider returns `NULL`, `UNKNOWN`, or experiences network failure.

### Protocol 2: Safety-First Anti-Cheat Arbitration
In strict compliance with `REQ-SAFE-001`, if Provider A reports `anti_cheat_status == NONE` and Provider B reports `anti_cheat_status == KERNEL_LEVEL`, the system **must immediately adopt `KERNEL_LEVEL`**. Safety determinations always default to the most restrictive security classification to prevent kernel anti-cheat violations.

### Protocol 3: Conflict Preservation Invariant
When a primary provider overrides a secondary provider's conflicting value, the secondary claim is not discarded. It is recorded in a `MetadataConflictRecord` value object attached to the entity's provenance collection, preserving full historical auditability.

---

## 4. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture & Game Technology Team |
| Step | 26 — Game Metadata Sources & Provider Evaluation Specification |
| Document ID | NLE-MAT-AUT-001 |
| Created | 2026-11-17 |
| Classification | Source Authority & Conflict Governance Specification |
| Implementation Authorization | Research, matrix definition, and governance only |

