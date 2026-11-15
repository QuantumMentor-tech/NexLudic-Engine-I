# NEXLUDIC ENGINE — CONFLICTS & OPEN QUESTIONS REGISTER
## Document ID: NLE-DOC-ISSUE-001
**Phase:** 1 — Product Definition & Research  
**Step:** 1 of 100 — Universal Product Source Audit, Evidence Verification, and Requirements Reconciliation  
**Date:** November 15, 2026  
**Auditor / Architect:** Senior Systems Architect & Requirements Analyst  

---

## 1. Overview & Issue Lifecycle

This document records architectural conflicts, missing empirical evidence, technical uncertainties, and unresolved decisions identified during Step 1. Every issue is tracked with a stable identifier (`ISSUE-###`), its impact on subsequent phases, and a recommended resolution path.

---

## 2. Issues & Conflicts Analysis

### ISSUE-001 (Issue A): Universal Engine Scope vs. Original Two-Game Scope
- **Category:** Scope & Product Definition
- **Description:** Earlier specification documents (GameWeaver QPIS and Spider-Craft AI QPIS) describe a bespoke, hardcoded bridge between Minecraft Java Edition and Marvel's Spider-Man Remastered. The confirmed mandate from the project owner defines NexLudic Engine as a universal platform capable of creating mashups across arbitrary PC game pairings.
- **Supporting Evidence:**
  - GameWeaver QPIS §1–§5: References Minecraft Fabric mod and Spider-Man process specifically.
  - Project Owner Mandate §2: "The platform must not be hardcoded to Minecraft, Spider-Man... Those games are examples used to explore the product concept, not the permanent limits of the platform."
- **Impact on Architecture & Roadmap:** The core engine architecture must not assume JVM, Fabric, or Spider-Man memory models. All game-specific logic must be isolated in pluggable adapter manifests and code generators.
- **Current Status:** Reconciled at Architecture Level; Awaiting Pluggable SDK Definition.
- **Information Still Needed:** Formal definition of the minimum adapter interface required to support non-JVM, non-PC-exclusive games.
- **Recommended Resolution Path:** Treat Minecraft and Spider-Man as an initial experimental reference test case for adapter testing in Phase 3/4, while designing the engine core as a game-agnostic orchestrator.
- **Relevant Later Step/Phase:** Phase 2 (Architecture & Adapter Contract) and Phase 3 (Adapter Implementation).

---

### ISSUE-002 (Issue B): Definition of "Engine" — Runtime Engine vs. Orchestration Platform
- **Category:** Product Identity & Core Functionality
- **Description:** The name "NexLudic Engine" could be misinterpreted as a new from-scratch 3D game rendering engine (similar to Unreal Engine or Unity). In reality, NexLudic is a **meta-engine and orchestration platform** that scaffolds, coordinates, bridges, and validates game mashups built on existing commercial game runtimes.
- **Supporting Evidence:**
  - Owner Mandate §10 Issue B: "Distinguish between: A general-purpose 3D game engine, Game interoperability middleware, A game-mashup development platform, A tool that orchestrates AI coding agents..."
- **Impact on Architecture & Roadmap:** Eliminates the need to develop proprietary graphics rendering, audio mixing, or native 3D physics engines from scratch. Platform development focuses on project orchestration, AI agent management, IPC bridging, and mod scaffolding.
- **Current Status:** Confirmed by Project Owner.
- **Information Still Needed:** Final branding copy to clarify this distinction for end users in the desktop UI.
- **Recommended Resolution Path:** Explicitly document the product category as "Universal AI-Assisted Game Mashup & Interoperability Platform" across all UI screens, documentation, and metadata.
- **Relevant Later Step/Phase:** Step 2 (Product Taxonomy) and Phase 4 (UI Design).

---

### ISSUE-003 (Issue C): Host-Game Strategy Verification & Missing Source Repositories
- **Category:** Technical Provenance & Reproducibility
- **Description:** The YouTube transcript (Source C) describes successful mashups (Minecraft + RDR2, Minecraft + BO3, Rocket League + Cyberpunk) created with Claude Code and Melty. However, the exact prompt texts, repository commits, and generated source files are not physically present in the workspace or referenced by immutable repository URLs.
- **Supporting Evidence:**
  - Source C text summary: High-level narration of video gameplay.
  - Workspace Audit: No source code or local text transcripts found.
