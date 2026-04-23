namespace TeamCollabApp.TasksApi.Models
{
    public class BoardTask
    {
        public int Id { get; set; }
        public int ColumnId { get; set; }
        public BoardColumn Column { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.None;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TaskAssignee> Assignees { get; set; } = [];
        public ICollection<TaskChecklistItem> ChecklistItems { get; set; } = [];
    }
}
