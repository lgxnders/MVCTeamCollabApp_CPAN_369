using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.TasksApi.Data;
using TeamCollabApp.TasksApi.Models;

namespace TeamCollabApp.TasksApi.Controllers
{
    [ApiController]
    [Route("tasks")]
    public class TasksController(TasksDbContext dbContext) : ControllerBase
    {
        [HttpGet("/board/{projectId}")]
        public async Task<IActionResult> GetBoard(int projectId)
        {
            var columns = await dbContext.BoardColumns
                .Where(column => column.ProjectId == projectId)
                .OrderBy(column => column.Position)
                .Include(column => column.Tasks.OrderBy(task => task.Position))
                    .ThenInclude(task => task.Assignees)
                .Include(column => column.Tasks)
                    .ThenInclude(task => task.ChecklistItems.OrderBy(item => item.Position))
                .ToListAsync();

            if (!columns.Any())
                columns = await CreateDefaultColumns(projectId);

            return Ok(columns);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var positionInColumn = await dbContext.BoardTasks
                .CountAsync(task => task.ColumnId == request.ColumnId);

            var newTask = new BoardTask
            {
                ColumnId = request.ColumnId,
                Title = request.Title,
                Description = request.Description,
                Position = positionInColumn
            };

            dbContext.BoardTasks.Add(newTask);
            await dbContext.SaveChangesAsync();

            if (request.AssigneeUserIds != null)
            {
                foreach (var uid in request.AssigneeUserIds)
                {
                    dbContext.TaskAssignees.Add(new TaskAssignee { TaskId = newTask.Id, UserId = uid });
                }
                await dbContext.SaveChangesAsync();
            }

            // Reload with assignees so the response includes them
            await dbContext.Entry(newTask).Collection(t => t.Assignees).LoadAsync();

            return Ok(newTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await dbContext.BoardTasks.FindAsync(id);
            if (task is null) return NotFound();

            task.Title = request.Title;
            task.Description = request.Description;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;

            await dbContext.SaveChangesAsync();
            return Ok(task);
        }

        [HttpPut("{id}/move")]
        public async Task<IActionResult> MoveTask(int id, [FromBody] MoveTaskRequest request)
        {
            var task = await dbContext.BoardTasks.FindAsync(id);
            if (task is null) return NotFound();

            var oldColumnId = task.ColumnId;
            task.ColumnId = request.ColumnId;
            await dbContext.SaveChangesAsync();

            // Renumber destination column with the task inserted at the requested position
            var destTasks = await dbContext.BoardTasks
                .Where(t => t.ColumnId == request.ColumnId && t.Id != id)
                .OrderBy(t => t.Position)
                .ToListAsync();

            destTasks.Insert(Math.Clamp(request.Position, 0, destTasks.Count), task);
            for (int i = 0; i < destTasks.Count; i++)
                destTasks[i].Position = i;

            // If the task moved to a different column, renumber the source column too
            if (oldColumnId != request.ColumnId)
            {
                var srcTasks = await dbContext.BoardTasks
                    .Where(t => t.ColumnId == oldColumnId)
                    .OrderBy(t => t.Position)
                    .ToListAsync();

                for (int i = 0; i < srcTasks.Count; i++)
                    srcTasks[i].Position = i;
            }

            await dbContext.SaveChangesAsync();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await dbContext.BoardTasks.FindAsync(id);
            if (task is null) return NotFound();

            dbContext.BoardTasks.Remove(task);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{id}/assignees")]
        public async Task<IActionResult> AddAssignee(int id, [FromBody] AssigneeRequest request)
        {
            var alreadyAssigned = await dbContext.TaskAssignees
                .AnyAsync(assignee => assignee.TaskId == id && assignee.UserId == request.UserId);

            if (alreadyAssigned) return Conflict();

            dbContext.TaskAssignees.Add(new TaskAssignee { TaskId = id, UserId = request.UserId });
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}/assignees/{userId}")]
        public async Task<IActionResult> RemoveAssignee(int id, string userId)
        {
            var assignee = await dbContext.TaskAssignees
                .FirstOrDefaultAsync(assignee => assignee.TaskId == id && assignee.UserId == userId);

            if (assignee is null) return NotFound();

            dbContext.TaskAssignees.Remove(assignee);
            await dbContext.SaveChangesAsync();
            return Ok();
        }

        private async Task<List<BoardColumn>> CreateDefaultColumns(int projectId)
        {
            var defaultTitles = new[] { "To Do", "In Progress", "Done" };
            var defaultColumns = defaultTitles.Select((title, index) => new BoardColumn
            {
                ProjectId = projectId,
                Title = title,
                Position = index
            }).ToList();

            dbContext.BoardColumns.AddRange(defaultColumns);
            await dbContext.SaveChangesAsync();
            return defaultColumns;
        }
    }

    public record CreateTaskRequest(int ColumnId, string Title, string? Description = null, List<string>? AssigneeUserIds = null);
    public record UpdateTaskRequest(string Title, string? Description, TaskPriority Priority, DateTime? DueDate);
    public record MoveTaskRequest(int ColumnId, int Position);
    public record AssigneeRequest(string UserId);
}
