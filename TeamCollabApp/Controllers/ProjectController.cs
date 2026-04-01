using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.Data;
using TeamCollabApp.Models;
using TeamCollabApp.Services;
using TeamCollabApp.ViewModels;

namespace YourApp.Controllers
{

    [Authorize]
    public class ProjectsController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IGuestSessionService guestSessionService,
        ILogger<ProjectsController> logger) : Controller
    {

        private string? GetCurrentUserId() => userManager.GetUserId(User);

        private async Task<ProjectRole?> GetUserRoleAsync(int projectId, string userId)
        {
            var m = await db.ProjectMemberships
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            return m?.Role;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var projects = await db.ProjectMemberships
                .Where(m => m.UserId == userId)
                .Select(m => new ProjectSummaryViewModel
                {
                    Id = m.Project.Id,
                    Name = m.Project.Name,
                    Description = m.Project.Description,
                    IsPublicLink = m.Project.IsPublicLink,
                    CreatedAt = m.Project.CreatedAt,
                    OwnerDisplayName = m.Project.Owner.DisplayName,
                    OtherMemberCount = m.Project.Memberships
                        .Count(mb => mb.UserId != userId)
                })
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role is null) return Forbid();

            var project = await db.Projects
                .Include(p => p.Owner)
                .Include(p => p.Memberships).ThenInclude(m => m.User)
                .Include(p => p.Memberships).ThenInclude(m => m.GuestSession)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null) return NotFound();

            var vm = new ProjectDetailsViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ShareToken = project.ShareToken,
                IsPublicLink = project.IsPublicLink,
                CreatedAt = project.CreatedAt,
                OwnerDisplayName = project.Owner.DisplayName,
                CurrentUserRole = role.Value,
                Members = project.Memberships.Select(m => new MemberViewModel
                {
                    MembershipId = m.Id,
                    DisplayName = m.DisplayName,
                    Role = m.Role,
                    IsGuest = m.GuestSessionId.HasValue,
                    JoinedAt = m.JoinedAt
                }).ToList()
            };

            return View(vm);
        }

        public IActionResult Create() => View(new CreateProjectViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Unauthorized();

            var project = new Project
            {
                Name = vm.Name,
                Description = vm.Description,
                IsPublicLink = vm.IsPublicLink,
                OwnerId = user.Id
            };

            try
            {
                db.Projects.Add(project);
                await db.SaveChangesAsync();

                db.ProjectMemberships.Add(new ProjectMembership
                {
                    ProjectId = project.Id,
                    UserId = user.Id,
                    Role = ProjectRole.Owner
                });
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to create project for user {UserId}", userId);
                ModelState.AddModelError("", "A database error occurred. Please try again.");
                return View(vm);
            }

            logger.LogInformation("User {UserId} created project {ProjectId}", userId, project.Id);
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescription(int id, string? description)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role < ProjectRole.Editor) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            try
            {
                project.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
                project.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to update description for project {ProjectId} by user {UserId}", id, userId);
                TempData["Error"] = "A database error occurred while saving the description.";
            }

            logger.LogInformation("User {UserId} updated description of project {ProjectId}", userId, id);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicLink(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            try
            {
                project.IsPublicLink = !project.IsPublicLink;
                project.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to toggle public link for project {ProjectId} by user {UserId}", id, userId);
                TempData["Error"] = "A database error occurred while updating the share link setting.";
            }

            logger.LogInformation("User {UserId} toggled public link for project {ProjectId} to {State}",
                userId, id, project.IsPublicLink);
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            return project is null ? NotFound() : View(project);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            try
            {
                db.Projects.Remove(project);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to delete project {ProjectId} by user {UserId}", id, userId);
                TempData["Error"] = "A database error occurred while deleting the project.";
                return RedirectToAction(nameof(Details), new { id });
            }

            logger.LogInformation("User {UserId} deleted project {ProjectId}", userId, id);
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Join(string id)
        {
            var project = await db.Projects
                .FirstOrDefaultAsync(p => p.ShareToken == id);

            if (project is null || !project.IsPublicLink)
                return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetCurrentUserId();
                if (userId is null) return Unauthorized();

                var existing = await db.ProjectMemberships
                    .AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId);

                if (!existing)
                {
                    try
                    {
                        db.ProjectMemberships.Add(new ProjectMembership
                        {
                            ProjectId = project.Id,
                            UserId = userId,
                            Role = ProjectRole.Viewer
                        });
                        await db.SaveChangesAsync();
                        logger.LogInformation("User {UserId} joined project {ProjectId} via public link", userId, project.Id);
                    }
                    catch (DbUpdateException ex)
                    {
                        logger.LogWarning(ex, "Concurrent join by user {UserId} for project {ProjectId}, ignoring duplicate", userId, project.Id);
                    }
                }

                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            else
            {
                var guest = await guestSessionService.GetOrCreateAsync(HttpContext);

                var existing = await db.ProjectMemberships
                    .AnyAsync(m => m.ProjectId == project.Id && m.GuestSessionId == guest.Id);

                if (!existing)
                {
                    try
                    {
                        db.ProjectMemberships.Add(new ProjectMembership
                        {
                            ProjectId = project.Id,
                            GuestSessionId = guest.Id,
                            Role = ProjectRole.Viewer
                        });
                        await db.SaveChangesAsync();
                        logger.LogInformation("Guest {GuestId} joined project {ProjectId} via public link", guest.Id, project.Id);
                    }
                    catch (DbUpdateException ex)
                    {
                        logger.LogWarning(ex, "Concurrent join by guest {GuestId} for project {ProjectId}, ignoring duplicate", guest.Id, project.Id);
                    }
                }

                return RedirectToAction("GuestView", new { id = project.Id });
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> GuestView(int id)
        {
            var project = await db.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project is null) return NotFound();

            return View(project);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteMember(InviteMemberViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Details), new { id = vm.ProjectId });

            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(vm.ProjectId, userId);
            if (role < ProjectRole.Owner) return Forbid();

            var searchName = vm.DisplayName.Trim();
            var invitee = await db.Users
                .FirstOrDefaultAsync(u => u.DisplayName.ToLower() == searchName.ToLower());

            if (invitee is null)
            {
                TempData["Error"] = $"No user found with the display name \"{searchName}\".";
                return RedirectToAction(nameof(Details), new { id = vm.ProjectId });
            }

            if (invitee.Id == userId)
            {
                TempData["Error"] = "You cannot invite yourself.";
                return RedirectToAction(nameof(Details), new { id = vm.ProjectId });
            }

            try
            {
                await using var tx = await db.Database.BeginTransactionAsync();

                var already = await db.ProjectMemberships
                    .AnyAsync(m => m.ProjectId == vm.ProjectId && m.UserId == invitee.Id);

                if (!already)
                {
                    db.ProjectMemberships.Add(new ProjectMembership
                    {
                        ProjectId = vm.ProjectId,
                        UserId = invitee.Id,
                        Role = vm.Role
                    });
                    await db.SaveChangesAsync();
                    logger.LogInformation("User {InviterId} invited {InviteeDisplayName} ({InviteeId}) to project {ProjectId} as {Role}",
                        userId, invitee.DisplayName, invitee.Id, vm.ProjectId, vm.Role);
                }

                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to invite user {InviteeId} to project {ProjectId}", invitee.Id, vm.ProjectId);
                TempData["Error"] = "A database error occurred while adding the member.";
            }

            return RedirectToAction(nameof(Details), new { id = vm.ProjectId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int membershipId, ProjectRole newRole)
        {
            var membership = await db.ProjectMemberships
                .Include(m => m.Project)
                .FirstOrDefaultAsync(m => m.Id == membershipId);

            if (membership is null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var callerRole = await GetUserRoleAsync(membership.ProjectId, userId);
            if (callerRole < ProjectRole.Owner) return Forbid();

            try
            {
                if (membership.Role == ProjectRole.Owner && newRole < ProjectRole.Owner)
                {
                    await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                    // Re-count owners inside the transaction to prevent race-condition.
                    var ownerCount = await db.ProjectMemberships
                        .CountAsync(m => m.ProjectId == membership.ProjectId && m.Role == ProjectRole.Owner);

                    if (ownerCount <= 1)
                    {
                        TempData["Error"] = "Cannot demote the last owner.";
                        return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
                    }

                    membership.Role = newRole;
                    await db.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                else
                {
                    membership.Role = newRole;
                    await db.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to update role for membership {MembershipId}", membershipId);
                TempData["Error"] = "A database error occurred while updating the role.";
                return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
            }

            logger.LogInformation("User {UserId} changed membership {MembershipId} to {NewRole}", userId, membershipId, newRole);
            return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int membershipId)
        {
            var membership = await db.ProjectMemberships.FindAsync(membershipId);
            if (membership is null) return NotFound();

            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var callerRole = await GetUserRoleAsync(membership.ProjectId, userId);
            if (callerRole < ProjectRole.Owner) return Forbid();

            try
            {
                db.ProjectMemberships.Remove(membership);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to remove membership {MembershipId} by user {UserId}", membershipId, userId);
                TempData["Error"] = "A database error occurred while removing the member.";
                return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
            }

            logger.LogInformation("User {UserId} removed membership {MembershipId} from project {ProjectId}", userId, membershipId, membership.ProjectId);
            return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateShareToken(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            try
            {
                project.ShareToken = Guid.NewGuid().ToString("N");
                project.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to regenerate share token for project {ProjectId} by user {UserId}", id, userId);
                TempData["Error"] = "A database error occurred while regenerating the share token.";
                return RedirectToAction(nameof(Details), new { id });
            }

            logger.LogInformation("User {UserId} regenerated share token for project {ProjectId}", userId, id);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Rename(int id, string name)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            var role = await GetUserRoleAsync(id, userId);
            if (role < ProjectRole.Editor) return Forbid();

            name = name?.Trim() ?? string.Empty;
            if (name.Length == 0 || name.Length > 200)
            {
                TempData["Error"] = "Project name must be between 1 and 200 characters.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            try
            {
                project.Name = name;
                project.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Failed to rename project {ProjectId} by user {UserId}", id, userId);
                TempData["Error"] = "A database error occurred while renaming the project.";
                return RedirectToAction(nameof(Details), new { id });
            }

            logger.LogInformation("User {UserId} renamed project {ProjectId} to \"{Name}\"", userId, id, name);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
};
