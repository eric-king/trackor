using Fluxor;

namespace Trackor.Features.ActivityLog;

public record ActivityLogLoadAction();
public record ActivityLogSetLoadedAction(bool IsLoaded);

public record ActivityLogState
{
    public bool IsLoaded { get; init; }
}

public class ActivityLogFeature : Feature<ActivityLogState>
{
    public override string GetName() => "ActivityLog";

    protected override ActivityLogState GetInitialState()
    {
        return new ActivityLogState
        {
            IsLoaded = false
        };
    }
}

public static class ActivityLogReducers
{
    [ReducerMethod]
    public static ActivityLogState OnActivityLogSetLoadedAction(ActivityLogState state, ActivityLogSetLoadedAction action)
    {
        return state with
        {
            IsLoaded = action.IsLoaded
        };
    }
}

public class ActivityLogEffects 
{
    [EffectMethod(typeof(ActivityLogLoadAction))]
    public async Task OnActivityLogSetLoaded(IDispatcher dispatcher) 
    {
        // simulate loading something
        await Task.Delay(500);
        dispatcher.Dispatch(new ActivityLogSetLoadedAction(true));
    }
}