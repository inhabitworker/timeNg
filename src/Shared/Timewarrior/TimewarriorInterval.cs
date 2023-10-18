using Shared.Models;

namespace Shared.Timewarrior;

/// <summary>
/// Generic interval implementation with minimal validation attribute. 
/// </summary>
public class IntervalTimewarrior : IInterval<string>
{
    public virtual int Id { get; set; } 
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Annotation { get; set; } = "";
}

internal class TimewarriorInterval
{
}
