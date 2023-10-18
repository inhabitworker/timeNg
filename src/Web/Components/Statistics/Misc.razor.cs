using Microsoft.AspNetCore.Components;

namespace Web.Components.Statistics;

public class MiscBase : ComponentBase
{
    [Parameter] public MiscStats? Data { get; set; } = null;

}
