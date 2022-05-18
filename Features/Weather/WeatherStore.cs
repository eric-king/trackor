using Fluxor;
using SqliteWasmHelper;
using System.Net.Http.Json;
using Trackor.Features.Database;

namespace Trackor.Features.Weather;

public record WeatherLoadConfigAction();
public record WeatherSaveConfigAction(string PostalCode, string CountryCode, string Units, string ApiKey);
public record WeatherSetConfigAction(string PostalCode, string CountryCode, string Units, string ApiKey);
public record WeatherSetConditionsAction(WeatherConditions WeatherConditions);
public record WeatherMustConfigureAction();
public record WeatherGetCurrentConditionsAction();

public record WeatherState
{
    public bool IsWeatherConfigLoaded { get; set; }
    public string PostalCode { get; init; }
    public string CountryCode { get; init; }
    public string Units { get; init; }
    public string ApiKey { get; init; }
    public WeatherConditions WeatherConditions { get; init; }
}

public class WeatherFeature : Feature<WeatherState>
{
    public override string GetName() => "Weather";

    protected override WeatherState GetInitialState()
    {
        return new WeatherState
        {
            IsWeatherConfigLoaded = false,
            PostalCode = string.Empty,
            CountryCode = string.Empty,
            Units = string.Empty,
            ApiKey = string.Empty,
            WeatherConditions = null
        };
    }
}

public static class WeatherReducers
{
    [ReducerMethod]
    public static WeatherState SetWeatherConfig(WeatherState state, WeatherSetConfigAction action)
    {
        return state with
        {
            IsWeatherConfigLoaded = true,
            PostalCode = action.PostalCode,
            CountryCode = action.CountryCode,
            Units = action.Units,
            ApiKey = action.ApiKey
        };
    }

    [ReducerMethod]
    public static WeatherState SetWeatherConditions(WeatherState state, WeatherSetConditionsAction action)
    {
        return state with
        {
            WeatherConditions = action.WeatherConditions
        };
    }
}

public class WeatherEffects
{
    private const string APP_SETTING_POSTAL_CODE = "WeatherPostalCode";
    private const string APP_SETTING_COUNTRY_CODE = "WeatherCountryCode";
    private const string APP_SETTING_UNITS = "WeatherUnits";
    private const string APP_SETTING_API_KEY = "WeatherApiKey";

    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;
    private readonly HttpClient _httpClient;
    private readonly IState<WeatherState> _weatherState;

    public WeatherEffects(ISqliteWasmDbContextFactory<TrackorContext> db, HttpClient httpClient, IState<WeatherState> weatherState)
    {
        _db = db;
        _httpClient = httpClient;
        _weatherState = weatherState;
    }

    [EffectMethod(typeof(WeatherLoadConfigAction))]
    public async Task OnLoadConfig(IDispatcher dispatcher)
    {
        if (_weatherState.Value.IsWeatherConfigLoaded) { return; }

        using var dbContext = await _db.CreateDbContextAsync();

        var weatherConfigRecords = dbContext.ApplicationSettings.Where(x => x.Key.StartsWith("Weather")).ToList();
        if (weatherConfigRecords.Count != 4)
        {
            dispatcher.Dispatch(new WeatherMustConfigureAction());
        }
        else
        {
            var postalCode = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_POSTAL_CODE)).Value;
            var countryCode = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_COUNTRY_CODE)).Value;
            var units = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_UNITS)).Value;
            var apiKey = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_API_KEY)).Value;

            dispatcher.Dispatch(new WeatherSetConfigAction(postalCode, countryCode, units, apiKey));
        }
    }

    [EffectMethod]
    public async Task OnSaveConfig(WeatherSaveConfigAction action, IDispatcher dispatcher)
    {
        ApplicationSetting postalCode;
        ApplicationSetting countryCode;
        ApplicationSetting units;
        ApplicationSetting apiKey;

        using var dbContext = await _db.CreateDbContextAsync();
        var weatherConfigRecords = dbContext.ApplicationSettings.Where(x => x.Key.StartsWith("Weather")).ToList();

        if (weatherConfigRecords.Count != 4)
        {
            postalCode = new ApplicationSetting { Key = APP_SETTING_POSTAL_CODE, Value = action.PostalCode };
            countryCode = new ApplicationSetting { Key = APP_SETTING_COUNTRY_CODE, Value = action.CountryCode };
            units = new ApplicationSetting { Key = APP_SETTING_UNITS, Value = action.Units };
            apiKey = new ApplicationSetting { Key = APP_SETTING_API_KEY, Value = action.ApiKey };

            dbContext.ApplicationSettings.Add(postalCode);
            dbContext.ApplicationSettings.Add(countryCode);
            dbContext.ApplicationSettings.Add(units);
            dbContext.ApplicationSettings.Add(apiKey);
        }
        else
        {
            postalCode = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_POSTAL_CODE));
            countryCode = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_COUNTRY_CODE));
            units = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_UNITS));
            apiKey = weatherConfigRecords.Single(x => x.Key.Equals(APP_SETTING_API_KEY));

            postalCode.Value = action.PostalCode;
            countryCode.Value = action.CountryCode;
            units.Value = action.Units;
            apiKey.Value = action.ApiKey;
        }

        dbContext.SaveChanges();
        dispatcher.Dispatch(new WeatherSetConfigAction(postalCode.Value, countryCode.Value, units.Value, apiKey.Value));
    }

    [EffectMethod(typeof(WeatherGetCurrentConditionsAction))]
    public async Task OnGetCurrentConditions(IDispatcher dispatcher) 
    {
        var zip = _weatherState.Value.PostalCode;
        var countryCode = _weatherState.Value.CountryCode;
        var apiKey = _weatherState.Value.ApiKey;
        var units = _weatherState.Value.Units;

        var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?zip={zip},{countryCode}&appid={apiKey}&units={units}";
        var response = await _httpClient.GetAsync(apiUrl);
        
        // Todo: Display error message on unsuccessful response

        var openWeatherMapConditions = await response.Content.ReadFromJsonAsync<OpenWeatherMapConditions>();
        dispatcher.Dispatch(new WeatherSetConditionsAction(openWeatherMapConditions.ToWeatherConditions(_weatherState.Value.Units)));
    } 
}
