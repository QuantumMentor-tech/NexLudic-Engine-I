# STATE SYNCHRONIZATION VERSIONING AND INVALIDATION POLICY

**Document Identifier:** `NLE-POL-SVI-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 75 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Version Governance  
**Upstream Dependencies:** `NLE-SPC-LSS-001` (Step 75), `NLE-POL-MIR-001` (Step 74), `NLE-CON-MVF-001` (Step 74)  
**Downstream Dependents:** `NLE-SPC-SRR-001` (Step 75), `NLE-MAT-STP-075` (Step 75)

---

## 1. Purpose and Scope

This policy governs the multi-tiered versioning hierarchy across bridge sessions and defines the precise invalidation matrix when schema, mapping, or runtime contract versions change during active synchronization.

---

## 2. Multi-Tiered Versioning Hierarchy

The synchronization engine coordinates nine distinct version markers that **shall never** be conflated:

```
+-----------------------------------+---------------------------------------+---------------------------+
| Version Marker                    | Identifier & Format                   | Scope & Mutability        |
+-----------------------------------+---------------------------------------+---------------------------+
| 1. Session Epoch                  | uint64 monotonic counter              | Per-session connection    |
| 2. Snapshot Cutoff Revision       | uint64 state marker                   | Per-snapshot baseline     |
| 3. Source State Revision          | uint64 state tick                     | Per-update source tick    |
| 4. Update Stream Sequence Number  | uint64 sequence counter               | Per-stream delivery       |
| 5. State Schema Version           | SemVer 2.0.0 (e.g. 2.1.0)             | Domain Concept Descriptor |
| 6. Message Contract Version       | SemVer 2.0.0 (e.g. 1.0.0)             | Step 73 Envelope Contract |
| 7. Semantic Mapping Version       | SemVer 2.0.0 (e.g. 1.2.0)             | Step 74 SMR URN           |
| 8. Adapter Capability Version     | SemVer 2.0.0 (e.g. 3.0.0)             | Step 71/72 Registration   |
| 9. Game Build / Hash              | SHA-256 binary hash                   | Host game executable      |
+-----------------------------------+---------------------------------------+---------------------------+
```

---

## 3. Dynamic Version Invalidation Matrix

When an event triggers a version change during active synchronization, the engine executes the following policy:

| Version Change Event | Scope of Impact | Immediate Action | Synchronization Status | Recovery Path |
| :--- | :--- | :--- | :---: | :--- |
| **Session Epoch Increment** | Entire Session | Invalidate all queued deltas and cached baselines from old epoch. | `NOT_SYNCHRONIZED` | Require complete fresh handshake & initial snapshot. |
| **Semantic Mapping Major Bump** | Specific Domain | Halt incremental deltas; discard compiled bytecode cache. | `RESYNC_REQUIRED` | Invalidate baseline; acquire new snapshot with new SMR. |
| **Semantic Mapping Minor Bump** | Specific Domain | Hot-reload mapping if backward-compatible; continue delta stream. | `ACTIVE` | Log informational audit event; no resync needed. |
| **Domain Schema Change** | Specific Domain | Reject incoming deltas under old schema. | `RESYNC_REQUIRED` | Invalidate baseline; require fresh snapshot matching schema. |
| **Adapter Capability Drop** | Endpoint | Revoke active write permissions for affected domains. | `DEGRADED` / `FAILED` | Suspend synchronization for affected domains. |
| **Host Game Restart / Re-attach**| Endpoint | Full reset of local proxy entities; invalidate all CEID mappings. | `ESTABLISHING_BASELINE` | Full resynchronization protocol. |

---

## 4. Invalidation Scoping Rules

1. **Domain Isolation:** Invalidation of one domain (e.g. `inventory`) **shall not** invalidate unrelated domains (e.g. `vehicle_transform`) unless an explicit cross-domain dependency is registered.
2. **Explicit Audit Logging:** Every invalidation event emits a structured audit record detailing the triggering version change, affected domains, and the chosen recovery pathway.
