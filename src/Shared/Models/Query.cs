using Microsoft.AspNetCore.Http.Extensions;
using Shared.Validation;

namespace Shared.Models;

public class Query
{
    /// <summary>
    /// Skip n*Limit results, paging. 
    /// </summary>
    public int Page { get; set; } = 0;

    /// <summary>
    /// Limit number of results returned. 0 = unlimited.
    /// </summary>
    public int PageSize { get; set; } = 20;

    public override string ToString()
    {
        var query = new QueryBuilder
        {
            { "Page", Page.ToString() },
            {"PageSize", PageSize.ToString() },
        };

        return query.ToString();
    }
}

/// <summary>
/// Interval querying filters for use when fetching.
/// </summary>
public class IntervalQuery : Query
{
    /// <summary>
    /// Include potential active interval which has null end time.
    /// </summary>
    public bool IncludeActive { get; set; } = false;

    /// <summary>
    /// Earliest start time and interval can have. 
    /// </summary>
    public DateTime? Earliest { get; set; } = null;

    // Might be interesting to return virtual "partial interval" truncated

    /// <summary>
    /// Latest start time an interval can have. Intervals with an end  
    /// time after this limit will be included, if start is in range.
    /// </summary>
    public DateTime? Latest { get; set; } = null;

    /// <summary>
    /// Intervals must have a tag that matches these queries.
    /// </summary>
    [Tags]
    public IEnumerable<string>? Tags { get; set; }

    /// <summary>
    /// Return only intervals with annotations, or not.
    /// </summary>
    public bool? IsAnnotated { get; set; } = null;

    /// <summary>
    /// Exclude intervals from result by id.
    /// </summary>
    public IEnumerable<int> Exclude { get; set; } = Enumerable.Empty<int>();

    /// <summary>
    /// Earliest time an interval was updated.
    /// </summary>
    public DateTime Updated { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Converts to encoded query string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var query = new QueryBuilder
        {
            { "IncludeActive", IncludeActive == false ? "false" : "true" },
            { "Page", Page.ToString() },
            { "Tags", Tags.ToString() }
        };

        if (Earliest != null) query.Add("Earliest", Earliest.Value.ToString());
        if (Latest != null) query.Add("Latest", Latest.Value.ToString());
        if (PageSize != 0) query.Add("Lmit", PageSize.ToString());
        if (IsAnnotated != null) query.Add("Annotated", IsAnnotated == false ? "false" : "true" );

        return query.ToString();
    }
}


/// <summary>
/// Query to use for filtering tags.
/// </summary>
public class TagQuery : Query
{
    /// <summary>
    /// Tag must match, case insensitive.
    /// </summary>
    [CleanString]
    public string? Name { get; set; } = null;

    // public int? Frequency { get; set; } = null;

    /// <summary>
    /// Exclude tags matching this input value. 
    /// </summary>
    public IEnumerable<string>? Exclude { get; set; } = null;


    /// <summary>
    /// Encode to URL query string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var query = new QueryBuilder
        {
            { "Page", Page.ToString() }
        };

        if (Name != null) query.Add("Name", Name);
        if (PageSize != 0) query.Add("Limit", PageSize.ToString());
        if (Exclude != null) query.Add("Exclude", Exclude);

        return query.ToString();
    }
}

