using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Components.Tags;

public class TagCardBase : ComponentBase
{
    [CascadingParameter] public bool ReadOnly { get; set; } 
    [CascadingParameter] public ConfigDTO _config { get; set; }
    [Inject] public IJSRuntime jsRuntime { get; set; }
    [Inject] public ICommandService _command { get; set; }

    [Parameter, EditorRequired] public TagDTO Tag { get; set; }
    public string newName { get; set; }

    protected string? Colour { get; set; }

    public bool _isEditing { get; set; } = false;
    public bool IsEditing { get { return _isEditing; } set { _isEditing = value; StateHasChanged(); } }


    protected async override Task OnParametersSetAsync()
    {
        newName = Tag.Name; 
        Colour = _config.GetColour(Tag.Name);
    }

    protected async Task HandleDelete()
    {
        if (!await jsRuntime.InvokeAsync<bool>("confirm", $"Are you sure you want to delete tag '{Tag.Name}'?"))
            return; 

        await _command.DeleteTags(new List<int> { Tag.Id });
    }


    public async Task TrySubmit()
    {
        if(!newName.Contains('"')) 
        {
            var updated = Tag;
            updated.Name = newName;
            await _command.ApplyTag(updated);
            IsEditing = false;
        }
    }

}
