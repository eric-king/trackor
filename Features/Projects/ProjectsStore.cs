using Fluxor;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Features.ActivityLog;
using Trackor.Features.Database;

namespace Trackor.Features.Projects;

public record ProjectsLoadAction();
public record ProjectsEditProjectAction(Project Project, bool CopyOnly);
public record ProjectsSetAction(Project[] Projects);
public record ProjectsSaveAction(Project Project);
public record ProjectsAddAction(Project Project);
public record ProjectsUpdateAction(Project Project);
public record ProjectsDeleteAction(Project Project);
public record ProjectsRemoveAction(Project Project);

public record ProjectsState
{
    public Project[] Projects { get; init; }
}

public class ProjectsFeature : Feature<ProjectsState>
{
    public override string GetName() => "Projects";

    protected override ProjectsState GetInitialState()
    {
        return new ProjectsState
        {
            Projects = Array.Empty<Project>()
        };
    }
}

public static class ProjectsReducers
{
    [ReducerMethod]
    public static ProjectsState OnSetProducts(ProjectsState state, ProjectsSetAction action)
    {
        return state with
        {
            Projects = action.Projects
        };
    }

    [ReducerMethod]
    public static ProjectsState OnProjectAdd(ProjectsState state, ProjectsAddAction action)
    {
        var projectList = state.Projects.ToList();
        projectList.Add(action.Project);

        var newProjectArray = projectList
            .OrderBy(x => x.Title)
            .ToArray();

        return state with
        {
            Projects = newProjectArray
        };
    }

    [ReducerMethod]
    public static ProjectsState OnProjectUpdate(ProjectsState state, ProjectsUpdateAction action)
    {
        var projectList = state.Projects.ToList();
        var existingProject = projectList.Single(x => x.Id == action.Project.Id);
        projectList.Remove(existingProject);
        projectList.Add(action.Project);

        var newProjectArray = projectList
               .OrderBy(x => x.Title)
               .ToArray();

        return state with
        {
            Projects = newProjectArray
        };
    }

    [ReducerMethod]
    public static ProjectsState OnProjectRemove(ProjectsState state, ProjectsRemoveAction action)
    {
        var projectList = state.Projects.ToList();
        var existingProject = projectList.Single(x => x.Id == action.Project.Id);
        projectList.Remove(existingProject);

        var newProjectArray = projectList
               .OrderBy(x => x.Title)
               .ToArray();

        return state with
        {
            Projects = newProjectArray
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static ProjectsState OnDatabaseDeleted(ProjectsState state)
    {
        return state with
        {
            Projects = Array.Empty<Project>()
        };
    }
}

public class ProjectsEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public ProjectsEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _db = dbFactory;
    }

    [EffectMethod(typeof(ProjectsLoadAction))]
    public async Task OnProjectsLoad(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var items = dbContext.Projects.ToList().OrderBy(x => x.Title).ToArray();
        dispatcher.Dispatch(new ProjectsSetAction(items));
    }

    [EffectMethod]
    public async Task OnProjectSave(ProjectsSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (action.Project.Id == 0)
        {
            dbContext.Projects.Add(action.Project);
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new ProjectsAddAction(action.Project));
        }
        else
        {
            var tracking = dbContext.Projects.Attach(action.Project);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new ProjectsUpdateAction(action.Project));
        }
    }

    [EffectMethod]
    public async Task OnProjectDelete(ProjectsDeleteAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var logsWithThisProject = dbContext.ActivityLogItems.Where(x => x.ProjectId == action.Project.Id);
        foreach (var log in logsWithThisProject) { log.ProjectId = null; }
        var tracking = dbContext.Projects.Attach(action.Project);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new ProjectsRemoveAction(action.Project));
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }
}