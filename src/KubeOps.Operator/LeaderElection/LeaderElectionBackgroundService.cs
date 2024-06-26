using k8s.LeaderElection;

using Microsoft.Extensions.Hosting;

namespace KubeOps.Operator.LeaderElection;

/// <summary>
/// This background service connects to the API and continuously watches the leader election.
/// </summary>
/// <param name="elector">The elector.</param>
internal sealed class LeaderElectionBackgroundService(LeaderElector elector)
    : IHostedService, IDisposable, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // The current implementation of IHostedService expects that StartAsync is "really" asynchronous.
        // Blocking calls are not allowed, they would stop the rest of the startup flow.
        //
        // This is an open issue since 2019 and not expected to be closed soon. (https://github.com/dotnet/runtime/issues/36063)
        // For reasons unknown at the time of writing this code, "await Task.Yield()" didn't work as expected, it caused
        // a deadlock in 1/10 of the cases.
        //
        // Therefore, we use Task.Run() and put the work to queue. The passed cancellation token of the StartAsync
        // method is not used, because it would only cancel the scheduling (which we definitely don't want to cancel).
        // To make this intention explicit, CancellationToken.None gets passed.
        _ = Task.Run(() => elector.RunUntilLeadershipLostAsync(_cts.Token), CancellationToken.None);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Dispose();
        elector.Dispose();
        _disposed = true;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            return Task.CompletedTask;
        }

#if NET8_0_OR_GREATER
        return _cts.CancelAsync();
#else
        _cts.Cancel();
        return Task.CompletedTask;
#endif
    }

    public async ValueTask DisposeAsync()
    {
        await CastAndDispose(_cts);
        await CastAndDispose(elector);

        _disposed = true;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }
}
