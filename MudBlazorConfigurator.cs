using MudBlazor;
using MudBlazor.Services;

namespace Trackor;

public static class MudBlazorConfigurator
{
    public static void ConfigureWithTrackorDefaults(this MudServicesConfiguration config) 
    {
        config.SnackbarConfiguration.PreventDuplicates = false;
        config.SnackbarConfiguration.NewestOnTop = false;
        config.SnackbarConfiguration.ShowCloseIcon = true;
        config.SnackbarConfiguration.VisibleStateDuration = 10000;
        config.SnackbarConfiguration.HideTransitionDuration = 500;
        config.SnackbarConfiguration.ShowTransitionDuration = 500;
        config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomEnd;
        config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
        config.SnackbarConfiguration.MaxDisplayedSnackbars = 10;
    }
}
