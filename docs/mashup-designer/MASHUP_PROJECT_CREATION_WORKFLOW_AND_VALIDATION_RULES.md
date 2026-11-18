# MASHUP PROJECT CREATION WORKFLOW AND VALIDATION RULES
**Document ID:** `NLE-PRO-MCW-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Technical Product Designer, Senior QA Automation Engineer, Principal Software Architect, Domain Modeling Specialist  
**Classification:** Operational Procedure & Validation Catalog  

---

## 1. PURPOSE AND WORKFLOW OBJECTIVES

### 1.1 Purpose
This specification establishes the detailed step-by-step workflow and comprehensive catalog of domain validation rules governing the creation and validation of a **Mashup Project** (`MashupProject`) within NexLudic Engine. 

Creating a cross-game mashup project must be frictionless and intuitive for creators while maintaining strict architectural rigor. The workflow allows users to save partial, incomplete ideas, defer complex technical decisions, and receive actionable, non-destructive validation guidance at every step.

---

## 2. STEP-BY-STEP PROJECT CREATION WORKFLOW

```mermaid
flowchart TD
    Step1["Step 1: Initialize Project (Assign UUID & Name)"] --> Step2["Step 2: Select Participating Games (Catalog Pick)"]
    Step2 --> Step3["Step 3: Define Version & Environment Constraints"]
    Step3 --> Step4["Step 4: Author Creative Objective & Thesis"]
    Step4 --> Step5["Step 5: Add Feature Requirements (Mandatory/Optional)"]
    Step5 --> Step6["Step 6: Select Integration Strategy (or Undecided)"]
    Step6 --> Step7["Step 7: Pre-Flight Domain Validation"]
    Step7 -->|Validation Fails (Errors)| FixErrors["Display Non-Destructive Error Guidance"]
    FixErrors --> Step7
    Step7 -->|Validation Passes| Step8["Step 8: Atomic Commit to Volume D (project.json)"]
    Step8 --> Step9["Step 9: Initialize Revision 1 & Transition to READY"]
