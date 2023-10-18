using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Intervals;

public class IntervalListBase : ComponentBase 
{
    [Inject] public ICommandService _command { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [CascadingParameter] protected bool ReadOnly { get; set; }

    [Parameter, EditorRequired] public List<IntervalDTO> Intervals { get; set; }
    public List<IntervalSelectable> SelectableIntervals { get; set; }

    protected override void OnParametersSet()
    {
        SelectableIntervals = Intervals.Select(i => i.ToSelectable()).ToList();
    }

    // Selection
    protected void SelectNone() 
        => SelectableIntervals!.Select(i => i.IsSelected = false);
    protected void SelectInverse()
        => SelectableIntervals!.Select(i => i.IsSelected = !i.IsSelected);
    protected async Task DeleteSelected()
    {
        // taking only visible in pagedCase for safe?
        var candidates = SelectableIntervals!.Where(i => i.IsSelected == true);

        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete {candidates.Count()} selected intervals?"))
            return; 

        await _command.DeleteIntervals(candidates.Select(i => i.Id));
    }

}