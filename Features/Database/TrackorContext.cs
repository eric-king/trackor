using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trackor.Features.ActivityLog;
using Trackor.Features.Categories;
using Trackor.Features.LinkLibrary;
using Trackor.Features.Projects;
using Trackor.Features.SnippetLibrary;
using Trackor.Features.TaskList;

namespace Trackor.Features.Database
{
    public class TrackorContext(DbContextOptions<TrackorContext> options) : DbContext(options)
    {
        public DbSet<ActivityLogItem> ActivityLogItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } 
        public DbSet<TaskListItem> TaskListItems { get; set; }
        public DbSet<CodeSnippet> CodeSnippets { get; set; }
        public DbSet<LinkLibraryItem> Links { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses. Convert the values to a supported type:
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }

}
