namespace TeamCollabApp.SearchApi.Models
{
    public class ProjectView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TaskView
    {
        public int Id { get; set; }
        public int ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class MemberView
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class ProjectMembershipView
    {
        public int ProjectId { get; set; }
        public string? UserId { get; set; }
    }

    public class BoardColumnView
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
    }
}
