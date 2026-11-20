# CROSS-GAME IDENTIFIER, ENUMERATION AND STATE VALUE MAPPING POLICY

**Document Identifier:** `NLE-POL-IES-001`  
**Lifecycle Stage:** Authoritative Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 74 of 100)  
**Security & Integrity Level:** Medium Integrity Level (IL) / Identity Governance  
**Upstream Dependencies:** `NLE-SPC-CSM-001` (Step 74), `NLE-POL-TRU-001` (Step 74), `NLE-CON-EDR-001` (Step 71)  
**Downstream Dependents:** `NLE-CON-MDP-001` (Step 74), `NLE-SPC-DMD-001` (Step 74)

---

## 1. Purpose and Scope

This policy governs the mapping, translation, and namespace isolation of discrete data types across disparate game engines:
1. **Entity and Actor Identifiers** (mapping proprietary local actor IDs to cross-game canonical UUIDs).
2. **Enumerations and Ordinals** (translating game-specific status codes, item classifications, and damage types).
3. **Bitmasks and Discrete State Flags** (reconciling packed integer bitfields with disparate bit layouts).

---

## 2. Cross-Game Entity Identity Resolution

### 2.1 The Two-Tier Identity Model
Every participating actor, vehicle, or entity in a bridge session possesses two distinct identities:
1. **Local Endpoint Identity:** Ephemeral, native to the host game (e.g., UE5 `AActor*` address, Unity `InstanceID` int32, Godot `RID`).
2. **Bridge Canonical Entity Identifier (CEID):** A version 4 or version 5 UUID assigned by the NexLudic Bridge Session Coordinator upon entity introduction:
   ```
   ceid:8f3c7e42-1234-4567-89ab-cdef01234567
   ```

### 2.2 Entity Mapping Registry
The engine maintains an in-memory, lock-free bidirectional translation table:
```
Local Endpoint ID (Endpoint A) <===> Canonical CEID <===> Local Endpoint ID (Endpoint B)
```
- When Endpoint A sends an event referencing `LocalActorID = 4022`, the semantic mapping layer resolves it to `CEID = 8f3c...`.
- When projecting to Endpoint B, `CEID = 8f3c...` is translated to Endpoint B's mapped local proxy ID (e.g., `UnityInstanceID = 98112`).
- If no mapping exists for Endpoint B, the entity proxy allocator generates an admission request before dispatching the payload.

---

## 3. Enumeration Mapping Protocols

### 3.1 Mapping Topologies

```
1-to-1 Mapping (Lossless):
  UE5: DAMAGE_FIRE (1)  =======>  CIR: DMG_FIRE  =======>  Unity: ElementalDamage.Fire (3)

N-to-1 Mapping (Lossy Projection / Generalization):
  UE5: DMG_FIRE_LAVA (12) --\
  UE5: DMG_FIRE_PLASMA(13) --->  CIR: DMG_FIRE  =======>  Unity: ElementalDamage.Fire (3)
  UE5: DMG_FIRE_FLAME (14) --/

1-to-N Mapping (Specialization with Context / Disambiguation):
  Unity: DamageType.Physical ===> CIR: DMG_PHYSICAL ===> UE5: DMG_PHYS_BLUNT (Default)
                                                     ===> UE5: DMG_PHYS_SLASH (Requires Context)
```

### 3.2 Formal Enum Mapping Table Schema
```json
{
  "enum_mapping_id": "map-enum:damage_types:ue5_to_unity",
  "source_enum_uri": "nexludic://enums/ue5/damage_type",
  "target_enum_uri": "nexludic://enums/unity/elemental_damage",
  "default_fallback": "ElementalDamage.Neutral",
  "unknown_handling_strategy": "FALLBACK_AND_LOG",
  "mappings": [
    { "source_val": "EDamageType::Fire", "target_val": "ElementalDamage.Fire", "lossy": false },
    { "source_val": "EDamageType::Cold", "target_val": "ElementalDamage.Ice", "lossy": false },
    { "source_val": "EDamageType::Frostbite", "target_val": "ElementalDamage.Ice", "lossy": true },
    { "source_val": "EDamageType::Electric", "target_val": "ElementalDamage.Lightning", "lossy": false }
  ]
}
```

### 3.3 Unknown Enum Value Handling Strategies
When a source emits an enum ordinal not declared in the mapping table:
1. `FALLBACK_AND_LOG` (Default for gameplay): Replaces value with declared `default_fallback` and logs warning.
2. `FAIL_CLOSED` (Default for transactions/inventory): Drops payload and emits `ERR_SEM_ENUM_UNMAPPED`.
3. `PASSTHROUGH_TAG`: Retains numerical ordinal in unmapped property container.

---

## 4. Bitmask and State Flag Normalization

Different engines pack boolean states into arbitrary bitmasks:
- Game A: Bit 0 = Crouching, Bit 1 = Sprinting, Bit 2 = AimingDownSights
- Game B: Bit 0 = InAir, Bit 1 = Crouching, Bit 2 = InCover, Bit 3 = Sprinting

**Inviolable Rule:** Bitmasks **shall never** be mapped via raw integer bit shifting. 
Every bitflag must be unpacked into canonical discrete boolean attributes (`is_crouching`, `is_sprinting`) in the Canonical Intermediate Representation, and repacked into the target bitmask according to target field definitions.
