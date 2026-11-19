# NexLudic Engine: Provider Registry Synthetic Scenarios & Test Plan

**Document ID:** `NLE-CAT-APS-001`  
**Phase:** 7 of 12 (AI Coding-Agent Orchestration)  
**Step:** 54 of 100  
**Classification:** Quality Assurance & Synthetic Conformance Test Plan  
**Status:** IMPLEMENTATION-READY SPECIFICATION  

---

## 1. Test Architecture & Conformance Baseline

This catalog establishes 20 comprehensive synthetic verification scenarios (Scenarios A through T) designed to test every facet of the **Provider Registry, Capability Profiles & Eligibility Matching Engine**.

> **NOTICE: SYNTHETIC AND ILLUSTRATIVE SPECIFICATION**  
> All entity identifiers, model tags, provider names, and test payloads in these scenarios (e.g., `prov-synth-anthropic`, `mod-synth-claude`, `agt-synth-coder`) are fictional test fixtures. They do not connect to live third-party APIs, do not invoke models on host hardware, and do not execute real software during this documentation milestone.

---

## 2. The 20 Synthetic Scenarios (Scenarios A through T)

### Scenario A: Basic Eligible Candidate
- **Task Requirement:** Normalized task `tsk-synth-001` (`DOCUMENTATION_AUTHORING`). Requires `CAP_DOC_AUTHORING`, `CAP_TOOL_FS_READ`, `CAP_TOOL_FS_WRITE_DIFF`. Sensitivity: `PUBLIC_SPECIFICATION`.
- **Candidate Tuple:** $\langle \text{prov-synth-anthropic}, \text{mod-synth-claude-3-7}, \text{agt-doc-author-v1}, \text{env-desktop-medium-il} \rangle$.
- **Evidence:** Active conformance test fixture `ev-conf-doc-001` passed 3 days ago.
- **Evaluation:** All 9 hard predicates pass.
- **Expected Outcome:** **`ELIGIBLE`**. Candidate enters the eligible candidate pool for Step 55.

---

### Scenario B: Missing Required Capability
- **Task Requirement:** Task `tsk-synth-002` (`CODE_MODIFICATION`). Requires `CAP_SOURCE_EDIT_DIFF` and `CAP_CODE_GEN_CSHARP`.
- **Candidate Tuple:** $\langle \text{prov-synth-local}, \text{mod-synth-chat-7b}, \text{agt-chat-assistant-v1}, \text{env-desktop-medium-il} \rangle$.
- **Profile:** Possesses `CAP_CODE_GEN_CSHARP` but lacks `CAP_SOURCE_EDIT_DIFF` (can only generate conversational blocks, not unified diffs).
- **Evaluation:** Fails `PRED_05_CAPABILITIES_VERIFIED`.
- **Expected Outcome:** **`INELIGIBLE_MISSING_CAPABILITY`** (`ERR_CAP_MISSING_MANDATORY`).
- **Negative Invariant:** Matcher does not accept conversational code blocks as a substitute for structured diff generation.

---

### Scenario C: Declared but Unverified Capability
- **Task Requirement:** Task `tsk-synth-003` (`TEST_AUTHORING`). Requires `CAP_UNIT_TEST_AUTHORING`.
- **Candidate Tuple:** Candidate's profile lists `CAP_UNIT_TEST_AUTHORING` under `declared_capabilities`, but `verified_capabilities` is empty (no conformance test executed).
- **Evaluation:** Evaluated at Stage 07. Fails `PRED_05` because state is `DECLARED`, not `VERIFIED`.
- **Expected Outcome:** **`INELIGIBLE_MISSING_CAPABILITY`** (`ERR_CAP_UNVERIFIED_CLAIM`).
- **Remediation:** Conformance test fixture must be executed before agent can be matched.

---

### Scenario D: Capability Versus Authorization
- **Task Requirement:** Task `tsk-synth-004` is an architecture review (`ARCHITECTURE_REVIEW`). Authorization token explicitly sets `allow_write: false`.
- **Candidate Tuple:** Candidate agent possesses verified `CAP_SOURCE_EDIT_DIFF` and `CAP_TOOL_FS_WRITE_DIFF`.
- **Evaluation:** Stage 08 detects that candidate's active tool profile exposes write access while task token forbids writes.
- **Expected Outcome:** Candidate is evaluated as **`ELIGIBLE` for read-only tools**, but write tools are stripped from the active session envelope. If the agent harness requires ambient write access to function, it evaluates to **`INELIGIBLE_PERMISSION_SCOPE`**.

---

