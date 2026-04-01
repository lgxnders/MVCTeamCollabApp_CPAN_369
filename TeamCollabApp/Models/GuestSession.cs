namespace TeamCollabApp.Models
{
    public class GuestSession
    {
        public int Id { get; set; }

        public string SessionToken { get; set; } = string.Empty;
        // Associated session token stored in the user's browser's local storage.

        public string DisplayName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(1);
        // Guest session expires in one day after creation.

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public ICollection<ProjectMembership> ProjectMemberships { get; set; } = [];
        // Which projects is this guest working on / been granted access to?
    }
}
