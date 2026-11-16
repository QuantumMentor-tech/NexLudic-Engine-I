# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## AI-Provider & External Tool Configuration Policy

**Document ID:** `NLE-POL-AIT-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical AI & Tool Governance Policy  
**Traceability:** `ADR-0008` (AI Provider Abstraction), `ADR-0009` (Workspace Isolation & Approval), `REQ-AI-001` (Prompt Generation), `REQ-SAFE-002` (Local Privacy), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Policy Scope

NexLudic Engine empowers creators by coordinating autonomous AI coding agents, Model Context Protocol (MCP) tool servers, and external modding toolchains (compilers, patchers, decompilers).

Because AI reasoning models and third-party tools are untrusted, fallible external systems, this policy defines:
1. The **Vendor-Neutral AI Provider Configuration Model** decoupling the platform from proprietary cloud APIs.
2. The **Granular Context-Sharing & Privacy Controls** ensuring user project data is not leaked without explicit consent.
3. The **External Toolchain Governance Standards** mandating SHA-256 binary hash pinning and argument allowlisting.
4. The **Graceful Failure & Offline Degradation Mechanisms** guaranteeing full engine usability when external AI services fail or when operating completely offline.

---

## 2. Universal AI-Provider Configuration Model

In strict alignment with `ADR-0008`, AI integrations are managed through a dual-layer abstraction: **Model Context Protocol (MCP)** for standardized tool calling and the **`IAgentRunner`** execution interface:

```
+----------------------------------------------------------------------------------------------------+
|                                AI PROVIDER CONFIGURATION SCHEMA                                    |
|                                                                                                    |
|  {                                                                                                |
|    "provider_id": "google_gemini",                   // Provider discriminator                     |
|    "model_id": "gemini-1.5-pro",                     // Targeted model variant                     |
|    "api_version": "v1beta",                          // Upstream API version                       |
|    "credential_ref": "cred-4bf92f35-77b3-4da6-a3ce", // Opaque DPAPI vault reference               |
|                                                                                                    |
|    "universal_parameters": {                         // Standardized across all providers         |
|      "temperature": 0.2,                             // Deterministic low-entropy code gen         |
|      "max_output_tokens": 8192,                      // Output budget                              |
|      "timeout_seconds": 60,                          // Request deadline budget                    |
|      "max_retries": 2                                // Exponential backoff retries                |
|    },                                                                                              |
|                                                                                                    |
|    "provider_specific_options": {                    // Opaque parameters scoped to provider       |
|      "top_k": 40,                                                                                  |
|      "safety_settings": [ ... ]                                                                    |
|    },                                                                                              |
|                                                                                                    |
|    "data_sharing_consent": {                         // Granular privacy toggles                   |
|      "share_project_blueprint": true,                                                              |
|      "share_source_code_diffs": true,                                                              |
|      "share_diagnostic_logs": false,                                                               |
|      "allow_third_party_training": false                                                           |
|    }                                                                                               |
|  }                                                                                                |
+----------------------------------------------------------------------------------------------------+
```

### Supported Provider Types:
1. `google_gemini`: Google Gemini API via official SDK (`gemini-1.5-pro`, `gemini-1.5-flash`).
2. `anthropic_claude`: Anthropic Claude API / Claude Code CLI (`claude-3-5-sonnet`).
3. `openai_compatible`: OpenAI API or local OpenAI-compatible endpoints (Ollama, vLLM, LM Studio).
4. `local_slm`: In-process or local CLI inference (ONNX Runtime, Llama.cpp).
5. `manual_prompt_handoff`: 100% offline; exports prompts as formatted Markdown files for manual browser copy-paste.

---

## 3. Context-Sharing & Data Privacy Controls

NexLudic Engine enforces strict data minimization before transmitting project information to cloud AI providers:

```
[ Raw Project Workspace State ]
               |
               v
