using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.Models;

namespace TeamCollabApp.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Project>           Projects            => Set<Project>();
        public DbSet<ProjectMembership> ProjectMemberships  => Set<ProjectMembership>();
        public DbSet<GuestSession>      GuestSessions       => Set<GuestSession>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // construct project
            builder.Entity<Project>(e =>
            {
                e.HasIndex(p => p.ShareToken).IsUnique();
                e.Property(p => p.ShareToken).HasMaxLength(64);
                e.Property(p => p.Name).HasMaxLength(200).IsRequired();

                e.HasOne(p => p.Owner)
                    .WithMany()
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // construct guest session
            builder.Entity<GuestSession>(e =>
            {
                e.HasIndex(g => g.SessionToken).IsUnique();
                e.Property(g => g.SessionToken).HasMaxLength(128);
                e.Property(g => g.DisplayName).HasMaxLength(100);
            });

            // construct project membership
            builder.Entity<ProjectMembership>(e =>
            {
                e.HasIndex(m => new { m.ProjectId, m.UserId })
                    .IsUnique()
                    .HasFilter("[UserId] IS NOT NULL");

                e.HasIndex(m => new { m.ProjectId, m.GuestSessionId })
                    .IsUnique()
                    .HasFilter("[GuestSessionId] IS NOT NULL");

                e.HasOne(m => m.Project)
                    .WithMany(p => p.Memberships)
                    .HasForeignKey(m => m.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(m => m.User)
                    .WithMany(u => u.ProjectMemberships)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);

                e.HasOne(m => m.GuestSession)
                     .WithMany(g => g.ProjectMemberships)
                     .HasForeignKey(m => m.GuestSessionId)
                     .OnDelete(DeleteBehavior.Cascade)
                     .IsRequired(false);
            });
        }
    }
}
