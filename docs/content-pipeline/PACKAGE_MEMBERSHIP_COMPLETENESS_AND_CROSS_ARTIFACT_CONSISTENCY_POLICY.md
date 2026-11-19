# Package Membership, Completeness & Cross-Artifact Consistency Policy

**Document Identifier:** `NLE-POL-PMC-001`  
**Classification:** Quality Assurance & Package Consistency Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-POL-PMS-001`, `NLE-POL-PDG-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Consistency Philosophy

A collection of individually valid artifacts does not automatically constitute a functional package. Individual assets can reference missing textures, target conflicting paths, or rely on contradictory engine build assumptions.

This policy specifies the membership admission criteria, destination conflict checks, and completeness rules enforced during Stage K.

---

## 2. Cross-Artifact Consistency Verifications

Before package assembly completes, the member inventory is verified against four cross-artifact invariants:

```
[Member 1: blade.nif] ---------> Destination: "Meshes/Weapons/blade.nif"
                                      |
                                      +---> [Check 1: Destination Collision Check]
                                      |     (Reject overlapping target_relative_path)
                                      |
[Member 2: blade_d.dds] --------> Bound to material slot in blade.nif
                                      |
                                      +---> [Check 2: Internal Reference Resolution]
                                      |     (Confirm all internal asset paths exist in package)
                                      |
[Member 3: blade_n.dds] --------> Target Build: "1.6.1170.0"
                                      |
                                      +---> [Check 3: Target Release Homogeneity]
                                      |     (Confirm all members target identical game build)
                                      |
                                      +---> [Check 4: License Compatibility Matrix]
                                            (Confirm transitive license compatibility)
```

---

## 3. Required vs. Optional Member Management

- **Required Members:** An artifact flagged `is_optional: false` can NEVER be excluded. If a required member is missing or invalid, package assembly FAILS (`ERR_PMC_REQUIRED_MEMBER_MISSING`).
- **Optional Members:** An artifact flagged `is_optional: true` (e.g. 4K texture pack or alternate sound cues) may be omitted if it fails validation, provided an explicit `PackageExclusionRecord` is logged in the assembly dossier.

---

## 4. Package Completeness Contract

A package definition is classified as **`PACKAGE_COMPLETE`** if and only if:
1. Every required asset referenced by included meshes/materials is present in `members[]` or resolved in `dependencies[]`.
2. All declared dependency ranges have concrete, verified candidate assignments.
3. Zero unresolved destination collisions exist.
4. All mandatory integrity digests are computed and non-null.
