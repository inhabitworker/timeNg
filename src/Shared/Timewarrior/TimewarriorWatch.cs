using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Shared.Timewarrior;

// Watch data files. Interruptible, not a typical background service, as far as I can see.

// Docker:
// DOTNET_USE_POLLING_FILE_WATCHER

public class TimewarriorWatch
{
    public bool Override { get; set; } = false;

    public IChangeToken _changeToken;
    protected ILogger<TimewarriorWatch> _logger;
    private IFileProvider _provider;

    public TimewarriorWatch(ILogger<TimewarriorWatch> logger, IFileProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public Task Start(CancellationToken cancellation)
    {
        return Task.Run(() =>
        {
            while (!cancellation.IsCancellationRequested)
            {
                Watch().GetAwaiter().GetResult();
            }
        });
    }

    protected async Task Watch()
    {
        _logger.LogInformation("Watching data files.");

        _changeToken = _provider.Watch("data/*.*");

        // creating task for awaiting 
        var changeSource = new TaskCompletionSource<object>();
        _changeToken.RegisterChangeCallback(state => ((TaskCompletionSource<object>)state).TrySetResult(null), changeSource);
        await changeSource.Task.ConfigureAwait(false);
        // callback has been awaited

        if (!Override) OnChangeDetected?.Invoke();
    }

    public event Action? OnChangeDetected;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
