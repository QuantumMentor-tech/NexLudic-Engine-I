# NexLudic Engine — Domain Boundaries and Contracts

**Document ID:** `NLE-ARC-DOM-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 10 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Domain Architecture  

---

## 1. Executive Summary & Purpose

The **Domain Boundaries and Contracts** specification formalizes the core entities, value objects, aggregate roots, capability contracts, and lifecycle state machines within `NexLudic.Domain`.

In strict accordance with Domain-Driven Design (DDD) principles and Clean Architecture, `NexLudic.Domain` maintains the authoritative representation of project state, feasibility determinations, capability dependencies, and adapter traits. It remains **100% free of external infrastructure packages, game-specific SDKs, and UI references**.

> [!NOTE]
> **Illustrative Architecture Standard Notice:**  
> All C# code listings in this specification represent **conceptual contract definitions** to guide future implementation in Step 11. They are not compiled source files and do not represent an existing runtime SDK.

---

## 2. Core Domain Aggregates & Entity Catalog

```
+----------------------------------------------------------------------------------------------------+
|                                    DOMAIN ENTITY CLASSIFICATION                                    |
+-------------------+--------------------+--------------------+--------------------------------------+
| Conceptual Entity | Entity Type        | Authoritative Owner| Persistence Requirement              |
+-------------------+--------------------+--------------------+--------------------------------------+
| GamePairProject   | Aggregate Root     | CMP-01 (Project)   | Persistent (LiteDB `projects`)       |
| GameIdentity      | Entity             | CMP-02 (Registry)  | Persistent (LiteDB `game_catalog`)   |
| GameVersionRef    | Value Object       | CMP-02 (Registry)  | Embedded in `GameSignature`          |
| RequestedCapab    | Entity             | CMP-03 (Capability)| Persistent (LiteDB `capabilities`)   |
| CapabilityDep     | Value Object       | CMP-03 (Capability)| Embedded in `CapabilityContract`     |
| AdapterDescriptor | Entity             | CMP-07 (Catalog)   | Persistent (LiteDB `adapters`)       |
| CapabilityDecl    | Value Object       | CMP-07 (Catalog)   | Embedded in `AdapterDescriptor`      |
| FeasibilityAssess | Aggregate Root     | CMP-04 (Feas Engine| Persistent (LiteDB `feasibility`)    |
| EvidenceRecord    | Value Object       | CMP-05 (Evidence)  | Persistent (Append-only)             |
| RiskRecord        | Entity             | CMP-04 (Feas Engine| Persistent (LiteDB `risks`)          |
| AgentTask         | Entity             | CMP-10 (AI Orchestr| Persistent (LiteDB `agent_tasks`)    |
| ExecutionApproval | Value Object       | CMP-01 (Project)   | Persistent (Audit trail)             |
| WorkspaceRef      | Value Object       | CMP-11 (Workspace) | Ephemeral / Configuration            |
| ArtifactRecord    | Entity             | CMP-11 (Workspace) | Persistent (LiteDB `artifacts`)      |
| TestSpecification | Entity             | CMP-13 (Validation)| Persistent (LiteDB `test_specs`)     |
| TestExecutionRec  | Entity             | CMP-13 (Validation)| Persistent (LiteDB `test_results`)   |
+-------------------+--------------------+--------------------+--------------------------------------+
```

---

## 3. In-Depth Domain Model Specifications

### 3.1 Aggregate Root: `GamePairProject`
- **Purpose:** Represents the central mashup project binding two distinct games, requested capabilities, active adapters, and feasibility status.
- **Identity Invariant:** Unique project slug (e.g. `proj-mcjava-rdr2-western`).
- **Key Invariants:**
  - Game A and Game B cannot point to the exact same game executable signature.
  - A project cannot transition to `Status = Feasible` without passing Gate G1 and G2.
  - Rollback script `cleanup.ps1` must be registered before any deployment is authorized.

### 3.2 Entity: `CapabilityContract` (`CAP-###`)
- **Purpose:** Defines an atomic, testable integration requirement (e.g., `CAP-001: Western Revolver Weapon Item`).
- **Lifecycle States:**
  - `Discovered` $\to$ `Specified` $\to$ `Feasible` $\to$ `Staged` $\to$ `Validated` $\to$ `Deprecated`.
- **Validation Invariants:**
  - Must define explicit input parameters, expected observable behaviors, and quantitative performance budgets.
  - Cannot depend on unverified capabilities without inheriting their uncertainty status.

### 3.3 Aggregate Root: `FeasibilityAssessment`
- **Purpose:** Encapsulates the formal 14-question feasibility inquiry and Gate G0–G6 status.
- **Formal Decision States:**
  - `FEASIBLE`, `CONDITIONALLY FEASIBLE`, `BLOCKED PENDING POC`, `UNFEASIBLE (TECHNICAL)`, `UNFEASIBLE (POLICY)`, `DEFERRED`.
- **Invariants:**
  - Detection of active kernel anti-cheat immediately forces `UNFEASIBLE (POLICY)`.
  - Epistemic Tier cannot be marked `Tier 4 (Empirically Proven)` without an attached `TestExecutionRecord` verifying pass criteria.

---

## 4. Universal Adapter Contract Specification

Game adapters interact with the universal core through **Compositional Capability Traits** (`ADR-0002`):

```csharp
// Conceptual base adapter trait implemented by all game adapters
public interface IGameAdapterContract
{
    string AdapterId { get; }
    string TargetGameId { get; }
    SemanticVersionRange SupportedGameVersions { get; }
    AdapterExecutionModel ExecutionModel { get; }
    IReadOnlyCollection<CapabilityDeclaration> DeclaredCapabilities { get; }
    
    Task<AdapterHealthReport> CheckHealthAsync(CancellationToken ct);
    Task ShutdownAsync(CancellationToken ct);
}

// Conceptual trait for Strategy A (Host Mod) scaffolding
public interface IModScaffoldableTrait
{
    Task<ScaffoldManifest> GenerateScaffoldAsync(ScaffoldContext context, CancellationToken ct);
    Task<BuildResult> ExecuteBuildAsync(StagingPath stagingPath, CancellationToken ct);
    Task<RollbackManifest> GenerateRollbackManifestAsync(CancellationToken ct);
}

// Conceptual trait for Strategy B (Live Bridge) Named Pipe telemetry
public interface ITelemetrySourceTrait
{
    string NamedPipeEndpoint { get; }
    ProtocolSchema TelemetrySchema { get; }
    Task<bool> InitializePipeSessionAsync(CancellationToken ct);
    Task DisconnectSessionAsync(CancellationToken ct);
}

// Conceptual trait for user-mode synthetic input forwarding
public interface IInputTargetTrait
{
    Task<InputDispatchResult> ForwardInputAsync(SyntheticInputEvent inputEvent, CancellationToken ct);
}
```

---

## 5. Contract Ownership & Versioning Policy

Every architectural contract is governed by an explicit ownership and versioning policy:

| Contract Boundary | Authoritative Owner | Consumers | Versioning Policy | Breaking Change Governance |
| :--- | :--- | :--- | :--- | :--- |
| **Domain Model Contracts** | `NexLudic.Domain` | Application, Infrastructure | Semantic Versioning (`v1.0.0`) | Deprecate over 1 major release; ARB approval req. |
| **Adapter Trait Contracts** | `NexLudic.Domain` | Game Adapters, Scaffolding | Strict Semantic Versioning | Backward-compatible trait additions only. |
| **Named Pipe Protocol Wire**| `NexLudic.Infrastructure`| Adapter Workers, Live Games | Protocol Buffers (`proto3`) | Additive field numbers only; no tag recycling. |
| **Staging File Manifests** | `NexLudic.Core` | Toolchains, Rollback | JSON Schema Versioning | Schema hash validation at project load. |

---

## 6. Open Contract Questions for Step 11

The following technical implementation questions are formally deferred to Step 11:
1. **Serialization Format Finalization:** Whether to standardize on Protocol Buffers (`proto3`) or MessagePack for ultra-low-latency named pipe transport (`ADR-0003`).
2. **Process Supervision Wire:** Standardizing JSON-RPC over stdin/stdout vs. local loopback sockets for out-of-process adapter control messages.

---

*End of Domain Boundaries and Contracts (NLE-ARC-DOM-001).*

