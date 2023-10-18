using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Import;

public class TogglSource : IImportSource
{
    public string Name { get; } = "Toggl Track";
    public string Information { get; } = "Import tracked items from Toggl Track via API v9.";

    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }    

    private HttpClient _http;

    private string VerificationURI = "https://api.track.toggl.com/api/v9/me";
    private string DataURI = "https://api.track.toggl.com/api/v9/me/time_entries";

    public TogglSource(HttpClient http)
    {
        _http = http;
    }

    public Task<bool> Verify()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IntervalDTO>> GetData()
    {
        throw new NotImplementedException();
    }

}
