using Shared.Validation;
using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Entity;

public class ConfigEntity : TrackedEntity
{
    [Key]
    public int Id { get; set; }

    public string Theme { get; set; } = "Light";

    public string Highlight { get; set; } = @"#215287";

    public ICollection<ColourMatch>? Colours { get; set; } 

    public DateTime Updated { get; set; } = DateTime.Now;
}

public class ConfigDTO : TrackedEntity
{
    [IsTheme]
    public string Theme { get; set; } = "Light";

    [ColourHex]
    public string Highlight { get; set; } = @"#215287";

    [ColourMatches]
    public IEnumerable<ColourMatch> Colours { get; set; } = Enumerable.Empty<ColourMatch>();

    public DateTime Updated { get; set; } = DateTime.Now;

}

public class ColourMatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [RegexPattern]
    public string Regex { get; set; }

    [ColourHex]
    public string Colour { get; set; }

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

