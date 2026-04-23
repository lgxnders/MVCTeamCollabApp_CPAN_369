using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.TasksApi.Data;
using TeamCollabApp.TasksApi.Models;

namespace TeamCollabApp.TasksApi.Controllers
{
    [ApiController]
    [Route("checklist")]
    public class ChecklistController(TasksDbContext dbContext) : ControllerBase
    {
        [HttpPost("/tasks/{taskId}/checklist")]
        public async Task<IActionResult> AddItem(int taskId, [FromBody] AddChecklistItemRequest request)
        {
            var positionInTask = await dbContext.TaskChecklistItems
                .CountAsync(item => item.TaskId == taskId);

            var newItem = new TaskChecklistItem
            {
                TaskId = taskId,
                Text = request.Text,
                Position = positionInTask
            };

            dbContext.TaskChecklistItems.Add(newItem);
            await dbContext.SaveChangesAsync();
            return Ok(newItem);
        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> ToggleItem(int id)
        {
            var item = await dbContext.TaskChecklistItems.FindAsync(id);
            if (item is null) return NotFound();

            item.IsComplete = !item.IsComplete;
            await dbContext.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await dbContext.TaskChecklistItems.FindAsync(id);
            if (item is null) return NotFound();

            dbContext.TaskChecklistItems.Remove(item);
            await dbContext.SaveChangesAsync();
            return Ok();
        }
    }

    public record AddChecklistItemRequest(string Text);
}