```

### Detailed Workflow Stages:
1. **Step 1: Project Initialization:**  
   - User clicks "New Mashup" in the NexLudic Desktop Application Shell (`NLE-SPC-SHL-001`).
   - The engine generates a cryptographically random RFC 4122 UUIDv4 `project_id`.
   - User inputs a human-readable `display_name` (3–80 characters).
   - System allocates workspace directory `D:\NexLudic\Projects\<projectId>\` (`DEC-009`).
2. **Step 2: Participant Game Selection:**  
   - User queries the Phase 4 Canonical Game Catalog (`NLE-CAT-CGI-001`).
   - User selects Game A and Game B. Assigns initial architectural roles (`HOST_ROLE`, `GUEST_ROLE`, or `PEER_ROLE`).
3. **Step 3: Environment & Version Constraints:**  
   - User selects target game edition (e.g. Java, Bedrock, Remastered) and SemVer constraint.
   - If the game is already installed locally, user can optionally link the detected local installation.
4. **Step 4: Creative Objective & Thesis:**  
   - User provides the mashup title and authors the `core_thesis` (minimum 20 characters) explaining the gameplay vision.
5. **Step 5: Feature Requirements Authoring:**  
   - User drafts initial feature items, marking each as `MANDATORY` (critical to core thesis) or `OPTIONAL` (enhancement).
6. **Step 6: Strategy Selection:**  
   - User chooses initial strategy: `STRATEGY_1_HOST_MOD`, `STRATEGY_2_LIVE_BRIDGE`, `STRATEGY_3_HYBRID`, or selects `STRATEGY_UNDECIDED`.
7. **Step 7: Pre-Flight Domain Validation:**  
   - Engine executes the 32 formal validation rules defined below.
   - If blocking errors exist, user is guided to the exact fields needing correction. Incomplete drafts can be saved as `PROJECT_INCOMPLETE`.
8. **Step 8: Atomic Workspace Commit:**  
   - Engine serializes the project aggregate to `project.json` via atomic file swap (`.tmp` $\rightarrow$ `project.json`).
9. **Step 9: Revision 1 Initialization:**  
   - Snapshot written to `revisions/rev_0001.json`; audit hash recorded; lifecycle set to `PROJECT_READY_FOR_ASSESSMENT`.

---

## 3. FORMAL DOMAIN VALIDATION RULE CATALOG (32 RULES)

### 3.1 Identity & Metadata Validation Rules (`VR-ID`)

| Rule ID | Trigger Condition | Severity | Validation Check & Requirement | Diagnostic Error Code |
| :--- | :--- | :---: | :--- | :--- |
| `VR-ID-01` | Project creation | `ERROR` | `project_id` MUST be valid UUIDv4 string format. | `ERR_PRJ_INVALID_UUID` |
| `VR-ID-02` | Name input | `ERROR` | `display_name` MUST be 3 to 80 characters, non-whitespace. | `ERR_PRJ_INVALID_NAME` |
| `VR-ID-03` | Name input | `WARN` | `display_name` SHOULD be unique across local workspace. | `WARN_PRJ_DUPLICATE_NAME` |
| `VR-ID-04` | Workspace path | `CRITICAL`| `workspace_path` MUST start with `D:\NexLudic\Projects\`. | `CRIT_SEC_INVALID_WORKSPACE` |
| `VR-ID-05` | Schema version | `ERROR` | `schema_version` MUST match supported SemVer pattern (`1.x`). | `ERR_PRJ_UNSUPPORTED_SCHEMA` |

### 3.2 Participant Validation Rules (`VR-PAR`)

| Rule ID | Trigger Condition | Severity | Validation Check & Requirement | Diagnostic Error Code |
| :--- | :--- | :---: | :--- | :--- |
| `VR-PAR-01` | Project validation | `ERROR` | `participants` array MUST contain at least 2 distinct games. | `ERR_PAR_MIN_PARTICIPANTS` |
| `VR-PAR-02` | Participant added | `ERROR` | Cannot add duplicate participant with same `canonical_game_id`. | `ERR_PAR_DUPLICATE_GAME` |
| `VR-PAR-03` | Participant added | `ERROR` | `canonical_game_id` MUST resolve against Phase 4 catalog. | `ERR_PAR_UNKNOWN_GAME_ID` |
| `VR-PAR-04` | Role assignment | `ERROR` | Assigned role MUST be one of 5 formal role enums. | `ERR_PAR_INVALID_ROLE` |
| `VR-PAR-05` | Host Mod strategy | `ERROR` | If `STRATEGY_1_HOST_MOD`, exactly one participant MUST be `HOST_ROLE`. | `ERR_ROL_MISSING_HOST` |
| `VR-PAR-06` | Host Mod strategy | `ERROR` | If `STRATEGY_1_HOST_MOD`, at least one participant MUST be `GUEST_ROLE`. | `ERR_ROL_MISSING_GUEST` |
| `VR-PAR-07` | Live Bridge strategy| `ERROR` | If `STRATEGY_2_LIVE_BRIDGE`, participants MUST be `PEER_ROLE` or `SRC/TGT`.| `ERR_ROL_INVALID_PEER` |
| `VR-PAR-08` | Version constraint | `WARN` | Unspecified version defaults to `*`; emits advisory warning. | `WARN_PAR_UNCONSTRAINED_VERSION`|

### 3.3 Objectives & Requirements Validation Rules (`VR-REQ`)

| Rule ID | Trigger Condition | Severity | Validation Check & Requirement | Diagnostic Error Code |
| :--- | :--- | :---: | :--- | :--- |
| `VR-REQ-01` | Objective input | `ERROR` | `core_thesis` MUST be between 20 and 2,000 characters. | `ERR_OBJ_THESIS_TOO_SHORT` |
| `VR-REQ-02` | Requirement input | `ERROR` | Requirement `title` MUST be 5 to 120 characters. | `ERR_REQ_INVALID_TITLE` |
| `VR-REQ-03` | Requirement input | `ERROR` | Requirement `description` MUST be 10 to 2,000 characters. | `ERR_REQ_DESC_TOO_SHORT` |
| `VR-REQ-04` | Category check | `ERROR` | `category` MUST match one of 7 formal requirement categories. | `ERR_REQ_INVALID_CATEGORY` |
| `VR-REQ-05` | Priority check | `ERROR` | `priority` MUST be `CRITICAL`, `HIGH`, `MEDIUM`, or `LOW`. | `ERR_REQ_INVALID_PRIORITY` |
| `VR-REQ-06` | Criticality check | `ERROR` | `criticality` MUST be `MANDATORY`, `OPTIONAL`, or `CONDITIONAL`. | `ERR_REQ_INVALID_CRITICALITY` |
| `VR-REQ-07` | Mandatory check | `WARN` | Project SHOULD have at least one `MANDATORY` requirement. | `WARN_REQ_NO_MANDATORY_REQS` |
| `VR-REQ-08` | Dependency graph | `ERROR` | Requirement dependency edges MUST NOT contain circular cycles. | `ERR_REQ_CIRCULAR_DEPENDENCY` |
| `VR-REQ-09` | Dependency link | `ERROR` | Target of `REQUIRES` edge MUST exist within project requirements. | `ERR_REQ_DANGLING_DEPENDENCY` |
| `VR-REQ-10` | Conflict check | `ERROR` | Two requirements linked by `CONFLICTS_WITH` cannot both be `ACCEPTED`.| `ERR_REQ_MUTUAL_CONFLICT` |

### 3.4 Strategy & Security Rules (`VR-STR` & `VR-SEC`)

| Rule ID | Trigger Condition | Severity | Validation Check & Requirement | Diagnostic Error Code |
| :--- | :--- | :---: | :--- | :--- |
| `VR-STR-01` | Strategy input | `ERROR` | `strategy_type` MUST be one of 4 formal strategy enums. | `ERR_STR_INVALID_STRATEGY` |
| `VR-SEC-01` | Anti-Cheat Scan | `WARN` | If participant uses kernel anti-cheat, flag non-bypassable warning. | `WARN_SEC_ANTI_CHEAT_DETECTED` |
| `VR-SEC-02` | Anti-Cheat Hook | `CRITICAL`| Memory hooking / injection strategy banned on anti-cheat titles (`BLK-SEC-01`). | `CRIT_SEC_ANTI_CHEAT_BLOCKED` |
| `VR-SEC-03` | Script Injection | `CRITICAL`| Project text fields MUST NOT contain script tags or shell escape codes. | `CRIT_SEC_MALICIOUS_INPUT` |
| `VR-SEC-04` | Storage Path | `CRITICAL`| Disallow directory traversal (`..`) in project relative paths. | `CRIT_SEC_PATH_TRAVERSAL` |

### 3.5 Lifecycle & Revision Rules (`VR-LFC` & `VR-REV`)

| Rule ID | Trigger Condition | Severity | Validation Check & Requirement | Diagnostic Error Code |
| :--- | :--- | :---: | :--- | :--- |
| `VR-LFC-01` | Lifecycle shift | `ERROR` | Transition MUST conform to formal State Transition Matrix. | `ERR_LFC_ILLEGAL_TRANSITION` |
| `VR-LFC-02` | Leapfrog check | `ERROR` | Cannot transition to `PLAN_AVAILABLE` without completing assessment. | `ERR_LFC_PREMATURE_PLAN` |
| `VR-REV-01` | Revision save | `ERROR` | `revision_number` MUST increase monotonically on every save. | `ERR_REV_NON_MONOTONIC` |
| `VR-REV-02` | Checksum verify | `ERROR` | `snapshot_sha256` MUST match computed SHA-256 of snapshot JSON. | `ERR_REV_CHECKSUM_MISMATCH` |
| `VR-REV-03` | Invalidation trigger| `INFO` | Modifying participants resets lifecycle to `READY_FOR_ASSESSMENT`. | `INFO_LFC_STATE_RESET` |

---

## 4. EDGE CASE & FAULT RECOVERY BEHAVIOR

### 4.1 Autosave and Recovery from Interrupted Sessions
If the desktop application terminates unexpectedly during project creation:
1. **Autosave Draft:** The wizard saves an ephemeral draft to `D:\NexLudic\Projects\<id>\.draft.json` upon every valid field entry.
2. **Crash Recovery Prompt:** Upon next application startup, the user is presented with a "Resume Project Draft" prompt.
3. **Draft Cleanup:** Once successfully committed to `project.json`, the `.draft.json` file is deleted.

### 4.2 Handling Incomplete Projects
If a creator wishes to pause creation before adding requirements or selecting games:
- The project is committed with status `PROJECT_INCOMPLETE`.
- Incomplete projects are prominently tagged in the workspace UI with an amber badge, allowing creators to resume editing at any time.

---
*End of Specification — Document `NLE-PRO-MCW-001`.*
