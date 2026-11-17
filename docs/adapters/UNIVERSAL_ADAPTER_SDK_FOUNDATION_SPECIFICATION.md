# Universal Adapter SDK Foundation Specification

**Document ID:** `NLE-SPC-SDK-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 34 of 100 — Universal Adapter SDK Foundation & Contract Reconciliation Specification  
**Classification:** Foundational Architecture & SDK Contract Specification  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-GAT-PH4-001` (Phase 4 Gate), `NLE-SPC-ADP-001` (Phase 2 Adapter Contract), `NLE-DOM-SPEC-001` (Core Domain), `NLE-CAT-RIA-001` (Rule Inputs), `NLE-CAT-DRC-001` (8 Dimensions)

---

## 1. Executive Summary & Purpose

The **Universal Adapter SDK** defines the foundational, language-neutral development and contract model through which game-specific adapters integrate with the universal core of **NexLudic Engine**.

NexLudic Engine evaluates, designs, and orchestrates cross-game mashups across three primary architectures:
1. **Strategy A (Host-Game Modification):** Translating and injecting guest assets and logic directly into the host runtime via mod loaders, engine hooks, or asset replacement.
2. **Strategy B (Live Interoperability Bridging):** Executing both games concurrently in sandboxed environments, exchanging real-time state, telemetry, and input events over high-speed IPC.
3. **Strategy C (Hybrid Integration):** Partitioned execution combining host-side asset rendering with live simulation bridges.

### 1.1 The Heterogeneous Game Ecosystem Mandate
Modern PC gaming is built upon radically disparate technologies:
- Proprietary C++ engines (Bethesda Creation Engine, id Tech, Unreal Engine).
- Managed C# runtimes (Unity Mono/IL2CPP, Godot .NET).
- JVM runtimes (Minecraft Java / Fabric / Forge).
- Custom proprietary scripting environments (REDengine CET Lua, Source Engine VScript).

