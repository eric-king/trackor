using Fluxor;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Features.ActivityLog;
using Trackor.Features.Database;

namespace Trackor.Features.Categories;

public record CategoriesLoadAction();
public record CategoriesEditCategoryAction(Category Category, bool CopyOnly);
public record CategoriesSetAction(Category[] Categories);
public record CategoriesSaveAction(Category Category);
public record CategoriesAddAction(Category Category);
public record CategoriesDeleteAction(Category Category);
public record CategoriesRemoveAction(Category Category);
public record CategoriesUpdateAction(Category Category);

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

        var newCategoryArray = categoryList
               .OrderBy(x => x.Title)
               .ToArray();

        return state with
        {
            Categories = newCategoryArray
        };
    }

    [ReducerMethod]
    public static CategoriesState OnCategoryUpdate(CategoriesState state, CategoriesUpdateAction action)
    {
        var categoryList = state.Categories.ToList();
        var existingCategory = categoryList.Single(x => x.Id == action.Category.Id);
        categoryList.Remove(existingCategory);
        categoryList.Add(action.Category);

        var newCategoryArray = categoryList
               .OrderBy(x => x.Title)
               .ToArray();

        return state with
        {
            Categories = newCategoryArray
        };
    }

    [ReducerMethod]
    public static CategoriesState OnCategoryRemove(CategoriesState state, CategoriesRemoveAction action)
    {
        var categoryList = state.Categories.ToList();
        var existingCategory = categoryList.Single(x => x.Id == action.Category.Id);
        categoryList.Remove(existingCategory);

        var newCategoryArray = categoryList
               .OrderBy(x => x.Title)
               .ToArray();

        return state with
        {
            Categories = newCategoryArray
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
        var items = dbContext.Categories.ToArray();
        dispatcher.Dispatch(new CategoriesSetAction(items));
    }

    [EffectMethod]
    public async Task OnCategorySave(CategoriesSaveAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (action.Category.Id == 0)
        {
            dbContext.Categories.Add(action.Category);
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new CategoriesAddAction(action.Category));
        }
        else
        {
            var tracking = dbContext.Categories.Attach(action.Category);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            dispatcher.Dispatch(new CategoriesUpdateAction(action.Category));
        }
    }

    [EffectMethod]
    public async Task OnCategoryDelete(CategoriesDeleteAction action, IDispatcher dispatcher)
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var logsWithThisCategory = dbContext.ActivityLogItems.Where(x => x.CategoryId == action.Category.Id);
        foreach (var log in logsWithThisCategory) { log.CategoryId = null; }
        var tracking = dbContext.Categories.Attach(action.Category);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
        dispatcher.Dispatch(new CategoriesRemoveAction(action.Category));
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }
}
