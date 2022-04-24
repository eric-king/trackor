using Fluxor;
using Microsoft.JSInterop;

namespace Trackor.Features.Core;

public record CoreBuildDbDownloadUrlAction();
public record CoreSetDbDownloadUrlAction(string Url);

public record CoreState
{
    public string DbDownloadUrl { get; init; }
}

public class CoreFeature : Feature<CoreState>
{
    public override string GetName() => "Core";

    protected override CoreState GetInitialState()
    {
        return new CoreState
        {
            DbDownloadUrl = string.Empty
        };
    }
}

public static class CoreReducers
{
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
    private readonly IJSRuntime _js;
    private readonly IState<CoreState> _state;

    public CoreEffects(IJSRuntime jsRuntime, IState<CoreState> state)
    {
        _js = jsRuntime;
        _state = state;
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