# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Configuration Inventory & Precedence Policy

**Document ID:** `NLE-POL-CFG-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Configuration Standard & Precedence Policy  
**Traceability:** `ADR-0007` (Configuration Baseline), `ADR-0009` (Workspace Isolation), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Inventory Scope

This document establishes the formal **Configuration Inventory and Precedence Policy** for the NexLudic Engine platform. It categorizes all settings across 8 functional domains, classifies each setting across 17 standardized dimensions, and defines the deterministic, auditable precedence rules governing how configuration values are resolved at runtime.

Crucially, this policy establishes **non-overridable security boundaries**: while project-level configuration may customize toolchain paths, compilation options, and UI preferences, it is strictly forbidden from weakening mandatory platform security invariants, disabling staging sandboxes, or auto-authorizing high-impact actions.

---

## 2. The 8 Primary Configuration Categories

```
                              NEXLUDIC CONFIGURATION INVENTORY
                                             |
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |                    |
 1. APPLICATION       2. PROJECT          3. ADAPTER           4. ENVIRONMENT       5. AI PROVIDER
    PREFERENCES          SETTINGS            CONFIGURATION        PROFILES             SETTINGS
    - UI theme/layout    - Selected games    - Compiler flags     - Dev / Test / Prod  - Provider/Model ID
    - Default project dir- Strategy mapping  - Custom prefixes    - Verbosity levels   - Token references
    - Window states      - Custom params     - Sampling rates     - Mock endpoints     - Context limits
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |
 6. TOOLCHAIN         7. EXECUTION &      8. SECRETS &
    LOCATIONS            SECURITY POLICY     CREDENTIALS
    - SDK/Compiler paths - Staging jail      - DPAPI key vault
    - Tool versions      - Approval gates    - Opaque token refs
    - CLI arguments      - Medium IL only    - Revocation status
