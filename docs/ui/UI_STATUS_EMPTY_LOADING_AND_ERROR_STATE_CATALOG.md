# UI STATUS, EMPTY, LOADING & ERROR STATE CATALOG

**Document Identifier:** `NLE-CAT-STE-001`  
**Phase:** 3 (Desktop Interface & Workspace Management) — Step 22 of 100  
**Status:** Approved Architectural Specification  
**Classification:** Semantic Status Catalog, State Presentation & Epistemic Honesty Rules  
**Parent Document:** `NLE-SPC-CMP-001`

> **Core Epistemic Rule:** The UI must never represent a state as more certain than the authoritative domain record declares it. Unknown is not failure. Undeclared is not success. Not evaluated is not compatible.

---

## 1. Project Lifecycle Status

**Component:** `ProjectLifecycleBadge` (CMP-040)  
**Authoritative Source:** `ProjectManifest.LifecycleState` via `IProjectManagementService`

| State | Label | Color Token | Accessible Description |
|:---|:---|:---|:---|
| `CREATED` | "Created" | `NleSurface.Neutral` | "Project created, setup not started" |
| `RESEARCH` | "Research" | `NleStatus.Info` (blue) | "Project under feasibility research" |
| `PLANNING` | "Planning" | `NleStatus.InfoAlt` (indigo) | "Integration strategy being planned" |
| `PROTOTYPE` | "Prototype" | `NleStatus.Warning` (orange) | "Prototype implementation in progress" |
| `VALIDATED` | "Validated" | `NleStatus.Success` (green) | "Project validated within defined scope" |
| `BLOCKED` | "Blocked" | `NleStatus.Critical` (red) | "Project blocked — action required" |
| `ARCHIVED` | "Archived" | `NleSurface.Muted` | "Project archived — read only" |
| `UNKNOWN` | "Unknown" | `NleStatus.Caution` (yellow) | "Project state could not be determined" |

---

## 2. Feasibility Outcome

**Component:** `FeasibilityOutcomeBadge` (CMP-041)  
**Authoritative Source:** `FeasibilityAssessment.Outcome` via `FeasibilityAssessmentService`

| Outcome | Label | Color | Tooltip (Always Present) |
|:---|:---|:---|:---|
| `UNEVALUATED` | "Not Assessed" | Amber | "No feasibility audit has been run for this project." |
| `FEASIBLE` | "Conditionally Feasible" | Green outlined | "Feasibility is conditional on specific versions and adapter support. Integration testing required to confirm." |
| `PARTIAL` | "Partially Feasible" | Orange | "Some requested capabilities are feasible; others are blocked or unresolved." |
| `BLOCKED` | "Blocked" | Red | "One or more critical capability dependencies could not be satisfied." |
| `IMPOSSIBLE` | "Not Feasible" | Dark red | "The declared requirements cannot be achieved with currently known methods." |
| `STALE` | "Stale — Reassessment Required" | Yellow | "The feasibility assessment was based on a game version or configuration that has since changed." |
| `UNKNOWN` | "Unknown" | Grey | "Feasibility state could not be retrieved." |

**Hard rule:** `FEASIBLE` and `PARTIAL` badges must display a warning tooltip at all times. These are research outcomes, not guaranteed compatibility assertions.

---

## 3. Capability Status

**Component:** `CapabilityStatusRow` (CMP-042)  
**Authoritative Source:** `CapabilityRequirement.EpistemicTier` + `CoverageStatus`

| Epistemic Tier | Label | Color |
|:---|:---|:---|
| `DECLARED` | "Declared (Unverified)" | Yellow |
| `DETECTED` | "Detected (Filesystem)" | Blue |
| `PROBED` | "Adapter Probed" | Indigo |
| `VERIFIED` | "Integration Verified" | Green |

| Coverage Status | Label |
|:---|:---|
| `UNCOVERED` | "No adapter coverage" |
| `PARTIAL` | "Partially covered" |
| `COVERED` | "Covered by adapter" |
| `BLOCKED` | "Blocked — no known mechanism" |

---

## 4. Adapter Qualification

**Component:** `AdapterQualificationCard` (CMP-043)  
**Authoritative Source:** `AdapterRegistryService`

| Qualification | Label | Color |
|:---|:---|:---|
| `UNSCANNED` | "Not Scanned" | Grey |
| `QUALIFIED` | "Qualified (Version-Specific)" | Green |
| `PARTIAL` | "Partial — Limited Capabilities" | Orange |
| `INCOMPATIBLE` | "Incompatible with Target Version" | Red |
| `UNSUPPORTED` | "Not Supported for This Game" | Dark red |

**Hard rule:** `QUALIFIED` card must always display: `Supported: [VersionRange]`. It must never display universally without version scope.

---

## 5. Operation & Task Status

**Component:** `OperationStatusRow` (CMP-044)  
**Authoritative Source:** `OperationRecord.Status` from operation service

| Status | Label | Color | Spinner |
|:---|:---|:---|:---|
| `QUEUED` | "Queued" | Grey | No |
| `AWAITING_APPROVAL` | "Awaiting Your Review" | Blue | No |
| `RUNNING` | "Running…" | Blue | Yes |
| `COMPLETED` | "Completed" | Green | No |
| `PARTIAL` | "Partially Completed" | Orange | No |
| `FAILED` | "Failed" | Red | No |
| `CANCELLED` | "Cancelled" | Muted | No |
| `UNCERTAIN` | "Outcome Unknown — Check Diagnostics" | Yellow | No |