1. Context Extraction Filter:
   - Extract only files explicitly declared in current AI Task Scope.
   - Filter out all files matching .gitignore or .nexludicignore.
               |
               v
2. Secret & Sensitive Data Sanitization Gate:
   - Scan context text for API tokens, passwords, and private drive paths.
   - Replace any detected credentials with [REDACTED].
   - Block transmission if unredacted secrets are detected.
               |
               v
3. User Transparency Disclosure Dialog:
   - Renders exact character count, estimated token cost, and payload diff.
   - Requires explicit click: [Proceed and Transmit to AI Provider].
               |
               v
4. Dispatch Outbound HTTPS Request via IAgentRunner
```

### Prohibited Context Elements:
* Plaintext credentials, DPAPI vault contents, or `credential_ref` tokens.
* Unrelated projects on Volume `D:`.
* Binary game executable bytes or copyrighted proprietary asset archives (`REQ-MOD-003`).

---

## 4. External Toolchain Governance & Binary Verification

External compilers and modding binaries (e.g. `PapyrusCompiler.exe`, `bepinex_patcher.exe`) executed by the engine must be registered in `toolchains.json` and satisfy strict verification rules:

```
+----------------------------------------------------------------------------------------------------+
|                               TOOLCHAIN REGISTRATION & VERIFICATION                                |
+-----------------------+----------------------------------+-----------------------------------------+
| Verification Rule     | Requirement                      | Enforcement Action                      |
+-----------------------+----------------------------------+-----------------------------------------+
| Path Whitelisting     | Absolute path on local machine   | Must exist; symlinks outside dir blocked|
| SHA-256 Hash Pinning  | Binary hash matches known tool   | Hash mismatch blocks execution          |
| Zero Elevation Check  | Medium Integrity Level only      | Rejects executables requesting UAC admin|
| Sandbox Job Object    | Assigned to Win32 Job Object     | 5-min timeout cap, 4 GB RAM commit limit|
| Argument Allowlist    | CLI args match approved template | Shell metacharacters (`&`, `|`, `;`) drop|
| Working Dir Jail      | Staging directory jail           | Cannot execute from System32 or Root C: |
+-----------------------+----------------------------------+-----------------------------------------+
```

---

## 5. External Service Failure & Offline Fallback Modes

When an external AI provider fails, the platform degrades gracefully without blocking core functionality:

```
+-----------------------------+-----------------------------+----------------------------------------+
| Failure Mode                | Trigger Condition           | Graceful Fallback Strategy             |
+-----------------------------+-----------------------------+----------------------------------------+
| **Rate Limit (HTTP 429)**   | Provider quota exhausted    | Exponential backoff with jitter; pause |
|                             |                             | task; offer switch to alternative model.|
| **Auth Failure (HTTP 401)** | Invalid or expired token    | Flag `credential_ref` as `INVALID`;   |
|                             |                             | prompt user to update key in Settings. |
| **Provider Outage (500/503)**| Cloud service unavailable  | Prompt user to retry or switch to      |
|                             |                             | `manual_prompt_handoff` mode.          |
| **Total Offline State**     | Zero network connection     | Core engine remains 100% operational;  |
|                             |                             | AI tasks fall back to local SLM/offline|
+-----------------------------+-----------------------------+----------------------------------------+
```

---

## 6. AI Agent Boundary & Anti-Tampering Rules

In compliance with `ADR-0009`:
1. **No Self-Privilege Escalation:** AI agents CANNOT modify `security_policy.json`, alter staging boundaries, or authorize their own diffs.
2. **Mandatory Human Approval:** Generated blueprints and code changes remain in `<project>/staging/` until the user inspects the unified diff and clicks **Approve**.
3. **Restricted MCP Tools:** AI agents interacting via MCP are supplied solely with safe, sandboxed tools (`read_blueprint`, `write_staged_file`, `validate_syntax`). System shell execution is strictly blocked.

