# Host-Game Modification Failure, Resource & Diagnostic Policy

**Document Identifier:** `NLE-POL-GMF-001`  
**Classification:** Reliability Engineering & Diagnostics Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-CON-CPS-001`, `NLE-POL-GIP-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Scope & Reliability Objectives

This policy defines failure taxonomies, resource bounds, diagnostic data collection, and remediation boundaries for the content pipeline and host-game modification engine.

---

## 2. Failure Severity Taxonomy

| Severity Level | Definition | Pipeline Reaction | Recovery Action |
| :--- | :--- | :--- | :--- |
| **FATAL_SECURITY** | Path traversal, unsigned script, malicious payload, anti-cheat risk. | Immediate halt; delete staging sandbox. | Quarantine package; alert user; log event. |
| **FATAL_IO** | Disk write failure, permission denied, target file locked. | Abort current stage; trigger rollback. | Execute `ROLLBACK_TRANSACTION`; preserve journal. |
| **RECOVERABLE_DEP**| Missing dependency, version conflict. | Pause pipeline at Stage J. | Offer automatic download or alternative version. |
| **NON_FATAL_WARN** | Optional asset format unsupported (e.g. ambient SFX). | Log warning; proceed with degradation. | Record warning in final AppliedModificationRecord. |

---

## 3. Resource Governance & Pipeline Ceilings

To prevent system exhaustion during large asset compilations:
- **Maximum Staging Cache Size:** 25 GB per pipeline execution run.
- **Maximum Asset File Size:** 4 GB per single asset; 10 GB per `.nlpkg` archive.
- **Decompression Ratio Ceiling:** $100:1$ (Decompression bomb mitigation).
- **Worker Thread Pool:** Bounded to $\max(1, \text{LogicalCores} - 2)$ to maintain desktop UI responsiveness.
- **Memory Ceiling:** Transcoder processes bounded to 4 GB RAM per worker.

---

## 4. Diagnostic Bundle Architecture (`.nlediag`)

When a pipeline failure occurs or is manually requested by the user, the diagnostic engine packages a standardized `.nlediag` archive containing:
1. `pipeline_run.json`: Complete state transitions, timings, and error codes.
2. `environment_profile.json`: OS version, GPU model, driver version, storage free space (anonymized, zero PII).
3. `target_game_descriptor.json`: Detected engine, store platform, build version, mod loader status.
4. `journal_excerpt.json`: Copy of the deployment journal (if Stage N was reached).
5. `adapter_stdout.log`: Sanitized log output from the game adapter transcode worker.
