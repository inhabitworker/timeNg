using System.Drawing;
using System.Text.RegularExpressions;
using System.Text;
using Shared.Entity;

namespace Shared.Helpers;

public static class Style
{
    public static List<string> ThemeNames() => new List<string>() { "Light", "Dark", "Black" };

    public static Models.ITheme GetTheme(this ConfigDTO config) => GetTheme(config.Theme);

    public static Models.ITheme GetTheme(string theme)
    {
        switch (theme)
        {
            case "Dark":
                return new Models.DarkTheme();

            case "Black":
                return new Models.BlackTheme();

            case "Light":
                return new Models.LightTheme();

            default:
                return new Models.LightTheme();
        }
    }


    public static Dictionary<string, string> ToCss(this ConfigDTO config)
    {
        var theme = config.GetTheme();

        return new()
        {
            { "--highlight", config.Highlight },
            {  "--highlightText", ColorTranslator.ToHtml(Style.GetFontColour(ColorTranslator.FromHtml(config.Highlight))) },
            {  "--bg", ColorTranslator.ToHtml(theme.Background) },
            {  "--mg", ColorTranslator.ToHtml(theme.Shade) },
            {  "--fg", ColorTranslator.ToHtml(theme.Foreground) },
            {  "--positive", "hsl(124, 40%, 47%)" },
            {  "--negative", "hsl(0, 40%, 47%)" }
        };
    }

    public static byte[] ToCssAppend(this ConfigDTO config)
    {
        var encode = new UTF8Encoding();
        
        string themeString = string.Join("\n", config.ToCss().Select(kv => "\t" + kv.Key + ": " + kv.Value + ";"));
        string asString = ":root {\n" + themeString + "\n}\n";

        return encode.GetBytes(asString);
    }

    public static string? GetColour(this ConfigDTO config, string name)
    {
        var match = config.Colours.FirstOrDefault(c => new Regex(c.Regex).IsMatch(name));
        return match != null ? match.Colour : null;
    }

    // Colours (UI/Config)
    /*public static double RelativeLuminance(Color color)
    {
        var R = color.R/255 <= 0.03928 ? (color.R/255) / 12.92 : Math.Pow(((color.R/255) + 0.055) / 1.055, 2.4);
        var G = color.G/255 <= 0.03928 ? (color.G/255) / 12.92 : Math.Pow(((color.G/255) + 0.055) / 1.055, 2.4);
        var B = color.B/255 <= 0.03928 ? (color.B/255) / 12.92 : Math.Pow(((color.B/255) + 0.055) / 1.055, 2.4);

        var rel = (0.2126 * R) + (0.7152 * G) + (0.0722 * B);

        return rel;
    }

    public static double ContrastRatio(Color col1, Color col2)
    {
        // https://www.w3.org/TR/WCAG20/#contrast-ratiodef

        var rlum1 = RelativeLuminance(col1);
        var rlum2 = RelativeLuminance(col2);

        var ratio = (rlum1 + 0.05) / (rlum2 + 0.05);

        if(ratio < 1) ratio = Math.Pow(ratio, -1);

        return ratio;
    }

    public static Color GetFontColour(Color colour)
    {
        List<Color> colours = new()
        {
            Color.White,
            Color.Black
        };

        return colours.OrderByDescending(c => ContrastRatio(c, colour)).ElementAt(0);
    } */

    /// <summary>
    /// Get optimal colour for font, by providing background colour.
    /// </summary>
    /// <param name="colour"></param>
    /// <returns>White or black.</returns>
    public static Color GetFontColour(Color colour)
    {
        if (colour.GetBrightness() > 0.55) return Color.Black;
        return Color.White;
    }
    public static Color GetFontColour(string hex)
    {
        var colour = ColorTranslator.FromHtml(hex);
        return GetFontColour(colour);
    }

}

