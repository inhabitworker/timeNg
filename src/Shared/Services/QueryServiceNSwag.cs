namespace Shared.Services;

public class QueryServiceNSwag : IQueryService
{
    protected IIntervalsClient _intervalsClient;
    protected ITagsClient _tagsClient;
    protected IConfigClient _configClient;

    public QueryServiceNSwag(IIntervalsClient intervalsClient, ITagsClient tagsClient, IConfigClient configClient)
    {
        _intervalsClient = intervalsClient;
        _tagsClient = tagsClient;
        _configClient = configClient;
    }

    public async Task<QueryResponse<IntervalDTO>> Intervals(IntervalQuery query)
    {
        bool IncludeActive = query.IncludeActive;
        DateTime? Earliest = query.Earliest;
        DateTime? Latest = query.Latest;
        int PageSize = query.PageSize;
        int Page = query.Page;
        IEnumerable<string>? Tags = query.Tags;
        bool? IsAnnotated = query.IsAnnotated;
        IEnumerable<int> Exclude = query.Exclude;
        DateTime Updated = query.Updated;
        return await _intervalsClient
            .GetIntervalsByQueryAsync(IncludeActive, Earliest, Latest, Tags, IsAnnotated, Exclude, Updated, Page, PageSize);
    }

    // NSwag generated code throws an exception on status 204.

    public async Task<IntervalDTO?> Latest()
    {
        try
        {
            return await _intervalsClient.GetLatestAsync();
        }
        catch(ApiException)
        {
            return null; 
        }
    }

    public async Task<IntervalDTO?> Next(DateTime time)
    {
        try
        {
            return await _intervalsClient.GetNextAsync(time);
        }
        catch(ApiException)
        {
            return null; 
        }
    }

    public async Task<IntervalDTO?> Previous(DateTime time)
    {
        try
        {
            return await _intervalsClient.GetPreviousAsync(time);
        }
        catch(ApiException) 
        {
            return null;
        }
    }

    public async Task<QueryResponse<TagDTO>> Tags(TagQuery tagFilter)
    {
        string? Name = tagFilter.Name;
        int PageSize = tagFilter.PageSize;
        int Page = tagFilter.Page;
        IEnumerable<string>? Exclude = tagFilter.Exclude;

        return await _tagsClient.GetTagsByQueryAsync(Name, Exclude, Page, PageSize);
    }

    public async Task<ConfigDTO> Config()
        => await _configClient.GetConfigAsync();
}

