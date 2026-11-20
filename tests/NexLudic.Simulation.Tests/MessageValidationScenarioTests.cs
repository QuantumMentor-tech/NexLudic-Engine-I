using NexLudic.Simulation.Determinism;
using NexLudic.Simulation.Messaging;
using NexLudic.Simulation.Sessions;
using Xunit;

namespace NexLudic.Simulation.Tests;

public class MessageValidationScenarioTests
{
    private readonly DeterministicClock _clock = new();
    private readonly MessageValidator _validator;
    private readonly SimulatedSession _session;

    public MessageValidationScenarioTests()
    {
        _validator = new MessageValidator(_clock);
        _session = new SimulatedSession("sess-valid-01");
        _session.RegisterEndpoint("endpoint-sender-01");
        _session.RegisterEndpoint("endpoint-recipient-01");
        _session.AuthorizeTopic("player.teleport");
    }

    [Fact(DisplayName = "S80-06: Valid message is accepted by validation pipeline")]
    public void S80_06_ValidMessageAccepted()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "endpoint-sender-01",
            recipientEndpointId: "endpoint-recipient-01",
            category: MessageCategory.CommandRequest,
            topic: "player.teleport",
            payloadJson: "{\"x\": 10, \"y\": 20}",
            sequenceNumber: 1,
            correlationId: "req-001"
        );

        var result = _validator.Validate(envelope, _session);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
    }

    [Fact(DisplayName = "S80-07: Missing required field receives defined validation error (ERR_PME_002)")]
    public void S80_07_MissingRequiredFieldRejected()
    {
        var envelope = new BridgeMessageEnvelope
        {
            EnvelopeVersion = "1.0",
            MessageId = "msg-001",
            SessionId = _session.SessionId,
            SenderEndpointId = "", // Missing!
            RecipientEndpointId = "endpoint-recipient-01",
            Topic = "player.teleport",
            PayloadJson = "{}"
        };

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_002", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-08: Invalid message identifier rejected (ERR_PME_002)")]
    public void S80_08_InvalidMessageIdRejected()
    {
        var envelope = new BridgeMessageEnvelope
        {
            EnvelopeVersion = "1.0",
            MessageId = "invalid_id_without_prefix", // Invalid: must start with 'msg-'
            SessionId = _session.SessionId,
            SenderEndpointId = "endpoint-sender-01",
            RecipientEndpointId = "endpoint-recipient-01",
            Topic = "player.teleport",
            PayloadJson = "{}"
        };

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_002", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-09: Invalid payload structure / digest mismatch rejected (ERR_PME_003)")]
    public void S80_09_PayloadDigestMismatchRejected()
    {
        var envelope = new BridgeMessageEnvelope
        {
            EnvelopeVersion = "1.0",
            MessageId = "msg-001",
            SessionId = _session.SessionId,
            SenderEndpointId = "endpoint-sender-01",
            RecipientEndpointId = "endpoint-recipient-01",
            Topic = "player.teleport",
            PayloadDigest = "sha256:invalid_or_tampered_digest",
            PayloadJson = "{\"valid\": true}"
        };

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_003", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-10: Unsupported envelope version produces defined error (ERR_PME_001)")]
    public void S80_10_UnsupportedVersionRejected()
    {
        var envelope = new BridgeMessageEnvelope
        {
            EnvelopeVersion = "9.9", // Unsupported! Expected 1.0
            MessageId = "msg-001",
            SessionId = _session.SessionId,
            SenderEndpointId = "endpoint-sender-01",
            RecipientEndpointId = "endpoint-recipient-01",
            Topic = "player.teleport"
        };

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_001", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-11: Unknown or unauthorized topic produces defined error (ERR_PME_008)")]
    public void S80_11_UnauthorizedTopicRejected()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "endpoint-sender-01",
            recipientEndpointId: "endpoint-recipient-01",
            category: MessageCategory.DataEvent,
            topic: "admin.super_nuke", // Unauthorized topic!
            payloadJson: "{}"
        );

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_008", result.ErrorCode);
    }

    [Fact(DisplayName = "S80-12: Contradictory sender metadata not registered in session rejected (ERR_PME_006)")]
    public void S80_12_ContradictorySenderMetadataRejected()
    {
        var envelope = BridgeMessageEnvelope.Create(
            sessionId: _session.SessionId,
            senderEndpointId: "endpoint-rogue-imposter", // Not registered in session!
            recipientEndpointId: "endpoint-recipient-01",
            category: MessageCategory.DataEvent,
            topic: "player.teleport",
            payloadJson: "{}"
        );

        var result = _validator.Validate(envelope, _session);

        Assert.False(result.IsValid);
        Assert.Equal("ERR_PME_006", result.ErrorCode);
    }
}

