# Error Taxonomy and User Message Catalog

**Document ID:** NLE-CAT-ERR-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Classification:** UI Architecture — Error Catalog  

---

## 1. Purpose and Scope

This catalog defines the 13 canonical error categories for NexLudic Engine, specifying severity, blocking status, message patterns, redaction rules, grouping behaviour, persistence classification, and recovery destination for each. It is the authoritative reference for error-message design across all forms and operation surfaces.

**Message patterns in this document are examples** illustrating structure and language. Exact wording is an implementation detail and may be refined during implementation provided the actionability requirements of `NLE-SPC-FVR-001 §3.4` are met.

**Authoritative references:** `NLE-SPC-FVR-001`, `NLE-DOM-SPEC-001`, `REQ-SEC-001`, `REQ-SAFE-001`, `DEC-009`.

---

## 2. Catalog Entry Format

Each category entry includes:

| Field | Description |
|---|---|
| **Category ID** | `CAT-NN` |
| **Name** | Human-readable category name |
| **Severity** | Critical / Error / Warning / Info |
| **Blocking** | Whether the error prevents stage advance or submit |
| **Dismissible** | Whether the user can dismiss the error without resolving it |
| **Persistence** | Transient (clears on resolution) / Persistent (stays until corrected) / Session (clears on restart) |
| **Presentation Component** | Which component renders this error |
| **Redaction Rules** | What must never appear in user-visible message |
| **Grouping** | How multiple instances are grouped |
| **Message Pattern(s)** | Example messages (not prescriptive wording) |
| **Recovery Destination** | Where the user is directed after error |
| **Domain References** | Relevant domain model concepts |

---

## 3. CAT-01 — Input Validation Error

| Field | Value |
|---|---|
| **Category ID** | CAT-01 |
| **Name** | Input Validation Error |
| **Severity** | Error |
| **Blocking** | Yes — prevents stage advance or form submission |
| **Dismissible** | No — clears only when the field value satisfies all rules |
| **Persistence** | Persistent until corrected |
| **Presentation** | `NleInlineValidation` (CMP-010) below the offending field; Form-level error summary when ≥ 2 fields invalid |
| **Redaction** | No internal paths, no domain enum class names, no raw regex patterns exposed |
| **Grouping** | Each field shows its own error; summary lists all fields with errors |
| **Recovery Destination** | Focus returned to first invalid field |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Required field empty | "[Field label] is required." |
| Length exceeded | "[Field label] must be [n] characters or fewer." |
| Format mismatch | "[Field label] contains characters that are not allowed. Use only [description]." |
| Pattern failure | "[Field label] must follow the format [description]. Example: [example]." |
| Whitespace-only | "[Field label] cannot be blank spaces. Enter a value or leave the field empty." |

**Domain References:** Any `Project`, `Requirement`, `GameTitle`, `GameVersion`, `GameEnvironment` attribute subject to Tier 1 or Tier 2 validation.

---

## 4. CAT-02 — Incomplete but Permitted State

| Field | Value |
|---|---|
| **Category ID** | CAT-02 |
| **Name** | Incomplete but Permitted |
| **Severity** | Info |
| **Blocking** | No |
| **Dismissible** | Yes |
| **Persistence** | Transient — shown at field blur; disappears on user action |
| **Presentation** | Helper text below field (advisory, not error styling); `NleStatusBanner` (Info) at stage level if surfaced collectively |
| **Redaction** | N/A — no sensitive data involved |
| **Grouping** | Per-field advisory; not included in error summary count |
| **Recovery Destination** | None required — user may optionally complete the field |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| UNKNOWN version | "Version not specified. NexLudic Engine will use available detection data." |
| UNEVALUATED feasibility | "Feasibility has not yet been evaluated. This is normal for new projects." |
| UNEVALUATED evidence | "Evidence not yet evaluated. This is normal for new requirements." |
| Optional field empty | "[Field label] is optional. You can add this later in project settings." |

