using Microsoft.AspNetCore.Components;

namespace Web.Components.Overlay;

public class NotificationsBase : ComponentBase, IDisposable  
{
    [Inject] IEventsServiceClient _events { get; set; }
    protected List<Notification> _notifications { get; set; } = new();

    protected override void OnInitialized()
    {
        _events.OnNotificationReceived += Create;
    }

    public void Create(string message, bool isError = false, int seconds = 5)
        => Add(new(message, isError, seconds));

    public void Add(Notification notification)
    {
        notification.OnExpire += () => Remove(notification);
        _notifications.Add(notification);
        InvokeAsync(StateHasChanged);
    }

    public void Remove(Notification notification)
    {
        _notifications.Remove(notification);
        notification.Dispose();
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _notifications.ForEach(Remove);
        _events.OnNotificationReceived -= Create;
    }
}