**Hard rule:** `UNCERTAIN` must never be collapsed into `FAILED` or `COMPLETED`. Side effects may have occurred.

---

## 6. Approval Status

**Component:** `ApprovalSummaryCard` (CMP-045)

| Status | Label | Color |
|:---|:---|:---|
| `PENDING` | "Awaiting Your Decision" | Blue |
| `APPROVED` | "Approved" | Green |
| `REJECTED` | "Rejected" | Red |
| `EXPIRED` | "Expired — Re-request Required" | Muted |

---

## 7. Test Result

**Component:** `TestResultBadge` (CMP-046)

| Outcome | Label | Color |
|:---|:---|:---|
| `NOT_RUN` | "Not Executed" | Grey |
| `PASSED` | "Passed" | Green |
| `FAILED` | "Failed" | Red |
| `INCOMPLETE` | "Incomplete" | Orange |
| `SKIPPED` | "Skipped" | Muted |
| `BLOCKED` | "Blocked" | Yellow |

**Hard rule:** An empty test collection (zero records) must render as `NOT_RUN`, not as `PASSED` or any neutral success indicator.

---

## 8. Evidence Provenance & Freshness

**Component:** `EvidenceReferenceRow` (CMP-047)

| Source Type | Label | Trust Visual |
|:---|:---|:---|
| `USER_DECLARED` | "User Declared" | Yellow — unverified |
| `DOCUMENTATION` | "Published Documentation" | Blue — referenced |
| `PROTOTYPE` | "Prototype Result" | Orange — experimental |
| `INTEGRATION_TEST` | "Integration Test Evidence" | Green — tested |
| `REGRESSION` | "Regression Validated" | Dark green — regression |

| `IsFresh` | Display |
|:---|:---|
| `true` | Normal display |
| `false` | Yellow warning chip: "⚠ Stale — Game version has changed since this evidence was collected" |

---

## 9. Empty States

All empty states use `EmptyStatePanel` (CMP-024). The following are the canonical approved messages:

| Destination / Context | Icon | Title | Description | Primary Action |
|:---|:---|:---|:---|:---|
| Project Overview (new) | Rocket | "Welcome" | "Your workspace is ready. Start by reviewing your game pair or declaring requirements." | "Review Games" |
| Requirements (empty) | List | "No Requirements Declared" | "Declare the capabilities your mashup needs." | "+ Add Requirement" |
| Feasibility (unevaluated) | Magnifier | "Not Assessed" | "Run a feasibility audit to evaluate declared capabilities." | "Run Feasibility Audit" |
| Testing (no runs) | Flask | "No Test Runs" | "Zero test executions recorded. This does not indicate compatibility." | "Run Smoke Tests" |
| AI Tasks (empty) | Robot | "No Active Tasks" | "Dispatch an AI agent to scaffold code or analyze integration options." | "New AI Task" |
| Artifacts (empty) | Box | "Staging Clean" | "No generated artifacts present. Artifacts appear here after build steps." | "Open Staging Folder" |
| Adapters (unbound) | Plug | "No Adapters Bound" | "Scan for local mod loaders or browse the adapter catalog." | "Scan Local Adapters" |
| Search (no results) | Search | "No Matches" | "No items matched your search query. Try broader terms." | "Clear Search" |

---

## 10. Loading States

| Scope | Pattern | Duration Guidance |
|:---|:---|:---|
| Field-level (async lookup) | Inline `ProgressRing` (16px) next to field | < 2s expected |
| List/grid loading | Skeleton row placeholders (3 animated grey bars) | < 3s expected |
| Page-level context load | Full-area skeleton layout matching final structure | < 5s expected |
| Background operation | `OperationProgressCard` with indeterminate bar | Indefinite |
| Pre-flight audit | Inline progress per check item | 1–10s |

**Hard rules:**
- Do NOT show a percentage progress value unless the authoritative service reports actual `ProgressValue` (0.0–1.0).
- Do NOT show `ProgressRing` after a result is received; transition immediately to result state.
- Do NOT block unrelated UI areas while a single section loads.

---

## 11. Error & Degraded States

| Error Category | Presentation | Recovery Action |
|:---|:---|:---|
| Recoverable load failure | Inline error card: icon + message + [Retry] button | Retry triggers re-fetch via ViewModel |
| Invalid project reference | Full-area "Project Not Found" with [Open Projects List] | Navigate to global Projects Manager |
| Write permission denied | Amber banner in Context Header: "Read-Only Mode" | User resolves permissions externally |
| AI provider offline | Disabled AI task controls with tooltip: "AI Provider Offline — Configure in Settings" | Navigate to Global Settings |
| Corrupt manifest | Full-area recovery prompt: "Project files may be corrupted — view diagnostics" | Link to `NAV-PRJ-DIA` |
| Stale project context | Yellow banner: "Project data may be outdated — Refresh" | [Refresh] command re-loads manifest |

**Hard rule:** Raw exception messages, stack traces, and internal file paths must never appear in user-facing error cards. Log these to Serilog; surface only a human-readable summary.

---

## 12. Partial Data & Stale Context States

| Condition | Presentation |
|:---|:---|
| Some sections loaded, others pending | Loaded sections render normally; pending sections show skeleton loaders |
| Evidence exists but freshness unknown | Yellow "Freshness Unknown" chip on `EvidenceReferenceRow` |
| Feasibility data present but game version changed since audit | `FeasibilityOutcomeBadge` shows `STALE` state |
| Project manifest loaded but operational logs unavailable | Diagnostics section shows "Log Service Unavailable" inline error; Overview remains functional |

