# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Configuration Versioning, Migration & Export Policy

**Document ID:** `NLE-POL-MIG-002`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Evolution & Portability Standard  
**Traceability:** `ADR-0007` (Storage Baseline), `NLE-POL-MIG-001` (Persistence Migration), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Policy Scope

The **Configuration Versioning, Migration, and Export Policy** establishes the rules governing configuration format evolution, backward compatibility, project export sanitization, and cross-machine portability.

Configuration must evolve seamlessly across application updates without losing user customizations, while project export packages (`.nxpkg`) must be 100% portable and completely stripped of machine-specific drive paths, user-specific identities, and private cryptographic secrets.

---

## 2. Configuration Schema Versioning Hierarchy

Configuration schemas are decoupled from the application binary version using Semantic Versioning 2.0.0 (`MAJOR.MINOR.PATCH`):

```
+----------------------------------------------------------------------------------------------------+
|                                CONFIGURATION VERSION HIERARCHY                                     |
+---------------------------------+-----------------------------------+------------------------------+
| Schema Identifier               | Target File                       | Version Semantics            |
+---------------------------------+-----------------------------------+------------------------------+
| `schema_appsettings`            | `%LocalAppData%\appsettings.json` | SemVer (e.g. `1.0.0`)        |
| `schema_project_config`         | `<project_root>\project.json`     | SemVer (e.g. `1.1.0`)        |
| `schema_adapter_config`         | `<project>\adapters\*.json`       | SemVer (e.g. `1.0.0`)        |
| `schema_ai_provider_config`     | `<project>\ai-settings.json`      | SemVer (e.g. `1.0.0`)        |
| `schema_toolchain_cache`        | `%LocalAppData%\toolchains.json`  | SemVer (e.g. `1.0.0`)        |
+---------------------------------+-----------------------------------+------------------------------+
```

### SemVer Evolution Rules:
* **MAJOR:** Incompatible structural changes, deleted required fields, or altered validation rules. Requires explicit migration.
* **MINOR:** Backward-compatible additive optional fields. Default values populated automatically.
* **PATCH:** Non-breaking documentation fixes or expanded description metadata.

---

## 3. Configuration Migration Safeguards

When loading a configuration file whose `schema_version` is older than the current engine version:

```
[ Load Configuration File ]
             |
             v
1. Inspect schema_version Tag
   - If schema_version == Current: Load directly.
   - If schema_version > Current: Enforce READ-ONLY MODE (Future Version Protection).
   - If schema_version < Current: Trigger Migration Pipeline.
             |
             v
2. Safe Migration Pipeline:
   a. Capture Pre-Migration Snapshot in <project>/backups/
   b. Execute sequential migration steps (e.g. v1.0 -> v1.1 -> v1.2)
   c. Validate transformed configuration against target JSON Schema
   d. Commit transformed file using atomic File.Replace
   e. Record migration event in Serilog diagnostic log
```

### Handling Unknown & Future Settings:
1. **Unknown Non-Security Fields:** Preserved in an unmapped extension dictionary and serialized back out to disk.
2. **Unknown Security Policy Fields:** Causes immediate configuration rejection; unknown permission keys fail-closed.
3. **Future Major Version:** A project created in Schema `v2.0` opened in a `v1.x` Core is locked in **Read-Only Mode** to prevent data loss.

---

## 4. Project Export Sanitization & Cross-Machine Portability

When a project is packaged for export (`File -> Export Project -> .nxpkg`), the **Export Sanitizer** processes all configuration manifests:

```
+----------------------------------------------------------------------------------------------------+
|                                 PROJECT EXPORT SANITIZATION PIPELINE                               |
+--------------------------+-------------------------------------+-----------------------------------+
| Configuration Property   | Treatment in Export Archive         | Rationale                         |
+--------------------------+-------------------------------------+-----------------------------------+
| `credential_ref`         | **STRIPPED / SET TO NULL**          | Secrets must never leave machine  |
| Local Game Paths         | **CONVERTED TO GAME ID METADATA**   | `D:\Games\Skyrim` -> `skyrim_se`   |
| Local Compiler Paths     | **CONVERTED TO TOOL IDENTITY**      | `C:\Tools\Papyrus` -> `papyrus_v1`|
| Project UUID             | Preserved                           | Maintains project identity        |
| Mashup Blueprints        | Preserved                           | Core integration logic            |
| Evidence Citations       | Preserved                           | Audit provenance                  |
+--------------------------+-------------------------------------+-----------------------------------+
```

---

## 5. Cross-Machine Project Import Resolution

When a user imports an `.nxpkg` package on a new workstation:
1. **Dependency & Path Validation:** The Core inspects the imported project metadata and checks if target games (`game_id`) and required tools are installed on the local system.
2. **Interactive Mapping Wizard:** If a local game install path is missing or different:
   - The UI prompts the user to locate the game executable on the new system.
   - The Core validates the local game executable hash against the project's requirements.
3. **Credential Re-Binding:** If the imported project specifies an AI provider, the user is prompted to link an existing local credential from their Windows DPAPI vault or input a new token.
4. **Zero Auto-Execution:** No imported toolchain, adapter script, or mod code is executed until the user explicitly reviews and accepts the project.

