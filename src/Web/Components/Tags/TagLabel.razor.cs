using Microsoft.AspNetCore.Components;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Web.Components.Tags;
public class TagLabelBase : ComponentBase
{
    [CascadingParameter] ConfigDTO config { get; set; }

    [Parameter, EditorRequired] public string Name { get; set; }
    [Parameter] public EventCallback<string> Remove { get; set; }
    [Parameter] public bool ReadOnly { get; set; } = true;
    [Parameter] public bool Small { get; set; } = false;

    protected string StyleString { get; set; } = "";

    protected override async Task OnParametersSetAsync() 
        => await Refresh();

    protected async Task Refresh()
    {
        var match = config.Colours.FirstOrDefault(c => new Regex(c.Regex).IsMatch(Name));

        if (match != null)
        {
            var highlightText = Style.GetFontColour(match.Colour);
            StyleString = $"--highlight: {match}; --highlightText: {ColorTranslator.ToHtml(highlightText)};";
        }

        await InvokeAsync(StateHasChanged);
    }
}
