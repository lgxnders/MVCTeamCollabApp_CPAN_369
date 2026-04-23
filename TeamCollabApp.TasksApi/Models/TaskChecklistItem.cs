namespace TeamCollabApp.TasksApi.Models
{
    public class TaskChecklistItem
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public BoardTask Task { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
        public int Position { get; set; }
    }
}
