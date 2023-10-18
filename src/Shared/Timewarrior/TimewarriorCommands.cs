using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Models;
using Shared.Services;
using Shared.Timewarrior.Helper;

namespace Shared.Timewarrior;

/// <summary>
/// Command repository for when working with Timewarrior binary, implicitly including DB, applying to binary first.
/// </summary>
public class TimewarriorCommands : Commands
{
    private TimewarriorService _timew;

    public TimewarriorCommands(TimeDbContext context, TimewarriorService timew) : base(context)
    {
        _timew = timew;
    }

    public async override Task CreateInterval(IntervalNew input)
    {
        await _timew.Track(input.Start, input.End, input.Tags);
        if (input.Annotation != null) 
            await _timew.Annotate(TimewarriorHelper.TimewId(_context, input.Start), input.Annotation);

        await base.CreateInterval(input);
    }

    public override async Task UpdateInterval(IInterval input)
    {
        // delete (not calling repo method, to get id)
        var timewId = TimewarriorHelper.TimewId(_context, input.Id);
        await _timew.Delete(timewId);
        await _timew.Track(input.Start, input.End, input.Tags);
        if (input.Annotation != null) 
            await _timew.Annotate(TimewarriorHelper.TimewId(_context, input.Start), input.Annotation);

        await base.UpdateInterval(input);
    }

    public override async Task DeleteInterval(IEnumerable<int> ids)
    {
        if (ids.Count() == 1)
        {
            var timewId = TimewarriorHelper.TimewId(_context, ids.ToList()[0]);
            await _timew.Delete(timewId);
        }
        else
        {
            var timewIds = ids.Select(i => TimewarriorHelper.TimewId(_context, i));
            await _timew.Delete(timewIds);
        }

        await base.DeleteInterval(ids);
    }

    public override async Task RenameTag(TagDTO input)
    {
        var existing = _context.Tags
            .AsNoTracking()
            .Single(t => t.Id == input.Id);

        var timewIds = input.Intervals
            .Select(i => TimewarriorHelper.TimewId(_context, i))
            .ToList();

        await _timew.Untag(timewIds, existing.Name);
        await _timew.Tag(timewIds, input.Name);

        await base.RenameTag(input);
    }

    public override async Task DeleteTag(int id)
    {
        var tag = _context.Tags.AsNoTracking().Single(t => t.Id == id);
        var timewIds = tag.Intervals.Select(i => TimewarriorHelper.TimewId(_context, i.Id));
        await _timew.Untag(timewIds, tag.Name);

        await base.DeleteTag(id);
    }

}