```

---

## 3. Comprehensive 17-Dimension Configuration Classification Matrix

### 3.1 Category 1: Application-Wide Preferences (`CFG-APP-001`)
1. **Configuration ID:** `CFG-APP-001`
2. **Setting Name:** `ApplicationPreferences`
3. **Purpose:** Governs desktop UI display, theme, language, and default project storage root.
4. **Scope:** User Global (`%LocalAppData%\NexLudic\appsettings.json`).
5. **Authoritative Owner:** `DesktopPresentationLayer` (`NexLudic.App`).
6. **Data Type:** JSON Object (`Theme`, `DefaultProjectRoot`, `TelemetryOptIn`).
7. **Default Behaviour:** Theme: `"System"`, DefaultProjectRoot: `"D:\\NexLudic\\Projects"`, TelemetryOptIn: `false`.
8. **Source / Provenance:** User configuration via Desktop Settings Dialog.
9. **Validation Requirements:** Valid directory paths on Volume `D:`, valid enum strings.
10. **Mutability:** Mutable by user at runtime.
11. **Sensitivity:** Public / Non-Sensitive.
12. **Exportable:** No (Contains machine-specific drive paths).
13. **Versioned:** Yes (Schema SemVer `1.0.0`).
14. **Override Capability:** Overrides System Defaults; overridden by Project Settings for project-specific options.
15. **Affects Permissions:** No.
16. **Related ADRs:** `ADR-0003` (Desktop UI), `ADR-0007` (Storage), `DEC-009` (Volume D:).
17. **Decision Status:** Approved Baseline.

---

### 3.2 Category 2: Project-Specific Settings (`CFG-PRJ-001`)
1. **Configuration ID:** `CFG-PRJ-001`
2. **Setting Name:** `ProjectConfiguration`
3. **Purpose:** Configures target games, active integration strategy, and custom build arguments for a specific mashup.
4. **Scope:** Project Local (`<project_root>\project.json`).
5. **Authoritative Owner:** `ProjectCoordinator` (`NexLudic.Core`).
6. **Data Type:** JSON Object.
7. **Default Behaviour:** Derived from chosen mashup template; requires explicit game paths.
8. **Source / Provenance:** User-authored during project wizard or AI-assisted blueprint generator.
9. **Validation Requirements:** JSON Schema Draft 2020-12, target game executable hashes match disk.
10. **Mutability:** Versioned-Mutable via atomic `File.Replace`.
11. **Sensitivity:** Confidential / Project Intellectual Property.
12. **Exportable:** Yes (Included in `.nxpkg` project exports).
13. **Versioned:** Yes (Project Schema SemVer `1.0.0`).
14. **Override Capability:** Overrides User Preferences; overridden by explicit operation CLI args.
15. **Affects Permissions:** No (Cannot broaden execution permissions).
16. **Related ADRs:** `ADR-0007`, `REQ-CORE-004`.
17. **Decision Status:** Approved Baseline.

---

### 3.3 Category 3: Game-Specific Adapter Configuration (`CFG-ADP-001`)
1. **Configuration ID:** `CFG-ADP-001`
2. **Setting Name:** `AdapterConfiguration`
3. **Purpose:** Defines adapter-specific operational options, telemetry sampling frequency, and compiler arguments.
4. **Scope:** Adapter-in-Project (`<project_root>\adapters\<adapter_id>.json`).
5. **Authoritative Owner:** `AdapterRegistryService` (`NexLudic.Core`).
6. **Data Type:** JSON Object.
7. **Default Behaviour:** Default sampling rate 60 Hz; default compiler debug stubs enabled.
8. **Source / Provenance:** Adapter manifest defaults merged with user project overrides.
9. **Validation Requirements:** Adapter capability schema validation; values bounded by declared ranges.
10. **Mutability:** Mutable during project configuration.
11. **Sensitivity:** Internal / Technical.
12. **Exportable:** Yes (Non-secret portions included in project export).
13. **Versioned:** Yes (Adapter Contract SemVer).
14. **Override Capability:** Overrides Engine Defaults; cannot override Security Policy.
15. **Affects Permissions:** No.
16. **Related ADRs:** `ADR-0002` (Out-of-Process), `ADR-0005`, `NLE-SPC-MSG-001`.
17. **Decision Status:** Approved Baseline.

---

### 3.4 Category 4: Environment Profile Configuration (`CFG-ENV-001`)
1. **Configuration ID:** `CFG-ENV-001`
2. **Setting Name:** `EnvironmentProfile`
3. **Purpose:** Selects active operational profile (`Development`, `Testing`, `Production`).
4. **Scope:** Workstation Execution Environment (`%LocalAppData%\NexLudic\profile.json`).
5. **Authoritative Owner:** `EnvironmentManager` (`NexLudic.Core`).
6. **Data Type:** JSON Object (`ActiveProfile`, `MockModeEnabled`, `LogLevel`).
7. **Default Behaviour:** Defaults to `"Production"` for packaged desktop app; `"Development"` in debug builds.
8. **Source / Provenance:** CLI flag `--profile <name>` or local developer setting.
9. **Validation Requirements:** Enum check (`Development` | `Testing` | `Production`).
10. **Mutability:** Read-only at application startup; requires restart to switch.
11. **Sensitivity:** Internal / Non-Sensitive.
12. **Exportable:** No.
13. **Versioned:** Yes (Profile Schema `1.0.0`).
14. **Override Capability:** Controls diagnostic sinks and synthetic stubs.
15. **Affects Permissions:** No (Testing profile isolates workspaces, does not grant elevation).
16. **Related ADRs:** `ADR-0007`, `ADR-0010`.
17. **Decision Status:** Approved Baseline.

---

### 3.5 Category 5: AI-Provider Configuration (`CFG-AIP-001`)
1. **Configuration ID:** `CFG-AIP-001`
2. **Setting Name:** `AIProviderConfiguration`
3. **Purpose:** Configures selected AI provider, target model, context limits, and token reference pointer.
4. **Scope:** User Global or Project Override.
5. **Authoritative Owner:** `AIOrchestrationService` (`NexLudic.Core`).
6. **Data Type:** JSON Object (`ProviderId`, `ModelId`, `MaxContextTokens`, `CredentialRef`).
7. **Default Behaviour:** Local SLM / Manual Prompt Handoff (Zero cloud API dependency by default).
8. **Source / Provenance:** AI Settings UI.
9. **Validation Requirements:** Valid provider identifier, known model ID, valid `CredentialRef` UUID.
10. **Mutability:** Mutable via AI Settings.
11. **Sensitivity:** Confidential (Contains model parameters and credential pointers, but NEVER raw keys).
12. **Exportable:** No (Credential pointers are machine-local).
13. **Versioned:** Yes (Schema SemVer `1.0.0`).
14. **Override Capability:** Project AI settings override Global AI settings.
15. **Affects Permissions:** No.
16. **Related ADRs:** `ADR-0008` (AI Abstraction), `REQ-AI-001`.
17. **Decision Status:** Approved Baseline.

---

### 3.6 Category 6: External Toolchain Locations (`CFG-TLS-001`)
1. **Configuration ID:** `CFG-TLS-001`
2. **Setting Name:** `ToolchainConfiguration`
3. **Purpose:** Discovered and verified local filesystem paths to external game compilers, SDKs, and mod tools.
4. **Scope:** User Global (`%LocalAppData%\NexLudic\toolchains.json`).
5. **Authoritative Owner:** `ToolchainDiscoveryService` (`NexLudic.Infrastructure`).
6. **Data Type:** JSON Object (`Tools: { ToolId: { ExecutablePath, Version, Sha256Hash } }`).
7. **Default Behaviour:** Auto-discovered via registry / standard Steam library inspection.
8. **Source / Provenance:** Environment audit and explicit user confirmation.
9. **Validation Requirements:** File must exist, match declared SHA-256 hash, and execute without elevation.
10. **Mutability:** Mutable via Tool Settings.
11. **Sensitivity:** Internal / Local Path Metadata.
12. **Exportable:** No (Paths are local machine-specific).
13. **Versioned:** Yes.
14. **Override Capability:** Project-local tool override takes precedence over global auto-discovery.
15. **Affects Permissions:** Yes (Only whitelisted tools can be executed by workers).
16. **Related ADRs:** `ADR-0005`, `ADR-0009`.
17. **Decision Status:** Approved Baseline.

---

### 3.7 Category 7: Execution & Security Policy (`CFG-SEC-001`)
1. **Configuration ID:** `CFG-SEC-001`
2. **Setting Name:** `MandatorySecurityPolicy`
3. **Purpose:** Non-negotiable platform safety rules: staging jail boundaries, anti-cheat blocking, diff gating.
4. **Scope:** Engine Wide (Hardcoded Core Assembly Constants).
5. **Authoritative Owner:** `SecurityGovernor` (`NexLudic.Core`).
6. **Data Type:** Immutable C# Invariant Rules.
7. **Default Behaviour:** Staging jail strictly enforced; human approval required for all diffs; Medium IL only.
8. **Source / Provenance:** Core Engine Architecture Baseline.
9. **Validation Requirements:** Evaluated at every I/O and process launch boundary.
10. **Mutability:** **IMMUTABLE / HARDCODED.** Cannot be modified by config files or user settings.
11. **Sensitivity:** Public / Security Policy Standard.
12. **Exportable:** Yes (Published policy).
13. **Versioned:** Engine Core Version.
14. **Override Capability:** **NON-OVERRIDABLE.** Highest priority in precedence hierarchy.
15. **Affects Permissions:** **CRITICAL.** Enforces zero-elevation and sandbox containment.
16. **Related ADRs:** `ADR-0001`, `ADR-0002`, `ADR-0009`, `REQ-SEC-001`, `REQ-SAFE-001`.
17. **Decision Status:** Approved Baseline.

---

### 3.8 Category 8: Secrets & Protected References (`CFG-SCR-001`)
1. **Configuration ID:** `CFG-SCR-001`
2. **Setting Name:** `SecretsVault`
3. **Purpose:** Encrypted storage of external API tokens and secrets.
4. **Scope:** Windows User Profile Vault (`%LocalAppData%\NexLudic\credentials.dat`).
5. **Authoritative Owner:** `SecretsManager` (`NexLudic.Infrastructure` via Windows DPAPI).
6. **Data Type:** Encrypted Binary Blob (AES-256-GCM / DPAPI CurrentUser scope).
7. **Default Behaviour:** Empty vault; zero keys stored.
8. **Source / Provenance:** User input via masked UI credential dialog.
9. **Validation Requirements:** Valid token format for targeted provider; encrypted immediately.
10. **Mutability:** Mutable via Credential Manager UI (Add, Update, Revoke).
11. **Sensitivity:** **RESTRICTED / HIGH SENSITIVITY.**
12. **Exportable:** **STRICTLY FORBIDDEN.** Never exported under any circumstance.
13. **Versioned:** Vault Format SemVer `1.0.0`.
14. **Override Capability:** Not applicable (Opaque credential store).
15. **Affects Permissions:** Enables authenticated outbound AI calls.
16. **Related ADRs:** `ADR-0008`, `REQ-SEC-001`, `REQ-SAFE-002`.
17. **Decision Status:** Approved Baseline.

---

## 4. Deterministic Precedence Hierarchy & Conflict Resolution

When a component requests a configuration setting, the **Configuration Resolution Engine** evaluates sources according to the following strict 7-Layer Precedence Hierarchy:

```
+----------------------------------------------------------------------------------------------------+
|                                DETERMINISTIC PRECEDENCE HIERARCHY                                  |
+-------+---------------------------------------+---------------------+------------------------------+
| Layer | Configuration Source                  | Scope               | Can Override Previous Layer? |
+-------+---------------------------------------+---------------------+------------------------------+
| **1** | **Mandatory Security Policy**         | Engine Core         | **NEVER (Supreme Authority)**|
| **2** | **Invocation Command Arguments (CLI)**| Transient Operation | Overrides Layers 3 - 7       |
| **3** | **Project-Specific Configuration**    | <project>/project   | Overrides Layers 4 - 7       |
| **4** | **Adapter-Specific Configuration**    | <project>/adapters  | Overrides Layers 5 - 7       |
| **5** | **Active Environment Profile**        | Workstation Profile | Overrides Layers 6 - 7       |
| **6** | **User Global Preferences**           | User AppData        | Overrides Layer 7            |
| **7** | **System Embedded Defaults**          | Core Assembly       | Lowest Priority Fallback     |
+-------+---------------------------------------+---------------------+------------------------------+
```

### 4.1 Non-Overridable Security Rules:
1. **Staging Jail Invariant:** A project file setting `"staging_path": "C:\\Windows\\System32"` is intercepted at Layer 1 and rejected. Security policy mandates staging paths MUST resolve within `<project_root>/staging/`.
2. **Approval Gate Invariant:** A project setting `"auto_deploy_without_review": true` is hard-blocked at Layer 1. Human diff review is non-overridable.
3. **Zero Elevation Invariant:** A setting requesting administrative execution (`require_uac: true`) is rejected with `SECURITY_UNAUTHORIZED_CALLER`.

### 4.2 The "Effective Configuration" Model:
To provide complete transparency, the UI provides an **Effective Configuration Inspector** showing:
* **Effective Value:** The computed value currently used by the runtime.
* **Originating Source:** Which layer supplied the winning value (e.g. `ProjectConfiguration`).
* **Superseded Sources:** Any lower-priority values that were overridden.
* **Lock State:** Whether the setting is locked by Mandatory Security Policy.

