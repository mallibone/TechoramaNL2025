using ConferenceApp.Models;

namespace ConferenceApp.Services;

public interface IConferenceRepository
{
    Task<ConferenceData?> GetConferenceDataAsync();
    Task<List<Session>> GetSessionsAsync();
    Task<List<Session>> GetSessionsByDayAsync(int dayIndex);
    Task<Session?> GetSessionByIdAsync(string sessionId);
    Task<List<Speaker>> GetSpeakersAsync();
    Task<Speaker?> GetSpeakerByIdAsync(string speakerId);
    Task<List<Track>> GetTracksAsync();
    Task<List<Room>> GetRoomsAsync();
    Task<List<Day>> GetDaysAsync();
    Task RefreshDataAsync();
    
    // Favorites
    Task<bool> IsFavoriteAsync(string sessionId);
    Task ToggleFavoriteAsync(string sessionId);
    Task<List<Session>> GetFavoriteSessionsAsync();
}
