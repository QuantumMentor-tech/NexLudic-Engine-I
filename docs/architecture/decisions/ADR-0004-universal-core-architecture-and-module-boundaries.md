# ADR-0004: Universal Core Architecture and Clean Module Boundaries
## Document ID: NLE-ADR-0004
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Principal Software Architect  

---

### 1. Context and Problem
NexLudic Engine coordinates diverse subsystems: game discovery, natural language intake, blueprint compilation, AI agent orchestration, file sandboxing, compiler invocation, and IPC bridging. Without strict module boundaries, game-specific logic or AI execution code could leak into unrelated modules, creating a fragile monolithic spaghetti architecture.

### 2. Requirements Addressed
- `REQ-CORE-002`: Pluggable Engine Architecture.
- `REQ-CORE-004`: Project Lifecycle Management.
- `AC-06`: Universal Capability Boundaries.

### 3. Constraints and Assumptions
- Core platform must remain completely decoupled from any single game engine or mod loader.
- Unnecessary distributed architecture (microservices) must be avoided per Section 4.2.

### 4. Options Considered
- **Option 1: Clean Modular Monolith with Inverted Dependencies (Ports & Adapters).**
- **Option 2: Microservices Architecture.** Multiple local micro-daemons communicating over HTTP/gRPC. Excessive process overhead and networking complexity.
- **Option 3: Layered Monolith without strict boundaries.** High risk of architectural degradation.

### 5. Evaluation Criteria
- Separation of concerns.
- Testability without live game or compiler dependencies.
- Simplicity and maintainability.

### 6. Evidence and Source Links
- Step 4 Research (`UNIVERSAL_ADAPTER_CAPABILITY_BOUNDARIES.md`).
- Established Domain-Driven Design (DDD) and Clean Architecture standards.

### 7. Decision
NexLudic Engine adopts a **Clean Modular Monolith** architecture based on Domain-Driven Design principles with four distinct horizontal layers and strict dependency inversion:
1. **Presentation Layer (`NexLudic.App`):** WPF Views, ViewModels, UI navigation, and user prompts.
2. **Application Core Layer (`NexLudic.Core`):** Workflow orchestrators, Blueprint compiler, Strategy evaluator, and test runners.
3. **Domain Layer (`NexLudic.Domain`):** Pure entities, value objects, adapter trait contracts (`IGameAdapter`), and validation rules.
4. **Infrastructure Layer (`NexLudic.Infrastructure`):** FileSystem staging sandbox, NamedPipe IPC, Process runners, and AI agent connectors.

### 8. Rationale
The Clean Modular Monolith guarantees that the Domain and Core layers remain 100% agnostic of game-specific mod loaders, Windows UI frameworks, or specific AI providers. All external interactions are mediated through interfaces (Ports).

### 9. Advantages
- Single executable solution; simple build, debug, and packaging.
- High testability; all core services can be unit-tested using in-memory mock adapters.
- Extensible; new capabilities are added as clean plugins or infrastructure adapters.

### 10. Disadvantages and Risks
- Requires architectural discipline to prevent developers from bypassing interface boundaries.

### 11. Consequences for Other Components
All future solution projects will adhere to strict project reference rules (Presentation -> Core -> Domain; Infrastructure -> Domain).

### 12. Dependencies
- Standard .NET 8.0 Class Libraries.

### 13. Conditions That Would Invalidate the Decision
Evidence demonstrating that in-process execution cannot satisfy performance or safety constraints.

### 14. Follow-Up Work
Structure the C# project solution according to these module boundaries in Phase 2.

### 15. Owner Approval Required
Pending Project Owner formal approval.

