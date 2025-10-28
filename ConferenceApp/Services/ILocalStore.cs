using ConferenceApp.Models;

namespace ConferenceApp.Services;

public interface ILocalStore
{
    Task<ConferenceData?> LoadConferenceDataAsync();
    Task SaveConferenceDataAsync(ConferenceData data);
    Task<string> GetCachedVersionAsync();
    Task<string> GetCachedETagAsync();
    Task SetCachedETagAsync(string etag);
}
