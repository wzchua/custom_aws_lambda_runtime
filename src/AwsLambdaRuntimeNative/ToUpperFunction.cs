using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwsLambdaRuntimeNative;

public readonly struct ToUpperFunction : ILambdaFunction<ToUpperFunction>
{
    public static ValueTask<ToUpperFunction> CreateAsync(CancellationToken cancellationToken)
    {
        _ = ToUpperJsonSerializerContext.Default.String;
        return new ValueTask<ToUpperFunction>(new ToUpperFunction());
    }

    public async ValueTask<TypedJsonContent> InvokeAsync(InvocationRequest invocationRequest, CancellationToken cancellationToken)
    {
        var text = await JsonSerializer.DeserializeAsync(invocationRequest.Body,
            ToUpperJsonSerializerContext.Default.String, cancellationToken);

        text = text?.ToUpper(CultureInfo.InvariantCulture);
        
        return new TypedJsonContent<string>(ToUpperJsonSerializerContext.Default.String, text);
    }
}

[JsonSerializable(typeof(string))]
internal partial class ToUpperJsonSerializerContext : JsonSerializerContext
{
}