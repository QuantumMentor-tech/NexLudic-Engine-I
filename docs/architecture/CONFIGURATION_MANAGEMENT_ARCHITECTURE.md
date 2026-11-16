# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Configuration Management Architecture

**Document ID:** `NLE-ARC-CFG-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Architecture Specification  
**Traceability:** `ADR-0007` (Configuration Baseline), `ADR-0008` (AI Provider Abstraction), `ADR-0009` (Workspace Isolation & Approval), `REQ-CORE-004` (Lifecycle Management), `REQ-SEC-001` (Zero Elevation), `REQ-SAFE-002` (Local Privacy), `DEC-009` (Volume D: Allocation)

---

## 1. Executive Summary & Purpose

The **NexLudic Engine** is a Windows-first, AI-assisted game mashup and interoperability platform. It coordinates diverse external runtimes, modding compilers, game executables, AI coding agents, and user-defined integration blueprints.

Because different game integrations require wildly divergent toolchains (e.g. Skyrim Creation Kit / Papyrus compilers vs. Subnautica BepInEx / C# / .NET runtimes vs. Godot GDScript tools) and communicate with external AI providers, configuration management must guarantee:
1. **Scope and Authority Separation:** Global application preferences, project-specific settings, adapter configurations, and security policies must never collide or grant unauthorized privilege escalation.
2. **Deterministic Precedence:** An explicit hierarchy governs setting resolution. Project-level settings can customize build parameters, but can NEVER override mandatory system security restrictions (e.g. disabling staging jails or bypassing human approval gates).
3. **Local-First & Offline Privacy:** Core engine features operate 100% offline without requiring network connections. External AI services remain strictly optional.
4. **Secret Separation & Zero Key Leakage:** Sensitive credentials (e.g. AI provider API keys) are cryptographically segregated using Windows Data Protection API (DPAPI) and NEVER stored in ordinary project JSON files, git repositories, export packages, or diagnostic logs.

This document establishes the **Configuration Management Architecture**, defining the configuration categories, component boundaries, precedence model, environment profiles, and security invariants for the platform.

```
+----------------------------------------------------------------------------------------------------+
|                                NEXLUDIC CONFIGURATION ARCHITECTURE                                 |
|                                                                                                    |
|  +--------------------+   +-----------------------+   +--------------------+   +----------------+  |
|  | Desktop UI Shell   |   | Project Coordinator   |   | AI Orchestrator    |   | Adapter Runner |  |
|  +---------+----------+   +-----------+-----------+   +---------+----------+   +--------+-------+  |
+------------|--------------------------|-------------------------|-----------------------|----------+
             | Request Settings         | Project Parameters      | Provider Options      | Toolpaths
             v                          v                         v                       v          
+----------------------------------------------------------------------------------------------------+
|                             CONFIGURATION MANAGER & RESOLUTION ENGINE                              |
|                                                                                                    |
|  [ LAYER 1: MANDATORY SECURITY POLICY ] (Hardcoded Engine Invariants - NON-OVERRIDABLE)            |
|  - Staging Jail Enforcement (<project>/staging/) | Zero Administrative Elevation (Medium IL)       |
|  - Human-in-the-Loop Diff Approval Gate         | Anti-Cheat Hard Block                            |
|                                                                                                    |
|  [ LAYER 2: ENVIRONMENT PROFILES ] (Development | Testing | Production)                             |
|  - Diagnostic Verbosity | Fixture Path Isolation | Synthetic Provider Stubs                        |
|                                                                                                    |
|  [ LAYER 3: PROJECT-SPECIFIC CONFIGURATION ] (<project>/project.json)                              |
|  - Target Game Editions & Hashes | Strategy Mapping | Custom Build Arguments                        |
|                                                                                                    |
|  [ LAYER 4: ADAPTER-SPECIFIC SETTINGS ] (<project>/adapters/<id>.json)                             |
|  - Compiler Flags | Custom Mod Staging Prefixes | Telemetry Sampling Rates                         |
|                                                                                                    |
|  [ LAYER 5: USER-WIDE PREFERENCES ] (%LocalAppData%/NexLudic/appsettings.json)                     |
|  - UI Theme & Layout | Default Volume D: Project Root | Tool Discovery Cache                       |
|                                                                                                    |
|  [ LAYER 6: SYSTEM DEFAULTS ] (Embedded Engine Fallbacks)                                          |
|                                                                                                    |
|  [ SEGREGATED SECRETS STORE ] (Windows DPAPI Protected - %LocalAppData%/NexLudic/credentials.dat) |
|  - AI Provider Tokens (Gemini, Claude, OpenAI) | Referenced by ID; NEVER serialized in manifests    |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Foundational Configuration Architecture Principles

The configuration subsystem is governed by twelve architectural principles:

