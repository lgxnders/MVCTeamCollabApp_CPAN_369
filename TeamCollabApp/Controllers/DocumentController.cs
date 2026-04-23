using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.Data;
using TeamCollabApp.Hubs;
using TeamCollabApp.Models;
using TeamCollabApp.Services;
using TeamCollabApp.ViewModels;

namespace TeamCollabApp.Controllers
{
    [Authorize]
    public class DocumentController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IGuestSessionService guestSessionService,
        IHubContext<CollaborationHub> hubContext) : Controller
    {
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

        [AllowAnonymous]
        public async Task<IActionResult> Index(int id)
        {
            var (membership, isGuest) = await GetMembershipAsync(id);
            if (membership is null) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            var document = await db.ProjectDocuments
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.ProjectId == id);

            if (document is null)
            {
                document = new ProjectDocument { ProjectId = id };
                db.ProjectDocuments.Add(document);
                await db.SaveChangesAsync();
            }

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

            var projectMembers = await db.ProjectMemberships
                .Where(m => m.ProjectId == id && m.UserId != null)
                .Include(m => m.User)
                .Select(m => new ProjectMemberInfo
                {
                    DisplayName = m.User!.DisplayName,
                    Role = m.Role.ToString()
                })
                .ToListAsync();

            var vm = new DocumentViewModel
            {
                ProjectId = id,
                ProjectName = project.Name,
                DocumentId = document.Id,
                ContentJson = document.ContentJson,
                CurrentUserRole = membership.Role,
                IsGuest = isGuest,
                CurrentUserDisplayName = displayName,
                ProjectMembers = projectMembers,
                Comments = document.Comments
                    .Where(c => !c.IsResolved)
                    .Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        CommentKey = c.CommentKey,
                        UserId = c.UserId,
                        UserDisplayName = c.UserDisplayName,
                        CommentText = c.CommentText,
                        IsResolved = c.IsResolved,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, [FromForm] string contentJson)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor)
                return Forbid();

            var document = await db.ProjectDocuments.FirstOrDefaultAsync(d => d.ProjectId == id);
            if (document is null) return NotFound();

            document.ContentJson = contentJson;
            document.UpdatedAt = DateTime.UtcNow;
            document.LastEditedByUserId = GetCurrentUserId();
            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int id, [FromForm] string commentKey, [FromForm] string commentText)
        {
            var (membership, isGuest) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Commenter)
                return Forbid();

            var document = await db.ProjectDocuments.FirstOrDefaultAsync(d => d.ProjectId == id);
            if (document is null) return NotFound();

            string userId;
            string displayName;
            if (!isGuest)
            {
                userId = GetCurrentUserId() ?? string.Empty;
                var user = await userManager.GetUserAsync(User);
                displayName = user?.DisplayName ?? "Unknown";
            }
            else
            {
                var guestToken2 = HttpContext.Request.Cookies[GuestCookieName];
                var guest = guestToken2 != null ? await guestSessionService.ResolveAsync(guestToken2) : null;
                userId = $"guest-{guest?.Id}";
                displayName = guest?.DisplayName ?? "Guest";
            }

            var comment = new DocumentComment
            {
                DocumentId = document.Id,
                CommentKey = commentKey,
                UserId = userId,
                UserDisplayName = displayName,
                CommentText = commentText,
                CreatedAt = DateTime.UtcNow
            };

            db.DocumentComments.Add(comment);
            await db.SaveChangesAsync();

            var payload = new { comment.CommentKey, comment.UserDisplayName, comment.CommentText, comment.CreatedAt };
            await hubContext.Clients.Group($"project-{id}").SendAsync("OnCommentAdded", payload);

            return Ok(payload);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveComment(int id, [FromForm] string commentKey)
        {
            var (membership, _) = await GetMembershipAsync(id);
            if (membership is null || membership.Role < ProjectRole.Editor)
                return Forbid();

            var comment = await db.DocumentComments.FirstOrDefaultAsync(c => c.CommentKey == commentKey);
            if (comment is null) return NotFound();

            comment.IsResolved = true;
            await db.SaveChangesAsync();

            await hubContext.Clients.Group($"project-{id}").SendAsync("OnCommentResolved", commentKey);

            return Ok();
        }
    }
}
