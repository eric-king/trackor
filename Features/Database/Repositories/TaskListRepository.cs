using Microsoft.EntityFrameworkCore;
using Trackor.Features.TaskList;

namespace Trackor.Features.Database.Repositories;

public class TaskListRepository(IDbContextFactory<TrackorContext> db)
{
    public async Task<TaskListItem[]> Get()
    {
        using var dbContext = await db.CreateDbContextAsync();
        var items = dbContext.TaskListItems.ToArray();
        return items;
    }

    public async Task<TaskListItem> Save(TaskListItem task)
    {
        using var dbContext = await db.CreateDbContextAsync();

        if (task.Id == 0)
        {
            dbContext.TaskListItems.Add(task);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            var tracking = dbContext.TaskListItems.Attach(task);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }

        return task;
    }

    public async Task Delete(TaskListItem task)
    {
        using var dbContext = await db.CreateDbContextAsync();

        var tracking = dbContext.TaskListItems.Attach(task);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
    }
}
