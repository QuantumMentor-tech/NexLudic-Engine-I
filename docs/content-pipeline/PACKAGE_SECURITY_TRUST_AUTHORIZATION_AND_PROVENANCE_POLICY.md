# Package Security, Trust, Authorization & Provenance Policy

**Document Identifier:** `NLE-POL-PST-001`  
**Classification:** Information Security & Packaging Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 64 of 100  
**Upstream Dependencies:** `NLE-SPC-PVA-001`, `NLE-POL-CIS-001`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Threat Model

A package definition aggregates diverse binary and structured assets into a unified container. This aggregation introduces specific security risks:
1. **Archive Path Traversal (Zip Slip):** Malicious destination paths attempting to overwrite system files upon unbundling.
2. **Decompression Bomb Payloads:** High-ratio archives designed to exhaust disk or memory quotas.
3. **Privilege Over-Extension:** Assuming that permission to create a local package grants permission to install or redistribute.
4. **AI Coding-Agent Hallucination:** Automated agents declaring dependencies or compatibility without empirical validation.

---

## 2. Path Sanitization & Destination Confinement

Every member's `target_relative_path` in `manifest.json` must pass strict sanitization:
- **Disallow Path Traversal:** Reject any path containing `..`, `./`, `.\`, or null bytes (`\0`).
- **Disallow Absolute Schemes:** Reject paths starting with `C:\`, `/`, `\\`, or drive letter identifiers.
- **Disallow Windows Reserved Names:** Reject paths using `CON`, `PRN`, `AUX`, `NUL`, `COM1..9`, `LPT1..9`.
- **Virtual Staging Root Confinement:**
  $$\text{target\_path.canonicalize().starts\_with(virtual\_staging\_root)}$$

---

## 3. Six Granular Packaging Authorization Boundaries

| Boundary Gate | Permitted Activity | Default Rule for Internal | Default Rule for Retail Game Assets |
| :--- | :--- | :--- | :--- |
| **Gate 1: Read Source** | Stream source bytes into RAM for inspection. | Allowed | Allowed |
| **Gate 2: Transcode** | Convert format via sandboxed worker. | Allowed | Allowed (Fair Use / Local) |
| **Gate 3: Package Membership** | Include artifact in local package definition. | Allowed | Allowed for Local User |
| **Gate 4: Container Build** | Emit physical `.nlpkg` archive to staging disk.| Allowed | Allowed (Local Staging) |
| **Gate 5: Game Installation** | Copy or link files into live game directories. | Requires Stage M/N Sign-off | Requires Stage M/N Sign-off |
| **Gate 6: Redistribution** | Upload package to remote public repository. | Allowed (If Open License) | **STRICTLY PROHIBITED** |

---

## 4. AI Coding-Agent Guardrails

In compliance with Phase 7 governance:
- **AI Suggestions are Untrusted:** An AI agent proposing dependency links or manifest metadata is treated as Tier 3 untrusted input.
- **Deterministic Verification Required:** All AI-proposed package definitions must pass deterministic schema and DAG cycle validation before acceptance.
- **Zero Policy Bypass:** AI agents CANNOT issue waivers, bypass security gates, or approve unverified third-party content.
