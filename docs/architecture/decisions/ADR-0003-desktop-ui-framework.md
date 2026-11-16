# ADR-0003: Desktop Presentation UI Framework Selection
## Document ID: NLE-ADR-0003
**Status:** Accepted  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Software Architect & UX Systems Analyst  

---

### 1. Context and Problem
NexLudic Engine requires a native Windows desktop user interface to support game library browsing, intake questionnaires, blueprint reviews, live agent log streaming, side-by-side diff viewers, and multi-tier verification dashboards. The UI must remain responsive under heavy background compilation tasks and consume minimal system resources.

### 2. Requirements Addressed
- `REQ-UI-001`: Modern Desktop Interface.
- `REQ-UI-002`: Visual Branding & Logo Integration.
- `REQ-UI-003`: Real-Time Agent Execution Feed.
- `REQ-UI-004`: Emergency Stop Visual Indicator.

### 3. Constraints and Assumptions
- The development host has `Microsoft.WindowsDesktop.App` (WPF runtime) installed.
- The GPU on the dev workstation is an entry-level NVIDIA Quadro K420 with 2 GB VRAM; heavy Chromium-based frameworks contend excessively for RAM and VRAM.
- Rust is absent, precluding Tauri without new toolchain installations.

### 4. Options Considered
- **Option 1: Windows Presentation Foundation (WPF) with .NET 8.**
- **Option 2: WinUI 3 / Windows App SDK.** Fragile runtime packaging; higher memory use.
- **Option 3: Electron.** 300MB+ RAM footprint; heavy CPU overhead.
- **Option 4: Tauri v2.** Requires Rust toolchain which is absent on the host.

### 5. Evaluation Criteria
- Memory and GPU overhead during active gaming.
- Native Windows 11 styling capabilities.
- Live streaming log virtualization performance.
- Availability on the development workstation.

### 6. Evidence and Source Links
- Step 3 Audit (`DEVELOPMENT_ENVIRONMENT_AUDIT.md`) verifying `Microsoft.WindowsDesktop.App 8.0.31`.
- Step 5 Technology Evaluation (`TECHNOLOGY_EVALUATION.md`).

### 7. Decision
NexLudic Engine adopts **WPF (Windows Presentation Foundation) under .NET 8** utilizing modern MVVM (`CommunityToolkit.Mvvm`) and Fluent design styling as the desktop presentation framework.

### 8. Rationale
WPF is pre-installed, rock-solid, hardware-accelerated via DirectX, and consumes under 70 MB of RAM. Its mature data-binding engine handles virtualized list rendering (e.g. streaming hundreds of compiler log lines per second) without locking the UI thread.

### 9. Advantages
- Zero third-party runtime bloat.
- Instant sub-second application startup.
- Full XAML styling control and high-DPI scaling for branding assets.

### 10. Disadvantages and Risks
- Strictly Windows-only presentation layer.

### 11. Consequences for Other Components
The presentation layer will be strictly decoupled from core business logic using the MVVM pattern.

### 12. Dependencies
- .NET 8.0 Windows Desktop SDK.

### 13. Conditions That Would Invalidate the Decision
A strategic requirement to support Linux/macOS desktops prior to Phase 11.

### 14. Follow-Up Work
Author WPF application shell and MVVM viewmodels in Phase 4.

### 15. Owner Approval Required
Approved via architectural consensus (`DEC-008`).

