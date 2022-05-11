﻿using Fluxor;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SqliteWasmHelper;
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

public record DatabaseState
{
    public string DownloadUrl { get; init; }
    public string DbVersion { get; init; }
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
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;
    private readonly IJSRuntime _js;
    private readonly IState<DatabaseState> _state;

    public DatabaseEffects(ISqliteWasmDbContextFactory<TrackorContext> db, IJSRuntime jsRuntime, IState<DatabaseState> state)
    {
        _db = db;
        _js = jsRuntime;
        _state = state;
    }

    [EffectMethod(typeof(DatabaseSetUpDbAction))]
    public async Task OnSetupDb(IDispatcher dispatcher)
    {
        var dbModule = await _js.InvokeAsync<IJSObjectReference>("import", "./database.js");
        var dbContext = await _db.CreateDbContextAsync();

        //var dbCreationSql = dbContext.Database.GenerateCreateScript();
        //Console.WriteLine("Db Creation SQL:");
        //Console.WriteLine(dbCreationSql);

        var migrator = new TrackorDbMigrator(dbContext);
        bool freshDbCreated = await dbContext.Database.EnsureCreatedAsync();

        string dbVersion;
        if (freshDbCreated)
        {
            dbVersion = await migrator.ApplyCurrentDbVersionAsync();
        }
        else 
        {
            var dbVersionAppSetting = await dbContext.ApplicationSettings.FirstOrDefaultAsync(x => x.Key == ApplicationSettingKeys.DbVersion);
            dbVersion = await migrator.EnsureDbMigratedAsync(dbVersionAppSetting?.Value);
        }

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
            var dbContext = await _db.CreateDbContextAsync();
            _ = await dbContext.Database.EnsureDeletedAsync();
            dispatcher.Dispatch(new DatabaseDeletedAction());
        }
    }

    [EffectMethod]
    public async Task OnUpload(DatabaseUploadAction action, IDispatcher dispatcher)
    {
            await _state.Value.DbCacheModule.InvokeVoidAsync("uploadDatabase", action.DbFile);
            dispatcher.Dispatch(new DatabaseUploadedAction());
    }
}