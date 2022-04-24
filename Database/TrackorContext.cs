using Microsoft.EntityFrameworkCore;
using Trackor.Features.ActivityLog.Model;
using Trackor.Features.Categories.Model;
using Trackor.Features.Projects.Model;

namespace Trackor.Database
{
    public class TrackorContext : DbContext
    {
        public TrackorContext(DbContextOptions<TrackorContext> options) : base(options) { }

        public DbSet<ActivityLogItem> ActivityLogItems { get; set; } = null;
        public DbSet<Category> Categories { get; set; } = null;
        public DbSet<Project> Projects { get; set; } = null;
    }
}
