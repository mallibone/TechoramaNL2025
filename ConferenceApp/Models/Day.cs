namespace ConferenceApp.Models;

public class Day
{
    public int Index { get; set; }
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string Tz { get; set; } = "UTC"; // IANA timezone
    public string FriendlyName { get; set; } = string.Empty;

    public DateTime GetDate() => DateTime.Parse(Date);
}
