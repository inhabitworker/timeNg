using Microsoft.AspNetCore.Components;

namespace Web.Components.Data;

public abstract class DataComponentBase<T, U> : ComponentBase, IDisposable where U : Query
{
    [Inject] public IQueryService _query { get; set; }
    [Inject] public IEventsServiceClient _events { get; set; }


    private U _filter;
    public U Filter { get => _filter; set { _filter = value; Refresh(); } }

    public QueryResponse<T>? Response { get; set; }


    public bool IsLoading { get; set; } = true;
    public bool IsRefreshing { get; set; } = false;
    public bool IsFailed { get; set; } = false;


    protected override void OnInitialized()
    {
        _events.OnDataChanged += async () => await Refresh();
    }

    protected override async Task OnParametersSetAsync()
    {
        Response = await Fetch();
        IsLoading = false;
    }

    public abstract Task<QueryResponse<T>> Fetch();

    public async Task Refresh()
    {
        IsRefreshing = true;
        StateHasChanged();

        Response = await Fetch();
        IsRefreshing = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        _events.OnDataChanged -= async () => await Refresh();
    }

}
