# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Environment Profiles & Isolation Policy

**Document ID:** `NLE-POL-ENV-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Environment Policy & Sandbox Specification  
**Traceability:** `ADR-0007` (Storage Baseline), `ADR-0009` (Workspace Isolation), `ADR-0010` (Testing Strategy), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Purpose

The **Environment Profiles and Isolation Policy** defines the operational profiles supported by NexLudic Engine and establishes strict resource isolation boundaries to ensure that automated testing fixtures, developer debugging options, and synthetic provider stubs never contaminate or endanger production user data, live game installations, or real credentials.

---

## 2. The Four Standard Environment Profiles

NexLudic Engine formally establishes four distinct operational profiles:

```
                              NEXLUDIC ENVIRONMENT PROFILES
                                             |
    +--------------------+-------------------+--------------------+--------------------+
    |                    |                   |                    |                    |
 1. PRODUCTION        2. DEVELOPMENT      3. TESTING           4. ISOLATED VALIDATION
    (Default User App)   (Internal Dev)      (Automated QA/CI)    (Untrusted Adapters)
    - Real DPAPI vault   - Mock/Local tools  - Synthetic fixtures - Maximum containment
    - Vol D: projects    - Verbose Serilog   - Ephemeral temp dirs- Zero network egress
    - Standard Medium IL - Hot reload enabled- Fast fail-asserts  - Strict memory caps
```

### 2.1 Profile 1: Production (`Production`) — Default
* **Target Audience:** End-user desktop gamers, modders, and creators.
* **Operational Characteristics:**
  - Default profile for packaged Windows desktop distributions.
  - Strict security policy: staging jail strictly enforced; mandatory human-in-the-loop diff review.
  - Minimal diagnostic collection (Serilog set to `Information` / `Warning`); 14-day rolling auto-purge.
  - Uses the real Windows DPAPI credential vault for configured AI providers.
  - Project root default: `D:\NexLudic\Projects\` (`DEC-009`).

### 2.2 Profile 2: Development (`Development`)
* **Target Audience:** Core platform engineers and adapter authors.
* **Operational Characteristics:**
  - Activated via `--profile Development` CLI argument or visual dev switcher.
  - Verbose diagnostic logging enabled (`Debug` / `Verbose`).
  - Supports local mock toolchains and hot-reloading of UI components.
  - Uses segregated dev project directory: `D:\NexLudic\DevProjects\`.
  - UI displays an unmistakable high-contrast banner: `[DEVELOPMENT ENVIRONMENT ACTIVE]`.

### 2.3 Profile 3: Testing (`Testing`)
* **Target Audience:** Automated test suites, CI pipelines, and regression runners.
* **Operational Characteristics:**
  - Activated via `--profile Testing` or test runner harnesses.
  - Purely synthetic credentials; real DPAPI user vault is **HARD-LOCKED** and inaccessible.
  - Workspaces placed in isolated ephemeral directories (`D:\NexLudic\TempTest\<guid>\`) and auto-purged on test completion.
  - External AI providers replaced with deterministic mock runners (`MockAgentRunner`).
  - Game installations replaced with synthetic mock filesystems to prevent accidental edits to live games.

### 2.4 Profile 4: Isolated Validation (`IsolatedValidation`)
* **Target Audience:** Creators testing unverified, third-party community adapters.
* **Operational Characteristics:**
  - Strictest Win32 Job Object sandbox: memory commit cap reduced to 1 GB; process execution timeout capped at 60 seconds.
  - Outbound network access completely blocked at the OS socket level.
  - Staging jail monitored with real-time canary tripwires.

---

## 3. Profile Selection & Anti-Tampering Rules

To prevent accidental profile crossover or privilege escalation:

```
[ Application Boot: Command Line & Environment Evaluation ]
                            |
                            v
1. Inspect CLI Parameter: --profile <name>
   - If omitted: Default to "Production" (Packaged) or "Development" (Debug build)
                            |
                            v
2. Security Validation Gate:
   - Did a project manifest request a profile switch?
     * YES: REJECT! Project files are strictly FORBIDDEN from selecting profiles.
     * NO: Proceed.
                            |
                            v
3. Lock Active Profile for Process Lifetime
   - Active profile is immutable once initialized; requires process restart to alter.
                            |
                            v
4. Render UI Environment Banner if Non-Production
```

---

## 4. Resource Isolation Matrix Across Profiles

| Resource Domain | Production Profile | Development Profile | Testing Profile | Isolated Validation Profile |
| :--- | :--- | :--- | :--- | :--- |
| **Project Workspace Root** | `D:\NexLudic\Projects\` | `D:\NexLudic\DevProjects\` | `D:\NexLudic\TempTest\<guid>\` | `D:\NexLudic\Quarantine\<id>\` |
| **Credential Vault** | Real Windows DPAPI | Real DPAPI (Dev tagged) | **Mock Vault (Synthetic)** | **Zero Credentials (Empty)** |
| **Diagnostic Log Path** | `%LocalAppData%\logs\prod.log` | `%LocalAppData%\logs\dev.log` | `%LocalAppData%\logs\test.log` | `%LocalAppData%\logs\quarantine.log` |
| **Serilog Log Level** | `Information` | `Debug` | `Verbose` | `Debug` |
| **Job Object Memory Limit**| 2.0 GB Commit Limit | 4.0 GB Commit Limit | 1.0 GB Commit Limit | **1.0 GB Hard Cap** |
| **AI Agent Providers** | User-Configured Providers | Local CLI / Mocks | **Deterministic Mocks Only**| **Hard-Blocked (Offline Only)** |
| **Network Egress** | Permitted (AI endpoints) | Permitted (Local / Web) | **Localhost Only** | **Completely Blocked** |
| **Game Executable Hooks** | Offline single-player only | Offline single-player only | **Synthetic Mock Dirs Only**| **Synthetic Mock Dirs Only** |

---

## 5. Production Safeguards & Release Invariants

1. **Release Compiler Invariant:** Production release builds of NexLudic Engine MUST compile with `#DEBUG` directives disabled, stripping out development bypasses.
2. **Zero Synthetic Keys in Release:** Test runner synthetic credentials are compiled strictly within test assemblies; release binaries contain zero mock keys.
3. **Fail-Closed on Unknown Profile:** If an unapproved profile name is supplied (`--profile HackerMode`), the engine logs a fatal error and terminates immediately.

