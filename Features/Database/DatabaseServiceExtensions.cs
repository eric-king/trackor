using Trackor.Features.Database.Repositories;

namespace Trackor.Features.Database;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddTrackorDb(this IServiceCollection services)
    {
        services.AddSingleton<TrackorDbMigrator>();
        services.AddSingleton<ActivityLogRepository>();
        services.AddSingleton<ApplicationSettingRepository>();
        services.AddSingleton<CategoryRepository>();
        services.AddSingleton<CodeSnippetRepository>();
        services.AddSingleton<DatabaseStatsRepository>();
        services.AddSingleton<LinkLibraryRepository>();
        services.AddSingleton<ProjectRepository>();
        services.AddSingleton<TaskListRepository>();

        return services;
    }
}
