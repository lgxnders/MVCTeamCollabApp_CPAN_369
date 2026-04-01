using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TeamCollabApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProjectMembership> ProjectMemberships { get; set; } = [];
    }
}
