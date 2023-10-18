using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Shared.Entity;

namespace Web.Components.Overlay;

// active service?
public class ActiveBase : ComponentBase
{
    [Inject] public IQueryService _query { get; set; }
    [Inject] public ICommandService _commands { get; set; }
    [Inject] public IJSRuntime _jsRuntime { get; set; }

    public bool IsActive { get; set; } = false;
    public IntervalDTO Active { get; set; } = new IntervalDTO() { Id = 0, Start = DateTime.Now };
    public IntervalDTO? Latest { get; set; } = null;


    protected EditContext? editContext;
    protected string _elapsedString { get; set; } =  "-- : -- : --";

    protected override async Task OnInitializedAsync()
    {
        await Refresh();
        editContext = new(Active);
        editContext.OnFieldChanged += Update;
    }

    protected async override Task OnParametersSetAsync()
    {
        base.OnParametersSet();

        if(IsActive) 
        {
            var span = (DateTime.Now - Active.Start);
            int h = (int)span.TotalHours;
            int m = (int)span.TotalMinutes - (int)span.TotalHours * 60;
            int s = (int)span.TotalSeconds - ((int)span.TotalMinutes * 60 );

            _elapsedString = $"{Leading(h)}:{Leading(m)}:{Leading(s)}";
        }
    }

    protected async override Task OnAfterRenderAsync(bool first)
    {
        if (IsActive) await _jsRuntime.InvokeVoidAsync("elapsed", Active.Start.ToString("O"));
        await base.OnAfterRenderAsync(first);
    }

    public async Task Refresh()
    {
        var interval = await _query.Latest();
        
        if (interval != null)
        {
            if(interval.End == null)
            {
                IsActive = true;
                Active = interval;
            }
            else
            {
                Latest = interval;
            }
        }
    }

    public async Task Start() 
    {
        if(editContext.GetValidationMessages().Count() == 0)
        {
            Active.Start = DateTime.Now;
            await _commands.ApplyInterval(Active);
        }
    }

    public async void Update(object? sender, FieldChangedEventArgs e)
    {
        editContext.Validate();

        if (IsActive && editContext.GetValidationMessages().Count() == 0 && editContext.IsModified())
            await _commands.ApplyInterval(Active);
    }

    public async Task Continue() =>
        await _commands.ApplyInterval(new IntervalDTO() { Start = DateTime.Now, Tags = Latest.Tags });

    public async Task Stop() {
        if (editContext.GetValidationMessages().Count() == 0)
        {
            Active.End = DateTime.Now;
            await _commands.ApplyInterval(Active);
        }
    }

    public async Task Cancel() => await _commands.DeleteIntervals(new List<int> { Active.Id });

    private string Leading(int input) => ((double)(input / 10) < 1 ? $"0{input}" : input.ToString());

    public void Dispose()
    {
        editContext.OnFieldChanged -= Update;
    }
}
