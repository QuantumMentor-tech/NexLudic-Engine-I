# STEP 70 OPEN ARCHITECTURE DECISIONS REGISTER

**Document ID:** `NLE-REG-STP-070`  
**Classification:** Architectural Decision Records (ADRs)  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 70)  
**Status:** Authoritative Foundation  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `DEC-002`, `BLK-SEC-01`, `P8-C01`, `P8-C02`, `P8-C03`  
**Downstream Consumed By:** Phase 9 Steps 71–78  

---

## 1. Overview

This register formally documents the ten fundamental architectural decisions (`ADR-BRG-001` through `ADR-BRG-010`) established during Step 70 for the NexLudic Engine Live Game Interoperability Bridge and Hybrid Integration Foundation.

---

## 2. Architectural Decision Records (`ADR-BRG-001` to `ADR-BRG-010`)

### `ADR-BRG-001`: Central Hub-and-Spoke Gateway vs Direct Peer-to-Peer Inter-Game IPC
- **Context:** Connecting Game A to Game B could theoretically be done directly via a shared socket between Game A's mod and Game B's mod, or centrally through NexLudic Engine.
- **Decision:** Mandate a **Central Hub-and-Spoke Topology** routed through the Central Bridge Coordinator. Direct peer-to-peer IPC between games is forbidden.
- **Rationale:** Direct P2P prevents centralized schema translation, eliminates security policy enforcement, makes multi-game mashups ($N > 2$) intractable ($O(N^2)$ connections), and prevents unified telemetry logging.
- **Consequences:** Bridge engine introduces a negligible routing hop (< 0.5ms on local loopback) but guarantees 100% authorization, schema enforcement, and flow control.

### `ADR-BRG-002`: Length-Prefixed Framing vs Delimiter/Chunked Framing
- **Context:** Stream-oriented IPC transports (TCP, byte-mode pipes) do not delineate message boundaries.
- **Decision:** Standardize on **Length-Prefixed Framing** with a 4-byte magic signature (`0x4E4C4542`) followed by a 4-byte big-endian payload length.
- **Rationale:** Delimiter framing (e.g. newline `\n` or `0x00`) requires byte-by-byte scanning and payload escaping, introducing severe CPU overhead for large binary payloads. Length prefixing allows immediate single-pass buffer allocation and DMA reading.
- **Consequences:** Maximum frame size is explicitly bounded (default 1 MB; hard ceiling 16 MB).

### `ADR-BRG-003`: Decoupled Hybrid Integration Lifecycle (Phase 8 vs Phase 9)
- **Context:** Hybrid mashups involve both installed mod files (Phase 8) and runtime live bridging (Phase 9).
- **Decision:** Enforce **Strict Subsystem Decoupling**. A failure or disconnection in a live bridge session **MUST NEVER** trigger an automated filesystem rollback of Phase 8 packages.
- **Rationale:** Live bridge sockets disconnect frequently due to player alt-tabs, level transitions, or game crashes. Triggering a destructive filesystem rollback while a game process is running risks severe save corruption and unrecoverable host state.
- **Consequences:** Bridge recovery is limited to transport reconnects or session teardowns; filesystem modifications remain persistent and verified.

### `ADR-BRG-004`: Non-Invasive OS IPC vs Process Memory Hooking (`BLK-SEC-01`)
- **Context:** Some traditional game modding tools use memory hooking (`VirtualProtect`, detours, byte patching) or DLL injection.
- **Decision:** Categorically prohibit memory hooking, DLL injection, and `ReadProcessMemory`/`WriteProcessMemory`. Live bridging must use only standard OS IPC channels or developer-sanctioned mod APIs.
- **Rationale:** Memory hooking triggers immediate anti-cheat detection (resulting in permanent bans for users), causes host game instability upon engine updates, and violates user security trust.
- **Consequences:** Requires game-specific bridge connectors to run as official mods, companion processes, or scripts within the game's supported mod loader.

### `ADR-BRG-005`: Multi-Phase Capability Negotiation Handshake
- **Context:** Endpoints must determine what features, topics, and schemas they mutually support.
- **Decision:** Require a formal **Multi-Phase Handshake** (`HANDSHAKE_INIT` -> `HANDSHAKE_ACK` -> `CAPABILITY_OFFER` -> `CAPABILITY_ACCEPT`). Zero game data may be dispatched prior to `CAPABILITY_ACCEPT`.
- **Rationale:** Eliminates runtime deserialization crashes caused by mismatched schema versions or incompatible tick rates.
- **Consequences:** Slight connection setup latency (~10ms) before the bridge activates, which is negligible compared to game load times.

### `ADR-BRG-006`: Windows Medium Integrity Level Adherence (`DEC-002`)
- **Context:** Some desktop utilities require administrator privileges or UAC elevation.
- **Decision:** Enforce that NexLudic Engine and all bridge threads run strictly at **Windows Medium Integrity Level**.
- **Rationale:** Desktop gaming platforms should never operate with administrator rights. Running at Medium IL protects the host OS from compromised mod scripts and complies with enterprise and consumer security best practices.
- **Consequences:** Bridge cannot install kernel drivers or modify system-protected directories; all bridge communication must use user-scoped IPC.

### `ADR-BRG-007`: Token-Bucket Rate Limiting & Backpressure High-Water Mark
- **Context:** A fast game simulation (e.g. 144Hz physics) can easily saturate a slower game or flood engine memory.
- **Decision:** Implement **Per-Topic Token Bucket Rate Limiting** combined with transport socket read pausing at a **75% High-Water Mark**.
- **Rationale:** Prevents buffer bloat, eliminates memory exhaustion crashes, and protects slow consumer runtimes from desynchronization.
- **Consequences:** Producers emitting telemetry faster than negotiated limits will experience rate-limit frame drops or socket backpressure.

### `ADR-BRG-008`: Canonical Coordinate Standardization (Right-Handed, Y-Up, SI Meters)
- **Context:** Game engines use divergent 3D coordinate conventions (Unreal: Left-Handed Z-Up; Unity: Left-Handed Y-Up; Source: Right-Handed Z-Up).
- **Decision:** Standardize all live spatial telemetry on **Canonical Right-Handed, Y-Up, SI Meters**.
- **Rationale:** Avoids $O(N^2)$ direct coordinate conversion matrices. Each game connector converts only between its local coordinate space and the canonical space ($2N$ converters).
- **Consequences:** Connectors must apply 4x4 affine matrix transformations on outbound and inbound spatial vectors.

### `ADR-BRG-009`: Ephemeral Handshake Token Authentication for Loopback TCP
- **Context:** Loopback TCP sockets (`127.0.0.1`) can theoretically be connected to by any process running on the same machine.
- **Decision:** Require a **256-Bit Ephemeral Cryptographic Token** generated per session and passed to the game connector via secure local bootstrap arguments or a temporary token file.
- **Rationale:** Prevents local unprivileged processes from hijacking or eavesdropping on live bridge communications.
- **Consequences:** Connections failing to supply the valid token within 500ms are dropped immediately.

### `ADR-BRG-010`: Design-Only Specification Milestone (`P8-C02`)
- **Context:** Step 70 establishes the architectural foundation for Phase 9.
- **Decision:** Restrict Step 70 strictly to engineering specifications, domain models, contracts, and synthetic QA test designs. **Zero runtime code or live process hooks are implemented.**
- **Rationale:** Strict adherence to project roadmap governance. Prevents premature, untested implementation before foundational contracts and security boundaries are fully approved.
- **Consequences:** Runtime bridge implementation begins in subsequent Phase 9 steps.
