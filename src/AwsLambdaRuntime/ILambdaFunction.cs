namespace AwsLambdaRuntime;

public interface ILambdaFunction<T> where T : ILambdaFunction<T>
{
    static abstract ValueTask<T> CreateAsync(CancellationToken cancellationToken);
    ValueTask<TypedJsonContent> InvokeAsync(InvocationRequest invocationRequest, CancellationToken cancellationToken);
}