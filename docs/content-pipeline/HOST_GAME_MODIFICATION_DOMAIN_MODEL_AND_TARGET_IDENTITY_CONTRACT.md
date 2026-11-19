# Host-Game Modification Domain Model & Target Identity Contract

**Document Identifier:** `NLE-MOD-GMS-001`  
**Classification:** Technical Data Contract & Domain Model Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-DIS-001` (Discovery Contract), `NLE-MOD-GMD-001` (Game Metadata Model)  
**Security Level:** Internal Engineering Architecture  

---

## 1. Domain Entities & Conceptual Model

The Host-Game Modification domain encompasses six core entity classes that model the relationship between abstract game definitions, physical local installations, modification capabilities, declarative modification plans, concrete content artifacts, and audit logs of applied changes.

```
+------------------------------+             +-------------------------------+
|    CanonicalHostGameRef      | 1         * |     InstalledGameInstance     |
| (GameId, EngineType, Hashes) |<----------->| (InstanceId, InstallPath, FS) |
+------------------------------+             +-------------------------------+
               | 1                                           | 1
               |                                             |
               v *                                           v *
+------------------------------+             +-------------------------------+
|    ModificationMechanism     |             |       ModificationPlan        |
| (MechanismId, Family, Perms) |<----------->| (PlanId, TargetInst, Ops)     |
+------------------------------+ 1         * +-------------------------------+
                                                             | 1
                                                             |
                                                             v *
+------------------------------+             +-------------------------------+
|       ContentArtifact        | 1         * |    AppliedModificationRecord  |
| (ArtifactId, Type, Integrity)|<----------->| (RecordId, Baseline, Status)  |
+------------------------------+             +-------------------------------+
```

---

## 2. Canonical Host-Game Reference (`CanonicalHostGameRef`)

Represents the authoritative, platform-agnostic identity of a video game title recognized by NexLudic Engine.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/canonical-host-game.json",
  "game_id": "game-skyrim-se",
  "title": "The Elder Scrolls V: Skyrim Special Edition",
  "publisher": "Bethesda Softworks",
  "developer": "Bethesda Game Studios",
  "engine_type": "CREATION_ENGINE",
  "engine_version_family": "Creation_v1_6",
  "distribution_editions": [
    {
      "store_platform": "STEAM",
      "app_id": "489830",
      "executable_relative_path": "SkyrimSE.exe",
      "executable_sha256": "8a3f...d9e1"
    },
    {
      "store_platform": "GOG",
      "app_id": "1711234",
      "executable_relative_path": "SkyrimSE.exe",
      "executable_sha256": "3b2c...a8f4"
    }
  ],
  "supported_mechanisms": [
    "MOD_DIRECTORY_DROP",
    "MOD_LOADER_PLUGIN",
    "ARCHIVE_REPACK"
  ]
}
```

### Invariants:
- `game_id` is globally unique, lowercase, matching `^[a-z0-9_-]{3,64}$`.
- `engine_type` must map to an enumerated engine family (`UNREAL_ENGINE_4`, `UNREAL_ENGINE_5`, `UNITY_MONO`, `UNITY_IL2CPP`, `GODOT`, `CREATION_ENGINE`, `SOURCE_ENGINE`, `CUSTOM_PROPRIETARY`).
- Modification mechanisms must only be listed if verified by engine capability profiles.

---

## 3. Installed-Game Instance (`InstalledGameInstance`)

