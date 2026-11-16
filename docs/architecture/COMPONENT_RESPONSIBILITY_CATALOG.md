# NexLudic Engine — Component Responsibility Catalog

**Document ID:** `NLE-CAT-COMP-001`  
**Version:** 1.0.0  
**Phase:** 2 — Architecture & Development Foundation (Step 10 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Systems Architecture  

---

## 1. Catalog Overview & Engineering Rigor

The **Component Responsibility Catalog** establishes the exhaustive, binding specification for each of the 15 logical components comprising **NexLudic Engine**.

This catalog ensures that future developers and AI coding agents have unambiguous guidance regarding component ownership, preventing responsibility bleeding, architectural drift, and accidental circular dependencies.

---

## 2. Component Specifications (CMP-01 through CMP-15)

---

### Component ID: `CMP-01`
- **Component Name:** **Project Management Service**
- **Primary Responsibility:** Orchestrating project workspace lifecycles, project configuration, and project-level commands.
- **Explicit Non-Responsibilities:** Directly compiling mods, executing game processes, or writing directly to game installations.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `CreateProjectCommand`, `OpenProjectCommand`, `ProjectSettingsDto`.
  - *Outputs:* `ProjectEntity`, `ProjectCreatedEvent`, `ProjectStatusSummary`.
- **Authoritative Data Owned:** `ProjectEntity`, project directory paths, and project configuration manifests (`project.json`).
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-02` (Game Registry), `CMP-11` (Workspace Manager), `CMP-14` (Persistence).
- **Forbidden Dependencies:** Game-specific SDKs, direct Win32 P/Invoke, `NexLudic.App` (UI presentation classes).
- **Security & Trust Boundary:** Boundary TB-04 (User Input $\to$ Core). Enforces path sanitization within user directory.
- **Failure Modes:** Corrupted project file, disk full, path traversal attempt.
- **Required Diagnostics:** Project initialization timing, directory creation traces, state transition logs.
- **Required Tests:** Unit tests for command validation; integration tests for project creation and directory layout.
- **Relevant Requirements & ADRs:** `REQ-F-001`, `ADR-0001`, `ADR-0008`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Separates high-level project lifecycle governance from low-level file I/O and domain rules.

---

### Component ID: `CMP-02`
- **Component Name:** **Game Registry & Version Catalog**
- **Primary Responsibility:** Detecting, recording, and verifying target game executables, editions, and SHA-256 binary signatures.
- **Explicit Non-Responsibilities:** Launching game executables, modifying game files, or reverse-engineering game binaries.
- **Architectural Layer:** Domain & Application Layer (`NexLudic.Domain` / `NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `GamePathVerificationRequest`, target directory paths.
  - *Outputs:* `GameSignatureEntity`, `SHA256Hash`, `GameVersionProfile`.
- **Authoritative Data Owned:** Verified `GameSignature` records, known baseline game hashes, and edition metadata.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-14` (Persistence Engine).
- **Forbidden Dependencies:** Game mod loaders, external network APIs, game process handles.
- **Security & Trust Boundary:** Read-only inspection of game installations; validates against known anti-cheat executables.
- **Failure Modes:** Executable missing, hash mismatch against baseline, file access locked by running process.
- **Required Diagnostics:** Measured hash computation duration, detected executable version metadata.
- **Required Tests:** Unit tests for SHA-256 calculation; mock tests for version string parsing.
- **Relevant Requirements & ADRs:** `REQ-F-002`, `REQ-CMP-005`, `RSK-03`, `ADR-0004`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Centralizes binary signature verification to detect game patches before integration runs.

---

### Component ID: `CMP-03`
- **Component Name:** **Capability & Requirements Service**
- **Primary Responsibility:** Decomposing user mashup intent into atomic, testable capability contracts (`CAP-###`) and tracking support states.
- **Explicit Non-Responsibilities:** Generating code, verifying feasibility, or executing tests.
- **Architectural Layer:** Domain & Application Layer (`NexLudic.Domain` / `NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* User intent text, capability definition DTOs.
  - *Outputs:* `CapabilityContractCollection`, `CapabilityDependencyGraph`.
- **Authoritative Data Owned:** Atomic `CapabilityContract` definitions, capability status states (`Supported`, `Partial`, etc.).
- **Permitted Dependencies:** `NexLudic.Domain`.
- **Forbidden Dependencies:** Infrastructure components, UI views, external toolchains.
- **Security & Trust Boundary:** Pure domain logic; sanitizes user intent input.
- **Failure Modes:** Circular capability dependencies, malformed capability schema.
- **Required Diagnostics:** Capability decomposition trace, dependency graph cycle detection logs.
- **Required Tests:** Pure domain unit tests for capability state transitions and dependency validation.
- **Relevant Requirements & ADRs:** `REQ-F-003`, `REQ-F-013`, `ADR-0001`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Encapsulates capability representation independent of how the capability is implemented.

---

### Component ID: `CMP-04`
- **Component Name:** **Feasibility Assessment Engine**
- **Primary Responsibility:** Executing the 14-question feasibility inquiry (`Q1`–`Q14`), evaluating strategy trade-offs, and enforcing policy gates.
- **Explicit Non-Responsibilities:** Authoring adapter code or running empirical test harnesses.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `GameSignatureEntity`, `CapabilityContractCollection`, adapter capability declarations.
  - *Outputs:* `FeasibilityReport`, `RecommendedStrategy` (A, B, or C), Gate G1/G2 clearance determinations.
- **Authoritative Data Owned:** Formal feasibility determinations (`FEASIBLE`, `CONDITIONALLY FEASIBLE`, etc.) and gate records.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-02` (Game Registry), `CMP-03` (Capabilities), `CMP-05` (Evidence), `CMP-07` (Adapters).
- **Forbidden Dependencies:** Raw game process memory, external AI execution.
- **Security & Trust Boundary:** Enforces Mandatory Blockers (anti-cheat detection, DRM circumvention refusal).
- **Failure Modes:** Contradictory evidence, incomplete adapter metadata, unverified interface claims.
- **Required Diagnostics:** 14-question evaluation logs, weighted strategy scorecard calculations.
- **Required Tests:** Scenario tests evaluating edge-case pairings (e.g., VAC anti-cheat detection).
- **Relevant Requirements & ADRs:** `REQ-F-004`, `REQ-F-005`, `REQ-SAFE-001`, `ADR-0004`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Isolates analytical feasibility logic from mod execution and build tooling.

---

### Component ID: `CMP-05`
- **Component Name:** **Evidence & Provenance Tracker**
- **Primary Responsibility:** Recording, classifying, and persisting source references, test traces, and Epistemic Tiers (Tiers 1–4).
- **Explicit Non-Responsibilities:** Generating documentation or running test cases.
- **Architectural Layer:** Domain & Infrastructure Layer (`NexLudic.Domain` / `NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* Documentation citations, test trace JSON files, benchmark logs.
  - *Outputs:* `EvidenceRecord`, `EpistemicTierClassification`.
- **Authoritative Data Owned:** Immutable evidence records and audit provenance chains.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-14` (Persistence Engine).
- **Forbidden Dependencies:** External scrapers, UI view classes.
- **Security & Trust Boundary:** Validates URL references and enforces tamper-evident log appending.
- **Failure Modes:** Stale citation link, missing test artifact reference.
- **Required Diagnostics:** Provenance validation traces, tier transition logs.
- **Required Tests:** Unit tests for Epistemic Tier validation rules and immutable append behavior.
- **Relevant Requirements & ADRs:** `REQ-F-005`, `ADR-0001`, `ADR-0007`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Ensures all platform claims remain traceable to concrete empirical or sourced evidence.

---

### Component ID: `CMP-06`
- **Component Name:** **Architecture Decision Tracker**
- **Primary Responsibility:** Linking project implementation choices to authoritative ADRs and tracking provisional decisions.
- **Explicit Non-Responsibilities:** Enforcing runtime behavior or making business decisions.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* ADR references (`ADR-0001` to `0011`), project decision override requests.
  - *Outputs:* `DecisionAuditReport`, unresolved decision warnings.
- **Authoritative Data Owned:** Project-level architecture decision links and override logs.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-14` (Persistence).
- **Forbidden Dependencies:** Build tools, game processes.
- **Security & Trust Boundary:** Internal governance; prevents unauthorized override of safety ADRs.
- **Failure Modes:** Missing ADR reference, unresolved provisional decision flag.
- **Required Diagnostics:** Architecture compliance checks during gate reviews.
- **Required Tests:** Traceability audit tests against `REQUIREMENTS_TRACEABILITY_MATRIX.md`.
- **Relevant Requirements & ADRs:** `ADR-0001` through `ADR-0011`.
- **Current Status:** **PROVISIONAL (Internal Governance).**
- **Boundary Rationale:** Maintains explicit traceability to architecture decision records.

---

### Component ID: `CMP-07`
- **Component Name:** **Adapter Catalog & Metadata Registry**
- **Primary Responsibility:** Discovering, indexing, and validating adapter descriptors, capability declarations, and supported version ranges.
- **Explicit Non-Responsibilities:** Executing adapters, managing adapter processes, or compiling adapter code.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `AdapterDescriptorDto`, adapter manifest files.
  - *Outputs:* `RegisteredAdapterEntity`, `AdapterCapabilityMatrix`.
- **Authoritative Data Owned:** Catalog of registered adapters, supported game version ranges, and trait metadata.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-14` (Persistence).
- **Forbidden Dependencies:** Concrete adapter assemblies, in-process DLL loading.
- **Security & Trust Boundary:** Validates that adapter descriptors do not declare prohibited capabilities (e.g. kernel drivers).
- **Failure Modes:** Corrupted adapter manifest, unsupported API version declared.
- **Required Diagnostics:** Adapter registration logs, compatibility range checks.
- **Required Tests:** Unit tests for manifest validation and version compatibility matching.
- **Relevant Requirements & ADRs:** `REQ-F-006`, `ADR-0002`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Decouples adapter discovery and metadata inspection from adapter execution.

---

### Component ID: `CMP-08`
- **Component Name:** **Adapter Lifecycle Supervisor**
- **Primary Responsibility:** Starting, monitoring, health-checking, and cleanly terminating out-of-process adapter worker processes.
- **Explicit Non-Responsibilities:** Executing mod logic, rendering game graphics, or compiling source code.
- **Architectural Layer:** Infrastructure Layer (`NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* `StartAdapterCommand`, `StopAdapterCommand`, heartbeat ping requests.
  - *Outputs:* `AdapterProcessHandle`, `AdapterHealthStatus`, `AdapterCrashEvent`.
- **Authoritative Data Owned:** Active adapter worker process IDs, heartbeat timestamps, and exit codes.
- **Permitted Dependencies:** `NexLudic.Domain`, Win32 Process APIs, `CMP-12` (Toolchain Execution Driver).
- **Forbidden Dependencies:** In-process game SDK loading, UI presentation layers.
- **Security & Trust Boundary:** Boundary TB-02 (Running Game / Worker $\leftrightarrow$ Core). Spawns workers in Medium IL.
- **Failure Modes:** Worker crash (`0xC0000005`), worker startup timeout ($>10\text{ s}$), unresponsive heartbeat.
- **Required Diagnostics:** Worker launch arguments (sanitized), memory working set samples, exit code traces.
- **Required Tests:** Process lifecycle tests simulating abrupt worker crash and verifying UI stability.
- **Relevant Requirements & ADRs:** `REQ-NF-005`, `ADR-0002`, `ADR-0009`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Enforces the Out-of-Process Adapter Worker Model to guarantee crash containment.

---

### Component ID: `CMP-09`
- **Component Name:** **Integration Coordination Service**
- **Primary Responsibility:** Managing the Windows Named Pipe IPC host (`\\.\pipe\nexludic_*`), routing 60 Hz telemetry, and coordinating bridge sessions.
- **Explicit Non-Responsibilities:** Modifying game memory or executing direct input hooks.
- **Architectural Layer:** Application & Infrastructure Layer (`NexLudic.Core` / `NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* `StartBridgeSessionCommand`, raw telemetry packets from Game A.
  - *Outputs:* Deserialized event packets dispatched to Game B, round-trip latency metrics.
- **Authoritative Data Owned:** Active IPC pipe handles, session connection state, and in-flight telemetry queues.
- **Permitted Dependencies:** `NexLudic.Domain`, `System.IO.Pipes`, Protocol Buffers library.
- **Forbidden Dependencies:** Game-specific network sockets, kernel-mode drivers.
- **Security & Trust Boundary:** Enforces Windows Access Control Lists (ACLs) restricting pipe access to the current user SID.
- **Failure Modes:** Broken pipe (`BrokenPipeException`), buffer overflow, packet deserialization corruption.
- **Required Diagnostics:** High-resolution latency histograms ($\le 16.6\text{ ms}$ budget), packet throughput counters.
- **Required Tests:** Benchmark tests measuring round-trip latency under simulated 60 Hz traffic.
- **Relevant Requirements & ADRs:** `REQ-F-007`, `REQ-NF-001`, `ADR-0003`.
- **Current Status:** **APPROVED (Strategy B/C Baseline).**
- **Boundary Rationale:** Centralizes inter-process communication logic and real-time protocol handling.

---

### Component ID: `CMP-10`
- **Component Name:** **AI Agent Task Orchestrator**
- **Primary Responsibility:** Translating user intent and capability requirements into bounded prompts, managing agent sessions, and staging proposals.
- **Explicit Non-Responsibilities:** Directly executing code, invoking OS commands, or deploying files to game folders.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `ScaffoldPromptContext`, user intent, capability contract.
  - *Outputs:* Staged source code files, mod manifests, build instructions.
- **Authoritative Data Owned:** Agent session history, prompt templates, and raw agent output logs.
- **Permitted Dependencies:** `NexLudic.Domain`, external LLM client interfaces, `CMP-11` (Workspace Manager).
- **Forbidden Dependencies:** Win32 P/Invoke, OS command execution, direct game directory access.
- **Security & Trust Boundary:** Boundary TB-01 (External AI $\leftrightarrow$ Core). Treats all model outputs as untrusted proposals.
- **Failure Modes:** LLM API rate limit, network timeout, hallucinated syntax error.
- **Required Diagnostics:** Token usage counts, API response latency, prompt context dumps.
- **Required Tests:** Mock tests verifying prompt sanitization and output isolation to staging.
- **Relevant Requirements & ADRs:** `REQ-F-008`, `ADR-0008`, `ADR-0009`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Strictly decouples AI prompt generation from operating system execution.

---

### Component ID: `CMP-11`
- **Component Name:** **Workspace & Staging Manager**
- **Primary Responsibility:** Managing the `<project_root>/staging/` directory jail, computing file diffs, and staging files for human review.
- **Explicit Non-Responsibilities:** Executing compilers or deciding whether a diff is acceptable.
- **Architectural Layer:** Infrastructure Layer (`NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* Generated source files from `CMP-10`, target game directory paths.
  - *Outputs:* `FileDiffManifest`, staging file paths, directory integrity hashes.
- **Authoritative Data Owned:** Staging directory contents, file diff records, and pre-deployment hash snapshots.
- **Permitted Dependencies:** `NexLudic.Domain`, `System.IO`.
- **Forbidden Dependencies:** AI provider APIs, game memory access.
- **Security & Trust Boundary:** Boundary TB-03 (Staging $\leftrightarrow$ Game Directory). Enforces staging isolation.
- **Failure Modes:** Disk full, file lock on target game file, path traversal attempt.
- **Required Diagnostics:** Staging file manifests, computed diff lines, directory hash snapshots.
- **Required Tests:** Unit tests validating path traversal prevention and exact file diff generation.
- **Relevant Requirements & ADRs:** `REQ-F-008`, `REQ-F-009`, `ADR-0008`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Provides the isolated physical workspace required for human diff review prior to deployment.

---

### Component ID: `CMP-12`
- **Component Name:** **Toolchain Execution Driver**
- **Primary Responsibility:** Invoking external compilers (Gradle, MSBuild, CMake) strictly within Win32 Job Object sandboxes.
- **Explicit Non-Responsibilities:** Authoring source code or deploying built binaries.
- **Architectural Layer:** Infrastructure Layer (`NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* `CompileCommand`, toolchain executable path, working directory (`staging/`).
  - *Outputs:* `BuildResult`, standard output/error logs, compiler exit code.
- **Authoritative Data Owned:** Active Win32 Job Object handles, compiler process trees, and build exit codes.
- **Permitted Dependencies:** `NexLudic.Domain`, Win32 Job Object P/Invoke APIs, `System.Diagnostics.Process`.
- **Forbidden Dependencies:** Direct game file access, UI classes.
- **Security & Trust Boundary:** Boundary TB-05 (Compiler $\to$ Host OS). Enforces $2\text{ GB}$ RAM limits and 60-second timeouts.
- **Failure Modes:** Compiler syntax error, build timeout, process runaway memory consumption.
- **Required Diagnostics:** Compiler execution duration, captured stdout/stderr, peak memory usage.
- **Required Tests:** Integration tests verifying that a runaway compiler process is terminated by the Job Object.
- **Relevant Requirements & ADRs:** `REQ-F-006`, `REQ-SEC-005`, `ADR-0009`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Centralizes secure, resource-bounded execution of third-party compilation tools.

---

### Component ID: `CMP-13`
- **Component Name:** **Testing & Validation Manager**
- **Primary Responsibility:** Coordinating automated test harnesses, verifying quantitative thresholds, and clearing Gates G4 through G6.
- **Explicit Non-Responsibilities:** Generating source code or modifying core domain rules.
- **Architectural Layer:** Application Layer (`NexLudic.Core`).
- **Main Inputs & Outputs:**
  - *Inputs:* `PoCSpecification`, test execution commands, telemetry logs.
  - *Outputs:* `TestValidationReport`, Gate clearance status, threshold compliance flags.
- **Authoritative Data Owned:** Empirical test logs, latency samples, and Gate G4 validation records.
- **Permitted Dependencies:** `NexLudic.Domain`, `CMP-05` (Evidence), `CMP-09` (IPC Host), `CMP-15` (Rollback).
- **Forbidden Dependencies:** Direct game memory hacking.
- **Security & Trust Boundary:** Enforces that only tests clearing quantitative thresholds ($\le 16.6\text{ ms}$, $\le 2\text{ FPS}$) pass Gate G4.
- **Failure Modes:** Frame rate degradation exceeding threshold, unhandled host crash during fault test.
- **Required Diagnostics:** High-resolution latency histograms, frame rate impact counters, test pass/fail logs.
- **Required Tests:** Meta-tests evaluating the validation manager's ability to reject failing test runs.
- **Relevant Requirements & ADRs:** `REQ-F-011`, `REQ-NF-001`, `REQ-NF-002`, `NLE-STD-POC-001`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Guarantees objective, quantitative validation before declaring an integration feasible.

---

### Component ID: `CMP-14`
- **Component Name:** **Persistence & Config Engine (LiteDB)**
- **Primary Responsibility:** Providing durable local storage for project metadata, verified game signatures, evidence, and settings.
- **Explicit Non-Responsibilities:** Storing large binary mod packages or streaming live telemetry.
- **Architectural Layer:** Infrastructure Layer (`NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* Domain entities (`ProjectEntity`, `GameSignatureEntity`, `EvidenceRecord`).
  - *Outputs:* Persisted documents, LINQ query results.
- **Authoritative Data Owned:** LiteDB database file (`.db`) located within `<project_root>/.nexludic/`.
- **Permitted Dependencies:** `NexLudic.Domain`, LiteDB library.
- **Forbidden Dependencies:** Network database drivers, UI presentation logic.
- **Security & Trust Boundary:** Local file permissions; rejects storing plaintext credentials or private tokens.
- **Failure Modes:** Database file corruption, disk full, concurrent write lock conflict.
- **Required Diagnostics:** Query execution duration, database file size metrics, transaction commit logs.
- **Required Tests:** Integration tests verifying CRUD operations, schema migrations, and backup restoration.
- **Relevant Requirements & ADRs:** `REQ-F-001`, `ADR-0007`.
- **Current Status:** **APPROVED (LiteDB Embedded Store Baseline).**
- **Boundary Rationale:** Encapsulates local embedded persistence behind generic repository interfaces.

---

### Component ID: `CMP-15`
- **Component Name:** **Diagnostics, Recovery & Rollback Engine**
- **Primary Responsibility:** Generating and executing `cleanup.ps1`, verifying pristine file hashes after teardown, and handling crash dumps.
- **Explicit Non-Responsibilities:** Modifying game logic or overriding human approval.
- **Architectural Layer:** Infrastructure Layer (`NexLudic.Infrastructure`).
- **Main Inputs & Outputs:**
  - *Inputs:* `ExecuteRollbackCommand`, pre-test file hash manifest.
  - *Outputs:* `RollbackExecutionResult`, post-teardown hash comparison report, crash diagnostics.
- **Authoritative Data Owned:** Rollback manifests (`rollback.json`), error logs, and system crash dumps.
- **Permitted Dependencies:** `NexLudic.Domain`, `System.IO`, PowerShell execution wrapper.
- **Forbidden Dependencies:** Destructive system cleanup outside project and game directories.
- **Security & Trust Boundary:** Boundary TB-03/04. Guarantees 100% clean rollback without administrative privileges.
- **Failure Modes:** Locked file during rollback, hash discrepancy detected after cleanup.
- **Required Diagnostics:** Executed cleanup commands, pre/post hash comparisons, orphaned file alerts.
- **Required Tests:** End-to-end integration tests verifying byte-for-byte pristine hash restoration.
- **Relevant Requirements & ADRs:** `REQ-F-010`, `REQ-NF-006`, `ADR-0008`, `RSK-12`.
- **Current Status:** **APPROVED (Core MVP).**
- **Boundary Rationale:** Guarantees platform reversibility and safe system recovery.

---

*End of Component Responsibility Catalog (NLE-CAT-COMP-001).*

