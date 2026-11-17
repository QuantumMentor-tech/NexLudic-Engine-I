# Universal Core and Adapter Responsibility Boundary

**Document ID:** `NLE-SPC-UAR-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 34 of 100 — Universal Adapter SDK Foundation & Contract Reconciliation Specification  
**Classification:** Architectural Boundary & System Responsibility Contract  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-SPC-SDK-001`, `NLE-DOM-SPEC-001`, `UNIVERSAL_CORE_AND_GAME_BOUNDARIES.md`, `ADR-0002`

---

## 1. Responsibility Partitioning Architecture

The core value of NexLudic Engine lies in its clean separation between **Universal Platform Intelligence** and **Game-Specific Modding Mechanisms**. 

If game-specific logic (e.g. parsing a Skyrim `.esp` file or reading a Minecraft `.nbt` chunk) bleeds into the core platform, the engine becomes brittle, difficult to maintain, and impossible to scale across hundreds of games. Conversely, if adapters attempt to manage project workspaces, evaluate compatibility rules, or bypass platform authorization, system integrity collapses.

```mermaid
flowchart LR
    subgraph UniversalCore["Universal Core Responsibilities (Engine Host)"]
        direction TB
        UC1["Canonical Game Identity (Step 25)"]
        UC2["Compatibility Rule Engine (Step 31)"]
        UC3["Assessment & Evidence Lifecycle (Step 29/30)"]
        UC4["User Authorization & Consent (CMP-045)"]
        UC5["Lifecycle Supervision & Process Launch (CMP-08)"]
        UC6["Volume D Storage Isolation (DEC-009)"]
        UC7["UI & Diagnostic Presentation (Step 32)"]
    end

    subgraph Boundary["Strict IPC & Contract Boundary (ADR-0002)"]
        IPC["Named Pipes / Loopback Sockets (JSON-RPC 2.0)"]
    end

    subgraph AdapterResponsibilities["Game-Specific Adapter Responsibilities (Worker Process)"]
        direction TB
        AR1["Target Context & Version Declarations"]
        AR2["Mod Toolchain Orchestration (Fabric, SKSE)"]
        AR3["Game-Specific Binary Probing & Offset Resolution"]
        AR4["Format Translation (e.g. OBJ -> NIF, JSON -> NBT)"]
        AR5["Live IPC Telemetry Source / Sink (Shared Memory)"]
        AR6["Structured Error Reporting (12-Outcome Taxonomy)"]
        AR7["Self-Diagnostics & Health Reporting"]
    end

    UniversalCore <--> Boundary
    Boundary <--> AdapterResponsibilities
```

---

## 2. Universal Core Responsibilities (Non-Delegable)

The universal core maintains exclusive, non-delegable ownership of global state, project metadata, security governance, and user interaction. The following responsibilities **must never be implemented inside an adapter**:

| Subsystem Domain | Core Responsibility Statement | Authoritative Source Document |
|:---|:---|:---|
| **Canonical Game Catalog** | Defining canonical game identity, edition hierarchies, and universal metadata mappings. Adapters refer to `CanonicalGameId`; they never define it. | `NLE-SPC-GDC-001` (Step 25) |
| **Local Game Discovery** | Discovering game installation directories, reading PE binary headers, and verifying binary hashes (`SHA-256`) under user consent. | `NLE-SPC-LID-001` (Step 27) |
| **Compatibility Assessment** | Evaluating the 8 compatibility dimensions, executing the 10-stage rule pipeline, enforcing the anti-aggregation rule, and synthesizing recommendations. | `NLE-SPC-CAC-001` (Step 29), `NLE-SPC-CRD-001` (Step 31) |
| **Evidence & Provenance** | Generating cryptographic evidence records, managing 90-day retention policies, and verifying source integrity. | `NLE-SPC-ECL-001` (Step 30) |
| **User Authorization & Consent** | Enforcing Windows Medium Integrity Level, requesting human confirmation via `CMP-045`, and controlling project transitions. | `REQ-SEC-001`, `NLE-SPC-RUX-001` (Step 32) |
| **Adapter Lifecycle Supervision** | Spawning adapter worker processes, monitoring heartbeats, enforcing execution timeouts (10s boot), and terminating errant workers. | `NLE-SPC-ADP-001` (Step 11/12) |
| **Storage Containment** | Enforcing project directory scoping to Volume D (`D:\NexLudic\Projects\<id>\`). Adapters are allocated isolated sub-folders. | `DEC-009` |
| **Presentation & UI** | Rendering compatibility reports, wireframe views, and forensic drawers. Adapters have zero direct UI rendering authority. | `NLE-SPC-RUX-001` (Step 32) |

---

## 3. Game-Specific Adapter Responsibilities

An adapter is a specialized, out-of-process worker component responsible for bridging the gap between NexLudic Engine's abstract capability contracts and a specific game runtime.

Depending on its declared role, an adapter owns:
1. **Context & Capability Declaration:** Authoring and maintaining `adapter.json`, declaring which canonical capabilities it supports, which game versions it has been tested against, and its minimum toolchain prerequisites.
2. **Game-Specific Toolchain Orchestration:** Interacting with native modding frameworks (e.g. invoking Gradle for Fabric mods, calling MSBuild for SKSE64 plugins, or configuring BepInEx for Unity).
3. **Format Translation & Transcoding:** Converting platform-neutral asset representations (meshes, textures, animations, collision hulls) into game-native file formats (e.g., converting glTF to Bethesda `.nif` or Unreal `.uasset`).
4. **Binary Probing & Offset Resolution:** Inspecting static PE export tables, reading configuration files, or resolving memory offsets for games without modding APIs.
5. **Live Runtime Communication:** Exposing or consuming live IPC shared memory rings (`Strategy B/C`) for real-time telemetry streaming (physics, player transforms, inputs).
6. **Structured Failure Reporting:** Expressing every operational outcome strictly using the engine's standardized **12-Outcome Invocation Taxonomy** (`Completed_Success`, `Rejected_PreconditionFail`, `Failed_BeforeSideEffects`, etc.).
7. **Diagnostics & Limitations:** Exposing technical limitations (e.g., "Havok collision sub-stepping limited to 60 Hz") without concealing failure states.

---

## 4. Reconciled Architectural Ownership Decisions

To eliminate ambiguity across Phase 2 and Phase 5, the following cross-cutting responsibilities are formally resolved:

### 4.1 Process Model & Host Boundary (`ADR-0002`)
- **Decision:** Adapters execute strictly as **Out-of-Process Workers** (`OutOfProcessWorker`).
- **Rejected Alternative:** Loading adapters as in-process C++ DLLs or .NET assemblies inside the engine host process.
- **Architectural Rationale:** An unhandled exception, access violation (`0xC0000005`), or memory leak in a game adapter must never crash the NexLudic Engine desktop application. Process isolation guarantees host stability.

### 4.2 Adapter Launch & Process Ownership
- **Decision:** The universal engine’s **Adapter Lifecycle Supervisor** (`CMP-08`) owns the responsibility of launching, monitoring, and terminating adapter worker processes.
- **Protocol:** The supervisor spawns the adapter process using standard Win32 `CreateProcessW` under standard user privileges (`Medium Integrity`), passing a temporary IPC pipe name via command-line arguments (`--nexludic-pipe=\\.\pipe\nle-worker-<guid>`).

### 4.3 Filesystem Access Scoping (`DEC-009`, `REQ-SEC-001`)
- **Decision:** Adapters are architecturally restricted to two filesystem locations:
  1. **Read-Only Access:** The verified local game installation directory (`<GameInstallPath>\`), conditioned on explicit user consent.
  2. **Read/Write Access:** The project's isolated adapter workspace on Volume D (`D:\NexLudic\Projects\<id>\Adapters\<adapterId>\`).
- **Enforcement:** Adapters attempting to write to system directories (`C:\Windows`, `C:\Program Files`) or user profile roots are rejected and flagged for security review.

### 4.4 Verification of Installed Game Identity
- **Decision:** The **Universal Core** (specifically `LocalGameDiscoveryService`, Step 27) retains exclusive authority over identifying installed game versions via PE header analysis and binary hashing.
- **Architectural Rationale:** Adapters cannot be trusted to independently determine whether an installation is valid. An adapter receives a verified `GameVersionSummary` from the core; it does not claim authority over the host environment.

### 4.5 Secret & Configuration Management
- **Decision:** If an adapter requires API tokens (e.g., downloading symbol packages or querying authenticated mod databases), secrets are managed by the core engine’s encrypted vault (`Step 15`) and injected into the adapter process over the secure IPC pipe on-demand. Secrets are never stored in plain-text `adapter.json` manifests.

