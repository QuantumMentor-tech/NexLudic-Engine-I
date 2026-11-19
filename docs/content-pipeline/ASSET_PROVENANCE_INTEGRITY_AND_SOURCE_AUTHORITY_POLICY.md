# Asset Provenance, Integrity & Source Authority Policy

**Document Identifier:** `NLE-POL-API-001`  
**Classification:** Information Security & Provenance Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-CON-CSD-001`, `NLE-CON-CPM-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Provenance Principles

Every asset introduced into the NexLudic Engine ecosystem must maintain an unbroken, auditable chain-of-custody. This policy codifies the provenance schema, source authority ranking, cryptographic integrity verification, and handling of unverified or mutated sources.

---

## 2. Six-Tier Source Authority Hierarchy

When evaluating claims of asset authenticity, ownership, or game compatibility, the engine enforces a strict authority ranking:

```
[Tier 1: Project-Internal Signed Artifacts] (HIGHEST AUTHORITY)
   - Cryptographically signed with NexLudic internal release keys (Ed25519).
   - Fully auditable build attestation and Git commit provenance.

[Tier 2: Official Game Vendor SDK & Documentation]
   - Assets and schemas distributed by Bethesda, Epic, Unity, Valve, etc.
   - Official modding kit outputs with vendor digital signatures.

[Tier 3: Verified User-Provided Assets]
   - Assets imported by the user from local storage with explicit confirmation.

[Tier 4: Curated Community Mod Repositories]
   - Packages from vetted community repositories (e.g. NexusMods verified authors).

[Tier 5: External Unverified Third-Party Sources]
   - Arbitrary downloads, public forums, or unknown ZIP files. Requires quarantine.

[Tier 6: AI-Generated Content & Descriptions] (ZERO INDEPENDENT AUTHORITY)
   - AI agent declarations, prompts, or LLM-generated compatibility claims.
   - CANNOT establish copyright ownership, compatibility, or safety.
```

---

## 3. Provenance Attestation Schema (`provenance-attestation.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/provenance-attestation.json",
  "attestation_id": "attest-20261119-001",
  "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "revision_id": "rev-20261119-02",
  "source_origin": {
    "source_id": "src-project-celestial-models",
    "origin_uri": "https://git.nexludic.internal/models/celestial-armory.git",
    "commit_hash": "c4d29a1b8e90f2345678abcdef0123456789abcd",
    "author_identity": "NexLudic Core Modeling Team <art@nexludic.org>"
  },
  "content_integrity": {
    "algorithm": "SHA-256",
    "digest": "8d3e91f0a2b4c6e8d1a3b5c7e9f2a4b6c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8",
    "byte_size": 1420580
  },
  "creation_timestamp": "2026-11-19T17:55:00Z",
  "signature": {
    "key_id": "ed25519-nexludic-release-2026",
    "signature_base64": "v3kL9...8mZb"
  }
}
```

---

## 4. Cryptographic Integrity Verification Contract

1. **Digest Computation:** The engine must independently stream and compute the SHA-256 digest of every candidate asset before metadata extraction.
2. **Digest Matching:** If the computed digest does not match the manifest or attestation, the asset is immediately marked `INTEGRITY_MISMATCH` and rejected (`ERR_PRV_INTEGRITY_MISMATCH`).
3. **Mid-Discovery Mutation Detection:** If a source file's size or timestamp changes between discovery (Stage C) and validation (Stage E), discovery is invalidated and must be restarted (`ERR_PRV_SOURCE_MUTATED`).

---

## 5. Provenance Failures & Quarantining

If an asset lacks valid provenance:
- It MUST NOT be marked `VERIFIED_PROVENANCE`.
- It is assigned status `UNRESOLVED_PROVENANCE`.
- If the asset is targeted for local, private analysis only, it may proceed in a quarantined sandbox if authorized by the user.
- If the asset is targeted for redistribution or packaging, it is BLOCKED by policy (`ERR_PRV_REDISTRIBUTION_DENIED`).
