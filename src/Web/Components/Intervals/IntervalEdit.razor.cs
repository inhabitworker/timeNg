using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Intervals;

public class IntervalEditBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public ICommandService _commands { get; set; }

    [Parameter] public IntervalDTO? Interval { get; set; }

    [Parameter] public DateTime? EndLimit { get; set; }
    [Parameter] public DateTime? StartLimit { get; set; }

    [Parameter] public EventCallback Close { get; set; }

    private bool NoSubmit { get; set; } = false;

    protected override void OnParametersSet()
    {
        if (EndLimit == null) EndLimit = DateTime.MinValue; // this might be a mistake.
        if (StartLimit == null) StartLimit = DateTime.Now;

        if (Interval == null)
        {
            Interval = new()
            {
                Id = 0,
                Start = StartLimit.Value.AddSeconds(1),
                End = EndLimit.Value.AddSeconds(-1)
            };
        }
    }

    protected async Task HandleValidSubmit()
    {
        if (!NoSubmit) await _commands.ApplyInterval(Interval!);
        await Close.InvokeAsync();
    }

    protected async Task HandleDelete()
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete interval ID {Interval!.Id}?"))
            return;

        NoSubmit = true;
        await _commands.DeleteIntervals(new List<int> { Interval.Id });
        await Close.InvokeAsync();
    }

    protected async Task HandleCancel()
    {
        NoSubmit = true;
        await Close.InvokeAsync();
    }

    protected string FormatDate(DateTime? input)
    {
        if (input != null) return $"{input.Value.ToString("yyyy-MM-dd")}T{input.Value.ToString("HH:mm")}";
        return "";
    }

}
