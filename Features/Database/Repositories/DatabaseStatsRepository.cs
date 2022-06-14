using SqliteWasmHelper;

namespace Trackor.Features.Database.Repositories;

public class DatabaseStatsRepository
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public DatabaseStatsRepository(ISqliteWasmDbContextFactory<TrackorContext> db)
    {
        _db = db;
    }

    public async Task<int> GetActivityLogItemCount() 
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.ActivityLogItems.Count();
    }

    public async Task<int> GetCategoryCount()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.Categories.Count();
    }

    public async Task<int> GetCodeSnippetCount()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.CodeSnippets.Count();
    }

    public async Task<int> GetLinkCount()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.Links.Count();
    }

    public async Task<int> GetProjectCount()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.Projects.Count();
    }

    public async Task<int> GetTaskCount()
    {
        using var dbContext = await _db.CreateDbContextAsync();
        return dbContext.TaskListItems.Count();
    }
}
