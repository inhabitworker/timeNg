namespace Shared.Interfaces;

public interface IEventsService
{
    public Task SendNotification(string messsage, bool error = false, int seconds = 3);
    public Task DataChanged();
    public Task ConfigChanged();
}
