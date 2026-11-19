# Content Transformation Eligibility & Precondition Policy

**Document Identifier:** `NLE-POL-TEP-001`  
**Classification:** Quality Assurance & Precondition Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 63 of 100  
**Upstream Dependencies:** `NLE-SPC-TRF-001`, `NLE-MOD-TRF-001`, `NLE-CON-AEP-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Precondition Principles

A transformation operation must never execute based on optimistic assumptions. Before a transcode worker is spawned in Stage H, the transformation engine must verify that the candidate asset, the transformation definition, the adapter capabilities, the target environment, and the task authorization satisfy ten strict preconditions.

---

## 2. Ten Mandatory Preconditions (`PRE-TRF-01..10`)

| Code | Precondition Requirement | Verification Method | Failure Code |
| :--- | :--- | :--- | :--- |
| `PRE-TRF-01` | Source asset validated in Step 62 | Status equals `VALIDATED_ASSET` in record | `ERR_PRE_SOURCE_NOT_VALIDATED` |
| `PRE-TRF-02` | Verified source format match | Magic bytes match transformation input schema | `ERR_PRE_FORMAT_MISMATCH` |
| `PRE-TRF-03` | Transformation definition approved | `approval_state == APPROVED_STANDARD` | `ERR_PRE_DEFINITION_UNAPPROVED` |
| `PRE-TRF-04` | Adapter capability verified | Adapter capability query confirms transcode support | `ERR_PRE_CAPABILITY_MISSING` |
| `PRE-TRF-05` | Mapping specification complete | All non-optional target fields mapped | `ERR_PRE_MAPPING_INCOMPLETE` |
| `PRE-TRF-06` | Operation authorization granted | Task contract grants Gate G4 (Transform) | `ERR_PRE_AUTH_DENIED` |
| `PRE-TRF-07` | SPDX license permits modification | License permits derivative work (`modification_permitted`) | `ERR_PRE_LICENSE_RESTRICTED` |
| `PRE-TRF-08` | Dependencies resolved in DAG | All referenced textures/materials available in DAG | `ERR_PRE_DEPENDENCIES_UNRESOLVED` |
| `PRE-TRF-09` | Resource quotas within limits | Staging storage $\ge 2 \times$ input size; RAM within 4 GB | `ERR_PRE_RESOURCE_EXCEEDED` |
| `PRE-TRF-10` | Non-prohibited operation check | Zero executable patching, DLL injection, or DRM bypass | `ERR_PRE_PROHIBITED_OPERATION` |

---

## 3. Capability vs. Proof Doctrine

A registered adapter's declared capability (e.g. `CAP_TRANSCODE_NIF_V20`) is a prerequisite, but it does NOT constitute proof of conversion correctness for a specific asset:
1. **Declared Capability:** Indicates the adapter contains the code pathways to parse and write the format.
2. **Empirical Verification:** The generated output must undergo post-transcode structural (`Stage I`) and semantic validation before downstream acceptance.
3. **No Blind Trust:** An adapter claiming $100\%$ compatibility must still have its output verified against metric invariants.

---

## 4. Unsupported Transformations & Degradation Handling

If no transformation definition exists to convert Format $A$ to Format $B$:
- **Direct Conversion Absence:** The engine queries whether a multi-stage path exists ($A \rightarrow \text{IR} \rightarrow B$).
- **Optional Asset Degradation:** If the asset is marked `is_optional: true`, it is cleanly skipped with warning `WARN_TRF_SKIPPED_OPTIONAL`.
- **Critical Asset Block:** If the asset is mandatory, the pipeline halts with `ERR_TRF_NO_CONVERSION_PATH`. Renaming file extensions to fake conversion is strictly prohibited.
