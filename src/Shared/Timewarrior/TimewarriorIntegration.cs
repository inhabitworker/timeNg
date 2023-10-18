using Shared.Models;
using Shared.Data;
using Microsoft.EntityFrameworkCore;
using KellermanSoftware.CompareNetObjects;
using Shared.Helpers;

namespace Shared.Timewarrior;

public class Integration
{
    private ILogger<Integration> _logger;
    private TimewarriorService _timew;
    private TimeDbContext _context;

    public Integration(ILogger<Integration> logger, TimeDbContext context, TimewarriorService timew)
    {
        _logger = logger;
        _timew = timew;
        _context = context;
    }

    public async Task<IntegrationPayload> Check()
    {
        _logger.LogInformation($"Checking database against binary...");

        var payloadTask = new TaskCompletionSource<IntegrationPayload>();
        var payload = new IntegrationPayload();

        // map db and export to same type (marker class used by export)
        // ...as such, avoid id
        var timewIntervals = await _timew.Export();
        IQueryable<Interval> localIntervalsUnfolded = _context.Intervals
            .AsNoTracking()
            .Select(i => new Interval
            {
                // Id = Common.TimewId(_context, i.Id) ? ,
                Id = i.Id,
                Start = i.Start,
                End = i.End ?? null,
                Tags = i.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
                Annotation = i.Annotation,
            });

        Dictionary<string, Interval> dbIntervals = await localIntervalsUnfolded
            .ToDictionaryAsync(i => i.Start.ToString());

        // store checked to help identify orphans
        var found = new List<DateTime>();

        // first treat conflicting open interval
        var hactive = timewIntervals.FirstOrDefault(i => i.End == null);
        var lactive = localIntervalsUnfolded.FirstOrDefault(v => v.End == null);

        if (hactive != null && lactive != null)
        {
            if (hactive.Start != lactive.Start)
            {
                _logger.LogInformation("Non-matching open intervals, dropping local.");
                var suspect = _context.Intervals.FirstOrDefault(i => i.Id == lactive.Id);

                if (suspect == null) throw new Exception();
                // context.Intervals.Remove(suspect);
            }
        }

        // loop through all exported (host) intervals, checking against the
        // local/db versions (or lack thereof) and make changes according to
        // results of comparisons

        // limit number of conflicts before breaking out of long process?

        foreach (var x in timewIntervals)
        {
            if (dbIntervals.ContainsKey(x.Start.ToString()))
            {
                var y = dbIntervals[x.Start.ToString()];
                var results = Compare((Interval)x, y);

                if (!results.AreEqual)
                {
                    _logger.LogInformation($"Found differences at {y.Start}: {results.DifferencesString}");
                    payload.Conflicting.Add(new() { Timewarrior = x, Local = y });
                }
                else
                {
                    payload.Valid++;
                }

                found.Add(x.Start);
            }
            else
            {
                _logger.LogWarning($"Missing interval locally at {x.Start}.");

                payload.MissingLocal.Add((Interval)x);
                found.Add(x.Start);
            }
        }

        // consult with list of ids that went unmatched and remove
        var timewMissing = _context.Intervals
            .AsNoTracking()
            .Select(i => i.IntervalToDTO())
            .Where(i => !found.Contains(i.Start))
            .ToList();

        if (timewMissing.Count > 0)
        {
            _logger.LogInformation($"Discovered intervals missing in Timewarrior.");
            payload.MissingTimewarrior.AddRange(timewMissing);
        }

        payloadTask.SetResult(payload);

        return await payloadTask.Task;
    }

    private ComparisonResult Compare(Interval x, Interval y)
    {
        // be wary of DateTimeKind, will trip up comparison
        CompareLogic logic = new CompareLogic();

        logic.Config.MembersToIgnore = new List<string> { "Id" };
        logic.Config.DateTimeKindToUseWhenUnspecified = DateTimeKind.Utc;

        var results = logic.Compare(x, y);
        return results;
    }

    private void Amend(IntegrationPayload input)
    {
        throw new NotImplementedException();
    }
}


public record IntegrationPayload 
{
    public int Total { get; set; } = 0;
    public int Valid { get; set; } = 0;

    // straight up Update?
    public List<IntervalConflict> Conflicting { get; set; } = new();
    public List<Interval> MissingLocal { get; set; } = new();
    public List<Interval> MissingTimewarrior { get; set; } = new();

}

public record IntervalConflict
{
    public IInterval Timewarrior { get; set; }
    public IInterval Local { get; set; }
}
