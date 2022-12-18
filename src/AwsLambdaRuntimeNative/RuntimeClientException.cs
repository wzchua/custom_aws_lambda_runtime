namespace AwsLambdaRuntimeNative;

public sealed class RuntimeClientException : Exception
{
    public RuntimeClientException(string message) : base(message)
    {
    }
    public RuntimeClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}