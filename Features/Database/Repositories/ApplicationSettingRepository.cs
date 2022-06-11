﻿using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace Trackor.Features.Database.Repositories;

public class ApplicationSettingRepository
{
    private readonly ISqliteWasmDbContextFactory<TrackorContext> _db;

    public ApplicationSettingRepository(ISqliteWasmDbContextFactory<TrackorContext> db)
    {
        _db = db;
    }

    public async Task<ApplicationSetting> GetOrAdd(string key, string defaultValue)
    {
        using var dbContext = await _db.CreateDbContextAsync();

        var appSetting = dbContext.ApplicationSettings.SingleOrDefault(x => x.Key == key);
        if (appSetting == null) 
        {
            appSetting = new ApplicationSetting { Key = key, Value = defaultValue };
            dbContext.ApplicationSettings.Add(appSetting);
            await dbContext.SaveChangesAsync();
        }

        return appSetting;
    }

    public async Task Update(string key, string value)
    {
        var dbContext = await _db.CreateDbContextAsync();
        var existing = dbContext.ApplicationSettings.FirstOrDefault(x => x.Key == key);
        existing.Value = value;
        dbContext.SaveChanges();
    }
}
