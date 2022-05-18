namespace Trackor.Features.Weather;

public record WeatherConditions
{
    public DateTime Time { get; init; }
    public string Location { get; init; }
    public string Temperature { get; init; }
    public string FeelsLike { get; init; }
    public string Description { get; init; }
    public string[] Icons { get; init;  }
}
