# Capability Evidence and Declaration Consistency Rules

**Document ID:** `NLE-RUL-EDC-001`  
**Version:** 1.0.0  
**Phase:** 5 of 12 — Universal Adapter SDK & Capability System  
**Step:** 36 of 100 — Capability Declaration Validation & Conformance Rules Specification  
**Classification:** Evidence Integrity, Conflict Detection & Epistemic Audit Rule Catalog  
**Status:** Approved Specification  
**Authoritative Reference:** `NLE-SPC-CVC-001`, `NLE-SPC-ECL-001`, `NLE-CAT-RIA-001`, `NLE-MOD-CDV-001`

---

## 1. Catalog Scope & Objectives

This catalog defines the formal, deterministic rules evaluated during **Stage G (Evidence-Reference Integrity)** and **Stage H (Declaration Consistency and Conflict Detection)** of the Capability Declaration Validation Pipeline.

These rules ensure that:
1. Every claim citation (`EVD-xxxx`) points to a legitimate, cryptographically verified record in the Compatibility Evidence Catalog (`Step 30`).
2. An adapter’s declared capabilities do not contradict its own dependencies or metadata.
3. Declarations claiming full support without evidence are downgraded from achieving unearned certification badges.

---

## 2. Stage G: Evidence-Reference Integrity Rules (`VLD-EVD`)

Stage G validates that cited evidence records exist, match the declared game context, and possess adequate epistemic maturity.

```
+----------------------------------------------------------------------------------------------------+
|                                    STAGE G: EVIDENCE RULES SUMMARY                                 |
+-------------+------------------------------------+----------+------------------+-------------------+
| Rule ID     | Rule Title                         | Severity | Blocks Admission | Diagnostic Code   |
+-------------+------------------------------------+----------+------------------+-------------------+
| VLD-EVD-001 | Evidence Record Existence          | ERROR    | YES (If cited)   | `ERR-VAL-EVD-001` |
| VLD-EVD-002 | Evidence Scope Alignment           | ERROR    | YES              | `ERR-VAL-EVD-002` |
| VLD-EVD-003 | Evidence Freshness & Expiration    | WARNING  | NO               | `WRN-VAL-EVD-003` |
| VLD-EVD-004 | Cryptographic Digest Parity Check  | ERROR    | YES              | `ERR-VAL-EVD-004` |
| VLD-EVD-005 | Evidence Absence Downgrade         | WARNING  | NO (Downgrades)  | `WRN-VAL-EVD-005` |
+-------------+------------------------------------+----------+------------------+-------------------+
```

### 2.1 Rule `VLD-EVD-001`: Evidence Record Existence
- **Purpose:** Ensure that evidence identifiers cited in declarations resolve to valid records.
- **Authority:** `NLE-SPC-ECL-001` (Step 30 Evidence Lifecycle).
- **Target Field:** `provenanceAndEvidence.evidenceReferences[]`.
- **Validation Logic:** For every cited key (e.g. `EVD-8021`), query Evidence Catalog. Assert that record exists.
- **Severity:** `ERROR` (Blocks qualification of the specific declaration).
- **Diagnostic Code:** `ERR-VAL-EVD-001`
- **Error Message:** *"Cited evidence record '{evidenceId}' does not exist in the Compatibility Evidence Catalog."*
- **Remediation:** Verify evidence ID or ingest source documentation into catalog prior to citing.
- **Test ID:** `TC-CAP-VAL-027`

### 2.2 Rule `VLD-EVD-002`: Evidence Scope Alignment
- **Purpose:** Prevent citing evidence from an unrelated game or incompatible version.
- **Authority:** `NLE-RUL-VCE-001` (Step 28 Version Evidence Rules).
- **Validation Logic:** Assert that `EvidenceRecord.targetGameId == Declaration.canonicalGameId`. Assert that the evidence version range overlaps with the declared version range.
- **Severity:** `ERROR` (Blocks evidence association).
- **Diagnostic Code:** `ERR-VAL-EVD-002`
- **Error Message:** *"Evidence '{evidenceId}' is scoped to game '{evdGame}' and cannot be used for target game '{declGame}'."*
- **Remediation:** Cite evidence records specifically evaluated against the target game title.
- **Test ID:** `TC-CAP-VAL-028`

### 2.3 Rule `VLD-EVD-003`: Evidence Freshness & Expiration Check
- **Purpose:** Alert authors when cited evidence has expired under 90-day retention policies.
- **Authority:** `NLE-POL-FCD-001` (Step 30 Freshness Policy).
- **Validation Logic:** If `EvidenceRecord.IsFresh == false` or `Status == "SUPERSEDED"`, emit warning diagnostic.
- **Severity:** `WARNING` (Non-blocking).
- **Diagnostic Code:** `WRN-VAL-EVD-003`
- **Warning Message:** *"Cited evidence record '{evidenceId}' is stale or superseded. Re-verification recommended."*
- **Remediation:** Refresh evidence citation or re-run benchmark test harness.
- **Test ID:** `TC-CAP-VAL-029`

### 2.4 Rule `VLD-EVD-004`: Cryptographic Digest Parity Check
- **Purpose:** Detect tampering or corruption in local cached evidence files.
- **Authority:** `NLE-SPC-EPS-001` (Step 30 Cryptographic Schema).
- **Validation Logic:** Compute SHA-256 of local cached artifact; assert it matches `EvidenceRecord.contentHash`.
- **Severity:** `ERROR` (Blocks evidence qualification).
- **Diagnostic Code:** `ERR-VAL-EVD-004`
- **Error Message:** *"Cryptographic hash mismatch for evidence '{evidenceId}'. Local cached file may be corrupted."*
- **Remediation:** Re-download or re-generate evidence artifact from authoritative catalog repository.
- **Test ID:** `TC-CAP-VAL-030`

