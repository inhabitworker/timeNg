using Microsoft.AspNetCore.Components;

namespace Web.Pages;

public class StatisticsBase : ComponentBase 
{
    [Inject] public IEventsServiceClient _events { get; set; }
    [Inject] public IStatsService statsService { get; set; }

    public Dictionary<DateTime, double>? HeatmapData { get; set; }
    public IEnumerable<TagDTO>? TopTagsData { get; set; }
    public MiscStats? MiscStatsData { get; set; }

    protected override void OnInitialized()
    {
        LoadHeatmap();
        LoadTopTags();
        LoadMiscStats();
    }

    public async Task LoadHeatmap()
    {
        HeatmapData = await statsService.Heatmap();
        StateHasChanged();
    }

    public async Task LoadTopTags()
    {
        TopTagsData = await statsService.TopTags();
        StateHasChanged();
    }

    public async Task LoadMiscStats()
    {
        MiscStatsData = await statsService.Misc();
        StateHasChanged();
    }
    
}
