using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Web.Components.Common.Base;

/// <summary>
/// Input an enumerable by concatenating stringified values with commas, escaping commas for accurate parsing.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EnumerableInputComponentBase<T> : InputBase<IEnumerable<T>>
{
    [DisallowNull] public ElementReference? Element { get; protected set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        // BindConverter.FormatValue(CurrentValue);
        builder.AddAttribute(3, "value", BindConverter.FormatValue(CurrentValueAsString));
        builder.AddAttribute(4, "onchange", EventCallback.Factory
            .CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        builder.AddAttribute(5, "hidden");
        builder.AddElementReferenceCapture(5, __inputReference => Element = __inputReference);
        builder.CloseElement();
    }

    protected string EscapeJoin(IEnumerable<T>? value) => string.Join(",", value
        .Select(s => StringifyValue(s).Replace(",", "#,"))) ?? "";

    protected List<T> EscapeSplit(string value) => Regex.Split(value, @"(?<!#),")
        .Where(s => !string.IsNullOrEmpty(s))
        .Select(s => DeserializeValue(s.Replace("#,", ","))).ToList();

    protected abstract string StringifyValue(T value);
    protected abstract T DeserializeValue(string input);

    protected override string FormatValueAsString(IEnumerable<T>? value)
    {
        if (value == null || value.Count() == 0) return "";

        return EscapeJoin(value);
    }

    protected override bool TryParseValueFromString(string? value, out IEnumerable<T> result, out string validationErrorMessage)
    {
        if (string.IsNullOrEmpty(value) || EscapeSplit(value).Count() < 1)
        {
            validationErrorMessage = "";
            result = new List<T>();
            return true;
        }

        validationErrorMessage = "";
        result = EscapeSplit(value);
        return true;
    }

    protected virtual void Add(T input)
    {
        var value = StringifyValue(input);

        if (value == "") return;

        var esc = value.Replace(",", "#,");

        if (CurrentValueAsString == null || CurrentValueAsString == "")
        {
            CurrentValueAsString = esc;
            return;
        }

        CurrentValueAsString = CurrentValueAsString + "," + esc;

        StateHasChanged();
    }

    protected virtual void Remove(T input)
    {
        var newValue = CurrentValue.ToList();
        newValue.Remove(input);
        CurrentValueAsString = EscapeJoin(newValue);
        StateHasChanged();
    }
}
