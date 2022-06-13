using System.Globalization;

namespace Trackor.Features.Weather;

public static class OpenWeatherMapConverter
{
    public static WeatherConditions ToWeatherConditions(this OpenWeatherMapConditions openWeatherMapConditions, string units) 
    {
        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

        var degreesSuffix = units switch
        {
            "Imperial" => "°F",
            "Metric" => "°C",
            "Standard" => "°K",
            _ => string.Empty
        };

        var trackorConditions = new WeatherConditions
        {
            Temperature = $"{Math.Round(openWeatherMapConditions.main.temp)}{degreesSuffix}",
            FeelsLike = $"{Math.Round(openWeatherMapConditions.main.feels_like)}{degreesSuffix}",
            Time = DateTime.Now,
            Location = openWeatherMapConditions.name,
            Description = ti.ToTitleCase(string.Join(", ", openWeatherMapConditions.weather.Select(x => x.description))),
            Icons = openWeatherMapConditions.weather.Select(x => $"https://openweathermap.org/img/wn/{x.icon}@2x.png" ).ToArray()
        };

        return trackorConditions;
    }
}
