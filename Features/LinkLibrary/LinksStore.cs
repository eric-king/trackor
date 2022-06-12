using Fluxor;
using Trackor.Features.Database;
using Trackor.Features.Database.Repositories;
using Trackor.Features.Notifications;

namespace Trackor.Features.LinkLibrary;

public record LinkLibrarySetLinksAction(LinkLibraryItem[] Links);
public record LinkLibrarySearchLinksAction(string SearchTerm);
public record LinkLibrarySaveLinkAction(LinkLibraryItem Link);
public record LinkLibraryAddLinkAction(LinkLibraryItem Link);
public record LinkLibraryUpdateLinkAction(LinkLibraryItem Link);
public record LinkLibraryDeleteLinkAction(LinkLibraryItem Link);
public record LinkLibraryRemoveLinkAction(LinkLibraryItem Link);
public record LinkLibraryEditLinkAction(LinkLibraryItem Link);

public record LinkLibraryState
{
    public LinkLibraryItem[] Links { get; init; }
}

public class LinksFeature : Feature<LinkLibraryState>
{
    public override string GetName() => "LinkLibrary";

    protected override LinkLibraryState GetInitialState()
    {
        return new LinkLibraryState
        {
            Links = Array.Empty<LinkLibraryItem>()
        };
    }
}

public static class LinksReducers
{
    [ReducerMethod]
    public static LinkLibraryState OnSetLinks(LinkLibraryState state, LinkLibrarySetLinksAction action)
    {
        return state with
        {
            Links = action.Links
        };
    }

    [ReducerMethod]
    public static LinkLibraryState OnAddLink(LinkLibraryState state, LinkLibraryAddLinkAction action)
    {
        var links = state.Links.ToList();
        links.Insert(0, action.Link);

        return state with
        {
            Links = links.ToArray()
        };
    }

    [ReducerMethod]
    public static LinkLibraryState OnUpdateLink(LinkLibraryState state, LinkLibraryUpdateLinkAction action)
    {
        var links = state.Links.ToList();
        var exists = links.Any(x => x.Id == action.Link.Id);
        if (!exists) { return state; }

        var existingLink = links.Single(x => x.Id == action.Link.Id);
        var existingIndex = links.IndexOf(existingLink);
        links.Remove(existingLink);
        links.Insert(existingIndex, action.Link);

        return state with
        {
            Links = links.ToArray()
        };
    }

    [ReducerMethod]
    public static LinkLibraryState OnRemoveLink(LinkLibraryState state, LinkLibraryRemoveLinkAction action)
    {
        var links = state.Links.ToList();
        var exists = links.Any(x => x.Id == action.Link.Id);
        if (!exists) { return state; }

        var existingLink = links.Single(x => x.Id == action.Link.Id);
        links.Remove(existingLink);

        return state with
        {
            Links = links.ToArray()
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static LinkLibraryState OnDatabaseDeleted(LinkLibraryState state)
    {
        return state with
        {
            Links = Array.Empty<LinkLibraryItem>()
        };
    }
}

public class LinksEffects
{
    private readonly LinkLibraryRepository _repo;

    public LinksEffects(LinkLibraryRepository repo)
    {
        _repo = repo;
    }

    [EffectMethod]
    public async Task OnLinkSearch(LinkLibrarySearchLinksAction action, IDispatcher dispatcher)
    {
        dispatcher.Dispatch(new LinkLibrarySetLinksAction(Array.Empty<LinkLibraryItem>()));

        var links = await _repo.Search(action.SearchTerm);
        if (!links.Any())
        {
            dispatcher.Dispatch(new SnackbarShowWarningAction($"No links match your search of {action.SearchTerm}"));
        }
        else
        {
            dispatcher.Dispatch(new LinkLibrarySetLinksAction(links));
        }
    }

    [EffectMethod]
    public async Task OnLinkSave(LinkLibrarySaveLinkAction action, IDispatcher dispatcher)
    {
        var isNew = action.Link.Id == 0;
        var link = await _repo.Save(action.Link);

        if (isNew)
        {
            dispatcher.Dispatch(new LinkLibraryAddLinkAction(link));
        }
        else
        {
            dispatcher.Dispatch(new LinkLibraryUpdateLinkAction(link));
        }
    }

    [EffectMethod]
    public async Task OnLinkDelete(LinkLibraryDeleteLinkAction action, IDispatcher dispatcher)
    {
        await _repo.Delete(action.Link);
        dispatcher.Dispatch(new LinkLibraryRemoveLinkAction(action.Link));
    }
}