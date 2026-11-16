# NexLudic Engine — Pilot Selection Criteria

**Document ID:** `NLE-STD-SEL-001`  
**Version:** 1.0.0  
**Phase:** 1 — Product Definition & Research (Step 7 of 100)  
**Status:** Approved Architecture Standard  
**Date:** November 16, 2026  
**Authority:** NexLudic Architecture Review Board (ARB) & Senior QA Engineering  

---

## 1. Overview & Purpose

The **Pilot Selection Criteria** standard defines the objective evaluation model used by NexLudic Engine to screen, rate, prioritize, and select candidate game pairs for the pilot validation portfolio.

The selection model ensures that candidate selection is governed by **engineering necessity, technological diversity, and risk-gated safety**, rather than visual novelty, franchise popularity, or unverified claims.

```
+----------------------------------------------------------------------------------------------------+
|                                    CANDIDATE SELECTION WORKFLOW                                    |
+----------------------------------------------------------------------------------------------------+
                                                  |
                                                  v
                     +----------------------------------------------------------+
                     |            GATE 0: MANDATORY BLOCKER SCREEN              |
                     |  (Anti-Cheat, Drivers, Piracy, Elev, Redundancy, Crash)  |
                     +----------------------------+-----------------------------+
                                                  | Pass All Blockers
                                                  v
                     +----------------------------------------------------------+
                     |          STAGE 1: 15-DIMENSION EVALUATION SCORECARD      |
                     |    (Evidence, Feasibility, Architecture, Toolchains)     |
                     +----------------------------+-----------------------------+
                                                  | Scored & Tagged
                                                  v
                     +----------------------------------------------------------+
                     |         STAGE 2: PORTFOLIO ORTHOGONALITY AUDIT           |
                     |      (Ensures Strategy A, B, C & Runtime Diversity)      |
                     +----------------------------+-----------------------------+
                                                  | Verified Distinct
                                                  v
                     +----------------------------------------------------------+
                     |            STAGE 3: ARB HUMAN REVIEW SIGN-OFF            |
                     |        (Formal Promotion to Pilot Shortlist at G3)       |
                     +----------------------------------------------------------+
```

---

## 2. Mandatory Blockers (Zero-Tolerance Constraints)

Before a candidate pair can be scored on qualitative or architectural criteria, it must pass all six **Mandatory Blockers**. Failure on any single blocker results in an immediate **REJECTED** or **DEFERRED** determination. Blocker criteria **cannot be overridden by a high numerical score or architectural interest**.

| Blocker ID | Blocker Name | Evaluation Condition | Enforcement Action |
| :--- | :--- | :--- | :--- |
| **BLK-01** | **Anti-Cheat Interference** | Game A or Game B enforces active kernel-mode anti-cheat (Easy Anti-Cheat, BattlEye, Vanguard, Ricochet, VAC) during the targeted execution mode (`REQ-SAFE-001`, `RSK-01`). | **IMMEDIATE REJECTION.** Prohibited permanently. |
| **BLK-02** | **Proprietary Asset Redistribution** | Integration requires packaging, hosting, or distributing copyrighted textures, audio, models, or executables (`REQ-MOD-003`, `DEC-006`, `RSK-06`). | **IMMEDIATE REJECTION.** Must use local user-side extraction scripts only. |
| **BLK-03** | **Kernel-Mode Driver Dependency** | Requires kernel-mode driver installation (e.g., archived ViGEmBus), test-signing mode, or OS integrity bypass (`DEC-003`, `RSK-08`). | **IMMEDIATE REJECTION.** User-mode APIs (`SendInput`) only. |
| **BLK-04** | **Privilege Elevation Requirement** | Execution requires administrative privileges (`Run as Administrator`, high IL) (`ADR-0009`, `RSK-09`). | **IMMEDIATE REJECTION.** Standard user privilege mandatory. |
| **BLK-05** | **Closed Native Memory Injection** | Integration depends on dynamic memory injection into closed, proprietary physics/rendering threads without documented SDK hooks (`ADR-0004`, `RSK-04`). | **IMMEDIATE REJECTION.** Fails basic feasibility. |
| **BLK-06** | **Portfolio Redundancy** | Candidate replicates the engine runtime, language, and integration strategy of an already shortlisted pilot without introducing new architectural validation value. | **DEFERRED.** Avoids duplicate engineering effort. |

---

## 3. The 15 Candidate Evaluation Dimensions

Every candidate clearing the Mandatory Blockers is evaluated across 15 structured dimensions.

