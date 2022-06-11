using Fluxor;
using Trackor.Features.Database.Repositories;

namespace Trackor.Features.TaskList;

public record TaskListLoadAction();
public record TaskListSetAction(TaskListItem[] Tasks);
public record TaskListSetUseArrowsAction(bool UseArrows);
public record TaskListSaveUseArrowsAction(bool UseArrows);
public record TaskListAddTaskAction(TaskListItem Task);
public record TaskListEditTaskAction(TaskListItem Task);
public record TaskListSaveTaskAction(TaskListItem Task);
public record TaskListUpdateTaskAction(TaskListItem Task);
public record TaskListDeleteTaskAction(TaskListItem Task);
public record TaskListRemoveTaskAction(TaskListItem Task);

public record TaskListState
{
    public TaskListItem[] Tasks { get; init; }
    public bool UseArrows { get; init; }
}

public class TaskListSFeature : Feature<TaskListState>
{
    public override string GetName() => "TaskList";

    protected override TaskListState GetInitialState()
    {
        return new TaskListState
        {
            Tasks = Array.Empty<TaskListItem>(),
            UseArrows = false
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
    public static TaskListState OnSetUseArrows(TaskListState state, TaskListSetUseArrowsAction action)
    {
        return state with
        {
            UseArrows = action.UseArrows
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
    private const string APP_SETTING_USE_ARROWS = "TaskListUseArrows";
    private readonly TaskListRepository _taskListRepo;
    private readonly ApplicationSettingRepository _appSettingRepo;

    public TaskListEffects(TaskListRepository taskListRepo, ApplicationSettingRepository appSettingRepo)
    {
        _taskListRepo = taskListRepo;
        _appSettingRepo = appSettingRepo;
    }

    [EffectMethod(typeof(TaskListLoadAction))]
    public async Task OnLoadTasks(IDispatcher dispatcher)
    {
        var appSetting = await _appSettingRepo.GetOrAdd(APP_SETTING_USE_ARROWS, defaultValue: "false");
        dispatcher.Dispatch(new TaskListSetUseArrowsAction(bool.Parse(appSetting.Value)));

        var items = await _taskListRepo.Get();
        dispatcher.Dispatch(new TaskListSetAction(items));
    }

    [EffectMethod]
    public async Task OnSaveUseArrows(TaskListSaveUseArrowsAction action, IDispatcher dispatcher)
    {
        await _appSettingRepo.Update(APP_SETTING_USE_ARROWS, action.UseArrows.ToString());
        dispatcher.Dispatch(new TaskListSetUseArrowsAction(action.UseArrows));
    }

    [EffectMethod]
    public async Task OnTaskSave(TaskListSaveTaskAction action, IDispatcher dispatcher)
    {
        var isNew = action.Task.Id == 0;
        var task = await _taskListRepo.Save(action.Task);

        if (isNew)
        {
            dispatcher.Dispatch(new TaskListAddTaskAction(task));
        }
        else
        {
            dispatcher.Dispatch(new TaskListUpdateTaskAction(task));
        }
    }

    [EffectMethod]
    public async Task OnTaskDelete(TaskListDeleteTaskAction action, IDispatcher dispatcher)
    {
        await _taskListRepo.Delete(action.Task);
        dispatcher.Dispatch(new TaskListRemoveTaskAction(action.Task));
    }
}
