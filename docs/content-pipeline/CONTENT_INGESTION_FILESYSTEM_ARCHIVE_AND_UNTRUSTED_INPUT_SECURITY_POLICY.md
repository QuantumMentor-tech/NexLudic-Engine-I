# Content Ingestion Filesystem, Archive & Untrusted-Input Security Policy

**Document Identifier:** `NLE-POL-CIS-001`  
**Classification:** Information Security & Containment Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-SEC-CSG-001`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Ingestion Threat Model

Content ingestion represents an untrusted attack surface: community mod archives, user-supplied folders, and network endpoints can contain malicious payloads designed to compromise the host operating system. This policy establishes mandatory security boundaries for filesystem traversal, archive inspection, script handling, and resource containment.

---

## 2. Filesystem Containment & Path Traversal Guards

```
[Candidate Entry Discovered]
             |
             +---> [Check 1: Disallow Absolute Path Schemes ("C:\...", "/etc/...")]
             |
             +---> [Check 2: Disallow Path Traversal Characters ("../", "..\")]
             |
             +---> [Check 3: Disallow Windows Device Names (CON, PRN, AUX, NUL, COM1..9)]
             |
             +---> [Check 4: Canonical Prefix Confinement]
                      canonical_dest.startswith(canonical_staging_root)
             |
             +---> [Check 5: Symlink & Reparse Point Resolution]
                      Reject if symlink target is outside authorized source root
```

Any violation of Checks 1 through 5 triggers immediate quarantine and aborts ingestion with fatal error code `ERR_SEC_PATH_TRAVERSAL`.

---

## 3. Safe Archive Inspection Standards (Zip Slip & Zip Bomb Mitigations)

When inspecting `.zip`, `.7z`, `.tar`, or proprietary package archives (`.pak`, `.ba2`):

| Security Parameter | Hard Limit | Enforcement Action on Breach |
| :--- | :--- | :--- |
| **Max Uncompressed Size** | 10 GB per archive | Abort extraction (`ERR_SEC_ARCHIVE_OVERSIZED`) |
| **Max Compression Ratio** | $100:1$ | Abort extraction (`ERR_SEC_ZIP_BOMB_DETECTED`) |
| **Max Nested Archive Depth** | 2 levels deep | Reject nested archive (`ERR_SEC_NESTED_ARCHIVE_LIMIT`)|
| **Max Entry Count** | 50,000 files per archive | Abort extraction (`ERR_SEC_ENTRY_COUNT_EXCEEDED`)|
| **Executable Archive Members**| Zero `.exe`, `.dll`, `.so`, `.sys` | Immediate quarantine (`ERR_SEC_EXECUTABLE_PAYLOAD`)|

---

## 4. Script-Bearing Content & Zero-Execution Rule

In accordance with Invariant `INV-CID-01`:
1. **Zero Execution During Ingestion:** Discovery, format probing, and metadata extraction MUST NEVER execute Python, Lua, Papyrus, C#, or PowerShell script files.
2. **Abstract Syntax Tree (AST) Inspection:** If a script asset is inspected for dependencies or metadata, it must be parsed using a sandboxed, static AST parser.
3. **Prohibited System Calls:** The AST analyzer must flag and reject any script referencing:
   - Operating system shell commands (`os.system`, `subprocess`, `ShellExecute`).
   - Network socket creation (`socket`, `urllib`, `curl`).
   - Arbitrary filesystem writes outside user mod directories.
   - Dynamic reflection or code evaluation (`eval`, `loadstring`).

---

## 5. Prohibited Host-Game Interactions

The ingestion subsystem runs strictly at Windows Medium Integrity Level and is explicitly prohibited from:
- Attaching debuggers to running game processes (`DebugActiveProcess`).
- Injecting DLLs into target game processes (`CreateRemoteThread`, `VirtualAllocEx`).
- Overwriting or byte-patching official game executables (`SkyrimSE.exe`, etc.).
- Modifying anti-cheat driver registry keys or service configurations.
