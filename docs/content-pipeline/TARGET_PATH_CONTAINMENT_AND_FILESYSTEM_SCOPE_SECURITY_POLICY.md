# Target Path Containment and Filesystem Scope Security Policy

**Document Identifier:** `NLE-POL-TPC-001`  
**Lifecycle Stage:** Content Pipeline Stage I Security Policy  
**Parent Document:** [HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_TARGET_VALIDATION_PACKAGE_MAPPING_AND_APPLICATION_PLANNING_SPECIFICATION.md) (`NLE-SPC-TVP-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Filesystem Security & Sandbox Containment  

---

## 1. Objective & Containment Boundary

The **Target Path Containment and Filesystem Scope Security Policy** establishes strict spatial and navigational boundaries for all resource destinations mapped in an application plan.

It guarantees that under no circumstances can package metadata, malicious path injection, or misconfigured mapping rules allow proposed operations to escape the authorized target game directory or access sensitive operating system and user files.

---

## 2. Authorized Target Scope vs. Forbidden Zones

```
+--------------------------------------------------------------------------------------------------+
|                            FILESYSTEM CONTAINMENT ARCHITECTURE                                   |
+--------------------------------------------------------------------------------------------------+
|                                                                                                  |
|   AUTHORIZED TARGET BOUNDARY:                                                                    |
|   <TargetInstallRoot>/ (e.g., D:\SteamLibrary\steamapps\common\Skyrim Special Edition\)          |
|   |-- Data/                       --> ALLOWED (Meshes, Textures, Sound, Scripts, Overlays)   |
|   `-- Mods/                       --> ALLOWED (Virtual Mod Directories)                          |
|                                                                                                  |
|   STRICTLY FORBIDDEN TARGET ZONES:                                                               |
|   - System Root Directories: C:\Windows, C:\Program Files (outside designated game folder)        |
|   - Anti-Cheat & Protected Binaries: EasyAntiCheat*, BattlEye*, Game Executable (SkyrimSE.exe)   |
|   - User Profile Directories: C:\Users\<User>\AppData, Documents, Desktop (except mod configs)   |
|   - Network & UNC Paths: \\server\share, //192.168.1.1/...                                       |
+--------------------------------------------------------------------------------------------------+
```

---

## 3. Path Normalization & Injection Defense Rules

All proposed destination paths MUST undergo strict deterministic sanitization:

1. **POSIX Normalization:** All directory delimiters MUST be forward slashes (`/`). Windows backslashes (`\`) are rejected.
2. **Path Traversal Blocking:** Any path containing `..` (parent traversal) or `.` (current directory token) triggers `ERR_TPC_TRAVERSAL_DETECTED`.
3. **Drive Letter & Root Slash Blocking:** Paths must NEVER begin with a drive letter (`C:`) or root slash (`/`). All destinations must be strictly relative to the authorized target directory.
4. **Forbidden Character Rejection:** Characters `:`, `*`, `?`, `"`, `<`, `>`, `|`, and control characters (`0x00`-`0x1F`) cause immediate planning rejection.
5. **Junction & Symlink Escape Defense:** Before planning operations, any directory junction or NTFS reparse point within the target hierarchy must be verified to ensure its target remains wholly within the authorized target boundary.

---

## 4. Policy Diagnostic Codes

| Code | Severity | Description | Action |
| :--- | :--- | :--- | :--- |
| `ERR_TPC_TRAVERSAL_DETECTED` | FATAL | Path contains `..` or illegal relative tokens | Abort planning. Security alert logged. |
| `ERR_TPC_FORBIDDEN_ZONE_TARGET` | FATAL | Path attempts to target system/anti-cheat files | Abort planning. High-severity alert. |
| `ERR_TPC_DRIVE_LETTER_INJECTED` | FATAL | Path contains absolute Windows drive prefix | Abort planning. Reject mapping rule. |
| `ERR_TPC_SYMLINK_ESCAPE` | FATAL | Destination traverses symlink out of bounds | Abort planning. Quarantine target path. |
