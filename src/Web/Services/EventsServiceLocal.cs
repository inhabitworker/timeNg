namespace Web.Services;

public class EventsServiceLocal : IEventsService
{
    protected IEventsServiceClient _client;

    public EventsServiceLocal(IEventsServiceClient client)
    {
        _client = client;
    }

    public async Task SendNotification(string message, bool error = false, int seconds = 3)
        => await _client.ReceiveNotification(message, error, seconds);

    public Task DataChanged() 
        => _client.DataChanged();

    public Task ConfigChanged()
        => _client.ConfigChanged();
}
