using System.ComponentModel.DataAnnotations;
using TeamCollabApp.Models;

namespace TeamCollabApp.ViewModels
{
    public class ProjectSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublicLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerDisplayName { get; set; } = string.Empty;
        public int OtherMemberCount { get; set; }
    }

    public class CreateProjectViewModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsPublicLink { get; set; } = false;
    }

    public class EditProjectViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsPublicLink { get; set; }
    }

    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ShareToken { get; set; } = string.Empty;
        public bool IsPublicLink { get; set; }
        public DateTime CreatedAt { get; set; }
        public string OwnerDisplayName { get; set; } = string.Empty;
        public ProjectRole CurrentUserRole { get; set; }
        public List<MemberViewModel> Members { get; set; } = [];
    }

    public class MemberViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public ProjectRole Role { get; set; }
        public bool IsGuest { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class InviteMemberViewModel
    {
        public int ProjectId { get; set; }

        [Required, MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;

        public ProjectRole Role { get; set; } = ProjectRole.Viewer;
    }
};