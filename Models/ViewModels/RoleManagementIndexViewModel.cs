using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class RoleManagementIndexViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
        public bool? IsCurrentUser { get; set; }
    }
}