# Content Source Definition & Authorized Discovery Contract

**Document Identifier:** `NLE-CON-CSD-001`  
**Classification:** Technical Contract & Schema Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Source Model

A **Content Source** is a formally declared and authorized origin from which candidate assets may be discovered. NexLudic Engine enforces strict containment: no discovery operation may inspect, list, or read any directory or endpoint that has not been explicitly registered as an authorized source.

---

## 2. Permitted Source Categories

The ingestion architecture recognizes eight distinct source categories:

| Source Category | Code | Description | Default Trust Tier | Access Mode |
| :--- | :--- | :--- | :--- | :--- |
| **Project-Owned Assets** | `SRC_PROJECT_INTERNAL` | Canonical assets stored within the NexLudic project workspace. | Tier 1 (Verified) | Read-Only |
| **User-Provided Assets** | `SRC_USER_IMPORT` | Assets explicitly selected by the user via local file picker dialogs. | Tier 2 (Curated) | Read-Only |
| **Authorized Local Directory** | `SRC_LOCAL_DIR` | Dedicated asset directory explicitly authorized by the user. | Tier 2 (Curated) | Read-Only |
| **Game Installation Reference** | `SRC_GAME_INSTALL` | Verified, non-executable data folders of detected host games. | Tier 2 (Curated) | Read-Only (Non-destructive) |
| **Official Modding Resource** | `SRC_OFFICIAL_MOD_SDK` | Official development kits (e.g. Creation Kit, UE Mod SDK, REDkit). | Tier 1 (Verified) | Read-Only |
| **External Artifact Reference**| `SRC_EXTERNAL_REGISTRY`| Remote package registry (e.g. NexusMods API, ModDB, GitHub release). | Tier 3 (Untrusted) | Read-Only / Signed |
| **Approved Content Package** | `SRC_APPROVED_PACKAGE` | Pre-packaged `.nlpkg` archives with verified Ed25519 signatures. | Tier 1 (Verified) | Read-Only / Archive |
| **Pipeline Intermediate Cache**| `SRC_PIPELINE_CACHE` | Previously validated intermediate artifacts in `.staging/`. | Tier 1 (Verified) | Read/Write (Internal) |

---

## 3. Source Definition Schema (`source-definition.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/content-source-definition.json",
  "source_id": "src-local-skyrim-data",
  "category": "SRC_GAME_INSTALL",
  "display_name": "Skyrim Special Edition Data Folder",
  "location_uri": "file:///D:/Games/SteamLibrary/steamapps/common/Skyrim%20Special%20Edition/Data",
  "authority": {
    "owner": "Local User (Player)",
    "canonical_game_id": "game-skyrim-se",
    "verified_instance_id": "inst-skyrim-steam-01"
  },
  "authorization_scope": {
    "read_permitted": true,
    "write_permitted": false,
    "recursive_subdirectories": true,
    "allowed_extensions": [".nif", ".dds", ".wav", ".pex", ".esp"],
    "excluded_subdirectories": ["Video", "Shaders", "Music/Copyrighted"]
  },
  "trust_classification": "TIER_2_COMMUNITY_CURATED",
  "freshness_policy": {
    "check_on_discovery": true,
    "cache_ttl_seconds": 3600
  },
  "discovery_method": "ADAPTER_STRUCTURED_SCAN",
  "adapter_binding": {
    "adapter_id": "adapter-skyrim-creation-engine",
    "minimum_adapter_version": "1.4.0"
  },
  "lifecycle_state": "ACTIVE"
}
```

---

## 4. Source Reference vs. Source Content Invariant

A `Source Reference` (URI, path, or network endpoint) represents only an address. It does NOT establish:
1. That the target endpoint is online or readable.
2. That the content stored therein is authentic or untampered.
3. That the user possesses copyright or licensing rights to use or redistribute the content.
4. That the content matches the engine schema of the target host game.

All four conditions must be verified independently by Stages D and E before any candidate asset is promoted.

---

## 5. Filesystem Discovery Boundaries & Traversal Protections

When executing filesystem discovery within `SRC_LOCAL_DIR` or `SRC_GAME_INSTALL`:
- **Root Confinement:** The canonicalized absolute path of every visited item must strictly start with the canonicalized source root:
  $$\text{target\_path.canonicalize().starts\_with(source\_root.canonicalize())}$$
- **Symlink & Junction Policy:** Reparse points and symbolic links pointing outside `source_root` are strictly skipped and logged with warning `WARN_SYMLINK_ESCAPE`.
- **System Directory Exclusion:** Discovery must never enter `C:\Windows`, `C:\Program Files\Common Files`, or user credential directories (`.ssh`, `.aws`, `.gnupg`).
