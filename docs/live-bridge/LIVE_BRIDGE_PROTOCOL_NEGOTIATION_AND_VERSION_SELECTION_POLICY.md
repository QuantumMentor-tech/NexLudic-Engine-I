# LIVE BRIDGE PROTOCOL NEGOTIATION & VERSION SELECTION POLICY

**Document ID:** `NLE-POL-PNV-001`  
**Classification:** Protocol Governance & Version Negotiation Policy  
**Phase:** Phase 9 — Live Game Bridge & Hybrid Integration (Step 72 of 100)  
**Status:** Authoritative Specification  
**Upstream Dependencies:** `NLE-SPC-LBA-001`, `NLE-SPC-PME-001`, `NLE-POL-SLC-001`, `NLE-CON-PCM-001`  
**Downstream Consumed By:** `NLE-CON-CAS-001`, Steps 73–78  

---

## 1. Scope & Protocol Negotiation Objectives

Once peers are authenticated, they must establish an exact, mutually supported wire protocol contract and reconcile all mandatory message features and optional extensions before game data can be transmitted.

This policy defines the deterministic protocol version selection algorithm, feature resolution rules, and incompatibility handling policies.

---

## 2. Wire Protocol Family & Version Taxonomy

NexLudic Engine live bridging standardizes on the **`NLE-WIRE`** protocol family.

```
Supported Wire Protocol Versions:
- NLE-WIRE-v1 (Authoritative Phase 9 wire protocol)
  * Binary length-prefixed framing (0x4E4C4542 magic bytes)
  * Canonical JSON envelope encoding
  * SHA-256 payload digest verification
  * Monotonic 32-bit sequence counters
  * Millisecond-precision UTC timestamps
```

### 2.1 Version Compatibility Matrix

| Requested Version | Endpoint A Supported | Endpoint B Supported | Reconciled Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| `NLE-WIRE-v1` | `["NLE-WIRE-v1"]` | `["NLE-WIRE-v1"]` | `NLE-WIRE-v1` | **SUCCESS** |
| `NLE-WIRE-v1` | `["NLE-WIRE-v1"]` | `["NLE-WIRE-v2"]` | None | **REJECTED (No Overlap)** |
| `NLE-WIRE-v1` | `["NLE-WIRE-v1", "v2"]` | `["NLE-WIRE-v1"]` | `NLE-WIRE-v1` | **SUCCESS (Common Version)** |
| `NLE-WIRE-v2` | `["NLE-WIRE-v1"]` | `["NLE-WIRE-v1"]` | None | **REJECTED (Unsupported Requested)** |

---

## 3. Protocol Feature Negotiation (Mandatory vs Optional)

Protocols define discrete feature flags that govern wire behavior:

```
+-----------------------------------+---------------------------------------+-------------+
| Feature Identifier                | Description                           | Default     |
+-----------------------------------+---------------------------------------+-------------+
| `feat.wire.framing.length_prefix` | Mandatory 4-byte length prefix.       | MANDATORY   |
| `feat.wire.digest.sha256`         | Mandatory SHA-256 payload integrity.  | MANDATORY   |
| `feat.wire.compression.lz4`       | Optional LZ4 frame compression.       | OPTIONAL    |
| `feat.wire.batching`              | Optional multi-message frame batching.| OPTIONAL    |
| `feat.wire.binary_json`           | Optional CBOR/MessagePack payload.    | OPTIONAL    |
+-----------------------------------+---------------------------------------+-------------+
```

### 3.1 Feature Reconciliation Algorithm
1. **Mandatory Features:** If either endpoint fails to declare support for a mandatory feature (e.g. `feat.wire.digest.sha256`), negotiation fails immediately with `ERR_PROTO_MANDATORY_FEATURE_MISSING`.
2. **Optional Features:** Optional features are enabled **ONLY** if both Endpoint A and Endpoint B explicitly offer them. If one endpoint lacks support, the feature is disabled for the session without error.
3. **Deterministic Selection:** Feature selection is commutative: $\text{Reconcile}(A, B) \equiv \text{Reconcile}(B, A)$.

---

## 4. `CAPABILITY_OFFER` & `CAPABILITY_ACCEPT` Handshake Exchange

Protocol and capability negotiation are conducted via normalized control envelopes:

### 4.1 `CAPABILITY_OFFER` Frame (Peer to Coordinator)
```json
{
  "$schema": "https://schemas.nexludic.dev/v1/bridge/capability-offer.json",
  "version": "1.0",
  "message_type": "CAPABILITY_OFFER",
  "session_ref": "creq-01938a4b-8800-7000-8801-abcdef123456",
  "supported_wire_protocols": ["NLE-WIRE-v1"],
  "supported_features": [
    "feat.wire.framing.length_prefix",
    "feat.wire.digest.sha256",
    "feat.wire.compression.lz4"
  ],
  "offered_capabilities": [
    {
      "capability_id": "telemetry.player.spatial",
      "version": "1.2.0",
      "direction": "OUTBOUND",
      "max_rate_per_sec": 60
    },
    {
      "capability_id": "inventory.item.transfer",
      "version": "1.0.0",
      "direction": "BIDIRECTIONAL",
      "max_rate_per_sec": 10
    }
  ]
}
```

### 4.2 Rejection on Protocol Incompatibility
If negotiation fails:
1. Coordinator emits `CAPABILITY_REJECT` frame with specific error code (`ERR_PROTO_NO_COMMON_VERSION` or `ERR_PROTO_FEATURE_CONFLICT`).
2. Coordinator logs diagnostic failure in `TDJ-08`.
3. Transport channel is closed within 100ms.
