using Shared.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.Entity;

/// <summary>
/// Interval class as both entity for efCore with various attribute, maintaining "Updated" time (TrackedEntity),
/// and as data transfer object, we've nothing to hide or fat to trim.
/// </summary>
[Index(nameof(Start), IsUnique = true, IsDescending = new[] { true })]
public class IntervalEntity : TrackedEntity
{
    [Key]
    public int Id { get; set; } = 0;

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime? End { get; set; } = null;

    [CleanString]
    public string? Annotation { get; set; } = null;

    public ICollection<TagEntity> Tags { get; set; }
}

[IntervalPartialValidation]
public class IntervalDTO
{
    public int Id { get; set; } = 0;

    public DateTime Start { get; set; }

    public DateTime? End { get; set; } = null;

    [CleanString]
    public string? Annotation { get; set; } = null;

    [Tags]
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    public DateTime Updated { get; set; } = DateTime.Now;
}

public class IntervalSelectable : IntervalDTO
{
    public bool IsSelected { get; set; } = false;
}


