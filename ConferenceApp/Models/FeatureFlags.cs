namespace ConferenceApp.Models;

public class FeatureFlags
{
    public string Version { get; set; } = "1.0";
    public string? ETag { get; set; }
    public bool SessionFeedbackEnabled { get; set; }
    
    // Future flags can be added here
    // public bool LiveStreamingEnabled { get; set; }
    // public bool NetworkingEnabled { get; set; }
    
    public static FeatureFlags CreateDefault()
    {
        return new FeatureFlags
        {
            Version = "1.0",
            SessionFeedbackEnabled = false
        };
    }
}
