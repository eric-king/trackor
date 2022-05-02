using Microsoft.EntityFrameworkCore;
using Trackor.Features.ActivityLog;
using Trackor.Features.Categories;
using Trackor.Features.Projects;

namespace Trackor.Features.Database
{
    public class TrackorContext : DbContext
    {
        public TrackorContext(DbContextOptions<TrackorContext> options) : base(options) { }

        public DbSet<ActivityLogItem> ActivityLogItems { get; set; } = null;
        public DbSet<Category> Categories { get; set; } = null;
        public DbSet<Project> Projects { get; set; } = null;
    }
}
