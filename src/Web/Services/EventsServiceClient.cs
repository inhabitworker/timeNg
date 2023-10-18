using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace Web.Services;

public class EventsServiceClient : IEventsServiceClient
{
    private HubConnection Hub;

    public EventsServiceClient(IWebAssemblyHostEnvironment env)
    {
        // Signalr hub for pushing changes from serve
        Hub = new HubConnectionBuilder().WithUrl(env.BaseAddress + "signal").Build();

        Hub.On<string, bool, int>("ReceiveNotification", ReceiveNotification);
        Hub.On("DataChanged", DataChanged);
        Hub.On("ConfigChanged", ConfigChanged);

        Hub.StartAsync(); 
    }

    public async Task ReceiveNotification(string message, bool error = false, int seconds = 3)
        => OnNotificationReceived?.Invoke(message, error, seconds);
    public event Action<string, bool, int>? OnNotificationReceived;

    public async Task DataChanged()
        => OnDataChanged?.Invoke();
    public event Action? OnDataChanged;

    public async Task ConfigChanged()
    {
        await ReceiveNotification("Config has been changed.");
        OnConfigChanged?.Invoke();
    }
    public event Action? OnConfigChanged;

}
