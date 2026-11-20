# STEP 71 OPEN ARCHITECTURE DECISIONS REGISTER

**Document ID:** `NLE-REG-STP-071`  
**Classification:** Architectural Decision Records (ADRs)  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 71 of 100)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-EDR-001`, `DEC-002`, `BLK-SEC-01`, `ADR-BRG-001..010`  
**Downstream Consumed By:** Phase 9 Steps 72–78  

---

## 1. Overview

This register formally documents the ten fundamental architectural decisions (`ADR-EDR-001` through `ADR-EDR-010`) established during Step 71 for Endpoint Discovery, Registration, and Identity Verification.

---

## 2. Architectural Decision Records (`ADR-EDR-001` to `ADR-EDR-010`)

### `ADR-EDR-001`: Strict Separation of Endpoint Lifecycle Stages
- **Context:** Many distributed systems conflate discovering a network service with registering it or granting it permission to send RPCs.
- **Decision:** Enforce a strict multi-stage lifecycle: Discovery != Registration != Verification != Authorization != Compatibility != Session.
- **Rationale:** Prevents untrusted observed candidates from executing unapproved actions or claiming permissions before identity proof and authorization are evaluated.
- **Consequences:** Endpoints require explicit progression through each lifecycle gate before bridge communication can occur.

### `ADR-EDR-002`: Sole Canonical Identity Authority by Central Registration Manager
- **Context:** External mod scripts or transport listeners could attempt to assign their own canonical endpoint names.
- **Decision:** Mandate that the Central Registration Manager (`ERM-02`) is the sole authoritative entity permitted to assign or validate canonical identities.
- **Rationale:** External processes cannot be trusted to self-mint globally unique canonical IDs without causing naming collisions or spoofing other game adapters.
- **Consequences:** External candidates submit claimed identities, which are validated or minted by the engine.

### `ADR-EDR-003`: Bounded Discovery Scope vs Open Network/Process Scanning (`BLK-SEC-01`)
- **Context:** Automated discovery tools often use subnet port scanning or OS process enumeration to find running games.
- **Decision:** Strictly bar subnet scanning, port probing, and OS process enumeration. Discovery must use only approved, targeted sources (manifests, verified deployment records, user selection, and designated local pipe handshakes).
- **Rationale:** Scanning processes or subnets triggers anti-cheat false positives, violates user privacy, and creates network noise.
- **Consequences:** The engine only discovers endpoints that explicitly integrate via authorized mechanisms.

### `ADR-EDR-004`: Four-Tier Evidence-Based Identity Attestation
- **Context:** How does the engine verify that an endpoint is genuinely what it claims to be?
- **Decision:** Enforce a four-tier proof evaluation: OS Process SID attestation, Ephemeral 256-bit handshake secret token, Host executable build SHA-256 digest, and Adapter signed manifest digest.
- **Rationale:** No single evidence type is sufficient. Combining OS token security, ephemeral secrets, and file integrity digests provides robust defense-in-depth against impersonation.
- **Consequences:** Endpoints failing any mandatory proof tier are classified as `UNVERIFIED` and barred from session eligibility.

### `ADR-EDR-005`: Monotonic Registration Revisions and Immutability of Core Identity
- **Context:** Endpoints may need to update locators or capability declarations over time.
- **Decision:** Mandate monotonic registration revision numbers (`registration_revision`). Core identity fields (`provider`, `title_id`, `build_hash`) are completely immutable once registered.
- **Rationale:** Prevents in-place race conditions, guarantees auditability, and stops an endpoint from morphing its identity post-registration.
- **Consequences:** Any change in game executable build hash requires a new registration record.

### `ADR-EDR-006`: Fail-Closed Quarantine for Competing Identity Claimants (`CONF-02`)
- **Context:** What happens if two different processes claim the exact same canonical endpoint identity?
- **Decision:** Immediately quarantine both claimants and halt session eligibility for that identity until explicit user or administrative disambiguation.
- **Rationale:** Blindly picking the newest or first claimant opens an obvious race-condition impersonation vulnerability.
- **Consequences:** Prevents rogue processes from hijacking existing endpoint identities.

### `ADR-EDR-007`: Separation of Transport Locators from Logical Endpoint Identities
- **Context:** Should an endpoint be identified by its Named Pipe path or TCP port?
- **Decision:** Categorically separate logical canonical identity from physical transport locators.
- **Rationale:** Locators are transient and can change upon restart or port collision; logical identities represent durable components and game associations.
- **Consequences:** An endpoint can rebind to a new port or pipe without losing its historical verification and capability contracts.

### `ADR-EDR-008`: AI Coding-Agent Proposal Subordination & Zero Self-Authorization
- **Context:** AI coding agents (Phase 7) may participate in configuring mashups and suggesting adapters.
- **Decision:** Classify all AI-generated registration proposals as untrusted suggestions. AI agents cannot self-assign identities, grant trust, or open bridge sessions.
- **Rationale:** AI agents can hallucinate or be prompted to bypass security controls; deterministic policy validation must hold ultimate authority.
- **Consequences:** AI proposals must pass through the identical validation and authorization gates as untrusted external inputs.

### `ADR-EDR-009`: Ephemeral Token Freshness Window and Heartbeat Liveness Governance
- **Context:** How long does a verified registration remain fresh?
- **Decision:** Ephemeral tokens expire after 2 hours; active liveness requires heartbeats every 1000ms. Missing 3 heartbeats (3000ms) transitions registration to `SUSPENDED`.
- **Rationale:** Prevents orphaned registrations from lingering indefinitely when game processes crash or hang.
- **Consequences:** Suspended endpoints cannot route messages until liveness is restored or re-authentication occurs.

### `ADR-EDR-010`: Design-Only Specification Milestone (`P8-C02`)
- **Context:** Step 71 establishes endpoint discovery, registration, and identity verification specifications.
- **Decision:** Restrict Step 71 strictly to engineering specifications, domain models, contracts, and synthetic QA test designs. **Zero runtime code or live network/process probes are implemented.**
- **Rationale:** Strict adherence to project roadmap governance. Prevents premature, untested implementation before foundational contracts are fully approved.
- **Consequences:** Runtime endpoint discovery and handshake implementation begin in subsequent Phase 9 steps.
