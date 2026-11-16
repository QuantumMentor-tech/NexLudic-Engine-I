# NexLudic Engine — Dependency Rules and Allowed References

**Document ID:** `NLE-STD-DEP-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 10 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior Systems Architecture  

---

## 1. Overview & Architectural Invariants

The **Dependency Rules and Allowed References** specification defines the binding rules governing all references, project imports, package dependencies, and inter-component invocations within **NexLudic Engine**.

In a universal multi-game platform, uncontrolled dependencies lead to catastrophic architectural decay: game-specific assemblies leak into domain models, UI components invoke OS commands directly, and circular references paralyze automated testing. This standard establishes **strict Clean Architecture dependency inversion rules** to guarantee long-term maintainability.

```
+----------------------------------------------------------------------------------------------------+
|                                    CLEAN DEPENDENCY DIRECTION                                      |
+----------------------------------------------------------------------------------------------------+
|                                                                                                    |
|  [ NexLudic.App ] (Presentation / UI)                                                              |
|        |                                                                                           |
|        +-----------------------------------+                                                       |
|        | (Direct Reference)                | (Direct Reference)                                    |
|        v                                   v                                                       |
|  [ NexLudic.Core ] (Application)   [ NexLudic.Infrastructure ] (OS / IO / Store)                  |
|        |                                   |                                                       |
|        +-----------------+                 |                                                       |
|                          |                 |                                                       |
|                          v                 v                                                       |
|                  [ NexLudic.Domain ] (Pure Entities & Traits - ZERO EXTERNAL REFERENCES)           |
|                                                                                                    |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. The Four Inviolable Dependency Invariants

1. **Invariant 1: The Domain Invariance Rule (`RULE-DEP-01`):**  
   `NexLudic.Domain` must have **zero references** to other NexLudic projects, zero third-party NuGet packages (excluding essential system primitives), zero UI frameworks, and zero game-specific SDKs. It contains only pure POCO entities, value objects, and trait interfaces.
2. **Invariant 2: The Core Decoupling Rule (`RULE-DEP-02`):**  
   `NexLudic.Core` depends **exclusively on `NexLudic.Domain`**. It orchestrates workflows by defining interfaces (ports) that are implemented by `NexLudic.Infrastructure` (adapters).
3. **Invariant 3: The Game Assembly Isolation Rule (`RULE-DEP-03`):**  
   No universal project (`NexLudic.App`, `Core`, `Domain`, `Infrastructure`) may reference game-specific DLLs (e.g. `FabricLoader.jar`, `UnityEngine.dll`, `Insomniac.dll`). Game-specific code exists **exclusively in out-of-process adapter workers** (`ADR-0002`).
4. **Invariant 4: The Presentation Isolation Rule (`RULE-DEP-04`):**  
   `NexLudic.App` may reference `NexLudic.Core` and `NexLudic.Domain` for ViewModels and commands. It must **never directly reference or invoke low-level infrastructure classes** (e.g. direct Win32 P/Invoke, raw file writes, or raw Named Pipe handles).

---

## 3. Master Component Dependency Matrix

The following matrix defines the permitted reference relationships between all 15 components:
- **`[D]` Allowed Direct Dependency:** Caller may directly reference and instantiate types.
- **`[C]` Contract-Mediated Dependency:** Caller must reference through an abstraction/interface defined in Domain or Core.
- **`[?]` Conditional Dependency:** Allowed only under documented architectural conditions.
- **`[F]` Forbidden Dependency:** Strictly prohibited; violates architectural layering.
- **`[-]` Self Reference.**

