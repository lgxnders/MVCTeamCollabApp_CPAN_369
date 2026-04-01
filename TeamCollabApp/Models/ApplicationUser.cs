using Microsoft.AspNetCore.Identity;
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-9.0&tabs=visual-studio

namespace TeamCollabApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectMembership> ProjectMemberships { get; set; } = [];
    }
}
