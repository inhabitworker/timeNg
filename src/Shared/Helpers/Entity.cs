using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shared.Entity;

namespace Shared.Helpers;

public static class Entity
{
    public static TagDTO ToDTO(this TagEntity entity)
    {
        return new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Intervals = entity.Intervals.Select(i => i.Id).AsEnumerable(),
            Updated = entity.Updated
        };
    }

    public static IntervalDTO ToDTO(this IntervalEntity entity)
    {
        return new()
        {
            Id = entity.Id,
            Start = entity.Start,
            End = entity.End,
            Tags = entity.Tags?.Select(t => t.Name).AsEnumerable() ?? Enumerable.Empty<string>(),
            Updated = entity.Updated
        };
    }

    public static IntervalSelectable ToSelectable(this IntervalDTO dto)
    {
        return new()
        {
            Id = dto.Id,
            Start = dto.Start,
            End = dto.End,
            Tags = dto.Tags,
            Updated = dto.Updated,
            IsSelected = false
        };
    }

    public static ConfigEntity ToEntity(this ConfigDTO dto)
    {
        return new()
        {
            Theme = dto.Theme,
            Highlight = dto.Highlight,
            Colours = dto.Colours.ToList(),
            Updated = dto.Updated
        };
    }


    public static ConfigDTO ToDTO(this ConfigEntity entity)
    {
        return new()
        {
            Theme = entity.Theme,
            Highlight = entity.Highlight,
            Colours = entity.Colours?.AsEnumerable() ?? Enumerable.Empty<ColourMatch>(),
            Updated = entity.Updated
        };
    }

}
