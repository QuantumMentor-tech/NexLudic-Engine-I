# NEXLUDIC ENGINE — ARCHITECTURAL DECISION LOG
## Document ID: NLE-DOC-DEC-001
**Phase:** 1 — Product Definition & Research  
**Step:** 1 of 100 — Universal Product Source Audit, Evidence Verification, and Requirements Reconciliation  
**Date:** November 15, 2026  
**Auditor / Architect:** Senior Systems Architect  

---

## 1. Governance & Decision Lifecycle

The NexLudic Engine decision log tracks key architectural, technical, and procedural determinations. Decisions are categorized as:
- **Approved by Owner:** Explicitly commanded or confirmed by the project owner.
- **Architectural Consensus:** Adopted as baseline engineering policy based on verified technical evidence.
- **Proposed / Under Review:** Recommended options awaiting formal stakeholder confirmation or empirical test results.
- **Deferred:** Intentionally postponed to a designated subsequent phase.
- **Rejected:** Formally evaluated and dismissed.

---

## 2. Decision Records

### DEC-001: Universal Multi-Game Scope over Bespoke Two-Game Pairing
- **Topic:** Product Scope & Core Vision
- **Current Status:** Approved by Owner
- **Context:** Previous specification drafts (GameWeaver and Spider-Craft) focused on integrating Minecraft with Marvel's Spider-Man Remastered. The project owner confirmed that NexLudic Engine must be a reusable platform supporting diverse game combinations.
- **Evidence:** User Request §2 ("Confirmed Product Direction: The platform must not be hardcoded to Minecraft, Spider-Man... The long-term product vision includes universal game-pair selection").
- **Options Considered:**
  1. *Option A:* Build a specialized Minecraft + Spider-Man bridge, then attempt to generalize later.
  2. *Option B:* Design the core engine from Day 1 as a universal, pluggable mashup orchestration platform.
- **Consequences:** Option B was adopted. The core architecture must remain completely decoupled from any single game engine, mod loader, or binary format. All game-specific code must reside in pluggable adapters or generated mod projects.
- **Further Approval / Research Needed:** None; foundational product mandate confirmed.

---

### DEC-002: Reclassification of Historical QPIS Metrics from Hard SLAs to Design Targets
- **Topic:** Technical Benchmarks & Latency Commitments
- **Current Status:** Architectural Consensus
- **Context:** Earlier specification documents listed quantitative targets such as sub-16 ms end-to-end latency, sub-150 MB memory consumption, and zero dropped frames. No empirical benchmark test scripts or hardware test reports were provided to substantiate these numbers.
- **Evidence:** Workspace inspection confirms zero historical performance harnesses. Community research indicates inter-process message passing latency is highly dependent on Windows process scheduling and thread priority.
- **Options Considered:**
  1. *Option A:* Accept historical figures as binding contractual requirements.
  2. *Option B:* Reclassify historical figures as aspirational design goals and establish an empirical benchmark harness in Phase 6.
- **Consequences:** Option B was adopted. Eliminates premature over-engineering while establishing rigorous verification criteria before making performance guarantees.
- **Further Approval / Research Needed:** Benchmarking harness to be developed and evaluated in Phase 6.

---

### DEC-003: Rejection of Kernel-Mode Drivers for Input Emulation
- **Topic:** Input Injection & System Safety
- **Current Status:** Architectural Consensus
- **Context:** Spider-Craft AI QPIS recommended using ViGEmBus for virtual controller emulation. External technical research revealed that ViGEmBus was officially retired and archived by its author in November 2023, and requires a Windows kernel-mode driver (`.sys`) with administrative installation privileges.
- **Evidence:** Official ViGEmBus GitHub repository (`nefarius/ViGEmBus`) status and commit history.
- **Options Considered:**
  1. *Option A:* Retain ViGEmBus, requiring users to install legacy third-party kernel drivers with administrator elevation.
  2. *Option B:* Reject kernel drivers; utilize user-mode Windows input APIs (`SendInput`, virtual HID, or game-specific mod input hooks) and evaluate modern user-mode controller frameworks.
- **Consequences:** Option B was adopted. Guarantees that NexLudic Engine can run under standard user privileges without blue-screen risk, anticheat driver flags, or security vulnerabilities.
- **Further Approval / Research Needed:** Validate `SendInput` reliability and game window focus requirements across target game genres in Phase 6.

