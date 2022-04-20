using Fluxor;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.Core.Model;

namespace Trackor.Features.Core;

public record CoreLoadCategoriesAction();
public record CoreLoadProjectsAction();
public record CoreSetCategoriesAction(Category[] Categories);
public record CoreSetProjectsAction(Project[] Projects);
public record CoreCategorySaveAction(Category Category);
public record CoreCategoryAddAction(Category Category);
public record CoreProjectSaveAction(Project Project);
public record CoreProjectAddAction(Project Project);

public record CoreState
{
    public Category[] Categories { get; init; }
    public Project[] Projects { get; init; }
}

public class CoreFeature : Feature<CoreState>
{
    public override string GetName() => "Core";

    protected override CoreState GetInitialState()
    {
        return new CoreState
        {
            Categories = Array.Empty<Category>(),
            Projects = Array.Empty<Project>()
        };
    }
}

public static class CoreReducers 
{
    [ReducerMethod]
    public static CoreState CoreSetCategories(CoreState state, CoreSetCategoriesAction action)
    {
        return state with
        {
            Categories = action.Categories
        };
    }

    [ReducerMethod]
    public static CoreState OnCategoryAdd(CoreState state, CoreCategoryAddAction action)
    {
        var categoryList = state.Categories.ToList();
        categoryList.Add(action.Category);

        return state with
        {
            Categories = categoryList.OrderBy(x => x.Title).ToArray()
        };
    }

    [ReducerMethod]
    public static CoreState CoreSetProducts(CoreState state, CoreSetProjectsAction action)
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
}

public class CoreEffects 
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _dbFactory;

    public CoreEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    [EffectMethod(typeof(CoreLoadCategoriesAction))]
    public async Task OnLoadCategories(IDispatcher dispatcher)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var items = dbContext.Categories.ToArray();
        dispatcher.Dispatch(new CoreSetCategoriesAction(items));
    }

    [EffectMethod(typeof(CoreLoadProjectsAction))]
    public async Task OnLoadProjects(IDispatcher dispatcher)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        var items = dbContext.Projects.ToArray();
        dispatcher.Dispatch(new CoreSetProjectsAction(items));
    }

    [EffectMethod]
    public async Task OnCategorySave(CoreCategorySaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        dbContext.Categories.Add(action.Category);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new CoreCategoryAddAction(action.Category));
    }

    [EffectMethod]
    public async Task OnProjectSave(CoreProjectSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _dbFactory.CreateDbContextAsync();
        dbContext.Projects.Add(action.Project);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new CoreProjectAddAction(action.Project));
    }
}