**Domain References:** `NLE-DOM-SPEC-001` epistemic tiers — `UNKNOWN`, `UNEVALUATED`.

**Critical Rule:** CAT-02 states must never be rendered as CAT-01 (Error). `UNKNOWN` and `UNEVALUATED` are valid domain states, not errors.

---

## 5. CAT-03 — Warning (Non-Blocking)

| Field | Value |
|---|---|
| **Category ID** | CAT-03 |
| **Name** | Warning — Non-Blocking |
| **Severity** | Warning |
| **Blocking** | No — user may proceed |
| **Dismissible** | Yes |
| **Persistence** | Session — shown until dismissed or the condition resolves |
| **Presentation** | `NleStatusBanner` (Warning level) at form or stage level |
| **Redaction** | No internal implementation details |
| **Grouping** | Multiple warnings grouped in a collapsible warning section |
| **Recovery Destination** | User proceeds; warning remains visible |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Strategy risk advisory | "The selected strategy combination may produce unpredictable results for some game versions. Review before proceeding." |
| Configuration advisory | "No AI provider is configured. AI-assisted operations will not be available until one is added in Project Configuration." |
| Disk space advisory | "Less than 20 GB is available on D:\\. Projects require at least 10 GB. Consider freeing space." |
| Capability coverage gap | "Some integration objectives do not yet have corresponding capability requirements. Consider adding requirements before proceeding." |

**Domain References:** Strategy configuration, capability coverage, infrastructure availability.

---

## 6. CAT-04 — Operation Rejection

| Field | Value |
|---|---|
| **Category ID** | CAT-04 |
| **Name** | Operation Rejection |
| **Severity** | Critical |
| **Blocking** | Yes — the operation cannot be performed; Confirm/Proceed button hidden |
| **Dismissible** | No — cannot be dismissed without resolving the underlying condition or abandoning the operation |
| **Persistence** | Persistent — remains until condition is resolved |
| **Presentation** | `NleStatusBanner` (Critical) above action buttons; Confirm button hidden (not disabled) |
| **Redaction** | No game-internal identifiers, no installation paths, no raw exception names |
| **Grouping** | At most one CAT-04 per pre-flight (the highest-severity blocker shown; additional blockers listed below) |
| **Recovery Destination** | User must correct the blocking condition or abandon the wizard; no partial workaround |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Anti-cheat hard block | "[Game name] uses active anti-cheat protection that is incompatible with NexLudic Engine. This project cannot be created with this game pair. Select different games." |
| Authorization block | "This operation requires permissions that are not available in the current context." |
| Business-rule violation | "The selected combination conflicts with [rule description]. [Corrective instruction]." |

**Domain References:** `REQ-SAFE-001` (anti-cheat), `REQ-SEC-001` (authorization). The anti-cheat hard block is the canonical CAT-04 instance.

---

## 7. CAT-05 — Resource Unavailable

| Field | Value |
|---|---|
| **Category ID** | CAT-05 |
| **Name** | Resource Unavailable |
| **Severity** | Error |
| **Blocking** | Blocks pre-flight and infrastructure operations; does not block form input editing |
| **Dismissible** | No on pre-flight; Yes on advisory |
| **Persistence** | Persistent — until resource becomes available and pre-flight is re-run |
| **Presentation** | `NleStatusBanner` (Error) at pre-flight or operation level |
| **Redaction** | Do not expose full filesystem path; use relative path relative to workspace root or redacted version |
| **Grouping** | Each resource failure listed separately |
| **Recovery Destination** | User resolves the resource issue (e.g., reconnect drive, check permissions); re-runs pre-flight |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Path not accessible | "The workspace location could not be accessed. Verify the path exists and you have write permission." |
| Volume unavailable | "Volume D:\\ is not available. Reconnect the drive and try again." |
| File locked | "A required file is in use by another process. Close any applications accessing it and try again." |
| Service unavailable | "A required background service is not running. Restart NexLudic Engine and try again." |

