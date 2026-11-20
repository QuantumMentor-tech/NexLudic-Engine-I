# STEP 74 SEMANTIC MAPPING REQUIREMENTS TRACEABILITY MATRIX

**Document Identifier:** `NLE-MAT-STP-074`  
**Lifecycle Stage:** Authoritative Traceability Matrix  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / System Assurance  
**Upstream Dependencies:** `NLE-SPC-CSM-001` through `NLE-POL-SAU-001`  
**Downstream Dependents:** Step 75, Step 76

---

## 1. Traceability Architecture

This matrix establishes formal traceability between **40 engineering requirements (`SMR-REQ-001` through `SMR-REQ-040`)**, their governing specifications, component implementations, synthetic test scenarios, and acceptance criteria.

```
+----------------------------------------------------------------------------------------------------+
| Requirement ID | Governing Specification | Primary Enforcing Component | Synthetic Test Scenario   |
+----------------------------------------------------------------------------------------------------+
```

---

## 2. Requirements Traceability Table

| Requirement ID | Requirement Description | Governing Specification | Enforcing Component | Test Scenario | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :---: | :--- |
| `SMR-REQ-001` | Multi-layer non-equivalence hierarchy enforcement | `NLE-SPC-CSM-001` | SemanticValidationEngine | A, B | Structural match does not bypass semantic validation |
| `SMR-REQ-002` | Disambiguation of mapping categories | `NLE-POL-MIR-001` | MappingRecordParser | V, W | Every mapping explicitly declared into 1 of 6 categories |
| `SMR-REQ-003` | Anti-Cheat and Medium IL sandbox inviolability | `NLE-SPC-CSM-001` | BridgeRuntimeSecurityGate | All | Zero memory scanning, zero DLL injection |
| `SMR-REQ-004` | No silent distortion or fabricated equivalence | `NLE-SPC-CSM-001` | PropertyNormalizer | S, Z | Clamping and defaults explicitly flagged |
| `SMR-REQ-005` | JSON-LD Domain Concept Descriptor Schema | `NLE-CON-STD-001` | ConceptDescriptorLoader | A, B | Complies with draft 2020-12 JSON Schema |
| `SMR-REQ-006` | Two-stage descriptor validation (structural + semantic) | `NLE-CON-STD-001` | DCDValidator | A, G | Separates syntax parsing from invariant checks |
| `SMR-REQ-007` | SemVer 2.0.0 enforcement for DCD revisions | `NLE-CON-STD-001` | RevisionGovernanceEngine | D | Breaking schema changes require major bump |
| `SMR-REQ-008` | SHA-256 integrity hash verification on DCD | `NLE-CON-STD-001` | IntegrityVerifier | C | Hash mismatch prevents descriptor load |
| `SMR-REQ-009` | Canonical URN naming for mapping records | `NLE-POL-MIR-001` | MappingRegistry | V | Matches `map:{src}->{tgt}:v{version}` format |
| `SMR-REQ-010` | 7-State Mapping Lifecycle FSM | `NLE-POL-MIR-001` | LifecycleManager | AL | Enforces legal transitions; blocks draft activation |
| `SMR-REQ-011` | Immutability of approved mapping definitions | `NLE-POL-MIR-001` | StorageProvider | C | No in-place updates; version bump mandatory |
| `SMR-REQ-012` | Multi-dimensional equivalence evaluation (6 axes) | `NLE-CON-SEC-001` | EquivalenceEvaluator | U | Scores structural, spatial, temporal, range, state, gov |
| `SMR-REQ-013` | 5-Tier Compatibility Classification (T0 - T4) | `NLE-CON-SEC-001` | CompatibilityClassifier | U | Correctly maps $S_{eq}$ score to tier |
| `SMR-REQ-014` | Fail-closed gate on critical zero dimensional score | `NLE-CON-SEC-001` | CompatibilityClassifier | S | Drops to T4 if any core dimension is 0 |
| `SMR-REQ-015` | Immutable Compatibility Assessment Report output | `NLE-CON-SEC-001` | AssessmentReporter | U | Emits signed report prior to session load |
| `SMR-REQ-016` | Type compatibility and conversion matrix | `NLE-POL-TRU-001` | TypeConverter | O, T | Safe promotion allowed; overflow clamped |
| `SMR-REQ-017` | IEEE 754 NaN and Infinity rejection | `NLE-POL-TRU-001` | FloatSanitizer | P, Q | Drops payloads containing NaN or Inf |
| `SMR-REQ-018` | Subnormal float scrubbing | `NLE-POL-TRU-001` | FloatSanitizer | R | Scrubs denormals to 0.0 or drops payload |
| `SMR-REQ-019` | Canonical SI unit normalization (m, s, kg, rad) | `NLE-POL-TRU-001` | UnitConverter | H, M, N | Non-SI values scaled by exact conversion factors |
| `SMR-REQ-020` | Canonical Coordinate Frame standard (RH, Y-Up) | `NLE-POL-TRU-001` | SpatialTransformer | H, I | Correctly transforms LH Z-up and LH Y-up |
| `SMR-REQ-021` | Quaternion normalization and sanity checking | `NLE-POL-TRU-001` | SpatialTransformer | K, L | Renormalizes norm near 1.0; replaces degenerate |
| `SMR-REQ-022` | Physical coordinate clamping bounds ($\pm 10^7$ m) | `NLE-POL-TRU-001` | SpatialTransformer | S | Clamps coordinates exceeding planetary scale |
| `SMR-REQ-023` | Two-tier entity identity model (Local vs CEID) | `NLE-POL-IES-001` | EntityRegistry | V | Resolves local IDs to cross-game UUIDs |
| `SMR-REQ-024` | 1-to-1, N-to-1, and 1-to-N enum mapping | `NLE-POL-IES-001` | EnumMapper | W, X | Generalization flags lossiness; specialization uses ctx |
| `SMR-REQ-025` | Unknown enum fallback and fail-closed strategies | `NLE-POL-IES-001` | EnumMapper | Y, Z | Configurable fallback vs fail-closed drop |
| `SMR-REQ-026` | Discrete boolean unpacking of packed bitmasks | `NLE-POL-IES-001` | BitmaskNormalizer | AA | Unpacks into discrete canonical bools before repacking |
| `SMR-REQ-027` | Differentiation of Missing, Null, Zero, Undefined | `NLE-CON-MDP-001` | ValueResolver | AE | Zero never treated as Null; Missing resolved via rule |
| `SMR-REQ-028` | Default value authority hierarchy (4 stages) | `NLE-CON-MDP-001` | DefaultResolver | Y | Record -> Descriptor -> Canonical -> Hard Reject |
| `SMR-REQ-029` | Opaque metadata vaulting (`vault_token`) | `NLE-CON-MDP-001` | VaultManager | AE, AF | AES-GCM-256 encrypted token preserves unmapped data |
| `SMR-REQ-030` | Vault token anti-tamper verification | `NLE-CON-MDP-001` | VaultManager | AF | Bit tampering triggers HMAC failure and drop |
| `SMR-REQ-031` | Vault token TTL expiration | `NLE-CON-MDP-001` | VaultManager | AG | Expired vault tokens discarded after TTL |
| `SMR-REQ-032` | Sandboxed AST derived property evaluator | `NLE-SPC-DMD-001` | AstEvaluator | AB | Executes pure math expressions without eval() |
| `SMR-REQ-033` | Kahn's algorithm topological sort & cycle check | `NLE-SPC-DMD-001` | DependencyGraph | AC, AD | Detects and blocks dependency cycles at load time |
| `SMR-REQ-034` | Bounded AST derivation execution limits | `NLE-SPC-DMD-001` | AstEvaluator | AI, AJ | Max depth 8; max operators 32 |
| `SMR-REQ-035` | Approximation error budgets ($\epsilon \le 0.05$ m) | `NLE-POL-LAF-001` | FidelityMonitor | H, J | Bounded spatial and scalar deviation limits |
| `SMR-REQ-036` | 14-Layer Semantic Validation Pipeline | `NLE-CON-MVF-001` | PipelineCoordinator | All | Payload passes all 14 layers in deterministic order |
| `SMR-REQ-037` | Message freshness and latency budgets | `NLE-CON-MVF-001` | FreshnessFilter | AG | Drops messages exceeding 250ms latency budget |
| `SMR-REQ-038` | Dynamic cache invalidation triggers | `NLE-CON-MVF-001` | CacheManager | D | Invalidates compiled mappings upon schema change |
| `SMR-REQ-039` | 4-Tier Security Trust Classification | `NLE-POL-SAU-001` | TrustAuthority | AL | Builtin -> Verified -> Community -> AI-Proposed |
| `SMR-REQ-040` | AI-Proposed mapping human-in-the-loop gate | `NLE-POL-SAU-001` | SecurityPolicyEnforcer | AL | Blocks AI drafts until signed by human administrator |
