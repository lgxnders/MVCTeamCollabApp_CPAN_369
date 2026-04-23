namespace TeamCollabApp.TasksApi.Models
{
    public class BoardColumn
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Position { get; set; }
        public ICollection<BoardTask> Tasks { get; set; } = [];
    }
}
