using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
namespace Web.Services;

/// <summary>
/// Provides meta data and events to orchestrate the app, including using servers signalr hub.
/// </summary>
public class StatusService
{
    private IJSRuntime _jsRuntime;
    public StatusService(IWebAssemblyHostEnvironment env, IJSRuntime jsRuntime) 
    {
        _jsRuntime = jsRuntime;
        var thisRef = DotNetObjectReference.Create(this);
        _jsRuntime.InvokeVoidAsync("registerStatus", thisRef);
        // init????
    }


    // PWA App utility which modify approach to query etc to support online and offlining
    private bool _online;
    public bool Online { get => _online; set { if (_online != value) { _online = value; OnStatusChange?.Invoke(); } } }

    private bool _dbAccessible;
    public bool DbAccessible { get => _dbAccessible; set { if (_dbAccessible != value) { _dbAccessible = value; OnStatusChange?.Invoke(); } } }


    [JSInvokable]
    public void SetOnline(bool isOnline) => Online = isOnline;
    public async Task<bool> IsDbAccessible() => await _jsRuntime.InvokeAsync<bool>("initDb");

    public event Action? OnStatusChange;



    // Loading / Busy
    private bool _isLoading = false;
    public bool IsLoading { get => _isLoading; set { if (_isLoading != value) { _isLoading = value; OnLoadingChange?.Invoke(); } } }

    private string? _loadingMessage = null;
    public string? LoadingMessage { get => _loadingMessage; set { if (_loadingMessage != value) { _loadingMessage = value; OnLoadingChange?.Invoke(); } } }

    public async Task<T?> StartLoading<T>(Task<T> task, string? message = null)
    {
        // queue?
        await Task.Run(() =>
        {
            while (IsLoading == true) {}
        });

        IsLoading = true;
        if (message != null) LoadingMessage = message;
        var res = await task;
        IsLoading = false;
        return res;
    }

    public async Task StartLoading(Task task, string? message = null)
    {
        // queue?
        await Task.Run(() =>
        {
            while (IsLoading == true) {}
        });

        IsLoading = true;
        if (message != null) LoadingMessage = message;
        await task;
        IsLoading = false;
    }

    public event Action? OnLoadingChange;
}
