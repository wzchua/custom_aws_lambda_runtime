using System.Diagnostics;

namespace AwsLambdaRuntime;

public sealed class LambdaRuntime<TLambdaFunction> where TLambdaFunction : ILambdaFunction<TLambdaFunction>
{
    private readonly RuntimeApiClient _runtimeApiClient = new();
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var lambdaFunction = await InitializeAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await InvokeAsync(lambdaFunction, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async ValueTask<TLambdaFunction> InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _runtimeApiClient.InitAsync(cancellationToken);
            var lambdaFunction = await TLambdaFunction.CreateAsync(cancellationToken);
            return lambdaFunction;
        }
        catch (Exception e)
        {
            StackTrace stackTrace = new StackTrace(e, true);
            var error = new LambdaErrorRequest
            {
                ErrorMessage = e.Message,
                ErrorType = e.GetType().ToString(),
                StackTrace = stackTrace.GetFrames().Select(f => f.ToString()).ToArray(),
            };
            await _runtimeApiClient.SendInitErrorAsync(error, cancellationToken);
            throw;
        }
    }

    private async Task InvokeAsync(TLambdaFunction lambdaFunction, CancellationToken cancellationToken)
    {
        using var request = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

        try
        {
            using var response = await lambdaFunction.InvokeAsync(request, cancellationToken);
            await _runtimeApiClient.SendInvocationResponseAsync(request.RequestId, response, cancellationToken);
        }
        catch (Exception e)
        {
            StackTrace stackTrace = new StackTrace(e, true);
            var error = new LambdaErrorRequest
            {
                ErrorMessage = e.Message,
                ErrorType = e.GetType().ToString(),
                StackTrace = stackTrace.GetFrames().Select(f => f.ToString()).ToArray(),
            };
            await _runtimeApiClient.SendInvocationErrorAsync(request.RequestId, error, cancellationToken);
        }
    }
}