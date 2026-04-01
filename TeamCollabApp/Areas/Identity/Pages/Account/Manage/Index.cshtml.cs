#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TeamCollabApp.Data;
using TeamCollabApp.Models;

namespace TeamCollabApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AppDbContext _db;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Display name must be between 3 and 50 characters.")]
            [RegularExpression(@"^[\w][\w\s.\-]*$",
                ErrorMessage = "Display name may only contain letters, numbers, spaces, underscores, hyphens, and periods.")]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Email = await _userManager.GetEmailAsync(user);
            Input = new InputModel { DisplayName = user.DisplayName };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var newName = Input.DisplayName.Trim();

            if (newName != user.DisplayName)
            {
                var taken = await _db.Users.AnyAsync(
                    u => u.Id != user.Id && u.DisplayName.ToLower() == newName.ToLower());

                if (taken)
                {
                    ModelState.AddModelError(string.Empty, "That display name is already taken.");
                    await LoadAsync(user);
                    return Page();
                }

                user.DisplayName = newName;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving your display name.");
                    await LoadAsync(user);
                    return Page();
                }

                await _signInManager.RefreshSignInAsync(user);
            }

            StatusMessage = "Display name updated successfully.";
            return RedirectToPage();
        }
    }
}
