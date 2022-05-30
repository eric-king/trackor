using Microsoft.EntityFrameworkCore;

namespace Trackor.Features.Database
{
    public class TrackorDbMigrator
    {
        public const string APP_SETTING_DB_VERSION = "DbVersion";

        private const string CurrentDbVersion = "1.02";
        private readonly TrackorContext _dbContext;

        public TrackorDbMigrator(TrackorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> ApplyCurrentDbVersionAsync()
        {
            _dbContext.ApplicationSettings.Add(new ApplicationSetting { Key = APP_SETTING_DB_VERSION, Value = CurrentDbVersion });
            await _dbContext.SaveChangesAsync();
            return CurrentDbVersion;
        }

        public async Task<string> EnsureDbMigratedAsync(string dbVersion)
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
                dbVersion = CurrentDbVersion;
            }

            if (dbVersion == "1.01")
            {
                await Migrate_102_CodeSnippets();
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
    }
}
