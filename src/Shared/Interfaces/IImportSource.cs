namespace Shared.Interfaces;

public interface IImportSource
{
    public string Name { get; }
    public string Information { get; }

    public Task<bool> Verify();
    public Task<IEnumerable<IntervalDTO>> GetData();
}

