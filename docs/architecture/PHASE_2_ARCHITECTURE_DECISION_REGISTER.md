# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Phase 2 Architecture Decision Register

**Document ID:** `NLE-REG-ADR-002`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 16 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Authoritative Decision Register  
**Traceability:** `ADR-0001` through `ADR-0011`, Steps 9–15 Architectural Decisions

---

## 1. Executive Summary & Governance Model

This register consolidates all authoritative Architecture Decision Records (ADRs) and architectural determinations governing Phase 2 of NexLudic Engine. Every decision is categorized by its formal lifecycle status:
* **Accepted:** Formally approved baseline decision; mandatory constraint for future phases.
* **Provisional:** Working recommendation subject to empirical validation during initial implementation.
* **Deferred:** Formally evaluated and deliberately postponed to a subsequent phase.
* **Superseded:** Replaced by a newer authorized architectural determination.
* **Conflicted:** Conflicting requirements requiring architectural reconciliation.

---

## 2. Authoritative Architecture Decision Register

| Decision ID | Title | Status | Core Determination | Related Components | Requirements Addressed |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`ADR-0001`** | Initial Platform & OS Boundary | **Accepted** | Windows 10/11 x64, Medium Integrity Level, zero administrative elevation (`UAC`). | All Subsystems | `REQ-SAFE-003`, `REQ-SEC-001` |
| **`ADR-0002`** | Core Language & Runtime | **Accepted** | C# 12 / .NET 8.0 LTS as the universal platform language and runtime. | `NexLudic.Core`, `Domain` | `REQ-CORE-001`, `DEC-001` |
| **`ADR-0003`** | Desktop UI Framework | **Accepted** | Windows Presentation Foundation (WPF) with modern XAML styling on .NET 8. | `NexLudic.App` | `REQ-CORE-002`, `DEC-002` |
| **`ADR-0004`** | Module Boundaries & Core Decoupling| **Accepted** | Modular Monolith following Clean Architecture and DDD; zero game-specific logic in Core. | Core, Domain, Adapters | `REQ-CORE-003`, `DEC-004` |
| **`ADR-0005`** | Adapter Execution & Isolation Model | **Accepted** | Out-of-process worker processes; isolated child runtimes communicate via IPC. | Adapter Supervisor, Workers | `REQ-MOD-001`, `DEC-005` |
| **`ADR-0006`** | Shared Contract & Protocol Approach | **Accepted** | Strongly typed, transport-independent capability traits and message envelopes. | `NexLudic.Domain`, IPC | `REQ-MASH-004`, `REQ-TEL-001` |
| **`ADR-0007`** | Configuration, Storage & Diagnostics | **Accepted** | Tiered file-based storage on Volume `D:`; Serilog rolling logs; SQLite deferred. | `NexLudic.Infrastructure` | `REQ-CORE-004`, `DEC-009` |
| **`ADR-0008`** | AI Provider Abstraction | **Accepted** | Dual-layer Model Context Protocol (MCP) server + extensible `IAgentRunner` CLI subprocess. | `NexLudic.Core`, AI Agents | `REQ-AI-001`, `DEC-005` |
| **`ADR-0009`** | Workspace Isolation & Approval Model| **Accepted** | 3-tier sandbox: staging jail, Win32 Job Objects (2GB, timeout), human diff approval. | Core, Infrastructure, UI | `REQ-AI-002`, `REQ-AI-004` |
| **`ADR-0010`** | Testing & Validation Strategy | **Accepted** | 8-to-10 level automated testing suites; synthetic fixtures; mock game adapters. | QA Test Harnesses | `REQ-FEAS-003`, `REQ-CORE-005` |
| **`ADR-0011`** | Packaging, Dependencies & Rollback | **Accepted** | Portable desktop bundle; pre-operation snapshots; atomic rollback journals. | Packaging, Recovery | `REQ-CORE-004`, `DEC-008` |
| **`DEC-ARC-001`**| Dual-Channel IPC Baseline | **Accepted** | Stdio (`ndjson` JSON-RPC) for Control; Named Pipes (`proto3` Message Mode) for Data. | `NexLudic.Core.Ipc` | `ADR-0003`, `REQ-TEL-001` |
| **`DEC-ARC-002`**| Named Pipe Windows Security DACL | **Accepted** | Win32 SDDL restricted strictly to Current User SID (`TokenUser`); Deny Anonymous & Network.| IPC Transport, Security | `REQ-SEC-001`, `REQ-SAFE-002` |
| **`DEC-ARC-003`**| 18-Category Universal Error Taxonomy| **Accepted** | Structured error taxonomy, two-phase cooperative cancellation, and circuit breaker. | IPC, Worker Lifecycle | `REQ-SAFE-001`, `ADR-0005` |
| **`DEC-ARC-004`**| Atomic Write-Rename Durability | **Accepted** | Manifest serialization to `.tmp` followed by atomic Win32 `File.Replace`. | Persistence Repositories | `ADR-0007`, `REQ-CORE-004` |
| **`DEC-ARC-005`**| Opaque Secrets Vault via DPAPI | **Accepted** | Project manifests store opaque UUIDs; tokens encrypted in `%LocalAppData%` via DPAPI. | Secrets Manager, Core | `REQ-SEC-001`, `REQ-SAFE-002` |
| **`DEC-ARC-006`**| 7-Layer Precedence Hierarchy | **Accepted** | Precedence hierarchy where Mandatory Security Policy is supreme and non-overridable. | Configuration Manager | `ADR-0007`, `ADR-0009` |
| **`DEC-ARC-007`**| SQLite Telemetry Indexing Deferral | **Deferred** | Embedded SQLite evaluated and deferred to Phase 3; activated only if query complexity demands.| Telemetry Store (Phase 3)| `ADR-0007`, `REQ-TEL-001` |

---

## 3. Review of Deferred & Open Architectural Decisions

1. **`DEC-ARC-007` (SQLite Deferral):**
   * *Status:* Formally Deferred to Phase 3.
   * *Rationale:* Phase 2 project and blueprint manifests are flat, low-volume, and benefit enormously from plain text git-diffability. Deep telemetry indexing and relational cross-project querying will be reassessed during Phase 3/4 when real game telemetry feeds are connected.
   * *Impact on Phase 3:* Non-blocking. Phase 3 (UI & Workspace) operates exclusively on Tier 1 JSON project manifests and staging folders.

2. **In-Engine C++ Hook Injection:**
   * *Status:* Prohibited for MVP Core; evaluated strictly under Strategy B/C offline single-player adapters.
   * *Rationale:* Prevents anti-cheat false positives (`REQ-SAFE-001`). Driverless input simulation (`SendInput`) and standard modding interfaces (BepInEx/Creation Kit) are prioritized.

