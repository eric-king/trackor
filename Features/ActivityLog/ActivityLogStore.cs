using Fluxor;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.ActivityLog.Model;
using Trackor.Features.Database;

namespace Trackor.Features.ActivityLog;

public record ActivityLogLoadItemsAction();
public record ActivityLogSetLogItemsAction(ActivityLogItem[] Items);
public record ActivityLogAddItemAction(ActivityLogItem Item);
public record ActivityLogUpdateItemAction(ActivityLogItem Item);
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
            ActivityLogItems = action.Items.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToArray(),
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

    [ReducerMethod]
    public static ActivityLogState OnActivityLogEdit(ActivityLogState state, ActivityLogUpdateItemAction action)
    {
        var itemList = state.ActivityLogItems.ToList();
        var existingItem = itemList.Single(x => x.Id == action.Item.Id);
        itemList.Remove(existingItem);
        itemList.Add(action.Item);

        return state with
        {
            ActivityLogItems = itemList.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).ToArray()
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static ActivityLogState OnDatabaseDeleted(ActivityLogState state)
    {
        return state with
        {
            ActivityLogItems = Array.Empty<ActivityLogItem>(),
            IsLoaded = false
        };
    }
}

public class ActivityLogEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public ActivityLogEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _db = dbFactory;
    }

    [EffectMethod(typeof(ActivityLogLoadItemsAction))]
    public async Task OnActivityLogLoad(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        _ = await dbContext.Database.EnsureCreatedAsync();
        var items = dbContext.ActivityLogItems.ToArray();
        dispatcher.Dispatch(new ActivityLogSetLogItemsAction(items));
    }

    [EffectMethod]
    public async Task OnActivityLogSave(ActivityLogSaveItemAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (action.Item.Id == 0)
        {
            dbContext.ActivityLogItems.Add(action.Item);
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new ActivityLogAddItemAction(action.Item));
        }
        else 
        {
            var tracking = dbContext.ActivityLogItems.Attach(action.Item);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new ActivityLogUpdateItemAction(action.Item));
        }
        
    }
}
