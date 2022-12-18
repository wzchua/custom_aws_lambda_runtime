namespace AwsLambdaRuntime;

public static class LambdaEnvironment
{
    internal const string EnvVarFunctionMemorySize = "AWS_LAMBDA_FUNCTION_MEMORY_SIZE";
    internal const string EnvVarFunctionName = "AWS_LAMBDA_FUNCTION_NAME";
    internal const string EnvVarFunctionVersion = "AWS_LAMBDA_FUNCTION_VERSION";
    internal const string EnvVarHandler = "_HANDLER";
    internal const string EnvVarLogGroupName = "AWS_LAMBDA_LOG_GROUP_NAME";
    internal const string EnvVarLogStreamName = "AWS_LAMBDA_LOG_STREAM_NAME";
    internal const string EnvVarServerHostAndPort = "AWS_LAMBDA_RUNTIME_API";
    internal const string EnvVarTraceId = "_X_AMZN_TRACE_ID";

    public static readonly string FunctionMemorySize = Environment.GetEnvironmentVariable(EnvVarFunctionMemorySize) ?? "";
    public static readonly string FunctionName = Environment.GetEnvironmentVariable(EnvVarFunctionName) ?? "";
    public static readonly string FunctionVersion = Environment.GetEnvironmentVariable(EnvVarFunctionVersion) ?? "";
    public static readonly string LogGroupName = Environment.GetEnvironmentVariable(EnvVarLogGroupName) ?? "";
    public static readonly string LogStreamName = Environment.GetEnvironmentVariable(EnvVarLogStreamName) ?? "";
    public static readonly string RuntimeServerHostAndPort = Environment.GetEnvironmentVariable(EnvVarServerHostAndPort) ?? "";
    public static readonly string Handler = Environment.GetEnvironmentVariable(EnvVarHandler) ?? "";
    
}