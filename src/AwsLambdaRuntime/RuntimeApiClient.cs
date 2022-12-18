using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AwsLambdaRuntime;

public sealed class RuntimeApiClient
{
    private static readonly MediaTypeWithQualityHeaderValue s_jsonAccept = new("application/json");
    private readonly HttpClient _client;

    public RuntimeApiClient()
    {
        _client = new HttpClient();
        _client.Timeout = Timeout.InfiniteTimeSpan;
        _client.BaseAddress = new Uri($"http://{LambdaEnvironment.RuntimeServerHostAndPort}/2018-06-01/");
    }

    public ValueTask InitAsync(CancellationToken cancellationToken)
    {
        _ = RuntimeJsonSerializerContext.LambdaContext.LambdaErrorRequest;
        _ = RuntimeJsonSerializerContext.LambdaContext.ErrorResponse;

        return new ValueTask();
    }

    public async Task<InvocationRequest> GetNextInvocationAsync(CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "runtime/invocation/next");
        request.Headers.Accept.Add(s_jsonAccept);
        var response = await _client.SendAsync(request, cancellationToken);
        switch (response.StatusCode)
        {
            case HttpStatusCode.Forbidden:
                ThrowForbiddenError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case HttpStatusCode.InternalServerError:
                ThrowInternalServerError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case not HttpStatusCode.OK and not HttpStatusCode.NoContent:
                ThrowUnexpectedStatusCodeError(response, await response.Content.ReadAsStringAsync(cancellationToken));
                break;
        }
        
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return new InvocationRequest
        {
            RequestId = response.Headers.GetValues("Lambda-Runtime-Aws-Request-Id").First(),
            DeadlineMs = GetFirstHeaderOrNull(response.Headers, "Lambda-Runtime-Deadline-Ms") is {} value ? long.Parse(value) : null,
            InvokedFunctionArn = GetFirstHeaderOrNull(response.Headers, "Lambda-Runtime-Invoked-Function-Arn"),
            TraceId = GetFirstHeaderOrNull(response.Headers, "Lambda-Runtime-Trace-Id"),
            ClientContext = GetFirstHeaderOrNull(response.Headers, "Lambda-Runtime-Client-Context"),
            CognitoIdentity = GetFirstHeaderOrNull(response.Headers, "Lambda-Runtime-Cognito-Identity"),
            Body = stream,
        };

        static string? GetFirstHeaderOrNull(HttpResponseHeaders responseHeaders, string key)
        {
            if (responseHeaders.TryGetValues(key, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }
    }

    public async Task SendInvocationResponseAsync(string awsRequestId, TypedJsonContent jsonContent, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(awsRequestId);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"runtime/invocation/{awsRequestId}/response");
        request.Headers.Accept.Add(s_jsonAccept);
        request.Content = jsonContent;
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        switch (response.StatusCode)
        {
            case HttpStatusCode.Forbidden:
                ThrowForbiddenError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case HttpStatusCode.InternalServerError:
                ThrowInternalServerError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case HttpStatusCode.RequestEntityTooLarge:
                ThrowPayloadTooLarge(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case not HttpStatusCode.Accepted and not HttpStatusCode.NoContent:
                ThrowUnexpectedStatusCodeError(response, await response.Content.ReadAsStringAsync(cancellationToken));
                break;
        }
    }
    
    public async Task SendInitErrorAsync(LambdaErrorRequest jsonContent, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"runtime/init/error");
        request.Headers.Accept.Add(s_jsonAccept);
        request.Content = new TypedJsonContent<LambdaErrorRequest>(RuntimeJsonSerializerContext.Default.LambdaErrorRequest, jsonContent);
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        switch (response.StatusCode)
        {
            case HttpStatusCode.Forbidden:
                ThrowForbiddenError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case HttpStatusCode.InternalServerError:
                ThrowInternalServerError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case not HttpStatusCode.Accepted and not HttpStatusCode.NoContent:
                ThrowUnexpectedStatusCodeError(response, await response.Content.ReadAsStringAsync(cancellationToken));
                break;
        }
    }
    
    public async Task SendInvocationErrorAsync(string awsRequestId, LambdaErrorRequest jsonContent, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(awsRequestId);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"runtime/invocation/{awsRequestId}/error");
        request.Headers.Accept.Add(s_jsonAccept);
        request.Content = new TypedJsonContent<LambdaErrorRequest>(RuntimeJsonSerializerContext.LambdaContext.LambdaErrorRequest, jsonContent);
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        switch (response.StatusCode)
        {
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Forbidden:
                ThrowForbiddenError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case HttpStatusCode.InternalServerError:
                ThrowInternalServerError(await response.Content.ReadAsStringAsync(cancellationToken));
                break;
            case not HttpStatusCode.Accepted and not HttpStatusCode.NoContent:
                ThrowUnexpectedStatusCodeError(response, await response.Content.ReadAsStringAsync(cancellationToken));
                break;
        }
    }

    private static void ThrowUnexpectedStatusCodeError(HttpResponseMessage response, string responseBody)
    {
        throw new RuntimeClientException($"Unexpected Status Code {response.StatusCode} {responseBody}.");
    }

    private static void ThrowInternalServerError(string responseBody)
    {
        throw new RuntimeClientException($"InternalServerError {responseBody}.");
    }
    
    private static void ThrowPayloadTooLarge(string responseBody)
    {
        throw new RuntimeClientException($"Payload Too Large {responseBody}.");
    }

    private static void ThrowForbiddenError(string responseBody)
    {
        try
        {
            _ = JsonSerializer.Deserialize(responseBody, RuntimeJsonSerializerContext.Default.ErrorResponse);
            throw new RuntimeClientException($"Forbidden {responseBody}.");
        }
        catch (Exception e)
        {
            throw new RuntimeClientException($"Could not deserialize the response body {responseBody}.", e);
        }
    }
}