using Microsoft.AspNetCore.Components;
using Shared.Entity;

namespace Web.Components.Statistics;
public class TopTagsBase : ComponentBase
{
    [CascadingParameter] public ConfigDTO config { get; set; } 
    [Parameter] public IEnumerable<TagDTO>? Data { get; set; } = null;

}
