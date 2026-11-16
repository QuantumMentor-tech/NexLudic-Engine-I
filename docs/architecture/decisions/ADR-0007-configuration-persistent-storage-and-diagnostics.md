# ADR-0007: Configuration, Persistent Storage, and Diagnostic Logging
## Document ID: NLE-ADR-0007
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** Senior Systems Architect & Data Systems Lead  

---

### 1. Context and Problem
NexLudic Engine must persist user-created mashup projects, blueprint specifications, game library scan caches, adapter capability declarations, and audit logs. The storage architecture must be lightweight, human-readable, corrupt-resistant, and easily backed up and restored without requiring database servers or complex ORMs during the initial MVP.

### 2. Requirements Addressed
- `REQ-CORE-004`: Project Lifecycle Management.
- `REQ-MASH-004`: Structured Mashup Blueprint.
- `REQ-SAFE-002`: Local-Only Telemetry & Privacy.
- `DEC-009`: Workspace allocation to Volume D:.

### 3. Constraints and Assumptions
- Mashup projects represent self-contained filesystem directories.
- SQLite is unnecessary for MVP flat configurations, but may be useful in Phase 3 for deep telemetry indexing.
- System drive `C:` has limited free space (9.79 GB), mandating that all persistent project storage reside on Volume `D:`.

### 4. Options Considered
- **Option 1: Tiered File-Based Storage (Versioned JSON + Serilog).** Flat JSON files for manifests/blueprints; rolling text files for structured logs.
- **Option 2: Mandatory SQLite Database Day 1.** Binary `.db` file; requires database migrations, locks, and complex corruption recovery.
- **Option 3: Embedded Document Store (LiteDB).** Additional third-party dependency; binary format opaque to users.

### 5. Evaluation Criteria
- Human inspectability and git diffability.
- Backup, restore, and portability simplicity.
- Resilience against process termination and crashes.
- Dependency minimization.

### 6. Evidence and Source Links
- Step 3 Audit (`DEVELOPMENT_ENVIRONMENT_AUDIT.md`) noting C: drive storage limits.
- Step 5 Technology Evaluation (`TECHNOLOGY_EVALUATION.md`).

### 7. Decision
NexLudic Engine adopts **Tiered File-Based Storage for MVP**:
1. **Project & Blueprint State:** Versioned JSON manifests (`manifest.json`, `mashup-blueprint.json`, `adapter.json`) serialized via `System.Text.Json` with Draft 2020-12 schema validation.
2. **Diagnostic Logs:** Structured JSON and text logs written via **Serilog** with rolling file sinks and an in-memory streaming sink for the desktop UI console.
3. **Storage Location:** Default project root configured to `D:\App Winrar Files\Nexludic Engine (working)\projects\` (or user-selected Volume D: path).
4. **Relational Database (SQLite):** Formally deferred to Phase 3.

### 8. Rationale
Flat JSON manifests make every project completely self-contained and portable. A user or modder can copy a project folder to another drive, inspect its blueprint in any text editor, and restore it without database migration scripts or corruption risks.

### 9. Advantages
- Zero external database dependencies; zero database lock contention.
- Bit-for-bit atomic backup and rollback by copying the project directory.
- 100% human-readable configurations.

### 10. Disadvantages and Risks
- Inefficient for querying thousands of historical live telemetry events (mitigated by deferring deep telemetry recording to Phase 3 SQLite).

### 11. Consequences for Other Components
The `NexLudic.Infrastructure` layer will implement a strongly typed `JsonProjectRepository` and configure Serilog rolling sinks.

### 12. Dependencies
- `System.Text.Json`, `Serilog`, `Serilog.Sinks.File`.

### 13. Conditions That Would Invalidate the Decision
Evidence in Phase 3 that project indexing or search performance requires relational querying.

### 14. Follow-Up Work
Define the JSON schemas for `manifest.json` and `mashup-blueprint.json` in Phase 2.

### 15. Owner Approval Required
Pending Project Owner formal approval.

