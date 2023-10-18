using Shared.Entity;

namespace Shared.Interfaces;

public interface IQueryService
{
    // Intervals
    // public Task<IntervalDTO?> Interval(int id);
    // public Task<IntervalDTO?> Interval(DateTime start);
    public Task<QueryResponse<IntervalDTO>> Intervals(IntervalQuery intervalFilter);

    // Exotic interval queries
    public Task<IntervalDTO?> Latest();
    public Task<IntervalDTO?> Next(DateTime time);
    public Task<IntervalDTO?> Previous(DateTime time);

    // Tags
    // public Task<List<TagDTO>> Tags(int id);
    // public Task<List<TagDTO>> Tag(string name);
    public Task<QueryResponse<TagDTO>> Tags(TagQuery tagFilter);

    // Config
    public Task<ConfigDTO> Config();
}