### Scenario E: Provider Supports Model, Agent Does Not Support Task
- **Task Requirement:** Task `tsk-synth-005` (`ADAPTER_DEVELOPMENT`). Requires `CAP_ADAPTER_WIRE_SERIALIZATION`.
- **Candidate Tuple:** Provider exposes `mod-synth-claude-3-7` (capable model), but selected agent is `agt-doc-author-v1` whose harness only includes markdown authoring tools.
- **Evaluation:** Agent implementation governs operational tool boundaries. Fails `PRED_05`.
- **Expected Outcome:** **`INELIGIBLE_MISSING_CAPABILITY`**. The model's raw power cannot override the agent harness's limitations.

---

### Scenario F: Conditional Tool Dependency
- **Task Requirement:** Task `tsk-synth-006` requires `CAP_TOOL_FS_WRITE_DIFF`.
- **Candidate Profile:** Profile specifies `CAP_TOOL_FS_WRITE_DIFF` as conditional upon `tool.git.diff` being available in the environment.
- **Environment:** `env-desktop-minimal` lacks `git.exe` in its system path.
- **Evaluation:** Stage 09 detects missing tool binary. Fails `PRED_06_TOOLS_AVAILABLE`.
- **Expected Outcome:** **`INELIGIBLE_ENVIRONMENT`** (`ERR_CAP_TOOL_UNAVAILABLE`).

---

### Scenario G: Evidence Expiration (TTL Exceeded)
- **Task Requirement:** Task `tsk-synth-007` (`CODE_MODIFICATION`).
- **Candidate Tuple:** Cloud API provider with evidence record `ev-conf-diff-001`.
- **Initial State:** Conformance test was executed 18 days ago. Verified TTL for public cloud endpoints is 14 days.
- **Evaluation:** Stage 11 detects $t_{\text{current}} > t_{\text{expiration}}$. Fails `PRED_08_EVIDENCE_FRESH`.
- **Expected Outcome:** **`INELIGIBLE_EVIDENCE`** (`ERR_CAP_EVIDENCE_EXPIRED`).

---

### Scenario H: Incompatible Data-Handling Policy
- **Task Requirement:** Task `tsk-synth-008` contains proprietary game binary dumps (`CONFIDENTIAL_GAME_ASSET`). Requires `ZERO_RETENTION_AIR_GAPPED`.
- **Candidate Tuple:** High-performing public cloud API (`prov-synth-cloud`, `data_processing_tier: "STANDARD_CLOUD"`).
- **Evaluation:** Stage 12 evaluates data residency. Fails `PRED_09_PRIVACY_COMPLIANT`.
- **Expected Outcome:** **`INELIGIBLE_PRIVACY_CONSTRAINT`** (`ERR_CAP_PRIVACY_VIOLATION`).
- **Negative Invariant:** Cloud agent is permanently excluded regardless of benchmark scores.

---

### Scenario I: Stale Environment Profile
- **Task Requirement:** Task requires compilation test in Windows Medium IL.
- **Candidate Tuple:** Environment was verified on Windows 11 Build 22621, but host has updated to Build 26100 with altered toolchains.
- **Evaluation:** Environment hash mismatch triggers `INV-DEC-07`.
- **Expected Outcome:** **`INELIGIBLE_ENVIRONMENT`** until host environment re-profiling is completed.

---

### Scenario J: Provider Suspension
- **Task Requirement:** Standard coding task.
- **Candidate Tuple:** Candidate was approved yesterday, but today provider endpoint was placed in `SUSPENDED` due to an unaddressed security advisory.
- **Evaluation:** Stage 03 inspects provider lifecycle. Fails `PRED_03_PROVIDER_APPROVED`.
- **Expected Outcome:** **`INELIGIBLE_PROVIDER_STATE`** (`ERR_CAP_PROVIDER_SUSPENDED`).

---

### Scenario K: Conflicting Evidence
- **Candidate Profile:** Provider documentation claims native tool-calling support, but local conformance test `ev-conf-tools-002` failed with schema parsing errors.
- **Evaluation:** Empirical primacy rule applies. Conformance failure overrides vendor documentation.
- **Expected Outcome:** `CAP_TOOL_EXEC_CMD` marked `UNSUPPORTED`; candidate evaluated as **`INELIGIBLE_MISSING_CAPABILITY`**.

---

### Scenario L: Unverified Provider Claim
- **Candidate Profile:** Vendor marketing claims: "State-of-the-art 100% autonomous software development on any repo."
- **Registry State:** Zero empirical tests exist in the evidence repository.
- **Evaluation:** Claims reside at Tier 6. Fails `PRED_05`.
- **Expected Outcome:** **`INELIGIBLE_MISSING_CAPABILITY`** (`ERR_CAP_UNVERIFIED_CLAIM`).

