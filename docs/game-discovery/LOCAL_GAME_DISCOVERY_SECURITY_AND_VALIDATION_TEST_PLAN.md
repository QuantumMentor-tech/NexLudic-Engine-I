# Local Game Discovery Security & Validation Test Plan

**Document ID:** `NLE-PLN-LDS-001`  
**Version:** `1.0.0`  
**Status:** `APPROVED`  
**Workstream:** `Local Environment Discovery, Installation Identification, Detection Authorization, Version Verification, Privacy, and Recovery`  
**Phase:** `4 of 12 — Game Discovery & Compatibility Intelligence`  
**Step:** `27 of 100`  
**Related Documents:**
- [LOCAL_GAME_INSTALLATION_DISCOVERY_AND_DETECTION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_GAME_INSTALLATION_DISCOVERY_AND_DETECTION_SPECIFICATION.md) (`NLE-SPC-LID-001`)
- [LOCAL_GAME_DETECTION_SOURCE_CATALOG_AND_EVALUATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_GAME_DETECTION_SOURCE_CATALOG_AND_EVALUATION.md) (`NLE-CAT-LDS-001`)
- [LOCAL_INSTALLATION_IDENTITY_AND_VERIFICATION_STATE_MODEL.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_INSTALLATION_IDENTITY_AND_VERIFICATION_STATE_MODEL.md) (`NLE-DOM-INS-001`)
- [LOCAL_GAME_DISCOVERY_AUTHORIZATION_AND_PRIVACY_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_GAME_DISCOVERY_AUTHORIZATION_AND_PRIVACY_SPECIFICATION.md) (`NLE-SPC-LPR-001`)
- [LOCAL_GAME_DETECTION_VERSION_AND_IDENTITY_MATCHING_RULES.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_GAME_DETECTION_VERSION_AND_IDENTITY_MATCHING_RULES.md) (`NLE-RUL-LVM-001`)
- [LOCAL_GAME_DISCOVERY_FAILURE_AND_RECOVERY_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/game-discovery/LOCAL_GAME_DISCOVERY_FAILURE_AND_RECOVERY_SPECIFICATION.md) (`NLE-SPC-LDF-001`)

---

## 1. Test Strategy & Architectural Mandates

This test plan defines the comprehensive verification suite for the local game discovery and installation detection subsystems of **NexLudic Engine**.

Testing local discovery requires extreme care: tests must execute safely on developer machines and automated CI runners without requiring actual commercial game installations, without scanning user host disks, without triggering antivirus alerts, and without attempting administrative elevation.

### 1.1 Synthetic Fixture Architecture
All discovery tests operate against **mock and synthetic test fixtures**:
1. **Virtual Filesystem Mocking:** Directory trees, launcher manifests (`libraryfolders.vdf`, `appmanifest.acf`, `.item`), and executable binaries are synthesized in ephemeral test directories or mocked via filesystem abstraction interfaces.
2. **Synthetic Minimal PE Binaries:** Pre-compiled minimal $2\text{ KB}$ binary stubs containing valid `IMAGE_DOS_HEADER`, `IMAGE_NT_HEADERS`, and `RT_VERSION` resources (both 32-bit and 64-bit) are used to test PE parsing without running any executable code.
3. **Registry Virtualization:** Registry queries are executed against mocked registry providers or temporary sandboxed keys under `HKCU\Software\NexLudicTest\`.

---

## 2. Test Suite Specification (33 Design-Level Test Cases)

### 2.1 Group 1: User Authorization & Bounded Scopes (TLDS-AUT)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-AUT-001` | Explicit File Picker Selection | Verify detection from user-selected directory. | User selects `D:\MockGames\SkyrimSE`. | Scan inspects only that folder; zero traversal outside. | `CANDIDATE_FOUND` emitted; root isolated. |
| `TLDS-AUT-002` | Whole-Drive Scan Rejection | Verify rejection of root drive paths. | Input path: `C:\` or `D:\`. | Immediate rejection with `ERR_UNBOUNDED_SCAN_ROOT`. | No directory iteration initiated. |
| `TLDS-AUT-003` | System Path Blacklist Enforcement | Verify blacklisting of protected OS folders. | Input path: `C:\Windows\System32`. | Immediate block with `ERR_PROTECTED_SYSTEM_PATH`. | Scanner refuses traversal. |
| `TLDS-AUT-004` | Granular Launcher Consent | Verify launcher scan requires affirmative opt-in. | Steam toggle unchecked in settings. | Scanner skips Steam registry and manifest checks. | Zero queries dispatched to Steam registry. |

### 2.2 Group 2: Detection Sources & Manifest Parsing (TLDS-SRC)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-SRC-001` | Steam Library VDF Multi-Drive | Verify parsing of multi-library `libraryfolders.vdf`. | Synthetic VDF defining libraries on `C:\` and `D:\`. | Correctly parses all library roots and game manifests. | All 3 declared games identified. |
| `TLDS-SRC-002` | Steam ACF Parsing State Flags | Verify handling of incomplete downloads. | Manifest with `StateFlags = 1026` (Updating). | Identifies installation but marks state incomplete. | Flagged as downloading; non-blocking. |
| `TLDS-SRC-003` | GOG Registry & goglog.ini | Verify GOG installation detection. | Mock GOG registry key + `goglog.ini` on disk. | Extracts GOG GameID, title, path, and version string. | High confidence match ($C \ge 0.85$). |
| `TLDS-SRC-004` | Epic Games JSON Manifest | Verify Epic `.item` manifest parsing. | Synthetic `.item` JSON file in mock app data. | Parses `AppName`, `InstallLocation`, and `AppVersionString`. | Correctly maps to Epic release. |
| `TLDS-SRC-005` | Corrupted Manifest Recovery | Verify parser handles syntax corruption safely. | Synthetic truncated/malformed JSON `.item` file. | Logs `ERR_CORRUPT_MANIFEST`; skips bad file gracefully. | Scanner does not crash; other items parsed. |

### 2.3 Group 3: PE Header & VersionInfo Inspection (TLDS-PEH)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-PEH-001` | 64-Bit x64 PE Header Parsing | Verify extraction of x64 architecture. | Synthetic PE32+ stub with `Machine = 0x8664`. | Reports `architecture = X86_64`. | Architecture identified accurately. |
| `TLDS-PEH-002` | 32-Bit x86 PE Header Parsing | Verify extraction of x86 architecture. | Synthetic PE32 stub with `Machine = 0x014c`. | Reports `architecture = X86_32`. | Architecture identified accurately. |
| `TLDS-PEH-003` | VS_FIXEDFILEINFO Version Extraction | Verify 4-part numeric version extraction. | Stub with `dwFileVersionMS = 0x00010006`, `LS = 0x04920000`. | Extracts `"1.6.1170.0"`. | Exact version string matches. |
| `TLDS-PEH-004` | Truncated / Corrupt PE Binary | Verify non-crashing handling of invalid PE. | $50\text{ byte}$ random binary file named `Game.exe`. | Returns `ERR_CORRUPT_PE`; transitions to failed state. | Graceful error encapsulation. |

