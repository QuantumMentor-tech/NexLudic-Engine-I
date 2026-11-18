# MASHUP PARTICIPANT IDENTITY AND ENVIRONMENT REFERENCE SPECIFICATION
**Document ID:** `NLE-SPC-MPI-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 43 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Game Interoperability Specialist, Domain Modeling Engineer, Software Architect, Senior QA Engineer  
**Classification:** Core Domain Specification  

---

## 1. PURPOSE AND ARCHITECTURAL INTEGRATION

### 1.1 Purpose
This specification establishes the authoritative rules, data bindings, role definitions, and environment references governing **Project Participants** (`ProjectParticipant`) within a NexLudic Mashup Project.

Participating games form the foundational anchors of any cross-game integration. To prevent data corruption and false compatibility claims, this specification:
1. Reconciles project participant references with Phase 4 Canonical Game Identity standards (`NLE-CAT-CGI-001`).
2. Defines the participant role taxonomy and its relationship to integration strategies.
3. Decouples canonical game catalog entries from optional local installation instances.
4. Formalizes environment constraints, SemVer version matching, and change invalidation triggers.

---

## 2. CANONICAL GAME IDENTITY BINDING

### 2.1 Immutable Canonical Identity vs. Free-Text Labels
A common defect in game modding tools is identifying games by localized display titles or arbitrary folder names. The NexLudic platform strictly forbids this:
1. **Canonical Identifier Mandate:** Every participant MUST bind to an authoritative canonical game identifier (`canonical_game_id`) defined in the Phase 4 catalog (Step 25).
2. **Regex Syntax:** Identifiers conform strictly to `^game\.[a-z0-9_.-]+$` (e.g. `game.minecraft.java`, `game.doom.1993`, `game.synth.arena_fps`).
3. **Display Name Independence:** Users may assign custom labels or nicknames to a participant in the UI, but the underlying domain references, capability matches, and compatibility assessments bind exclusively to `canonical_game_id`.

### 2.2 Pre-Installation Project Creation Principle
In accordance with product baseline requirements:
- **No Local Install Required at Creation:** A creator MUST be permitted to design, configure, and evaluate a mashup project for games that are NOT currently installed on their workstation.
- **Local Installation Optionality:** The field `local_installation_ref` is strictly **optional**. When present, it references a verified local installation discovered by Phase 4 (`NLE-MOD-LID-001`). When absent, the project operates in "Conceptual Planning" mode using canonical catalog metadata.

---

## 3. MULTI-PARTICIPANT TOPOLOGY AND ARBITRARY PAIRS

### 3.1 Pairwise Baseline and N-Way Extensibility
- **Pairwise Baseline ($N = 2$):** The standard mashup topology consists of two distinct games: Game A and Game B.
- **N-Way Extensibility ($N \ge 2$):** The domain model represents participants as an array (`participants: []`), allowing future expansion to tri-game or multi-game mashups (e.g. Minecraft + Terraria + Doom) without schema changes. Max participants capped at 8 (`maxItems: 8`).
- **Duplicate Prevention Invariant:** A single project CANNOT contain duplicate participants sharing identical `(canonical_game_id, edition_id)` tuples. Adding the same game twice is rejected with `ERR_PAR_DUPLICATE_GAME`.

---

## 4. PARTICIPANT ROLE TAXONOMY & STRATEGY CONSTRAINTS

Each participant is assigned a specific functional role that dictates how its assets, logic, and runtime processes interact:

```
+--------------------------------------------------------------------------------------------------------+
|                                    PARTICIPANT ROLE TAXONOMY                                           |
|                                                                                                        |
|  [ HOST_ROLE ]   ---> The primary execution container. Render loop and player input host.              |
|  [ GUEST_ROLE ]  ---> The guest game contributing assets, logic, or items into the host.               |
|  [ PEER_ROLE ]   ---> Symmetric participant in a live bridge (neither game dominates execution).       |
|  [ SOURCE_ROLE ] ---> Directional data/state producer in an asymmetrical telemetry bridge.              |
|  [ TARGET_ROLE ] ---> Directional data/state consumer in an asymmetrical telemetry bridge.              |
+--------------------------------------------------------------------------------------------------------+
```

### 4.1 Strategy-Dependent Role Assignment Rules

| Integration Strategy | Allowed Role Topologies | Mandatory Invariants | Rejection Condition |
| :--- | :--- | :--- | :--- |
| **`STRATEGY_1_HOST_MOD`** | Exactly 1 `HOST_ROLE` + $\ge 1$ `GUEST_ROLE` | Host game receives modded content from guest. | Missing host or missing guest $\rightarrow$ `ERR_ROL_INVALID_HOST_MOD` |
| **`STRATEGY_2_LIVE_BRIDGE`**| All `PEER_ROLE` **OR** (`SOURCE_ROLE` + `TARGET_ROLE`) | Dual-process execution; data flows via IPC. | Assigning `HOST_ROLE` in peer bridge $\rightarrow$ `ERR_ROL_INVALID_PEER` |
| **`STRATEGY_3_HYBRID`** | Exactly 1 `HOST_ROLE` + $\ge 1$ `PEER_ROLE` / `GUEST_ROLE` | Host runs modified hooks; secondary runs bridge. | Undefined primary host $\rightarrow$ `ERR_ROL_INVALID_HYBRID` |
| **`STRATEGY_UNDECIDED`** | Any valid role assignment | Permitted during early exploratory drafting. | None (Roles remain advisory). |

---

## 5. ENVIRONMENT CONSTRAINTS AND VERSION BINDING

### 5.1 SemVer Version Constraints
Each participant records a target version constraint governing compatibility evaluation:
- Syntax: Standard SemVer 2.0.0 range expressions (e.g. `^1.20.1`, `>=1.12.2 <1.13.0`, `1.0.0`).
- Open / Unspecified Versions: If the creator does not know the exact version, the constraint defaults to `*` (Any Version). In this state, the project is flagged with an advisory notice (`WARN_PAR_UNCONSTRAINED_VERSION`).

### 5.2 Environment Profiles and Modding Frameworks
The `EnvironmentConstraint` entity captures prerequisite host requirements:
- **Operating System:** Windows 10 x64 / Windows 11 x64.
- **Required Modding Frameworks:** Declares dependencies on loader platforms (e.g. `Minecraft:Forge:^47.2.0`, `UnrealEngine:UE4SS:^3.0.0`).
- **Missing Information Rule:** Missing environment constraints are treated as "Unassessed Constraints", NOT as proof of universal cross-platform compatibility.

---

## 6. CHANGE INVALIDATION AND DOWNSTREAM CASCADES

When a user modifies participant data in an existing project, downstream derived artifacts are invalidated according to strict rules:

```
[ Participant Game ID or Edition Changed ]
         |
         +---> Invalidate Compatibility Assessment Reports (Step 32)
         +---> Invalidate Discovered Candidate Adapters (Step 37)
         +---> Invalidate Capability Matches (Step 38)
         +---> Invalidate Negotiated Parameters (Step 39)
         +---> Reset Lifecycle State to PROJECT_READY_FOR_ASSESSMENT

[ Version Constraint Narrowed / Changed ]
         |
         +---> Mark Compatibility Assessment as STALE
         +---> Trigger Re-Evaluation of Candidate Adapters
```

---
*End of Specification — Document `NLE-SPC-MPI-001`.*
