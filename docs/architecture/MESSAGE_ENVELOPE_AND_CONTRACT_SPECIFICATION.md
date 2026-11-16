# NEXLUDIC ENGINE ARCHITECTURE SPECIFICATION
## Message Envelope & Contract Specification

**Document ID:** `NLE-SPC-ENV-001`  
**Phase:** 2 — Architecture & Development Foundation  
**Step:** 13 of 100  
**Status:** Approved Architecture Baseline  
**Classification:** Technical Contract Specification  
**Traceability:** `NLE-SPC-MSG-001` (Universal Message Protocol), `ADR-0003` (Dual-Channel IPC), `NLE-ARC-IPC-001` (IPC Architecture)

---

## 1. Executive Summary

This document defines the formal schema, mandatory and optional fields, validation constraints, and serialization representations for message envelopes and payload contracts across both the **Control Channel** (JSON / `ndjson`) and the **Telemetry Data Channel** (Protocol Buffers `proto3`).

Every message exchanged across the NexLudic Engine IPC boundaries MUST strictly conform to the specifications and contracts documented herein.

---

## 2. Universal Message Envelope Schema (JSON Schema Definition)

The formal JSON Schema definition for the Control Channel envelope is defined as follows:

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://nexludic.engine/schemas/v1/message_envelope.json",
  "title": "UniversalMessageEnvelope",
  "type": "object",
  "required": [
    "protocol_version",
    "message_id",
    "message_type",
    "timestamp_utc",
    "source_endpoint",
    "target_endpoint",
    "channel_type",
    "sequence_number"
  ],
  "properties": {
    "protocol_version": {
      "type": "string",
      "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$"
    },
    "message_id": {
      "type": "string",
      "format": "uuid"
    },
    "correlation_id": {
      "type": ["string", "null"],
      "format": "uuid"
    },
    "message_type": {
      "type": "string",
      "pattern": "^[a-z]+(\\.[a-z_]+)+$"
    },
    "timestamp_utc": {
      "type": "string",
      "format": "date-time"
    },
    "source_endpoint": {
      "type": "string",
      "minLength": 3,
      "maxLength": 64
    },
    "target_endpoint": {
      "type": "string",
      "minLength": 3,
      "maxLength": 64
    },
    "channel_type": {
      "type": "string",
      "enum": ["CONTROL", "DATA", "DIAGNOSTIC"]
    },
    "sequence_number": {
      "type": "integer",
      "minimum": 0
    },
    "timeout_ms": {
      "type": ["integer", "null"],
      "minimum": 100,
      "maximum": 86400000
    },
    "deadline_utc": {
      "type": ["string", "null"],
      "format": "date-time"
    },
    "trace_id": {
      "type": ["string", "null"],
      "maxLength": 128
    },
    "payload": {
      "type": ["object", "null"],
      "description": "Polymorphic payload object specific to message_type"
    },
    "error": {
      "$ref": "#/$defs/ErrorPayload"
    }
  },
  "additionalProperties": false,
  "$defs": {
    "ErrorPayload": {
      "type": ["object", "null"],
      "required": ["code", "category", "message", "retryable"],
      "properties": {
        "code": { "type": "string" },
        "category": { "type": "string" },
        "message": { "type": "string" },
        "details": { "type": "string" },
        "retryable": { "type": "boolean" },
        "trace_id": { "type": ["string", "null"] }
      },
      "additionalProperties": false
    }
  }
}
```

---

## 3. Concrete JSON Payload Contracts (Control Channel)

### 3.1 Session Handshake Request (`session.handshake.request`)

Dispatched by the Core Supervisor immediately upon launching the worker process:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "a5d8b742-1e94-4d2a-89cf-47d0f9a2e101",
  "correlation_id": null,
  "message_type": "session.handshake.request",
  "timestamp_utc": "2026-11-16T12:00:00.0000000Z",
  "source_endpoint": "core.supervisor",
  "target_endpoint": "adapter.worker_01",
  "channel_type": "CONTROL",
  "sequence_number": 1,
  "timeout_ms": 10000,
  "deadline_utc": "2026-11-16T12:00:10.0000000Z",
  "trace_id": "trace-4bf92f3577b34da6a3ce929d0e0e4736",
  "payload": {
    "assigned_worker_id": "adapter.worker_01",
    "supported_protocol_versions": ["1.0.0"],
    "session_auth_token": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    "telemetry_pipe_path": "\\\\.\\pipe\\nexludic_telemetry_sess_01_worker_01",
    "log_level": "DEBUG",
    "environment": {
      "workspace_root": "D:\\NexLudic\\Projects\\MashupAlpha",
      "staging_root": "D:\\NexLudic\\Projects\\MashupAlpha\\staging",
      "target_game_id": "skyrim_se",
      "target_game_install_path": "C:\\Games\\Skyrim Special Edition"
    }
  },
  "error": null
}
```

