using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class EventsService : Hub<IEventsServiceClientHub>, IEventsService
{
    public async Task SendNotification(string message, bool error = false, int seconds = 3)
        => await Clients.All.ReceiveNotification(message, error, seconds);

    public async Task DataChanged()
        => await Clients.All.DataChanged();

    public async Task ConfigChanged()
        => await Clients.All.ConfigChanged();
}
