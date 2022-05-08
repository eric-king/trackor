using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Trackor.Features.ActivityLog;
using Trackor.Features.Categories;
using Trackor.Features.Projects;

namespace Trackor.Features.Database
{
    public class TrackorContext : DbContext
    {
        /// <summary>
        /// FIXME: This is required for EF Core 6.0 as it is not compatible with trimming.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private static Type _keepDateOnly = typeof(DateOnly);

        public TrackorContext(DbContextOptions<TrackorContext> options) : base(options) { }

        public DbSet<ActivityLogItem> ActivityLogItems { get; set; } = null;
        public DbSet<Category> Categories { get; set; } = null;
        public DbSet<Project> Projects { get; set; } = null;
        public DbSet<ApplicationSetting> ApplicationSettings { get; set; } = null;
    }

}
