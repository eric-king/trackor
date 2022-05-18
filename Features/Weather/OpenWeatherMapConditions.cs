using System.Text.Json.Serialization;

namespace Trackor.Features.Weather;
/// <summary>
/// Weather conditions as returned from the Current Weather 
/// Data API at OpenWeatherMap.org. More details at 
/// https://openweathermap.org/current#current_JSON
/// </summary>
public record OpenWeatherMapConditions
{
    public Coord coord { get; set; }
    public Weather[] weather { get; set; }
    public Main main { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public Rain rain { get; set; }
    public Snow snow { get; set; } 
    public Sys sys { get; set; }
    
    /// <summary>
    /// Internal parameter
    /// </summary>
    public string _base { get; set; }

    /// <summary>
    ///  Visibility, meter. The maximum value of the visibility is 10km
    /// </summary>
    public int visibility { get; set; }

    /// <summary>
    /// Time of data calculation, unix, UTC
    /// </summary>
    public int dt { get; set; }

    /// <summary>
    /// Shift in seconds from UTC
    /// </summary>
    public int timezone { get; set; }

    /// <summary>
    /// City ID
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// City name
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// Internal parameter
    /// </summary>
    public int cod { get; set; }
}

public record Coord
{
    /// <summary>
    /// City geo location, longitude
    /// </summary>
    public float lon { get; set; }

    /// <summary>
    /// City geo location, latitude
    /// </summary>
    public float lat { get; set; }
}

public record Weather
{
    /// <summary>
    /// Weather condition id
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// Group of weather parameters (Rain, Snow, Extreme etc.)
    /// </summary>
    public string main { get; set; }

    /// <summary>
    /// Weather condition within the group. You can get the output in your language.
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// Weather icon id
    /// </summary>
    public string icon { get; set; }
}

public record Main
{
    /// <summary>
    /// Temperature. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
    /// </summary>
    public float temp { get; set; }

    /// <summary>
    /// Temperature. This temperature parameter accounts for the human perception of 
    /// weather. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
    /// </summary>
    public float feels_like { get; set; }

    /// <summary>
    /// Minimum temperature at the moment. This is minimal currently observed 
    /// temperature (within large megalopolises and urban areas). Unit Default: 
    /// Kelvin, Metric: Celsius, Imperial: Fahrenheit.
    /// </summary>
    public float temp_min { get; set; }

    /// <summary>
    /// Maximum temperature at the moment. This is maximal currently observed 
    /// temperature (within large megalopolises and urban areas). 
    /// Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
    /// </summary>
    public float temp_max { get; set; }

    /// <summary>
    /// Atmospheric pressure (on the sea level, if there is no sea_level or 
    /// grnd_level data), hPa
    /// </summary>
    public int pressure { get; set; }

    /// <summary>
    /// Humidity, %
    /// </summary>
    public int humidity { get; set; }
}

public record Wind
{
    /// <summary>
    /// Wind speed. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour.
    /// </summary>
    public float speed { get; set; }

    /// <summary>
    /// Wind direction, degrees (meteorological)
    /// </summary>
    public int deg { get; set; }

    /// <summary>
    /// Wind gust. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour
    /// </summary>
    public float gust { get; set; }
}

public record Clouds
{
    /// <summary>
    ///  Cloudiness, %
    /// </summary>
    public int all { get; set; }
}

public record Rain
{
    /// <summary>
    /// Rain volume for the last 1 hour, mm
    /// </summary>
    [JsonPropertyName("1h")]
    public float OneHour { get; set; }

    /// <summary>
    /// Rain volume for the last 3 hours, mm
    /// </summary>
    [JsonPropertyName("3h")]
    public float ThreeHour { get; set; }
}

public record Snow
{
    /// <summary>
    /// Snow volume for the last 1 hour, mm
    /// </summary>
    [JsonPropertyName("1h")]
    public float OneHour { get; set; }

    /// <summary>
    /// Snow volume for the last 3 hours, mm
    /// </summary>
    [JsonPropertyName("3h")]
    public float ThreeHour { get; set; }
}

public record Sys
{
    /// <summary>
    /// Internal parameter
    /// </summary>
    public int type { get; set; }

    /// <summary>
    /// Internal parameter
    /// </summary>
    public int id { get; set; }

    /// <summary>
    /// Internal parameter
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// Country code (GB, JP etc.)
    /// </summary>
    public string country { get; set; }

    /// <summary>
    /// Sunrise time, unix, UTC
    /// </summary>
    public int sunrise { get; set; }

    /// <summary>
    /// Sunset time, unix, UTC
    /// </summary>
    public int sunset { get; set; }
}
