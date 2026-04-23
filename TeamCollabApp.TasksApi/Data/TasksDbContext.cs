using Microsoft.EntityFrameworkCore;
using TeamCollabApp.TasksApi.Models;

namespace TeamCollabApp.TasksApi.Data
{
    public class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
    {
        public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
        public DbSet<BoardTask> BoardTasks => Set<BoardTask>();
        public DbSet<TaskAssignee> TaskAssignees => Set<TaskAssignee>();
        public DbSet<TaskChecklistItem> TaskChecklistItems => Set<TaskChecklistItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskAssignee>()
                .HasKey(assignee => new { assignee.TaskId, assignee.UserId });

            modelBuilder.Entity<BoardColumn>()
                .Property(column => column.Title)
                .HasMaxLength(200);

            modelBuilder.Entity<BoardTask>()
                .Property(task => task.Title)
                .HasMaxLength(500);

        }
    }
}
