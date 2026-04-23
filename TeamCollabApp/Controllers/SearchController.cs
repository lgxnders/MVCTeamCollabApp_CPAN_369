using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamCollabApp.Data;
using TeamCollabApp.HttpClients;
using TeamCollabApp.Models;
using TeamCollabApp.ViewModels;

namespace TeamCollabApp.Controllers
{
    [Authorize]
    public class SearchController(
        SearchClient searchClient,
        UserManager<ApplicationUser> userManager,
        AppDbContext db) : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<IActionResult> Index(string? q, string? type)
        {
            var vm = new SearchViewModel { Query = q ?? string.Empty, TypeFilter = type };

            if (string.IsNullOrWhiteSpace(q))
                return View(vm);

            vm.SearchPerformed = true;
            var userId = userManager.GetUserId(User);
            if (userId is null) return Unauthorized();

            try
            {
                var raw = await searchClient.SearchAsync<JsonElement>(q, userId);

                // Build a project name lookup from search results first.
                var projectNameMap = new Dictionary<int, string>();

                if (raw.TryGetProperty("projects", out var projects) && projects.ValueKind == JsonValueKind.Array)
                {
                    vm.Projects = projects.EnumerateArray()
                        .Select(p =>
                        {
                            var id = p.GetProperty("id").GetInt32();
                            var name = p.GetProperty("name").GetString() ?? string.Empty;
                            projectNameMap[id] = name;
                            return new SearchResultItem
                            {
                                Id = id.ToString(),
                                Title = name,
                                Subtitle = p.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
                                LinkUrl = Url.Action("Details", "Projects", new { id })
                            };
                        }).ToList();
                }

                if (raw.TryGetProperty("tasks", out var tasks) && tasks.ValueKind == JsonValueKind.Array)
                {
                    // Collect any project IDs not already in the map so we can look them up.
                    var taskItems = tasks.EnumerateArray().Select(t =>
                    {
                        int? pid = t.TryGetProperty("projectId", out var pidEl) && pidEl.ValueKind != JsonValueKind.Null
                            ? pidEl.GetInt32() : null;
                        return new
                        {
                            Id = t.GetProperty("id").GetInt32().ToString(),
                            Title = t.GetProperty("title").GetString() ?? string.Empty,
                            Subtitle = t.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null ? d.GetString() : null,
                            ProjectId = pid
                        };
                    }).ToList();

                    var missingProjectIds = taskItems
                        .Where(t => t.ProjectId.HasValue && !projectNameMap.ContainsKey(t.ProjectId.Value))
                        .Select(t => t.ProjectId!.Value)
                        .Distinct()
                        .ToList();

                    if (missingProjectIds.Count > 0)
                    {
                        var fetched = await db.Projects
                            .Where(p => missingProjectIds.Contains(p.Id))
                            .Select(p => new { p.Id, p.Name })
                            .ToListAsync();
                        foreach (var p in fetched)
                            projectNameMap[p.Id] = p.Name;
                    }

                    vm.Tasks = taskItems.Select(t => new SearchResultItem
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Subtitle = t.Subtitle,
                        ProjectId = t.ProjectId,
                        ProjectName = t.ProjectId.HasValue && projectNameMap.TryGetValue(t.ProjectId.Value, out var pn) ? pn : null,
                        WorkspaceType = "Task Board",
                        LinkUrl = t.ProjectId.HasValue
                            ? Url.Action("Index", "Board", new { id = t.ProjectId.Value })
                            : null
                    }).ToList();
                }

                if (raw.TryGetProperty("members", out var members) && members.ValueKind == JsonValueKind.Array)
                    vm.Members = members.EnumerateArray()
                        .Select(m => new SearchResultItem
                        {
                            Id = m.GetProperty("id").GetString() ?? string.Empty,
                            Title = m.GetProperty("displayName").GetString() ?? string.Empty
                        }).ToList();
            }
            catch
            {
                vm.ServiceUnavailable = true;
            }

            return View(vm);
        }
    }
}
