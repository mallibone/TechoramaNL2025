namespace ConferenceApp.Models;

public class Speaker
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public SpeakerSocials Socials { get; set; } = new();
    public string HeadshotUrl { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime LastUpdated { get; set; }

    // Navigation property (populated by repository)
    public List<Session> Sessions { get; set; } = new();
}

public class SpeakerSocials
{
    public string? Twitter { get; set; }
    public string? Mastodon { get; set; }
    public string? GitHub { get; set; }
    public string? LinkedIn { get; set; }
    public string? Website { get; set; }
}
