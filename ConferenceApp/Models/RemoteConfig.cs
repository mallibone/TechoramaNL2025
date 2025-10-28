namespace ConferenceApp.Models;

public class RemoteConfig
{
    public string BaseUrl { get; set; } = "https://YOUR-BLOB-URL.blob.core.windows.net/main";
    public string ConferenceJsonPath { get; set; } = "conference.json";
    public string ContentFullUrl => $"{BaseUrl}/{ConferenceJsonPath}";
    
    public string FlagsJsonPath { get; set; } = "featureflags.json";
    public string FlagsFullUrl => $"{BaseUrl}/{FlagsJsonPath}";
    
    public bool Enabled { get; set; } = true;
}
