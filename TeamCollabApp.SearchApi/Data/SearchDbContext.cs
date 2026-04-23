using Microsoft.EntityFrameworkCore;
using TeamCollabApp.SearchApi.Models;

namespace TeamCollabApp.SearchApi.Data
{
    public class SearchDbContext(DbContextOptions<SearchDbContext> options) : DbContext(options)
    {
        public DbSet<ProjectView> Projects => Set<ProjectView>();
        public DbSet<TaskView> Tasks => Set<TaskView>();
        public DbSet<MemberView> Members => Set<MemberView>();
        public DbSet<ProjectMembershipView> ProjectMemberships => Set<ProjectMembershipView>();
        public DbSet<BoardColumnView> BoardColumns => Set<BoardColumnView>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProjectView>(e =>
            {
                e.ToTable("Projects");
                e.HasKey(p => p.Id);
            });

            builder.Entity<TaskView>(e =>
            {
                e.ToTable("BoardTasks");
                e.HasKey(t => t.Id);
            });

            builder.Entity<MemberView>(e =>
            {
                e.ToTable("AspNetUsers");
                e.HasKey(m => m.Id);
            });

            builder.Entity<ProjectMembershipView>(e =>
            {
                e.ToTable("ProjectMemberships");
                e.HasKey(m => new { m.ProjectId, m.UserId });
            });

            builder.Entity<BoardColumnView>(e =>
            {
                e.ToTable("BoardColumns");
                e.HasKey(c => c.Id);
            });
        }
    }
}
