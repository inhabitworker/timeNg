using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Web.Components.Common.Base;

namespace Web.Components.Common.Input;

public class InputTagsBase : EnumerableInputComponentBase<string>
{
    [Inject] public IQueryService _query { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Parameter] public bool Small { get; set; } = false;
    protected override string StringifyValue(string value) => value;
    protected override string DeserializeValue(string input) => input;

    // Interact + Suggestions
    protected bool IsAdding { get; set; }
    protected string inputText { get; set; } = "";
    protected ElementReference inputField;

    protected List<string> SuggestedTags { get; set; } = new();
    protected System.Timers.Timer? Debounce = null;

    protected async override Task OnAfterRenderAsync(bool first)
    {
        if (IsAdding)
        {
            await inputField.FocusAsync();
            await JSRuntime.InvokeVoidAsync("disableSubmit", "tagInput");
        }
    }

    protected override void Add(string tag)
    {
        CancelAdding();
        base.Add(tag);
    }

    protected void BeginAdding()
    {
        IsAdding = true;
        UpdateSuggestions();
        StateHasChanged();
    }

    protected void CancelAdding()
    {
        IsAdding = false;
        inputText = "";
        SuggestedTags.Clear();
        StateHasChanged();
    }

    protected void OnKeyUp(KeyboardEventArgs k) 
    {
        if (k.Key == "Escape") CancelAdding();
        if (k.Key == "Enter") Add(inputText);

        DisposeTimer();
        Debounce = new System.Timers.Timer(300);
        Debounce.Elapsed += OnElapsed;
        Debounce.Enabled = true;
        Debounce.Start();
    }

    protected async void OnElapsed(object? sender, EventArgs e)
    {
        DisposeTimer();
        await UpdateSuggestions();
    }

    protected void DisposeTimer()
    {
        if (Debounce != null)
        {
            Debounce.Enabled = false;
            Debounce.Elapsed -= OnElapsed;
            Debounce.Dispose();
            Debounce = null;
        }
    }

    protected async Task UpdateSuggestions()
    {

        TagQuery query = new();
        if (inputText != null && inputText != "") query.Name = inputText;
        if (CurrentValue != null && CurrentValue.Count() != 0) query.Exclude = CurrentValue.ToList();

        var tags = await _query.Tags(query);
        SuggestedTags = tags.Data.Select(t => t.Name).ToList();

        await InvokeAsync(StateHasChanged);
    }

}
