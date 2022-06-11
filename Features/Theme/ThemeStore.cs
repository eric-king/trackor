using Fluxor;
using Trackor.Features.Database.Repositories;

namespace Trackor.Features.Theme;

public record ThemeToggleDarkModeAction();
public record ThemeSetDarkModeAction(bool IsDarkMode);
public record ThemeLoadDarkModeAction();

public record ThemeState
{
    public bool IsDarkMode { get; init; }
}

public class ThemeFeature : Feature<ThemeState>
{
    public override string GetName() => "Theme";

    protected override ThemeState GetInitialState()
    {
        return new ThemeState
        {
            IsDarkMode = false
        };
    }
}

public static class ThemeReducers 
{
    [ReducerMethod]
    public static ThemeState OnSetDarkMode(ThemeState state, ThemeSetDarkModeAction action) 
    {
        return state with
        {
            IsDarkMode = action.IsDarkMode
        };
    }
}

public class ThemeEffects 
{
    private const string APP_SETTING_IS_DARK_MODE = "IsDarkMode";
    private readonly ApplicationSettingRepository _appSettingRepo;

    public ThemeEffects(ApplicationSettingRepository appSettingRepo)
    {
        _appSettingRepo = appSettingRepo;
    }

    [EffectMethod(typeof(ThemeLoadDarkModeAction))]
    public async Task OnLoadDarkMode(IDispatcher dispatcher) 
    {
        var appSetting = await _appSettingRepo.GetOrAdd(APP_SETTING_IS_DARK_MODE, "false");
        dispatcher.Dispatch(new ThemeSetDarkModeAction(bool.Parse(appSetting.Value)));
    }

    [EffectMethod(typeof(ThemeToggleDarkModeAction))]
    public async Task OnToggleDarkModeAction(IDispatcher dispatcher)
    {
        var appSetting = await _appSettingRepo.Toggle(APP_SETTING_IS_DARK_MODE);
        dispatcher.Dispatch(new ThemeSetDarkModeAction(bool.Parse(appSetting.Value)));
    }
}
