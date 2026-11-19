# Host-Game Modification Strategy Selection & Approval Policy

**Document Identifier:** `NLE-POL-GMS-001`  
**Classification:** Operational Governance & Security Policy  
**Status:** DRAFT / APPROVED FOR ARCHITECTURAL BASELINE  
**Phase:** Phase 8 (Host-Game Modification & Content Pipeline) — Step 61 of 100  
**Upstream Dependencies:** `NLE-SPC-GMS-001`, `NLE-MOD-GMS-001`, `NLE-GAT-PH7-001`  
**Security Level:** Internal Engineering Architecture  

---

## 1. Objective & Policy Scope

This policy governs the automated evaluation, ranking, selection, and user approval workflow for applying modification strategies to target game installations. The engine MUST never apply any change without passing through this deterministic decision and approval gate.

---

## 2. Evaluation Inputs & Matrix

The selection engine evaluates candidate strategies against eight strict environmental and metadata inputs:
1. **Target Engine Type & Version:** (e.g. Unreal 5.3, Unity Mono 2022.3, Source).
2. **Store Platform & File Permissions:** Is the install directory read-only or UWP-sandboxed?
3. **Anti-Cheat Profile:** Presence of BattlEye, EAC, Vanguard, or Denuvo.
4. **Mod Loader Presence:** Are community loaders (BepInEx, SKSE, UE4SS, REDmod) detected and operational?
5. **Modification Scope:** Are we adding static assets, replacing existing textures, injecting configs, or running scripts?
6. **Reversibility Guarantee:** Can the operation be cleanly undone without full game reinstallation?
7. **Existing User Modifications:** Are there conflicting files or mod managers in place?
8. **User Privilege Level:** The operation must strictly succeed within Windows Medium Integrity Level.

---

## 3. Strategy Family Hierarchy & Precedence

When multiple mechanisms are technically possible, the engine MUST select the safest mechanism following this strict hierarchy:

```
[Rank 1: Engine-Native Mod Directory Drop]
        | (Safest, completely non-destructive, isolated namespace)
        v
[Rank 2: Documented Mod Loader Plugin]
        | (Utilizes official/community mod framework APIs)
        v
[Rank 3: User-Data Configuration & Override]
        | (Modifies only designated user Documents/AppData files)
        v
[Rank 4: Virtual File System / Archive Repackaging]
        | (Higher complexity; requires baseline archive backup)
        v
[Rank 5: Prohibited Strategies (REJECTED IMMEDIATELY)]
```

### Prohibited Strategies (Strict Rejection):
- **`REJ-BIN-PATCH`:** Modifying native executable (`.exe`) or core DLL bytes.
- **`REJ-MEM-INJECT`:** Dynamic library injection into running game processes.
- **`REJ-AC-BYPASS`:** Disabling, spoofing, or tampering with anti-cheat drivers or binaries.
- **`REJ-ELEVATION`:** Requesting administrative (High/System IL) elevation.

---

## 4. Deterministic Decision Tree

```
                       [Target Instance Evaluated]
                                    |
                    +---------------+---------------+
                    |                               |
          [Anti-Cheat Detected?]          [No Anti-Cheat / Offline]
                    |                               |
             +------+------+                        |
             |             |                        v
         [YES]            [NO]         [Native Mod Folder Supported?]
           |               |                        |
           v               v                 +------+------+
     [REJECT ALL]    (Proceed)               |             |
   (Online Protected)                      [YES]          [NO]
                                             |             |
                                             v             v
                                        [Select Rank 1]  [Mod Loader Detected?]
                                                           |
                                                    +------+------+
                                                    |             |
                                                  [YES]          [NO]
                                                    |             |
                                                    v             v
                                              [Select Rank 2]  [Archive Repack Supported?]
                                                                  |
                                                           +------+------+
                                                           |             |
                                                         [YES]          [NO]
                                                           |             |
                                                           v             v
                                                     [Select Rank 4]  [UNSUPPORTED_STRATEGY]
```

---

## 5. Human-in-the-Loop Approval Workflow

In compliance with `P7-C01` and safety governance:
1. **Plan Formulation:** Modification Strategy Engine produces a `ModificationPlan` (`NLE-MOD-GMS-001`).
2. **Impact Analysis:** Engine generates a human-readable summary listing:
   - Target Game Title and exact install path.
   - Selected mechanism and rationale.
   - List of all files to be added, overwritten, or created.
   - Disk space required for staged content and baseline backup.
   - Reversibility status (`ATOMIC_SNAPSHOT_REVERT`).
3. **User Confirmation Prompt:** The desktop UI presents the explicit modification review dialog.
4. **Cryptographic Signing:** Upon user confirmation, a signed token is generated (`UserApprovalReceipt`), binding the specific `plan_id` and checksums.
5. **Execution Authorization:** The pipeline execution engine executes the handoff only if a valid, unexpired approval receipt is present.