### 3.2 Session Handshake Response (`session.handshake.response`)

Returned by the adapter worker confirming successful initialization:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "c1f72e39-5094-482a-a9e4-bc92d1f4a902",
  "correlation_id": "a5d8b742-1e94-4d2a-89cf-47d0f9a2e101",
  "message_type": "session.handshake.response",
  "timestamp_utc": "2026-11-16T12:00:00.4120000Z",
  "source_endpoint": "adapter.worker_01",
  "target_endpoint": "core.supervisor",
  "channel_type": "CONTROL",
  "sequence_number": 1,
  "timeout_ms": null,
  "deadline_utc": null,
  "trace_id": "trace-4bf92f3577b34da6a3ce929d0e0e4736",
  "payload": {
    "status": "READY",
    "selected_protocol_version": "1.0.0",
    "adapter_identity": {
      "adapter_id": "skyrim_creationkit_adapter",
      "version": "1.2.0",
      "author": "NexLudic Core Team",
      "runtime_engine": "DotNetCore_8_0",
      "process_id": 14920
    },
    "telemetry_pipe_connected": true,
    "diagnostics": {
      "initialization_duration_ms": 412,
      "clr_version": "8.0.2"
    }
  },
  "error": null
}
```

### 3.3 Capability Manifest Response (`capability.manifest.response`)

Returned by the adapter detailing its supported integration traits and operation contracts:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "77e382b0-8802-4f81-8b29-19ec59b3c401",
  "correlation_id": "31b99014-99a3-4817-bfd2-08a9462d7c03",
  "message_type": "capability.manifest.response",
  "timestamp_utc": "2026-11-16T12:00:00.6000000Z",
  "source_endpoint": "adapter.worker_01",
  "target_endpoint": "core.supervisor",
  "channel_type": "CONTROL",
  "sequence_number": 2,
  "timeout_ms": null,
  "deadline_utc": null,
  "trace_id": "trace-4bf92f3577b34da6a3ce929d0e0e4736",
  "payload": {
    "supported_traits": [
      "IModScaffoldableTrait",
      "ITelemetrySourceTrait"
    ],
    "operations": [
      {
        "name": "scaffold_mod_workspace",
        "description": "Scaffolds an isolated mod development workspace with Papyrus build configurations",
        "timeout_default_ms": 30000,
        "is_idempotent": true,
        "input_schema_version": "1.0.0",
        "output_schema_version": "1.0.0"
      },
      {
        "name": "compile_papyrus_scripts",
        "description": "Compiles raw .psc Papyrus source files into .pex binaries inside staging jail",
        "timeout_default_ms": 60000,
        "is_idempotent": true,
        "input_schema_version": "1.0.0",
        "output_schema_version": "1.0.0"
      }
    ],
    "telemetry_streams": [
      {
        "stream_name": "player_transform",
        "format": "protobuf",
        "nominal_rate_hz": 60,
        "schema_id": "proto.nexludic.telemetry.PlayerTransform"
      }
    ]
  },
  "error": null
}
```

### 3.4 Operation Invocation: Scaffold Mod Workspace (`operation.invoke.request`)

