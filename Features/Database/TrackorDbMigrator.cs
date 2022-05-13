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
            {
            }

            {
                await ApplyDbVersionAsync(CurrentDbVersion);
                return CurrentDbVersion;
            }

            return dbVersion;
        }

        private async Task ApplyDbVersionAsync(string dbVersion)
        {
            var appSettingDbVersion = _dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == ApplicationSettingKeys.DbVersion);
            if (appSettingDbVersion is not null)
            {
                appSettingDbVersion.Value = dbVersion;
            }
            else
            {
                _dbContext.ApplicationSettings.Add(new ApplicationSetting { Key = ApplicationSettingKeys.DbVersion, Value = dbVersion });
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
    }
}
