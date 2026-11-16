# ADR-0010: Testing, Quality Assurance, and Multi-Tier Validation Strategy
## Document ID: NLE-ADR-0010
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Senior QA Automation Engineer & Platform Architect  

---

### 1. Context and Problem
NexLudic Engine must validate both its own internal platform code and the generated game mashups. Commercial PC games are rarely installed on developer CI environments or automated test runners. The platform requires a testing strategy that enables full automated testing of core orchestration logic without requiring installed games, while providing a rigorous multi-tier verification pipeline for user-executed mashups.

### 2. Requirements Addressed
- `REQ-TEST-001`: Mock Adapter Test Suite.
- `REQ-TEST-002`: Automated Mod Build Verification.
- `REQ-TEST-003`: Multi-Tier Verification Levels.
- `REQ-TEST-004`: Regression & Diff Testing.

### 3. Constraints and Assumptions
- Development workstations and CI pipelines cannot assume commercial games are installed.
- Compilation success does not prove gameplay functionality.

### 4. Options Considered
- **Option 1: Multi-Tier Verification Architecture with First-Class Mock Adapters.**
  - Platform testing: xUnit, FluentAssertions, Moq, and Mock Game Adapters.
  - Mashup validation: 4-Tier pipeline (Build OK -> Launch OK -> Interaction OK -> Session Stable).
- **Option 2: Relying Solely on In-Game Testing.** Untestable in CI; requires human tester playing games for every commit.
- **Option 3: Pure Static Code Analysis.** Fails to catch runtime IPC desynchronization or game process crashes.

### 5. Evaluation Criteria
- Automated test coverage in headless environments.
- Protection against false-positive success claims.
- Regression detection across iterative AI code patches.

### 6. Evidence and Source Links
- Step 1 Mandate (§2.6 Testing & Validation; §7 Requirements Register).
- Step 2 Product Scope (`MVP_AND_RELEASE_BOUNDARIES.md`).

### 7. Decision
NexLudic Engine adopts a **Dual-Domain Testing & Multi-Tier Validation Strategy**:
1. **Core Platform Test Suite:**
   - Framework: **xUnit** with **FluentAssertions** and **Moq**.
   - **Mock Game Adapters (`MockGameAdapter`):** Reusable in-memory and out-of-process test doubles that simulate game discovery, process heartbeats, named pipe event streams, and build scripts. Enables 100% automated CI execution without game dependencies.
2. **Mashup Verification Pipeline (4 Explicit Tiers):**
   - **Tier 1 (Build OK):** Automated headless compiler invocation capturing exit codes and compiler errors.
   - **Tier 2 (Launch OK):** Automated or guided process boot monitoring; verifying process PID stability for >=30 seconds.
   - **Tier 3 (Interaction OK):** In-game mechanic verification; assertion checklist for player actions (web-swinging, shooting).
   - **Tier 4 (Session Stable):** Sustained 10-minute gameplay soak test measuring frame times, memory growth, and IPC latency.

### 8. Rationale
Completely decouples platform software quality from commercial game licensing constraints. Enables robust automated regression testing on any developer workstation while enforcing strict, honest verification tiers for end-users before a mashup is declared stable.

### 9. Advantages
- Zero game dependencies required to run unit and integration tests.
- Transparent progress reporting; users are never misled into believing a mashup works merely because code compiled.
- Automated regression detection catching breaking changes introduced by iterative AI patches.

### 10. Disadvantages and Risks
- Tier 3 and Tier 4 for closed games require guided manual user checklists when headless automation is impossible.

### 11. Consequences for Other Components
The `NexLudic.Core` layer will define `IVerificationRunner` and implement automated Tier 1 and Tier 2 checkers.

### 12. Dependencies
- `xunit`, `xunit.runner.visualstudio`, `FluentAssertions`, `Moq`.

### 13. Conditions That Would Invalidate the Decision
Evidence that mock adapters fail to represent real-world game IPC behavior.

### 14. Follow-Up Work
Author the base Mock Adapter library and Tier 1 build runner in Phase 3.

### 15. Owner Approval Required
Pending Project Owner formal approval.

