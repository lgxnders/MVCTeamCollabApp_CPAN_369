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
        IGuestSessionService guestSessionService) : Controller
    {

        private async Task<ApplicationUser> CurrentUserAsync() =>
            (await userManager.GetUserAsync(User))!;

        private async Task<ProjectRole?> GetUserRoleAsync(int projectId, string userId)
        {
            var m = await db.ProjectMemberships
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
            return m?.Role;
        }

        public async Task<IActionResult> Index()
        {
            var userId = userManager.GetUserId(User)!;

            var projects = await db.ProjectMemberships
                .Where(m => m.UserId == userId)
                .Include(m => m.Project)
                .Select(m => m.Project)
                .ToListAsync();

            return View(projects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = userManager.GetUserId(User)!;
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

            var user = await CurrentUserAsync();

            var project = new Project
            {
                Name = vm.Name,
                Description = vm.Description,
                IsPublicLink = vm.IsPublicLink,
                OwnerId = user.Id
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // add creator as Owner in the memberships table.
            db.ProjectMemberships.Add(new ProjectMembership
            {
                ProjectId = project.Id,
                UserId = user.Id,
                Role = ProjectRole.Owner
            });
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = project.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(id, userId);
            if (role < ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            return View(new EditProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsPublicLink = project.IsPublicLink
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProjectViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(id, userId);
            if (role < ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            project.Name = vm.Name;
            project.Description = vm.Description;
            project.IsPublicLink = vm.IsPublicLink;
            project.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            return project is null ? NotFound() : View(project);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            db.Projects.Remove(project);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Join(string token)
        {
            var project = await db.Projects
                .FirstOrDefaultAsync(p => p.ShareToken == token);

            if (project is null || !project.IsPublicLink)
                return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = userManager.GetUserId(User)!;
                var existing = await db.ProjectMemberships
                    .AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId);

                if (!existing)
                {
                    db.ProjectMemberships.Add(new ProjectMembership
                    {
                        ProjectId = project.Id,
                        UserId = userId,
                        Role = ProjectRole.Viewer
                    });
                    await db.SaveChangesAsync();
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
                    db.ProjectMemberships.Add(new ProjectMembership
                    {
                        ProjectId = project.Id,
                        GuestSessionId = guest.Id,
                        Role = ProjectRole.Viewer
                    });
                    await db.SaveChangesAsync();
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

            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(vm.ProjectId, userId);
            if (role < ProjectRole.Owner) return Forbid();

            var invitee = await userManager.FindByEmailAsync(vm.Email);
            if (invitee is null)
            {
                TempData["Error"] = $"No user found with email {vm.Email}.";
                return RedirectToAction(nameof(Details), new { id = vm.ProjectId });
            }

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

            var userId = userManager.GetUserId(User)!;
            var callerRole = await GetUserRoleAsync(membership.ProjectId, userId);
            if (callerRole < ProjectRole.Owner) return Forbid();

            // prevent demoting the last owner
            if (membership.Role == ProjectRole.Owner && newRole < ProjectRole.Owner)
            {
                var ownerCount = await db.ProjectMemberships
                    .CountAsync(m => m.ProjectId == membership.ProjectId && m.Role == ProjectRole.Owner);
                if (ownerCount <= 1)
                {
                    TempData["Error"] = "Cannot demote the last owner.";
                    return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
                }
            }

            membership.Role = newRole;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int membershipId)
        {
            var membership = await db.ProjectMemberships.FindAsync(membershipId);
            if (membership is null) return NotFound();

            var userId = userManager.GetUserId(User)!;
            var callerRole = await GetUserRoleAsync(membership.ProjectId, userId);
            if (callerRole < ProjectRole.Owner) return Forbid();

            db.ProjectMemberships.Remove(membership);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = membership.ProjectId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateShareToken(int id)
        {
            var userId = userManager.GetUserId(User)!;
            var role = await GetUserRoleAsync(id, userId);
            if (role != ProjectRole.Owner) return Forbid();

            var project = await db.Projects.FindAsync(id);
            if (project is null) return NotFound();

            project.ShareToken = Guid.NewGuid().ToString("N");
            project.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }
    }
};