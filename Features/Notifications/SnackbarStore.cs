using Fluxor;
using MudBlazor;

namespace Trackor.Features.Notifications;

public record SnackbarShowInfoAction(string Content, Action ClickAction = null);
public record SnackbarShowSuccessAction(string Content, Action ClickAction = null);
public record SnackbarShowWarningAction(string Content, Action ClickAction = null);
public record SnackbarShowErrorAction(string Content, Action ClickAction = null);

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
        if (action.ClickAction is null)
        {
            _snackbar.Add(action.Content, Severity.Info);
        }
        else 
        {
            _snackbar.Add(action.Content, Severity.Info, config =>
            {
                config.Onclick = snackbar =>
                {
                    action.ClickAction();
                    return Task.CompletedTask;
                };
            });
        }

        await Task.Yield();
    }

    [EffectMethod]
    public async Task ShowSuccess(SnackbarShowSuccessAction action, IDispatcher dispatcher)
    {
        if (action.ClickAction is null)
        {
            _snackbar.Add(action.Content, Severity.Success);
        }
        else
        {
            _snackbar.Add(action.Content, Severity.Success, config =>
            {
                config.Onclick = snackbar =>
                {
                    action.ClickAction();
                    return Task.CompletedTask;
                };
            });
        }

        await Task.Yield();
    }

    [EffectMethod]
    public async Task ShowWarning(SnackbarShowWarningAction action, IDispatcher dispatcher)
    {
        if (action.ClickAction is null)
        {
            _snackbar.Add(action.Content, Severity.Warning);
        }
        else
        {
            _snackbar.Add(action.Content, Severity.Warning, config =>
            {
                config.Onclick = snackbar =>
                {
                    action.ClickAction();
                    return Task.CompletedTask;
                };
            });
        }

        await Task.Yield();
    }

    [EffectMethod]
    public async Task ShowError(SnackbarShowErrorAction action, IDispatcher dispatcher)
    {
        if (action.ClickAction is null)
        {
            _snackbar.Add(action.Content, Severity.Error);
        }
        else
        {
            _snackbar.Add(action.Content, Severity.Error, config =>
            {
                config.Onclick = snackbar =>
                {
                    action.ClickAction();
                    return Task.CompletedTask;
                };
            });
        }

        await Task.Yield();
    }
}
