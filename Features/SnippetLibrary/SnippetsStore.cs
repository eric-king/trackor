using Fluxor;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Features.Database;

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
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public SnippetLibraryEffects(ISqliteWasmDbContextFactory<TrackorContext> db)
    {
        _db = db;
    }

    [EffectMethod]
    public async Task OnCodeSnippetSearch(SnippetLibrarySearchCodeSnippetsAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new SnippetLibrarySetCodeSnippetsAction(Array.Empty<CodeSnippet>()));

        using var dbContext = await _db.CreateDbContextAsync();
        var snippets = dbContext.CodeSnippets
            .Where(x => EF.Functions.Like(x.Label, $"%{action.SearchTerm}%") 
                     || EF.Functions.Like(x.Content, $"%{action.SearchTerm}%")
                     || EF.Functions.Like(x.Language, $"%{action.SearchTerm}%"))
            .OrderBy(x => x.Label)
            .ToArray();
        dispatcher.Dispatch(new SnippetLibrarySetCodeSnippetsAction(snippets));
    }

    [EffectMethod]
    public async Task OnCodeSnippetSave(SnippetLibrarySaveCodeSnippetAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (action.CodeSnippet.Id == 0)
        {
            dbContext.CodeSnippets.Add(action.CodeSnippet);
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new SnippetLibraryAddCodeSnippetAction(action.CodeSnippet));
        }
        else 
        {
            var tracking = dbContext.CodeSnippets.Attach(action.CodeSnippet);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new SnippetLibraryUpdateCodeSnippetAction(action.CodeSnippet));
        }
    }

    [EffectMethod]
    public async Task OnCodeSnippetDelete(SnippetLibraryDeleteCodeSnippetAction action, IDispatcher dispatcher) 
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var tracking = dbContext.CodeSnippets.Attach(action.CodeSnippet);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new SnippetLibraryRemoveCodeSnippetAction(action.CodeSnippet));
    }
}