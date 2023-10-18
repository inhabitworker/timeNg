namespace Shared.Services;

using Shared.Entity;
using Heatmap = Dictionary<DateTime, double>;

// virtualize for server query case

public class StatsService : IStatsService
{
    protected IQueryService _query;

    public StatsService(IQueryService query)
    { 
        _query = query;
    }

    public virtual async Task<Heatmap> Heatmap()
    {
        // ToDo: filter from config -> exclude tags, target/threshold, enabled by default 

        DateTime start = DateTime.Now.Date;
        DateTime end = DateTime.Now.Date.AddYears(-1);

        while (start.DayOfWeek != DayOfWeek.Saturday) start = start.AddDays(1);
        while (end.DayOfWeek != DayOfWeek.Sunday) end = end.AddDays(-1);

        // not capturing potential spillover outside of 1y for simplicity sake.

        IntervalQuery query = new IntervalQuery() { PageSize = 0 };
        var intervals = (await _query.Intervals(query)).Data;

        Dictionary<DateTime, int> heatmapSeconds = new();

        foreach (var interval in intervals)
        {
            if (interval.End == null) interval.End = DateTime.Now;

            // init
            if (!heatmapSeconds.ContainsKey(interval.Start.Date)) heatmapSeconds.Add(interval.Start.Date, 0);

            // handle common contained in single date intervals
            if (interval.End!.Value.Date == interval.Start.Date)
            {
                heatmapSeconds[interval.Start.Date] += (int)(interval.End!.Value - interval.Start).TotalSeconds;
            }
            // handle spread across dates intervals
            else
            {
                // walk thru days from interval start
                DateTime date = interval.Start.Date;

                // collect initial
                heatmapSeconds[interval.Start.Date] += (int)(interval.Start.Date.AddDays(1) - interval.Start).TotalSeconds;
                date = date.AddDays(1);

                // prolonged intervals spanning entire days... otherwise we're having overnighter skipped.
                while (date != interval.End.Value.Date)
                {
                    if (!heatmapSeconds.ContainsKey(date)) heatmapSeconds.Add(date, 0);
                    heatmapSeconds[date] += 24 * 60 * 60;
                    date = date.AddDays(1);
                }

                // suck up past midnight 
                if (!heatmapSeconds.ContainsKey(interval.End.Value.Date)) heatmapSeconds.Add(interval.End.Value.Date, 0);
                heatmapSeconds[interval.End.Value.Date] += (int)(interval.End.Value - date).TotalSeconds;
            }
        }

        // map to rate of 24h
        Heatmap heatmap = new();
        foreach (var pair in heatmapSeconds) heatmap.Add(pair.Key, pair.Value / (double)(24 * 60 * 60));

        return heatmap;
    }

    public virtual async Task<IEnumerable<TagDTO>> TopTags() => (await _query.Tags(new TagQuery() { PageSize = 10 })).Data;

    public virtual async Task<MiscStats> Misc() 
    {
        var intervals = (await _query.Intervals(new IntervalQuery() { PageSize = 0 })).Data;
        var tags = (await _query.Tags(new TagQuery() { PageSize = 0 })).Data;

        var longest = intervals  
            .OrderByDescending(i => (i.End ?? DateTime.Now) - i.Start).FirstOrDefault();

        return new()
        {
            TotalTags = tags.Count(),
            TotalIntervals = intervals.Count(),
            Oldest = intervals.OrderBy(i => i.Start).FirstOrDefault(),
            Longest = longest
        };
    }
}

