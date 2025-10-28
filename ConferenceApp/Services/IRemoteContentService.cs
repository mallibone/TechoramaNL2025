using ConferenceApp.Models;

namespace ConferenceApp.Services;

public interface IRemoteContentService
{
    Task<RemoteContentResult> FetchConferenceDataAsync(string? cachedETag = null);
}

public class RemoteContentResult
{
    public bool Success { get; set; }
    public ConferenceData? Data { get; set; }
    public string? ETag { get; set; }
    public bool NotModified { get; set; }
    public string? ErrorMessage { get; set; }
}
