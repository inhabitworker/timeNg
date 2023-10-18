namespace Web.Services;

public class QueryService : IQueryService
{
    private StatusService _status;
    public QueryService(StatusService status)
    {
        _status = status;
    }

    public Task<QueryResponse<IntervalDTO>> Intervals(IntervalQuery intervalFilter)
        => throw new NotImplementedException(); 

    public Task<IntervalDTO?> Latest()
        => throw new NotImplementedException(); 
    public Task<IntervalDTO?> Next(DateTime time)
        => throw new NotImplementedException(); 

    public Task<IntervalDTO?> Previous(DateTime time)
        => throw new NotImplementedException(); 

    
    public Task<QueryResponse<TagDTO>> Tags(TagQuery tagFilter)
        => throw new NotImplementedException(); 


    public Task<ConfigDTO> Config()
        => throw new NotImplementedException(); 


    /*
     public async Task GetConfigJs()
        {
            // alter to fetch? no?
            var config = await GetConfig();
            await _jsRuntime.InvokeVoidAsync("getCss", config.ToCss());
        }
     */

}