**Domain References:** `DEC-009` (Volume D), `REQ-SEC-001` (Medium IL — no elevation).

---

## 8. CAT-06 — Persistence Failure

| Field | Value |
|---|---|
| **Category ID** | CAT-06 |
| **Name** | Persistence Failure |
| **Severity** | Error |
| **Blocking** | Blocks navigation away if save was user-initiated and failed |
| **Dismissible** | No until resolved or user explicitly abandons |
| **Persistence** | Persistent — `IsDirty` remains true (INV-001) |
| **Presentation** | `NleStatusBanner` (Error) above action buttons; Retry button offered if eligible per `NLE-SPC-REC-001` |
| **Redaction** | No internal paths, no OS error codes in user message |
| **Grouping** | Per-save-operation; multiple failures listed if partial |
| **Recovery Destination** | Remain on form; offer retry if eligible; offer diagnostics export |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| General write failure | "Your changes could not be saved. Your work is preserved. Try saving again or contact support if the problem persists." |
| Disk full | "Your changes could not be saved — the disk is full. Free space on D:\\ and try again." |
| File locked | "The project file is currently in use. Close any conflicting applications and try saving again." |
| Partial save | "Some changes were saved, but [n] item(s) could not be written. Items that were not saved are still shown here." |

**Domain References:** INV-001 (input preserved on save failure). GAP-FVR-002 (idempotency not yet defined).

---

## 9. CAT-07 — Unexpected Internal Error

| Field | Value |
|---|---|
| **Category ID** | CAT-07 |
| **Name** | Unexpected Internal Error |
| **Severity** | Error |
| **Blocking** | Depends on context — blocking if the failing operation was required |
| **Dismissible** | Yes — but operation remains incomplete |
| **Persistence** | Session — cleared if operation is re-attempted and succeeds |
| **Presentation** | `NleStatusBanner` (Error) with correlation reference ID |
| **Redaction** | No stack traces, no exception class names, no internal paths, no system usernames |
| **Grouping** | Per-operation instance with unique correlation ID |
| **Recovery Destination** | Offer retry; offer diagnostics export; remain on current form |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| General unexpected | "An unexpected error occurred. Your data has not been changed. [Retry / View Diagnostics] Reference: [correlation-id]" |
| Recoverable unexpected | "Something went wrong while processing your request. Try again, or contact support with reference [correlation-id]." |

**Domain References:** Serilog logging captures full details; user-visible message contains only correlation ID. Correlation ID is a session-scoped opaque string — not an internal path or exception identifier.

---

## 10. CAT-08 — Partial Completion

| Field | Value |
|---|---|
| **Category ID** | CAT-08 |
| **Name** | Partial Completion |
| **Severity** | Warning |
| **Blocking** | No — the operation completed for some items |
| **Dismissible** | Yes |
| **Persistence** | Session — until addressed or dismissed |
| **Presentation** | `NleStatusBanner` (Warning) with per-item breakdown; `OperationStatus = PARTIAL` in operation log |
| **Redaction** | No internal paths in item-level breakdown |
| **Grouping** | Show succeeded items and failed items in separate lists |
| **Recovery Destination** | Show per-item status; offer targeted retry for failed items (if eligible per GAP-FVR-002) |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Mixed result | "[n] of [total] items completed successfully. [m] item(s) could not be processed. Review the failed items below." |
| Partial write | "Project configuration was partially saved. [Component] was not saved. Your other changes were applied." |

**Domain References:** `OperationStatus.PARTIAL` (NLE-DOM-SPEC-001). INV-007 (IsDirty preserved for unsaved portion). GAP-FVR-002 (idempotency gap limits safe targeted retry).

---

## 11. CAT-09 — Uncertain Outcome

