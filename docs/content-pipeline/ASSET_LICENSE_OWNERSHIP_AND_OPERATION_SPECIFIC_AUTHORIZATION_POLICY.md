# Asset License, Ownership & Operation-Specific Authorization Policy

**Document Identifier:** `NLE-POL-ALO-001`  
**Classification:** Legal Compliance & Authorization Governance Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 62 of 100  
**Upstream Dependencies:** `NLE-SPC-CID-001`, `NLE-MOD-CDR-001`, `NLE-SEC-CSG-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Legal Engineering Principles

A fundamental premise of NexLudic Engine is that **technical discoverability does not constitute legal permission**. An asset being present in a user directory or game folder does not grant rights to extract, transcode, repackage, or distribute it.

This policy specifies how the ingestion engine parses, validates, and enforces intellectual property (IP) rights, SPDX licensing declarations, and nine granular, operation-specific authorization gates.

---

## 2. Nine Discrete Operation Authorization Gates

To prevent unauthorized usage or redistribution of proprietary game content, permissions are evaluated independently across nine granular activity boundaries:

| Gate | Permitted Activity | Default Rule for Internal Content | Default Rule for Game Assets | Default Rule for Untrusted Mod |
| :--- | :--- | :--- | :--- | :--- |
| **G1: Discover** | Scan directory or package index to list candidates. | Allowed | Allowed (Read-only) | Allowed |
| **G2: Read** | Read binary header and stream bytes into RAM. | Allowed | Allowed | Allowed (Sandbox) |
| **G3: Analyze** | Extract metadata, inspect topology, check magic bytes.| Allowed | Allowed | Allowed (AST/Static) |
| **G4: Transform**| Transcode format (e.g. glTF $\rightarrow$ NIF / DDS). | Allowed | Requires Fair-Use / Perm | Allowed (If Licensed) |
| **G5: Local Package**| Bundle asset into local `.nlpkg` for private use. | Allowed | Allowed for Local User | Allowed |
| **G6: Apply to Game**| Stage and copy asset into user's game directory. | Allowed | Allowed (Offline/Single) | Requires User Sign-off |
| **G7: Local Share**| Transfer `.nlpkg` over LAN to authenticated peer. | Allowed | **DENIED (Proprietary)** | Requires License Perm |
| **G8: Redistribute**| Upload `.nlpkg` to public community repository. | Allowed (If Open) | **STRICTLY PROHIBITED** | Requires Explicit Perm |
| **G9: Publish Mashup**| Distribute a commercial or standalone mashup experience.| Allowed (If Owned) | **STRICTLY PROHIBITED** | **STRICTLY PROHIBITED** |

> [!IMPORTANT]
> Raw or converted proprietary commercial game assets (extracted from retail games) are strictly locked to gates G1–G6 on the local user's machine. They MUST NEVER be authorized for gates G7, G8, or G9.

---

## 3. License Metadata Schema (`license-metadata.json`)

```json
{
  "$schema": "https://nexludic.org/schemas/v1/license-metadata.json",
  "asset_id": "asset.nexludic.skyrim.mesh.celestial-blade",
  "license_spdx": "CC-BY-4.0",
  "copyright_notice": "Copyright (c) 2026 NexLudic Art Contributors",
  "attribution_required": true,
  "attribution_string": "Celestial Blade modeled by ArtTeam (NexLudic)",
  "commercial_use_permitted": false,
  "modification_permitted": true,
  "redistribution_permitted": true,
  "source_code_disclosure_required": false,
  "known_restrictions": [
    "Non-commercial attribution required in downstream credits."
  ],
  "verification_evidence": {
    "license_file_sha256": "3a8c...9f2b",
    "verified_by": "Compliance_Automated_Audit_v1.0"
  }
}
```

---

## 4. Handling Missing or Uncertain Licensing

When licensing metadata cannot be verified:
1. **Unresolved State:** The asset is marked `NEEDS_LICENSE_REVIEW`.
2. **Local Bounded Use:** The asset may be authorized for G1, G2, G3, and G5 (private offline design) if the user confirms ownership.
3. **Public Operation Block:** All redistribution gates (G7, G8, G9) are locked by policy (`ERR_AUTH_LICENSE_UNRESOLVED`).
4. **No Circumvention:** The engine must never spoof license tags or bypass DRM/EULA terms to enable sharing.