---

### Scenario M: Zero Eligible Candidates
- **Task Requirement:** Complex task requiring both `CAP_CODE_GEN_RUST` and `ZERO_RETENTION_AIR_GAPPED` local execution.
- **Registry State:** Local model supports only C#; cloud model supports Rust but violates air-gapped policy.
- **Evaluation:** All candidate tuples fail either `PRED_05` or `PRED_09`.
- **Expected Outcome:** **`INELIGIBLE_MISSING_CAPABILITY`** (`ERR_CAP_ZERO_CANDIDATES`). System emits full diagnostic without relaxing privacy or capability constraints.

---

### Scenario N: Ranking Temptation (Security vs. Cost)
- **Candidate 1:** Extremely fast and inexpensive ($0.001/1k tokens), but its provider retains prompts for training (violates `PRED_09`).
- **Candidate 2:** More expensive cloud model with verified zero-retention enterprise contract.
- **Evaluation:** Candidate 1 is eliminated at Stage 12. Only Candidate 2 reaches `ELIGIBLE`.
- **Expected Outcome:** Candidate 1 is barred from ranking. Preference scoring is applied *only* to Candidate 2.

---

### Scenario O: Profile Tampering
- **Attack Vector:** An untrusted user script manually edits `provider-registry-v1.json` on disk to set `CAP_TOOL_EXEC_CMD` to `VERIFIED` without a valid evidence record.
- **Evaluation:** Stage 06 validates digital signatures and cross-references evidence hash. Evidence record does not exist.
- **Expected Outcome:** Matcher flags tampering; entry transitions to `SUSPENDED`; returns **`EVALUATION_FAILED`**.

---

### Scenario P: Multiple Eligible Candidates
- **Task Requirement:** Standard documentation authoring task.
- **Candidates:** Candidate A (Local Ollama), Candidate B (Anthropic Claude), Candidate C (OpenAI GPT-4o) all satisfy all 9 hard predicates.
- **Evaluation:** All three receive outcome `ELIGIBLE`.
- **Expected Outcome:** Matcher outputs an **Eligible Candidate Set** containing all three tuples. Matcher halts and yields to Step 55 for selection.

---

### Scenario Q: Task Revision Invalidates Decision
- **Initial State:** Candidate evaluated as `ELIGIBLE` for Task Rev 1.0.0.
- **Trigger:** Task owner edits requirements, adding `CAP_ADAPTER_WIRE_SERIALIZATION` (Rev 1.1.0).
- **Evaluation:** Invalidation trigger `INV-DEC-01` fires. Previous decision is voided.
- **Expected Outcome:** Task is re-matched against Rev 1.1.0; candidate lacking wire serialization is marked **`INELIGIBLE_MISSING_CAPABILITY`**.

---

### Scenario R: Required Capability Has No Verification Method
- **Task Requirement:** Task demands an experimental capability: `CAP_AUTONOMOUS_GAME_INTEGRATION_TEST`.
- **Registry State:** The platform has not yet authored an approved conformance test fixture for this capability.
- **Evaluation:** No valid evidence can be generated.
- **Expected Outcome:** **`BLOCKED_BY_DEPENDENCY`** (`ERR_CAP_UNVERIFIED_CLAIM`). The task cannot proceed until an approved test fixture is established.

---

### Scenario S: Upstream Gate Is Blocked (Step 51 Phase 6 Unverified)
- **Task Requirement:** Task requests runtime implementation of a Step 49 mashup integration plan (`ACCEPTED_FOR_PLANNING`).
- **Repository State:** Step 51 readiness review is missing; Phase 6 gate is `UNVERIFIED`.
- **Evaluation:** Evaluated at Stage 02 (`PRED_02_UPSTREAM_GATES_SATISFIED`).
- **Expected Outcome:** **`BLOCKED_BY_DEPENDENCY`** (`ERR_PRECONDITION_PHASE6_UNVERIFIED`). Candidate evaluation is suspended until Phase 6 is approved.

---

### Scenario T: Unauthorized Operation Concealed in Broad Capability
- **Candidate Profile:** Candidate declares broad "Game Modding Capability".
- **Operation Proposal:** Agent attempts to load a kernel driver to hook DirectX memory.
- **Evaluation:** Stage 08 asserts anti-cheat dominance (`BLK-SEC-01`) and Medium IL (`DEC-002`).
- **Expected Outcome:** Hard security veto: **`INELIGIBLE_POLICY_CONFLICT`** (`ERR_CAP_SECURITY_VETO`).
