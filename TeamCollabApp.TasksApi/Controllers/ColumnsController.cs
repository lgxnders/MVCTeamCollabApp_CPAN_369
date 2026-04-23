using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.TasksApi.Data;
using TeamCollabApp.TasksApi.Models;

namespace TeamCollabApp.TasksApi.Controllers
{
    [ApiController]
    [Route("columns")]
    public class ColumnsController(TasksDbContext dbContext) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateColumn([FromBody] CreateColumnRequest request)
        {
            var existingCount = await dbContext.BoardColumns
                .CountAsync(column => column.ProjectId == request.ProjectId);

            var newColumn = new BoardColumn
            {
                ProjectId = request.ProjectId,
                Title = request.Title,
                Position = existingCount
            };

            dbContext.BoardColumns.Add(newColumn);
            await dbContext.SaveChangesAsync();
            return Ok(newColumn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> RenameColumn(int id, [FromBody] RenameColumnRequest request)
        {
            var column = await dbContext.BoardColumns.FindAsync(id);
            if (column is null) return NotFound();

            column.Title = request.Title;
            await dbContext.SaveChangesAsync();
            return Ok(column);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteColumn(int id)
        {
            var column = await dbContext.BoardColumns
                .Include(column => column.Tasks)
                .FirstOrDefaultAsync(column => column.Id == id);

            if (column is null) return NotFound();

            dbContext.BoardColumns.Remove(column);
            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    public record CreateColumnRequest(int ProjectId, string Title);
    public record RenameColumnRequest(string Title);
}
