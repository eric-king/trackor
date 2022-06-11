using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;
using Trackor.Features.Projects;

namespace Trackor.Features.Database.Repositories;

public class ProjectRepository
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public ProjectRepository(ISqliteWasmDbContextFactory<TrackorContext> db)
    {
        _db = db;
    }

    public async Task<Project[]> Get()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        var items = dbContext.Projects.OrderBy(x => x.Title).ToArray();

        return items;
    }

    public async Task<Project> Save(Project project)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        if (project.Id == 0)
        {
            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            var tracking = dbContext.Projects.Attach(project);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }

        return project;
    }

    public async Task Delete(Project project)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        DisconnectActivityLogItemsFromProject(project, dbContext);

        var tracking = dbContext.Projects.Attach(project);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
    }

    private static void DisconnectActivityLogItemsFromProject(Project project, TrackorContext dbContext)
    {
        foreach (var activityLog in dbContext.ActivityLogItems.Where(x => x.ProjectId == project.Id))
        {
            activityLog.ProjectId = null;
        }
    }
}
