using Fluxor;
using Microsoft.JSInterop;
using Trackor.Features.Database.Repositories;
using Trackor.Features.Theme;

namespace Trackor.Features.Database;

public record DatabaseSetUpDbAction();
public record DatabaseSetDbCacheModuleAction(IJSObjectReference DbCacheModule);
public record DatabaseBuildDownloadUrlAction();
public record DatabaseSetDownloadUrlAction(string Url);
public record DatabaseSetDbVersionAction(string DbVersion);
public record DatabaseDeleteAction();
public record DatabaseDeletedAction();
public record DatabaseUploadAction(byte[] DbFile);
public record DatabaseUploadedAction();
public record DatabaseLoadStatsAction();
public record DatabaseSetStatsAction(int ActivityCount, int CategoryCount, int CodeSnippetCount, int LinkCount, int ProjectCount, int TaskCount);

public record DatabaseState
{
    public string DownloadUrl { get; init; }
    public string DbVersion { get; init; }
    public int ActivityCount { get; init; }
    public int CategoryCount { get; init; }
    public int CodeSnippetCount { get; init; }
    public int LinkCount { get; init; }
    public int ProjectCount { get; init; }
    public int TaskCount { get; init; }
    public IJSObjectReference DbCacheModule { get; init; }
}

public class DatabaseFeature : Feature<DatabaseState>
{
    public override string GetName() => "Database";

    protected override DatabaseState GetInitialState()
    {
        return new DatabaseState
        {
            DownloadUrl = string.Empty,
            DbVersion = "Unknown",
            ActivityCount = 0,
            CategoryCount = 0,
            CodeSnippetCount = 0,
            LinkCount = 0,
            ProjectCount = 0,
            TaskCount = 0,
            DbCacheModule = null
        };
    }
}

public static class CoreReducers
{
    [ReducerMethod]
    public static DatabaseState OnSetDbCacheModule(DatabaseState state, DatabaseSetDbCacheModuleAction action)
    {
        return state with
        {
            DbCacheModule = action.DbCacheModule
        };
    }

    [ReducerMethod]
    public static DatabaseState OnSetDbVersion(DatabaseState state, DatabaseSetDbVersionAction action)
    {
        return state with
        {
            DbVersion = action.DbVersion
        };
    }

    [ReducerMethod]
    public static DatabaseState OnSetDownloadUrl(DatabaseState state, DatabaseSetDownloadUrlAction action)
    {
        return state with
        {
            DownloadUrl = action.Url
        };
    }

    [ReducerMethod]
    public static DatabaseState OnSetStats(DatabaseState state, DatabaseSetStatsAction action)
    {
        return state with
        {
            ActivityCount = action.ActivityCount,
            CategoryCount = action.CategoryCount,
            CodeSnippetCount = action.CodeSnippetCount,
            LinkCount = action.LinkCount,
            ProjectCount = action.ProjectCount,
            TaskCount = action.TaskCount
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static DatabaseState OnDatabaseDeleted(DatabaseState state)
    {
        return state with
        {
            DownloadUrl = string.Empty
        };
    }
}

public class DatabaseEffects
{
    private readonly IJSRuntime _js;
    private readonly IState<DatabaseState> _state;
    private readonly TrackorDbMigrator _dbMigrator;
    private readonly DatabaseStatsRepository _statsRepo;

    public DatabaseEffects(TrackorDbMigrator dbMigrator, IJSRuntime jsRuntime, IState<DatabaseState> state, DatabaseStatsRepository statsRepo)
    {
        _dbMigrator = dbMigrator;
        _js = jsRuntime;
        _state = state;
        _statsRepo = statsRepo;
    }

    [EffectMethod(typeof(DatabaseSetUpDbAction))]
    public async Task OnSetupDb(IDispatcher dispatcher)
    {
        var dbModule = await _js.InvokeAsync<IJSObjectReference>("import", "./js/database.js");
        var dbVersion = await _dbMigrator.EnsureDbCreated();

        dispatcher.Dispatch(new DatabaseSetDbVersionAction(dbVersion));
        dispatcher.Dispatch(new DatabaseSetDbCacheModuleAction(dbModule));
        dispatcher.Dispatch(new ThemeLoadDarkModeAction());
    }

    [EffectMethod(typeof(DatabaseBuildDownloadUrlAction))]
    public async Task OnBuildDbDownloadUrl(IDispatcher dispatcher)
    {
        // bail if the download url has already been generated
        if (!string.IsNullOrEmpty(_state.Value.DownloadUrl)) return;

        var downloadUrl = await _state.Value.DbCacheModule.InvokeAsync<string>("generateDownloadUrl");

        if (!string.IsNullOrEmpty(downloadUrl))
        {
            dispatcher.Dispatch(new DatabaseSetDownloadUrlAction(downloadUrl));
        }
    }

    [EffectMethod(typeof(DatabaseDeleteAction))]
    public async Task OnDelete(IDispatcher dispatcher)
    {
        var success = await _state.Value.DbCacheModule.InvokeAsync<bool>("deleteDatabase");

        if (success)
        {
            await _dbMigrator.DeleteDatabase();
            dispatcher.Dispatch(new DatabaseDeletedAction());
        }
    }

    [EffectMethod]
    public async Task OnUpload(DatabaseUploadAction action, IDispatcher dispatcher)
    {
            await _state.Value.DbCacheModule.InvokeVoidAsync("uploadDatabase", action.DbFile);
            dispatcher.Dispatch(new DatabaseUploadedAction());
    }

    [EffectMethod(typeof(DatabaseLoadStatsAction))]
    public async Task OnLoadStats(IDispatcher dispatcher)
    {
        int activityCount = await _statsRepo.GetActivityLogItemCount();
        int categoryCount = await _statsRepo.GetCategoryCount();
        int codeSnippetCount = await _statsRepo.GetCodeSnippetCount();
        int linkCount = await _statsRepo.GetLinkCount();
        int projectCount = await _statsRepo.GetProjectCount();
        int taskCount = await _statsRepo.GetTaskCount();

        dispatcher.Dispatch(new DatabaseSetStatsAction(activityCount, categoryCount, codeSnippetCount, linkCount, projectCount, taskCount));
    }
}