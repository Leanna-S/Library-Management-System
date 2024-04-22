using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Models
{
    public static class UserHelper
    {
        public static ApplicationUser? GetUser(System.Security.Claims.ClaimsPrincipal user, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            string? userId = GetUserId(user, userManager);
            ApplicationUser? applicationUser = context.Users.Include(u => u.BookReturns).FirstOrDefault(u => u.Id == userId);
            return applicationUser;
        }

        public static bool UserInRole(System.Security.Claims.ClaimsPrincipal user, params string[] roles)
        {
            foreach (var role in roles)
            {
                if (user.IsInRole(role))
                {
                    return true;
                }
            }
            return false;
        }

        public static string? GetUserId(System.Security.Claims.ClaimsPrincipal user, UserManager<ApplicationUser> userManager)
        {
            return userManager.GetUserId(user);
        }
    }
}