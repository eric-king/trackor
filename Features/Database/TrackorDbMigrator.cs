using Microsoft.EntityFrameworkCore;

namespace Trackor.Features.Database
{
    public class TrackorDbMigrator
    {
        private const string CurrentDbVersion = "1.01";
        private readonly TrackorContext _dbContext;

        public TrackorDbMigrator(TrackorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> ApplyCurrentDbVersionAsync()
        {
            _dbContext.ApplicationSettings.Add(new ApplicationSetting { Key = ApplicationSettingKeys.DbVersion, Value = CurrentDbVersion });
            await _dbContext.SaveChangesAsync();
            return CurrentDbVersion;
        }

        public async Task<string> EnsureDbMigratedAsync(string dbVersion)
        {
            if (dbVersion == CurrentDbVersion) 
            {
                return CurrentDbVersion;
            }

            if (string.IsNullOrEmpty(dbVersion) || dbVersion == "1.0")
            {
                await Migrate_101_TaskListItems();
                dbVersion = CurrentDbVersion;
            }

            return dbVersion;
        }

        private async Task ApplyDbVersionAsync(string dbVersion)
        {
            _dbContext.ApplicationSettings.Add(new ApplicationSetting { Key = ApplicationSettingKeys.DbVersion, Value = dbVersion });
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
    }
}
