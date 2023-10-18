namespace Shared.Interfaces;

/// <summary>
/// Service for querying various statistics about intervals/tags.
/// </summary>
public interface IStatsService
{
    /// <summary>
    /// Provides a dictionary of fp 0-1 rate of time spent per each day of the past year.
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<DateTime, double>> Heatmap();

    public Task<IEnumerable<TagDTO>> TopTags();

    public Task<MiscStats> Misc();
}
