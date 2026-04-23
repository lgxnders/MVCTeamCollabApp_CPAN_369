using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamCollabApp.Data;
using TeamCollabApp.HttpClients;
using TeamCollabApp.Hubs;
using TeamCollabApp.Models;
using TeamCollabApp.Services;
using TeamCollabApp.ViewModels;

namespace TeamCollabApp.Controllers
{
    [Authorize]
    public class BoardController(
        AppDbContext db,
        TasksBoardClient tasksClient,
        UserManager<ApplicationUser> userManager,
        IGuestSessionService guestSessionService,
        IHubContext<CollaborationHub> hubContext) : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private string? GetCurrentUserId() => userManager.GetUserId(User);

        private const string GuestCookieName = "guest_session";

        private async Task<(ProjectMembership? membership, bool isGuest)> GetMembershipAsync(int projectId)
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                var m = await db.ProjectMemberships
                    .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
                return (m, false);
            }

            var guestToken = HttpContext.Request.Cookies[GuestCookieName];
            if (guestToken != null)
            {
                var guest = await guestSessionService.ResolveAsync(guestToken);
                if (guest != null)
                {
                    var m = await db.ProjectMemberships
                        .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.GuestSessionId == guest.Id);
                    return (m, true);
                }
            }

            return (null, false);
        }

        private string GetCallerId()
        {
            var userId = GetCurrentUserId();
            return userId ?? "guest";
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int id)
        {
            var (membership, isGuest) = await GetMembershipAsync(id);
            if (membership is null) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            var callerId = GetCallerId();
            List<BoardColumnViewModel> columns;
            try
            {
                var raw = await tasksClient.GetAsync<JsonElement>($"board/{id}", callerId);
                columns = ParseColumns(raw);
            }
            catch
            {
                columns = [];
                TempData["Error"] = "Could not reach the Tasks service. Task board is unavailable.";
            }

            var projectMembers = await db.ProjectMemberships
                .Where(m => m.ProjectId == id && m.UserId != null)
                .Include(m => m.User)
                .Select(m => new ProjectMemberSummary
                {
                    UserId = m.UserId!,
                    DisplayName = m.User!.DisplayName,
                    Role = m.Role.ToString()
                })
                .ToListAsync();

            string displayName;
            if (!isGuest)
            {
                var user = await userManager.GetUserAsync(User);
                displayName = user?.DisplayName ?? "Unknown";
            }
            else
            {
                var guestToken = HttpContext.Request.Cookies[GuestCookieName];
                var guest = guestToken != null ? await guestSessionService.ResolveAsync(guestToken) : null;
                displayName = guest?.DisplayName ?? "Guest";
            }

            var vm = new BoardViewModel
            {
                ProjectId = id,
                ProjectName = project.Name,
                CurrentUserRole = membership.Role,
                IsGuest = isGuest,
                CurrentUserDisplayName = displayName,
                Columns = columns,
                ProjectMembers = projectMembers
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTask(int id, [FromForm] int columnId, [FromForm] string title,
            [FromForm] string? description, [FromForm] List<string>? assigneeUserIds)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            var response = await tasksClient.PostAsync("tasks", new
            {
                ColumnId = columnId,
                Title = title,
                Description = description,
                AssigneeUserIds = assigneeUserIds ?? []
            }, GetCallerId());
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var task = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
            await hubContext.Clients.Group($"project-{id}").SendAsync("OnTaskCreated", task);

            return Ok(json);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveTask(int id, [FromForm] int taskId, [FromForm] int columnId, [FromForm] int position)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            var response = await tasksClient.PutAsync($"tasks/{taskId}/move", new { ColumnId = columnId, Position = position }, GetCallerId());
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            var payload = new { taskId, columnId, position };
            await hubContext.Clients.Group($"project-{id}").SendAsync("OnTaskMoved", payload);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTask(int id, [FromForm] int taskId, [FromForm] string title,
            [FromForm] string? description, [FromForm] int priority, [FromForm] string? dueDate)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            DateTime? parsedDueDate = DateTime.TryParse(dueDate, out var dt) ? dt : null;
            var response = await tasksClient.PutAsync($"tasks/{taskId}", new { Title = title, Description = description, Priority = priority, DueDate = parsedDueDate }, GetCallerId());
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var task = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions);
            await hubContext.Clients.Group($"project-{id}").SendAsync("OnTaskUpdated", task);

            return Ok(json);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTask(int id, [FromForm] int taskId)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            var response = await tasksClient.DeleteAsync($"tasks/{taskId}", GetCallerId());
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            await hubContext.Clients.Group($"project-{id}").SendAsync("OnTaskDeleted", taskId);

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddColumn(int id, [FromForm] string title)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Owner) return Forbid();

            var response = await tasksClient.PostAsync("columns", new { ProjectId = id, Title = title }, GetCallerId());
            if (!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);

            return Ok(await response.Content.ReadAsStringAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenameColumn(int id, [FromForm] int columnId, [FromForm] string title)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Owner) return Forbid();

            var response = await tasksClient.PutAsync($"columns/{columnId}", new { Title = title }, GetCallerId());
            return response.IsSuccessStatusCode ? Ok() : StatusCode((int)response.StatusCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteColumn(int id, [FromForm] int columnId)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Owner) return Forbid();

            var response = await tasksClient.DeleteAsync($"columns/{columnId}", GetCallerId());
            return response.IsSuccessStatusCode ? Ok() : StatusCode((int)response.StatusCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignee(int id, [FromForm] int taskId, [FromForm] string userId)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            var response = await tasksClient.PostAsync($"tasks/{taskId}/assignees", new { UserId = userId }, GetCallerId());
            return response.IsSuccessStatusCode ? Ok() : StatusCode((int)response.StatusCode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAssignee(int id, [FromForm] int taskId, [FromForm] string userId)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor) return Forbid();

            var response = await tasksClient.DeleteAsync($"tasks/{taskId}/assignees/{userId}", GetCallerId());
            return response.IsSuccessStatusCode ? Ok() : StatusCode((int)response.StatusCode);
        }

        private static List<BoardColumnViewModel> ParseColumns(JsonElement raw)
        {
            var result = new List<BoardColumnViewModel>();
            if (raw.ValueKind != JsonValueKind.Array) return result;

            foreach (var col in raw.EnumerateArray())
            {
                var column = new BoardColumnViewModel
                {
                    Id = col.GetProperty("id").GetInt32(),
                    ProjectId = col.GetProperty("projectId").GetInt32(),
                    Title = col.GetProperty("title").GetString() ?? string.Empty,
                    Position = col.GetProperty("position").GetInt32(),
                };

                if (col.TryGetProperty("tasks", out var tasksEl) && tasksEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in tasksEl.EnumerateArray())
                    {
                        var task = new BoardTaskViewModel
                        {
                            Id = t.GetProperty("id").GetInt32(),
                            ColumnId = t.GetProperty("columnId").GetInt32(),
                            Title = t.GetProperty("title").GetString() ?? string.Empty,
                            Description = t.TryGetProperty("description", out var d) ? d.GetString() : null,
                            Position = t.GetProperty("position").GetInt32(),
                            Priority = t.TryGetProperty("priority", out var p) ? p.ToString() : "None",
                            DueDate = t.TryGetProperty("dueDate", out var dd) && dd.ValueKind != JsonValueKind.Null ? dd.GetDateTime() : null,
                        };

                        if (t.TryGetProperty("assignees", out var assignees) && assignees.ValueKind == JsonValueKind.Array)
                            foreach (var a in assignees.EnumerateArray())
                                if (a.TryGetProperty("userId", out var uid))
                                    task.AssigneeUserIds.Add(uid.GetString() ?? string.Empty);

                        column.Tasks.Add(task);
                    }
                }

                result.Add(column);
            }

            return result;
        }
    }
}
