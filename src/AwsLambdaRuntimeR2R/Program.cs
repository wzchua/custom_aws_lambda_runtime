using System.Runtime.InteropServices;
using AwsLambdaRuntime;
using AwsLambdaRuntimeR2R;

using var cts = new CancellationTokenSource();

void Cancel(PosixSignalContext context)
{
    context.Cancel = true;

    // ReSharper disable once AccessToDisposedClosure
    cts.Cancel();
}
using var reg = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Cancel);
using var reg2 = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, Cancel);

var runtime = new LambdaRuntime<ToUpperFunction>();
await runtime.RunAsync(cts.Token);