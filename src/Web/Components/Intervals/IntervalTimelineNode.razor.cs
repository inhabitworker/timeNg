using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Intervals;

public class IntervalTimelineNodeBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public ICommandService _commands { get; set; } 

    [CascadingParameter] public bool ReadOnly { get; set; } = true;
    [CascadingParameter] public IntervalQuery Filter { get; set; }

    [Parameter, EditorRequired] public IntervalDTO Interval { get; set; }


    /* Self contained? Kind of too heavy...
    [Parameter] public DateTime? Next { get; set; }
    [Parameter] public DateTime? Previous { get; set; }
    [Parameter] public bool IsTail { get; set; } */


    private bool _isEditing = false;
    public bool IsEditing { get { return _isEditing; } set { _isEditing = value; StateHasChanged(); } }

    protected TimeSpan Span { get; set; }

    protected string Head { get; set; }
    protected string Tail { get; set; }

    protected override void OnInitialized()
    {
        Span = Interval.End == null ? DateTime.Now - Interval.Start : Interval.End.Value - Interval.Start;
        Head = GetHead();
        Tail = GetTail();
    }

    protected async void HandleDelete()
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete interval {Interval.Start}?")) 
            return; 

        await _commands.DeleteIntervals(new List<int>() { Interval.Id });
    }
    protected string GetHead()
    {
        int? daysToEnd = Interval.End == null ? null : (int)Math.Abs((Interval.End.Value.Date - Filter.Earliest!.Value.Date).TotalDays);

        if (daysToEnd == null)
        {
            return "Now";
        }

        var endTime = Interval.End!.Value.ToString("HH:mm:ss");

        switch (daysToEnd)
        {
            case 0:
                return endTime;
            case 1:
                return "Following day at " + endTime;
            default:
                return Interval.End.Value.Date.ToString("d") + " at " + endTime;
        }
    }

    protected string GetTail()
    {
        int daysFromStart = (int)Math.Abs((Filter.Earliest!.Value.Date - Interval.Start.Date).TotalDays);
        var startTime = Interval.Start.ToString("HH:mm:ss");

        switch (daysFromStart)
        {
            case 0:
                return startTime;
            case 1:
                return "Previous day at " + startTime;
            default:
                return Interval.Start.Date.ToString("d") + " at " + startTime;
        }
    }
}
