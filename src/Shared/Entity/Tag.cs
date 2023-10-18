using Microsoft.EntityFrameworkCore;
using Shared.Validation;
using System.ComponentModel.DataAnnotations;

namespace Shared.Entity;

/// <summary>
/// Tags unique string with associated intervals.
/// </summary>

[Index(nameof(Name), IsUnique = true)]
public class TagEntity : TrackedEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public ICollection<IntervalEntity> Intervals { get; set; }
}

/// <summary>
/// Tag data transfer 
/// </summary>
public class TagDTO
{
    public int Id { get; set; } = 0;

    [CleanString]
    public string Name { get; set; }

    public IEnumerable<int> Intervals { get; set; } = Enumerable.Empty<int>();

    public DateTime Updated { get; set; } = DateTime.Now;
}