| Dimension ID | Dimension Name | Core Evaluation Question | Rating Scale & Interpretation |
| :--- | :--- | :--- | :--- |
| **DIM-01** | **Evidence Quality** | How well supported are the critical technical claims? | **High:** Official SDK docs / verified code.<br>**Mod:** Maintained community loader.<br>**Low:** Forum posts / single unverified demo.<br>**Unknown:** No source found. |
| **DIM-02** | **Capability Feasibility** | Can the intended user-visible outcome plausibly be achieved? | **High:** 100% of capabilities proven.<br>**Mod:** Core mechanics proven; asset/audio unverified.<br>**Low:** Critical mechanics unproven.<br>**Unknown:** Feasibility unknown. |
| **DIM-03** | **Interface Availability** | What documented or credibly supported access surfaces exist? | **High:** Public API / stable event hooks.<br>**Mod:** Decompiled bytecode / Lua bindings.<br>**Low:** Fragile memory signatures.<br>**Unknown:** No known hook. |
| **DIM-04** | **Integration Diversity** | Does this candidate test a new integration technique? | **High:** Tests Strategy B (Bridge) or C (Hybrid).<br>**Mod:** Tests distinct variant of Strategy A.<br>**Low:** Identical technique to existing pilot. |
| **DIM-05** | **Architectural Value** | Will it test an important universal-platform assumption? | **High:** Exercises cross-process IPC, Win32 Job sandbox, or multi-toolchain.<br>**Mod:** Tests single-engine mod packaging.<br>**Low:** Trivial config tweak. |
| **DIM-06** | **Language Diversity** | Does it exercise a distinct toolchain or runtime? | **High:** Introduces new language (C# MSBuild, C++ CMake, Lua).<br>**Mod:** Minor language variant.<br>**Low:** Replicates existing language stack (Java). |
| **DIM-07** | **Dependency Maturity** | Are necessary tools available and actively maintained? | **High:** Active 2026 commits, automated tests.<br>**Mod:** Maintained, but slower release cycle.<br>**Low:** Archived / unmaintained $>2$ years.<br>**Unknown:** Maintenance status unverified. |
| **DIM-08** | **Version Stability** | How sensitive is the proposed integration to game updates? | **High:** Stable API; resilient across game patches.<br>**Mod:** Minor hook adjustments per game patch.<br>**Low:** Breaks on every minor game hash update. |
| **DIM-09** | **Security & Policy** | Can the integration remain strictly within applicable policies? | **High:** Fully offline, official mod policy sanctioned.<br>**Mod:** Unofficial modding, single-player only.<br>**Low:** Ambiguous EULA boundary.<br>**Blocker:** Policy violation. |
| **DIM-10** | **Testability** | Can critical behavior be observed and reproduced? | **High:** Headless execution or automated CLI testing.<br>**Mod:** Deterministic GUI / screen recording.<br>**Low:** Intermittent, timing-sensitive glitches. |
| **DIM-11** | **Reversibility** | Can the integration be disabled and removed safely? | **High:** Automated `cleanup.ps1` restores pristine state.<br>**Mod:** Manual profile deletion required.<br>**Low:** Modifies base game files destructively. |
| **DIM-12** | **Setup Complexity** | How difficult is environment setup and configuration? | **Low:** Automated setup script ($<5\text{ min}$).<br>**Mod:** Requires 2–3 manual tool installs ($<20\text{ min}$).<br>**High:** Complex compilation, manual registry edits ($>1\text{ hr}$). |
| **DIM-13** | **Maintenance Burden** | What recurring engineering work is expected? | **Low:** Set-and-forget; stable interface.<br>**Mod:** Periodic update sync with upstream mod loader.<br>**High:** Constant reverse-engineering of game offsets. |
| **DIM-14** | **Portfolio Redundancy** | Does it duplicate existing coverage? | **Low:** Zero overlap with existing candidates.<br>**Mod:** Minor overlap in secondary toolchain.<br>**High:** High overlap; identical to an active pilot. |
| **DIM-15** | **Development Uncertainty**| How many consequential questions remain unanswered? | **Low:** $\le 1$ minor documentation question.<br>**Mod:** 2–3 technical questions requiring small PoC.<br>**High:** Fundamental architecture questions open.<br>**Unknown:** Entire surface unresearched. |

---

## 4. Rating Scales & Rigorous Uncertainty Treatment

### 4.1 Strict Treatment of "Unknown"
In many evaluation frameworks, "Unknown" is mistakenly treated as neutral or low risk. **Under NexLudic Engine, an "Unknown" rating is treated as a high-risk uncertainty gate:**
- Any candidate with $>3$ "Unknown" ratings across Dimensions 1–5 cannot advance to the Pilot Shortlist.
- An "Unknown" in Dimension 9 (Security & Policy) or Dimension 2 (Capability Feasibility) functions as a temporary freeze until desk research or non-invasive inspection resolves the status.

### 4.2 Rating Scale Definitions
- **High (Favorable):** Documented, verified, resilient, safe, or provides major architectural value.
- **Moderate (Acceptable):** Proven with minor limitations, manageable maintenance, or semi-automated setup.
- **Low (Unfavorable):** Fragile, high maintenance, poorly documented, or redundant.
- **Unknown (Gated):** Absence of verifiable data. Requires targeted research before scoring.
- **Mandatory Blocker (Fatal):** Violation of security, safety, legal, or feasibility baselines. Immediate disqualification.

---

## 5. Candidate Prioritization & Scoring Methodology

To assist the Architecture Review Board in comparing non-blocked candidates, a structured scoring model is applied.

### 5.1 Scoring Formula
$$\text{Composite Score} = \sum_{i=1}^{15} (W_i \times S_i)$$
Where:
- $W_i$ is the weight of Dimension $i$ (normalized so $\sum W_i = 100$).
- $S_i$ is the score value: $\text{High} = 3$, $\text{Moderate} = 2$, $\text{Low} = 1$, $\text{Unknown} = 0$.
*(Note: Inverted scales apply to Complexity, Maintenance, Redundancy, and Uncertainty, where Low = 3 and High = 1).*

### 5.2 Dimension Weights Table

| Dimension ID | Dimension Name | Category | Weight ($W_i$) | Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **DIM-01** | Evidence Quality | Rigor | **10%** | Protects against unverified hallucinations. |
| **DIM-02** | Capability Feasibility | Core | **12%** | Ensures intended gameplay is achievable. |
| **DIM-03** | Interface Availability | Core | **10%** | Requires documented extension surfaces. |
| **DIM-04** | Integration Diversity | Portfolio | **10%** | Ensures coverage of Strategies A, B, and C. |
| **DIM-05** | Architectural Value | Core | **12%** | Tests universal platform assumptions. |
| **DIM-06** | Language Diversity | Portfolio | **8%** | Drives toolchain independence (C#, Java, C++, Lua).|
| **DIM-07** | Dependency Maturity | Ecosystem | **6%** | Avoids dead or archived mod tools. |
| **DIM-08** | Version Stability | Maintenance| **6%** | Reduces fragility against game patches. |
| **DIM-09** | Security & Policy | Safety | **8%** | Enforces offline single-player boundaries. |
| **DIM-10** | Testability | QA | **6%** | Ensures results can be logged and verified. |
| **DIM-11** | Reversibility | QA / Safety| **4%** | Enforces clean rollback via `cleanup.ps1`. |
| **DIM-12** | Setup Complexity | Operations | **2%** | Minor factor; engineering depth prioritized over ease. |
| **DIM-13** | Maintenance Burden | Operations | **2%** | Ongoing adapter support costs. |
| **DIM-14** | Portfolio Redundancy | Portfolio | **4%** | Penalizes duplication of existing pilots. |
| **DIM-15** | Development Uncertainty| Risk | **4%** | Penalizes open, unresearched questions. |
| **Total** | | | **100%** | |

### 5.3 Sensitivity Analysis & Score Limits
- **The score is an advisory ranking tool, not a decision substitute:** A high numerical score cannot promote a candidate that fails a Mandatory Blocker or leaves critical security questions unanswered.
- **Sensitivity Tolerance:** A candidate must lead its nearest archetype rival by at least $\ge 15\%$ composite score margin to be selected automatically. In the event of a tie ($<15\%$ margin), the candidate with the higher **Evidence Quality** and **Architectural Value** score is selected.

---

## 6. Selection Readiness Tiers & Governance

Every candidate evaluated against this standard receives an official disposition:

```
+----------------------------------------------------------------------------------------------------+
|                                    READINESS CLASSIFICATIONS                                       |
+---------------------+------------------------------------------------------------------------------+
| Classification      | Criteria & Definition                                                        |
+---------------------+------------------------------------------------------------------------------+
| **PILOT SHORTLIST** | Score $\ge 75\%$, all Blockers PASS, zero "Unknown" in D1–D5. Promoted to    |
|                     | Gate G3 for formal PoC specification authoring.                              |
+---------------------+------------------------------------------------------------------------------+
| **RESEARCH          | Score 50–74%, all Blockers PASS, minor unknowns in D6–D15. Suitable for      |
| QUALIFIED**         | Phase 2 portfolio consideration after targeted desk research.                |
+---------------------+------------------------------------------------------------------------------+
| **CONDITIONALLY     | Meets technical threshold, but blocked by local hardware, toolchain, or      |
| SUITABLE**          | license verification (e.g., requires $\ge 6\text{ GB}$ VRAM GPU test rig).   |
+---------------------+------------------------------------------------------------------------------+
| **DEFERRED**        | Technologically valid, but redundant with an existing higher-scoring pilot.  |
+---------------------+------------------------------------------------------------------------------+
| **REJECTED**        | Fails any Mandatory Blocker (Anti-cheat, DRM, drivers, admin, memory hack). |
+---------------------+------------------------------------------------------------------------------+
```

### Human Review & ARB Sign-off Requirement
Autonomous AI agents are strictly prohibited from finalizing pilot selection or promoting a candidate to execution status. All candidate scores and readiness classifications must be formally audited and signed off by the human Architecture Review Board.

---

*End of Pilot Selection Criteria (NLE-STD-SEL-001).*