1. **Clear Ownership & Scoping:** Every configuration setting has an explicit owner component, defined lifecycle, and unambiguous scope (Global, Project, Adapter, Environment, or Security).
2. **Deterministic Precedence:** Precedence order is strictly enforced and auditable. A lower-priority source never silently overwrites a higher-priority value.
3. **Secure Defaults & Fail-Closed:** Missing, corrupt, or unparseable security-sensitive settings default to the safest, most restrictive posture. Permissions are never granted by default.
4. **Secret Segregation:** Credential values are stored in a dedicated, encrypted secrets vault (DPAPI) and represented in configuration manifests strictly as non-secret opaque references (`credential_ref: "cred-gemini-dev"`).
5. **Least Privilege & No Policy Overrides:** Project-level settings, adapter manifests, and AI-generated blueprints are strictly prohibited from relaxing system security rules or escaping staging jails.
6. **Explicit Schema Validation:** All configuration sources are validated against formal JSON Schema Draft 2020-12 specifications upon load. Unknown security fields trigger load rejections.
7. **Reproducibility & Provenance:** Every build, compilation, and test execution captures a hash of its effective non-secret configuration to guarantee exact reproducibility.
8. **Local-First Autonomy:** The platform functions fully offline. External AI provider settings are isolated behind the `IAgentRunner` provider abstraction (`ADR-0008`).
9. **Environment Profile Isolation:** Development, Testing, and Production profiles use isolated workspace directories, separate diagnostic logs, and distinct mock/real credential stores.
10. **Version-Aware Evolution:** Configuration schemas are versioned via SemVer 2.0.0. Forward and backward migration rules prevent silent data loss or setting corruption.
11. **User Transparency & Traceability:** The UI provides an "Effective Configuration Inspector" that reveals the computed value of every setting, its source, and any overridden values.
12. **Minimal Necessary Complexity:** Configuration utilizes human-readable structured JSON files (`ADR-0007`) on Volume `D:` (`DEC-009`); external cloud configuration servers are avoided.

---

## 3. Configuration Component Boundaries & Dependency Rules

In compliance with Step 10 Component Architecture (`CORE_COMPONENT_ARCHITECTURE.md`) and Step 14 Persistence Architecture (`PERSISTENCE_AND_DATA_MANAGEMENT_ARCHITECTURE.md`), configuration responsibilities are strictly allocated:

```
[ NexLudic.App (Presentation Layer) ]
   - Renders Settings UI, Profile Selector, and Diff Review Dialogs
   - Zero direct file I/O; consumes strongly-typed settings from Core
             |
             v
[ NexLudic.Core (Application Workflows) ]
   - Defines configuration models: ProjectSettings, AdapterSettings, EnginePolicy
   - Evaluates EffectiveConfiguration via IConfigurationResolver
   - Enforces MandatorySecurityPolicy gates
             |
             v
[ NexLudic.Domain (Pure Business Invariants) ]
   - Defines configuration contracts and validation rules
   - Zero references to System.IO, DPAPI, or Serilog
             ^
             | Implements persistence & crypto
             |
[ NexLudic.Infrastructure (Concrete Storage & Crypto Adapters) ]
   - JsonConfigurationRepository (Atomic File.Replace, JSON Schema validation)
   - DpapiSecretsStore (Windows DPAPI cryptographic envelope)
   - EnvironmentProfileLoader (Profile directory resolver)
```

---

## 4. Configuration Storage Architecture & Volume Allocation

Configuration is physically segregated across storage tiers to prevent credential leaks and satisfy capacity limits (`DEC-009`):

```
+----------------------------------------------------------------------------------------------------+
|                               PHYSICAL CONFIGURATION LOCATIONS                                     |
+--------------------------+-------------------------------------+-----------------------------------+
| Configuration Scope      | Physical Path                       | Storage Mechanism & Security      |
+--------------------------+-------------------------------------+-----------------------------------+
| System Defaults          | Embedded in Core DLL Assemblies     | Read-only embedded resource       |
| User Global Preferences  | %LocalAppData%\NexLudic\appsettings.json | Plaintext JSON (Atomic Write)    |
| Segregated Secrets Store | %LocalAppData%\NexLudic\credentials.dat| Binary AES-GCM via Windows DPAPI |
| Project Settings         | D:\...\projects\<id>\project.json   | Plaintext JSON (Git-trackable)    |
| Adapter Configurations   | D:\...\projects\<id>\adapters\*.json| Plaintext JSON (Staging bound)    |
| Environment Profiles     | %LocalAppData%\NexLudic\profiles\*.json| Plaintext JSON (Admin/Dev only)   |
| Diagnostic Log Config    | %LocalAppData%\NexLudic\serilog.json| Plaintext JSON (Serilog sinks)    |
+--------------------------+-------------------------------------+-----------------------------------+
```

---

## 5. Architectural Decisions & Status

| Decision Item | Approved Baseline | Alternative Evaluated | Status | Governance Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Settings Format** | Structured JSON (Schema 2020-12) | XML / YAML / TOML | **Approved (`ADR-0007`)** | Universal runtime support in .NET 8, human readable, git diffable. |
| **Secret Storage** | Windows DPAPI Local Store | Plaintext `.env` / HashiCorp Vault | **Approved (`REQ-SEC-001`)** | Native OS security, zero external server dependencies, user-scoped. |
| **AI Provider Model** | Dual-Layer MCP + Runner | Hardcoded Vendor Cloud SDKs | **Approved (`ADR-0008`)** | Zero vendor lock-in; supports local SLMs and offline tools. |
| **Security Precedence** | Mandatory Policy > Project Config | Project overrides all | **Approved (`ADR-0009`)** | Prevents malicious/buggy projects from bypassing staging sandboxes. |
| **Profile Isolation** | Isolated directories on Vol D: | In-place shared database | **Approved Baseline** | Eliminates state contamination between test fixtures and user work. |

