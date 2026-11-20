# STEP 71 ENDPOINT REQUIREMENTS TRACEABILITY MATRIX

**Document ID:** `NLE-MAT-STP-071`  
**Classification:** Requirements Traceability & Verification Matrix  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 71 of 100)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-EDR-001` through `NLE-CAT-EDR-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), System Audit Teams  

---

## 1. Overview & Verification Methodology

This matrix establishes 100% bidirectional traceability for all **34 Architectural Requirements (`EDR-REQ-001` through `EDR-REQ-034`)** governing Endpoint Discovery, Registration, and Identity Verification for the NexLudic Engine Live Bridge.

Each requirement maps directly to its authoritative specification document, implementing architectural component, verifying synthetic test scenarios (from `NLE-CAT-EDR-001`), and compliance status.

---

## 2. Requirements Traceability Matrix (`EDR-REQ-001` to `EDR-REQ-034`)

| Req ID | Requirement Title & Description | Authoritative Specification | Implementing Component | Synthetic Test Verification | Compliance Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`EDR-REQ-001`** | **Lifecycle Separation:** Discovery != Registration != Verification != Authorization != Compatibility != Session. | `NLE-SPC-EDR-001` §1 | Architecture Wide | Scenarios A, G, V, AE | **100% Specified** |
| **`EDR-REQ-002`** | **Untrusted Metadata Boundary:** All externally reported endpoint metadata treated as untrusted Zone 3 inputs. | `NLE-SPC-EDR-001` §2 | `EDM-01`, `ERM-02` | Scenarios D, Z, AD | **100% Specified** |
| **`EDR-REQ-003`** | **Non-Invasive Security Inviolability:** Zero port scanning, zero memory inspection, zero DLL injection (`BLK-SEC-01`). | `NLE-SPC-EDR-001` §2 | Subsystem Boundary | Scenarios C, AG, AH | **100% Specified** |
| **`EDR-REQ-004`** | **Windows Medium IL Enforcement:** All discovery and registration services run strictly at Medium IL (`DEC-002`). | `NLE-SPC-EDR-001` §2 | Engine Host | Scenario AH | **100% Specified** |
| **`EDR-REQ-005`** | **Fail-Closed Verification Semantics:** Missing, ambiguous, or expired evidence causes immediate fail-closed behavior. | `NLE-SPC-EDR-001` §2 | `IVS-03` | Scenarios G, R, V | **100% Specified** |
| **`EDR-REQ-006`** | **Endpoint Category Classification:** Endpoints strictly classified into 4 supported categories. | `NLE-CON-EID-001` §2 | `ERM-02` | Scenarios A, J | **100% Specified** |
| **`EDR-REQ-007`** | **Canonical Endpoint Identity Format:** Deterministic `provider:title-id:build-hash:instance-id` naming. | `NLE-CON-EID-001` §3 | `ERM-02` | Scenarios I, K, L | **100% Specified** |
| **`EDR-REQ-008`** | **Identity vs Locator Decoupling:** Logical identity is completely decoupled from transient transport locators. | `NLE-CON-EID-001` §4 | `TLA-09`, `ERM-02` | Scenarios S, T | **100% Specified** |
| **`EDR-REQ-009`** | **Sole Minting Authority:** Central Registration Manager (`ERM-02`) is sole authority permitted to assign IDs. | `NLE-CON-EID-001` §5 | `ERM-02` | Scenarios I, AD | **100% Specified** |
| **`EDR-REQ-010`** | **Core Identity Immutability:** `provider`, `title_id`, and `build_hash` are completely immutable once registered. | `NLE-CON-EID-001` §5 | `ERM-02` | Scenario N | **100% Specified** |
| **`EDR-REQ-011`** | **Bounded Discovery Scope:** Discovery queries strictly bounded to approved target game scopes; wildcards barred. | `NLE-POL-DSP-001` §1 | `EDM-01` | Scenario Y | **100% Specified** |
| **`EDR-REQ-012`** | **Approved Discovery Source Catalog:** Only 5 approved sources (`SRC-DSP-01..05`); unapproved sources dropped. | `NLE-POL-DSP-001` §2 | `EDM-01` | Scenario C | **100% Specified** |
| **`EDR-REQ-013`** | **Discovery Request Contract:** Structured request with target game, required category, and source whitelist. | `NLE-POL-DSP-001` §3 | `EDM-01` | Scenarios A, B, F | **100% Specified** |
| **`EDR-REQ-014`** | **Discovery Candidate Record Schema:** Immutable record capturing observed locator, claimed ID, and provenance. | `NLE-POL-DSP-001` §4 | `EDM-01` | Scenarios A, D | **100% Specified** |
| **`EDR-REQ-015`** | **Candidate Freshness Window:** Unregistered candidates expire after 30 seconds. | `NLE-POL-DSP-001` §5 | `EDM-01` | Scenario AH | **100% Specified** |
| **`EDR-REQ-016`** | **Registration Invariant:** Recording an endpoint in registry grants zero active bridge communication rights. | `NLE-CON-ERR-001` §1 | `ERM-02` | Scenarios G, AF | **100% Specified** |
| **`EDR-REQ-017`** | **Deterministic Registration FSM:** 9-state machine governing endpoint progression from requested to deregistered. | `NLE-CON-ERR-001` §2 | `ERM-02` | Scenarios G, P, AC | **100% Specified** |
| **`EDR-REQ-018`** | **Registration Request & Result Contracts:** Formally defined JSON schemas for registration inputs and outputs. | `NLE-CON-ERR-001` §3, §4 | `ERM-02` | Scenarios G, J, M | **100% Specified** |
| **`EDR-REQ-019`** | **Claimed vs Verified Identity Distinction:** Claims treated as untrusted until validated against evidence. | `NLE-POL-EIV-001` §2 | `IVS-03` | Scenarios G, L | **100% Specified** |
| **`EDR-REQ-020`** | **Four-Tier Identity Proofs:** OS Token SID, Ephemeral Secret Token, Build Digest, and Adapter Manifest. | `NLE-POL-EIV-001` §3 | `IVS-03` | Scenarios G, L, AH | **100% Specified** |
| **`EDR-REQ-021`** | **Endpoint Trust Classification:** Explicit trust classes (`UNVERIFIED`, `PROVISIONAL`, `VERIFIED`, `REVOKED`). | `NLE-POL-EIV-001` §4 | `IVS-03` | Scenarios G, P, AD | **100% Specified** |
| **`EDR-REQ-022`** | **Verification Record Schema:** Cryptographic proof evaluation audit entity with expiration timestamp. | `NLE-POL-EIV-001` §5 | `IVS-03` | Scenarios O, AH | **100% Specified** |
| **`EDR-REQ-023`** | **Monotonic Registration Revisioning:** Revision increments on updates; unversioned in-place mutation barred. | `NLE-POL-RRR-001` §1 | `ERM-02` | Scenarios M, T, W | **100% Specified** |
| **`EDR-REQ-024`** | **Environmental Invalidation Triggers:** Heartbeat loss, target file drift, or build patch suspends/invalidates record. | `NLE-POL-RRR-001` §2 | `CBC-01`, `ERM-02` | Scenarios O, P | **100% Specified** |
| **`EDR-REQ-025`** | **Revocation Cascade Semantics:** Immediate session termination, token nullification, and re-discovery block. | `NLE-POL-RRR-001` §3 | `CBC-01`, `ERM-02` | Scenarios P, Q | **100% Specified** |
| **`EDR-REQ-026`** | **Permanent Audit Trail Retention:** Deregistration clears active registry; audit records preserved permanently. | `NLE-POL-RRR-001` §4 | `TDJ-08` | Scenarios P, AH | **100% Specified** |
| **`EDR-REQ-027`** | **Protocol Version Validation:** Advertised wire protocol must match registered version (`NLE-WIRE-v1`). | `NLE-CON-PCM-001` §2 | `CSN-03` | Scenario K | **100% Specified** |
| **`EDR-REQ-028`** | **Five-Gate Capability Validation:** Syntax, schema dereferencing, directionality, rate limits, Phase 8 prereqs. | `NLE-CON-PCM-001` §3 | `CSN-03` | Scenarios U, W | **100% Specified** |
| **`EDR-REQ-029`** | **Privilege Non-Escalation Invariant:** Capability declarations convey zero execution rights; authorization required. | `NLE-CON-PCM-001` §5 | `ASG-05` | Scenario X | **100% Specified** |
| **`EDR-REQ-030`** | **Six-Class Conflict Taxonomy:** Idempotent duplicates, competing claimants, substitution, mutation, spoofing, re-claim. | `NLE-POL-ICI-001` §2 | `ERM-02` | Scenarios H, M, S, T | **100% Specified** |
| **`EDR-REQ-031`** | **Fail-Closed Claimant Quarantine:** Competing claimants to same identity are both quarantined immediately. | `NLE-POL-ICI-001` §3 | `ERM-02` | Scenario H | **100% Specified** |
| **`EDR-REQ-032`** | **AI Coding-Agent Subordination:** AI agent proposals treated as untrusted; self-authorization strictly barred. | `NLE-POL-DAP-001` §3 | Security Guard | Scenario AD | **100% Specified** |
| **`EDR-REQ-033`** | **Privacy & Data Minimization:** Strict data collection bounds; zero ambient scanning; secret redaction in logs. | `NLE-POL-DAP-001` §4 | `EDM-01`, `TDJ-08` | Scenarios Z, AH | **100% Specified** |
| **`EDR-REQ-034`** | **Resource Quotas & DoS Defenses:** Quotas on concurrency, timeouts, candidate counts, and metadata sizes. | `NLE-POL-DAP-001` §5 | Flow Controller | Scenarios AA, AB | **100% Specified** |
