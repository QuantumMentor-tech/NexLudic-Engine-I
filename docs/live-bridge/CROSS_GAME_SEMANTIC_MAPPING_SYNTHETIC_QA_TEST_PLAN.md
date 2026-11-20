# CROSS-GAME SEMANTIC MAPPING SYNTHETIC QA TEST PLAN

**Document Identifier:** `NLE-CAT-CSM-001`  
**Lifecycle Stage:** Authoritative Test Plan  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Synthetic Verification  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-STD-001` (Step 74), `NLE-POL-MIR-001` (Step 74)  
**Downstream Dependents:** Step 75 (Continuous State Synchronization), Step 76 (Gameplay Orchestration)

---

## 1. Scope, Methodology and Test Harness Architecture

This test plan specifies **39 synthetic test scenarios (Scenarios A through AM)** covering all structural, physical, numerical, logical, and security dimensions of the Cross-Game Semantic Mapping engine.

### Synthetic Test Harness Standard
- All tests execute within an isolated in-memory simulation harness.
- **Zero live game dependencies:** No running game executables, no graphics hardware requirements, no live network sockets.
- Inputs are mock JSON/Binary payloads simulating UE5, Unity, Godot, and custom engine telemetry.
- Deterministic random seeds (`SEED = 0x5E4A71C`) guarantee bitwise test repeatability.

---

## 2. Test Scenarios Matrix (Scenarios A through AM)

### Domain 1: Structural & Concept Descriptor Validation (Scenarios A – G)
- **Scenario A (Valid DCD Ingestion):** Ingest valid UE5 actor transform descriptor; verify schema passes validation, hash matches, state becomes `APPROVED`.
- **Scenario B (Malformed JSON Schema):** Ingest DCD with missing `required` block; verify `ERR_SEM_SRC_SCHEMA_VIOLATION` is emitted.
- **Scenario C (Corrupted Integrity Hash):** Alter one character in DCD property type while preserving original `integrity_hash`; verify rejection at boot.
- **Scenario D (SemVer Increment Rule):** Attempt to publish breaking schema change under minor version increment; verify rejection.
- **Scenario E (Invalid Concept URI Format):** Provide concept URI with uppercase letters or illegal characters; verify regex validation failure.
- **Scenario F (Duplicate Property Registration):** Ingest DCD containing conflicting duplicate property names; verify parse failure.
- **Scenario G (Unsupported Primitive Type):** Ingest DCD with undefined datatype `FLOAT128`; verify rejection.

### Domain 2: Coordinate & Unit Transformation (Scenarios H – N)
- **Scenario H (UE5 to Canonical Transform):** Feed UE5 LH Z-up vector `[100, 200, 300]` (cm); verify transformed to Canonical RH Y-up `[2.0, 3.0, 1.0]` (m).
- **Scenario I (Unity to Canonical Transform):** Feed Unity LH Y-up vector `[10, 20, 30]` (m); verify transformed to Canonical RH Y-up `[-10, 20, 30]` (m).
- **Scenario J (Round-Trip Transform Invariance):** Execute UE5 -> Canonical -> UE5 transform cycle; verify residual error $\|\mathbf{p} - \mathbf{p}'\| < 10^{-6}$ m.
- **Scenario K (Quaternion Renormalization):** Feed unnormalized quaternion with norm $1.02$; verify normalized to norm $1.000000 \pm 10^{-7}$.
- **Scenario L (Degenerate Quaternion):** Feed zero quaternion `[0, 0, 0, 0]`; verify fallback to identity `[0, 0, 0, 1]` with warning log.
- **Scenario M (Unit Scaling - Velocity):** Feed speed in $\text{km/h}$ ($100.0$); verify normalized to $27.77778$ m/s.
- **Scenario N (Unit Scaling - Angular Velocity):** Feed angular speed in RPM ($3000$); verify normalized to $314.15926$ rad/s.

### Domain 3: Type, Range, and Clamping Invariants (Scenarios O – U)
- **Scenario O (Lossless Type Promotion):** Feed `int16` value `32000` mapped to `float64`; verify exact promotion.
- **Scenario P (IEEE 754 NaN Injection):** Inject payload containing `NaN` in float attribute; verify immediate drop with `ERR_SEM_INVALID_NUMERIC_VALUE`.
- **Scenario Q (IEEE 754 Infinity Injection):** Inject payload containing `+Infinity`; verify rejection.
- **Scenario R (Subnormal Float Scrubbing):** Inject denormalized float $10^{-42}$; verify scrubbed to $0.0$ or dropped.
- **Scenario S (Extreme Coordinate Clamping):** Feed coordinate $1.5 \times 10^8$ m; verify clamped to $1.0 \times 10^7$ m and `clamped: true` set.
- **Scenario T (Integer Overflow Protection):** Attempt to map `uint64` ($2^{62}$) to `int32`; verify clamped to `INT32_MAX` with warning.
- **Scenario U (Precision Loss Accounting):** Convert `float64` to `float32`; verify lossiness record created and fidelity tier adjusted.

### Domain 4: Discrete Identity & Enum Mapping (Scenarios V – AA)
- **Scenario V (Canonical CEID Translation):** Feed local actor ID $401$; verify translated to `CEID:8f3c...` and mapped to target proxy $98112$.
- **Scenario W (1-to-1 Lossless Enum):** Map `DAMAGE_FIRE` to `ElementalDamage.Fire`; verify lossless mapping flag.
- **Scenario X (N-to-1 Enum Generalization):** Map `DMG_FIRE_PLASMA` to generic `Fire`; verify generalized mapping with fidelity warning.
- **Scenario Y (Unmapped Enum Fallback):** Feed undefined enum ordinal $999$ with `FALLBACK_AND_LOG` strategy; verify replaced by neutral fallback.
- **Scenario Z (Unmapped Enum Fail-Closed):** Feed undefined enum ordinal $999$ with `FAIL_CLOSED` strategy; verify payload dropped with `ERR_SEM_ENUM_UNMAPPED`.
- **Scenario AA (Bitmask State Flag Unpacking):** Feed packed bitmask `0x05` (Bits 0 and 2 set); verify correctly unpacked to discrete booleans.

### Domain 5: Derivations, Cycles & Lossiness Vaulting (Scenarios AB – AG)
- **Scenario AB (Simple Derived Speed):** Derive `speed` from 3D velocity `[3.0, 4.0, 0.0]`; verify calculated value is exactly $5.0$ m/s.
- **Scenario AC (Cyclic Dependency Rejection):** Define mapping where $A$ depends on $B$ and $B$ depends on $A$; verify rejection via Kahn's algorithm at registration.
- **Scenario AD (Multi-Level Derivation DAG):** Define 3-level derivation tree ($A \to B \to C$); verify correct topological evaluation.
- **Scenario AE (Opaque Vault Token Round-Trip):** Map 20 fields to 5 fields + `vault_token`; modify 2 fields; map back; verify all 20 fields intact.
- **Scenario AF (Vault Token Tamper Detection):** Flip 1 bit inside `vault_token`; verify HMAC failure, vault rejection, and security log.
- **Scenario AG (Expired Vault Token TTL):** Present `vault_token` aged $15$ seconds; verify expired token dropped gracefully.

### Domain 6: Security, Malicious AST & Untrusted Definitions (Scenarios AH – AM)
- **Scenario AH (AST Timeout Enforcement):** Execute complex mathematical derivation loop; verify aborted after $50 \ \mu\text{s}$.
- **Scenario AI (AST Max Depth Exceeded):** Ingest derivation expression with AST depth $= 12$; verify parse rejection (max depth $= 8$).
- **Scenario AJ (Excessive Operator Count):** Ingest expression with 45 operators; verify rejection (max operators $= 32$).
- **Scenario AK (Division by Zero in AST):** Ingest derivation $X / Y$ where $Y = 0$; verify graceful fallback without crash.
- **Scenario AL (AI-Proposed Mapping Gating):** Attempt to activate `DRAFT_AI_PROPOSED` mapping in live session; verify hard refusal.
- **Scenario AM (Oversized Payload Rejection):** Send message payload exceeding $256$ KiB; verify immediate drop at Layer 1.

---

## 3. Acceptance Criteria and Pass Thresholds

- **Zero Test Failures:** All 39 scenarios must achieve 100% pass rate in automated synthetic test runner.
- **Zero Host Game Execution:** Verified that no test imports or launches external binaries or hooks live processes.
- **Hot-Path Performance:** Benchmark must demonstrate $< 15 \ \mu\text{s}$ average translation latency per message under synthetic load.
