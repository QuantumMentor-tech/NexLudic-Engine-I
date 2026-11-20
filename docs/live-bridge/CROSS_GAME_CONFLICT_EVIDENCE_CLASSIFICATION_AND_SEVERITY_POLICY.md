# CROSS-GAME CONFLICT EVIDENCE CLASSIFICATION AND SEVERITY POLICY

**Document Identifier:** `NLE-POL-CEC-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 76 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Conflict Classification  
**Upstream Dependencies:** `NLE-SPC-CRR-001` (Step 76), `NLE-CON-CAA-001` (Step 76), `NLE-POL-SFM-001` (Step 75)  
**Downstream Dependents:** `NLE-CON-CRF-001` (Step 76), `NLE-SPC-RSC-001` (Step 76)

---

## 1. Purpose and Scope

This policy defines the evidence requirements, confidence scoring standards, and severity taxonomy used to classify detected state conflicts.

Conflict decisions must be backed by verifiable data. This policy mandates that confidence scores represent measurable evidentiary completeness, not subjective guesses, and establishes strict severity boundaries to prevent localized discrepancies from halting unrelated game systems.

---

## 2. The 12 Dimensions of Conflict Evidence

Before classifying a conflict candidate, the reconciliation coordinator compiles an immutable **Conflict Evidence Dossier** evaluating twelve orthogonal dimensions:

```
+-----------------------------------+-------------------------------------------------------------------+
| Evidence Dimension                | Description & Verification Source                                 |
+-----------------------------------+-------------------------------------------------------------------+
| 1. Endpoint Identity & Auth       | Cryptographic validation of claiming endpoints (Step 71/72).       |
| 2. Domain & Concept Reference     | Canonical URI matching active Domain Concept Descriptor (Step 74).|
| 3. Source & Target Revisions      | Monotonic sequence markers and expected prior revisions (Step 75).|
| 4. Session & Epoch Validation     | Active session UUID and monotonic epoch comparison (Step 75).     |
| 5. Causal Vector / Dependency     | Lamport timestamp or predecessor hash chain if available.         |
| 6. Snapshot Baseline Alignment    | Cutoff revision alignment against active published baseline.      |
| 7. Semantic Mapping Version       | Exact Step 74 SMR URN and compatibility status.                   |
| 8. Ownership Registry Record      | Domain ownership contract binding (NLE-CON-CAA-001).              |
| 9. State Freshness & Age          | Monotonic time delta relative to freshness SLA (NLE-POL-SFM-001). |
| 10. Target State Read-Back        | Current verified state of target simulation if safely observable. |
| 11. Application Ack History       | Watermark status: R_obs, R_val, R_app, R_ack.                     |
| 12. Capability Evidence           | Verified adapter capability flags for proposed resolution write.  |
+-----------------------------------+-------------------------------------------------------------------+
```

---

## 3. Evidence-Backed Confidence Scoring Standards

Confidence is a multi-dimensional metric ($C \in [0.0, 1.0]$) reflecting evidentiary completeness:

$$C_{aggregate} = w_{auth} C_{auth} + w_{rev} C_{rev} + w_{sem} C_{sem} + w_{cap} C_{cap}$$

Where default weights are $w_{auth} = 0.35, w_{rev} = 0.25, w_{sem} = 0.20, w_{cap} = 0.20$.

### Inviolable Confidence Guardrails:
1. **Confidence $\neq$ Authorization:** A confidence score of $1.0$ **shall never** override a missing authorization signature or an explicit domain invariant.
2. **Zero-Threshold Fail-Closed:** If any individual component confidence drops to zero ($C_i = 0$), the aggregate score is forced to $0.0$, requiring escalation to `UNRESOLVED`.
3. **No Fabricated Probability:** Confidence reflects factual evidence presence (e.g. 10 of 12 dossier fields verified = 0.833), never heuristic guesses.

---

## 4. Conflict Severity Taxonomy & Containment Scope

```
+---------------+---------------------------------------------------+-----------------------------------+
| Severity Tier | Defining Criteria & Impact Area                   | Operational Containment Action    |
+---------------+---------------------------------------------------+-----------------------------------+
| **INFORMATIONAL**| Minor temporal lag or display telemetry skew.    | Log audit event; continuous sync. |
| **LIMITED**   | Reversible scalar attribute discrepancy.          | Isolate field; apply safe policy. |
| **SIGNIFICANT**| Transform/physics collision or kinematics desync. | Isolate domain; clamp/quarantine. |
| **CRITICAL**  | Inventory, quest progression, financial economy.  | Freeze domain; require human gate.|
| **BLOCKED**   | Security violation, broken invariant, NaN poison. | Halt session; force recovery FSM. |
+---------------+---------------------------------------------------+-----------------------------------+
```

### Domain Isolation Boundary:
A conflict classified as `SIGNIFICANT` or `CRITICAL` in Domain A (e.g. `inventory`) **must be strictly isolated**. Unrelated domains (e.g. `vehicle_telemetry`) continue active synchronization provided no cross-domain dependency is registered in the session manifest.
