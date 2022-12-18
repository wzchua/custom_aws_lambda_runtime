using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AwsLambdaRuntime;

public abstract class TypedJsonContent : HttpContent
{
    protected static readonly MediaTypeWithQualityHeaderValue JsonContentType = new("application/json");

    protected sealed override Task SerializeToStreamAsync(Stream stream, TransportContext? context) => throw new NotSupportedException();
    protected abstract override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken);

    protected sealed override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }
}

public sealed class TypedJsonContent<T> : TypedJsonContent
{
    private readonly JsonTypeInfo<T> _typeInfo;
    private readonly T? _value;

    public TypedJsonContent(JsonTypeInfo<T> typeInfo, T? value)
    {
        _typeInfo = typeInfo;
        _value = value;
        Headers.ContentType = JsonContentType;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        await JsonSerializer.SerializeAsync(stream, _value, _typeInfo!, cancellationToken);
    }
}