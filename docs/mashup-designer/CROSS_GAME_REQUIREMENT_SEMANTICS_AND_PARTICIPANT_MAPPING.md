# CROSS-GAME REQUIREMENT SEMANTICS AND PARTICIPANT MAPPING
**Document ID:** `NLE-SPC-CRS-001`  
**Version:** 1.0.0  
**Phase:** Phase 6 (Mashup Designer & Integration Planning)  
**Step:** Step 44 of 100  
**Status:** Approved Architectural Standard  
**Authors:** Game Interoperability Specialist, Systems Integration Architect, Domain Modeling Engineer, Senior QA Lead  
**Classification:** Core Interoperability Specification  

---

## 1. PURPOSE AND INTEROPERABILITY CHALLENGES

### 1.1 Purpose
This specification establishes the formal semantic rules, data transformation models, and directional mapping standards for **Cross-Game Requirements** within NexLudic Engine. 

Different games operate on radically divergent engine architectures, memory models, coordinate systems, tick rates, and physics engines. A universal mashup platform cannot assume that games share common internal structures. This document defines:
1. How requirements bind to source and target participants using Phase 4 canonical identities (`NLE-CAT-CGI-001`).
2. The distinction between conceptual semantic equivalence and low-level technical representation.
3. The source-to-target property transformation contracts (preserved, adapted, and dropped properties).
4. The architectural boundary separating requirement intent from adapter capability declarations.

---

## 2. PARTICIPANT-AWARE REQUIREMENT TOPOLOGIES

Every `FeatureRequirement` declares its cross-game relationship using explicit participant references:

```
+--------------------------------------------------------------------------------------------------------+
|                                    REQUIREMENT PARTICIPANT TOPOLOGIES                                  |
|                                                                                                        |
|  [ TOPOLOGY 1: INTRA-GAME ]       ---> Single Participant (Target Game internal modification)          |
|  [ TOPOLOGY 2: ASYMMETRIC BRIDGE ]---> Source Participant =====(Data Flow)=====> Target Participant     |
|  [ TOPOLOGY 3: SYMMETRIC PEER ]   ---> Participant A <=====(Bidirectional Sync)=====> Participant B   |
|  [ TOPOLOGY 4: ROLE-UNDECIDED ]   ---> Desired feature defined; Source/Target binding deferred         |
+--------------------------------------------------------------------------------------------------------+
```

### 2.1 Topology Classification Table

| Topology Type | Source Participant | Target Participant | Description & Example |
| :--- | :---: | :---: | :--- |
| **`INTRA_GAME`** | `null` | Required | Behavior confined to one game (e.g. "Render custom HUD health bar in Minecraft"). |
| **`ASYMMETRIC_BRIDGE`** | Required | Required | Directional state/asset flow (e.g. "Extract Doom 1993 Super Shotgun sprite into Minecraft item"). |
| **`SYMMETRIC_PEER`** | Required | Required | Real-time bidirectional event sync (e.g. "Simultaneous player movement in dual-game bridge"). |
| **`ROLE_UNDECIDED`** | Optional | Optional | Conceptual requirement authored before participant roles are finalized. |

---

## 3. SEMANTIC EQUIVALENCE VS. TECHNICAL REPRESENTATION

A fatal trap in cross-game modding is assuming that because two games use identical words, their underlying mechanics are compatible. The engine enforces semantic differentiation across three core concepts:

### 3.1 The "Item" Concept
- **Conceptual Meaning:** A portable object possessed by a player providing utility, weaponry, or resources.
- **Divergent Technical Representation:**
  - *Game A (Classic Shooter):* Fixed weapon array index (`weapon_slot_3`), binary ammo count integer, zero durability, zero inventory weight, static sprite.
  - *Game B (Survival Sandbox):* Compound NBT JSON structure, 64-stack limit, float durability wear, enchantment dictionary, 3D voxel mesh, grid inventory coordinates.
- **Rule:** A requirement cannot assume direct item transfer. It MUST declare property transformation mapping.

### 3.2 The "Player Transform" Concept
- **Conceptual Meaning:** The physical position and orientation of the player avatar in space.
- **Divergent Technical Representation:**
  - *Game A:* Left-Handed, Y-Up, fixed-point integer coordinates, 35Hz tick rate.
  - *Game B:* Right-Handed, Z-Up, double-precision float coordinates, 60Hz tick rate.
- **Rule:** Telemetry synchronization requirements MUST specify coordinate conversion and interpolation tolerances.

### 3.3 The Fallacy of Label Matching
- **Invariant:** Identical display labels (e.g. both games having a "Shotgun" or "Health Potion") MUST NEVER be treated by the engine as proof of compatibility. Equivalence requires verified Phase 5 capability contracts.

---

## 4. SOURCE-TO-TARGET PROPERTY TRANSFORMATION CONTRACTS

Where a requirement transfers data or mechanics across games, it MUST classify property disposition into three categories:

```
+--------------------------------------------------------------------------------------------------------+
|                                  PROPERTY TRANSFORMATION DISPOSITION                                   |
|                                                                                                        |
|  [ 1. PRESERVED PROPERTIES ] ---> Transferred verbatim (e.g. Raw weapon damage integer: 20 -> 20)      |
|  [ 2. ADAPTED PROPERTIES ]   ---> Converted via formula (e.g. Coordinate transform, tick interpolation)|
|  [ 3. DROPPED PROPERTIES ]   ---> Intentionally discarded (e.g. Source-specific sector lighting dropped)|
+--------------------------------------------------------------------------------------------------------+
```

### 4.1 Transformation Mapping Specification

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://schemas.nexludic.internal/mashup-designer/property-mapping.schema.json",
  "title": "CrossGamePropertyMapping",
  "type": "object",
  "required": ["source_concept", "target_concept", "preserved_properties", "adapted_properties"],
  "properties": {
    "source_concept": { "type": "string" },
    "target_concept": { "type": "string" },
    "preserved_properties": {
      "type": "array",
      "items": { "type": "string" }
    },
    "adapted_properties": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["property_name", "conversion_rule"],
        "properties": {
          "property_name": { "type": "string" },
          "conversion_rule": { "type": "string" }
        },
        "additionalProperties": false
      }
    },
    "dropped_properties": {
      "type": "array",
      "items": { "type": "string" }
    },
    "unresolved_mappings": {
      "type": "array",
      "items": { "type": "string" }
    }
  },
  "additionalProperties": false
}
```

---

## 5. REQUIREMENT SCOPE VS. ADAPTER CAPABILITY BOUNDARIES

The requirements analysis engine enforces strict separation between requirement demand and adapter capability supply:
1. **Requirements Describe Demand:** `FeatureRequirement` specifies what the player experience demands.
2. **Capabilities Describe Supply:** `CanonicalCapability` (Phase 5) declares what the adapter can deliver.
3. **Decoupled Lifecycle:** A requirement exists and remains valid in the project even if zero adapters currently exist to fulfill it. This preserves the creator's vision for future adapter releases or custom AI agent development.

---
*End of Specification — Document `NLE-SPC-CRS-001`.*
