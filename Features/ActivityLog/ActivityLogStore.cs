using Fluxor;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.ActivityLog.Model;

namespace Trackor.Features.ActivityLog;

public record ActivityLogLoadItemsAction();
public record ActivityLogSetLogItemsAction(ActivityLogItem[] Items);
public record ActivityLogAddItemAction(ActivityLogItem Item);
public record ActivityLogSaveItemAction(ActivityLogItem Item);

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
    public static ActivityLogState OnActivityLogSetLogItems(ActivityLogState state, ActivityLogSetLogItemsAction action)
    {
        return state with
        {
            ActivityLogItems = action.Items,
            IsLoaded = true
        };
    }

    [ReducerMethod]
    public static ActivityLogState OnActivityLogAdd(ActivityLogState state, ActivityLogAddItemAction action)
    {
        var itemList = state.ActivityLogItems.ToList();
        itemList.Add(action.Item);

        return state with
        {
            ActivityLogItems = itemList.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToArray()
        };
    }
}

public class ActivityLogEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _dbFactory;

    public ActivityLogEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [EffectMethod(typeof(ActivityLogLoadItemsAction))]
    public async Task OnActivityLogLoad(IDispatcher dispatcher)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var items = dbContext.ActivityLogItems.ToArray();
        dispatcher.Dispatch(new ActivityLogSetLogItemsAction(items));
    }

    [EffectMethod]
    public async Task OnActivityLogSave(ActivityLogSaveItemAction action, IDispatcher dispatcher) 
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        dbContext.ActivityLogItems.Add(action.Item);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new ActivityLogAddItemAction(action.Item));
    }

}
