using Fluxor;
using MudBlazor;

namespace Trackor.Features.Theme;

public record ThemeState
{
    public MudTheme Theme { get; init; }
}

public class ThemeFeature : Feature<ThemeState>
{
    public override string GetName() => "Theme";

    protected override ThemeState GetInitialState()
    {
        return new ThemeState
        {
            Theme = new MudTheme
            {
                //Palette = new Palette()
                //{
                //    Primary = Colors.Purple.Darken2,
                //    Secondary = Colors.Yellow.Accent2,
                //    Background = Colors.BlueGrey.Lighten4,
                //    DrawerBackground = Colors.BlueGrey.Lighten5,
                //    AppbarBackground = Colors.Purple.Darken2
                //}
            }
        };
    }
}
