# SEMANTIC MAPPING VALIDATION, FRESHNESS AND INVALIDATION CONTRACT

**Document Identifier:** `NLE-CON-MVF-001`  
**Lifecycle Stage:** Authoritative Contract  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Safety Pipeline  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-SPC-MVR-001` (Step 73), `NLE-POL-LAF-001` (Step 74)  
**Downstream Dependents:** `NLE-POL-SAU-001` (Step 74), `NLE-CAT-CSM-001` (Step 74)

---

## 1. Purpose and Scope

This contract defines the authoritative **14-Layer Semantic Validation Pipeline**, runtime freshness constraints, cache management, and dynamic invalidation protocols for all semantic mappings executed within the live game bridge.

Every message payload flowing through the NexLudic Engine bridge must clear these fourteen deterministic validation layers before target injection.

---

## 2. The 14-Layer Semantic Validation Pipeline

```
+----------------------------------------------------------------------------------------------------+
| 1. ENVELOPE INTEGRITY LAYER: Verify Step 73 Envelope HMAC, Session ID, and Format                  |
+----------------------------------------------------------------------------------------------------+
| 2. SOURCE SCHEMA CONFORMANCE LAYER: Validate raw payload matches Source DCD JSON Schema            |
+----------------------------------------------------------------------------------------------------+
| 3. CONCEPT RESOLUTION LAYER: Verify Source Concept URI & Version exist in Session Registry         |
+----------------------------------------------------------------------------------------------------+
| 4. TYPE COMPATIBILITY LAYER: Validate scalar and composite types against Conversion Matrix         |
+----------------------------------------------------------------------------------------------------+
| 5. SPATIAL & COORDINATE NORMALIZATION LAYER: Transform handedness and axes to Canonical RH-Y-Up    |
+----------------------------------------------------------------------------------------------------+
| 6. UNIT CONVERSION LAYER: Convert non-SI physical units to Canonical SI Base Units                 |
+----------------------------------------------------------------------------------------------------+
| 7. NUMERICAL INTEGRITY & NAN-GUARD LAYER: Reject NaN, Infinity, Subnormal, and Denormalized floats |
+----------------------------------------------------------------------------------------------------+
| 8. ENUMERATION & ORDINAL MAPPING LAYER: Resolve discrete enums via Mapping Tables or Fallbacks     |
+----------------------------------------------------------------------------------------------------+
| 9. RANGE & BOUNDARY CLAMPING LAYER: Verify values within physical boundaries; apply safe clamps    |
+----------------------------------------------------------------------------------------------------+
| 10. DERIVATION EXECUTION LAYER: Execute sandboxed AST derivation expressions in topological order  |
+----------------------------------------------------------------------------------------------------+
| 11. VAULT TOKEN VERIFICATION / GENERATION LAYER: Seal unmapped fields into encrypted vault_token   |
+----------------------------------------------------------------------------------------------------+
| 12. FIDELITY & LOSSINESS AUDIT LAYER: Verify transformation satisfies Session Fidelity Threshold   |
+----------------------------------------------------------------------------------------------------+
| 13. TARGET SCHEMA CONFORMANCE LAYER: Validate compiled record complies with Target DCD Schema      |
+----------------------------------------------------------------------------------------------------+
| 14. FRESHNESS, RATE & REPLAY PROTECTION LAYER: Verify timestamp freshness and anti-replay counters |
+----------------------------------------------------------------------------------------------------+
```

### Layer Failure Behavior:
- Any failure in Layers 1–4, 7, 10, or 13 results in **immediate fail-closed drop** and emission of an audit log.
- Layer 9 (Range/Clamping) may clamp if policy allows, but must attach a lossiness warning.
- Layer 12 triggers session renegotiation or down-tier notification if fidelity drops below agreed thresholds.

---

## 3. Freshness Bounds and Temporal Degradation

1. **Maximum Message Latency Budget:** A semantic payload is considered stale if:
   $$t_{current\_utc} - t_{source\_timestamp} > \Delta t_{max\_freshness}$$
   Where default $\Delta t_{max\_freshness} = 250 \text{ ms}$ for continuous kinematics, and $5000 \text{ ms}$ for discrete gameplay events.
2. **Clock Drift Accommodation:** Bridges tolerate up to $\pm 100 \text{ ms}$ of clock drift. Greater drift triggers NTP clock sync warning and temporal extrapolation throttling.
3. **Out-of-Order Delivery Handling:** Packets arriving with monotonically older timestamps than the highest processed packet for that entity are discarded (`ERR_SEM_STALE_TIMESTAMP`).

---

## 4. Cache Invalidation and Dynamic Eviction

The bridge maintains compiled, high-performance transformation bytecode caches. A cache entry is invalidated immediately upon:
1. **Schema Revision:** Target or Source endpoint updates its Domain Concept Descriptor version.
2. **Capability Renegotiation:** The session contract is modified under Step 72.
3. **Endpoint State Change:** An endpoint disconnects, restarts, or enters recovery.
4. **Fidelity Violation Threshold:** If an active mapping produces $> 5\%$ clamped values over a 10-second window, the cache entry is flagged for re-assessment.
