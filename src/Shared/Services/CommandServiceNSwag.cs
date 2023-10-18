namespace Shared.Services;

public class CommandServiceNSwag : ICommandService
{
    protected IIntervalsClient _intervalsClient;
    protected ITagsClient _tagsClient;
    protected IConfigClient _configClient;
    public CommandServiceNSwag(IIntervalsClient intervalsClient, ITagsClient tagsClient, IConfigClient configClient) 
    { 
        _intervalsClient = intervalsClient;
        _tagsClient = tagsClient;
        _configClient = configClient;
    }

    public Task ApplyInterval(IntervalDTO interval)
        => _intervalsClient.ApplyIntervalAsync(interval);

    public Task DeleteIntervals(IEnumerable<int> ids) 
        => _intervalsClient.DeleteIntervalsAsync(ids);

    public Task ApplyTag(TagDTO input)
        => _tagsClient.ApplyTagAsync(input);

    public Task DeleteTags(IEnumerable<int> ids)
        => _tagsClient.DeleteTagsAsync(ids);

    public Task ImportIntervals(IEnumerable<IntervalDTO> input)
        => _intervalsClient.ImportIntervalsAsync(input);

    public Task SetConfig(ConfigDTO config)
        => _configClient.SetConfigAsync(config);

} 
