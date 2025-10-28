namespace ConferenceApp.Models;

public class Room
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public MapCoordinates? MapCoordinates { get; set; }
    public string IndoorMapImageUrl { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class MapCoordinates
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public int? Level { get; set; }
}
