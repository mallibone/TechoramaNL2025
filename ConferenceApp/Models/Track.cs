namespace ConferenceApp.Models;

public class Track
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#512BD4";
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public string IconUrl { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
