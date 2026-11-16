# ADR-0008: AI Coding-Agent Provider Abstraction
## Document ID: NLE-ADR-0008
**Status:** Proposed  
**Date Created:** November 16, 2026  
**Decision Owner:** AI Development Systems Engineer & Security Architect  

---

### 1. Context and Problem
NexLudic Engine assists users by directing AI coding agents to generate, modify, and fix game modding projects. Tying the engine directly to a single proprietary cloud API (e.g. Anthropic Claude API or OpenAI API) creates vendor lock-in, exposes user credentials, incurs recurring cloud costs, and prevents users from using their preferred local coding tools (such as Claude Code CLI, Gemini CLI, Aider, or local SLMs).

### 2. Requirements Addressed
- `REQ-AI-001`: Structured Prompt Generation.
- `REQ-AI-002`: Sandboxed Workspace Isolation.
- `REQ-SAFE-002`: Local-Only Telemetry & Privacy.
- `DEC-005`: Agent Sandboxing & Diff Tracking.

### 3. Constraints and Assumptions
- Different coding agent tools possess divergent interfaces, command arguments, context windows, and execution models.
- The engine must never store or transmit secret API keys without explicit user configuration.
- The platform must support offline and local execution where possible.

### 4. Options Considered
- **Option 1: Dual-Layer Protocol Abstraction (Model Context Protocol [MCP] + Subprocess CLI Runner).**
- **Option 2: Direct Hardcoded Cloud Vendor SDKs.** Tightly couples the core to Anthropic/OpenAI; violates vendor neutrality.
- **Option 3: Pure Manual Prompt Copy-Paste.** High user friction; no automated file staging or error-repair loops.

### 5. Evaluation Criteria
- Vendor neutrality and long-term vitality.
- Tool-calling standardization and safety sandboxing.
- Support for both local CLI tools and modern AI agent protocols.

### 6. Evidence and Source Links
- Step 4 Tool Catalog (`INTEGRATION_TOOL_CATALOG.md`) covering Model Context Protocol (MCP).
- Anthropic MCP Specification (`github.com/modelcontextprotocol`).

### 7. Decision
NexLudic Engine adopts a **Vendor-Neutral Dual-Layer Agent Architecture**:
1. **Tool Standardization (MCP Host & Server):** NexLudic Engine implements a Model Context Protocol (MCP) server interface, exposing safe tools (`read_blueprint`, `write_staged_file`, `compile_mod`, `run_tests`) to any MCP-compliant coding agent.
2. **Execution Subprocess Runner (`IAgentRunner`):** An extensible interface supporting multiple provider adapters:
   - `ClaudeCodeCliRunner`: Orchestrates local Claude Code CLI in subshell.
   - `GeminiCliRunner`: Orchestrates local Gemini CLI.
   - `LocalLlmRunner`: Connects to local OpenAI-compatible endpoints (Ollama / ONNX).
   - `ManualPromptHandoff`: Generates formatted text files for manual web UI copy-paste.

### 8. Rationale
MCP is rapidly becoming the open industry standard for AI tool calling. Combining MCP with a controlled subprocess runner gives users total freedom to select their preferred agent tool while ensuring NexLudic Engine strictly governs what the agent is allowed to see and modify.

### 9. Advantages
- 100% vendor-neutral; zero lock-in to any single AI provider.
- Complete privacy; the core platform does not require central server accounts or cloud telemetry.
- AI agents cannot perform actions outside the tools explicitly provided by the MCP server.

### 10. Disadvantages and Risks
- Requires maintaining CLI argument parsers for different third-party agent tools.

### 11. Consequences for Other Components
The `NexLudic.Core` layer will define `IAgentRunner` and `IAgentToolProvider`, implemented in `NexLudic.Infrastructure`.

### 12. Dependencies
- Model Context Protocol C# / JSON-RPC specification.

### 13. Conditions That Would Invalidate the Decision
Abandonment of MCP by major AI agent ecosystems in favor of an incompatible standard.

### 14. Follow-Up Work
Author the MCP tool schemas and prompt template engine in Phase 5.

### 15. Owner Approval Required
Pending Project Owner formal approval.

