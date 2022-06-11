using Fluxor;
using Trackor.Features.Database;
using Trackor.Features.Database.Repositories;

namespace Trackor.Features.ActivityLog;

public record ActivityLogLoadItemsAction();
public record ActivityLogSetLogItemsAction(ActivityLogItem[] Items);
public record ActivityLogEditItemAction(ActivityLogItem Item, bool CopyOnly);
public record ActivityLogAddItemAction(ActivityLogItem Item);
public record ActivityLogUpdateItemAction(ActivityLogItem Item);
public record ActivityLogSaveItemAction(ActivityLogItem Item);
public record ActivityLogDeleteItemAction(ActivityLogItem Item);
public record ActivityLogRemoveItemAction(ActivityLogItem Item);
public record ActivityLogArchiveAction(DateOnly Start, DateOnly End);
public record ActivityLogUnarchiveAction(DateOnly Start, DateOnly End);

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
        var newItemArray = itemList
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .ToArray();

        return state with
        {
            ActivityLogItems = newItemArray
        };
    }

    [ReducerMethod]
    public static ActivityLogState OnActivityLogEdit(ActivityLogState state, ActivityLogUpdateItemAction action)
    {
        var itemList = state.ActivityLogItems.ToList();
        var existingItem = itemList.Single(x => x.Id == action.Item.Id);
        itemList.Remove(existingItem);
        itemList.Add(action.Item);
        var newItemArray = itemList
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .ToArray();

        return state with
        {
            ActivityLogItems = newItemArray
        };
    }

    [ReducerMethod]
    public static ActivityLogState OnActivityLogRemove(ActivityLogState state, ActivityLogRemoveItemAction action)
    {
        var itemList = state.ActivityLogItems.ToList();
        var existingItem = itemList.Single(x => x.Id == action.Item.Id);
        itemList.Remove(existingItem);
        var newItemArray = itemList
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .ToArray();

        return state with
        {
            ActivityLogItems = newItemArray
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
    private readonly ActivityLogRepository _repo;

    public ActivityLogEffects(ActivityLogRepository repo)
    {
        _repo = repo;
    }

    [EffectMethod(typeof(ActivityLogLoadItemsAction))]
    public async Task OnActivityLogLoad(IDispatcher dispatcher)
    {
        var items = await _repo.GetActive();
        dispatcher.Dispatch(new ActivityLogSetLogItemsAction(items));
    }

    [EffectMethod]
    public async Task OnActivityLogSave(ActivityLogSaveItemAction action, IDispatcher dispatcher)
    {
        var isNew = action.Item.Id == 0;
        var item = await _repo.Save(action.Item);
        if (isNew)
        {
            dispatcher.Dispatch(new ActivityLogAddItemAction(item));
        }
        else
        {
            dispatcher.Dispatch(new ActivityLogUpdateItemAction(item));
        }
    }

    [EffectMethod]
    public async Task OnActivityLogDelete(ActivityLogDeleteItemAction action, IDispatcher dispatcher)
    {
        await _repo.Delete(action.Item);
        dispatcher.Dispatch(new ActivityLogRemoveItemAction(action.Item));
    }

    [EffectMethod]
    public async Task OnActivityLogArchive(ActivityLogArchiveAction action, IDispatcher dispatcher)
    {
        await _repo.Archive(action.Start, action.End);
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }

    [EffectMethod]
    public async Task OnActivityLogUnarchive(ActivityLogUnarchiveAction action, IDispatcher dispatcher)
    {
        await _repo.Unarchive(action.Start, action.End);
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }
}
