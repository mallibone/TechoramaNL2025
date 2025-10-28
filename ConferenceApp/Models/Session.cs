namespace ConferenceApp.Models;

public class Session
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime StartUtc { get; set; }
    public string StartDisplay => StartUtc.ToLocalTime().ToString("hh:mm tt");
    public DateTime EndUtc { get; set; }
    public string EndDisplay => EndUtc.ToLocalTime().ToString("hh:mm tt");
    public int DayIndex { get; set; }
    public List<string> TrackIds { get; set; } = new();
    public List<string> SpeakerIds { get; set; } = new();
    public string RoomId { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public bool IsLiveStream { get; set; }
    public string? RecordingUrl { get; set; }
    public string? LiveUrl { get; set; }
    public bool IsWorkshop { get; set; }
    public bool RequiresRegistration { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime LastUpdated { get; set; }

    // Computed/navigation properties (populated by repository)
    public List<Speaker> Speakers { get; set; } = new();
    public List<Track> Tracks { get; set; } = new();
    public Room? Room { get; set; }
    public bool IsFavorite { get; set; }
}
