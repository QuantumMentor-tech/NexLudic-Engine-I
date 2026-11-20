# CROSS-GAME MAPPING LOSSINESS, APPROXIMATION AND FIDELITY POLICY

**Document Identifier:** `NLE-POL-LAF-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Data Integrity & Audit  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-CON-SEC-001` (Step 74), `NLE-POL-TRU-001` (Step 74)  
**Downstream Dependents:** `NLE-CON-MVF-001` (Step 74), `NLE-POL-SAU-001` (Step 74)

---

## 1. Purpose and Scope

This policy defines the standards, error budgets, lossiness classification rules, and audit logging requirements for data approximation and information loss during cross-game state translation.

When bridging games with differing simulation fidelity (e.g., a high-precision flight simulator operating with 64-bit double precision physics versus an arcade action game operating with 8-bit quantized values), information loss is unavoidable. This policy mandates that all lossiness must be **bounded, budgeted, explicitly declared, auditable, and non-destructive to round-trip workflows**.

---

## 2. Taxonomy of Information Loss in Cross-Game Bridges

| Loss Type | Mechanism | Detectability | Invertible? | Mitigation Strategy |
| :--- | :--- | :--- | :---: | :--- |
| **Precision Truncation** | Float64 -> Float32 or Float32 -> Int16 quantization. | Deterministic calculation ($\Delta v = |v_{orig} - v_{quant}|$) | No | Bounded Error Budget ($\epsilon \le 10^{-4}$); declare lossiness flag. |
| **Range Clamping** | Clamping extreme values exceeding target limits (e.g., $v > 500$ m/s clamped to $255$). | Immediate boundary check | No | Flag `clamped: true`; record audit warning on saturation. |
| **Dimensional Projection** | Projecting 3D vector to 2D plane (e.g., 3D flight sim to 2D radar). | Structural comparison | No | Store unmapped axis ($Z$ or $Y$) into `vault_token`. |
| **Enum Generalization** | Specialised sub-types mapped to general category (e.g., 5 fire types -> 1 generic fire). | Discrete mapping lookup | Partial | Cache original enum tag in `vault_token` for reverse routing. |
| **Temporal Downsampling** | 120Hz physics stream downsampled to 30Hz target update rate. | Timestamp delta analysis | No | Apply low-pass filtering and dead-reckoning extrapolation hints. |

---

## 3. Strict Approximation Error Budgets

Every Semantic Mapping Record declaring an `APPROXIMATE` or `LOSSY` transformation must adhere to maximum acceptable error bounds ($\epsilon_{\max}$):

1. **Spatial Position Tolerance:**
   $$\epsilon_{pos} = \|\mathbf{p}_{target} - \mathbf{p}_{canonical}\| \le 0.05 \text{ meters}$$
   *(Deviations greater than 5 centimeters in standard human-scale environments require explicit user override).*
2. **Angular Orientation Tolerance:**
   $$\epsilon_{rot} = 2 \arccos(|\mathbf{q}_{target} \cdot \mathbf{q}_{canonical}|) \le 0.0175 \text{ radians} \ (\approx 1.0^\circ)$$
3. **Linear Velocity Tolerance:**
   $$\epsilon_{vel} \le 0.1 \text{ m/s}$$
4. **Normalized Scalar Attributes (Health, Stamina, Shield):**
   $$\epsilon_{scalar} \le 0.005 \ (0.5\% \text{ of full scale})$$

If an operational transformation exceeds its declared error budget during runtime execution, the engine must increment `MetricFidelityViolationCounter` and trigger an alert.

---

## 4. Opaque Metadata Vaulting Protocol (`vault_token`)

To maintain round-trip lossless fidelity when routing data through a game with a lower-dimensional schema, NexLudic Engine enforces Opaque Metadata Vaulting:

```
[High-Fidelity Engine A]
  Properties: { pos: Vector3, vel: Vector3, mass: float, aerodynamics: AeroStruct, engine_rpm: float }
         |
         v (Mapping: A -> B with Vaulting)
[Low-Fidelity Engine B]
  Properties: { pos: Vector3, vel: Vector3 }
  Attachment: vault_token = Encrypt_HMAC({ mass, aerodynamics, engine_rpm, original_timestamp })
         |
         v (Engine B updates pos/vel, returns state to Engine A)
[High-Fidelity Engine A Unpack]
  Restores: { pos: Vector3_new, vel: Vector3_new, mass, aerodynamics, engine_rpm }
```

### Security and Vault Governance:
- **Key Derivation:** Vault tokens are encrypted using AES-GCM-256 with an ephemeral key derived from the active `NegotiatedSessionContract` (Step 72).
- **Integrity Tag:** Any byte corruption by Engine B or network glitch results in authentication failure; corrupted tokens are safely discarded while mapped properties are retained.

---

## 5. Prohibited Semantic Substitutions

The following substitutions are **strictly prohibited** by NexLudic Engine:
1. **Fabricating Physics Mass:** Substituting mass $= 0$ or mass $= 1000$ when mass is absent from the source without an explicit `DEFAULTED_FALLBACK` attribution.
2. **Silent Teleportation:** Substituting position $(0, 0, 0)$ when spatial data is missing or corrupted.
3. **Fictitious Damage Types:** Converting an unknown magical/exotic damage type to physical damage without explicit mapping authorization.
4. **Undisclosed Clamping:** Clamping a value to prevent a crash without logging an audit trail or setting `is_clamped = true`.
