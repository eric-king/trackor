using Fluxor;
using Microsoft.JSInterop;

namespace Trackor.Features.Database;

public record DatabaseBuildDownloadUrlAction();
public record DatabaseSetDownloadUrlAction(string Url);

public record DatabaseState
{
    public string DownloadUrl { get; init; }
}

public class DatabaseFeature : Feature<DatabaseState>
{
    public override string GetName() => "Database";

    protected override DatabaseState GetInitialState()
    {
        return new DatabaseState
        {
            DownloadUrl = string.Empty
        };
    }
}

public static class CoreReducers
{
    [ReducerMethod]
    public static DatabaseState OnSetDownloadUrl(DatabaseState state, DatabaseSetDownloadUrlAction action)
    {
        return state with
        {
            DownloadUrl = action.Url
        };
    }
}

public class DatabaseEffects
{
    private readonly IJSRuntime _js;
    private readonly IState<DatabaseState> _state;

    public DatabaseEffects(IJSRuntime jsRuntime, IState<DatabaseState> state)
    {
        _js = jsRuntime;
        _state = state;
    }

    [EffectMethod(typeof(DatabaseBuildDownloadUrlAction))]
    public async Task OnBuildDbDownloadUrl(IDispatcher dispatcher)
    {
        // bail if the download url has already been generated
        if (!string.IsNullOrEmpty(_state.Value.DownloadUrl)) return;

        var dbModule = await _js.InvokeAsync<IJSObjectReference>("import", "./database.js");
        var downloadUrl = await dbModule.InvokeAsync<string>("generateDownloadUrl");

        if (!string.IsNullOrEmpty(downloadUrl))
        {
            dispatcher.Dispatch(new DatabaseSetDownloadUrlAction(downloadUrl));
        }
    }
}