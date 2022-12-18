namespace AwsLambdaRuntime;

public sealed class InvocationRequest : IDisposable
{
    public required string RequestId { get; init; }
    public long? DeadlineMs { get; init; }
    public string? InvokedFunctionArn { get; init; }
    public string? TraceId { get; init; }
    public string? ClientContext { get; init; }
    public string? CognitoIdentity { get; init; }
    public required Stream Body { get; init; }

    void IDisposable.Dispose()
    {
        Body.Dispose();
    }
}