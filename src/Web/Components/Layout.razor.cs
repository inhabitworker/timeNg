using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Web.Services;

namespace Web.Components;

public class LayoutBase : LayoutComponentBase, IDisposable
{
    [Inject] public IQueryService _query { get; set; }
    [Inject] public IEventsServiceClient _events { get; set; }
    [Inject] public IJSRuntime _runtime { get; set; }
    [Inject] public StatusService _status { get; set; }
    [Inject] public NavigationManager navigationManager { get; set; }

    public ConfigDTO Config { get; set; }
    public bool Online { get; set; }

    public bool ReadOnly { get; set; } = true;

    protected async override Task OnInitializedAsync()
    {
        Config = await _query.Config();

        _events.OnConfigChanged += async () => await RefreshConfig();
        _status.OnStatusChange += RefreshStatus;
    }

    public async Task RefreshConfig()
    {
        Config = await _query.Config();

        await _runtime.InvokeVoidAsync("getCss");
        StateHasChanged();
    }

    public void RefreshStatus()
    {
        if (Online != _status.Online)
        {
            Online = _status.Online;
            StateHasChanged();
        }
    }

    public void ToggleReadOnly()
    {
        ReadOnly = !ReadOnly;
        StateHasChanged();
    }

    public void Dispose()
    {
        _events.OnConfigChanged -= async () => await RefreshConfig();
        _status.OnStatusChange -= RefreshStatus;
    }

}
