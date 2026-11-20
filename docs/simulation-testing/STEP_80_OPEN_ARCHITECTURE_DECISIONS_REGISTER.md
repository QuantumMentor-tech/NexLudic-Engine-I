# STEP 80 OPEN ARCHITECTURE DECISIONS REGISTER

**Document Identifier:** `NLE-REG-STP-080`  
**Phase:** Phase 10 of 12 — Simulation, Automated Testing & QA  
**Roadmap Position:** Step 80 of 100  
**Status:** ALL DECISIONS EVALUATED AND ACCEPTED  
**Authoring Roles:** Principal Software Architect, Senior Distributed-Systems Engineer  

---

## 1. Architectural Decisions Summary

| ADR ID | Decision Title | Decision Status | Summary of Resolution |
| :--- | :--- | :---: | :--- |
| **ADR-COM-001** | Hub-and-Spoke In-Memory Routing | **ACCEPTED** | `CentralMessageRouter` (`CMR-02`) routes all inter-endpoint traffic. No direct P2P. |
| **ADR-COM-002** | Seven-Stage Validation Pipeline | **ACCEPTED** | Envelope, Session, Sender, Recipient, Topic, Digest, TTL verified sequentially. |
| **ADR-COM-003** | SHA-256 Payload Digest Checking | **ACCEPTED** | In-memory SHA-256 hashing guarantees bit-for-bit payload integrity (`ERR_PME_003`). |
| **ADR-COM-004** | Monotonic Sequence Numbers per Sender| **ACCEPTED** | Sequence numbers strictly monotonic; duplicates & regressions dropped (`ERR_PME_004`). |
| **ADR-COM-005** | Declarative Fault Injection | **ACCEPTED** | `CommunicationFaultConfig` defines drops, delays, rejections, disconnects deterministically. |
| **ADR-COM-006** | Transport ACK vs Operation Apply | **ACCEPTED** | Strict separation: ACK confirms envelope receipt, NOT game state mutation. |
| **ADR-COM-007** | Uncertain State Representation | **ACCEPTED** | Absent ACK results in `MessageLifecycleState.Uncertain` to prevent unsafe replay. |
| **ADR-COM-008** | Correlation ID RPC Tracking | **ACCEPTED** | `RequestResponseCorrelator` maps requests to responses with virtual clock deadlines. |
| **ADR-COM-009** | Session Epoch Invalidation | **ACCEPTED** | `SimulatedSession.AdvanceEpoch()` invalidates stale in-flight messages immediately. |
| **ADR-COM-010** | Non-Invasive Execution Boundary | **ACCEPTED** | Strict Medium IL; `DisallowProcessMemoryAccess = true`; zero live process hooking. |