```
+-------------------------------------------------------------------------------------------------------------------+
|                                            COMPONENT DEPENDENCY MATRIX                                            |
+--------+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+----------------+
| Target | C01 | C02 | C03 | C04 | C05 | C06 | C07 | C08 | C09 | C10 | C11 | C12 | C13 | C14 | C15 | Target Layer   |
| Caller | Proj| Game| Cap | Feas| Evid| Arch| Adpt| Life| IPC | AI  | Stg | Tool| Test| DB  | Roll|                |
+--------+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+----------------+
| CMP-01 |  -  |  D  |  D  |  D  |  C  |  D  |  D  |  C  |  C  |  D  |  C  |  F  |  C  |  C  |  C  | Application    |
| CMP-02 |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  C  |  F  | Domain/App     |
| CMP-03 |  F  |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  | Domain/App     |
| CMP-04 |  F  |  D  |  D  |  -  |  D  |  F  |  D  |  F  |  F  |  F  |  F  |  F  |  F  |  C  |  F  | Application    |
| CMP-05 |  F  |  F  |  F  |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  C  |  F  | Domain/Infra   |
| CMP-06 |  F  |  F  |  F  |  F  |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  C  |  F  | Application    |
| CMP-07 |  F  |  F  |  D  |  F  |  F  |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  |  C  |  F  | Application    |
| CMP-08 |  F  |  F  |  F  |  F  |  F  |  F  |  D  |  -  |  C  |  F  |  F  |  C  |  F  |  F  |  F  | Infrastructure |
| CMP-09 |  F  |  F  |  C  |  F  |  C  |  F  |  F  |  F  |  -  |  F  |  F  |  F  |  F  |  F  |  F  | Infrastructure |
| CMP-10 |  F  |  F  |  D  |  F  |  F  |  F  |  F  |  F  |  F  |  -  |  C  |  F  |  F  |  F  |  F  | Application    |
| CMP-11 |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  -  |  F  |  F  |  F  |  C  | Infrastructure |
| CMP-12 |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  D  |  -  |  F  |  F  |  F  | Infrastructure |
| CMP-13 |  D  |  D  |  D  |  D  |  D  |  F  |  D  |  C  |  C  |  F  |  C  |  F  |  -  |  C  |  C  | Application    |
| CMP-14 |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  -  |  F  | Infrastructure |
| CMP-15 |  F  |  D  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  F  |  D  |  F  |  F  |  F  |  -  | Infrastructure |
+--------+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+-----+----------------+
```

---

## 4. Specific Boundary Rules

### 4.1 Domain Layer Isolation (`CMP-02`, `CMP-03`, `CMP-05`)
- May never call out to `NexLudic.Core`, `NexLudic.Infrastructure`, or `NexLudic.App`.
- Contains pure interfaces, entities, and validation rules.
- Circular references are mathematically impossible because the layer has zero outgoing project references.

### 4.2 Application Orchestration (`CMP-01`, `CMP-04`, `CMP-07`, `CMP-10`, `CMP-13`)
- May reference `NexLudic.Domain` directly.
- Communicates with Infrastructure components (`CMP-08`, `CMP-09`, `CMP-11`, `CMP-12`, `CMP-14`, `CMP-15`) **strictly through dependency-inversion interfaces** (e.g. `IWorkspaceManager`, `IToolchainExecutionDriver`, `IPersistenceStore`).

### 4.3 AI Orchestration & Tool Execution Boundary (`CMP-10` vs. `CMP-12`)
- **Prohibited Reference:** `CMP-10` (AI Orchestrator) is **strictly forbidden** from calling `CMP-12` (Toolchain Execution Driver) directly.
- **Enforcement:** AI agents cannot invoke compilers or OS processes. AI output must be routed through `CMP-01` and `CMP-11` to stage files and request human diff approval before compilation can occur.

---

## 5. Architectural Exception Governance

If an engineering requirement cannot be satisfied within the established dependency rules, an **Architectural Exception** must be formally requested:

```
+----------------------------------------------------------------------------------------------------+
|                                    ARCHITECTURAL EXCEPTION LIFECYCLE                               |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |              SUBMIT EXCEPTION REQUEST (Form)             |
                     |  (Document conflict, trade-offs, and alternative designs) |
                     +----------------------------+-----------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |           SECURITY & ARCHITECTURE BOARD AUDIT            |
                     |   (Verify zero anti-cheat, zero driver, zero admin breach|
                     +----------------------------+-----------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |             ARB FORMAL DISPOSITION (PASS / REJECT)       |
                     |    (If approved, recorded in DECISION_LOG with expiration|
                     +----------------------------------------------------------+
```

### Mandatory Non-Bypassable Invariants:
Under no circumstances may an architectural exception be granted to:
- Bypass anti-cheat protection (`REQ-SAFE-001`).
- Install kernel-mode drivers (`DEC-003`).
- Request administrative privilege elevation (`REQ-SEC-001`).
- Redistribute raw copyrighted game assets (`REQ-MOD-003`).

---

*End of Dependency Rules and Allowed References (NLE-STD-DEP-001).*

