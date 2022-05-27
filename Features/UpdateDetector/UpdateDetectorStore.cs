using Fluxor;
using Microsoft.AspNetCore.Components;

namespace Trackor.Features.UpdateDetector;

public record UpdateDetectorNotificationClickedAction();
public record UpdateDetectorSetUpdateAvailableAction();

public record UpdateDetectorState 
{
    public bool UpdateAvailable { get; init; }
}

public class SnackbarFeature : Feature<UpdateDetectorState>
{
    public override string GetName() => "UpdateDetector";

    protected override UpdateDetectorState GetInitialState()
    {
        return new UpdateDetectorState 
        {
            UpdateAvailable = false
        };
    }
}

public static class UpdateDetectorReducers 
{
    [ReducerMethod(typeof(UpdateDetectorSetUpdateAvailableAction))]
    public static UpdateDetectorState OnSetUpdateAvailable(UpdateDetectorState state) 
    {
        return state with
        {
            UpdateAvailable = true
        };
    } 
}

public class UpdateDetectorEffects
{
    private readonly NavigationManager _navigationManager;

    public UpdateDetectorEffects(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    [EffectMethod(typeof(UpdateDetectorNotificationClickedAction))]
    public async Task OnNotificationClick(IDispatcher dispatcher)
    {
        await Task.Delay(0);
        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }
}