using Microsoft.EntityFrameworkCore;
using Trackor.Features.Categories;

namespace Trackor.Features.Database.Repositories;

public class CategoryRepository(IDbContextFactory<TrackorContext> db)
{
    public async Task<Category[]> Get()
    {
        using var dbContext = await db.CreateDbContextAsync();
        var items = dbContext.Categories.OrderBy(x => x.Title).ToArray();

        return items;
    }

    public async Task<Category> Save(Category category)
    {
        using var dbContext = await db.CreateDbContextAsync();

        if (category.Id == 0)
        {
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            var tracking = dbContext.Categories.Attach(category);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }

        return category;
    }

    public async Task Delete(Category category)
    {
        using var dbContext = await db.CreateDbContextAsync();

        DisconnectActivityLogItemsFromCategory(category, dbContext);

        var tracking = dbContext.Categories.Attach(category);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
    }

    private static void DisconnectActivityLogItemsFromCategory(Category category, TrackorContext dbContext)
    {
        foreach (var activityLog in dbContext.ActivityLogItems.Where(x => x.CategoryId == category.Id))
        {
            activityLog.CategoryId = null;
        }
    }
}
