# LIVE BRIDGE ENDPOINT DISCOVERY & REGISTRATION SYNTHETIC TEST PLAN

**Document ID:** `NLE-CAT-EDR-001`  
**Classification:** Quality Assurance & Synthetic Test Catalog  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 71 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-EDR-001` through `NLE-POL-DAP-001`  
**Downstream Consumed By:** Step 78 (Phase 9 Readiness Gate), Test Automation Harness  

---

## 1. Testing Strategy & Synthetic Execution Boundary

In accordance with the **Design-Only Milestone Constraint (`P8-C02`)**, zero live game processes are hooked, and zero network probes are executed. 

All verification is conducted via **Deterministic Synthetic Test Fixtures**, exercising the complete state machine, schema validators, conflict detectors, and authorization gates against mock candidates and simulated adapters.

---

## 2. Test Scenario Matrix (Scenarios A through AH — 34 Scenarios)

```
+-------+--------------------------------------+--------------------+---------------------+
| Group | Domain Area                          | Scenarios          | Scenario Count      |
+-------+--------------------------------------+--------------------+---------------------+
| 1     | Discovery & Candidate Handling       | Scenarios A - F    | 6 Scenarios         |
| 2     | Identity Verification & Registration | Scenarios G - N    | 8 Scenarios         |
| 3     | Trust & Revocation                   | Scenarios O - T    | 6 Scenarios         |
| 4     | Capabilities & Compatibility         | Scenarios U - X    | 4 Scenarios         |
| 5     | Security, Privacy & Reliability      | Scenarios Y - AH   | 10 Scenarios        |
+-------+--------------------------------------+--------------------+---------------------+
| TOTAL | Comprehensive Test Catalog           | Scenarios A - AH   | 34 Test Scenarios   |
+-------+--------------------------------------+--------------------+---------------------+
```

---

## 3. Detailed Synthetic Scenario Specifications

### Group 1: Discovery & Candidate Handling (Scenarios A – F)

#### Scenario A: Single Valid Synthetic Candidate
- **Objective:** Verify candidate record creation from an approved discovery source.
- **Fixture:** `SRC-DSP-01` returns 1 well-formed candidate matching requested target game.
- **Assertion:** Candidate record created with status `CANDIDATE_OBSERVED`; candidate is NOT registered or trusted.

#### Scenario B: No Candidates Found in Scope
- **Objective:** Verify safe handling when discovery finds zero matching endpoints.
- **Fixture:** Query for non-existent game title `custom:uninstalled-game`.
- **Assertion:** Discovery returns empty candidate list without error; reports search boundaries honestly.

#### Scenario C: Candidate from Unapproved Discovery Source
- **Objective:** Verify rejection of candidates originating from unapproved sources.
- **Fixture:** Mock submission tagged with `source_id: "SRC_ARBITRARY_SUBNET_SCAN"`.
- **Assertion:** Candidate dropped immediately; security event logged in `TDJ-08`; zero state change.

#### Scenario D: Malformed Candidate Metadata Rejection
- **Objective:** Verify schema validation catches invalid or missing fields.
- **Fixture:** Candidate JSON payload missing mandatory `claimed_identity.provider` field.
- **Assertion:** Validation fails with `ERR_DSP_SCHEMA_VIOLATION`; candidate rejected.

#### Scenario E: Multiple Discovered Candidates Requiring Disambiguation
- **Objective:** Verify engine does not guess when multiple candidates match a query.
- **Fixture:** Discovery returns 2 valid candidates with differing `instance_id` values.
- **Assertion:** Both candidates recorded; status flags ambiguity; engine prompts for user/policy disambiguation.

#### Scenario F: Cancelled Discovery Operation
- **Objective:** Verify clean resource release when a discovery request is aborted mid-flight.
- **Fixture:** Caller issues `CancellationToken.Cancel()` 50ms into a 3000ms query.
- **Assertion:** Operation completes with `DISCOVERY_CANCELLED`; zero orphaned handles or memory leaks.

---

### Group 2: Identity Verification & Registration (Scenarios G – N)

#### Scenario G: Identity Claim Lacking Supporting Evidence
- **Objective:** Verify candidate without proofs remains unverified upon registration.
- **Fixture:** Registration request submitted with zero `identity_proof_references`.
- **Assertion:** Record registered with state `REGISTERED_UNVERIFIED` and trust `UNVERIFIED`; session eligibility denied.

#### Scenario H: Two Candidates Claiming Identical Canonical Identity
- **Objective:** Verify immediate detection of competing claimants (`CONF-02`).
- **Fixture:** Candidates from PID 101 and PID 102 both claim `mojang:minecraft-java:99b1c43f01:inst-01`.
- **Assertion:** Both records placed in `QUARANTINED` state; conflict logged; session eligibility blocked.

#### Scenario I: New Endpoint Requests Identity Assignment
- **Objective:** Verify engine mints canonical ID following strict format rules.
- **Fixture:** Valid registration request providing verified game identity and component digest.
- **Assertion:** Engine assigns canonical ID matching regex; registers record under Revision 1.

#### Scenario J: Valid Identity with Unauthorized Registration Request
- **Objective:** Verify registration is blocked if caller lacks registration authority.
- **Fixture:** Registration request submitted by untrusted guest script context.
- **Assertion:** Request rejected with `ERR_AUTH_REGISTRATION_DENIED`; record not registered.

#### Scenario K: Registration Metadata Containing Unsupported Protocol Version
- **Objective:** Verify rejection of unsupported wire protocol versions.
- **Fixture:** Candidate advertises `advertised_protocol_version: "NLE-WIRE-v99"`.
- **Assertion:** Registration rejected with `ERR_UNSUPPORTED_PROTOCOL_VERSION`.

#### Scenario L: Claimed Game Release Inconsistent with Verified Evidence
- **Objective:** Verify detection of build hash tampering.
- **Fixture:** Claimed build hash `99b1c43f01`, but binary file scan indicates hash `a8f9c12e5d`.
- **Assertion:** Verification fails; candidate flagged `ERR_BUILD_HASH_MISMATCH`; registration rejected.

#### Scenario M: Idempotent Registration for Identical Revision
- **Objective:** Verify identical duplicate registration requests succeed idempotently.
- **Fixture:** Identical registration request re-submitted for an active endpoint record.
- **Assertion:** Returns existing registration record (Revision 1); no duplicate entry or state mutation.

#### Scenario N: Endpoint Requests Unapproved Core Identity Change
- **Objective:** Verify core identity fields are immutable across revisions.
- **Fixture:** Active record attempts update changing `provider` from `mojang` to `valve`.
- **Assertion:** Request rejected with `ERR_CORE_IDENTITY_IMMUTABLE`; revision increment refused.

---

### Group 3: Trust & Revocation (Scenarios O – T)

#### Scenario O: Stale Identity Evidence Suspends Registration
- **Objective:** Verify expiration of ephemeral token moves endpoint to `SUSPENDED`.
- **Fixture:** Active session running past 2-hour token validity window without token renewal.
- **Assertion:** Registration transitions from `REGISTERED_VERIFIED` to `SUSPENDED`; message routing halts.

#### Scenario P: Administrative Revocation Cascade
- **Objective:** Verify administrative revocation strips trust and terminates sessions.
- **Fixture:** User triggers `RevokeEndpoint(endpoint_id)` via management interface.
- **Assertion:** State -> `REVOKED`; active bridge session receives `SESSION_TERMINATE`; handles closed.

#### Scenario Q: Revoked Endpoint Re-Discovery Block
- **Objective:** Verify revoked instance ID cannot silently re-enter via discovery.
- **Fixture:** Discovery scanner encounters instance ID that was previously revoked.
- **Assertion:** Candidate flagged `REVOKED_IDENTITY_BLOCKED`; rejected before registration.

#### Scenario R: Identity Verification Service Unavailable
- **Objective:** Verify fail-closed behavior when verification service is offline.
- **Fixture:** Mock `IVS-03` throws timeout during proof evaluation.
- **Assertion:** Endpoint transitions to `PENDING_VALIDATION`; trust NOT granted; session blocked.

#### Scenario S: Identity Substitution on Stable Locator Detection
- **Objective:** Verify detection of process replacement on an existing pipe (`CONF-03`).
- **Fixture:** New process (PID 9999) attempts to communicate on pipe previously authenticated by PID 1000.
- **Assertion:** Coordinator detects PID change; severs pipe immediately; requires full re-handshake.

#### Scenario T: Legitimate Endpoint Locator Mutation
- **Objective:** Verify seamless locator rebind when authenticated token is preserved (`CONF-04`).
- **Fixture:** Verified endpoint reconnects on new dynamic port, presenting valid ephemeral secret token.
- **Assertion:** Token validated; registration updated to Revision 2; locator updated; session restored.

---

### Group 4: Capabilities & Compatibility (Scenarios U – X)

#### Scenario U: Endpoint Declares Unsupported Capability
- **Objective:** Verify undeclared or unknown capabilities are excluded from registration.
- **Fixture:** Endpoint declares `capability_id: "magic.spell.cast"` not found in schema catalog.
- **Assertion:** Capability rejected with `ERR_CAP_SCHEMA_NOT_FOUND`; excluded from active contract.

#### Scenario V: Protocol Valid but Cross-Game Compatibility Unresolved
- **Objective:** Verify protocol validity does not imply cross-game semantic compatibility.
- **Fixture:** Game A and Game B both verified on `NLE-WIRE-v1`, but no semantic mapping exists.
- **Assertion:** Both endpoints reach `REGISTERED_VERIFIED`, but `SEG-04` denies session eligibility.

#### Scenario W: Runtime Capability Revision Invalidation
- **Objective:** Verify updating capability declarations invalidates existing session eligibility.
- **Fixture:** Registered endpoint updates capability rate limits; registration increments to Revision 2.
- **Assertion:** Session eligibility revoked; requires re-negotiation under new revision contract.

#### Scenario X: Current Registration Lacking Capability Authorization Grant
- **Objective:** Verify registration of a capability does not bypass authorization check.
- **Fixture:** Endpoint registered with `inventory.transfer`, but user denied inventory permission.
- **Assertion:** Topic blocked by `ASG-05`; message frames on this topic rejected with `ERR_ACCESS_DENIED`.

---

### Group 5: Security, Privacy & Reliability (Scenarios Y – AH)

#### Scenario Y: Broad Discovery Scope Enforcement
- **Objective:** Verify discovery scope boundary cannot be expanded by external callers.
- **Fixture:** Request specifies target game `mojang:minecraft-java`, but tries to pass wildcard `*`.
- **Assertion:** Wildcard rejected; scope strictly bounded to explicitly approved target game.

#### Scenario Z: Passive Treatment of Execution Instructions in Metadata
- **Objective:** Verify metadata fields containing shell commands are treated strictly as passive text.
- **Fixture:** Candidate display name contains `; rm -rf /; calc.exe; <script>alert(1)</script>`.
- **Assertion:** String sanitized/escaped; zero execution; passed as pure text data.

#### Scenario AA: Oversized Discovery Record Set Truncation
- **Objective:** Verify enforcement of candidate result set quotas.
- **Fixture:** Mock discovery source returns 150 candidate records (ceiling: 50).
- **Assertion:** Result truncated to exactly 50 candidates; `WARN_CANDIDATE_SET_TRUNCATED` logged.

#### Scenario AB: Concurrent Registration Race on Same Identity
- **Objective:** Verify optimistic concurrency control prevents race conditions.
- **Fixture:** Two threads attempt concurrent registration updates to Revision 1 of same endpoint.
- **Assertion:** Thread 1 succeeds (Revision 2); Thread 2 receives `ERR_CONCURRENCY_CONFLICT` and retries.

#### Scenario AC: Interrupted Registration Workflow Recovery
- **Objective:** Verify registration interrupted before verification leaves clean state.
- **Fixture:** Process crash simulated during `PENDING_VALIDATION` state.
- **Assertion:** On restart, pending record marked `REGISTRATION_ABORTED`; zero orphaned active records.

#### Scenario AD: AI-Generated Registration Proposal Scope Guard
- **Objective:** Verify AI-generated registration proposals cannot self-authorize.
- **Fixture:** AI agent submits registration payload attempting to set `trust_classification: VERIFIED`.
- **Assertion:** System policy resets trust to `UNVERIFIED`; deterministic validation enforced.

#### Scenario AE: Registered Endpoint Ineligible for Bridge Session
- **Objective:** Verify clear separation between registered state and session eligibility.
- **Fixture:** Endpoint successfully registered, but target game process is not running.
- **Assertion:** Registration remains valid in registry; session eligibility reported as `FALSE (PROCESS_NOT_FOUND)`.

#### Scenario AF: Registration Reporting Without False Communication Claims
- **Objective:** Verify query reports registration status without claiming bridge connectivity.
- **Fixture:** Query endpoint status via API.
- **Assertion:** Response returns `REGISTERED_VERIFIED`; explicitly flags `ACTIVE_SESSION: FALSE`.

#### Scenario AG: Rejection of Prohibited Runtime Techniques
- **Objective:** Verify rejection of integration candidate requiring memory injection.
- **Fixture:** Candidate manifest specifies `integration_technique: "DLL_INJECTION_DETOURS"`.
- **Assertion:** Candidate rejected immediately under `BLK-SEC-01`; flagged `PROHIBITED_INTEGRATION_TECHNIQUE`.

#### Scenario AH: Complete End-to-End Synthetic Discovery & Registration Lifecycle
- **Objective:** Traverse full synthetic lifecycle from discovery to registration, verification, and revocation.
- **Fixture:** Synthetic mock adapter and channel emulator.
- **Assertion:** Clean state progression: `CANDIDATE` -> `REGISTERED_UNVERIFIED` -> `REGISTERED_VERIFIED` -> `ELIGIBLE` -> `REVOKED` -> `DEREGISTERED`. Zero live processes or sockets invoked.
