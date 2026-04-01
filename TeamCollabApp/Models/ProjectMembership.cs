namespace TeamCollabApp.Models
{
    public enum ProjectRole
    {
        Viewer = 0,
        Commenter = 1,
        Editor = 2,
        Owner = 3
    }

    /*
     * Represents a registered user or guest's membership in a project.
     * A project may have multiple ProjectMemberships.
     * One user can have one ProjectMembership for a project at a time.
    */
    public class ProjectMembership
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        // Which project is this membership associated with?

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        // If this membership includes a registered user.

        public int? GuestSessionId { get; set; }
        public GuestSession? GuestSession { get; set; }
        // if this membership includes a guest / unregistered user.

        public ProjectRole Role { get; set; } = ProjectRole.Viewer;
        // By default, set the role to viewer.

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public string DisplayName => User?.DisplayName ?? GuestSession?.DisplayName ?? "UnnamedGuest";
        // Determine what the display name should be based on the type of user.
    }
}