| Field | Value |
|---|---|
| **Category ID** | CAT-09 |
| **Name** | Uncertain Outcome |
| **Severity** | Warning |
| **Blocking** | Blocks auto-navigation and auto-retry |
| **Dismissible** | Yes — user acknowledges and decides |
| **Persistence** | Session — until user takes explicit action |
| **Presentation** | `NleStatusBanner` (Warning, distinct uncertain styling) with `OperationStatus = UNCERTAIN` label |
| **Redaction** | No internal operation IDs that reveal implementation details |
| **Grouping** | Per-operation instance |
| **Recovery Destination** | User reviews operation log; manually decides whether to retry, investigate, or accept current state |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| General uncertain | "The outcome of this operation could not be confirmed. It may have succeeded or failed. Review the operation log and check the current state before proceeding." |
| Timeout uncertain | "The operation timed out before a result could be confirmed. The operation may still be running or may have failed silently." |

**Critical Rules:**
1. `UNCERTAIN` must never be relabelled as `COMPLETED` or `FAILED` in the UI.
2. No automatic retry may be initiated when outcome is `UNCERTAIN`.
3. IsDirty must remain unchanged (INV-001).

**Domain References:** `OperationStatus.UNCERTAIN` (NLE-DOM-SPEC-001). GAP-FVR-002 (no idempotency tokens — blind retry prohibited).

---

## 12. CAT-10 — Cancellation

| Field | Value |
|---|---|
| **Category ID** | CAT-10 |
| **Name** | Cancellation |
| **Severity** | Info |
| **Blocking** | No — user chose to cancel |
| **Dismissible** | Yes |
| **Persistence** | Transient — clears when user re-attempts |
| **Presentation** | `NleStatusBanner` (Info) or inline status indicator |
| **Redaction** | N/A |
| **Grouping** | Per-operation |
| **Recovery Destination** | Remain on form; re-attempt is user decision |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| User-initiated cancel | "Operation cancelled. Your unsaved changes are preserved." |
| System timeout cancel | "The operation was cancelled due to timeout. Try again when ready." |

**Domain References:** `OperationStatus.CANCELLED`. INV-001 (IsDirty unchanged after cancellation).

---

## 13. CAT-11 — Stale / Conflicting Information

| Field | Value |
|---|---|
| **Category ID** | CAT-11 |
| **Name** | Stale or Conflicting Information |
| **Severity** | Warning |
| **Blocking** | Blocks submission until user resolves conflict |
| **Dismissible** | No — user must review and choose action |
| **Persistence** | Persistent until conflict resolved |
| **Presentation** | `NleStatusBanner` (Warning) with conflict details and action options |
| **Redaction** | Do not expose other users' data or internal timestamps |
| **Grouping** | Per-conflicting record |
| **Recovery Destination** | Show current state; offer Reload (adopt external changes) or Keep Mine (overwrite); Cancel to stay |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Record changed externally | "This project was updated since you opened it. Review the current version before saving. [Reload] [Keep my changes] [Cancel]" |
| Stale validation result | "Project information has changed. The validation results shown may be outdated. Re-validate before proceeding." |

**Domain References:** CAT-11 detection uses correlation tokens (VR-GLB-003). Merge UX is a future capability (GAP-FVR-003).

---

## 14. CAT-12 — Access Denied

| Field | Value |
|---|---|
| **Category ID** | CAT-12 |
| **Name** | Access Denied |
| **Severity** | Error |
| **Blocking** | Yes — operation cannot proceed |
| **Dismissible** | No |
| **Persistence** | Persistent until authorization condition changes |
| **Presentation** | `NleStatusBanner` (Error) |
| **Redaction** | Do not expose the name of internal permission checks, privilege levels, or Windows ACL entries |
| **Grouping** | Per-operation instance |
| **Recovery Destination** | Advise user to check permissions; do not offer elevation (REQ-SEC-001) |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Path permission | "NexLudic Engine does not have permission to write to the selected location. Choose a folder where you have write access." |
| Feature access | "This feature is not available in the current configuration. Check project settings or contact your administrator." |

