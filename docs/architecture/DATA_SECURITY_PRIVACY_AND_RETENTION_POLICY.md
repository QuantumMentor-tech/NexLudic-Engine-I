# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Data Security, Privacy & Retention Policy

**Document ID:** `NLE-POL-SEC-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 14 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Security & Privacy Standard  
**Traceability:** `REQ-SEC-001` (Zero Elevation), `REQ-SAFE-002` (Local Privacy), `ADR-0007` (Storage Baseline), `ADR-0009` (Workspace Isolation)

---

## 1. Executive Summary & Policy Scope

The **Data Security, Privacy, and Retention Policy** establishes the non-negotiable security requirements for persistent data, project workspaces, secrets, diagnostic logs, and exported packages in NexLudic Engine.

Because NexLudic Engine integrates AI coding agents, third-party modding tools, and user-authored game scripts, the persistence architecture must rigorously defend against credential leaks, directory traversal vulnerabilities, malicious package execution, and inadvertent data destruction.

---

## 2. Four-Tier Data Sensitivity Classification

All data managed by the platform is assigned to one of four sensitivity tiers:

```
+----------------------------------------------------------------------------------------------------+
|                                    DATA SENSITIVITY TIERS                                          |
+----+--------------+------------------------------------+-------------------------------------------+
| Tier| Classification| Scope & Examples                   | Storage & Access Controls                 |
+----+--------------+------------------------------------+-------------------------------------------+
| 01 | PUBLIC       | Official game schemas, public wiki | Plaintext JSON. Safe for distribution.    |
|    |              | citations, open adapter descriptors|                                           |
+----+--------------+------------------------------------+-------------------------------------------+
| 02 | INTERNAL     | Engine architecture specs, test    | Local filesystem only. Non-distributive.  |
|    |              | specifications, component contracts|                                           |
+----+--------------+------------------------------------+-------------------------------------------+
| 03 | CONFIDENTIAL | User project designs, custom mod   | User profile / Volume D: storage. Never   |
|    |              | scripts, local workspace paths     | transmitted without user consent.         |
+----+--------------+------------------------------------+-------------------------------------------+
| 04 | RESTRICTED   | AI Provider API keys, access       | Encrypted via Windows DPAPI. NEVER stored |
|    |              | tokens, private signing keys       | in project manifests or diagnostic logs.  |
+----+--------------+------------------------------------+-------------------------------------------+
```

---

## 3. Secret & Credential Handling Policy (`REQ-SEC-001`, `REQ-SAFE-002`)

To eliminate credential leaks:
1. **Absolute Manifest Prohibition:** API keys, access tokens, and passwords MUST NEVER be stored in `project.json`, `mashup-blueprint.json`, or any other file within a project workspace directory.
2. **Windows DPAPI Encryption:** User credentials for external AI providers (e.g. Gemini, Anthropic, OpenAI) are encrypted using the **Windows Data Protection API (DPAPI)** with `DataProtectionScope.CurrentUser` and stored exclusively in `%LocalAppData%/NexLudic/credentials.dat`.
3. **Export Package Scrubbing:** The project export subsystem strictly checks and refuses to package any files containing credential tokens or environment variable assignments (`.env`, `api_key`).
4. **Log Scrubber Filter:** The Serilog diagnostic logging sink is configured with regex redaction masks that automatically replace API keys, tokens, and authorization headers with `[REDACTED]`.

---

## 4. Workspace Path Security & Directory Traversal Defense

To prevent untrusted modding tools or buggy scripts from overwriting system files:

```
[ Incoming File Path from Adapter / Tool / Script ]
                         |
                         v
1. Path Normalization & Canonicalization (Path.GetFullPath)
                         |
                         v
2. Security Containment Boundary Check:
   targetPath.StartsWith(projectStagingRoot, StringComparison.OrdinalIgnoreCase)
                         |
           +-------------+-------------+
           | True                      | False
           v                           v
   [ ALLOW WRITE ]             [ REJECT IMMEDIATELY ]
   - Proceed with atomic write - Log Security Alert: SECURITY_SANDBOX_VIOLATION
                               - Throw UnauthorizedAccessException
```

### Safety Rules:
* Target paths containing `..` or leading slashes that resolve outside `<project_root>/staging/` are rejected before file handles are requested.
* Windows Junctions, Symlinks, and Hard Links inside the staging jail are inspected; links pointing outside the project boundary are rejected.

---

## 5. Untrusted Imported Data & Archive Policy

Exported project archives (`.nxpkg`) and third-party community blueprints must be treated as **UNTRUSTED**:
1. **Zero Auto-Execution:** The Core MUST NOT execute binaries, scripts, or installer hooks contained within an imported archive upon import.
2. **Schema & Anti-Virus Scans:** Imported JSON files are validated against JSON Schemas; binary files are placed in an isolated quarantine directory (`.staging_untrusted/`) until user-approved.
3. **Manual Approval Gate (`ADR-0009`):** The user is presented with a comprehensive manifest summary of all files contained within an import package before they are merged into an active project.

---

## 6. Data Retention, Archiving & Safe Deletion Rules

```
+----------------------------------------------------------------------------------------------------+
|                                    DATA RETENTION STANDARDS                                        |
+---------------------+-------------------+---------------------+------------------------------------+
| Data Category       | Retention Window  | Action at Expiry    | Deletion Safety Rule               |
+---------------------+-------------------+---------------------+------------------------------------+
| Active Projects     | Indefinite        | Active on Volume D: | Cannot be deleted without confirm. |
| Soft-Deleted Proj   | 30 Days           | Moved to .trash/    | Single-click restoration available.|
| Archived Projects   | Indefinite        | Compressed on D:    | Read-only; staging caches cleaned. |
| Evidence Records    | Permanent / Audit | Immutable Archive   | CANNOT BE DELETED if cited.        |
| Test Execution Logs | Last 10 Runs      | FIFO Prune Old Pass | Failed test runs retained longer.  |
| Diagnostic Logs     | 14 Days           | Auto-purge (Serilog)| Capped at 500 MB total log volume. |
| In-Flight Journals  | 24 Hours          | Auto-cleanup        | Cleared after successful commit.   |
+---------------------+-------------------+---------------------+------------------------------------+
```

### Deletion Protections:
* **Evidence Protection:** Historical evidence citations (`DAT-EVD-001`) can NEVER be deleted while an active project references them, even if newer contradictory tests exist.
* **User Content Distinction:** When cleaning a mod workspace, NexLudic Engine checks `artifacts-manifest.json`. Files not generated by the engine (user-created scripts) are preserved and never deleted automatically.