### 2.4 Group 4: Identity & Version Matching Rules (TLDS-MAT)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-MAT-001` | Conclusive Multi-Indicator Match | Verify automatic match above threshold. | Steam AppID ($0.90$) + Exe name ($0.80$) match. | Composite score $C \ge 0.85 \rightarrow$ `IDENTITY_MATCHED`. | Automatic link to canonical record. |
| `TLDS-MAT-002` | Generic Exe Name Low Confidence | Verify generic `game.exe` avoids false positive. | Folder `CustomMod` contains `game.exe` without manifest. | Score $C = 0.10 \rightarrow$ remains `CANDIDATE_FOUND`. | No false positive link established. |
| `TLDS-MAT-003` | Sequels Disambiguation (Skyrim vs SE) | Verify architecture separates distinct editions. | Target has 32-bit `TESV.exe` vs 64-bit `SkyrimSE.exe`. | Maps to distinct `CanonicalGameId` records. | Zero conflation between original and SE. |
| `TLDS-MAT-004` | Non-SemVer Version Preservation | Verify raw custom build strings are preserved. | Binary reports `"Build 2023-11-04 (Hotfix 3)"`. | Preserves raw string in `raw_version_string`. | Verbatim string retained without truncation. |
| `TLDS-MAT-005` | Cryptographic SHA-256 Verification | Verify SHA-256 match transitions to verified. | Synthetic binary matching catalog signature digest. | State transitions to `VERSION_VERIFIED` ($C = 1.00$). | Fully verified status awarded. |

### 2.5 Group 5: Multiple Installations & Edge Cases (TLDS-MUL)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-MUL-001` | Concurrent Steam and GOG Installs | Verify coexistence of duplicate game copies. | User has both Steam and GOG copies on disk. | Creates two distinct `LocalInstallationInstance` records. | Neither record overwrites the other. |
| `TLDS-MUL-002` | Multi-Game Bundle / Anthology | Verify distinct games in subfolders of one root. | Root directory containing `Game1\app.exe` and `Game2\app.exe`. | Identifies two independent installations. | Each binds to its respective canonical ID. |
| `TLDS-MUL-003` | Ambiguity User Confirmation | Verify ambiguous matches require manual prompt. | Two games match equal weight ($C = 0.70$). | Sets `IDENTITY_AMBIGUOUS`; queues user choice. | Requires explicit user selection in UI. |

