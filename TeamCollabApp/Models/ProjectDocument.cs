namespace TeamCollabApp.Models
{
    public class ProjectDocument
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string ContentJson { get; set; } = "{}";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? LastEditedByUserId { get; set; }
        public ICollection<DocumentComment> Comments { get; set; } = [];
    }

    public class DocumentComment
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public ProjectDocument Document { get; set; } = null!;
        public string CommentKey { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
