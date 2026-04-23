namespace TeamCollabApp.TasksApi.Models
{
    public class TaskAssignee
    {
        public int TaskId { get; set; }
        public BoardTask Task { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
    }
}
