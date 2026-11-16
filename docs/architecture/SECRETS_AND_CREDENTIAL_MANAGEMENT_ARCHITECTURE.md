# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Secrets & Credential Management Architecture

**Document ID:** `NLE-ARC-SEC-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 15 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Security Architecture Specification  
**Traceability:** `REQ-SEC-001` (Zero Elevation), `REQ-SAFE-002` (Local Privacy), `ADR-0007` (Storage Baseline), `ADR-0008` (AI Abstraction), `NLE-ARC-CFG-001` (Configuration Architecture)

---

## 1. Executive Summary & Security Objectives

NexLudic Engine allows users to integrate external AI reasoning models (such as Google Gemini, Anthropic Claude, or OpenAI) to assist in game mod scaffolding, syntax analysis, and error diagnosis.

Because external AI providers require authentication tokens, the platform must guarantee that:
1. **Credentials Are Never Exposed in Plaintext:** Secrets are never written to project manifests, git working trees, export packages, diagnostic logs, or console streams.
2. **References Are Strictly Segregated from Secret Values:** Project configurations contain only opaque, non-secret UUID pointers (`credential_ref`).
3. **Storage Leverages Native Windows Cryptography:** Tokens are encrypted on disk using the **Windows Data Protection API (DPAPI)**, binding decryption keys strictly to the local Windows user login token.
4. **Least-Privilege In-Memory Access:** Credentials are decrypted only at the exact instant an outbound HTTPS request is dispatched and zeroed out immediately after use. External game adapters and AI model prompt contexts NEVER receive raw credentials.

---

## 2. The Opaque Secret-Reference Model

To decouple project configurations from machine-specific credentials, NexLudic Engine enforces a strict **Secret Reference Architecture**:

```
+----------------------------------------------------------------------------------------------------+
|                                 OPAQUE SECRET REFERENCE MODEL                                     |
|                                                                                                    |
|  [ IN-PROJECT MANIFEST: project.json (GIT-TRACKABLE & EXPORTABLE) ]                                |
|  {                                                                                                |
|    "ai_configuration": {                                                                           |
|      "provider": "google_gemini",                                                                  |
|      "model": "gemini-1.5-pro",                                                                    |
|      "credential_ref": "cred-9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"  <-- OPAQUE UUID REFERENCE    |
|    }                                                                                              |
|  }                                                                                                |
+----------------------------------------------------------------------------------------------------+
                                             |
                                             | Resolves pointer via ISecurityContext
                                             v
+----------------------------------------------------------------------------------------------------+
|                       ISOLATED LOCAL VAULT: %LocalAppData%\NexLudic\credentials.dat                |
|                       (ENCRYPTED VIA WINDOWS DPAPI - CURRENT USER SCOPE)                           |
|                                                                                                    |
|  Vault Entry [cred-9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d]:                                         |
|  - Provider: "google_gemini"                                                                       |
|  - Encrypted Blob: [0x01, 0x00, 0x00, 0x00, 0xD0, 0x8C, 0x9D, ... DPAPI Ciphertext]              |
|  - CreatedUtc: "2026-11-16T12:00:00Z"                                                             |
|  - Purpose: "AI_GENERATION"                                                                       |
|  - Status: "VALIDATED"                                                                             |
+----------------------------------------------------------------------------------------------------+
```

### Key Architectural Invariants:
* Copying or sharing `project.json` leaks **ZERO CREDENTIALS**. A recipient cannot decrypt or access the original creator's token.
* If a project is imported onto a new workstation where the `credential_ref` does not exist, the Core flags the reference as `UNRESOLVED` and prompts the user to select or input a local credential.

---

## 3. Storage Mechanism Evaluation & Technical Baseline

| Mechanism Evaluated | Windows Compatibility | Security Profile | Dependencies | Decision Status | Rationale |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Windows DPAPI (`ProtectedData`)** | Native Win32 / .NET | High: User-token master key derivation; zero hardcoded keys | Zero (.NET runtime built-in) | **APPROVED BASELINE** | Native OS security, standard user compatible, zero third-party dependencies. |
| **Windows Credential Manager** | Native Win32 API | High: Secure OS store | P/Invoke or Win32 library | **Approved Alternative** | Excellent for interactive user creds; DPAPI selected for unified vault serialization. |
| **Plaintext `.env` / JSON** | Universal | **CRITICAL RISK:** Accidental git commits & leaks | Zero | **STRICTLY PROHIBITED** | Violates `REQ-SEC-001` and `REQ-SAFE-002`. |
| **Cloud Vault (HashiCorp / AWS)**| Network-dependent | High, but violates local-first mandate | Heavy external network stack | **REJECTED** | Violates local-first offline requirements (`REQ-SAFE-002`). |

### 3.1 DPAPI Cryptographic Implementation Specification
* **API:** `System.Security.Cryptography.ProtectedData.Protect` and `Unprotect`.
* **Scope:** `DataProtectionScope.CurrentUser` (Restricts decryption strictly to the logged-in Windows user account).
* **Optional Entropy:** A machine-specific, application-derived salt (SHA-256 of user SID + local machine GUID) is passed to `optionalEntropy` to provide defense-in-depth against malicious local processes.

---

## 4. Formal Credential Lifecycle State Machine

```
      [User Inputs Token]
               |
               v
     +-------------------+          Token Verified via Canary Ping          +-----------+
     | REFERENCE_DEFINED | -----------------------------------------------> | VALIDATED |
     +-------------------+                                                  +-----------+
               |                                                                  |
               | Network Timeout / DNS Fail                                       | API 401 Unauthorized
               v                                                                  v
     +-------------------+         User Updates or Deletes Key              +-----------+
     |    UNVERIFIED     | <----------------------------------------------- |  INVALID  |
     +-------------------+                                                  +-----------+
               |                                                                  |
               | Explicit Removal                                                 | Revoked
               v                                                                  v
     +-------------------+ <------------------------------------------------------+
     |      REMOVED      |
     +-------------------+
