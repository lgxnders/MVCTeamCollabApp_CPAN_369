using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.Data;
using TeamCollabApp.Models;

namespace TeamCollabApp.Services
{
    public class DatabaseSeeder(IServiceProvider services)
    {
        public async Task SeedAsync()
        {
            var db = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            if (await db.Projects.AnyAsync()) return;

            var users = new[]
            {
                new ApplicationUser { UserName = "alex@gmail.com",      Email = "alex@gmail.com",      DisplayName = "Alex",      EmailConfirmed = true },
                new ApplicationUser { UserName = "john@gmail.com",      Email = "john@gmail.com",      DisplayName = "John",      EmailConfirmed = true },
                new ApplicationUser { UserName = "jane@gmail.com",      Email = "jane@gmail.com",      DisplayName = "Jane",      EmailConfirmed = true },
                new ApplicationUser { UserName = "karen@gmail.com",     Email = "karen@gmail.com",     DisplayName = "Karen",     EmailConfirmed = true },
                new ApplicationUser { UserName = "joe@gmail.com",       Email = "joe@gmail.com",       DisplayName = "Joe",       EmailConfirmed = true },
                new ApplicationUser { UserName = "bob@gmail.com",       Email = "bob@gmail.com",       DisplayName = "Bob",       EmailConfirmed = true },
                new ApplicationUser { UserName = "sam@gmail.com",       Email = "sam@gmail.com",       DisplayName = "Sam",       EmailConfirmed = true },
                new ApplicationUser { UserName = "ruby@gmail.com",      Email = "ruby@gmail.com",      DisplayName = "Ruby",      EmailConfirmed = true },
                new ApplicationUser { UserName = "adam@gmail.com",      Email = "adam@gmail.com",      DisplayName = "Adam",      EmailConfirmed = true },
                new ApplicationUser { UserName = "cassandra@gmail.com", Email = "cassandra@gmail.com", DisplayName = "Cassandra", EmailConfirmed = true },
                new ApplicationUser { UserName = "austin@gmail.com",    Email = "austin@gmail.com",    DisplayName = "Austin",    EmailConfirmed = true },
                new ApplicationUser { UserName = "kevin@gmail.com",     Email = "kevin@gmail.com",     DisplayName = "Kevin",     EmailConfirmed = true },
            };

            foreach (var user in users)
            {
                if (await userManager.FindByEmailAsync(user.Email!) is null)
                    await userManager.CreateAsync(user, "Peanut!123!");
            }

            var alex      = await userManager.FindByEmailAsync("alex@gmail.com")      ?? users[0];
            var john      = await userManager.FindByEmailAsync("john@gmail.com")      ?? users[1];
            var jane      = await userManager.FindByEmailAsync("jane@gmail.com")      ?? users[2];
            var karen     = await userManager.FindByEmailAsync("karen@gmail.com")     ?? users[3];
            var joe       = await userManager.FindByEmailAsync("joe@gmail.com")       ?? users[4];
            var bob       = await userManager.FindByEmailAsync("bob@gmail.com")       ?? users[5];
            var sam       = await userManager.FindByEmailAsync("sam@gmail.com")       ?? users[6];
            var ruby      = await userManager.FindByEmailAsync("ruby@gmail.com")      ?? users[7];
            var adam      = await userManager.FindByEmailAsync("adam@gmail.com")      ?? users[8];
            var cassandra = await userManager.FindByEmailAsync("cassandra@gmail.com") ?? users[9];
            var austin    = await userManager.FindByEmailAsync("austin@gmail.com")    ?? users[10];
            var kevin     = await userManager.FindByEmailAsync("kevin@gmail.com")     ?? users[11];

            var projects = new List<Project>
            {
                new() { Name = "Website Redesign",       Description = "Overhaul the company marketing website.",               OwnerId = alex.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = true,  CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Name = "Mobile App v2",          Description = "Second major version of the iOS and Android apps.",     OwnerId = alex.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-25), UpdatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Name = "API Gateway Migration",  Description = "Move all services behind the new API gateway.",         OwnerId = john.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { Name = "Design System",          Description = "Build a shared component library and design tokens.",   OwnerId = jane.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = true,  CreatedAt = DateTime.UtcNow.AddDays(-18), UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Name = "Data Pipeline",          Description = "ETL pipeline for the analytics warehouse.",             OwnerId = karen.Id,     ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-15), UpdatedAt = DateTime.UtcNow.AddDays(-4) },
                new() { Name = "Security Audit",         Description = "Q2 penetration test and remediation tracking.",         OwnerId = joe.Id,       ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-12), UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Name = "Onboarding Portal",      Description = "Self-service onboarding for new employees.",            OwnerId = sam.Id,       ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = true,  CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Name = "Analytics Dashboard",    Description = "Real-time KPI dashboard for the executive team.",       OwnerId = ruby.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-8),  UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Name = "DevOps Tooling",         Description = "CI/CD pipeline improvements and Kubernetes migration.", OwnerId = adam.Id,      ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = false, CreatedAt = DateTime.UtcNow.AddDays(-6),  UpdatedAt = DateTime.UtcNow },
                new() { Name = "Customer Feedback Loop", Description = "Systematic collection and triage of user feedback.",    OwnerId = cassandra.Id, ShareToken = Guid.NewGuid().ToString("N")[..32], IsPublicLink = true,  CreatedAt = DateTime.UtcNow.AddDays(-4),  UpdatedAt = DateTime.UtcNow },
            };

            db.Projects.AddRange(projects);
            await db.SaveChangesAsync();

            var memberships = new List<ProjectMembership>
            {
                new() { ProjectId = projects[0].Id, UserId = alex.Id,   Role = ProjectRole.Owner,     JoinedAt = projects[0].CreatedAt },
                new() { ProjectId = projects[0].Id, UserId = john.Id,   Role = ProjectRole.Editor,    JoinedAt = projects[0].CreatedAt },
                new() { ProjectId = projects[0].Id, UserId = jane.Id,   Role = ProjectRole.Commenter, JoinedAt = projects[0].CreatedAt.AddDays(1) },
                new() { ProjectId = projects[0].Id, UserId = karen.Id,  Role = ProjectRole.Viewer,    JoinedAt = projects[0].CreatedAt.AddDays(2) },

                new() { ProjectId = projects[1].Id, UserId = alex.Id,   Role = ProjectRole.Owner,  JoinedAt = projects[1].CreatedAt },
                new() { ProjectId = projects[1].Id, UserId = joe.Id,    Role = ProjectRole.Editor,  JoinedAt = projects[1].CreatedAt },
                new() { ProjectId = projects[1].Id, UserId = bob.Id,    Role = ProjectRole.Viewer,  JoinedAt = projects[1].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[2].Id, UserId = john.Id,   Role = ProjectRole.Owner,  JoinedAt = projects[2].CreatedAt },
                new() { ProjectId = projects[2].Id, UserId = adam.Id,   Role = ProjectRole.Editor,  JoinedAt = projects[2].CreatedAt },
                new() { ProjectId = projects[2].Id, UserId = cassandra.Id, Role = ProjectRole.Editor, JoinedAt = projects[2].CreatedAt.AddDays(1) },
                new() { ProjectId = projects[2].Id, UserId = kevin.Id,  Role = ProjectRole.Viewer,  JoinedAt = projects[2].CreatedAt.AddDays(2) },

                new() { ProjectId = projects[3].Id, UserId = jane.Id,   Role = ProjectRole.Owner,     JoinedAt = projects[3].CreatedAt },
                new() { ProjectId = projects[3].Id, UserId = alex.Id,   Role = ProjectRole.Editor,    JoinedAt = projects[3].CreatedAt },
                new() { ProjectId = projects[3].Id, UserId = ruby.Id,   Role = ProjectRole.Commenter, JoinedAt = projects[3].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[4].Id, UserId = karen.Id,  Role = ProjectRole.Owner,  JoinedAt = projects[4].CreatedAt },
                new() { ProjectId = projects[4].Id, UserId = kevin.Id,  Role = ProjectRole.Editor,  JoinedAt = projects[4].CreatedAt },
                new() { ProjectId = projects[4].Id, UserId = cassandra.Id, Role = ProjectRole.Viewer, JoinedAt = projects[4].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[5].Id, UserId = joe.Id,    Role = ProjectRole.Owner,  JoinedAt = projects[5].CreatedAt },
                new() { ProjectId = projects[5].Id, UserId = john.Id,   Role = ProjectRole.Editor,  JoinedAt = projects[5].CreatedAt },
                new() { ProjectId = projects[5].Id, UserId = jane.Id,   Role = ProjectRole.Viewer,  JoinedAt = projects[5].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[6].Id, UserId = sam.Id,    Role = ProjectRole.Owner,     JoinedAt = projects[6].CreatedAt },
                new() { ProjectId = projects[6].Id, UserId = ruby.Id,   Role = ProjectRole.Editor,    JoinedAt = projects[6].CreatedAt },
                new() { ProjectId = projects[6].Id, UserId = adam.Id,   Role = ProjectRole.Commenter, JoinedAt = projects[6].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[7].Id, UserId = ruby.Id,   Role = ProjectRole.Owner,  JoinedAt = projects[7].CreatedAt },
                new() { ProjectId = projects[7].Id, UserId = karen.Id,  Role = ProjectRole.Editor,  JoinedAt = projects[7].CreatedAt },
                new() { ProjectId = projects[7].Id, UserId = alex.Id,   Role = ProjectRole.Viewer,  JoinedAt = projects[7].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[8].Id, UserId = adam.Id,   Role = ProjectRole.Owner,  JoinedAt = projects[8].CreatedAt },
                new() { ProjectId = projects[8].Id, UserId = john.Id,   Role = ProjectRole.Editor,  JoinedAt = projects[8].CreatedAt },
                new() { ProjectId = projects[8].Id, UserId = austin.Id, Role = ProjectRole.Editor,  JoinedAt = projects[8].CreatedAt.AddDays(1) },

                new() { ProjectId = projects[9].Id, UserId = cassandra.Id, Role = ProjectRole.Owner,     JoinedAt = projects[9].CreatedAt },
                new() { ProjectId = projects[9].Id, UserId = alex.Id,      Role = ProjectRole.Editor,    JoinedAt = projects[9].CreatedAt },
                new() { ProjectId = projects[9].Id, UserId = jane.Id,      Role = ProjectRole.Commenter, JoinedAt = projects[9].CreatedAt.AddDays(1) },
                new() { ProjectId = projects[9].Id, UserId = sam.Id,       Role = ProjectRole.Viewer,    JoinedAt = projects[9].CreatedAt.AddDays(2) },
            };

            db.ProjectMemberships.AddRange(memberships);
            await db.SaveChangesAsync();

            var documents = projects.Select(p => new ProjectDocument
            {
                ProjectId = p.Id,
                ContentJson = $"{{\"ops\":[{{\"insert\":\"Welcome to {p.Name}\\n\"}}]}}",
                UpdatedAt = p.UpdatedAt,
                LastEditedByUserId = p.OwnerId
            }).ToList();

            db.ProjectDocuments.AddRange(documents);
            await db.SaveChangesAsync();

            var comments = new List<DocumentComment>
            {
                new() { DocumentId = documents[0].Id, UserId = john.Id,      UserDisplayName = "John",      CommentText = "Should we add a hero video here?",        IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new() { DocumentId = documents[0].Id, UserId = jane.Id,      UserDisplayName = "Jane",      CommentText = "Agreed. I can source the footage.",         IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new() { DocumentId = documents[1].Id, UserId = joe.Id,       UserDisplayName = "Joe",       CommentText = "Navigation flow needs another pass.",        IsResolved = true,  CreatedAt = DateTime.UtcNow.AddDays(-6) },
                new() { DocumentId = documents[2].Id, UserId = adam.Id,      UserDisplayName = "Adam",      CommentText = "Rate limiting strategy still undefined.",    IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { DocumentId = documents[3].Id, UserId = alex.Id,      UserDisplayName = "Alex",      CommentText = "Token naming convention looks good to me.",  IsResolved = true,  CreatedAt = DateTime.UtcNow.AddDays(-7) },
                new() { DocumentId = documents[4].Id, UserId = kevin.Id,     UserDisplayName = "Kevin",     CommentText = "Partitioning strategy needs discussion.",    IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { DocumentId = documents[5].Id, UserId = john.Id,      UserDisplayName = "John",      CommentText = "OWASP Top 10 checklist is missing here.",    IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new() { DocumentId = documents[6].Id, UserId = ruby.Id,      UserDisplayName = "Ruby",      CommentText = "The SSO integration path is unclear.",       IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { DocumentId = documents[7].Id, UserId = karen.Id,     UserDisplayName = "Karen",     CommentText = "Daily vs. hourly rollup to confirm.",        IsResolved = true,  CreatedAt = DateTime.UtcNow.AddDays(-4) },
                new() { DocumentId = documents[8].Id, UserId = austin.Id,    UserDisplayName = "Austin",    CommentText = "Which Helm chart version are we targeting?", IsResolved = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new() { DocumentId = documents[9].Id, UserId = cassandra.Id, UserDisplayName = "Cassandra", CommentText = "NPS or CSAT as the primary metric?",         IsResolved = false, CreatedAt = DateTime.UtcNow },
            };

            db.DocumentComments.AddRange(comments);
            await db.SaveChangesAsync();
        }
    }
}
