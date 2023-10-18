using System.ComponentModel.DataAnnotations;

namespace Shared.Entity;

/// <summary>
/// Entities that require persisted tracking can inherit this.
/// </summary>
public class TrackedEntity
{
    [ScaffoldColumn(false)]
    public DateTime Updated { get; set; } = DateTime.Now;
}
