using NexLudic.Simulation.Contracts;
using NexLudic.Simulation.Doubles;
using NexLudic.Simulation.Exceptions;

namespace NexLudic.Simulation.Tests.TestScenarios;

public class SampleEndpointScenario : IScenario
{
    private readonly SyntheticEndpointBehavior _behavior;
    private ISyntheticEndpoint? _endpoint;

    public string ScenarioId { get; }
    public string Title => $"Synthetic Endpoint Scenario ({_behavior})";
    public string Description => "Exercises synthetic endpoint mock message exchange under Medium Integrity Level.";
    public string Layer => "Simulation";
    public IReadOnlyList<string> RequirementIds => new[] { "REQ-TEST-001", "REQ-LIVE-001" };
    public IReadOnlyList<string> RequiredCapabilities => new[] { "InProcessMockBridge" };
    public IReadOnlyList<string> Prerequisites => Array.Empty<string>();

    public SampleEndpointScenario(string id, SyntheticEndpointBehavior behavior)
    {
        ScenarioId = id;
        _behavior = behavior;
    }

    public async Task SetupAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        var config = new SyntheticEndpointConfig
        {
            EndpointId = $"sim-{ScenarioId}",
            DefaultBehavior = _behavior,
            RejectionReason = "Test rejection trigger",
            EnforceMediumIntegrityOnly = true,
            DisallowProcessMemoryAccess = true
        };

        _endpoint = new SyntheticMockEndpoint(config, context.Clock, context.Diagnostics);
        context.TrackDisposable(_endpoint);
        await _endpoint.ConnectAsync(cancellationToken);
    }

    public async Task<ScenarioExecutionResult> ExecuteAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        if (_endpoint == null || !_endpoint.IsConnected)
        {
            return ScenarioExecutionResult.Fail(ScenarioId, context.RunId, "Endpoint not connected", new[] { "Endpoint is null or disconnected" });
        }

        var envelope = new SyntheticMessageEnvelope(
            MessageId: "msg-001",
            SessionId: "ses-001",
            SourceEndpointId: "harness-client",
            TargetEndpointId: _endpoint.EndpointId,
            MessageType: "M01.STATE_SYNC",
            SequenceNumber: 1,
            Timestamp: context.Clock.UtcNow,
            PayloadJson: "{\"position\": [10.0, 20.0, 30.0]}"
        );

        var response = await _endpoint.SendAndReceiveAsync(envelope, cancellationToken);

        if (_behavior == SyntheticEndpointBehavior.Acknowledge)
        {
            if (response == null || response.MessageType != "M01.ACK")
            {
                throw new ScenarioAssertionException(ScenarioId, "Expected M01.ACK response from mock endpoint", "M01.ACK", response?.MessageType ?? "null");
            }
            return ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "Received expected ACK envelope.");
        }
        else if (_behavior == SyntheticEndpointBehavior.Reject)
        {
            if (response == null || response.MessageType != "M02.NACK")
            {
                throw new ScenarioAssertionException(ScenarioId, "Expected M02.NACK response from mock endpoint", "M02.NACK", response?.MessageType ?? "null");
            }
            return ScenarioExecutionResult.Pass(ScenarioId, context.RunId, "Received expected NACK envelope.");
        }

        return ScenarioExecutionResult.Pass(ScenarioId, context.RunId, $"Executed behavior {_behavior}");
    }

    public async Task TeardownAsync(ScenarioContext context, CancellationToken cancellationToken = default)
    {
        if (_endpoint != null && _endpoint.IsConnected)
        {
            await _endpoint.DisconnectAsync(cancellationToken);
        }
    }
}