Represents a concrete, detected installation of a game on the user's physical filesystem.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/installed-game-instance.json",
  "instance_id": "inst-skyrim-steam-01",
  "canonical_game_id": "game-skyrim-se",
  "store_platform": "STEAM",
  "install_root_path": "D:/Games/SteamLibrary/steamapps/common/Skyrim Special Edition",
  "executable_path": "D:/Games/SteamLibrary/steamapps/common/Skyrim Special Edition/SkyrimSE.exe",
  "executable_sha256": "8a3f4e2b1c8d7e9f0a5b4c3d2e1f0a9b8c7d6e5f4a3b2c1d0e9f8a7b6c5d4e3f",
  "build_version": "1.6.1170.0",
  "architecture": "x86_64",
  "writable_scope": {
    "is_install_root_writable": true,
    "user_data_path": "C:/Users/Player/Documents/My Games/Skyrim Special Edition",
    "is_user_data_writable": true,
    "designated_mod_path": "D:/Games/SteamLibrary/steamapps/common/Skyrim Special Edition/Data"
  },
  "detected_mod_loaders": [
    {
      "loader_name": "SKSE64",
      "loader_version": "2.2.6",
      "is_active": true
    }
  ],
  "detected_anti_cheat": "NONE",
  "existing_modifications_detected": true,
  "existing_modification_count": 42
}
```

---

## 4. Modification Mechanism (`ModificationMechanism`)

Defines an abstract or concrete strategy by which content or logic can be introduced into a host game.

| Mechanism ID | Family | Privilege Required | Anti-Cheat Impact | Reversibility Rating |
| :--- | :--- | :--- | :--- | :--- |
| `MECH-MOD-DIR` | `MOD_DIRECTORY_DROP` | Medium IL (User) | Low / Safe | HIGH (Delete dropped files) |
| `MECH-MOD-LOADER` | `DOCUMENTED_MOD_LOADER`| Medium IL (User) | Medium (Offline Only) | HIGH (Unregister plugin) |
| `MECH-ARCHIVE-REP`| `ARCHIVE_REPACK` | Medium IL (User) | Medium / High | MEDIUM (Restore backup archive) |
| `MECH-CFG-OVERRIDE`| `CONFIG_OVERRIDE` | Medium IL (User) | Low / Safe | HIGH (Revert text/ini diff) |
| `MECH-SCRIPT-HOOK`| `SCRIPT_SANDBOX_HOOK` | Medium IL (User) | Medium | HIGH (Remove script payload) |
| `MECH-BIN-PATCH` | `BINARY_PATCH` | **PROHIBITED** | **BANNED (High Risk)**| **DISALLOWED (`INV-GMS-01`)** |
| `MECH-MEM-INJECT` | `DLL_INJECTION` | **PROHIBITED** | **BANNED (Anti-Cheat)**| **DISALLOWED (`P7-C03`)** |

---

## 5. Modification Plan (`ModificationPlan`)

Declarative document describing all planned filesystem and configuration changes before execution.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/modification-plan.json",
  "plan_id": "plan-mod-skyrim-weapons-v1",
  "target_instance_id": "inst-skyrim-steam-01",
  "selected_mechanism_id": "MECH-MOD-DIR",
  "pipeline_run_id": "pipe-run-2026-11-19-001",
  "packages_to_install": [
    {
      "package_id": "pkg-celestial-swords-v1.2",
      "content_hash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  ],
  "planned_operations": [
    {
      "op_type": "COPY_FILE",
      "source_staged_path": ".staging/celestial_blade.nif",
      "target_relative_path": "Data/Meshes/Weapons/Celestial/blade.nif",
      "overwrite_existing": false
    },
    {
      "op_type": "COPY_FILE",
      "source_staged_path": ".staging/celestial_blade.dds",
      "target_relative_path": "Data/Textures/Weapons/Celestial/blade.dds",
      "overwrite_existing": false
    }
  ],
  "rollback_strategy": "ATOMIC_SNAPSHOT_REVERT",
  "approval_state": "PENDING_USER_CONFIRMATION",
  "created_at": "2026-11-19T17:25:00Z"
}
```

---

## 6. Content Artifact (`ContentArtifact`)

Represents any individual asset generated, transformed, or passed through the content pipeline.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/content-artifact.json",
  "artifact_id": "art-mesh-blade-001",
  "content_type": "3D_MESH",
  "format_mime": "model/gltf-binary",
  "source_provenance": {
    "origin_type": "USER_MASHUP_STUDIO",
    "author": "Creator_01",
    "license_spdx": "CC-BY-4.0",
    "source_file_sha256": "1234abcd5678ef01..."
  },
  "transformation_history": [
    {
      "stage": "STAGE_H_TRANSFORMATION",
      "tool": "Adapter_CreationEngine_Transcoder",
      "source_format": "model/gltf-binary",
      "target_format": "application/x-nif-mesh",
      "timestamp": "2026-11-19T17:20:00Z"
    }
  ],
  "intermediate_path": ".staging/art-mesh-blade-001.nif",
  "integrity_sha256": "9876fedcba543210..."
}
```

---

## 7. Applied Modification Record (`AppliedModificationRecord`)

Immutable ledger entry generated after a modification plan has been physically committed to disk.

```json
{
  "$schema": "https://nexludic.org/schemas/v1/applied-modification-record.json",
  "record_id": "rec-applied-20261119-001",
  "plan_id": "plan-mod-skyrim-weapons-v1",
  "instance_id": "inst-skyrim-steam-01",
  "applied_at": "2026-11-19T17:26:30Z",
  "baseline_snapshot_id": "snap-baseline-20261119-001",
  "files_created": [
    "Data/Meshes/Weapons/Celestial/blade.nif",
    "Data/Textures/Weapons/Celestial/blade.dds"
  ],
  "files_modified": [],
  "files_deleted": [],
  "status": "APPLIED_VERIFIED",
  "post_validation_result": {
    "all_files_present": true,
    "checksums_match": true,
    "game_launch_test": "UNTESTED_OFFLINE"
  }
}
```
