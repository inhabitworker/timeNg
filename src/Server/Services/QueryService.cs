using Microsoft.EntityFrameworkCore;

namespace Shared.Services;

/// <summary>
/// Repository of methods for querying from dbContext.
/// </summary>
public class QueryService : IQueryService
{
    private TimeNetDbContext _context;

    public QueryService(TimeNetDbContext context)
    {
        _context = context;
    }
    public async Task<QueryResponse<IntervalDTO>> Intervals(IntervalQuery query)
    {
        var res = _context.Intervals
            .Include(i => i.Tags)
            .AsNoTracking()
            .OrderByDescending(i => i.Start)
            .Where(i => query.Exclude.Count() == 0 || !query.Exclude.Contains(i.Id))
            .Where(i => query.IncludeActive == true || i.End != null)
            .Where(i => query.Earliest == null ||
                DateTime.Compare(i.End ?? DateTime.Now, query.Earliest.Value) > 0 ||
                DateTime.Compare(i.Start, query.Earliest.Value) > 0)
            .Where(i => query.Latest == null ||
                DateTime.Compare(i.Start, query.Latest.Value) < 0 ||
                DateTime.Compare(i.End ?? DateTime.Now, query.Latest.Value) < 0)
            .Where(i => query.Tags == null || query.Tags.Count() == 0 || i.Tags.Any(t => query.Tags.Contains(t.Name)))
            .Where(i => query.IsAnnotated == null || (i.Annotation != null && query.IsAnnotated == true) || (i.Annotation == null && query.IsAnnotated == false))
            .AsQueryable();

        var total = res.Count();
        var dto = res.Select(i => i.ToDTO());

        if (query.PageSize != 0) dto = dto.Skip(query.Page * query.PageSize).Take(query.PageSize);

        return new QueryResponse<IntervalDTO>()
        {
            Data = dto,
            Page = query.Page,
            PageSize = query.PageSize,
            Total = total
        };
    }

    public Task<IntervalDTO?> Latest() 
        => _context.Intervals.OrderByDescending(i => i.Start).Select(i => i.ToDTO()).FirstOrDefaultAsync();
    public Task<IntervalDTO?> Next(DateTime time)
        => _context.Intervals.AsNoTracking().OrderByDescending(i => i.Start).Where(i => i.Start > time).Select(i => i.ToDTO()).LastOrDefaultAsync();
    public Task<IntervalDTO?> Previous(DateTime time)
        => _context.Intervals.AsNoTracking().OrderByDescending(i => i.Start).Where(i => i.Start < time).Select(i => i.ToDTO()).LastOrDefaultAsync();


    public async Task<QueryResponse<TagDTO>> Tags(TagQuery query)
    {
        var res = _context.Tags
            .Include(t => t.Intervals)
            .OrderByDescending(t => t.Intervals.Count())
            .Where(t => (query.Name == null || query.Name == "") || t.Name.ToLower().Contains(query.Name.ToLower()))
            .Where(t => (query.Exclude == null || !query.Exclude.Contains(t.Name)));

        var total = res.Count();
        var dto = res.Select(i => i.ToDTO());

        if (query.PageSize != 0) dto = dto.Skip(query.Page * query.PageSize).Take(query.PageSize);

        return new QueryResponse<TagDTO>()
        {
            Data = dto,
            Page = query.Page,
            PageSize = query.PageSize,
            Total = total
        };
    }

    public async Task<ConfigDTO> Config()
    {
        var config = await _context.Config.AsNoTracking().Include(c => c.Colours).FirstOrDefaultAsync();
        if (config == null) return new();
        return config.ToDTO();
    }
}
