namespace TeamCollabApp.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public string? TypeFilter { get; set; }
        public List<SearchResultItem> Projects { get; set; } = [];
        public List<SearchResultItem> Tasks { get; set; } = [];
        public List<SearchResultItem> Members { get; set; } = [];
        public bool SearchPerformed { get; set; } = false;
        public bool ServiceUnavailable { get; set; } = false;
    }

    public class SearchResultItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? LinkUrl { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? WorkspaceType { get; set; }
    }
}
