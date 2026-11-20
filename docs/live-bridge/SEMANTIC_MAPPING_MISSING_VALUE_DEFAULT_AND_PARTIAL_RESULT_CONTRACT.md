# SEMANTIC MAPPING MISSING VALUE, DEFAULT AND PARTIAL RESULT CONTRACT

**Document Identifier:** `NLE-CON-MDP-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Data Fidelity  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-STD-001` (Step 74), `NLE-POL-IES-001` (Step 74)  
**Downstream Dependents:** `NLE-POL-LAF-001` (Step 74), `NLE-CON-MVF-001` (Step 74)

---

## 1. Purpose and Scope

This contract governs the handling of missing fields, null values, defaulted parameters, and partial mapping results when transforming game data.

In cross-game interoperability, schema symmetry is rare. Game A may produce a 20-field telemetry record while Game B only accepts 5 fields. Conversely, Game B may require an attribute that Game A does not simulate. This contract establishes strict rules to prevent hallucinated data while guaranteeing round-trip data preservation.

---

## 2. Fundamental Distinction: Missing vs. Null vs. Zero vs. Default

The engine strictly differentiates between five states of data presence:

| State | Semantic Definition | Handling in Transformation | Prohibited Action |
| :--- | :--- | :--- | :--- |
| **PRESENT** | Value observed and measured by source engine. | Transformed directly or normalized. | N/A |
| **NULL** | Explicitly declared absence of value (e.g., target has no equipped weapon). | Preserved as `NULL` if target allows nullable; else safe empty object. | Do not substitute arbitrary dummy ID. |
| **ZERO** | Measured numerical zero (e.g., velocity = 0.0 m/s, health = 0). | Treated as a real measured quantity. | Never conflate `ZERO` with `NULL` or `MISSING`. |
| **MISSING** | Property omitted from payload (schema discrepancy or partial update). | Resolved via Default Value Hierarchy or vaulted. | Never fabricate a non-default arbitrary value. |
| **UNDEFINED**| Concept not known to source schema. | Preserved in Opaque Vault (`vault_token`). | Never fabricate simulated physics. |

---

## 3. Default Value Authority Hierarchy

When a target schema strictly requires a field that is absent or unmapped from the source, the default value is resolved through the following strict hierarchical precedence:

```
+-----------------------------------------------------------------------------+
| 1. RECORD-LEVEL OVERRIDE (Explicit default declared in SemanticMappingRecord)|
+-----------------------------------------------------------------------------+
                                       |
                                       v (If not specified)
+-----------------------------------------------------------------------------+
| 2. TARGET DESCRIPTOR DEFAULT (Field default defined in Target DCD)          |
+-----------------------------------------------------------------------------+
                                       |
                                       v (If not specified)
+-----------------------------------------------------------------------------+
| 3. CANONICAL SYSTEM DEFAULT (Zero, Identity, or Empty String)               |
+-----------------------------------------------------------------------------+
                                       |
                                       v (If field is non-nullable & no default)
+-----------------------------------------------------------------------------+
| 4. HARD REJECTION (ERR_SEM_MANDATORY_FIELD_MISSING)                         |
+-----------------------------------------------------------------------------+
```

---

## 4. Opaque Metadata Vaulting (`vault_token`)

To achieve lossless round-trip replication across asymmetric pipelines (e.g., Game A -> Game B -> Game A), unmapped or missing fields from Game A are preserved in an encrypted, authenticated vault token:

```
[Game A: 20 Fields] 
         |
         v (Map to Game B: 5 Fields)
[Game B Payload: 5 Mapped Fields + vault_token (containing 15 vaulted fields)]
         |
         v (Game B processes state, modifies 2 fields, sends update)
[Game B Response: 5 Fields + untouched vault_token]
         |
         v (Map back to Game A)
[Game A Unpacked: 5 Modified Fields + 15 Restored Fields from vault_token = 20 Fields Complete!]
```

### Vault Token Invariants:
1. **Cryptographic Integrity:** The `vault_token` is signed using HMAC-SHA256 with the bridge session secret.
2. **Anti-Tampering:** If Game B or an intermediary alters the `vault_token` bytes, the signature check fails, the vaulted data is invalidated, and `ERR_SEM_VAULT_INTEGRITY_COMPROMISED` is logged.
3. **Bounded Lifetime:** A `vault_token` expires if not reconciled within a configurable TTL (default 10.0 seconds).
