# Package Assembly Determinism, Revisioning & Freshness Contract

**Document Identifier:** `NLE-CON-PAD-001`  
**Classification:** Technical Contract & Assembly Governance Specification  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-POL-PMS-001`, `NLE-POL-TDP-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Deterministic Assembly Philosophy

Package assembly must be strictly deterministic: identical member revisions, dependency decisions, and metadata inputs MUST yield identical package definitions and manifests regardless of execution time, machine architecture, or worker thread ordering.

---

## 2. Fifteen-Step Deterministic Assembly Sequence

Stage K executes assembly planning across fifteen sequential, auditable steps:

```
[1. Verify Request Authorization] ---------> Confirm Gate G5 granted in task contract
[2. Resolve Target Game Context] ----------> Bind exact target build and engine limits
[3. Freeze Member Artifact Revisions] -----> Lock exact revision_id and SHA-256 digests
[4. Evaluate Artifact Eligibility] --------> Verify all 5 layers in NLE-CON-AEF-001
[5. Collect Dependency Declarations] ------> Aggregate internal and external dependencies
[6. Construct Dependency DAG] -------------> Run ALG-DAG-CYCLE and verify acyclicity
[7. Resolve Candidate Versions] -----------> Apply SemVer 2.0 and deterministic tie-breaker
[8. Detect Destination Collisions] --------> Verify unique target_relative_path entries
[9. Verify Package Completeness] ----------> Confirm zero dangling material/mesh refs
[10. Sort Member Inventory] ---------------> Lexicographical sort by target_relative_path
[11. Generate Canonical Manifest JSON] ----> Normalize JSON keys, UTF-8, LF newlines
[12. Compute Package-Level Digest] --------> Compute SHA-256 over canonical manifest bytes
[13. Recheck Evidence Freshness] ----------> Verify source hashes have not mutated
[14. Determine Readiness Classification] --> Render PACKAGE_READY or BLOCKED status
[15. Hand Off Package Dossier] ------------> Emit NLE-CON-PRH-001 dossier to Stage M
```

---

## 3. Manifest Canonicalization & Digest Stability

To guarantee bit-exact manifest digest stability (`ALG-CANON-JSON`):
1. **Key Sorting:** All JSON keys in `manifest.json` are sorted lexicographically.
2. **Whitespace Normalization:** Indentation fixed at 2 spaces; zero trailing spaces; POSIX line feeds (`\n`).
3. **Timestamp Exclusion:** Dynamic generation timestamps (`generated_at`) are excluded from the `package_integrity.manifest_digest` hash calculation to prevent digest churn.

---

## 4. Evidence Freshness & Invalidation Triggers

A previously validated package definition is IMMEDIATELY invalidated if:
1. An included artifact's file bytes change on disk without a revision bump (`ERR_PAD_ARTIFACT_MUTATED`).
2. An upstream dependency provider publishes a new revision or revokes compatibility.
3. The host game installation receives a version patch or binary update.
4. The user revokes task authorization or licensing status.
