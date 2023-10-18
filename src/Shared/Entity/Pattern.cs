using Shared.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Shared.Entity;

[Index(nameof(Weight))]
public class PatternEntity : TrackedEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [RegexPattern]
    public string Pattern { get; set; }

    public int Weight { get; set; } = 0;

    [ColourHex]
    public string? Colour { get; set; } = null;

    public string? Name { get; set; } = null;
}

public class PatternDTO 
{
    public int Id { get; set; }

    [Required]
    [RegexPattern]
    public string Pattern { get; set; }

    public int Weight { get; set; } = 0;

    [ColourHex]
    public string? Colour { get; set; } = null;

    public string? Name { get; set; } = null;

    public DateTime Updated { get; set; } = DateTime.Now;
}


