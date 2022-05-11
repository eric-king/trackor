namespace Trackor.Features.Database
{
    public class TrackorDbMigrator
    {
        private const string CurrentDbVersion = "1.0";
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
            if (string.IsNullOrEmpty(dbVersion)) // Must be a pre-release database
            {
                return await ApplyCurrentDbVersionAsync();
            }

            if (dbVersion == CurrentDbVersion) // Up to date!
            {
                return CurrentDbVersion;
            }

            // need to migrate to CurrentDbVersion
            // coming soon
            //_ = await _dbContext.Database.ExecuteSqlRawAsync("Some Sql");
            await Task.Yield();
            return dbVersion;
        }
    }
}
