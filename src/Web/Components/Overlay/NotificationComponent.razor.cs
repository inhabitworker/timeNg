using Microsoft.AspNetCore.Components;

namespace Web.Components.Overlay;
public class NotificationComponentBase : ComponentBase, IDisposable
{
    [Parameter, EditorRequired] public Notification Value { get; set; }
    public string DecayStyle { get; set; } = "--remaining: 100%;";
    public string IconStyle { get; set; }

    protected override void OnInitialized()
    {
        IconStyle = "background-color: " + (Value.IsError ? "var(--negative)" : "var(--positive)") + ";";
        Value.OnTick += Decay;
    }

    protected async override Task OnAfterRenderAsync(bool first)
    {
        if (first)
        {
            // yeah so what.
            DecayStyle = "--remaining: 100%";
            await InvokeAsync(StateHasChanged);
            await Task.Delay(100);
            DecayStyle = GetDecay();
            await InvokeAsync(StateHasChanged);
        }
    }

    private void Decay()
    {
        DecayStyle = GetDecay();
        InvokeAsync(StateHasChanged);
    }

    private string GetDecay()
    {
        // look ahead to death
        if (Value.Remaining == 1) return "--remaining: 0%;";

        double remain = (double)(Value.Remaining - 1)/(double)Value.Duration;
        return "--remaining: " + Math.Round(100 * remain).ToString() + "%;";
    }

    public void Dispose()
    {
        Value.OnTick -= Decay;
    }
}

