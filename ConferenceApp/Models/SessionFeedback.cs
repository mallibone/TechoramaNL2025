namespace ConferenceApp.Models;

public class SessionFeedback
{
    public string SessionId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public DateTime SubmittedAt { get; set; }
}