### 2.5 Rule `VLD-EVD-005`: Evidence Absence Downgrade
- **Purpose:** Prevent bare claims without citations from satisfying user requirements.
- **Authority:** `NLE-MOD-CDV-001` Section 4 (Anti-Deception Invariants).
- **Validation Logic:** If `declaredSupportLevel == "SUPPORTED"` but `evidenceReferences` is empty, emit warning and downgrade rule input state to `INPUT_UNKNOWN`.
- **Severity:** `WARNING` (Non-blocking for syntax, but blocks requirement certification).
- **Diagnostic Code:** `WRN-VAL-EVD-005`
- **Warning Message:** *"Capability '{capabilityId}' claims SUPPORTED without supporting evidence. Claim treated as unverified hypothesis."*
- **Remediation:** Provide at least one Tier 1 or Tier 2 evidence citation to certify support.
- **Test ID:** `TC-CAP-VAL-031`

---

## 3. Stage H: Declaration Consistency and Conflict Detection Rules (`VLD-CNF`)

Stage H detects internal contradictions within an adapter manifest or across multiple overlapping declarations.

```
+----------------------------------------------------------------------------------------------------+
|                                    STAGE H: CONFLICT RULES SUMMARY                                 |
+-------------+------------------------------------+----------+------------------+-------------------+
| Rule ID     | Rule Title                         | Severity | Blocks Admission | Diagnostic Code   |
+-------------+------------------------------------+----------+------------------+-------------------+
| VLD-CNF-001 | Duplicate Capability in Same Scope | ERROR    | YES              | `ERR-VAL-CNF-001` |
| VLD-CNF-002 | Declared vs Dependency Contradict. | ERROR    | YES              | `ERR-VAL-CNF-002` |
| VLD-CNF-003 | Limitation vs Support Contradiction| ERROR    | YES              | `ERR-VAL-CNF-003` |
| VLD-CNF-004 | Conflicting Overlapping Adapters   | ERROR    | YES (At Binding) | `ERR-VAL-CNF-004` |
+-------------+------------------------------------+----------+------------------+-------------------+
```

### 3.1 Rule `VLD-CNF-001`: Duplicate Capability Declaration in Same Scope
- **Purpose:** Prevent ambiguous multiple declarations for the same capability within a single adapter.
- **Authority:** `NLE-SCH-ACD-001` Section 1.
- **Validation Logic:** Assert that no two capability declarations within the same manifest share identical `capabilityId` and overlapping `supportedVersionRanges`.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-CNF-001`
- **Error Message:** *"Duplicate declaration for capability '{capabilityId}' with overlapping version scope."*
- **Remediation:** Consolidate declarations into a single entry or partition version ranges cleanly.
- **Test ID:** `TC-CAP-VAL-032`

### 3.2 Rule `VLD-CNF-002`: Declaration vs. Dependency Contradiction
- **Purpose:** Prevent an adapter from claiming support while declaring that a required dependency is missing.
- **Authority:** `NLE-CON-CDC-001` Section 4.
- **Validation Logic:** If Capability A is declared `SUPPORTED`, assert that no capability listed in its `HARD_PREREQUISITE` dependencies is simultaneously declared `UNSUPPORTED` within the same manifest.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-CNF-002`
- **Error Message:** *"Contradiction: Capability '{capA}' is marked SUPPORTED, but its prerequisite '{capB}' is declared UNSUPPORTED."*
- **Remediation:** Resolve dependency or downgrade parent capability to `UNSUPPORTED`.
- **Test ID:** `TC-CAP-VAL-033`

### 3.3 Rule `VLD-CNF-003`: Limitation vs. Support Contradiction
- **Purpose:** Prevent deceptive declarations where text limitations completely refute claimed support.
- **Authority:** `NLE-SCH-ACD-001` Section 3.5.
- **Validation Logic:** Scan `knownLimitations` for contradiction patterns asserting that the primary capability does not function in any version.
- **Severity:** `ERROR` (Blocks qualification).
- **Diagnostic Code:** `ERR-VAL-CNF-003`
- **Error Message:** *"Known limitation '{limitationText}' directly refutes claimed SUPPORTED status."*
- **Remediation:** Mark capability as `UNSUPPORTED` if the feature is completely broken.
- **Test ID:** `TC-CAP-VAL-034`

### 3.4 Rule `VLD-CNF-004`: Conflicting Overlapping Adapters
- **Purpose:** Prevent two adapters from claiming incompatible hooks into the same game process.
- **Authority:** `NLE-CON-CDC-001` Section 2.1 (`MUTUALLY_EXCLUSIVE`).
- **Validation Logic:** During project workspace binding, if Adapter 1 and Adapter 2 both declare `MUTUALLY_EXCLUSIVE` capabilities, halt binding.
- **Severity:** `ERROR` (Blocks project binding).
- **Diagnostic Code:** `ERR-VAL-CNF-004`
- **Error Message:** *"Adapter '{adapterA}' conflicts with '{adapterB}' on mutually exclusive capability '{capabilityId}'."*
- **Remediation:** Select only one of the conflicting adapters for active project execution.
- **Test ID:** `TC-CAP-VAL-035`
