using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambdaRuntime;
using Npgsql;

namespace AwsLambdaRuntimeR2R;

public sealed class PostgresQueryFunction : ILambdaFunction<PostgresQueryFunction>
{
    public static ValueTask<PostgresQueryFunction> CreateAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<PostgresQueryFunction>(new PostgresQueryFunction());
    }

    public async ValueTask<TypedJsonContent> InvokeAsync(InvocationRequest invocationRequest, CancellationToken cancellationToken)
    {
        var query = await JsonSerializer.DeserializeAsync(invocationRequest.Body,
            PostgresQueryFunctionJsonSerializerContext.LambdaContext.Query, cancellationToken);
        ArgumentNullException.ThrowIfNull(query);
        
        using var connection = new NpgsqlConnection("");
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();

        command.CommandText = query.SqlText;
        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

        var response = new QueryResponse
        {
            Message = "success",
            AffectedRows = affectedRows
        };
        return new TypedJsonContent<QueryResponse>(
            PostgresQueryFunctionJsonSerializerContext.LambdaContext.QueryResponse, response);
    }
    
    public sealed class Query
    {
        public string? SqlText { get; set; }
    }
    
    public sealed class QueryResponse
    {
        public string? Message { get; set; }
        public int AffectedRows { get; set; }
    }
}

[JsonSerializable(typeof(PostgresQueryFunction.Query))]
[JsonSerializable(typeof(PostgresQueryFunction.QueryResponse))]
internal partial class PostgresQueryFunctionJsonSerializerContext : JsonSerializerContext
{
    public static readonly PostgresQueryFunctionJsonSerializerContext LambdaContext = Create();
    private static PostgresQueryFunctionJsonSerializerContext Create()
    {
        var option = new JsonSerializerOptions
        {
            WriteIndented = Environment.GetEnvironmentVariable("WRITE_INDENTED_JSON") is "true",
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return new PostgresQueryFunctionJsonSerializerContext(option);
    }
}