**Critical Rule:** UAC elevation must never be offered or attempted (`REQ-SEC-001`, Medium Integrity Level).

**Domain References:** `REQ-SEC-001`.

---

## 15. CAT-13 — Dependency / Capability Conflict

| Field | Value |
|---|---|
| **Category ID** | CAT-13 |
| **Name** | Dependency or Capability Conflict |
| **Severity** | Error |
| **Blocking** | Yes — prevents commit of conflicting configuration |
| **Dismissible** | No |
| **Persistence** | Persistent until conflict resolved |
| **Presentation** | `NleInlineValidation` (for field-level) or `NleStatusBanner` (Error, for multi-item conflicts) |
| **Redaction** | No internal domain model type names in user message |
| **Grouping** | List all conflicting pairs or requirements |
| **Recovery Destination** | User reviews and resolves conflict — remove one requirement, reduce scope, or choose different strategy |

**Message Patterns:**

| Sub-type | Example Pattern |
|---|---|
| Capability conflict | "Capability requirements [A] and [B] cannot both be satisfied by the selected adapter combination. Remove one or change the strategy." |
| Strategy conflict | "The selected strategy [X] is incompatible with the capability [Y]. Adjust your selections to remove the conflict." |
| Adapter conflict | "The selected adapters have conflicting memory access requirements. Review the capability declarations." |

**Domain References:** `NLE-DOM-SPEC-001` §CapabilityRequirement, §AdapterManifest.

---

## 16. Cross-Cutting Redaction Rules (All Categories)

The following content must never appear in any user-visible error message, notification, banner, tooltip, or dialog across all 13 categories:

| Forbidden Content | Authoritative Rule | Replacement |
|---|---|---|
| Raw API tokens, API keys | REQ-SEC-001 | `[credential reference]` |
| DPAPI GUIDs | REQ-SEC-001 | `••••••••-••••-••••-••••-••••••••••••` |
| Full filesystem paths revealing install layout | REQ-SEC-001 | Relative path from workspace root, or omitted |
| Stack traces | NLE-SPC-FVR-001 §8.4 | Correlation reference ID only |
| Exception class names (e.g., `NullReferenceException`) | NLE-SPC-FVR-001 §8.4 | User-intelligible description |
| OS-level usernames or account paths | REQ-SEC-001 | Omitted |
| Internal domain enum technical names | NLE-SPC-FVR-001 §3.4 | Human-readable description |
| Raw OS error codes (e.g., `0x80070005`) | NLE-SPC-FVR-001 §3.4 | Translated message |
| Internal operation or transaction IDs (beyond correlation ID) | NLE-SPC-FVR-001 §3.4 | Correlation ID only |

---

## 17. Severity-to-Presentation Mapping Summary

| Severity | Live Region | Presentation | Dismissible by Default |
|---|---|---|---|
| `Critical` | Assertive | Pre-flight banner (non-dismissible) | No |
| `Error` | Assertive | NleInlineValidation / NleStatusBanner Error | No (inline); context-dependent (banner) |
| `Warning` | Polite | NleStatusBanner Warning | Yes |
| `Info` | Polite | NleStatusBanner Info / Helper text | Yes |

---

## 18. Operation Status to Error Category Mapping

| OperationStatus | Corresponding Category | Presentation |
|---|---|---|
| `FAILED` | CAT-06 (persistence), CAT-04 (rejection), CAT-07 (unexpected) | Error banner |
| `PARTIAL` | CAT-08 | Warning banner with per-item list |
| `UNCERTAIN` | CAT-09 | Uncertain banner — no auto-classification |
| `CANCELLED` | CAT-10 | Info banner |
| `COMPLETED` | — (success) | Success confirmation |
| `RUNNING` | — (progress) | Progress indicator |
| `QUEUED` / `AWAITING_APPROVAL` | — (pending) | Pending state indicator |

---

## 19. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Parent | NLE-SPC-FVR-001 |
| Implementation Authorization | Documentation only |

