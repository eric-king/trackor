using Fluxor;
using SqliteWasmHelper;
using Trackor.Features.Database;

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
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public ThemeEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _db = dbFactory;
    }

    [EffectMethod(typeof(ThemeLoadDarkModeAction))]
    public async Task OnLoadDarkMode(IDispatcher dispatcher) 
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var appSetting = dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == APP_SETTING_IS_DARK_MODE);
        if (appSetting is null) 
        {
            appSetting = new ApplicationSetting { Key = APP_SETTING_IS_DARK_MODE, Value = false.ToString() };
            dbContext.ApplicationSettings.Add(appSetting);
            dbContext.SaveChanges();
        }
        dispatcher.Dispatch(new ThemeSetDarkModeAction(bool.Parse(appSetting.Value)));
    }

    [EffectMethod(typeof(ThemeToggleDarkModeAction))]
    public async Task OnToggleDarkModeAction(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var appSetting = dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == APP_SETTING_IS_DARK_MODE);
        var isDarkMode = !bool.Parse(appSetting.Value);
        appSetting.Value = isDarkMode.ToString();
        dbContext.SaveChanges();
        dispatcher.Dispatch(new ThemeSetDarkModeAction(isDarkMode));
    }

}