using System.Drawing;

namespace Shared.Models;

public record Config
{
    public string Theme { get; set; } = "Light";
    public string Highlight { get; set; } = @"#215287";
    public bool Background { get; set; } = true;
}


public interface ITheme
{
    public Color Foreground { get; }
    public Color Shade { get; }
    public Color Background { get; }
}

public record BlackTheme : ITheme
{
    public Color Foreground { get; } = Color.White;
    public Color Shade { get; } = Color.Gray;
    public Color Background { get; } = Color.Black;
};

public record DarkTheme : ITheme
{
    public Color Foreground { get; } = Color.White;
    public Color Shade { get; } = Color.DimGray;
    public Color Background { get; } = ColorTranslator.FromHtml("#FF1E1E1E");
};

public record LightTheme : ITheme
{
    public Color Foreground { get; } = Color.Black;
    public Color Shade { get; } = Color.DimGray;
    public Color Background { get; } = Color.White;
};

