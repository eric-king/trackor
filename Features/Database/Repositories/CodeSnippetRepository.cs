using Microsoft.EntityFrameworkCore;
using Trackor.Features.SnippetLibrary;

namespace Trackor.Features.Database.Repositories;

public class CodeSnippetRepository(IDbContextFactory<TrackorContext> db)
{
    public async Task<CodeSnippet[]> Search(string searchTerm) 
    {
        using var dbContext = await db.CreateDbContextAsync();
        var snippets = dbContext.CodeSnippets
            .Where(x => EF.Functions.Like(x.Label, $"%{searchTerm}%")
                     || EF.Functions.Like(x.Content, $"%{searchTerm}%")
                     || EF.Functions.Like(x.Language, $"%{searchTerm}%"))
            .OrderBy(x => x.Label)
            .ToArray();

        return snippets;
    }

    public async Task<CodeSnippet> Save(CodeSnippet codeSnippet)
    {
        using var dbContext = await db.CreateDbContextAsync();

        if (codeSnippet.Id == 0)
        {
            dbContext.CodeSnippets.Add(codeSnippet);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            var tracking = dbContext.CodeSnippets.Attach(codeSnippet);
            tracking.State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }

        return codeSnippet;
    }

    public async Task Delete(CodeSnippet codeSnippet)
    {
        using var dbContext = await db.CreateDbContextAsync();
        var tracking = dbContext.CodeSnippets.Attach(codeSnippet);
        tracking.State = EntityState.Deleted;
        await dbContext.SaveChangesAsync();
    }
}
