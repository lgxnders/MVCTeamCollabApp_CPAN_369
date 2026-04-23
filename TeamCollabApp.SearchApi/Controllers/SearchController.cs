using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.SearchApi.Data;

namespace TeamCollabApp.SearchApi.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController(SearchDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(q) || string.IsNullOrWhiteSpace(userId))
                return BadRequest("q and userId are required.");

            var term = q.Trim().ToLower();

            // Get all project IDs this user is a member of
            var memberProjectIds = await dbContext.ProjectMemberships
                .Where(m => m.UserId == userId)
                .Select(m => m.ProjectId)
                .ToListAsync();

            // Search projects the user has access to
            var projects = await dbContext.Projects
                .Where(p => memberProjectIds.Contains(p.Id) &&
                            (p.Name.ToLower().Contains(term) ||
                             (p.Description != null && p.Description.ToLower().Contains(term))))
                .Select(p => new { p.Id, p.Name, p.Description, Type = "project" })
                .ToListAsync();

            // Get accessible column IDs and their project mapping
            var columnIds = await dbContext.BoardColumns
                .Where(c => memberProjectIds.Contains(c.ProjectId))
                .Select(c => new { c.Id, c.ProjectId })
                .ToListAsync();

            var accessibleColumnIds = columnIds.Select(c => c.Id).ToList();
            var columnProjectMap = columnIds.ToDictionary(c => c.Id, c => c.ProjectId);

            // Search tasks in accessible columns
            var tasks = await dbContext.Tasks
                .Where(t => accessibleColumnIds.Contains(t.ColumnId) &&
                            (t.Title.ToLower().Contains(term) ||
                             (t.Description != null && t.Description.ToLower().Contains(term))))
                .Select(t => new { t.Id, t.Title, t.Description, t.ColumnId, Type = "task" })
                .ToListAsync();

            var tasksWithProject = tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.ColumnId,
                ProjectId = columnProjectMap.TryGetValue(t.ColumnId, out var pid) ? pid : 0,
                t.Type
            });

            // Search members by display name
            var members = await dbContext.Members
                .Where(m => m.DisplayName.ToLower().Contains(term))
                .Select(m => new { m.Id, m.DisplayName, Type = "member" })
                .ToListAsync();

            return Ok(new
            {
                Projects = projects,
                Tasks = tasksWithProject,
                Members = members
            });
        }
    }
}
