using Trackor.Features.Database.Repositories;

namespace Trackor.Features.Database;

public static class DatabaseServiceExtensions
{
    public static IServiceCollection AddTrackorRepositories(this IServiceCollection services)
    {
        services.AddSingleton<ActivityLogRepository>();
        services.AddSingleton<ApplicationSettingRepository>();
        services.AddSingleton<CategoryRepository>();

        return services;
    }
}
