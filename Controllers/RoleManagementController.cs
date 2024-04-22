using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize(Roles = $"{Constants.AdminRole},{Constants.SuperAdminRole}")]
    public class RoleManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RoleManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            // for each user, get roles
            List<RoleManagementIndexViewModel> users = [];
            foreach (var user in _context.Users)
            {
                RoleManagementIndexViewModel vm = new()
                {
                    User = user,
                    Roles = (List<string>)await _userManager.GetRolesAsync(user),
                };
                users.Add(vm);
            }
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(string role, string id)
        {
            //get current user
            ApplicationUser? admin = UserHelper.GetUser(User, _userManager, _context);
            if (admin == null)
            {
                return Forbid();
            }
            if (!UserHelper.UserInRole(User, Constants.AdminRole, Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if ((role == Constants.AdminRole || role == Constants.SuperAdminRole) && !UserHelper.UserInRole(User, Constants.SuperAdminRole))
            {
                return Forbid();
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return NotFound("Role does not exist");
            }

            // get user whose role is being changed
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound("User not found");
            }
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                return NotFound("User not in role");
            }

            //remove user from role
            await _userManager.RemoveFromRoleAsync(user, role);
            //create role change
            RoleChange roleChange = new()
            {
                Admin = admin,
                UserAffected = user,
                RoleAffected = role,
                TimeOfChange = DateTime.UtcNow,
                Type = Constants.RoleChangeType.Removed
            };
            _context.RoleChanges.Add(roleChange);

            _context.SaveChanges();

            // sign the user out and in if admin is modifying their own roles
            if (admin.Id == user.Id)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, false);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetRoleToRemove(string userId)
        {
            // get user whose roles you are editing
            ApplicationUser? user = _context.Users.FirstOrDefault(u => u.Id == userId);

            //get current users id
            string? adminId = _userManager.GetUserId(User);

            if (user == null)
            {
                return NotFound("User not found");
            }

            RoleManagementIndexViewModel vm = new()
            {
                User = user,
                Roles = (List<string>)await _userManager.GetRolesAsync(user),
                IsCurrentUser = (user.Id == adminId),
            };
            return View("RemoveRole", vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetRoleToAdd(string userId)
        {
            //get user whose role you are modifing
            ApplicationUser? user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            //get users roles
            var userRoles = await _userManager.GetRolesAsync(user);

            //get the roles the user is not in
            var rolesUserNotIn = _roleManager.Roles
                .Where(r => r.Name != null && !userRoles.Contains(r.Name))
                .Select(r => r.Name ?? string.Empty)
                .ToList();

            RoleManagementIndexViewModel vm = new()
            {
                User = user,
                Roles = rolesUserNotIn,
            };
            return View("AddRole", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserToRole(string id, string role)
        {
            //get current user
            ApplicationUser? admin = UserHelper.GetUser(User, _userManager, _context);
            if (admin == null)
            {
                return Forbid();
            }
            //makes sure user in valid role
            if (!UserHelper.UserInRole(User, Constants.SuperAdminRole, Constants.AdminRole))
            {
                return Forbid();
            }
            //makes sure the user has premission to modify for that role
            if ((role == Constants.AdminRole || role == Constants.SuperAdminRole) && !UserHelper.UserInRole(User, Constants.SuperAdminRole))
            {
                return Forbid();
            }
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return NotFound("Role does not exist");
            }

            //gets user whose role is being modified
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            //add user to the role
            await _userManager.AddToRoleAsync(user, role);

            //create role change
            RoleChange roleChange = new()
            {
                Admin = admin,
                UserAffected = user,
                RoleAffected = role,
                TimeOfChange = DateTime.UtcNow,
                Type = Constants.RoleChangeType.Added,
            };
            _context.RoleChanges.Add(roleChange);

            _context.SaveChanges();

            //signs user out and in if user modifing their own roles
            if (admin.Id == user.Id)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, false);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}