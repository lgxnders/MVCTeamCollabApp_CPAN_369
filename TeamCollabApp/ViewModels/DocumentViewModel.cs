using TeamCollabApp.Models;

namespace TeamCollabApp.ViewModels
{
    public class DocumentViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int DocumentId { get; set; }
        public string ContentJson { get; set; } = "{}";
        public ProjectRole CurrentUserRole { get; set; }
        public bool IsGuest { get; set; }
        public string CurrentUserDisplayName { get; set; } = string.Empty;
        public List<CommentViewModel> Comments { get; set; } = [];
        public List<ProjectMemberInfo> ProjectMembers { get; set; } = [];
    }

    public class ProjectMemberInfo
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CommentViewModel
    {
        public int Id { get; set; }
        public string CommentKey { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
