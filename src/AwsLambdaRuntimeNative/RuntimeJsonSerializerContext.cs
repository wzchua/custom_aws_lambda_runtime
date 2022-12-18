using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwsLambdaRuntimeNative;

[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(LambdaErrorRequest))]
internal partial class RuntimeJsonSerializerContext : JsonSerializerContext
{
    public static readonly RuntimeJsonSerializerContext LambdaContext = Create();
    private static RuntimeJsonSerializerContext Create()
    {
        var option = new JsonSerializerOptions
        {
            WriteIndented = Environment.GetEnvironmentVariable("WRITE_INDENTED_JSON") is "true",
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        return new RuntimeJsonSerializerContext(option);
    }
}