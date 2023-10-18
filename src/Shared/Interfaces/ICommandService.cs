namespace Shared.Interfaces;

public interface ICommandService
{
    public Task ApplyInterval(IntervalDTO interval);
    public Task ImportIntervals(IEnumerable<IntervalDTO> intervals);
    public Task DeleteIntervals(IEnumerable<int> ids);

    public Task ApplyTag(TagDTO tag);
    public Task DeleteTags(IEnumerable<int> ids);

    public Task SetConfig(ConfigDTO config);
}
