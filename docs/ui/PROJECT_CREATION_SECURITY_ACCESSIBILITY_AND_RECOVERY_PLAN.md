# PROJECT CREATION SECURITY, ACCESSIBILITY AND RECOVERY PLAN

**Document Identifier:** `NLE-PLN-SEC-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 20 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Security Engineering, Accessibility (a11y) & Resilience Plan  
**Parent Document:** `NLE-SPC-WIZ-001` ([file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md](file:///d:/App%20Winrar%20Files/Nexludic%20Engine%20(working)/docs/ui/PROJECT_CREATION_WIZARD_SPECIFICATION.md))  

---

## 1. Security Architecture & Input Boundaries

The Project Creation Wizard represents the outermost boundary where arbitrary external user input enters the NexLudic Engine.

### 1.1 Input Sanitization & Path Traversal Prevention
- **Project Slugs & Folder Names:** Slugs are constrained strictly to regex `^[a-z0-9]+(-[a-z0-9]+)*$`. Disallow relative traversal sequences (`..`, `\`, `/`) or Windows reserved filenames (`CON`, `PRN`, `AUX`, `NUL`, `COM1-9`, `LPT1-9`).
- **Disk Path Constraints (`DEC-009`):** Target workspace directories must resolve strictly to Volume `D:\` (e.g. `D:\NexLudic\Projects\<slug>\`). Any attempt to select system directories (e.g. `C:\Windows`, `C:\Program Files`) is rejected with a hard validation error.
- **Raw User Intent Sanitization:** User descriptions in Stage 4 are stored as UTF-8 plaintext in JSON envelopes without script evaluation. When rendered in UI components, XAML text blocks escape markup to prevent XAML injection.

### 1.2 Principle of Least Privilege (`REQ-SEC-001`)
- The wizard executes entirely under Windows Medium Integrity Level.
- No UAC elevation prompts are triggered.
- Target workspace initialization permissions are inherited from the executing user account without altering ACLs.

### 1.3 Anti-Cheat & Single-Player Guardrail (`REQ-SAFE-001`)
- If either selected game is flagged in the catalog as having active kernel-level anti-cheat (e.g. Easy Anti-Cheat, BattlEye, Vanguard, Ricochet), Stage 3 displays a non-dismissible hard block:
  > *"Safety Violation: The selected title employs active kernel-level anti-cheat. NexLudic Engine strictly operates on offline, single-player environments to protect user accounts and prevent anti-tamper conflicts. Project creation for this title is prohibited."*

---

## 2. Accessibility (a11y) Architecture

The wizard complies with WCAG 2.1 Level AA and Microsoft Fluent accessibility standards.

### 2.1 Keyboard Navigation & Focus Management
- **Logical Tab Order:** Seamless sequential navigation across all interactive elements (`Tab` and `Shift+Tab`).
- **Focus Trap within Wizard:** While the wizard is active as a modal dialog, focus is trapped inside the wizard boundary.
- **First-Error Auto-Focus:** When stage validation fails, focus automatically shifts to the first invalid control, announcing the error message via UI Automation.
- **Shortcut Keys:**
  - `Alt + N` / `Enter` (when not in multiline text): Next Step / Create.
  - `Alt + B`: Back.
  - `Escape`: Trigger Cancel Confirmation dialog.

### 2.2 Screen Reader & High Contrast Support
- All custom controls implement `AutomationPeer` subclasses with appropriate `AutomationControlType`.
- Dynamic status banners use `AutomationProperties.LiveSetting = LiveSetting.Assertive`.
- Minimum color contrast ratio of 4.5:1 for standard text and 3:1 for large text/badges across both Light and Dark themes.

### 2.3 Window Scaling & Minimum Resolution
- Supported minimum application shell resolution is **1024 x 700 pixels**.
- Wizard content panels utilize dynamic `ScrollViewer` containers with responsive grid layouts to prevent content truncation or button clipping at 100%, 125%, and 150% Windows DPI scaling.

---

## 3. Failure Modes & Recovery Workflows

| Failure Event | Root Cause | User Presentation | Recovery Action |
| :--- | :--- | :--- | :--- |
| **Directory Collision** | Target project slug folder already exists on `D:\` | Alert banner: *"Project folder already exists on disk."* | Wizard auto-increments slug suffix (e.g. `-2`) or allows user to edit slug. |
| **Disk Space Exhaustion** | Volume `D:` has < 10 GB free space | Pre-flight error: *"Insufficient disk space on drive D: (Required: 10 GB)."* | User must free disk space on drive D: and click *Retry Pre-Flight*. |
| **Write Permission Denied** | Read-only folder or locked directory | Error dialog: *"Unable to create directory D:\NexLudic\Projects\... Access Denied."* | User prompted to check folder permissions or select a subfolder with full write rights. |
| **Process Crash During Creation** | Power failure or task termination during atomic write | Partial files cleaned up via temporary staging directory pattern | On next engine launch, orphaned temporary `.tmp` creation artifacts are purged automatically. |