- **Impact on Architecture & Roadmap:** The platform cannot rely on undisclosed proprietary scripts or magic prompts from that video. It must engineer its own verifiable prompt templates, clarification workflows, and mod generators.
- **Current Status:** Unverified External Demonstration; Replaced by Engine Specification.
- **Information Still Needed:** Empirical testing of prompt scaffolds with open coding agents (e.g. Claude Code, Gemini CLI, Aider) against actual mod loaders.
- **Recommended Resolution Path:** Develop self-contained, open-source mod project templates (starting with Fabric Loom for Java host mods and CET for Lua host mods) that don't depend on proprietary external tooling.
- **Relevant Later Step/Phase:** Phase 5 (Mod Generation & Prompt Engineering).

---

### ISSUE-004 (Issue D): Live Cross-Process Physics & World Synchronization Feasibility
- **Category:** Technical Feasibility & Safety
- **Description:** Spider-Craft AI QPIS (Source B) asserts that real-time 3D voxel terrain can be extracted from Minecraft and injected as dynamic collision geometry into Spider-Man's physics simulation at 60 fps with sub-16 ms latency.
- **Supporting Evidence:**
  - Spider-Craft AI QPIS §4: "Voxel terrain extraction and real-time mesh injection into Marvel's Spider-Man memory space."
  - Research Audit: Marvel's Spider-Man Remastered has no documented public API or dynamic collision injection interface. Injecting collision meshes into proprietary closed-source game engines in real-time requires reverse-engineered memory pointers, hooking internal Havok/proprietary physics threads, and bypasses memory integrity protections.
- **Impact on Architecture & Roadmap:** Presenting this as a core verified feature is dangerous and misleading. It risks severe game crashes, corrupted saves, or anti-cheat triggers.
- **Current Status:** Classified as *Experimental & High-Risk*; Excluded from Core MVP.
- **Information Still Needed:** Empirical reverse-engineering analysis of whether Spider-Man or other closed titles can accept dynamic mesh updates without unstable memory injection.
- **Recommended Resolution Path:** Defer cross-process 3D mesh injection to Phase 12 (Experimental R&D). For standard live bridge operations, restrict interactions to supported events, state flags, and user-mode input dispatch.
- **Relevant Later Step/Phase:** Phase 6 (Live Bridge) and Phase 12 (Advanced Experimental Features).

---

### ISSUE-005 (Issue E): Desktop Platform Technology Architecture (.NET vs. Node/Web vs. Native)
- **Category:** Core Technology Selection
- **Description:** The workspace currently has .NET 8.0 SDK (LTS), Node.js v26.8, and Python 3.14 installed, but no Java or Gradle. Earlier drafts suggested C# / .NET for the desktop app and Java/Fabric for mods. The platform architecture must decide whether the desktop GUI is built with .NET (WPF/Avalonia), web technologies (Electron/Tauri), or another stack.
- **Supporting Evidence:**
  - Host Audit: .NET 8.0.425 and Node.js v26.8.2 both operational.
  - Previous workspace in user profile (`Nano Router SLM`) utilized Tauri (`src-tauri`).
- **Impact on Architecture & Roadmap:** Dictates UI framework, IPC serialization libraries, packaging pipelines, and developer prerequisites.
- **Current Status:** Open Decision for Phase 2.
- **Information Still Needed:** Formal evaluation of cross-platform desktop UI options (.NET Avalonia vs. Tauri vs. Electron) balancing native Windows IPC performance against modern UI agility.
- **Recommended Resolution Path:** Perform a structured architectural trade-off analysis in Step 3 / Phase 2 before committing to a UI framework.
- **Relevant Later Step/Phase:** Phase 2 (System Architecture & Technology Stack Selection).

---

### ISSUE-006 (Issue F): Coding Agent Integration & Sandboxing Protocol
- **Category:** AI Agent Orchestration & Security
- **Description:** The engine must orchestrate AI coding agents (such as Claude Code, Gemini CLI, or local LLMs) to generate and edit files. The method of invocation (direct process spawning, Model Context Protocol [MCP], or HTTP API integration) and the sandboxing mechanism must be established.
- **Supporting Evidence:**
  - Owner Mandate §2.4, §12: Coding agents must work within a controlled project workspace and cannot modify system files or parent directories.
- **Impact on Architecture & Roadmap:** Without strict process sandboxing, an autonomous agent could execute destructive shell commands, delete system directories, or leak user credentials.
- **Current Status:** Open Architecture Requirement (`REQ-AI-002`, `REQ-SAFE-003`).
- **Information Still Needed:** Capabilities of target CLI coding agents regarding working-directory confinement and tool whitelisting.
- **Recommended Resolution Path:** Implement an execution sandbox that redirects all file writes to a dedicated `<project>/staging` workspace, capturing diffs and requiring explicit user approval before applying changes to live game or project folders.
- **Relevant Later Step/Phase:** Phase 5 (AI Agent Orchestration & Safety Sandboxing).

---

