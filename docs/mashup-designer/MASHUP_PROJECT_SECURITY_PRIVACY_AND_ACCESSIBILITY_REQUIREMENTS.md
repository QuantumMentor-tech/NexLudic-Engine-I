# MASHUP PROJECT SECURITY, PRIVACY, AND ACCESSIBILITY REQUIREMENTS
**Document ID:** `NLE-REQ-MPS-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Senior Security Architect, Accessibility Specialist, Desktop UX Architect, Privacy Reviewer  
**Classification:** Non-Functional Requirements Specification  

---

## 1. PURPOSE AND SCOPE

### 1.1 Purpose
This specification establishes the mandatory security boundaries, privacy safeguards, data minimization policies, and accessibility standards (WCAG 2.1 AA) governing the **Mashup Project Domain and Creation Experience** within NexLudic Engine.

The Mashup Designer operates as a creative workspace where users compose and exchange cross-game project concepts. This document guarantees that:
1. Malicious project files, imported archives, or crafted text fields cannot execute arbitrary code or compromise the host desktop.
2. Creator privacy is protected by scrubbing local paths and personal credentials.
3. The project creation experience is universally accessible to creators of all abilities.

---

## 2. SECURITY CONTROLS & LEAST PRIVILEGE MANDATES

```
+--------------------------------------------------------------------------------------------------------+
|                                    PROJECT SECURITY & INTEGRITY MODEL                                  |
|                                                                                                        |
|  [ CONTROL 1: Non-Executing Design Principle ] ---> Zero game or adapter execution during planning.   |
|  [ CONTROL 2: Medium Integrity Level (DEC-002) ] -> Standard user permissions; Zero UAC prompts.       |
|  [ CONTROL 3: Anti-Cheat Dominance (BLK-SEC-01) ]-> Instant warning & block on kernel anti-cheat titles|
|  [ CONTROL 4: Filesystem Sandbox (DEC-009) ]   ---> Storage bounded strictly to D:\NexLudic\Projects\...|
|  [ CONTROL 5: Defensive Text Sanitization ]   ---> Strict strip of HTML, script tags, and shell codes |
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 The Non-Executing Design Principle
- **Strict Separation of Planning from Execution:** Authoring, validating, or saving a Mashup Project MUST NEVER spawn an external game process, attach a debugger, inject a DLL, or execute third-party adapter code.
- **Static Domain Operations:** All project creation, validation, and revision operations are strictly in-memory data manipulations and local JSON file writes.

### 2.2 Input Sanitization and Code Injection Mitigation
1. **XSS and Script Injection:** All text fields (`display_name`, `description`, `core_thesis`, requirement `description`) are stripped of HTML tags (`<script>`, `<iframe>`), JavaScript pseudo-protocols, and format control codes before persistence.
2. **Shell Escape Protection:** Input strings are validated against dangerous shell escape sequences (e.g. `|`, `;`, `&`, `$(...)`, `` `...` ``) to prevent command injection if strings are passed to tooling scripts in later phases.
3. **Path Traversal Protection:** Relative file paths declared in projects are normalized using canonical path resolution. Any path containing `..`, leading slashes, or Windows drive letters is rejected (`CRIT_SEC_PATH_TRAVERSAL`).

### 2.3 Anti-Cheat Dominance Guardrail (`BLK-SEC-01`)
- When a user selects a game known to feature kernel-level anti-cheat software (EasyAntiCheat, BattlEye, Vanguard, Ricochet), the project creation UI MUST display an explicit, non-bypassable warning badge (`WARN_SEC_ANTI_CHEAT_DETECTED`).
- The project model forbids selecting `STRATEGY_1_HOST_MOD` or live memory injection for such games, steering creators toward non-invasive bridging or single-player sandbox titles.

---

## 3. PRIVACY AND DATA MINIMIZATION

1. **Path Sanitization:** When exporting a project (`.nlproj`) or submitting reports, local installation paths (e.g. `C:\Users\JaneDoe\Games\...`) are automatically scrubbed and replaced with generic markers (`<LOCAL_INSTALL_PATH>`).
2. **Zero Personal Telemetry:** The project aggregate root contains ZERO personal telemetry, user IP addresses, or hardware MAC addresses.
3. **Non-Invasive Game Discovery:** The project creation workflow does NOT perform blind scans of the host filesystem; game selection queries the local catalog in-memory.

---

## 4. ACCESSIBILITY REQUIREMENTS (WCAG 2.1 AA COMPLIANCE)

In accordance with Phase 3 UI specifications (`NLE-SPC-SHL-001`), the project creation workflow adheres strictly to WCAG 2.1 Level AA standards:

### 4.1 Keyboard Navigation & Focus Management
- **Full Keyboard Operability:** Every stage of project creation can be navigated and submitted using the keyboard alone (`Tab`, `Shift+Tab`, `Space`, `Enter`, `Esc`).
- **Logical Focus Progression:** Advancing wizard stages moves keyboard focus directly to the first interactive input of the new stage. Closing modal dialogs returns focus to the triggering element.
- **Escape to Cancel:** Pressing `Esc` triggers a confirmation prompt before discarding uncommitted inputs.

### 4.2 Screen Reader Compatibility & Assistive Technology
- **Accessible Names & Roles:** Every text input, dropdown, and button provides explicit accessible names (`AutomationProperties.Name` / `aria-label`) and role attributes.
- **Live Error Announcements:** When pre-flight validation fails, error messages are dispatched to screen readers via live regions (`aria-live="polite"`), vocalizing the count of errors and the exact field needing correction.

### 4.3 Visual Contrast & Status Redundancy
- **Color Independence:** Status and validation indicators NEVER rely on color alone. Errors use an exclamation icon + red border + text error message; warnings use a triangle icon + amber border + text note.
- **High Contrast:** All text elements maintain a minimum contrast ratio of 4.5:1 against their backgrounds (7:1 for headers).

### 4.4 Non-Destructive Error Recovery
- If validation fails, user-entered text is NEVER wiped. Inputs remain preserved in place with inline error callouts, allowing creators to correct mistakes without re-typing.

---
*End of Specification — Document `NLE-REQ-MPS-001`.*
