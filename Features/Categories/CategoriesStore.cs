using Fluxor;
using SqliteWasmHelper;
using Trackor.Database;
using Trackor.Features.Categories.Model;
using Trackor.Features.Database;

namespace Trackor.Features.Categories;

public record CategoriesLoadAction();
public record CategoriesSetAction(Category[] Categories);
public record CategoriesSaveAction(Category Category);
public record CategoriesAddAction(Category Category);

public record CategoriesState
{
    public Category[] Categories { get; init; }
}

public class CategoriesFeature : Feature<CategoriesState>
{
    public override string GetName() => "Categories";

    protected override CategoriesState GetInitialState()
    {
        return new CategoriesState
        {
            Categories = Array.Empty<Category>()
        };
    }
}

public static class CategoriesReducers
{
    [ReducerMethod]
    public static CategoriesState OnSetCategories(CategoriesState state, CategoriesSetAction action)
    {
        return state with
        {
            Categories = action.Categories
        };
    }

    [ReducerMethod]
    public static CategoriesState OnCategoryAdd(CategoriesState state, CategoriesAddAction action)
    {
        var categoryList = state.Categories.ToList();
        categoryList.Add(action.Category);

        return state with
        {
            Categories = categoryList.OrderBy(x => x.Title).ToArray()
        };
    }

    [ReducerMethod(typeof(DatabaseDeletedAction))]
    public static CategoriesState OnDatabaseDeleted(CategoriesState state)
    {
        return state with
        {
            Categories = Array.Empty<Category>()
        };
    }
}

public class CategoriesEffects
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public CategoriesEffects(ISqliteWasmDbContextFactory<TrackorContext> dbFactory)
    {
        _db = dbFactory;
    }

    [EffectMethod(typeof(CategoriesLoadAction))]
    public async Task OnLoadCategories(IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        _ = await dbContext.Database.EnsureCreatedAsync();
        var items = dbContext.Categories.ToArray();
        dispatcher.Dispatch(new CategoriesSetAction(items));
    }

    [EffectMethod]
    public async Task OnCategorySave(CategoriesSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        dbContext.Categories.Add(action.Category);
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new CategoriesAddAction(action.Category));
    }
}
