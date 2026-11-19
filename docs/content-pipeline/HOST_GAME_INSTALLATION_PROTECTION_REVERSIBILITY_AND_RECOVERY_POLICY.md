# Host-Game Installation Protection, Reversibility & Recovery Policy

**Document Identifier:** `NLE-POL-GIP-001`  
**Classification:** Operational Safety & Reliability Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`, `NLE-CON-CPS-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Principles of Non-Destructive Operation

Under no circumstances may NexLudic Engine render a host game installation unbootable, unrepairable, or corrupt existing user modifications. Installation modifications must follow atomic filesystem transaction semantics with bit-for-bit rollback capability.

---

## 2. Pre-Modification Baseline Snapshot (`NLE-SNAP`)

Before a single file write occurs in Stage N, the engine MUST construct a cryptographic baseline snapshot of the target directory scope:
1. **Target Boundary Scan:** Recursive scan of the designated mod directory (e.g., `<InstallRoot>/Data` or `<InstallRoot>/Mods`).
2. **File Fingerprint Catalog:** Record path, size, modification timestamp, read-only attributes, and SHA-256 hash for every existing file.
3. **Collision Detection:** Cross-reference planned package destination paths against the fingerprint catalog.
   - If an existing file was created by an earlier NexLudic package, it may be updated according to package lifecycle rules.
   - If an existing file belongs to the base game or a third-party user mod, it is marked **`COLLISION_USER_OWNED`**.

---

## 3. Transaction Safety & Staging Protocol

```
[Phase 1: Local Staging]
   Compile all transcoded assets into a private staging cache:
   `%LOCALAPPDATA%/NexLudic/staging/<run_id>/`

[Phase 2: Pre-Commit Baseline & Journaling]
   Write atomic journal file: `.nexludic_journal.json`
   Backup colliding files to `%LOCALAPPDATA%/NexLudic/backups/<instance_id>/<snapshot_id>/`

[Phase 3: Atomic Staged Deployment]
   Copy/link staged files to target directory.
   Update journal status to `COMMITTED`.

[Phase 4: Post-Verification]
   Verify hashes of deployed files.
   If check fails: TRIGGER AUTOMATIC ROLLBACK.
```

---

## 4. Preservation of User-Owned Modifications

To honor Invariant `INV-GMS-03`:
1. **Dedicated Namespacing:** NexLudic packages must write to dedicated subfolders (e.g., `Data/Meshes/NexLudic/`) or use distinct file naming prefixes whenever the host engine permits.
2. **Explicit Collision Prompt:** If a package must replace a non-NexLudic file (e.g., an engine configuration file), the user MUST be prompted with an interactive side-by-side diff.
3. **Preservation of Untracked Files:** During uninstallation or rollback, files not tracked in the package journal must NEVER be deleted.

---

## 5. Rollback Contract & Recovery Procedures

The rollback engine guarantees three recovery modes:

| Recovery Mode | Trigger Condition | Execution Mechanism | Expected Duration |
| :--- | :--- | :--- | :--- |
| **`ROLLBACK_TRANSACTION`** | Stage N failure, power loss, or crash during write. | Replay journal in reverse: delete created files, restore backed-up files. | $< 10$ seconds |
| **`ROLLBACK_VERIFICATION`** | Stage O post-application verification failure. | Cleanly unregister package, restore baseline snapshot. | $< 30$ seconds |
| **`USER_DISASTER_RECOVERY`** | Game fails to boot or behaves erratically. | Complete reset to pre-NexLudic snapshot (`snap-baseline-001`). | $< 2$ minutes |

If rollback encounters missing backup archives or I/O access denial, the instance state transitions to **`CORRUPTED_MANUAL_REPAIR_REQUIRED`**, generating a diagnostic report directing the user to the platform's repair feature (e.g., Steam "Verify Integrity of Game Files").
