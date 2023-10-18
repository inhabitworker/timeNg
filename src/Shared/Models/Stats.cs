using Shared.Entity;

namespace Shared.Models;

public record MiscStats
{
    public int TotalTags { get; set; } = 0;
    public int TotalIntervals { get; set; } = 0;
    public IntervalDTO? Oldest { get; set; }
    public IntervalDTO? Longest { get; set; }
}

