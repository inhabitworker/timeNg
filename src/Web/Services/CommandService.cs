using Shared.Entity;

namespace Web.Services;

public class CommandService : ICommandService
{
    private StatusService _status;

    public CommandService(StatusService status)
    {
        _status = status;
    }

    public Task ApplyInterval(IntervalDTO interval)
        => throw new NotImplementedException();
    public Task DeleteIntervals(IEnumerable<int> ids)
        => throw new NotImplementedException();

    public Task ApplyTag(TagDTO renamedTag)
        => throw new NotImplementedException();
    public Task DeleteTags(IEnumerable<int> ids)
        => throw new NotImplementedException();

    public Task ImportIntervals(IEnumerable<IntervalDTO> data)
        => throw new NotImplementedException();

    public Task SetConfig(ConfigDTO config)
        => throw new NotImplementedException();

}