The Universal Adapter SDK establishes a **strictly language-neutral, contract-first boundary**. It allows adapter authors to write game integrations in their game’s native toolchain (C++, C#, Java, Python, or Rust) while communicating with the universal engine through deterministic OS-level IPC primitives, standardized JSON manifests (`adapter.json`), and canonical capability contracts.

> [!IMPORTANT]
> **Specification Invariant:**  
> This document defines the **conceptual interface, contract architecture, and developer integration model**. It is an architectural specification, **NOT a compiled SDK package, runtime daemon, or executable plugin library**. Zero code is executed or installed during this step.

---

## 2. Phase 4 Gate Reconciliation & Governing Conditions

Step 34 formally inherits and strictly honors the phase-exit determination of **READY WITH CONDITIONS** established in `NLE-GAT-PH4-001` (Step 33). The SDK foundation reconciles the three mandatory Phase 4 exit conditions:

### 2.1 Condition C-01: Adapter Schema Alignment (`NLE-CAT-RIA-001`)
- **Mandate:** Phase 5 must consume the `RuleInputDomain` and `CapabilityRequirement` contracts established in `NLE-CAT-RIA-001` and `NLE-CAT-DRC-001` without introducing competing status vocabularies.
- **SDK Enforcement:** The SDK models adapter capabilities strictly as projections of the 8 approved compatibility dimensions:
  1. `ASSET_PIPELINE`
  2. `PHYSICS`
  3. `MEMORY_HOOKING`
  4. `INPUT_CONTROL`
  5. `TELEMETRY_IPC`
  6. `RENDER_HOOK`
  7. `AUDIO`
  8. `STATE_SYNC`
- Adapters declare capability support levels (`SUPPORTED`, `CONDITIONALLY_SUPPORTED`, `UNSUPPORTED`), which map directly to Phase 4 input domain states (`INPUT_CONFIRMED_VERIFIED`, `INPUT_UNSUPPORTED`).

### 2.2 Condition C-02: Safety & Integrity Enforcement (`REQ-SEC-001`, `REQ-SAFE-001`, `DEC-009`)
- **Mandate:** All adapter worker processes and SDK tools must operate strictly within the **Windows Medium Integrity Level**, Volume D storage isolation, and zero unauthorized process injection.
- **SDK Enforcement:** In-process DLL injection into the NexLudic Engine core is **architecturally prohibited** (`ADR-0002`). Adapters execute exclusively as out-of-process workers (`OutOfProcessWorker`). Adapters requesting UAC elevation (`runas`) or kernel drivers are rejected at registration.

### 2.3 Condition C-03: Advisory Boundary Maintenance (`CMP-045`)
- **Mandate:** Phase 5 adapter management tools must not bypass user approval or trigger silent binary modifications.
- **SDK Enforcement:** Adapter discovery, qualification, and staging are strictly diagnostic. An adapter cannot bind to a project workspace or execute game modifications without explicit human confirmation via the `CMP-045 ApprovalSummaryCard` workflow.

---

## 3. High-Level Architectural System Context

```mermaid
flowchart TB
    subgraph UniversalCore["NexLudic Universal Core (Process Boundary: Engine Host)"]
        direction TB
        CatalogService["Universal Game Catalog (Step 25)"]
        RuleEngine["Compatibility Rule Engine (Step 31)"]
        ReportUI["Compatibility Report UX (Step 32 / NAV-PRJ-FEA)"]
        Supervisor["Adapter Lifecycle Supervisor (CMP-08)"]
        IPCBroker["Universal IPC Broker (Step 13)"]
    end

    subgraph SDKBoundary["Universal Adapter SDK Contract Boundary (NLE-SPC-SDK-001)"]
        ManifestSchema["adapter.json (Descriptor Schema)"]
        TraitContracts["Compositional Capability Traits (IGameAdapterContract)"]
        OutcomeTaxonomy["12-Outcome Invocation Taxonomy"]
        WireProtocol["Standard JSON-RPC 2.0 / Named Pipe Transport"]
    end

    subgraph OutOfProcessAdapters["External Game Adapters (OutOfProcessWorker Boundary)"]
        AdapterA["Game A Adapter (e.g. C++ / SKSE64 Worker)"]
        AdapterB["Game B Adapter (e.g. Java / Fabric Worker)"]
        AdapterC["Generic Telemetry Adapter (e.g. C# / Memory Worker)"]
    end

    UniversalCore <-->|Named Pipes / Loopback IPC| SDKBoundary
    SDKBoundary <-->|Out-of-Process Execution| OutOfProcessAdapters
    
    UniversalCore -.->|Volume D Scoping (DEC-009)| LocalStorage["D:\\NexLudic\\Projects\\<id>\\Adapters\\"]
```

---

## 4. Universal Adapter Model: Core Operating Principles

1. **Architecture Before Implementation:** Contracts and lifecycle state machines govern system behavior; no adapter may depend on uncommitted internal engine APIs.
2. **Explicit Contracts Over Presumptions:** Adapters must declare exact supported game version ranges, tested build hashes, toolchain prerequisites, and required permissions. Absence of declaration is treated as unsupported.
3. **Capability Declarations != Capability Verification:** An adapter declaring support for a capability (e.g., `CAP-TELEMETRY-STREAM`) represents an *assertion*. It only becomes *verified* when backed by Tier 1 (`REPRODUCED_BEHAVIOR`) or Tier 2 (`DOCUMENTED_CAPABILITY`) evidence records (`NLE-SPC-ECL-001`).
4. **Least Privilege & Zero Elevation:** Adapters must declare their minimal required permissions. Requests for administrator elevation, raw disk sectors, or unbounded filesystem writes are fatal descriptor errors.
5. **Fail-Safe & Graceful Degradation:** When an adapter process crashes, times out, or fails a precondition, the universal engine isolates the failure, executes rollback routines where necessary, and maintains report stability.
6. **Game-Agnostic Core:** The universal core contains zero game-specific hardcoding. Skyrim-specific Havok bone trees, Minecraft NBT formats, and Unreal uasset schemas reside strictly within their respective adapter boundaries.

---

## 5. Foundational SDK Architecture Components

The Universal Adapter SDK consists of five reconciled architectural pillars, detailed in subsequent Phase 5 specifications:

| Pillar Number | Architectural Domain | Governing Document | Core Responsibility |
|:---:|:---|:---|:---|
| **Pillar 1** | **Responsibility Boundaries** | `NLE-SPC-UAR-001` | Establishes the clean separation between Universal Core responsibilities (catalog, rules, policy, UI) and Adapter responsibilities (translation, probing, toolchains). |
| **Pillar 2** | **Contract & Metadata Baseline** | `NLE-CAT-ACM-001` | Defines the `adapter.json` descriptor manifest, identity rules, target game binding, and contract invariants. |
| **Pillar 3** | **Capability System Integration** | `NLE-SPC-SCI-001` | Maps adapter capability declarations to Phase 4's 8 assessment dimensions and epistemic evidence tiers. |
| **Pillar 4** | **Versioning & Runtime Neutrality**| `NLE-SPC-AVR-001` | Multi-dimensional version matrix (SDK, Adapter, RuleSet, Game, Protocol) and out-of-process IPC wire contracts. |
| **Pillar 5** | **Trust, Permissions & Lifecycle** | `NLE-SPC-ATF-001` | Sandboxing boundaries, 12-outcome invocation taxonomy, error contracts, and lifecycle supervision. |

---

## 6. Implementation Readiness & Non-Goals

### 6.1 What Step 34 Delivers
- Reconciles Phase 2 architectural ADRs with Phase 4 compatibility intelligence contracts.
- Formally defines the universal developer contract for future adapter authors.
- Establishes unambiguous failure, error, and lifecycle contracts.
- Defines developer ergonomics, documentation standards, and future validation test plans.

### 6.2 Explicit Non-Goals (Strict Stop Boundary)
- **Zero SDK Code:** No NuGet, npm, Maven, or Cargo packages are authored or built.
- **Zero Production Services:** No adapter loader, daemon supervisor, or IPC broker services are spawned.
- **Zero Game Execution:** No games are launched, hooked, or scanned.
- **Zero Advancement:** Step 35 (Adapter Manifest & Capability Declaration Specification) is NOT started.

