using Fluxor;
using Trackor.Features.ActivityLog.Model;

namespace Trackor.Features.ActivityLog;

public record ActivityLogLoadAction();
public record ActivityLogSetLogItemsAction(ActivityLogItem[] Items);
public record ActivityLogSetLoadedAction(bool IsLoaded);

public record ActivityLogState
{
    public bool IsLoaded { get; init; }
    public ActivityLogItem[] ActivityLogItems { get; init; }
}

public class ActivityLogFeature : Feature<ActivityLogState>
{
    public override string GetName() => "ActivityLog";

    protected override ActivityLogState GetInitialState()
    {
        return new ActivityLogState
        {
            IsLoaded = false,
            ActivityLogItems = Array.Empty<ActivityLogItem>()
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

    [ReducerMethod]
    public static ActivityLogState OnActivityLogSetLogItemsAction(ActivityLogState state, ActivityLogSetLogItemsAction action)
    {
        return state with
        {
            ActivityLogItems = action.Items,
            IsLoaded = true
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
        var items = BuildSampleActivityLogItems();

        dispatcher.Dispatch(new ActivityLogSetLogItemsAction(items));
    }

    private ActivityLogItem[] BuildSampleActivityLogItems()
    {
        ActivityLogItem[] activityLogItems = new ActivityLogItem[]
        {
        new ActivityLogItem { Id = 1, CategoryId = null, ProjectId = null, Title = "Test Item 1", Narrative = "This is test item 1", Date = DateOnly.FromDateTime(DateTime.Now), Duration = TimeSpan.FromMinutes(15) },
        new ActivityLogItem { Id = 2, CategoryId = null, ProjectId = null, Title = "Test Item 2", Narrative = "This is test item 2", Date = DateOnly.FromDateTime(DateTime.Now), Duration = TimeSpan.FromMinutes(20) },
        new ActivityLogItem { Id = 3, CategoryId = null, ProjectId = null, Title = "Test Item 3", Narrative = "This is test item 3", Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), Duration = TimeSpan.FromMinutes(35) },
        new ActivityLogItem { Id = 4, CategoryId = null, ProjectId = null, Title = "Test Item 4", Narrative = "This is test item 4", Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), Duration = TimeSpan.FromMinutes(22) },
        new ActivityLogItem { Id = 5, CategoryId = null, ProjectId = null, Title = "Test Item 5", Narrative = "This is test item 5", Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)), Duration = TimeSpan.FromMinutes(9) }
        };

        return activityLogItems;
    }
}
