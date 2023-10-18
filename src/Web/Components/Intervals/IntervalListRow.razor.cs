using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Intervals;

public class IntervalBase : ComponentBase 
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public ICommandService _commands { get; set; } 

    [CascadingParameter] protected bool ReadOnly { get; set; }
    [Parameter, EditorRequired] public IntervalSelectable Interval { get; set; }

    private bool _isEditing = false;
    public bool IsEditing { get { return _isEditing; } set { _isEditing = value; StateHasChanged(); } }

    protected TimeSpan Span { get; set; }

    protected override void OnParametersSet()
    {
        Span = Interval.End == null ? DateTime.Now - Interval.Start : Interval.End.Value - Interval.Start;
    }

    protected async void HandleDelete()
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete interval {Interval.Start}?")) 
            return; 

        await _commands.DeleteIntervals(new List<int>() { Interval.Id });
    }

}