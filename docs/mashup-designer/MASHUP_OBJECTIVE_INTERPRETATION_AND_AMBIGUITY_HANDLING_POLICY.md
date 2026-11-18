# MASHUP OBJECTIVE INTERPRETATION AND AMBIGUITY HANDLING POLICY
**Document ID:** `NLE-POL-MOI-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Systems Analyst, Product Systems Designer, Technical Product Designer, Principal Software Architect  
**Classification:** Operational Policy & Analysis Specification  

---

## 1. PURPOSE AND POLICY SCOPE

### 1.1 Purpose
This policy establishes the formal rules, boundaries, and validation protocols for **Interpreting Natural-Language Mashup Objectives** and **Resolving Ambiguities** within NexLudic Engine. 

Creators describe their dream cross-game mashup in everyday conversational language. However, automated systems and AI agents face severe failure modes when converting casual descriptions into technical plans: hallucinating compatibility, silently discarding unfulfillable details, or converting technical guesses into mandatory requirements. This policy guarantees that:
1. The creator's exact words are preserved verbatim and remain the authoritative touchstone of intent.
2. Explicitly stated facts are strictly separated from technical inferences and speculative suggestions.
3. Vague or unbounded descriptions trigger structured, non-blocking clarification workflows rather than silent assumptions.
4. No system-generated proposal can become a mandatory requirement without explicit creator approval.

---

## 2. PRESERVATION OF ORIGINAL USER INTENT

### 2.1 The Verbatim Retention Mandate
1. **Immutable Thesis Anchor:** The `core_thesis` and `intended_experience` text fields authored by the creator within `MashupObjective` are immutable historical records. They MUST NEVER be edited, truncated, rephrased, or overwritten by automated interpretation engines.
2. **Intent Excerpt Traceability:** Every `Feature` and `FeatureRequirement` generated through decomposition MUST record a `user_intent_excerpt` pointing directly to the exact sentence or clause in the user's objective that motivated it.

---

## 3. SEPARATION OF EXTRACTION, INFERENCE, AND SPECULATION

To maintain truthfulness throughout requirements analysis, information extracted from user input is strictly classified into four epistemic tiers:

```
+--------------------------------------------------------------------------------------------------------+
|                                    INFORMATION PROVENANCE TIERS                                        |
|                                                                                                        |
|  [ TIER 1: USER_EXPLICIT ]            ---> Explicitly stated facts ("Spawn a Doom Cacodemon")         |
|  [ TIER 2: USER_CONFIRMED_INFERENCE ] ---> Inferred mechanics confirmed by user ("It shoots fireballs")|
|  [ TIER 3: SYSTEM_PROPOSED ]          ---> System suggestions awaiting review ("Add floating SFX")    |
|  [ TIER 4: TECHNICAL_ASSUMPTION ]     ---> Technical prerequisites ("Minecraft supports custom AI")    |
+--------------------------------------------------------------------------------------------------------+
```

### 3.1 Tier Definitions & Verification Rules

| Tier Identifier | Definition & Criteria | Confirmation Requirement | Impact on Planning |
| :--- | :--- | :---: | :--- |
| **`USER_EXPLICIT`** | Directly stated by user in objective or requirement form. | Pre-approved by user | Formally included in project scope. |
| **`USER_CONFIRMED_INFERENCE`** | Logical deduction presented to and approved by creator. | User click / prompt | Promoted to formal requirement. |
| **`SYSTEM_PROPOSED`** | AI/System suggestion not yet reviewed by creator. | **Pending Approval** | Tagged `DRAFT`; excluded from baseline plan. |
| **`TECHNICAL_ASSUMPTION`** | Presumed platform or adapter technical capability. | Authoritative Probe | Captured as `AssumptionRecord`; remains unverified. |

---

## 4. AMBIGUITY TAXONOMY & RESOLUTION PROTOCOLS

Creators frequently use vague, open-ended phrases. The requirements analysis engine categorizes ambiguities and applies deterministic resolution protocols:

| Vague Expression | Ambiguity Category | Failure Risk if Unchecked | Engine Resolution Protocol |
| :--- | :--- | :--- | :--- |
| *"Make the games work together"* | **Global Scope Ambiguity** | Infinite scope creep; impossible planning | Emit `OpenQuestionRecord`: Prompt user to pick primary host and guest games. |
| *"Transfer everything"* | **Unbounded Asset Ambiguity** | Terabyte asset extraction; memory crash | Emit `OpenQuestionRecord`: Present asset checklist (items, mobs, sounds, textures). |
| *"Keep behavior identical"* | **Equivalence Ambiguity** | Disjoint physics/AI simulation engines | Document `AssumptionRecord`: Highlight physics engine divergence and trade-offs. |
| *"Seamless integration"* | **Performance/Latency Ambiguity** | Unachievable 0ms cross-process latency | Establish measurable `AcceptanceCriterion` (e.g. latency $\le 16.6\text{ ms}$). |
| *"Support all items"* | **Inventory Mapping Ambiguity** | Target game lacks item attribute slots | Emit `OpenQuestionRecord`: Suggest core subset of iconic items first. |

---

## 5. USER APPROVAL WORKFLOW FOR SYSTEM SUGGESTIONS

When automated decomposition tools or AI assistant routines analyze an objective:
1. **Suggestions Tagged `SYSTEM_PROPOSED`:** All decomposed features start in the `SYSTEM_PROPOSED` state.
2. **Review Dialog:** The UI displays a "Review Proposed Decomposition" tray, highlighting suggested features, detected assumptions, and open questions.
3. **Explicit User Promotion:**
   - User clicks **"Accept Feature"** $\rightarrow$ Origin updates to `USER_CONFIRMED_INFERENCE`.
   - User clicks **"Reject Feature"** $\rightarrow$ Feature is deleted or archived with rationale.
   - User clicks **"Modify"** $\rightarrow$ Creator edits text and approves as `USER_EXPLICIT`.
4. **Prohibition of Silent Promotion:** The engine strictly forbids any automated routine from promoting a suggestion to `MANDATORY` without explicit creator click confirmation.

---
*End of Policy — Document `NLE-POL-MOI-001`.*
