namespace Shared.Interfaces;

public interface IEventsServiceClient : IEventsServiceClientHub
{
    public event Action<string, bool, int>? OnNotificationReceived;
    public event Action? OnDataChanged;
    public event Action? OnConfigChanged;
}

public interface IEventsServiceClientHub
{
    public Task ReceiveNotification(string message, bool error = false, int seconds = 3);
    public Task DataChanged();
    public Task ConfigChanged();

}
