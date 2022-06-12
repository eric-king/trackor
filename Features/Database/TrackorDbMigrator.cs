using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace Trackor.Features.Database
{
    public class TrackorDbMigrator
    {
        private const string APP_SETTING_DB_VERSION = "DbVersion";
        private const string CurrentDbVersion = "1.03";
        private TrackorContext _dbContext;
        private readonly ISqliteWasmDbContextFactory<TrackorContext> _dbContextFactory;

        public TrackorDbMigrator(ISqliteWasmDbContextFactory<TrackorContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<string> EnsureDbCreated()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();
            
            // uncomment the following line to see the CREATE TABLE
            // scripts in the browser console when generating a new
            // database - this is useful when creating new migrations
            //OutputDbScriptToConsole();

            _ = await _dbContext.Database.EnsureCreatedAsync();
            var dbVersionAppSetting = await _dbContext.ApplicationSettings.FirstOrDefaultAsync(x => x.Key == APP_SETTING_DB_VERSION);
            var dbVersion = await EnsureDbMigratedAsync(dbVersionAppSetting?.Value);

            return dbVersion;
        }

        public async Task DeleteDatabase()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();
            _ = await _dbContext.Database.EnsureDeletedAsync();
        }

        private async Task<string> EnsureDbMigratedAsync(string dbVersion)
        {
            if (dbVersion == CurrentDbVersion)
            {
                return dbVersion;
            }

            if (dbVersion is null)
            {
                await ApplyDbVersionAsync(CurrentDbVersion);
                return CurrentDbVersion;
            }

            if (dbVersion == "1.0")
            {
                await Migrate_101_TaskListItems();
                await Migrate_102_CodeSnippets();
                await Migrate_103_Links();
                dbVersion = CurrentDbVersion;
            }

            if (dbVersion == "1.01")
            {
                await Migrate_102_CodeSnippets();
                await Migrate_103_Links();
                dbVersion = CurrentDbVersion;
            }

            if (dbVersion == "1.02")
            {
                await Migrate_103_Links();
                dbVersion = CurrentDbVersion;
            }

            return dbVersion;
        }

        private async Task ApplyDbVersionAsync(string dbVersion)
        {
            var appSettingDbVersion = _dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == APP_SETTING_DB_VERSION);
            if (appSettingDbVersion is not null)
            {
                appSettingDbVersion.Value = dbVersion;
            }
            else
            {
                _dbContext.ApplicationSettings.Add(new ApplicationSetting { Key = APP_SETTING_DB_VERSION, Value = dbVersion });
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task Migrate_101_TaskListItems()
        {
            const string Create_Table_Task_List_Items = @"CREATE TABLE ""TaskListItems"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_TaskListItems"" PRIMARY KEY AUTOINCREMENT,
                ""CategoryId"" INTEGER NULL,
                ""ProjectId"" INTEGER NULL,
                ""Narrative"" TEXT NULL,
                ""Priority"" INTEGER NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Due"" TEXT NULL
            );";

            _ = await _dbContext.Database.ExecuteSqlRawAsync(Create_Table_Task_List_Items);
            await ApplyDbVersionAsync("1.01");
        }

        private async Task Migrate_102_CodeSnippets()
        {
            const string Create_Table_CODE_SNIPPETS = @"CREATE TABLE ""CodeSnippets"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_CodeSnippets"" PRIMARY KEY AUTOINCREMENT,
                ""Label"" TEXT NULL,
                ""Content"" TEXT NULL,
                ""Language"" TEXT NULL,
                ""SourceUrl"" TEXT NULL
            );";

            _ = await _dbContext.Database.ExecuteSqlRawAsync(Create_Table_CODE_SNIPPETS);
            await ApplyDbVersionAsync("1.02");
        }

        private async Task Migrate_103_Links()
        {
            const string Create_Table_LINKS = @"CREATE TABLE ""Links"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_Links"" PRIMARY KEY AUTOINCREMENT,
                ""Url"" TEXT NULL,
                ""Label"" TEXT NULL,
                ""Description"" TEXT NULL
            );";

            _ = await _dbContext.Database.ExecuteSqlRawAsync(Create_Table_LINKS);
            await ApplyDbVersionAsync("1.03");
        }

        private void OutputDbScriptToConsole()
        {
            var dbCreationSql = _dbContext.Database.GenerateCreateScript();
            Console.WriteLine("Db Creation SQL:");
            Console.WriteLine(dbCreationSql);
        }
    }
}
