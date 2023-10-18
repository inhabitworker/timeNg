using Microsoft.AspNetCore.Components;

namespace Web.Components.Statistics;

public class HeatmapBase : ComponentBase {
    [Parameter] public Dictionary<DateTime, double> Data { get; set; }
    [Inject] public NavigationManager _navMan { get; set; }
    protected DateTime Start { get; set; } = DateTime.Now.Date;
    protected DateTime End { get; set; } = DateTime.Now.Date.AddYears(-1);
    protected DateTime Current { get; set; }

    protected override void OnParametersSet()
    {
        while (Start.DayOfWeek != DayOfWeek.Saturday) Start = Start.AddDays(1);
        while (End.DayOfWeek != DayOfWeek.Sunday) End = End.AddDays(-1); 
        Current = End;
    }

    protected void DayNavigate(string param)
    {
        _navMan.NavigateTo($"/day/{param}");
    }

}
