namespace Shared.Interfaces;

public interface IImportService<T>
{
    T Source { get; set; }
    IEnumerable<IntervalDTO> MappedData { get; set; }
    IEnumerable<IntervalDTO> Conflicts { get; set; }

    public Task Start();
    public Task VerifySource(T source);
    public Task<IEnumerable<IntervalDTO>> MapSourceData();
    public Task<IEnumerable<IntervalDTO>> Validate();

}
