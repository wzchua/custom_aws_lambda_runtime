using System.Text.Json.Serialization;

namespace AwsLambdaRuntime;

public sealed class LambdaErrorRequest
{
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
    [JsonPropertyName("errorType")]
    public string? ErrorType { get; set; }
    [JsonPropertyName("stackTrace")]
    public string[]? StackTrace { get; set; }
}