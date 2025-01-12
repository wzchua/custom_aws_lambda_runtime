﻿using System.Text.Json.Serialization;

namespace AwsLambdaRuntime;

internal sealed class ErrorResponse
{
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = null!;
    [JsonPropertyName("errorType")]
    public string ErrorType { get; set; } = null!;
}