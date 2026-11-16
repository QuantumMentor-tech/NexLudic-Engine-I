# ADR-0011: Packaging, Dependency Management, and Atomic Rollback Strategy
## Document ID: NLE-ADR-0011
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Build & Release Engineer & Security Architect  

---

### 1. Context and Problem
NexLudic Engine must be distributable as a lightweight desktop application, while the mashups it generates must be deployable to target games and exportable to other users. Modifying game directories carries risks of file corruption, broken game installations, and licensing violations if copyrighted game assets are bundled.

### 2. Requirements Addressed
- `REQ-PKG-001`: Automated Backup Prior to Modding.
- `REQ-PKG-002`: Atomic Rollback Engine.
- `REQ-PKG-003`: Standalone Mashup Package Export.
- `REQ-MOD-003`: Non-Distributive Asset Policy.
- `DEC-006`: Non-Distributive Asset Compliance.

### 3. Constraints and Assumptions
- Core application must run without requiring administrative installers.
- Distributable mashup bundles must never include proprietary game executables or assets.
- Users must be able to restore their game directories to an unmodded state instantly.

### 4. Options Considered
- **Option 1: Self-Contained Single-File Publish + Atomic Directory Snapshots + Non-Distributive Export.**
- **Option 2: Windows Installer (MSI/InnoSetup) with Direct Binary Replacement.** Modifies files without backups; high risk of leaving orphaned files.
- **Option 3: Bundling Game Assets into Mod ZIPs.** Direct copyright and EULA infringement.

### 5. Evaluation Criteria
- Ease of desktop installation and uninstallation.
- Guaranteed reversibility (100% hash parity on rollback).
- Strict legal and copyright compliance.

### 6. Evidence and Source Links
- Step 1 Mandate (§2.7 Packaging & Recovery; §12 Security, Privacy, Licensing).
- Step 2 Product Scope (`MVP_AND_RELEASE_BOUNDARIES.md`).

### 7. Decision
NexLudic Engine adopts a **Three-Pillar Packaging & Safety Architecture**:
1. **Core Platform Distribution:** Published as a self-contained, single-file Windows x64 executable (`NexLudicEngine.exe`) via .NET 8, requiring zero pre-installed runtimes and zero administrative UAC prompts.
2. **Atomic Pre-Modding Backup & Rollback:** Before copying any file into a target game folder, the engine creates a timestamped snapshot of the target directory in `<project>/backups/<timestamp>/` and records a manifest of file hashes. Triggering "One-Click Rollback" completely restores the baseline files and purges injected mod files within 2 seconds.
3. **Non-Distributive Standalone Mashup Export:** Exporting a mashup produces a clean distribution archive (`.zip`) containing:
   - Compiled mod files and scripts authored by the user / AI agent.
   - `mashup-manifest.json` describing dependencies and load orders.
   - A portable local asset extraction script that instructs the recipient's machine to unpack necessary sound/texture files directly from their own legally purchased game installation.
   - **Strict Rule:** Raw proprietary game binaries (`.exe`, `.dll`) and raw copyrighted asset containers (`.pak`, `.rpf`, `.awc`) are strictly excluded from the export archive.

### 8. Rationale
Guarantees user system safety and complete peace of mind through atomic reversibility, while shielding the platform and community creators from copyright infringement and EULA violations.

### 9. Advantages
- Zero-risk mod testing; one click restores the vanilla game.
- Clean, standalone desktop executable requiring no installer scripts.
- Legally compliant distribution model for community sharing.

### 10. Disadvantages and Risks
- Recipient users of exported mashups must own legal copies of both games to extract required assets.

### 11. Consequences for Other Components
The `NexLudic.Infrastructure` layer will implement `BackupRestoreEngine` and `StandalonePackageExporter`.

### 12. Dependencies
- `System.IO.Compression`, `System.Security.Cryptography.SHA256`.

### 13. Conditions That Would Invalidate the Decision
Evidence demonstrating that target game file locks prevent atomic backup creation.

### 14. Follow-Up Work
Implement the backup hash verifier and ZIP packager in Phase 7.

### 15. Owner Approval Required
Pending Project Owner formal approval.

