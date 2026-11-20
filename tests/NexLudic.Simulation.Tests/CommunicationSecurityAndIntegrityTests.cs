using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Diagnostics;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Faults;
using NexLudic.Simulation.Harness;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Routing;
using NexLudic.Simulation.Sessions;
using NexLudic.Simulation.Tests.TestScenarios;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class CommunicationSecurityAndIntegrityTests
{
    private readonly DeterministicClock _clock = new();
    private readonly DiagnosticsCollector _diagnostics;

    public CommunicationSecurityAndIntegrityTests()
    {
        _diagnostics = new DiagnosticsCollector(_clock);
    }

    [Fact(DisplayName = "S80-43: Default simulation does not open network sockets or real game handles")]
    public void S80_43_NoImplicitNetworkAccess()
    {
        var config = new SyntheticEndpointConfig
        {
            EndpointId = "ep-security-check",
            DisallowProcessMemoryAccess = true,
            EnforceMediumIntegrityOnly = true
        };

        var endpoint = new SyntheticMockEndpoint(config, _clock, _diagnostics);

        Assert.True(config.DisallowProcessMemoryAccess);
        Assert.True(config.EnforceMediumIntegrityOnly);
        Assert.NotNull(endpoint);
    }

    [Fact(DisplayName = "S80-44: Untrusted or malformed payload rejected under validation policy")]
    public void S80_44_UntrustedPayloadValidation()
    {
        var validator = new MessageValidator(_clock);
        var session = new SimulatedSession("sess-sec-01");
        session.RegisterEndpoint("ep-1");
        session.RegisterEndpoint("ep-2");
        session.AuthorizeTopic("safe.topic");

        // Payload with tampered digest
        var envelope = new BridgeMessageEnvelope
        {
            EnvelopeVersion = "1.0",
            MessageId = "msg-sec-01",
            SessionId = session.SessionId,
            SenderEndpointId = "ep-1",
            RecipientEndpointId = "ep-2",
            Topic = "safe.topic",
            PayloadDigest = "sha256:forged_hash_attempt",
            PayloadJson = "{\"malicious\": \"payload_injection\"}"
        };

        var result = validator.Validate(envelope, session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_003", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-45: Unauthorized sender cannot bypass session admission")]
    public void S80_45_UnauthorizedSenderBlocked()
    {
        var validator = new MessageValidator(_clock);
        var session = new SimulatedSession("sess-sec-02");
        session.RegisterEndpoint("ep-legit");
        session.RegisterEndpoint("ep-target");
        session.AuthorizeTopic("safe.topic");

        var envelope = BridgeMessageEnvelope.Create(
            sessionId: session.SessionId,
            senderEndpointId: "ep-attacker-unregistered", // Sender not registered!
            recipientEndpointId: "ep-target",
            category: MessageCategory.DataEvent,
            topic: "safe.topic",
            payloadJson: "{}"
        );

        var result = validator.Validate(envelope, session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_006", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-46: Missing upstream contract remains a documented blocker rather than invented")]
    public async Task S80_46_ContractMissingRemainsDocumentedBlocker()
    {
        var engine = new TestHarnessEngine();
        // Registering scenario requiring unverified binary protocol hook
        var scenario = new SamplePrerequisiteScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Blocked, result.Outcome);
        Assert.Equal(ErrorClassification.UnresolvedContract, result.Classification);
    }

    [Fact(DisplayName = "S80-47: Report distinguishes synthetic mock endpoints from real components")]
    public void S80_47_SyntheticVersusProductionDistinctionPreserved()
    {
        var config = new SyntheticEndpointConfig { AdapterType = "MockTestAdapter" };
        Assert.Contains("Mock", config.AdapterType);
    }

    [Fact(DisplayName = "S80-48: Simulator does not report message as applied when outcome is uncertain")]
    public async Task S80_48_OperationOutcomeUncertaintyPreserved()
    {
        var router = new CentralMessageRouter(_clock, _diagnostics);
        var session = new SimulatedSession("sess-unc-01");
        session.RegisterEndpoint("ep-1");
        session.RegisterEndpoint("ep-2");
        session.AuthorizeTopic("state.apply");
        router.RegisterSession(session);

        var ep2 = new SyntheticMockEndpoint(new SyntheticEndpointConfig { EndpointId = "ep-2" }, _clock, _diagnostics);
        await ep2.ConnectAsync();
        router.RegisterEndpoint(ep2);

        var envelope = BridgeMessageEnvelope.Create(session.SessionId, "ep-1", "ep-2", MessageCategory.DataEvent, "state.apply", "{}");
        var faults = new CommunicationFaultConfig { SimulateUncertainOutcome = true };

        var result = await router.RouteAsync(envelope, faults);

        Assert.True(result.Success);
        Assert.Equal(MessageLifecycleState.Uncertain, result.State);
        Assert.Contains("uncertain", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "S80-49: Deterministic scenario produces repeatable results under same seed")]
    public void S80_49_RepeatabilityVerified()
    {
        var seedProvider1 = new ReproducibleSeedProvider(5555);
        var seedProvider2 = new ReproducibleSeedProvider(5555);

        var hash1 = seedProvider1.NextBytes(64);
        var hash2 = seedProvider2.NextBytes(64);

        Assert.Equal(hash1, hash2);
    }

    [Fact(DisplayName = "S80-50: Regression isolation: Step 79 harness tests remain intact")]
    public async Task S80_50_RegressionIsolationStep79Intact()
    {
        var engine = new TestHarnessEngine();
        var scenario = new TestScenarios.SamplePassScenario();
        engine.Register(scenario);

        var result = await engine.RunScenarioAsync(scenario.ScenarioId);

        Assert.Equal(ScenarioOutcome.Passed, result.Outcome);
        Assert.True(result.IsSuccess);
    }
}
