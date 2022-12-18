using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace AwsLambdaRuntime;

public sealed class LambdaRuntime<TLambdaFunction> where TLambdaFunction : ILambdaFunction<TLambdaFunction>
{
    private readonly RuntimeApiClient _runtimeApiClient = new();
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(new CustomJsonFormatter())
            .CreateLogger();
        var x = new SerilogLoggerFactory(Log.Logger);
        var logger = x.CreateLogger<LambdaRuntime<TLambdaFunction>>();
        
        try
        {
            var lambdaFunction = await InitializeAsync(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await InvokeAsync(lambdaFunction, logger, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            logger.LogError(e, "unexpected runtime error");
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
            var error = BuildLambdaErrorRequest(e);
            await _runtimeApiClient.SendInitErrorAsync(error, cancellationToken);
            throw;
        }
    }

    private async Task InvokeAsync(TLambdaFunction lambdaFunction, ILogger<LambdaRuntime<TLambdaFunction>> logger,
        CancellationToken cancellationToken)
    {
        using var request = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

        try
        {
            using var response = await lambdaFunction.InvokeAsync(request, cancellationToken);
            await _runtimeApiClient.SendInvocationResponseAsync(request.RequestId, response, cancellationToken);
        }
        catch (Exception e)
        {
            var error = BuildLambdaErrorRequest(e);
            await _runtimeApiClient.SendInvocationErrorAsync(request.RequestId, error, cancellationToken);
            logger.LogError(e, "unexpected invocation error");
        }
    }

    private static LambdaErrorRequest BuildLambdaErrorRequest(Exception e)
    {
        var error = new LambdaErrorRequest
        {
            ErrorMessage = e.Message,
            ErrorType = e.GetType().ToString(),
            StackTrace = e.StackTrace?.Split('\n').Select(s => s.Trim()).ToArray(),
        };
        return error;
    }
}