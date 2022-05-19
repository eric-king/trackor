using Fluxor;
using MudBlazor;

namespace Trackor.Features.Notifications;

public record SnackbarShowInfoAction(string Content);
public record SnackbarShowSuccessAction(string Content);
public record SnackbarShowWarningAction(string Content);
public record SnackbarShowErrorAction(string Content);

public record SnackbarState { }

public class SnackbarFeature : Feature<SnackbarState>
{
    public override string GetName() => "Notifications_Snackbar";

    protected override SnackbarState GetInitialState()
    {
        return new SnackbarState();
    }
}

public class SnackbarEffects 
{
    private readonly ISnackbar _snackbar;

    public SnackbarEffects(ISnackbar snackbar)
    {
        _snackbar = snackbar;
    }

    [EffectMethod]
    public async Task ShowInfo(SnackbarShowInfoAction action, IDispatcher dispatcher)
    {
        await Task.Delay(0);
        _snackbar.Add(action.Content, Severity.Info);
    }

    [EffectMethod]
    public async Task ShowSuccess(SnackbarShowSuccessAction action, IDispatcher dispatcher)
    {
        await Task.Delay(0);
        _snackbar.Add(action.Content, Severity.Success);
    }

    [EffectMethod]
    public async Task ShowWarning(SnackbarShowWarningAction action, IDispatcher dispatcher)
    {
        await Task.Delay(0);
        _snackbar.Add(action.Content, Severity.Warning);
    }

    [EffectMethod]
    public async Task ShowError(SnackbarShowErrorAction action, IDispatcher dispatcher)
    {
        await Task.Delay(0);
        _snackbar.Add(action.Content, Severity.Error);
    }
}
