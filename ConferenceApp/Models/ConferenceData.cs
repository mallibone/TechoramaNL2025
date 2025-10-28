namespace ConferenceApp.Models;

public class ConferenceData
{
    public string SchemaVersion { get; set; } = "1.0.0";
    public string ContentVersion { get; set; } = "1";
    public DateTime GeneratedAtUtc { get; set; }
    public ConferenceInfo Conference { get; set; } = new();
    public ApiEndpoints Api { get; set; } = new();
    public string MinAppVersion { get; set; } = "1.0";

    public List<Session> Sessions { get; set; } = new();
    public List<Speaker> Speakers { get; set; } = new();
    public List<Track> Tracks { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Day> Days { get; set; } = new();
    public List<Sponsor> Sponsors { get; set; } = new();
}

public class ConferenceInfo
{
    public string Name { get; set; } = string.Empty;
    public string Tz { get; set; } = "UTC";
    public string Venue { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty; // YYYY-MM-DD
    public string EndDate { get; set; } = string.Empty; // YYYY-MM-DD
}

public class ApiEndpoints
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AssetsBaseUrl { get; set; } = string.Empty;
}
