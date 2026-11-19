# Package Dependency Graph, Version Resolution & Conflict Policy

**Document Identifier:** `NLE-POL-PDG-001`  
**Classification:** Technical Architecture & Dependency Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-CON-AEF-001`, `NLE-SPC-ADD-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Dependency Scope

A package definition must account for all internal and external dependencies required for its assets to function correctly in the host game. Stage J (Dependency & Compatibility Resolution) constructs the complete dependency graph, resolves version constraints, and detects mutual conflicts deterministically.

---

## 2. Nine Dependency Categories

| Category | Code | Description | Missing Action |
| :--- | :--- | :--- | :--- |
| **Required Artifact** | `DEP_REQ_ARTIFACT` | Essential texture, skeleton, or mesh. | Fatal (`ERR_DEP_UNRESOLVED`) |
| **Optional Artifact** | `DEP_OPT_ARTIFACT` | Higher-resolution textures, optional audio. | Permitted Skip if Isolated |
| **Version-Constrained**| `DEP_VER_CONSTRAINED`| Specific SemVer range (`>=1.2.0 <2.0.0`). | Fatal on Range Mismatch |
| **Capability** | `DEP_CAPABILITY` | Host engine capability (e.g. `CAP_BC7`). | Blocked if Unsupported |
| **Target Build** | `DEP_TARGET_BUILD` | Specific game patch (e.g. `1.6.1170.0`). | Incompatible Block |
| **Execution Ordering** | `DEP_ORDERED` | Resource $A$ must be indexed before $B$.| Topological Order Enforced |
| **External Package** | `DEP_EXTERNAL_PKG` | Framework mod (e.g. SKSE64, UE4SS). | Unverified External Block |
| **Conflicting** | `DEP_CONFLICTING` | Declared mutual exclusivity with mod $X$.| Assembly Halted |
| **Conditional** | `DEP_CONDITIONAL` | Required only if target build $> 1.6$. | Evaluated against Target |

---

## 3. Deterministic Version Resolution & Tie-Breaking

When resolving candidate providers for a dependency declaration:
1. **SemVer 2.0 Evaluation:** Candidate versions must strictly satisfy range bounds.
2. **Deterministic Tie-Breaking:** If multiple candidates satisfy all constraints:
   - Highest compatible SemVer version is selected.
   - If version strings are identical, candidate with earliest immutable `revision_id` is selected.
   - Recency of discovery or arbitrary filesystem order MUST NEVER be used as a tie-breaker.
3. **No Automatic Downgrade:** If no candidate satisfies a `^2.0.0` constraint, the resolver must NEVER silently select `1.9.0` to make the graph resolve.

---

## 4. Directed Acyclic Graph (DAG) & Cycle Detection

All dependency relationships must form a strict Directed Acyclic Graph (DAG).

```
   [Celestial Weapon Package]
              |
       +------+------+
       v             v
[Celestial Blade] [Celestial Bow]
       |             |
       +------+------+
              v
[Celestial Shared Texture Atlas]  <-- (Diamond Resolution: Permitted)
```

### Cycle Rejection (`ALG-DAG-CYCLE`):
The dependency graph is checked via Depth-First Search with three-color node tracking. Any back-edge representing a dependency loop ($A \rightarrow B \rightarrow A$) triggers immediate halt with `ERR_DEP_CYCLE_DETECTED`.

---

## 5. Conflict Detection & Destination Invariant

Two package members MUST NOT target the same virtual game relative path (e.g. `Meshes/Weapons/blade.nif`) unless:
1. An explicit `OverrideDeclaration` exists with defined precedence.
2. Both assets originate from the same package with identical hashes.

Otherwise, assembly halts immediately with `ERR_PKG_DESTINATION_CONFLICT`.
