using Fluxor;
using Trackor.Features.Database;
using Trackor.Features.Database.Repositories;
using Trackor.Features.Notifications;

namespace Trackor.Features.SnippetLibrary;

public record SnippetLibrarySetCodeSnippetsAction(CodeSnippet[] CodeSnippets);
public record SnippetLibrarySearchCodeSnippetsAction(string SearchTerm);
public record SnippetLibrarySaveCodeSnippetAction(CodeSnippet CodeSnippet);
public record SnippetLibraryAddCodeSnippetAction(CodeSnippet CodeSnippet);
public record SnippetLibraryUpdateCodeSnippetAction(CodeSnippet CodeSnippet);
public record SnippetLibraryDeleteCodeSnippetAction(CodeSnippet CodeSnippet);
public record SnippetLibraryRemoveCodeSnippetAction(CodeSnippet CodeSnippet);
public record SnippetLibraryEditCodeSnippetAction(CodeSnippet CodeSnippet);

public record SnippetLibraryState
{
    public CodeSnippet[] CodeSnippets { get; init; }
}

public class SnippetLibraryFeature : Feature<SnippetLibraryState>
{
    public override string GetName() => "SnippetLibrary";

    protected override SnippetLibraryState GetInitialState()
    {
        return new SnippetLibraryState
        {
            CodeSnippets = Array.Empty<CodeSnippet>()
        };
    }
}

public static class SnippetLibraryReducers
{
    [ReducerMethod]
    public static SnippetLibraryState OnSetCodeSnippets(SnippetLibraryState state, SnippetLibrarySetCodeSnippetsAction action)
    {
        return state with
        {
            CodeSnippets = action.CodeSnippets
        };
    }

    [ReducerMethod]
    public static SnippetLibraryState OnAddCodeSnippet(SnippetLibraryState state, SnippetLibraryAddCodeSnippetAction action)
    {
        var snippets = state.CodeSnippets.ToList();
        snippets.Insert(0, action.CodeSnippet);

        return state with
        {
            CodeSnippets = snippets.ToArray()
        };
    }

    [ReducerMethod]
    public static SnippetLibraryState OnUpdateCodeSnippet(SnippetLibraryState state, SnippetLibraryUpdateCodeSnippetAction action)
    {
        var snippets = state.CodeSnippets.ToList();
        var exists = snippets.Any(x => x.Id == action.CodeSnippet.Id);
        if (!exists) { return state; }

        var existingSnippet = snippets.Single(x => x.Id == action.CodeSnippet.Id);
        var existingIndex = snippets.IndexOf(existingSnippet);
        snippets.Remove(existingSnippet);
        snippets.Insert(existingIndex, action.CodeSnippet);

        return state with
        {
            CodeSnippets = snippets.ToArray()
        };
    }

    [ReducerMethod]
    public static SnippetLibraryState OnRemoveCodeSnippet(SnippetLibraryState state, SnippetLibraryRemoveCodeSnippetAction action)
    {
        var snippets = state.CodeSnippets.ToList();
        var exists = snippets.Any(x => x.Id == action.CodeSnippet.Id);
        if (!exists) { return state; }

        var existingSnippet = snippets.Single(x => x.Id == action.CodeSnippet.Id);
        snippets.Remove(existingSnippet);

        return state with
        {
            CodeSnippets = snippets.ToArray()
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static SnippetLibraryState OnDatabaseDeleted(SnippetLibraryState state)
    {
        return state with
        {
            CodeSnippets = Array.Empty<CodeSnippet>()
        };
    }
}

public class SnippetLibraryEffects
{
    private readonly CodeSnippetRepository _repo;

    public SnippetLibraryEffects(CodeSnippetRepository repo)
    {
        _repo = repo;
    }

    [EffectMethod]
    public async Task OnCodeSnippetSearch(SnippetLibrarySearchCodeSnippetsAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SnippetLibrarySetCodeSnippetsAction(Array.Empty<CodeSnippet>()));
        
        var snippets = await _repo.Search(action.SearchTerm);
        if (!snippets.Any())
        {
            dispatcher.Dispatch(new SnackbarShowWarningAction($"No snippets match your search of {action.SearchTerm}"));
        }
        else 
        {
            dispatcher.Dispatch(new SnippetLibrarySetCodeSnippetsAction(snippets));
        }
    }

    [EffectMethod]
    public async Task OnCodeSnippetSave(SnippetLibrarySaveCodeSnippetAction action, IDispatcher dispatcher)
    {
        var isNew = action.CodeSnippet.Id == 0;
        var codeSnippet = await _repo.Save(action.CodeSnippet);

        if (isNew)
        {
            dispatcher.Dispatch(new SnippetLibraryAddCodeSnippetAction(codeSnippet));
        }
        else 
        {
            dispatcher.Dispatch(new SnippetLibraryUpdateCodeSnippetAction(codeSnippet));
        }
    }

    [EffectMethod]
    public async Task OnCodeSnippetDelete(SnippetLibraryDeleteCodeSnippetAction action, IDispatcher dispatcher) 
    {
        await _repo.Delete(action.CodeSnippet);
        dispatcher.Dispatch(new SnippetLibraryRemoveCodeSnippetAction(action.CodeSnippet));
    }
}