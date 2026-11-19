# Game Content Security, Trust & License Governance Specification

**Document Identifier:** `NLE-SEC-CSG-001`  
**Classification:** Information Security & IP Compliance Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-CPM-001`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Security Scope & Threat Model

Cross-game modification pipelines ingest third-party archives, community assets, and automated agent transformations. This specification establishes mandatory defenses against:
1. **Malicious Payloads & Code Execution:** Trojans disguised as textures, mesh exploits, buffer overflows in asset parsers.
2. **Archive Path Traversal (Zip Slip):** Extracting files outside target mod directories (e.g., `../../Windows/System32`).
3. **Decompression Bombs (Zip Bombs):** High-ratio compressed archives designed to exhaust disk or memory.
4. **Script & Logic Exploits:** Unsandboxed Lua, Python, or C# scripts making unauthorized OS system calls.
5. **Prompt & Context Injection:** Malicious metadata or comments embedded in assets targeting downstream AI agents.
6. **Intellectual Property & Licensing Infringement:** Redistribution of proprietary game assets without legal authorization.

---

## 2. Content Trust Classification

All incoming content is assigned a trust tier that dictates pipeline privileges:

| Trust Tier | Definition | Inspection Level | Privilege Bounds |
| :--- | :--- | :--- | :--- |
| **Tier 1: Official Verified** | Signed by NexLudic official release keys or verified partners. | Standard static verification | Full static modification capabilities |
| **Tier 2: Community Curated** | Sourced from vetted mod repositories with public source code. | Deep static analysis & AST scan | Static assets & sandboxed scripts |
| **Tier 3: Untrusted Third-Party**| Sourced from arbitrary URLs, local user folders, or unknown agents. | Quarantined sandbox, full binary inspection | Static assets only; NO executable scripts |

---

## 3. Safe Archive Inspection & Path Traversal Defenses

Every archive extracted during Stage C, E, or K must pass strict safe-extraction filters:

```
[Archive Received]
       |
       +---> [Header & Magic Byte Verification] (Reject mismatches)
       |
       +---> [Decompression Ratio Check] (Max ratio 100:1; max uncompressed size 10 GB)
       |
       +---> [Canonical Path Sanitization]
                * Disallow absolute paths (e.g., "C:\...", "/etc/...")
                * Disallow parent traversal ("../" or "..\")
                * Disallow reserved Windows device names (CON, PRN, AUX, NUL, COM1..9, LPT1..9)
                * Enforce target staging root constraint:
                  canonical_target.startswith(canonical_staging_root)
```

---

## 4. Script-Bearing Content Governance

If a modification package contains executable script assets (e.g. Lua, Python, Papyrus, C# script files):
1. **Tier 3 Ban:** Untrusted Third-Party packages containing executable scripts are rejected at Stage L (`ERR_SEC_UNTRUSTED_SCRIPT`).
2. **Static AST Analysis:** All scripts must undergo abstract syntax tree (AST) inspection to detect banned system calls:
   - File I/O outside sandboxed user mod directory.
   - Network socket creation, HTTP requests, or DNS queries.
   - Process execution, shell spawning (`os.system`, `subprocess`, `ShellExecute`).
   - Dynamic code evaluation (`eval`, `loadstring`, reflection).
3. **Execution Sandbox:** At runtime (Phase 9), scripts run strictly within isolated engine runtimes without host OS privileges.

---

## 5. Intellectual Property & License Compliance

NexLudic Engine enforces strict copyright and licensing governance:
1. **SPDX License Identifiers:** Every package manifest MUST declare an SPDX license identifier (`license_spdx`).
2. **Proprietary Asset Protection:** The engine prohibits packaging or redistributing raw, unmodified proprietary assets extracted from commercial games.
3. **Cross-Game Asset Porting Rules:** When assets are adapted from Game A to Game B:
   - Creator must possess licensing rights or explicit modding permission for both titles.
   - Package manifest must explicitly document original asset provenance and fair-use / permission documentation.
