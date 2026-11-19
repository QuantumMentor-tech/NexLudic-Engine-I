# Asset Duplicate, Conflict & Revision Management Policy

**Document Identifier:** `NLE-POL-ADC-001`  
**Classification:** Quality Assurance & State Management Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-SPC-CAI-001`, `NLE-POL-API-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Overview & Conflict Challenges

When ingesting content from multiple user directories, mod archives, and game installations, identical filenames or conflicting asset revisions frequently arise. This policy specifies how the ingestion system identifies duplicates, manages revisions, and resolves resource collisions deterministically without silent data overwrites.

---

## 2. Four Classes of Asset Duplication

```
1. Exact Binary Duplicate:
   [Asset A: SHA-256 = X] <== IDENTICAL BYTE MATCH ==> [Asset B: SHA-256 = X]
   Action: Deduplicate in staging storage; link canonical asset.

2. Logical Resource Collision:
   [Asset A: id = "blade.nif", hash = X] <!= DIFFERENT =!> [Asset B: id = "blade.nif", hash = Y]
   Action: Namespace by package ID; prompt user if both target the same game relative path.

3. Versioned Revision Evolution:
   [Asset A: rev-01 (v1.0.0)] --------> [Asset A: rev-02 (v1.1.0)]
   Action: Record revision lineage; maintain history; update active head pointer.

4. Divergent Multi-Package Fork:
   Same base mesh modified independently by two distinct authors.
   Action: Preserve separate provenance records; forbid automatic merging.
```

---

## 3. Collision Resolution Strategy & Precedence

If two discovered candidates claim the exact same target installation relative path (e.g. `Data/Meshes/Weapons/blade.nif`):

```
                       [Path Collision Detected]
                                   |
                  +----------------+----------------+
                  |                                 |
     [Is one asset User-Owned?]        [Both assets from Packages]
                  |                                 |
           +------+------+                   +------+------+
           |             |                   |             |
         [YES]          [NO]           [Declared Conflict?] [Undeclared]
           |             |                   |             |
           v             v                   v             v
   [PRESERVE USER]  (Check Package    [BLOCK PIPELINE] [PROMPT USER]
   (Never Overwrite   Precedence)     (Stage J Block)   (Side-by-side
    without prompt)                                      diff dialog)
```

---

## 4. Revalidation & Invalidation Triggers

A previously validated asset record is IMMEDIATELY invalidated and queued for re-ingestion if:
1. **Source Byte Hash Changes:** File modified on disk (detected via polling or filesystem watcher).
2. **Metadata Contract Updated:** Target adapter schema or format specification updated.
3. **Target Game Patch:** Host game binary or data version updated.
4. **License Status Revoked:** License terms expired or retracted by source repository.
5. **Security Policy Update:** New banned function or AST vulnerability rule added to engine core.
