namespace TeamCollabApp.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string Name { get    ; set; } = string.Empty;
        public string? Description { get; set; }

        public string ShareToken { get; set; } = Guid.NewGuid().ToString("N");
        public bool IsPublicLink { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser Owner { get; set; } = null;

        public ICollection<ProjectMembership> Memberships { get; set; } = [];
    }
}
