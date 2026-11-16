# ADR-0009: Workspace Isolation, Diff Tracking, and Command-Approval Model
## Document ID: NLE-ADR-0009
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Security Architect & Senior QA Automation Engineer  

---

### 1. Context and Problem
Autonomous AI coding agents generate, modify, and delete source files, and may attempt to invoke shell commands (compilers, build scripts, package managers). Allowing an agent unconstrained write permissions to the host filesystem creates catastrophic risks: accidental deletion of user documents, modifying system files, executing malicious scripts, or corrupting live game installations.

### 2. Requirements Addressed
- `REQ-AI-002`: Sandboxed Workspace Isolation.
- `REQ-AI-003`: Diff Tracking & Change Audit.
- `REQ-AI-004`: Human-in-the-Loop Consequential Gating.
- `REQ-SAFE-003`: Non-administrative Execution.
- `DEC-005`: Mandatory Staging Directory Sandbox.

### 3. Constraints and Assumptions
- AI agents must be treated as untrusted, fallible workers.
- The engine must provide clear visibility into every file mutation before changes are applied.
- The platform must run under standard Windows user permissions without requiring Hyper-V or administrator elevation.

### 4. Options Considered
- **Option 1: Staging Directory Sandbox + Windows Job Objects + Human-in-the-Loop Diff Gating.**
- **Option 2: Docker Containers / WSL2.** Requires virtualization hardware, Hyper-V, and high memory; fails on systems without Docker.
- **Option 3: Direct Live Working Tree Modification with Git Auto-Commit.** High risk of unreviewed file overwrites; cannot prevent out-of-tree file writes.

### 5. Evaluation Criteria
- Security isolation without requiring administrative privileges.
- Transparency and user review visibility (unified diffs).
- Execution control (timeouts, resource limits, kill-switches).

### 6. Evidence and Source Links
- Step 1 Mandate (§12 Security, Privacy, Licensing).
- Microsoft Win32 Job Objects API documentation.

### 7. Decision
NexLudic Engine adopts a **Three-Tier Workspace & Execution Sandbox**:
1. **Filesystem Staging Sandbox:** All AI agent operations are strictly confined to `<project_dir>/staging/`. Any attempt to write to parent directories, system folders, or live game directories is intercepted and hard-blocked by a custom `SandboxedFileSystem` wrapper.
2. **Windows Job Object Process Sandboxing:** When invoking agent CLIs or compilers, the process is assigned to a Win32 Job Object enforcing:
   - Hard execution timeouts (e.g. 5 minutes max).
   - Memory limits (e.g. 4 GB max).
   - Cascading termination (killing the parent terminates all orphaned child processes).
3. **Human-in-the-Loop Diff Review:** All staged file additions, edits, and deletions produce a unified git diff. High-impact operations (file deployment, shell executions, dependency downloads) pause execution and require explicit user click approval in the UI.

### 8. Rationale
Provides rock-solid security and user confidence without requiring complex containerization or administrator elevation. The user always sees exactly what the agent did before anything touches their live game folder.

### 9. Advantages
- Zero administrative UAC prompts required.
- Instant, risk-free experimentation; if an agent fails, wiping `/staging/` resets the workspace cleanly.
- Full auditability; every file edit has an associated diff and timestamp.

### 10. Disadvantages and Risks
- Requires maintaining an in-memory diff engine for large files.

### 11. Consequences for Other Components
The `NexLudic.Infrastructure` layer will implement `SandboxedFileSystemWatcher`, `JobObjectProcessRunner`, and `DiffGenerator`.

### 12. Dependencies
- Win32 Job Object APIs (`P/Invoke`), `DiffPlex` (or native C# unified diff generator).

### 13. Conditions That Would Invalidate the Decision
Evidence that Windows Job Objects fail to constrain rogue subprocesses on Windows 10/11.

### 14. Follow-Up Work
Implement the `SandboxedProcessRunner` and UI diff viewer in Phase 5.

### 15. Owner Approval Required
Pending Project Owner formal approval.

