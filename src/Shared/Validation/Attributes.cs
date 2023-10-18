using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Shared.Entity;

namespace Shared.Validation;

/// <summary>
/// Ensure a string is compatible with binary, whether tag or annotation content.
/// </summary>
public class CleanString : ValidationAttribute
{
    public Regex Expression = new Regex("['\"#]+");
    public string GetErrorMessage() => "Tags/annotations should not include \', \" or #.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var input = (string)value;
        
        if (input == null || input == "") return ValidationResult.Success;
        if (Expression.IsMatch(input)) return new ValidationResult(GetErrorMessage());

        return ValidationResult.Success;
    }
}

/// <summary>
/// Calculate validity of a list of strings, namely for tag values.
/// </summary>
public class Tags : CleanString
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var input = (List<string>)value!;
        if (input == null || input.Count == 0) return ValidationResult.Success;

        foreach (var singleInput in input)
        {
            if (singleInput == null || singleInput == "") return new ValidationResult("Tag cannot be empty.");
            var result = base.IsValid(singleInput, validationContext);
            if (result != ValidationResult.Success) return result;
        }

        if (input.Count != input.Distinct().Count()) return new ValidationResult("Duplicate tags.");

        return ValidationResult.Success;
    }
}

/// <summary>
/// Ensure theme exists.
/// </summary>
public class IsTheme : ValidationAttribute
{
    public string GetErrorMessage() => "Include a theme option: 'Dark', 'Light' or 'Black'.";

    protected override ValidationResult? IsValid(
        object? value, ValidationContext validationContext)
    {
        var input = (string)value!;
        if (input == null) return new ValidationResult(GetErrorMessage());
        
        if (Style.ThemeNames().Contains(input)) return ValidationResult.Success;

        return new ValidationResult(GetErrorMessage());
    }
}

/// <summary>
/// Ensure a string is a colour hex.
/// </summary>
public class ColourHex : ValidationAttribute
{
    public string GetErrorMessage() => "Format should be a colour hex number.";
    public Regex Expression = new Regex(@"#[A-F0-9^g-zG-Z]{6,6}", RegexOptions.IgnoreCase );

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var code = (string)value!;

        if (code == "" || code == null) return ValidationResult.Success;

        if (Expression.IsMatch(code)) return ValidationResult.Success;

        return new ValidationResult(GetErrorMessage());
    }
}

/// <summary>
/// Ensure regex facility can successfully interperet given string as usable pattern.
/// </summary>
public class RegexPattern : ValidationAttribute
{
    public string GetErrorMessage() => "Must be valid regex string.";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var input = (string)value!;

        if (input == "") return new ValidationResult("Empty string/whitespace not valid.");

        try
        {
            var regex = new Regex(input);
        }
        catch
        {
            return new ValidationResult("Must be a valid regular expression.");
        }

        return ValidationResult.Success;
    }
}

public class ColourMatches : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var matches = (IEnumerable<ColourMatch>)value!;
        var regexes = matches.Select(m => m.Regex);
        
        if (matches.Count() == 0) return ValidationResult.Success;
        if (regexes.Count() != regexes.Distinct().Count()) return new ValidationResult("Duplicate regex strings are not allowed.");

        return ValidationResult.Success;
    }
}

/// <summary>
/// Partial quick validation of interval. Further checking against data is required of course.
/// </summary>
public class IntervalPartialValidation : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var interval = (IntervalDTO)value!;

        if (interval.End != null && DateTime.Compare(interval.Start, interval.End.Value) >= 0) 
            return new ValidationResult("Start time cannot be equal to or later than accompanying end.");

        if ((interval.End != null && DateTime.Compare(interval.End.Value, DateTime.Now) >= 0)
            || DateTime.Compare(interval.Start, DateTime.Now) >=0
                ) return new ValidationResult("Cannot use times more recent than now.");

        return ValidationResult.Success;
    }
}



