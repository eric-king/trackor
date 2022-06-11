using Fluxor;
using Trackor.Features.ActivityLog;
using Trackor.Features.Database;
using Trackor.Features.Database.Repositories;

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
    private readonly CategoryRepository _repo;

    public CategoriesEffects(CategoryRepository repo)
    {
        _repo = repo;
    }

    [EffectMethod(typeof(CategoriesLoadAction))]
    public async Task OnLoadCategories(IDispatcher dispatcher)
    {
        var items = await _repo.Get();
        dispatcher.Dispatch(new CategoriesSetAction(items));
    }

    [EffectMethod]
    public async Task OnCategorySave(CategoriesSaveAction action, IDispatcher dispatcher)
    {
        var isNew = action.Category.Id == 0;

        var category = await _repo.Save(action.Category);

        if (isNew)
        {
            dispatcher.Dispatch(new CategoriesAddAction(category));
        }
        else
        {
            dispatcher.Dispatch(new CategoriesUpdateAction(category));
        }
    }

    [EffectMethod]
    public async Task OnCategoryDelete(CategoriesDeleteAction action, IDispatcher dispatcher)
    {
        await _repo.Delete(action.Category);
        dispatcher.Dispatch(new CategoriesRemoveAction(action.Category));

        // reload the Activity Log items because some of them
        // may have been assigned to this category
        dispatcher.Dispatch(new ActivityLogLoadItemsAction());
    }
}
