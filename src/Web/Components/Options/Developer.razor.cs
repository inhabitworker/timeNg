using Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Options;

public class DeveloperBase : ComponentBase 
{
    [Inject] protected IDeveloperClient _devClient { get; set; }
    [Inject] protected IEventsService _events { get; set; }
    [Inject] protected IJSRuntime JSRuntime { get; set; }
    [Inject] protected StatusService _status { get; set; }

    protected async Task Seed(int months = 3)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to generate mock data? Currently loaded data will be lost."))
            return;

        await _devClient.SeedAsync(months);
    }

    protected async Task GenerateColour()
    {
        throw new NotImplementedException();
    }

    protected async Task FakeLoad()
    {
        // multiple sequence tasks, enumerable?
        await _status.StartLoading(Task.Delay(3000), "Testing loading modal.");
    }

    protected async Task AddNotification(bool error)
    {
        var random = new Random();
        await _events.SendNotification($"Test {(error ? "error" : "notification")}: {random.Next(100)}", error, 7);
    }

    protected async Task AddNotificationGlobal(bool error)
    {
        var random = new Random();
        await _devClient
            .SendNotificationAsync($"Remote test: {(error ? "error" : "notification")}: {random.Next(100)}", error, 7);
    }
}
