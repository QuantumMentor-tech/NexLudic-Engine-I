# ADR-0001: Initial Platform and Operating-System Support Boundary
## Document ID: NLE-ADR-0001
**Status:** Accepted  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Software Architect & Project Owner  

---

### 1. Context and Problem
PC games and their associated modding ecosystems (Steam, DirectX, mod loaders, process hooks) operate overwhelmingly on the Microsoft Windows platform. NexLudic Engine must establish an explicit operating-system boundary for its initial development and release milestones without making unverified cross-platform compatibility promises that cannot be sustained on the host environment.

### 2. Requirements Addressed
- `REQ-CORE-001`: Arbitrary Game-Pair Selection on PC.
- `REQ-SAFE-003`: Non-administrative user execution.
- `AC-05`: Platform Support Boundaries.

### 3. Constraints and Assumptions
- The development workstation is a physical Windows 11 Pro 64-bit desktop (Build 26100).
- Windows represents >96% of commercial PC modding ecosystems.
- Linux and macOS enforce fundamentally different process namespaces, security entitlements, and graphics APIs.

### 4. Options Considered
- **Option 1: Windows 10/11 (x64) Exclusively for Initial Release.**
- **Option 2: Multi-Platform Day 1 (Windows, Linux, macOS).** Requires abstracting window managers, Proton/Wine namespaces, and macOS security notarization.

### 5. Evaluation Criteria
- Ecosystem compatibility with target games.
- Toolchain availability on the development host.
- Architectural complexity and testing overhead.

### 6. Evidence and Source Links
- Step 3 Environment Audit (`DEVELOPMENT_ENVIRONMENT_AUDIT.md`) confirming Windows 11 host.
- Industry gaming hardware and OS survey data confirming Windows x64 dominance in modding.

### 7. Decision
NexLudic Engine adopts **Microsoft Windows 10 and 11 (64-bit, x64 architecture)** as the exclusive operating system target for Phase 1 through Phase 8 (MVP release). Cross-platform expansion (Linux/Proton) is formally deferred to a post-v2.0 milestone.

### 8. Rationale
Focusing on Windows enables deep, native optimization for Win32 window handles, Windows Named Pipes, and `SendInput` APIs without introducing heavy cross-platform abstraction layers.

### 9. Advantages
- Maximum performance and zero runtime translation overhead.
- Direct alignment with commercial game mod loaders (Fabric, CET, BakkesMod, ScriptHook).
- Immediate testability on the developer workstation.

### 10. Disadvantages and Risks
- Excludes native Linux (Steam Deck / SteamOS) and macOS users from initial releases.

### 11. Consequences for Other Components
All core IPC, window discovery, and input injection modules can safely utilize standard Win32 and .NET Windows Desktop APIs.

### 12. Dependencies
- 64-bit Windows 10 (21H2+) or Windows 11 (21H2+).

### 13. Conditions That Would Invalidate the Decision
Formal owner directive mandating SteamOS / Linux compatibility as an MVP launch requirement.

### 14. Follow-Up Work
Re-evaluate Linux/Proton compatibility in Phase 11.

### 15. Owner Approval Required
Approved by Owner Mandate (Step 1).