Core instructs the adapter to generate an isolated mod staging environment:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "5f60b8b2-32b1-4f93-b672-9a00de93c801",
  "correlation_id": null,
  "message_type": "operation.invoke.request",
  "timestamp_utc": "2026-11-16T12:00:01.0000000Z",
  "source_endpoint": "core.supervisor",
  "target_endpoint": "adapter.worker_01",
  "channel_type": "CONTROL",
  "sequence_number": 3,
  "timeout_ms": 30000,
  "deadline_utc": "2026-11-16T12:00:31.0000000Z",
  "trace_id": "trace-91823749817293847192384719283741",
  "payload": {
    "operation_id": "op-scaffold-001",
    "action": "scaffold_mod_workspace",
    "parameters": {
      "mod_name": "MashupBridge_Skyrim_Subnautica",
      "target_staging_directory": "D:\\NexLudic\\Projects\\MashupAlpha\\staging\\skyrim_mod",
      "template_id": "papyrus_event_bridge_v1",
      "configuration": {
        "author": "NexLudic AI",
        "version": "0.1.0",
        "enable_debug_events": true
      }
    }
  },
  "error": null
}
```

### 3.5 Operation Progress Event (`operation.invoke.progress`)

Emitted asynchronously by worker during long execution:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "31b40291-a832-47de-98a1-c27b08e2f111",
  "correlation_id": "5f60b8b2-32b1-4f93-b672-9a00de93c801",
  "message_type": "operation.invoke.progress",
  "timestamp_utc": "2026-11-16T12:00:02.5000000Z",
  "source_endpoint": "adapter.worker_01",
  "target_endpoint": "core.supervisor",
  "channel_type": "CONTROL",
  "sequence_number": 3,
  "timeout_ms": null,
  "deadline_utc": null,
  "trace_id": "trace-91823749817293847192384719283741",
  "payload": {
    "operation_id": "op-scaffold-001",
    "progress_percent": 65.0,
    "current_stage": "Extracting Papyrus script templates",
    "items_completed": 13,
    "items_total": 20,
    "status_message": "Unpacking Papyrus source stubs into staging directory"
  },
  "error": null
}
```

### 3.6 Operation Completion (`operation.invoke.completed`)

Final response delivering generated artifacts and execution statistics:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "99e12084-2101-44af-8802-bd928019a552",
  "correlation_id": "5f60b8b2-32b1-4f93-b672-9a00de93c801",
  "message_type": "operation.invoke.completed",
  "timestamp_utc": "2026-11-16T12:00:03.2000000Z",
  "source_endpoint": "adapter.worker_01",
  "target_endpoint": "core.supervisor",
  "channel_type": "CONTROL",
  "sequence_number": 4,
  "timeout_ms": null,
  "deadline_utc": null,
  "trace_id": "trace-91823749817293847192384719283741",
  "payload": {
    "operation_id": "op-scaffold-001",
    "status": "SUCCESS",
    "duration_ms": 2200,
    "artifacts_created": [
      {
        "relative_path": "scripts/Source/NexLudicBridge.psc",
        "file_size_bytes": 4120,
        "sha256": "4b227777d4dd1fc61c6f884f48641d02b4d121d3fd328cb08b5531fcacdabf8a"
      },
      {
        "relative_path": "mod_manifest.json",
        "file_size_bytes": 850,
        "sha256": "ef2d127de37b942baad06145e54b0c619a1f22327b2ebbcfbec78f5564afe39d"
      }
    ],
    "execution_metrics": {
      "peak_memory_bytes": 48234496,
      "cpu_time_ms": 312
    }
  },
  "error": null
}
```

### 3.7 Standardized Error Response Example

Returned when an operation fails validation, preconditions, or runtime execution:

```json
{
  "protocol_version": "1.0.0",
  "message_id": "8e920144-8802-45e1-8891-b92819208a01",
  "correlation_id": "5f60b8b2-32b1-4f93-b672-9a00de93c801",
  "message_type": "operation.invoke.completed",
  "timestamp_utc": "2026-11-16T12:00:03.5000000Z",
  "source_endpoint": "adapter.worker_01",
  "target_endpoint": "core.supervisor",
  "channel_type": "CONTROL",
  "sequence_number": 5,
  "timeout_ms": null,
  "deadline_utc": null,
  "trace_id": "trace-91823749817293847192384719283741",
  "payload": {
    "operation_id": "op-scaffold-001",
    "status": "FAILED"
  },
  "error": {
    "code": "STAGING_PATH_OUTSIDE_JAIL",
    "category": "SECURITY_SANDBOX_VIOLATION",
    "message": "The requested target staging directory escapes the configured workspace boundary.",
    "details": "Target path 'C:\\Windows\\System32\\mod' does not resolve within jail 'D:\\NexLudic\\Projects\\MashupAlpha\\staging'.",
    "retryable": false,
    "trace_id": "trace-91823749817293847192384719283741"
  }
}
```

---

## 4. Telemetry Data Channel Contract (Protobuf Specification)

For high-throughput, low-latency state synchronization across Windows Named Pipes, messages are defined using **Protocol Buffers v3 (`proto3`)**:

```protobuf
syntax = "proto3";

