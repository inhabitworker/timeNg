using Microsoft.AspNetCore.Components;
using Web.Components.Common.Base;

namespace Web.Components.Common.Input;

// key = regex, value = colour
// simple encode known length colourhex + key/regex
public class InputColourMatchesBase : EnumerableInputComponentBase<ColourMatch>
{
    [CascadingParameter] public ConfigDTO config { get; set; }
    protected override string StringifyValue(ColourMatch input)
    {
        return input.Colour + input.Regex;
    }

    protected override ColourMatch DeserializeValue(string input)
    {
        // guaranteed value length means good time.
        var col = input.Substring(0, 7);
        var reg = input.Substring(7);

        return new() { Colour = col, Regex = reg };
    }

    protected System.Timers.Timer? Debounce { get; set; }
    protected List<ColourMatch> _mutable { get; set; }

    protected void Update()
    {
        //debouncer?
        CurrentValueAsString = FormatValueAsString(_mutable);
        _mutable = CurrentValue.ToList();
        StateHasChanged();
    }

    protected void Mutate() 
    {
        if(Debounce != null)
        {
            DisposeTimer();
            Debounce = new System.Timers.Timer(300);
            Debounce.Elapsed += OnElapsed;
            Debounce.Enabled = true;
            Debounce.Start();
        }
    }

    protected async void OnElapsed(object? sender, EventArgs e)
    {
        DisposeTimer();
        Update();
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

    // really might be helpful to have specialized case of suggesting tags, and auto add escaped regex string.
}
