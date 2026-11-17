# Form Field and Validation Rule Catalog

**Document ID:** NLE-CAT-FVR-001  
**Version:** 1.0.0  
**Status:** Approved — Architecture & Design  
**Phase:** 3 — Desktop Interface & Workspace Management  
**Step:** 23 of 100  
**Parent Specification:** NLE-SPC-FVR-001  
**Extends:** NLE-CAT-WIZ-001 (Project Creation Field & Validation Catalog — not duplicated here)  
**Classification:** UI Architecture — Validation Catalog  

---

## 1. Purpose and Scope

This catalog enumerates all validation rules across NexLudic Engine forms beyond those already specified in `NLE-CAT-WIZ-001`. It extends, never replaces, the wizard-specific catalog. Rules use a standardised format for consistency with implementation planning.

**Scope:** Requirements management forms, project configuration forms, workspace settings, capability declaration inputs, adapter configuration, project-wide policy fields, and cross-cutting rules not covered by the wizard catalog.

**Non-scope:** Step-by-step wizard field rules (covered by `NLE-CAT-WIZ-001`). Implementors must consult both catalogs.

---

## 2. Rule Entry Format

Each rule entry carries the following fields:

| Field | Description |
|---|---|
| **Rule ID** | Unique identifier: `VR-[group]-[seq]` |
| **Field / Form** | Form name and field label |
| **Domain Mapping** | Entity / attribute from `NLE-DOM-SPEC-001` |
| **Rule Description** | Precise, implementation-neutral description of the constraint |
| **Tier** | Validation owner tier (1–4) |
| **Trigger** | When validation executes |
| **Blocking** | Yes / No — does failure prevent stage advance or submission |
| **Severity** | Critical / Error / Warning / Info |
| **User Message (pattern)** | Example message; exact wording is implementation detail |
| **Accessibility Behaviour** | Focus and announcement expectations |
| **Data Preservation** | Input preserved on failure? |
| **Authoritative Source** | Reference document(s) |
| **Recovery Action** | What the user does to resolve |
| **Status** | Existing (from prior spec) / Approved / Provisional |

---

## 3. Project Metadata Rules

### VR-META-001: Project Name — Required

| Field | Value |
|---|---|
| **Rule ID** | VR-META-001 |
| **Field / Form** | All project-metadata forms / Project Name |
| **Domain Mapping** | `Project.displayName` |
| **Rule Description** | Project name must not be empty or whitespace-only |
| **Tier** | 1 |
| **Trigger** | Blur, Stage-exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Project name is required." |
| **Accessibility** | Focus returned to Name field; announced Assertive |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-001 (cross-reference) |
| **Recovery Action** | Enter a project name |
| **Status** | Existing |

### VR-META-002: Project Name — Maximum Length

| Field | Value |
|---|---|
| **Rule ID** | VR-META-002 |
| **Field / Form** | All project-metadata forms / Project Name |
| **Domain Mapping** | `Project.displayName` |
| **Rule Description** | Project name must not exceed 100 characters |
| **Tier** | 1 |
| **Trigger** | Typing (proximity indicator at 90+), Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Project name must be 100 characters or fewer. Currently [n] characters." |
| **Accessibility** | Announced Polite at 90 chars; Assertive at limit breach |
| **Data Preservation** | Yes — do not truncate automatically |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-002 |
| **Recovery Action** | Shorten the project name |
| **Status** | Existing |

### VR-META-003: Project Slug — Format

| Field | Value |
|---|---|
| **Rule ID** | VR-META-003 |
| **Field / Form** | Project Settings / Project Slug |
| **Domain Mapping** | `Project.slug` |
| **Rule Description** | Slug must match `^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$` (auto-derived from name; editable in settings) |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Project ID must use only lowercase letters, numbers, and hyphens, and must not start or end with a hyphen." |
| **Accessibility** | Focus returned to Slug field |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-003; NLE-DOM-SPEC-001 §Project.slug |
| **Recovery Action** | Edit slug to conform to pattern |
| **Status** | Existing |

### VR-META-004: Project Slug — Uniqueness

| Field | Value |
|---|---|
| **Rule ID** | VR-META-004 |
| **Field / Form** | Project Settings / Project Slug |
| **Domain Mapping** | `Project.slug` — must be unique across all projects on this installation |
| **Rule Description** | Slug must not duplicate an existing project slug in the workspace registry |
| **Tier** | 3 |
| **Trigger** | Pre-flight (Stage 7 entry); Settings save |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "This project ID is already in use. Choose a different identifier." |
| **Accessibility** | Focus returned to Slug field after async result |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001; DEC-009 |
| **Recovery Action** | Choose a different slug |
| **Status** | Approved |