package nexludic.ipc.v1;

option csharp_namespace = "NexLudic.Core.Ipc.Protobuf";

// High-speed binary telemetry frame wrapper
message TelemetryFrame {
  // Monotonically increasing frame sequence
  uint64 sequence_number = 1;
  
  // High-precision nanosecond timestamp (Unix epoch)
  int64 timestamp_ns = 2;
  
  // Game session source identifier
  string source_game_id = 3;
  
  // Frame payload discriminator
  oneof payload {
    SpatialTransformTelemetry spatial_transform = 10;
    PlayerStateTelemetry player_state = 11;
    InputStateTelemetry input_state = 12;
    GenericMetricBatch generic_metrics = 13;
  }
}

// 3D Spatial coordinates and orientation
message SpatialTransformTelemetry {
  double position_x = 1;
  double position_y = 2;
  double position_z = 3;
  
  // Quaternion rotation
  float rotation_x = 4;
  float rotation_y = 5;
  float rotation_z = 6;
  float rotation_w = 7;
  
  // Linear velocity vector (m/s)
  float velocity_x = 8;
  float velocity_y = 9;
  float velocity_z = 10;
}

// Character or vehicle vital stats
message PlayerStateTelemetry {
  float health_current = 1;
  float health_maximum = 2;
  float stamina_current = 3;
  float stamina_maximum = 4;
  float oxygen_current = 5;
  float oxygen_maximum = 6;
  bool is_in_combat = 7;
  bool is_submerged = 8;
  uint32 character_level = 9;
}

// Low-latency input capture and injection frame
message InputStateTelemetry {
  // Bitmask representing digital buttons
  uint64 digital_buttons_mask = 1;
  
  // Analog stick axes normalized [-1.0 .. 1.0]
  float left_stick_x = 2;
  float left_stick_y = 3;
  float right_stick_x = 4;
  float right_stick_y = 5;
  
  // Analog triggers normalized [0.0 .. 1.0]
  float left_trigger = 6;
  float right_trigger = 7;
}

// Generic key-value metric batch
message GenericMetricBatch {
  map<string, double> numeric_metrics = 1;
  map<string, string> text_attributes = 2;
  map<string, bool> boolean_flags = 3;
}
```

---

## 5. Contract Invariants & Architectural Traceability

1. **Deterministic Correlation:** Responses MUST echo `message_id` into `correlation_id`.
2. **Strict Time Budgeting:** If `deadline_utc` is provided and current time exceeds `deadline_utc`, receiver MUST abort processing immediately and return `TIMEOUT_OPERATION_EXPIRED`.
3. **Traceability:** Aligns with `NLE-SPC-MSG-001`, `ADR-0002`, `ADR-0003`, and `REQ-SEC-001`.

