using Fluxor;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Features.Database;

namespace Trackor.Features.TaskList;

public record TaskListLoadAction();
public record TaskListSetAction(TaskListItem[] Tasks);
public record TaskListAddTaskAction(TaskListItem Task);
public record TaskListEditTaskAction(TaskListItem Task);
public record TaskListSaveTaskAction(TaskListItem Task);
public record TaskListUpdateTaskAction(TaskListItem Task);
public record TaskListDeleteTaskAction(TaskListItem Task);
public record TaskListRemoveTaskAction(TaskListItem Task);

public record TaskListState
{
    public TaskListItem[] Tasks { get; init; }
}

public class TaskListSFeature : Feature<TaskListState>
{
    public override string GetName() => "TaskList";

    protected override TaskListState GetInitialState()
    {
        return new TaskListState
        {
            Tasks = Array.Empty<TaskListItem>()
        };
    }
}

public static class TaskListReducers
{
    [ReducerMethod]
    public static TaskListState OnSetTasks(TaskListState state, TaskListSetAction action)
    {
        return state with
        {
            Tasks = action.Tasks.OrderByDescending(x => x.Priority)
               .ThenByDescending(x => x.Due)
               .ThenBy(x => x.Narrative)
               .ToArray()
        };
    }

    [ReducerMethod]
    public static TaskListState OnTaskAdd(TaskListState state, TaskListAddTaskAction action)
    {
        var taskList = state.Tasks.ToList();
        taskList.Add(action.Task);

        var newTaskArray = taskList
               .OrderByDescending(x => x.Priority)
               .ThenByDescending(x => x.Due)
               .ThenBy(x => x.Narrative)
               .ToArray();

        return state with
        {
            Tasks = newTaskArray
        };
    }

    [ReducerMethod]
    public static TaskListState OnTaskUpdate(TaskListState state, TaskListUpdateTaskAction action)
    {
        var taskList = state.Tasks.ToList();
        var existingTask = taskList.Single(x => x.Id == action.Task.Id);
        taskList.Remove(existingTask);
        taskList.Add(action.Task);

        var newTaskArray = taskList
               .OrderByDescending(x => x.Priority)
               .ThenByDescending(x => x.Due)
               .ThenBy(x => x.Narrative)
               .ToArray();

        return state with
        {
            Tasks = newTaskArray
        };
    }

    [ReducerMethod]
    public static TaskListState OnTaskRemove(TaskListState state, TaskListRemoveTaskAction action)
    {
        var taskList = state.Tasks.ToList();
        var existingTask = taskList.Single(x => x.Id == action.Task.Id);
        taskList.Remove(existingTask);

        var newTaskArray = taskList
              .OrderByDescending(x => x.Priority)
              .ThenByDescending(x => x.Due)
              .ThenBy(x => x.Narrative)
              .ToArray();

        return state with
        {
            Tasks = newTaskArray
        };
    }
}

public class TaskListEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public TaskListEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _db = dbFactory;
    }

    [EffectMethod(typeof(TaskListLoadAction))]
    public async Task OnLoadTasks(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var items = dbContext.TaskListItems.ToArray();
        dispatcher.Dispatch(new TaskListSetAction(items));
    }

    [EffectMethod]
    public async Task OnTaskSave(TaskListSaveTaskAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (action.Task.Id == 0)
        {
            dbContext.TaskListItems.Add(action.Task);
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new TaskListAddTaskAction(action.Task));
        }
        else
        {
            var tracking = dbContext.TaskListItems.Attach(action.Task);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new TaskListUpdateTaskAction(action.Task));
        }
    }

    [EffectMethod]
    public async Task OnTaskDelete(TaskListDeleteTaskAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var tracking = dbContext.TaskListItems.Attach(action.Task);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new TaskListRemoveTaskAction(action.Task));
    }
}