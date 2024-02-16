using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Trackor.Features.ActivityLog;
using Trackor.Features.Categories;
using Trackor.Features.LinkLibrary;
using Trackor.Features.Projects;
using Trackor.Features.SnippetLibrary;
using Trackor.Features.TaskList;

namespace Trackor.Features.Database
{
    public class TrackorContext : DbContext
    {
        public TrackorContext(DbContextOptions<TrackorContext> options) : base(options) { }

        public DbSet<ActivityLogItem> ActivityLogItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } 
        public DbSet<TaskListItem> TaskListItems { get; set; }
        public DbSet<CodeSnippet> CodeSnippets { get; set; }
        public DbSet<LinkLibraryItem> Links { get; set; }
    }

}
