using Trackor.Features.Database.Repositories;

namespace Trackor.Features.Database;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddTrackorDb(this IServiceCollection services)
    {
        services.AddScoped<TrackorDbMigrator>();
        services.AddScoped<ActivityLogRepository>();
        services.AddScoped<ApplicationSettingRepository>();
        services.AddScoped<CategoryRepository>();
        services.AddScoped<CodeSnippetRepository>();
        services.AddScoped<DatabaseStatsRepository>();
        services.AddScoped<LinkLibraryRepository>();
        services.AddScoped<ProjectRepository>();
        services.AddScoped<TaskListRepository>();

        return services;
    }
}
