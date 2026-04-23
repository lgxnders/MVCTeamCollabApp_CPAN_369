using TeamCollabApp.Models;

namespace TeamCollabApp.ViewModels
{
    public class BoardViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public ProjectRole CurrentUserRole { get; set; }
        public bool IsGuest { get; set; }
        public string CurrentUserDisplayName { get; set; } = string.Empty;
        public List<BoardColumnViewModel> Columns { get; set; } = [];
        public List<ProjectMemberSummary> ProjectMembers { get; set; } = [];
    }

    public class BoardColumnViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Position { get; set; }
        public List<BoardTaskViewModel> Tasks { get; set; } = [];
    }

    public class BoardTaskViewModel
    {
        public int Id { get; set; }
        public int ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public string Priority { get; set; } = "None";
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> AssigneeUserIds { get; set; } = [];
    }

    public class ProjectMemberSummary
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
