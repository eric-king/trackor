using Fluxor;
using Microsoft.JSInterop;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.Core.Model;

namespace Trackor.Features.Core;

public record CoreLoadProjectsAction();
public record CoreSetProjectsAction(Project[] Projects);
public record CoreProjectSaveAction(Project Project);
public record CoreProjectAddAction(Project Project);

public record CoreBuildDbDownloadUrlAction();
public record CoreSetDbDownloadUrlAction(string Url);

public record CoreState
{
    public Project[] Projects { get; init; }
    public string DbDownloadUrl { get; init; }
}

public class CoreFeature : Feature<CoreState>
{
    public override string GetName() => "Core";

    protected override CoreState GetInitialState()
    {
        return new CoreState
        {
            Projects = Array.Empty<Project>(),
            DbDownloadUrl = string.Empty
        };
    }
}

public static class CoreReducers
{
    [ReducerMethod]
    public static CoreState OnSetProducts(CoreState state, CoreSetProjectsAction action)
    {
        return state with
        {
            Projects = action.Projects
        };
    }

    [ReducerMethod]
    public static CoreState OnProjectAdd(CoreState state, CoreProjectAddAction action)
    {
        var projectList = state.Projects.ToList();
        projectList.Add(action.Project);

        return state with
        {
            Projects = projectList.OrderBy(x => x.Title).ToArray()
        };
    }

    [ReducerMethod]
    public static CoreState OnSetDbDownloadUrl(CoreState state, CoreSetDbDownloadUrlAction action)
    {
        return state with
        {
            DbDownloadUrl = action.Url
        };
    }
}

public class CoreEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;
    private readonly IJSRuntime _js;
    private readonly IState<CoreState> _state;

    public CoreEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory, IJSRuntime jsRuntime, IState<CoreState> state)
    {
        _db = dbFactory;
        _js = jsRuntime;
        _state = state;
    }

    [EffectMethod(typeof(CoreLoadProjectsAction))]
    public async Task OnLoadProjects(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var items = dbContext.Projects.ToArray();
        dispatcher.Dispatch(new CoreSetProjectsAction(items));
    }

    [EffectMethod]
    public async Task OnProjectSave(CoreProjectSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        dbContext.Projects.Add(action.Project);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new CoreProjectAddAction(action.Project));
    }

    [EffectMethod(typeof(CoreBuildDbDownloadUrlAction))]
    public async Task OnBuildDbDownloadUrl(IDispatcher dispatcher)
    {
        // bail if the download url has already been generated
        if (!string.IsNullOrEmpty(_state.Value.DbDownloadUrl)) return;

        var jsModule = await _js.InvokeAsync<IJSObjectReference>("import", "./dbDownload.js");
        var downloadUrl = await jsModule.InvokeAsync<string>("generateDownloadUrl");

        if (!string.IsNullOrEmpty(downloadUrl))
        {
            dispatcher.Dispatch(new CoreSetDbDownloadUrlAction(downloadUrl));
        }
    }
}