using Fluxor;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.Projects.Model;

namespace Trackor.Features.Projects;

public record ProjectsLoadAction();
public record ProjectsSetAction(Project[] Projects);
public record ProjectsSaveAction(Project Project);
public record ProjectsAddAction(Project Project);

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

public static class CoreReducers
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

        return state with
        {
            Projects = projectList.OrderBy(x => x.Title).ToArray()
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
        var items = dbContext.Projects.ToArray();
        dispatcher.Dispatch(new ProjectsSetAction(items));
    }

    [EffectMethod]
    public async Task OnProjectsSave(ProjectsSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        dbContext.Projects.Add(action.Project);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new ProjectsAddAction(action.Project));
    }
}