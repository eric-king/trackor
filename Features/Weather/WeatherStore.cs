using Fluxor;
using System.Net.Http.Json;
using Trackor.Features.Database.Repositories;
using Trackor.Features.Notifications;

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

    private readonly HttpClient _httpClient;
    private readonly IState<WeatherState> _weatherState;
    private readonly ApplicationSettingRepository _appSettingRepo;

    public WeatherEffects(HttpClient httpClient, IState<WeatherState> weatherState, ApplicationSettingRepository appSettingRepo)
    {
        _httpClient = httpClient;
        _weatherState = weatherState;
        _appSettingRepo = appSettingRepo;
    }

    [EffectMethod(typeof(WeatherLoadConfigAction))]
    public async Task OnLoadConfig(IDispatcher dispatcher)
    {
        if (_weatherState.Value.IsWeatherConfigLoaded) { return; }

        var postalCode = await _appSettingRepo.GetOrAdd(APP_SETTING_POSTAL_CODE, defaultValue: string.Empty);
        var countryCode = await _appSettingRepo.GetOrAdd(APP_SETTING_COUNTRY_CODE, defaultValue: "US");
        var units = await _appSettingRepo.GetOrAdd(APP_SETTING_UNITS, defaultValue: "Imperial");
        var apiKey = await _appSettingRepo.GetOrAdd(APP_SETTING_API_KEY, defaultValue: string.Empty);

        dispatcher.Dispatch(new WeatherSetConfigAction(postalCode.Value, countryCode.Value, units.Value, apiKey.Value));

        if (string.IsNullOrEmpty(postalCode.Value) || string.IsNullOrEmpty(apiKey.Value))
        {
            dispatcher.Dispatch(new WeatherMustConfigureAction());
        }

    }

    [EffectMethod]
    public async Task OnSaveConfig(WeatherSaveConfigAction action, IDispatcher dispatcher)
    {
        await _appSettingRepo.Update(APP_SETTING_POSTAL_CODE, action.PostalCode);
        await _appSettingRepo.Update(APP_SETTING_COUNTRY_CODE, action.CountryCode);
        await _appSettingRepo.Update(APP_SETTING_UNITS, action.Units);
        await _appSettingRepo.Update(APP_SETTING_API_KEY, action.ApiKey);

        dispatcher.Dispatch(new WeatherSetConfigAction(action.PostalCode, action.CountryCode, action.Units, action.ApiKey));
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

        if (!response.IsSuccessStatusCode) 
        {
            dispatcher.Dispatch(new SnackbarShowErrorAction($"Error getting current weather conditions. Please ensure your Postal Code and API Key are valid."));
            return;
        }

        var openWeatherMapConditions = await response.Content.ReadFromJsonAsync<OpenWeatherMapConditions>();
        dispatcher.Dispatch(new WeatherSetConditionsAction(openWeatherMapConditions.ToWeatherConditions(_weatherState.Value.Units)));
    }
}
