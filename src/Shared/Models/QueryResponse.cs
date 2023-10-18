namespace Shared.Models;

public class QueryResponse<T>
{
    public IEnumerable<T> Data { get; set; }

    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public int Total { get; set; }

}

public class QueryResponseOfIntervalDTO : QueryResponse<IntervalDTO> { }
public class QueryResponseOfTagDTO : QueryResponse<TagDTO> { }

