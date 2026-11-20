namespace NexLudic.Simulation.Messaging;

/// <summary>
/// Result of the 7-stage message validation pipeline.
/// </summary>
public sealed class MessageValidationResult
{
    public bool IsValid { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public int FailedStage { get; }

    private MessageValidationResult(bool isValid, string? errorCode = null, string? errorMessage = null, int failedStage = 0)
    {
        IsValid = isValid;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        FailedStage = failedStage;
    }

    public static MessageValidationResult Success() => new(true);

    public static MessageValidationResult Fail(string errorCode, string message, int stage) =>
        new(false, errorCode, message, stage);
}