### ISSUE-007 (Issue G): Reusable Adapter Contract Abstraction Limits
- **Category:** Interface Design & Extensibility
- **Description:** Game engines differ fundamentally across genres (e.g. an FPS with hitscan weapons vs. an open-world RPG with quests vs. a sports game with physics balls). Defining a "universal" adapter interface risks either being an empty abstraction (lowest common denominator) or overly tailored to one genre.
- **Supporting Evidence:**
  - Owner Mandate §2.5: "Adding a new game adapter should not automatically require rewriting the entire platform. However, do not assume that every game can be supported through one universal plugin implementation."
- **Impact on Architecture & Roadmap:** A monolithic interface (`IGameAdapter`) with rigid methods (`GetPlayerWeapon()`, `GetVoxelGrid()`) will fail on non-voxel or non-combat games like Rocket League.
- **Current Status:** Under Review.
- **Information Still Needed:** Capability taxonomy categorizing games into feature traits (e.g. `ITelemetrySource`, `IInputTarget`, `IModScaffoldable`, `IAssetExtractable`).
- **Recommended Resolution Path:** Design a capability-based composition model where adapters declare and implement optional capability interfaces rather than a monolithic contract.
- **Relevant Later Step/Phase:** Phase 2 (Adapter Contract Design).

---

### ISSUE-008 (Issue H): Latency Targets & Resource Claims vs. Empirical Measurement
- **Category:** Performance & Engineering Rigor
- **Description:** Historical specifications claimed sub-16 ms end-to-end latency for live synchronization, memory limits below 150 MB, and zero dropped frames. These figures were speculative targets, not measured benchmarks.
- **Supporting Evidence:**
  - Source A & B: Unverified performance claims.
  - Owner Mandate §10 Issue H: "Separate source-stated targets from actual measurements. Do not treat sub-16 ms latency... as proven universal requirements."
- **Impact on Architecture & Roadmap:** Unrealistic performance commitments could lead to over-engineering (e.g. writing custom kernel drivers) where standard user-mode IPC is completely sufficient for gameplay events.
- **Current Status:** Reclassified as *Design Goals*, Pending Empirical Benchmark.
- **Information Still Needed:** Real-world latency measurements of named pipes and loopback WebSockets under dual-game GPU load on Windows.
- **Recommended Resolution Path:** Establish an automated IPC benchmark harness in Phase 6 to measure empirical latency on test machines and set calibrated, evidence-backed SLAs.
- **Relevant Later Step/Phase:** Phase 6 (Live Bridge & Latency Profiling).

---

### ISSUE-009 (Issue I): EULA, Anti-Cheat, and Copyright Compliance
- **Category:** Legal, Security & Safety
- **Description:** Commercial PC games enforce End User License Agreements (EULAs), Terms of Service, anti-cheat drivers, and copyright protections. Redistributing game assets or hooking multiplayer processes creates legal exposure and account ban risks for users.
- **Supporting Evidence:**
  - Owner Mandate §12: Explicit prohibitions against DRM bypassing, kernel hooking, and asset redistribution.
- **Impact on Architecture & Roadmap:** NexLudic Engine must be strictly defensive, single-player/offline-focused, non-distributive, and transparent about anti-cheat risks.
- **Current Status:** Architectural Policy Confirmed (`REQ-SAFE-001`, `REQ-MOD-003`).
- **Information Still Needed:** Legal guidelines regarding local user-directed asset format conversion for personal modding use.
- **Recommended Resolution Path:** Embed automated checks that detect online multiplayer modes or anti-cheat presence and refuse to attach live bridges to protected processes. Restrict all distribution bundles to open code, configuration diffs, and local user extraction scripts.
- **Relevant Later Step/Phase:** Phase 1 (Safety Policy) and Phase 11 (Security Audit & Compliance).

---

### ISSUE-010 (Issue J): Roadmap Cadence & Step Boundaries
- **Category:** Project Management & Governance
- **Description:** The project follows a rigorous 100-step roadmap across 12 phases. There is a risk of scope creep where an agent attempts to implement future phase features (such as building UI or mod templates) prematurely during baseline research steps.
- **Supporting Evidence:**
  - Owner Mandate §17: Strict stop boundary after Step 1.
- **Impact on Architecture & Roadmap:** Every step must maintain crisp, audited completion gates and never bleed into subsequent steps.
- **Current Status:** Confirmed Governance Policy.
- **Information Still Needed:** Approved detailed step breakdown for Phase 1 and Phase 2.
- **Recommended Resolution Path:** Enforce acceptance criteria (`AC-01` through `AC-10`) at the conclusion of every step with explicit verification reports before proceeding.
- **Relevant Later Step/Phase:** All Phases (Steps 1 through 100).

