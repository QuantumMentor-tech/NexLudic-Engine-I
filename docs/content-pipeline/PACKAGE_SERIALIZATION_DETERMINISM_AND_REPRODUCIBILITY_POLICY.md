# Package Serialization Determinism and Reproducibility Policy

**Document Identifier:** `NLE-POL-SDR-001`  
**Lifecycle Stage:** Content Pipeline Stage M Governance & Determinism  
**Parent Document:** [HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/content-pipeline/HOST_GAME_PACKAGE_MATERIALIZATION_SERIALIZATION_AND_INTEGRITY_VERIFICATION_SPECIFICATION.md) (`NLE-SPC-PMS-001`)  
**System Status:** Authoritative Architectural Policy  
**Classification:** Build Reproducibility & Binary Invariance  

---

## 1. Principle of Bit-for-Bit Reproducibility

The **Package Serialization Determinism and Reproducibility Policy** mandates that identical inputs, target schemas, and serialization parameters MUST yield bit-for-bit identical physical package container files across all builds:

$$\text{Serialize}(\text{Snapshot}_K, \text{Profile}_P) \equiv \text{Serialize}(\text{Snapshot}_K, \text{Profile}_P) \quad \forall \; \text{Environments}, \text{Timestamps}$$

This invariant is essential to support cryptographic content addressing, distributed package caching, verifiable build provenance, and differential patch generation.

---

## 2. Sources of Non-Determinism & Mitigation Rules

To achieve absolute determinism, serializers must eliminate every potential source of runtime entropy:

```
+--------------------------------------------------------------------------------------------------+
|                            ENTROPY SOURCES & ELIMINATION MANDATES                                |
+--------------------------------------------------------------------------------------------------+
|  1. Filesystem Traversal Order  --> Lexicographical byte sorting of all member paths              |
|  2. Archive Timestamps           --> Fixed canonical timestamp: 1980-01-01 00:00:00 UTC           |
|  3. JSON Key Serialization       --> RFC 8785 Canonical JSON (ALG-CANON-JSON)                    |
|  4. Compression Algorithm Drift  --> Fixed Deflate level (6), fixed window size (32KB), fixed mem |
|  5. File Permissions / Modes     --> Uniform normalized POSIX mode: 0644 (files) / 0755 (dirs)    |
|  6. Environmental Leakage        --> Zero local hostnames, usernames, or absolute build paths     |
+--------------------------------------------------------------------------------------------------+
```

### 2.1 Lexicographical Member Sorting Rule
Before serialization, all archive entries MUST be sorted strictly in ascending order according to the UTF-8 byte values of their normalized physical paths:
$$\text{Entry}_A < \text{Entry}_B \iff \text{UTF8\_Bytes}(\text{Path}_A) < \text{UTF8\_Bytes}(\text{Path}_B)$$

For example, the correct ordering is:
1. `assets/audio/click.wav`
2. `assets/textures/diffuse.dds`
3. `manifest.json`
4. `overrides/config.ini`
5. `provenance.json`

### 2.2 Canonical JSON Formatting (`ALG-CANON-JSON`)
All serialized JSON metadata files (`manifest.json`, `provenance.json`) must strictly conform to **RFC 8785 (JSON Canonicalization Scheme - JCS)**:
- Object keys sorted lexicographically by UTF-16 code units.
- No insignificant whitespace (no indentations, no extra spaces or newlines).
- Floating-point numbers normalized to ECMAScript standard formatting.
- String literals encoded in UTF-8 without unnecessary escape sequences.

### 2.3 Normalized Archive Timestamps & Headers
For ZIP-based containers (`.nlpkg`):
- All DOS modification date/time fields MUST be fixed to `1980-01-01 00:00:00 UTC` (DOS date: `0x0021`, DOS time: `0x0000`).
- Unix extra fields (e.g., `Info-ZIP Extended Timestamp`, UID/GID) must be omitted.
- External attributes MUST be set to fixed permissions: `0x81A40000` (POSIX regular file, `0644`).

### 2.4 Compression Parameter Pinning
When Deflate compression is enabled:
- Compression level MUST be pinned to Level 6.
- Deflate strategy MUST be pinned to `Z_DEFAULT_STRATEGY`.
- Sliding window size MUST be pinned to 32 KB ($2^{15}$ bytes).
- Memory level MUST be pinned to Level 8.
- Different compression libraries (e.g., zlib, libdeflate, miniz) must produce identical bitstreams; if cross-library divergence is detected, the engine standardizes on the bundled `miniz` or `zlib` implementation.

---

## 3. Verification of Determinism

During quality assurance and automated test execution, determinism is validated through **Two-Pass Materialization Verification**:
1. Materialize package from Snapshot $A \rightarrow \text{Package}_1$.
2. Purge workspace, reload Snapshot $A$, and re-materialize $\rightarrow \text{Package}_2$.
3. Compute SHA-256 digests:
   $$\text{SHA256}(\text{Package}_1) \stackrel{?}{=} \text{SHA256}(\text{Package}_2)$$
4. Any discrepancy in byte length, checksum, or archive header bits results in build rejection (`ERR_SDR_DETERMINISM_VIOLATION`).

---

## 4. Policy Compliance Invariant

Any serializer implementation that introduces runtime-dependent timestamps, host environment variables, unsorted directory listings, or non-deterministic compression parameters violates `NLE-POL-SDR-001` and is permanently barred from deployment in production pipelines.