### VR-META-005: Project Description — Maximum Length

| Field | Value |
|---|---|
| **Rule ID** | VR-META-005 |
| **Field / Form** | Project Settings / Description |
| **Domain Mapping** | `Project.description` |
| **Rule Description** | Description must not exceed 500 characters if provided |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Description must be 500 characters or fewer." |
| **Accessibility** | Announced Polite at 450 chars; Error at breach |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-005 |
| **Recovery Action** | Shorten description |
| **Status** | Existing |

---

## 4. Storage Path Rules

### VR-PATH-001: Workspace Path — Required

| Field | Value |
|---|---|
| **Rule ID** | VR-PATH-001 |
| **Field / Form** | Project Creation Stage 1 / Workspace Location |
| **Domain Mapping** | `Project.workspacePath` |
| **Rule Description** | Workspace path must not be empty |
| **Tier** | 1 |
| **Trigger** | Blur, Stage-exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "A workspace location is required." |
| **Accessibility** | Focus returned to path field |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-010; DEC-009 |
| **Recovery Action** | Enter or browse to a path |
| **Status** | Existing |

### VR-PATH-002: Workspace Path — Volume D Requirement

| Field | Value |
|---|---|
| **Rule ID** | VR-PATH-002 |
| **Field / Form** | Project Creation Stage 1 / Workspace Location |
| **Domain Mapping** | `Project.workspacePath` |
| **Rule Description** | Path must begin with `D:\`. Paths on any other volume are hard-blocked. This is a non-dismissible Error, not a Warning. |
| **Tier** | 1 (preliminary format check) + 4 (enforcement) |
| **Trigger** | Blur (Tier 1 format check), Pre-flight (Tier 4 hard enforcement) |
| **Blocking** | Yes — hard-blocked |
| **Severity** | Error (Critical at pre-flight) |
| **User Message** | "Workspace location must be on volume D:\\. Paths on other drives are not supported." |
| **Accessibility** | Non-dismissible; Focus returned to path field |
| **Data Preservation** | Yes |
| **Authoritative Source** | DEC-009; NLE-CAT-WIZ-001 VR-WIZ-011 |
| **Recovery Action** | Change path to a D:\ location |
| **Status** | Existing |

### VR-PATH-003: Workspace Path — Unique

| Field | Value |
|---|---|
| **Rule ID** | VR-PATH-003 |
| **Field / Form** | Project Creation Stage 1 / Workspace Location |
| **Domain Mapping** | `Project.workspacePath` |
| **Rule Description** | Path must not be in use by any existing NexLudic project |
| **Tier** | 3 |
| **Trigger** | Pre-flight |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "This location is already used by another project. Choose a different folder." |
| **Accessibility** | Focus returned to path field |
| **Data Preservation** | Yes |
| **Authoritative Source** | DEC-009; NLE-DOM-SPEC-001 |
| **Recovery Action** | Browse to or enter an unused folder |
| **Status** | Approved |

### VR-PATH-004: Workspace Path — Writeable

| Field | Value |
|---|---|
| **Rule ID** | VR-PATH-004 |
| **Field / Form** | Project Creation Stage 1 / Workspace Location |
| **Domain Mapping** | `Project.workspacePath` |
| **Rule Description** | Path must be writeable by the application at Medium Integrity Level. UAC elevation is never attempted. |
| **Tier** | 3 |
| **Trigger** | Pre-flight |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "NexLudic Engine cannot write to this location. Choose a folder where you have write permission." |
| **Accessibility** | Focus returned to path field |
| **Data Preservation** | Yes |
| **Authoritative Source** | REQ-SEC-001; DEC-009 |
| **Recovery Action** | Choose a writeable path; check folder permissions |
| **Status** | Approved |

### VR-PATH-005: Disk Space — Minimum Available

| Field | Value |
|---|---|
| **Rule ID** | VR-PATH-005 |
| **Field / Form** | Pre-flight audit (Stage 7) |
| **Domain Mapping** | Infrastructure constraint — not a user field |
| **Rule Description** | Volume D must have at least 10 GB available at time of project creation |
| **Tier** | 4 |
| **Trigger** | Pre-flight commit |
| **Blocking** | Yes — prevents project creation |
| **Severity** | Critical |
| **User Message** | "Insufficient disk space on D:\\. At least 10 GB free is required. Currently [n] GB available. Free space and try again." |
| **Accessibility** | Pre-flight banner (non-dismissible); announced Assertive |
| **Data Preservation** | Yes — wizard data retained |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-012; DEC-009 |
| **Recovery Action** | Free disk space on D:\ then re-run pre-flight |
| **Status** | Existing |

---

## 5. Game Pair Rules

### VR-GAME-001: Anti-Cheat Hard Block

| Field | Value |
|---|---|
| **Rule ID** | VR-GAME-001 |
| **Field / Form** | Project Creation Stage 3 / Game Selection |
| **Domain Mapping** | `GameTitle.antiCheatStatus` = `KERNEL_LEVEL` or `ONLINE_ENFORCED` |
| **Rule Description** | Games with online or kernel-level anti-cheat are entirely blocked. This check is non-dismissible and irreversible at this stage. No workaround exists. |
| **Tier** | 2 (ViewModel) + 4 (Pre-flight confirmation) |
| **Trigger** | Stage 3 exit |
| **Blocking** | Yes — non-dismissible |
| **Severity** | Critical |
| **User Message** | "[Game Name] uses active anti-cheat protection that is incompatible with NexLudic Engine. This game pair cannot be created." |
| **Accessibility** | Non-dismissible Critical banner; Confirm button hidden |
| **Data Preservation** | Yes — user may select different games |
| **Authoritative Source** | REQ-SAFE-001; NLE-CAT-WIZ-001 VR-WIZ-013 |
| **Recovery Action** | Select a different game that does not use kernel/online anti-cheat |
| **Status** | Existing |

### VR-GAME-002: Distinct Game Pair Required

| Field | Value |
|---|---|
| **Rule ID** | VR-GAME-002 |
| **Field / Form** | Project Creation Stage 3 / Game Selection |
| **Domain Mapping** | `GamePair.sourceGame` ≠ `GamePair.targetGame` |
| **Rule Description** | Source and target games must be distinct titles |
| **Tier** | 2 |
| **Trigger** | Stage 3 exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Source game and target game must be different titles." |
| **Accessibility** | Focus returned to second game selector |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 §GamePair; NLE-CAT-WIZ-001 VR-WIZ-014 |
| **Recovery Action** | Select a different game for one of the two slots |
| **Status** | Existing |

### VR-GAME-003: At Least One Game Must Be Provided

| Field | Value |
|---|---|
| **Rule ID** | VR-GAME-003 |
| **Field / Form** | Project Creation Stage 3 / Game Selection |
| **Domain Mapping** | `GamePair.sourceGame`, `GamePair.targetGame` |
| **Rule Description** | Both game slots must be filled before stage exit |
| **Tier** | 1 |
| **Trigger** | Stage 3 exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Both a source game and a target game must be selected." |
| **Accessibility** | Focus returned to empty game selector |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-SPC-WIZ-001; NLE-CAT-WIZ-001 |
| **Recovery Action** | Select both games |
| **Status** | Existing |

---

## 6. Version and Environment Rules

### VR-VER-001: UNKNOWN Version Is Permitted

| Field | Value |
|---|---|
| **Rule ID** | VR-VER-001 |
| **Field / Form** | Project Creation Stage 4 / Game Versions |
| **Domain Mapping** | `GameVersion.epistemicTier` = `DECLARED` or `UNKNOWN` |
| **Rule Description** | A game version may be `UNKNOWN`. This is a valid epistemic state per `NLE-DOM-SPEC-001`. It must never produce a validation error. The field may display an informational advisory note, but not an error or warning that implies the state is incorrect. |
| **Tier** | 1 (advisory only) |
| **Trigger** | Blur |
| **Blocking** | No |
| **Severity** | Info |
| **User Message** | "Version not specified. NexLudic Engine will work with available detection information." |
| **Accessibility** | Announced Polite |
| **Data Preservation** | N/A |
| **Authoritative Source** | NLE-DOM-SPEC-001 §Epistemic Tiers; NLE-CAT-WIZ-001 VR-WIZ-016 |
| **Recovery Action** | None required; user may optionally enter a version |
| **Status** | Existing |

### VR-VER-002: Version Format When Provided

| Field | Value |
|---|---|
| **Rule ID** | VR-VER-002 |
| **Field / Form** | Project Creation Stage 4 / Game Versions |
| **Domain Mapping** | `GameVersion.versionString` |
| **Rule Description** | If a version is provided, it must not be whitespace-only. Any non-empty string is accepted (versions vary widely across games). |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes (if whitespace-only) |
| **Severity** | Error |
| **User Message** | "Version cannot be blank spaces only. Enter a version number or leave the field empty." |
| **Accessibility** | Focus returned to version field |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 |
| **Recovery Action** | Enter a valid version string or clear the field |
| **Status** | Approved |

### VR-VER-003: Environment Field — Permitted Values

| Field | Value |
|---|---|
| **Rule ID** | VR-VER-003 |
| **Field / Form** | Project Creation Stage 4 / Launch Environment |
| **Domain Mapping** | `GameEnvironment.type` |
| **Rule Description** | Environment must be selected from the predefined list (Steam, Epic, GOG, Standalone, Custom, Unknown). `Unknown` is a valid selection. |
| **Tier** | 1 |
| **Trigger** | Stage-exit |
| **Blocking** | Yes if no selection made |
| **Severity** | Error |
| **User Message** | "Please select a launch environment for this game." |
| **Accessibility** | Focus returned to Environment selector |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001; NLE-CAT-WIZ-001 VR-WIZ-017 |
| **Recovery Action** | Select an environment from the list |
| **Status** | Existing |

---

## 7. Mashup Thesis Rules

### VR-THESIS-001: Integration Intent — Minimum Content

| Field | Value |
|---|---|
| **Rule ID** | VR-THESIS-001 |
| **Field / Form** | Project Creation Stage 5 / Integration Intent |
| **Domain Mapping** | `Project.mashupThesis.rawIntent` |
| **Rule Description** | The integration intent text field must contain at least 10 non-whitespace characters when provided |
| **Tier** | 1 |
| **Trigger** | Blur, Stage-exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Please describe your integration goal in at least a few words." |
| **Accessibility** | Focus returned to intent field |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-019 |
| **Recovery Action** | Provide a brief description of the goal |
| **Status** | Existing |

### VR-THESIS-002: Integration Intent — Maximum Length

| Field | Value |
|---|---|
| **Rule ID** | VR-THESIS-002 |
| **Field / Form** | Project Creation Stage 5 / Integration Intent |
| **Domain Mapping** | `Project.mashupThesis.rawIntent` |
| **Rule Description** | Integration intent must not exceed 1000 characters |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Integration goal must be 1000 characters or fewer." |
| **Accessibility** | Proximity indicator at 900+ chars |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 |
| **Recovery Action** | Shorten the description |
| **Status** | Approved |

---

## 8. Capability Requirement Rules

### VR-CAP-001: At Least One Capability or Exploratory Flag

| Field | Value |
|---|---|
| **Rule ID** | VR-CAP-001 |
| **Field / Form** | Project Creation Stage 6 / Capability Requirements |
| **Domain Mapping** | `Project.capabilityRequirements` list length ≥ 1, or `Project.exploratoryMode = true` |
| **Rule Description** | At least one capability requirement must be declared, or the project must be flagged as exploratory |
| **Tier** | 2 |
| **Trigger** | Stage 6 exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Add at least one capability requirement, or mark this project as exploratory if requirements are not yet known." |
| **Accessibility** | Focus returned to capability list area |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-021; NLE-DOM-SPEC-001 §CapabilityRequirement |
| **Recovery Action** | Add a capability requirement or check Exploratory Mode |
| **Status** | Existing |

### VR-CAP-002: Capability Evidence Tier Noted

| Field | Value |
|---|---|
| **Rule ID** | VR-CAP-002 |
| **Field / Form** | Requirements Management / Capability Evidence |
| **Domain Mapping** | `CapabilityRequirement.evidenceTier` |
| **Rule Description** | Each capability requirement may carry an evidence tier (`DECLARED`, `DETECTED`, `PROBED`, `VERIFIED`). No tier is required at creation; absence is `UNEVALUATED`. `UNEVALUATED` is not a validation error. |
| **Tier** | 1 (advisory) |
| **Trigger** | Form load, blur |
| **Blocking** | No |
| **Severity** | Info |
| **User Message** | "Evidence not yet evaluated. This is normal for new requirements." |
| **Accessibility** | Polite announcement only |
| **Data Preservation** | N/A |
| **Authoritative Source** | NLE-DOM-SPEC-001 §Epistemic Tiers |
| **Recovery Action** | None required at creation |
| **Status** | Approved |

---

## 9. Strategy Preference Rules

### VR-STRAT-001: Strategy Disclaimer Acknowledgement

| Field | Value |
|---|---|
| **Rule ID** | VR-STRAT-001 |
| **Field / Form** | Project Creation Stage 6 / Strategy Preferences |
| **Domain Mapping** | `Project.strategyPreferences.disclaimerAcknowledged` |
| **Rule Description** | User must acknowledge the strategy-preference disclaimer before stage exit. The checkbox must be explicitly checked. |
| **Tier** | 2 |
| **Trigger** | Stage 6 exit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Please acknowledge the strategy disclaimer before continuing." |
| **Accessibility** | Focus returned to acknowledgement checkbox |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-CAT-WIZ-001 VR-WIZ-022 |
| **Recovery Action** | Check the disclaimer acknowledgement |
| **Status** | Existing |

---

## 10. Requirements Management Form Rules

### VR-REQ-001: Requirement Title — Required

| Field | Value |
|---|---|
| **Rule ID** | VR-REQ-001 |
| **Field / Form** | Requirements Management / New Requirement |
| **Domain Mapping** | `Requirement.title` |
| **Rule Description** | Requirement title must not be empty |
| **Tier** | 1 |
| **Trigger** | Blur, Form submit |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Requirement title is required." |
| **Accessibility** | Focus returned to title field |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 §Requirement |
| **Recovery Action** | Enter a requirement title |
| **Status** | Approved |

### VR-REQ-002: Requirement Title — Maximum Length

| Field | Value |
|---|---|
| **Rule ID** | VR-REQ-002 |
| **Field / Form** | Requirements Management / New Requirement |
| **Domain Mapping** | `Requirement.title` |
| **Rule Description** | Title must not exceed 200 characters |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Requirement title must be 200 characters or fewer." |
| **Accessibility** | Proximity indicator at 180+ chars |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 |
| **Recovery Action** | Shorten title |
| **Status** | Approved |

### VR-REQ-003: Requirement Status — Permitted Values

| Field | Value |
|---|---|
| **Rule ID** | VR-REQ-003 |
| **Field / Form** | Requirements Management / Edit Requirement |
| **Domain Mapping** | `Requirement.status` |
| **Rule Description** | Status must be from the domain-defined enum: `DRAFT`, `ACTIVE`, `DEFERRED`, `OBSOLETE`. No free-text entry. |
| **Tier** | 1 |
| **Trigger** | On selection change |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Please select a valid requirement status." |
| **Accessibility** | Announced on selection |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 §Requirement.status |
| **Recovery Action** | Select from the available statuses |
| **Status** | Approved |

### VR-REQ-004: Requirement Body — Maximum Length

| Field | Value |
|---|---|
| **Rule ID** | VR-REQ-004 |
| **Field / Form** | Requirements Management / New or Edit Requirement |
| **Domain Mapping** | `Requirement.body` |
| **Rule Description** | Body text must not exceed 5000 characters if provided |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Requirement description must be 5000 characters or fewer." |
| **Accessibility** | Proximity indicator at 4500+ chars |
| **Data Preservation** | Yes |
| **Authoritative Source** | NLE-DOM-SPEC-001 |
| **Recovery Action** | Shorten body text |
| **Status** | Approved |

---

## 11. Project Configuration Rules

### VR-CFG-001: AI Provider Credential Reference — Format

| Field | Value |
|---|---|
| **Rule ID** | VR-CFG-001 |
| **Field / Form** | Project Configuration / AI Provider |
| **Domain Mapping** | `ProjectConfiguration.aiProviderCredentialRef` |
| **Rule Description** | Credential reference must be a non-empty UUID if provided. Raw API tokens must never be entered in this field. The field stores only a DPAPI opaque reference UUID. |
| **Tier** | 1 |
| **Trigger** | Blur |
| **Blocking** | Yes if entered and malformed |
| **Severity** | Error |
| **User Message** | "Invalid credential reference. Use the credential manager to link an AI provider key." |
| **Accessibility** | Focus returned to field |
| **Data Preservation** | Yes — do not log or display what was entered |
| **Authoritative Source** | REQ-SEC-001 |
| **Recovery Action** | Use credential manager; enter only the reference UUID |
| **Status** | Approved |

### VR-CFG-002: AI Provider — Required for AI Operations

| Field | Value |
|---|---|
| **Rule ID** | VR-CFG-002 |
| **Field / Form** | Project Configuration / AI Provider |
| **Domain Mapping** | `ProjectConfiguration.aiProviderCredentialRef` |
| **Rule Description** | An AI provider credential reference is required before AI-assisted operations can be triggered. This is not a creation-time requirement (projects may be created without AI config), but is enforced at AI operation initiation. |
| **Tier** | 2 (at operation time) |
| **Trigger** | AI operation trigger |
| **Blocking** | Yes — AI operation rejected |
| **Severity** | Error |
| **User Message** | "An AI provider must be configured before running this operation. Go to Project Configuration > AI Provider." |
| **Accessibility** | Inline banner with navigation link |
| **Data Preservation** | Yes |
| **Authoritative Source** | REQ-SEC-001; NLE-DOM-SPEC-001 |
| **Recovery Action** | Configure AI provider in project settings |
| **Status** | Approved |

### VR-CFG-003: Output Directory — Volume D Required

| Field | Value |
|---|---|
| **Rule ID** | VR-CFG-003 |
| **Field / Form** | Project Configuration / Output Paths |
| **Domain Mapping** | `ProjectConfiguration.outputDirectory` |
| **Rule Description** | All configured output directories must reside on `D:\`. Same hard-block rule as workspace path. |
| **Tier** | 1 (format) + 4 (enforcement) |
| **Trigger** | Blur, Save |
| **Blocking** | Yes |
| **Severity** | Error |
| **User Message** | "Output path must be on volume D:\\." |
| **Accessibility** | Focus returned to path field |
| **Data Preservation** | Yes |
| **Authoritative Source** | DEC-009 |
| **Recovery Action** | Enter a D:\ path |
| **Status** | Approved |

---

## 12. Cross-Cutting and Global Rules

### VR-GLB-001: Credential Fields — No Raw Token Entry

| Field | Value |
|---|---|
| **Rule ID** | VR-GLB-001 |
| **Field / Form** | Any form with a credential or API key field |
| **Domain Mapping** | Any `*CredentialRef` attribute |
| **Rule Description** | Credential fields must be input-masked and must not accept or display raw API tokens. The displayed value is always masked: `••••••••-••••-••••-••••-••••••••••••`. |
| **Tier** | 1 |
| **Trigger** | On paste, on render |
| **Blocking** | Yes if raw token format detected |
| **Severity** | Error |
| **User Message** | "Enter a credential reference from the credential manager, not a raw API key." |
| **Accessibility** | Input type PasswordBox or masked TextBox; announced as "sensitive field" |
| **Data Preservation** | Never log the entered value |
| **Authoritative Source** | REQ-SEC-001 |
| **Recovery Action** | Use credential manager to obtain reference |
| **Status** | Approved |

### VR-GLB-002: Unsaved Changes — Navigation Guard

| Field | Value |
|---|---|
| **Rule ID** | VR-GLB-002 |
| **Field / Form** | All forms with IsDirty support |
| **Domain Mapping** | `ViewModel.IsDirty` |
| **Rule Description** | When `IsDirty = true`, any navigation-away attempt (tab switch, project switch, window close, app exit) must trigger the ConfirmationDialog (Save / Discard Changes / Cancel). Default focus is Cancel. |
| **Tier** | 2 |
| **Trigger** | Navigation attempt with IsDirty = true |
| **Blocking** | Yes — navigation blocked until dialog resolved |
| **Severity** | Warning (dialog) |
| **User Message** | "You have unsaved changes. Save before leaving, or discard your changes?" |
| **Accessibility** | Dialog announced; Cancel focused by default |
| **Data Preservation** | Yes — input preserved unless Discard chosen |
| **Authoritative Source** | NLE-SPC-CTX-002; NLE-SPC-DLG-001 |
| **Recovery Action** | Save or Discard; Cancel returns to form |
| **Status** | Existing |

### VR-GLB-003: Stale Async Result Discard

| Field | Value |
|---|---|
| **Rule ID** | VR-GLB-003 |
| **Field / Form** | All forms with async Tier 3/4 validation |
| **Domain Mapping** | Correlation token vs. current form sequence |
| **Rule Description** | Async validation results with a correlation token that does not match the current form's active token must be silently discarded. ViewModel state must not be updated from stale results. |
| **Tier** | 2 (ViewModel guard) |
| **Trigger** | Async result receipt |
| **Blocking** | Stale result: no effect |
| **Severity** | N/A — silent discard |
| **User Message** | None — invisible to user |
| **Accessibility** | N/A |
| **Data Preservation** | Yes — current input unaffected |
| **Authoritative Source** | NLE-SPC-FVR-001 §4; NLE-SPC-FRM-001 |
| **Recovery Action** | None — handled automatically |
| **Status** | Approved |

### VR-GLB-004: UNCERTAIN Outcome Must Not Collapse

| Field | Value |
|---|---|
| **Rule ID** | VR-GLB-004 |
| **Field / Form** | All operation result presentations |
| **Domain Mapping** | `OperationStatus.UNCERTAIN` |
| **Rule Description** | When an operation's outcome status is `UNCERTAIN`, the UI must display `UNCERTAIN` and must not automatically classify it as `COMPLETED` or `FAILED`. No blind retry may be initiated. |
| **Tier** | 2 |
| **Trigger** | Operation result received |
| **Blocking** | Blocks auto-navigation on completion |
| **Severity** | Warning |
| **User Message** | "The outcome of this operation could not be confirmed. Review the operation log and decide how to proceed." |
| **Accessibility** | Announced Assertive |
| **Data Preservation** | Yes — IsDirty unchanged |
| **Authoritative Source** | NLE-DOM-SPEC-001 §OperationStatus; NLE-SPC-FVR-001 §5.9 |
| **Recovery Action** | User reviews state; decides on retry eligibility |
| **Status** | Approved |

---

## 13. Rule Index Summary

| Rule ID | Form Area | Tier | Blocking |
|---|---|---|---|
| VR-META-001 | Project Name | 1 | Yes |
| VR-META-002 | Project Name length | 1 | Yes |
| VR-META-003 | Slug format | 1 | Yes |
| VR-META-004 | Slug uniqueness | 3 | Yes |
| VR-META-005 | Description length | 1 | Yes |
| VR-PATH-001 | Path required | 1 | Yes |
| VR-PATH-002 | Volume D enforcement | 1+4 | Yes (Critical) |
| VR-PATH-003 | Path unique | 3 | Yes |
| VR-PATH-004 | Path writeable | 3 | Yes |
| VR-PATH-005 | Disk space ≥ 10 GB | 4 | Yes (Critical) |
| VR-GAME-001 | Anti-cheat hard block | 2+4 | Yes (Critical) |
| VR-GAME-002 | Distinct game pair | 2 | Yes |
| VR-GAME-003 | Both games required | 1 | Yes |
| VR-VER-001 | UNKNOWN version permitted | 1 (Info) | No |
| VR-VER-002 | Version format | 1 | Yes |
| VR-VER-003 | Environment permitted values | 1 | Yes |
| VR-THESIS-001 | Intent minimum length | 1 | Yes |
| VR-THESIS-002 | Intent maximum length | 1 | Yes |
| VR-CAP-001 | Capability or exploratory | 2 | Yes |
| VR-CAP-002 | Evidence tier advisory | 1 (Info) | No |
| VR-STRAT-001 | Disclaimer acknowledgement | 2 | Yes |
| VR-REQ-001 | Requirement title required | 1 | Yes |
| VR-REQ-002 | Requirement title length | 1 | Yes |
| VR-REQ-003 | Requirement status values | 1 | Yes |
| VR-REQ-004 | Requirement body length | 1 | Yes |
| VR-CFG-001 | Credential ref format | 1 | Yes |
| VR-CFG-002 | AI provider for AI ops | 2 | Yes |
| VR-CFG-003 | Output dir Volume D | 1+4 | Yes |
| VR-GLB-001 | No raw token | 1 | Yes |
| VR-GLB-002 | Unsaved change guard | 2 | Yes |
| VR-GLB-003 | Stale async discard | 2 | No |
| VR-GLB-004 | UNCERTAIN not collapsed | 2 | Blocks auto-nav |

---

## 14. Document Control

| Field | Value |
|---|---|
| Author | NexLudic Engine Architecture Team |
| Step | 23 — Forms, Validation & Error-Recovery Experience |
| Created | 2026-11-17 |
| Extends | NLE-CAT-WIZ-001 — Project Creation Field Catalog |
| Implementation Authorization | Documentation only |

