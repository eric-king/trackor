using Fluxor;
using Trackor.Features.ActivityLog;
using Trackor.Features.Database;
using Trackor.Features.Database.Repositories;

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
    private readonly ProjectRepository _repo;

    public ProjectsEffects(ProjectRepository repo)
    {
        _repo = repo;
    }

    [EffectMethod(typeof(ProjectsLoadAction))]
    public async Task OnProjectsLoad(IDispatcher dispatcher)
    {
        var items = await _repo.Get();
        dispatcher.Dispatch(new ProjectsSetAction(items));
    }

    [EffectMethod]
    public async Task OnProjectSave(ProjectsSaveAction action, IDispatcher dispatcher)
    {
        var isNew = action.Project.Id == 0;
        var project = await _repo.Save(action.Project);

        if (isNew)
        {
            dispatcher.Dispatch(new ProjectsAddAction(project));
        }
        else
        {
            dispatcher.Dispatch(new ProjectsUpdateAction(project));
        }
    }

    [EffectMethod]
    public async Task OnProjectDelete(ProjectsDeleteAction action, IDispatcher dispatcher)
    {
        await _repo.Delete(action.Project);
        
        dispatcher.Dispatch(new ProjectsRemoveAction(action.Project));

        // reload the Activity Log items because some of them
        // may have been assigned to this project
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }
}