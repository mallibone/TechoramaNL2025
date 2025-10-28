using ConferenceApp.Models;

namespace ConferenceApp.Services;

public class ConferenceRepository : IConferenceRepository
{
    private readonly ILocalStore _localStore;
    private readonly IRemoteContentService _remoteContentService;
    private ConferenceData? _cachedData;
    private HashSet<string> _favorites;
    private const string FavoritesKey = "session_favorites";

    public ConferenceRepository(ILocalStore localStore, IRemoteContentService remoteContentService)
    {
        _localStore = localStore;
        _remoteContentService = remoteContentService;
        _favorites = LoadFavorites();
    }

    public async Task<ConferenceData?> GetConferenceDataAsync()
    {
        if (_cachedData == null)
        {
            _cachedData = await _localStore.LoadConferenceDataAsync();
            if (_cachedData != null)
            {
                HydrateNavigationProperties(_cachedData);
            }
        }
        return _cachedData;
    }

    public async Task<List<Session>> GetSessionsAsync()
    {
        var data = await GetConferenceDataAsync();
        return data?.Sessions ?? new List<Session>();
    }

    public async Task<List<Session>> GetSessionsByDayAsync(int dayIndex)
    {
        var sessions = await GetSessionsAsync();
        return sessions.Where(s => s.DayIndex == dayIndex).OrderBy(s => s.StartUtc).ToList();
    }

    public async Task<Session?> GetSessionByIdAsync(string sessionId)
    {
        var sessions = await GetSessionsAsync();
        return sessions.FirstOrDefault(s => s.Id == sessionId);
    }

    public async Task<List<Speaker>> GetSpeakersAsync()
    {
        var data = await GetConferenceDataAsync();
        return data?.Speakers ?? new List<Speaker>();
    }

    public async Task<Speaker?> GetSpeakerByIdAsync(string speakerId)
    {
        var speakers = await GetSpeakersAsync();
        return speakers.FirstOrDefault(s => s.Id == speakerId);
    }

    public async Task<List<Track>> GetTracksAsync()
    {
        var data = await GetConferenceDataAsync();
        return data?.Tracks.OrderBy(t => t.Order).ToList() ?? new List<Track>();
    }

    public async Task<List<Room>> GetRoomsAsync()
    {
        var data = await GetConferenceDataAsync();
        return data?.Rooms ?? new List<Room>();
    }

    public async Task<List<Day>> GetDaysAsync()
    {
        var data = await GetConferenceDataAsync();
        return data?.Days.OrderBy(d => d.Index).ToList() ?? new List<Day>();
    }

    public async Task RefreshDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ConferenceRepository: Starting refresh from remote");
            
            // Get cached ETag for conditional request
            var cachedETag = await _localStore.GetCachedETagAsync();
            
            // Fetch from remote
            var result = await _remoteContentService.FetchConferenceDataAsync(cachedETag);
            
            if (result.Success && !result.NotModified && result.Data != null)
            {
                // Compare versions - only update if remote is newer
                var cachedVersion = await _localStore.GetCachedVersionAsync();
                var remoteVersion = result.Data.ContentVersion;
                
                System.Diagnostics.Debug.WriteLine($"ConferenceRepository: Cached version: {cachedVersion}, Remote version: {remoteVersion}");
                
                if (string.IsNullOrEmpty(cachedVersion) || CompareVersions(remoteVersion, cachedVersion) > 0)
                {
                    System.Diagnostics.Debug.WriteLine("ConferenceRepository: Remote version is newer, saving to cache");
                    
                    // Save new data and ETag
                    await _localStore.SaveConferenceDataAsync(result.Data);
                    if (!string.IsNullOrEmpty(result.ETag))
                    {
                        await _localStore.SetCachedETagAsync(result.ETag);
                    }
                    
                    // Reload in-memory cache
                    _cachedData = result.Data;
                    HydrateNavigationProperties(_cachedData);
                    
                    System.Diagnostics.Debug.WriteLine("ConferenceRepository: Refresh complete - new data loaded");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ConferenceRepository: Remote version is not newer, keeping cached version");
                }
            }
            else if (result.NotModified)
            {
                System.Diagnostics.Debug.WriteLine("ConferenceRepository: Content not modified (304) - using cached data");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ConferenceRepository: Remote fetch failed - {result.ErrorMessage}");
                
                // Fallback to local cache
                _cachedData = await _localStore.LoadConferenceDataAsync();
                if (_cachedData != null)
                {
                    HydrateNavigationProperties(_cachedData);
                    System.Diagnostics.Debug.WriteLine("ConferenceRepository: Using cached data as fallback");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConferenceRepository: Exception during refresh: {ex.Message}");
            
            // Fallback to local cache
            _cachedData = await _localStore.LoadConferenceDataAsync();
            if (_cachedData != null)
            {
                HydrateNavigationProperties(_cachedData);
            }
        }
    }

    private int CompareVersions(string version1, string version2)
    {
        // Simple numeric comparison for now
        // Future: could implement SemVer comparison
        if (int.TryParse(version1, out var v1) && int.TryParse(version2, out var v2))
        {
            return v1.CompareTo(v2);
        }
        
        // Fallback to string comparison
        return string.Compare(version1, version2, StringComparison.Ordinal);
    }

    public Task<bool> IsFavoriteAsync(string sessionId)
    {
        return Task.FromResult(_favorites.Contains(sessionId));
    }

    public async Task ToggleFavoriteAsync(string sessionId)
    {
        if (_favorites.Contains(sessionId))
        {
            _favorites.Remove(sessionId);
        }
        else
        {
            _favorites.Add(sessionId);
        }
        SaveFavorites();

        // Update session favorite flag in cache
        var session = await GetSessionByIdAsync(sessionId);
        if (session != null)
        {
            session.IsFavorite = _favorites.Contains(sessionId);
        }
    }

    public async Task<List<Session>> GetFavoriteSessionsAsync()
    {
        var sessions = await GetSessionsAsync();
        return sessions.Where(s => _favorites.Contains(s.Id)).OrderBy(s => s.StartUtc).ToList();
    }

    private void HydrateNavigationProperties(ConferenceData data)
    {
        // Create lookup dictionaries
        var speakersDict = data.Speakers.ToDictionary(s => s.Id);
        var tracksDict = data.Tracks.ToDictionary(t => t.Id);
        var roomsDict = data.Rooms.ToDictionary(r => r.Id);

        // Hydrate sessions
        foreach (var session in data.Sessions)
        {
            session.Speakers = session.SpeakerIds
                .Where(id => speakersDict.ContainsKey(id))
                .Select(id => speakersDict[id])
                .ToList();

            session.Tracks = session.TrackIds
                .Where(id => tracksDict.ContainsKey(id))
                .Select(id => tracksDict[id])
                .ToList();

            if (roomsDict.TryGetValue(session.RoomId, out var room))
            {
                session.Room = room;
            }

            session.IsFavorite = _favorites.Contains(session.Id);
        }

        // Hydrate speakers with their sessions
        foreach (var speaker in data.Speakers)
        {
            speaker.Sessions = data.Sessions
                .Where(s => s.SpeakerIds.Contains(speaker.Id))
                .ToList();
        }
    }

    private HashSet<string> LoadFavorites()
    {
        var json = Preferences.Get(FavoritesKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return new HashSet<string>();
        }

        try
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
            return list != null ? new HashSet<string>(list) : new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    private void SaveFavorites()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(_favorites.ToList());
        Preferences.Set(FavoritesKey, json);
    }
}