### 2.6 Group 6: Failure, Timeout & Cancellation (TLDS-FLR)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-FLR-001` | Offline External Drive Disconnection | Verify missing drive sets `UNAVAILABLE`. | Target path points to disconnected drive `Z:\`. | State transitions to `UNAVAILABLE`; bindings kept. | Zero cascade deletion of project data. |
| `TLDS-FLR-002` | Drive Reconnection Recovery | Verify reattaching drive restores verified state. | Mock drive path becomes accessible again. | Revalidation transitions `UNAVAILABLE` $\rightarrow$ `VERSION_VERIFIED`. | Status restored cleanly. |
| `TLDS-FLR-003` | Patch Staleness Detection | Verify modified timestamp flags `STALE_CHANGED`. | Executable timestamp modified on disk. | State transitions to `STALE_CHANGED`. | Prompts user to revalidate. |
| `TLDS-FLR-004` | Query Timeout Enforcement | Verify individual query aborts after $3000\text{ ms}$. | Mock network/hung share query sleeps for $5000\text{ ms}$. | Aborts at $3000\text{ ms}$ with `ERR_SCAN_TIMEOUT`. | UI thread does not stall. |
| `TLDS-FLR-005` | User Cancellation Token Handling | Verify immediate response to user cancellation. | User clicks Cancel mid-scan. | Scanner terminates in $< 200\text{ ms}$; resources freed. | Zero orphaned file locks. |

### 2.7 Group 7: Security, Privilege & Medium IL (TLDS-SEC)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-SEC-001` | Medium Integrity Token Assertion | Verify scanner executes non-elevated. | Process token queried for integrity level. | Token matches `SECURITY_MANDATORY_MEDIUM_RID`. | Zero elevation tokens detected. |
| `TLDS-SEC-002` | Access Denied Non-Elevation Grace | Verify `E_ACCESSDENIED` does not prompt UAC. | Target folder has restricted ACLs (admin-only). | Catches `UnauthorizedAccessException`; logs `ACCESS_DENIED`. | No UAC elevation dialog triggered. |
| `TLDS-SEC-003` | Zero Process Execution Assertion | Verify discovery never launches processes. | Audit hooks monitor `CreateProcessW`. | Zero child process creation calls recorded. | Absolute zero process execution. |
| `TLDS-SEC-004` | Anti-Cheat Memory Hooking Ban | Verify scanner never calls `OpenProcess`. | Audit hooks monitor memory/process APIs. | Zero calls to `OpenProcess` or `VirtualAllocEx`. | Zero anti-cheat trigger vectors. |

### 2.8 Group 8: Privacy & Local Containment (TLDS-PRV)

| Test ID | Test Title | Objective | Input / Precondition | Expected Result | Pass Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `TLDS-PRV-001` | Host Path Network Egress Ban | Verify host paths never leave local machine. | Network traffic monitor attached during scan. | Zero outbound network requests contain path strings. | 100% path containment verified. |
| `TLDS-PRV-002` | Diagnostic Log Path Redaction | Verify user paths are scrubbed in logs. | Log output generated from `D:\Users\Alice\Games\...`. | Path written as `<INSTALL_ROOT>\SkyrimSE.exe`. | Username and host drive redacted. |
| `TLDS-PRV-003` | Volume D Storage Containment | Verify all caches write strictly to Volume D. | File I/O audit attached during discovery. | All file writes restricted to `D:\NexLudic\...`. | Zero writes to `C:\`, `%APPDATA%`, or `%TEMP%`. |

---

## 3. Test Traceability Matrix

| Requirement / Standard | Addressed Test Cases |
| :--- | :--- |
| `REQ-SEC-001` (Medium Integrity Level) | `TLDS-SEC-001`, `TLDS-SEC-002` |
| `REQ-SAFE-001` (Anti-Cheat & Non-Invasive)| `TLDS-SEC-003`, `TLDS-SEC-004` |
| `DEC-009` (Volume D Containment) | `TLDS-PRV-003` |
| Bounded Traversal & Authorization | `TLDS-AUT-001`, `TLDS-AUT-002`, `TLDS-AUT-003`, `TLDS-AUT-004` |
| Launcher Manifest Integrity | `TLDS-SRC-001`, `TLDS-SRC-002`, `TLDS-SRC-003`, `TLDS-SRC-004`, `TLDS-SRC-005` |
| PE Header & Build Extraction | `TLDS-PEH-001`, `TLDS-PEH-002`, `TLDS-PEH-003`, `TLDS-PEH-004` |
| Version & Identity Resolution | `TLDS-MAT-001`, `TLDS-MAT-002`, `TLDS-MAT-003`, `TLDS-MAT-004`, `TLDS-MAT-005` |
| Duplicate / Complex Environments | `TLDS-MUL-001`, `TLDS-MUL-002`, `TLDS-MUL-003` |
| Resilience, Recovery & Timeouts | `TLDS-FLR-001`, `TLDS-FLR-002`, `TLDS-FLR-003`, `TLDS-FLR-004`, `TLDS-FLR-005` |
| Privacy & Path Redaction | `TLDS-PRV-001`, `TLDS-PRV-002` |

---

## 4. Document Sign-Off & Verification

| Role | Reviewer | Verdict | Date |
| :--- | :--- | :--- | :--- |
| **Principal Security Test Engineer** | Security Assurance | Approved | 2026-11-17 |
| **Lead Quality Assurance Architect**| Test Architecture | Approved | 2026-11-17 |
| **Senior Windows Systems Engineer** | Core Platform Team | Approved | 2026-11-17 |