```

### 4.1 State Definitions:
* **REFERENCE_DEFINED:** Token entered, encrypted with DPAPI, stored in vault, but not yet tested against provider endpoint.
* **VALIDATED:** Core successfully executed a lightweight canary metadata call (e.g. `models.list`) using the token.
* **UNVERIFIED:** Canary check failed due to transient network failure or timeout; credential remains stored.
* **INVALID:** Provider endpoint returned a permanent `401 Unauthorized` or `403 Forbidden` response.
* **REMOVED:** Cryptographic zeroing of memory and deletion from the DPAPI vault file.

---

## 5. Access Control Boundaries & Least Privilege Retrieval

```
+----------------------------------------------------------------------------------------------------+
|                                    CREDENTIAL ACCESS BOUNDARIES                                    |
+--------------------------+---------------------+-----------------------+---------------------------+
| Actor / Subsystem        | Can Read Plaintext? | Can Read Opaque Ref?  | Permitted Operation       |
+--------------------------+---------------------+-----------------------+---------------------------+
| Desktop Settings UI      | NO (Masked input)   | YES                   | Enter / Update Token      |
| Project Manifest Writer  | **FORBIDDEN**       | YES (`credential_ref`)| Serialize `project.json`  |
| AI Orchestration Engine  | **YES (Ephemeral)** | YES                   | Sign outbound HTTPS req   |
| Model Prompt Context     | **STRICTLY BLOCKED**| **STRICTLY BLOCKED**  | Assemble LLM prompt text  |
| External Game Adapters   | **STRICTLY BLOCKED**| **STRICTLY BLOCKED**  | Execute modding tools     |
| Serilog Diagnostic Sink  | **STRICTLY BLOCKED**| YES                   | Emit structured logs      |
| Project Exporter         | **STRICTLY BLOCKED**| **STRICTLY BLOCKED**  | Build `.nxpkg` package    |
+--------------------------+---------------------+-----------------------+---------------------------+
```

---

## 6. Secret Redaction & Sanitization Standard

1. **Serilog Masking Sink:** All logging sinks in `NexLudic.Infrastructure` utilize regex interceptors:
   * Patterns: `Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*`, `(?:AIzaSy|sk-[a-zA-Z0-9]{20,})`.
   * Replacement: `[REDACTED_API_TOKEN]`.
2. **UI Display Masking:** UI text fields render tokens as `sk-ant-api...****` showing only the first 6 and last 4 characters.
3. **In-Memory Lifespan:** Plaintext string credentials are converted to pinned `byte[]` or `SecureString` structures, decrypted solely for the duration of HTTP request construction, and zeroed via `Array.Clear` immediately after transmission.

