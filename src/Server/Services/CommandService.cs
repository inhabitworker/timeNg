using Microsoft.EntityFrameworkCore;
using Shared.Validation;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Server.Services;

public class CommandService : ICommandService 
{
    private TimeNetDbContext _context;
    private IQueryService _query;

    public CommandService(TimeNetDbContext context, IQueryService query) 
    {
        _context = context;
        _query = query;
    }

    public async Task ApplyInterval(IntervalDTO input)
    {
        await Validators.DisjointInterval(_query, input);

        var existingTags = _context.Tags.AsNoTracking().Where(t => input.Tags.Contains(t.Name));

        var newTags = input.Tags.Where(t => !existingTags.Select(te => te.Name).Contains(t))
            .Select(t => new TagEntity()
            {
                Name = t
            }).ToList();

        EntityEntry<IntervalEntity> res;
        if (input.Id == 0)
        {
            // default id value is passed over in ef?
            // default int type value 0.
            res = _context.Intervals.Add(new IntervalEntity
            {
                Id = 0,
                Start = input.Start,
                End = input.End,
                Tags = existingTags.ToList(),
                Annotation = input.Annotation 
            });

            newTags.ForEach(res.Entity.Tags.Add);
        }
        else
        {
            var existing = _context.Intervals
                .Include(i => i.Tags)
                .Single(i => i.Id == input.Id);

            res = _context.Intervals.Attach(new IntervalEntity
            {
                Id = input.Id,
                Start = input.Start,
                End = input.End,
                Tags = existingTags.ToList(),
                Annotation = input.Annotation 
            });

            newTags.ToList().ForEach(res.Entity.Tags.Add);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ImportIntervals(IEnumerable<IntervalDTO> data)
    {
       // insert intervals with no tag references
        _context.Intervals.AttachRange(data.Select(i => new IntervalEntity
        {
            Start = i.Start,
            Tags = new List<TagEntity>(),
            End = i.End,
            Annotation = i.Annotation
        }));

        // inversion of intervals list to map to tag entity list
        var tags = data
            .Where(i => i.Tags != null)
            .SelectMany(i => i.Tags.Select(t => new { i.Start, t }))
            .GroupBy(it => it.t)
            .Select(it => new TagEntity
            {
                Name = it.Key,
                Intervals = _context.Intervals
                    .Local
                    .Select(i => i)
                    .Where(i => it.Select(it => it.Start).ToList().Contains(i.Start))
                    .ToList()
            });

        _context.Tags.AttachRange(tags);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteIntervals(IEnumerable<int> ids)
    {
        if (ids.Count() == 1)
        {
            var interval = _context.Intervals.Single(i => i.Id == ids.ToList()[0]);
            _context.Remove(interval);
        }
        else
        {
            var intervals = _context.Intervals.Where(i => ids.Contains(i.Id));
            _context.RemoveRange(intervals);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ApplyTag(TagDTO tag)
    {
        var existing = _context.Tags.FirstOrDefault(t => t.Id == tag.Id);
        if (existing == null) return;

        existing.Name = tag.Name;
        await _context.SaveChangesAsync();
    }
    public async Task DeleteTags(IEnumerable<int> ids) 
        => _context.Tags.RemoveRange(_context.Tags.Where(t => ids.Contains(t.Id)));

    public async Task SetConfig(ConfigDTO config)
    {
        ConfigEntity? current = await _context.Config.FirstOrDefaultAsync(c => c.Id == 1);

        if (current == null)
        {
            _context.Config.Add(config.ToEntity());
        }
        else
        {
            _context.Colours.RemoveRange(_context.Colours.ToList());

            var newEnt = config.ToEntity();

            current.Colours = config.Colours.ToList();
            current.Theme = config.Theme;
            current.Highlight = config.Highlight;
        }

        await _context.SaveChangesAsync();
    }
}