---

### DEC-004: Strategy A (Host-Game Modding) as the Default Primary Path
- **Topic:** Integration Strategy Prioritization
- **Current Status:** Architectural Consensus
- **Context:** The engine must decide how to approach new mashup requests. Comparing Strategy A (Host Modding), Strategy B (Live Bridge), and Strategy C (Hybrid) revealed that Host Modding offers the highest visual fidelity, native input handling, zero cross-process lag, and the highest probability of AI coding agent success.
- **Evidence:** Verified community demonstrations (Claude Code + Melty transcript) successfully produced working mashups via host-game modding (Minecraft mods implementing RDR2, BO3 mechanics).
- **Options Considered:**
  1. *Option A:* Force all mashups into a dual-running-game live bridge architecture (Strategy B).
  2. *Option B:* Default to Strategy A (Host Modding) when a host mod loader exists; offer Strategy B and C only when technically justified.
- **Consequences:** Option B was adopted. Minimizes hardware load (runs one game instead of two), eliminates window-focus input conflicts, and provides a polished in-engine experience.
- **Further Approval / Research Needed:** Verify compatibility of AI-generated scaffolding across Fabric (Minecraft), CET (Cyberpunk 2077), and Steam Mod Tools (BO3) in Phase 5.

---

### DEC-005: Strict AI Coding Agent Sandboxing & Workspace Isolation
- **Topic:** AI Agent Safety & Consequential Execution Gating
- **Current Status:** Architectural Consensus
- **Context:** Autonomous AI coding agents will generate files, scripts, and build commands. Unconstrained agent execution on a developer's desktop poses risks of accidental file deletion, path traversal, or executing destructive shell scripts.
- **Evidence:** Owner Mandate §2.4, §12; industry security best practices for agentic systems.
- **Options Considered:**
  1. *Option A:* Allow the coding agent direct, unconstrained access to the user's filesystem and shell.
  2. *Option B:* Confine the coding agent to a sandboxed staging directory (`<project>/staging`), capture structured diffs, and require human-in-the-loop approval before applying changes to live game or repository paths.
- **Consequences:** Option B was adopted. All agent operations are audited, tracked via diffs, and gated by explicit user consent.
- **Further Approval / Research Needed:** Design the specific staging sandbox protocol in Phase 5.

---

### DEC-006: Non-Distributive Asset Policy for Copyrighted Game Data
- **Topic:** Intellectual Property & Legal Compliance
- **Current Status:** Approved by Owner
- **Context:** Game mashups frequently utilize textures, sounds, 3D models, or icons from commercial PC games. Packaging or redistributing raw proprietary assets with NexLudic Engine or user-shared mashups would infringe game publishers' copyrights and EULAs.
- **Evidence:** Owner Mandate §2.7 ("It must not distribute proprietary game executables or unauthorized game assets").
- **Options Considered:**
  1. *Option A:* Bundle converted guest game assets directly into downloadable mashup ZIPs.
  2. *Option B:* Distribute only open-source code, scripts, and configurations; require the end user to locally extract assets from their legally owned game copies using automated local extraction scripts.
- **Consequences:** Option B was adopted. Protects the NexLudic platform and its users from intellectual property claims.
- **Further Approval / Research Needed:** Research extraction scripts for major game engines (Unreal, Unity, proprietary archives) in Phase 7.

---

### DEC-007: Preservation of Workspace State & Deferral of Git Initialization
- **Topic:** Change Control & Workspace Management
- **Current Status:** Approved by Owner (Step 1 Mandate)
- **Context:** The workspace `D:\App Winrar Files\Nexludic Engine (working)` is not currently a Git repository. Step 1 directives strictly forbid initializing Git if the workspace is not already a repository merely for this audit.
- **Evidence:** Owner Mandate §13 ("Do not initialize Git if the workspace is not a repository merely for this audit").
- **Options Considered:**
  1. *Option A:* Run `git init` automatically to track initial documentation files.
  2. *Option B:* Comply strictly with Section 13; document the non-git status, preserve all pre-existing files, and defer Git initialization until explicitly authorized.
- **Consequences:** Option B was adopted. Preserves strict audit boundaries without side-effects.
- **Further Approval / Research Needed:** Project owner authorization for repository initialization in Step 2.

