using Shared.Models;

namespace Shared.Timewarrior;

public interface ITimewarriorService
{
    public Task<string> Execute(string cmd);
    public Task Execute(IEnumerable<string> cmds);
    public Task<IEnumerable<IInterval>> Export();
    public Task<IInterval?> ExportSingle(int id);
    public Task Track(DateTime start, DateTime? end, List<string> tags);
    public Task Delete(int id);
    public Task Delete(IEnumerable<int> id);
    public Task Annotate(int id, string annotation);
    public Task Tag(int id, string tag);
    public Task Tag(IEnumerable<int> id, string tag);
    public Task Untag(int id, string tag);
    public Task Untag(IEnumerable<int> id, string tag);
    public Task Undo();
    public Task Seed(IEnumerable<IInterval> data);
}

